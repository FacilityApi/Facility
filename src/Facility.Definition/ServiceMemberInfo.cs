using System;
using System.Collections.Generic;

namespace Facility.Definition
{
	/// <summary>
	/// Properties common to service members.
	/// </summary>
	public abstract class ServiceMemberInfo : ServiceElementWithAttributesInfo, IServiceHasName, IServiceHasSummary
	{
		/// <summary>
		/// The name of the service member.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The summary of the service member.
		/// </summary>
		public string Summary { get; }

		/// <summary>
		/// The remarks of the service member.
		/// </summary>
		public IReadOnlyList<string> Remarks { get; internal set; }

		private protected ServiceMemberInfo(string name, IEnumerable<ServiceAttributeInfo>? attributes, string? summary, IEnumerable<string>? remarks, IReadOnlyList<ServicePart> parts)
			: base(attributes, parts)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Summary = summary ?? "";
			Remarks = remarks.ToReadOnlyList();

			ValidateName();
		}
	}
}
