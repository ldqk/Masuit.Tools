# Additional MSBuild anti-patterns

This continues the original catalog with AP-16 through AP-23. The same rule applies:
verify the behavior and exceptions before changing a pattern.

## AP-16: Exec for string or path operations

**Smell:** invoking a shell or another runtime for simple transformations.

**Impact:** platform dependence, process overhead, and awkward property capture.

```xml
<!-- BAD: a shell pipeline for a simple transformation. -->
<Target Name="GetCleanVersion">
  <Exec Command="echo $(Version) | sed 's/-preview//'" ConsoleToMSBuild="true">
    <Output TaskParameter="ConsoleOutput" PropertyName="CleanVersion" />
  </Exec>
</Target>

<!-- GOOD: property functions, where the transformation is the intended contract. -->
<PropertyGroup>
  <CleanVersion>$(Version.Replace('-preview', ''))</CleanVersion>
  <HasPrerelease>$(Version.Contains('-'))</HasPrerelease>
  <LowerName>$(AssemblyName.ToLowerInvariant())</LowerName>
  <NormalizedOutput>$([MSBuild]::NormalizeDirectory('$(OutputPath)'))</NormalizedOutput>
  <ToolPath>$([MSBuild]::NormalizePath('$(MSBuildThisFileDirectory)', 'tools', 'mytool.exe'))</ToolPath>
</PropertyGroup>
```

Preserve transformation semantics and evaluation timing. This does not mean arbitrary external
tools or stateful operations should become property functions.

## AP-17: Updating an item before it exists

**Smell:** `Update` is evaluated before the intended item is included.

**Impact:** the metadata change matches no existing item.

```xml
<!-- BAD: no matching item exists at the update. -->
<ItemGroup>
  <Extra Update="external.dat" Visible="false" />
  <Extra Include="external.dat" />
</ItemGroup>

<!-- GOOD: Include then Update in the SAME group is valid. -->
<ItemGroup>
  <Extra Include="external.dat" />
  <Extra Update="external.dat" Visible="false" />
</ItemGroup>
```

The original catalog incorrectly treated mixing Include and Update in one ItemGroup as a
defect. Order and existence are what matter; splitting groups is neither necessary nor a fix
for an update that still precedes its include.

## AP-18: Apparently redundant transitive project references

**Smell:** `App` references both `Core` and `Utils`, and `Core` also references `Utils`.

**Impact:** a truly unnecessary edge can add coupling or resolution work, but transitive
reachability alone does not prove redundancy or an ordering bug.

```xml
<!-- Candidate for review, not automatically incorrect. -->
<ItemGroup>
  <ProjectReference Include="..\Core\Core.csproj" />
  <ProjectReference Include="..\Utils\Utils.csproj" />
</ItemGroup>
```

Keep `App -> Utils` if App uses its API directly, needs its metadata/build-order contract, or
repository policy requires explicit dependencies. Remove an edge only after verifying equivalent
compile/runtime/pack behavior and supported SDK/framework configurations. Removing this edge does
not itself shorten the required `App -> Core -> Utils` reference chain.
Use [build-perf-baseline](../build-perf-baseline.md) for a measured graph experiment.

## AP-19: Side effects during evaluation

**Smell:** property evaluation writes files, mutates state, or performs network operations.

**Impact:** evaluation can repeat during IDE/design-time and graph discovery, making side effects
unpredictable. Some such functions are not permitted by default; do not loosen restrictions to
make a side-effecting property expression work.

```xml
<!-- BAD, and not a generally permitted property function. -->
<PropertyGroup>
  <Timestamp>$([System.IO.File]::WriteAllText('stamp.txt', 'built'))</Timestamp>
</PropertyGroup>

<!-- Execution-time work belongs in a properly scheduled target. -->
<Target Name="WriteTimestamp" BeforeTargets="Build">
  <WriteLinesToFile File="stamp.txt" Lines="built" Overwrite="true" />
</Target>
```

Moving work to a target still requires deciding its inputs, outputs, and whether it must run
every time. For evaluation-time read/CPU cost, use [eval-performance](../eval-performance.md).

