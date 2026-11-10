# Diagnosing MSBuild Evaluation Performance

Source: original `eval-performance` skill.

Evaluation is work MSBuild does before executing targets: reading project files,
processing imports, expanding globs, and establishing definitions. Find and
confirm the bottleneck first; recommend a change only when measurements justify it.

## Confirm the Problem Before Changing Anything

Engage only when evaluation is **measurably** costly. Do not act when:

- **Slowness is in compilation or target execution.** Use
  [build performance diagnostics](build-perf-diagnostics.md).
- **The complaint is unnecessary rebuilding.** Use
  [incremental-build diagnosis](incremental-build.md).
- **There is no measurement.** Reuse a matching binlog or timing summary, or
  collect one through [binlog generation](binlog-generation.md). Do not infer
  slowness solely from project syntax.
- **A pattern below appears but evaluation is already fast.** Broad globs, deep
  imports, and item-default settings are not reasons to rewrite healthy builds.

Report unmeasured patterns as observations, not causes. Prefer the smallest
targeted change; never disable SDK defaults as the first move.

## MSBuild Evaluation Phases

1. **Initial properties:** environment variables, global properties, and reserved properties.
2. **Imports and properties:** process imports and property groups in evaluation order.
3. **Item definitions:** establish metadata defaults.
4. **Items:** process `Include`, `Remove`, `Update`, and glob expansion.
5. **UsingTask definitions:** register task implementations.
6. **Target definitions:** establish targets for subsequent execution.

Evaluation precedes execution for a project instance. It can be expensive even
when compilation later skips; a source generator running in `Csc` is execution,
not project evaluation.

## Diagnosing Evaluation Performance

### Using Binlog Replay

