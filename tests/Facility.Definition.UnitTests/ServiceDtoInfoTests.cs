using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public class ServiceDtoInfoTests
	{
		[Test]
		public void InvalidNameThrows()
		{
			new ServiceDtoInfo(name: "4u").IsValid.Should().BeFalse();
		}

		[Test]
		public void DuplicateFieldThrows()
		{
			var fields = new[]
			{
				new ServiceFieldInfo("why", "int32"),
				new ServiceFieldInfo("Why", "int32"),
			};
			new ServiceDtoInfo(name: "x", fields: fields).IsValid.Should().BeFalse();
		}
	}
}
