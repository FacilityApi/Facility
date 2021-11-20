using System.Globalization;

namespace Facility.Definition;

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

		foreach (var parameter in attribute.Parameters)
		{
			switch (parameter.Name)
			{
				case "length":
					var length = GetRange(attribute, parameter);
					if (length != null && length.IsValid() && length.Minimum is null or >= 0)
						LengthRange = length;
					else
						parameter.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attribute.Name, parameter));
					break;
				case "count":
					var count = GetRange(attribute, parameter);
					if (count != null && count.IsValid() && count.Minimum is null or >= 0)
						CountRange = count;
					else
						parameter.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attribute.Name, parameter));
					break;
				case "value":
					var value = GetRange(attribute, parameter);
					if (value != null && value.IsValid())
						ValueRange = value;
					else
						parameter.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attribute.Name, parameter));
					break;
				case "regex":
					RegexPattern = parameter.Value;
					break;
				default:
					parameter.AddValidationError(ServiceDefinitionUtility.CreateUnexpectedAttributeParameterError(parameter.Name, parameter));
					break;
			}
		}
	}

	internal ServiceAttributeInfo Attribute { get; }

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

	private static ServiceFieldValidationRange? GetRange(ServiceAttributeInfo attribute, ServiceAttributeParameterInfo parameter)
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
			int? value = int.TryParse(first, NumberStyles.Number, CultureInfo.InvariantCulture, out var intValue) ? intValue : null;
			if (value == null)
			{
				parameter.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attribute.Name, parameter));
				return null;
			}

			return new ServiceFieldValidationRange(value, value);
		}

		if (string.IsNullOrEmpty(first) && !string.IsNullOrEmpty(second))
		{
			int? upperBound = int.TryParse(second, NumberStyles.Number, CultureInfo.InvariantCulture, out var intValue) ? intValue : null;
			if (upperBound == null)
			{
				parameter.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attribute.Name, parameter));
				return null;
			}

			return new ServiceFieldValidationRange(null, upperBound);
		}

		if (!string.IsNullOrEmpty(first) && string.IsNullOrEmpty(second))
		{
			int? lowerBound = int.TryParse(first, NumberStyles.Number, CultureInfo.InvariantCulture, out var intValue) ? intValue : null;
			if (lowerBound == null)
			{
				parameter.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attribute.Name, parameter));
				return null;
			}

			return new ServiceFieldValidationRange(lowerBound, null);
		}

		int? minimum = int.TryParse(first, NumberStyles.Number, CultureInfo.InvariantCulture, out var minValue) ? minValue : null;
		int? maximum = int.TryParse(second, NumberStyles.Number, CultureInfo.InvariantCulture, out var maxValue) ? maxValue : null;
		if (minimum == null || maximum == null)
		{
			parameter.AddValidationError(ServiceDefinitionUtility.CreateInvalidAttributeValueError(attribute.Name, parameter));
			return null;
		}

		return new ServiceFieldValidationRange(minimum, maximum);
	}
}
