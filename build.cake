#addin nuget:?package=Cake.Git&version=0.17.0
#addin nuget:?package=Cake.XmlDocMarkdown&version=1.4.0

using System.Text.RegularExpressions;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var versionSuffix = Argument("versionSuffix", "");

var solutionFileName = "Facility.sln";
var docsAssemblies = new[] { "Facility.Definition", "Facility.CodeGen.Console" };
var docsAssemblyDirectory = $"src/fsdgenfsd/bin/{configuration}";
var docsSourceUri = "https://github.com/FacilityApi/Facility/tree/master/src";
var codeGenExe = $"src/fsdgenfsd/bin/{configuration}/fsdgenfsd.exe";

var nugetSource = "https://api.nuget.org/v3/index.json";
var nugetApiKey = EnvironmentVariable("NUGET_API_KEY");
var docsRepoUri = "https://github.com/FacilityApi/FacilityApi.github.io.git";
var docsRepoBranch = "master";
var buildBotUserName = "FacilityApiBot";
var buildBotEmail = "facilityapi@gmail.com";
var buildBotPassword = EnvironmentVariable("BUILD_BOT_PASSWORD");
var trigger = EnvironmentVariable("APPVEYOR_REPO_TAG_NAME");;
var buildBranch = EnvironmentVariable("APPVEYOR_REPO_BRANCH");

void CodeGen(bool verify)
{
	ExecuteCodeGen("example/ExampleApi.fsd example/output/fsd", verify);
	ExecuteCodeGen("example/ExampleApi.fsd example/output/nowidgets --excludeTag widgets", verify);
	ExecuteCodeGen("example/ExampleApi.fsd example/output/swagger --swagger", verify);
	ExecuteCodeGen("example/ExampleApi.fsd example/output/swagger --swagger --yaml", verify);
	ExecuteCodeGen("example/output/swagger/ExampleApi.json example/output/swagger/fsd", verify);
	ExecuteCodeGen("example/output/swagger/ExampleApi.yaml example/output/swagger/fsd", verify: true);

	foreach (var yamlPath in GetFiles($"example/*.yaml"))
		ExecuteCodeGen($"{yamlPath} example/output/fsd", verify);

	CreateDirectory("example/output/fsd/swagger");
	foreach (var fsdPath in GetFiles("example/output/fsd/*.fsd"))
		ExecuteCodeGen($"{fsdPath} example/output/fsd/swagger/{System.IO.Path.GetFileNameWithoutExtension(fsdPath.FullPath)}.yaml --swagger --yaml", verify);
}

Task("Clean")
	.Does(() =>
	{
		CleanDirectories("src/**/bin");
		CleanDirectories("src/**/obj");
		CleanDirectories("tests/**/bin");
		CleanDirectories("tests/**/obj");
		CleanDirectories("release");
	});

Task("Build")
	.Does(() =>
	{
		DotNetCoreRestore(solutionFileName);
		DotNetCoreBuild(solutionFileName, new DotNetCoreBuildSettings { Configuration = configuration, ArgumentCustomization = args => args.Append("--verbosity normal") });
	});

Task("Rebuild")
	.IsDependentOn("Clean")
	.IsDependentOn("Build");

Task("CodeGen")
	.IsDependentOn("Build")
	.Does(() => CodeGen(verify: false));

Task("VerifyCodeGen")
	.IsDependentOn("Build")
	.Does(() => CodeGen(verify: true));

Task("Test")
	.IsDependentOn("VerifyCodeGen")
	.Does(() =>
	{
		foreach (var projectPath in GetFiles("tests/**/*.csproj").Select(x => x.FullPath))
			DotNetCoreTest(projectPath, new DotNetCoreTestSettings { Configuration = configuration });
	});

Task("Package")
	.IsDependentOn("Rebuild")
	.IsDependentOn("Test")
	.Does(() =>
	{
		if (string.IsNullOrEmpty(versionSuffix) && !string.IsNullOrEmpty(trigger))
			versionSuffix = Regex.Match(trigger, @"^v[^\.]+\.[^\.]+\.[^\.]+-(.+)").Groups[1].ToString();

		foreach (var projectPath in GetFiles("src/**/*.csproj").Select(x => x.FullPath))
			DotNetCorePack(projectPath, new DotNetCorePackSettings { Configuration = configuration, OutputDirectory = "release", VersionSuffix = versionSuffix });
	});

