using System.Collections.Generic;

namespace Facility.Definition
{
	/// <summary>
	/// Properties common to service elements.
	/// </summary>
	public interface IServiceElementInfo : IServiceNamedInfo
	{
		/// <summary>
		/// The attributes.
		/// </summary>
		IReadOnlyList<ServiceAttributeInfo> Attributes { get; }

		/// <summary>
		/// The summary.
		/// </summary>
		string Summary { get; }
	}
}
