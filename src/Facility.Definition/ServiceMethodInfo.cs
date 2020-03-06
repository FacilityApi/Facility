using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// A service method.
	/// </summary>
	public sealed class ServiceMethodInfo : ServiceMemberInfo
	{
		/// <summary>
		/// Creates a method.
		/// </summary>
		public ServiceMethodInfo(string name, IEnumerable<ServiceFieldInfo>? requestFields = null, IEnumerable<ServiceFieldInfo>? responseFields = null, IEnumerable<ServiceAttributeInfo>? attributes = null, string? summary = null, IEnumerable<string>? remarks = null, params ServicePart[] parts)
			: base(name, attributes, summary, remarks, parts)
		{
			RequestFields = requestFields.ToReadOnlyList();
			ResponseFields = responseFields.ToReadOnlyList();

			ValidateNoDuplicateNames(RequestFields, "request field");
			ValidateNoDuplicateNames(ResponseFields, "response field");
		}

		/// <summary>
		/// The request fields of the method.
		/// </summary>
		public IReadOnlyList<ServiceFieldInfo> RequestFields { get; }

		/// <summary>
		/// The response fields of the method.
		/// </summary>
		public IReadOnlyList<ServiceFieldInfo> ResponseFields { get; }

		private protected override IEnumerable<ServiceElementInfo> GetExtraChildrenCore() => RequestFields.AsEnumerable<ServiceElementInfo>().Concat(ResponseFields);
	}
}
