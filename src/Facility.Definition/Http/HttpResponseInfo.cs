using System.Net;

namespace Facility.Definition.Http
{
	/// <summary>
	/// Information about a valid method response.
	/// </summary>
	public sealed class HttpResponseInfo
	{
		/// <summary>
		/// The status code used by the response.
		/// </summary>
		public HttpStatusCode StatusCode { get; }

		/// <summary>
		/// True if the response has any fields.
		/// </summary>
		public bool HasResponseFields { get; }

		/// <summary>
		/// The field that corresponds to the entire response body; if null, the ResponseNormalFields are used, if any.
		/// </summary>
		public HttpBodyFieldInfo ResponseBodyField { get; }

		internal HttpResponseInfo(HttpStatusCode statusCode, bool hasResponseFields, HttpBodyFieldInfo responseBodyField)
		{
			StatusCode = statusCode;
			HasResponseFields = hasResponseFields;
			ResponseBodyField = responseBodyField;
		}
	}
}
