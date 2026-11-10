---
name: setup-local-sdk
license: MIT
description: >
  Install a .NET SDK locally for safe preview testing, specific-version pinning, or
  reproducible team setups — without modifying the system-wide installation.
  USE FOR: trying .NET previews safely, testing specific SDK versions, installing MAUI
  or other workloads on a preview, updating or replacing an existing local SDK,
  creating reproducible team/CI install scripts, configuring global.json paths.
  DO NOT USE FOR: system-wide SDK installs, .NET hosts older than 10, runtime-only
  installs, or projects not using SDK-style commands.
---

# setup-local-sdk

## Purpose

Guide the user through installing a .NET SDK into a project-local `.dotnet/`
directory and wiring it up via the `global.json` `paths` feature (.NET 10+).
The examples use .NET 11, but this works with any version — prerelease or stable.

The result is a fully isolated SDK that:
- Does **not** modify the system-wide .NET installation.
- Is picked up automatically by `dotnet` commands from the project root.
- Can be deleted to revert (`rm -rf .dotnet/` or `Remove-Item -Recurse -Force .\.dotnet`).

## When NOT to use

- User wants a **system-wide** install — direct to the official installer.
- Host `dotnet` is **older than v10** — `paths` doesn't exist; explain and stop.
- User needs a **runtime-only** install — `paths` applies to SDK resolution only.

## Inputs / Prerequisites

| Input | Required | Default | Notes |
|---|---|---|---|
| Channel or version | No | `11.0` | e.g. `11.0`, `STS`, `LTS`, or an exact version like `11.0.100-preview.2.26159.112` |
| Quality | No | `preview` | One of: `daily`, `preview`, `ga` |
| jq | No | — | Optional for bash team scripts when patching an existing `global.json`; without it, do not overwrite the file |

### Prerequisites

1. **A .NET 10+ SDK is installed globally** — run `dotnet --version`; major ≥ 10.
2. **curl** (macOS/Linux) or **PowerShell** (Windows) is available.

## Workflow

### Step 1 — Clarify what to install

If the user didn't specify, ask what .NET SDK version they want (e.g., "latest
.NET 11 preview" or an exact version like `11.0.100-preview.2.26159.112`).
Map the answer to `--channel`/`--quality` or `--version` flags.

### Step 2 — Verify .NET 10+ host

If the user already provided `dotnet --version` output, treat that as the
authoritative version for their machine. Do not override it with the agent
workspace's version; if the two differ, explain that the workspace differs and
continue advising for the user's machine.

```bash
dotnet --version
```

If major version < 10, stop before downloading anything: the `paths` feature
requires a .NET 10+ host SDK. Tell the user to install .NET 10 or later
system-wide first, then return to the local SDK setup.

### Step 3 — Detect operating system

Run `uname -s 2>/dev/null`. If it succeeds (including `MINGW*`, `MSYS*`, `CYGWIN*` —
these are bash-capable environments like Git Bash) → use bash/`dotnet-install.sh`.
If it fails (native Windows without Git Bash) → use PowerShell/`dotnet-install.ps1`.

### Step 4 — Check for existing local SDK

**macOS / Linux:**

```bash
test -d .dotnet && echo "exists" || echo "not found"
```

**Windows (PowerShell):**

```powershell
if (Test-Path -LiteralPath .\.dotnet) { "exists" } else { "not found" }
```

If `.dotnet/` exists, ask: update with the new version, or skip and keep it?

### Step 5 — Download and run the install script

**macOS / Linux:**

```bash
INSTALL_SCRIPT="$(mktemp "${TMPDIR:-/tmp}/dotnet-install.XXXXXX")"
trap 'rm -f "$INSTALL_SCRIPT"' EXIT
curl -fsSL https://dot.net/v1/dotnet-install.sh -o "$INSTALL_SCRIPT"
bash "$INSTALL_SCRIPT" --channel <CHANNEL> --quality <QUALITY> --install-dir .dotnet
```

**Windows (PowerShell):**

