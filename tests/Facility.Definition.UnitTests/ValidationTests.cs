using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public sealed class ValidationTests
	{
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
			var exception = TestUtility.ParseInvalidTestApi(@"
service TestApi {
  method do
  {
    [validate(length: /^\d+/)]
    one: string;
  }: {}
}");
			exception.Message.Should().Be(@"TestApi.fsd(5,15): 'length' value '^\d+' for 'validate' attribute is invalid.");
		}

		[Test]
		public void InvalidStringValidatePatternArgument()
		{
			var exception = TestUtility.ParseInvalidTestApi(@"
service TestApi {
  method do
  {
    [validate(regex: 0..1)]
    one: string;
  }: {}
}");
			exception.Message.Should().Be(@"TestApi.fsd(5,15): 'regex' value '0..1' for 'validate' attribute is invalid.");
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
			range.Minimum.Should().Be(decimal.Zero);
			range.Maximum.Should().Be(decimal.One);
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
			range.Minimum.Should().BeNull();
			range.Maximum.Should().Be(decimal.Zero);
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
			range.Minimum.Should().Be(decimal.Zero);
			range.Maximum.Should().BeNull();
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
			range.Minimum.Should().Be(decimal.One);
			range.Maximum.Should().Be(decimal.One);
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
			var exception = TestUtility.ParseInvalidTestApi(@"
service TestApi {
  method do
  {
    [validate(value: 0..infinity)]
    one: double;
  }: {}
}");
			exception.Message.Should().Be(@"TestApi.fsd(5,15): 'value' value '0..infinity' for 'validate' attribute is invalid.");
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
