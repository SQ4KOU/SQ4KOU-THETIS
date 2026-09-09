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

# 1. Reproduce the 2.10.3.16 Extended Final packaging model exactly:
#    all seven shader binaries are manifest resources named Thetis.<filename>.
$projectText = [System.IO.File]::ReadAllText($csproj)
foreach ($shader in $shaders) {
    $escaped = [regex]::Escape($shader)
    $contentPattern = '<Content Include="' + $escaped + '">\s*<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>\s*</Content>'
    $embedded = '<EmbeddedResource Include="' + $shader + '"><LogicalName>Thetis.' + $shader + '</LogicalName></EmbeddedResource>'

    if ($projectText -match $contentPattern) {
        $projectText = [regex]::Replace($projectText, $contentPattern, $embedded, 1)
    }
    elseif ($projectText -notmatch ('<EmbeddedResource Include="' + $escaped + '"[^>]*>.*?<LogicalName>Thetis\.' + $escaped + '</LogicalName>.*?</EmbeddedResource>|<EmbeddedResource Include="' + $escaped + '"\s+LogicalName="Thetis\.' + $escaped + '"\s*/>')) {
        throw "Shader project item not found in expected Content or EmbeddedResource form: $shader"
    }
}
Write-Utf8NoBom $csproj $projectText

# 2. FFT loader: embedded resource first, loose file only as fallback.
$pipelineText = [System.IO.File]::ReadAllText($pipeline)
if ($pipelineText -notmatch 'GetManifestResourceStream\("Thetis\." \+ filename\)') {
    $pattern = '(?s)\tprivate static byte\[\] LoadBytecode\(string filename\)\s*\{.*?\n\t\}\n\n\tprivate static int Log2'
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

	private static int Log2
'@
    $newPipeline = [regex]::Replace($pipelineText, $pattern, $replacement, 1)
    if ($newPipeline -eq $pipelineText) { throw 'GPUWaterfallPipeline.LoadBytecode patch did not match' }
    Write-Utf8NoBom $pipeline $newPipeline
}

# 3. Waterfall row renderer loader: same resource-first policy.
$rendererText = [System.IO.File]::ReadAllText($renderer)
if ($rendererText -notmatch 'GetManifestResourceStream\("Thetis\.waterfall_row_cs\.bin"\)') {
    $pattern = '(?s)\tprivate static byte\[\] LoadShaderBytecode\(\)\s*\{.*?\n\t\}\n\n\tprivate static void LogGPU'
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

	private static void LogGPU
'@
    $newRenderer = [regex]::Replace($rendererText, $pattern, $replacement, 1)
    if ($newRenderer -eq $rendererText) { throw 'WaterfallGPURenderer.LoadShaderBytecode patch did not match' }
    Write-Utf8NoBom $renderer $newRenderer
}

# 4. Static gate before compilation.
$projectText = [System.IO.File]::ReadAllText($csproj)
foreach ($shader in $shaders) {
    $logical = 'Thetis.' + $shader
    if ($projectText -notmatch [regex]::Escape($logical)) { throw "EmbeddedResource LogicalName missing after patch: $logical" }
}
$pipelineText = [System.IO.File]::ReadAllText($pipeline)
$rendererText = [System.IO.File]::ReadAllText($renderer)
if ($pipelineText -notmatch 'GetManifestResourceStream\("Thetis\." \+ filename\)') { throw 'FFT resource-first loader verification failed' }
if ($rendererText -notmatch 'GetManifestResourceStream\("Thetis\.waterfall_row_cs\.bin"\)') { throw 'Row resource-first loader verification failed' }

Write-Host 'GPU shader embedding patch OK.'
Write-Host 'Embedded resources:'
$shaders | ForEach-Object { Write-Host ('  Thetis.' + $_) }
