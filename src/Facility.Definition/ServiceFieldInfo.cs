using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// A field of a DTO.
	/// </summary>
	public sealed class ServiceFieldInfo : ServiceElementWithAttributesInfo, IServiceHasName, IServiceHasSummary
	{
		/// <summary>
		/// Creates a field.
		/// </summary>
		public ServiceFieldInfo(string name, string typeName, IEnumerable<ServiceAttributeInfo> attributes = null, string summary = null, params ServicePart[] parts)
			: base(attributes, parts)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
			Summary = summary ?? "";

			ValidateName();
		}

		/// <summary>
		/// The name of the field.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The name of the type of the field.
		/// </summary>
		public string TypeName { get; }

		/// <summary>
		/// The summary of the field.
		/// </summary>
		public string Summary { get; }

		private protected override IEnumerable<ServiceElementInfo> GetExtraChildrenCore() => Enumerable.Empty<ServiceElementInfo>();
	}
}
