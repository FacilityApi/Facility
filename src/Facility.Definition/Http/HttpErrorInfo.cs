using System.Collections.Generic;
using System.Net;

namespace Facility.Definition.Http
{
	/// <summary>
	/// The HTTP mapping of an error.
	/// </summary>
	public sealed class HttpErrorInfo
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

			foreach (var parameter in errorInfo.GetHttpParameters())
			{
				if (parameter.Name == "code")
				{
					StatusCode = HttpAttributeUtility.TryParseStatusCodeInteger(parameter, out var error) ?? HttpStatusCode.InternalServerError;
					if (error != null)
						m_errors.Add(error);
				}
				else if (parameter.Name != "from")
				{
					m_errors.Add(parameter.CreateInvalidHttpParameterError());
				}
			}
		}

		internal IEnumerable<ServiceDefinitionError> GetValidationErrors()
		{
			return m_errors;
		}

		private readonly List<ServiceDefinitionError> m_errors = new List<ServiceDefinitionError>();
	}
}
