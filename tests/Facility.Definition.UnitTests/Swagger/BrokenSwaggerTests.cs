using Facility.Definition.Swagger;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests.Swagger
{
	[TestFixture]
	public class BrokenSwaggerTests
	{
		[TestCase("", "(1,1): Service definition is missing.")]
		[TestCase("{", "(1,1): Unexpected end when reading JSON.")]
		[TestCase(" {", "(1,2): Unexpected end when reading JSON.")]
		[TestCase("# empty", "(1,1): Service definition is missing.")]
		[TestCase("invalid", "(1,8): Invalid cast from")]
		[TestCase(" \n {}", "(2,2): swagger field is missing.")]
		[TestCase("info: {}", "(1,1): swagger field is missing.")]
		[TestCase("{swagger:{}}", "(1,10): Unexpected character")]
		[TestCase("swagger: {}", "(1,10): Failed to create an instance of type")]
		[TestCase("{swagger:'1.0'}", "(1,14): swagger should be '2.0'.")]
		[TestCase("swagger: '1.0'", "(1,15): swagger should be '2.0'.")]
		[TestCase("{swagger:'2.0'}", "(1,1): info is missing.")]
		[TestCase("swagger: '2.0'", "(1,1): info is missing.")]
		[TestCase("{swagger:'2.0',info:{}}", "(1,21): info/title is missing.")]
		[TestCase("swagger: '2.0'\ninfo: {}", "(2,7): info/title is missing.")]
		[TestCase("{swagger:'2.0',info:{title:' '}}", "(1,30): info/title is not a valid service name.")]
		[TestCase("swagger: '2.0'\ninfo:\n  title: ' '\n", "(3,13): info/title is not a valid service name.")]
		[TestCase("{swagger:'2.0',info:{title:' ','x-identifier':' '}}", "(1,49): info/x-identifier is not a valid service name.")]
		[TestCase("swagger: '2.0'\ninfo:\n  title: ' '\n  x-identifier: ' '\n", "(4,20): info/x-identifier is not a valid service name.")]
		public void SwaggerParseFailure(string text, string messagePrefix)
		{
			try
			{
				new SwaggerParser().ParseDefinition(new ServiceDefinitionText("", text));
				Assert.Fail("Parse didn't fail.");
			}
			catch (ServiceDefinitionException exception)
			{
				exception.Message.Should().StartWith(messagePrefix);
			}
		}
	}
}
