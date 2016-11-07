using NUnit.Framework;
using Shouldly;

namespace Facility.Definition.UnitTests
{
	public class ServiceTextPositionTests
	{
		[Test]
		public void SourceNameOnly()
		{
			var position = new NamedTextPosition("source");
			position.ToString().ShouldBe("source");
		}

		[Test]
		public void LineNumberOnly()
		{
			var position = new NamedTextPosition("source", 3);
			position.ToString().ShouldBe("source(3)");
		}

		[Test]
		public void FullPosition()
		{
			var position = new NamedTextPosition("source", 3, 14);
			position.ToString().ShouldBe("source(3,14)");
		}
	}
}
