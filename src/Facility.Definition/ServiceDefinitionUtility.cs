using System;
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
		/// Attempts to determine the format of the service definition.
		/// </summary>
		public static ServiceDefinitionFormat? DetectFormat(NamedText namedText)
		{
			if (namedText.Name.EndsWith(".fsd", StringComparison.OrdinalIgnoreCase))
				return ServiceDefinitionFormat.Fsd;

			if (namedText.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
				namedText.Name.EndsWith(".yml", StringComparison.OrdinalIgnoreCase) ||
				namedText.Name.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
			{
				return ServiceDefinitionFormat.Swagger;
			}

			return null;
		}

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
			return element.TryGetObsoleteAttribute() != null;
		}

		/// <summary>
		/// Returns the obsolete message for an element with the 'obsolete' attribute.
		/// </summary>
		/// <remarks>Use <see cref="IsObsolete"/> to determine if the element is obsolete.</remarks>
		public static string TryGetObsoleteMessage(this IServiceElementInfo element)
		{
			return element.TryGetObsoleteAttribute()?.TryGetParameterValue("message");
		}

		/// <summary>
		/// Returns any tag names for the element.
		/// </summary>
		public static IReadOnlyList<string> GetTagNames(this IServiceElementInfo element)
		{
			return element?.Attributes?.Where(x => x.Name == "tag").Select(x => x.TryGetParameterValue("name")).ToList();
		}

		/// <summary>
		/// Returns true if the name is a valid service member name.
		/// </summary>
		public static bool IsValidName(string name)
		{
			return name != null && s_validNameRegex.IsMatch(name);
		}

		/// <summary>
		/// Excludes a tag from the specified service.
		/// </summary>
		public static ServiceInfo ExcludeTag(this ServiceInfo service, string tagName)
		{
			if (TryExcludeTag(service, tagName, out var newService, out var errors))
				return newService;
			else
				throw new ServiceDefinitionException(errors);
		}

		/// <summary>
		/// Attempts to exclude a tag from the specified service.
		/// </summary>
		public static bool TryExcludeTag(this ServiceInfo service, string tagName, out ServiceInfo newService, out IReadOnlyList<ServiceDefinitionError> errors)
		{
			newService = new ServiceInfo(ValidationMode.Return,
				name: service.Name,
				members: service.Members.Where(shouldNotExclude).Select(excludeTag),
				attributes: service.Attributes,
				summary: service.Summary,
				remarks: service.Remarks,
				position: service.Position);

			errors = newService.GetValidationErrors()
				.Select(x => new ServiceDefinitionError($"{x.Message} ('{tagName}' tags are excluded.)", x.Position, x.Exception))
				.ToList();

			return errors.Count == 0;

			bool shouldNotExclude(IServiceElementInfo element) => !element.GetTagNames().Contains(tagName);

			IServiceMemberInfo excludeTag(IServiceMemberInfo member)
			{
				if (member is ServiceMethodInfo method)
				{
					return new ServiceMethodInfo(ValidationMode.Return,
						name: method.Name,
						requestFields: method.RequestFields.Where(shouldNotExclude),
						responseFields: method.ResponseFields.Where(shouldNotExclude),
						attributes: method.Attributes,
						summary: method.Summary,
						remarks: method.Remarks,
						position: method.Position);
				}
				else if (member is ServiceDtoInfo dto)
				{
					return new ServiceDtoInfo(ValidationMode.Return,
						name: dto.Name,
						fields: dto.Fields.Where(shouldNotExclude),
						attributes: dto.Attributes,
						summary: dto.Summary,
						remarks: dto.Remarks,
						position: dto.Position);
				}
				else if (member is ServiceEnumInfo @enum)
				{
					return new ServiceEnumInfo(ValidationMode.Return,
						name: @enum.Name,
						values: @enum.Values.Where(shouldNotExclude),
						attributes: @enum.Attributes,
						summary: @enum.Summary,
						remarks: @enum.Remarks,
						position: @enum.Position);
				}
				else if (member is ServiceErrorSetInfo errorSet)
				{
					return new ServiceErrorSetInfo(ValidationMode.Return,
						name: errorSet.Name,
						errors: errorSet.Errors.Where(shouldNotExclude),
						attributes: errorSet.Attributes,
						summary: errorSet.Summary,
						remarks: errorSet.Remarks,
						position: errorSet.Position);
				}
				else
				{
					return member;
				}
			}
		}

		internal static IEnumerable<ServiceDefinitionError> ValidateName(string name, NamedTextPosition position)
		{
			if (!IsValidName(name))
				yield return new ServiceDefinitionError($"Invalid name '{name}'.", position);
		}

		internal static IEnumerable<ServiceDefinitionError> ValidateTypeName(string name, NamedTextPosition position)
		{
			if (ServiceTypeInfo.TryParse(name, x => s_validNameRegex.IsMatch(x) ? new ServiceDtoInfo(x) : null, position, out var error) == null)
				yield return error;
		}

		internal static IEnumerable<ServiceDefinitionError> ValidateNoDuplicateNames(IEnumerable<IServiceNamedInfo> infos, string description)
		{
			return infos
				.GroupBy(x => x.Name.ToLowerInvariant())
				.Where(x => x.Count() != 1)
				.Select(x => x.Skip(1).First())
				.Select(duplicate => new ServiceDefinitionError($"Duplicate {description}: {duplicate.Name}", duplicate.Position));
		}

		internal static void ThrowIfAny(this IEnumerable<ServiceDefinitionError> errors)
		{
			var errorList = errors.ToList();
			if (errorList.Count != 0)
				throw new ServiceDefinitionException(errorList);
		}

		internal static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> items)
		{
			return new ReadOnlyCollection<T>((items ?? Enumerable.Empty<T>()).ToList());
		}

		private static ServiceAttributeInfo TryGetObsoleteAttribute(this IServiceElementInfo element)
		{
			return element.TryGetAttribute("obsolete");
		}

		static readonly Regex s_validNameRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");
	}
}
