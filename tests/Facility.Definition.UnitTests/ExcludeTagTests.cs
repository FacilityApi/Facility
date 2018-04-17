using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Facility.Definition.UnitTests
{
	public class ExcludeTagTests
	{
		[Test]
		public void NoTags()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { it: It; }: { } data It { value: string; } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.FindMember(newService.Methods.Single().RequestFields.Single().TypeName).Should().Be(newService.Dtos.Single());
		}

		[Test]
		public void ExcludeMethod()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { [tag(name: hidden)] method do { it: It; }: { it: It; } data It { value: string; } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.Methods.Should().BeEmpty();
		}

		[Test]
		public void ExcludeRequestField()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { [tag(name: hidden)] it: It; }: { it: It; } data It { value: string; } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.Methods.Single().RequestFields.Should().BeEmpty();
		}

		[Test]
		public void ExcludeResponseField()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { it: It; }: { [tag(name: hidden)] it: It; } data It { value: string; } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.Methods.Single().ResponseFields.Should().BeEmpty();
		}

		[Test]
		public void ExcludeDtoInUseThrow()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { it: It; }: { it: It; } [tag(name: hidden)] data It { value: string; } }");
			try
			{
				oldService.ExcludeTag("hidden");
				Assert.Fail("didn't throw");
			}
			catch (ServiceDefinitionException exception)
			{
				exception.Message.Should().Be("TestApi.fsd(1,35): Unknown field type 'It'. ('hidden' tags are excluded.)");
			}
		}

		[Test]
		public void ExcludeDtoInUseNoThrow()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { it: It; }: { it: It; } [tag(name: hidden)] data It { value: string; } }");
			oldService.TryExcludeTag("hidden", out var _, out var errors).Should().BeFalse();
			errors.Select(x => x.ToString()).Should().Equal(
				"TestApi.fsd(1,35): Unknown field type 'It'. ('hidden' tags are excluded.)",
				"TestApi.fsd(1,48): Unknown field type 'It'. ('hidden' tags are excluded.)");
		}

		[Test]
		public void ExcludeDtoAndUses()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { [tag(name: hidden)] it: It; }: { [tag(name: hidden)] it: It; } [tag(name: hidden)] data It { value: string; } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.Dtos.Should().BeEmpty();
		}

		[Test]
		public void ExcludeDtoField()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { it: It; }: { it: It; } data It { [tag(name: hidden)] value: string; } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.Dtos.Single().Fields.Should().BeEmpty();
		}

		[Test]
		public void ExcludeEnumInUse()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { it: It; }: { it: It; } [tag(name: hidden)] enum It { a, b } }");
			oldService.TryExcludeTag("hidden", out var _, out var errors).Should().BeFalse();
			errors.First().ToString().Should().Be("TestApi.fsd(1,35): Unknown field type 'It'. ('hidden' tags are excluded.)");
		}

		[Test]
		public void ExcludeEnumAndUses()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { [tag(name: hidden)] it: It; }: { [tag(name: hidden)] it: It; } [tag(name: hidden)] enum It { a, b } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.Enums.Should().BeEmpty();
		}

		[Test]
		public void ExcludeEnumField()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { it: It; }: { it: It; } enum It { [tag(name: hidden)] a } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.Enums.Single().Values.Should().BeEmpty();
		}

		[Test]
		public void ExcludeErrorSet()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { [tag(name: hidden)] errors Errors { error } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.ErrorSets.Should().BeEmpty();
		}

		[Test]
		public void ExcludeError()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { errors Errors { [tag(name: hidden)] error } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.ErrorSets.Single().Errors.Should().BeEmpty();
		}
	}
}
