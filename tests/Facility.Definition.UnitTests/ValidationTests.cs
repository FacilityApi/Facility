using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests;

public sealed class ValidationTests
{
	[Test]
	public void DuplicateValidation()
	{
		var errors = TestUtility.TryParseInvalidTestApi("""
			service TestApi {
				enum One
				{
					X
				}

				method do
				{
					[validate]
					[validate]
					one: One;
					}: {}
			}
			""");
		errors.Single().Message.Should().Be("'validate' attribute is duplicated.");
	}

	[Test]
	public void StringValidateInvalidParameter()
	{
		var exception = TestUtility.TryParseInvalidTestApi("""
			service TestApi {
			  method do
			  {
			    [validate(value: "^\\d+")]
			    one: string;
			  }: {}
			}
			""");
		exception[0].Message.Should().Be("Missing 'validate' parameters: [length, regex].");
		exception[1].Message.Should().Be("'value' value '^\\d+' for 'validate' attribute is invalid.");
	}

	[Test]
	public void InvalidStringValidateLengthArgument()
	{
		var exception = TestUtility.ParseInvalidTestApi("""
			service TestApi {
			  method do
			  {
			    [validate(length: "^\\d+")]
			    one: string;
			  }: {}
			}
			""");
		exception.Message.Should().Be("TestApi.fsd(5,6): Missing 'validate' parameters: [length, regex].");
	}

	[Test]
	public void InvalidMinimumStringLengthValidateParameter()
	{
		var errors = TestUtility.TryParseInvalidTestApi("""
			service TestApi {
			  method do
			  {
			    [validate(length: -1..10)]
			    one: string;
			  }: {}
			}
			""");
		errors[0].Message.Should().Be("Missing 'validate' parameters: [length, regex].");
		errors[1].Message.Should().Be("'length' value '-1..10' for 'validate' attribute is invalid.");
	}

	[Test]
	public void InvalidMaximumStringLengthValidateParameter()
	{
		var errors = TestUtility.TryParseInvalidTestApi("""
			service TestApi {
			  method do
			  {
			    [validate(length: 100..10)]
			    one: string;
			  }: {}
			}
			""");
		errors[0].Message.Should().Be("Missing 'validate' parameters: [length, regex].");
		errors[1].Message.Should().Be("'length' value '100..10' for 'validate' attribute is invalid.");
	}

	[Test]
	public void CollectionValidateParameter()
	{
		var service = TestUtility.ParseTestApi("""
			service TestApi {
			  method do
			  {
			    [validate(count: 1..10)]
			    one: double[];
			  }: {}
			}
			""");
		var range = service.Methods.Single().RequestFields.Single().Validation!.CountRange!;
		range.Minimum.Should().Be(1);
		range.Maximum.Should().Be(10);
	}

	[Test]
	public void InvalidMinimumCollectionValidateParameter()
	{
		var errors = TestUtility.TryParseInvalidTestApi("""
			service TestApi {
			  method do
			  {
			    [validate(count: -1..10)]
			    one: double[];
			  }: {}
			}
			""");
		errors[0].Message.Should().Be("Missing 'validate' parameters: [count].");
		errors[1].Message.Should().Be("'count' value '-1..10' for 'validate' attribute is invalid.");
	}

	[Test]
	public void InvalidMaximumCollectionValidateParameter()
	{
		var errors = TestUtility.TryParseInvalidTestApi("""
			service TestApi {
			  method do
			  {
			    [validate(count: 100..10)]
			    one: double[];
			  }: {}
			}
			""");
		errors[0].Message.Should().Be("Missing 'validate' parameters: [count].");
		errors[1].Message.Should().Be("'count' value '100..10' for 'validate' attribute is invalid.");
	}

	[Test]
	public void NumericValidateParameter()
	{
		var service = TestUtility.ParseTestApi("""
			service TestApi {
			  method do
			  {
			    [validate(value: 0..1)]
			    one: double;
			  }: {}
			}
			""");
		var range = service.Methods.Single().RequestFields.Single().Validation!.ValueRange!;
		range.Minimum.Should().Be(0);
		range.Maximum.Should().Be(1);
	}

	[Test]
	public void NumericValidateUnboundedMinimumParameter()
	{
		var service = TestUtility.ParseTestApi("""
			service TestApi {
			  method do
			  {
			    [validate(value: ..0)]
			    one: double;
			  }: {}
			}
			""");
		var range = service.Methods.Single().RequestFields.Single().Validation!.ValueRange!;
		range.Minimum.Should().BeNull();
		range.Maximum.Should().Be(0);
	}

