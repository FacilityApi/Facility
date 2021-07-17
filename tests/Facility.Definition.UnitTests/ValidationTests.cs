using System;

namespace Facility.Definition.UnitTests
{
	using System.Linq;
	using FluentAssertions;
	using NUnit.Framework;

	public sealed class ValidationTests
	{
		[Test]
		public void DefinedEnumValidation()
		{
			var service = TestUtility.ParseTestApi(@"
service TestApi {
	enum One
	{
		X
	}

	method do
	{
		[validate]
		one: One;
		}: {}
}");

			service.Methods.Single().RequestFields.Single().Validation!.IsDefinedEnum.Should().Be(true);
		}

		[Test]
		public void DuplicateValidation()
		{
			var errors = TestUtility.TryParseInvalidTestApi(@"
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
}");
			errors.Single().Message.Should().Be("'validate' attribute is duplicated.");
		}

		[Test]
		public void StringValidateInvalidParameter()
		{
			var exception = TestUtility.TryParseInvalidTestApi(@"
service TestApi {
  method do
  {
    [validate(value: /^\d+/)]
    one: string;
  }: {}
}");
			exception[0].Message.Should().Be("Unexpected 'validate' parameter 'value'.");
			exception[1].Message.Should().Be("Missing 'validate' parameters: [length, regex].");
		}

		[Test]
		public void InvalidStringValidateLengthArgument()
		{
			var exception = TestUtility.TryParseInvalidTestApi(@"
service TestApi {
  method do
  {
    [validate(length: /^\d+/)]
    one: string;
  }: {}
}");
			exception[0].Message.Should().Be(@"Missing 'validate' parameters: [length, regex].");
			exception[1].Message.Should().Be(@"'length' value '^\d+' for 'validate' attribute is invalid.");
		}

		[Test]
		public void InvalidStringValidatePatternArgument()
		{
			var exception = TestUtility.TryParseInvalidTestApi(@"
service TestApi {
  method do
  {
    [validate(regex: 0..1)]
    one: string;
  }: {}
}");
			exception[0].Message.Should().Be(@"Missing 'validate' parameters: [length, regex].");
			exception[1].Message.Should().Be(@"'regex' value '0..1' for 'validate' attribute is invalid.");
		}

		[Test]
		public void NumericValidateParameter()
		{
			var service = TestUtility.ParseTestApi(@"
service TestApi {
  method do
  {
    [validate(value: 0..1)]
    one: double;
  }: {}
}");
			var range = service.Methods.Single().RequestFields.Single().Validation!.ValueRange!;
			range.StartInclusive.Should().Be(decimal.Zero);
			range.EndInclusive.Should().Be(decimal.One);
		}

		[Test]
		public void NumericValidateUnboundedStartParameter()
		{
			var service = TestUtility.ParseTestApi(@"
service TestApi {
  method do
  {
    [validate(value: ..0)]
    one: double;
  }: {}
}");
			var range = service.Methods.Single().RequestFields.Single().Validation!.ValueRange!;
			range.StartInclusive.Should().BeNull();
			range.EndInclusive.Should().Be(decimal.Zero);
		}

		[Test]
		public void NumericValidateUnboundedEndParameter()
		{
			var service = TestUtility.ParseTestApi(@"
service TestApi {
  method do
  {
    [validate(value: 0..)]
    one: double;
  }: {}
}");
			var range = service.Methods.Single().RequestFields.Single().Validation!.ValueRange!;
			range.StartInclusive.Should().Be(decimal.Zero);
			range.EndInclusive.Should().BeNull();
		}

		[Test]
		public void NumericValidateSingleValueParameter()
		{
			var service = TestUtility.ParseTestApi(@"
service TestApi {
  method do
  {
    [validate(value: 1)]
    one: double;
  }: {}
}");
			var range = service.Methods.Single().RequestFields.Single().Validation!.ValueRange!;
			range.StartInclusive.Should().Be(decimal.One);
			range.EndInclusive.Should().Be(decimal.One);
		}

		[Test]
		public void InvalidNumericValidateParameter()
		{
			var exception = TestUtility.ParseInvalidTestApi(@"
service TestApi {
  method do
  {
    [validate(regex: /\d+.{2}/)]
    one: double;
  }: {}
}");
			exception.Message.Should().Be(@"TestApi.fsd(5,15): Unexpected 'validate' parameter 'regex'.");
		}

		[Test]
		public void PartiallyInvalidNumericValidateParameter()
		{
			var exception = TestUtility.TryParseInvalidTestApi(@"
service TestApi {
  method do
  {
    [validate(value: 0..infinity)]
    one: double;
  }: {}
}");
			exception[0].Message.Should().Be(@"Missing 'validate' parameters: [value].");
			exception[1].Message.Should().Be(@"'value' value '0..infinity' for 'validate' attribute is invalid.");
		}

		[Test]
		public void InvalidCollectionValidateParameter()
		{
			var exception = TestUtility.ParseInvalidTestApi(@"
service TestApi {
  enum One
  {
    X
  }

  method do
  {
    [validate(regex: /\d+.{2}/)]
    one: One[];
  }: {}
}");
			exception.Message.Should().Be(@"TestApi.fsd(10,15): Unexpected 'validate' parameter 'regex'.");
		}
	}
}
