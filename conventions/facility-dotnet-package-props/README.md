# facility-dotnet-package-props

The `facility-dotnet-package-props` convention manages universal MSBuild package properties for FacilityApi .NET package repositories.

## Behavior

The convention writes a managed section to `Directory.Build.props` containing the FacilityApi organization, repository name, and MIT package license expression. The managed section is inserted before the `dotnet-common-props` managed section when it exists so repository-owned properties are available to the common properties.

The repository name is derived from the target repository directory name. The convention is surgical: it does not remove old properties, rewrite package metadata, change versions, or perform one-time cleanup.

## Example

```yaml
conventions:
  - path: FacilityApi/Facility/conventions/facility-dotnet-package-props
```
