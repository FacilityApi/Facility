using System;
using System.Collections.Generic;

namespace Facility.Definition
{
	/// <summary>
	/// An attribute parameter.
	/// </summary>
	public sealed class ServiceAttributeParameterInfo : IServiceNamedInfo
	{
		/// <summary>
		/// Creates an attribute parameter.
		/// </summary>
		public ServiceAttributeParameterInfo(string name, string value, NamedTextPosition position)
			: this(ValidationMode.Throw, name, value, position, null)
		{
		}

		/// <summary>
		/// Creates an attribute parameter.
		/// </summary>
		public ServiceAttributeParameterInfo(string name, string value, NamedTextPosition position = null, NamedTextPosition valuePosition = null)
			: this(ValidationMode.Throw, name, value, position, valuePosition)
		{
		}

		internal ServiceAttributeParameterInfo(ValidationMode validationMode, string name, string value, NamedTextPosition position, NamedTextPosition valuePosition)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Value = value ?? throw new ArgumentNullException(nameof(value));
			Position = position;
			ValuePosition = valuePosition;

			if (validationMode == ValidationMode.Throw)
				GetValidationErrors().ThrowIfAny();
		}

		/// <summary>
		/// The name of the parameter.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The value of the parameter.
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// The position of the parameter.
		/// </summary>
		public NamedTextPosition Position { get; }

		/// <summary>
		/// The position of the parameter value.
		/// </summary>
		public NamedTextPosition ValuePosition { get; set; }

		internal IEnumerable<ServiceDefinitionError> GetValidationErrors()
		{
			return ServiceDefinitionUtility.ValidateName(Name, Position);
		}
	}
}
