using System.Net;

namespace Facility.Definition.Http;

/// <summary>
/// The HTTP mapping of an error.
/// </summary>
public sealed class HttpErrorInfo : HttpElementInfo
{
	/// <summary>
	/// The error.
	/// </summary>
	public ServiceErrorInfo ServiceError { get; }

	/// <summary>
	/// The HTTP status code used by the error.
	/// </summary>
	public HttpStatusCode StatusCode { get; }

	internal HttpErrorInfo(ServiceErrorInfo errorInfo)
	{
		ServiceError = errorInfo;
		StatusCode = HttpStatusCode.InternalServerError;

		foreach (var parameter in GetHttpParameters(errorInfo))
		{
			switch (parameter.Name)
			{
				case "code":
					StatusCode = TryParseStatusCodeInteger(parameter) ?? HttpStatusCode.InternalServerError;
					break;

				case "from":
					break;

				default:
					AddInvalidHttpParameterError(parameter);
					break;
			}
		}
	}

	/// <summary>
	/// The children of the element, if any.
	/// </summary>
	public override IEnumerable<HttpElementInfo> GetChildren() => [];
}