## AP-20: Platform-specific Exec without a platform contract

**Smell:** `chmod`, `cmd /c`, or a platform-specific executable is invoked on every OS.

**Impact:** the wrong shell/tool or path syntax fails on another platform.

```xml
<Target Name="MakeExecutable" AfterTargets="Build"
        Condition="!$([MSBuild]::IsOSPlatform('Windows'))">
  <Exec Command="chmod +x &quot;$([MSBuild]::NormalizePath('$(OutputPath)', 'mytool'))&quot;" />
</Target>
```

Prefer a portable built-in task when equivalent. Otherwise gate supported platforms, quote
paths, and confirm that skipping the operation on other systems preserves their required behavior.
An OS condition is not a substitute for implementing a capability the other platform also needs.

## AP-21: An early property condition reads TargetFramework too soon

**Smell:** a PropertyGroup or property in an early `.props` file conditions on a framework that
the project body has not set yet.

**Impact:** a single-targeted project can silently miss the assignment. A multi-targeted inner
build may instead receive TargetFramework globally; distinguish those cases.

```xml
<!-- BAD in an early Directory.Build.props when the project sets TargetFramework later. -->
<PropertyGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>
</PropertyGroup>

<!-- The same assignment can be placed after the project defines the framework,
     for example in Directory.Build.targets for a single-targeted/inner build. -->
<PropertyGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>
</PropertyGroup>
```

An outer cross-targeting build may still have no single TargetFramework. Check the intended
instance, and preserve existing constants rather than replacing them.

Move the dependent assignment, not an intentionally project-owned framework, and confirm its
consumer has not already used the value before the new location. For unknown evaluated values,
use the [property/item queries](../eval-performance.md), retaining the original global properties;
preprocessed XML alone does not prove which conditions selected properties or items.

**Item conditions are different:** items are evaluated after properties. These early-file
patterns can be correct and must not be flagged merely for using TargetFramework:

```xml
<!-- Valid in Directory.Build.props when the project later sets this framework. -->
<ItemGroup Condition="'$(TargetFramework)' == 'net472'">
  <PackageReference Include="System.Memory" />
</ItemGroup>

<!-- Valid per-framework package version selection. -->
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.11" />
</ItemGroup>
```

Target conditions are evaluated at execution time. Do not move all item/target conditions just
because an early **property** condition is wrong.

## AP-22: Project instances with path-neutral global properties

**Smell:** an MSBuild task builds/publishes the same project, or a referenced project, with an
extra global property that does not isolate its output paths.

```xml
<!-- Both shapes can create a distinct instance sharing existing bin/obj paths. -->
<MSBuild Projects="$(MSBuildProjectFullPath)" Targets="Publish" Properties="_IsPublishing=true" />
<MSBuild Projects="..\tool\tool.csproj" Targets="Publish" Properties="_IsPublishing=true" />
```

**Impact:** instance identity includes the project path and global properties. Distinct instances
can write the same assemblies, PDBs, assets files, or generated files, producing duplicated work
and intermittent parallel-build locks.

Confirm the actual global-property differences and evaluated output/intermediate paths using
[replayed build evidence](../binlog-failure-analysis.md). A property difference alone is not a
collision if every relevant output is isolated.

**Repair choices depend on the build contract:**

1. Remove an unnecessary second invocation/property discriminator.
2. For work that belongs to one instance, schedule a suitable target in that instance, after
   checking its dependency graph for cycles.
3. Make a producer own its artifacts, and have consumers sequence and consume them rather than
   re-publishing the same project under a different property set.
4. If different instances are intentional, isolate all relevant outputs/intermediate paths
   early enough for SDK and NuGet consumers, and use matching restore/build settings.

**Publish-on-build needs special care.** The original example used a private `_IsPublishing`
guard and `AfterTargets="Build" DependsOnTargets="Publish"`. That can work through `dotnet build`
and `dotnet publish` yet produce `MSB4006` through `dotnet msbuild -t:Publish`, because Publish
already depends on Build. It is not a universal repair.

