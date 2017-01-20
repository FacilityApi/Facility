using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Facility.Definition.CodeGen;
using Newtonsoft.Json;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.ObjectFactories;

namespace Facility.Definition.Swagger
{
	/// <summary>
	/// Parses Swagger (OpenAPI) 2.0.
	/// </summary>
	public sealed class SwaggerParser
	{
		/// <summary>
		/// The service name (defaults to 'info/x-identifier' or 'info/title').
		/// </summary>
		public string ServiceName { get; set; }

		/// <summary>
		/// Parses Swagger (OpenAPI) 2.0 into a service definition.
		/// </summary>
		public ServiceInfo ParseDefinition(NamedText source)
		{
			if (string.IsNullOrWhiteSpace(source.Text))
				throw new ServiceDefinitionException("Service definition is missing.", new NamedTextPosition(source.Name, 1, 1));

			SwaggerService swaggerService;
			SwaggerParserContext context;

			if (!s_detectJsonRegex.IsMatch(source.Text))
			{
				// parse YAML
				var yamlObjectFactory = new DefaultObjectFactory();
				var yamlDeserializer = new DeserializerBuilder()
					.WithObjectFactory(yamlObjectFactory)
					.WithNodeDeserializer(new OurNodeDeserializer(yamlObjectFactory))
					.IgnoreUnmatchedProperties()
					.WithNamingConvention(new OurNamingConvention())
					.Build();
				using (var stringReader = new StringReader(source.Text))
				{
					try
					{
						swaggerService = yamlDeserializer.Deserialize<SwaggerService>(stringReader);
					}
					catch (YamlException exception)
					{
						var exceptionError = exception.InnerException?.Message ?? exception.Message;
						const string errorStart = "): ";
						int errorStartIndex = exceptionError.IndexOf(errorStart, StringComparison.OrdinalIgnoreCase);
						if (errorStartIndex != -1)
							exceptionError = exceptionError.Substring(errorStartIndex + errorStart.Length);

						var exceptionPosition = new NamedTextPosition(source.Name, exception.End.Line, exception.End.Column);
						throw new ServiceDefinitionException(exceptionError, exceptionPosition);
					}
				}
				if (swaggerService == null)
					throw new ServiceDefinitionException("Service definition is missing.", new NamedTextPosition(source.Name, 1, 1));

				context = SwaggerParserContext.FromYaml(source);
			}
			else
			{
				// parse JSON
				using (var stringReader = new StringReader(source.Text))
				using (var jsonTextReader = new JsonTextReader(stringReader))
				{
					try
					{
						swaggerService = JsonSerializer.Create(SwaggerUtility.JsonSerializerSettings).Deserialize<SwaggerService>(jsonTextReader);
					}
					catch (JsonException exception)
					{
						var exceptionPosition = new NamedTextPosition(source.Name, jsonTextReader.LineNumber, jsonTextReader.LinePosition);
						throw new ServiceDefinitionException(exception.Message, exceptionPosition);
					}

					context = SwaggerParserContext.FromJson(source);
				}
			}

			if (swaggerService.Swagger == null)
				throw context.CreateException("swagger field is missing.");
			if (swaggerService.Swagger != SwaggerUtility.SwaggerVersion)
				throw context.CreateException($"swagger should be '{SwaggerUtility.SwaggerVersion}'.", "swagger");

			if (swaggerService.Info == null)
				throw context.CreateException("info is missing.");

			string name = ServiceName;
			if (name != null && !ServiceDefinitionUtility.IsValidName(name))
				throw context.CreateException("ServiceName generator option is not a valid service name.");
			if (name == null)
				name = swaggerService.Info?.Identifier;
			if (name != null && !ServiceDefinitionUtility.IsValidName(name))
				throw context.CreateException("info/x-identifier is not a valid service name.", "info/x-identifier");
			if (name == null)
				name = CodeGenUtility.ToPascalCase(swaggerService.Info?.Title);
			if (name == null)
				throw context.CreateException("info/title is missing.", "info");
			if (name != null && !ServiceDefinitionUtility.IsValidName(name))
				throw context.CreateException("info/title is not a valid service name.", "info/title");

			var attributes = new List<ServiceAttributeInfo>();

			string version = swaggerService.Info?.Version;
			if (!string.IsNullOrWhiteSpace(version))
			{
				attributes.Add(new ServiceAttributeInfo("info",
					new[] { new ServiceAttributeParameterInfo("version", version, context.CreatePosition("info/version")) },
					context.CreatePosition("info")));
			}

			string scheme = GetBestScheme(swaggerService.Schemes);
			string host = swaggerService.Host;
			string basePath = swaggerService.BasePath ?? "";
			if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(scheme))
			{
				string url = new UriBuilder(scheme, host) { Path = basePath }.Uri.AbsoluteUri;
				attributes.Add(new ServiceAttributeInfo("http",
					new[] { new ServiceAttributeParameterInfo("url", url, context.CreatePosition()) },
					context.CreatePosition()));
			}

