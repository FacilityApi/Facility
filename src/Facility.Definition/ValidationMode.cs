using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// Validation mode.
	/// </summary>
	public enum ValidationMode
	{
		/// <summary>
		/// Throw ServiceDefinitionException on the first validation error.
		/// </summary>
		Throw,

		/// <summary>
		/// Return ServiceDefinitionErrors on validation errors.
		/// </summary>
		Return
	}

	internal interface IValidatable
	{
		IEnumerable<ServiceDefinitionError> Validate();
	}
}
