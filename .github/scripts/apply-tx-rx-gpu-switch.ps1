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

# Port only the proven TX<->RX GPU waterfall source switching mechanism.
# Frozen PASS RX capture remains channels 0/1. TX uses isolated ring 2.

$cmasterPath = 'Project Files\Source\ChannelMaster\cmaster.c'
$cmaster = Read-Text $cmasterPath
$txMarker = '// SQ4KOU GPUWF TX source: post-DSP transmitter IQ, isolated from RX rings.'
if (-not $cmaster.Contains($txMarker)) {
    $anchor = 'xpipe (stream, 1, pcm->xmtr[tx].out);'
    $idx = $cmaster.IndexOf($anchor)
    if ($idx -lt 0) { throw 'cmaster.c TX xpipe anchor not found' }
    $insertAt = $idx + $anchor.Length
    $insert = "`r`n`t`t$txMarker`r`n`t`tCM_WaterfallIQ_Push(2, pcm->xmtr[tx].ch_outsize, pcm->xmtr[tx].out[0]);"
    $cmaster = $cmaster.Insert($insertAt, $insert)
}
Write-Text $cmasterPath $cmaster

$gpuPath = 'Project Files\Source\Console\Display.GPUWaterfall.cs'
$gpu = Read-Text $gpuPath

if (-not $gpu.Contains('_gpuLastIQSource')) {
    $anchor = 'private static bool[] _gpuFirstFillDone = new bool[2];'
    if (-not $gpu.Contains($anchor)) { throw 'Display.GPUWaterfall.cs state anchor not found' }
    $gpu = $gpu.Replace($anchor, $anchor + "`r`n`r`n        // 0=RX1, 1=RX2, 2=TX post-DSP ring. Prevent RX/TX sample mixing across MOX transitions.`r`n        private static int[] _gpuLastIQSource = new int[2] { -1, -1 };")
}

if (-not $gpu.Contains('for (int ch = 0; ch < 3; ch++)')) {
    $rxEnable = New-Object System.Text.RegularExpressions.Regex('(?s)(private static void SetNativeWaterfallIQEnabled\(bool enabled\)\s*\{\s*for \(int ch = 0; ch < )2(; ch\+\+\))')
    if (-not $rxEnable.IsMatch($gpu)) { throw 'SetNativeWaterfallIQEnabled loop anchor not found' }
    $gpu = $rxEnable.Replace($gpu, '${1}3${2}', 1)
}

if (-not $gpu.Contains('bool gpuTxSource = localMox(rx) && !DisplayDuplex;')) {
    $ratePattern = '(?s)            int inputRate = cmaster\.GetInputRate\(0, rx - 1\);\s*            if \(gPUWaterfallPipeline != null && inputRate > 0 && Math\.Abs\(gPUWaterfallPipeline\.SampleRate - \(float\)inputRate\) > 1f\)\s*            \{\s*                EnsureGPUWaterfallPipeline\(rx, width, 1\);\s*                gPUWaterfallPipeline = \(\(rx == 1\) \? _gpuFFT1 : _gpuFFT2\);\s*                ResetGPUWaterfallState\(rx\);\s*            \}'
    $rxRate = New-Object System.Text.RegularExpressions.Regex($ratePattern)
    if (-not $rxRate.IsMatch($gpu)) { throw 'ProcessGPUWaterfall sample-rate block not found' }
    $replacement = @'
            bool gpuTxSource = localMox(rx) && !DisplayDuplex;
            int gpuSourceStream = gpuTxSource ? 2 : ((rx != 1) ? 1 : 0);
            int inputRate = gpuTxSource ? cmaster.GetChannelOutputRate(1, 0) : cmaster.GetInputRate(0, rx - 1);
            if (inputRate <= 0 && gpuTxSource)
            {
                inputRate = cmaster.GetInputRate(1, 0);
            }
            if (inputRate <= 0)
            {
                inputRate = gpuTxSource ? 192000 : ((rx == 1) ? SampleRateRX1 : SampleRateRX2);
            }
            if (gPUWaterfallPipeline != null && inputRate > 0 && Math.Abs(gPUWaterfallPipeline.SampleRate - (float)inputRate) > 1f)
            {
                // Keep the same recovered GPU pipeline, but bind it to the active IQ source rate.
                gPUWaterfallPipeline.Resize(_gpuWaterfallFFTSize, width, inputRate);
                if (gPUWaterfallPipeline.IsInitialized)
                {
                    gPUWaterfallPipeline.WindowType = _gpuWaterfallWindowType;
                    gPUWaterfallPipeline.KaiserBeta = _gpuWaterfallKaiserBeta;
                    gPUWaterfallPipeline.MagnitudeMode = _gpuWaterfallMagnitudeMode;
                    gPUWaterfallPipeline.LanczosWindow = _gpuWaterfallLanczosWindow;
                    gPUWaterfallPipeline.ResamplingMode = _gpuWaterfallResamplingMode;
                }
                ResetGPUWaterfallState(rx);
            }
'@
    $gpu = $rxRate.Replace($gpu, $replacement, 1)
}

