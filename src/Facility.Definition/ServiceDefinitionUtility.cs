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
		/// Returns the attribute with the specified name.
		/// </summary>
		/// <remarks>Throws a ServiceDefinitionException if the attribute is duplicated.</remarks>
		public static ServiceAttributeInfo TryGetAttribute(this IServiceElementInfo element, string name)
		{
			var attributes = element?.Attributes;
			if (attributes == null)
				return null;

			var matchingAttributes = attributes.Where(x => x.Name == name).ToList();
			if (matchingAttributes.Count > 1)
				throw new ServiceDefinitionException($"'{name}' attribute is duplicated.", matchingAttributes[1].Position);

			return matchingAttributes.FirstOrDefault();
		}

		/// <summary>
		/// Returns the attribute parameter with the specified name.
		/// </summary>
		public static ServiceAttributeParameterInfo TryGetParameter(this ServiceAttributeInfo attribute, string name)
		{
			return attribute?.Parameters?.FirstOrDefault(x => x.Name == name);
		}

		/// <summary>
		/// Returns the value of the attribute parameter with the specified name.
		/// </summary>
		public static string TryGetParameterValue(this ServiceAttributeInfo attribute, string name)
		{
			return attribute.TryGetParameter(name)?.Value;
		}

		/// <summary>
		/// Returns true if the element has the 'obsolete' attribute.
		/// </summary>
		public static bool IsObsolete(this IServiceElementInfo element)
		{
			return element.TryGetAttribute("obsolete") != null;
		}

		internal static void ValidateName(string name, NamedTextPosition position)
		{
			if (!s_validNameRegex.IsMatch(name))
				throw new ServiceDefinitionException($"Invalid name '{name}'.", position);
		}

		internal static void ValidateTypeName(string name, NamedTextPosition position)
		{
			ServiceTypeInfo.Parse(name, x => s_validNameRegex.IsMatch(x) ? new ServiceDtoInfo(x) : null, position);
		}

		internal static void ValidateNoDuplicateNames(IEnumerable<IServiceNamedInfo> infos, string description)
		{
			var duplicate = infos
				.GroupBy(x => x.Name.ToLowerInvariant())
				.Where(x => x.Count() != 1)
				.Select(x => x.Skip(1).First())
				.FirstOrDefault();
			if (duplicate != null)
				throw new ServiceDefinitionException($"Duplicate {description}: {duplicate.Name}", duplicate.Position);
		}

		internal static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> items)
		{
			return new ReadOnlyCollection<T>((items ?? Enumerable.Empty<T>()).ToList());
		}

		static readonly Regex s_validNameRegex = new Regex(@"^[a-zA-Z][a-zA-Z0-9]*$");
	}
}
