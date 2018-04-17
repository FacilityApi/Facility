using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Facility.Definition.Http
{
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
				if (parameter.Name == "code")
					StatusCode = TryParseStatusCodeInteger(parameter) ?? HttpStatusCode.InternalServerError;
				else if (parameter.Name != "from")
					AddInvalidHttpParameterError(parameter);
			}
		}

		/// <summary>
		/// The children of the element, if any.
		/// </summary>
		public override IEnumerable<HttpElementInfo> GetChildren() => Enumerable.Empty<HttpElementInfo>();
	}
}
