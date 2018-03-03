using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public class ServiceDtoInfoTests
	{
		[Test]
		public void InvalidNameThrows()
		{
			var position = new NamedTextPosition("source");
			TestUtility.ThrowsServiceDefinitionException(() => new ServiceDtoInfo(name: "4u", position: position), position);
		}

		[Test]
		public void DuplicateFieldThrows()
		{
			var fields = new[]
			{
				new ServiceFieldInfo("why", "int32", position: new NamedTextPosition("source", 1)),
				new ServiceFieldInfo("Why", "int32", position: new NamedTextPosition("source", 2)),
			};
			TestUtility.ThrowsServiceDefinitionException(() => new ServiceDtoInfo(name: "x", fields: fields), fields[1].Position);
		}
	}
}
