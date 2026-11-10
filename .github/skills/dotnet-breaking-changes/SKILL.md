---
name: dotnet-breaking-changes
description: >
  Use before changing .NET/C# code whose callers you do not control: a NuGet library, or a repo with
  PublicAPI.Shipped/Unshipped.txt, ApiCompat, multiple TargetFrameworks, InternalsVisibleTo, or
  generated (.g.cs) partials. Green builds miss silent breaks from optional parameters, parameter
  names, sealing, const values, nullability, and #if.
  USE FOR: "without breaking existing callers"; adding a public method, overload, optional
  parameter, or interface member to a shipped library; renaming a public method or an internal
  helper a friend assembly uses; consolidating duplicated logic; changing a signature,
  nullability, default, or attribute; adding a target framework; RS0016/RS0017 or ApiCompat errors;
  "is this PR breaking?". For a rename, extract, or merge, also invoke csharp-refactoring.
  DO NOT USE FOR: an executable whose callers all live in one project; renaming a local variable
  or private member, even in a shipped library; formatting; framework/SDK/NuGet upgrades
  (use dotnet-upgrade).
license: MIT
---

# .NET breaking changes: the contract is wider than the file

When you edit .NET code, the observable contract is usually **larger than the snippet in front of you**.
A change that compiles and keeps tests green can still break a downstream consumer, another target
framework, a friend assembly, or regenerate away on the next build. This applies to a feature or a bug
fix as much as a refactor, and to **additions** — a new public member, overload, or target framework — as
much as removals: added surface is a permanent contract obligation, and added behavior on a multi-targeted
or partial type must stay correct on every target and survive the next regeneration. (For the
behavior-preserving refactoring *process*, use the `csharp-refactoring` skill; this skill is the
compatibility knowledge it — and any other change — taps into.)

## Inspect first — find which surfaces this repo actually has

The most valuable move is to **look before you leap**: find which hidden surfaces exist here, then check
only those. Don't recite these or assume — the markers present dictate the plan; markers absent tell you a
surface is not in play.

```bash
# Public-API gate (which one? they are NOT interchangeable)
git ls-files "**/PublicAPI.Shipped.txt" "**/PublicAPI.Unshipped.txt"                 # PublicApiAnalyzers
grep -rl "EnablePackageValidation\|ApiCompat" --include=*.props --include=*.targets --include=*.csproj .
# Multi-targeting and conditional code
grep -rl "<TargetFrameworks>" --include=*.csproj --include=*.props . ; grep -rn "#if " --include=*.cs .
# Generated / partial code
git ls-files "*.g.cs" "*.generated.cs" ; grep -rln "partial class\|partial record\|partial struct" --include=*.cs .
# Friend assemblies
grep -rn "InternalsVisibleTo" --include=*.cs --include=*.csproj --include=*.props .
```

Then read only the reference(s) for the surfaces you actually found. For every `InternalsVisibleTo`
name, check whether that project is in the solution: a friend **not** built here is an external
consumer of every internal it can see, exactly like a public caller, and a green solution build proves
nothing about it.

Before editing, confirm the request's premise is true in the code. For "consolidate", "deduplicate", or
"add X" requests, trace the actual calls: if one type already delegates to the other, or the member
already exists, the work is done — change nothing and report the delegating call (see Stop and escalate).

## The hidden surfaces (depth in references/)

Public API (1) is the **default** surface to check on any library edit; the other three are
**conditional** — pursue them only when the inspect-first markers above show they are in play.

1. **Public API.** Renaming/moving/removing/re-signing a public member — or changing nullability, a
   generic constraint, or a trimming/AOT attribute — is a breaking change a green test run will not catch.
   Repos gate this **two non-interchangeable ways**: source-level (`PublicApiAnalyzers` + `PublicAPI.*.txt`,
   which you maintain) and binary/package-level (`ApiCompat` / `<EnablePackageValidation>`). Respect the
   gate that exists; if none exists, review the surface by hand and flag it — do **not** bolt on analyzer
   infrastructure as a side effect. Move a public type via a `[TypeForwardedTo]` forwarder; a *rename*
   needs an `[Obsolete]` shim, not a forwarder. See `references/public-api.md`.

