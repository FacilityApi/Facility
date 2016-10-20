#addin "nuget:?package=Cake.Git"
#addin "nuget:?package=Octokit"
#tool "nuget:?package=coveralls.io"
#tool "nuget:?package=gitlink"
#tool "nuget:?package=OpenCover"
#tool "nuget:?package=ReportGenerator"
#tool "nuget:?package=xunit.runner.console"

using LibGit2Sharp;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var nugetApiKey = Argument("nugetApiKey", "");
var githubApiKey = Argument("githubApiKey", "");
var coverallsApiKey = Argument("coverallsApiKey", "");

var solutionFileName = "Facility.sln";
var assemblyFileName = "src/Facility.Definition/bin/Release/Facility.Definition.dll";
var githubOwner = "FacilityApi";
var githubRepo = "Facility";
var githubRawUri = "http://raw.githubusercontent.com";
var nugetSource = "https://www.nuget.org/api/v2/package";

var gitRepository = new LibGit2Sharp.Repository(MakeAbsolute(Directory(".")).FullPath);

var githubClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("build.cake"));
if (!string.IsNullOrEmpty(githubApiKey))
	githubClient.Credentials = new Octokit.Credentials(githubApiKey);

string headSha = null;
string version = null;

string GetSemVerFromFile(string path)
{
	var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(path);
	return $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}";
}

Task("Clean")
	.Does(() =>
	{
		CleanDirectories($"src/**/bin");
		CleanDirectories($"src/**/obj");
		CleanDirectories($"tests/**/bin");
		CleanDirectories($"tests/**/obj");
		CleanDirectories("release");
	});

Task("Build")
	.IsDependentOn("Clean")
	.Does(() =>
	{
		NuGetRestore(solutionFileName);
		MSBuild(solutionFileName, settings => settings.SetConfiguration(configuration));
	});

Task("Test")
	.IsDependentOn("Build")
	.Does(() => XUnit2($"tests/**/bin/**/*.UnitTests.dll"));

Task("SourceIndex")
	.IsDependentOn("Test")
	.WithCriteria(() => configuration == "Release")
	.Does(() =>
	{
		var dirtyEntry = gitRepository.RetrieveStatus().FirstOrDefault(x => x.State != FileStatus.Unaltered && x.State != FileStatus.Ignored);
		if (dirtyEntry != null)
			throw new InvalidOperationException($"The git working directory must be clean, but '{dirtyEntry.FilePath}' is dirty.");

		headSha = gitRepository.Head.Tip.Sha;
		try
		{
			githubClient.Repository.Commit.GetSha1(githubOwner, githubRepo, headSha).GetAwaiter().GetResult();
		}
		catch (Octokit.NotFoundException exception)
		{
			throw new InvalidOperationException($"The current commit '{headSha}' must be pushed to GitHub.", exception);
		}

		GitLink(MakeAbsolute(Directory(".")).FullPath, new GitLinkSettings
		{
			RepositoryUrl = $"{githubRawUri}/{githubOwner}/{githubRepo}",
			ArgumentCustomization = args => args.Append($"-ignore Bom,BomTest"),
		});

		version = GetSemVerFromFile(assemblyFileName);
	});

Task("NuGetPack")
	.IsDependentOn("SourceIndex")
	.Does(() =>
	{
		CreateDirectory("release");

		foreach (var nuspecPath in GetFiles($"src/*.nuspec"))
		{
			NuGetPack(nuspecPath, new NuGetPackSettings
			{
				Version = version,
				OutputDirectory = "release",
			});
		}
	});

Task("NuGetPublishOnly")
	.IsDependentOn("NuGetPack")
	.WithCriteria(() => !string.IsNullOrEmpty(nugetApiKey))
	.Does(() =>
	{
		foreach (var nupkgPath in GetFiles($"release/*.nupkg"))
		{
			NuGetPush(nupkgPath, new NuGetPushSettings
			{
				ApiKey = nugetApiKey,
				Source = nugetSource,
			});
		}
	});

Task("NuGetTagOnly")
	.IsDependentOn("NuGetPack")
	.WithCriteria(() => !string.IsNullOrEmpty(githubApiKey))
	.Does(() =>
	{
		var tagName = $"nuget-{version}";
		Information($"Creating git tag '{tagName}'...");
		githubClient.Git.Reference.Create(githubOwner, githubRepo,
			new Octokit.NewReference($"refs/tags/{tagName}", headSha)).GetAwaiter().GetResult();
	});

Task("NuGetPublish")
	.IsDependentOn("NuGetPublishOnly")
	.IsDependentOn("NuGetTagOnly");

Task("Coverage")
	.IsDependentOn("Build")
	.Does(() =>
	{
		CreateDirectory("release");
		if (FileExists("release/coverage.xml"))
			DeleteFile("release/coverage.xml");
		foreach (var testDllPath in GetFiles($"tests/**/bin/**/*.UnitTests.dll"))
		{
			StartProcess(@"cake\OpenCover\tools\OpenCover.Console.exe",
				$@"-register:user -mergeoutput ""-target:cake\xunit.runner.console\tools\xunit.console.exe"" ""-targetargs:{testDllPath} -noshadow"" ""-output:release\coverage.xml"" -skipautoprops -returntargetcode ""-filter:+[Facility*]*""");
		}
	});

Task("CoverageReport")
	.IsDependentOn("Coverage")
	.Does(() =>
	{
		StartProcess(@"cake\ReportGenerator\tools\ReportGenerator.exe", $@"""-reports:release\coverage.xml"" ""-targetdir:release\coverage""");
	});

Task("CoveragePublish")
	.IsDependentOn("Coverage")
	.Does(() =>
	{
		StartProcess(@"cake\coveralls.io\tools\coveralls.net.exe", $@"--opencover ""release\coverage.xml"" --full-sources --repo-token {coverallsApiKey}");
	});

Task("Default")
	.IsDependentOn("Test");

RunTarget(target);
