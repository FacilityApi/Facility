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
		internal ServiceFieldValidation(ServiceAttributeInfo attribute)
		{
			Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));

			foreach (var parameterInfo in attribute.Parameters)
			{
				switch (parameterInfo.Name)
				{
					case "length":
						LengthRange = GetRange<ulong>(attribute, parameterInfo, s => ulong.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : null);
						break;
					case "count":
						CountRange = GetRange<ulong>(attribute, parameterInfo, s => ulong.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : null);
						break;
					case "value":
						ValueRange = GetRange<long>(attribute, parameterInfo, s => long.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : null);
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

		internal ServiceAttributeInfo Attribute { get; }

		/// <summary>
		/// Allowed range for the collection entry count.
		/// </summary>
		public ServiceFieldValidationRange<ulong>? CountRange { get; }

		/// <summary>
		/// Allowed range for the numeric value.
		/// </summary>
		public ServiceFieldValidationRange<long>? ValueRange { get; }

		/// <summary>
		/// Allowed range for the string length.
		/// </summary>
		public ServiceFieldValidationRange<ulong>? LengthRange { get; }

		/// <summary>
		/// Allowed pattern to which a string must conform.
		/// </summary>
		public string? RegexPattern { get; }

		private static ServiceFieldValidationRange<T>? GetRange<T>(ServiceAttributeInfo attributeInfo, ServiceAttributeParameterInfo parameterInfo, Func<string, T?> parse)
			where T : struct
		{
			if (string.IsNullOrEmpty(parameterInfo.Value))
			{
				parameterInfo.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attributeInfo.Name, parameterInfo));
				return null;
			}

			var fullRangeMatch = s_fullRange.Match(parameterInfo.Value).Groups;
			if (fullRangeMatch.Count > 2)
			{
				var start = parse(fullRangeMatch[1].Value);
				var end = parse(fullRangeMatch[2].Value);
				if (start == null || end == null)
				{
					parameterInfo.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attributeInfo.Name, parameterInfo));
					return null;
				}

				return new ServiceFieldValidationRange<T>(start, end);
			}

			var unboundedStartMatch = s_unboundedStartRange.Match(parameterInfo.Value).Groups;
			if (unboundedStartMatch.Count > 1)
			{
				var end = parse(unboundedStartMatch[1].Value);
				if (end == null)
				{
					parameterInfo.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attributeInfo.Name, parameterInfo));
					return null;
				}

				return new ServiceFieldValidationRange<T>(null, end);
			}

			var unboundedEndMatch = s_unboundedEndRange.Match(parameterInfo.Value).Groups;
			if (unboundedEndMatch.Count > 1)
			{
				var start = parse(unboundedEndMatch[1].Value);
				if (start == null)
				{
					parameterInfo.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attributeInfo.Name, parameterInfo));
					return null;
				}

				return new ServiceFieldValidationRange<T>(start, null);
			}

			var value = parse(parameterInfo.Value);
			if (value == null)
			{
				parameterInfo.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attributeInfo.Name, parameterInfo));
				return null;
			}

			return new ServiceFieldValidationRange<T>(value, value);
		}

		private static readonly Regex s_unboundedStartRange = new(@"^\.\.([0-9]+(?:\.[0-9]+)?)$");
		private static readonly Regex s_unboundedEndRange = new(@"^([0-9]+(?:\.[0-9]+)?)\.\.$");
		private static readonly Regex s_fullRange = new(@"^([0-9]+(?:\.[0-9]+)?)\.\.([0-9]+(?:\.[0-9]+)?)$");
	}
}
