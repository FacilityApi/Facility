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
		/// The fields of the response DTO that correspond to HTTP headers.
		/// </summary>
		public IReadOnlyList<HttpHeaderFieldInfo> ResponseHeaderFields { get; }

		/// <summary>
		/// The valid responses.
		/// </summary>
		public IReadOnlyList<HttpResponseInfo> ValidResponses { get; }

		/// <summary>
		/// Compares service methods by HTTP route.
		/// </summary>
		/// <remarks>Orders methods by path, then by HTTP method. Critically, it orders potentially ambiguous routes
		/// in the order that they should be considered, e.g. `/widgets/query` before `/widgets/{id}`.</remarks>
		public static readonly IComparer<HttpMethodInfo> ByRouteComparer = new NestedByRouteComparer();

		internal HttpMethodInfo(ServiceMethodInfo methodInfo, ServiceInfo serviceInfo)
		{
			ServiceMethod = methodInfo;

			Method = HttpMethod.Post;
			Path = $"/{methodInfo.Name}";
			HttpStatusCode? statusCode = null;

			foreach (var methodParameter in methodInfo.GetHttpParameters())
			{
				if (methodParameter.Name == "method")
				{
					Method = GetHttpMethodFromParameter(methodParameter);
				}
				else if (methodParameter.Name == "path")
				{
					if (methodParameter.Value.Length == 0 || methodParameter.Value[0] != '/')
						m_errors.Add(new ServiceDefinitionError("'path' value must start with a slash.", methodParameter.Position));
					Path = methodParameter.Value;
				}
				else if (methodParameter.Name == "code")
				{
					statusCode = HttpAttributeUtility.ParseStatusCodeInteger(methodParameter);
				}
				else
				{
					m_errors.Add(methodParameter.CreateInvalidHttpParameterError());
				}
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
						m_errors.Add(new ServiceDefinitionError("Request field used in path must use a simple type.", requestField.Position));
					var pathInfo = new HttpPathFieldInfo(requestField);
					if (!pathParameterNames.Remove(pathInfo.Name))
						m_errors.Add(new ServiceDefinitionError("Request field with [http(from: path)] has no placeholder in the method path.", requestField.Position));
					requestPathFields.Add(pathInfo);
				}
				else if (from == "query")
				{
					if (!IsValidPathOrQueryField(requestField, serviceInfo))
						m_errors.Add(new ServiceDefinitionError("Request field used in query must use a simple type.", requestField.Position));
					requestQueryFields.Add(new HttpQueryFieldInfo(requestField));
				}
				else if (from == "normal")
				{
					if (Method == HttpMethod.Get || Method == HttpMethod.Delete)
						m_errors.Add(new ServiceDefinitionError($"HTTP {Method} does not support normal fields.", requestField.Position));
					requestNormalFields.Add(new HttpNormalFieldInfo(requestField));
				}
				else if (from == "body")
				{
					if (!IsValidRequestBodyField(requestField, serviceInfo))
						m_errors.Add(new ServiceDefinitionError("Request fields with [http(from: body)] must not use a primitive type.", requestField.Position));
					if (requestBodyField != null)
						m_errors.Add(new ServiceDefinitionError("Requests do not support multiple [http(from: body)] fields.", requestField.Position));
					var bodyInfo = new HttpBodyFieldInfo(requestField);
					if (bodyInfo.StatusCode != null)
						m_errors.Add(new ServiceDefinitionError("Request fields do not support status codes.", requestField.Position));
					requestBodyField = bodyInfo;
				}
				else if (from == "header")
				{
					if (!IsValidHeaderField(requestField, serviceInfo))
						m_errors.Add(new ServiceDefinitionError("Request fields with [http(from: header)] must use the string type.", requestField.Position));
					requestHeaderFields.Add(new HttpHeaderFieldInfo(requestField));
				}
				else if (from != null)
				{
					m_errors.Add(new ServiceDefinitionError($"Unsupported 'from' parameter of 'http' attribute: '{from}'", requestField.Position));
				}
				else if (pathParameterNames.Remove(requestField.Name))
				{
					if (!IsValidPathOrQueryField(requestField, serviceInfo))
						m_errors.Add(new ServiceDefinitionError("Request field used in path must use a simple type.", requestField.Position));
					requestPathFields.Add(new HttpPathFieldInfo(requestField));
				}
				else if (Method == HttpMethod.Get || Method == HttpMethod.Delete)
				{
					if (!IsValidPathOrQueryField(requestField, serviceInfo))
						m_errors.Add(new ServiceDefinitionError("Request field used in query must use a simple type.", requestField.Position));
					requestQueryFields.Add(new HttpQueryFieldInfo(requestField));
				}
				else
				{
					requestNormalFields.Add(new HttpNormalFieldInfo(requestField));
				}
			}

			if (pathParameterNames.Count != 0)
				m_errors.Add(new ServiceDefinitionError($"Unused path parameter '{pathParameterNames.First()}'.", methodInfo.Position));
			if (requestBodyField != null && requestNormalFields.Count != 0)
				m_errors.Add(new ServiceDefinitionError("A request cannot have a normal field and a body field.", requestBodyField.ServiceField.Position));

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

				if (from == "path" || from == "query")
				{
					m_errors.Add(new ServiceDefinitionError($"Response fields do not support '[http(from: {from})]'.", responseField.Position));
				}
				else if (from == "body")
				{
					if (!IsValidResponseBodyField(responseField, serviceInfo))
						m_errors.Add(new ServiceDefinitionError("Response fields with [http(from: body)] must be a non-primitive type or a Boolean.", responseField.Position));
					responseBodyFields.Add(new HttpBodyFieldInfo(responseField));
				}
				else if (from == "header")
				{
					if (!IsValidHeaderField(responseField, serviceInfo))
						m_errors.Add(new ServiceDefinitionError("Response fields with [http(from: header)] must use the string type.", responseField.Position));
					responseHeaderFields.Add(new HttpHeaderFieldInfo(responseField));
				}
				else if (from == "normal" || from == null)
				{
					responseNormalFields.Add(new HttpNormalFieldInfo(responseField));
				}
				else
				{
					m_errors.Add(new ServiceDefinitionError($"Unsupported 'from' parameter of 'http' attribute: '{from}'", responseField.Position));
				}
			}

			ResponseHeaderFields = responseHeaderFields;
			ValidResponses = GetValidResponses(serviceInfo, statusCode, responseNormalFields, responseBodyFields).OrderBy(x => x.StatusCode).ToList();

			var duplicateStatusCode = ValidResponses.GroupBy(x => x.StatusCode).FirstOrDefault(x => x.Count() > 1);
			if (duplicateStatusCode != null)
				m_errors.Add(new ServiceDefinitionError($"Multiple handlers for status code {(int) duplicateStatusCode.Key}.", methodInfo.Position));
		}

		internal IEnumerable<ServiceDefinitionError> GetValidationErrors()
		{
			return m_errors
				.Concat(PathFields.SelectMany(x => x.GetValidationErrors()))
				.Concat(QueryFields.SelectMany(x => x.GetValidationErrors()))
				.Concat(RequestNormalFields.SelectMany(x => x.GetValidationErrors()))
				.Concat(RequestBodyField?.GetValidationErrors() ?? Enumerable.Empty<ServiceDefinitionError>())
				.Concat(RequestHeaderFields.SelectMany(x => x.GetValidationErrors()))
				.Concat(ResponseHeaderFields.SelectMany(x => x.GetValidationErrors()))
				.Concat(ValidResponses.SelectMany(x => x.GetValidationErrors()));
		}

		private HttpMethod GetHttpMethodFromParameter(ServiceAttributeParameterInfo parameter)
		{
			try
			{
				return new HttpMethod(parameter.Value.ToUpperInvariant());
			}
			catch (FormatException)
			{
				m_errors.Add(new ServiceDefinitionError($"Invalid HTTP method '{parameter.Value}'.", parameter.Position));
				return null;
			}
		}

		private static bool IsValidPathOrQueryField(ServiceFieldInfo fieldInfo, ServiceInfo serviceInfo)
		{
			var fieldTypeKind = serviceInfo.GetFieldType(fieldInfo).Kind;
			return fieldTypeKind == ServiceTypeKind.String ||
				fieldTypeKind == ServiceTypeKind.Boolean ||
				fieldTypeKind == ServiceTypeKind.Double ||
				fieldTypeKind == ServiceTypeKind.Int32 ||
				fieldTypeKind == ServiceTypeKind.Int64 ||
				fieldTypeKind == ServiceTypeKind.Decimal ||
				fieldTypeKind == ServiceTypeKind.Enum;
		}

		private static bool IsValidHeaderField(ServiceFieldInfo fieldInfo, ServiceInfo serviceInfo)
		{
			return serviceInfo.GetFieldType(fieldInfo).Kind == ServiceTypeKind.String;
		}

		private static bool IsValidRequestBodyField(ServiceFieldInfo fieldInfo, ServiceInfo serviceInfo)
		{
			var fieldTypeKind = serviceInfo.GetFieldType(fieldInfo).Kind;
			return fieldTypeKind == ServiceTypeKind.Object ||
				fieldTypeKind == ServiceTypeKind.Error ||
				fieldTypeKind == ServiceTypeKind.Dto ||
				fieldTypeKind == ServiceTypeKind.Result ||
				fieldTypeKind == ServiceTypeKind.Array ||
				fieldTypeKind == ServiceTypeKind.Map;
		}

		private static bool IsValidResponseBodyField(ServiceFieldInfo fieldInfo, ServiceInfo serviceInfo)
		{
			return IsValidRequestBodyField(fieldInfo, serviceInfo) ||
				serviceInfo.GetFieldType(fieldInfo).Kind == ServiceTypeKind.Boolean;
		}

		private IEnumerable<HttpResponseInfo> GetValidResponses(ServiceInfo serviceInfo, HttpStatusCode? statusCode, IReadOnlyList<HttpNormalFieldInfo> responseNormalFields, IReadOnlyList<HttpBodyFieldInfo> responseBodyFields)
		{
			foreach (var responseBodyField in responseBodyFields)
			{
				// use the status code on the field or the default: OK or NoContent
				HttpStatusCode bodyStatusCode;
				bool isBoolean = serviceInfo.GetFieldType(responseBodyField.ServiceField).Kind == ServiceTypeKind.Boolean;
				if (responseBodyField.StatusCode != null)
					bodyStatusCode = responseBodyField.StatusCode.Value;
				else
					bodyStatusCode = isBoolean ? HttpStatusCode.NoContent : HttpStatusCode.OK;

				// 204 and 304 don't support content
				if (IsNoContentStatusCode(bodyStatusCode) && !isBoolean)
					m_errors.Add(new ServiceDefinitionError($"A body field with HTTP status code {(int) bodyStatusCode} must be Boolean.", responseBodyField.ServiceField.Position));

				yield return new HttpResponseInfo(
					statusCode: bodyStatusCode,
					bodyField: responseBodyField);
			}

			// if the DTO has a status code, or there are any normal fields, or there are no body fields, the DTO must represent a status code
			HttpStatusCode? responseStatusCode = null;
			if (statusCode != null)
				responseStatusCode = statusCode;
			else if (responseNormalFields.Count != 0 || responseBodyFields.Count == 0)
				responseStatusCode = HttpStatusCode.OK;
			if (responseStatusCode != null)
			{
				// 204 and 304 don't support content
				if (IsNoContentStatusCode(responseStatusCode) && responseNormalFields.Count != 0)
					m_errors.Add(new ServiceDefinitionError($"HTTP status code {(int) responseStatusCode} does not support normal fields.", responseNormalFields[0].ServiceField.Position));

				yield return new HttpResponseInfo(
					statusCode: responseStatusCode.Value,
					normalFields: responseNormalFields);
			}
		}

		private static bool IsNoContentStatusCode(HttpStatusCode? statusCode)
		{
			return statusCode == HttpStatusCode.NoContent || statusCode == HttpStatusCode.NotModified;
		}

		private static IReadOnlyList<string> GetPathParameterNames(string routePath)
		{
			return s_regexPathParameterRegex.Matches(routePath).Cast<Match>().Select(x => x.Groups[1].ToString()).ToList();
		}

		private class NestedByRouteComparer : IComparer<HttpMethodInfo>
		{
			public int Compare(HttpMethodInfo left, HttpMethodInfo right)
			{
				if (left == null)
					return right == null ? 0 : -1;
				if (right == null)
					return 1;

				var leftParts = left.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
				var rightParts = right.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
				int partIndex = 0;
				while (true)
				{
					string leftPart = partIndex < leftParts.Length ? leftParts[partIndex] : null;
					string rightPart = partIndex < rightParts.Length ? rightParts[partIndex] : null;
					if (leftPart == null && rightPart == null)
						break;
					if (leftPart == null)
						return -1;
					if (rightPart == null)
						return 1;

					bool leftPlaceholder = leftPart[0] == '{';
					bool rightPlaceholder = rightPart[0] == '{';
					if (!leftPlaceholder || !rightPlaceholder)
					{
						if (leftPlaceholder || rightPlaceholder)
							return leftPlaceholder ? 1 : -1;

						int partCompare = string.CompareOrdinal(leftPart, rightPart);
						if (partCompare != 0)
							return partCompare;
					}

					partIndex++;
				}

				int leftRank = s_httpMethods.IndexOf(left.Method);
				int rightRank = s_httpMethods.IndexOf(right.Method);
				if (leftRank >= 0 && rightRank >= 0)
					return leftRank.CompareTo(rightRank);
				if (leftRank >= 0)
					return -1;
				if (rightRank >= 0)
					return 1;

				return string.CompareOrdinal(left.Method?.ToString(), right.Method?.ToString());
			}

			static readonly List<HttpMethod> s_httpMethods = new List<HttpMethod> { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, new HttpMethod("PATCH"), HttpMethod.Delete };
		}

		static readonly Regex s_regexPathParameterRegex = new Regex(@"\{([^\}]+)\}", RegexOptions.CultureInvariant);

		private readonly List<ServiceDefinitionError> m_errors = new List<ServiceDefinitionError>();
	}
}
