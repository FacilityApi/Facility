using System.Collections.Generic;

namespace Facility.Definition
{
	/// <summary>
	/// A service enumerated type.
	/// </summary>
	public sealed class ServiceEnumInfo : ServiceMemberInfo
	{
		/// <summary>
		/// Creates an enumerated type.
		/// </summary>
		public ServiceEnumInfo(string name, IEnumerable<ServiceEnumValueInfo>? values = null, IEnumerable<ServiceAttributeInfo>? attributes = null, string? summary = null, IEnumerable<string>? remarks = null, params ServicePart[] parts)
			: base(name, attributes, summary, remarks, parts)
		{
			Values = values.ToReadOnlyList();

			ValidateNoDuplicateNames(Values, "enumerated value");
		}

		/// <summary>
		/// The values of the enumerated type.
		/// </summary>
		public IReadOnlyList<ServiceEnumValueInfo> Values { get; }

		private protected override IEnumerable<ServiceElementInfo> GetExtraChildrenCore() => Values;
	}
}
