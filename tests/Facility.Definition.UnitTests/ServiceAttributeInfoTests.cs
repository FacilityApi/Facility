using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public class ServiceAttributeInfoTests
	{
		[Test]
		public void InvalidName()
		{
			new ServiceAttributeInfo(name: "4u").IsValid.Should().BeFalse();
		}
	}
}
