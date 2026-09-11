$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$consoleDir = Join-Path $root 'Project Files\Source\Console'
$utf8 = New-Object System.Text.UTF8Encoding($false)

function Read-Text([string]$path) {
    return [System.IO.File]::ReadAllText($path)
}

function Write-Text([string]$path, [string]$text) {
    [System.IO.File]::WriteAllText($path, $text, $utf8)
}

# -----------------------------------------------------------------------------
# 1. SETUP LIFETIME FIX
# Never restore defaults merely because Setup/Waterfall is opened again.
# Defaults remain available only through the explicit "EU2AV defaults" button.
# Also do not force Render/Gamma defaults on every Setup instance.
# -----------------------------------------------------------------------------
$setupFile = Get-ChildItem $consoleDir -Filter '*.cs' -File | Where-Object {
    (Select-String -Path $_.FullName -SimpleMatch 'private void InitGPUWaterfallSetupUI()' -Quiet)
} | Select-Object -First 1

if (-not $setupFile) { throw 'InitGPUWaterfallSetupUI source not found' }

$setup = Read-Text $setupFile.FullName
$methodStart = $setup.IndexOf('private void InitGPUWaterfallSetupUI()')
$tabCreate = $setup.IndexOf('tpWaterfall = new TabPage();', $methodStart)
if ($methodStart -lt 0 -or $tabCreate -lt 0) { throw 'Waterfall Setup init anchors not found' }

$head = $setup.Substring(0, $methodStart)
$initPrefix = $setup.Substring($methodStart, $tabCreate - $methodStart)
$tail = $setup.Substring($tabCreate)

$initPrefix = [regex]::Replace($initPrefix, '(?m)^\s*Display\.ApplyGPUWaterfallEU2AVDefaults\(\);\s*\r?\n', '')
$initPrefix = [regex]::Replace($initPrefix, '(?m)^\s*WaterfallEnhancer\.SetQuality\([^\r\n;]+\);[^\r\n]*\r?\n', '')
$initPrefix = [regex]::Replace($initPrefix, '(?m)^\s*WaterfallEnhancer\.SetGamma\([^\r\n;]+\);[^\r\n]*\r?\n', '')
$initPrefix = [regex]::Replace($initPrefix, '(?m)^\s*// V4 controls use new DB names[^\r\n]*\r?\n', '')
$initPrefix = [regex]::Replace($initPrefix, '(?m)^\s*// override the Extended reference defaults[^\r\n]*\r?\n', '')

$setup = $head + $initPrefix + $tail
Write-Text $setupFile.FullName $setup
Write-Host "Patched Setup state lifetime: $($setupFile.Name)"

# -----------------------------------------------------------------------------
# 2. GPU CONFIGURATION FIX
# FFT-size changes require D3D/FFT resource rebuild. Window/magnitude/overlap/
# resampling changes do not: native CM_GPUWaterfall_Configure supports hot config.
# Avoid tearing down the ring/D3D device for every combo-box click.
# -----------------------------------------------------------------------------
$displayFile = Get-ChildItem $consoleDir -Filter '*.cs' -File | Where-Object {
    (Select-String -Path $_.FullName -SimpleMatch 'public static int GPUWaterfallWindowType' -Quiet) -and
    (Select-String -Path $_.FullName -SimpleMatch 'private static bool ConfigureGPUWaterfall(int channel)' -Quiet)
} | Select-Object -First 1

if (-not $displayFile) { throw 'Display GPU Waterfall implementation not found' }

$display = Read-Text $displayFile.FullName

$helper = @'
        private static void ReconfigureGPUWaterfallRuntime()
        {
            for (int ch = 0; ch < 2; ch++)
            {
                // A configuration change starts a new coherent waterfall history,
                // but it must not tear down the D3D device or IQ ring.
                _gpuPrevValid[ch] = false;
                _gpuCalibrationValid[ch] = false;
                _gpuCalibrationOffset[ch] = 0.0f;
                _gpuCalibrationFrame[ch] = 0;

                if (_gpuWaterfallConfiguredFFT[ch] != 0 && !_gpuWaterfallFailed[ch])
                {
                    if (!ConfigureGPUWaterfall(ch))
                    {
                        try { GPUWaterfallNative.CM_GPUWaterfall_Free(ch); } catch { }
                        _gpuWaterfallConfiguredFFT[ch] = 0;
                        // Allow the normal EnsureGPUWaterfall path to retry cleanly.
                        _gpuWaterfallFailed[ch] = false;
                    }
                }
            }
        }

'@

if (-not $display.Contains('private static void ReconfigureGPUWaterfallRuntime()')) {
    $anchor = '        private static bool ConfigureGPUWaterfall(int channel)'
    $idx = $display.IndexOf($anchor)
    if ($idx -lt 0) { throw 'ConfigureGPUWaterfall insertion anchor not found' }
    $display = $display.Insert($idx, $helper)
}

$hotProperties = @(
    'GPUWaterfallWindowType',
    'GPUWaterfallKaiserBeta',
    'GPUWaterfallMagnitudeMode',
    'GPUWaterfallAutoOverlap',
    'GPUWaterfallOverlapPercent',
    'GPUWaterfallLanczosWindow',
    'GPUWaterfallResamplingMode'
)

foreach ($property in $hotProperties) {
    $marker = "public static"
    $nameAt = $display.IndexOf($property)
    if ($nameAt -lt 0) { throw "Property not found: $property" }
    $start = $display.LastIndexOf($marker, $nameAt)
    if ($start -lt 0) { throw "Property start not found: $property" }
    $next = $display.IndexOf($marker, $nameAt + $property.Length)
    if ($next -lt 0) { $next = $display.Length }
    $segment = $display.Substring($start, $next - $start)
    if ($segment.Contains('ResetGPUWaterfallState();')) {
        $segment = $segment.Replace('ResetGPUWaterfallState();', 'ReconfigureGPUWaterfallRuntime();')
        $display = $display.Substring(0, $start) + $segment + $display.Substring($next)
    }
}

Write-Text $displayFile.FullName $display
Write-Host "Patched GPU hot reconfiguration: $($displayFile.Name)"

# Guard against regression in this build.
$verifySetup = Read-Text $setupFile.FullName
$ms = $verifySetup.IndexOf('private void InitGPUWaterfallSetupUI()')
$mt = $verifySetup.IndexOf('tpWaterfall = new TabPage();', $ms)
$prefix = $verifySetup.Substring($ms, $mt - $ms)
if ($prefix.Contains('ApplyGPUWaterfallEU2AVDefaults')) { throw 'Defaults are still applied during Setup init' }
if ($prefix.Contains('WaterfallEnhancer.SetQuality')) { throw 'Render quality is still forced during Setup init' }
if ($prefix.Contains('WaterfallEnhancer.SetGamma')) { throw 'Gamma is still forced during Setup init' }

Write-Host 'GPU Waterfall state/reset fix applied and verified.'
