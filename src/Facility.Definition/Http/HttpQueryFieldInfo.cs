namespace Facility.Definition.Http;

/// <summary>
/// Information about a field that corresponds to a request query parameter.
/// </summary>
public sealed class HttpQueryFieldInfo : HttpFieldInfo
{
	/// <summary>
	/// The name of the query parameter.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// The children of the element, if any.
	/// </summary>
	public override IEnumerable<HttpElementInfo> GetChildren() => [];

	internal HttpQueryFieldInfo(ServiceFieldInfo fieldInfo)
		: base(fieldInfo)
	{
		Name = fieldInfo.Name;

		foreach (var parameter in GetHttpParameters(fieldInfo))
		{
			switch (parameter.Name)
			{
				case "name":
					Name = parameter.Value;
					break;

				case "from":
					break;

				default:
					AddInvalidHttpParameterError(parameter);
					break;
			}
		}
	}
}
