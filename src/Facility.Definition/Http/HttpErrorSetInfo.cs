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

			var httpParameter = errorSetInfo.GetHttpParameters().FirstOrDefault();
			if (httpParameter != null)
				throw httpParameter.CreateInvalidHttpParameterException();

			Errors = errorSetInfo.Errors.Select(x => new HttpErrorInfo(x)).ToList();
		}
	}
}
