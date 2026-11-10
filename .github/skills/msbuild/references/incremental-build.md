# Optimizing MSBuild Incremental Builds

Source: original `incremental-build` skill.

## How MSBuild Incremental Build Works

MSBuild can skip targets whose outputs are up to date, reducing work on subsequent
ordinary builds.

- **`Inputs` and `Outputs`:** file-producing targets use file timestamps to
  determine freshness. Outputs must exist and be at least as new as the relevant
  inputs. One-to-one item transforms can support partial incremental execution
  rather than requiring every output to be rebuilt.
- **Without file-based checks:** a target with neither attribute has no such
  freshness test when reached. It can still be skipped by conditions or because
  it already ran for that project instance. Cheap orchestration or discovery
  targets may correctly execute on every invocation that reaches them.
- **No `Incremental` target attribute:** `Incremental="false"` is not a valid
  MSBuild `Target` attribute. Do not add it to force execution.
- **Timestamps, not hashes:** touching an input can invalidate the target even
  without content changes. Conversely, unchanged timestamps can hide changes.

```xml
<!-- A one-to-one input/output mapping -->
<Target Name="Transform"
        Inputs="@(TransformFiles)"
        Outputs="@(TransformFiles->'$(OutputPath)%(Filename).out')">
  <!-- Existing transformation task goes here -->
</Target>

<!-- No file-based freshness check when this target is invoked -->
<Target Name="PrintMessage">
  <Message Text="This runs when the target is reached" />
</Target>
```

Give transformed outputs distinct paths if inputs can have duplicate basenames.
Do not add fictitious output files merely to skip necessary work.

## Why Incremental Builds Break (Top Causes)

1. **Missing Inputs/Outputs on file-producing custom targets.** Declare meaningful
   file dependencies for expensive repeatable work, not indiscriminately for every
   orchestration target.
2. **Volatile output paths.** Timestamps, build counters, or GUIDs in a path prevent
   finding the prior output.
3. **Writes outside declared Outputs.** A target may skip even when an undeclared
   generated file is missing or stale; track all required outputs.
4. **Missing FileWrites registration.** Standard Clean tracking may miss custom
   generated files. Merely putting a file under `obj` does not register it.
5. **Glob changes.** Additions and removals alter item membership, but general
   timestamp checks do not remember the previous set. Removing an input, or
   adding an older input, need not invalidate a custom target. Use appropriate
   input-set tracking and obsolete-output cleanup when the output depends on
   membership.
6. **Property changes.** MSBuild does not automatically compare prior property
   values used by a generator. Track relevant values explicitly. Changed
   configuration/TFM/RID paths can select missing outputs, but returning to an
   already-built configuration can reuse its isolated output set.
7. **NuGet package updates.** Assets and resolved reference changes can correctly
   invalidate compilation and resolution work. Preserve necessary invalidation.
8. **Compiler-server state.** A recycled `VBCSCompiler` can make a compilation that
   executes slower. It does not by itself explain why an up-to-date `CoreCompile`
   ran; separate execution decisions from compiler warm-up.

## Diagnosing "Why Did This Rebuild?"

Use binlogs to identify exactly why a target executed instead of skipping.

### Step-by-Step Using Binlog

1. Reuse matching first/second-build artifacts if available. Otherwise follow
   [binlog generation](binlog-generation.md) and capture two ordinary builds:

   Choose unused explicit names for both logs; the names below are illustrative.

   ```powershell
   dotnet build "-bl:first.binlog"
   if ($LASTEXITCODE -ne 0) { throw "First build failed; no valid incremental baseline." }
   dotnet build "-bl:second.binlog"
   ```

   Keep configuration, TFM/RID, properties, restore policy, and inputs unchanged.
   Analyze the second successful build. `Rebuild` and `--no-incremental` force
   full work and cannot establish a no-change skip result.
