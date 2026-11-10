# Incremental inputs and outputs on custom targets

Expensive artifact-producing targets need complete inputs and real outputs. This is not a rule
that every validation/orchestration target must have a stamp file.

The original catalog's project-file-only example was incomplete when generation depended on
`$(Version)`: a command-line property can change without touching the project file. A stable
value-input file can represent that dependency while a separate target registers outputs even
when generation is skipped:

```xml
<Target Name="WriteBuildInfoInputs">
  <ItemGroup>
    <FileWrites Include="$(IntermediateOutputPath)BuildInfo.inputs" />
  </ItemGroup>
  <MakeDir Directories="$(IntermediateOutputPath)" />
  <WriteLinesToFile File="$(IntermediateOutputPath)BuildInfo.inputs"
                    Lines="$(Version)" Overwrite="true" WriteOnlyWhenDifferent="true" />
</Target>

<Target Name="GenerateBuildInfo"
        DependsOnTargets="WriteBuildInfoInputs"
        Inputs="$(MSBuildProjectFullPath);$(MSBuildAllProjects);$(IntermediateOutputPath)BuildInfo.inputs"
        Outputs="$(IntermediateOutputPath)BuildInfo.g.cs">
  <WriteLinesToFile File="$(IntermediateOutputPath)BuildInfo.g.cs"
                    Lines="// Generated for $(Version)" Overwrite="true" />
</Target>

<Target Name="RegisterBuildInfo"
        BeforeTargets="CoreCompile"
        DependsOnTargets="GenerateBuildInfo">
  <ItemGroup>
    <Compile Include="$(IntermediateOutputPath)BuildInfo.g.cs" />
    <FileWrites Include="$(IntermediateOutputPath)BuildInfo.g.cs" />
  </ItemGroup>
</Target>
```

This illustrates a simple version-dependent producer. Adapt it to the actual generator:

- Add its templates, tools, configuration, and source files to the inputs.
- Serialize every relevant non-file value into a stable fingerprint without losing distinctions.
  Git/clock/network state is not tracked merely by listing the project file.
- Keep outputs in stable, configuration/framework/runtime-isolated intermediate paths.
- Ensure generated compile items are registered exactly once on both first and skipped builds.
  Intermediate files are normally excluded from SDK default source globs.
- Register the state file in its producer before generation can fail, so failed-build cleanup
  records its ownership too. Register generated outputs on successful and skipped generation.
  FileWrites is not a substitute for incremental inputs/outputs or compiler item inclusion.
- Timestamp checks do not remember a removed wildcard input. Track input-set membership and
  remove obsolete owned outputs when the generator's contract requires it.

Verify first build, identical second build, a relevant file/property change, removed inputs,
deleted output, successful-build Clean, and failed-generation Clean. Do not assume a timestamp-only
model can distinguish arbitrary same-timestamp mutations. Keep mutation/cleanup experiments confined
to approved disposable outputs. See [incremental-build](../incremental-build.md) for the full workflow
and IDE checks.