```powershell
$installScript = Join-Path $env:TEMP "dotnet-install-$([guid]::NewGuid()).ps1"
try {
    Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile $installScript
    & $installScript -Channel <CHANNEL> -Quality <QUALITY> -InstallDir .dotnet
}
finally {
    if (Test-Path -LiteralPath $installScript) {
        Remove-Item -LiteralPath $installScript -Force
    }
}
```

For exact versions: use `--version <VERSION>` (bash) or `-Version <VERSION>` (PowerShell)
instead of channel/quality flags. The install scripts are from Microsoft's official
URLs: `https://dot.net/v1/dotnet-install.sh` and `https://dot.net/v1/dotnet-install.ps1`.

### Step 6 — Identify the installed version

```bash
./.dotnet/dotnet --version          # macOS/Linux
.\.dotnet\dotnet.exe --version      # Windows
```

Record the exact version string (e.g., `11.0.100-preview.2.26159.112`) for `global.json`.

### Step 7 — Create or update global.json

```json
{
  "sdk": {
    "version": "<INSTALLED_VERSION>",
    "allowPrerelease": true,
    "rollForward": "latestFeature",
    "paths": [".dotnet", "$host$"],
    "errorMessage": "Required .NET SDK not found. Run ./install-dotnet.sh (or .ps1) to install it locally."
  }
}
```

- `paths`: `.dotnet` first (local priority), `$host$` = system-wide fallback.
- `rollForward: "latestFeature"`: use for latest-preview or floating feature-band installs.
- Exact version requests: use `rollForward: "disable"` so SDK resolution doesn't move to a different feature band.
- `allowPrerelease`: set to `true` only when installing a prerelease SDK. Omit for stable versions.
- `errorMessage`: include only when team install scripts are created (Step 10). Otherwise omit.

If `global.json` already exists, **merge** carefully: preserve existing properties (`msbuild-sdks`,
`tools`, etc.) and only add/update the `sdk` section. Read the existing file first, update/add
the `sdk` object, then write it back. This ensures cross-project config (e.g., MSBuild settings)
isn't lost. Always back up the original file (e.g., `global.json.bak`) before modifying.

