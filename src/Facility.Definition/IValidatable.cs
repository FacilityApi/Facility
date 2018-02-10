using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	internal interface IValidatable
	{
		IEnumerable<ServiceDefinitionError> Validate();
	}

	internal static class ValidationUtility
	{
		public static IEnumerable<ServiceDefinitionError> Validate(this IValidatable validatable)
		{
			return validatable.Validate();
		}
	}
}