if (-not $gpu.Contains('_gpuLastIQSource[num2] != gpuSourceStream')) {
    $anchor = "            int num2 = rx - 1;`r`n            if (fFTSize != num)"
    if (-not $gpu.Contains($anchor)) {
        $anchor = "            int num2 = rx - 1;`n            if (fFTSize != num)"
    }
    if (-not $gpu.Contains($anchor)) { throw 'ProcessGPUWaterfall num2/source-switch anchor not found' }
    $nl = if ($gpu.Contains("`r`n")) { "`r`n" } else { "`n" }
    $insert = @"
            int num2 = rx - 1;
            if (_gpuLastIQSource[num2] != gpuSourceStream)
            {
                // RX<->TX transition: discard accumulated samples so one FFT can never mix sources.
                _gpuIQringHead[num2] = 0;
                _gpuIQringCount[num2] = 0;
                _gpuSampleCredit[num2] = 0;
                _gpuFirstFillDone[num2] = false;
                _gpuRendererHasData[num2] = false;
                _gpuLastIQSource[num2] = gpuSourceStream;
                if (rx == 1)
                {
                    _gpuCalInitRX1 = false;
                    _gpuCalStartupCountRX1 = 0;
                }
                else
                {
                    _gpuCalInitRX2 = false;
                    _gpuCalStartupCountRX2 = 0;
                }
            }
            if (fFTSize != num)
"@
    $insert = $insert -replace "`r?`n", $nl
    $gpu = $gpu.Replace($anchor, $insert.TrimEnd("`r","`n"))
}

if ($gpu.Contains('int stream = ((rx != 1) ? 1 : 0);')) {
    $gpu = $gpu.Replace('int stream = ((rx != 1) ? 1 : 0);', 'int stream = gpuSourceStream;')
}
if (-not $gpu.Contains('int stream = gpuSourceStream;')) { throw 'GPU IQ stream selection patch was not applied' }
Write-Text $gpuPath $gpu

$displayPath = 'Project Files\Source\Console\display.cs'
$display = Read-Text $displayPath
if ($display.Contains('if (ManagedGPUFFTRequested && !local_mox)')) {
    $display = $display.Replace('if (ManagedGPUFFTRequested && !local_mox)', 'if (ManagedGPUFFTRequested)')
}
if (-not $display.Contains('if (ManagedGPUFFTRequested)')) { throw 'display.cs managed GPU TX gate patch was not applied' }
Write-Text $displayPath $display

$verifyCM = Read-Text $cmasterPath
$verifyGPU = Read-Text $gpuPath
$verifyDisplay = Read-Text $displayPath
if (-not $verifyCM.Contains('CM_WaterfallIQ_Push(rx, pcm->xcm_insize[stream], pcm->in[stream]);')) { throw 'RX GPU IQ producer was altered/missing' }
if (-not $verifyCM.Contains('CM_WaterfallIQ_Push(2, pcm->xmtr[tx].ch_outsize, pcm->xmtr[tx].out[0]);')) { throw 'TX GPU IQ producer missing' }
if (-not $verifyGPU.Contains('for (int ch = 0; ch < 3; ch++)')) { throw 'TX IQ ring is not initialized/enabled' }
if (-not $verifyGPU.Contains('bool gpuTxSource = localMox(rx) && !DisplayDuplex;')) { throw 'TX source selector missing' }
if (-not $verifyGPU.Contains('int stream = gpuSourceStream;')) { throw 'TX source ring is not consumed' }
if (-not $verifyGPU.Contains('cmaster.GetChannelOutputRate(1, 0)')) { throw 'TX GPU FFT does not follow transmitter output rate' }
if ($verifyDisplay.Contains('ManagedGPUFFTRequested && !local_mox')) { throw 'MOX still blocks the managed GPU waterfall' }
Write-Host 'SQ4KOU_TX_RX_GPU_SWITCH=PASS'
