using System.Globalization;
using System.Linq;
using System.Net;
using Facility.Definition.Fsd;
using Facility.Definition.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests.Http
{
	public class HttpMethodInfoTests : HttpInfoTestsBase
	{
		[Test]
		public void OneMinimalMethod()
		{
			var method = ParseHttpApi("service TestApi { method do {}: {} }").Methods.Single();
			method.ServiceMethod.Name.Should().Be("do");
			method.Method.Should().Be("POST");
			method.Path.Should().Be("/do");
			method.PathFields.Count.Should().Be(0);
			method.QueryFields.Count.Should().Be(0);
			method.RequestNormalFields.Count.Should().Be(0);
			method.RequestBodyField.Should().BeNull();
			method.RequestHeaderFields.Count.Should().Be(0);
			method.ResponseHeaderFields.Count.Should().Be(0);

			var response = method.ValidResponses.Single();
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			response.NormalFields.Count.Should().Be(0);
			response.BodyField.Should().BeNull();
		}

		[TestCase("get")]
		[TestCase("post")]
		[TestCase("put")]
		[TestCase("delete")]
		[TestCase("patch")]
		public void HttpMethod(string name)
		{
			var method = ParseHttpApi("service TestApi { [http(method: _)] method do {}: {} }".Replace("_", name)).Methods.Single();
			method.Method.Should().Be(name.ToUpperInvariant());
		}

		[Test]
		public void UnsupportedHttpOptionsMethod()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: options)] method do {}: {} }")
				.ToString().Should().Be("TestApi.fsd(1,33): Unsupported HTTP method 'OPTIONS'.");
		}

		[Test]
		public void BadHttpMethodName()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: \" bad \")] method do {}: {} }")
				.ToString().Should().Be("TestApi.fsd(1,33): Unsupported HTTP method ' BAD '.");
		}

		[Test]
		public void BadHttpMethodParameter()
		{
			ParseInvalidHttpApi("service TestApi { [http(metho: get)] method do {}: {} }")
				.ToString().Should().Be("TestApi.fsd(1,25): Unexpected 'http' parameter 'metho'.");
		}

		[Test]
		public void MultipleHttpMethods()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get)] [http(method: post)] method do {}: {} }")
				.ToString().Should().Be("TestApi.fsd(1,40): 'http' attribute is duplicated.");
		}

		[Test]
		public void MethodPath()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get, path: \"/xyzzy\")] method do {}: {} }").Methods.Single();
			method.Path.Should().Be("/xyzzy");
		}

		[Test]
		public void MethodPathEmpty()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get, path: \"\")] method do {}: {} }")
				.ToString().Should().Be("TestApi.fsd(1,44): 'path' value must start with a slash.");
		}

		[Test]
		public void MethodPathNoSlash()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get, path: \"xyzzy\")] method do {}: {} }")
				.ToString().Should().Be("TestApi.fsd(1,44): 'path' value must start with a slash.");
		}

		[Test]
		public void MethodStatusCode()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get, code: 202)] method do {}: {} }").Methods.Single();
			method.ValidResponses.Single().StatusCode.Should().Be(HttpStatusCode.Accepted);
		}

		[Test]
		public void MethodStatusCodeOutOfRange()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get, code: 999)] method do {}: {} }")
				.ToString().Should().Be("TestApi.fsd(1,44): 'code' parameter must be an integer between 200 and 599.");
		}

		[Test]
		public void ImplicitPathField()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get, path: \"/xyzzy/{id}\")] method do { id: string; }: {} }").Methods.Single();
			var field = method.PathFields.Single();
			field.Name.Should().Be("id");
		}

		[Test]
		public void ExplicitPathField()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get, path: \"/xyzzy/{id}\")] method do { [http(from: path)] id: string; }: {} }").Methods.Single();
			var field = method.PathFields.Single();
			field.Name.Should().Be("id");
		}

		[Test]
		public void WrongCasePathField()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get, path: \"/xyzzy/{Id}\")] method do { iD: string; }: {} }")
				.ToString().Should().Be("TestApi.fsd(1,60): Unused path parameter 'Id'.");
		}

		[Test]
		public void MissingPathPlaceholder()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get)] method do { [http(from: path)] id: string; }: {} }")
				.ToString().Should().Be("TestApi.fsd(1,70): Path request field has no placeholder in the method path.");
		}

		[Test]
		public void ExtraPathPlaceholder()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get, path: \"/xyzzy/{id}\")] method do {}: {} }")
				.ToString().Should().Be("TestApi.fsd(1,60): Unused path parameter 'id'.");
		}

		[Test]
		public void UnusedPathField()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get)] method do { [http(from: path)] id: string; }: {} }")
				.ToString().Should().Be("TestApi.fsd(1,70): Path request field has no placeholder in the method path.");
		}

		[Test]
		public void PathFieldBadParameter()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get, path: \"/xyzzy/{id}\")] method do { [http(name: identifier)] id: string; }: {} }")
				.ToString().Should().Be("TestApi.fsd(1,78): Unexpected 'http' parameter 'name'.");
		}

		[Test]
		public void ImplicitQueryFieldGet()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get)] method do { id: string; }: {} }").Methods.Single();
			var field = method.QueryFields.Single();
			field.Name.Should().Be("id");
			field.ServiceField.Name.Should().Be("id");
		}

		[Test]
		public void ImplicitQueryFieldDelete()
		{
			var method = ParseHttpApi("service TestApi { [http(method: delete)] method do { id: string; }: {} }").Methods.Single();
			var field = method.QueryFields.Single();
			field.Name.Should().Be("id");
			field.ServiceField.Name.Should().Be("id");
		}

		[Test]
		public void ExplicitQueryField()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get)] method do { [http(from: query)] id: string; }: {} }").Methods.Single();
			var field = method.QueryFields.Single();
			field.Name.Should().Be("id");
			field.ServiceField.Name.Should().Be("id");
		}

		[Test]
		public void NamedQueryField()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get)] method do { [http(name: id)] ident: string; }: {} }").Methods.Single();
			var field = method.QueryFields.Single();
			field.Name.Should().Be("id");
			field.ServiceField.Name.Should().Be("ident");
		}

		[Test]
		public void WrongCaseQueryField()
		{
			var method = ParseHttpApi("service TestApi { [http(method: get)] method do { iD: string; }: {} }").Methods.Single();
			var field = method.QueryFields.Single();
			field.Name.Should().Be("iD");
			field.ServiceField.Name.Should().Be("iD");
		}

		[Test]
		public void QueryFieldBadParameter()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get)] method do { [http(nam: identifier)] id: string; }: {} }")
				.ToString().Should().Be("TestApi.fsd(1,57): Unexpected 'http' parameter 'nam'.");
		}

		[Test]
		public void ImplicitNormalRequestField()
		{
			var method = ParseHttpApi("service TestApi { method do { id: string; }: {} }").Methods.Single();
			method.RequestNormalFields.Single().ServiceField.Name.Should().Be("id");
		}

		[Test]
		public void ExplicitNormalRequestField()
		{
			var method = ParseHttpApi("service TestApi { method do { [http(from: normal)] id: string; }: {} }").Methods.Single();
			method.RequestNormalFields.Single().ServiceField.Name.Should().Be("id");
		}

		[Test]
		public void HttpGetNormalRequestField()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get)] method do { [http(from: normal)] id: string; }: {} }")
				.ToString().Should().Be("TestApi.fsd(1,72): HTTP GET does not support normal fields.");
		}

		[Test]
		public void HttpDeleteNormalRequestField()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: delete)] method do { [http(from: normal)] id: string; }: {} }")
				.ToString().Should().Be("TestApi.fsd(1,75): HTTP DELETE does not support normal fields.");
		}

		[TestCase("Dto")]
		[TestCase("object")]
		[TestCase("error")]
		[TestCase("result<Dto>")]
		[TestCase("Dto[]")]
		[TestCase("map<Dto>")]
		[TestCase("map<string[]>[]")]
		[TestCase("string")]
		[TestCase("bytes")]
		public void BodyRequestField(string type)
		{
			var method = ParseHttpApi("service TestApi { data Dto {} enum Enum { x } method do { [http(from: body)] id: xyzzy; }: {} }".Replace("xyzzy", type)).Methods.Single();
			method.RequestBodyField.ServiceField.Name.Should().Be("id");
		}

		[TestCase("boolean")]
		[TestCase("double")]
		[TestCase("int32")]
		[TestCase("int64")]
		[TestCase("decimal")]
		[TestCase("Enum")]
		public void BodyRequestFieldInvalidType(string type)
		{
			ParseInvalidHttpApi("service TestApi { data Dto {} enum Enum { x } method do { [http(from: body)] id: xyzzy; }: {} }".Replace("xyzzy", type))
				.ToString().Should().Be("TestApi.fsd(1,78): Type not supported by body request field.");
		}

		[Test]
		public void BodyRequestFieldNoStatusCode()
		{
			ParseInvalidHttpApi("service TestApi { method do { [http(from: body, code: 200)] id: Thing; }: {} data Thing { id: string; } }")
				.ToString().Should().Be("TestApi.fsd(1,61): Request fields do not support status codes.");
		}

		[Test]
		public void MultipleBodyRequestFields()
		{
			ParseInvalidHttpApi("service TestApi { method do { [http(from: body)] body1: Thing; [http(from: body)] body2: Thing; }: {} data Thing { id: string; } }")
				.ToString().Should().Be("TestApi.fsd(1,83): Requests do not support multiple body fields.");
		}

		[Test]
		public void BodyAndNormalRequestFields()
		{
			ParseInvalidHttpApi("service TestApi { method do { id: string; [http(from: body)] body: Thing; }: {} data Thing { id: string; } }")
				.ToString().Should().Be("TestApi.fsd(1,62): A request cannot have a normal field and a body field.");
		}

		[Test]
		public void HeaderRequestField()
		{
			var method = ParseHttpApi("service TestApi { method do { [http(from: header)] xyzzy: string; }: {} }").Methods.Single();
			method.RequestHeaderFields.Single().ServiceField.Name.Should().Be("xyzzy");
			method.RequestHeaderFields.Single().Name.Should().Be("xyzzy");
		}

		[Test]
		public void NamedHeaderRequestField()
		{
			var method = ParseHttpApi("service TestApi { method do { [http(from: header, name: Our-Xyzzy)] xyzzy: string; }: {} }").Methods.Single();
			method.RequestHeaderFields.Single().ServiceField.Name.Should().Be("xyzzy");
			method.RequestHeaderFields.Single().Name.Should().Be("Our-Xyzzy");
		}

		[TestCase(false)]
		[TestCase(true)]
		public void NormalResponseField(bool isExplicit)
		{
			var method = ParseHttpApi("service TestApi { method do {}: { [...] id: string; } }".Replace("[...]",
				isExplicit ? "[http(from: normal)]" : "")).Methods.Single();

			var response = method.ValidResponses.Single();
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			response.NormalFields!.Single().ServiceField.Name.Should().Be("id");
			response.BodyField.Should().BeNull();
		}

		[TestCase(false)]
		[TestCase(true)]
		public void NormalResponseFieldNoContent(bool isExplicit)
		{
			ParseInvalidHttpApi("service TestApi { [http(code: 204)] method do {}: { [...] id: string; } }".Replace("[...]",
					isExplicit ? "[http(from: normal)]" : ""))
				.ToString().Should().Be($"TestApi.fsd(1,{(isExplicit ? 74 : 54)}): HTTP status code 204 does not support normal fields.");
		}

		[TestCase(false)]
		[TestCase(true)]
		public void NormalResponseFieldNotModified(bool isExplicit)
		{
			ParseInvalidHttpApi("service TestApi { [http(code: 304)] method do {}: { [...] id: string; } }".Replace("[...]",
					isExplicit ? "[http(from: normal)]" : ""))
				.ToString().Should().Be($"TestApi.fsd(1,{(isExplicit ? 74 : 54)}): HTTP status code 304 does not support normal fields.");
		}

		[Test]
		public void NormalFieldBadParameter()
		{
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(name: identifier)] id: string; } }")
				.ToString().Should().Be("TestApi.fsd(1,41): Unexpected 'http' parameter 'name'.");
		}

		[TestCase("Dto")]
		[TestCase("object")]
		[TestCase("error")]
		[TestCase("result<Dto>")]
		[TestCase("Dto[]")]
		[TestCase("map<Dto>")]
		[TestCase("map<string[]>[]")]
		[TestCase("string")]
		[TestCase("bytes")]
		public void BodyResponseField(string type)
		{
			var method = ParseHttpApi("service TestApi { data Dto {} enum Enum { x } method do {}: { [http(from: body)] id: xyzzy; } }".Replace("xyzzy", type)).Methods.Single();

			var response = method.ValidResponses.Single();
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			response.NormalFields.Should().BeNull();
			response.BodyField.ServiceField.Name.Should().Be("id");
			response.BodyField.StatusCode.Should().BeNull();
		}

		[TestCase("double")]
		[TestCase("int32")]
		[TestCase("int64")]
		[TestCase("decimal")]
		[TestCase("Enum")]
		public void BodyResponseFieldInvalidType(string type)
		{
			ParseInvalidHttpApi("service TestApi { data Dto {} enum Enum { x } method do {}: { [http(from: body)] id: xyzzy; } }".Replace("xyzzy", type))
				.ToString().Should().Be("TestApi.fsd(1,82): Type not supported by body response field.");
		}

		[Test]
		public void BodyResponseFieldWithStatusCode()
		{
			var method = ParseHttpApi("service TestApi { method do {}: { [http(from: body, code: 202)] body: Thing; } data Thing { id: string; } }").Methods.Single();

			var response = method.ValidResponses.Single();
			response.StatusCode.Should().Be(HttpStatusCode.Accepted);
			response.NormalFields.Should().BeNull();
			response.BodyField.ServiceField.Name.Should().Be("body");
			response.BodyField.StatusCode.Should().Be(HttpStatusCode.Accepted);
		}

		[Test]
		public void BodyResponseFieldWithMethodStatusCode()
		{
			var method = ParseHttpApi("service TestApi { [http(method: post, code: 202)] method do {}: { [http(from: body)] body: Thing; } data Thing { id: string; } }").Methods.Single();

			var responses = method.ValidResponses;
			responses.Count.Should().Be(2);
			responses[0].StatusCode.Should().Be(HttpStatusCode.OK);
			responses[0].NormalFields.Should().BeNull();
			responses[0].BodyField.ServiceField.Name.Should().Be("body");
			responses[0].BodyField.StatusCode.Should().BeNull();
			responses[1].StatusCode.Should().Be(HttpStatusCode.Accepted);
			responses[1].NormalFields.Count.Should().Be(0);
			responses[1].BodyField.Should().BeNull();
		}

		[Test]
		public void BodyResponseFieldWithMethodStatusCodeConflict()
		{
			ParseInvalidHttpApi("service TestApi { [http(method: post, code: 200)] method do {}: { [http(from: body)] body: Thing; } data Thing { id: string; } }")
				.ToString().Should().Be("TestApi.fsd(1,51): Multiple handlers for status code 200.");
		}

		[Test]
		public void TwoBodyResponseFields()
		{
			var method = ParseHttpApi("service TestApi { data Empty {} method do {}: { [http(from: body)] body1: Empty; [http(from: body, code: 201)] body2: Thing; } data Thing { id: string; } }").Methods.Single();

			var responses = method.ValidResponses;
			responses.Count.Should().Be(2);
			responses[0].StatusCode.Should().Be(HttpStatusCode.OK);
			responses[0].NormalFields.Should().BeNull();
			responses[0].BodyField.ServiceField.Name.Should().Be("body1");
			responses[0].BodyField.StatusCode.Should().BeNull();
			responses[1].StatusCode.Should().Be(HttpStatusCode.Created);
			responses[1].NormalFields.Should().BeNull();
			responses[1].BodyField.ServiceField.Name.Should().Be("body2");
			responses[1].BodyField.StatusCode.Should().Be(HttpStatusCode.Created);
		}

		[Test]
		public void TwoBodyResponseFieldsWithMethodStatusCode()
		{
			var method = ParseHttpApi("service TestApi { [http(method: post, code: 400)] method do {}: { [http(from: body)] body1: Empty; [http(from: body, code: 201)] body2: Thing; } data Thing { id: string; } data Empty {} }").Methods.Single();

			var responses = method.ValidResponses;
			responses.Count.Should().Be(3);
			responses[0].StatusCode.Should().Be(HttpStatusCode.OK);
			responses[0].NormalFields.Should().BeNull();
			responses[0].BodyField.ServiceField.Name.Should().Be("body1");
			responses[0].BodyField.StatusCode.Should().BeNull();
			responses[1].StatusCode.Should().Be(HttpStatusCode.Created);
			responses[1].NormalFields.Should().BeNull();
			responses[1].BodyField.ServiceField.Name.Should().Be("body2");
			responses[1].BodyField.StatusCode.Should().Be(HttpStatusCode.Created);
			responses[2].StatusCode.Should().Be(HttpStatusCode.BadRequest);
			responses[2].NormalFields.Count.Should().Be(0);
			responses[2].BodyField.Should().BeNull();
		}

		[Test]
		public void TwoBodyResponseFieldsWithExtraStatusCode()
		{
			var method = ParseHttpApi("service TestApi { [http(method: post, code: 204)] method do {}: { [http(from: body, code: 200)] body1: Empty; [http(from: body, code: 201)] body2: Thing; } data Thing { id: string; } data Empty {} }").Methods.Single();

			var responses = method.ValidResponses;
			responses.Count.Should().Be(3);
			responses[0].StatusCode.Should().Be(HttpStatusCode.OK);
			responses[0].NormalFields.Should().BeNull();
			responses[0].BodyField.ServiceField.Name.Should().Be("body1");
			responses[0].BodyField.StatusCode.Should().Be(HttpStatusCode.OK);
			responses[1].StatusCode.Should().Be(HttpStatusCode.Created);
			responses[1].NormalFields.Should().BeNull();
			responses[1].BodyField.ServiceField.Name.Should().Be("body2");
			responses[1].BodyField.StatusCode.Should().Be(HttpStatusCode.Created);
			responses[2].StatusCode.Should().Be(HttpStatusCode.NoContent);
			responses[2].NormalFields.Count.Should().Be(0);
			responses[2].BodyField.Should().BeNull();
		}

		[Test]
		public void BooleanResponseBodyField()
		{
			var method = ParseHttpApi("service TestApi { method do {}: { [http(from: body)] body: boolean; } }").Methods.Single();

			var response = method.ValidResponses.Single();
			response.StatusCode.Should().Be(HttpStatusCode.NoContent);
			response.BodyField.ServiceField.Name.Should().Be("body");
			response.BodyField.StatusCode.Should().BeNull();
		}

		[TestCase(HttpStatusCode.NoContent)]
		[TestCase(HttpStatusCode.NotModified)]
		public void BooleanNoContentResponseBodyField(HttpStatusCode statusCode)
		{
			var method = ParseHttpApi("service TestApi { method do {}: { [http(from: body, code: CODE)] body: boolean; } }".Replace("CODE", ((int) statusCode).ToString(CultureInfo.InvariantCulture))).Methods.Single();

			var response = method.ValidResponses.Single();
			response.StatusCode.Should().Be(statusCode);
			response.BodyField.ServiceField.Name.Should().Be("body");
			response.BodyField.StatusCode.Should().Be(statusCode);
		}

		[TestCase(HttpStatusCode.NoContent)]
		[TestCase(HttpStatusCode.NotModified)]
		public void NonBooleanNoContentResponseBodyField(HttpStatusCode statusCode)
		{
			string statusCodeString = ((int) statusCode).ToString(CultureInfo.InvariantCulture);
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(from: body, code: CODE)] body: error; } }".Replace("CODE", statusCodeString))
				.ToString().Should().Be("TestApi.fsd(1,65): A body field with HTTP status code CODE must be Boolean.".Replace("CODE", statusCodeString));
		}

		[Test]
		public void ConflictingBodyResponseFields()
		{
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(from: body)] body1: Thing; [http(from: body)] body2: Thing; } data Thing { id: string; } }")
				.ToString().Should().Be("TestApi.fsd(1,19): Multiple handlers for status code 200.");
		}

		[Test]
		public void BodyFieldBadParameter()
		{
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(from: body, cod: 200)] id: Thing; } data Thing { id: string; } }")
				.ToString().Should().Be("TestApi.fsd(1,53): Unexpected 'http' parameter 'cod'.");
		}

		[Test]
		public void ResponsePathField()
		{
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(from: path)] id: string; } }")
				.ToString().Should().Be("TestApi.fsd(1,54): Response fields must not be path or query fields.");
		}

		[Test]
		public void ResponseQueryField()
		{
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(from: query)] id: string; } }")
				.ToString().Should().Be("TestApi.fsd(1,55): Response fields must not be path or query fields.");
		}

		[Test]
		public void HeaderResponseField()
		{
			var method = ParseHttpApi("service TestApi { method do {}: { [http(from: header)] xyzzy: string; } }").Methods.Single();
			method.ResponseHeaderFields.Single().ServiceField.Name.Should().Be("xyzzy");
			method.ResponseHeaderFields.Single().Name.Should().Be("xyzzy");
		}

		[Test]
		public void NamedHeaderResponseField()
		{
			var method = ParseHttpApi("service TestApi { method do {}: { [http(from: header, name: Our-Xyzzy)] xyzzy: string; } }").Methods.Single();
			method.ResponseHeaderFields.Single().ServiceField.Name.Should().Be("xyzzy");
			method.ResponseHeaderFields.Single().Name.Should().Be("Our-Xyzzy");
		}

		[Test]
		public void RequestHeaderFieldBadParameter()
		{
			ParseInvalidHttpApi("service TestApi { method do { [http(from: header, nam: x)] xyzzy: string; }: {} }")
				.ToString().Should().Be("TestApi.fsd(1,51): Unexpected 'http' parameter 'nam'.");
		}

		[Test]
		public void ResponseHeaderFieldBadParameter()
		{
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(from: header, nam: x)] xyzzy: string; } }")
				.ToString().Should().Be("TestApi.fsd(1,55): Unexpected 'http' parameter 'nam'.");
		}

		[Test]
		public void RequestFieldInvalidFrom()
		{
			ParseInvalidHttpApi("service TestApi { method do { [http(from: heade)] xyzzy: string; }: {} }")
				.ToString().Should().Be("TestApi.fsd(1,51): Unsupported 'from' parameter of 'http' attribute: 'heade'");
		}

		[Test]
		public void ResponseFieldInvalidFrom()
		{
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(from: heade)] xyzzy: string; } }")
				.ToString().Should().Be("TestApi.fsd(1,55): Unsupported 'from' parameter of 'http' attribute: 'heade'");
		}

		[TestCase("string")]
		[TestCase("boolean")]
		[TestCase("double")]
		[TestCase("int32")]
		[TestCase("int64")]
		[TestCase("decimal")]
		[TestCase("Enum")]
		[TestCase("string[]")]
		[TestCase("boolean[]")]
		[TestCase("double[]")]
		[TestCase("int32[]")]
		[TestCase("int64[]")]
		[TestCase("decimal[]")]
		[TestCase("Enum[]")]
		public void SimpleFieldTypeSupported(string type)
		{
			ParseHttpApi("service TestApi { [http(method: get, path: \"/xyzzy/{id}\")] method do { id: _; }: {} enum Enum { x } }".Replace("_", type))
				.Methods.Single().PathFields.Single().ServiceField.TypeName.Should().Be(type);
			ParseHttpApi("service TestApi { [http(method: get, path: \"/xyzzy/{id}\")] method do { [http(from: path)] id: _; }: {} enum Enum { x } }".Replace("_", type))
				.Methods.Single().PathFields.Single().ServiceField.TypeName.Should().Be(type);

			ParseHttpApi("service TestApi { [http(method: get, path: \"/xyzzy\")] method do { id: _; }: {} data Thing { id: string; } enum Enum { x } }".Replace("_", type))
				.Methods.Single().QueryFields.Single().ServiceField.TypeName.Should().Be(type);
			ParseHttpApi("service TestApi { [http(method: get, path: \"/xyzzy\")] method do { [http(from: query)] id: _; }: {} data Thing { id: string; } enum Enum { x } }".Replace("_", type))
				.Methods.Single().QueryFields.Single().ServiceField.TypeName.Should().Be(type);

			ParseHttpApi("service TestApi { method do { [http(from: header)] xyzzy: _; }: {} data Thing { id: string; } enum Enum { x } }".Replace("_", type))
				.Methods.Single().RequestHeaderFields.Single().ServiceField.TypeName.Should().Be(type);
			ParseHttpApi("service TestApi { method do {}: { [http(from: header)] xyzzy: _; } data Thing { id: string; } enum Enum { x } }".Replace("_", type))
				.Methods.Single().ResponseHeaderFields.Single().ServiceField.TypeName.Should().Be(type);
		}

		[TestCase("bytes")]
		[TestCase("object")]
		[TestCase("error")]
		[TestCase("map<string>")]
		[TestCase("result<string>")]
		[TestCase("string[][]")]
		[TestCase("Thing")]
		public void NonSimpleFieldTypeNotSupported(string type)
		{
			ParseInvalidHttpApi("service TestApi { [http(method: get, path: \"/xyzzy/{id}\")] method do { id: _; }: {} data Thing { id: string; } }".Replace("_", type))
				.ToString().Should().Be("TestApi.fsd(1,72): Type not supported by path field.");
			ParseInvalidHttpApi("service TestApi { [http(method: get, path: \"/xyzzy/{id}\")] method do { [http(from: path)] id: _; }: {} data Thing { id: string; } }".Replace("_", type))
				.ToString().Should().Be("TestApi.fsd(1,91): Type not supported by path field.");

			ParseInvalidHttpApi("service TestApi { [http(method: get, path: \"/xyzzy\")] method do { id: _; }: {} data Thing { id: string; } }".Replace("_", type))
				.ToString().Should().Be("TestApi.fsd(1,67): Type not supported by query field.");
			ParseInvalidHttpApi("service TestApi { [http(method: get, path: \"/xyzzy\")] method do { [http(from: query)] id: _; }: {} data Thing { id: string; } }".Replace("_", type))
				.ToString().Should().Be("TestApi.fsd(1,87): Type not supported by query field.");

			ParseInvalidHttpApi("service TestApi { method do { [http(from: header)] xyzzy: _; }: {} data Thing { id: string; } }".Replace("_", type))
				.ToString().Should().Be("TestApi.fsd(1,52): Type not supported by header request field.");
			ParseInvalidHttpApi("service TestApi { method do {}: { [http(from: header)] xyzzy: _; } data Thing { id: string; } }".Replace("_", type))
				.ToString().Should().Be("TestApi.fsd(1,56): Type not supported by header response field.");
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
		public void ByRouteComparer(string leftHttp, string rightHttp, int expected)
		{
			string fsdText = "service TestApi { [left] method left { id: string; }: {} [right] method right { id: string; }: {} }".Replace("[left]", leftHttp).Replace("[right]", rightHttp);
			var service = HttpServiceInfo.Create(new FsdParser().ParseDefinition(new ServiceDefinitionText("", fsdText)));
			var left = service.Methods.Single(x => x.ServiceMethod.Name == "left");
			var right = service.Methods.Single(x => x.ServiceMethod.Name == "right");
			var actual = HttpMethodInfo.ByRouteComparer.Compare(left, right);
			if (expected < 0)
				actual.Should().BeLessThan(0);
			else
				actual.Should().BeGreaterThan(0);
		}
	}
}
