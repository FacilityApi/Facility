using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public class ServiceFieldInfoTests
	{
		[Test]
		public void InvalidNameThrows()
		{
			var position = new NamedTextPosition("source", 1, 2);
			TestUtility.ThrowsServiceDefinitionException(() => new ServiceFieldInfo(name: "4u", typeName: "int32", position: position), position);
		}

		[Test]
		public void InvalidTypeNameThrows()
		{
			var position = new NamedTextPosition("source", 1, 2);
			TestUtility.ThrowsServiceDefinitionException(() => new ServiceFieldInfo(name: "x", typeName: "4u", position: position), position);
		}
	}
}
