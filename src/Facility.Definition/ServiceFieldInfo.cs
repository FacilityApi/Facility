using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// A field of a DTO.
	/// </summary>
	public sealed class ServiceFieldInfo : IServiceElementInfo
	{
		/// <summary>
		/// Creates a field.
		/// </summary>
		public ServiceFieldInfo(string name, string typeName, IEnumerable<ServiceAttributeInfo> attributes, string summary, NamedTextPosition position)
			: this(name, typeName, attributes, summary, position, null)
		{
		}

		/// <summary>
		/// Creates a field.
		/// </summary>
		public ServiceFieldInfo(string name, string typeName, IEnumerable<ServiceAttributeInfo> attributes = null, string summary = null, NamedTextPosition position = null, NamedTextPosition typeNamePosition = null)
			: this(ValidationMode.Throw, name, typeName, attributes, summary, position, typeNamePosition)
		{
		}

		internal ServiceFieldInfo(ValidationMode validationMode, string name, string typeName, IEnumerable<ServiceAttributeInfo> attributes, string summary, NamedTextPosition position, NamedTextPosition typeNamePosition)
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

			if (validationMode == ValidationMode.Throw)
				GetValidationErrors().ThrowIfAny();
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

		internal IEnumerable<ServiceDefinitionError> GetValidationErrors()
		{
			return ServiceDefinitionUtility.ValidateName(Name, Position)
				.Concat(ServiceDefinitionUtility.ValidateTypeName(TypeName, TypeNamePosition));
		}
	}
}
