using System;

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
		public ServiceAttributeParameterInfo(string name, string value, ServiceTextPosition position = null)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			Name = name;
			Value = value;
			Position = position;

			ServiceDefinitionUtility.ValidateName(Name, Position);
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
		public ServiceTextPosition Position { get; }
	}
}
