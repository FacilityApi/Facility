using System.Collections.Generic;

namespace Facility.Definition
{
	/// <summary>
	/// Properties common to service members.
	/// </summary>
	public interface IServiceMemberInfo : IServiceElementInfo
	{
		/// <summary>
		/// The remarks.
		/// </summary>
		IReadOnlyList<string> Remarks { get; }
	}
}
