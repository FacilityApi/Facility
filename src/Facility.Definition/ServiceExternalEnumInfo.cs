namespace Facility.Definition;

/// <summary>
/// An external service enumerated type.
/// </summary>
public sealed class ServiceExternalEnumInfo : ServiceMemberInfo
{
	/// <summary>
	/// Creates an external enumerated type.
	/// </summary>
	public ServiceExternalEnumInfo(string name, IEnumerable<ServiceAttributeInfo>? attributes = null, string? summary = null, IEnumerable<string>? remarks = null, params ServicePart[] parts)
		: base(name, attributes, summary, remarks, parts)
	{
	}

	private protected override IEnumerable<ServiceElementInfo> GetExtraChildrenCore() => Array.Empty<ServiceElementInfo>();
}
