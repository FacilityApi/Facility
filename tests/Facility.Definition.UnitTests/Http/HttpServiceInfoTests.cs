using Facility.Definition.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests.Http;

internal sealed class HttpServiceInfoTests : HttpInfoTestsBase
{
	[Test]
	public void EmptyServiceDefinition()
	{
		var info = ParseHttpApi("service TestApi{}");
		info.Service.Name.Should().Be("TestApi");
		info.Url.Should().BeNull();
		info.Servers.Should().BeEmpty();
		info.Methods.Should().BeEmpty();
		info.Events.Should().BeEmpty();
		info.ErrorSets.Should().BeEmpty();
	}

	[Test]
	public void OneMinimalMethod()
	{
		var info = ParseHttpApi("service TestApi { method do {}: {} }");
		info.Service.Name.Should().Be("TestApi");
		info.Url.Should().BeNull();
		info.Servers.Should().BeEmpty();
		info.Methods.Count.Should().Be(1);
		info.Events.Should().BeEmpty();
		info.ErrorSets.Should().BeEmpty();
	}

	[Test]
	public void OneMinimalEvent()
	{
		var info = ParseHttpApi("service TestApi { event do {}: {} }");
		info.Service.Name.Should().Be("TestApi");
		info.Url.Should().BeNull();
		info.Servers.Should().BeEmpty();
		info.Methods.Should().BeEmpty();
		info.Events.Count.Should().Be(1);
		info.ErrorSets.Should().BeEmpty();
	}

	[Test]
	public void EmptyServiceAttribute()
	{
		var info = ParseHttpApi("[http] service TestApi { method do {}: {} }");
		info.Url.Should().BeNull();
		info.Servers.Should().BeEmpty();
	}

	[Test]
	public void FullServiceAttribute()
	{
		var info = ParseHttpApi("[http(url: \"https://api.example.com\")] service TestApi { method do {}: {} }");
		info.Url.Should().Be("https://api.example.com");
		info.Servers.Count.Should().Be(1);
		info.Servers[0].Url.Should().Be("https://api.example.com");
		info.Servers[0].Description.Should().BeNull();
	}

	[Test]
	public void TwoServers()
	{
		var info = ParseHttpApi("[http(url: \"https://api.example.com\"), http(url: \"https://test.api.example.com\", description: \"test\")] service TestApi { method do {}: {} }");
		info.Url.Should().Be("https://api.example.com");
		info.Servers.Count.Should().Be(2);
		info.Servers[0].Url.Should().Be("https://api.example.com");
		info.Servers[0].Description.Should().BeNull();
		info.Servers[1].Url.Should().Be("https://test.api.example.com");
		info.Servers[1].Description.Should().Be("test");
	}

	[Test]
	public void UnexpectedHttpParameter()
	{
		ParseInvalidHttpApi("[http(xyzzy: true)] service TestApi { method do {}: {} }")
			.ToString().Should().Be("TestApi.fsd(1,7): Unexpected 'http' parameter 'xyzzy'.");
	}

	[Test]
	public void UnexpectedHttpAttribute()
	{
		ParseInvalidHttpApi("service TestApi { [http] data Hey {} }")
			.ToString().Should().Be("TestApi.fsd(1,20): Unexpected 'http' attribute.");
	}

	[Test]
	public void TwoMethodsSameRoute()
	{
		ParseInvalidHttpApi("service TestApi { method do {}: {} [http(path: \"/do\")] method doNot {}: {} }")
			.ToString().Should().Be("TestApi.fsd(1,56): Methods 'do' and 'doNot' have the same route: POST /do");
	}

	[Test]
	public void MultipleErrors()
	{
		HttpServiceInfo.TryCreate(TestUtility.ParseTestApi("[http(xyzzy: true)] service TestApi { [http] data Hey {} }"), out _, out var errors);
		errors.Count.Should().Be(2);
	}
}