2. **Multi-targeting and `#if`.** Code that multi-targets (`<TargetFrameworks>`) or compiles conditionally
   (`#if NET8_0_OR_GREATER`, platform/CoreCLR/Mono/NativeAOT branches) can build for the target your editor
   shows and break one it never invoked. Inspect every branch — including inactive ones — preserve
   intentional divergence, and re-gate each TFM/RID. See `references/multi-targeting.md`.

3. **Source-generated and partial code.** A type is often `partial` across several files, and part may be
   **generated** (source generators, Razor, `*.g.cs`). Include every partial declaration; treat generated
   files as derived and change the generator input/template unless the repo checks the output in as source.
   See `references/source-generation.md`.

4. **InternalsVisibleTo.** `internal` is not private across a solution: test and friend assemblies bind to
   internals (and, when strong-named, to a public key). An internal rename/move/removal can break a
   consumer with no reference in the declaring project. A friend built outside this repo cannot be
   updated, so keep the old internal name as an `[Obsolete]` forwarder. See `references/internals-visible-to.md`.

## Decisions a green build will not make for you

Each row below compiles every caller and passes every test, yet breaks a consumer. Do not stop at a
green build or test run. Use the safe shape instead.

When the safe shape departs from the literal request — an overload instead of the requested optional
parameter, a forwarder kept instead of a deleted or renamed member, or no change at all — say so in your
final answer in one or two sentences: what you did instead, and which consumer it would have broken
(e.g. "already-compiled callers would throw `MissingMethodException`"). A silent substitution reads as
ignoring the request.

Treat that substitution as one atomic deliverable: implement the safe shape, preserve the old contract
and shipped baseline, test both old and new entry points, and explain the compatibility reason. After the
last edit, inspect the diff or reopen the implementation file before claiming success. A green test run
does not prove a failed patch applied when the unchanged fixture already passes those tests.

Do not use labels such as "for binary compatibility" as the whole explanation. Name the exact old
entry point that remains, the consumer that still binds to it, and the concrete failure avoided. For
example: "Kept `Parse(string?, int)` unchanged because already-compiled consumers still request that
exact method and would throw `MissingMethodException` if it disappeared." For source-only breaks, name
the affected syntax (`raw:` named arguments, derivation from a newly sealed class, or nullable callers)
instead. This causal sentence is part of the implementation, not optional release-note polish.

| Tempting change | Why it breaks | Safe shape |
|-----------------|---------------|------------|
| Add an optional parameter to a shipped method (`Foo(a)` → `Foo(a, b = null)`) | **Binary break**: the old signature is gone, so already-compiled consumers throw `MissingMethodException` until they recompile | Keep `Foo(a)` unchanged and add a new overload `Foo(a, b)` with `b` required on that overload; omission continues to select the old method. Record only the new overload |
| Fix RS0017 by editing or deleting a line in `PublicAPI.Shipped.txt` | Shipped lines are the released contract; rewriting one hides the break the gate exists to report | Leave shipped lines alone. Restore the shipped member and record new surface in `PublicAPI.Unshipped.txt` |
| Add a member to a shipped public interface | Every existing implementer, including ones in other packages, stops compiling | Add a default interface implementation (requires runtime support: not on `netstandard2.0` or .NET Framework targets), or add a new interface. A default member is callable only through the interface, so also add a public member on any concrete type that must expose it, and record both |
| Rename a shipped member | Source and binary break | Add the new name, keep the old one as an `[Obsolete]` forwarder, and record only the new member |
| Rename or remove an `internal` member when `InternalsVisibleTo` names an assembly not in the solution | That friend was compiled against the old name and cannot be updated from this repo; it fails at bind time | Add the new name, migrate in-repo callers, keep the old internal name as an `[Obsolete]` forwarder, keep the grant, and name the protected friend in the answer |
| Rename a parameter of a shipped member | Callers passing it by name (`raw: x`) stop compiling and late-bound/`dynamic` callers fail; positional callers in this repo hide it. PublicAPI analyzers report it as RS0016 + RS0017 | Keep the shipped name — no overload can carry a different name for the same signature. Leave it unchanged and edit nothing else; you may offer (not make) a clearer XML `<param>` doc, or the rename at the next major version |
| Seal a class, make a member non-virtual, or narrow a parameter's nullability | Derived types, overrides, or nullable callers break | Treat as a breaking change; escalate rather than edit |
| Change a `const` value or an optional parameter's default | The old value is compiled into callers and persists until they recompile | Preserve the shipped constant/default; introduce a separately named member (which may be `static readonly`) or a new overload for new behavior. Replacing an existing `const` with `static readonly` is itself source-breaking |
| Change a value under `#if` for one target | Only consumers of that target see it; tests edited to match will not catch it | Treat as a behavior change for that target and call it out |

