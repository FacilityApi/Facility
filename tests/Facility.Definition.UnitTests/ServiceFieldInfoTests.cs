using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public class ServiceFieldInfoTests
	{
		[Test]
		public void InvalidNameThrows()
		{
			new ServiceFieldInfo(name: "4u", typeName: "int32").IsValid.Should().BeFalse();
		}
	}
}
