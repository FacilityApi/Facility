# Convention Configuration

RepoConventions uses the same convention reference model in repository configuration and in convention-local `convention.yml` files. This page covers the YAML elements used by repository consumers and convention authors: references, settings, commit settings, and pull request settings.

## Convention References

Each convention reference must contain a non-empty `path`. It may also contain `settings`, `commit`, and `pull-request`.

`path` identifies a convention directory. Each convention should document its own settings, behavior, and required tools.

Supported path forms:

| Form | Meaning |
| --- | --- |
| `owner/repo/path@ref` | Clone a GitHub repository and use `path` inside it. `path` may be omitted to use the repository root. `@ref` may be omitted to use the default branch. |
| `./relative/path` | Resolve relative to the YAML file that contains the reference. |
| `../relative/path` | Resolve relative to the YAML file that contains the reference. The result must stay inside that YAML file's repository. |
| `/root/relative/path` | Resolve from the root of the repository that contains the YAML file. |

Path examples:

```yaml
conventions:
  - path: Faithlife/CodingGuidelines/conventions/dotnet-sdk-10@v1.2.3
  - path: Faithlife/CodingGuidelines/conventions/gitattributes-lf
  - path: /.github/conventions/local-policy
  - path: /conventions/root-policy
```

Local paths must stay inside the repository that contains the YAML file. This rule applies to conventions checked into the target repository and to convention repositories cloned from GitHub.

`settings` is passed to the convention as JSON-compatible data. Use YAML objects, arrays, strings, numbers, booleans, or null values. Each convention documents the settings it accepts.

Settings examples:

```yaml
conventions:
  - path: Faithlife/CodingGuidelines/conventions/example
    settings:
      name: RepoConventions
      version: 10
      enabled: true
      labels:
        - automation
        - conventions
      metadata:
        owner: Faithlife
      optionalNote: null
```

## Commit Settings

Commit settings control the automatic commit created when `convention.ps1` leaves uncommitted changes and does not create commits itself.

Supported properties:

| Property | Type | Description |
| --- | --- | --- |
| `message` | string | Commit message for the automatic commit. Empty or whitespace-only values are treated as unspecified. |

Behavior:

- If no message is configured, RepoConventions uses `Apply convention <name>`.
- A convention reference's `commit` settings override the convention's own defaults.
- Composite conventions pass the effective commit message down to child conventions. A child convention's own `commit` settings, or settings on that child reference, can override the inherited message.
- Commit settings do not affect commits created directly by `convention.ps1`.
- When adjacent automatic commits in the same run use the same message, RepoConventions amends the previous automatic commit instead of creating a second adjacent commit with the same message.

Use a custom `message` when the convention has a stable, recognizable purpose. Prefer a concise imperative subject, such as `Update .NET SDK version` or `Refresh generated CI files`.

Example:

```yaml
conventions:
  - path: Faithlife/CodingGuidelines/conventions/generated-files
    commit:
      message: Refresh generated files
```

## Pull Request Settings

Pull request settings describe metadata for the pull request generated from applying conventions. This metadata is used when the command runs with `--open-pr`.

Supported properties:

| Property | Type | Description |
| --- | --- | --- |
| `labels` | string sequence | Labels to add to the generated pull request. Missing labels are created automatically. The `repo-conventions` label is always added. |
| `reviewers` | string sequence | GitHub users or teams to request as reviewers. |
| `assignees` | string sequence | GitHub users to assign. |
| `draft` | boolean | When true, create the pull request as a draft. |
| `auto-merge` | boolean | When true, enable GitHub auto-merge after opening the pull request. |
| `merge-method` | string | Preferred auto-merge method: `merge`, `squash`, or `rebase`. Defaults to `squash` when auto-merge is enabled and no single method is configured. |

Pull request settings can appear at three levels:

- Top-level repository `pull-request` settings apply to the whole generated pull request.
- A convention reference's `pull-request` settings apply only if that convention contributes commits to the generated pull request.
- A convention's own `convention.yml` can provide default `pull-request` settings for that convention, whether the convention is stored in the target repository or cloned from a remote repository.

Merge behavior:

- `labels`, `reviewers`, and `assignees` are appended, then de-duplicated case-insensitively.
- `draft`, `auto-merge`, and `merge-method` are scalar settings; reference-level settings override convention defaults.
- Convention-level pull request metadata is ignored when the convention does not create commits.
- When auto-merge is enabled, reviewers and assignees are not requested.
- If multiple contributing conventions request conflicting merge methods and no repository-level or reference-level setting resolves the conflict, RepoConventions falls back to `squash`.

CLI flags override configured pull request settings for a single run:

- `--draft` and `--no-draft` override `draft`.
- `--auto-merge` and `--no-auto-merge` override `auto-merge`.
- `--merge-method merge|squash|rebase` overrides `merge-method`.

If a requested merge method is disabled or rejected by GitHub, RepoConventions tries other allowed methods, preferring `squash` as the first fallback. If auto-merge was enabled by configuration and cannot be enabled, the command reports the failure but still succeeds. If `--auto-merge` was provided explicitly and auto-merge cannot be enabled, the command fails.

Top-level pull request example:

```yaml
pull-request:
  labels:
    - automation
  reviewers:
    - octocat
  draft: true
```

Convention-specific pull request example:

```yaml
conventions:
  - path: Faithlife/CodingGuidelines/conventions/dependency-updates
    pull-request:
      labels:
        - dependencies
      auto-merge: true
      merge-method: squash
```
