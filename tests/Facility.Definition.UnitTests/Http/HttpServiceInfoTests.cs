using NUnit.Framework;
using Shouldly;

namespace Facility.Definition.UnitTests.Http
{
	public class HttpServiceInfoTests : HttpInfoTestsBase
	{
		[Test]
		public void EmptyServiceDefinition()
		{
			var info = ParseHttpApi("service TestApi{}");
			info.Service.Name.ShouldBe("TestApi");
			info.Url.ShouldBe(null);
			info.Methods.Count.ShouldBe(0);
			info.ErrorSets.Count.ShouldBe(0);
		}

		[Test]
		public void OneMinimalMethod()
		{
			var info = ParseHttpApi("service TestApi { method do {}: {} }");
			info.Service.Name.ShouldBe("TestApi");
			info.Url.ShouldBe(null);
			info.Methods.Count.ShouldBe(1);
			info.ErrorSets.Count.ShouldBe(0);
		}

		[Test]
		public void EmptyServiceAttribute()
		{
			var info = ParseHttpApi("[http] service TestApi { method do {}: {} }");
			info.Url.ShouldBe(null);
		}

		[Test]
		public void FullServiceAttribute()
		{
			var info = ParseHttpApi("[http(url: \"https://api.example.com\")] service TestApi { method do {}: {} }");
			info.Url.ShouldBe("https://api.example.com");
		}

		[Test]
		public void TwoServiceAttributes()
		{
			ParseInvalidHttpApi("[http] [http] service TestApi { method do {}: {} }")
				.Message.ShouldBe("TestApi.fsd(1,9): 'http' attribute is duplicated.");
		}

		[Test]
		public void UnexpectedHttpParameter()
		{
			ParseInvalidHttpApi("[http(xyzzy: true)] service TestApi { method do {}: {} }")
				.Message.ShouldBe("TestApi.fsd(1,7): Unexpected 'http' parameter 'xyzzy'.");
		}

		[Test]
		public void UnexpectedHttpAttribute()
		{
			ParseInvalidHttpApi("service TestApi { [http] data Hey {} }")
				.Message.ShouldBe("TestApi.fsd(1,20): 'http' attribute not supported on this element.");
		}

		[Test]
		public void TwoMethodsSameRoute()
		{
			ParseInvalidHttpApi("service TestApi { method do {}: {} [http(path: \"/do\")] method doNot {}: {} }")
				.Message.ShouldBe("TestApi.fsd(1,56): Methods 'do' and 'doNot' have the same route: POST /do");
		}

		[Test]
		public void MultipleErrors()
		{
			TryParseInvalidHttpApi("[http(xyzzy: true)] service TestApi { [http] data Hey {} }")
				.Count.ShouldBe(2);
		}
	}
}
