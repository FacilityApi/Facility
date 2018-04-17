using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public class ServiceTypeInfoTests
	{
		[TestCase("string", ServiceTypeKind.String)]
		[TestCase("boolean", ServiceTypeKind.Boolean)]
		[TestCase("double", ServiceTypeKind.Double)]
		[TestCase("int32", ServiceTypeKind.Int32)]
		[TestCase("int64", ServiceTypeKind.Int64)]
		[TestCase("decimal", ServiceTypeKind.Decimal)]
		[TestCase("bytes", ServiceTypeKind.Bytes)]
		[TestCase("object", ServiceTypeKind.Object)]
		[TestCase("error", ServiceTypeKind.Error)]
		public void PrimitiveTypes(string name, ServiceTypeKind kind)
		{
			var service = new ServiceInfo(name: "MyApi", members: new[] { new ServiceDtoInfo("MyDto", fields: new[] { new ServiceFieldInfo("myField", name) }) });
			var type = service.GetFieldType(service.Dtos[0].Fields[0]);
			type.Kind.Should().Be(kind);
			type.Dto.Should().BeNull();
			type.Enum.Should().BeNull();
			type.ValueType.Should().BeNull();
			type.ToString().Should().Be(name);
		}

		[Test]
		public void DtoType()
		{
			var service = new ServiceInfo(name: "MyApi",
				members: new ServiceMemberInfo[] { new ServiceMethodInfo("myMethod", requestFields: new[] { new ServiceFieldInfo("myField", "MyDto") }), new ServiceDtoInfo("MyDto") });
			var type = service.GetFieldType(service.Methods[0].RequestFields[0]);
			type.Kind.Should().Be(ServiceTypeKind.Dto);
			type.Dto.Should().Be(service.Dtos[0]);
			type.Enum.Should().BeNull();
			type.ValueType.Should().BeNull();
			type.ToString().Should().Be("MyDto");
		}

		[Test]
		public void EnumType()
		{
			var service = new ServiceInfo(name: "MyApi",
				members: new ServiceMemberInfo[] { new ServiceMethodInfo("myMethod", requestFields: new[] { new ServiceFieldInfo("myField", "MyEnum") }), new ServiceEnumInfo("MyEnum") });
			var type = service.GetFieldType(service.Methods[0].RequestFields[0]);
			type.Kind.Should().Be(ServiceTypeKind.Enum);
			type.Dto.Should().BeNull();
			type.Enum.Should().Be(service.Enums[0]);
			type.ValueType.Should().BeNull();
			type.ToString().Should().Be("MyEnum");
		}

		[TestCase("result<MyDto>", ServiceTypeKind.Result)]
		[TestCase("MyDto[]", ServiceTypeKind.Array)]
		[TestCase("map<MyDto>", ServiceTypeKind.Map)]
		public void ContainerOfDtoType(string name, ServiceTypeKind kind)
		{
			var service = new ServiceInfo(name: "MyApi",
				members: new ServiceMemberInfo[] { new ServiceMethodInfo("myMethod", requestFields: new[] { new ServiceFieldInfo("myField", name) }), new ServiceDtoInfo("MyDto") });
			var type = service.GetFieldType(service.Methods[0].RequestFields[0]);
			type.Kind.Should().Be(kind);
			type.Dto.Should().BeNull();
			type.Enum.Should().BeNull();
			type.ValueType.Dto.Should().Be(service.Dtos[0]);
			type.ToString().Should().Be(name);
		}
	}
}