**Minimal config** (when version pinning isn't needed):
`{"sdk":{"paths":[".dotnet","$host$"]}}`

### Step 8 — Update .gitignore

**macOS / Linux (or Git Bash):**

```bash
grep -qxF '.dotnet/' .gitignore 2>/dev/null || printf '\n.dotnet/\n' >> .gitignore
```

**Windows (PowerShell):**

```powershell
if (-not (Test-Path .gitignore) -or -not (Select-String -Path .gitignore -Pattern '^\.dotnet/$' -Quiet)) {
    Add-Content -Path .gitignore -Value '.dotnet/'
}
```

### Step 9 — Install workloads (if requested)

Only do this after `global.json` and `.gitignore` are complete, so a slow or
platform-limited workload install does not prevent the base local SDK setup from
being usable.

If the user mentioned MAUI, mobile, workload, Blazor WASM, or cross-platform,
install using the **local** binary (no sudo needed):

```bash
./.dotnet/dotnet workload install <workload>       # macOS/Linux
.\.dotnet\dotnet.exe workload install <workload>   # Windows
```

Verify: `./.dotnet/dotnet workload list` (or `.\.dotnet\dotnet.exe workload list`).

For MAUI, pick a workload supported by the current OS and target platform. On
Linux, the full `maui` meta-workload is not available; use a supported workload
such as `maui-android` when Android is the target, or explain the platform
limitation and ask which target to configure.

> **Always use the local dotnet binary for workload commands.** Workload metadata
> is stored relative to the host process's dotnet root. The system `dotnet` puts
> metadata in the wrong location. (See [dotnet/sdk#49825](https://github.com/dotnet/sdk/issues/49825).)

### Step 10 — Create team install scripts

Create if user mentioned "team", "share", "CI", "scripts", etc. Otherwise offer.
These examples back up `global.json` and preserve existing settings. The bash script
uses `jq` when an existing `global.json` must be patched; if `jq` is unavailable,
it refuses to overwrite the file and prints the settings to merge manually.
Adapt script variables to the install choice from Step 1: exact versions should
use `--version` / `-Version` and `rollForward: "disable"`; channel installs should
use channel/quality and only set `allowPrerelease: true` for prerelease SDKs.
If `global.json` already pins `sdk.version` and the user mainly needs team
scripts, reuse that version in the scripts and update `global.json` first; do
not start a long SDK download just to discover the version. When the user asks
for both setup and scripts, create the scripts/config before any long install so
the reproducible setup exists even if download or workload installation is slow.

**install-dotnet.sh:**

```bash
#!/usr/bin/env bash
set -euo pipefail
INSTALL_DIR=".dotnet"
CHANNEL="11.0"
QUALITY="preview"
VERSION=""
ROLL_FORWARD="latestFeature"
ALLOW_PRERELEASE="true"
WORKLOADS=("${@}")
ERROR_MESSAGE="Required .NET SDK not found. Run ./install-dotnet.sh (or .ps1) to install it locally."
INSTALL_SCRIPT="$(mktemp "${TMPDIR:-/tmp}/dotnet-install.XXXXXX")"
GLOBAL_JSON_TMP=""
cleanup() {
    rm -f "$INSTALL_SCRIPT"
    [ -n "$GLOBAL_JSON_TMP" ] && rm -f "$GLOBAL_JSON_TMP"
}
trap cleanup EXIT
curl -fsSL https://dot.net/v1/dotnet-install.sh -o "$INSTALL_SCRIPT"
INSTALL_ARGS=(--install-dir "$INSTALL_DIR")
if [ -n "$VERSION" ]; then
    INSTALL_ARGS+=(--version "$VERSION")
    ROLL_FORWARD="disable"
else
    INSTALL_ARGS+=(--channel "$CHANNEL" --quality "$QUALITY")
fi
bash "$INSTALL_SCRIPT" "${INSTALL_ARGS[@]}"
SDK_VERSION=$("$INSTALL_DIR/dotnet" --version)
write_global_json() {
    if [ -f global.json ]; then
        cp global.json global.json.bak
        if ! command -v jq >/dev/null 2>&1; then
            echo "global.json exists; install succeeded, but this script will not overwrite it without jq." >&2
            echo "Merge these sdk settings manually so existing global.json properties are preserved:" >&2
            cat >&2 <<EOF
{
  "sdk": {
    "version": "$SDK_VERSION",
    "allowPrerelease": $ALLOW_PRERELEASE,
    "rollForward": "$ROLL_FORWARD",
    "paths": [".dotnet", "\$host\$"],
    "errorMessage": "$ERROR_MESSAGE"
  }
}
EOF
            exit 1
        fi
        GLOBAL_JSON_TMP="$(mktemp "${TMPDIR:-/tmp}/global-json.XXXXXX")"
        jq --arg version "$SDK_VERSION" --arg rollForward "$ROLL_FORWARD" --argjson allowPrerelease "$ALLOW_PRERELEASE" --arg errorMessage "$ERROR_MESSAGE" '
          .sdk = ((.sdk // {}) + {
            version: $version,
            allowPrerelease: $allowPrerelease,
            rollForward: $rollForward,
            paths: [".dotnet", "$host$"],
            errorMessage: $errorMessage
          })
        ' global.json > "$GLOBAL_JSON_TMP"
        mv "$GLOBAL_JSON_TMP" global.json
        GLOBAL_JSON_TMP=""
    else
        cat > global.json <<EOF
{
  "sdk": {
    "version": "$SDK_VERSION",
    "allowPrerelease": $ALLOW_PRERELEASE,
    "rollForward": "$ROLL_FORWARD",
    "paths": [".dotnet", "\$host\$"],
    "errorMessage": "$ERROR_MESSAGE"
  }
}
EOF
    fi
}
write_global_json
grep -qxF '.dotnet/' .gitignore 2>/dev/null || printf '\n.dotnet/\n' >> .gitignore
[ ${#WORKLOADS[@]} -gt 0 ] && "$INSTALL_DIR/dotnet" workload install "${WORKLOADS[@]}"
echo "Done. SDK: $SDK_VERSION"
```

```bash
chmod +x install-dotnet.sh
```

**install-dotnet.ps1:**

```powershell
param([string[]]$Workloads = @())
$ErrorActionPreference = 'Stop'
$installDir = '.dotnet'; $channel = '11.0'; $quality = 'preview'
$version = ''; $rollForward = 'latestFeature'; $allowPrerelease = $true
$errorMessage = 'Required .NET SDK not found. Run ./install-dotnet.sh (or .ps1) to install it locally.'
$installScript = Join-Path $env:TEMP "dotnet-install-$([guid]::NewGuid()).ps1"
try {
    Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile $installScript
    $installArgs = @('-InstallDir', $installDir)
    if ($version) {
        $installArgs += @('-Version', $version)
        $rollForward = 'disable'
    } else {
        $installArgs += @('-Channel', $channel, '-Quality', $quality)
    }
    & $installScript @installArgs
}
finally {
    if (Test-Path -LiteralPath $installScript) {
        Remove-Item -LiteralPath $installScript -Force
    }
}
$sdkVersion = & "$installDir\dotnet.exe" --version
$globalJson = if (Test-Path 'global.json') {
    Copy-Item 'global.json' 'global.json.bak'
    Get-Content -Path 'global.json' -Raw | ConvertFrom-Json
} else {
    [pscustomobject]@{}
}
if (-not $globalJson.PSObject.Properties['sdk']) {
    $globalJson | Add-Member -MemberType NoteProperty -Name 'sdk' -Value ([pscustomobject]@{})
}
$updates = [ordered]@{
    version = $sdkVersion
    allowPrerelease = $allowPrerelease
    rollForward = $rollForward
    paths = @('.dotnet', '$host$')
    errorMessage = $errorMessage
}
foreach ($entry in $updates.GetEnumerator()) {
    $property = $globalJson.sdk.PSObject.Properties[$entry.Key]
    if ($property) {
        $property.Value = $entry.Value
    } else {
        $globalJson.sdk | Add-Member -MemberType NoteProperty -Name $entry.Key -Value $entry.Value
    }
}
$globalJson | ConvertTo-Json -Depth 10 | Set-Content -Path 'global.json' -Encoding UTF8
if (-not (Test-Path .gitignore) -or -not (Select-String -Path .gitignore -Pattern '^\.dotnet/$' -Quiet)) {
    Add-Content -Path .gitignore -Value '.dotnet/'
}
if ($Workloads.Count -gt 0) { & "$installDir\dotnet.exe" workload install @Workloads }
Write-Host "Done. SDK: $sdkVersion"
```

Commit these scripts to the repo so teammates can run them.

### Step 11 — Verify SDK resolution

```bash
dotnet --version
```

Output should match the locally installed version. If not, check: global.json
location, `paths` array contents, host dotnet version ≥ 10.

### Step 12 — Summarize and explain cleanup

Tell the user: SDK installed, global.json configured, .dotnet/ gitignored, system
install untouched. Cleanup: delete `.dotnet/`, remove `paths`/`errorMessage` from
global.json, optionally delete install scripts. Include the final `global.json`
`sdk` values (or a short snippet) so the user can see the configured version,
`paths`, and any `errorMessage`. If workloads were requested, include the local
`dotnet workload install ...` command used and the workload verification result
or the exact blocker if the workload could not be installed.

## Common pitfalls

| Pitfall | Cause | Fix |
|---|---|---|
| `paths` ignored | Host `dotnet` < v10 | Install .NET 10+ system-wide |
| Wrong SDK resolves | `global.json` in parent directory | Check for global.json up the tree |
| Teammates get "SDK not found" | `.dotnet/` gitignored, no install script run | Use `errorMessage` in global.json |
| Workloads missing | Used system `dotnet` instead of local | Use `./.dotnet/dotnet workload install` |
| `dotnet app.dll` wrong runtime | `paths` is SDK-only, not apphost | Use `dotnet run` or set `DOTNET_ROOT` |
