using Xunit;

namespace Facility.Definition.UnitTests
{
	public class ServiceEnumInfoTests
	{
		[Fact]
		public void InvalidNameThrows()
		{
			var position = new ServiceTextPosition("source");
			TestUtility.ThrowsServiceDefinitionException(() => new ServiceEnumInfo(name: "4u", position: position), position);
		}
	}
}
