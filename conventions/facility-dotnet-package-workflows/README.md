# facility-dotnet-package-workflows

The `facility-dotnet-package-workflows` convention manages GitHub Actions workflows for normal cross-platform FacilityApi .NET package repositories.

## Behavior

The convention writes `.github/workflows/ci.yml`, `.github/workflows/copilot-setup-steps.yml`, and `.github/workflows/apply-repo-conventions.yml` from published templates. It is intended for repositories that can restore, build, test, and package on Ubuntu, Windows, and macOS.

When a workflow template includes a `cron` expression with minute `0`, the convention writes a random minute between `1` and `59` inclusive instead. Minute-only cron differences are treated as equivalent during comparison, so re-running the convention does not rewrite workflows just to change that randomized minute.

The apply workflow uses the FacilityApi-owned `Facility Bot` GitHub App. Repositories using this convention must define `FACILITY_BOT_CLIENT_ID` as an Actions variable and `FACILITY_BOT_PRIVATE_KEY` as an Actions secret.

Repositories with materially different CI needs should use a different workflow convention instead of adding settings to this convention.

## Example

```yaml
conventions:
  - path: FacilityApi/Facility/conventions/facility-dotnet-package-workflows
```
