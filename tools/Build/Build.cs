using System;
using System.IO;
using System.Linq;
using Faithlife.Build;
using static Faithlife.Build.BuildUtility;
using static Faithlife.Build.DotNetRunner;

internal static class Build
{
	public static int Main(string[] args) => BuildRunner.Execute(args, build =>
	{
		var codegen = "fsdgen___";

		var dotNetTools = new DotNetTools(Path.Combine("tools", "bin")).AddSource(Path.Combine("tools", "bin"));

		var dotNetBuildSettings = new DotNetBuildSettings
		{
			NuGetApiKey = Environment.GetEnvironmentVariable("NUGET_API_KEY"),
			DocsSettings = new DotNetDocsSettings
			{
				GitLogin = new GitLoginInfo("FacilityApiBot", Environment.GetEnvironmentVariable("BUILD_BOT_PASSWORD") ?? ""),
				GitAuthor = new GitAuthorInfo("FacilityApiBot", "facilityapi@gmail.com"),
				GitBranchName = Environment.GetEnvironmentVariable("APPVEYOR_REPO_BRANCH"),
				SourceCodeUrl = "https://github.com/FacilityApi/RepoName/tree/master/src",
				ProjectHasDocs = name => !name.StartsWith("fsdgen", StringComparison.Ordinal),
			},
			DotNetTools = dotNetTools,
		};

		build.AddDotNetTargets(dotNetBuildSettings);

		build.Target("codegen")
			.DependsOn("build")
			.Describe("Generates code from the FSD")
			.Does(() => codeGen(verify: false));

		build.Target("verify-codegen")
			.DependsOn("build")
			.Describe("Ensures the generated code is up-to-date")
			.Does(() => codeGen(verify: true));

		build.Target("test")
			.DependsOn("verify-codegen");

		void codeGen(bool verify)
		{
			var configuration = dotNetBuildSettings.BuildOptions!.ConfigurationOption!.Value;
			var toolPath = FindFiles($"src/{codegen}/bin/{configuration}/netcoreapp*/{codegen}.dll").FirstOrDefault();

			var verifyOption = verify ? "--verify" : null;

			RunDotNet(toolPath, "___", "___", "--newline", "lf", verifyOption);
		}
	});
}
