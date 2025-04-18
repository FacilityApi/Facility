namespace Facility.Definition.Http;

/// <summary>
/// The HTTP mapping for a service.
/// </summary>
public sealed class HttpServiceInfo : HttpElementInfo
{
	/// <summary>
	/// Creates an HTTP mapping for a service.
	/// </summary>
	/// <exception cref="ServiceDefinitionException">Thrown if there are errors.</exception>
	public static HttpServiceInfo Create(ServiceInfo serviceInfo) =>
		TryCreate(serviceInfo, out var service, out var errors) ? service : throw new ServiceDefinitionException(errors);

	/// <summary>
	/// Attempts to create an HTTP mapping for a service.
	/// </summary>
	/// <returns>True if there are no errors.</returns>
	/// <remarks>Even if there are errors, an invalid HTTP mapping will be returned.</remarks>
	public static bool TryCreate(ServiceInfo serviceInfo, out HttpServiceInfo httpServiceInfo, out IReadOnlyList<ServiceDefinitionError> errors)
	{
		httpServiceInfo = new HttpServiceInfo(serviceInfo);
		errors = [.. httpServiceInfo.GetValidationErrors()];
		return errors.Count == 0;
	}

	/// <summary>
	/// The service.
	/// </summary>
	public ServiceInfo Service { get; }

	/// <summary>
	/// The URL of the HTTP service.
	/// </summary>
	public string? Url { get; }

	/// <summary>
	/// The HTTP mapping for all methods (normal methods and event methods).
	/// </summary>
	public IReadOnlyList<HttpMethodInfo> AllMethods { get; }

	/// <summary>
	/// The HTTP mapping for normal methods.
	/// </summary>
	public IReadOnlyList<HttpMethodInfo> Methods => [.. AllMethods.Where(x => x.ServiceMethod.Kind == ServiceMethodKind.Normal)];

	/// <summary>
	/// The HTTP mapping for event methods.
	/// </summary>
	public IReadOnlyList<HttpMethodInfo> Events => [.. AllMethods.Where(x => x.ServiceMethod.Kind == ServiceMethodKind.Event)];

	/// <summary>
	/// The HTTP mapping for the error sets.
	/// </summary>
	public IReadOnlyList<HttpErrorSetInfo> ErrorSets { get; }

	/// <summary>
	/// The children of the element, if any.
	/// </summary>
	public override IEnumerable<HttpElementInfo> GetChildren() => AllMethods.AsEnumerable<HttpElementInfo>().Concat(ErrorSets);

	private HttpServiceInfo(ServiceInfo serviceInfo)
	{
		Service = serviceInfo;

		foreach (var parameter in GetHttpParameters(serviceInfo))
		{
			if (parameter.Name == "url")
				Url = parameter.Value;
			else
				AddInvalidHttpParameterError(parameter);
		}

		foreach (var descendant in serviceInfo.GetElementAndDescendants().OfType<ServiceElementWithAttributesInfo>())
		{
			var httpAttributes = descendant.GetAttributes("http");
			if (httpAttributes.Count != 0)
			{
				if (!(descendant is ServiceInfo or ServiceMethodInfo or ServiceFieldInfo or ServiceErrorSetInfo or ServiceErrorInfo))
					AddValidationError(ServiceDefinitionUtility.CreateUnexpectedAttributeError(httpAttributes[0]));
				else if (httpAttributes.Count > 1)
					AddValidationError(ServiceDefinitionUtility.CreateDuplicateAttributeError(httpAttributes[1]));
			}
		}

		AllMethods = [.. serviceInfo.AllMethods.Select(x => new HttpMethodInfo(x, serviceInfo))];
		ErrorSets = [.. serviceInfo.ErrorSets.Select(x => new HttpErrorSetInfo(x))];

		var methodsByRoute = AllMethods.OrderBy(x => x, HttpMethodInfo.ByRouteComparer).ToList();
		for (var index = 1; index < methodsByRoute.Count; index++)
		{
			var left = methodsByRoute[index - 1];
			var right = methodsByRoute[index];
			if (HttpMethodInfo.ByRouteComparer.Compare(left, right) == 0)
				AddValidationError(new ServiceDefinitionError($"Methods '{left.ServiceMethod.Name}' and '{right.ServiceMethod.Name}' have the same route: {right.Method} {right.Path}", right.ServiceMethod.Position));
		}
	}
}