2. [Replay the second binlog](binlog-failure-analysis.md#replay-a-binary-log)
   locally with diagnostic verbosity and a performance summary, substituting `second.binlog`
   for `build.binlog`. The searches below use that command's `full-01.log`; substitute the
   actual path if you chose another name. Replay the first build to a different file when comparing.
3. Find executed/skipped targets and the accompanying reasons:

   ```powershell
   Select-String -Path .\full-01.log -Pattern 'Building target|Skipping target|was not skipped|is newer than output' -Context 0,5
   ```

4. Interpret messages in context (`...` stands for the target name):
   - **`Building target "..." completely.`:** full target execution; read the reason
     instead of assuming all outputs were missing.
   - **`Building target "..." partially, because some output files are out of date
     with respect to their input files.`:** partial execution for an applicable
     input/output mapping.
   - **`Skipping target "..." because all output files are up-to-date with respect
     to the input files.`:** freshness checks permitted a skip; verify the declared
     dependency set is complete.
5. Record the precise input/output path, timestamp, missing output, or condition
   responsible, for the correct project/global-property instance.

### Additional Diagnostic Techniques

- Compare the two replayed logs for changed properties, items, paths, and reasons.
- Find expensive second-build targets:

  ```powershell
  Select-String -Path .\full-01.log -Pattern 'Target Performance Summary|Task Performance Summary' -Context 0,30
  ```

- Aggregate/nested timings are not additive wall time. Follow orchestration waits
  to actual task work using [performance diagnostics](build-perf-diagnostics.md).
- A zero-duration target that runs is not automatically a defect. Inspect its
  dependencies and actual cost before changing it.
- If evaluation dominates while compilation correctly skips, use
  [evaluation performance](eval-performance.md), not a fake incremental shortcut.

## FileWrites and Clean Build

`FileWrites` lets the standard Clean infrastructure track build-generated files.
It is separate from target freshness and compiler inclusion.

- **`FileWrites`:** register owned generated files so the normal Clean pipeline
  can discover them.
- **`FileWritesShareable`:** do not assume reference-counted ownership or protection
  because another project still uses a file. Inspect the selected Clean targets,
  locations, and shared-file ownership before relying on it.
- **Missing registration:** files can accumulate, leaving stale inputs or outputs.
  A directory named `obj` is not automatic per-file tracking.

### Pattern for Registering Generated Files

For a generator with declared input files and generated line content:

```xml
<Target Name="MyGenerator"
        Inputs="$(MSBuildProjectFullPath);@(GeneratorInput)"
        Outputs="$(IntermediateOutputPath)generated.cs">
  <WriteLinesToFile File="$(IntermediateOutputPath)generated.cs"
                    Lines="@(GeneratedLines)"
                    Overwrite="true" />
  <ItemGroup>
    <FileWrites Include="$(IntermediateOutputPath)generated.cs" />
  </ItemGroup>
</Target>
```

Ensure the directory exists and that all settings/tool/template dependencies are
modeled; project-file timestamps do not track arbitrary property values. Use
`Overwrite="true"` when replacing generated content rather than appending it.

An `ItemGroup` inside a target with an up-to-date skip can still be evaluated by
**output inference**. Thus the source pattern of registering `FileWrites` and
`Compile` inside that target is valid when the paths/items can be inferred.
Do not move it solely because the task skipped. If an item depends only on task
output that cannot be inferred, make that item's definition available separately.

Use only authorized, scoped Clean operations. Never register user-authored files
or another project's outputs merely to make cleanup more aggressive.

## Visual Studio Fast Up-to-Date Check

Visual Studio's Fast Up-to-Date Check (FUTDC) is distinct from MSBuild's
`Inputs`/`Outputs`: it can skip invoking MSBuild entirely.

- **Fast checks:** the project system examines known inputs, outputs, copy items,
  and timestamps without a full build.
- **Custom work:** custom targets, generated files, or nonstandard/dynamic items
  may not be visible to that check.
- **Diagnostic comparison:** temporarily force MSBuild checks when investigating
  a Visual Studio-only discrepancy:

  ```xml
  <PropertyGroup>
    <DisableFastUpToDateCheck>true</DisableFastUpToDateCheck>
  </PropertyGroup>
  ```

  Do not leave FUTDC disabled as a default performance fix.
- **Logging:** in Visual Studio's **Tools > Options > Projects and Solutions >
  SDK-Style Projects**, enable verbose up-to-date-check logging and inspect the
  Output window for the actual file/reason.
- **Common cases:** unregistered custom build actions, items added only during
  target execution, and changed `Content`/`None` copy items. Diagnose FUTDC's
  decision separately from MSBuild target skip reasons.

## Making Custom Targets Incremental

The following example combines C# templates under `config\$(ConfigFlavor)` into a
generated compilation input. The small state file tracks both the selected flavor
and input membership; a plain timestamp list would miss some changes to either.
Adapt the generator step and declared dependencies to the repository's real tool.

```xml
<PropertyGroup>
  <ConfigFlavor Condition="'$(ConfigFlavor)' == ''">Default</ConfigFlavor>
</PropertyGroup>
<ItemGroup>
  <ConfigInput Include="config\$(ConfigFlavor)\*.cs.in" />
</ItemGroup>

<Target Name="WriteConfigInputs">
  <MakeDir Directories="$(IntermediateOutputPath)" />
  <WriteLinesToFile File="$(IntermediateOutputPath)config.inputs"
                    Lines="Flavor=$(ConfigFlavor);@(ConfigInput->'%(FullPath)')"
                    Overwrite="true"
                    WriteOnlyWhenDifferent="true" />
  <ItemGroup>
    <FileWrites Include="$(IntermediateOutputPath)config.inputs" />
  </ItemGroup>
</Target>

<Target Name="GenerateConfig"
        DependsOnTargets="WriteConfigInputs"
        Inputs="$(MSBuildProjectFullPath);@(ConfigInput);$(IntermediateOutputPath)config.inputs"
        Outputs="$(IntermediateOutputPath)config.generated.cs"
        BeforeTargets="CoreCompile">
  <ReadLinesFromFile File="%(ConfigInput.Identity)"
                     Condition="'@(ConfigInput)' != ''">
    <Output TaskParameter="Lines" ItemName="_ConfigLines" />
  </ReadLinesFromFile>
  <WriteLinesToFile File="$(IntermediateOutputPath)config.generated.cs"
                    Lines="@(_ConfigLines)"
                    Overwrite="true" />
  <ItemGroup>
    <FileWrites Include="$(IntermediateOutputPath)config.generated.cs" />
    <Compile Include="$(IntermediateOutputPath)config.generated.cs" />
  </ItemGroup>
</Target>
```

**Key points:**

- The project file, actual input files, and state file participate in freshness.
  Also declare imported generator logic, tool binaries, templates, and relevant
  settings when they affect the real generator.
- `WriteOnlyWhenDifferent` prevents the **state file** from causing churn on every
  build. Include every relevant property/input-set value in an unambiguous,
  stable representation. Materialize valid state even when the initial input set
  is empty; comparing two empty strings must not suppress a missing state file.
- This state file records input identities and settings, not input contents.
  Keep the actual files in `Inputs` for normal timestamp-based edit detection.
  Do not describe a membership manifest as detecting content edits whose
  timestamps were preserved. Use content hashing only when that is the required
  contract; avoid a custom temporary-file/compare/copy scheme when
  `WriteOnlyWhenDifferent` already supplies the needed behavior.
- Generated output is overwritten when regeneration is necessary. If a real
  generator preserves output timestamps for identical content, use appropriate
  completion-state tracking so newer inputs do not make it execute forever;
  still detect missing real outputs.
- `IntermediateOutputPath` should isolate configurations/TFMs/RIDs; inspect custom
  overrides. `FileWrites`, not the directory name, supplies clean registration.
- `BeforeTargets="CoreCompile"` makes the file available before compilation.
  `Compile` inclusion and `FileWrites` registration can be inferred on a skip.
- Do not duplicate the generated item or rely on an evaluation-time glob to find
  a file that does not yet exist.
- For per-input generated outputs, detect removal and clean obsolete **owned**
  outputs as well as updating the input-set state.

Verify initial generation, an unchanged skip, ordinary edits, older-file additions,
removals, missing outputs, and the contract's empty-input behavior. Run every
destructive check on a disposable copy of the project and its required imports,
never on the original inputs, even if they would be restored afterward. Make the
copy byte-for-byte; `Get-Content`/`Set-Content` round-trips can change encodings
and line endings. If the copy cannot reproduce the build, report those checks as
blocked. Compare original hashes before and after as a final confirmation, not as
permission to mutate them.
Explain separately what the manifest tracks and what the file timestamps track.

### Common Mistakes to Avoid

```xml
<!-- Expensive generation without file-based freshness checks -->
<Target Name="BadTarget" BeforeTargets="CoreCompile">
  <Exec Command="generate-code.exe" />
</Target>

<!-- Volatile output path: the next invocation cannot reuse it -->
<Target Name="BadTarget2"
        Inputs="@(Compile)"
        Outputs="$(OutputPath)gen_$([System.DateTime]::Now.Ticks).cs">
  <Exec Command="generate-code.exe" />
</Target>

<!-- Stable-output pattern; declare the real tool/configuration inputs too -->
<Target Name="GoodTarget"
        Inputs="$(MSBuildProjectFullPath);@(GeneratorInput)"
        Outputs="$(IntermediateOutputPath)generated.cs"
        BeforeTargets="CoreCompile">
  <Exec Command="generate-code.exe -o &quot;$(IntermediateOutputPath)generated.cs&quot;" />
  <ItemGroup>
    <FileWrites Include="$(IntermediateOutputPath)generated.cs" />
    <Compile Include="$(IntermediateOutputPath)generated.cs" />
  </ItemGroup>
</Target>
```

These external-tool patterns assume the actual generator exists and produces the
declared file. Add property/input-set tracking when needed, as in the full example.
Do not include generated outputs in their own input set.

## Performance Summary and Preprocess

- **`-clp:PerformanceSummary`:** identifies cumulative project/target/task costs:

  ```powershell
  dotnet build -clp:PerformanceSummary
  ```

  Compare the relevant second-build work; inclusive durations are not exclusive
  CPU time.
- **`-pp:preprocess.xml`:** expands imports to locate definitions and their origin:

  ```powershell
  dotnet msbuild .\MyProject.csproj "-pp:preprocess.xml"
  ```

  It is not a dump of fully evaluated property/item values. Find the definitions
  of `Inputs`, `Outputs`, and imports, then inspect the matching build instance.
- Use the summary for what ran and `/pp` for where it was defined, together with
  replayed skip reasons.

## Common Fixes

- **Declare meaningful Inputs/Outputs** for repeatable file-producing work,
  including missing-output recovery and all real dependencies.
- **Use stable intermediate paths** while preserving configuration/TFM/RID
  isolation and avoiding output collisions.
- **Register owned generated files in FileWrites**; validate the applicable
  scoped Clean behavior rather than assuming every intermediate file is tracked.
- **Avoid unnecessary volatile data.** If per-build timestamps are required,
  preserve that contract and isolate their downstream impact. For Git revision
  state, a project-file timestamp or `.git\HEAD` alone is insufficient: branch
  refs can change without either changing. Use the repository's reliable state
  mechanism or a cheap probe that writes only changed content; report probe
  failures instead of freezing a stale value.
- **Use Returns for return-only item collections.** These are alternative
  definitions, not two targets to install together:

  ```xml
  <!-- Outputs also participates in file-based freshness -->
  <Target Name="GetFiles" Outputs="@(DiscoveredFiles)" />

  <!-- Returns supplies items without a file-based freshness contract -->
  <Target Name="GetFiles" Returns="@(DiscoveredFiles)" />
  ```

- **Choose copy metadata deliberately** using
  [CopyToOutputDirectory modes](copy-to-output-directory.md), especially when
  consumers mutate copied files.

## Validation

Compare equivalent ordinary builds using the
[baseline](build-perf-baseline.md). In an authorized disposable/scoped workspace,
verify: a no-change second build; input modification, addition, and removal;
property/template/tool changes; deletion of a real generated output; downstream
compilation and required diagnostics; relevant configuration/TFM/RID variants;
and approved Clean behavior. For copied content, include destination mutation.

Report the target/project instance, exact reason/file evidence, the correction,
before/after timings, and checks actually completed. A faster build that misses
required regeneration, leaves stale outputs, or changes an agreed metadata
contract is not a successful incremental-build fix.
