using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Facility.Definition.Http
{
	/// <summary>
	/// Information about a valid method response.
	/// </summary>
	public sealed class HttpResponseInfo : HttpElementInfo
	{
		/// <summary>
		/// The status code used by the response.
		/// </summary>
		public HttpStatusCode StatusCode { get; }

		/// <summary>
		/// The fields from the response DTO that correspond to the response body.
		/// </summary>
		public IReadOnlyList<HttpNormalFieldInfo>? NormalFields { get; }

		/// <summary>
		/// The field that corresponds to the entire response body.
		/// </summary>
		public HttpBodyFieldInfo? BodyField { get; }

		/// <summary>
		/// The children of the element, if any.
		/// </summary>
		public override IEnumerable<HttpElementInfo> GetChildren() => NormalFields?.AsEnumerable<HttpElementInfo>() ?? new[] { BodyField! };

		internal HttpResponseInfo(HttpStatusCode statusCode, IReadOnlyList<HttpNormalFieldInfo> normalFields)
		{
			StatusCode = statusCode;
			NormalFields = normalFields;
		}

		internal HttpResponseInfo(HttpStatusCode statusCode, HttpBodyFieldInfo bodyField)
		{
			StatusCode = statusCode;
			BodyField = bodyField;
		}
	}
}
