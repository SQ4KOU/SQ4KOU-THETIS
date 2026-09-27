$ErrorActionPreference = 'Stop'

function Read-Norm([string]$Path) {
    return ([IO.File]::ReadAllText($Path) -replace "`r`n", "`n")
}

function Write-Norm([string]$Path, [string]$Text) {
    $enc = New-Object System.Text.UTF8Encoding($false)
    [IO.File]::WriteAllText($Path, ($Text -replace "`n", "`r`n"), $enc)
}

function Replace-Between([string]$Text, [string]$Start, [string]$End, [string]$New, [string]$Label) {
    $s = $Text.IndexOf($Start, [StringComparison]::Ordinal)
    if ($s -lt 0) { throw "Missing start anchor: $Label" }
    $e = $Text.IndexOf($End, $s + $Start.Length, [StringComparison]::Ordinal)
    if ($e -lt 0) { throw "Missing end anchor: $Label" }
    return $Text.Substring(0, $s) + $New + $Text.Substring($e)
}

function Insert-After([string]$Text, [string]$Anchor, [string]$Insert, [string]$Label) {
    $i = $Text.IndexOf($Anchor, [StringComparison]::Ordinal)
    if ($i -lt 0) { throw "Missing insertion anchor: $Label" }
    if ($Text.IndexOf($Anchor, $i + $Anchor.Length, [StringComparison]::Ordinal) -ge 0) {
        throw "Insertion anchor is not unique: $Label"
    }
    $p = $i + $Anchor.Length
    return $Text.Substring(0, $p) + $Insert + $Text.Substring($p)
}

$displayPath = "Project Files/Source/Console/display.cs"
$consolePath = "Project Files/Source/Console/console.cs"
$projPath = "Project Files/Source/Console/Thetis.csproj"
$d = Read-Norm $displayPath

$bgStart = '                    if (native3DCandidate || nativeWfCandidate)'
$bgEnd = '                    // ---- strict phase 2: native D3D11 only ----'
$newBg = @'
                    if (native3DCandidate || nativeWfCandidate)
                    {
                        // Exactly one D2D BeginDraw/EndDraw pair per frame. The skin
                        // is composited natively before Pan3D/waterfall. Diagnostics
                        // proved the redundant frame-start ClearState+Flush can block,
                        // so there is deliberately no synchronous boundary here.
                        GPUWaterfallLogger.FrameStage("GPU_BG_NATIVE");
                        _bGpuBackdropDone = true;

                        if (!RenderNativeBackground())
                        {
                            GPUWaterfallLogger.LogRateLimited(
                                "BG-NATIVE", "fallback-solid", 1000,
                                "native background draw declined; using solid clear");
                            if (EnsureMeshRTV(_device))
                            {
                                _device.ImmediateContext.ClearRenderTargetView(
                                    _meshRTV,
                                    new Color4(
                                        m_cDX2_display_background_clear_colour.R,
                                        m_cDX2_display_background_clear_colour.G,
                                        m_cDX2_display_background_clear_colour.B,
                                        1f));
                            }
                        }
                    }

'@
$d = Replace-Between $d $bgStart $bgEnd $newBg "native background block"

$powerAnchor = @'
        public static void RequestDXRestart()
        {
            GPUWaterfallLogger.Log("STATE", "RequestDXRestart setup=" + _bDX2Setup + " path=" + RenderPathString());
            if (!_bDX2Setup) return;
            m_bDXRestartPending = true;
            m_bWarpDowngradeAttempted = false;
        }
'@
if (-not $d.Contains('public static void OnRadioPowerChanged(bool oldPower, bool newPower)')) {
    $powerMethod = @'

        public static void OnRadioPowerChanged(bool oldPower, bool newPower)
        {
            GPUWaterfallLogger.Log("POWER-DX",
                "radio power " + oldPower + " -> " + newPower +
                " setup=" + _bDX2Setup +
                " path=" + RenderPathString() +
                " pipeline=" + _gpuWaterfallPipelineEnabled +
                " forceCPU=" + m_bForceCPURendering);

            // POWER OFF stops Audio/xcmaster. Do not leave exact-IQ credit or
            // Pan3D input state armed across that discontinuity.
            try
            {
                ResetExactGPUWaterfallSourceForModeChange(
                    newPower && !m_bForceCPURendering && _gpuWaterfallPipelineEnabled);
            }
            catch (Exception ex)
            {
                GPUWaterfallLogger.Log("POWER-DX",
                    "exact source reset failed: " + ex.GetType().FullName + ": " + ex.Message);
            }

            ResetGPUWaterfallState(1, resetCalibration: false);
            ResetGPUWaterfallState(2, resetCalibration: false);
            data_ready = false;

            if (!newPower)
            {
                GPUWaterfallLogger.Log("POWER-DX",
                    "POWER OFF: renderer retained, input state invalidated");
                return;
            }

            // Treat POWER ON as a new native-render session. Audio.Start() has
            // already completed before PowerChangeHandlers fires. Recreate all
            // shared D2D/D3D resources only after the active frame reaches Present,
            // never synchronously on the UI thread.
            _native3DCircuitOpen = false;
            _nativeWfCircuitOpen = false;
            _native3DGoodFrames = 0;
            _nativeWfGoodFrames = 0;
            _pan3DNativeWarmupFrames = _pan3DEnabled ? 2 : 0;

            RequestDXRestart();
            GPUWaterfallLogger.Log("POWER-DX",
                "POWER ON: exact source reset + deferred full DX restart requested");
        }
'@
    $d = Insert-After $d $powerAnchor $powerMethod "radio power lifecycle"
}

