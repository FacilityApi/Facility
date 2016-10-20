using Shouldly;
using Xunit;

namespace Facility.Definition.UnitTests
{
	public class ServiceTextPositionTests
	{
		[Fact]
		public void SourceNameOnly()
		{
			var position = new ServiceTextPosition("source");
			position.ToString().ShouldBe("source");
		}

		[Fact]
		public void LineNumberOnly()
		{
			var position = new ServiceTextPosition("source", 3);
			position.ToString().ShouldBe("source(3)");
		}

		[Fact]
		public void FullPosition()
		{
			var position = new ServiceTextPosition("source", 3, 14);
			position.ToString().ShouldBe("source(3,14)");
		}
	}
}
