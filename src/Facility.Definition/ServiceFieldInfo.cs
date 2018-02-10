﻿using System;
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
		public ServiceFieldInfo(string name, string typeName, IEnumerable<ServiceAttributeInfo> attributes = null, string summary = null, NamedTextPosition position = null, ValidationMode validationMode = ValidationMode.Throw)
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

			this.Validate(validationMode);
		}

		IEnumerable<ServiceDefinitionError> IValidatable.Validate()
		{
			return ServiceDefinitionUtility.ValidateName(Name, Position)
				.Concat(ServiceDefinitionUtility.ValidateTypeName(TypeName, Position));
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
	}
}
