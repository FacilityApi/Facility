using System.Linq;
using System.Net;
using System.Net.Http;
using Facility.Definition.Fsd;
using Facility.Definition.Http;
using NUnit.Framework;
using Shouldly;

namespace Facility.Definition.UnitTests.Http
{
	public class HttpMethodInfoTests : HttpInfoTestsBase
	{
		[Test]
		public void OneMinimalMethod()
		{
			var method = ParseHttpApi("service TestApi { method do {}: {} }").Methods.Single();
			method.ServiceMethod.Name.ShouldBe("do");
			method.Method.ShouldBe(HttpMethod.Post);
			method.Path.ShouldBe("/do");
			method.PathFields.Count.ShouldBe(0);
			method.QueryFields.Count.ShouldBe(0);
			method.RequestNormalFields.Count.ShouldBe(0);
			method.RequestBodyField.ShouldBe(null);
			method.RequestHeaderFields.Count.ShouldBe(0);
			method.ResponseHeaderFields.Count.ShouldBe(0);

			var response = method.ValidResponses.Single();
			response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
			response.NormalFields.Count.ShouldBe(0);
			response.BodyField.ShouldBe(null);
		}

		[Test]
		public void HttpGetMethod()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get)] method do {}: {} }").Methods.Single();
			method.Method.ShouldBe(HttpMethod.Get);
		}

		[Test]
		public void HttpPostMethod()
		{
			var method = ParseHttpApi("service TestApi { [http(method: post)] method do {}: {} }").Methods.Single();
			method.Method.ShouldBe(HttpMethod.Post);
		}

		[Test]
		public void HttpPutMethod()
		{
			var method = ParseHttpApi("service TestApi { [http(method: put)] method do {}: {} }").Methods.Single();
			method.Method.ShouldBe(HttpMethod.Put);
		}

		[Test]
		public void HttpDeleteMethod()
		{
			var method = ParseHttpApi("service TestApi { [http(method: delete)] method do {}: {} }").Methods.Single();
			method.Method.ShouldBe(HttpMethod.Delete);
		}

		[Test]
		public void HttpPatchMethod()
		{
			var method = ParseHttpApi("service TestApi { [http(method: patch)] method do {}: {} }").Methods.Single();
			method.Method.ToString().ShouldBe("PATCH");
		}

		[Test]
		public void HttpOptionsMethod()
		{
			var method = ParseHttpApi("service TestApi { [http(method: options)] method do {}: {} }").Methods.Single();
			method.Method.ToString().ShouldBe("OPTIONS");
		}

		[Test]
		public void BadHttpMethodName()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: \" bad \")] method do {}: {} }")
				.Message.ShouldBe("TestApi.fsd(1,25): Invalid HTTP method ' bad '.");
		}

		[Test]
		public void BadHttpMethodParameter()
		{
			ParseInvalidHttpApi("service TestApi { [http(metho: get)] method do {}: {} }")
				.Message.ShouldBe("TestApi.fsd(1,25): Unexpected 'http' parameter 'metho'.");
		}

		[Test]
		public void MultipleHttpMethods()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get)] [http(method: post)] method do {}: {} }")
				.Message.ShouldBe("TestApi.fsd(1,40): 'http' attribute is duplicated.");
		}

		[Test]
		public void MethodPath()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get, path: \"/xyzzy\")] method do {}: {} }").Methods.Single();
			method.Path.ShouldBe("/xyzzy");
		}

		[Test]
		public void MethodStatusCode()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get, code: 202)] method do {}: {} }").Methods.Single();
			method.ValidResponses.Single().StatusCode.ShouldBe(HttpStatusCode.Accepted);
		}

		[Test]
		public void MethodStatusCodeOutOfRange()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get, code: 999)] method do {}: {} }")
				.Message.ShouldBe("TestApi.fsd(1,38): 'code' parameter must be an integer between 200 and 599.");
		}

		[Test]
		public void ImplicitPathField()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get, path: \"/xyzzy/{id}\")] method do { id: string; }: {} }").Methods.Single();
			var field = method.PathFields.Single();
			field.ServiceField.Name.ShouldBe("id");
		}

		[Test]
		public void ExplicitPathField()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get, path: \"/xyzzy/{id}\")] method do { [http(from: path)] id: string; }: {} }").Methods.Single();
			var field = method.PathFields.Single();
			field.ServiceField.Name.ShouldBe("id");
		}

		[Test]
		public void WrongCasePathField()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get, path: \"/xyzzy/{Id}\")] method do { iD: string; }: {} }")
				.Message.ShouldBe("TestApi.fsd(1,60): Unused path parameter 'Id'.");
		}

		[Test]
		public void MissingPathPlaceholder()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get)] method do { [http(from: path)] id: string; }: {} }")
				.Message.ShouldBe("TestApi.fsd(1,70): Request field with [http(from: path)] has no placeholder in the method path.");
		}

		[Test]
		public void ExtraPathPlaceholder()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get, path: \"/xyzzy/{id}\")] method do {}: {} }")
				.Message.ShouldBe("TestApi.fsd(1,60): Unused path parameter 'id'.");
		}

		[Test]
		public void UnusedPathField()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get)] method do { [http(from: path)] id: string; }: {} }")
				.Message.ShouldBe("TestApi.fsd(1,70): Request field with [http(from: path)] has no placeholder in the method path.");
		}

		[Test]
		public void ImplicitPathFieldTypeNotSupported()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get, path: \"/xyzzy/{id}\")] method do { id: Thing; }: {} data Thing { id: string; } }")
				.Message.ShouldBe("TestApi.fsd(1,72): Request field used in path must use a simple type.");
		}

		[Test]
		public void ExplicitPathFieldTypeNotSupported()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get, path: \"/xyzzy/{id}\")] method do { [http(from: path)] id: Thing; }: {} data Thing { id: string; } }")
				.Message.ShouldBe("TestApi.fsd(1,91): Request field used in path must use a simple type.");
		}

		[Test]
		public void PathFieldBadParameter()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get, path: \"/xyzzy/{id}\")] method do { [http(name: identifier)] id: string; }: {} }")
				.Message.ShouldBe("TestApi.fsd(1,78): Unexpected 'http' parameter 'name'.");
		}

		[Test]
		public void ImplicitQueryFieldGet()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get)] method do { id: string; }: {} }").Methods.Single();
			var field = method.QueryFields.Single();
			field.Name.ShouldBe("id");
			field.ServiceField.Name.ShouldBe("id");
		}

		[Test]
		public void ImplicitQueryFieldDelete()
		{
			var method = ParseHttpApi("service TestApi { [http(method: delete)] method do { id: string; }: {} }").Methods.Single();
			var field = method.QueryFields.Single();
			field.Name.ShouldBe("id");
			field.ServiceField.Name.ShouldBe("id");
		}

		[Test]
		public void ExplicitQueryField()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get)] method do { [http(from: query)] id: string; }: {} }").Methods.Single();
			var field = method.QueryFields.Single();
			field.Name.ShouldBe("id");
			field.ServiceField.Name.ShouldBe("id");
		}

		[Test]
		public void NamedQueryField()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get)] method do { [http(name: id)] ident: string; }: {} }").Methods.Single();
			var field = method.QueryFields.Single();
			field.Name.ShouldBe("id");
			field.ServiceField.Name.ShouldBe("ident");
		}

		[Test]
		public void WrongCaseQueryField()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get)] method do { iD: string; }: {} }").Methods.Single();
			var field = method.QueryFields.Single();
			field.Name.ShouldBe("iD");
			field.ServiceField.Name.ShouldBe("iD");
		}

		[Test]
		public void ImplicitQueryFieldTypeNotSupported()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get, path: \"/xyzzy\")] method do { id: Thing; }: {} data Thing { id: string; } }")
				.Message.ShouldBe("TestApi.fsd(1,67): Request field used in query must use a simple type.");
		}

		[Test]
		public void ExplicitQueryFieldTypeNotSupported()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get, path: \"/xyzzy\")] method do { [http(from: query)] id: Thing; }: {} data Thing { id: string; } }")
				.Message.ShouldBe("TestApi.fsd(1,87): Request field used in query must use a simple type.");
		}

		[Test]
		public void QueryFieldBadParameter()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get)] method do { [http(nam: identifier)] id: string; }: {} }")
				.Message.ShouldBe("TestApi.fsd(1,57): Unexpected 'http' parameter 'nam'.");
		}

		[Test]
		public void ImplicitNormalRequestField()
		{
			var method = ParseHttpApi("service TestApi { method do { id: string; }: {} }").Methods.Single();
			method.RequestNormalFields.Single().ServiceField.Name.ShouldBe("id");
		}

		[Test]
		public void ExplicitNormalRequestField()
		{
			var method = ParseHttpApi("service TestApi { method do { [http(from: normal)] id: string; }: {} }").Methods.Single();
			method.RequestNormalFields.Single().ServiceField.Name.ShouldBe("id");
		}

		[Test]
		public void HttpGetNormalRequestField()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get)] method do { [http(from: normal)] id: string; }: {} }")
				.Message.ShouldBe("TestApi.fsd(1,72): HTTP GET does not support normal fields.");
		}

		[Test]
		public void HttpDeleteNormalRequestField()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: delete)] method do { [http(from: normal)] id: string; }: {} }")
				.Message.ShouldBe("TestApi.fsd(1,75): HTTP DELETE does not support normal fields.");
		}

		[TestCase("Dto")]
		public void BodyRequestField(string type)
		{
			var method = ParseHttpApi("service TestApi { data Dto {} enum Enum { x } method do { [http(from: body)] id: xyzzy; }: {} }".Replace("xyzzy", type)).Methods.Single();
			method.RequestBodyField.ServiceField.Name.ShouldBe("id");
		}

		[TestCase("string")]
		[TestCase("boolean")]
		[TestCase("double")]
		[TestCase("int32")]
		[TestCase("int64")]
		[TestCase("bytes")]
		[TestCase("Enum")]
		[TestCase("object")]
		[TestCase("error")]
		[TestCase("result<Dto>")]
		[TestCase("Dto[]")]
		[TestCase("map<Dto>")]
		public void BodyRequestFieldInvalidType(string type)
		{
			ParseInvalidHttpApi("service TestApi { data Dto {} enum Enum { x } method do { [http(from: body)] id: xyzzy; }: {} }".Replace("xyzzy", type))
				.Message.ShouldBe("TestApi.fsd(1,78): Request fields with [http(from: body)] must use a DTO type.");
		}

		[Test]
		public void BodyRequestFieldNoStatusCode()
		{
			ParseInvalidHttpApi("service TestApi { method do { [http(from: body, code: 200)] id: Thing; }: {} data Thing { id: string; } }")
				.Message.ShouldBe("TestApi.fsd(1,61): Request fields do not support status codes.");
		}

		[Test]
		public void MultipleBodyRequestFields()
		{
			ParseInvalidHttpApi("service TestApi { method do { [http(from: body)] body1: Thing; [http(from: body)] body2: Thing; }: {} data Thing { id: string; } }")
				.Message.ShouldBe("TestApi.fsd(1,83): Requests do not support multiple [http(from: body)] fields.");
		}

		[Test]
		public void BodyAndNormalRequestFields()
		{
			ParseInvalidHttpApi("service TestApi { method do { id: string; [http(from: body)] body: Thing; }: {} data Thing { id: string; } }")
				.Message.ShouldBe("TestApi.fsd(1,62): A request cannot have a normal field and a body field.");
		}

		[Test]
		public void HeaderRequestField()
		{
			var method = ParseHttpApi("service TestApi { method do { [http(from: header)] xyzzy: string; }: {} }").Methods.Single();
			method.RequestHeaderFields.Single().ServiceField.Name.ShouldBe("xyzzy");
			method.RequestHeaderFields.Single().Name.ShouldBe("xyzzy");
		}

		[Test]
		public void NamedHeaderRequestField()
		{
			var method = ParseHttpApi("service TestApi { method do { [http(from: header, name: Our-Xyzzy)] xyzzy: string; }: {} }").Methods.Single();
			method.RequestHeaderFields.Single().ServiceField.Name.ShouldBe("xyzzy");
			method.RequestHeaderFields.Single().Name.ShouldBe("Our-Xyzzy");
		}

		[Test]
		public void ImplicitNormalResponseField()
		{
			var method = ParseHttpApi("service TestApi { method do {}: { id: string; } }").Methods.Single();

			var response = method.ValidResponses.Single();
			response.StatusCode.ShouldBe(HttpStatusCode.OK);
			response.NormalFields.Single().ServiceField.Name.ShouldBe("id");
			response.BodyField.ShouldBe(null);
		}

		[Test]
		public void ExplicitNormalResponseField()
		{
			var method = ParseHttpApi("service TestApi { method do {}: { [http(from: normal)] id: string; } }").Methods.Single();

			var response = method.ValidResponses.Single();
			response.StatusCode.ShouldBe(HttpStatusCode.OK);
			response.NormalFields.Single().ServiceField.Name.ShouldBe("id");
			response.BodyField.ShouldBe(null);
		}

		[Test]
		public void NormalFieldBadParameter()
		{
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(name: identifier)] id: string; } }")
				.Message.ShouldBe("TestApi.fsd(1,41): Unexpected 'http' parameter 'name'.");
		}

		[TestCase("Dto")]
		public void BodyResponseField(string type)
		{
			var method = ParseHttpApi("service TestApi { data Dto {} enum Enum { x } method do {}: { [http(from: body)] id: xyzzy; } }".Replace("xyzzy", type)).Methods.Single();

			var response = method.ValidResponses.Single();
			response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK);
			response.NormalFields.ShouldBe(null);
			response.BodyField.ServiceField.Name.ShouldBe("id");
			response.BodyField.StatusCode.ShouldBe(null);
		}

		[TestCase("string")]
		[TestCase("double")]
		[TestCase("int32")]
		[TestCase("int64")]
		[TestCase("bytes")]
		[TestCase("Enum")]
		[TestCase("object")]
		[TestCase("error")]
		[TestCase("result<Dto>")]
		[TestCase("Dto[]")]
		[TestCase("map<Dto>")]
		public void BodyResponseFieldInvalidType(string type)
		{
			ParseInvalidHttpApi("service TestApi { data Dto {} enum Enum { x } method do {}: { [http(from: body)] id: xyzzy; } }".Replace("xyzzy", type))
				.Message.ShouldBe("TestApi.fsd(1,82): Response fields with [http(from: body)] must be a DTO or a Boolean.");
		}

		[Test]
		public void BodyResponseFieldWithStatusCode()
		{
			var method = ParseHttpApi("service TestApi { method do {}: { [http(from: body, code: 202)] body: Thing; } data Thing { id: string; } }").Methods.Single();

			var response = method.ValidResponses.Single();
			response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
			response.NormalFields.ShouldBe(null);
			response.BodyField.ServiceField.Name.ShouldBe("body");
			response.BodyField.StatusCode.ShouldBe(HttpStatusCode.Accepted);
		}

		[Test]
		public void BodyResponseFieldWithMethodStatusCode()
		{
			var method = ParseHttpApi("service TestApi { [http(method: post, code: 202)] method do {}: { [http(from: body)] body: Thing; } data Thing { id: string; } }").Methods.Single();

			var responses = method.ValidResponses;
			responses.Count.ShouldBe(2);
			responses[0].StatusCode.ShouldBe(HttpStatusCode.OK);
			responses[0].NormalFields.ShouldBe(null);
			responses[0].BodyField.ServiceField.Name.ShouldBe("body");
			responses[0].BodyField.StatusCode.ShouldBe(null);
			responses[1].StatusCode.ShouldBe(HttpStatusCode.Accepted);
			responses[1].NormalFields.Count.ShouldBe(0);
			responses[1].BodyField.ShouldBe(null);
		}

		[Test]
		public void BodyResponseFieldWithMethodStatusCodeConflict()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: post, code: 200)] method do {}: { [http(from: body)] body: Thing; } data Thing { id: string; } }")
				.Message.ShouldBe("TestApi.fsd(1,51): Multiple handlers for status code 200.");
		}

		[Test]
		public void TwoBodyResponseFields()
		{
			var method = ParseHttpApi("service TestApi { data Empty {} method do {}: { [http(from: body)] body1: Empty; [http(from: body, code: 201)] body2: Thing; } data Thing { id: string; } }").Methods.Single();

			var responses = method.ValidResponses;
			responses.Count.ShouldBe(2);
			responses[0].StatusCode.ShouldBe(HttpStatusCode.OK);
			responses[0].NormalFields.ShouldBe(null);
			responses[0].BodyField.ServiceField.Name.ShouldBe("body1");
			responses[0].BodyField.StatusCode.ShouldBe(null);
			responses[1].StatusCode.ShouldBe(HttpStatusCode.Created);
			responses[1].NormalFields.ShouldBe(null);
			responses[1].BodyField.ServiceField.Name.ShouldBe("body2");
			responses[1].BodyField.StatusCode.ShouldBe(HttpStatusCode.Created);
		}

		[Test]
		public void TwoBodyResponseFieldsWithMethodStatusCode()
		{
			var method = ParseHttpApi("service TestApi { [http(method: post, code: 400)] method do {}: { [http(from: body)] body1: Empty; [http(from: body, code: 201)] body2: Thing; } data Thing { id: string; } data Empty {} }").Methods.Single();

			var responses = method.ValidResponses;
			responses.Count.ShouldBe(3);
			responses[0].StatusCode.ShouldBe(HttpStatusCode.OK);
			responses[0].NormalFields.ShouldBe(null);
			responses[0].BodyField.ServiceField.Name.ShouldBe("body1");
			responses[0].BodyField.StatusCode.ShouldBe(null);
			responses[1].StatusCode.ShouldBe(HttpStatusCode.Created);
			responses[1].NormalFields.ShouldBe(null);
			responses[1].BodyField.ServiceField.Name.ShouldBe("body2");
			responses[1].BodyField.StatusCode.ShouldBe(HttpStatusCode.Created);
			responses[2].StatusCode.ShouldBe(HttpStatusCode.BadRequest);
			responses[2].NormalFields.Count.ShouldBe(0);
			responses[2].BodyField.ShouldBe(null);
		}

		[Test]
		public void TwoBodyResponseFieldsWithExtraStatusCode()
		{
			var method = ParseHttpApi("service TestApi { [http(method: post, code: 204)] method do {}: { [http(from: body, code: 200)] body1: Empty; [http(from: body, code: 201)] body2: Thing; } data Thing { id: string; } data Empty {} }").Methods.Single();

			var responses = method.ValidResponses;
			responses.Count.ShouldBe(3);
			responses[0].StatusCode.ShouldBe(HttpStatusCode.OK);
			responses[0].NormalFields.ShouldBe(null);
			responses[0].BodyField.ServiceField.Name.ShouldBe("body1");
			responses[0].BodyField.StatusCode.ShouldBe(HttpStatusCode.OK);
			responses[1].StatusCode.ShouldBe(HttpStatusCode.Created);
			responses[1].NormalFields.ShouldBe(null);
			responses[1].BodyField.ServiceField.Name.ShouldBe("body2");
			responses[1].BodyField.StatusCode.ShouldBe(HttpStatusCode.Created);
			responses[2].StatusCode.ShouldBe(HttpStatusCode.NoContent);
			responses[2].NormalFields.Count.ShouldBe(0);
			responses[2].BodyField.ShouldBe(null);
		}

		[Test]
		public void BooleanResponseField()
		{
			var method = ParseHttpApi("service TestApi { method do {}: { [http(from: body)] body: boolean; } }").Methods.Single();

			var response = method.ValidResponses.Single();
			response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
			response.BodyField.ServiceField.Name.ShouldBe("body");
			response.BodyField.StatusCode.ShouldBe(null);
		}

		[Test]
		public void ConflictingBodyResponseFields()
		{
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(from: body)] body1: Thing; [http(from: body)] body2: Thing; } data Thing { id: string; } }")
				.Message.ShouldBe("TestApi.fsd(1,19): Multiple handlers for status code 200.");
		}

		[Test]
		public void BodyFieldBadParameter()
		{
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(from: body, cod: 200)] id: Thing; } data Thing { id: string; } }")
				.Message.ShouldBe("TestApi.fsd(1,53): Unexpected 'http' parameter 'cod'.");
		}

		[Test]
		public void ResponsePathField()
		{
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(from: path)] id: string; } }")
				.Message.ShouldBe("TestApi.fsd(1,54): Response fields do not support '[http(from: path)]'.");
		}

		[Test]
		public void ResponseQueryField()
		{
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(from: query)] id: string; } }")
				.Message.ShouldBe("TestApi.fsd(1,55): Response fields do not support '[http(from: query)]'.");
		}

		[Test]
		public void HeaderResponseField()
		{
			var method = ParseHttpApi("service TestApi { method do {}: { [http(from: header)] xyzzy: string; } }").Methods.Single();
			method.ResponseHeaderFields.Single().ServiceField.Name.ShouldBe("xyzzy");
			method.ResponseHeaderFields.Single().Name.ShouldBe("xyzzy");
		}

		[Test]
		public void NamedHeaderResponseField()
		{
			var method = ParseHttpApi("service TestApi { method do {}: { [http(from: header, name: Our-Xyzzy)] xyzzy: string; } }").Methods.Single();
			method.ResponseHeaderFields.Single().ServiceField.Name.ShouldBe("xyzzy");
			method.ResponseHeaderFields.Single().Name.ShouldBe("Our-Xyzzy");
		}

		[Test]
		public void RequestHeaderFieldBadType()
		{
			ParseInvalidHttpApi("service TestApi { method do { [http(from: header)] xyzzy: error; }: {} }")
				.Message.ShouldBe("TestApi.fsd(1,52): Request fields with [http(from: header)] must use the string type.");
		}

		[Test]
		public void ResponseHeaderFieldBadType()
		{
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(from: header)] xyzzy: error; } }")
				.Message.ShouldBe("TestApi.fsd(1,56): Response fields with [http(from: header)] must use the string type.");
		}

		[Test]
		public void RequestHeaderFieldBadParameter()
		{
			ParseInvalidHttpApi("service TestApi { method do { [http(from: header, nam: x)] xyzzy: string; }: {} }")
				.Message.ShouldBe("TestApi.fsd(1,51): Unexpected 'http' parameter 'nam'.");
		}

		[Test]
		public void ResponseHeaderFieldBadParameter()
		{
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(from: header, nam: x)] xyzzy: string; } }")
				.Message.ShouldBe("TestApi.fsd(1,55): Unexpected 'http' parameter 'nam'.");
		}

		[Test]
		public void RequestFieldInvalidFrom()
		{
			ParseInvalidHttpApi("service TestApi { method do { [http(from: heade)] xyzzy: string; }: {} }")
				.Message.ShouldBe("TestApi.fsd(1,51): Unsupported 'from' parameter of 'http' attribute: 'heade'");
		}

		[Test]
		public void ResponseFieldInvalidFrom()
		{
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(from: heade)] xyzzy: string; } }")
				.Message.ShouldBe("TestApi.fsd(1,55): Unsupported 'from' parameter of 'http' attribute: 'heade'");
		}

		[TestCase("", "", -1)]
		[TestCase("[http(path: \"/right\")]", "[http(path: \"/left\")]", 1)]
		[TestCase("[http(path: \"/xyzzy\")]", "[http(path: \"/\")]", 1)]
		[TestCase("[http(path: \"/\")]", "[http(path: \"/xyzzy\")]", -1)]
		[TestCase("[http(path: \"/xyzzy\")]", "[http(path: \"/{id}\")]", -1)]
		[TestCase("[http(path: \"/{id}\")]", "[http(path: \"/xyzzy\")]", 1)]
		[TestCase("[http(path: \"/{id}/xyzzy\")]", "[http(path: \"/xyzzy/{id}\")]", 1)]
		[TestCase("[http(method: get, path: \"/\")]", "[http(method: post, path: \"/\")]", -1)]
		[TestCase("[http(method: post, path: \"/\")]", "[http(method: put, path: \"/\")]", -1)]
		[TestCase("[http(method: delete, path: \"/\")]", "[http(method: put, path: \"/\")]", 1)]
		[TestCase("[http(method: get, path: \"/\")]", "[http(method: apple, path: \"/\")]", -1)]
		[TestCase("[http(method: xyzzy, path: \"/\")]", "[http(method: post, path: \"/\")]", 1)]
		public void ByRouteComparer(string leftHttp, string rightHttp, int expected)
		{
			string fsdText = "service TestApi { [left] method left { id: string; }: {} [right] method right { id: string; }: {} }".Replace("[left]", leftHttp).Replace("[right]", rightHttp);
			var service = new HttpServiceInfo(new FsdParser().ParseDefinition(new NamedText("", fsdText)));
			var left = service.Methods.Single(x => x.ServiceMethod.Name == "left");
			var right = service.Methods.Single(x => x.ServiceMethod.Name == "right");
			int actual = HttpMethodInfo.ByRouteComparer.Compare(left, right);
			if (expected < 0)
				actual.ShouldBeLessThan(0);
			else
				actual.ShouldBeGreaterThan(0);
		}
	}
}
