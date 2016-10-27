using System.Linq;
using System.Net;
using NUnit.Framework;
using Shouldly;

namespace Facility.Definition.UnitTests.Http
{
	public class HttpErrorSetInfoTests : HttpInfoTestsBase
	{
		[Test]
		public void OneMinimalErrorSet()
		{
			var error = ParseHttpApi("service TestApi { errors bad { boom } }").ErrorSets.Single().Errors.Single();
			error.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
		}

		[Test]
		public void ErrorWithStatusCode()
		{
			var error = ParseHttpApi("service TestApi { errors bad { [http(code: 426)] boom } }").ErrorSets.Single().Errors.Single();
			error.StatusCode.ShouldBe(HttpStatusCode.UpgradeRequired);
		}

		[Test]
		public void ErrorStatusCodeOutOfRange()
		{
			ParseInvalidHttpApi("service TestApi { errors bad { [http(code: 999)] boom } }")
				.Message.ShouldBe("TestApi.fsd(1,38): 'code' parameter must be an integer between 200 and 599.");
		}

		[Test]
		public void ErrorInvalidParameter()
		{
			ParseInvalidHttpApi("service TestApi { errors bad { [http(cod: 400)] boom } }")
				.Message.ShouldBe("TestApi.fsd(1,38): Unexpected 'http' parameter 'cod'.");
		}

		[Test]
		public void OptionalHttpAttribute()
		{
			var error = ParseHttpApi("service TestApi { [http] errors bad { boom } }").ErrorSets.Single().Errors.Single();
			error.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
		}

		[Test]
		public void HttpAttributeParameter()
		{
			ParseInvalidHttpApi("service TestApi { [http(name: error)] errors bad { boom } }")
				.Message.ShouldBe("TestApi.fsd(1,25): Unexpected 'http' parameter 'name'.");
		}
	}
}
