using Facility.Definition.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests.Http
{
	public class HttpServiceInfoTests : HttpInfoTestsBase
	{
		[Test]
		public void EmptyServiceDefinition()
		{
			var info = ParseHttpApi("service TestApi{}");
			info.Service.Name.Should().Be("TestApi");
			info.Url.Should().BeNull();
			info.Methods.Count.Should().Be(0);
			info.ErrorSets.Count.Should().Be(0);
		}

		[Test]
		public void OneMinimalMethod()
		{
			var info = ParseHttpApi("service TestApi { method do {}: {} }");
			info.Service.Name.Should().Be("TestApi");
			info.Url.Should().BeNull();
			info.Methods.Count.Should().Be(1);
			info.ErrorSets.Count.Should().Be(0);
		}

		[Test]
		public void EmptyServiceAttribute()
		{
			var info = ParseHttpApi("[http] service TestApi { method do {}: {} }");
			info.Url.Should().BeNull();
		}

		[Test]
		public void FullServiceAttribute()
		{
			var info = ParseHttpApi("[http(url: \"https://api.example.com\")] service TestApi { method do {}: {} }");
			info.Url.Should().Be("https://api.example.com");
		}

		[Test]
		public void TwoServiceAttributes()
		{
			ParseInvalidHttpApi("[http] [http] service TestApi { method do {}: {} }")
				.ToString().Should().Be("TestApi.fsd(1,9): 'http' attribute is duplicated.");
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
}
