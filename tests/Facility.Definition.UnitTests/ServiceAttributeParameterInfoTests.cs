using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public class ServiceAttributeParameterInfoTests
	{
		[Test]
		public void InvalidNameThrows()
		{
			new ServiceAttributeParameterInfo(name: "4u", value: "").IsValid.Should().BeFalse();
		}
	}
}
