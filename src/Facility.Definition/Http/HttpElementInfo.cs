using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace Facility.Definition.Http
{
	/// <summary>
	/// Base class for HTTP service elements.
	/// </summary>
	public abstract class HttpElementInfo
	{
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
		/// The children of the element, if any.
		/// </summary>
		public abstract IEnumerable<HttpElementInfo> GetChildren();

		private protected HttpElementInfo()
		{
			m_validationErrors = new List<ServiceDefinitionError>();
		}

		private protected void AddValidationError(ServiceDefinitionError error) => m_validationErrors.Add(error);

		private protected IReadOnlyList<ServiceAttributeParameterInfo> GetHttpParameters(ServiceElementWithAttributesInfo element) =>
			element.TryGetHttpAttribute()?.Parameters ?? Array.Empty<ServiceAttributeParameterInfo>();

		private protected void AddInvalidHttpParameterError(ServiceAttributeParameterInfo parameter) =>
			AddValidationError(ServiceDefinitionUtility.CreateUnexpectedAttributeParameterError("http", parameter));

		private protected HttpStatusCode? TryParseStatusCodeInteger(ServiceAttributeParameterInfo parameter)
		{
			int.TryParse(parameter.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var valueAsInteger);
			if (valueAsInteger < 200 || valueAsInteger >= 599)
			{
				AddValidationError(new ServiceDefinitionError($"'{parameter.Name}' parameter must be an integer between 200 and 599.", parameter.GetPart(ServicePartKind.Value)?.Position));
				return null;
			}

			return (HttpStatusCode) valueAsInteger;
		}

		private readonly List<ServiceDefinitionError> m_validationErrors;
	}
}
