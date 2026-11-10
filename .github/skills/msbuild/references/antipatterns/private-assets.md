# PrivateAssets for analyzers and build tools

Use `PrivateAssets="all"` when a package is an implementation-only build dependency and must not
become a dependency of consumers of the produced package.

```xml
<!-- These packages are intended to remain local to this project's build. -->
<PackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.556" PrivateAssets="all" />
<PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0" PrivateAssets="all" />
<PackageReference Include="MinVer" Version="5.0.0" PrivateAssets="all" />
```

Typical candidates from the original catalog include:

- Analyzer and code-fix packages.
- Source generators intended to run only in the producer.
- SourceLink providers.
- Versioning tools such as MinVer or Nerdbank.GitVersioning.
- Build-only compatibility/checking tools.

Do not infer a leak merely from a missing literal attribute. Inspect evaluated metadata,
`IncludeAssets`/`ExcludeAssets`, package contents, generated restore assets, and the packed
dependency contract. PackageReference's default private-asset categories already restrict some
asset flow, while `buildTransitive` and other dependencies can have different effects.

An analyzer or build extension deliberately distributed to consumers is not a private
implementation detail. Do not suppress intentional transitive behavior, hide required runtime
dependencies, or treat `PrivateAssets` as a way to skip building a project.
Verify the produced package and a representative consumer before declaring the change safe.
