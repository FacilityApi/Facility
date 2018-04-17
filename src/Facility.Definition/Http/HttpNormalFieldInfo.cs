using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition.Http
{
	/// <summary>
	/// Information about a normal request or response field.
	/// </summary>
	public sealed class HttpNormalFieldInfo : HttpElementInfo
	{
		/// <summary>
		/// The service field.
		/// </summary>
		public ServiceFieldInfo ServiceField { get; }

		/// <summary>
		/// The children of the element, if any.
		/// </summary>
		public override IEnumerable<HttpElementInfo> GetChildren() => Enumerable.Empty<HttpElementInfo>();

		internal HttpNormalFieldInfo(ServiceFieldInfo fieldInfo)
		{
			ServiceField = fieldInfo;

			foreach (var parameter in GetHttpParameters(fieldInfo))
			{
				if (parameter.Name != "from")
					AddInvalidHttpParameterError(parameter);
			}
		}
	}
}
