$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$consoleDir = Join-Path $repo 'Project Files\Source\Console'
$csproj = Join-Path $consoleDir 'Thetis.csproj'
$pipeline = Join-Path $consoleDir 'GPUWaterfallPipeline.cs'
$renderer = Join-Path $consoleDir 'WaterfallGPURenderer.cs'
$displayGpu = Join-Path $consoleDir 'Display.GPUWaterfall.cs'
$display = Join-Path $consoleDir 'display.cs'

$shaders = @(
    'waterfall_postproc.bin',
    'waterfall_row_cs.bin',
    'waterfall_fft_bitreverse_cs.bin',
    'waterfall_fft_stage_ab_cs.bin',
    'waterfall_fft_stage_ba_cs.bin',
    'waterfall_fft_magnitude_cs.bin',
    'waterfall_resolve_cs.bin'
)

function Write-Utf8NoBom([string]$Path, [string]$Text) {
    [System.IO.File]::WriteAllText($Path, $Text, [System.Text.UTF8Encoding]::new($false))
}

function Replace-MethodBefore([string]$Text, [string]$MethodSignature, [string]$NextSignature, [string]$Replacement) {
    $start = $Text.IndexOf($MethodSignature, [System.StringComparison]::Ordinal)
    if ($start -lt 0) { throw "Method signature not found: $MethodSignature" }

    $lineStart = $Text.LastIndexOf("`n", $start)
    if ($lineStart -lt 0) { $lineStart = 0 } else { $lineStart++ }

    $next = $Text.IndexOf($NextSignature, $start, [System.StringComparison]::Ordinal)
    if ($next -lt 0) { throw "Following signature not found: $NextSignature" }

    $nextLineStart = $Text.LastIndexOf("`n", $next)
    if ($nextLineStart -lt 0) { $nextLineStart = $next } else { $nextLineStart++ }

    return $Text.Substring(0, $lineStart) + $Replacement.TrimEnd("`r", "`n") + "`r`n`r`n" + $Text.Substring($nextLineStart)
}

# Extract/replace one C# method by balanced braces. The two methods patched below
# do not contain interpolated-string brace literals, so this is deterministic for
# the exact recovered/current source family used by this branch.
function Get-CSharpMethodRange([string]$Text, [string]$MethodSignature) {
    $sig = $Text.IndexOf($MethodSignature, [System.StringComparison]::Ordinal)
    if ($sig -lt 0) { throw "C# method signature not found: $MethodSignature" }
    $lineStart = $Text.LastIndexOf("`n", $sig)
    if ($lineStart -lt 0) { $lineStart = 0 } else { $lineStart++ }
    $body = $Text.IndexOf('{', $sig)
    if ($body -lt 0) { throw "C# method body not found: $MethodSignature" }
    $depth = 0
    $finish = -1
    for ($i = $body; $i -lt $Text.Length; $i++) {
        $ch = $Text[$i]
        if ($ch -eq '{') { $depth++ }
        elseif ($ch -eq '}') {
            $depth--
            if ($depth -eq 0) { $finish = $i + 1; break }
        }
    }
    if ($finish -lt 0) { throw "C# method end not found: $MethodSignature" }
    return [pscustomobject]@{
        Start = $lineStart
        End = $finish
        Text = $Text.Substring($lineStart, $finish - $lineStart)
    }
}

function Replace-CSharpMethod([string]$Text, [string]$MethodSignature, [string]$Replacement) {
    $range = Get-CSharpMethodRange $Text $MethodSignature
    return $Text.Substring(0, $range.Start) + $Replacement.TrimEnd("`r", "`n") + "`r`n" + $Text.Substring($range.End)
}

