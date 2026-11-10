# MSBuild anti-pattern catalog

Based on the original `msbuild-antipatterns` skill. The numbered catalog retains its
smell, impact, and correction structure. Use relevant entries for troubleshooting or an
explicit authoring review, not as a mandate to rewrite every matching pattern.

Confirm evaluated behavior and exceptions before editing. Preserve framework/package versions,
intentional overrides, item metadata, and public/output contracts. Legacy project-system
migration is a separate task, not an automatic consequence of finding verbose XML.

For framework-conditioned shared XML, check
[property versus item evaluation in AP-21](antipatterns/additional-antipatterns.md#ap-21-an-early-property-condition-reads-targetframework-too-soon)
before treating an early import as proof that both conditions are wrong.

## AP-01: Exec for operations with built-in tasks

**Smell:** shell `mkdir`, `copy`, or `del` commands in a target.

**Impact:** shell-specific syntax, weak file-level logging, and more difficult incremental
tracking. Built-in tasks integrate with MSBuild, but using one does not automatically make the
containing target incremental.

```xml
<!-- BAD: shell-dependent file operations. -->
<Target Name="PrepareOutput">
  <Exec Command="mkdir $(OutputPath)logs" />
  <Exec Command="copy config.json $(OutputPath)" />
  <Exec Command="del $(IntermediateOutputPath)*.tmp" />
</Target>

<!-- GOOD: preserve the intended operation with built-in tasks. -->
<Target Name="PrepareOutput">
  <MakeDir Directories="$(OutputPath)logs" />
  <Copy SourceFiles="config.json" DestinationFolder="$(OutputPath)" />
  <ItemGroup>
    <TempFiles Include="$(IntermediateOutputPath)*.tmp" />
  </ItemGroup>
  <Delete Files="@(TempFiles)" />
</Target>
```

| Shell operation | MSBuild task |
| --- | --- |
| `mkdir` | `MakeDir` |
| `copy` / `cp` / recursive copy | `Copy`, with the intended item set |
| `del` / `rm` | `Delete`, scoped to known owned files |
| `move` / `mv` | `Move` |
| Write text | `WriteLinesToFile` |
| Update a timestamp | `Touch` |

Keep real external tools as tools, with correct quoting, platform support, and error handling.
Preserve copy/reset semantics and never widen the deletion scope during this conversion.

## AP-02: Unquoted string comparisons

**Smell:** `Condition="$(Foo) == Bar"`.

**Impact:** empty values or values containing spaces/special characters can change parsing.

```xml
<!-- BAD -->
<PropertyGroup Condition="$(Configuration) == Release">
  <Optimize>true</Optimize>
</PropertyGroup>

<!-- GOOD -->
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <Optimize>true</Optimize>
</PropertyGroup>
```

Quote both string operands. Do not flag valid boolean/function conditions merely because they
are not string comparisons.

## AP-03: Hardcoded absolute paths

**Smell:** machine-specific tool or import paths.

**Impact:** builds fail on another checkout, machine, or operating system.

```xml
<!-- BAD -->
<PropertyGroup>
  <ToolPath>C:\tools\mytool\mytool.exe</ToolPath>
</PropertyGroup>
<Import Project="C:\repos\shared\common.props" />

<!-- GOOD: RepoRoot must be defined by the repository. -->
<PropertyGroup>
  <ToolPath>$(MSBuildThisFileDirectory)tools\mytool\mytool.exe</ToolPath>
</PropertyGroup>
<Import Project="$(RepoRoot)eng\common.props" />
```

| Property/function | Meaning |
| --- | --- |
| `MSBuildThisFileDirectory` | Directory of the file defining the current import/logic |
| `MSBuildProjectDirectory` | Directory of the project being built |
| `GetDirectoryNameOfFileAbove` / `GetPathOfFileAbove` | Find a repository marker or parent file |
| `NormalizePath` / `NormalizeDirectory` | Combine and normalize path segments |

Resolve relative paths against the intended owner, not an incidental caller working directory.

## AP-04: Restating SDK defaults

**Smell:** properties repeat the selected SDK's defaults.

**Impact:** noise can obscure real overrides and accidentally pin behavior across SDK changes.

```xml
<!-- Potentially redundant for a library named MyLib with these SDK defaults. -->
<PropertyGroup>
  <OutputType>Library</OutputType>
  <EnableDefaultItems>true</EnableDefaultItems>
  <EnableDefaultCompileItems>true</EnableDefaultCompileItems>
  <RootNamespace>MyLib</RootNamespace>
  <AssemblyName>MyLib</AssemblyName>
  <AppendTargetFrameworkToOutputPath>true</AppendTargetFrameworkToOutputPath>
</PropertyGroup>

<!-- Keep the settings that actually express the project's contract. -->
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
</PropertyGroup>
```

Verify the actual SDK/imports, configurations, naming, and intentional pins first. A setting that
matches today's default can still deliberately preserve behavior; do not remove it blindly.

## AP-05: Redundant file lists in SDK-style projects

**Smell:** explicit C#/VB source includes duplicate enabled SDK default items.

**Impact:** duplicate items, merge noise, and source additions that can be missed when defaults
have instead been disabled.

```xml
<!-- BAD when default Compile items already contain these files. -->
<ItemGroup>
  <Compile Include="Program.cs" />
  <Compile Include="Services\MyService.cs" />
</ItemGroup>

<!-- GOOD: keep only intentional exclusions or metadata. -->
<ItemGroup>
  <Compile Remove="LegacyCode\**" />
  <Compile Update="Services\MyService.cs" Visible="false" />
</ItemGroup>
```

**Exceptions:** legacy projects, projects with default items intentionally disabled, linked files
outside default globs, generated files, and items with special metadata need their actual contract
preserved. `Update` modifies an existing item; it does not add a missing one.

**F# is order-dependent.** Keep explicit `.fsproj` compile items in dependency order. An `.fsi`
signature must immediately precede its matching `.fs`; the entry point is last. Do not apply C#
implicit-glob cleanup to F#.

The standard F# SDK already disables default compile items. Do not prescribe
`EnableDefaultCompileItems=false` as a required repair without evidence that the selected
SDK/imports actually enable unwanted defaults.

## AP-06: Package DLL HintPaths instead of package references

**Smell:** package assemblies referenced through a legacy `packages` directory.

**Impact:** the project depends on a fixed restored layout instead of PackageReference asset
selection. This is a migration candidate, not proof that existing packages.config restore is broken.

```xml
<!-- Legacy package reference. -->
<Reference Include="Newtonsoft.Json">
  <HintPath>..\packages\Newtonsoft.Json.13.0.3\lib\netstandard2.0\Newtonsoft.Json.dll</HintPath>
</Reference>

<!-- PackageReference replacement, when migration is authorized and compatible. -->
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

Preserve package versions, conditions, content/build assets, and restore behavior. Do not remove
GAC, local, or vendor assembly references just because they use `Reference` or `HintPath`.

## AP-07: Build-only packages exposed to consumers

**Smell:** a local analyzer/tool becomes an unwanted dependency or build asset of consumers.

**Impact:** consumers can receive implementation-only dependencies or transitive build behavior.
Inspect effective metadata and package assets; absence of a literal `PrivateAssets` attribute
does not prove all analyzer assets propagate.

Use `PrivateAssets="all"` for dependencies intended to remain entirely private. Preserve
intentionally public/transitive build contracts. See
[private-assets.md](antipatterns/private-assets.md) for examples and exceptions.

## AP-08: Copy-pasted shared properties

**Smell:** the same property block is repeated across several projects.

**Impact:** inconsistent updates and unclear ownership.

```xml
<!-- Directory.Build.props at the intended shared scope. -->
<Project>
  <PropertyGroup>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

Move only genuinely shared values. Preserve project overrides and property evaluation timing.
Only the nearest Directory.Build file is automatically discovered; see
[extension-points](extension-points.md#directorybuild-discovery) before changing hierarchy.

## AP-09: Unintended package-version drift

**Smell:** projects unintentionally use different versions of the same package.

**Impact:** divergent behavior, conflicts, and difficult dependency maintenance.

```xml
<!-- ProjectA.csproj -->
<PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
<!-- ProjectB.csproj -->
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

Central Package Management can consolidate versions when that migration is requested; see
[the NuGet guidance](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management).
Keep intentional framework/project differences. Do not silently upgrade packages or centralize
the whole repository while fixing an unrelated build.

## AP-10: Monolithic targets

**Smell:** one target mixes generation, copying, signing, and other unrelated operations.

**Impact:** the steps cannot be scheduled, diagnosed, or skipped independently.

```xml
<!-- BAD: unrelated work in one target. -->
<Target Name="PrepareRelease" BeforeTargets="Build">
  <WriteLinesToFile File="version.txt" Lines="$(Version)" Overwrite="true" />
  <Copy SourceFiles="LICENSE" DestinationFolder="$(OutputPath)" />
  <Exec Command="signtool sign /f cert.pfx $(OutputPath)*.dll" />
</Target>

<!-- GOOD: separate responsibilities with their own contracts. -->
<Target Name="WriteVersionFile" BeforeTargets="CoreCompile">
  <MakeDir Directories="$(IntermediateOutputPath)" />
  <WriteLinesToFile File="$(IntermediateOutputPath)version.txt"
                    Lines="$(Version)" Overwrite="true" WriteOnlyWhenDifferent="true" />
  <ItemGroup>
    <FileWrites Include="$(IntermediateOutputPath)version.txt" />
  </ItemGroup>
</Target>
<Target Name="CopyLicense" AfterTargets="CopyFilesToOutputDirectory">
  <Copy SourceFiles="LICENSE" DestinationFolder="$(OutputPath)" SkipUnchangedFiles="true">
    <Output TaskParameter="CopiedFiles" ItemName="FileWrites" />
  </Copy>
</Target>
<Target Name="SignAssemblies" AfterTargets="Build" DependsOnTargets="CopyLicense"
        Condition="'$(SignAssemblies)' == 'true'">
  <ItemGroup>
    <AssemblyFiles Include="$(OutputPath)*.dll" />
  </ItemGroup>
  <Error Condition="'@(AssemblyFiles)' == ''"
         Text="Signing was requested, but no assemblies were found in '$(OutputPath)'." />
  <Exec Command="signtool sign /f cert.pfx &quot;%(AssemblyFiles.Identity)&quot;" />
</Target>
```

The cheap version writer intentionally checks the value each build: `Version` can change through
global properties without any project-file timestamp changing. A project-file-only `Inputs`
declaration would incorrectly reuse stale content. For expensive generation, model value changes
with a stable input fingerprint as in [AP-11's example](antipatterns/incremental-build-inputs-outputs.md).

Preserve original target order, supported platforms, required tools, and copy semantics.
`SkipUnchangedFiles` is appropriate only when its timestamp/size heuristic satisfies the contract.
Register the copied destinations in `FileWrites` before MSBuild records them for a subsequent
`Clean`. Hooking `CopyLicense` after `CopyFilesToOutputDirectory` does this even for unchanged
copies; registering them in an `AfterTargets="Build"` target would be too late.
Discover the assemblies inside the signing target, after the build has produced them, rather
than with an evaluation-time glob. This retains the original output-directory DLL selection.

## AP-11: Untracked artifact-producing targets

**Smell:** expensive file generation runs on every no-change build because its inputs/outputs are
not modeled, or skips despite a relevant change.

**Impact:** repeated work or stale outputs. Inputs must cover actual files and relevant property,
configuration, generator, and import changes.

See [incremental-build-inputs-outputs.md](antipatterns/incremental-build-inputs-outputs.md) for a
correctness-oriented example, and [incremental-build](incremental-build.md) for full diagnosis.
Cheap validation, orchestration, and item-discovery targets may legitimately run without
file-based outputs; do not invent a stamp to suppress necessary work.

## AP-12: Defaults defined after their consumers

**Smell:** a default is set in a late import after another file already used it.

**Impact:** consumers observe an empty or unintended earlier value.

```xml
<!-- custom.props: an early, overridable default. -->
<PropertyGroup>
  <MyToolVersion Condition="'$(MyToolVersion)' == ''">2.0</MyToolVersion>
</PropertyGroup>

<!-- custom.targets: execution using that setting. -->
<Target Name="RunMyTool">
  <Exec Command="mytool --version $(MyToolVersion)" />
</Target>
```

`.props` normally carries early defaults; `.targets` normally carries later build logic.
Timing matters more than the suffix: a value depending on a project-defined property must not
move earlier than that property exists. See AP-21 in
[additional anti-patterns](antipatterns/additional-antipatterns.md).

## AP-13: Missing guards on optional imports

**Smell:** an optional developer/environment override is imported unconditionally.

**Impact:** a fresh checkout fails because a file that is optional by contract is absent.

```xml
<!-- Guard an optional override. -->
<Import Project="$(RepoRoot)eng\local.props"
        Condition="Exists('$(RepoRoot)eng\local.props')" />
```

**Required imports must fail when missing.** Do not add an existence guard to hide a missing SDK,
required build file, or broken package contract.

**NuGet forwarders need a packed-layout check.** Files in `build`/`buildTransitive` can import
paths that do not exist in source but are created by `.nuspec` mappings, `PackagePath` metadata,
or SDK pack conventions. First inspect the effective `NuspecFile`, `NuspecBasePath`, and
`NuspecProperties`, including imports and global-property overrides. Follow the configured
nuspec even outside the project tree and resolve its mappings with those settings. Only when
no custom nuspec is configured, inspect nearby `.nuspec` files in the project and its immediate
parent as a bounded fallback. Inspect the project's packaging items too; do not search unrelated
ancestors or treat an unavailable configured nuspec as proof of a broken import.

Only flag the target if it is absent from both applicable source and projected package layouts.
See [source tree vs packed layout](extension-points.md#source-tree-vs-packed-layout).
For per-TFM forwarders, derive the matched folder from the file's own location rather than the
consumer's `TargetFramework`; see the [forwarding chain](extension-points.md#forwarding-chain).

## AP-14: Backslashes where path normalization does not apply

**Smell:** Windows separators in a path intended to reach a non-MSBuild consumer on another OS.

**Correctness issue:** raw shell strings, generated source/response files, or custom tasks that
use OS APIs directly may interpret backslashes differently.

**Not automatically a correctness issue:** `Import` paths, item globs, and file paths consumed by
built-in tasks use MSBuild path handling. An ordinary backslash-style import is not, by itself,
a Linux failure.

```xml
<!-- A raw Windows shell/tool path needs actual platform support and quoting. -->
<Exec Command="$(MSBuildThisFileDirectory)tools\release\sign.exe $(OutputPath)" />

<!-- MSBuild handles this import path. Do not report a defect solely for its separators. -->
<Import Project="$(MSBuildThisFileDirectory)..\..\build\common.props" />
```

Trace the consumer before assigning severity. `MSBuildThisFileDirectory` already ends with a
separator; use path functions when constructing paths rather than concatenating arbitrary strings.

## AP-15: Unexpected property overrides across scopes

**Smell:** a shared value is unexpectedly replaced by another unconditional assignment.

**Impact:** last-write behavior is difficult to explain and can alter output paths or options.

```xml
<!-- Directory.Build.props: a default that permits an existing value. -->
<PropertyGroup>
  <OutputPath Condition="'$(OutputPath)' == ''">bin\custom\</OutputPath>
</PropertyGroup>
```

Trace assignments and consumers first. A project overriding a repository default can be
intentional, and command-line global properties have different override semantics. Do not add
conditions everywhere or change path-defining properties too late for their SDK/NuGet consumers.

For AP-16 through AP-23 and the quick-reference checklist, read
[additional-antipatterns.md](antipatterns/additional-antipatterns.md).
