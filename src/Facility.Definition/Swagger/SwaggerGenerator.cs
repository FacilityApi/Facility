using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Facility.Definition.CodeGen;
using Facility.Definition.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace Facility.Definition.Swagger
{
	/// <summary>
	/// Generates a Swagger (OpenAPI) 2.0 file for a service definition.
	/// </summary>
	public sealed class SwaggerGenerator : CodeGenerator
	{
		/// <summary>
		/// True to generate YAML.
		/// </summary>
		public bool Yaml { get; set; }

		/// <summary>
		/// Generates Swagger (OpenAPI 2.0) for a service definition.
		/// </summary>
		public SwaggerService GenerateSwaggerService(ServiceInfo service)
		{
			var httpServiceInfo = HttpServiceInfo.Create(service);

			var swaggerService = new SwaggerService
			{
				Swagger = SwaggerUtility.SwaggerVersion,
				Info = new SwaggerInfo
				{
					Identifier = service.Name,
					Title = GetSummaryOrNull(service) ?? service.Name,
					Description = GetRemarksOrNull(service),
					Version = service.TryGetAttribute("info")?.TryGetParameterValue("version") ?? "0.0.0",
					CodeGen = CodeGenUtility.GetCodeGenComment(GeneratorName),
				}
			};

			string defaultBaseUri = httpServiceInfo.Url;
			if (defaultBaseUri != null)
			{
				var baseUri = new Uri(defaultBaseUri);
				swaggerService.Host = baseUri.Host;
				swaggerService.Schemes = new[] { baseUri.Scheme };

				string basePath = baseUri.PathAndQuery;
				if (!string.IsNullOrEmpty(basePath) && basePath != "/")
					swaggerService.BasePath = baseUri.PathAndQuery;
			}

			var paths = new OurDictionary<string, SwaggerOperations>();
			foreach (var httpMethodInfo in httpServiceInfo.Methods)
				AddMethodToPaths(paths, service, httpMethodInfo);
			swaggerService.Paths = paths;

			var dtoInfos = new OurDictionary<string, ServiceDtoInfo>();
			foreach (var httpMethodInfo in httpServiceInfo.Methods)
			{
				if (httpMethodInfo.RequestBodyField != null)
					AddDtos(dtoInfos, GetDtosForType(service.GetFieldType(httpMethodInfo.RequestBodyField.ServiceField)));

				AddDto(dtoInfos, TryCreateMethodRequestBodyType(httpMethodInfo)?.Dto);

				foreach (var httpResponseInfo in httpMethodInfo.ValidResponses)
				{
					if (httpResponseInfo.BodyField != null)
						AddDtos(dtoInfos, GetDtosForType(service.GetFieldType(httpResponseInfo.BodyField.ServiceField)));

					AddDto(dtoInfos, TryCreateMethodResponseBodyType(httpMethodInfo, httpResponseInfo)?.Dto);
				}
			}

			while (true)
			{
				int dtoCount = dtoInfos.Count;
				foreach (var field in dtoInfos.Values.SelectMany(x => x.Fields).ToList())
					AddDtos(dtoInfos, GetDtosForType(service.GetFieldType(field)));
				if (dtoCount == dtoInfos.Count)
					break;
			}

			var definitions = new OurDictionary<string, SwaggerSchema>();
			foreach (var dtoInfo in dtoInfos.Values)
				definitions[dtoInfo.Name] = GetDtoSchema(service, dtoInfo);
			swaggerService.Definitions = definitions.Count == 0 ? null : definitions;

			return swaggerService;
		}

		/// <summary>
		/// Generates a Swagger (OpenAPI 2.0) file for a service definition.
		/// </summary>
		protected override CodeGenOutput GenerateOutputCore(ServiceInfo service)
		{
			var swaggerService = GenerateSwaggerService(service);

			if (Yaml)
			{
				return new CodeGenOutput(CreateFile($"{service.Name}.yaml", code =>
				{
					var yamlObject = ConvertJTokenToObject(JToken.FromObject(swaggerService, JsonSerializer.Create(SwaggerUtility.JsonSerializerSettings)));
					new SerializerBuilder().DisableAliases().EmitDefaults().WithEventEmitter(x => new OurEventEmitter(x)).Build().Serialize(code.TextWriter, yamlObject);
				}));
			}
			else
			{
				return new CodeGenOutput(CreateFile($"{service.Name}.json", code =>
				{
					using (var jsonTextWriter = new JsonTextWriter(code.TextWriter) { Formatting = Formatting.Indented, CloseOutput = false })
						JsonSerializer.Create(SwaggerUtility.JsonSerializerSettings).Serialize(jsonTextWriter, swaggerService);
				}));
			}
		}

		private void AddDtos(IDictionary<string, ServiceDtoInfo> dictionary, IEnumerable<ServiceDtoInfo> dtos)
		{
			foreach (var dto in dtos)
				AddDto(dictionary, dto);
		}

		private void AddDto(IDictionary<string, ServiceDtoInfo> dictionary, ServiceDtoInfo dto)
		{
			if (dto != null && !dictionary.ContainsKey(dto.Name))
				dictionary[dto.Name] = dto;
		}

		private static ServiceTypeInfo TryCreateMethodRequestBodyType(HttpMethodInfo httpMethodInfo)
		{
			if (httpMethodInfo.RequestNormalFields == null || httpMethodInfo.RequestNormalFields.Count == 0)
				return null;

			return ServiceTypeInfo.CreateDto(new ServiceDtoInfo(
				name: $"{CodeGenUtility.ToPascalCase(httpMethodInfo.ServiceMethod.Name)}Request",
				fields: httpMethodInfo.RequestNormalFields.Select(x => x.ServiceField)));
		}

		private static ServiceTypeInfo TryCreateMethodResponseBodyType(HttpMethodInfo httpMethodInfo, HttpResponseInfo httpResponseInfo)
		{
			if (httpResponseInfo.NormalFields == null || httpResponseInfo.NormalFields.Count == 0)
				return null;

			return ServiceTypeInfo.CreateDto(new ServiceDtoInfo(
				name: $"{CodeGenUtility.ToPascalCase(httpMethodInfo.ServiceMethod.Name)}Response",
				fields: httpResponseInfo.NormalFields.Select(x => x.ServiceField)));
		}

		private IEnumerable<ServiceDtoInfo> GetDtosForType(ServiceTypeInfo type)
		{
			switch (type.Kind)
			{
			case ServiceTypeKind.Error:
				yield return GetErrorDto();
				break;
			case ServiceTypeKind.Dto:
				yield return type.Dto;
				break;
			case ServiceTypeKind.Result:
				yield return GetResultDto(type);
				break;
			}

			if (type.ValueType != null)
			{
				foreach (var dto in GetDtosForType(type.ValueType))
					yield return dto;
			}
		}

		private static string GetSummaryOrNull(IServiceHasSummary info) => info.Summary.Length == 0 ? null : info.Summary;

		private static string GetRemarksOrNull(ServiceMemberInfo info) => info.Remarks.Count == 0 ? null : string.Join("\n", info.Remarks);

		private static bool? GetObsoleteOrNull(ServiceElementWithAttributesInfo info) => info.IsObsolete ? true : default(bool?);

		private static ServiceDtoInfo GetErrorDto()
		{
			return new ServiceDtoInfo(name: "Error",
				fields: new[]
				{
					new ServiceFieldInfo(name: "code", typeName: "string", summary: "The error code."),
					new ServiceFieldInfo(name: "message", typeName: "string", summary: "The error message."),
					new ServiceFieldInfo(name: "details", typeName: "object", summary: "Advanced error details."),
					new ServiceFieldInfo(name: "innerError", typeName: "error", summary: "The inner error."),
				},
				summary: "An error.");
		}

		private static string GetTypeAsDtoName(ServiceTypeInfo type)
		{
			var typeKind = type.Kind;
			switch (typeKind)
			{
			case ServiceTypeKind.Dto:
				return type.Dto.Name;
			case ServiceTypeKind.Enum:
				return type.Enum.Name;
			case ServiceTypeKind.Result:
			case ServiceTypeKind.Array:
			case ServiceTypeKind.Map:
				return GetTypeAsDtoName(type.ValueType) + typeKind;
			default:
				return typeKind.ToString();
			}
		}

		private static ServiceDtoInfo GetResultDto(ServiceTypeInfo type)
		{
			return new ServiceDtoInfo(name: GetTypeAsDtoName(type),
				fields: new[]
				{
					new ServiceFieldInfo(name: "value", typeName: type.ValueType.ToString(), summary: "The value."),
					new ServiceFieldInfo(name: "error", typeName: "error", summary: "The error."),
				},
				summary: "A result value or error.");
		}

		private static void AddMethodToPaths(IDictionary<string, SwaggerOperations> paths, ServiceInfo service, HttpMethodInfo httpMethodInfo)
		{
			var methodInfo = httpMethodInfo.ServiceMethod;

			if (!paths.TryGetValue(httpMethodInfo.Path, out var operations))
				paths[httpMethodInfo.Path] = operations = new SwaggerOperations();

			var operation = new SwaggerOperation
			{
				Summary = GetSummaryOrNull(methodInfo),
				Description = GetRemarksOrNull(methodInfo),
				OperationId = methodInfo.Name,
				Deprecated = GetObsoleteOrNull(methodInfo),
				Tags = methodInfo.TagNames.Count == 0 ? null : methodInfo.TagNames.ToList(),
			};

			if (httpMethodInfo.RequestNormalFields.Count != 0 || httpMethodInfo.RequestBodyField != null)
				operation.Consumes = new[] { "application/json" };
			if (httpMethodInfo.ValidResponses.Any(x => (x.NormalFields != null && x.NormalFields.Count != 0) || (x.BodyField != null && service.GetFieldType(x.BodyField.ServiceField).Kind != ServiceTypeKind.Boolean)))
				operation.Produces = new[] { "application/json" };

			var parameters = new List<SwaggerParameter>();

			foreach (var httpPathInfo in httpMethodInfo.PathFields)
				parameters.Add(CreateSwaggerParameter(service, httpPathInfo.ServiceField, SwaggerParameterKind.Path, httpPathInfo.Name));

			foreach (var httpQueryInfo in httpMethodInfo.QueryFields)
				parameters.Add(CreateSwaggerParameter(service, httpQueryInfo.ServiceField, SwaggerParameterKind.Query, httpQueryInfo.Name));

			foreach (var httpHeaderInfo in httpMethodInfo.RequestHeaderFields)
				parameters.Add(CreateSwaggerParameter(service, httpHeaderInfo.ServiceField, SwaggerParameterKind.Header, httpHeaderInfo.Name));

			var requestBodyFieldType = httpMethodInfo.RequestBodyField == null ? null : service.GetFieldType(httpMethodInfo.RequestBodyField.ServiceField);
			if (requestBodyFieldType != null && requestBodyFieldType.Kind != ServiceTypeKind.Boolean)
				parameters.Add(CreateSwaggerRequestBodyParameter(requestBodyFieldType, "request", httpMethodInfo.RequestBodyField.ServiceField.Summary));
			else if (httpMethodInfo.RequestNormalFields.Count != 0)
				parameters.Add(CreateSwaggerRequestBodyParameter(TryCreateMethodRequestBodyType(httpMethodInfo), "request"));

			if (parameters.Count != 0)
				operation.Parameters = parameters;

			var responses = new OurDictionary<string, SwaggerResponse>();

			foreach (var validResponse in httpMethodInfo.ValidResponses)
			{
				string statusCodeString = ((int) validResponse.StatusCode).ToString(CultureInfo.InvariantCulture);

				var bodyField = validResponse.BodyField;
				var bodyFieldType = bodyField == null ? null : service.GetFieldType(bodyField.ServiceField);
				if (bodyField != null)
					responses[statusCodeString] = CreateSwaggerResponse(bodyFieldType, bodyField.ServiceField.Name, bodyField.ServiceField.Summary);
				else if (validResponse.NormalFields != null && validResponse.NormalFields.Count != 0)
					responses[statusCodeString] = CreateSwaggerResponse(TryCreateMethodResponseBodyType(httpMethodInfo, validResponse));
				else
					responses[statusCodeString] = CreateSwaggerResponse();
			}

			operation.Responses = responses;

			string httpMethod = httpMethodInfo.Method.ToLowerInvariant();
			switch (httpMethod)
			{
			case "get":
				operations.Get = operation;
				break;
			case "post":
				operations.Post = operation;
				break;
			case "put":
				operations.Put = operation;
				break;
			case "delete":
				operations.Delete = operation;
				break;
			case "options":
				operations.Options = operation;
				break;
			case "head":
				operations.Head = operation;
				break;
			case "patch":
				operations.Patch = operation;
				break;
			default:
				throw new InvalidOperationException("Unexpected HTTP method: " + httpMethod);
			}
		}

		private static SwaggerParameter CreateSwaggerParameter(ServiceInfo service, ServiceFieldInfo fieldInfo, string inKind, string name)
		{
			var parameterObject = GetTypeSchema<SwaggerParameter>(service.GetFieldType(fieldInfo));
			parameterObject.In = inKind;
			parameterObject.Name = name ?? fieldInfo.Name;
			if (parameterObject.Name != fieldInfo.Name)
				parameterObject.Identifier = fieldInfo.Name;
			parameterObject.Description = GetSummaryOrNull(fieldInfo);
			parameterObject.Required = inKind == SwaggerParameterKind.Path ? true : default(bool?);
			parameterObject.Obsolete = GetObsoleteOrNull(fieldInfo);
			return parameterObject;
		}

		private static SwaggerParameter CreateSwaggerRequestBodyParameter(ServiceTypeInfo type, string name, string description = null)
		{
			return new SwaggerParameter
			{
				In = SwaggerParameterKind.Body,
				Name = name,
				Description = description,
				Required = true,
				Schema = GetTypeSchema<SwaggerSchema>(type),
			};
		}

		private static SwaggerResponse CreateSwaggerResponse(ServiceTypeInfo type = null, string identifier = null, string description = null)
		{
			return new SwaggerResponse
			{
				Description = description ?? "",
				Schema = type != null && type.Kind != ServiceTypeKind.Boolean ? GetTypeSchema<SwaggerSchema>(type) : null,
				Identifier = identifier,
			};
		}

		private static T GetTypeSchema<T>(ServiceTypeInfo type) where T : ISwaggerSchema, new()
		{
			switch (type.Kind)
			{
			case ServiceTypeKind.String:
				return new T { Type = SwaggerSchemaType.String };
			case ServiceTypeKind.Boolean:
				return new T { Type = SwaggerSchemaType.Boolean };
			case ServiceTypeKind.Double:
				return new T { Type = SwaggerSchemaType.Number, Format = SwaggerSchemaTypeFormat.Double };
			case ServiceTypeKind.Int32:
				return new T { Type = SwaggerSchemaType.Integer, Format = SwaggerSchemaTypeFormat.Int32 };
			case ServiceTypeKind.Int64:
				return new T { Type = SwaggerSchemaType.Integer, Format = SwaggerSchemaTypeFormat.Int64 };
			case ServiceTypeKind.Decimal:
				return new T { Type = SwaggerSchemaType.Number, Format = SwaggerSchemaTypeFormat.Decimal };
			case ServiceTypeKind.Bytes:
				return new T { Type = SwaggerSchemaType.String, Format = SwaggerSchemaTypeFormat.Byte };
			case ServiceTypeKind.Object:
				return new T { Type = SwaggerSchemaType.Object };
			case ServiceTypeKind.Error:
				return GetErrorSchemaRef<T>();
			case ServiceTypeKind.Dto:
				return GetDtoSchemaRef<T>(type.Dto);
			case ServiceTypeKind.Enum:
				return GetEnumSchema<T>(type.Enum);
			case ServiceTypeKind.Result:
				return GetResultTypeRef<T>(type);
			case ServiceTypeKind.Array:
				return GetArrayOfSchema<T>(type.ValueType);
			case ServiceTypeKind.Map:
				return (T) (object) GetMapOfSchema(type.ValueType);
			default:
				throw new InvalidOperationException("Unexpected field type kind: " + type.Kind);
			}
		}

		private static SwaggerSchema GetDtoSchema(ServiceInfo serviceInfo, ServiceDtoInfo dtoInfo)
		{
			var propertiesObject = new OurDictionary<string, SwaggerSchema>();

			foreach (var fieldInfo in dtoInfo.Fields)
			{
				SwaggerSchema propertyObject = GetTypeSchema<SwaggerSchema>(serviceInfo.GetFieldType(fieldInfo));
				if (propertyObject.Ref == null)
				{
					propertyObject.Description = GetSummaryOrNull(fieldInfo);
					propertyObject.Obsolete = GetObsoleteOrNull(fieldInfo);
				}
				propertiesObject[fieldInfo.Name] = propertyObject;
			}

			return new SwaggerSchema
			{
				Type = SwaggerSchemaType.Object,
				Description = GetSummaryOrNull(dtoInfo),
				Properties = propertiesObject,
				Obsolete = GetObsoleteOrNull(dtoInfo),
				Remarks = GetRemarksOrNull(dtoInfo),
			};
		}

		private static T GetDtoSchemaRef<T>(ServiceDtoInfo dtoInfo) where T : ISwaggerSchema, new()
		{
			return new T
			{
				Ref = "#/definitions/" + dtoInfo.Name,
			};
		}

		private static T GetEnumSchema<T>(ServiceEnumInfo enumInfo) where T : ISwaggerSchema, new()
		{
			return new T
			{
				Type = SwaggerSchemaType.String,
				Enum = enumInfo.Values.Select(x => (JToken) x.Name).ToList(),
			};
		}

		private static T GetErrorSchemaRef<T>() where T : ISwaggerSchema, new()
		{
			return new T
			{
				Ref = "#/definitions/Error",
			};
		}

		private static T GetResultTypeRef<T>(ServiceTypeInfo type) where T : ISwaggerSchema, new()
		{
			return new T
			{
				Ref = "#/definitions/" + GetTypeAsDtoName(type),
			};
		}

		private static T GetArrayOfSchema<T>(ServiceTypeInfo type) where T : ISwaggerSchema, new()
		{
			return new T
			{
				Type = SwaggerSchemaType.Array,
				Items = GetTypeSchema<SwaggerSchema>(type),
			};
		}

		private static SwaggerSchema GetMapOfSchema(ServiceTypeInfo type)
		{
			return new SwaggerSchema
			{
				Type = SwaggerSchemaType.Object,
				AdditionalProperties = GetTypeSchema<SwaggerSchema>(type),
			};
		}

		private static object ConvertJTokenToObject(JToken token)
		{
			if (token is JValue value)
				return value.Value;

			if (token is JArray)
				return token.AsEnumerable().Select(ConvertJTokenToObject).ToList();

			if (token is JObject)
			{
				var dictionary = new OurDictionary<string, object>();
				foreach (var property in token.AsEnumerable().Cast<JProperty>())
					dictionary[property.Name] = ConvertJTokenToObject(property.Value);
				return dictionary;
			}

			throw new InvalidOperationException("Unexpected token: " + token);
		}

		private sealed class OurEventEmitter : ChainedEventEmitter
		{
			public OurEventEmitter(IEventEmitter nextEmitter)
				: base(nextEmitter)
			{
			}

			public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
			{
				// prefer the literal style for multi-line strings
				if (eventInfo.Source.Type == typeof(string) && eventInfo.Style == ScalarStyle.Any && ((string) eventInfo.Source.Value).IndexOf('\n') != -1)
					eventInfo.Style = ScalarStyle.Literal;

				// ensure strings that look like numbers remain strings
				double unused;
				if (eventInfo.Source.Type == typeof(string) && eventInfo.Style == ScalarStyle.Any && double.TryParse((string) eventInfo.Source.Value, out unused))
					eventInfo.Style = ScalarStyle.SingleQuoted;

				base.Emit(eventInfo, emitter);
			}
		}

		private sealed class OurDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
		{
			public OurDictionary()
			{
				m_dictionary = new Dictionary<TKey, TValue>();
				m_keys = new List<TKey>();
			}

			public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
			{
				foreach (var key in m_keys)
					yield return new KeyValuePair<TKey, TValue>(key, m_dictionary[key]);
			}

			public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

			public void Clear()
			{
				m_dictionary.Clear();
				m_keys.Clear();
			}

			public bool Contains(KeyValuePair<TKey, TValue> item) => m_dictionary.Contains(item);

			public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
			{
				throw new NotImplementedException();
			}

			public bool Remove(KeyValuePair<TKey, TValue> item)
			{
				if (!((ICollection<KeyValuePair<TKey, TValue>>) m_dictionary).Remove(item))
					return false;
				m_keys.Remove(item.Key);
				return true;
			}

			public int Count => m_dictionary.Count;

			public bool IsReadOnly => false;

			public void Add(TKey key, TValue value)
			{
				m_dictionary.Add(key, value);
				m_keys.Add(key);
			}

			public bool ContainsKey(TKey key) => m_dictionary.ContainsKey(key);

			public bool Remove(TKey key)
			{
				if (!m_dictionary.Remove(key))
					return false;
				m_keys.Remove(key);
				return true;
			}

			public bool TryGetValue(TKey key, out TValue value) => m_dictionary.TryGetValue(key, out value);

			public TValue this[TKey key]
			{
				get => m_dictionary[key];
				set
				{
					bool replace = m_dictionary.ContainsKey(key);
					m_dictionary[key] = value;
					if (!replace)
						m_keys.Add(key);
				}
			}

			public ICollection<TKey> Keys => m_keys.ToList();

			public ICollection<TValue> Values => m_keys.Select(x => m_dictionary[x]).ToList();

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

			IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

			readonly Dictionary<TKey, TValue> m_dictionary;
			readonly List<TKey> m_keys;
		}
	}
}