# 1. Reproduce the recovered 2.10.3.16 Extended Final packaging model:
#    all seven shader binaries are manifest resources named Thetis.<filename>.
$projectText = [System.IO.File]::ReadAllText($csproj)
foreach ($shader in $shaders) {
    $escaped = [regex]::Escape($shader)
    $contentPattern = '<Content Include="' + $escaped + '">\s*<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>\s*</Content>'
    $embedded = '<EmbeddedResource Include="' + $shader + '"><LogicalName>Thetis.' + $shader + '</LogicalName></EmbeddedResource>'

    if ($projectText -match $contentPattern) {
        $projectText = [regex]::Replace($projectText, $contentPattern, $embedded, 1)
    }
    elseif ($projectText -notmatch ('<EmbeddedResource Include="' + $escaped + '"')) {
        throw "Shader project item not found in expected Content or EmbeddedResource form: $shader"
    }
}
Write-Utf8NoBom $csproj $projectText

# 2. FFT loader: embedded resource first, loose file only as fallback.
$pipelineText = [System.IO.File]::ReadAllText($pipeline)
if ($pipelineText -notmatch 'GetManifestResourceStream\(resourceName\)') {
    $replacement = @'
	private static byte[] LoadBytecode(string filename)
	{
		string resourceName = "Thetis." + filename;
		try
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				if (stream != null)
				{
					using (MemoryStream memoryStream = new MemoryStream())
					{
						stream.CopyTo(memoryStream);
						byte[] bytes = memoryStream.ToArray();
						if (bytes.Length > 0)
						{
							LogGPU("Loaded embedded shader: " + resourceName);
							return bytes;
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			LogGPU("Embedded shader load failed (" + resourceName + "): " + ex.Message);
		}

		try
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename);
			if (!File.Exists(path))
			{
				LogGPU("Shader not found as embedded resource or loose file: " + resourceName + " | " + path);
				return null;
			}
			LogGPU("Embedded shader missing; using loose-file fallback: " + path);
			return File.ReadAllBytes(path);
		}
		catch (Exception ex)
		{
			LogGPU("LoadBytecode(" + filename + ") fallback failed: " + ex.Message);
			return null;
		}
	}
'@
    $pipelineText = Replace-MethodBefore $pipelineText 'private static byte[] LoadBytecode(string filename)' 'private static int Log2' $replacement
    Write-Utf8NoBom $pipeline $pipelineText
}

# 3. Waterfall row renderer loader: same resource-first policy.
$rendererText = [System.IO.File]::ReadAllText($renderer)
if ($rendererText -notmatch 'Thetis\.waterfall_row_cs\.bin' -or $rendererText -notmatch 'GetManifestResourceStream\(resourceName\)') {
    $replacement = @'
	private static byte[] LoadShaderBytecode()
	{
		const string filename = "waterfall_row_cs.bin";
		const string resourceName = "Thetis.waterfall_row_cs.bin";
		try
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				if (stream != null)
				{
					using (MemoryStream memoryStream = new MemoryStream())
					{
						stream.CopyTo(memoryStream);
						byte[] bytes = memoryStream.ToArray();
						if (bytes.Length > 0)
						{
							LogGPU("Loaded embedded shader: " + resourceName);
							return bytes;
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			LogGPU("Embedded shader load failed (" + resourceName + "): " + ex.Message);
		}

		try
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename);
			if (!File.Exists(path))
			{
				LogGPU("Shader not found as embedded resource or loose file: " + resourceName + " | " + path);
				return null;
			}
			LogGPU("Embedded shader missing; using loose-file fallback: " + path);
			return File.ReadAllBytes(path);
		}
		catch (Exception ex)
		{
			LogGPU("LoadShaderBytecode fallback failed: " + ex.Message);
			return null;
		}
	}
'@
    $rendererText = Replace-MethodBefore $rendererText 'private static byte[] LoadShaderBytecode()' 'private static void LogGPU' $replacement
    Write-Utf8NoBom $renderer $rendererText
}

# 4. Archive-parity GPU palette upload.
# WaterfallPalette.Sample() intentionally stays 0..255 for the CPU renderer.
# The recovered 2.10.3.16 GPU path normalises the palette to 0..1 at upload.
$displayGpuText = [System.IO.File]::ReadAllText($displayGpu)
$oldPalette = @'
                _gpuPaletteUpload[n] = r;
                _gpuPaletteUpload[n + 1] = g;
                _gpuPaletteUpload[n + 2] = b;