$boundaryStart = '        private static void SharedContextBoundary(string label)'
$boundaryEnd = '        private static void ClearNativeSubpassState(string label)'
$newBoundary = @'
        private static void SharedContextBoundary(string label)
        {
            if (_device == null || _device.ImmediateContext == null) return;
            try
            {
                GPUWaterfallLogger.FrameStage("DX_BOUNDARY_CLEARSTATE:" + label);
                _device.ImmediateContext.ClearState();
                GPUWaterfallLogger.FrameStage("DX_BOUNDARY_FLUSH:" + label);
                _device.ImmediateContext.Flush();
                GPUWaterfallLogger.FrameStage("DX_BOUNDARY_DONE:" + label);
                GPUWaterfallLogger.LogRateLimited("DX-BOUNDARY", label, 1000,
                    "ClearState+Flush " + label);
            }
            catch (Exception ex)
            {
                GPUWaterfallLogger.Log("DX-BOUNDARY-FAIL",
                    label + " " + ex.GetType().FullName + ": " + ex.Message);
                throw;
            }
        }

'@
$d = Replace-Between $d $boundaryStart $boundaryEnd $newBoundary "SharedContextBoundary"

$setBgStart = '        public static void SetDX2BackgoundImage(System.Drawing.Image image)'
$setBgEnd = '        private static ID2D1Bitmap _bitmapBackground;'
$ss = $d.IndexOf($setBgStart, [StringComparison]::Ordinal)
$se = $d.IndexOf($setBgEnd, $ss, [StringComparison]::Ordinal)
if ($ss -lt 0 -or $se -lt 0) { throw "Cannot locate SetDX2BackgoundImage" }
$segment = $d.Substring($ss, $se - $ss)
$needle = '                if (image != null)'
$ni = $segment.IndexOf($needle, [StringComparison]::Ordinal)
if ($ni -lt 0) { throw "Cannot locate image gate in SetDX2BackgoundImage" }
if (-not $segment.Contains('SetNativeBackgroundImage(image);')) {
    $segment = $segment.Substring(0, $ni) +
        "                SetNativeBackgroundImage(image);`n`n" +
        $segment.Substring($ni)
    $d = $d.Substring(0, $ss) + $segment + $d.Substring($se)
}

$shutdownAnchor = '                    ShutdownDXStage("releaseGlowLayer", () => releaseGlowLayer());'
if (-not $d.Contains('ShutdownDXStage("ReleaseNativeBackgroundResources"')) {
    $d = Insert-After $d $shutdownAnchor "`n                    ShutdownDXStage(`"ReleaseNativeBackgroundResources`", () => ReleaseNativeBackgroundResources());" "native background shutdown"
}

Write-Norm $displayPath $d

$c = Read-Norm $consolePath
$onPowerOld = @'
        private void OnPowerChangeHander(bool oldPower, bool newPower)
        {
            if (newPower)
            {
'@
$onPowerNew = @'
        private void OnPowerChangeHander(bool oldPower, bool newPower)
        {
            GPUWaterfallLogger.Log("POWER",
                "PowerChangeHandlers " + oldPower + " -> " + newPower +
                " DataFlowing=" + DataFlowing);
            Display.OnRadioPowerChanged(oldPower, newPower);

            if (newPower)
            {
'@
if (-not $c.Contains('Display.OnRadioPowerChanged(oldPower, newPower);')) {
    $c = Replace-Between $c $onPowerOld '                // reset meter pixel historory when powering up' ($onPowerNew + '                // reset meter pixel historory when powering up' + "`n") "OnPowerChangeHander lifecycle hook"
}
Write-Norm $consolePath $c



$p = Read-Norm $projPath
if (-not $p.Contains('<Compile Include="Display.NativeBackground.cs" />')) {
    $projAnchor = '    <Compile Include="Display.Pan3DMesh.cs" />'
    $p = Insert-After $p $projAnchor "`n    <Compile Include=`"Display.NativeBackground.cs`" />" "native background compile item"
}
Write-Norm $projPath $p

Write-Host "PASS: applied Pan3D stall fix and native D3D11 skin compositor"
