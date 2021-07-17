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
    [validate(value: ""^\\d+"")]
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
    [validate(length: ""^\\d+"")]
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
			range.StartInclusive.Should().Be(new decimal(0));
			range.EndInclusive.Should().Be(new decimal(1));
		}

		[Test]
		public void InvalidNumericValidateParameter()
		{
			var exception = TestUtility.ParseInvalidTestApi(@"
service TestApi {
  method do
  {
    [validate(regex: ""\\d+.{2}"")]
    one: double;
  }: {}
}");
			exception.Message.Should().Be(@"TestApi.fsd(5,15): Unexpected 'validate' parameter 'regex'.");
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
    [validate(regex: ""\\d+.{2}"")]
    one: One[];
  }: {}
}");
			exception.Message.Should().Be(@"TestApi.fsd(10,15): Unexpected 'validate' parameter 'regex'.");
		}
	}
}
