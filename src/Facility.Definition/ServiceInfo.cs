using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// Information about a service from a definition.
	/// </summary>
	public sealed class ServiceInfo : IServiceMemberInfo
	{
		/// <summary>
		/// Creates a service.
		/// </summary>
		public ServiceInfo(string name, IEnumerable<IServiceMemberInfo> members = null, IEnumerable<ServiceAttributeInfo> attributes = null, string summary = null, IEnumerable<string> remarks = null, NamedTextPosition position = null)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Name = name;
			Members = members.ToReadOnlyList();
			Attributes = attributes.ToReadOnlyList();
			Summary = summary ?? "";
			Remarks = remarks.ToReadOnlyList();
			Position = position;

			ServiceDefinitionUtility.ValidateName(Name, Position);

			foreach (var member in Members)
			{
				if (!(member is ServiceMethodInfo) && !(member is ServiceDtoInfo) && !(member is ServiceEnumInfo) && !(member is ServiceErrorSetInfo))
					throw new ServiceDefinitionException($"Unsupported member type '{member.GetType()}'.");
			}

			ServiceDefinitionUtility.ValidateNoDuplicateNames(Members, "service member");
			m_membersByName = new ReadOnlyDictionary<string, IServiceMemberInfo>(Members.ToDictionary(x => x.Name, x => x));

			var fieldTypes = new Dictionary<string, ServiceTypeInfo>();
			foreach (var field in Methods.SelectMany(x => x.RequestFields.Concat(x.ResponseFields)).Concat(Dtos.SelectMany(x => x.Fields)))
			{
				if (!fieldTypes.ContainsKey(field.TypeName))
					fieldTypes.Add(field.TypeName, ServiceTypeInfo.Parse(field.TypeName, FindMember, field.Position));
			}
			m_fieldTypes = new ReadOnlyDictionary<string, ServiceTypeInfo>(fieldTypes);
		}

		/// <summary>
		/// The service name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// All of the service members..
		/// </summary>
		public IReadOnlyList<IServiceMemberInfo> Members { get; }

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
		/// The service attributes.
		/// </summary>
		public IReadOnlyList<ServiceAttributeInfo> Attributes { get; }

		/// <summary>
		/// The service summary.
		/// </summary>
		public string Summary { get; }

		/// <summary>
		/// The service remarks.
		/// </summary>
		public IReadOnlyList<string> Remarks { get; }

		/// <summary>
		/// The position of the service.
		/// </summary>
		public NamedTextPosition Position { get; }

		/// <summary>
		/// Finds the member of the specified name.
		/// </summary>
		public IServiceMemberInfo FindMember(string name)
		{
			IServiceMemberInfo member;
			m_membersByName.TryGetValue(name, out member);
			return member;
		}

		/// <summary>
		/// Gets the type of the specified name.
		/// </summary>
		public ServiceTypeInfo GetType(string typeName)
		{
			return ServiceTypeInfo.Parse(typeName, FindMember);
		}

		/// <summary>
		/// Gets the field type for a field.
		/// </summary>
		public ServiceTypeInfo GetFieldType(ServiceFieldInfo field)
		{
			ServiceTypeInfo type;
			if (!m_fieldTypes.TryGetValue(field.TypeName, out type))
				throw new ArgumentException("Unexpected field.", nameof(field));
			return type;
		}

		readonly ReadOnlyDictionary<string, IServiceMemberInfo> m_membersByName;
		readonly ReadOnlyDictionary<string, ServiceTypeInfo> m_fieldTypes;
	}
}
