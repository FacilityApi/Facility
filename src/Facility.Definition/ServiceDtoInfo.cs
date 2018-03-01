using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// A service DTO.
	/// </summary>
	public sealed class ServiceDtoInfo : IServiceMemberInfo
	{
		/// <summary>
		/// Creates a DTO.
		/// </summary>
		public ServiceDtoInfo(string name, IEnumerable<ServiceFieldInfo> fields = null, IEnumerable<ServiceAttributeInfo> attributes = null, string summary = null, IEnumerable<string> remarks = null, NamedTextPosition position = null)
			: this(ValidationMode.Throw, name, fields, attributes, summary, remarks, position)
		{
		}

		internal ServiceDtoInfo(ValidationMode validationMode, string name, IEnumerable<ServiceFieldInfo> fields, IEnumerable<ServiceAttributeInfo> attributes, string summary, IEnumerable<string> remarks, NamedTextPosition position)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Name = name;
			Fields = fields.ToReadOnlyList();
			Attributes = attributes.ToReadOnlyList();
			Summary = summary ?? "";
			Remarks = remarks.ToReadOnlyList();
			Position = position;

			if (validationMode == ValidationMode.Throw)
				GetValidationErrors().ThrowIfAny();
		}

		/// <summary>
		/// The name of the DTO.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The fields of the DTO.
		/// </summary>
		public IReadOnlyList<ServiceFieldInfo> Fields { get; }

		/// <summary>
		/// The attributes of the DTO.
		/// </summary>
		public IReadOnlyList<ServiceAttributeInfo> Attributes { get; }

		/// <summary>
		/// The summary for the DTO.
		/// </summary>
		public string Summary { get; }

		/// <summary>
		/// The remarks for the DTO.
		/// </summary>
		public IReadOnlyList<string> Remarks { get; }

		/// <summary>
		/// The position of the DTO in the definition.
		/// </summary>
		public NamedTextPosition Position { get; }

		internal IEnumerable<ServiceDefinitionError> GetValidationErrors()
		{
			return ServiceDefinitionUtility.ValidateName(Name, Position)
				.Concat(ServiceDefinitionUtility.ValidateNoDuplicateNames(Fields, "field"));
		}
	}
}
