using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Facility.Definition;

/// <summary>
/// Helper methods for working with service definitions.
/// </summary>
public static class ServiceDefinitionUtility
{
	/// <summary>
	/// Creates an error for a duplicate attribute.
	/// </summary>
	public static ServiceDefinitionError CreateDuplicateAttributeError(ServiceAttributeInfo attribute) =>
		new($"'{attribute.Name}' attribute is duplicated.", attribute.Position);

	/// <summary>
	/// Creates an error for an unexpected attribute parameter.
	/// </summary>
	public static ServiceDefinitionError CreateUnexpectedAttributeError(ServiceAttributeInfo attribute) =>
		new($"Unexpected '{attribute.Name}' attribute.", attribute.Position);

	/// <summary>
	/// Creates an error for an unexpected attribute parameter.
	/// </summary>
	public static ServiceDefinitionError CreateUnexpectedAttributeParameterError(string attributeName, ServiceAttributeParameterInfo parameter) =>
		new($"Unexpected '{attributeName}' parameter '{parameter.Name}'.", parameter.Position);

	internal static ServiceDefinitionError CreateInvalidAttributeParameterForTypeError(ServiceAttributeInfo attribute, ServiceTypeInfo type, string parameter) =>
		new($"'{attribute.Name}' parameter '{parameter}' is invalid for {type.Kind}.", attribute.Position);

	internal static ServiceDefinitionError CreateMissingAttributeParametersError(ServiceAttributeInfo attribute, params string[] missingParameterName) =>
		new($"Missing '{attribute.Name}' parameters: [{string.Join(", ", missingParameterName)}].", attribute.Position);

	internal static ServiceDefinitionError CreateInvalidAttributeValueError(string attributeName, ServiceAttributeParameterInfo parameter) =>
		new($"'{parameter.Name}' value '{parameter.Value}' for '{attributeName}' attribute is invalid.", parameter.Position);

	/// <summary>
	/// Returns true if the name is a valid service element name.
	/// </summary>
	public static bool IsValidName(string? name) => name is not null && s_validNameRegex.IsMatch(name);

	internal static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T>? items) => new ReadOnlyCollection<T>((items ?? []).ToList());

#if NET6_0_OR_GREATER
	internal static bool ContainsOrdinal(this string text, string value) => text.Contains(value, StringComparison.Ordinal);
	internal static string ReplaceOrdinal(this string text, string oldValue, string newValue) => text.Replace(oldValue, newValue, StringComparison.Ordinal);
#else
	internal static bool ContainsOrdinal(this string text, string value) => text.Contains(value);
	internal static string ReplaceOrdinal(this string text, string oldValue, string newValue) => text.Replace(oldValue, newValue);
#endif

	private static readonly Regex s_validNameRegex = new("^[a-zA-Z_][a-zA-Z0-9_]*$");
}
