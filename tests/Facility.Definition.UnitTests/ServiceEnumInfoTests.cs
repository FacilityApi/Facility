using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public class ServiceEnumInfoTests
	{
		[Test]
		public void InvalidNameThrows()
		{
			var position = new NamedTextPosition("source");
			TestUtility.ThrowsServiceDefinitionException(() => new ServiceEnumInfo(name: "4u", position: position), position);
		}
	}
}
