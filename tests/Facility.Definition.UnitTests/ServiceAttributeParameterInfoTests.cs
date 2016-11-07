using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public class ServiceAttributeParameterInfoTests
	{
		[Test]
		public void InvalidNameThrows()
		{
			var position = new NamedTextPosition("source");
			TestUtility.ThrowsServiceDefinitionException(() => new ServiceAttributeParameterInfo(name: "4u", value: "", position: position), position);
		}
	}
}
