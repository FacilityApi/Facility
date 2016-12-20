using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Facility.Definition.CodeGen;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Facility.Definition.Swagger
{
	/// <summary>
	/// Parses Swagger (OpenAPI) 2.0.
	/// </summary>
	public sealed class SwaggerParser
	{
		/// <summary>
		/// The service name (defaults to '/info/x-identifier' or '/info/title').
		/// </summary>
		public string ServiceName { get; set; }

		/// <summary>
		/// Parses Swagger (OpenAPI) 2.0 into a service definition.
		/// </summary>
		public ServiceInfo ParseDefinition(NamedText source)
		{
			var position = new NamedTextPosition(source.Name);

			string json = source.Text;
			if (!s_detectJsonRegex.IsMatch(json))
			{
				// convert YAML to JSON
				using (var stringReader = new StringReader(source.Text))
					json = JsonConvert.SerializeObject(new DeserializerBuilder().Build().Deserialize(stringReader), SwaggerUtility.JsonSerializerSettings);
			}

			SwaggerService swaggerService;
			using (var stringReader = new StringReader(json))
			using (var jsonTextReader = new JsonTextReader(stringReader))
				swaggerService = JsonSerializer.Create(SwaggerUtility.JsonSerializerSettings).Deserialize<SwaggerService>(jsonTextReader);

			string name = ServiceName ?? swaggerService.Info?.Identifier ?? CodeGenUtility.ToPascalCase(swaggerService.Info?.Title ?? "");
			if (name == null)
				throw new ServiceDefinitionException("Missing service info title.", position);

			var attributes = new List<ServiceAttributeInfo>();

			string version = swaggerService.Info?.Version;
			if (!string.IsNullOrWhiteSpace(version))
				attributes.Add(new ServiceAttributeInfo("info", new[] { new ServiceAttributeParameterInfo("version", version, position) }, position));

			string scheme = GetBestScheme(swaggerService.Schemes);
			string host = swaggerService.Host;
			string basePath = swaggerService.BasePath ?? "";
			if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(scheme))
			{
				string url = new UriBuilder(scheme, host) { Path = basePath }.Uri.AbsoluteUri;
				attributes.Add(new ServiceAttributeInfo("http", new[] { new ServiceAttributeParameterInfo("url", url) }));
			}

			var members = new List<IServiceMemberInfo>();

			foreach (var swaggerPath in swaggerService.Paths.EmptyIfNull())
			{
				var swaggerOperations = swaggerService.ResolveOperations(swaggerPath.Value, position);
				AddServiceMethod(members, "GET", swaggerPath.Key, swaggerOperations.Get, swaggerOperations.Parameters, swaggerService, position);
				AddServiceMethod(members, "POST", swaggerPath.Key, swaggerOperations.Post, swaggerOperations.Parameters, swaggerService, position);
				AddServiceMethod(members, "PUT", swaggerPath.Key, swaggerOperations.Put, swaggerOperations.Parameters, swaggerService, position);
				AddServiceMethod(members, "DELETE", swaggerPath.Key, swaggerOperations.Delete, swaggerOperations.Parameters, swaggerService, position);
				AddServiceMethod(members, "OPTIONS", swaggerPath.Key, swaggerOperations.Options, swaggerOperations.Parameters, swaggerService, position);
				AddServiceMethod(members, "HEAD", swaggerPath.Key, swaggerOperations.Head, swaggerOperations.Parameters, swaggerService, position);
				AddServiceMethod(members, "PATCH", swaggerPath.Key, swaggerOperations.Patch, swaggerOperations.Parameters, swaggerService, position);
			}

			foreach (var swaggerDefinition in swaggerService.Definitions.EmptyIfNull())
			{
				if ((swaggerDefinition.Value.Type ?? SwaggerSchemaType.Object) == SwaggerSchemaType.Object &&
					!members.OfType<ServiceMethodInfo>().Any(x => swaggerDefinition.Key.Equals(x.Name + "Request", StringComparison.OrdinalIgnoreCase)) &&
					!members.OfType<ServiceMethodInfo>().Any(x => swaggerDefinition.Key.Equals(x.Name + "Response", StringComparison.OrdinalIgnoreCase)) &&
					!swaggerService.IsFacilityError(swaggerDefinition) &&
					swaggerService.TryGetFacilityResultOfType(swaggerDefinition, position) == null)
				{
					AddServiceDto(members, swaggerDefinition.Key, swaggerDefinition.Value, swaggerService, position);
				}
			}

			return new ServiceInfo(name, members: members, attributes: attributes,
				summary: PrepareSummary(swaggerService.Info?.Title),
				remarks: SplitRemarks(swaggerService.Info?.Description),
				position: position);
		}

		private static string GetBestScheme(IReadOnlyList<string> schemes)
		{
			return schemes?.FirstOrDefault(x => x == "https") ?? schemes?.FirstOrDefault(x => x == "http") ?? schemes?.FirstOrDefault();
		}

		private void AddServiceDto(List<IServiceMemberInfo> members, string name, SwaggerSchema schema, SwaggerService swaggerService, NamedTextPosition position)
		{
			var attributes = new List<ServiceAttributeInfo>();

			if (schema.Obsolete.GetValueOrDefault())
				attributes.Add(new ServiceAttributeInfo("obsolete"));

			var fields = new List<ServiceFieldInfo>();

			foreach (var property in schema.Properties.EmptyIfNull())
			{
				var fieldAttributes = new List<ServiceAttributeInfo>();

				if (property.Value.Obsolete.GetValueOrDefault())
					fieldAttributes.Add(new ServiceAttributeInfo("obsolete"));

				string typeName = swaggerService.TryGetFacilityTypeName(property.Value, position);
				if (typeName != null)
				{
					fields.Add(new ServiceFieldInfo(
						property.Key,
						typeName: typeName,
						attributes: fieldAttributes,
						summary: PrepareSummary(property.Value.Description),
						position: position));
				}
			}

			members.Add(new ServiceDtoInfo(
				name: name,
				fields: fields,
				attributes: attributes,
				summary: PrepareSummary(schema.Description),
				remarks: SplitRemarks(schema.Remarks),
				position: position));
		}

		private void AddServiceMethod(IList<IServiceMemberInfo> members, string method, string path, SwaggerOperation swaggerOperation, IReadOnlyList<SwaggerParameter> swaggerOperationsParameters, SwaggerService swaggerService, NamedTextPosition position)
		{
			if (swaggerOperation == null)
				return;

			path = s_pathParameter.Replace(path, match =>
			{
				string paramName = match.ToString().Substring(1, match.Length - 2);
				if (!ServiceDefinitionUtility.IsValidName(paramName))
					paramName = CodeGenUtility.ToCamelCase(paramName);
				return $"{{{paramName}}}";
			});

			string name = CodeGenUtility.ToCamelCase(swaggerOperation.OperationId);
			if (!ServiceDefinitionUtility.IsValidName(name))
				name = CodeGenUtility.ToCamelCase($"{method} {path}");

			var httpAttributeValues = new List<ServiceAttributeParameterInfo>
			{
				new ServiceAttributeParameterInfo("method", method),
				new ServiceAttributeParameterInfo("path", path),
			};

			var requestFields = new List<ServiceFieldInfo>();
			foreach (var swaggerParameter in swaggerOperationsParameters.EmptyIfNull().Concat(swaggerOperation.Parameters.EmptyIfNull()))
				AddRequestFields(requestFields, swaggerService.ResolveParameter(swaggerParameter, position), name, method, swaggerService, position);

			var responseFields = new List<ServiceFieldInfo>();
			var swaggerResponsePairs = swaggerOperation.Responses.EmptyIfNull()
				.Where(x => x.Key[0] == '2' || x.Key[0] == '3' || !string.IsNullOrEmpty(x.Value.Identifier)).ToList();
			foreach (var swaggerResponsePair in swaggerResponsePairs)
			{
				AddResponseFields(responseFields, swaggerResponsePair.Key, swaggerService.ResolveResponse(swaggerResponsePair.Value, position),
					name, httpAttributeValues, swaggerOperation.Responses.Count == 1, swaggerService, position);
			}

			members.Add(new ServiceMethodInfo(
				name: name,
				requestFields: requestFields,
				responseFields: responseFields,
				attributes: new[] { new ServiceAttributeInfo("http", httpAttributeValues) },
				summary: PrepareSummary(swaggerOperation.Summary),
				remarks: SplitRemarks(swaggerOperation.Description),
				position: position));
		}

		private void AddRequestFields(IList<ServiceFieldInfo> requestFields, SwaggerParameter swaggerParameter, string serviceMethodName, string httpMethod, SwaggerService swaggerService, NamedTextPosition position)
		{
			string kind = swaggerParameter.In;
			if (kind == SwaggerParameterKind.Path || kind == SwaggerParameterKind.Query || kind == SwaggerParameterKind.Header)
			{
				string typeName = swaggerService.TryGetFacilityTypeName(swaggerParameter, position);
				if (typeName != null)
				{
					if (typeName.EndsWith("[]", StringComparison.Ordinal))
						typeName = "string";

					var attributes = new List<ServiceAttributeInfo>();

					if (swaggerParameter.Obsolete.GetValueOrDefault())
						attributes.Add(new ServiceAttributeInfo("obsolete"));

					string fieldName = swaggerParameter.Identifier ?? swaggerParameter.Name;
					if (!ServiceDefinitionUtility.IsValidName(fieldName))
						fieldName = CodeGenUtility.ToCamelCase(fieldName);

					if (kind == SwaggerParameterKind.Query)
					{
						var parameters = new List<ServiceAttributeParameterInfo>();
						if (httpMethod != "GET")
							parameters.Add(new ServiceAttributeParameterInfo("from", "query"));
						if (fieldName != swaggerParameter.Name)
							parameters.Add(new ServiceAttributeParameterInfo("name", swaggerParameter.Name));
						if (parameters.Count != 0)
							attributes.Add(new ServiceAttributeInfo("http", parameters));
					}
					else if (kind == SwaggerParameterKind.Header)
					{
						attributes.Add(new ServiceAttributeInfo("http",
							new[]
							{
								new ServiceAttributeParameterInfo("from", "header"),
								new ServiceAttributeParameterInfo("name", swaggerParameter.Name),
							}));
					}

					requestFields.Add(new ServiceFieldInfo(
						fieldName,
						typeName: typeName,
						attributes: attributes,
						summary: PrepareSummary(swaggerParameter.Description),
						position: position));
				}
			}
			else if (kind == SwaggerParameterKind.Body)
			{
				var bodySchema = swaggerService.ResolveDefinition(swaggerParameter.Schema, position);
				if ((bodySchema.Value.Type ?? SwaggerSchemaType.Object) == SwaggerSchemaType.Object &&
					(bodySchema.Key == null || bodySchema.Key.Equals(serviceMethodName + "Request", StringComparison.OrdinalIgnoreCase)))
				{
					AddFieldsFromSchema(requestFields, swaggerService, position, bodySchema);
				}
				else
				{
					string typeName = bodySchema.Key ?? SwaggerUtility.FilterBodyTypeName(swaggerService.TryGetFacilityTypeName(bodySchema.Value, position));
					if (typeName != null)
					{
						requestFields.Add(new ServiceFieldInfo(
							bodySchema.Value.Identifier ?? "body",
							typeName: typeName,
							attributes: new[] { new ServiceAttributeInfo("http", new[] { new ServiceAttributeParameterInfo("from", "body", position) }) },
							summary: PrepareSummary(swaggerParameter.Description),
							position: position));
					}
				}
			}
		}

		private void AddResponseFields(IList<ServiceFieldInfo> responseFields, string statusCode, SwaggerResponse swaggerResponse, string serviceMethodName, IList<ServiceAttributeParameterInfo> httpAttributeValues, bool isOnlyResponse, SwaggerService swaggerService, NamedTextPosition position)
		{
			var bodySchema = default(KeyValuePair<string, SwaggerSchema>);

			if (swaggerResponse.Schema != null)
				bodySchema = swaggerService.ResolveDefinition(swaggerResponse.Schema, position);

			if (bodySchema.Value != null && (bodySchema.Value.Type ?? SwaggerSchemaType.Object) == SwaggerSchemaType.Object &&
				(bodySchema.Key == null || bodySchema.Key.Equals(serviceMethodName + "Response", StringComparison.OrdinalIgnoreCase)))
			{
				httpAttributeValues.Add(new ServiceAttributeParameterInfo("code", statusCode, position));
				AddFieldsFromSchema(responseFields, swaggerService, position, bodySchema);
			}
			else if (swaggerResponse.Identifier == null && isOnlyResponse && swaggerResponse.Schema == null)
			{
				httpAttributeValues.Add(new ServiceAttributeParameterInfo("code", statusCode, position));
			}
			else
			{
				responseFields.Add(new ServiceFieldInfo(
					swaggerResponse.Identifier ?? CodeGenUtility.ToCamelCase(bodySchema.Key) ?? GetBodyFieldNameForStatusCode(statusCode),
					typeName: bodySchema.Key ?? (bodySchema.Value != null ? SwaggerUtility.FilterBodyTypeName(swaggerService.TryGetFacilityTypeName(bodySchema.Value, position)) : null) ?? "boolean",
					attributes: new[]
					{
						new ServiceAttributeInfo("http",
						new[]
						{
							new ServiceAttributeParameterInfo("from", "body", position),
							new ServiceAttributeParameterInfo("code", statusCode, position),
						})
					},
					summary: PrepareSummary(swaggerResponse.Description),
					position: position));
			}
		}

		private static string GetBodyFieldNameForStatusCode(string statusCode)
		{
			int statusCodeNumber;
			if (int.TryParse(statusCode, out statusCodeNumber))
			{
				string name = ((HttpStatusCode) statusCodeNumber).ToString();
				if (name != statusCode)
					return CodeGenUtility.ToCamelCase(name);
			}

			return CodeGenUtility.ToCamelCase($"status {statusCode}");
		}

		private static void AddFieldsFromSchema(IList<ServiceFieldInfo> requestFields, SwaggerService swaggerService, NamedTextPosition position, KeyValuePair<string, SwaggerSchema> bodySchema)
		{
			if ((bodySchema.Value.Type ?? SwaggerSchemaType.Object) != SwaggerSchemaType.Object)
				throw new NotImplementedException();

			foreach (var property in bodySchema.Value.Properties.EmptyIfNull())
			{
				var attributes = new List<ServiceAttributeInfo>();

				if (property.Value.Obsolete.GetValueOrDefault())
					attributes.Add(new ServiceAttributeInfo("obsolete"));

				string typeName = swaggerService.TryGetFacilityTypeName(property.Value, position);
				if (typeName != null)
				{
					requestFields.Add(new ServiceFieldInfo(
						property.Key,
						typeName: typeName,
						attributes: attributes,
						summary: PrepareSummary(property.Value.Description),
						position: position));
				}
			}
		}

		private static string PrepareSummary(string summary)
		{
			return string.IsNullOrWhiteSpace(summary) ? null : Regex.Replace(summary.Trim(), @"\s+", " ");
		}

		private static IReadOnlyList<string> SplitRemarks(string remarks)
		{
			return string.IsNullOrWhiteSpace(remarks) ? null : Regex.Split(remarks, @"\r?\n");
		}

		static readonly Regex s_detectJsonRegex = new Regex(@"^\s*[{/]", RegexOptions.Singleline);
		static readonly Regex s_pathParameter = new Regex(@"\{[^}]+\}");
	}
}
