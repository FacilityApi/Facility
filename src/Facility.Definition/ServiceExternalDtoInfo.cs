namespace Facility.Definition;

/// <summary>
/// An external service DTO.
/// </summary>
public sealed class ServiceExternalDtoInfo : ServiceMemberInfo
{
	/// <summary>
	/// Creates an external DTO.
	/// </summary>
	public ServiceExternalDtoInfo(string name, IEnumerable<ServiceAttributeInfo>? attributes = null, string? summary = null, IEnumerable<string>? remarks = null, params ServicePart[] parts)
		: base(name, attributes, summary, remarks, parts)
	{
	}

	private protected override IEnumerable<ServiceElementInfo> GetExtraChildrenCore() => [];
}
