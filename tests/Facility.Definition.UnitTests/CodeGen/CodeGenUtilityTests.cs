using Facility.Definition.CodeGen;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests.CodeGen
{
	public sealed class CodeGenUtilityTests
	{
		[Test]
		public void CapitalizeLowerCase()
		{
			CodeGenUtility.Capitalize("xyzzy").Should().Be("Xyzzy");
		}

		[Test]
		public void CapitalizeUpperCase()
		{
			CodeGenUtility.Capitalize("Xyzzy").Should().Be("Xyzzy");
		}

		[Test]
		public void CapitalizeNumber()
		{
			CodeGenUtility.Capitalize("1234").Should().Be("1234");
		}
	}
}
