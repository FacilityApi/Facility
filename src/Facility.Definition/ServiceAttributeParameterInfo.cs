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
		public ServiceAttributeParameterInfo(string name, string value, NamedTextPosition position = null)
			: this(ValidationMode.Throw, name, value, position)
		{
		}

		internal ServiceAttributeParameterInfo(ValidationMode validationMode, string name, string value, NamedTextPosition position)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			Name = name;
			Value = value;
			Position = position;

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

		internal IEnumerable<ServiceDefinitionError> GetValidationErrors()
		{
			return ServiceDefinitionUtility.ValidateName(Name, Position);
		}
	}
}
