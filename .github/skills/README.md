# Workspace Agent Skills

Skills in this directory are available to coding agents working in this repository. The repository-specific `masuit-tools-*` skills are maintained here; the following skills are vendored from Microsoft's [`dotnet/skills`](https://github.com/dotnet/skills) repository:

- `csharp-refactoring`
- `dotnet-breaking-changes`
- `msbuild`
- `setup-local-sdk`

The vendored skills are pinned to upstream commit `96c3d68dad2855e3ab5178aa1a262789d86a1466`. They are distributed under the MIT license; see [dotnet-skills-LICENSE](./dotnet-skills-LICENSE).

To update them, copy the complete skill directories (including their `references` and other resources) from `plugins/dotnet/skills/` at the chosen upstream revision, and update the pinned commit above.
