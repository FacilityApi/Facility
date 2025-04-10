using Facility.Definition.Fsd;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests;

internal sealed class ServiceTests
{
	[TestCase("service TestApi")]
	[TestCase("service TestApi;")]
	[TestCase("service TestApi ; ")]
	[TestCase("service TestApi{}")]
	[TestCase(" \r\n\t service \r\n\t TestApi \r\n\t { \r\n\t } \r\n\t ")]
	public void EmptyServiceDefinition(string definition)
	{
		var service = TestUtility.ParseTestApi(definition);

		service.Name.Should().Be("TestApi");
		service.Attributes.Count.Should().Be(0);
		service.ErrorSets.Count.Should().Be(0);
		service.Enums.Count.Should().Be(0);
		service.Dtos.Count.Should().Be(0);
		service.AllMethods.Count.Should().Be(0);
		service.Summary.Should().Be("");
		service.Remarks.Count.Should().Be(0);

		TestUtility.GenerateFsd(service).Should().Equal(
			"// DO NOT EDIT: generated by TestUtility",
			"",
			"service TestApi",
			"{",
			"}",
			"");
	}

	[Test]
	public void GenerateEmptyFileScopedService()
	{
		var service = TestUtility.ParseTestApi("service TestApi {}");

		service.Name.Should().Be("TestApi");
		service.Attributes.Count.Should().Be(0);
		service.ErrorSets.Count.Should().Be(0);
		service.Enums.Count.Should().Be(0);
		service.Dtos.Count.Should().Be(0);
		service.AllMethods.Count.Should().Be(0);
		service.Summary.Should().Be("");
		service.Remarks.Count.Should().Be(0);

		TestUtility.GenerateFsd(service, new FsdGeneratorSettings { FileScopedService = true }).Should().Equal(
			"// DO NOT EDIT: generated by TestUtility",
			"",
			"service TestApi;",
			"");
	}

	[Test]
	public void BlankServiceDefinition()
	{
		TestUtility.ParseInvalidTestApi("").Message.Should().Be("TestApi.fsd(1,1): expected '[' or 'service'");
	}

	[Test]
	public void WhitespaceServiceDefinition()
	{
		TestUtility.ParseInvalidTestApi(" \r\n\t ").Message.Should().Be("TestApi.fsd(2,3): expected '[' or 'service'");
	}

	[Test]
	public void MissingServiceName()
	{
		TestUtility.ParseInvalidTestApi("service{}").Message.Should().Be("TestApi.fsd(1,8): expected service name");
	}

	[Test]
	public void MissingEndBrace()
	{
		TestUtility.ParseInvalidTestApi("service TestApi {").Message.Should().Be("TestApi.fsd(1,18): expected '}' or '[' or 'data' or 'enum' or 'errors' or 'extern' or 'method' or 'event'");
	}

	[Test]
	public void MissingExternType()
	{
		TestUtility.ParseInvalidTestApi("service TestApi { extern xyz; }").Message.Should().Be("TestApi.fsd(1,26): expected 'data' or 'enum'");
	}

	[Test]
	public void DuplicatedService()
	{
		TestUtility.ParseInvalidTestApi("service TestApi{} service TestApi{}").Message.Should().Be("TestApi.fsd(1,19): expected end");
	}

	[Test]
	public void DuplicateMethod()
	{
		TestUtility.ParseInvalidTestApi("service TestApi { method xyzzy {}: {} method xyzzy {}: {} }")
			.Message.Should().Be("TestApi.fsd(1,39): Duplicate service member: xyzzy");
	}

	[Test]
	public void DuplicateDto()
	{
		TestUtility.ParseInvalidTestApi("service TestApi { data xyzzy {} data xyzzy {} }")
			.Message.Should().Be("TestApi.fsd(1,33): Duplicate service member: xyzzy");
	}

	[Test]
	public void DuplicateEnum()
	{
		TestUtility.ParseInvalidTestApi("service TestApi { enum xyzzy { x } enum xyzzy { x } }")
			.Message.Should().Be("TestApi.fsd(1,36): Duplicate service member: xyzzy");
	}

	[Test]
	public void DuplicateExternalDto()
	{
		TestUtility.ParseInvalidTestApi("service TestApi { extern data xyzzy; extern data xyzzy; }")
			.Message.Should().Be("TestApi.fsd(1,45): Duplicate service member: xyzzy");
	}