Whenever you kept an old entry point or departed from the literal request, the final answer must
contain this line, filled in — terse bullets such as "preserved for binary consumers" do not satisfy it:

> **Compatibility:** Kept `<exact old signature>` unchanged because `<who still binds to it>` would
> `<concrete failure>` if it changed. `<What you did instead of the literal request, and why.>`

Before finishing a compatibility-preserving edit, make the delta easy to audit:

1. Name the **new** entry point or behavior and the **exact old** entry point that remains.
2. Name the in-repo caller, generated template, or friend assembly you migrated, plus any external
   consumer you could not migrate and therefore protected with a forwarder.
3. State which baseline stayed unchanged and which new surface was recorded.
4. Report the exact compatibility check that exercised the old entry point as well as the new one.

For a no-change outcome, lead with `No files changed.` and identify the decisive evidence before any
general explanation: the existing delegation, shipped signature, parameter name, or nullability line.

When asked **"is this a breaking change?"** or to review a diff, make no edits unless asked and answer
in this shape, scaled to the diff:

1. One verdict line first (e.g. "Do not merge as-is: N changes break consumers").
2. One numbered item per consumer-visible change: what changed, **binary** (already-compiled
   consumers fail), **source** (consumers fail to recompile), or **behavioral** (same signature,
   different result, possibly on only one target), who breaks, **why CI stayed green** (a shipped
   API line rewritten, a test edited to match, or a change the analyzer does not track, such as
   `sealed` or a private `const`), and the safe shape. State each as fact or not at all: a
   behavioral claim needs a concrete input whose old and new results differ; if unsure, probe both
   versions with a one-line program — framework parsers already tolerate much (e.g. `bool.TryParse`
   ignores surrounding whitespace), and a no-op rewrite is not a break.
3. One closing "Not breaking:" line covering every remaining hunk with its reason (internal-only
   edits, probed no-ops), instead of side notes inside the numbered items.

Cover only surfaces the diff touches; do not add trimming, AOT, or tooling commentary it does not raise.

## Verify before reporting

Build and test **every** target framework (`dotnet test -f <tfm>` for each), not only the default. If
the project sets `EnablePackageValidation`, run `dotnet pack` — package validation runs only after
`Pack`, never on build. Report exactly which commands ran and their result.

## Stop and escalate (do not silently proceed)

- The requested change is **already in place** — e.g. the "duplicated" code already delegates to one
  implementation, or the overload already exists. Say so and change nothing; do not invent a
  refactor to have something to show.
- A **public/shipped API** would change and you cannot verify compatibility — no shim/forwarder path and
  no PublicApiAnalyzers/ApiCompat/package-validation gate to catch a break.
- The edit changes an **observable annotation** (nullability, `[DynamicallyAccessedMembers]`,
  `[RequiresUnreferencedCode]`, generic constraints) on a public member.
- The change can be made consistent across **some but not all** target frameworks or platforms.
- The only way to apply it is editing **generated output** that the next build will overwrite.

When you change nothing (the request is already done or has no safe shape), a bare "no" looks like
ignoring the request. Make the final answer show the work, in this order: "No files changed." and why;
the evidence that decided it (the shipped `PublicAPI.Shipped.txt` line, or the call that already
delegates, with its file); who would break, or where the single implementation already lives; what the
user *can* do instead, if anything; and the validation you ran.

## Reference files

Load a reference only for a surface the inspect-first step actually found:

- **[references/public-api.md](references/public-api.md)** — the two API gates and why they are not
  interchangeable, nullable/trimming/AOT as observable API, `[Obsolete]` shims and type-forwarders,
  suppression baselines.
- **[references/multi-targeting.md](references/multi-targeting.md)** — TFMs, `#if` and platform symbols,
  RIDs, and how to inspect and re-gate every target.
- **[references/source-generation.md](references/source-generation.md)** — partial types, generated output
  vs checked-in source, editing the generator input. (See also `dotnet-msbuild/including-generated-files`.)
- **[references/internals-visible-to.md](references/internals-visible-to.md)** — friend/test assemblies
  and strong-name keys.
