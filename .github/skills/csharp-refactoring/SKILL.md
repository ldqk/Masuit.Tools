---
name: csharp-refactoring
description: "Safely refactors C#/.NET code without changing behavior. USE FOR any request to refactor, rename, move, extract, inline, merge, consolidate, deduplicate, split, or modernize C# code, even a small extract, inline, or rename, or merging near-identical types; green builds miss drift in #if branches, generated partials, and reflection/config names. When a changed member is visible outside the assembly (PublicAPI.*.txt, InternalsVisibleTo, shipped package), also invoke dotnet-breaking-changes; local/private renames do not need it. Also use when a request calls a feature, bug fix, package/framework upgrade, or public nullability change a refactor and the behavior-changing part must be separated or declined. DO NOT USE FOR ordinary feature or bug fixes not presented as refactoring; standalone upgrades after reclassification (use dotnet-upgrade); adding tests; or formatting-only work."
license: MIT
---

# C# Refactoring (behavior-preserving)

A refactor changes **structure**, never observable **behavior**. Do the edit with binding-aware tools,
then confirm behavior held with a build + the relevant tests. Keep the effort proportional to the change:
a one-line local rename does not need the ceremony a public multi-targeted change does.

## Mandatory gate: classify before validation or editing

Read only enough repository context to classify **each requested operation**. Do this before restoring,
building, or making an edit. Classification precedence is:

- If the request explicitly asks for at least one separable behavior-preserving operation, complete
  that structural work and defer only the behavior-changing or contract-changing operations. Do not
  invent or infer structural work to avoid the stop response.
- If the structural and behavior-changing parts cannot be separated, use the whole-request stop
  response and state why they are inseparable.
- Otherwise, use the whole-request stop response only when **every** requested operation is outside
  behavior-preserving refactoring.

For a whole request that is outside behavior-preserving refactoring:

1. State: `Not a behavior-preserving refactor: <specific reason>.`
2. State: `No files changed.`
3. State: `Next workflow: <workflow>.` Then stop. Do not add manual implementation steps, alternatives,
   an offer to proceed without the workflow, or a follow-up question — even when that workflow is unavailable.

| Requested as a "refactor" | Classification and action |
|---|---|
| Framework or NuGet version change | **Upgrade.** Do not edit or validate the upgrade here; hand off to `dotnet-upgrade`. |
| New capability, flag, endpoint, tier, or behavior | **Feature.** Do not implement it here; hand off to the repository's feature workflow. |
| Threshold, rate, output, or bug-result change | **Behavior change.** Defer it and hand off to the repository's bug-fix or behavior-change workflow; still complete any clearly separable structural operation. |
| Tighten or loosen a shipped/public nullable annotation | **Source-contract change.** Leave the declaration and API record unchanged; hand off to the `dotnet-breaking-changes` skill. |

For a mixed request, never stop after classification. Perform the separable structural operation,
explicitly defer the behavior/contract change, and never state `No files changed.` after completing
structural work. Never modify tests to make an unauthorized behavior change appear preserved.

## Work only in the current workspace

Use the current agent workspace as the boundary. If it is a Git checkout, resolve its repository root
(`git rev-parse --show-toplevel`) and stay inside it. If Git metadata is absent, treat the current working
directory and its subdirectories as the boundary, and use a solution or project named in the request
inside it; Git is not a prerequisite for a refactor. Resolve prompt-provided relative paths inside that
boundary.

Search and edit only that workspace. Never use filesystem-wide search or select a similarly named clone,
another worktree, build output, or unrelated temporary directory because a file also exists there. If a
named path is absent, stop and report the mismatch instead of guessing another workspace. If a tool rejects
an in-workspace path for a mechanical reason such as path form or unsupported tool root, retry through
another in-workspace mechanism. If the rejection is a permission or policy denial, report it instead of
working around it. Never search outside the boundary.

## Rename / move by bindings, not text

