using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Facility.Definition.CodeGen;

namespace Facility.Definition.Swagger
{
	internal sealed class SwaggerConversion
	{
		public static SwaggerConversion Create(SwaggerService swaggerService, string serviceName, SwaggerParserContext context)
		{
			var conversion = new SwaggerConversion(swaggerService, serviceName);
			conversion.Convert(context);
			return conversion;
		}

		public ServiceInfo Service { get; private set; }

		public IReadOnlyList<ServiceDefinitionError> Errors => m_errors;

		private SwaggerConversion(SwaggerService swaggerService, string serviceName)
		{
			m_swaggerService = swaggerService;
			m_serviceName = serviceName;
			m_errors = new List<ServiceDefinitionError>();
		}

		private void Convert(SwaggerParserContext context)
		{
			if (m_swaggerService.Swagger == null)
				m_errors.Add(context.CreateError("swagger field is missing."));
			else if (m_swaggerService.Swagger != SwaggerUtility.SwaggerVersion)
				m_errors.Add(context.CreateError($"swagger should be '{SwaggerUtility.SwaggerVersion}'.", "swagger"));

			if (m_swaggerService.Info == null)
				m_errors.Add(context.CreateError("info is missing."));

			string name = m_serviceName;
			if (name != null && !ServiceDefinitionUtility.IsValidName(name))
				m_errors.Add(context.CreateError("ServiceName generator option is not a valid service name."));
			if (name == null)
				name = m_swaggerService.Info?.Identifier;
			if (name != null && !ServiceDefinitionUtility.IsValidName(name))
				m_errors.Add(context.CreateError("info/x-identifier is not a valid service name.", "info/x-identifier"));
			if (name == null)
				name = CodeGenUtility.ToPascalCase(m_swaggerService.Info?.Title);
			if (name == null)
				m_errors.Add(context.CreateError("info/title is missing.", "info"));
			if (name != null && !ServiceDefinitionUtility.IsValidName(name))
				m_errors.Add(context.CreateError("info/title is not a valid service name.", "info/title"));

			var attributes = new List<ServiceAttributeInfo>();

			string version = m_swaggerService.Info?.Version;
			if (!string.IsNullOrWhiteSpace(version))
			{
				attributes.Add(new ServiceAttributeInfo("info",
					new[] { new ServiceAttributeParameterInfo("version", version, context.CreatePart("info/version")) },
					context.CreatePart("info")));
			}

			string scheme = GetBestScheme(m_swaggerService.Schemes);
			string host = m_swaggerService.Host;
			string basePath = m_swaggerService.BasePath ?? "";
			if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(scheme))
			{
				string url = new UriBuilder(scheme, host) { Path = basePath }.Uri.AbsoluteUri;
				attributes.Add(new ServiceAttributeInfo("http",
					new[] { new ServiceAttributeParameterInfo("url", url, context.CreatePart()) },
					context.CreatePart()));
			}

			var position = context.CreatePosition();

			var members = new List<ServiceMemberInfo>();

			foreach (var swaggerPath in m_swaggerService.Paths.EmptyIfNull())
			{
				var swaggerOperations = swaggerPath.Value;
				var operationsContext = context.CreateContext("paths/swaggerPath");
				ResolveOperations(ref swaggerOperations, ref operationsContext);
				AddServiceMethod(members, "GET", swaggerPath.Key, swaggerOperations.Get, swaggerOperations.Parameters, operationsContext.CreateContext("get"));
				AddServiceMethod(members, "POST", swaggerPath.Key, swaggerOperations.Post, swaggerOperations.Parameters, operationsContext.CreateContext("post"));
				AddServiceMethod(members, "PUT", swaggerPath.Key, swaggerOperations.Put, swaggerOperations.Parameters, operationsContext.CreateContext("put"));
				AddServiceMethod(members, "DELETE", swaggerPath.Key, swaggerOperations.Delete, swaggerOperations.Parameters, operationsContext.CreateContext("delete"));
				AddServiceMethod(members, "OPTIONS", swaggerPath.Key, swaggerOperations.Options, swaggerOperations.Parameters, operationsContext.CreateContext("options"));
				AddServiceMethod(members, "HEAD", swaggerPath.Key, swaggerOperations.Head, swaggerOperations.Parameters, operationsContext.CreateContext("head"));
				AddServiceMethod(members, "PATCH", swaggerPath.Key, swaggerOperations.Patch, swaggerOperations.Parameters, operationsContext.CreateContext("patch"));
			}

