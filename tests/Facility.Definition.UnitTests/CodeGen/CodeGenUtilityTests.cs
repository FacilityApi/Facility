using Facility.Definition.CodeGen;
using Shouldly;
using Xunit;

namespace Facility.Definition.UnitTests.CodeGen
{
	public sealed class CodeGenUtilityTests
	{
		[Fact]
		public void CapitalizeLowerCase()
		{
			CodeGenUtility.Capitalize("xyzzy").ShouldBe("Xyzzy");
		}

		[Fact]
		public void CapitalizeUpperCase()
		{
			CodeGenUtility.Capitalize("Xyzzy").ShouldBe("Xyzzy");
		}

		[Fact]
		public void CapitalizeNumber()
		{
			CodeGenUtility.Capitalize("1234").ShouldBe("1234");
		}
	}
}
