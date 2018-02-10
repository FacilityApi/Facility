﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// An error of an error set.
	/// </summary>
	public sealed class ServiceErrorInfo : IServiceElementInfo, IValidatable
	{
		/// <summary>
		/// Creates an error.
		/// </summary>
		public ServiceErrorInfo(string name, IEnumerable<ServiceAttributeInfo> attributes = null, string summary = null, NamedTextPosition position = null, bool validate = true)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Name = name;
			Attributes = attributes.ToReadOnlyList();
			Summary = summary ?? "";
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
			return ServiceDefinitionUtility.ValidateName2(Name, Position);
		}

		/// <summary>
		/// The name of the error.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The attributes of the error.
		/// </summary>
		public IReadOnlyList<ServiceAttributeInfo> Attributes { get; }

		/// <summary>
		/// The summary of the error.
		/// </summary>
		public string Summary { get; }

		/// <summary>
		/// The position of the error in the definition.
		/// </summary>
		public NamedTextPosition Position { get; }
	}
}
