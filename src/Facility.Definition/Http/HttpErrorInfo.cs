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
					StatusCode = HttpAttributeUtility.ParseStatusCodeInteger(parameter);
				else
					throw parameter.CreateInvalidHttpParameterException();
			}
		}
	}
}
