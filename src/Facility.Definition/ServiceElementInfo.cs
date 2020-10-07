using System;
using System.Collections.Generic;
using System.Linq;

namespace Facility.Definition
{
	/// <summary>
	/// Base class for service elements.
	/// </summary>
	public abstract class ServiceElementInfo
	{
		/// <summary>
		/// The position of the element.
		/// </summary>
		public ServiceDefinitionPosition? Position => m_parts.FirstOrDefault()?.Position;

		/// <summary>
		/// True if the element has no validation errors.
		/// </summary>
		public bool IsValid => !GetValidationErrors().Any();

		/// <summary>
		/// Gets the validation errors for the element, if any, including those of descendants by default.
		/// </summary>
		public IEnumerable<ServiceDefinitionError> GetValidationErrors(bool recurse = true)
		{
			var validationErrors = m_validationErrors.AsEnumerable();
			if (recurse)
				validationErrors = validationErrors.Concat(GetChildren().SelectMany(x => x.GetValidationErrors()));
			return validationErrors;
		}

		/// <summary>
		/// Gets the parts of the element, if any.
		/// </summary>
		public IReadOnlyList<ServicePart> GetParts() => m_parts;

		/// <summary>
		/// Gets the specified part of the element, if any.
		/// </summary>
		public ServicePart? GetPart(ServicePartKind kind) => m_parts.FirstOrDefault(x => x.Kind == kind);

		/// <summary>
		/// Returns the children of the service element, if any.
		/// </summary>
		public abstract IEnumerable<ServiceElementInfo> GetChildren();

		/// <summary>
		/// Returns the descendants of the element.
		/// </summary>
		public IEnumerable<ServiceElementInfo> GetDescendants() => GetChildren().SelectMany(x => x.GetElementAndDescendants());

		/// <summary>
		/// Returns the element followed by its descendants.
		/// </summary>
		public IEnumerable<ServiceElementInfo> GetElementAndDescendants() => new[] { this }.Concat(GetDescendants());

		private protected ServiceElementInfo(IEnumerable<ServicePart> parts)
		{
			m_parts = parts.ToReadOnlyList();
			m_validationErrors = new List<ServiceDefinitionError>();
		}

		private protected void AddValidationError(ServiceDefinitionError error)
		{
			if (error == null)
				throw new ArgumentNullException(nameof(error));

			m_validationErrors.Add(error);
		}

		private protected void AddValidationErrors(IEnumerable<ServiceDefinitionError> errors)
		{
			if (errors == null)
				throw new ArgumentNullException(nameof(errors));

			foreach (var error in errors)
				AddValidationError(error);
		}

		private protected void ValidateName()
		{
			var name = ((IServiceHasName) this).Name;
			if (!ServiceDefinitionUtility.IsValidName(name))
				AddValidationError(new ServiceDefinitionError($"Invalid name '{name}'.", Position));
		}

		private protected void ValidateNoDuplicateNames<T>(IEnumerable<T> element, string description)
			where T : ServiceElementInfo, IServiceHasName
		{
			AddValidationErrors(element
				.GroupBy(x => x.Name.ToLowerInvariant())
				.Where(x => x.Count() != 1)
				.Select(x => x.Skip(1).First())
				.Select(duplicate => new ServiceDefinitionError($"Duplicate {description}: {duplicate.Name}", duplicate.Position)));
		}

		private readonly IReadOnlyList<ServicePart> m_parts;
		private readonly List<ServiceDefinitionError> m_validationErrors;
	}
}
