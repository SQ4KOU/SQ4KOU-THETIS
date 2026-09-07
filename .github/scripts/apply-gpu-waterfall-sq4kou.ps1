$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$utf8 = New-Object System.Text.UTF8Encoding($false)

function Read-Text([string]$relative) {
    return [System.IO.File]::ReadAllText((Join-Path $root $relative))
}

function Write-Text([string]$relative, [string]$text) {
    [System.IO.File]::WriteAllText((Join-Path $root $relative), $text, $utf8)
}

# ChannelMaster: include the non-blocking IQ capture API.
$cmasterPath = 'Project Files\Source\ChannelMaster\cmaster.c'
$cmaster = Read-Text $cmasterPath
if (-not $cmaster.Contains('#include "waterfall_iq.h"')) {
    if (-not $cmaster.Contains('#include "cmcomm.h"')) { throw 'cmaster.c include anchor not found' }
    $cmaster = $cmaster.Replace('#include "cmcomm.h"', "#include `"cmcomm.h`"`r`n#include `"waterfall_iq.h`"")
}

# Capture exactly the receiver IQ seen by Spectrum0, after the existing ANB/NOB chain.
$pushMarker = 'CM_WaterfallIQ_Push(rx, pcm->xcm_insize[stream], pcm->in[stream]);'
if (-not $cmaster.Contains($pushMarker)) {
    $pattern = '(?m)^(\s*xnob \(pcm->rcvr\[rx\]\.pnob\);[^\r\n]*\r?\n)'
    $rx = New-Object System.Text.RegularExpressions.Regex($pattern)
    if (-not $rx.IsMatch($cmaster)) { throw 'cmaster.c RX/NOB hook anchor not found' }
    $cmaster = $rx.Replace($cmaster, { param($m) $m.Groups[1].Value + "`t`t$pushMarker`r`n" }, 1)
}
Write-Text $cmasterPath $cmaster

# DirectCompute source includes Windows.h. Suppress legacy min/max macros.
$gpuPath = 'Project Files\Source\ChannelMaster\gpu_waterfall.cpp'
$gpu = Read-Text $gpuPath
if (-not $gpu.StartsWith('#define NOMINMAX')) {
    if (-not $gpu.StartsWith('#include <Windows.h>')) { throw 'gpu_waterfall.cpp Windows.h anchor not found' }
    $gpu = "#define NOMINMAX`r`n" + $gpu
    Write-Text $gpuPath $gpu
}

# Native project: compile the C ring buffer and C++ DirectCompute backend.
$vcxPath = 'Project Files\Source\ChannelMaster\ChannelMaster.vcxproj'
$vcx = Read-Text $vcxPath
if (-not $vcx.Contains('<ClCompile Include="waterfall_iq.c" />')) {
    $anchor = '    <ClCompile Include="cmaster.c" />'
    if (-not $vcx.Contains($anchor)) { throw 'ChannelMaster.vcxproj cmaster anchor not found' }
    $vcx = $vcx.Replace($anchor, $anchor + "`r`n    <ClCompile Include=`"waterfall_iq.c`" />")
}
if (-not $vcx.Contains('<ClCompile Include="gpu_waterfall.cpp"')) {
    $anchor = '    <ClCompile Include="waterfall_iq.c" />'
    if (-not $vcx.Contains($anchor)) { throw 'ChannelMaster.vcxproj waterfall_iq anchor not found' }
    $entry = "    <ClCompile Include=`"gpu_waterfall.cpp`">`r`n      <CompileAs>CompileAsCpp</CompileAs>`r`n    </ClCompile>"
    $vcx = $vcx.Replace($anchor, $anchor + "`r`n" + $entry)
}
elseif ($vcx.Contains('<ClCompile Include="gpu_waterfall.cpp" />')) {
    $vcx = $vcx.Replace('    <ClCompile Include="gpu_waterfall.cpp" />', "    <ClCompile Include=`"gpu_waterfall.cpp`">`r`n      <CompileAs>CompileAsCpp</CompileAs>`r`n    </ClCompile>")
}
if (-not $vcx.Contains('<CompileAs>CompileAsCpp</CompileAs>')) {
    throw 'ChannelMaster.vcxproj C++ override was not applied'
}
Write-Text $vcxPath $vcx

# Managed project: compile native bridge, Display integration and the Waterfall setup page.
$csprojPath = 'Project Files\Source\Console\Thetis.csproj'
$csproj = Read-Text $csprojPath
if (-not $csproj.Contains('<Compile Include="GPUWaterfallNative.cs" />')) {
    $anchor = '    <Compile Include="WaterfallPixelWriter.cs" />'
    if (-not $csproj.Contains($anchor)) { throw 'Thetis.csproj WaterfallPixelWriter anchor not found' }
    $csproj = $csproj.Replace($anchor, $anchor + "`r`n    <Compile Include=`"GPUWaterfallNative.cs`" />")
}
if (-not $csproj.Contains('<Compile Include="Display.GPUWaterfall.cs" />')) {
    $anchor = '    <Compile Include="GPUWaterfallNative.cs" />'
    if (-not $csproj.Contains($anchor)) { throw 'Thetis.csproj GPU native bridge anchor not found' }
    $csproj = $csproj.Replace($anchor, $anchor + "`r`n    <Compile Include=`"Display.GPUWaterfall.cs`" />")
}
if (-not $csproj.Contains('<Compile Include="Setup.GPUWaterfall.cs" />')) {
    $anchor = '    <Compile Include="Display.GPUWaterfall.cs" />'
    if (-not $csproj.Contains($anchor)) { throw 'Thetis.csproj GPU Display anchor not found' }
    $csproj = $csproj.Replace($anchor, $anchor + "`r`n    <Compile Include=`"Setup.GPUWaterfall.cs`" />")
}
Write-Text $csprojPath $csproj

# Display: make the existing class partial and inject one GPU update call before the
# unchanged CPU ready/copy block. This preserves all SQ4KOU rendering patches.
$displayPath = 'Project Files\Source\Console\display.cs'
$display = Read-Text $displayPath
if (-not $display.Contains('partial class Display')) {
    $rxClass = New-Object System.Text.RegularExpressions.Regex('(?m)^(\s*)class Display\s*$')
    if (-not $rxClass.IsMatch($display)) { throw 'display.cs class Display anchor not found' }
    $display = $rxClass.Replace($display, '$1partial class Display', 1)
}

$displayMarker = 'TryUpdateGPUWaterfallRow(rx, nDecimatedWidth, local_mox);'
if (-not $display.Contains($displayMarker)) {
    $rxReady = New-Object System.Text.RegularExpressions.Regex('(?m)^(\s*)if \(rx == 1 && waterfall_data_ready\)\s*$')
    if (-not $rxReady.IsMatch($display)) { throw 'display.cs waterfall ready anchor not found' }
    $display = $rxReady.Replace($display, { param($m) $m.Groups[1].Value + $displayMarker + "`r`n" + $m.Value }, 1)
}
Write-Text $displayPath $display

# Build the new Setup > Display > Waterfall page before getOptions() restores controls.
$setupPath = 'Project Files\Source\Console\setup.cs'
$setup = Read-Text $setupPath
$setupMarker = '            InitGPUWaterfallSetupUI();'
if (-not $setup.Contains($setupMarker)) {
    $anchor = '            InitWaterfallQualityControls();'
    if (-not $setup.Contains($anchor)) { throw 'setup.cs Waterfall quality anchor not found' }
    $setup = $setup.Replace($anchor, $anchor + "`r`n" + $setupMarker)
    Write-Text $setupPath $setup
}

Write-Host 'SQ4KOU GPU Waterfall V3 Setup integration applied/verified.'
