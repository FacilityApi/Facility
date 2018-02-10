using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// An attribute parameter.
	/// </summary>
	public sealed class ServiceAttributeParameterInfo : IServiceNamedInfo, IValidatable
	{
		/// <summary>
		/// Creates an attribute parameter.
		/// </summary>
		public ServiceAttributeParameterInfo(string name, string value, NamedTextPosition position = null, bool validate = true)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			Name = name;
			Value = value;
			Position = position;

			if (validate)
			{
				var error = this.Validate().FirstOrDefault();
				if (error != null)
					throw error.CreateException();
			}
		}

		IEnumerable<ServiceDefinitionError> IValidatable.Validate()
		{
			return ServiceDefinitionUtility.ValidateName2(Name, Position);
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
	}
}
