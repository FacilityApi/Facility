using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// An error set.
	/// </summary>
	public sealed class ServiceErrorSetInfo : IServiceMemberInfo
	{
		/// <summary>
		/// Creates an error set.
		/// </summary>
		public ServiceErrorSetInfo(string name, IEnumerable<ServiceErrorInfo> errors = null, IEnumerable<ServiceAttributeInfo> attributes = null, string summary = null, IEnumerable<string> remarks = null, NamedTextPosition position = null)
			: this(ValidationMode.Throw, name, errors, attributes, summary, remarks, position)
		{
		}

		internal ServiceErrorSetInfo(ValidationMode validationMode, string name, IEnumerable<ServiceErrorInfo> errors, IEnumerable<ServiceAttributeInfo> attributes, string summary, IEnumerable<string> remarks, NamedTextPosition position)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Name = name;
			Errors = errors.ToReadOnlyList();
			Attributes = attributes.ToReadOnlyList();
			Summary = summary ?? "";
			Remarks = remarks.ToReadOnlyList();
			Position = position;

			if (validationMode == ValidationMode.Throw)
				GetValidationErrors().ThrowIfAny();
		}

		/// <summary>
		/// The name of the error set.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The errors of the error set.
		/// </summary>
		public IReadOnlyList<ServiceErrorInfo> Errors { get; }

		/// <summary>
		/// The attributes of the error set.
		/// </summary>
		public IReadOnlyList<ServiceAttributeInfo> Attributes { get; }

		/// <summary>
		/// The summary of the error set.
		/// </summary>
		public string Summary { get; }

		/// <summary>
		/// The remarks of the error set.
		/// </summary>
		public IReadOnlyList<string> Remarks { get; }

		/// <summary>
		/// The position of the error set in the definition.
		/// </summary>
		public NamedTextPosition Position { get; }

		internal IEnumerable<ServiceDefinitionError> GetValidationErrors()
		{
			return ServiceDefinitionUtility.ValidateName(Name, Position)
				.Concat(ServiceDefinitionUtility.ValidateNoDuplicateNames(Errors, "error"));
		}
	}
}