			foreach (var swaggerDefinition in m_swaggerService.Definitions.EmptyIfNull())
			{
				if ((swaggerDefinition.Value.Type ?? SwaggerSchemaType.Object) == SwaggerSchemaType.Object &&
					!members.OfType<ServiceMethodInfo>().Any(x => swaggerDefinition.Key.Equals(x.Name + "Request", StringComparison.OrdinalIgnoreCase)) &&
					!members.OfType<ServiceMethodInfo>().Any(x => swaggerDefinition.Key.Equals(x.Name + "Response", StringComparison.OrdinalIgnoreCase)) &&
					!IsFacilityError(swaggerDefinition) &&
					TryGetFacilityResultOfType(swaggerDefinition, position) == null)
				{
					AddServiceDto(members, swaggerDefinition.Key, swaggerDefinition.Value, context.CreatePart("definitions/" + swaggerDefinition.Key));
				}
			}

			Service = new ServiceInfo(name: name ?? "Api", members: members, attributes: attributes,
				summary: PrepareSummary(m_swaggerService.Info?.Title),
				remarks: SplitRemarks(m_swaggerService.Info?.Description),
				parts: context.CreatePart());
			m_errors.AddRange(Service.GetValidationErrors());
		}

		private static string GetBestScheme(IList<string> schemes)
		{
			return schemes?.FirstOrDefault(x => x == "https") ?? schemes?.FirstOrDefault(x => x == "http") ?? schemes?.FirstOrDefault();
		}

		private void AddServiceDto(List<ServiceMemberInfo> members, string name, SwaggerSchema schema, ServicePart part)
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

