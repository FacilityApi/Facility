using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// Information about a service from a definition.
	/// </summary>
	public sealed class ServiceInfo : ServiceMemberInfo
	{
		/// <summary>
		/// Creates a service.
		/// </summary>
		public ServiceInfo(string name, IEnumerable<ServiceMemberInfo> members, IEnumerable<ServiceAttributeInfo>? attributes = null, string? summary = null, IEnumerable<string>? remarks = null, params ServicePart[] parts)
			: base(name, attributes, summary, remarks, parts)
		{
			Members = members.ToReadOnlyList();

			ValidateName();
			ValidateNoDuplicateNames(Members, "service member");

			var unsupportedMember = Members.FirstOrDefault(x => !(x is ServiceMethodInfo || x is ServiceDtoInfo || x is ServiceEnumInfo || x is ServiceErrorSetInfo));
			if (unsupportedMember != null)
				throw new InvalidOperationException($"Unsupported member type: {unsupportedMember.GetType()}");

			m_membersByName = Members.GroupBy(x => x.Name).ToDictionary(x => x.First().Name, x => x.First());

			m_typesByName = new Dictionary<string, ServiceTypeInfo>();
			foreach (var fieldGroup in GetDescendants().OfType<ServiceFieldInfo>().GroupBy(x => x.TypeName))
			{
				var type = ServiceTypeInfo.TryParse(fieldGroup.Key, FindMember);
				if (type != null)
					m_typesByName.Add(fieldGroup.Key, type);
				else
					AddValidationErrors(fieldGroup.Select(x => new ServiceDefinitionError($"Unknown field type '{x.TypeName}'.", x.GetPart(ServicePartKind.TypeName)?.Position)));
			}
		}

		/// <summary>
		/// All of the service members..
		/// </summary>
		public IReadOnlyList<ServiceMemberInfo> Members { get; }

		/// <summary>
		/// The methods.
		/// </summary>
		public IReadOnlyList<ServiceMethodInfo> Methods => Members.OfType<ServiceMethodInfo>().ToReadOnlyList();

		/// <summary>
		/// The DTOs.
		/// </summary>
		public IReadOnlyList<ServiceDtoInfo> Dtos => Members.OfType<ServiceDtoInfo>().ToReadOnlyList();

		/// <summary>
		/// The enumerated types.
		/// </summary>
		public IReadOnlyList<ServiceEnumInfo> Enums => Members.OfType<ServiceEnumInfo>().ToReadOnlyList();

		/// <summary>
		/// The error sets.
		/// </summary>
		public IReadOnlyList<ServiceErrorSetInfo> ErrorSets => Members.OfType<ServiceErrorSetInfo>().ToReadOnlyList();

		/// <summary>
		/// Finds the member of the specified name.
		/// </summary>
		public ServiceMemberInfo? FindMember(string name) => m_membersByName.TryGetValue(name, out var member) ? member : null;

		/// <summary>
		/// Gets the field type for a field.
		/// </summary>
		public ServiceTypeInfo? GetFieldType(ServiceFieldInfo field) =>
			m_typesByName.TryGetValue(field.TypeName, out var type) ? type : ServiceTypeInfo.TryParse(field.TypeName, FindMember);

		/// <summary>
		/// Excludes a tag from the service.
		/// </summary>
		public ServiceInfo ExcludeTag(string tagName)
		{
			if (TryExcludeTag(tagName, out var newService, out var errors))
				return newService;
			else
				throw new ServiceDefinitionException(errors);
		}

		/// <summary>
		/// Attempts to exclude a tag from service.
		/// </summary>
		public bool TryExcludeTag(string tagName, out ServiceInfo service, out IReadOnlyList<ServiceDefinitionError> errors)
		{
			service = new ServiceInfo(
				name: Name,
				members: Members.Where(ShouldNotExclude).Select(DoExcludeTag),
				attributes: Attributes,
				summary: Summary,
				remarks: Remarks,
				parts: GetParts().ToArray());

			errors = service.GetValidationErrors()
				.Select(x => new ServiceDefinitionError($"{x.Message} ('{tagName}' tags are excluded.)", x.Position))
				.ToList();

			return errors.Count == 0;

			bool ShouldNotExclude(ServiceElementWithAttributesInfo element) => !element.TagNames.Contains(tagName);

			ServiceMemberInfo DoExcludeTag(ServiceMemberInfo member)
			{
				if (member is ServiceMethodInfo method)
				{
					return new ServiceMethodInfo(
						name: method.Name,
						requestFields: method.RequestFields.Where(ShouldNotExclude),
						responseFields: method.ResponseFields.Where(ShouldNotExclude),
						attributes: method.Attributes,
						summary: method.Summary,
						remarks: method.Remarks,
						parts: method.GetParts().ToArray());
				}
				else if (member is ServiceDtoInfo dto)
				{
					return new ServiceDtoInfo(
						name: dto.Name,
						fields: dto.Fields.Where(ShouldNotExclude),
						attributes: dto.Attributes,
						summary: dto.Summary,
						remarks: dto.Remarks,
						parts: dto.GetParts().ToArray());
				}
				else if (member is ServiceEnumInfo @enum)
				{
					return new ServiceEnumInfo(
						name: @enum.Name,
						values: @enum.Values.Where(ShouldNotExclude),
						attributes: @enum.Attributes,
						summary: @enum.Summary,
						remarks: @enum.Remarks,
						parts: @enum.GetParts().ToArray());
				}
				else if (member is ServiceErrorSetInfo errorSet)
				{
					return new ServiceErrorSetInfo(
						name: errorSet.Name,
						errors: errorSet.Errors.Where(ShouldNotExclude),
						attributes: errorSet.Attributes,
						summary: errorSet.Summary,
						remarks: errorSet.Remarks,
						parts: errorSet.GetParts().ToArray());
				}
				else
				{
					return member;
				}
			}
		}

		private protected override IEnumerable<ServiceElementInfo> GetExtraChildrenCore() => Members;

		private readonly Dictionary<string, ServiceMemberInfo> m_membersByName;
		private readonly Dictionary<string, ServiceTypeInfo> m_typesByName;
	}
}
