# Choosing a CopyToOutputDirectory Mode

Source: original `copy-to-output-directory` skill.

## Overview

`CopyToOutputDirectory` metadata controls whether an item such as `Content`,
`None`, `EmbeddedResource`, or `Compile` is copied next to the build output.
`CopyToPublishDirectory` controls the corresponding publish selection. Picking
the wrong mode can leave stale files or add recurring copy work.

As of **MSBuild 17.13 / .NET SDK 9.0.2xx**, the common build targets support four
values. Check the publishing SDK separately for publish behavior.

| Mode | Copies when | Incremental cost | Typical use |
| --- | --- | --- | --- |
| `Never` | Never through this copy pipeline | None | Files not needed in that output |
| `PreserveNewest` | Source is newer than destination, or destination is missing | Timestamp checks | Normal source-edited content |
| `Always` | Every build that reaches the copy target, unless a skip-unchanged override is enabled | Repeated copies in classic mode | A real unconditional-copy/reset requirement |
| `IfDifferent` | Timestamp or size differs in either direction, or destination is missing | Timestamp and size checks | Destination may be mutated between builds |

Omitted metadata commonly means `Never`, but SDKs/imports can supply item-specific
defaults. Inspect effective metadata before assuming omission disables copying.

The following `Include` examples are for explicitly included files. When an SDK
already includes a file, use `Update` on its actual item type instead; for example,
a web SDK can classify `appsettings.json` as `Content`, not `None`.

```xml
<ItemGroup>
  <None Include="appsettings.json" CopyToOutputDirectory="PreserveNewest" />
  <None Include="testdata\seed.db" CopyToOutputDirectory="IfDifferent" />
</ItemGroup>
```

The child-element form is equivalent:

```xml
<None Include="testdata\seed.db">
  <CopyToOutputDirectory>IfDifferent</CopyToOutputDirectory>
</None>
```

## Why `Always` Is Usually the Wrong Choice

Classic `Always` re-copies files whenever that copy pipeline executes, including
otherwise no-change MSBuild builds. Many or large content files can make this a
measurable recurring cost. An IDE up-to-date check that skips MSBuild entirely is
a separate decision.

Historically, `Always` addressed a specific scenario: **the destination changes
between builds**. Tests or applications might mutate a database, state file, or
configuration file. With `PreserveNewest`, a newer destination is not reset
because the source is no longer newer. `Always` forces a reset, but pays for it
even when the destination has not changed.

Do not replace `Always` solely because it appears in project text. First measure
copy work and establish whether unconditional copying is part of the required
behavior.

## `IfDifferent`: Copy When Different, in Either Direction

`IfDifferent` restores the source when the destination is missing or its timestamp
or size differs, whether the destination is newer or older. It skips unchanged
files according to that metadata comparison.

`_CopyDifferingSourceItemsToOutputDirectory` uses the `Copy` task with
`SkipUnchangedFiles="true"`. This is a **timestamp-and-size heuristic, not a content
hash**. A destination edited to the same size and last-write timestamp is treated
as unchanged and is not restored.

Use `IfDifferent` when:

- Tests or the application mutate copied databases, state/storage files, or
  editable configuration, and the metadata-based reset meets the contract.
- `Always` was used only to synchronize a drifting destination, not because
  every build must perform a fresh copy.

```xml
<ItemGroup>
  <!-- For an explicitly included fixture; use Update if already included -->
  <None Include="fixtures\catalog.db" CopyToOutputDirectory="IfDifferent" />
</ItemGroup>
```

If same-size/same-timestamp mutations must also be reset, retain genuine
unconditional copying or a separately verified content-based mechanism.

## Globally Softening `Always` with `$(SkipUnchangedFilesOnCopyAlways)`

For a codebase with many legacy `Always` items, a bulk opt-in is:

```xml
<PropertyGroup>
  <SkipUnchangedFilesOnCopyAlways>true</SkipUnchangedFilesOnCopyAlways>
</PropertyGroup>
```

The common `Always` copy target then passes `SkipUnchangedFiles="true"` to `Copy`,
giving those items the same timestamp/size skip heuristic as `IfDifferent`.

- Unset/default behavior is effectively `false`: classic `Always` copying.
- Setting it in `Directory.Build.props` can affect the entire repository.
- Prefer per-item intent when practical. A bulk property avoids individual item
  edits, but still changes behavior and requires checking every affected reset
  contract.
- Do not assume this build-target property governs every publish copy task.

## How the Modes Flow Through the Build

`GetCopyToOutputDirectoryItems` buckets items by their metadata. The common
`_CopySourceItemsToOutputDirectory` pipeline, invoked by
`CopyFilesToOutputDirectory`, uses:

- `_CopyOutOfDateSourceItemsToOutputDirectory` for `PreserveNewest`, with
  `Inputs`/`Outputs` timestamp checks.
- `_CopyOutOfDateSourceItemsToOutputDirectoryAlways` for `Always`, with the
  skip-unchanged property controlling its `Copy` task.
- `_CopyDifferingSourceItemsToOutputDirectory` for `IfDifferent`, with
  `SkipUnchangedFiles="true"`.

These standard build copies register `FileWrites` for the normal Clean pipeline.
Custom tasks and publish outputs can have different tracking; inspect ownership
and use only approved, scoped cleanup.

**Transitive copy:** copyable items can flow to referencing projects through
`ProjectReference`, including `_CopyToOutputDirectoryTransitiveItems`; `Never`
does not participate in that copy pipeline. Verify consumers as well as the
defining project. Supporting targets also collect `IfDifferent` for ClickOnce;
test the publishing mode and supported toolset actually used by the repository.

## Version Requirement

`IfDifferent` and `SkipUnchangedFilesOnCopyAlways` require **MSBuild 17.13+**
(**.NET SDK 9.0.2xx+ / Visual Studio 2022 17.13+**) for the common build-target
behavior described here. Older common targets can silently omit an item whose
mode they do not recognize.

Check the actual SDK/MSBuild host and the minimum supported toolset, not the
target framework. Gate usage or require a compatible SDK through `global.json`,
and check Visual Studio/MSBuild.exe hosts too. On older hosts, retaining `Always`
can preserve required reset behavior; substituting `PreserveNewest` may not.
An unsupported skip-unchanged property is not evidence of a speedup.

`CopyToPublishDirectory` is implemented by publishing targets. Verify recognition,
transitive collection, and reset behavior on the selected publishing SDK rather
than inferring support from the common build-target version alone.

## Quick Decision Guide

- Not needed in this output: explicitly choose `Never` when defaults might copy it.
- Normal source-edited file: `PreserveNewest`.
- Mutated destination should reset when metadata differs: `IfDifferent`.
- Fresh copy required on every pipeline execution, including metadata-identical
  changes: `Always`, without a skip-unchanged override.
- Many compatible legacy `Always` items: consider
  `SkipUnchangedFilesOnCopyAlways=true` after checking the same behavioral limits.

## Verify the Choice

In an approved disposable workspace or on explicitly scoped files, check an
ordinary no-change build, a source edit, a missing destination, and destination
mutations with newer and older timestamps. Check same-size/same-timestamp mutation
when strict reset matters. Also test source addition/removal and the intended
stale-output policy: copy metadata alone does not guarantee obsolete destinations
are deleted.

Check build, referencing projects, runtime use, and relevant publish/ClickOnce
outputs. Do not hardlink writable/resettable outputs to sources: writes through a
hardlink can change the source. Compare copy counts and same-scenario timings
using the [baseline](build-perf-baseline.md); preserve the output contract rather
than claiming success merely because fewer files were copied.
