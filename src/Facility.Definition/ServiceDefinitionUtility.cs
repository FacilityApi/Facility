using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Facility.Definition
{
	/// <summary>
	/// Helper methods for working with service definitions.
	/// </summary>
	public static class ServiceDefinitionUtility
	{
		/// <summary>
		/// Creates an error for a duplicate attribute.
		/// </summary>
		public static ServiceDefinitionError CreateDuplicateAttributeError(ServiceAttributeInfo attribute) =>
			new ServiceDefinitionError($"'{attribute.Name}' attribute is duplicated.", attribute.Position);

		/// <summary>
		/// Creates an error for an unexpected attribute parameter.
		/// </summary>
		public static ServiceDefinitionError CreateUnexpectedAttributeError(ServiceAttributeInfo attribute) =>
			new ServiceDefinitionError($"Unexpected '{attribute.Name}' attribute.", attribute.Position);

		/// <summary>
		/// Creates an error for an unexpected attribute parameter.
		/// </summary>
		public static ServiceDefinitionError CreateUnexpectedAttributeParameterError(string attributeName, ServiceAttributeParameterInfo parameter) =>
			new ServiceDefinitionError($"Unexpected '{attributeName}' parameter '{parameter.Name}'.", parameter.Position);

		/// <summary>
		/// Returns true if the name is a valid service element name.
		/// </summary>
		public static bool IsValidName(string? name) => name != null && s_validNameRegex.IsMatch(name);

		internal static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T>? items) => new ReadOnlyCollection<T>((items ?? Enumerable.Empty<T>()).ToList());

		private static readonly Regex s_validNameRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");
	}
}
