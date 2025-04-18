using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests;

internal sealed class ErrorSetTests
{
	[Test]
	public void OneMinimalErrorSet()
	{
		var service = TestUtility.ParseTestApi("service TestApi { errors One { X } }");

		var errorSet = service.ErrorSets.Single();
		errorSet.Name.Should().Be("One");
		errorSet.Attributes.Count.Should().Be(0);
		errorSet.Summary.Should().Be("");
		errorSet.Remarks.Count.Should().Be(0);
		var error = errorSet.Errors.Single();
		error.Name.Should().Be("X");
		error.Attributes.Count.Should().Be(0);
		error.Summary.Should().Be("");

		TestUtility.GenerateFsd(service).Should().Equal(
			"// DO NOT EDIT: generated by TestUtility",
			"",
			"service TestApi",
			"{",
			"\terrors One",
			"\t{",
			"\t\tX,",
			"\t}",
			"}",
			"");
	}
}