'@
$newPalette = @'
                _gpuPaletteUpload[n] = r / 255f;
                _gpuPaletteUpload[n + 1] = g / 255f;
                _gpuPaletteUpload[n + 2] = b / 255f;
'@
if ($displayGpuText.Contains($oldPalette)) {
    $displayGpuText = $displayGpuText.Replace($oldPalette, $newPalette)
}
elseif ($displayGpuText -notmatch '_gpuPaletteUpload\[n\]\s*=\s*r\s*/\s*255f;') {
    throw 'GPU palette upload block not found in expected current/archive-parity form'
}
Write-Utf8NoBom $displayGpu $displayGpuText

# 5. Archive-parity colour-depth rebuild.
# The recovered 2.10.3.16 first resizes the EXISTING swap chain to the new
# format. Only if that fails does it destroy DirectX and create a new swap chain.
# Reuse the already proven SQ4KOU resizeDX2D implementation and make a format
# overload from it, changing only the ResizeBuffers format argument.
$displayText = [System.IO.File]::ReadAllText($display)
$formatResizeSignature = 'private static bool resizeDX2DForFormat(Format newFormat, out string error)'
if ($displayText.IndexOf($formatResizeSignature, [System.StringComparison]::Ordinal) -lt 0) {
    $baseResize = Get-CSharpMethodRange $displayText 'private static bool resizeDX2D(out string error)'
    $formatResize = $baseResize.Text
    $formatResize = $formatResize.Replace('private static bool resizeDX2D(out string error)', $formatResizeSignature)
    if ($formatResize.IndexOf('_swapChain.Description.ModeDescription.Format', [System.StringComparison]::Ordinal) -lt 0) {
        throw 'Existing resizeDX2D does not contain expected swap-chain format expression'
    }
    $formatResize = $formatResize.Replace('_swapChain.Description.ModeDescription.Format', 'newFormat')
    $formatResize = $formatResize.Replace('newFormat, SwapChainFlags.None', 'newFormat, _swapChainFlags')
    $displayText = $displayText.Substring(0, $baseResize.Start) + $formatResize.TrimEnd("`r", "`n") + "`r`n`r`n" + $displayText.Substring($baseResize.Start)
}

$rebuildReplacement = @'
        public static bool RebuildForColorDepth()
        {
            if (displayTarget == null) return false;

            var requestedDepth = WaterfallEnhancer.Depth;
            WaterfallPixelWriter.UpdateFormat();

            // 2.10.3.16 Extended Final behaviour: keep the existing DXGI factory,
            // device and swap chain ownership whenever possible. resizeDX2D already
            // serialises with the render thread via _objDX2Lock, clears/flushes the
            // device context and rebuilds the D2D target after ResizeBuffers.
            if (_bDX2Setup && _swapChain1 != null && !_swapChain1.IsDisposed)
            {
                if (resizeDX2DForFormat(WaterfallPixelWriter.DxgiFormat, out string inPlaceError))
                {
                    ResetWaterfallBmp();
                    ResetWaterfallBmp2();
                    LogTool.AddLogEntry("Color depth changed in-place to " + WaterfallPixelWriter.DxgiFormat, "DX2D");
                    return true;
                }

                LogTool.AddLogEntry("In-place format resize failed: " + inPlaceError + " - falling back to full DX rebuild.", "DX2D");
            }

            // Recovery path only. This preserves the previous safe 8-bit fallback,
            // but avoids Factory.CreateSwapChain during a normal 8/16-bit change.
            ShutdownDX2D();
            WaterfallPixelWriter.UpdateFormat();

            try
            {
                initDX2D(DriverType.Hardware, _display_adaptor);
            }
            catch (Exception ex)
            {
                LogTool.AddLogEntry("RebuildForColorDepth init failed: " + ex.Message, "DX2D");
            }

            if (!_bDX2Setup)
            {
                if (requestedDepth != WaterfallEnhancer.ColorDepth.Bit8)
                {
                    WaterfallEnhancer.SetColorDepth(WaterfallEnhancer.ColorDepth.Bit8);
                    WaterfallPixelWriter.UpdateFormat();
                    try
                    {
                        initDX2D(DriverType.Hardware, _display_adaptor);
                    }
                    catch (Exception ex)
                    {
                        LogTool.AddLogEntry("RebuildForColorDepth 8-bit fallback failed: " + ex.Message, "DX2D");
                    }
                }
                return _bDX2Setup;
            }

            ResetWaterfallBmp();
            ResetWaterfallBmp2();
            return true;
        }
