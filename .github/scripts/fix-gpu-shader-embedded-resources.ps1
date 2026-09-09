$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$consoleDir = Join-Path $repo 'Project Files\Source\Console'
$csproj = Join-Path $consoleDir 'Thetis.csproj'
$pipeline = Join-Path $consoleDir 'GPUWaterfallPipeline.cs'
$renderer = Join-Path $consoleDir 'WaterfallGPURenderer.cs'

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

# 4. Static gate before compilation.
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

Write-Host 'GPU shader embedding patch OK.'
Write-Host 'Embedded resources:'
$shaders | ForEach-Object { Write-Host ('  Thetis.' + $_) }
