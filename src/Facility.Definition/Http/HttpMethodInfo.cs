using System.Net;
using System.Text.RegularExpressions;

namespace Facility.Definition.Http;

/// <summary>
/// The HTTP mapping for a service method.
/// </summary>
public sealed class HttpMethodInfo : HttpElementInfo
{
	/// <summary>
	/// The service method.
	/// </summary>
	public ServiceMethodInfo ServiceMethod { get; }

	/// <summary>
	/// The HTTP method (e.g. GET or POST).
	/// </summary>
	public string Method { get; }

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
	public HttpBodyFieldInfo? RequestBodyField { get; }

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

		Method = "POST";
		Path = $"/{methodInfo.Name}";
		HttpStatusCode? statusCode = null;
		var isEvent = methodInfo.Kind == ServiceMethodKind.Event;

		foreach (var methodParameter in GetHttpParameters(methodInfo))
		{
			switch (methodParameter.Name)
			{
				case "method":
					Method = GetHttpMethodFromParameter(methodParameter);
					break;

				case "path":
					if (methodParameter.Value.Length == 0 || methodParameter.Value[0] != '/')
						AddValidationError(new ServiceDefinitionError("'path' value must start with a slash.", methodParameter.GetPart(ServicePartKind.Value)?.Position));
					Path = methodParameter.Value;
					break;

				case "code":
					statusCode = TryParseStatusCodeInteger(methodParameter);
					break;

				default:
					AddInvalidHttpParameterError(methodParameter);
					break;
			}
		}

		var pathParameterNames = new HashSet<string>(GetPathParameterNames(Path));

		var requestPathFields = new List<HttpPathFieldInfo>();
		var requestQueryFields = new List<HttpQueryFieldInfo>();
		var requestNormalFields = new List<HttpNormalFieldInfo>();
		HttpBodyFieldInfo? requestBodyField = null;
		var requestHeaderFields = new List<HttpHeaderFieldInfo>();

		foreach (var requestField in methodInfo.RequestFields)
		{
			var from = requestField.TryGetHttpAttribute()?.TryGetParameterValue("from");
			if (from == "path")
			{
				if (!IsValidSimpleField(requestField, serviceInfo))
					AddValidationError(new ServiceDefinitionError("Type not supported by path field.", requestField.Position));
				var pathInfo = new HttpPathFieldInfo(requestField);
				if (!pathParameterNames.Remove(pathInfo.Name))
					AddValidationError(new ServiceDefinitionError("Path request field has no placeholder in the method path.", requestField.Position));
				requestPathFields.Add(pathInfo);
			}
			else if (from == "query")
			{
				if (!IsValidSimpleField(requestField, serviceInfo))
					AddValidationError(new ServiceDefinitionError("Type not supported by query field.", requestField.Position));
				requestQueryFields.Add(new HttpQueryFieldInfo(requestField));
			}
			else if (from == "normal")
			{
				if (IsNoContentMethod(Method))
					AddValidationError(new ServiceDefinitionError($"HTTP {Method} does not support normal fields.", requestField.Position));
				requestNormalFields.Add(new HttpNormalFieldInfo(requestField));
			}
			else if (from == "body")
			{
				if (!IsValidRequestBodyField(requestField, serviceInfo))
					AddValidationError(new ServiceDefinitionError("Type not supported by body request field.", requestField.Position));
				if (requestBodyField is not null)
					AddValidationError(new ServiceDefinitionError("Requests do not support multiple body fields.", requestField.Position));
				var bodyInfo = new HttpBodyFieldInfo(requestField);
				if (bodyInfo.StatusCode is not null)
					AddValidationError(new ServiceDefinitionError("Request fields do not support status codes.", requestField.Position));
				requestBodyField = bodyInfo;
			}
			else if (from == "header")
			{
				if (!IsValidSimpleField(requestField, serviceInfo))
					AddValidationError(new ServiceDefinitionError("Type not supported by header request field.", requestField.Position));
				requestHeaderFields.Add(new HttpHeaderFieldInfo(requestField));
			}
			else if (from == "custom")
			{
			}
			else if (from is not null)
			{
				AddValidationError(new ServiceDefinitionError($"Unsupported 'from' parameter of 'http' attribute: '{from}'", requestField.Position));
			}
			else if (pathParameterNames.Remove(requestField.Name))
			{
				if (!IsValidSimpleField(requestField, serviceInfo))
					AddValidationError(new ServiceDefinitionError("Type not supported by path field.", requestField.Position));
				requestPathFields.Add(new HttpPathFieldInfo(requestField));
			}
			else if (Method is "GET" or "DELETE")
			{
				if (!IsValidSimpleField(requestField, serviceInfo))
					AddValidationError(new ServiceDefinitionError("Type not supported by query field.", requestField.Position));
				requestQueryFields.Add(new HttpQueryFieldInfo(requestField));
			}
			else
			{
				requestNormalFields.Add(new HttpNormalFieldInfo(requestField));
			}
		}

