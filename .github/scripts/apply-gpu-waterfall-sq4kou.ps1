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

# ChannelMaster: include the lock-free IQ capture API.
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

# DirectCompute source includes Windows.h. Suppress legacy min/max macros so
# std::min/std::max compile correctly under the ChannelMaster Windows headers.
$gpuPath = 'Project Files\Source\ChannelMaster\gpu_waterfall.cpp'
$gpu = Read-Text $gpuPath
if (-not $gpu.StartsWith('#define NOMINMAX')) {
    if (-not $gpu.StartsWith('#include <Windows.h>')) { throw 'gpu_waterfall.cpp Windows.h anchor not found' }
    $gpu = "#define NOMINMAX`r`n" + $gpu
}

# V2 resolve: the first implementation sampled a single FFT bin (linear interpolation)
# for every screen pixel. With 32768 FFT points this aliases many bins into one pixel and
# produces the visible granular/checkerboard waterfall. Resolve in linear power over the
# full pixel bandwidth, with a small peak-preserving component for narrow CW carriers.
$resolveMarker = '// SQ4KOU V2 resolve: integrate FFT power over each display pixel.'
if (-not $gpu.Contains($resolveMarker)) {
    $resolvePattern = '(?s)    float span = displayHighHz - displayLowHz;.*?    return 1;'
    $resolveRegex = New-Object System.Text.RegularExpressions.Regex($resolvePattern)
    if (-not $resolveRegex.IsMatch($gpu)) { throw 'gpu_waterfall.cpp resolve anchor not found' }

    $resolveCode = @'
    // SQ4KOU V2 resolve: integrate FFT power over each display pixel.
    // This is the anti-alias/resolve stage missing from V1. Work in linear power,
    // then return to dB. A 20% peak term preserves narrow carriers without bringing
    // back the single-bin speckle pattern.
    float span = displayHighHz - displayLowHz;
    float nyquist = 0.5f * (float)sampleRate;
    const double kMinPower = 1.0e-30;

    for (int x = 0; x < displayWidth; ++x)
    {
        float fx0 = displayLowHz + span * ((float)x / (float)displayWidth);
        float fx1 = displayLowHz + span * ((float)(x + 1) / (float)displayWidth);
        double p0 = ((double)fx0 + (double)nyquist) / (double)sampleRate * (double)s.fftSize;
        double p1 = ((double)fx1 + (double)nyquist) / (double)sampleRate * (double)s.fftSize;

        if (p1 < p0)
        {
            double t = p0;
            p0 = p1;
            p1 = t;
        }

        if (p0 < 0.0) p0 = 0.0;
        if (p1 < 0.0) p1 = 0.0;
        if (p0 > (double)s.fftSize) p0 = (double)s.fftSize;
        if (p1 > (double)s.fftSize) p1 = (double)s.fftSize;

        double binSpan = p1 - p0;
        if (binSpan <= 1.25)
        {
            double center = 0.5 * (p0 + p1);
            if (center < 0.0) center = 0.0;
            if (center > (double)(s.fftSize - 1)) center = (double)(s.fftSize - 1);
            int i0 = (int)floor(center);
            int i1 = i0 + 1;
            if (i1 >= s.fftSize) i1 = s.fftSize - 1;
            double frac = center - (double)i0;
            double pow0 = pow(10.0, (double)s.magnitude[i0] / 10.0);
            double pow1 = pow(10.0, (double)s.magnitude[i1] / 10.0);
            double power = pow0 + (pow1 - pow0) * frac;
            if (power < kMinPower) power = kMinPower;
            outputDb[x] = (float)(10.0 * log10(power));
            continue;
        }

        int firstBin = (int)floor(p0);
        int lastBin = (int)ceil(p1);
        double sumPower = 0.0;
        double sumWeight = 0.0;
        double peakPower = 0.0;

        for (int b = firstBin; b < lastBin; ++b)
        {
            if (b < 0 || b >= s.fftSize) continue;
            double left = p0 > (double)b ? p0 : (double)b;
            double right = p1 < (double)(b + 1) ? p1 : (double)(b + 1);
            double weight = right - left;
            if (weight <= 0.0) continue;

            double power = pow(10.0, (double)s.magnitude[b] / 10.0);
            sumPower += power * weight;
            sumWeight += weight;
            if (power > peakPower) peakPower = power;
        }

        double meanPower = sumWeight > 0.0 ? (sumPower / sumWeight) : kMinPower;
        if (meanPower < kMinPower) meanPower = kMinPower;
        if (peakPower < meanPower) peakPower = meanPower;

        double resolvedPower = 0.80 * meanPower + 0.20 * peakPower;
        if (resolvedPower < kMinPower) resolvedPower = kMinPower;
        outputDb[x] = (float)(10.0 * log10(resolvedPower));
    }
    return 1;
'@
    $gpu = $resolveRegex.Replace($gpu, $resolveCode, 1)
}
Write-Text $gpuPath $gpu

# Native project: compile the new C ring buffer and C++ DirectCompute backend.
# ChannelMaster globally forces CompileAsC, therefore the GPU backend must override
# that setting for this file only.
$vcxPath = 'Project Files\Source\ChannelMaster\ChannelMaster.vcxproj'
$vcx = Read-Text $vcxPath
if (-not $vcx.Contains('<ClCompile Include="gpu_waterfall.cpp"')) {
    $anchor = '    <ClCompile Include="cmaster.c" />'
    if (-not $vcx.Contains($anchor)) { throw 'ChannelMaster.vcxproj compile anchor not found' }
    $replacement = $anchor + "`r`n    <ClCompile Include=`"waterfall_iq.c`" />`r`n    <ClCompile Include=`"gpu_waterfall.cpp`">`r`n      <CompileAs>CompileAsCpp</CompileAs>`r`n    </ClCompile>"
    $vcx = $vcx.Replace($anchor, $replacement)
}
elseif ($vcx.Contains('<ClCompile Include="gpu_waterfall.cpp" />')) {
    $vcx = $vcx.Replace('    <ClCompile Include="gpu_waterfall.cpp" />', "    <ClCompile Include=`"gpu_waterfall.cpp`">`r`n      <CompileAs>CompileAsCpp</CompileAs>`r`n    </ClCompile>")
}
if (-not $vcx.Contains('<CompileAs>CompileAsCpp</CompileAs>')) {
    throw 'ChannelMaster.vcxproj C++ override was not applied'
}
Write-Text $vcxPath $vcx

# Managed project: compile the bridge and Display partial integration.
$csprojPath = 'Project Files\Source\Console\Thetis.csproj'
$csproj = Read-Text $csprojPath
if (-not $csproj.Contains('<Compile Include="Display.GPUWaterfall.cs" />')) {
    $anchor = '    <Compile Include="WaterfallPixelWriter.cs" />'
    if (-not $csproj.Contains($anchor)) { throw 'Thetis.csproj compile anchor not found' }
    $replacement = $anchor + "`r`n    <Compile Include=`"GPUWaterfallNative.cs`" />`r`n    <Compile Include=`"Display.GPUWaterfall.cs`" />"
    $csproj = $csproj.Replace($anchor, $replacement)
    Write-Text $csprojPath $csproj
}

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

Write-Host 'SQ4KOU GPU Waterfall V2 integration patch applied/verified.'
