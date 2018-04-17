using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public class ServiceEnumInfoTests
	{
		[Test]
		public void InvalidNameThrows()
		{
			new ServiceEnumInfo(name: "4u").IsValid.Should().BeFalse();
		}
	}
}
