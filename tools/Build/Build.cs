using System;
using Faithlife.Build;
using static Faithlife.Build.AppRunner;

internal static class Build
{
	public static int Main(string[] args) => BuildRunner.Execute(args, build =>
	{
		var dotNetBuildSettings = new DotNetBuildSettings
		{
			DocsSettings = new DotNetDocsSettings
			{
				GitLogin = new GitLoginInfo("FacilityApiBot", Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD") ?? ""),
				GitAuthor = new GitAuthorInfo("FacilityApiBot", "facilityapi@gmail.com"),
				SourceCodeUrl = "https://github.com/FacilityApi/Facility/tree/master/src",
			},
		};

		build.AddDotNetTargets(dotNetBuildSettings);

		void codeGen(bool verify)
		{
			string configuration = dotNetBuildSettings.BuildOptions.ConfigurationOption.Value;
			string consoleAppPath = $"src/fsdgenfsd/bin/{configuration}/fsdgenfsd.exe";
			string verifyOption = verify ? "--verify" : null;

			RunDotNetFrameworkApp(consoleAppPath, "example/ExampleApi.fsd", "example/output", verifyOption);
			RunDotNetFrameworkApp(consoleAppPath, "example/ExampleApi.fsd", "example/output/ExampleApi-nowidgets.fsd", "--excludeTag", "widgets", verifyOption);

			if (verify)
			{
				RunDotNetFrameworkApp(consoleAppPath, "example/ExampleApi.fsd.md", "example/output", verifyOption);
				RunDotNetFrameworkApp(consoleAppPath, "example/ExampleApi.fsd.md", "example/output/ExampleApi-nowidgets.fsd", "--excludeTag", "widgets", verifyOption);
			}
		}

		build.Target("codegen")
			.DependsOn("build")
			.Does(() => codeGen(verify: false));

		build.Target("verify-codegen")
			.DependsOn("build")
			.Does(() => codeGen(verify: true));

		build.Target("test")
			.DependsOn("verify-codegen");

		build.Target("default")
			.DependsOn("build");
	});
}
