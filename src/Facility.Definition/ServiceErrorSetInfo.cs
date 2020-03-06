using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// An error set.
	/// </summary>
	public sealed class ServiceErrorSetInfo : ServiceMemberInfo
	{
		/// <summary>
		/// Creates an error set.
		/// </summary>
		public ServiceErrorSetInfo(string name, IEnumerable<ServiceErrorInfo> errors, IEnumerable<ServiceAttributeInfo>? attributes, string? summary, IEnumerable<string>? remarks, params ServicePart[] parts)
			: base(name, attributes, summary, remarks, parts)
		{
			Errors = errors.ToReadOnlyList();

			ValidateNoDuplicateNames(Errors, "error");
		}

		/// <summary>
		/// The errors of the error set.
		/// </summary>
		public IReadOnlyList<ServiceErrorInfo> Errors { get; }

		private protected override IEnumerable<ServiceElementInfo> GetExtraChildrenCore() => Attributes.AsEnumerable<ServiceElementInfo>().Concat(Errors);
	}
}
