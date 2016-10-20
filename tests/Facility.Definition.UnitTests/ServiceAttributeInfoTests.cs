using Xunit;

namespace Facility.Definition.UnitTests
{
	public class ServiceAttributeInfoTests
	{
		[Fact]
		public void InvalidNameThrows()
		{
			var position = new ServiceTextPosition("source");
			TestUtility.ThrowsServiceDefinitionException(() => new ServiceAttributeInfo(name: "4u", position: position), position);
		}
	}
}