Prefer an explicit publish operation when publish is the requested outcome, choosing an unused
log name first:

```powershell
dotnet publish .\Tool\Tool.csproj -c Release "-bl:publish-01.binlog"
```

If a repository genuinely needs publish-on-build orchestration, validate its target graph and
all supported entry commands; do not blindly substitute private SDK flags or add a cycle.
A build-order-only reference can sequence a producer:

```xml
<ProjectReference Include="..\Tool\Tool.csproj" ReferenceOutputAssembly="false" />
```

That reference schedules the producer's **build**, not automatically its Publish target.
The producer's actual artifact workflow must still exist. Retest the original parallel/graph
scenario; serialization or repeated cleaning is not a permanent collision fix.

## AP-23: Re-injecting a single-targeted project's own framework

**Smell:** a reference injects the TFM that a single-targeted project already declares.

```xml
<!-- Tool already declares TargetFramework=net8.0. -->
<ProjectReference Include="..\Tool\Tool.csproj" SetTargetFramework="TargetFramework=net8.0" />
```

**Impact:** a globally supplied TargetFramework can create an instance different from the ordinary
build, even though both resolve to the same framework output paths. Confirm that duplicate
instances actually share paths before attributing file-lock failures to this metadata.

```xml
<!-- Remove the redundant injection when the tool should build as declared. -->
<ProjectReference Include="..\Tool\Tool.csproj" />
```

Do not remove legitimate framework selection:

- Selecting one TFM of a multi-targeted reference can be intentional.
- Overriding a single-targeted reference to a **different** framework can be intentional if the
  project, dependencies, restore, and output paths support that contract.
- Different frameworks/configurations do not guarantee isolation when custom paths omit their
  discriminators; inspect actual paths.

### Build-only references across incompatible frameworks

An incompatible assembly cannot become a compiler reference. A build-order-only tool reference
may need both `ReferenceOutputAssembly="false"` and `SkipGetTargetFrameworkProperties="true"`.
Bypassing negotiation can also let the caller's global TargetFramework leak into the tool.

To build a single-targeted tool as declared:

```xml
<ProjectReference Include="..\Tool\Tool.csproj"
                  SkipGetTargetFrameworkProperties="true"
                  UndefineProperties="TargetFramework"
                  ReferenceOutputAssembly="false" />
```

When an explicit TFM must be selected instead, use `SetTargetFramework` with the intended value.
Do not simultaneously set and remove the same property. Do not apply this bypass to ordinary
compatible assembly references, and verify the tool's actual framework/output in the log.

## Quick-reference checklist

| Source | Check first | Typical concern |
| --- | --- | --- |
| AP-02 | Unsafe string conditions | Parsing/correctness |
| AP-19 | Evaluation side effects | Repeated or unsafe work |
| AP-21 | Early framework-dependent properties | Silent missing settings |
| AP-22, AP-23 | Distinct instances sharing outputs | Duplicate builds/races |
| AP-03 | Machine-specific paths | Portability |
| AP-06 | Legacy package HintPaths | Dependency migration |
| AP-07 | Unwanted consumer dependencies/assets | Package contract |
| AP-11 | Incomplete generation inputs/outputs | Stale or repeated work |
| AP-13 | Optional versus required imports | Missing dependency behavior |
| AP-05 | Duplicate source items versus language/order exceptions | Item correctness |
| AP-04 | Proven redundant defaults versus intentional pins | Maintainability |
| AP-08, AP-09 | Shared settings or unintended version drift | Consistency |
| AP-01, AP-16 | Shell operations replaceable by built-ins | Portability/overhead |
| AP-14, AP-20 | Actual path consumer and platform support | Cross-platform behavior |
| AP-10 | Mixed target responsibilities | Scheduling/incrementality |
| AP-12, AP-15 | Assignment timing and intended overrides | Property contracts |
| AP-17 | Update before Include, not merely a shared ItemGroup | Missing metadata |
| AP-18 | Real direct dependencies versus redundant edges | Graph/asset contracts |
