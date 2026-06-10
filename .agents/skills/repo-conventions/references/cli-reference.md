# CLI Reference

RepoConventions has three commands:

```pwsh
dnx repo-conventions add <path> [<path>...] [options]
dnx repo-conventions apply [options]
dnx repo-conventions validate [options]
```

## Common Path Options

| Option | Description |
| --- | --- |
| `--repo <path>` | Target repository root. Defaults to the current directory. Relative paths are resolved from the current process directory. |
| `--config <path>` | Conventions configuration file. Defaults to `.github/conventions.yml` under the target repository root. Relative paths are resolved from the current process directory. |
| `--temp <path>` | Temporary root for RepoConventions-managed transient files. Defaults to the system temp directory. Relative paths are resolved from the current process directory. |

## Common Pull Request Options

| Option | Description |
| --- | --- |
| `--open-pr` | Apply conventions, create commits, push a `repo-conventions` branch, and open or update a pull request. |
| `--draft` / `--no-draft` | Override configured draft behavior. These options cannot be used together. |
| `--auto-merge` / `--no-auto-merge` | Override configured auto-merge behavior. These options cannot be used together. |
| `--merge-method <method>` | Preferred auto-merge method. Must be `merge`, `squash`, or `rebase`. |
| `--git-no-verify` | Pass `--no-verify` to git commit and git push commands run by RepoConventions, and expose `git.noVerify` in the JSON input to convention scripts so they can bypass the same hooks when they create their own commits. |

## `add`

`repo-conventions add` appends one or more convention paths to the configuration file. If the file is missing, it creates it. New paths are validated before the configuration file is changed. If a path is already present with the same reference configuration, it leaves the file unchanged for that path. If a path is already present with different reference configuration, the command fails without changing the file.

Examples:

```pwsh
dnx repo-conventions add Faithlife/CodingGuidelines/conventions/dotnet-sdk-10
dnx repo-conventions add Faithlife/CodingGuidelines/conventions/gitattributes-lf
dnx repo-conventions add /.github/conventions/local-policy
dnx repo-conventions add Faithlife/CodingGuidelines/conventions/dotnet-sdk-10 Faithlife/CodingGuidelines/conventions/github-actions
dnx repo-conventions add Faithlife/CodingGuidelines/conventions/write-file --with "{settings: {file: docs/example.md, overwrite: false}}"
```

Use `--with <yaml>` with one convention path to add reference-level `settings`, `commit`, or `pull-request` configuration. The value must be a YAML mapping fragment and must not include `path`.

`add` requires the target repository path to be a Git repository root. When `--commit`, `--apply`, and `--open-pr` are not used, it can run when the target repository has tracked or untracked file changes.

Additional `add` options:

| Option | Description |
| --- | --- |
| `--commit` | Commit newly added convention references. Requires a clean repository. |
| `--apply` | Commit newly added convention references, then apply all conventions. Requires a clean repository. |
| `--open-pr` | Commit newly added convention references, apply all conventions, and open or update a pull request. Requires a clean repository. |
| `--with <yaml>` | YAML configuration for one convention reference. Supported top-level keys are `settings`, `commit`, and `pull-request`. |

With `--open-pr`, `add` commits any newly added convention references, applies the resulting configuration, commits convention changes, and opens or updates a pull request:

```pwsh
dnx repo-conventions add /.github/conventions/local-policy --open-pr
```

## `apply`

`repo-conventions apply` loads the configuration file, resolves the full convention plan, applies each convention in order, and creates commits for conventions that leave changes behind.

Examples:

```pwsh
dnx repo-conventions apply
dnx repo-conventions apply --git-no-verify
```

`apply` requires no tracked or untracked file changes in the target repository before it starts.

When running in GitHub Actions, RepoConventions groups output per convention and appends the final summary line to `GITHUB_STEP_SUMMARY` when that environment variable is available.

With `--open-pr`, `apply` pushes any convention commits and opens or updates a GitHub pull request:

```pwsh
dnx repo-conventions apply --open-pr
dnx repo-conventions apply --open-pr --draft
dnx repo-conventions apply --open-pr --auto-merge --merge-method rebase
dnx repo-conventions apply --open-pr --no-auto-merge
```

`--open-pr` requires:

- a non-detached starting branch
- no unpushed commits on the starting branch
- the GitHub CLI `gh` installed and authenticated

When opening a pull request, RepoConventions creates a branch named `repo-conventions`, `repo-conventions-2`, or the next available suffix. If an open RepoConventions pull request already targets the starting branch, the command updates that pull request instead of opening another one. If the base branch has advanced, the existing PR branch is rebuilt from the current base and force-pushed.

If applying the conventions produces no commits, RepoConventions returns to the starting branch and does not keep the generated local branch, push a branch, or open a pull request.

## `validate`

`repo-conventions validate` loads the configuration file and resolves the complete convention plan without running convention scripts, creating commits, or changing the working tree.

Examples:

```pwsh
dnx repo-conventions validate
dnx repo-conventions validate --config .config/repo-conventions.yml
```

`validate` requires the target repository path to be a Git repository root. It can run when the target repository has tracked or untracked file changes.

When validation succeeds, it prints a summary with the number of conventions that were validated.
