using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Facility.Definition.Http
{
	/// <summary>
	/// The HTTP mapping for a service method.
	/// </summary>
	public sealed class HttpMethodInfo
	{
		/// <summary>
		/// The service method.
		/// </summary>
		public ServiceMethodInfo ServiceMethod { get; }

		/// <summary>
		/// The HTTP method (e.g. GET or POST).
		/// </summary>
		public HttpMethod Method { get; }

		/// <summary>
		/// The path of the method, always starting with a slash.
		/// </summary>
		public string Path { get; }

		/// <summary>
		/// The status code used by the default response. (See ValidResponses.)
		/// </summary>
		public HttpStatusCode? StatusCode { get; }

		/// <summary>
		/// The fields of the request DTO that correspond to path parameters.
		/// </summary>
		public IReadOnlyList<HttpPathFieldInfo> PathFields { get; }

		/// <summary>
		/// The fields of the request DTO that correspond to query parameters.
		/// </summary>
		public IReadOnlyList<HttpQueryFieldInfo> QueryFields { get; }

		/// <summary>
		/// The fields of the request DTO that correspond to normal fields in the request body.
		/// </summary>
		public IReadOnlyList<HttpNormalFieldInfo> RequestNormalFields { get; }

		/// <summary>
		/// The field of the request DTO that corresponds to the entire request body.
		/// </summary>
		public HttpBodyFieldInfo RequestBodyField { get; }

		/// <summary>
		/// The fields of the request DTO that correspond to HTTP headers.
		/// </summary>
		public IReadOnlyList<HttpHeaderFieldInfo> RequestHeaderFields { get; }

		/// <summary>
		/// The fields of the response DTO that correspond to normal fields in the body of the default response. (See ValidResponses.)
		/// </summary>
		public IReadOnlyList<HttpNormalFieldInfo> ResponseNormalFields { get; }

		/// <summary>
		/// The fields of the response DTO that correspond to the entire response body of valid responses. (See ValidResponses.)
		/// </summary>
		public IReadOnlyList<HttpBodyFieldInfo> ResponseBodyFields { get; }

		/// <summary>
		/// The fields of the response DTO that correspond to HTTP headers.
		/// </summary>
		public IReadOnlyList<HttpHeaderFieldInfo> ResponseHeaderFields { get; }

		/// <summary>
		/// The valid responses, as inferred from ResponseNormalFields, ResponseBodyFields, and StatusCode.
		/// </summary>
		public IReadOnlyList<HttpResponseInfo> ValidResponses { get; }

		internal HttpMethodInfo(ServiceMethodInfo methodInfo, ServiceInfo serviceInfo)
		{
			ServiceMethod = methodInfo;

			Method = HttpMethod.Post;
			Path = $"/{methodInfo.Name}";

			foreach (var methodParameter in methodInfo.GetHttpParameters())
			{
				if (methodParameter.Name == "method")
					Method = GetHttpMethodFromParameter(methodParameter);
				else if (methodParameter.Name == "path")
					Path = methodParameter.Value;
				else if (methodParameter.Name == "code")
					StatusCode = HttpAttributeUtility.ParseStatusCodeInteger(methodParameter);
				else
					throw methodParameter.CreateInvalidHttpParameterException();
			}

			var pathParameterNames = new HashSet<string>(GetPathParameterNames(Path));

			var requestPathFields = new List<HttpPathFieldInfo>();
			var requestQueryFields = new List<HttpQueryFieldInfo>();
			var requestNormalFields = new List<HttpNormalFieldInfo>();
			HttpBodyFieldInfo requestBodyField = null;
			var requestHeaderFields = new List<HttpHeaderFieldInfo>();

			foreach (var requestField in methodInfo.RequestFields)
			{
				string from = requestField.TryGetHttpAttribute()?.TryGetParameterValue("from");

				if (from == "path")
				{
					if (!IsValidPathOrQueryField(requestField, serviceInfo))
						throw new ServiceDefinitionException("Request field used in path must use a simple type.", requestField.Position);
					var pathInfo = new HttpPathFieldInfo(requestField);
					if (!pathParameterNames.Remove(pathInfo.ServiceField.Name))
						throw new ServiceDefinitionException("Request field with [http(from: path)] has no placeholder in the method path.", requestField.Position);
					requestPathFields.Add(pathInfo);
				}
				else if (from == "query")
				{
					if (!IsValidPathOrQueryField(requestField, serviceInfo))
						throw new ServiceDefinitionException("Request field used in query must use a simple type.", requestField.Position);
					requestQueryFields.Add(new HttpQueryFieldInfo(requestField));
				}
				else if (from == "normal")
				{
					if (Method == HttpMethod.Get)
						throw new ServiceDefinitionException("HTTP GET does not support normal fields.", requestField.Position);
					requestNormalFields.Add(new HttpNormalFieldInfo(requestField));
				}
				else if (from == "body")
				{
					if (!IsValidRequestBodyField(requestField, serviceInfo))
						throw new ServiceDefinitionException("Request fields with [http(from: body)] must use a DTO type.", requestField.Position);
					if (requestBodyField != null)
						throw new ServiceDefinitionException("Requests do not support multiple [http(from: body)] fields.", requestField.Position);
					var bodyInfo = new HttpBodyFieldInfo(requestField);
					if (bodyInfo.StatusCode != null)
						throw new ServiceDefinitionException("Request fields do not support status codes.", requestField.Position);
					requestBodyField = bodyInfo;
				}
				else if (from == "header")
				{
					if (!IsValidHeaderField(requestField, serviceInfo))
						throw new ServiceDefinitionException("Request fields with [http(from: header)] must use the string type.", requestField.Position);
					requestHeaderFields.Add(new HttpHeaderFieldInfo(requestField));
				}
				else if (from != null)
				{
					throw new ServiceDefinitionException($"Unsupported 'from' parameter of 'http' attribute: '{from}'", requestField.Position);
				}
				else if (pathParameterNames.Remove(requestField.Name))
				{
					if (!IsValidPathOrQueryField(requestField, serviceInfo))
						throw new ServiceDefinitionException("Request field used in path must use a simple type.", requestField.Position);
					requestPathFields.Add(new HttpPathFieldInfo(requestField));
				}
				else if (Method == HttpMethod.Get)
				{
					if (!IsValidPathOrQueryField(requestField, serviceInfo))
						throw new ServiceDefinitionException("Request field used in query must use a simple type.", requestField.Position);
					requestQueryFields.Add(new HttpQueryFieldInfo(requestField));
				}
				else
				{
					requestNormalFields.Add(new HttpNormalFieldInfo(requestField));
				}
			}

			if (pathParameterNames.Count != 0)
				throw new ServiceDefinitionException($"Unused path parameter '{pathParameterNames.First()}'.", methodInfo.Position);
			if (requestBodyField != null && requestNormalFields.Count != 0)
				throw new ServiceDefinitionException("A request cannot have a normal field and a body field.", requestBodyField.ServiceField.Position);

			PathFields = requestPathFields;
			QueryFields = requestQueryFields;
			RequestNormalFields = requestNormalFields;
			RequestBodyField = requestBodyField;
			RequestHeaderFields = requestHeaderFields;

			var responseNormalFields = new List<HttpNormalFieldInfo>();
			var responseBodyFields = new List<HttpBodyFieldInfo>();
			var responseHeaderFields = new List<HttpHeaderFieldInfo>();

			foreach (var responseField in methodInfo.ResponseFields)
			{
				string from = responseField.TryGetHttpAttribute()?.TryGetParameterValue("from");

				if (from == "path")
				{
					throw new ServiceDefinitionException("Response fields do not support '[http(from: path)]'.", responseField.Position);
				}
				else if (from == "query")
				{
					throw new ServiceDefinitionException("Response fields do not support '[http(from: query)]'.", responseField.Position);
				}
				else if (from == "normal")
				{
					responseNormalFields.Add(new HttpNormalFieldInfo(responseField));
				}
				else if (from == "body")
				{
					if (!IsValidResponseBodyField(responseField, serviceInfo))
						throw new ServiceDefinitionException("Response fields with [http(from: body)] must be a DTO or a Boolean.", responseField.Position);
					responseBodyFields.Add(new HttpBodyFieldInfo(responseField));
				}
				else if (from == "header")
				{
					if (!IsValidHeaderField(responseField, serviceInfo))
						throw new ServiceDefinitionException("Response fields with [http(from: header)] must use the string type.", responseField.Position);
					responseHeaderFields.Add(new HttpHeaderFieldInfo(responseField));
				}
				else if (from != null)
				{
					throw new ServiceDefinitionException($"Unsupported 'from' parameter of 'http' attribute: '{from}'", responseField.Position);
				}
				else
				{
					responseNormalFields.Add(new HttpNormalFieldInfo(responseField));
				}
			}

			ResponseNormalFields = responseNormalFields;
			ResponseBodyFields = responseBodyFields;
			ResponseHeaderFields = responseHeaderFields;

			ValidResponses = DoGetValidResponses(serviceInfo).OrderBy(x => x.StatusCode).ToList();

			var duplicateStatusCode = ValidResponses.GroupBy(x => x.StatusCode).FirstOrDefault(x => x.Count() > 1);
			if (duplicateStatusCode != null)
				throw new ServiceDefinitionException($"Multiple handlers for status code {(int) duplicateStatusCode.Key}.", methodInfo.Position);
		}

		private static HttpMethod GetHttpMethodFromParameter(ServiceAttributeParameterInfo parameter)
		{
			try
			{
				return new HttpMethod(parameter.Value.ToUpperInvariant());
			}
			catch (FormatException)
			{
				throw new ServiceDefinitionException($"Invalid HTTP method '{parameter.Value}'.", parameter.Position);
			}
		}

		private static bool IsValidPathOrQueryField(ServiceFieldInfo fieldInfo, ServiceInfo serviceInfo)
		{
			var fieldTypeKind = serviceInfo.GetFieldType(fieldInfo).Kind;
			return fieldTypeKind == ServiceTypeKind.String ||
				fieldTypeKind == ServiceTypeKind.Boolean ||
				fieldTypeKind == ServiceTypeKind.Double ||
				fieldTypeKind == ServiceTypeKind.Int32 ||
				fieldTypeKind == ServiceTypeKind.Int64;
		}

		private static bool IsValidHeaderField(ServiceFieldInfo fieldInfo, ServiceInfo serviceInfo)
		{
			return serviceInfo.GetFieldType(fieldInfo).Kind == ServiceTypeKind.String;
		}

		private static bool IsValidRequestBodyField(ServiceFieldInfo fieldInfo, ServiceInfo serviceInfo)
		{
			return serviceInfo.GetFieldType(fieldInfo).Kind == ServiceTypeKind.Dto;
		}

		private static bool IsValidResponseBodyField(ServiceFieldInfo fieldInfo, ServiceInfo serviceInfo)
		{
			var fieldTypeKind = serviceInfo.GetFieldType(fieldInfo).Kind;
			return fieldTypeKind == ServiceTypeKind.Dto || fieldTypeKind == ServiceTypeKind.Boolean;
		}

		private IEnumerable<HttpResponseInfo> DoGetValidResponses(ServiceInfo serviceInfo)
		{
			foreach (var responseBodyField in ResponseBodyFields)
			{
				var fieldType = serviceInfo.GetFieldType(responseBodyField.ServiceField);

				yield return new HttpResponseInfo(
					statusCode: GetBodyInfoStatusCode(responseBodyField, serviceInfo),
					hasResponseFields: fieldType.Kind == ServiceTypeKind.Dto && fieldType.Dto.Fields.Count != 0,
					responseBodyField: responseBodyField);
			}

			var responseDtoStatusCode = GetResponseDtoStatusCode();
			if (responseDtoStatusCode != null)
			{
				yield return new HttpResponseInfo(
					statusCode: responseDtoStatusCode.Value,
					hasResponseFields: ResponseNormalFields.Count != 0,
					responseBodyField: null);
			}
		}

		private HttpStatusCode GetBodyInfoStatusCode(HttpBodyFieldInfo bodyFieldInfo, ServiceInfo serviceInfo)
		{
			// use the status code on the field
			if (bodyFieldInfo.StatusCode != null)
				return bodyFieldInfo.StatusCode.Value;

			// or the status code on the method
			if (StatusCode != null)
				return StatusCode.Value;

			// or the default: OK or NoContent
			return serviceInfo.GetFieldType(bodyFieldInfo.ServiceField).Kind == ServiceTypeKind.Boolean ? HttpStatusCode.NoContent : HttpStatusCode.OK;
		}

		private HttpStatusCode? GetResponseDtoStatusCode()
		{
			// if there are any normal fields, the DTO must represent a status code
			if (ResponseNormalFields.Count != 0)
				return StatusCode ?? HttpStatusCode.OK;

			// if there are no body fields, the DTO must represent a status code
			if (ResponseBodyFields.Count == 0)
				return StatusCode ?? HttpStatusCode.NoContent;

			// if the DTO has a status code and none of the body fields inherit it, the DTO must represent a status code
			if (StatusCode != null && ResponseBodyFields.All(x => x.StatusCode != null))
				return StatusCode;

			// the DTO does not represent a status code
			return null;
		}

		private static IReadOnlyList<string> GetPathParameterNames(string routePath)
		{
			return s_regexPathParameterRegex.Matches(routePath).Cast<Match>().Select(x => x.Groups[1].ToString()).ToList();
		}

		static readonly Regex s_regexPathParameterRegex = new Regex(@"\{([a-zA-Z][a-zA-Z0-9]*)\}", RegexOptions.CultureInvariant);
	}
}
