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
		public ServiceInfo(string name, IEnumerable<IServiceMemberInfo> members = null, IEnumerable<ServiceAttributeInfo> attributes = null, string summary = null, IEnumerable<string> remarks = null, NamedTextPosition position = null, bool validate = true)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Name = name;
			Members = members.ToReadOnlyList();
			Attributes = attributes.ToReadOnlyList();
			Summary = summary ?? "";
			Remarks = remarks.ToReadOnlyList();
			Position = position;

			m_membersByName = Members.ToLookup(x => x.Name);

			if (validate)
			{
				var error = Validate().FirstOrDefault();
				if (error != null)
					throw error.CreateException();
			}
		}

		internal IEnumerable<ServiceDefinitionError> Validate()
		{
			foreach (var error in ServiceDefinitionUtility.ValidateName2(Name, Position))
				yield return error;

			foreach (var member in Members)
			{
				if (!(member is ServiceMethodInfo) && !(member is ServiceDtoInfo) && !(member is ServiceEnumInfo) && !(member is ServiceErrorSetInfo))
					yield return new ServiceDefinitionError($"Unsupported member type '{member.GetType()}'.");
			}

			foreach (var error in ServiceDefinitionUtility.ValidateNoDuplicateNames2(Members, "service member"))
				yield return error;

			foreach (var field in Methods.SelectMany(x => x.RequestFields.Concat(x.ResponseFields)).Concat(Dtos.SelectMany(x => x.Fields)))
			{
				ServiceDefinitionError error;
				ServiceTypeInfo.TryParse(field.TypeName, FindMember, field.Position, out error);
				if (error != null)
					yield return error;
			}
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
			return m_membersByName[name].SingleOrDefault();
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
			return ServiceTypeInfo.Parse(field.TypeName, FindMember, field.Position);
		}

		readonly ILookup<string, IServiceMemberInfo> m_membersByName;
	}
}
