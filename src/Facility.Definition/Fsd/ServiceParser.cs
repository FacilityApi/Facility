namespace Facility.Definition.Fsd;

/// <summary>
/// Base class for service parsers.
/// </summary>
public abstract class ServiceParser
{
	/// <summary>
	/// Parses the text into a service definition.
	/// </summary>
	/// <exception cref="ServiceDefinitionException">Thrown if parsing fails or the service would be invalid.</exception>
	public ServiceInfo ParseDefinition(ServiceDefinitionText text)
	{
		if (!TryParseDefinition(text, out var service, out var errors))
			throw new ServiceDefinitionException(errors);

		return service!;
	}

	/// <summary>
	/// Parses the text into a service definition.
	/// </summary>
	/// <returns>True if parsing succeeds and the service is valid, i.e. there are no errors.</returns>
	/// <remarks>Even if parsing fails, an invalid service may be returned.</remarks>
	public bool TryParseDefinition(ServiceDefinitionText text, out ServiceInfo? service, out IReadOnlyList<ServiceDefinitionError> errors) =>
		TryParseDefinitionCore(text, out service, out errors);

	/// <summary>
	/// Implements TryParseDefinition.
	/// </summary>
	protected abstract bool TryParseDefinitionCore(ServiceDefinitionText text, out ServiceInfo? service, out IReadOnlyList<ServiceDefinitionError> errors);
}
