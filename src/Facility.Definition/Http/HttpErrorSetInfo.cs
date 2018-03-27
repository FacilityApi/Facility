using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition.Http
{
	/// <summary>
	/// The HTTP mapping of an error set.
	/// </summary>
	public sealed class HttpErrorSetInfo
	{
		/// <summary>
		/// The error set.
		/// </summary>
		public ServiceErrorSetInfo ServiceErrorSet { get; }

		/// <summary>
		/// The HTTP mapping of the errors.
		/// </summary>
		public IReadOnlyList<HttpErrorInfo> Errors { get; }

		internal HttpErrorSetInfo(ServiceErrorSetInfo errorSetInfo)
		{
			ServiceErrorSet = errorSetInfo;

			var parameter = errorSetInfo.GetHttpParameters().FirstOrDefault();
			if (parameter != null)
				m_errors.Add(parameter.CreateInvalidHttpParameterError());

			Errors = errorSetInfo.Errors.Select(x => new HttpErrorInfo(x)).ToList();
		}

		internal IEnumerable<ServiceDefinitionError> GetValidationErrors() => m_errors.Concat(Errors.SelectMany(x => x.GetValidationErrors()));

		private readonly List<ServiceDefinitionError> m_errors = new List<ServiceDefinitionError>();
	}
}