	[Test]
	public void NumericValidateUnboundedEndParameter()
	{
		var service = TestUtility.ParseTestApi("""
			service TestApi {
			  method do
			  {
			    [validate(value: 0..)]
			    one: double;
			  }: {}
			}
			""");
		var range = service.Methods.Single().RequestFields.Single().Validation!.ValueRange!;
		range.Minimum.Should().Be(0);
		range.Maximum.Should().BeNull();
	}

	[Test]
	public void NumericValidateSingleValueParameter()
	{
		var service = TestUtility.ParseTestApi("""
			service TestApi {
			  method do
			  {
			    [validate(value: 1)]
			    one: double;
			  }: {}
			}
			""");
		var range = service.Methods.Single().RequestFields.Single().Validation!.ValueRange!;
		range.Minimum.Should().Be(1);
		range.Maximum.Should().Be(1);
	}

	[Test]
	public void InvalidNumericValidateParameterStringValue()
	{
		var exception = TestUtility.ParseInvalidTestApi("""
			service TestApi {
			  method do
			  {
			    [validate(regex: "\\d+.{2}")]
			    one: double;
			  }: {}
			}
			""");
		exception.Message.Should().Be("TestApi.fsd(5,6): 'validate' parameter 'regex' is invalid for Double.");
	}

	[Test]
	public void InvalidNumericValidateParameterReversedValue()
	{
		var errors = TestUtility.TryParseInvalidTestApi("""
			service TestApi {
			  method do
			  {
			    [validate(value: 100..10)]
			    one: double;
			  }: {}
			}
			""");
		errors[0].Message.Should().Be("Missing 'validate' parameters: [value].");
		errors[1].Message.Should().Be("'value' value '100..10' for 'validate' attribute is invalid.");
	}

	[Test]
	public void PartiallyInvalidNumericValidateParameter()
	{
		var exception = TestUtility.ParseInvalidTestApi("""
			service TestApi {
			  method do
			  {
			    [validate(value: 0..infinity)]
			    one: double;
			  }: {}
			}
			""");
		exception.Message.Should().Be("TestApi.fsd(5,6): Missing 'validate' parameters: [value].");
	}

	[Test]
	public void InvalidCollectionValidateParameter()
	{
		var exception = TestUtility.ParseInvalidTestApi("""
			service TestApi {
			  enum One
			  {
			    X
			  }
			
			  method do
			  {
			    [validate(regex: "\\d+.{2}")]
			    one: One[];
			  }: {}
			}
			""");
		exception.Message.Should().Be("TestApi.fsd(10,6): 'validate' parameter 'regex' is invalid for Array.");
	}

	[Test]
	public void InvalidCollectionValidateValueNegativeMinimum()
	{
		var errors = TestUtility.TryParseInvalidTestApi("""
			service TestApi {
			  enum One
			  {
			    X
			  }
			
			  method do
			  {
			    [validate(count: -1..10)]
			    one: One[];
			  }: {}
			}
			""");
		errors[0].Message.Should().Be("Missing 'validate' parameters: [count].");
		errors[1].Message.Should().Be("'count' value '-1..10' for 'validate' attribute is invalid.");
	}

	[Test]
	public void CollectionValidateValueUnboundMinimum()
	{
		var service = TestUtility.ParseTestApi("""
			service TestApi {
			  enum One
			  {
			    X
			  }

			  method do
			  {
			    [validate(count: ..10)]
			    one: One[];
			  }: {}
			}
			""");
		var range = service.Methods.Single().RequestFields.Single().Validation!.CountRange!;
		range.Minimum.Should().BeNull();
		range.Maximum.Should().Be(10);
	}

	[Test]
	public void CollectionValidateValueUnboundMaximum()
	{
		var service = TestUtility.ParseTestApi("""
			service TestApi {
			  enum One
			  {
			    X
			  }

			  method do
			  {
			    [validate(count: 0..)]
			    one: One[];
			  }: {}
			}
			""");
		var range = service.Methods.Single().RequestFields.Single().Validation!.CountRange!;
		range.Minimum.Should().Be(0);
		range.Maximum.Should().BeNull();
	}

	[Test]
	public void CollectionValidateSingleValue()
	{
		var service = TestUtility.ParseTestApi("""
			service TestApi {
			  enum One
			  {
			    X
			  }

			  method do
			  {
			    [validate(count: 10)]
			    one: One[];
			  }: {}
			}
			""");
		var range = service.Methods.Single().RequestFields.Single().Validation!.CountRange!;
		range.Minimum.Should().Be(10);
		range.Maximum.Should().Be(10);
	}
}
