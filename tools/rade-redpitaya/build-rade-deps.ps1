param(
    [ValidateSet('Debug','Release')][string]$Configuration = 'Release'
)
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$vendor = Join-Path $root 'Project Files\lib\Thetis-RADE-vendor'
$libroot = Join-Path $vendor 'Project Files\lib'
if (-not (Test-Path $vendor)) { throw "RADE vendor submodule missing: $vendor" }

$pin = '408f2b5232ff0a2aec9b538a40d4cb1b02627b17'
$head = (& git -C $vendor rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $head -ne $pin) { throw "RADE vendor pin mismatch: $head expected $pin" }

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path $vswhere)) { throw 'vswhere.exe not found' }
$msbuild = (& $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1)
if (-not $msbuild) { throw 'MSBuild not found' }
Write-Host "MSBuild=$msbuild"

function Invoke-MSBuild([string]$Project, [string]$SolutionDir = '') {
    $args = @($Project, '/m', "/p:Configuration=$Configuration", '/p:Platform=x64', '/p:PlatformToolset=v145', '/v:minimal', '/nologo')
    if ($SolutionDir) { $args += "/p:SolutionDir=$SolutionDir" }
    & $msbuild @args
    if ($LASTEXITCODE -ne 0) { throw "MSBuild failed ($LASTEXITCODE): $Project" }
}

# SV1EIA pins Opus commit 940d4e5a..., but the repository intentionally vendors
# only the RADE-required subset.  Current CMake enumerates some platform headers
# from the complete Opus tree, so build Opus from the exact upstream commit while
# keeping all RADE source/model code pinned to Thetis-RADE v2.10.3.21.
$opusVendor = Join-Path $libroot 'opus_dnn'
$opusPin = '940d4e5af64351ca8ba8390df3f555484c567fbb'
$opusSource = Join-Path $env:RUNNER_TEMP "opus-$opusPin"
if (Test-Path $opusSource) { Remove-Item $opusSource -Recurse -Force }
New-Item -ItemType Directory -Path $opusSource -Force | Out-Null
& git -C $opusSource init -q
if ($LASTEXITCODE -ne 0) { throw 'Opus git init failed' }
& git -C $opusSource remote add origin https://github.com/xiph/opus.git
& git -C $opusSource fetch --depth 1 origin $opusPin
if ($LASTEXITCODE -ne 0) { throw 'Pinned Opus fetch failed' }
& git -C $opusSource checkout --detach FETCH_HEAD
if ($LASTEXITCODE -ne 0) { throw 'Pinned Opus checkout failed' }
$opusHead = (& git -C $opusSource rev-parse HEAD).Trim()
if ($opusHead -ne $opusPin) { throw "Opus pin mismatch: $opusHead" }
Write-Host "OPUS_UPSTREAM_PIN=$opusHead"

# Put build products below the vendor Opus directory so ChannelMaster can use
# the same stable paths as SV1EIA's project file.
$opusBuild = Join-Path $opusVendor 'build'
if (Test-Path $opusBuild) { Remove-Item $opusBuild -Recurse -Force }
New-Item -ItemType Directory -Path $opusBuild -Force | Out-Null
& cmake -S $opusSource -B $opusBuild -A x64 -T v145 `
    -DOPUS_DEEP_PLC=ON -DOPUS_DRED=ON -DOPUS_OSCE=ON `
    -DOPUS_BUILD_PROGRAMS=OFF -DOPUS_BUILD_TESTING=OFF -DBUILD_SHARED_LIBS=OFF
if ($LASTEXITCODE -ne 0) { throw 'Opus CMake configure failed' }
& cmake --build $opusBuild --config $Configuration --target opus -- /m
if ($LASTEXITCODE -ne 0) { throw 'Opus build failed' }

$opusLib = Get-ChildItem $opusBuild -Recurse -File -Include opus.lib,libopus.lib | Where-Object { $_.FullName -match "\\$Configuration\\" } | Sort-Object Length -Descending | Select-Object -First 1
if (-not $opusLib) { $opusLib = Get-ChildItem $opusBuild -Recurse -File -Include opus.lib,libopus.lib | Sort-Object Length -Descending | Select-Object -First 1 }
if (-not $opusLib) { throw 'Built Opus static library not found' }
$opusOut = Join-Path $opusBuild "x64\$Configuration"
New-Item -ItemType Directory -Path $opusOut -Force | Out-Null
Copy-Item $opusLib.FullName (Join-Path $opusOut 'opus.lib') -Force
Write-Host "OPUS_LIB=$(Join-Path $opusOut 'opus.lib')"

# Native conditioning dependencies shipped by the pinned SV1EIA tree.
$rnProj = Join-Path $libroot 'rnnoise\build\rnnoise.vcxproj'
$rnSolDir = (Join-Path $libroot 'rnnoise\build') + '\'
Invoke-MSBuild $rnProj $rnSolDir

$ebuProj = Join-Path $libroot 'libebur128\build\libebur128.vcxproj'
$ebuSolDir = (Join-Path $libroot 'libebur128\build') + '\'
Invoke-MSBuild $ebuProj $ebuSolDir

$agcProj = Join-Path $libroot 'WebRTC_AGC\build\WebRTC_AGC.vcxproj'
$agcSolDir = (Join-Path $libroot 'WebRTC_AGC\build') + '\'
Invoke-MSBuild $agcProj $agcSolDir

# RADE V1+V2 modem. Use headers from the exact full Opus source pin above.
$radeProj = Join-Path $libroot 'radae_c\msvc\radae_c.vcxproj'
& $msbuild $radeProj /m "/p:Configuration=$Configuration" /p:Platform=x64 /p:PlatformToolset=v145 "/p:OpusDir=$opusSource" /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'radae_c build failed' }

$required = @(
    (Join-Path $libroot "radae_c\build\x64\$Configuration\rade.lib"),
    (Join-Path $opusOut 'opus.lib'),
    (Join-Path $libroot "rnnoise\build\x64\$Configuration\rnnoise.lib"),
    (Join-Path $libroot "libebur128\build\x64\$Configuration\ebur128.lib"),
    (Join-Path $libroot "WebRTC_AGC\build\x64\$Configuration\WebRTC_AGC.lib")
)
foreach ($f in $required) {
    if (-not (Test-Path $f)) { throw "Required RADE library missing: $f" }
    Write-Host "RADE_DEP_PASS $f $((Get-Item $f).Length) bytes"
}
Write-Host 'RADE_DEPENDENCIES=PASS'
