using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition.Http
{
	/// <summary>
	/// Information about a normal request or response field.
	/// </summary>
	public sealed class HttpNormalFieldInfo : HttpFieldInfo
	{
		/// <summary>
		/// The children of the element, if any.
		/// </summary>
		public override IEnumerable<HttpElementInfo> GetChildren() => Enumerable.Empty<HttpElementInfo>();

		internal HttpNormalFieldInfo(ServiceFieldInfo fieldInfo)
			: base(fieldInfo)
		{
			foreach (var parameter in GetHttpParameters(fieldInfo))
			{
				if (parameter.Name != "from")
					AddInvalidHttpParameterError(parameter);
			}
		}
	}
}