The #1 way a "rename" silently corrupts code is editing textual matches (comments, strings, unrelated
overloads) instead of real **bindings**. Find every binding reference first, then edit semantically. Use
the strongest tool available: an IDE/Roslyn workspace refactoring, then the C# LSP the
[`dotnet` plugin declares](https://github.com/dotnet/skills/blob/main/plugins/dotnet/lsp.json)
(`findReferences`, `goToDefinition`, `incomingCalls`, `rename` code action), then analyzer code-fixes /
Roslynator, then compiler-validated edits (edit the true bindings, rebuild, let the compiler flag misses).
Plain find/replace only when scope is provably tiny and every hit is verified. Include **every** `partial`
declaration, and edit the generator input, never generated (`*.g.cs`) output.

For the operation → Roslyn-provider mapping and representative PRs, see
[references/operation-catalog.md](references/operation-catalog.md).

## Consolidate toward the existing source of truth

When de-duplicating, preserve the ownership direction stated by the code or request. If `B` duplicates
an implementation already owned by `A`, keep `A` canonical and make `B` delegate to it; do not invert
the dependency merely because either direction compiles. Preserve public compatibility wrappers when
the duplicate surface is shipped, and migrate only in-repo callers that are safe to move.

## Decisions that change the edit

Use the first matching row instead of applying the requested operation mechanically:

| Situation | Do | Never |
|---|---|---|
| Inline an internal, unshipped pass-through wrapper | Migrate every binding reference to the target, remove the wrapper, then compile to catch misses. | Keep dead indirection "for compatibility" when no compatibility boundary exists. |
| Inline or remove a shipped/public wrapper | Migrate ordinary in-repo callers, but retain an `[Obsolete]` forwarding entry point unless the request explicitly authorizes a breaking change. | Delete a shipped API merely because all current source callers were migrated. |
| Rename a member reached by a string, reflection, DI, or configuration | Rename binding-based callers; preserve the observed external name with a forwarding shim or metadata, and exercise the old-name path. | Rewrite an external/configured name just to make the new source name consistent. |
| Extract duplicated logic whose callers pass different values | Extract the algorithm and pass each caller's existing inputs through unchanged. | Collapse distinct inputs, evaluation order, rounding, or side effects into one caller's version. |
| Rename code compiled under `#if` or multiple TFMs | Update every source branch and validate each target framework explicitly. | Treat a green default-target build as evidence for unbuilt branches. |
| Merge near-identical types | Parameterize only the values that differ, migrate every construction site, and preserve each old value exactly. If the old types are internal/unshipped and the request says to merge into one type, delete their declarations. | Retain unnecessary aliases, static holders, factories, or wrapper types that leave the requested merge incomplete; introduce a new hierarchy or behavior. |

## Preserve contracts beyond C# call sites

Compilation proves binding compatibility, not every external contract. Before renaming or moving a
type/member, check whether its name or metadata is observed by serialization, reflection, dependency
injection, configuration binding, source generators, P/Invoke, or `dynamic`.

| Boundary | Required decision |
|---|---|
| Serialized/configuration name | Preserve the external name with the repository's existing mechanism (for example, `JsonPropertyName`) while migrating C# callers; run a focused round-trip or payload test. |
| Public nullable annotation | The mandatory classification gate applies: leave it unchanged and hand off as a source-contract change. |
| Uncovered reflection or runtime lookup | Do not guess that a compile-clean rename is safe. Preserve the observed name or stop and report the unverified runtime boundary. |

## Verify proportionally

Confirm behavior is preserved after the edit — scaled to blast radius, not a fixed ceremony:

- **Local / private** (method-local or `private` member, one file, single target framework, no public
  surface, no `partial`/generated/`#if`): skip a separate baseline unless the tree is already suspect.
  Make the edit, then run the narrowest build and relevant tests once. Let the compiler catch missed
  references.
- **Cross-boundary** (public/shipped symbol, multi-targeted project, `#if`/platform branches, or
  `partial`/generated code): establish a baseline, then run an explicit build and the relevant tests for
  **each** target framework after the edit (a test command's implicit build is not separate build evidence;
  a green default build can hide a break on another TFM), and run the hazards check below.

Use the repo's own build/test workflow when it documents one (`README`/`CONTRIBUTING`, `build.*`, `eng/`,
`global.json`, `.github/workflows`); its instructions win over any generic command.

### Typical workflow (one operation)
1. Choose one named refactoring operation and keep the step focused on that operation only.
2. Find true binding references (`findReferences`/`goToDefinition`/rename) and include all `partial` declarations.
3. Establish a baseline first only for a cross-boundary change or a tree not already known green.
4. Apply the change via the most semantics-aware tool available; avoid blind find/replace when possible.
5. Rebuild and run the relevant tests. If the gate goes red, report the failure and repair or reassess only
  your edit; never discard unrelated worktree changes.

Otherwise:
```bash
dotnet build   # 0 errors
dotnet test    # stays green; same pass count as before
```

One operation per step; never mix a refactor and a behavior change in the same step. On red, stop and report the failure; repair only your edit without discarding unrelated worktree changes.

## Final response contract

Keep the handoff concise and evidence-based:

- **Refactor:** name the structural operation and the symbols/files changed.
- **Preserved:** name the behavior or compatibility boundary and the mechanism that preserved it.
- **Validation:** report the exact commands and observed result; never claim success after a failed restore,
  build, target framework, or test run.
- **Deferred:** for a mixed request, name the behavior/contract change intentionally left undone and its
  correct next workflow. Omit this line when nothing was deferred.

## Cross-boundary hazards (only when it touches a boundary)

If — and only if — the change touches a **public** symbol, an **internal** symbol exposed to a friend
assembly through `InternalsVisibleTo`, a **multi-targeted** project, or `partial`/generated code, some
breaks won't show up as a failing test. Before invoking `dotnet-breaking-changes`, apply its scope
exclusions: skip it for local/private renames and for public-member renames in an executable whose
callers all live in that project. Otherwise invoke it before editing; it owns the compatibility
decisions, while this skill owns the structural steps. Search the repo for the surface
that governs the symbol (don't assume): the public-API gate (`PublicAPI.Shipped/Unshipped.txt` for
PublicApiAnalyzers, and/or `ApiCompat`/`<EnablePackageValidation>` — not interchangeable),
`<TargetFrameworks>`/`#if` branches, and `InternalsVisibleTo`. Moving a public type to another assembly
needs `[TypeForwardedTo]` in the original assembly; a move within one assembly does not. A public
*rename* needs an `[Obsolete]` shim, not a forwarder. For a provably local/private change, skip these
checks.

## Stop an in-scope refactor when

- The baseline is already red (you can't prove you preserved behavior).
- The requested structural change would alter a public/shipped API and no compatibility shim or forwarder
  can preserve it. Report the boundary. Use the gate's handoff format when no structural work was completed;
  after separable work, put the incompatible operation on the `Deferred:` line instead. Do not ask to make
  the breaking change.
- Equivalence depends on runtime behavior tests don't cover (reflection, DI, serialization, `dynamic`,
  P/Invoke) — report the unverified boundary instead of claiming behavior was preserved.
