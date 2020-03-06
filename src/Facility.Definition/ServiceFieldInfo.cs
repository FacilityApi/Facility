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
		public ServiceFieldInfo(string name, string typeName, IEnumerable<ServiceAttributeInfo>? attributes = null, string? summary = null, params ServicePart[] parts)
			: base(attributes, parts)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
			Summary = summary ?? "";

			ValidateName();

			var requiredAttributes = GetAttributes("required");
			if (requiredAttributes.Count > 1)
				AddValidationError(ServiceDefinitionUtility.CreateDuplicateAttributeError(requiredAttributes[1]));
			var requiredAttribute = requiredAttributes.Count == 0 ? null : requiredAttributes[0];
			if (requiredAttribute != null)
			{
				IsRequired = true;

				foreach (var requiredParameter in requiredAttribute.Parameters)
					AddValidationError(ServiceDefinitionUtility.CreateUnexpectedAttributeParameterError(requiredAttribute.Name, requiredParameter));
			}
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

		/// <summary>
		/// True if the field is required.
		/// </summary>
		public bool IsRequired { get; }

		private protected override IEnumerable<ServiceElementInfo> GetExtraChildrenCore() => Enumerable.Empty<ServiceElementInfo>();
	}
}
