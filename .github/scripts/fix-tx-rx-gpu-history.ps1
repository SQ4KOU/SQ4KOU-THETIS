$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$path = Join-Path $root 'Project Files\Source\Console\Display.GPUWaterfall.cs'
$utf8 = New-Object System.Text.UTF8Encoding($false)
$text = [System.IO.File]::ReadAllText($path)

# Runtime failure reproduced by operator: after TX the RX waterfall starts again from the top.
# Root cause: MOX/source transition marks the GPU renderer as empty, so the next RX row executes renderer.Clear().
# Preserve the renderer/history texture; reset only the IQ accumulation needed to avoid RX/TX FFT mixing.

$oldGate = 'if (index < 0 || index > 1 || localMox || !ManagedGPUFFTRequested || !IsGPUWaterfallPaletteScheme(scheme))'
$newGate = 'if (index < 0 || index > 1 || !ManagedGPUFFTRequested || !IsGPUWaterfallPaletteScheme(scheme))'
if ($text.Contains($oldGate)) {
    $text = $text.Replace($oldGate, $newGate)
} elseif (-not $text.Contains($newGate)) {
    throw 'Managed GPU renderer MOX gate anchor not found'
}

$oldTransition = @'
                _gpuSampleCredit[num2] = 0;
                _gpuFirstFillDone[num2] = false;
                _gpuRendererHasData[num2] = false;
                _gpuLastIQSource[num2] = gpuSourceStream;
'@
$newTransition = @'
                _gpuSampleCredit[num2] = 0;
                _gpuFirstFillDone[num2] = false;
                // Preserve WaterfallGPURenderer history across RX<->TX. Only the IQ accumulator is restarted.
                _gpuLastIQSource[num2] = gpuSourceStream;
'@
if ($text.Contains($oldTransition)) {
    $text = $text.Replace($oldTransition, $newTransition)
} elseif (-not $text.Contains('Preserve WaterfallGPURenderer history across RX<->TX')) {
    throw 'RX/TX source-transition renderer reset anchor not found'
}

$oldRateReset = @'
                    gPUWaterfallPipeline.ResamplingMode = _gpuWaterfallResamplingMode;
                }
                ResetGPUWaterfallState(rx);
            }
            if (gPUWaterfallPipeline == null || !gPUWaterfallPipeline.IsInitialized)
'@
$newRateReset = @'
                    gPUWaterfallPipeline.ResamplingMode = _gpuWaterfallResamplingMode;
                }
                // Sample-rate rebinding must not invalidate the already rendered waterfall history.
                // The source-transition block below resets the IQ accumulator without clearing the renderer.
            }
            if (gPUWaterfallPipeline == null || !gPUWaterfallPipeline.IsInitialized)
'@
if ($text.Contains($oldRateReset)) {
    $text = $text.Replace($oldRateReset, $newRateReset)
} elseif (-not $text.Contains('Sample-rate rebinding must not invalidate the already rendered waterfall history.')) {
    throw 'Sample-rate reset anchor not found'
}

[System.IO.File]::WriteAllText($path, $text, $utf8)

$verify = [System.IO.File]::ReadAllText($path)
if ($verify.Contains($oldGate)) { throw 'MOX still disables the managed GPU renderer' }
if (-not $verify.Contains($newGate)) { throw 'Managed GPU renderer gate verification failed' }
if ($verify.Contains($oldTransition)) { throw 'RX/TX transition still clears renderer history state' }
if (-not $verify.Contains('Preserve WaterfallGPURenderer history across RX<->TX')) { throw 'History-preservation marker missing' }
if ($verify.Contains($oldRateReset)) { throw 'Sample-rate change still performs full GPU waterfall reset' }
if (-not $verify.Contains('int stream = gpuSourceStream;')) { throw 'TX/RX IQ source switching was lost' }
if (-not $verify.Contains('CM_WaterfallIQ_Available(stream)')) { throw 'GPU IQ consumer path was lost' }

Write-Host 'SQ4KOU_TX_RX_GPU_HISTORY_FIX=PASS'
