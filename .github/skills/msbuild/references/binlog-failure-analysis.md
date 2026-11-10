# Analyzing MSBuild failures with binary logs

Based on the original `binlog-failure-analysis` skill. Use MSBuild's built-in replay to diagnose
an existing `.binlog`, including when the original checkout is unavailable.

## Before analysis

- Reuse a log that records the relevant invocation. If none exists and the build can be reproduced,
  follow [binlog-generation](binlog-generation.md) with the original arguments.
- A `.binlog` is binary. Do not use a text reader, `strings`, or text search directly on it.
- Recorded paths identify the original build; they are not permission to search unrelated
  checkouts or proof that source/project files exist locally.
- Synthesize findings as evidence emerges. Stop once actionable causes and remaining uncertainty
  are clear rather than spending the whole investigation collecting unrelated events.

## Replay a binary log

Choose unused text-log filenames, substitute the supplied binary-log path, and replay it:

```powershell
dotnet msbuild ".\build.binlog" -noconlog -fl "-flp:logfile=full-01.log;verbosity=diagnostic;PerformanceSummary" -fl1 "-flp1:logfile=errors-01.log;errorsonly" -fl2 "-flp2:logfile=warnings-01.log;warningsonly"
```

Quote each complete semicolon-delimited logger argument, especially in PowerShell. The command
reads the recorded build; it does not restore or rebuild its projects. A compatible `MSBuild.exe`
can replace `dotnet msbuild`; no analysis server or extra package is required.

Confirm the command completed and the diagnostic log is nonempty. Errors-only or warnings-only
logs can legitimately be empty. If the installed MSBuild cannot read the binary-log format,
report that blocker and the need for compatible tooling instead of pretending analysis succeeded.

Replay success is **not** proof that the recorded build succeeded. Replay duration is **not**
the recorded build's elapsed time. The same replay mechanics serve
[performance diagnostics](build-perf-diagnostics.md): use recorded events and the Project,
Target, and Task Performance Summaries, not the time spent converting the log.

## Inspect the replayed text

Start with errors, then search the diagnostic text around the actual codes, targets, and projects:

```powershell
Get-Content -LiteralPath .\errors-01.log
Select-String -LiteralPath .\full-01.log -Pattern 'CS0246', 'CoreCompile', 'Build FAILED' -Context 2
```

The patterns above are examples, not a filter that should discard other error codes. Inspect the
recorded evidence needed to explain the failure:

- Build errors and warnings, including custom task errors.
- Project-instance identity, configuration, target framework, and global properties.
- Property assignments and item/reference metadata visible in the log.
- Evaluation, import, target, and task execution or skip messages.

Text replay cannot manufacture data that was not recorded and does not guarantee extraction of
embedded project/import source. Read source only when it is available in the current checkout.
If the responsible definition is absent from both local source and replayed text, state that
limitation and request the specific missing evidence; do not invent an exact edit location.

## Find independent causes

1. Associate each error with its project **instance**, target, and task. The same project path
   under different global properties can represent distinct builds.
2. Follow producer/consumer dependencies. Parallel log order is not causal order: a missing
   referenced assembly may be downstream of a failed producer, while another project has an
   independent failure.
3. Inspect the actual values seen by the failing task rather than assuming the project body's
   value won. Check conditions, imports, item metadata, and command-line global properties.
4. Connect each root cause to a minimal repair. A namespace error alone does not prove which
   package should be added; confirm the relevant references and target framework.

Use [msbuild-antipatterns](msbuild-antipatterns.md) for a demonstrated authoring defect,
[extension-points](extension-points.md) for imports/hooks or packed-layout issues, and
[incremental-build](incremental-build.md) for stale outputs or incorrect skip decisions.
Do not turn failure diagnosis into an unrequested modernization pass.

## Verify and report

If source can be changed, rerun the original failing scenario with a new binary log, preserving
its configuration/framework/properties. Confirm the independent root errors and their cascades
are resolved, and exercise affected dependents. Check a no-change build when targets/items/outputs
were changed.

For artifact-only analysis, provide proposed changes and explicitly say they were not applied
or rebuilt. Report the evidence, root causes versus downstream failures, minimal repair, and
actual verification status. Treat replayed text as sensitive diagnostic data just like its binlog.
