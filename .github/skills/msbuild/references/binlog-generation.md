# Generate binary logs

Based on the original `binlog-generation` skill, reduced to capture, verification, and retention.
Capture a log for the build being investigated, not as an unconditional rule for every .NET task.
If a matching log already exists, use [MSBuild replay](binlog-failure-analysis.md#replay-a-binary-log)
instead of rerunning the build.

## Preserve the operation

Append the binary logger switch to the existing MSBuild invocation. Keep its entry point, working
directory, target, configuration, framework, runtime, restore behavior, and global properties.
Do not replace a repository wrapper with a bare `dotnet build` unless the wrapper's equivalent
invocation is known.

Choose an unused filename before each invocation. The examples below assume these paths do not
already exist:

```powershell
dotnet build App.sln -c Release --no-restore "-bl:build-01.binlog"
dotnet msbuild App.csproj -t:Pack "-bl:pack-01.binlog"
```

Retain the user's actual arguments rather than substituting these project names or adding
`--no-restore`. Every comparison or retry needs a different log path; do not use bare `-bl`,
which reuses `msbuild.binlog`.

Automatic `{}` filenames are optional and toolset-dependent. Do not assume every .NET 8 SDK
supports expansion. If support is confirmed, quote the complete argument in PowerShell, such as
`"-bl:build-{}.binlog"`, and verify that the resulting name actually expanded. Explicit unused
names avoid that compatibility issue and also work for CI uploaders needing a known path.

`dotnet build`, `restore`, `pack`, and `publish` accept MSBuild logger switches. Test runners and
wrapper scripts differ: only forward `-bl` through a supported MSBuild argument surface. A
Microsoft.Testing.Platform-native invocation or `dotnet test --no-build` is not automatically a
request to capture a new build.

## Verify the artifact, including on failure

Record the command's exit code and locate the newly created, nonempty `.binlog` before starting
analysis:

```powershell
Get-ChildItem -File *.binlog | Select-Object Name, Length, LastWriteTime
```

An intentional failing build can still produce a useful log. Failure before MSBuild starts
(missing SDK, invalid command-line arguments, wrapper rejection) may produce none. Report that
condition; do not invent a path or repeatedly rebuild without addressing it.

Keep the path, original exit code, and scenario together. For a capture-only request, this is the
deliverable. Otherwise continue to [failure analysis](binlog-failure-analysis.md) or
[performance diagnostics](build-perf-diagnostics.md).

## Privacy and retention

Binary logs can contain command lines, environment/property values, credentials, paths, and
embedded project/import contents. Replayed text logs can expose the same sensitive values.
Keep both local by default and out of commits. Preserve existing logs during any approved cleanup;
do not use a repository-wide clean to prepare capture.

If embedding imported files is inappropriate, use a quoted logger argument such as:

```powershell
dotnet build App.csproj "-bl:build-02.binlog;ProjectImports=None"
```

`ProjectImports=None` reduces embedded source coverage; it does **not** redact secrets from
properties, environment values, task arguments, or messages. Explain the diagnostic tradeoff,
and review/redact artifacts before any authorized sharing.
