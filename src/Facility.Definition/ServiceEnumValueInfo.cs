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
		public ServiceEnumValueInfo(string name, IEnumerable<ServiceAttributeInfo> attributes = null, string summary = null, ServiceTextPosition position = null)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Name = name;
			Attributes = attributes.ToReadOnlyList();
			Summary = summary ?? "";
			Position = position;

			ServiceDefinitionUtility.ValidateName(Name, Position);
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
		public ServiceTextPosition Position { get; }
	}
}