Task("UpdateDocs")
	.IsDependentOn("Build")
	.Does(() =>
	{
		bool shouldCommit = !string.IsNullOrEmpty(buildBotPassword);
		var docsDirectory = Directory($"release/{docsRepoBranch}");
		if (shouldCommit)
			GitClone(docsRepoUri, docsDirectory, new GitCloneSettings { BranchName = docsRepoBranch });

		var outputPath = $"release/{docsRepoBranch}/reference";
		var isPreview = buildBranch != "master" || trigger == null || !Regex.IsMatch(trigger, @"^(v[0-9]+\.[0-9]+\.[0-9]+|update-docs)$");
		if (isPreview)
			outputPath += $"/preview/{buildBranch}";

		Information($"Updating documentation at {outputPath}.");
		foreach (var docsAssembly in docsAssemblies)
		{
			XmlDocMarkdownGenerate($"{docsAssemblyDirectory}/{docsAssembly}.dll", $"{outputPath}/",
				new XmlDocMarkdownSettings { SourceCodePath = $"{docsSourceUri}/{docsAssembly}", NewLine = "\n", ShouldClean = true });
		}

		if (!shouldCommit)
		{
			Information("Set BUILD_BOT_PASSWORD to publish documentation changes.");
		}
		else if (GitHasUncommitedChanges(docsDirectory))
		{
			Information("Committing all documentation changes.");
			GitAddAll(docsDirectory);
			GitCommit(docsDirectory, buildBotUserName, buildBotEmail,
				"Automatic documentation update." + (isPreview ? $" (preview {buildBranch})" : ""));
			Information("Pushing updated documentation to GitHub.");
			GitPush(docsDirectory, buildBotUserName, buildBotPassword, docsRepoBranch);
		}
		else
		{
			Information("No documentation changes detected.");
		}
	});

Task("Publish")
	.IsDependentOn("Package")
	.IsDependentOn("UpdateDocs")
	.Does(() =>
	{
		var nupkgPaths = GetFiles("release/*.nupkg").Select(x => x.FullPath).ToList();

		string version = null;
		foreach (var nupkgPath in nupkgPaths)
		{
			string nupkgVersion = Regex.Match(nupkgPath, @"\.([^\.]+\.[^\.]+\.[^\.]+)\.nupkg$").Groups[1].ToString();
			if (version == null)
				version = nupkgVersion;
			else if (version != nupkgVersion)
				throw new InvalidOperationException($"Mismatched package versions '{version}' and '{nupkgVersion}'.");
		}

		if (!string.IsNullOrEmpty(nugetApiKey) && trigger != null && Regex.IsMatch(trigger, "^v[0-9]"))
		{
			if (trigger != $"v{version}")
				throw new InvalidOperationException($"Trigger '{trigger}' doesn't match package version '{version}'.");

			var pushSettings = new NuGetPushSettings { ApiKey = nugetApiKey, Source = nugetSource };
			foreach (var nupkgPath in nupkgPaths)
				NuGetPush(nupkgPath, pushSettings);
		}
		else
		{
			Information("To publish NuGet packages, push this git tag: v" + version);
		}
	});

Task("Default")
	.IsDependentOn("Test");

void ExecuteCodeGen(string args, bool verify)
{
	Information(args);
	string exePath = codeGenExe;
	if (IsRunningOnUnix())
	{
		args = exePath + " " + args;
		exePath = "mono";
	}
	int exitCode = StartProcess(exePath, args + " --newline lf" + (verify ? " --verify" : ""));
	if (exitCode == 1 && verify)
		throw new InvalidOperationException("Generated code doesn't match; use --target=CodeGen to regenerate.");
	else if (exitCode != 0)
		throw new InvalidOperationException($"Code generation failed with exit code {exitCode}.");
}

RunTarget(target);
