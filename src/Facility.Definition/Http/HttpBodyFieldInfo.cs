using System.Collections.Generic;
using System.Net;

namespace Facility.Definition.Http
{
	/// <summary>
	/// Information about a DTO field used as a request or response body.
	/// </summary>
	public sealed class HttpBodyFieldInfo
	{
		/// <summary>
		/// The service field.
		/// </summary>
		public ServiceFieldInfo ServiceField { get; }

		/// <summary>
		/// The specified status code, if any.
		/// </summary>
		public HttpStatusCode? StatusCode { get; }

		internal HttpBodyFieldInfo(ServiceFieldInfo fieldInfo)
		{
			ServiceField = fieldInfo;

			foreach (var parameter in fieldInfo.GetHttpParameters())
			{
				if (parameter.Name == "code")
					StatusCode = HttpAttributeUtility.ParseStatusCodeInteger(parameter);
				else if (parameter.Name != "from")
					m_errors.Add(parameter.CreateInvalidHttpParameterError());
			}
		}

		internal IEnumerable<ServiceDefinitionError> GetValidationErrors()
		{
			return m_errors;
		}

		private readonly List<ServiceDefinitionError> m_errors = new List<ServiceDefinitionError>();
	}
}
