# Contributing

To contribute to this project, please submit a pull request, referencing the corresponding issue as appropriate.

Changes should generally be verified with one or more new unit tests.

Use `build.ps1 test` to build the solution and run the unit tests from the command line. ([PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell) is supported on Windows, Linux, and macOS.)

If the code generation algorithm changes, run `build.ps1 codegen` to update the locally generated code, which must be up-to-date for the automated build to succeed.

To publish to NuGet, update the `<VersionPrefix>` in [`Directory.Build.props`](Directory.Build.props) and add a corresponding section to the top of [`ReleaseNotes.md`](ReleaseNotes.md). (To publish a prerelease version, add a `<VersionSuffix>` element below `<VersionPrefix>`.) When the version change is merged to `master`, publishing will happen automatically.

This repository uses the [`faithlife-build`](https://github.com/FacilityApi/RepoTemplate/tree/faithlife-build) template of [`FacilityApi/RepoTemplate`](https://github.com/FacilityApi/RepoTemplate).
