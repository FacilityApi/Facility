using Facility.Definition.CodeGen;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests.CodeGen
{
	public sealed class CodeGenUtilityTests
	{
		[TestCase("xyZzy", "XyZzy")]
		[TestCase("XyZzy", "XyZzy")]
		[TestCase("1234", "1234")]
		public void Capitalize(string before, string after)
		{
			CodeGenUtility.Capitalize(before).Should().Be(after);
		}

		[TestCase("xyZzy", "xyZzy")]
		[TestCase("XyZzy", "xyZzy")]
		[TestCase("1234", "1234")]
		public void Uncapitalize(string before, string after)
		{
			CodeGenUtility.Uncapitalize(before).Should().Be(after);
		}

		[TestCase("xyZzy", "xyZzy")]
		[TestCase("XyZzy", "xyZzy")]
		[TestCase("1234", "1234")]
		[TestCase("xy_zzy", "xyZzy")]
		[TestCase("me2you", "me2You")]
		[TestCase("IOStream", "ioStream")]
		public void CamelCase(string before, string after)
		{
			CodeGenUtility.ToCamelCase(before).Should().Be(after);
		}

		[TestCase("xyZzy", "XyZzy")]
		[TestCase("XyZzy", "XyZzy")]
		[TestCase("1234", "1234")]
		[TestCase("xy_zzy", "XyZzy")]
		[TestCase("me2you", "Me2You")]
		[TestCase("IOStream", "IOStream")]
		public void PascalCase(string before, string after)
		{
			CodeGenUtility.ToPascalCase(before).Should().Be(after);
		}

		[TestCase("xyZzy", "xy_zzy")]
		[TestCase("XyZzy", "xy_zzy")]
		[TestCase("1234", "1234")]
		[TestCase("xy_zzy", "xy_zzy")]
		[TestCase("me2you", "me_2_you")]
		[TestCase("IOStream", "io_stream")]
		public void SnakeCase(string before, string after)
		{
			CodeGenUtility.ToSnakeCase(before).Should().Be(after);
		}
	}
}
