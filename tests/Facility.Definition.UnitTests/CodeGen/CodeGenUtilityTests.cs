using Facility.Definition.CodeGen;
using NUnit.Framework;
using Shouldly;

namespace Facility.Definition.UnitTests.CodeGen
{
	public sealed class CodeGenUtilityTests
	{
		[Test]
		public void CapitalizeLowerCase()
		{
			CodeGenUtility.Capitalize("xyzzy").ShouldBe("Xyzzy");
		}

		[Test]
		public void CapitalizeUpperCase()
		{
			CodeGenUtility.Capitalize("Xyzzy").ShouldBe("Xyzzy");
		}

		[Test]
		public void CapitalizeNumber()
		{
			CodeGenUtility.Capitalize("1234").ShouldBe("1234");
		}
	}
}
