using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Facility.Definition.Http
{
	/// <summary>
	/// Information about a DTO field used as a request or response body.
	/// </summary>
	public sealed class HttpBodyFieldInfo : HttpElementInfo
	{
		/// <summary>
		/// The service field.
		/// </summary>
		public ServiceFieldInfo ServiceField { get; }

		/// <summary>
		/// The specified status code, if any.
		/// </summary>
		public HttpStatusCode? StatusCode { get; }

		/// <summary>
		/// The children of the element, if any.
		/// </summary>
		public override IEnumerable<HttpElementInfo> GetChildren() => Enumerable.Empty<HttpElementInfo>();

		internal HttpBodyFieldInfo(ServiceFieldInfo fieldInfo)
		{
			ServiceField = fieldInfo;

			foreach (var parameter in GetHttpParameters(fieldInfo))
			{
				if (parameter.Name == "code")
					StatusCode = TryParseStatusCodeInteger(parameter);
				else if (parameter.Name != "from")
					AddInvalidHttpParameterError(parameter);
			}
		}
	}
}
