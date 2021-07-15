using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Facility.Definition
{
	/// <summary>
	/// A validation criterion of a DTO field.
	/// </summary>
	public class ServiceFieldValidation
	{
		/// <summary>
		/// Creates a validation criterion of a DTO field
		/// </summary>
		public ServiceFieldValidation(ServiceAttributeInfo attributeInfo)
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
						if (s_regex.IsMatch(parameterInfo.Value))
							RegularExpression = new Regex(parameterInfo.Value);
						else
							parameterInfo.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attributeInfo.Name, parameterInfo));

						break;
					default:
						parameterInfo.AddValidationError(ServiceDefinitionUtility.CreateUnexpectedAttributeParameterError(parameterInfo.Name, parameterInfo));
						break;
				}
			}
		}

		/// <summary>
		/// Allowed range for the collection entry count
		/// </summary>
		public ServiceFieldValidationRange? CountRange { get; }

		/// <summary>
		/// Allowed range for the numeric value
		/// </summary>
		public ServiceFieldValidationRange? ValueRange { get; }

		/// <summary>
		/// Allowed range for the string length
		/// </summary>
		public ServiceFieldValidationRange? LengthRange { get; }

		/// <summary>
		/// Allowed pattern to which a string must conform
		/// </summary>
		public Regex? RegularExpression { get; }

		private static ServiceFieldValidationRange? GetRange(ServiceAttributeInfo attributeInfo, ServiceAttributeParameterInfo parameterInfo)
		{
			if (string.IsNullOrEmpty(parameterInfo.Value)) return null;

			var fullRangeMatch = s_fullRange.Matches(parameterInfo.Value);
			if (fullRangeMatch.Count > 0)
			{
				var start = Convert.ToDecimal(fullRangeMatch[0], CultureInfo.InvariantCulture);
				var end = Convert.ToDecimal(fullRangeMatch[1], CultureInfo.InvariantCulture);
				return new ServiceFieldValidationRange(start, end);
			}

			var unboundedStartMatch = s_unboundedStartRange.Matches(parameterInfo.Value);
			if (unboundedStartMatch.Count > 0)
			{
				var start = Convert.ToDecimal(unboundedStartMatch[0], CultureInfo.InvariantCulture);
				return new ServiceFieldValidationRange(start, null);
			}

			var unboundedEndMatch = s_unboundedEndRange.Matches(parameterInfo.Value);
			if (unboundedEndMatch.Count > 0)
			{
				var end = Convert.ToDecimal(unboundedEndMatch[0], CultureInfo.InvariantCulture);
				return new ServiceFieldValidationRange(null, end);
			}

			if (s_number.IsMatch(parameterInfo.Value))
			{
				var number = Convert.ToDecimal(parameterInfo.Value, CultureInfo.InvariantCulture);
				return new ServiceFieldValidationRange(number, number);
			}

			parameterInfo.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attributeInfo.Name, parameterInfo));

			return null;
		}

		private static readonly Regex s_regex = new(@"/.+/");
		private static readonly Regex s_number = new(@"\d+(?:\.\d+)?");
		private static readonly Regex s_unboundedStartRange = new(@"\.\.(\d+(?:\.\d+)?)");
		private static readonly Regex s_unboundedEndRange = new(@"(\d+(?:\.\d+)?)\.\.");
		private static readonly Regex s_fullRange = new(@"(\d+(?:\.\d+)?)\.\.(\d+(?:\.\d+)?)");
	}
}
