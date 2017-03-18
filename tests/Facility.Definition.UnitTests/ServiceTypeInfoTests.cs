using NUnit.Framework;
using Shouldly;

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
			type.Kind.ShouldBe(kind);
			type.Dto.ShouldBeNull();
			type.Enum.ShouldBeNull();
			type.ValueType.ShouldBeNull();
			type.ToString().ShouldBe(name);
		}

		[Test]
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

		[Test]
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

		[TestCase("result<MyDto>", ServiceTypeKind.Result)]
		[TestCase("MyDto[]", ServiceTypeKind.Array)]
		[TestCase("map<MyDto>", ServiceTypeKind.Map)]
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
