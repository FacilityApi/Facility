using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Facility.Definition
{
	/// <summary>
	/// A validation criterion of a DTO field.
	/// </summary>
	public sealed class ServiceFieldValidation
	{
		/// <summary>
		/// Creates a validation criterion of a DTO field.
		/// </summary>
		internal ServiceFieldValidation(ServiceAttributeInfo attributeInfo)
		{
			foreach (var parameterInfo in attributeInfo.Parameters)
			{
				switch (parameterInfo.Name)
				{
					case "length":
						LengthRange = GetRange(attributeInfo, parameterInfo);
						break;
					case "count":
						CountRange = GetRange(attributeInfo, parameterInfo);
						break;
					case "value":
						ValueRange = GetRange(attributeInfo, parameterInfo);
						break;
					case "regex":
						RegexPattern = parameterInfo.Value;
						break;
					default:
						parameterInfo.AddValidationError(ServiceDefinitionUtility.CreateUnexpectedAttributeParameterError(parameterInfo.Name, parameterInfo));
						break;
				}
			}
		}

		/// <summary>
		/// Allowed range for the collection entry count.
		/// </summary>
		public ServiceFieldValidationRange? CountRange { get; }

		/// <summary>
		/// Allowed range for the numeric value.
		/// </summary>
		public ServiceFieldValidationRange? ValueRange { get; }

		/// <summary>
		/// Allowed range for the string length.
		/// </summary>
		public ServiceFieldValidationRange? LengthRange { get; }

		/// <summary>
		/// Allowed pattern to which a string must conform.
		/// </summary>
		public string? RegexPattern { get; }

		private static ServiceFieldValidationRange? GetRange(ServiceAttributeInfo attributeInfo, ServiceAttributeParameterInfo parameterInfo)
		{
			if (string.IsNullOrEmpty(parameterInfo.Value))
			{
				parameterInfo.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attributeInfo.Name, parameterInfo));
				return null;
			}

			var fullRangeMatch = s_fullRange.Match(parameterInfo.Value).Groups;
			if (fullRangeMatch.Count > 2)
			{
				if (!decimal.TryParse(fullRangeMatch[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var start)
					|| !decimal.TryParse(fullRangeMatch[2].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var end))
				{
					parameterInfo.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attributeInfo.Name, parameterInfo));
					return null;
				}

				return new ServiceFieldValidationRange(start, end);
			}

			var unboundedStartMatch = s_unboundedStartRange.Match(parameterInfo.Value).Groups;
			if (unboundedStartMatch.Count > 1)
			{
				if (!decimal.TryParse(unboundedStartMatch[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var end))
				{
					parameterInfo.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attributeInfo.Name, parameterInfo));
					return null;
				}

				return new ServiceFieldValidationRange(null, end);
			}

			var unboundedEndMatch = s_unboundedEndRange.Match(parameterInfo.Value).Groups;
			if (unboundedEndMatch.Count > 1)
			{
				if (!decimal.TryParse(unboundedEndMatch[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var start))
				{
					parameterInfo.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attributeInfo.Name, parameterInfo));
					return null;
				}

				return new ServiceFieldValidationRange(start, null);
			}

			if (s_number.IsMatch(parameterInfo.Value))
			{
				if (!decimal.TryParse(parameterInfo.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
				{
					parameterInfo.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attributeInfo.Name, parameterInfo));
					return null;
				}

				return new ServiceFieldValidationRange(value, value);
			}

			parameterInfo.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attributeInfo.Name, parameterInfo));

			return null;
		}

		private static readonly Regex s_number = new(@"^[0-9]+(?:\.[0-9]+)?$");
		private static readonly Regex s_unboundedStartRange = new(@"^\.\.([0-9]+(?:\.[0-9]+)?)$");
		private static readonly Regex s_unboundedEndRange = new(@"^([0-9]+(?:\.[0-9]+)?)\.\.$");
		private static readonly Regex s_fullRange = new(@"^([0-9]+(?:\.[0-9]+)?)\.\.([0-9]+(?:\.[0-9]+)?)$");
	}
}