			var position = context.CreatePosition();

			var members = new List<IServiceMemberInfo>();

			foreach (var swaggerPath in swaggerService.Paths.EmptyIfNull())
			{
				var swaggerOperations = swaggerPath.Value;
				var operationsContext = context.CreateContext("paths/swaggerPath");
				swaggerService.ResolveOperations(ref swaggerOperations, ref operationsContext);
				AddServiceMethod(members, "GET", swaggerPath.Key, swaggerOperations.Get, swaggerOperations.Parameters, swaggerService, operationsContext.CreateContext("get"));
				AddServiceMethod(members, "POST", swaggerPath.Key, swaggerOperations.Post, swaggerOperations.Parameters, swaggerService, operationsContext.CreateContext("post"));
				AddServiceMethod(members, "PUT", swaggerPath.Key, swaggerOperations.Put, swaggerOperations.Parameters, swaggerService, operationsContext.CreateContext("put"));
				AddServiceMethod(members, "DELETE", swaggerPath.Key, swaggerOperations.Delete, swaggerOperations.Parameters, swaggerService, operationsContext.CreateContext("delete"));
				AddServiceMethod(members, "OPTIONS", swaggerPath.Key, swaggerOperations.Options, swaggerOperations.Parameters, swaggerService, operationsContext.CreateContext("options"));
				AddServiceMethod(members, "HEAD", swaggerPath.Key, swaggerOperations.Head, swaggerOperations.Parameters, swaggerService, operationsContext.CreateContext("head"));
				AddServiceMethod(members, "PATCH", swaggerPath.Key, swaggerOperations.Patch, swaggerOperations.Parameters, swaggerService, operationsContext.CreateContext("patch"));
			}

			foreach (var swaggerDefinition in swaggerService.Definitions.EmptyIfNull())
			{
				if ((swaggerDefinition.Value.Type ?? SwaggerSchemaType.Object) == SwaggerSchemaType.Object &&
					!members.OfType<ServiceMethodInfo>().Any(x => swaggerDefinition.Key.Equals(x.Name + "Request", StringComparison.OrdinalIgnoreCase)) &&
					!members.OfType<ServiceMethodInfo>().Any(x => swaggerDefinition.Key.Equals(x.Name + "Response", StringComparison.OrdinalIgnoreCase)) &&
					!swaggerService.IsFacilityError(swaggerDefinition) &&
					swaggerService.TryGetFacilityResultOfType(swaggerDefinition, position) == null)
				{
					AddServiceDto(members, swaggerDefinition.Key, swaggerDefinition.Value, swaggerService, context.CreatePosition("definitions/" + swaggerDefinition.Key));
				}
			}

			return new ServiceInfo(name, members: members, attributes: attributes,
				summary: PrepareSummary(swaggerService.Info?.Title),
				remarks: SplitRemarks(swaggerService.Info?.Description),
				position: context.CreatePosition());
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

		private void AddServiceMethod(IList<IServiceMemberInfo> members, string method, string path, SwaggerOperation swaggerOperation, IReadOnlyList<SwaggerParameter> swaggerOperationsParameters, SwaggerService swaggerService, SwaggerParserContext context)
		{
			if (swaggerOperation == null)
				return;

			var position = context.CreatePosition();

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

			var attributes = new List<ServiceAttributeInfo> { new ServiceAttributeInfo("http", httpAttributeValues) };
			if (swaggerOperation.Deprecated.GetValueOrDefault())
				attributes.Add(new ServiceAttributeInfo("obsolete"));

			members.Add(new ServiceMethodInfo(
				name: name,
				requestFields: requestFields,
				responseFields: responseFields,
				attributes: attributes,
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

		private sealed class OurNodeDeserializer : INodeDeserializer
		{
			public OurNodeDeserializer(IObjectFactory objectFactory)
			{
				m_dictionaryDeserializer = new DictionaryNodeDeserializer(objectFactory);
			}

			public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object> nestedObjectDeserializer, out object value)
			{
				if (expectedType.IsConstructedGenericType && expectedType.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))
				{
					var dictionaryType = typeof(Dictionary<,>).MakeGenericType(expectedType.GenericTypeArguments);
					return m_dictionaryDeserializer.Deserialize(reader, dictionaryType, nestedObjectDeserializer, out value);
				}
				else
				{
					value = null;
					return false;
				}
			}

			readonly INodeDeserializer m_dictionaryDeserializer;
		}

		private sealed class OurNamingConvention : INamingConvention
		{
			public string Apply(string value)
			{
				return value.StartsWith("x-", StringComparison.Ordinal) ? value : CodeGenUtility.ToCamelCase(value);
			}
		}

		static readonly Regex s_detectJsonRegex = new Regex(@"^\s*[{/]", RegexOptions.Singleline);
		static readonly Regex s_pathParameter = new Regex(@"\{[^}]+\}");
	}
}
