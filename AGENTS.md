# Agent Instructions

## Repository Overview

Odin, short for 'OrDinary INfrastructure', is a multi-package mono-repo containing packages for .NET line-of-business application development concerns.
The root solution is `Odin.sln`; `TestsOnly.sln` is used for test validation.

Primary component folders include:

- `BackgroundProcessing`
- `Commands`
- `Configuration`
- `Database`
- `DesignContracts`
- `DomainDrivenDesign`
- `Email`
- `Logging`
- `Messaging`
- `Notifications`
- `Queries`
- `RemoteFiles`
- `System`
- `Templating`
- `Testing`
- `Utility`

Most components follow a local layout of production projects plus a sibling `Tests` project.
Keep changes scoped to the component being modified unless a shared contract requires a broader update.

## Toolchain

- Use the .NET SDK pinned by `global.json`: `10.0.300` with `rollForward` set to `latestFeature`.
- Projects target `net8.0`, `net9.0`, and `net10.0` via `Directory.Build.props`.
- Package versions are centrally managed in `Directory.Packages.props`.
- Do not add package versions directly to individual `.csproj` files unless the repo pattern changes.

## Common Commands

Prefer narrow validation while developing, then broader validation when touching shared code.

```bash
dotnet build Odin.sln
dotnet test TestsOnly.sln -f net10.0 --filter "Category!=IntegrationTest"
dotnet test TestsOnly.sln -f net9.0 --filter "Category!=IntegrationTest"
dotnet test TestsOnly.sln -f net8.0 --filter "Category!=IntegrationTest"
```

For a single test project:

```bash
dotnet test path/to/Tests.Odin.Component.csproj -f net10.0 --filter "Category!=IntegrationTest"
```

Run integration tests only when the change explicitly needs them or the user asks for them.

## Coding Conventions

- Follow `.editorconfig`.
- Use 4-space indentation for C#.
- Nullable reference types and implicit usings are enabled.
- Prefer explicit types over `var`, matching the repository style.
- Keep braces on control-flow statements.
- Existing code commonly uses file-scoped package-style folders but block-scoped namespaces; match the file you edit.
- Public library APIs should include XML documentation when nearby public APIs do.
- Avoid broad refactors, formatting churn, or namespace moves unrelated to the task.

## Tests

- Add or update tests in the relevant `Tests` project for behavior changes.
- This repo uses xUnit in many test projects and NUnit utilities in dedicated testing packages; match the project already in place.
- Keep tests focused on public behavior and edge cases, especially nullability, validation, and serialization behavior.
- CI validates `TestsOnly.sln` for `net10.0`, `net9.0`, and `net8.0`, excluding `Category!=IntegrationTest`.

## Package and Release Notes

- Do not change package metadata, GitVersion settings, or publish workflows unless the task requires it.
- When adding a new package dependency, add the version in `Directory.Packages.props`.
- When adding a new project, ensure it is included in the appropriate solution file and follows existing naming patterns:
  - Production packages: `Odin.<Area>[.<Feature>]`
  - Tests: `Tests.Odin.<Area>`

## Package Publish to Nuget

- All packages in the Odin libraries are published to Nuget by the Github Action defined in '.github/workflows/publish.yml'
- When changing the path to a package, ensure the corresponding publish.yml step is updated to reflect the new path.

## Git Hygiene

- Preserve user changes already present in the worktree.
- Do not run destructive git commands unless explicitly requested.
- Keep commits, when requested, limited to the requested change scope.
