using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public class ServiceTextPositionTests
	{
		[Test]
		public void SourceNameOnly()
		{
			var position = new ServiceDefinitionPosition("source");
			position.ToString().Should().Be("source");
		}

		[Test]
		public void LineNumberOnly()
		{
			var position = new ServiceDefinitionPosition("source", 3);
			position.ToString().Should().Be("source(3)");
		}

		[Test]
		public void FullPosition()
		{
			var position = new ServiceDefinitionPosition("source", 3, 14);
			position.ToString().Should().Be("source(3,14)");
		}
	}
}
