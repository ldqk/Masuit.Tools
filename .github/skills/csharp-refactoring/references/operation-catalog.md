# C# refactoring operation catalog (detail)

Load this when you need the full operation taxonomy or the Roslyn provider each maps to.
The core SKILL.md keeps only the short operation list; this file is the depth.

## Contents

- Operation catalog (provider, recurrence, representative PR)
- "Modernize" is defined by the repo's analyzer config

## Operation catalog

Canonical operations, aligned with Roslyn's own IDE refactoring providers
(`src/Features/CSharp/Portable/CodeRefactorings`). The provider column is informational — a
headless CLI agent usually cannot invoke these IDE code actions directly, so apply them via the fallback
ladder in SKILL.md.

| Operation | Roslyn provider / tool | Recurrence | Representative real PR |
|-----------|------------------------|-----------:|------------------------|
| **Rename** symbol / type / file | rename engine (`Renamer`) | high | _runtime_ "Rename `DISABLE_CROSSGEN`"; _sdk_ "Rename `dnup` to `dotnetup`" |
| **Move** type / member / file | `MoveType`, `MoveStaticMembers` | high | _runtime_ "Move DSA tests into System.Security.Cryptography"; _sdk_ "Move SDK task unit test projects from src/ to test/" |
| **Consolidate / de-duplicate** | analyzer-assisted | high | _runtime_ "Consolidate ComWrappers implementation across platforms" |
| **Modernize / simplify** idioms | `UseExplicitOrImplicitType`, `UseRecursivePatterns`, `ConvertLocalFunctionToMethod`, `AddAwait` | high | _roslyn_ "Simplify lots of redundant code in code fix providers" |
| **Split** large class / file / assembly | extract + move | medium | _roslyn_ "Split `FeatureSwitchManager`"; _runtime_ "Move RPC contracts to a separate assembly" |
| **Extract** method / class / interface | `ExtractClass`, extract-method, extract-interface | medium | _runtime_ "Extract `ManifestBuilder` and `EventListener`" |
| **Inline** method / local / constant | `InlineMethod`, `InlineTemporary` | lower | _runtime_ "Refactor `UInt128` division" |
| **Pull up / push down** member | `PullMemberUp` | tail | move members between a type and its base/interface |
| **Sync namespace** to folder | `SyncNamespace` | tail | align `namespace` with folder layout after a move |

## "Modernize" is defined by the repo's analyzer config, not personal taste

These repos drive idiom rules through `.editorconfig` + `EnforceOnBuild` (roslyn #49995 "Set
EnforceOnBuild values for code style analyzers"). Apply the fixes the repo's own analyzers request
(IDE00xx / CAxxxx) via `dotnet format` / code-fixes; don't impose a style it hasn't opted into, and don't
blanket-suppress diagnostics to make an edit "pass." For _adopting_ nullable annotations specifically, see
`dotnet-upgrade/migrate-nullable-references`; enabling nullable across a project or changing public API
annotations is a migration or contract change, not a canonical behavior-preserving refactoring operation.
