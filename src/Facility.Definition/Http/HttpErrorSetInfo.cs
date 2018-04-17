using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition.Http
{
	/// <summary>
	/// The HTTP mapping of an error set.
	/// </summary>
	public sealed class HttpErrorSetInfo : HttpElementInfo
	{
		/// <summary>
		/// The error set.
		/// </summary>
		public ServiceErrorSetInfo ServiceErrorSet { get; }

		/// <summary>
		/// The HTTP mapping of the errors.
		/// </summary>
		public IReadOnlyList<HttpErrorInfo> Errors { get; }

		/// <summary>
		/// The children of the element, if any.
		/// </summary>
		public override IEnumerable<HttpElementInfo> GetChildren() => Errors;

		internal HttpErrorSetInfo(ServiceErrorSetInfo errorSetInfo)
		{
			ServiceErrorSet = errorSetInfo;

			var parameter = GetHttpParameters(errorSetInfo).FirstOrDefault();
			if (parameter != null)
				AddInvalidHttpParameterError(parameter);

			Errors = errorSetInfo.Errors.Select(x => new HttpErrorInfo(x)).ToList();
		}
	}
}
