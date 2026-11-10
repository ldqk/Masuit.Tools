# Build Performance Baseline & Optimization

Source: original `build-perf-baseline` skill.

## Overview

Before optimizing a build, you need a **baseline**. Without measurements,
optimization is guesswork. This reference covers establishing baselines and
applying systematic, measured optimization techniques.

**Related references:**
- [Build performance diagnostics](build-perf-diagnostics.md) - binlog-based bottleneck identification
- [Incremental builds](incremental-build.md) - Inputs/Outputs and up-to-date checks
- [Parallel build tuning](#step-6-parallel-build-tuning) - parallelism and critical paths
- [Evaluation performance](eval-performance.md) - glob and import-chain optimization

## Step 1: Establish a Performance Baseline

Measure three scenarios to understand where time is spent. Reuse matching existing
artifacts first; use [binlog generation](binlog-generation.md) for capture and
privacy guidance. Discover the repository's actual build entry point and retain
its configuration, TFM/RID, properties, SDK/toolset, node count, and restore policy
in the examples below.

The binlog filenames below are illustrative. Choose an unused explicit filename
for every capture; do not overwrite existing evidence.

Record those settings, source revision/local changes, machine resources, cache
state, and exact input edit. Compare successful builds of the same scenario, not
a failed build, a smaller solution, or differently instrumented runs.

For an advisory before/after claim, first assess the supplied measurements. A
`Rebuild` including restore and an unchanged ordinary build without restore do
not isolate an MSBuild setting. The corrective experiment compares the **old
setting value with the new value**, keeping the invocation, restore policy,
input edit, starting output state, and instrumentation equivalent in both arms.
Do not infer the unspecified setting from a command-line difference.

Conversely, accept comparable repeated measurements as evidence for their
stated workload, with sample-size and noise limits. A checkout, a fresh binlog,
or measurements of every other build scenario are not prerequisites for
answering a self-contained advisory question.

### Cold Build (First Build)

No previous output exists for the selected build. An end-to-end cold-output
measurement includes restore, compilation, and all required work.

Use an agreed disposable workspace or explicitly authorized cleanup of known
generated paths. Inspect custom Clean behavior and shared outputs first. Do not
recursively delete every directory named `bin` or `obj`, clear shared NuGet caches,
or stop shared build servers. If safe preparation is unavailable, report the cold
scenario as unmeasured.

```powershell
# After preparing the agreed cold-output state
dotnet build -m "-bl:cold-build.binlog"
```

Record whether packages, compiler servers, and filesystem caches are already
warm. Removing build outputs does not make those caches cold.

### Warm Build (Incremental Build)

Build output exists and some files have changed. This measures how well
incremental building handles a specific change.

```powershell
dotnet build -m
if ($LASTEXITCODE -ne 0) { throw "Initial build failed; no valid warm baseline." }

# Make one recorded, reversible source/input change, then build normally
dotnet build -m "-bl:warm-build.binlog"
```

A timestamp-only touch, implementation edit, and public-API edit can propagate
differently. Use the same kind of change before and after an optimization.

### No-Op Build (Nothing Changed)

Build output exists and nothing has changed. Expensive work whose inputs and
outputs are up to date should skip. A slow no-op can still be caused by
evaluation, restore checks, reference resolution, or orchestration rather than
broken compilation incrementality.

```powershell
dotnet build -m
if ($LASTEXITCODE -ne 0) { throw "Initial build failed; no valid no-op baseline." }

dotnet build -m "-bl:noop-build.binlog"
```

Label this **warm no-change** scenario separately from a warm changed-input build.
Do not use `Rebuild`, `-t:Rebuild`, or `--no-incremental` for the second build:
forced full work is a separate benchmark, not a no-op or proof of cold caches.

### What Good Looks Like

| Scenario | Expected behavior |
| --- | --- |
| Cold build | Complete required outputs, including compilation and required generation. This is the full-work baseline for the recorded cache state. |
| Warm build | Changed inputs and affected dependencies invalidate the appropriate work; unrelated recompilation needs explanation. |
| No-op build | Up-to-date expensive compilation/generation skips. Under roughly 5 seconds for small repositories or 30 seconds for large ones can be useful triage hints, not universal budgets. |

**Red flags to investigate, not automatic diagnoses:**
- No-op build over 30 seconds: distinguish evaluation, restore, copying, and
  unexpected execution before applying [incremental fixes](incremental-build.md).
- Warm build recompiles everything: inspect the input/API change and dependency
  chain; do not assume every downstream rebuild is unnecessary.
- Long cold restore: investigate feed/network work and cache state, not just
  package-cache problems.

### Recording Baselines

Prefer repeated measurements, such as three runs per scenario, and report their
median and spread. Re-establish the correct cold or warm state before each run.
Do not mix cold and warmed timings or silently discard inconvenient outliers.

| Scenario | Before median [range] | After median [range] | Improvement |
| --- | --- | --- | --- |
| Cold build, restore included/excluded | Not measured | Not measured | Seconds and percent |
| Warm build, named input edit | Not measured | Not measured | Seconds and percent |
| No-op build, unchanged command | Not measured | Not measured | Seconds and percent |

Fill this table with actual results and artifact paths. Percentage improvement is
`(before - after) / before * 100`; a negative result is a regression. Change one
variable at a time and retain the same success/output checks. Use distinct log
names for each run and for before/after results so baseline evidence is not
overwritten.

## Step 2: Artifacts Output Layout

`UseArtifactsOutput`, introduced in **.NET SDK 8**, centralizes output layout.
It can address output-management and collision problems and simplify CI caching;
it does not itself make compilation faster or validate a cache.

### Enabling Artifacts Output

```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <UseArtifactsOutput>true</UseArtifactsOutput>
</PropertyGroup>
```

### Before vs After

Illustrative single-target layout:

```text
# Traditional layout
src\
  MyLib\
    bin\Debug\net8.0\MyLib.dll
    obj\Debug\net8.0\...
  MyApp\
    bin\Debug\net8.0\MyApp.dll

# Artifacts layout
artifacts\
  bin\MyLib\debug\MyLib.dll
  bin\MyApp\debug\MyApp.dll
  obj\MyLib\debug\...
  obj\MyApp\debug\...
```

### Benefits

- **Separated outputs:** project/configuration pivots reduce accidental overlap;
  check duplicate project names and custom path/pivot overrides.
- **Easier cache management:** a common artifacts root is easier to collect,
  but cache keys must still cover the relevant inputs, options, SDK, and platform.
- **Simpler ignore rules:** the generated artifacts root can be ignored centrally.
- **Multi-targeting:** SDK pivots distinguish TFMs/RIDs where needed; inspect the
  evaluated paths instead of assuming one fixed subdirectory pattern.

### Customizing

```xml
<PropertyGroup>
  <ArtifactsPath>$(MSBuildThisFileDirectory)output</ArtifactsPath>
</PropertyGroup>
```

The CLI's `--artifacts-path` also requires SDK 8+. Keep restore, build, test,
pack/publish, scripts, and cleanup consistent with the new locations. Verify the
supported configuration/TFM/RID matrix and file ownership before adopting the
layout. A target framework change alone does not enable this SDK feature.

## Step 3: Deterministic Builds

Deterministic compilation produces identical compiler outputs for identical
compiler inputs. This supports caching and reproducibility, but does not make
every custom build step or package reproducible automatically.

### Enabling Deterministic Builds

```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <!-- Already enabled by default in modern .NET SDK projects -->
  <Deterministic>true</Deterministic>
  <ContinuousIntegrationBuild Condition="'$(CI)' == 'true'">true</ContinuousIntegrationBuild>
</PropertyGroup>
```

Use the repository's actual CI signal; not every runner supplies `CI=true`.

### What Deterministic Affects

- Removes time-dependent variation from compiler output; it does not simply zero
  every PE timestamp field.
- Makes compiler outputs repeatable for the same complete inputs.
- Works with source/path normalization for reproducible PDBs; deterministic
  compilation alone does not normalize arbitrary machine-specific paths.

### Why It Matters for Performance

- **Build caching:** deterministic outputs can be reused when the complete cache
  key and required artifacts are correct.
- **CI optimization:** correct input tracking can avoid rebuilding unchanged work.
- **Distributed builds:** cross-machine reuse also requires compatible toolsets,
  paths, options, dependencies, and generated content.

Verify reproducibility of the required artifacts. Neither these properties nor
NuGet lock files create a compiled-output cache or excuse untracked inputs.

## Step 4: Dependency Graph Trimming

Removing genuinely unnecessary references can reduce graph/resolution work and,
when it removes a critical dependency, shorten the critical path.

### Audit the Dependency Graph

Inspect existing binlogs for project references, global-property variants, build
times, and waits. If a new capture is needed:

```powershell
dotnet build "-bl:graph.binlog"
```

Use [local replay](binlog-failure-analysis.md#replay-a-binary-log) and
[performance diagnostics](build-perf-diagnostics.md) to follow the actual work.
Inclusive `ResolveProjectReferences` time includes waiting for dependencies; it
is not all reference-resolution CPU work.

### Techniques

#### Remove Redundant Transitive References

```xml
<!-- Before: Core also references Utils -->
<ItemGroup>
  <ProjectReference Include="..\Core\Core.csproj" />
  <ProjectReference Include="..\Utils\Utils.csproj" />
</ItemGroup>

<!-- Candidate only after proving the direct Utils edge is unnecessary -->
<ItemGroup>
  <ProjectReference Include="..\Core\Core.csproj" />
</ItemGroup>
```

Transitive reachability alone is insufficient. A direct reference may express
direct API use, repository policy, or important metadata. Verify compile/runtime
dependencies and pack/publish behavior across supported TFMs/configurations.
Removing `App -> Utils` does not necessarily shorten `App -> Core -> Utils`.

#### Build-Order-Only References

When a project must build first but its assembly is not a compiler reference:

```xml
<ProjectReference Include="..\CodeGen\CodeGen.csproj"
                  ReferenceOutputAssembly="false" />
```

This preserves the build-order dependency. It does **not** remove the scheduling
edge or make the projects independent.

#### Prevent Transitive Asset Flow

When a dependency is an implementation detail that should not be exposed through
the relevant NuGet asset/pack contract:

```xml
<ProjectReference Include="..\InternalHelpers\InternalHelpers.csproj"
                  PrivateAssets="all" />
```

Inspect restored assets, generated package dependencies, and consuming projects.
`PrivateAssets` is not a generic "do not build" or "remove the compiler reference"
switch, and this metadata alone does not establish a performance improvement.

#### Disable Transitive Project References

For deliberately explicit-only dependency management in supporting SDK builds:

```xml
<PropertyGroup>
  <DisableTransitiveProjectReferences>true</DisableTransitiveProjectReferences>
</PropertyGroup>
```

**Caution:** this requires explicit references to consumed dependencies. Consider
it only for measured transitive-closure overhead, with consumer validation. It
does not eliminate required project build ordering.

## Step 5: Static Graph Builds (`/graph`)

Static graph mode constructs the project graph before building, allowing
scheduling from the declared dependencies.

### Enabling Graph Build

```powershell
dotnet build -graph
dotnet build -graph "-bl:graph-build.binlog"
```

### Benefits

- **Scheduling:** knowing the complete graph can improve parallel scheduling.
- **Isolation:** project isolation is a separate contract, selected with
  `-isolateProjects`; `/graph` alone does not guarantee it.
- **Caching potential:** results-cache options are separate features with their
  own isolation/correctness requirements, not an automatic graph-build cache.

### When to Use

| Scenario | Recommendation |
| --- | --- |
| Large multi-project solution, for example 20+ projects | Measure graph mode as a candidate; project count is not proof of a benefit. |
| Small solution, for example fewer than 5 projects | Extra graph construction may outweigh scheduling gains; measure rather than assume. |
| CI builds | Try when dependencies are statically discoverable and output isolation is sound. |
| Local development | Compare both modes in the same cold, changed-input, or no-change scenario. |

### Troubleshooting Graph Build

All relevant project dependencies must be statically discoverable. References
created inside targets or hidden in programmatic `MSBuild` calls can violate
that model. Inspect the actual emitted error rather than assuming one error code.

Declare `ProjectReference` items during evaluation where that preserves their
semantics. Do not move execution-dependent logic blindly, drop dependencies, or
disable dependency builds to make graph construction pass. If the graph cannot
represent the build correctly, retain the normal build.

## Step 6: Parallel Build Tuning

### MaxCpuCount

```powershell
# Permit up to the logical processor count
dotnet build -m

# Explicit budget, useful on shared CI agents
dotnet build -m:4

# MSBuild.exe syntax
msbuild -m:8 .\MySolution.sln
```

Raw MSBuild defaults to one node without `-m`; the `dotnet build` driver can
supply switches itself. Inspect the actual invocation and record an explicit
budget when comparing runs. Respect memory limits, CPU quotas, and other users.

### Identifying Parallelism Bottlenecks

In recorded project-instance and scheduling evidence, look for:
- **Long sequential chains:** projects waiting on required predecessors.
- **Uneven load:** idle nodes while others remain busy.
- **Single-project bottleneck:** one long project on the critical path.

Name the chain with durations, for example `Core -> Api -> Web -> Tests`.
Aggregate target/task summaries alone cannot establish node utilization: totals
can overlap, nest, or include waits. If replay lacks sufficient scheduling detail,
report that gap rather than inventing utilization percentages.

For independent requests in a custom orchestration target, enable parallelism
on the `MSBuild` task as well as providing multiple nodes:

```xml
<MSBuild Projects="@(ProjectsToBuild)" Targets="Build" BuildInParallel="true" />
```

### Reducing the Critical Path

1. Split a measured large bottleneck only if the resulting work can run independently.
2. Remove proven unnecessary references using Step 4's compatibility checks.
3. Use `ReferenceOutputAssembly="false"` only for genuine build-order-only intent;
   it can reduce assembly-reference work, not the dependency wait itself.
4. Consider a shared base library only if the resulting graph is better; a new
   common prerequisite can also lengthen the serial chain.

More nodes cannot parallelize a required dependency chain and can worsen
memory- or I/O-bound workloads.

## Step 7: Additional Quick Wins

Treat these as candidates selected by evidence, not a list to apply wholesale.

### Separate Restore from Build

```powershell
dotnet restore
if ($LASTEXITCODE -ne 0) { throw "Restore failed; do not use stale assets." }
dotnet build --no-restore -m
if ($LASTEXITCODE -ne 0) { throw "Build failed; do not test stale outputs." }
dotnet test --no-build
```

Use matching configuration/TFM/RID and restore-affecting properties in each stage.
`--no-restore` requires current assets; `--no-build` requires successfully built
matching test outputs. Keep restore-inclusive and build-only timings separate.

### Skip Unnecessary Targets

For an explicitly agreed development scenario where these outputs/checks are
optional:

```powershell
dotnet build -p:GenerateDocumentationFile=false
dotnet build -p:RunAnalyzers=false
```

Measure their actual cost first. Preserve required XML documentation in packages,
required analyzer diagnostics in CI, and generator-produced code.
[Analyzer diagnostics](build-perf-diagnostics.md#2-roslyn-analyzers-and-source-generators)
covers conditional policy and global analyzer references. Do not silently call
disabled validation behavior-preserving.

### Use Project-Level Filtering

```powershell
dotnet build .\src\MyApp\MyApp.csproj
```

Build the project and its dependencies when that is the intended inner loop.
Solution filters can similarly select a supported subset. A smaller selection
is a different scenario, not proof that the full solution became faster; retain
the required integration and test coverage.

### Binary Log for All Investigations

Start with an existing matching binlog or capture through
[binlog generation](binlog-generation.md):

```powershell
dotnet build -m "-bl:perf.binlog"
```

Then use [build performance diagnostics](build-perf-diagnostics.md) to identify
the bottleneck before applying a change.

## Optimization Decision Tree

```text
Is the no-op build slow?
  YES -> Classify evaluation, restore, copies, and executed work first.
         Unexpected execution -> incremental-build.md.
         Slow evaluation -> eval-performance.md.
  NO / after classification:
    Is the cold build slow?
      Restore -> investigate feeds/cache/inputs; compare restore separately.
      Compilation -> diagnose analyzers/generators and critical-path work.
      Other targets -> inspect their recorded reasons and task costs.
    Is the warm changed-input build slow?
      Unexpected propagation -> incremental-build.md and dependency analysis.
      Required serial work -> inspect critical path and parallelism above.
    Are all scenarios healthy for the actual workload?
      Stop. Do not change healthy configuration just to apply a tuning flag.
```

Use the linked [incremental](incremental-build.md),
[evaluation](eval-performance.md), and [diagnostics](build-perf-diagnostics.md)
references for those branches.

## Validation and Results

Repeat the same scenario and input change with the same instrumentation. Verify
required outputs, downstream consumers, diagnostics, and relevant tests or
pack/publish behavior. For tracking changes, exercise input addition/change/removal
and missing outputs as described in [incremental builds](incremental-build.md).
For copy changes, verify [copy-mode semantics](copy-to-output-directory.md).

Report the command/environment, before/after median and spread, artifact paths,
one causal change, correctness checks, and unmeasured scenarios. Stop or revert
only your own experimental change if the benefit is within noise, the comparison
cannot succeed, or correctness regresses.
