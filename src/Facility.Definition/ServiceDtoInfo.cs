using System.Collections.Generic;

namespace Facility.Definition
{
	/// <summary>
	/// A service DTO.
	/// </summary>
	public sealed class ServiceDtoInfo : ServiceMemberInfo
	{
		/// <summary>
		/// Creates a DTO.
		/// </summary>
		public ServiceDtoInfo(string name, IEnumerable<ServiceFieldInfo>? fields = null, IEnumerable<ServiceAttributeInfo>? attributes = null, string? summary = null, IEnumerable<string>? remarks = null, params ServicePart[] parts)
			: base(name, attributes, summary, remarks, parts)
		{
			Fields = fields.ToReadOnlyList();

			ValidateNoDuplicateNames(Fields, "field");
		}

		/// <summary>
		/// The fields of the DTO.
		/// </summary>
		public IReadOnlyList<ServiceFieldInfo> Fields { get; }

		private protected override IEnumerable<ServiceElementInfo> GetExtraChildrenCore() => Fields;
	}
}
