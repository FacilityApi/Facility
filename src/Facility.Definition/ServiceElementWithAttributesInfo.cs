using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// Properties common to service members with attributes.
	/// </summary>
	public abstract class ServiceElementWithAttributesInfo : ServiceElementInfo
	{
		/// <summary>
		/// The attributes of the service element.
		/// </summary>
		[SuppressMessage("Naming", "CA1721:Property names should not match get methods", Justification = "Legacy.")]
		public IReadOnlyList<ServiceAttributeInfo> Attributes { get; }

		/// <summary>
		/// True if the element is obsolete.
		/// </summary>
		public bool IsObsolete { get; }

		/// <summary>
		/// The obsolete message, if any.
		/// </summary>
		public string? ObsoleteMessage { get; }

		/// <summary>
		/// The names of the tags of the element, if any.
		/// </summary>
		public IReadOnlyList<string> TagNames { get; }

		/// <summary>
		/// Returns any attributes with the specified name.
		/// </summary>
		public IReadOnlyList<ServiceAttributeInfo> GetAttributes(string name) => Attributes.Where(x => x.Name == name).ToList();

		/// <summary>
		/// Returns the first attribute with the specified name, if any.
		/// </summary>
		public ServiceAttributeInfo? TryGetAttribute(string name) => Attributes.FirstOrDefault(x => x.Name == name);

		/// <summary>
		/// The children of the service element, if any.
		/// </summary>
		public sealed override IEnumerable<ServiceElementInfo> GetChildren() => Attributes.AsEnumerable<ServiceElementInfo>().Concat(GetExtraChildrenCore());

		private protected ServiceElementWithAttributesInfo(IEnumerable<ServiceAttributeInfo>? attributes, IReadOnlyList<ServicePart> parts)
			: base(parts)
		{
			Attributes = attributes.ToReadOnlyList();

			var obsoleteAttributes = GetAttributes("obsolete");
			if (obsoleteAttributes.Count > 1)
				AddValidationError(ServiceDefinitionUtility.CreateDuplicateAttributeError(obsoleteAttributes[1]));
			var obsoleteAttribute = obsoleteAttributes.Count == 0 ? null : obsoleteAttributes[0];
			if (obsoleteAttribute != null)
			{
				IsObsolete = true;

				foreach (var obsoleteParameter in obsoleteAttribute.Parameters)
				{
					if (obsoleteParameter.Name == "message")
						ObsoleteMessage = obsoleteParameter.Value;
					else
						AddValidationError(ServiceDefinitionUtility.CreateUnexpectedAttributeParameterError(obsoleteAttribute.Name, obsoleteParameter));
				}
			}

			var tagNames = new List<string>();
			var tagAttributes = GetAttributes("tag");
			foreach (var tagAttribute in tagAttributes)
			{
				string? tagName = null;
				foreach (var tagParameter in tagAttribute.Parameters)
				{
					if (tagParameter.Name == "name")
						tagName = tagParameter.Value;
					else
						AddValidationError(ServiceDefinitionUtility.CreateUnexpectedAttributeParameterError(tagParameter.Name, tagParameter));
				}

				if (tagName != null)
					tagNames.Add(tagName);
				else
					AddValidationError(new ServiceDefinitionError("'tag' attribute is missing required 'name' parameter.", tagAttribute.Position));
			}

			TagNames = tagNames;
		}

		private protected abstract IEnumerable<ServiceElementInfo> GetExtraChildrenCore();
	}
}