		if (pathParameterNames.Count != 0)
			AddValidationError(new ServiceDefinitionError($"Unused path parameter '{pathParameterNames.First()}'.", methodInfo.Position));
		if (requestBodyField is not null && requestNormalFields.Count != 0)
			AddValidationError(new ServiceDefinitionError("A request cannot have a normal field and a body field.", requestBodyField.ServiceField.Position));

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
			var from = responseField.TryGetHttpAttribute()?.TryGetParameterValue("from");
			switch (from)
			{
				case "path" or "query":
					AddValidationError(new ServiceDefinitionError("Response fields must not be path or query fields.", responseField.Position));
					break;

				case "body":
					if (!IsValidResponseBodyField(responseField, serviceInfo))
						AddValidationError(new ServiceDefinitionError("Type not supported by body response field.", responseField.Position));
					responseBodyFields.Add(new HttpBodyFieldInfo(responseField));
					break;

				case "header":
					if (!IsValidSimpleField(responseField, serviceInfo))
						AddValidationError(new ServiceDefinitionError("Type not supported by header response field.", responseField.Position));
					responseHeaderFields.Add(new HttpHeaderFieldInfo(responseField));
					break;

				case "normal" or null:
					responseNormalFields.Add(new HttpNormalFieldInfo(responseField));
					break;

				case "custom":
					break;

				default:
					AddValidationError(new ServiceDefinitionError($"Unsupported 'from' parameter of 'http' attribute: '{from}'", responseField.Position));
					break;
			}
		}

		ResponseHeaderFields = responseHeaderFields;
		ValidResponses = [.. GetValidResponses(serviceInfo, statusCode, responseNormalFields, responseBodyFields).OrderBy(x => x.StatusCode)];

		if (isEvent)
		{
			if (responseHeaderFields.Count != 0)
				AddValidationError(new ServiceDefinitionError("Events do not support response headers.", methodInfo.Position));
			if (ValidResponses.Count != 1)
				AddValidationError(new ServiceDefinitionError("Events do not support multiple status codes.", methodInfo.Position));
		}

		var duplicateStatusCode = ValidResponses.GroupBy(x => x.StatusCode).FirstOrDefault(x => x.Count() > 1);
		if (duplicateStatusCode is not null)
			AddValidationError(new ServiceDefinitionError($"Multiple handlers for status code {(int) duplicateStatusCode.Key}.", methodInfo.Position));
	}

	/// <summary>
	/// The children of the element, if any.
	/// </summary>
	public override IEnumerable<HttpElementInfo> GetChildren() => PathFields.AsEnumerable<HttpElementInfo>()
		.Concat(QueryFields).Concat(RequestNormalFields).Concat(new[] { RequestBodyField }.Where(x => x is not null))
		.Concat(RequestHeaderFields).Concat(ResponseHeaderFields).Concat(ValidResponses)!;

	private string GetHttpMethodFromParameter(ServiceAttributeParameterInfo parameter)
	{
		var httpMethod = parameter.Value.ToUpperInvariant();
		if (!s_httpMethods.Contains(httpMethod))
		{
			AddValidationError(new ServiceDefinitionError($"Unsupported HTTP method '{httpMethod}'.", parameter.GetPart(ServicePartKind.Value)?.Position));
			return "POST";
		}

		return httpMethod;
	}

	private static bool IsValidSimpleField(ServiceFieldInfo fieldInfo, ServiceInfo serviceInfo)
	{
		var fieldType = serviceInfo.GetFieldType(fieldInfo);
		var fieldTypeKind = fieldType?.Kind;
		if (fieldTypeKind is null)
			return false;

		if (fieldTypeKind == ServiceTypeKind.Array)
			fieldTypeKind = fieldType!.ValueType!.Kind;

		return fieldTypeKind is ServiceTypeKind.String or ServiceTypeKind.Boolean or ServiceTypeKind.Float or ServiceTypeKind.Double or ServiceTypeKind.Int32 or ServiceTypeKind.Int64 or ServiceTypeKind.Decimal or ServiceTypeKind.Enum or ServiceTypeKind.ExternalEnum or ServiceTypeKind.DateTime;
	}

	private static bool IsValidRequestBodyField(ServiceFieldInfo fieldInfo, ServiceInfo serviceInfo)
	{
		var fieldTypeKind = serviceInfo.GetFieldType(fieldInfo)?.Kind;
		return fieldTypeKind is ServiceTypeKind.Object or ServiceTypeKind.Error or ServiceTypeKind.Dto or ServiceTypeKind.Result or ServiceTypeKind.Array or ServiceTypeKind.Map or ServiceTypeKind.Bytes or ServiceTypeKind.String or ServiceTypeKind.ExternalDto;
	}

	private static bool IsValidResponseBodyField(ServiceFieldInfo fieldInfo, ServiceInfo serviceInfo) =>
		IsValidRequestBodyField(fieldInfo, serviceInfo) ||
		serviceInfo.GetFieldType(fieldInfo)?.Kind == ServiceTypeKind.Boolean;

	private IEnumerable<HttpResponseInfo> GetValidResponses(ServiceInfo serviceInfo, HttpStatusCode? statusCode, List<HttpNormalFieldInfo> responseNormalFields, List<HttpBodyFieldInfo> responseBodyFields)
	{
		foreach (var responseBodyField in responseBodyFields)
		{
			// use the status code on the field or the default: OK or NoContent
			var isBoolean = serviceInfo.GetFieldType(responseBodyField.ServiceField)?.Kind == ServiceTypeKind.Boolean;
			var bodyStatusCode = responseBodyField.StatusCode ?? (isBoolean ? HttpStatusCode.NoContent : HttpStatusCode.OK);

			// 204 and 304 don't support content
			if (IsNoContentStatusCode(bodyStatusCode) && !isBoolean)
				AddValidationError(new ServiceDefinitionError($"A body field with HTTP status code {(int) bodyStatusCode} must be Boolean.", responseBodyField.ServiceField.Position));

			yield return new HttpResponseInfo(bodyStatusCode, responseBodyField);
		}

		// if the DTO has a status code, or there are any normal fields, or there are no body fields, the DTO must represent a status code
		HttpStatusCode? responseStatusCode = null;
		if (statusCode is not null)
			responseStatusCode = statusCode;
		else if (responseNormalFields.Count != 0 || responseBodyFields.Count == 0)
			responseStatusCode = HttpStatusCode.OK;
		if (responseStatusCode is not null)
		{
			// 204 and 304 don't support content
			if (IsNoContentStatusCode(responseStatusCode) && responseNormalFields.Count != 0)
				AddValidationError(new ServiceDefinitionError($"HTTP status code {(int) responseStatusCode} does not support normal fields.", responseNormalFields[0].ServiceField.Position));

			yield return new HttpResponseInfo(responseStatusCode.Value, responseNormalFields);
		}
	}

	private static bool IsNoContentMethod(string method) => method is "GET" or "DELETE";

	private static bool IsNoContentStatusCode(HttpStatusCode? statusCode) => statusCode is HttpStatusCode.NoContent or HttpStatusCode.NotModified;

	private static List<string> GetPathParameterNames(string routePath) =>
		[.. s_regexPathParameterRegex.Matches(routePath).Cast<Match>().Select(x => x.Groups[1].ToString())];

	private sealed class NestedByRouteComparer : IComparer<HttpMethodInfo>
	{
		public int Compare(HttpMethodInfo? left, HttpMethodInfo? right)
		{
			if (left is null)
				return right is null ? 0 : -1;
			if (right is null)
				return 1;

			var leftParts = left.Path.Split(s_oneSlash, StringSplitOptions.RemoveEmptyEntries);
			var rightParts = right.Path.Split(s_oneSlash, StringSplitOptions.RemoveEmptyEntries);
			var partIndex = 0;
			while (true)
			{
				var leftPart = partIndex < leftParts.Length ? leftParts[partIndex] : null;
				var rightPart = partIndex < rightParts.Length ? rightParts[partIndex] : null;
				if (leftPart is null && rightPart is null)
					break;
				if (leftPart is null)
					return -1;
				if (rightPart is null)
					return 1;

				var leftPlaceholder = leftPart[0] == '{';
				var rightPlaceholder = rightPart[0] == '{';
				if (!leftPlaceholder || !rightPlaceholder)
				{
					if (leftPlaceholder || rightPlaceholder)
						return leftPlaceholder ? 1 : -1;

					var partCompare = string.CompareOrdinal(leftPart, rightPart);
					if (partCompare != 0)
						return partCompare;
				}

				partIndex++;
			}

			var leftRank = s_httpMethods.IndexOf(left.Method);
			var rightRank = s_httpMethods.IndexOf(right.Method);
			if (leftRank >= 0 && rightRank >= 0)
				return leftRank.CompareTo(rightRank);
			if (leftRank >= 0)
				return -1;
			if (rightRank >= 0)
				return 1;

			return string.CompareOrdinal(left.Method, right.Method);
		}
	}

	private static readonly List<string> s_httpMethods = ["GET", "POST", "PUT", "PATCH", "DELETE"];
	private static readonly Regex s_regexPathParameterRegex = new(@"\{([^\}]+)\}", RegexOptions.CultureInvariant);
	private static readonly char[] s_oneSlash = ['/'];
}
