using Xunit;

namespace Facility.Definition.UnitTests
{
	public class ServiceAttributeParameterInfoTests
	{
		[Fact]
		public void InvalidNameThrows()
		{
			var position = new ServiceTextPosition("source");
			TestUtility.ThrowsServiceDefinitionException(() => new ServiceAttributeParameterInfo(name: "4u", value: "", position: position), position);
		}
	}
}
