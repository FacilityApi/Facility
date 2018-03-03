using System;
using System.Collections.Generic;

namespace Facility.Definition
{
	/// <summary>
	/// A value of an enumerated type.
	/// </summary>
	public sealed class ServiceEnumValueInfo : IServiceElementInfo
	{
		/// <summary>
		/// Creates an enum value.
		/// </summary>
		public ServiceEnumValueInfo(string name, IEnumerable<ServiceAttributeInfo> attributes = null, string summary = null, NamedTextPosition position = null)
			: this(ValidationMode.Throw, name, attributes, summary, position)
		{
		}

		internal ServiceEnumValueInfo(ValidationMode validationMode, string name, IEnumerable<ServiceAttributeInfo> attributes = null, string summary = null, NamedTextPosition position = null)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Attributes = attributes.ToReadOnlyList();
			Summary = summary ?? "";
			Position = position;

			if (validationMode == ValidationMode.Throw)
				GetValidationErrors().ThrowIfAny();
		}

		/// <summary>
		/// The name of the value.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The attributes of the value.
		/// </summary>
		public IReadOnlyList<ServiceAttributeInfo> Attributes { get; }

		/// <summary>
		/// The summary of the value.
		/// </summary>
		public string Summary { get; }

		/// <summary>
		/// The position of the value in the definition.
		/// </summary>
		public NamedTextPosition Position { get; }

		internal IEnumerable<ServiceDefinitionError> GetValidationErrors()
		{
			return ServiceDefinitionUtility.ValidateName(Name, Position);
		}
	}
}
