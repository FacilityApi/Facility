using System;
using System.Collections.Generic;

namespace Facility.Definition
{
	/// <summary>
	/// A service method.
	/// </summary>
	public sealed class ServiceMethodInfo : IServiceMemberInfo
	{
		/// <summary>
		/// Creates a method.
		/// </summary>
		public ServiceMethodInfo(string name, IEnumerable<ServiceFieldInfo> requestFields = null, IEnumerable<ServiceFieldInfo> responseFields = null, IEnumerable<ServiceAttributeInfo> attributes = null, string summary = null, IEnumerable<string> remarks = null, NamedTextPosition position = null)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Name = name;
			RequestFields = requestFields.ToReadOnlyList();
			ResponseFields = responseFields.ToReadOnlyList();
			Attributes = attributes.ToReadOnlyList();
			Summary = summary ?? "";
			Remarks = remarks.ToReadOnlyList();
			Position = position;

			ServiceDefinitionUtility.ValidateName(Name, Position);
			ServiceDefinitionUtility.ValidateNoDuplicateNames(RequestFields, "request field");
			ServiceDefinitionUtility.ValidateNoDuplicateNames(ResponseFields, "response field");
		}

		/// <summary>
		/// The name of the method.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The request fields of the method.
		/// </summary>
		public IReadOnlyList<ServiceFieldInfo> RequestFields { get; }

		/// <summary>
		/// The response fields of the method.
		/// </summary>
		public IReadOnlyList<ServiceFieldInfo> ResponseFields { get; }

		/// <summary>
		/// The attributes of the method.
		/// </summary>
		public IReadOnlyList<ServiceAttributeInfo> Attributes { get; }

		/// <summary>
		/// The summary of the method.
		/// </summary>
		public string Summary { get; }

		/// <summary>
		/// The remarks of the method.
		/// </summary>
		public IReadOnlyList<string> Remarks { get; }

		/// <summary>
		/// The position of the method in the definition.
		/// </summary>
		public NamedTextPosition Position { get; }
	}
}
