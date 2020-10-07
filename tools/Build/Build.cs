using System;
using System.Linq;
using Faithlife.Build;
using static Faithlife.Build.BuildUtility;
using static Faithlife.Build.DotNetRunner;

internal static class Build
{
	public static int Main(string[] args) => BuildRunner.Execute(args, build =>
	{
		var codegen = "fsdgenfsd";

		var dotNetBuildSettings = new DotNetBuildSettings
		{
			NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY"),
			DocsSettings = new DotNetDocsSettings
			{
				GitLogin = new GitLoginInfo("FacilityApiBot", Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD") ?? ""),
				GitAuthor = new GitAuthorInfo("FacilityApiBot", "facilityapi@gmail.com"),
				SourceCodeUrl = "https://github.com/FacilityApi/Facility/tree/master/src",
				ProjectHasDocs = name => !name.StartsWith("fsdgen", StringComparison.Ordinal),
			},
		};

		build.AddDotNetTargets(dotNetBuildSettings);

		build.Target("codegen")
			.DependsOn("build")
			.Describe("Generates code from the FSD")
			.Does(() => CodeGen(verify: false));

		build.Target("verify-codegen")
			.DependsOn("build")
			.Describe("Ensures the generated code is up-to-date")
			.Does(() => CodeGen(verify: true));

		build.Target("test")
			.DependsOn("verify-codegen");

		void CodeGen(bool verify)
		{
			var configuration = dotNetBuildSettings!.BuildOptions!.ConfigurationOption!.Value;
			var toolPath = FindFiles($"src/{codegen}/bin/{configuration}/netcoreapp3.1/{codegen}.dll").FirstOrDefault();

			var verifyOption = verify ? "--verify" : null;

			RunDotNet(toolPath, "example/ExampleApi.fsd", "example/output", "--newline", "lf", verifyOption);
			RunDotNet(toolPath, "example/ExampleApi.fsd.md", "example/output", "--newline", "lf", "--verify");

			RunDotNet(toolPath, "example/ExampleApi.fsd", "example/output/ExampleApi-nowidgets.fsd", "--excludeTag", "widgets", "--newline", "lf", verifyOption);
			RunDotNet(toolPath, "example/ExampleApi.fsd.md", "example/output/ExampleApi-nowidgets.fsd", "--excludeTag", "widgets", "--newline", "lf", "--verify");
		}
	});
}
