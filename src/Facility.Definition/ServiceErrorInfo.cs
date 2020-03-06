using System;
using System.Collections.Generic;

namespace Facility.Definition
{
	/// <summary>
	/// An error of an error set.
	/// </summary>
	public sealed class ServiceErrorInfo : ServiceElementWithAttributesInfo, IServiceHasName, IServiceHasSummary
	{
		/// <summary>
		/// Creates an error.
		/// </summary>
		public ServiceErrorInfo(string name, IEnumerable<ServiceAttributeInfo>? attributes, string? summary, params ServicePart[] parts)
			: base(attributes, parts)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Summary = summary ?? "";

			ValidateName();
		}

		/// <summary>
		/// The name of the error.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The summary of the error.
		/// </summary>
		public string Summary { get; }

		private protected override IEnumerable<ServiceElementInfo> GetExtraChildrenCore() => Attributes;
	}
}
