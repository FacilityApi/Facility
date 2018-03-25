#addin nuget:?package=Cake.Git&version=0.17.0
#addin nuget:?package=Cake.XmlDocMarkdown&version=1.2.1

using System.Text.RegularExpressions;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var nugetApiKey = Argument("nugetApiKey", "");
var trigger = Argument("trigger", "");
var versionSuffix = Argument("versionSuffix", "");

var solutionFileName = "Facility.sln";
var docsAssemblies = new[] { "Facility.Definition", "Facility.CodeGen.Console" };
var docsRepoUri = "https://github.com/FacilityApi/FacilityApi.github.io.git";
var docsRepoBranch = "master";
var docsSourceUri = "https://github.com/FacilityApi/Facility/tree/master/src";

var nugetSource = "https://api.nuget.org/v3/index.json";
var buildBotUserName = "ejball";
var buildBotPassword = EnvironmentVariable("BUILD_BOT_PASSWORD");
var slash = System.IO.Path.DirectorySeparatorChar;

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

Task("NuGetPackage")
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
	.WithCriteria(!string.IsNullOrEmpty(buildBotPassword))
	.IsDependentOn("Build")
	.Does(() =>
	{
		var docsDirectory = new DirectoryPath(docsRepoBranch);
		GitClone(docsRepoUri, docsDirectory, new GitCloneSettings { BranchName = docsRepoBranch });

		var outputPath = $"{docsRepoBranch}{slash}reference";
		var buildBranch = EnvironmentVariable("APPVEYOR_REPO_BRANCH");
		if (buildBranch != "master" || !Regex.IsMatch(trigger, @"^(v[0-9]+\.[0-9]+\.[0-9]+|update-docs)$"))
			outputPath += $"{slash}preview{slash}{buildBranch}";

		Information($"Updating documentation at {outputPath}.");
		foreach (var docsAssembly in docsAssemblies)
		{
			XmlDocMarkdownGenerate(File($"src/{docsAssembly}/bin/{configuration}/net461/{docsAssembly}.dll").ToString(), $"{outputPath}{slash}",
				new XmlDocMarkdownSettings { SourceCodePath = $"{docsSourceUri}/{docsAssembly}", NewLine = "\n", ShouldClean = true });
		}

		if (GitHasUncommitedChanges(docsDirectory))
		{
			Information("Committing all documentation changes.");
			GitAddAll(docsDirectory);
			GitCommit(docsDirectory, "ejball", "ejball@gmail.com", "Automatic documentation update.");
			Information("Pushing updated documentation to GitHub.");
			GitPush(docsDirectory, buildBotUserName, buildBotPassword, docsRepoBranch);
		}
		else
		{
			Information("No documentation changes detected.");
		}
	});

Task("NuGetPublish")
	.IsDependentOn("NuGetPackage")
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

		if (!string.IsNullOrEmpty(nugetApiKey) && (trigger == null || Regex.IsMatch(trigger, "^v[0-9]")))
		{
			if (trigger != null && trigger != $"v{version}")
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

void CodeGen(bool verify)
{
	ExecuteCodeGen($@"{File("example/ExampleApi.fsd")} {Directory("example/output/fsd")}{slash}", verify);
	ExecuteCodeGen($@"{File("example/ExampleApi.fsd")} {Directory("example/output/nowidgets")}{slash} --excludeTag widgets", verify);
	ExecuteCodeGen($@"{File("example/ExampleApi.fsd")} {Directory("example/output/swagger")}{slash} --swagger", verify);
	ExecuteCodeGen($@"{File("example/ExampleApi.fsd")} {Directory("example/output/swagger")}{slash} --swagger --yaml", verify);
	ExecuteCodeGen($@"{File("example/output/swagger/ExampleApi.json")} {Directory("example/output/swagger/fsd")}{slash}", verify);
	ExecuteCodeGen($@"{File("example/output/swagger/ExampleApi.yaml")} {Directory("example/output/swagger/fsd")}{slash}", verify: true);

	foreach (var yamlPath in GetFiles($"example/*.yaml"))
		ExecuteCodeGen($@"{yamlPath} {Directory("example/output/fsd")}{slash}", verify);

	CreateDirectory("example/output/fsd/swagger");
	foreach (var fsdPath in GetFiles($"example/output/fsd/*.fsd"))
		ExecuteCodeGen($@"{fsdPath} {File($"example/output/fsd/swagger/{System.IO.Path.GetFileNameWithoutExtension(fsdPath.FullPath)}.yaml")} --swagger --yaml", verify);
}

void ExecuteCodeGen(string args, bool verify)
{
	string exePath = File($"src/fsdgenfsd/bin/{configuration}/fsdgenfsd.exe");
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
