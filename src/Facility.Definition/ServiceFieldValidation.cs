using System;
using System.Globalization;

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

		private static ServiceFieldValidationRange<T>? GetRange<T>(ServiceAttributeInfo attribute, ServiceAttributeParameterInfo parameter, Func<string, T?> parse)
			where T : struct
		{
			if (string.IsNullOrEmpty(parameter.Value))
			{
				parameter.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attribute.Name, parameter));
				return null;
			}

			var bounds = parameter.Value.Split(new[] { ".." }, 2, StringSplitOptions.None);
			var first = bounds[0];
			var second = bounds.Length > 1 ? bounds[1] : null;

			if (bounds.Length == 1)
			{
				var value = parse(parameter.Value);
				if (value == null)
				{
					parameter.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attribute.Name, parameter));
					return null;
				}

				return new ServiceFieldValidationRange<T>(value, value);
			}

			if (string.IsNullOrEmpty(first) && !string.IsNullOrEmpty(second))
			{
				var upperBound = parse(second!);
				if (upperBound == null)
				{
					parameter.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attribute.Name, parameter));
					return null;
				}

				return new ServiceFieldValidationRange<T>(null, upperBound);
			}

			if (!string.IsNullOrEmpty(first) && string.IsNullOrEmpty(second))
			{
				var lowerBound = parse(first);
				if (lowerBound == null)
				{
					parameter.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attribute.Name, parameter));
					return null;
				}

				return new ServiceFieldValidationRange<T>(lowerBound, null);
			}

			var minimum = parse(first);
			var maximum = parse(second!);
			if (minimum == null || maximum == null)
			{
				parameter.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attribute.Name, parameter));
				return null;
			}

			return new ServiceFieldValidationRange<T>(minimum, maximum);
		}
	}
}
