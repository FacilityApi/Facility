using Facility.Definition.Fsd;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests;

public sealed class MethodTests
{
	[Test]
	public void InvalidName()
	{
		new ServiceMethodInfo(name: "4u").IsValid.Should().BeFalse();
	}

	[TestCase(true), TestCase(false)]
	public void DuplicateField(bool isRequest)
	{
		var fields = new[]
		{
			new ServiceFieldInfo("why", "int32"),
			new ServiceFieldInfo("Why", "int32"),
		};
		new ServiceMethodInfo(name: "x", requestFields: isRequest ? fields : null, responseFields: isRequest ? null : fields).IsValid.Should().BeFalse();
	}

	[TestCase(ServiceMethodKind.Normal)]
	[TestCase(ServiceMethodKind.Event)]
	public void OneMinimalMethod(ServiceMethodKind kind)
	{
		var keyword = kind.GetKeyword();
		var service = TestUtility.ParseTestApi($$"""service TestApi { {{keyword}} do {}: {} }""");

		var method = service.AllMethods.Single();
		method.Kind.Should().Be(kind);
		method.Name.Should().Be("do");
		method.Attributes.Count.Should().Be(0);
		method.Summary.Should().Be("");
		method.Remarks.Count.Should().Be(0);
		method.RequestFields.Count.Should().Be(0);
		method.ResponseFields.Count.Should().Be(0);

		TestUtility.GenerateFsd(service).Should().Equal(
			"// DO NOT EDIT: generated by TestUtility",
			"",
			"service TestApi",
			"{",
			$"\t{keyword} do",
			"\t{",
			"\t}:",
			"\t{",
			"\t}",
			"}",
			"");
	}

	[Test]
	public void OneMinimalMethodFileScopedService()
	{
		var service = TestUtility.ParseTestApi("service TestApi; method do {}: {}");

		var method = service.Methods.Single();
		method.Name.Should().Be("do");
		method.Attributes.Count.Should().Be(0);
		method.Summary.Should().Be("");
		method.Remarks.Count.Should().Be(0);
		method.RequestFields.Count.Should().Be(0);
		method.ResponseFields.Count.Should().Be(0);

		TestUtility.GenerateFsd(service, new FsdGeneratorSettings { FileScopedService = true }).Should().Equal(
			"// DO NOT EDIT: generated by TestUtility",
			"",
			"service TestApi;",
			"",
			"method do",
			"{",
			"}:",
			"{",
			"}",
			"");
	}
}
