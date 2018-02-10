using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// An attribute.
	/// </summary>
	public sealed class ServiceAttributeInfo : IServiceNamedInfo, IValidatable
	{
		/// <summary>
		/// Creates an attribute.
		/// </summary>
		public ServiceAttributeInfo(string name, IEnumerable<ServiceAttributeParameterInfo> parameters = null, NamedTextPosition position = null, bool validate = true)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Name = name;
			Parameters = parameters.ToReadOnlyList();
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
			return ServiceDefinitionUtility.ValidateName2(Name, Position)
				.Concat(ServiceDefinitionUtility.ValidateNoDuplicateNames2(Parameters, "attribute parameter"));
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
	}
}