1. [Replay the existing binary log](binlog-failure-analysis.md#replay-a-binary-log)
   locally with diagnostic verbosity and performance summaries. The search below uses that
   command's `full-01.log`; substitute the actual path if you chose another name.
2. Look for evaluation timing and instance information:

   ```powershell
   Select-String -Path .\full-01.log -Pattern 'Project Evaluation Performance Summary|Evaluation started|Evaluation finished|Project evaluation' -Context 0,20
   ```

3. Record the expensive project instance, global properties, configuration,
   TFM/RID, and invocation purpose. Distinct instances may be expected rather
   than duplicate work.
4. Inspect its imports and property/item definitions. If the capture lacks
   detailed evaluation timing, do not fabricate it from the number of imports.
   On supporting MSBuild versions, `-profileEvaluation:eval.tsv` provides a
   focused profile; record the extra instrumentation separately.

### Using /pp (Preprocess)

```powershell
dotnet msbuild .\MyProject.csproj "-pp:full.xml"
```

- Supply the same configuration/TFM/global properties as the affected instance.
- `/pp` inlines imports and shows their boundaries and definitions.
- It does **not** fully expand every property/item value or measure evaluation cost.
- Over 10,000 lines can be a clue to inspect, not proof of heavy evaluation.

For evaluated values, **MSBuild 17.8+** also supports queries such as:

```powershell
dotnet msbuild .\MyProject.csproj -p:TargetFramework=net8.0 -getProperty:Configuration,TargetFramework -getItem:Compile
```

Without a target request, this evaluates rather than builds. On older hosts, use
the available diagnostic evidence instead of assuming these query switches exist.

### Using /clp:PerformanceSummary

Add `-clp:PerformanceSummary` to the same build invocation for evaluation,
project, target, and task summaries. Keep evaluation separate from execution,
and remember that cumulative/nested/parallel durations are not additive wall time.

## Expensive Glob Patterns

Pursue these remedies only when item evaluation is slow and the globs are the
measured cause; a custom glob over a small intended tree is fine.

- Broad patterns such as `**\*.cs` can traverse large trees.
- SDK default globs have exclusions; custom globs may not share them.
- Watch for unexpected traversal of `node_modules`, `.git`, and generated
  `bin`/`obj` trees, not just the text of the pattern.
- Append appropriate `DefaultItemExcludes` rather than replacing existing
  exclusions. A custom item must actually consume the exclusion property.
- Narrow roots/file types where equivalent, such as `src\**\*.cs`.
- `EnableDefaultItems=false` is a last resort requiring an intentional
  replacement for compile/resource/content discovery and future file changes.
- Inspect evaluated `Compile` and other affected items for unexpected files.

For an offending custom item, adapt exclusions and paths to the intended input set:

```xml
<PropertyGroup>
  <DefaultItemExcludes>$(DefaultItemExcludes);**\node_modules\**;**\.git\**</DefaultItemExcludes>
</PropertyGroup>
<ItemGroup>
  <AdditionalFiles Include="src\**\*.json" Exclude="$(DefaultItemExcludes)" />
</ItemGroup>
```

Verify item equivalence and file addition/removal behavior, not just a faster scan.

## Import Chain Analysis

- Deep chains, for example over 20 levels, are candidates to inspect, not
  inherently slow builds.
- Imports require reading, parsing, and evaluation; measure the costly portion.
- Common sources include package `.props`/`.targets`, framework SDK imports,
  and `Directory.Build` chains.
- Use `/pp` import-boundary comments and file paths to understand ownership and
  nesting; do not depend on one exact comment phrase.
- Consolidate/remove imports or package-provided build content only when the
  chain is measurably costly and all property/item/target consumers are preserved.

## Multiple Evaluations

- The same project path can legitimately have multiple evaluations.
- Different global-property sets, TFM/RID/configurations, outer/inner
  multi-targeting builds, restore, and design-time builds can require distinct
  instances.
- Compare instance identity and call purpose, not just an evaluation count or a
  rule of "one evaluation per project per TFM."
- Investigate callers that introduce accidental global-property differences.
  Normalize only differences that do not change intended behavior.
- [Graph build](build-perf-baseline.md#step-5-static-graph-builds-graph) is an
  experiment for compatible graphs, not a guarantee of evaluation deduplication.

## TreatAsLocalProperty

- Allows listed global properties to be overridden within the current project.
- A local override is not automatically forwarded as a new global value to
  child projects. For example, a command-line value can still reach a child even
  though the parent locally changed that property.
- It is therefore not a blanket mechanism for removing inherited properties
  from child builds. Use explicit `MSBuild` task property controls, such as
  `Properties` or `RemoveProperties`, when that is the intended contract.
- Use it when local override semantics are genuinely needed. The number of names
  listed is not by itself evidence of measurable evaluation overhead.

## Property Function Cost

- Property functions used during evaluation run at evaluation time; expressions
  inside targets instead run when that execution reaches them.
- Most string operations are cheap.
- `$([System.IO.File]::ReadAllText(...))` can repeat file I/O on each evaluation.
- Network access and heavy computations can be costly; measure the actual
  function rather than declaring every file read a bottleneck.
- Keep evaluation-time functions fast and side-effect-free. Move work into a
  correctly incremental target only if evaluation-time consumers do not need
  its result.

## Optimization Checklist

- [ ] Inspect `/pp` definitions/imports without treating line count as timing.
- [ ] Explain evaluation counts by project-instance identity.
- [ ] Exclude large unintended directories from measured costly globs.
- [ ] Reduce measured unnecessary evaluation-time I/O.
- [ ] Simplify expensive import chains while preserving consumers.
- [ ] Compare graph mode only where statically discoverable and relevant.
- [ ] Remove `UsingTask` declarations only if their consumers and measurable cost
  justify it.

Repeat the same build scenario and evaluate the same intended item/property sets.
Check additions, changes, removals, affected configurations, and downstream
behavior. Report timing and evidence before/after; leave configuration unchanged
when evaluation is cheap, unmeasured, or the apparent gain is within noise.
