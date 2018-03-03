using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace Facility.Definition.UnitTests
{
	public class ExcludeTagTests
	{
		[Test]
		public void NoTags()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { it: It; }: { } data It { value: string; } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.FindMember(newService.Methods.Single().RequestFields.Single().TypeName).ShouldBe(newService.Dtos.Single());
		}

		[Test]
		public void ExcludeMethod()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { [tag(name: hidden)] method do { it: It; }: { it: It; } data It { value: string; } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.Methods.ShouldBeEmpty();
		}

		[Test]
		public void ExcludeRequestField()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { [tag(name: hidden)] it: It; }: { it: It; } data It { value: string; } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.Methods.Single().RequestFields.ShouldBeEmpty();
		}

		[Test]
		public void ExcludeResponseField()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { it: It; }: { [tag(name: hidden)] it: It; } data It { value: string; } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.Methods.Single().ResponseFields.ShouldBeEmpty();
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
				exception.Message.ShouldBe("TestApi.fsd(1,35): Unknown field type 'It'. ('hidden' tags are excluded.)");
			}
		}

		[Test]
		public void ExcludeDtoInUseNoThrow()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { it: It; }: { it: It; } [tag(name: hidden)] data It { value: string; } }");
			oldService.TryExcludeTag("hidden", out var _, out var errors).ShouldBeFalse();
			errors.Select(x => x.ToString()).ShouldBe(
				new[]
				{
					"TestApi.fsd(1,35): Unknown field type 'It'. ('hidden' tags are excluded.)",
					"TestApi.fsd(1,48): Unknown field type 'It'. ('hidden' tags are excluded.)",
				});
		}

		[Test]
		public void ExcludeDtoAndUses()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { [tag(name: hidden)] it: It; }: { [tag(name: hidden)] it: It; } [tag(name: hidden)] data It { value: string; } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.Dtos.ShouldBeEmpty();
		}

		[Test]
		public void ExcludeDtoField()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { it: It; }: { it: It; } data It { [tag(name: hidden)] value: string; } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.Dtos.Single().Fields.ShouldBeEmpty();
		}

		[Test]
		public void ExcludeEnumInUse()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { it: It; }: { it: It; } [tag(name: hidden)] enum It { a, b } }");
			oldService.TryExcludeTag("hidden", out var _, out var errors).ShouldBeFalse();
			errors.First().ToString().ShouldBe("TestApi.fsd(1,35): Unknown field type 'It'. ('hidden' tags are excluded.)");
		}

		[Test]
		public void ExcludeEnumAndUses()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { [tag(name: hidden)] it: It; }: { [tag(name: hidden)] it: It; } [tag(name: hidden)] enum It { a, b } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.Enums.ShouldBeEmpty();
		}

		[Test]
		public void ExcludeEnumField()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { method do { it: It; }: { it: It; } enum It { [tag(name: hidden)] a } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.Enums.Single().Values.ShouldBeEmpty();
		}

		[Test]
		public void ExcludeErrorSet()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { [tag(name: hidden)] errors Errors { error } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.ErrorSets.ShouldBeEmpty();
		}

		[Test]
		public void ExcludeError()
		{
			var oldService = TestUtility.ParseTestApi("service TestApi { errors Errors { [tag(name: hidden)] error } }");
			var newService = oldService.ExcludeTag("hidden");
			newService.ErrorSets.Single().Errors.ShouldBeEmpty();
		}
	}
}