				string typeName = TryGetFacilityTypeName(property.Value, part.Position);
				if (typeName != null)
				{
					fields.Add(new ServiceFieldInfo(
						property.Key,
						typeName: typeName,
						attributes: fieldAttributes,
						summary: PrepareSummary(property.Value.Description),
						parts: part));
				}
			}

			members.Add(new ServiceDtoInfo(
				name: name,
				fields: fields,
				attributes: attributes,
				summary: PrepareSummary(schema.Description),
				remarks: SplitRemarks(schema.Remarks),
				parts: part));
		}

		private void AddServiceMethod(IList<ServiceMemberInfo> members, string method, string path, SwaggerOperation swaggerOperation, IList<SwaggerParameter> swaggerOperationsParameters, SwaggerParserContext context)
		{
			if (swaggerOperation == null)
				return;

			var part = context.CreatePart();

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
				AddRequestFields(requestFields, ResolveParameter(swaggerParameter, part?.Position), name, method, part);

			var responseFields = new List<ServiceFieldInfo>();
			var swaggerResponsePairs = swaggerOperation.Responses.EmptyIfNull()
				.Where(x => x.Key[0] == '2' || x.Key[0] == '3' || !string.IsNullOrEmpty(x.Value.Identifier)).ToList();
			foreach (var swaggerResponsePair in swaggerResponsePairs)
			{
				AddResponseFields(responseFields, swaggerResponsePair.Key, ResolveResponse(swaggerResponsePair.Value, part?.Position),
					name, httpAttributeValues, swaggerOperation.Responses.Count == 1, part);
			}

			var attributes = new List<ServiceAttributeInfo> { new ServiceAttributeInfo("http", httpAttributeValues) };
			if (swaggerOperation.Deprecated.GetValueOrDefault())
				attributes.Add(new ServiceAttributeInfo("obsolete"));
			if (swaggerOperation.Tags != null)
				attributes.AddRange(swaggerOperation.Tags.Select(x => new ServiceAttributeInfo("tag", new[] { new ServiceAttributeParameterInfo("name", x) })));

			members.Add(new ServiceMethodInfo(
				name: name,
				requestFields: requestFields,
				responseFields: responseFields,
				attributes: attributes,
				summary: PrepareSummary(swaggerOperation.Summary),
				remarks: SplitRemarks(swaggerOperation.Description),
				parts: part));
		}

		private void AddRequestFields(IList<ServiceFieldInfo> requestFields, SwaggerParameter swaggerParameter, string serviceMethodName, string httpMethod, ServicePart part)
		{
			string kind = swaggerParameter.In;
			if (kind == SwaggerParameterKind.Path || kind == SwaggerParameterKind.Query || kind == SwaggerParameterKind.Header)
			{
				string typeName = TryGetFacilityTypeName(swaggerParameter, part.Position);
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
						parts: part));
				}
			}
			else if (kind == SwaggerParameterKind.Body)
			{
				var bodySchema = ResolveDefinition(swaggerParameter.Schema, part.Position);
				if ((bodySchema.Value.Type ?? SwaggerSchemaType.Object) == SwaggerSchemaType.Object &&
					(bodySchema.Key == null || bodySchema.Key.Equals(serviceMethodName + "Request", StringComparison.OrdinalIgnoreCase)))
				{
					AddFieldsFromSchema(requestFields, part, bodySchema);
				}
				else
				{
					string typeName = bodySchema.Key ?? FilterBodyTypeName(TryGetFacilityTypeName(bodySchema.Value, part.Position));
					if (typeName != null)
					{
						requestFields.Add(new ServiceFieldInfo(
							bodySchema.Value.Identifier ?? "body",
							typeName: typeName,
							attributes: new[] { new ServiceAttributeInfo("http", new[] { new ServiceAttributeParameterInfo("from", "body", part) }) },
							summary: PrepareSummary(swaggerParameter.Description),
							parts: part));
					}
				}
			}
		}

		private void AddResponseFields(IList<ServiceFieldInfo> responseFields, string statusCode, SwaggerResponse swaggerResponse, string serviceMethodName, IList<ServiceAttributeParameterInfo> httpAttributeValues, bool isOnlyResponse, ServicePart part)
		{
			var bodySchema = default(KeyValuePair<string, SwaggerSchema>);

			if (swaggerResponse.Schema != null)
				bodySchema = ResolveDefinition(swaggerResponse.Schema, part.Position);

			if (bodySchema.Value != null && (bodySchema.Value.Type ?? SwaggerSchemaType.Object) == SwaggerSchemaType.Object &&
				(bodySchema.Key == null || bodySchema.Key.Equals(serviceMethodName + "Response", StringComparison.OrdinalIgnoreCase)))
			{
				httpAttributeValues.Add(new ServiceAttributeParameterInfo("code", statusCode, part));
				AddFieldsFromSchema(responseFields, part, bodySchema);
			}
			else if (swaggerResponse.Identifier == null && isOnlyResponse && swaggerResponse.Schema == null)
			{
				httpAttributeValues.Add(new ServiceAttributeParameterInfo("code", statusCode, part));
			}
			else
			{
				responseFields.Add(new ServiceFieldInfo(
					swaggerResponse.Identifier ?? CodeGenUtility.ToCamelCase(bodySchema.Key) ?? GetBodyFieldNameForStatusCode(statusCode),
					typeName: bodySchema.Key ?? (bodySchema.Value != null ? FilterBodyTypeName(TryGetFacilityTypeName(bodySchema.Value, part.Position)) : null) ?? "boolean",
					attributes: new[]
					{
						new ServiceAttributeInfo("http",
							new[]
							{
								new ServiceAttributeParameterInfo("from", "body", part),
								new ServiceAttributeParameterInfo("code", statusCode, part),
							})
					},
					summary: PrepareSummary(swaggerResponse.Description),
					parts: part));
			}
		}

		private static string GetBodyFieldNameForStatusCode(string statusCode)
		{
			if (int.TryParse(statusCode, out var statusCodeNumber))
			{
				string name = ((HttpStatusCode) statusCodeNumber).ToString();
				if (name != statusCode)
					return CodeGenUtility.ToCamelCase(name);
			}

			return CodeGenUtility.ToCamelCase($"status {statusCode}");
		}

		private void AddFieldsFromSchema(IList<ServiceFieldInfo> requestFields, ServicePart part, KeyValuePair<string, SwaggerSchema> bodySchema)
		{
			if ((bodySchema.Value.Type ?? SwaggerSchemaType.Object) != SwaggerSchemaType.Object)
				throw new NotImplementedException();

			foreach (var property in bodySchema.Value.Properties.EmptyIfNull())
			{
				var attributes = new List<ServiceAttributeInfo>();

				if (property.Value.Obsolete.GetValueOrDefault())
					attributes.Add(new ServiceAttributeInfo("obsolete"));

				string typeName = TryGetFacilityTypeName(property.Value, part.Position);
				if (typeName != null)
				{
					requestFields.Add(new ServiceFieldInfo(
						property.Key,
						typeName: typeName,
						attributes: attributes,
						summary: PrepareSummary(property.Value.Description),
						parts: part));
				}
			}
		}

		private static string PrepareSummary(string summary) => string.IsNullOrWhiteSpace(summary) ? null : Regex.Replace(summary.Trim(), @"\s+", " ");

		private static IReadOnlyList<string> SplitRemarks(string remarks) => string.IsNullOrWhiteSpace(remarks) ? null : Regex.Split(remarks, @"\r?\n");

		private string GetDefinitionNameFromRef(string refValue, ServiceDefinitionPosition position)
		{
			const string refPrefix = "#/definitions/";
			if (!refValue.StartsWith(refPrefix, StringComparison.Ordinal))
				m_errors.Add(new ServiceDefinitionError("Definition $ref must start with '#/definitions/'.", position));
			return UnescapeRefPart(refValue.Substring(refPrefix.Length));
		}

		private KeyValuePair<string, SwaggerSchema> ResolveDefinition(SwaggerSchema swaggerDefinition, ServiceDefinitionPosition position)
		{
			string name = null;

			if (swaggerDefinition.Ref != null)
			{
				name = GetDefinitionNameFromRef(swaggerDefinition.Ref, position);
				if (!m_swaggerService.Definitions.TryGetValue(name, out swaggerDefinition))
					m_errors.Add(new ServiceDefinitionError($"Missing definition named '{name}'.", position));
			}

			return new KeyValuePair<string, SwaggerSchema>(name, swaggerDefinition);
		}

		private void ResolveOperations(ref SwaggerOperations swaggerOperations, ref SwaggerParserContext context)
		{
			if (swaggerOperations.Ref != null)
			{
				const string refPrefix = "#/paths/";
				if (!swaggerOperations.Ref.StartsWith(refPrefix, StringComparison.Ordinal))
					m_errors.Add(new ServiceDefinitionError("Operations $ref must start with '#/paths/'.", context.CreatePosition()));

				string name = UnescapeRefPart(swaggerOperations.Ref.Substring(refPrefix.Length));
				if (!m_swaggerService.Paths.TryGetValue(name, out swaggerOperations))
					m_errors.Add(new ServiceDefinitionError($"Missing path named '{name}'.", context.CreatePosition()));

				context = context.Root.CreateContext("paths/" + name);
			}
		}

		private SwaggerParameter ResolveParameter(SwaggerParameter swaggerParameter, ServiceDefinitionPosition position)
		{
			if (swaggerParameter.Ref != null)
			{
				const string refPrefix = "#/parameters/";
				if (!swaggerParameter.Ref.StartsWith(refPrefix, StringComparison.Ordinal))
					m_errors.Add(new ServiceDefinitionError("Parameter $ref must start with '#/parameters/'.", position));

				string name = UnescapeRefPart(swaggerParameter.Ref.Substring(refPrefix.Length));
				if (!m_swaggerService.Parameters.TryGetValue(name, out swaggerParameter))
					m_errors.Add(new ServiceDefinitionError($"Missing parameter named '{name}'.", position));
			}

			return swaggerParameter;
		}

		private SwaggerResponse ResolveResponse(SwaggerResponse swaggerResponse, ServiceDefinitionPosition position)
		{
			if (swaggerResponse.Ref != null)
			{
				const string refPrefix = "#/responses/";
				if (!swaggerResponse.Ref.StartsWith(refPrefix, StringComparison.Ordinal))
					m_errors.Add(new ServiceDefinitionError("Response $ref must start with '#/responses/'.", position));

				string name = UnescapeRefPart(swaggerResponse.Ref.Substring(refPrefix.Length));
				if (!m_swaggerService.Responses.TryGetValue(name, out swaggerResponse))
					m_errors.Add(new ServiceDefinitionError($"Missing response named '{name}'.", position));
			}

			return swaggerResponse;
		}

		private string TryGetFacilityTypeName(ISwaggerSchema swaggerSchema, ServiceDefinitionPosition position)
		{
			switch (swaggerSchema.Type ?? SwaggerSchemaType.Object)
			{
			case SwaggerSchemaType.String:
				return swaggerSchema.Format == SwaggerSchemaTypeFormat.Byte ? "bytes" : "string";

			case SwaggerSchemaType.Number:
				return swaggerSchema.Format == SwaggerSchemaTypeFormat.Decimal ? "decimal" : "double";

			case SwaggerSchemaType.Integer:
				return swaggerSchema.Format == SwaggerSchemaTypeFormat.Int64 ? "int64" : "int32";

			case SwaggerSchemaType.Boolean:
				return "boolean";

			case SwaggerSchemaType.Array:
				return swaggerSchema.Items?.Type == SwaggerSchemaType.Array ? null :
					$"{TryGetFacilityTypeName(swaggerSchema.Items, position)}[]";

			case SwaggerSchemaType.Object:
				if (swaggerSchema is SwaggerSchema fullSchema)
				{
					if (fullSchema.Ref != null)
					{
						var resolvedSchema = ResolveDefinition(fullSchema, position);

						if (IsFacilityError(resolvedSchema))
							return "error";

						string resultOfType = TryGetFacilityResultOfType(resolvedSchema, position);
						if (resultOfType != null)
							return $"result<{resultOfType}>";

						return resolvedSchema.Key;
					}

					if (fullSchema.AdditionalProperties != null)
						return $"map<{TryGetFacilityTypeName(fullSchema.AdditionalProperties, position)}>";
				}

				return "object";
			}

			return null;
		}

		internal static string FilterBodyTypeName(string typeName)
		{
			return typeName == "string" || typeName == "bytes" || typeName == "int32" || typeName == "int64" || typeName == "double" || typeName == "decimal" ? null : typeName;
		}

		internal static bool IsFacilityError(KeyValuePair<string, SwaggerSchema> swaggerSchema)
		{
			return swaggerSchema.Key == "Error" &&
				swaggerSchema.Value.Properties.EmptyIfNull().Any(x => x.Key == "code" && x.Value.Type == SwaggerSchemaType.String) &&
				swaggerSchema.Value.Properties.EmptyIfNull().Any(x => x.Key == "message" && x.Value.Type == SwaggerSchemaType.String);
		}

		internal string TryGetFacilityResultOfType(KeyValuePair<string, SwaggerSchema> swaggerSchema, ServiceDefinitionPosition position)
		{
			const string nameSuffix = "Result";
			if (!swaggerSchema.Key.EndsWith(nameSuffix, StringComparison.Ordinal))
				return null;

			var properties = swaggerSchema.Value.Properties.EmptyIfNull();
			if (!properties.Any(x => x.Key == "error" && x.Value.Ref == "#/definitions/Error"))
				return null;

			var valueSchema = properties.Where(x => x.Key == "value").Select(x => x.Value).FirstOrDefault();
			if (valueSchema == null)
				return null;

			return TryGetFacilityTypeName(valueSchema, position);
		}

		private static string UnescapeRefPart(string value) => value.Replace("~1", "/").Replace("~0", "~");

		private readonly SwaggerService m_swaggerService;
		private readonly string m_serviceName;
		private readonly List<ServiceDefinitionError> m_errors;

		static readonly Regex s_pathParameter = new Regex(@"\{[^}]+\}");
	}
}
