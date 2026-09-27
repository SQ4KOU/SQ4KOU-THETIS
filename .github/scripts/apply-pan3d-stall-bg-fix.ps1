$ErrorActionPreference = 'Stop'

function Read-Norm([string]$Path) {
    return ([IO.File]::ReadAllText($Path) -replace "`r`n", "`n")
}

function Write-Norm([string]$Path, [string]$Text) {
    $enc = New-Object System.Text.UTF8Encoding($false)
    [IO.File]::WriteAllText($Path, ($Text -replace "`n", "`r`n"), $enc)
}

function Replace-Once([string]$Text, [string]$Old, [string]$New, [string]$Label) {
    $first = $Text.IndexOf($Old, [StringComparison]::Ordinal)
    if ($first -lt 0) { throw "Missing patch anchor: $Label" }
    $second = $Text.IndexOf($Old, $first + $Old.Length, [StringComparison]::Ordinal)
    if ($second -ge 0) { throw "Patch anchor is not unique: $Label" }
    return $Text.Substring(0, $first) + $New + $Text.Substring($first + $Old.Length)
}

$displayPath = "Project Files/Source/Console/display.cs"
$projPath = "Project Files/Source/Console/Thetis.csproj"

$d = Read-Norm $displayPath

$oldBg = @'
                    if (native3DCandidate || nativeWfCandidate)
                    {
                        // Stability rule: exactly ONE Direct2D BeginDraw/EndDraw pair
                        // per frame.  The old background prepass opened a second D2D
                        // frame before native D3D11 work and eventually blocked inside
                        // that prepass.  Native passes now begin from a D3D11 clear.
                        GPUWaterfallLogger.FrameStage("GPU_BG_CLEAR");
                        SharedContextBoundary("frame-start-d2d-to-native");

                        // Set this before touching the RTV. Even if RTV recreation
                        // fails, native passes are forbidden from falling back to the
                        // legacy DrawSkinBackgroundPrepass() second D2D frame.
                        _bGpuBackdropDone = true;

                        if (EnsureMeshRTV(_device))
                        {
                            _device.ImmediateContext.ClearRenderTargetView(
                                _meshRTV,
                                new Color4(
                                    m_cDX2_display_background_clear_colour.R,
                                    m_cDX2_display_background_clear_colour.G,
                                    m_cDX2_display_background_clear_colour.B,
                                    1f));

                            if (_bitmapBackground != null)
                            {
                                GPUWaterfallLogger.LogRateLimited(
                                    "STABILITY", "native-bg-solid", 5000,
                                    "native frame uses solid background; D2D skin prepass disabled");
                            }
                        }
                    }
'@
$newBg = @'
                    if (native3DCandidate || nativeWfCandidate)
                    {
                        // Keep exactly one D2D BeginDraw/EndDraw pair per frame, but
                        // draw the skin natively before Pan3D/waterfall. The watchdog
                        // proved the redundant frame-start ClearState+Flush can block,
                        // so there is no synchronous boundary here.
                        GPUWaterfallLogger.FrameStage("GPU_BG_NATIVE");

                        // Never allow the legacy nested D2D background helper to
                        // re-enter this frame.
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
$d = Replace-Once $d $oldBg $newBg "native background + frame-start boundary"

$oldBoundary = @'
        private static void SharedContextBoundary(string label)
        {
            if (_device == null || _device.ImmediateContext == null) return;
            try
            {
                _device.ImmediateContext.ClearState();
                _device.ImmediateContext.Flush();
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
$d = Replace-Once $d $oldBoundary $newBoundary "boundary diagnostics"

$oldSync = @'
                if (_bitmapBackground != null)
                {
                    _bitmapBackground?.Dispose();
                    _bitmapBackground = null;
                }

                if (image != null)
'@
$newSync = @'
                if (_bitmapBackground != null)
                {
                    _bitmapBackground?.Dispose();
                    _bitmapBackground = null;
                }

                SetNativeBackgroundImage(image);

                if (image != null)
'@
$d = Replace-Once $d $oldSync $newSync "native skin synchronization"

$oldShutdown = @'
                    ShutdownDXStage("releaseGlowLayer", () => releaseGlowLayer());
                    ShutdownDXStage("ReleaseGpuMeshDeviceObjects", () => ReleaseGpuMeshDeviceObjects());
'@
$newShutdown = @'
                    ShutdownDXStage("releaseGlowLayer", () => releaseGlowLayer());
                    ShutdownDXStage("ReleaseNativeBackgroundResources", () => ReleaseNativeBackgroundResources());
                    ShutdownDXStage("ReleaseGpuMeshDeviceObjects", () => ReleaseGpuMeshDeviceObjects());
'@
$d = Replace-Once $d $oldShutdown $newShutdown "native skin shutdown"

Write-Norm $displayPath $d

$p = Read-Norm $projPath
$oldProj = @'
    <Compile Include="Display.Pan3DMesh.cs" />
    <Compile Include="Display.Pan2DMesh.cs" />
'@
$newProj = @'
    <Compile Include="Display.Pan3DMesh.cs" />
    <Compile Include="Display.NativeBackground.cs" />
    <Compile Include="Display.Pan2DMesh.cs" />
'@
$p = Replace-Once $p $oldProj $newProj "Display.NativeBackground compile item"
Write-Norm $projPath $p

Write-Host "PASS: applied Pan3D frame-start stall fix + native D3D11 skin compositor"
