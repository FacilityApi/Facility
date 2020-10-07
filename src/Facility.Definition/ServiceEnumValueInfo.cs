using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// A value of an enumerated type.
	/// </summary>
	public sealed class ServiceEnumValueInfo : ServiceElementWithAttributesInfo, IServiceHasName, IServiceHasSummary
	{
		/// <summary>
		/// Creates an enumerated type value.
		/// </summary>
		public ServiceEnumValueInfo(string name, IEnumerable<ServiceAttributeInfo> attributes, string? summary, params ServicePart[] parts)
			: base(attributes, parts)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Summary = summary ?? "";

			ValidateName();
		}

		/// <summary>
		/// The name of the value.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The summary of the value.
		/// </summary>
		public string Summary { get; }

		private protected override IEnumerable<ServiceElementInfo> GetExtraChildrenCore() => Enumerable.Empty<ServiceElementInfo>();
	}
}
