using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public sealed class ValidationTests
	{
		[Test]
		public void DefinedEnumValidation()
		{
			var service = TestUtility.ParseTestApi(@"
service TestApi {
	enum X
	{
		one,
	}

	method do
	{
		[validate]
		one: X;
	}:	{}
}");
			service.Methods.Single().RequestFields.Single().Validation!.IsDefinedEnum.Should().Be(true);
		}
	}
}
