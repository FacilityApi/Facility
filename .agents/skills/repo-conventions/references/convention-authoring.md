# Convention Authoring

Use this guide when creating or updating convention directories consumed by RepoConventions.

## Goal

Produce conventions with stable paths, documented settings, predictable output, and idempotent behavior that matches the RepoConventions execution contract.

## Authoring Checklist

- Define the policy boundary first. Prefer one coherent convention over a grab bag of unrelated changes.
- Inspect existing published conventions in the repository before introducing new structure or terminology.
- Choose whether the convention is composite, executable, or both.
- Keep the public surface small: stable path, clearly named settings, predictable outputs.
- Write or update the convention-local `README.md` in the same change.
- Test the non-compliant case, the already-compliant case, and a second successful run for idempotency.

## Directory Model

- A published convention directory may contain `convention.yml`, `convention.ps1`, or both.
- If both files exist, RepoConventions applies `convention.yml` first and then executes `convention.ps1`.
- `convention.yml` composes child conventions and can provide commit or pull request settings.
- `convention.ps1` inspects or rewrites the target repository.
- `README.md` documents the convention for consumers.
- Supporting files may be read by the script or by settings expressions.

Recommended layout:

```text
conventions/my-convention/
  README.md
  convention.yml
  convention.ps1
  convention.Tests.ps1
  files/
    supporting-file.txt
```

See [Convention Configuration](./convention-configuration.md) for child reference paths, settings values, commit settings, and pull request settings.

## `convention.yml`

Use `convention.yml` when a convention composes other conventions, provides default commit settings, provides default pull request settings, or any combination of those.

Composition-only conventions must include a `conventions` sequence. Executable conventions that also contain `convention.ps1` may omit `conventions` and include only `commit` or `pull-request` settings.

Minimal executable convention with a default commit message:

```yaml
commit:
  message: Normalize repository files
```

Minimal composite convention:

```yaml
conventions:
  - path: /conventions/dotnet-sdk-10
  - path: /conventions/dotnet-slnx
```

Example:

```yaml
commit:
  message: Update .NET repository conventions

pull-request:
  labels:
    - dependencies
  auto-merge: true
  merge-method: squash

conventions:
  - path: /conventions/dotnet-sdk-10
  - path: /conventions/dotnet-slnx
```

Guidelines:

- Keep child conventions in the order they should be applied.
- Prefer repository-root-relative local paths, such as `/conventions/dotnet-sdk-10`, for conventions published from the same repository.
- Keep settings JSON-compatible: objects, arrays, strings, numbers, booleans, or null.
- Keep settings shallow unless nesting communicates a real domain boundary.
- Avoid formatting-only churn in generated files unless formatting is the purpose of the convention.

Supported root properties:

| Property | Type | Description |
| --- | --- | --- |
| `conventions` | sequence | Child convention references to apply in declaration order. Required when the directory has no `convention.ps1`; optional when the convention is executable. |
| `commit` | object | Default automatic commit settings for this convention and its child conventions. |
| `pull-request` | object | Pull request metadata contributed when this convention creates commits. |

## Child Settings Expressions

Composite conventions can map parent settings into child settings with expressions.

`settings` lookup:

```yaml
conventions:
  - path: /conventions/dotnet-sdk
    settings:
      version: ${{ settings.sdk.version }}
```

- Reads a dotted property path from the parent convention's settings object.
- When the whole value is one expression, preserves JSON-compatible types such as strings, numbers, booleans, arrays, objects, and null.
- When embedded in a larger string, converts strings directly, null to `null`, and arrays or objects to compact JSON.
- Missing values are omitted from object properties and array items. If the missing expression is embedded in a larger string, it contributes an empty string.
- If an array expression is used as an array item, its items are spliced into the destination array.

Simple mapping examples:

```yaml
conventions:
  - path: /conventions/dotnet-sdk
    settings:
      version: ${{ settings.sdkVersion }}
  - path: /conventions/write-readme
    settings:
      title: ${{ settings.name }}
      heading: "Repository: ${{ settings.name }}"
      labels:
        - standard
        - ${{ settings.extraLabels }}
```

`readText("path")`:

```yaml
conventions:
  - path: /conventions/write-file
    settings:
      body: ${{ readText("/conventions/write-file/body.txt") }}
```