'@
$displayText = Replace-CSharpMethod $displayText 'public static bool RebuildForColorDepth()' $rebuildReplacement
Write-Utf8NoBom $display $displayText

# 6. Static gates before compilation.
$projectText = [System.IO.File]::ReadAllText($csproj)
foreach ($shader in $shaders) {
    $logical = 'Thetis.' + $shader
    if ($projectText -notmatch [regex]::Escape($logical)) { throw "EmbeddedResource LogicalName missing after patch: $logical" }
}
$pipelineText = [System.IO.File]::ReadAllText($pipeline)
$rendererText = [System.IO.File]::ReadAllText($renderer)
if ($pipelineText -notmatch 'string resourceName = "Thetis\." \+ filename;' -or $pipelineText -notmatch 'GetManifestResourceStream\(resourceName\)') {
    throw 'FFT resource-first loader verification failed'
}
if ($rendererText -notmatch 'const string resourceName = "Thetis\.waterfall_row_cs\.bin";' -or $rendererText -notmatch 'GetManifestResourceStream\(resourceName\)') {
    throw 'Row resource-first loader verification failed'
}

$displayGpuText = [System.IO.File]::ReadAllText($displayGpu)
if ($displayGpuText -notmatch '_gpuPaletteUpload\[n\]\s*=\s*r\s*/\s*255f;' -or
    $displayGpuText -notmatch '_gpuPaletteUpload\[n \+ 1\]\s*=\s*g\s*/\s*255f;' -or
    $displayGpuText -notmatch '_gpuPaletteUpload\[n \+ 2\]\s*=\s*b\s*/\s*255f;') {
    throw 'Archive-parity GPU palette normalisation verification failed'
}

$displayText = [System.IO.File]::ReadAllText($display)
$rebuildRange = Get-CSharpMethodRange $displayText 'public static bool RebuildForColorDepth()'
if ($rebuildRange.Text.IndexOf('resizeDX2DForFormat(WaterfallPixelWriter.DxgiFormat', [System.StringComparison]::Ordinal) -lt 0) {
    throw 'Archive-parity in-place colour-depth resize call missing'
}
if ($rebuildRange.Text.IndexOf('ShutdownDX2D();', [System.StringComparison]::Ordinal) -lt 0 -or
    $rebuildRange.Text.IndexOf('resizeDX2DForFormat(WaterfallPixelWriter.DxgiFormat', [System.StringComparison]::Ordinal) -gt $rebuildRange.Text.IndexOf('ShutdownDX2D();', [System.StringComparison]::Ordinal)) {
    throw 'Colour-depth rebuild does not attempt in-place ResizeBuffers before full DX shutdown'
}
$formatResizeRange = Get-CSharpMethodRange $displayText $formatResizeSignature
if ($formatResizeRange.Text.IndexOf('ResizeBuffers', [System.StringComparison]::Ordinal) -lt 0 -or
    $formatResizeRange.Text.IndexOf('newFormat', [System.StringComparison]::Ordinal) -lt 0) {
    throw 'Format-aware ResizeBuffers helper verification failed'
}

Write-Host 'GPU archive-parity patch OK.'
Write-Host '  Palette upload: 0..255 -> 0..1 (recovered 2.10.3.16 model)'
Write-Host '  Depth switch: in-place ResizeBuffers first; full DX rebuild only as fallback'
Write-Host 'Embedded resources:'
$shaders | ForEach-Object { Write-Host ('  Thetis.' + $_) }
