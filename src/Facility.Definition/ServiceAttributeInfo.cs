using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// An attribute.
	/// </summary>
	public sealed class ServiceAttributeInfo : IServiceNamedInfo
	{
		/// <summary>
		/// Creates an attribute.
		/// </summary>
		public ServiceAttributeInfo(string name, IEnumerable<ServiceAttributeParameterInfo> parameters = null, NamedTextPosition position = null)
			: this(ValidationMode.Throw, name, parameters, position)
		{
		}

		internal ServiceAttributeInfo(ValidationMode validationMode, string name, IEnumerable<ServiceAttributeParameterInfo> parameters, NamedTextPosition position)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Parameters = parameters.ToReadOnlyList();
			Position = position;

			if (validationMode == ValidationMode.Throw)
				GetValidationErrors().ThrowIfAny();
		}

		/// <summary>
		/// The name of the attribute.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The attribute parameters.
		/// </summary>
		public IReadOnlyList<ServiceAttributeParameterInfo> Parameters { get; }

		/// <summary>
		/// The position of the attribute.
		/// </summary>
		public NamedTextPosition Position { get; }

		internal IEnumerable<ServiceDefinitionError> GetValidationErrors()
		{
			return ServiceDefinitionUtility.ValidateName(Name, Position)
				.Concat(ServiceDefinitionUtility.ValidateNoDuplicateNames(Parameters, "attribute parameter"));
		}
	}
}
