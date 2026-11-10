# Public API surface — the two gates and how not to break it

## Contents

- Two gates, not interchangeable
- What counts as a breaking change (including annotations)
- Deciding and validating a public change
- Preserving identity: `[Obsolete]` shims and type-forwarders
- Suppression baselines

## Two gates, not interchangeable

.NET repos protect the public surface in **two fundamentally different ways**. Do not treat one as a
substitute for the other — a repo may use either, both, or neither, and each catches things the other
does not.

| Gate | What it compares | Where it lives | You maintain |
|------|------------------|----------------|--------------|
| **PublicApiAnalyzers** (RS0016/RS0017/…) | _Declared source API_ of the current compilation | `PublicAPI.Shipped.txt` + `PublicAPI.Unshipped.txt` per project | **Yes** — you edit the `.txt` files; the analyzer only enforces they match the code |
| **ApiCompat / package validation** | _Built assemblies / NuGet package_ against a baseline (previous version, or a contract/ref assembly) | `<EnablePackageValidation>`, `Microsoft.DotNet.ApiCompat.*` in props/targets | Baseline version + suppression file |

**Practical rule:** run/respect whichever gate the repo already has and update its files or baseline as
the change legitimately requires. If **neither** exists, review the public surface by hand and flag the
risk to the user — **do not add analyzer or package-validation infrastructure as a side effect** of an
unrelated change (that is scope creep and its own kind of breaking change to the build).

## What counts as a breaking change (including annotations)

Beyond the obvious rename/remove/move of a public type or member, these are **also** observable and can
break consumers or the API gate:

- Signature changes: parameter type/order, return type, adding a required parameter, `params`, default
  values, generic arity or **constraints**.
- **Parameter names**, including case: callers that pass arguments by name (`Parse(raw: s)`) and
  late-bound callers (`dynamic`, VB late binding) break. Positional callers hide this.
- **Adding an optional parameter** to an existing public method is **binary-breaking**, even when ordinary
  source calls still compile. Already-compiled call sites bind to the exact old signature and fail with
  `MissingMethodException`. It can also be **source-breaking**: method-group conversions to the old
  delegate type no longer compile. Keep the original method and add an overload instead. Make the new
  parameter required on the new overload: callers that omit it continue selecting the original method,
  while callers that need the capability select the new signature explicitly.
- **Adding a member to a public interface** breaks every implementer. A default interface member avoids
  this only where the runtime supports it (.NET Core 3.0+ and .NET 5+, not `netstandard2.0` or .NET
  Framework).
- **`const` values and optional-parameter defaults** are copied into the caller at compile time. Changing
  them does not reach existing callers until they recompile, so callers silently keep the old value.
- **Nullability** annotations (`string` → `string?`, `[NotNullWhen]`, `[MaybeNull]`) — these are part of
  the public contract under `#nullable enable`; changing them shifts consumer warnings and the API gate.
- **Trimming/AOT** attributes (`[DynamicallyAccessedMembers]`, `[RequiresUnreferencedCode]`,
  `[RequiresDynamicCode]`, `[UnconditionalSuppressMessage]`) — observable API for trim/AOT consumers.
- Accessibility widening/narrowing, `sealed`/`abstract`/`virtual`/`static` changes, `readonly`/`ref`.
- Moving a public type to another **assembly** (identity changes even if the name does not).

Preserve these unless the task is explicitly to change them.

## Deciding and validating a public change

1. Determine the project's role: **shipped library/package** (public API matters) vs **app/service**
   (external contract = HTTP/config/schema, internal surface can move) vs **private/single-target**
   (behavior + tests only).
2. If a gate exists, build/pack and let it run; update `PublicAPI.Unshipped.txt` or the ApiCompat
   baseline **intentionally**, never to silence a break you did not mean to make. `PublicAPI.Shipped.txt`
   moves only at release time. An RS0017 that points at a shipped line means a released member has
   disappeared; restore the member and do not edit the line.
3. If no gate exists in a library, diff the public surface manually (compare declarations, or a
   generated ref/`.txt`) and surface the delta to the user.
4. In the final response, do not stop at "binary compatible." Name the preserved signature and the
   failure it prevents for already-compiled consumers; name source-only hazards such as named arguments
   or nullable callers just as concretely.

## Preserving identity: `[Obsolete]` shims and type-forwarders

- **`[Obsolete]` shim:** keep the old member alongside the new one, forwarding to it, so source consumers
  keep compiling. Use for renames/relocations _within_ an assembly.
- **`[TypeForwardedTo]` type-forwarder:** preserves **binary identity** when a **public type moves to
  another assembly**. Forwarders solve _cross-assembly moves_; they do **not** help a rename (the name
  changed) and are unnecessary for moves within the same assembly.

## Suppression baselines

`ApiCompatSuppressions` / `GlobalSuppressions` and PublicApiAnalyzers baselines exist so intentional,
reviewed changes pass. Update them deliberately with the change; do not blanket-add suppressions to make
an edit "pass" — that hides the very break the gate exists to catch.
