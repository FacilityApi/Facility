using Shouldly;
using Xunit;

namespace Facility.Definition.UnitTests
{
	public class ServiceTypeInfoTests
	{
		[Theory]
		[InlineData("string", ServiceTypeKind.String)]
		[InlineData("boolean", ServiceTypeKind.Boolean)]
		[InlineData("double", ServiceTypeKind.Double)]
		[InlineData("int32", ServiceTypeKind.Int32)]
		[InlineData("int64", ServiceTypeKind.Int64)]
		[InlineData("bytes", ServiceTypeKind.Bytes)]
		[InlineData("object", ServiceTypeKind.Object)]
		[InlineData("error", ServiceTypeKind.Error)]
		public void PrimitiveTypes(string name, ServiceTypeKind kind)
		{
			var service = new ServiceInfo(name: "MyApi", members: new[] { new ServiceDtoInfo("MyDto", fields: new[] { new ServiceFieldInfo("myField", name) }) });
			var type = service.GetFieldType(service.Dtos[0].Fields[0]);
			type.Kind.ShouldBe(kind);
			type.Dto.ShouldBeNull();
			type.Enum.ShouldBeNull();
			type.ValueType.ShouldBeNull();
			type.ToString().ShouldBe(name);
		}

		[Fact]
		public void DtoType()
		{
			var service = new ServiceInfo(name: "MyApi",
				members: new IServiceMemberInfo[] { new ServiceMethodInfo("myMethod", requestFields: new[] { new ServiceFieldInfo("myField", "MyDto") }), new ServiceDtoInfo("MyDto") });
			var type = service.GetFieldType(service.Methods[0].RequestFields[0]);
			type.Kind.ShouldBe(ServiceTypeKind.Dto);
			type.Dto.ShouldBe(service.Dtos[0]);
			type.Enum.ShouldBeNull();
			type.ValueType.ShouldBeNull();
			type.ToString().ShouldBe("MyDto");
		}

		[Fact]
		public void EnumType()
		{
			var service = new ServiceInfo(name: "MyApi",
				members: new IServiceMemberInfo[] { new ServiceMethodInfo("myMethod", requestFields: new[] { new ServiceFieldInfo("myField", "MyEnum") }), new ServiceEnumInfo("MyEnum") });
			var type = service.GetFieldType(service.Methods[0].RequestFields[0]);
			type.Kind.ShouldBe(ServiceTypeKind.Enum);
			type.Dto.ShouldBeNull();
			type.Enum.ShouldBe(service.Enums[0]);
			type.ValueType.ShouldBeNull();
			type.ToString().ShouldBe("MyEnum");
		}

		[Theory]
		[InlineData("result<MyDto>", ServiceTypeKind.Result)]
		[InlineData("MyDto[]", ServiceTypeKind.Array)]
		[InlineData("map<MyDto>", ServiceTypeKind.Map)]
		public void ContainerOfDtoType(string name, ServiceTypeKind kind)
		{
			var service = new ServiceInfo(name: "MyApi",
				members: new IServiceMemberInfo[] { new ServiceMethodInfo("myMethod", requestFields: new[] { new ServiceFieldInfo("myField", name) }), new ServiceDtoInfo("MyDto") });
			var type = service.GetFieldType(service.Methods[0].RequestFields[0]);
			type.Kind.ShouldBe(kind);
			type.Dto.ShouldBeNull();
			type.Enum.ShouldBeNull();
			type.ValueType.Dto.ShouldBe(service.Dtos[0]);
			type.ToString().ShouldBe(name);
		}
	}
}
