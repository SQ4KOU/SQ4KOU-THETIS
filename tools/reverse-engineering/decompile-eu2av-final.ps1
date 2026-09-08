param(
    [string]$OutputRoot = "reverse-engineering/EU2AV-Thetis-2.10.3.16-Final"
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
Set-StrictMode -Version Latest

$RepoRoot = (Get-Location).Path
$OutputRoot = Join-Path $RepoRoot $OutputRoot
$TempRoot = Join-Path $env:RUNNER_TEMP 'eu2av-final-re'
$DownloadUrl = 'https://eu2av.net/download/file.php?id=2070'
$Package = Join-Path $TempRoot 'Thetis-v2.10.3.16-extended.x64.download'
$ArchiveRoot = Join-Path $TempRoot 'archive'
$InstallRoot = Join-Path $TempRoot 'installed'
$ToolRoot = Join-Path $TempRoot 'tools'
$ManagedRoot = Join-Path $OutputRoot 'managed'
$NativeRoot = Join-Path $OutputRoot 'native'
$ShaderRoot = Join-Path $OutputRoot 'dxbc'
$MetaRoot = Join-Path $OutputRoot 'metadata'
$LogsRoot = Join-Path $OutputRoot 'logs'

if (Test-Path $TempRoot) { Remove-Item $TempRoot -Recurse -Force }
if (Test-Path $OutputRoot) { Remove-Item $OutputRoot -Recurse -Force }
@($TempRoot,$ArchiveRoot,$InstallRoot,$ToolRoot,$OutputRoot,$ManagedRoot,$NativeRoot,$ShaderRoot,$MetaRoot,$LogsRoot) |
    ForEach-Object { New-Item -ItemType Directory -Force -Path $_ | Out-Null }

function Write-Utf8NoBom([string]$Path, [string[]]$Lines) {
    [System.IO.File]::WriteAllLines($Path, $Lines, [System.Text.UTF8Encoding]::new($false))
}

function Safe-Name([string]$Name) {
    return ($Name -replace '[^A-Za-z0-9._-]', '_')
}

function Sha256([string]$Path) {
    return (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash.ToLowerInvariant()
}

function Relative-To([string]$Base, [string]$Path) {
    return [IO.Path]::GetRelativePath($Base, $Path).Replace('\','/')
}

function Get-Magic([string]$Path) {
    $bytes = [IO.File]::ReadAllBytes($Path)
    $n = [Math]::Min(16, $bytes.Length)
    return (($bytes[0..($n-1)] | ForEach-Object { $_.ToString('X2') }) -join ' ')
}

function Test-ManagedAssembly([string]$Path) {
    try {
        [void][Reflection.AssemblyName]::GetAssemblyName($Path)
        return $true
    } catch {
        return $false
    }
}

function Invoke-Logged([string]$Log, [scriptblock]$Command) {
    try {
        & $Command 2>&1 | Tee-Object -FilePath $Log
        return $LASTEXITCODE
    } catch {
        $_ | Out-String | Add-Content -LiteralPath $Log
        return 999
    }
}

# ---------------------------------------------------------------------------
# 1. Acquire the exact public EU2AV 2.10.3.16 Final package.
# ---------------------------------------------------------------------------
Write-Host "Downloading EU2AV Final from $DownloadUrl"
$curlArgs = @(
    '-L','--fail-with-body','--retry','5','--retry-all-errors',
    '--connect-timeout','30','--max-time','1200',
    '-A','Mozilla/5.0 (Windows NT 10.0; Win64; x64) SQ4KOU-RE/1.0',
    '-e','https://eu2av.net/',
    '-o',$Package,$DownloadUrl
)
& curl.exe @curlArgs
if ($LASTEXITCODE -ne 0) { throw "curl failed with exit code $LASTEXITCODE" }
$pkgInfo = Get-Item $Package
if ($pkgInfo.Length -lt 10MB) {
    $head = Get-Content -LiteralPath $Package -TotalCount 20 -ErrorAction SilentlyContinue | Out-String
    throw "Downloaded payload is too small ($($pkgInfo.Length) bytes), probably not the installer archive. Head: $head"
}
$packageHash = Sha256 $Package
$packageMagic = Get-Magic $Package
Write-Host "Package size=$($pkgInfo.Length) SHA256=$packageHash magic=$packageMagic"

# ---------------------------------------------------------------------------
# 2. Extract archive and MSI while preserving the installed file layout.
# ---------------------------------------------------------------------------
$sevenZip = (Get-Command 7z.exe -ErrorAction SilentlyContinue).Source
if (-not $sevenZip) { $sevenZip = (Get-Command 7z -ErrorAction SilentlyContinue).Source }
if (-not $sevenZip) { throw '7-Zip is required on the runner but was not found.' }

$magic6 = ([IO.File]::ReadAllBytes($Package)[0..5] | ForEach-Object { $_.ToString('X2') }) -join ''
if ($magic6 -eq '377ABCAF271C') {
    & $sevenZip x $Package "-o$ArchiveRoot" -y
    if ($LASTEXITCODE -ne 0) { throw "7-Zip archive extraction failed: $LASTEXITCODE" }
} else {
    Copy-Item $Package (Join-Path $ArchiveRoot 'downloaded-package.bin') -Force
}

$msi = Get-ChildItem $ArchiveRoot -Recurse -File -Filter *.msi | Sort-Object Length -Descending | Select-Object -First 1
if ($msi) {
    Write-Host "Administrative MSI extraction: $($msi.FullName)"
    $p = Start-Process msiexec.exe -ArgumentList @('/a',"`"$($msi.FullName)`"",'/qn',"TARGETDIR=`"$InstallRoot`"") -Wait -PassThru
    if ($p.ExitCode -ne 0) {
        Write-Warning "msiexec /a failed with $($p.ExitCode), trying 7-Zip MSI extraction"
        & $sevenZip x $msi.FullName "-o$InstallRoot" -y
        if ($LASTEXITCODE -ne 0) { throw "Both MSI extraction methods failed (msiexec=$($p.ExitCode), 7z=$LASTEXITCODE)" }
        Get-ChildItem $InstallRoot -Recurse -File -Filter *.cab | ForEach-Object {
            $cabOut = Join-Path $InstallRoot ("cab_" + (Safe-Name $_.BaseName))
            New-Item -ItemType Directory -Force -Path $cabOut | Out-Null
            & $sevenZip x $_.FullName "-o$cabOut" -y | Out-Null
        }
    }
} else {
    Copy-Item (Join-Path $ArchiveRoot '*') $InstallRoot -Recurse -Force
}

$thetis = Get-ChildItem $InstallRoot -Recurse -File -Filter Thetis.exe | Sort-Object Length -Descending | Select-Object -First 1
if (-not $thetis) {
    $thetis = Get-ChildItem $ArchiveRoot -Recurse -File -Filter Thetis.exe | Sort-Object Length -Descending | Select-Object -First 1
    if ($thetis) { $InstallRoot = Split-Path $thetis.FullName -Parent }
}
if (-not $thetis) { throw 'Thetis.exe was not found after extracting the EU2AV Final package.' }
Write-Host "Thetis.exe: $($thetis.FullName) size=$($thetis.Length) SHA256=$(Sha256 $thetis.FullName)"

# Choose the narrowest installed tree containing Thetis.exe and sibling binaries.
$PayloadRoot = Split-Path $thetis.FullName -Parent

# ---------------------------------------------------------------------------
# 3. Exact inventory and PE classification.
# ---------------------------------------------------------------------------
$inventory = New-Object System.Collections.Generic.List[object]
$allFiles = Get-ChildItem $PayloadRoot -Recurse -File | Sort-Object FullName
foreach ($f in $allFiles) {
    $managed = $false
    if ($f.Extension -match '^\.(exe|dll)$') { $managed = Test-ManagedAssembly $f.FullName }
    $kind = if ($managed) { 'managed-pe' } elseif ($f.Extension -match '^\.(exe|dll)$') { 'native-pe' } elseif ($f.Extension -eq '.bin') { 'binary' } else { 'data' }
    $inventory.Add([pscustomobject]@{
        path = Relative-To $PayloadRoot $f.FullName
        size = $f.Length
        sha256 = Sha256 $f.FullName
        kind = $kind
    })
}
$inventory | Export-Csv (Join-Path $MetaRoot 'MANIFEST.csv') -NoTypeInformation -Encoding utf8

# ---------------------------------------------------------------------------
# 4. Full managed-code recovery with ILSpy: C# project + IL text.
# ---------------------------------------------------------------------------
$ilspyTool = Join-Path $ToolRoot 'ilspy'
New-Item -ItemType Directory -Force -Path $ilspyTool | Out-Null
& dotnet tool install ilspycmd --tool-path $ilspyTool --verbosity minimal
if ($LASTEXITCODE -ne 0) { throw "Unable to install ilspycmd: $LASTEXITCODE" }
$ilspy = Join-Path $ilspyTool 'ilspycmd.exe'
if (-not (Test-Path $ilspy)) { $ilspy = Join-Path $ilspyTool 'ilspycmd' }
$ilspyVersion = (& $ilspy --version 2>&1 | Out-String).Trim()
Write-Host "ILSpy: $ilspyVersion"

$managedAssemblies = $allFiles | Where-Object { $_.Extension -match '^\.(exe|dll)$' -and (Test-ManagedAssembly $_.FullName) }
foreach ($asm in $managedAssemblies) {
    $safe = Safe-Name ((Relative-To $PayloadRoot $asm.FullName) -replace '/','__')
    $out = Join-Path $ManagedRoot $safe
    New-Item -ItemType Directory -Force -Path $out | Out-Null
    $log = Join-Path $LogsRoot ("ilspy_" + $safe + '.log')
    Write-Host "ILSpy project: $($asm.FullName)"
    & $ilspy -p -o $out $asm.FullName *>&1 | Tee-Object -FilePath $log
    $projectExit = $LASTEXITCODE
    if ($projectExit -ne 0) {
        Write-Warning "ILSpy project mode failed for $($asm.Name), retrying single-file decompile"
        Remove-Item $out -Recurse -Force -ErrorAction SilentlyContinue
        New-Item -ItemType Directory -Force -Path $out | Out-Null
        & $ilspy -o $out $asm.FullName *>&1 | Tee-Object -FilePath $log -Append
        if ($LASTEXITCODE -ne 0) { throw "ILSpy failed for managed assembly $($asm.FullName)" }
    }

    $ilFile = Join-Path $out ($safe + '.il.txt')
    & $ilspy --ilcode $asm.FullName 2>&1 | Out-File $ilFile -Encoding utf8
    if ($LASTEXITCODE -ne 0) {
        "IL dump unavailable with this ilspycmd version. C# decompilation above is authoritative for this snapshot." | Set-Content $ilFile -Encoding utf8
    }
}

# ---------------------------------------------------------------------------
# 5. DXBC shader disassembly. Every waterfall shader must be represented.
# ---------------------------------------------------------------------------
$dxc = Get-Command dxc.exe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source -ErrorAction SilentlyContinue
$fxc = Get-Command fxc.exe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source -ErrorAction SilentlyContinue
if (-not $dxc) {
    $dxc = Get-ChildItem 'C:\Program Files (x86)\Windows Kits' -Recurse -File -Filter dxc.exe -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match '\\x64\\' } | Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $fxc) {
    $fxc = Get-ChildItem 'C:\Program Files (x86)\Windows Kits' -Recurse -File -Filter fxc.exe -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match '\\x64\\' } | Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
}

$shaderFiles = $allFiles | Where-Object { $_.Name -match '^waterfall_.*\.(bin|cso)$' }
foreach ($shader in $shaderFiles) {
    $safe = Safe-Name ((Relative-To $PayloadRoot $shader.FullName) -replace '/','__')
    $out = Join-Path $ShaderRoot ($safe + '.asm.txt')
    $ok = $false
    if ($fxc) {
        try {
            & $fxc /dumpbin /nologo $shader.FullName 2>&1 | Out-File $out -Encoding utf8
            if ($LASTEXITCODE -eq 0 -and (Get-Item $out).Length -gt 100) { $ok = $true }
        } catch { }
    }
    if (-not $ok -and $dxc) {
        try {
            & $dxc -dumpbin -all $shader.FullName 2>&1 | Out-File $out -Encoding utf8
            if ($LASTEXITCODE -eq 0 -and (Get-Item $out).Length -gt 100) { $ok = $true }
        } catch { }
    }
    if (-not $ok) {
        throw "Unable to disassemble DXBC shader $($shader.FullName); neither FXC nor DXC produced a valid dump."
    }
}

# ---------------------------------------------------------------------------
# 6. Native PE metadata using dumpbin.
# ---------------------------------------------------------------------------
$dumpbin = $null
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (Test-Path $vswhere) {
    $vs = (& $vswhere -latest -products '*' -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath | Select-Object -First 1)
    if ($vs) {
        $dumpbin = Get-ChildItem (Join-Path $vs 'VC\Tools\MSVC') -Recurse -File -Filter dumpbin.exe -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match '\\Hostx64\\x64\\dumpbin.exe$' } | Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
    }
}

$nativeAssemblies = $allFiles | Where-Object { $_.Extension -match '^\.(exe|dll)$' -and -not (Test-ManagedAssembly $_.FullName) }
if ($dumpbin) {
    foreach ($pe in $nativeAssemblies) {
        $safe = Safe-Name ((Relative-To $PayloadRoot $pe.FullName) -replace '/','__')
        $meta = Join-Path $NativeRoot $safe
        New-Item -ItemType Directory -Force -Path $meta | Out-Null
        & $dumpbin /headers $pe.FullName 2>&1 | Out-File (Join-Path $meta 'pe-headers.txt') -Encoding utf8
        & $dumpbin /exports $pe.FullName 2>&1 | Out-File (Join-Path $meta 'exports.txt') -Encoding utf8
        & $dumpbin /imports $pe.FullName 2>&1 | Out-File (Join-Path $meta 'imports.txt') -Encoding utf8
    }
}

# ---------------------------------------------------------------------------
# 7. Ghidra: analyze and decompile every native EXE/DLL in the payload.
# ---------------------------------------------------------------------------
$ghidraApi = 'https://api.github.com/repos/NationalSecurityAgency/ghidra/releases/latest'
$ghRelease = Invoke-RestMethod -Headers @{ 'User-Agent'='SQ4KOU-EU2AV-RE' } -Uri $ghidraApi
$ghAsset = $ghRelease.assets | Where-Object { $_.name -match '^ghidra_.*_PUBLIC_.*\.zip$' } | Select-Object -First 1
if (-not $ghAsset) { throw 'Could not locate the official Ghidra public ZIP in the latest release.' }
$ghZip = Join-Path $ToolRoot $ghAsset.name
Write-Host "Downloading Ghidra $($ghRelease.tag_name): $($ghAsset.browser_download_url)"
& curl.exe -L --fail --retry 5 --retry-all-errors -A 'SQ4KOU-EU2AV-RE/1.0' -o $ghZip $ghAsset.browser_download_url
if ($LASTEXITCODE -ne 0) { throw "Ghidra download failed: $LASTEXITCODE" }
$ghRoot = Join-Path $ToolRoot 'ghidra'
New-Item -ItemType Directory -Force -Path $ghRoot | Out-Null
& $sevenZip x $ghZip "-o$ghRoot" -y | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Ghidra extraction failed: $LASTEXITCODE" }
$headless = Get-ChildItem $ghRoot -Recurse -File -Filter analyzeHeadless.bat | Select-Object -First 1 -ExpandProperty FullName
if (-not $headless) { throw 'analyzeHeadless.bat was not found in the Ghidra distribution.' }
$ghidraVersion = $ghRelease.tag_name
$ghScriptDir = Join-Path $RepoRoot 'tools\reverse-engineering'
$ghProjects = Join-Path $TempRoot 'ghidra-projects'
New-Item -ItemType Directory -Force -Path $ghProjects | Out-Null

$nativeStatus = New-Object System.Collections.Generic.List[object]
$i = 0
foreach ($pe in $nativeAssemblies) {
    $i++
    $rel = Relative-To $PayloadRoot $pe.FullName
    $safe = Safe-Name ($rel -replace '/','__')
    $out = Join-Path $NativeRoot $safe
    New-Item -ItemType Directory -Force -Path $out | Out-Null
    $projectLocation = Join-Path $ghProjects ("p" + $i)
    New-Item -ItemType Directory -Force -Path $projectLocation | Out-Null
    $projectName = "EU2AV_$i"
    $log = Join-Path $LogsRoot ("ghidra_" + $safe + '.log')
    Write-Host "Ghidra [$i/$($nativeAssemblies.Count)]: $rel"
    & $headless $projectLocation $projectName -import $pe.FullName -overwrite -analysisTimeoutPerFile 1200 -scriptPath $ghScriptDir -postScript ExportDecomp.java $out 2>&1 |
        Tee-Object -FilePath $log
    $exit = $LASTEXITCODE
    $stats = Join-Path $out 'decompile-stats.txt'
    $nativeStatus.Add([pscustomobject]@{
        path=$rel
        exit_code=$exit
        stats_present=(Test-Path $stats)
        log=(Relative-To $OutputRoot $log)
    })
    Remove-Item $projectLocation -Recurse -Force -ErrorAction SilentlyContinue
}
$nativeStatus | Export-Csv (Join-Path $MetaRoot 'NATIVE_DECOMPILE_STATUS.csv') -NoTypeInformation -Encoding utf8

# ---------------------------------------------------------------------------
# 8. Verification gates. Required EU2AV application components must be present.
# ---------------------------------------------------------------------------
$requiredNative = @('ChannelMaster.dll','wdsp.dll')
foreach ($required in $requiredNative) {
    $src = $nativeAssemblies | Where-Object Name -eq $required | Select-Object -First 1
    if (-not $src) { throw "Required native EU2AV component missing from package: $required" }
    $safe = Safe-Name ((Relative-To $PayloadRoot $src.FullName) -replace '/','__')
    $stats = Join-Path (Join-Path $NativeRoot $safe) 'decompile-stats.txt'
    if (-not (Test-Path $stats)) { throw "Ghidra did not produce decompiler statistics for required component: $required" }
}

$thetisSafe = Safe-Name ((Relative-To $PayloadRoot $thetis.FullName) -replace '/','__')
$thetisManagedOut = Join-Path $ManagedRoot $thetisSafe
if (-not (Test-Path $thetisManagedOut)) { throw 'Thetis.exe managed decompilation output is missing.' }
if ((Get-ChildItem $thetisManagedOut -Recurse -File | Measure-Object).Count -lt 100) {
    throw 'Thetis.exe decompilation produced unexpectedly few source files.'
}

$requiredShaders = @(
    'waterfall_fft_bitreverse_cs.bin',
    'waterfall_fft_magnitude_cs.bin',
    'waterfall_fft_stage_ab_cs.bin',
    'waterfall_fft_stage_ba_cs.bin',
    'waterfall_postproc.bin',
    'waterfall_resolve_cs.bin',
    'waterfall_row_cs.bin'
)
foreach ($required in $requiredShaders) {
    $src = $shaderFiles | Where-Object Name -eq $required | Select-Object -First 1
    if (-not $src) { throw "Required GPU shader missing from EU2AV package: $required" }
    $safe = Safe-Name ((Relative-To $PayloadRoot $src.FullName) -replace '/','__')
    $dump = Join-Path $ShaderRoot ($safe + '.asm.txt')
    if (-not (Test-Path $dump) -or (Get-Item $dump).Length -lt 100) { throw "Shader disassembly missing/invalid: $required" }
}

# ---------------------------------------------------------------------------
# 9. Tool/package provenance and human-readable reference README.
# ---------------------------------------------------------------------------
$toolLines = @(
    "generated_utc=$([DateTime]::UtcNow.ToString('o'))",
    "source_url=$DownloadUrl",
    "package_size=$($pkgInfo.Length)",
    "package_sha256=$packageHash",
    "package_magic=$packageMagic",
    "thetis_exe_sha256=$(Sha256 $thetis.FullName)",
    "ilspy=$ilspyVersion",
    "ghidra=$ghidraVersion",
    "fxc=$fxc",
    "dxc=$dxc",
    "dumpbin=$dumpbin",
    "java_version=$((& java -version 2>&1 | Out-String).Trim() -replace '\r?\n',' | ')",
    "dotnet_version=$((& dotnet --version 2>&1 | Out-String).Trim())"
)
Write-Utf8NoBom (Join-Path $MetaRoot 'PROVENANCE.txt') $toolLines

$managedCount = ($managedAssemblies | Measure-Object).Count
$nativeCount = ($nativeAssemblies | Measure-Object).Count
$shaderCount = ($shaderFiles | Measure-Object).Count
$readme = @"
# EU2AV Thetis 2.10.3.16 Extended Final — reverse-engineering reference

This directory is a **reconstructed/decompiled reference snapshot** of the publicly distributed EU2AV Thetis 2.10.3.16 Extended Final package. It is not the original EU2AV source tree and it does not claim to recover original comments, identifiers lost during compilation, build scripts, or the exact original C/C++/HLSL text.

## Provenance

- Distribution URL: `$DownloadUrl`
- Downloaded package SHA-256: `$packageHash`
- Downloaded package size: $($pkgInfo.Length) bytes
- `Thetis.exe` SHA-256: `$(Sha256 $thetis.FullName)`

Exact per-file hashes and installed paths are in `metadata/MANIFEST.csv`. Tool versions are in `metadata/PROVENANCE.txt`.

## Coverage

- Managed PE assemblies discovered: **$managedCount**. Every managed assembly is passed through ILSpy. `managed/` contains reconstructed C# project/source output and an IL dump when supported by the installed ILSpy version.
- Native PE modules discovered: **$nativeCount**. Every native EXE/DLL is analyzed by Ghidra. `native/<module>/` contains chunked C-like decompiler output, `functions.csv`, `decompile-stats.txt`, and PE import/export/header metadata when `dumpbin` is available.
- Waterfall DXBC shaders discovered: **$shaderCount**. `dxbc/` contains actual DXBC disassembly produced by FXC or DXC. This is shader bytecode disassembly, **not original HLSL**.
- Full installed payload inventory: `metadata/MANIFEST.csv`.
- Per-native-module Ghidra status: `metadata/NATIVE_DECOMPILE_STATUS.csv`.
- Tool logs: `logs/`.

## Important interpretation rule

Use this branch as a binary-grounded implementation reference. Managed C# is decompiler reconstruction; Ghidra output is pseudocode; DXBC files are bytecode disassembly. Where exact behavior matters, cross-check the decompiled control flow with IL, PE metadata, shader disassembly, hashes, and runtime tests rather than treating reconstructed source formatting as authoritative.

## GPU-waterfall components explicitly verified by the pipeline

The verification gate requires successful recovery of `Thetis.exe`, `ChannelMaster.dll`, `wdsp.dll` and these EU2AV waterfall shaders:

- `waterfall_fft_bitreverse_cs.bin`
- `waterfall_fft_magnitude_cs.bin`
- `waterfall_fft_stage_ab_cs.bin`
- `waterfall_fft_stage_ba_cs.bin`
- `waterfall_postproc.bin`
- `waterfall_resolve_cs.bin`
- `waterfall_row_cs.bin`

The branch is intentionally isolated from `sq4kou` and is for reference/audit only.
"@
Write-Utf8NoBom (Join-Path $OutputRoot 'README.md') ($readme -split "`r?`n")

Write-Host "Reverse engineering complete. Output: $OutputRoot"
Write-Host "Managed assemblies: $managedCount; native modules: $nativeCount; waterfall shaders: $shaderCount"