- Reads UTF-8 text from a file. A UTF-8 BOM is ignored.
- Relative paths resolve from the YAML file that contains the expression.
- Paths beginning with `/` resolve from the root of the repository that contains the YAML file.
- Native absolute paths and paths that escape the containing repository are rejected.
- Use it when file-backed text is clearer than embedding long YAML strings.

## `convention.ps1`

Use `convention.ps1` when the convention must inspect repository state, run tools, or rewrite files.

Execution contract:

- The script runs with `pwsh -NoProfile`.
- The current working directory is the target Git repository root, not the convention directory.
- The first argument is the path to a JSON input file.
- Use `$args[0]` to access the input path so future arguments do not break the script.
- The JSON input file contains a `settings` property and a `git` property.
- `git.noVerify` is a boolean that reflects the `--git-no-verify` option. When it is `true`, scripts that create their own commits or pushes must pass `--no-verify` so they bypass the same hooks RepoConventions bypasses.
- RepoConventions captures stdout and stderr as UTF-8. Set `[Console]::OutputEncoding` before invoking native tools so their output is emitted as UTF-8 too.

Standard header for `convention.ps1`:

```pwsh
#requires -PSEdition Core
#requires -Version 7.0
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$utf8 = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8
```

Then, if settings are used:

```pwsh
$conventionInput = Get-Content -Raw $args[0] | ConvertFrom-Json
$settings = $conventionInput.settings
```

When the script creates its own commits, honor `git.noVerify` so it matches the rest of the run:

```pwsh
$conventionInput = Get-Content -Raw $args[0] | ConvertFrom-Json
$commitArguments = @('commit', '-m', 'Use LF')
if ($conventionInput.git.noVerify) {
    $commitArguments += '--no-verify'
}
git @commitArguments
```

Authoring expectations:

- Read the JSON input only when settings are needed.
- Make the script idempotent. A second successful run should produce no further changes.
- Exit with code zero when the repository is already compliant or after successfully making it compliant.
- Use a non-zero exit code only when the convention genuinely cannot complete.
- Avoid interactive prompts, editor launches, global machine-local state, and hidden credentials.
- Prefer deterministic file writes, stable ordering, and stable line endings.
- When the script has nothing to do, usually emit no output; already-compliant repositories are the most common case.
- Emit focused output that explains what changed or why the convention cannot continue.
- If the convention naturally consists of multiple meaningful steps, the script may create its own commits with informative messages. When it does, pass `--no-verify` to those commits whenever `git.noVerify` is `true`.

## Commit and Failure Behavior

- On success, if `convention.ps1` leaves tracked or untracked changes and does not create commits itself, RepoConventions creates an automatic commit using the effective `commit.message`, or `Apply convention <name>` when no message is configured.
- If the script creates commits itself, RepoConventions preserves those commits.
- If the convention leaves no changes or new commits, RepoConventions does not add a commit for that convention.
- If the script exits with a non-zero code, RepoConventions hard-resets the target repository to the commit before that convention started and stops the run.
- RepoConventions builds the convention plan before applying any convention, so path and settings-expression errors prevent partial application.

## Documentation

Always include a `README.md` in the convention directory. Document:

- what the convention does
- every supported setting, including defaults and examples
- required tools, frameworks, or repository assumptions
- notable files the convention creates, rewrites, or commits
- any important limitations or follow-up steps for consumers

Keep repository-level consumer docs focused on using RepoConventions.

## Testing

- Test the convention with Pester if possible.
- Put Pester tests in the same directory as the convention they cover, e.g. `conventions/my-convention/convention.Tests.ps1`.
- Prefer new temporary repositories with no preexisting tracked or untracked file changes so tests exercise real files, git state, and command behavior.
- Test both an already-compliant repository and a non-compliant repository.
- Re-run after the first successful application to confirm idempotency.
- If the convention has settings, exercise at least one non-default settings case.
- If the convention executes any git commits or pushes, test compliance with `git.noVerify` input against failing git hooks.
- Test failure paths when settings are required or external tools may be unavailable.

## Agent Workflow

When an AI agent updates a convention:

- Read the existing convention directory and nearby conventions first.
- Preserve the published path and setting names unless the user explicitly requests a breaking change.
- Update `convention.yml`, `convention.ps1`, local docs, and tests as one coherent change.
- Prefer small, deterministic scripts over broad repository rewrites.
- Validate by running the narrowest meaningful tests, then the repository's required final test command when appropriate.
