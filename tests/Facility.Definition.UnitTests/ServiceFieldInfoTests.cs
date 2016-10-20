using Xunit;

namespace Facility.Definition.UnitTests
{
	public class ServiceFieldInfoTests
	{
		[Fact]
		public void InvalidNameThrows()
		{
			var position = new ServiceTextPosition("source", 1, 2);
			TestUtility.ThrowsServiceDefinitionException(() => new ServiceFieldInfo(name: "4u", typeName: "int32", position: position), position);
		}

		[Fact]
		public void InvalidTypeNameThrows()
		{
			var position = new ServiceTextPosition("source", 1, 2);
			TestUtility.ThrowsServiceDefinitionException(() => new ServiceFieldInfo(name: "x", typeName: "4u", position: position), position);
		}
	}
}