	[Test]
	public void DuplicateExternalEnum()
	{
		TestUtility.ParseInvalidTestApi("service TestApi { extern enum xyzzy; extern enum xyzzy; }")
			.Message.Should().Be("TestApi.fsd(1,45): Duplicate service member: xyzzy");
	}

	[Test]
	public void DuplicateErrorSet()
	{
		TestUtility.ParseInvalidTestApi("service TestApi { errors xyzzy { x } errors xyzzy { x } }")
			.Message.Should().Be("TestApi.fsd(1,38): Duplicate service member: xyzzy");
	}

	[Test]
	public void DuplicateMember()
	{
		TestUtility.ParseInvalidTestApi("service TestApi { method xyzzy {}: {} data xyzzy {} }")
			.Message.Should().Be("TestApi.fsd(1,39): Duplicate service member: xyzzy");
	}

	[Test]
	public void ServiceSummary()
	{
		var service = TestUtility.ParseTestApi("/// test\n/// summary\nservice TestApi{}");

		service.Summary.Should().Be("test summary");

		TestUtility.GenerateFsd(service).Should().Equal(
			"// DO NOT EDIT: generated by TestUtility",
			"",
			"/// test summary",
			"service TestApi",
			"{",
			"}",
			"");
	}

	[Test]
	public void ServiceRemarks()
	{
		var service = TestUtility.ParseTestApi("service TestApi{}\n# TestApi\ntest\nremarks");

		service.Remarks[1].Should().Be("remarks");

		TestUtility.GenerateFsd(service).Should().Equal(
			"// DO NOT EDIT: generated by TestUtility",
			"",
			"service TestApi",
			"{",
			"}",
			"",
			"# TestApi",
			"",
			"test",
			"remarks",
			"");
	}

	[Test]
	public void MethodRemarks()
	{
		var service = TestUtility.ParseTestApi("service TestApi { method do {}: {} }\n# do\nremarks");

		service.Methods.Single().Remarks.Single().Should().Be("remarks");

		TestUtility.GenerateFsd(service).Should().Equal(
			"// DO NOT EDIT: generated by TestUtility",
			"",
			"service TestApi",
			"{",
			"\tmethod do",
			"\t{",
			"\t}:",
			"\t{",
			"\t}",
			"}",
			"",
			"# do",
			"",
			"remarks",
			"");
	}

	[Test]
	public void DuplicatedRemarks()
	{
		TestUtility.ParseInvalidTestApi("service TestApi { method do {}: {} }\n# do\nremarks\n# do\nremarks")
			.Message.Should().Be("TestApi.fsd(4,1): Duplicate remarks heading: do");
	}

	[Test]
	public void UnmatchedRemarks()
	{
		TestUtility.ParseInvalidTestApi("service TestApi{}\n# TestApi2\ntest\nremarks")
			.Message.Should().Be("TestApi.fsd(2,1): Unused remarks heading: TestApi2");
	}

	[Test]
	public void MultipleErrors()
	{
		TestUtility.TryParseInvalidTestApi("service TestApi { method do {}: {} data MyDto { x: InvalidData; } data One {} data One {} }\n# do\nremarks\n# do\nremarks\n# TestApi2\ntest\nremarks")
			.Count.Should().Be(4);
	}

	[Test]
	public void ServiceParts()
	{
		var service = TestUtility.ParseTestApi("service TestApi\r\n{\r\n}\r\n");

		var keywordPart = service.GetPart(ServicePartKind.Keyword)!;
		keywordPart.Position.LineNumber.Should().Be(1);
		keywordPart.Position.ColumnNumber.Should().Be(1);
		keywordPart.EndPosition.LineNumber.Should().Be(1);
		keywordPart.EndPosition.ColumnNumber.Should().Be(8);

		var namePart = service.GetPart(ServicePartKind.Name)!;
		namePart.Position.LineNumber.Should().Be(1);
		namePart.Position.ColumnNumber.Should().Be(9);
		namePart.EndPosition.LineNumber.Should().Be(1);
		namePart.EndPosition.ColumnNumber.Should().Be(16);

		var endPart = service.GetPart(ServicePartKind.End)!;
		endPart.Position.LineNumber.Should().Be(3);
		endPart.Position.ColumnNumber.Should().Be(1);
		endPart.EndPosition.LineNumber.Should().Be(3);
		endPart.EndPosition.ColumnNumber.Should().Be(2);
	}
}
