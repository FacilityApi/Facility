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
						LengthRange = GetRange(parameterInfo);
						break;
					case "count":
						CountRange = GetRange(parameterInfo);
						break;
					case "value":
						ValueRange = GetRange(parameterInfo);
						break;
					case "regex":
						RegularExpression = new Regex(parameterInfo.Value);
						break;
					default:
						parameterInfo.AddValidationError(ServiceDefinitionUtility.CreateUnexpectedAttributeParameterError(parameterInfo.Name, parameterInfo));
						break;
				}
			}
		}

		public ServiceFieldValidationRange? CountRange { get; }
		public ServiceFieldValidationRange? ValueRange { get; }
		public ServiceFieldValidationRange? LengthRange { get; }
		public Regex? RegularExpression { get; }

		private ServiceFieldValidationRange? GetRange(ServiceAttributeParameterInfo parameterInfo)
		{
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

			return null;
		}

		private static readonly Regex s_number = new(@"\d+(?:\.\d+)?");
		private static readonly Regex s_unboundedStartRange = new(@"\.\.(\d+(?:\.\d+)?)");
		private static readonly Regex s_unboundedEndRange = new(@"(\d+(?:\.\d+)?)\.\.");
		private static readonly Regex s_fullRange = new(@"(\d+(?:\.\d+)?)\.\.(\d+(?:\.\d+)?)");
	}
}
