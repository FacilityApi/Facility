using System;
using System.Collections.Generic;

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
		public ServiceAttributeInfo(string name, IEnumerable<ServiceAttributeParameterInfo> parameters = null, ServiceTextPosition position = null)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Name = name;
			Parameters = parameters.ToReadOnlyList();
			Position = position;

			ServiceDefinitionUtility.ValidateName(Name, Position);
			ServiceDefinitionUtility.ValidateNoDuplicateNames(Parameters, "attribute parameter");
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
		public ServiceTextPosition Position { get; }
	}
}
