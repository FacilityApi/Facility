using System.Net;

namespace Facility.Definition.Http;

/// <summary>
/// Information about a DTO field used as a request or response body.
/// </summary>
public sealed class HttpBodyFieldInfo : HttpFieldInfo
{
	/// <summary>
	/// The specified status code, if any.
	/// </summary>
	public HttpStatusCode? StatusCode { get; }

	/// <summary>
	/// The specified content type, if any.
	/// </summary>
	public string? ContentType { get; }

	/// <summary>
	/// The children of the element, if any.
	/// </summary>
	public override IEnumerable<HttpElementInfo> GetChildren() => [];

	internal HttpBodyFieldInfo(ServiceFieldInfo fieldInfo)
		: base(fieldInfo)
	{
		foreach (var parameter in GetHttpParameters(fieldInfo))
		{
			switch (parameter.Name)
			{
				case "code":
					StatusCode = TryParseStatusCodeInteger(parameter);
					break;

				case "type":
					ContentType = parameter.Value;
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
