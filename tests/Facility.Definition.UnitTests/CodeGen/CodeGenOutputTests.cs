using Facility.Definition.CodeGen;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests.CodeGen;

internal sealed class CodeGenOutputTests
{
	[Test]
	public void DuplicateFileName()
	{
		var files = new[] { new CodeGenFile("a.txt", ""), new CodeGenFile("A.txt", "") };
		Action action = () => _ = new CodeGenOutput(files, null);
		action.Should().Throw<ArgumentException>();
	}
}
