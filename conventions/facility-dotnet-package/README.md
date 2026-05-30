# facility-dotnet-package

The `facility-dotnet-package` convention applies the standard convention collection for FacilityApi .NET package repositories.

This convention composes the common .NET repository conventions from `Faithlife/CodingGuidelines`, including common build files, NuGet configuration, ignore files, contributing guidelines, common MSBuild properties, and the MIT license.

## Behavior

The convention is intended for SDK-style FacilityApi .NET package repositories that publish NuGet packages or .NET tools. It keeps repository-specific version numbers, target frameworks, package references, package descriptions, analyzer suppressions, package validation baselines, package metadata properties, and code generation behavior outside the convention.

Keep repository metadata such as `GitHubOrganization`, `RepositoryName`, and `PackageLicenseExpression` in the first `Directory.Build.props` property group before `Authors`.

Workflow files are intentionally handled by a separate convention so repositories with different CI shapes can choose the correct workflow convention independently.

## Example

```yaml
conventions:
  - path: FacilityApi/Facility/conventions/facility-dotnet-package
  - path: FacilityApi/Facility/conventions/facility-dotnet-package-workflows
```
