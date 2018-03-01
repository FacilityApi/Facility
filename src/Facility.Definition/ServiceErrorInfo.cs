using System;
using System.Collections.Generic;

namespace Facility.Definition
{
	/// <summary>
	/// An error of an error set.
	/// </summary>
	public sealed class ServiceErrorInfo : IServiceElementInfo
	{
		/// <summary>
		/// Creates an error.
		/// </summary>
		public ServiceErrorInfo(string name, IEnumerable<ServiceAttributeInfo> attributes = null, string summary = null, NamedTextPosition position = null)
			: this(ValidationMode.Throw, name, attributes, summary, position)
		{
		}

		internal ServiceErrorInfo(ValidationMode validationMode, string name, IEnumerable<ServiceAttributeInfo> attributes, string summary, NamedTextPosition position)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Name = name;
			Attributes = attributes.ToReadOnlyList();
			Summary = summary ?? "";
			Position = position;

			if (validationMode == ValidationMode.Throw)
				GetValidationErrors().ThrowIfAny();
		}

		/// <summary>
		/// The name of the error.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The attributes of the error.
		/// </summary>
		public IReadOnlyList<ServiceAttributeInfo> Attributes { get; }

		/// <summary>
		/// The summary of the error.
		/// </summary>
		public string Summary { get; }

		/// <summary>
		/// The position of the error in the definition.
		/// </summary>
		public NamedTextPosition Position { get; }

		internal IEnumerable<ServiceDefinitionError> GetValidationErrors()
		{
			return ServiceDefinitionUtility.ValidateName(Name, Position);
		}
	}
}
