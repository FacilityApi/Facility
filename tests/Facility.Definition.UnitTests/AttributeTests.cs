using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests;

public sealed class AttributeTests
{
	[Test]
	public void InvalidAttributeName()
	{
		new ServiceAttributeInfo(name: "4u").IsValid.Should().BeFalse();
	}

	[Test]
	public void InvalidParameterName()
	{
		new ServiceAttributeParameterInfo(name: "4u", value: "").IsValid.Should().BeFalse();
	}

	[Test]
	public void NoParameters()
	{
		var service = TestUtility.ParseTestApi("[x] service TestApi{}");

		var attribute = service.Attributes.Single();
		attribute.Name.Should().Be("x");
		attribute.Parameters.Count.Should().Be(0);

		TestUtility.GenerateFsd(service).Should().Equal(
			"// DO NOT EDIT: generated by TestUtility",
			"",
			"[x]",
			"service TestApi",
			"{",
			"}",
			"");
	}

	[Test]
	public void ZeroParameter()
	{
		var service = TestUtility.ParseTestApi("[x(y:0)] service TestApi{}");

		var attribute = service.Attributes.Single();
		attribute.Name.Should().Be("x");
		var parameter = attribute.Parameters.Single();
		parameter.Name.Should().Be("y");
		parameter.Value.Should().Be("0");

		TestUtility.GenerateFsd(service).Should().Equal(
			"// DO NOT EDIT: generated by TestUtility",
			"",
			"[x(y: 0)]",
			"service TestApi",
			"{",
			"}",
			"");
	}

	[Test]
	public void TokenParameter()
	{
		var service = TestUtility.ParseTestApi("[x(y:1b-3D_5f.7H+9J)] service TestApi{}");

		var attribute = service.Attributes.Single();
		attribute.Name.Should().Be("x");
		var parameter = attribute.Parameters.Single();
		parameter.Name.Should().Be("y");
		parameter.Value.Should().Be("1b-3D_5f.7H+9J");

		TestUtility.GenerateFsd(service).Should().Equal(
			"// DO NOT EDIT: generated by TestUtility",
			"",
			"[x(y: 1b-3D_5f.7H+9J)]",
			"service TestApi",
			"{",
			"}",
			"");
	}

	[Test]
	public void EmptyStringParameter()
	{
		var service = TestUtility.ParseTestApi("[x(y:\"\")] service TestApi{}");

		var attribute = service.Attributes.Single();
		attribute.Name.Should().Be("x");
		var parameter = attribute.Parameters.Single();
		parameter.Name.Should().Be("y");
		parameter.Value.Should().Be("");

		TestUtility.GenerateFsd(service).Should().Equal(
			"// DO NOT EDIT: generated by TestUtility",
			"",
			"[x(y: \"\")]",
			"service TestApi",
			"{",
			"}",
			"");
	}

	[Test]
	public void JsonStringParameter()
	{
		var service = TestUtility.ParseTestApi(@"[x(y:""á\\\""\/\b\f\n\r\t\u0001\u1234!"")] service TestApi{}");

		var attribute = service.Attributes.Single();
		attribute.Name.Should().Be("x");
		var parameter = attribute.Parameters.Single();
		parameter.Name.Should().Be("y");
		parameter.Value.Should().Be("á\\\"/\b\f\n\r\t\u0001\u1234!");

		TestUtility.GenerateFsd(service).Should().Equal(
			"// DO NOT EDIT: generated by TestUtility",
			"",
			"[x(y: \"á\\\\\\\"/\\b\\f\\n\\r\\t\\u0001\u1234!\")]",
			"service TestApi",
			"{",
			"}",
			"");
	}

	[Test]
	public void ManyAttributesAndParameters()
	{
		var service = TestUtility.ParseTestApi("[x, x(x:0)] [x(x:0,y:1)] service TestApi{}");

		service.Attributes.Count.Should().Be(3);

		TestUtility.GenerateFsd(service).Should().Equal(
			"// DO NOT EDIT: generated by TestUtility",
			"",
			"[x]",
			"[x(x: 0)]",
			"[x(x: 0, y: 1)]",
			"service TestApi",
			"{",
			"}",
			"");
	}

	[Test]
	public void ObsoleteEverything()
	{
		var service = TestUtility.ParseTestApi("[obsolete] service TestApi {" +
			"[obsolete] method myMethod { [obsolete] in: string; }: { [obsolete] out: string; }" +
			"[obsolete] data MyData { [obsolete] field: string; }" +
			"[obsolete] enum MyEnum { [obsolete] x }" +
			"[obsolete] errors MyErrors { [obsolete] x }" +
			"}");
		service.GetElementAndDescendants().OfType<ServiceElementWithAttributesInfo>().All(x => x.IsObsolete).Should().BeTrue();
	}

	[Test]
	public void ObsoleteMessage()
	{
		var service = TestUtility.ParseTestApi("[obsolete] service TestApi { [obsolete(message: hey)] data MyData {} }");
		service.ObsoleteMessage.Should().BeNull();
		service.Dtos.Single().ObsoleteMessage.Should().Be("hey");
	}

	[Test]
	public void TwoObsoletes()
	{
		TestUtility.ParseInvalidTestApi("[obsolete] [obsolete] service TestApi {}");
	}

	[Test]
	public void BadObsoleteParameter()
	{
		TestUtility.ParseInvalidTestApi("[obsolete(name: hey)] service TestApi {}");
	}

	[Test]
	public void RequiredFields()
	{
		var service = TestUtility.ParseTestApi("service TestApi {" +
			"method myMethod { [required] in: string; }: { [required] out: string; }" +
			"data MyData { [required] field: string; }" +
			"enum MyEnum { x }" +
			"errors MyErrors { x }" +
			"}");
		service.GetElementAndDescendants().OfType<ServiceFieldInfo>().All(x => x.IsRequired).Should().BeTrue();
	}

	[Test]
	public void TwoRequireds()
	{
		TestUtility.ParseInvalidTestApi("service TestApi data MyData { [required] [required] field: string; }");
	}

	[Test]
	public void BadRequiredParameter()
	{
		TestUtility.ParseInvalidTestApi("service TestApi data MyData { [required(hey: you)] field: string; }");
	}

	[Test]
	public void TagEverything()
	{
		var service = TestUtility.ParseTestApi("[tag(name: hey)] service TestApi {" +
			"[tag(name: hey)] method myMethod { [tag(name: hey)] in: string; }: { [tag(name: hey)] out: string; }" +
			"[tag(name: hey)] data MyData { [tag(name: hey)] field: string; }" +
			"[tag(name: hey)] enum MyEnum { [tag(name: hey)] x }" +
			"[tag(name: hey)] errors MyErrors { [tag(name: hey)] x }" +
			"}");
		service.GetElementAndDescendants().OfType<ServiceElementWithAttributesInfo>().All(x => x.TagNames.Single() == "hey").Should().BeTrue();
	}

	[Test]
	public void TwoTags()
	{
		var service = TestUtility.ParseTestApi("[tag(name: hey), tag(name: you)] service TestApi {}");
		service.TagNames.Should().BeEquivalentTo("hey", "you");
	}

	[Test]
	public void TagNoName()
	{
		TestUtility.ParseInvalidTestApi("[tag] service TestApi {}");
	}

	[Test]
	public void TagBadParameter()
	{
		TestUtility.ParseInvalidTestApi("[tag(names: hey)] service TestApi {}");
	}
}
