using System;
using System.Collections.Generic;

namespace Facility.Definition
{
	/// <summary>
	/// A service enumerated type.
	/// </summary>
	public sealed class ServiceEnumInfo : IServiceMemberInfo
	{
		/// <summary>
		/// Creates an enum.
		/// </summary>
		public ServiceEnumInfo(string name, IEnumerable<ServiceEnumValueInfo> values = null, IEnumerable<ServiceAttributeInfo> attributes = null, string summary = null, IEnumerable<string> remarks = null, NamedTextPosition position = null)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Name = name;
			Values = values.ToReadOnlyList();
			Attributes = attributes.ToReadOnlyList();
			Summary = summary ?? "";
			Remarks = remarks.ToReadOnlyList();
			Position = position;

			ServiceDefinitionUtility.ValidateName(Name, Position);
			ServiceDefinitionUtility.ValidateNoDuplicateNames(Values, "enumerated value");
		}

		/// <summary>
		/// The name of the enumerated type.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The values of the enumerated type.
		/// </summary>
		public IReadOnlyList<ServiceEnumValueInfo> Values { get; }

		/// <summary>
		/// The attributes of the enumerated type.
		/// </summary>
		public IReadOnlyList<ServiceAttributeInfo> Attributes { get; }

		/// <summary>
		/// The summary of the enumerated type.
		/// </summary>
		public string Summary { get; }

		/// <summary>
		/// The remarks of the enumerated type.
		/// </summary>
		public IReadOnlyList<string> Remarks { get; }

		/// <summary>
		/// The position of the enumerated type in the definition.
		/// </summary>
		public NamedTextPosition Position { get; }
	}
}
