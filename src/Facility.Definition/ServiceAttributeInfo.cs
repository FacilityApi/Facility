using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// A service attribute.
	/// </summary>
	public sealed class ServiceAttributeInfo : ServiceElementInfo, IServiceHasName
	{
		/// <summary>
		/// Creates a service attribute.
		/// </summary>
		public ServiceAttributeInfo(string name, IEnumerable<ServiceAttributeParameterInfo>? parameters = null, params ServicePart[] parts)
			: base(parts)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Parameters = parameters.ToReadOnlyList();

			ValidateName();
			ValidateNoDuplicateNames(Parameters, "attribute parameter");
		}

		/// <summary>
		/// The name of the attribute.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The attribute parameters.
		/// </summary>
		public IReadOnlyList<ServiceAttributeParameterInfo> Parameters { get; }

		/// <summary>
		/// Returns the attribute parameter with the specified name.
		/// </summary>
		public ServiceAttributeParameterInfo? TryGetParameter(string name) => Parameters.FirstOrDefault(x => x.Name == name);

		/// <summary>
		/// Returns the value of the attribute parameter with the specified name.
		/// </summary>
		public string? TryGetParameterValue(string name) => TryGetParameter(name)?.Value;

		/// <summary>
		/// The children of the service element, if any.
		/// </summary>
		public override IEnumerable<ServiceElementInfo> GetChildren() => Parameters;
	}
}
