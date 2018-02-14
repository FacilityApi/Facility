using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// A field of a DTO.
	/// </summary>
	public sealed class ServiceFieldInfo : IServiceElementInfo, IValidatable
	{
		/// <summary>
		/// Creates a field.
		/// </summary>
		public ServiceFieldInfo(string name, string typeName, IEnumerable<ServiceAttributeInfo> attributes, string summary, NamedTextPosition position)
			: this(name, typeName, attributes, summary, position, null, ValidationMode.Throw)
		{
		}

		/// <summary>
		/// Creates a field.
		/// </summary>
		public ServiceFieldInfo(string name, string typeName, IEnumerable<ServiceAttributeInfo> attributes, string summary, NamedTextPosition position, NamedTextPosition typeNamePosition)
			: this(name, typeName, attributes, summary, position, typeNamePosition, ValidationMode.Throw)
		{
		}

		/// <summary>
		/// Creates a field.
		/// </summary>
		public ServiceFieldInfo(string name, string typeName, IEnumerable<ServiceAttributeInfo> attributes = null, string summary = null, NamedTextPosition position = null, NamedTextPosition typeNamePosition = null, ValidationMode validationMode = ValidationMode.Throw)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (typeName == null)
				throw new ArgumentNullException(nameof(typeName));

			Name = name;
			TypeName = typeName;
			Attributes = attributes.ToReadOnlyList();
			Summary = summary ?? "";
			Position = position;
			TypeNamePosition = typeNamePosition ?? position;

			this.Validate(validationMode);
		}

		IEnumerable<ServiceDefinitionError> IValidatable.Validate()
		{
			return ServiceDefinitionUtility.ValidateName(Name, Position)
				.Concat(ServiceDefinitionUtility.ValidateTypeName(TypeName, TypeNamePosition));
		}

		/// <summary>
		/// The name of the field.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The name of the type of the field.
		/// </summary>
		public string TypeName { get; }

		/// <summary>
		/// The attributes of the field.
		/// </summary>
		public IReadOnlyList<ServiceAttributeInfo> Attributes { get; }

		/// <summary>
		/// The summary of the field.
		/// </summary>
		public string Summary { get; }

		/// <summary>
		/// The position of the field in the definition.
		/// </summary>
		public NamedTextPosition Position { get; }

		/// <summary>
		/// The position of the field type name in the definition.
		/// </summary>
		public NamedTextPosition TypeNamePosition { get; set; }
	}
}
