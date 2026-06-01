---
name: repo-conventions
description: Create, update, document, or use RepoConventions repository configurations, convention directories, convention.yml files, convention.ps1 scripts, settings, tests, and supporting files.
---

# RepoConventions

Use this skill when working with RepoConventions configuration, CLI usage, published convention directories, or convention authoring documentation.

## Agent Instructions

- Start with the feature page that matches the task, then inspect the current repository files before editing.
- Preserve published convention paths and setting names unless the user explicitly requests a breaking change.
- Keep convention changes coherent: update `convention.yml`, `convention.ps1`, convention-local `README.md`, tests, and supporting files together when the behavior changes.
- Prefer deterministic, idempotent scripts and stable YAML over broad rewrites or formatting-only churn.
- Validate with the narrowest meaningful tests first; for larger convention behavior changes, run the repository's required final test command when practical.

## Features

- [**Repository Configuration**](./references/repository-configuration.md) — Configure `.github/conventions.yml`, declare local and remote conventions, pass settings, and configure pull request metadata.
- [**CLI Reference**](./references/cli-reference.md) — Run the `add`, `validate`, and `apply` commands, with path options, pull request options, and clean-repository requirements.
- [**Convention Configuration**](./references/convention-configuration.md) — Learn the shared YAML model for convention references, settings, commit settings, and pull request settings.
- [**Convention Authoring**](./references/convention-authoring.md) — Create idempotent convention directories with `convention.yml`, scripts, local documentation, tests, and child settings expressions.
