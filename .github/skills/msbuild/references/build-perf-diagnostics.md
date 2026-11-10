# Diagnosing MSBuild Build Performance

Source: original `build-perf-diagnostics` skill.

## Performance Analysis Methodology

1. Establish the relevant cold, warm changed-input, or no-change
   [baseline](build-perf-baseline.md). Keep `Rebuild` and ordinary `Build` distinct.
2. Use an existing matching binlog first. If one is missing, follow
   [binlog generation](binlog-generation.md) without changing the reported scenario.
3. [Replay the binary log](binlog-failure-analysis.md#replay-a-binary-log) locally
   to a diagnostic text log with a performance summary. The searches below assume
   that log is named `full-01.log`, as in the replay command; use its actual path.
4. Find expensive projects, targets, and tasks, then inspect the corresponding
   instance, execution reason, inputs, and dependency waits. Select one cause
   for a controlled change, not every plausible optimization.

Performance summaries report cumulative/inclusive durations, not additive shares
of elapsed wall time or CPU usage. Parallel work overlaps; orchestration targets
and tasks can include child work and waits.

In particular, **`ResolveProjectReferences` can mostly represent waiting for
dependent builds**, while a yielded node performs other work. Do not optimize it
solely because it tops the target summary. Follow the dependencies to actual
`Csc`, `ResolveAssemblyReference`, `Copy`, or custom-task work. Even `MSBuild`
task duration can include child builds rather than exclusive self-time.

## Key Metrics and Thresholds

Use these as investigation hints, not universal budgets or proof of a cause:

- **Build duration:** historical rough examples are under 10 seconds for a small
  workload, 60 seconds for a medium one, and 5 minutes for a large one. Compare
  the repository's actual scenario and machine, not labels alone.
- **Node utilization:** activity below roughly 80% warrants checking dependency
  waits, limited parallel work, and resource contention. Text-summary totals
  alone cannot establish utilization.
- **Single-target domination:** investigate a target near or above 50% of elapsed
  build time, but separate inclusive waits and nested work.
- **Analyzer versus compiler time:** around 30% or more of `Csc` time deserves
  investigation. Analyzer timings can overlap; do not add them as wall time or
  remove diagnostics solely to reach a percentage.
- **RAR time:** over 5 seconds per project, especially over 15 seconds, merits
  examination of reference count, paths, and resolution work.

## Common Bottlenecks

### 1. ResolveAssemblyReference (RAR) Slowness

- **Symptoms:** substantial `ResolveAssemblyReference` task time, for example
  over 5 seconds per affected project.
- **Root causes:** many assembly references, network-based paths, or expensive
  assembly search paths.
- **Fixes:** remove demonstrated unused references/search paths and avoid
  unnecessary network resolution, preserving all consumers.
- **Diagnostic guardrail:** `DesignTimeBuild`, `ResolveAssemblyReferencesSilent`,
  and `ResolveAssemblyWarnOrErrorOnTargetArchitectureMismatch` affect build or
  diagnostic behavior; changing them is not an established RAR performance fix.
- **Key insight:** RAR can legitimately execute on no-change builds to account
  for external resolution state, including installed targeting packs or GAC
  assemblies. Do not force it to skip using an incomplete freshness check.
- **Reduce transitive references carefully:** `DisableTransitiveProjectReferences`
  is a last resort requiring explicit references to consumed dependencies.
  `ReferenceOutputAssembly="false"` is appropriate when only build ordering is
  needed, but still retains that ordering. Trim only unused package references.
  Use the [dependency-graph audit](build-perf-baseline.md#step-4-dependency-graph-trimming).

### 2. Roslyn Analyzers and Source Generators

- **Symptoms:** `Csc` takes much longer than a comparable compilation, sometimes
  more than twice a measured compiler-only scenario.
- **Diagnosis:** inspect `Csc` and analyzer/generator timing in the replayed log.
  If timing detail is absent, obtain compiler-supported diagnostic timing rather
  than guessing from package count. A controlled `-p:RunAnalyzers=false`
  comparison must compile the same inputs on both sides, not compare a no-op
  against forced compilation.
- **Conditional development policy:** after measuring and agreeing the policy,
  examples include:

  ```xml
  <PropertyGroup>
    <RunAnalyzers Condition="'$(ContinuousIntegrationBuild)' != 'true'">false</RunAnalyzers>
    <EnforceCodeStyleInBuild Condition="'$(ContinuousIntegrationBuild)' == 'true'">true</EnforceCodeStyleInBuild>
  </PropertyGroup>
  ```

- **Per-configuration:** if development Debug builds are the intended scope, also
  guard the CI case, for example
  `Condition="'$(Configuration)' == 'Debug' and '$(ContinuousIntegrationBuild)' != 'true'"`.
  CI can build Debug too.
- **Rule/package scope:** configure genuinely redundant or expensive diagnostics
  appropriately, including `.editorconfig` severity policy, instead of blindly
  removing analyzer packages.
- **Key principle:** preserve required analyzer enforcement in CI. Verify effective
  properties and that the pipeline actually sets the CI signal. A conditional
  assignment does not undo an earlier or later unconditional override.
- **Source generators:** do not assume `RunAnalyzers=false` disables or measures
  every generator. Preserve required generated code and inspect generator-specific
  evidence.
- **GlobalPackageReference:** examine the project, `Directory.Build.props`,
  `Directory.Packages.props`, and package imports. Active global analyzer
  references can affect production and test projects; a declaration alone does
  not prove restoration or inclusion in the compiler's analyzer list.
- **EnforceCodeStyleInBuild:** when enabled it adds code-style checks to compiler
  execution; it does not mean analyzers execute when compilation itself skips.
  Conditionalize only under an agreed development/CI policy.

### 3. Serialization Bottlenecks (Single-threaded Targets)

- **Symptoms:** one project dominates while other nodes are idle or waiting.
- **Common culprits:** a long dependency chain, a critical-path project, or custom
  orchestration issuing independent project requests sequentially.
- **Fixes:** optimize the measured critical path, remove false dependencies, or
  split a bottleneck only when it exposes independent work. In custom `MSBuild`
  tasks, `BuildInParallel="true"` also needs multiple available nodes.
- Follow [parallel build tuning](build-perf-baseline.md#step-6-parallel-build-tuning).
  More nodes cannot parallelize required serial dependencies.

### 4. Excessive File I/O (Copy Tasks)

- **Symptoms:** high Copy aggregate time or excessive copy invocations.
- **Root causes:** thousands of unnecessary files, network destinations, or
  accidental per-item task batching instead of an appropriate item batch.
- **First fixes:** inspect item metadata, source/destination paths, and invocation
  counts. Reduce only unnecessary copies and choose the correct
  [copy mode](copy-to-output-directory.md); replacing `Always` blindly with
  `PreserveNewest` can leave mutated destination files stale.
- **Skip unchanged files:** inspect whether the actual target consumes
  `SkipCopyUnchangedFiles`; it may already be true. Custom `Copy` tasks expose
  `SkipUnchangedFiles`. Neither setting is a content-hash comparison.
- **Hardlinks:** identify the hot copy target in the binlog, then set only the
  property that target passes to `Copy.UseHardlinksIfPossible`:
  `CreateHardLinksForCopyFilesToOutputDirectoryIfPossible` for primary build
  outputs, `CreateHardLinksForAdditionalFilesIfPossible` for content/additional
  files, or `CreateHardLinksForCopyLocalIfPossible` for copy-local references.
  Custom `Copy` tasks need their own `UseHardlinksIfPossible` value. Hardlinks
  can help compatible same-volume, immutable-file workloads. A destination
  mutation through a hardlink can modify its source, so do not use this for
  writable/resettable content.
- **Shared layout:** `UseCommonOutputDirectory=true` assumes a genuinely shared,
  collision-free output layout. Verify ownership and consumers first.
  [Artifacts output](build-perf-baseline.md#step-2-artifacts-output-layout), including
  `--artifacts-path`, requires SDK 8+ and is a separate measured layout change.
- **Dev Drive:** on supported Windows development or self-hosted CI machines,
  evaluate Dev Drive for a measured I/O bottleneck. Do not promise automatic
  copy-on-write gains for every task or disable security tooling for a benchmark.

### 5. Evaluation Overhead

- **Symptoms:** significant startup/evaluation time before target execution.
- **Root causes:** expensive `Directory.Build.props` logic, globs traversing
  large trees, imports, or SDK-resolution overhead. NuGet-based SDK resolver
  latency depends on the toolset and environment; historical timings are not a
  fixed per-project cost.
- **Fixes:** simplify only measured costly logic and narrow offending globs.
  `EnableDefaultItems=false` is a last resort requiring complete intentional item
  lists, not the first optimization. Changing SDK resolution must preserve the
  build's actual SDK contract.
- Use [evaluation performance](eval-performance.md) for focused evidence and fixes.

### 6. NuGet Restore in Build

- **Symptoms:** restore contributes substantial work repeatedly. Distinguish an
  up-to-date check from actual repeated network/dependency-resolution work.
- **Separate restore from build:** successfully restore matching inputs, then
  use `dotnet build --no-restore`. Keep build-only and restore-inclusive baselines
  distinct; dependency changes require fresh assets.
- **Static graph restore:** `RestoreUseStaticGraphEvaluation=true` in
  `Directory.Build.props` is a candidate for a measured large restore graph.
  Validate equivalent resolved assets and toolset support. It is distinct from
  graph build and its benefit is workload-dependent.
- Investigate changing inputs and feed/cache behavior rather than purging shared
  package caches.

### 7. Large Project Count and Graph Shape

- **Symptoms:** many tiny projects add evaluation/invocation overhead, or deep
  dependency chains serialize otherwise parallelizable work.
- **Consider:** graph mode for scheduling, consolidating small leaves for measured
  overhead, or splitting a large bottleneck into independent pieces. These are
  different remedies for different evidence.
- **Graph shape matters:** wide independent branches expose parallelism; a long
  required chain remains serial regardless of node count.
- **Actions:** identify the exact unnecessary edge or costly project. Preserve
  direct API/metadata contracts and compare the resulting critical path.
  [Static graph builds](build-perf-baseline.md#step-5-static-graph-builds-graph)
  do not automatically provide isolation or a cache.

## Using Binlog Replay for Performance Analysis

1. [Replay the existing binlog](binlog-failure-analysis.md#replay-a-binary-log)
   with diagnostic verbosity and a performance summary.
2. Read the target/task summaries, usually near the end of the replayed log:

   ```powershell
   Select-String -Path .\full-01.log -Pattern 'Target Performance Summary|Task Performance Summary' -Context 0,50
   ```

   Use the cumulative timings to select candidates, not to compute additive
   percentages of elapsed build time.
3. Find per-project timings and instances:

   ```powershell
   Select-String -Path .\full-01.log -Pattern 'Done Building Project|Project Performance Summary' -Context 0,20
   ```

4. Inspect recorded scheduling evidence:

   ```powershell
   Select-String -Path .\full-01.log -Pattern 'node.*assigned|building with|scheduler'
   ```

   These messages alone are not a utilization measurement. Report missing
   scheduling/timestamp detail rather than inferring an exact timeline.
5. Check analyzer timing, when the original capture contains it:

   ```powershell
   Select-String -Path .\full-01.log -Pattern 'Total analyzer execution time|analyzer.*elapsed|CompilerAnalyzerDriver'
   ```

6. Drill into the relevant project's slow target and its tasks:

   ```powershell
   Select-String -Path .\full-01.log -Pattern 'Target "CoreCompile"|Target "ResolveAssemblyReferences"' -Context 0,20
   ```

## Quick Wins Checklist

Choose only items supported by the diagnosis, one experiment at a time:

- [ ] Confirm the intended `-m` node budget rather than assuming host defaults.
- [ ] Separate restore and build only with current matching assets.
- [ ] Measure static graph restore and verify equivalent assets.
- [ ] Consider hardlinks only for compatible immutable-file workloads.
- [ ] Scope analyzer changes to an agreed development policy; retain CI enforcement.
- [ ] Inspect `ProduceReferenceAssembly` before enabling it; many SDK configurations
  already enable it. Verify implementation-only and public-API changes downstream.
- [ ] Check [incremental behavior](incremental-build.md) on an ordinary second build.
- [ ] Inspect evaluated output/intermediate paths for collisions between projects,
  configurations, TFMs, and RIDs; do not just give everything one common directory.
- [ ] Measure graph mode for a statically discoverable multi-project build.
- [ ] Evaluate SDK 8+ artifacts layout only with its path-consumer checks.
- [ ] Consider a separate Dev Drive environment experiment for Windows I/O limits.

## Impact Categorization

- **HIGH IMPACT:** a verified critical-path cost above roughly 10% of elapsed
  build time, or substantial real work behind a target reported above 50%.
- **MEDIUM IMPACT:** verified costs around 2-10%.
- **QUICK WINS:** low-effort changes with demonstrated modest benefit, not
  unmeasured property toggles.

Report the project/TFM, target/task, evidence, one targeted change, and a
same-scenario before/after comparison with spread. Verify required artifacts,
diagnostics, and relevant tests/consumers. Stop if the result is unmeasured, within
noise, or breaks correctness; do not claim success from disabled validation or
inclusive timing arithmetic.
