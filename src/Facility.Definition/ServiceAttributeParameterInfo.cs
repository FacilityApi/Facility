using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// An attribute parameter.
	/// </summary>
	public sealed class ServiceAttributeParameterInfo : ServiceElementInfo, IServiceHasName
	{
		/// <summary>
		/// Creates an attribute parameter.
		/// </summary>
		public ServiceAttributeParameterInfo(string name, string value, params ServicePart[] parts)
			: base(parts)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Value = value ?? throw new ArgumentNullException(nameof(value));

			ValidateName();
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
		/// The children of the service element, if any.
		/// </summary>
		public override IEnumerable<ServiceElementInfo> GetChildren() => Enumerable.Empty<ServiceElementInfo>();
	}
}
