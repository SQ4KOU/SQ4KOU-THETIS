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
}

# Freshness fix: the old implementation consumed exactly one configured hop per paint.
# At high overlap the producer writes much faster than the UI consumes, so the ring
# accumulates stale IQ. The displayed row then represents an old receiver frequency
# while Thetis labels/aligns it with the current VFO. This produces the large spectrum /
# waterfall horizontal drift seen during tuning and after changing FFT settings.
# Consume up to the newest available IQ on every rendered row while retaining at least
# the requested overlap when the UI is fast enough.
$freshMarker = '// SQ4KOU GPUWF freshness fix: never render stale ring-buffer IQ.'
if (-not $gpu.Contains($freshMarker)) {
    $pattern = '(?s)    UpdateHopSize\(s, sampleRate\);\s*int need = s\.primed \? s\.hopSize : s\.fftSize;.*?\s*    HRESULT hr = RunFFT\(s\);'
    $rxFresh = New-Object System.Text.RegularExpressions.Regex($pattern)
    if (-not $rxFresh.IsMatch($gpu)) { throw 'gpu_waterfall.cpp process/backlog anchor not found' }

    $replacement = @'
    UpdateHopSize(s, sampleRate);

    // SQ4KOU GPUWF freshness fix: never render stale ring-buffer IQ.
    // overlapPercent defines the minimum amount of new IQ required for a new row.
    // If more IQ accumulated between paints, advance farther (or re-prime from the
    // newest full FFT window) instead of leaving an ever-growing backlog behind.
    int available = CM_WaterfallIQ_Available(channel);
    int minimumNeeded = s.primed ? s.hopSize : s.fftSize;
    if (available < minimumNeeded) return 0;

    if (!s.primed || available >= s.fftSize)
    {
        // Re-prime from the newest complete FFT window. Discarding here is deliberate:
        // old IQ is useless for a real-time waterfall and is what caused VFO misalignment.
        int discard = available - s.fftSize;
        while (discard > 0)
        {
            int chunk = discard > s.fftSize ? s.fftSize : discard;
            int skipped = CM_WaterfallIQ_Get(channel, chunk, &s.tempI[0], &s.tempQ[0]);
            if (skipped <= 0) return 0;
            discard -= skipped;
        }

        int got = CM_WaterfallIQ_Get(channel, s.fftSize, &s.tempI[0], &s.tempQ[0]);
        if (got != s.fftSize) return 0;
        for (int i = 0; i < s.fftSize; ++i)
        {
            s.rolling[i].x = s.tempI[i];
            s.rolling[i].y = s.tempQ[i];
        }
        s.primed = true;
    }
    else
    {
        // Keep the configured overlap only when the UI is keeping pace. If several
        // hops arrived, consume all of them so this row stays tied to the current VFO.
        int advance = available;
        if (advance < s.hopSize) return 0;
        if (advance > s.fftSize) advance = s.fftSize;

        int keep = s.fftSize - advance;
        if (keep > 0)
            memmove(&s.rolling[0], &s.rolling[advance], (size_t)keep * sizeof(Float2));

        int got = CM_WaterfallIQ_Get(channel, advance, &s.tempI[0], &s.tempQ[0]);
        if (got != advance) return 0;
        for (int i = 0; i < advance; ++i)
        {
            int dstIndex = keep + i;
            s.rolling[dstIndex].x = s.tempI[i];
            s.rolling[dstIndex].y = s.tempQ[i];
        }
    }

    HRESULT hr = RunFFT(s);
'@
    $gpu = $rxFresh.Replace($gpu, $replacement, 1)
}
Write-Text $gpuPath $gpu

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

Write-Host 'SQ4KOU GPU Waterfall integration applied/verified, including real-time IQ freshness fix.'
