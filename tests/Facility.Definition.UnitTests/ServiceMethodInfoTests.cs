using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public class ServiceMethodInfoTests
	{
		[Test]
		public void InvalidNameThrows()
		{
			new ServiceMethodInfo(name: "4u").IsValid.Should().BeFalse();
		}

		[TestCase(true), TestCase(false)]
		public void DuplicateFieldThrows(bool isRequest)
		{
			var fields = new[]
			{
				new ServiceFieldInfo("why", "int32"),
				new ServiceFieldInfo("Why", "int32"),
			};
			new ServiceMethodInfo(name: "x", requestFields: isRequest ? fields : null, responseFields: isRequest ? null : fields).IsValid.Should().BeFalse();
		}
	}
}
