using System.Collections.Generic;
using System.Linq;
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
		/// The fields from the response DTO that correspond to the response body.
		/// </summary>
		public IReadOnlyList<HttpNormalFieldInfo> NormalFields { get; }

		/// <summary>
		/// The field that corresponds to the entire response body.
		/// </summary>
		public HttpBodyFieldInfo BodyField { get; }

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

		internal IEnumerable<ServiceDefinitionError> GetValidationErrors()
		{
			return NormalFields != null ? NormalFields.SelectMany(x => x.GetValidationErrors()) : BodyField.GetValidationErrors();
		}
	}
}
