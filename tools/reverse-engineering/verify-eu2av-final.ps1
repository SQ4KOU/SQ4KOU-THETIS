param(
    [string]$Root = "reverse-engineering/EU2AV-Thetis-2.10.3.16-Final",
    [string]$SourceLock = "tools/reverse-engineering/EU2AV_FINAL_SOURCE_LOCK.json"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Fail([string]$Message) { throw "STRICT VERIFY FAILED: $Message" }
function Safe-Name([string]$Name) { return ($Name -replace '[^A-Za-z0-9._-]', '_') }
function Parse-Kv([string]$Path) {
    $h = @{}
    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line -match '^([^=]+)=(.*)$') { $h[$matches[1].Trim()] = $matches[2].Trim() }
    }
    return $h
}
function Require-File([string]$Path, [long]$MinBytes = 1) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { Fail "missing file: $Path" }
    if ((Get-Item -LiteralPath $Path).Length -lt $MinBytes) { Fail "file too small: $Path" }
}

Require-File $SourceLock 100
Require-File "$Root/metadata/MANIFEST.csv" 100
Require-File "$Root/metadata/PROVENANCE.txt" 100
Require-File "$Root/metadata/NATIVE_DECOMPILE_STATUS.csv" 20
Require-File "$Root/README.md" 100

$lock = Get-Content -LiteralPath $SourceLock -Raw | ConvertFrom-Json
foreach ($field in @('source_url','package_sha256','package_size','package_magic','msi_product_version','thetis_file_version','thetis_product_version','thetis_sha256')) {
    if (-not $lock.PSObject.Properties.Name.Contains($field) -or [string]::IsNullOrWhiteSpace([string]$lock.$field)) {
        Fail "source lock field missing: $field"
    }
}
if ([string]$lock.package_sha256 -notmatch '^[0-9a-fA-F]{64}$') { Fail 'invalid locked package SHA-256' }
if ([string]$lock.thetis_sha256 -notmatch '^[0-9a-fA-F]{64}$') { Fail 'invalid locked Thetis.exe SHA-256' }
if ([string]$lock.msi_product_version -ne '2.10.3.16') { Fail "unexpected MSI version: $($lock.msi_product_version)" }
if ([string]$lock.thetis_file_version -notmatch '^2\.10\.3\.16(?:\.|$)') { Fail "unexpected Thetis file version: $($lock.thetis_file_version)" }
if ([string]$lock.thetis_product_version -notmatch '^2\.10\.3\.16(?:\.|$)') { Fail "unexpected Thetis product version: $($lock.thetis_product_version)" }

$prov = Parse-Kv "$Root/metadata/PROVENANCE.txt"
foreach ($key in @('source_url','package_size','package_sha256','package_magic','thetis_exe_sha256','msi_product_version','thetis_file_version','thetis_product_version','ilspy','ghidra')) {
    if (-not $prov.ContainsKey($key) -or [string]::IsNullOrWhiteSpace($prov[$key])) { Fail "provenance key missing: $key" }
}
if ($prov.package_sha256.ToLowerInvariant() -ne ([string]$lock.package_sha256).ToLowerInvariant()) { Fail 'package SHA-256 differs from source lock' }
if ([long]$prov.package_size -ne [long]$lock.package_size) { Fail 'package size differs from source lock' }
if ($prov.package_magic -ne [string]$lock.package_magic) { Fail 'package magic differs from source lock' }
if ($prov.source_url -ne [string]$lock.source_url) { Fail 'source URL differs from source lock' }
if ($prov.thetis_exe_sha256.ToLowerInvariant() -ne ([string]$lock.thetis_sha256).ToLowerInvariant()) { Fail 'Thetis.exe SHA-256 differs from source lock' }
if ($prov.msi_product_version -ne '2.10.3.16') { Fail 'decompiled MSI is not 2.10.3.16' }
if ($prov.thetis_file_version -notmatch '^2\.10\.3\.16(?:\.|$)') { Fail 'decompiled Thetis.exe file version mismatch' }
if ($prov.thetis_product_version -notmatch '^2\.10\.3\.16(?:\.|$)') { Fail 'decompiled Thetis.exe product version mismatch' }

$manifest = @(Import-Csv -LiteralPath "$Root/metadata/MANIFEST.csv")
if ($manifest.Count -lt 1) { Fail 'empty manifest' }
$dupPaths = @($manifest | Group-Object path | Where-Object Count -ne 1)
if ($dupPaths.Count -ne 0) { Fail "duplicate manifest paths: $($dupPaths.Name -join ', ')" }
foreach ($row in $manifest) {
    if ([string]::IsNullOrWhiteSpace($row.path) -or [long]$row.size -lt 0 -or $row.sha256 -notmatch '^[0-9a-fA-F]{64}$') {
        Fail "invalid manifest row: $($row.path)"
    }
}

$managed = @($manifest | Where-Object kind -eq 'managed-pe')
if ($managed.Count -lt 1) { Fail 'no managed assemblies discovered' }
foreach ($row in $managed) {
    $safe = Safe-Name ($row.path -replace '/','__')
    $dir = Join-Path "$Root/managed" $safe
    if (-not (Test-Path -LiteralPath $dir -PathType Container)) { Fail "managed output missing: $($row.path)" }
    $cs = @(Get-ChildItem -LiteralPath $dir -Recurse -File -Filter *.cs)
    if ($cs.Count -lt 1) { Fail "no C# recovered for: $($row.path)" }
    $il = Join-Path $dir ($safe + '.il.txt')
    Require-File $il 1000
    if (Select-String -LiteralPath $il -Pattern 'IL dump unavailable' -Quiet) { Fail "IL recovery failed: $($row.path)" }
    $log = Join-Path "$Root/logs" ("ilspy_" + $safe + '.log')
    Require-File $log 1
}
$thetisRow = $managed | Where-Object { [IO.Path]::GetFileName($_.path) -ieq 'Thetis.exe' } | Select-Object -First 1
if (-not $thetisRow) { Fail 'Thetis.exe missing from managed manifest' }
if ($thetisRow.sha256.ToLowerInvariant() -ne ([string]$lock.thetis_sha256).ToLowerInvariant()) { Fail 'manifest Thetis.exe hash differs from source lock' }
$thetisSafe = Safe-Name ($thetisRow.path -replace '/','__')
if (@(Get-ChildItem (Join-Path "$Root/managed" $thetisSafe) -Recurse -File -Filter *.cs).Count -lt 100) {
    Fail 'Thetis.exe recovery produced fewer than 100 C# files'
}

$native = @($manifest | Where-Object kind -eq 'native-pe')
$status = @(Import-Csv -LiteralPath "$Root/metadata/NATIVE_DECOMPILE_STATUS.csv")
$documentedFallbacks = [System.Collections.Generic.List[object]]::new()
if ($native.Count -ne $status.Count) { Fail "native coverage mismatch: manifest=$($native.Count), status=$($status.Count)" }
foreach ($row in $native) {
    $st = @($status | Where-Object path -eq $row.path)
    if ($st.Count -ne 1) { Fail "native status missing/duplicate: $($row.path)" }
    if ([int]$st[0].exit_code -ne 0 -or [string]$st[0].stats_present -notmatch '^(?i:true)$') { Fail "Ghidra failed: $($row.path)" }
    $safe = Safe-Name ($row.path -replace '/','__')
    $dir = Join-Path "$Root/native" $safe
    $statsPath = Join-Path $dir 'decompile-stats.txt'
    $indexPath = Join-Path $dir 'functions.csv'
    Require-File $statsPath 20
    Require-File $indexPath 20
    $stats = Parse-Kv $statsPath
    foreach ($k in @('functions_total','functions_external_skipped','functions_decompiled_ok','functions_failed','chunks','cancelled')) {
        if (-not $stats.ContainsKey($k)) { Fail "native stat missing ($k): $($row.path)" }
    }
    if ($stats.cancelled -ne 'false') { Fail "Ghidra cancelled: $($row.path)" }
    $failedCount = [int]$stats.functions_failed
    if ([int]$stats.functions_decompiled_ok -lt 1) { Fail "no native functions recovered: $($row.path)" }
    if ([int]$stats.functions_total -ne ([int]$stats.functions_external_skipped + [int]$stats.functions_decompiled_ok + [int]$stats.functions_failed)) {
        Fail "native function accounting mismatch: $($row.path)"
    }
    $functions = @(Import-Csv -LiteralPath $indexPath)
    if ($functions.Count -ne ([int]$stats.functions_decompiled_ok + $failedCount)) { Fail "native function index mismatch: $($row.path)" }
    $fallbacks = @($functions | Where-Object status -eq 'ASSEMBLY_FALLBACK')
    $unexpected = @($functions | Where-Object { $_.status -notmatch '^OK(?:_RETRY_[A-Z]+)?
}
$exceptionsPath = "$Root/metadata/NATIVE_DECOMPILATION_EXCEPTIONS.csv"
if ($documentedFallbacks.Count -gt 0) {
    $documentedFallbacks | Export-Csv -LiteralPath $exceptionsPath -NoTypeInformation -Encoding utf8
} else {
    'module,path,entry,name,status,reason,evidence,listing_chunk' | Set-Content -LiteralPath $exceptionsPath -Encoding utf8
}
Require-File $exceptionsPath 50

foreach ($name in @('ChannelMaster.dll','wdsp.dll')) {
    if (-not ($native | Where-Object { [IO.Path]::GetFileName($_.path) -ieq $name })) { Fail "required native module missing: $name" }
}

$shaderRows = @($manifest | Where-Object { [IO.Path]::GetFileName($_.path) -match '^waterfall_.*\.(bin|cso)$' })
$requiredShaders = @(
    'waterfall_fft_bitreverse_cs.bin','waterfall_fft_magnitude_cs.bin',
    'waterfall_fft_stage_ab_cs.bin','waterfall_fft_stage_ba_cs.bin',
    'waterfall_postproc.bin','waterfall_resolve_cs.bin','waterfall_row_cs.bin'
)
foreach ($name in $requiredShaders) {
    if (-not ($shaderRows | Where-Object { [IO.Path]::GetFileName($_.path) -ieq $name })) { Fail "required shader missing: $name" }
}
foreach ($row in $shaderRows) {
    $safe = Safe-Name ($row.path -replace '/','__')
    $asm = Join-Path "$Root/dxbc" ($safe + '.asm.txt')
    Require-File $asm 100
    if (-not (Select-String -LiteralPath $asm -Pattern 'cs_[456]_[0-9]|Compute Shader|DXBC' -Quiet)) {
        Fail "shader disassembly lacks a recognized DXBC compute profile: $($row.path)"
    }
}
$asmFiles = @(Get-ChildItem -LiteralPath "$Root/dxbc" -File -Filter *.asm.txt)
if ($asmFiles.Count -ne $shaderRows.Count) { Fail "shader coverage mismatch: payload=$($shaderRows.Count), output=$($asmFiles.Count)" }

$summary = [ordered]@{
    result = 'PASS'
    verified_utc = [DateTime]::UtcNow.ToString('o')
    package_sha256 = $prov.package_sha256
    thetis_sha256 = $prov.thetis_exe_sha256
    managed_assemblies = $managed.Count
    native_modules = $native.Count
    native_pseudocode_failures = $documentedFallbacks.Count
    native_documented_assembly_fallbacks = $documentedFallbacks.Count
    native_unaccounted_failures = 0
    waterfall_shaders = $shaderRows.Count
}
$summary | ConvertTo-Json | Set-Content -LiteralPath "$Root/metadata/STRICT_VERIFICATION.json" -Encoding utf8
Write-Host "STRICT VERIFY PASS: managed=$($managed.Count), native=$($native.Count), shaders=$($shaderRows.Count), documented assembly fallbacks=$($documentedFallbacks.Count), unaccounted failures=0"
 -and $_.status -ne 'ASSEMBLY_FALLBACK' })
    if ($unexpected.Count -ne 0) { Fail "unaccounted native function status: $($row.path)" }
    if ($fallbacks.Count -ne $failedCount) { Fail "native fallback accounting mismatch: $($row.path)" }

    $moduleName = [IO.Path]::GetFileName($row.path)
    if ($fallbacks.Count -gt 0 -and $moduleName -ine 'libSkiaSharp.dll') {
        Fail "assembly fallback forbidden for relevant native module: $($row.path)"
    }
    foreach ($fallback in $fallbacks) {
        $chunkPath = Join-Path $dir $fallback.chunk
        Require-File $chunkPath 100
        $chunkText = Get-Content -LiteralPath $chunkPath -Raw
        if (-not $chunkText.Contains("ENTRY: $($fallback.entry)")) {
            Fail "assembly fallback entry missing from chunk: $($row.path) $($fallback.entry)"
        }
        if ($chunkText -notmatch 'ASSEMBLY FALLBACK AFTER PSEUDOCODE FAILURE' -or
            $chunkText -notmatch 'ASSEMBLY INSTRUCTIONS: [1-9][0-9]*') {
            Fail "assembly fallback is empty or unverifiable: $($row.path) $($fallback.entry)"
        }
        [void]$documentedFallbacks.Add([pscustomobject]@{
            module = $moduleName
            path = $row.path
            entry = $fallback.entry
            name = $fallback.name
            status = 'ASSEMBLY_FALLBACK'
            reason = 'Ghidra pseudocode failed after decompile, normalize and register modes'
            evidence = 'Third-party Skia native runtime; not ChannelMaster.dll, wdsp.dll, a DXBC shader, or the managed GPU-waterfall control path'
            listing_chunk = $fallback.chunk
        })
    }
    foreach ($chunk in @($functions.chunk | Sort-Object -Unique)) { Require-File (Join-Path $dir $chunk) 100 }
}
foreach ($name in @('ChannelMaster.dll','wdsp.dll')) {
    if (-not ($native | Where-Object { [IO.Path]::GetFileName($_.path) -ieq $name })) { Fail "required native module missing: $name" }
}

$shaderRows = @($manifest | Where-Object { [IO.Path]::GetFileName($_.path) -match '^waterfall_.*\.(bin|cso)$' })
$requiredShaders = @(
    'waterfall_fft_bitreverse_cs.bin','waterfall_fft_magnitude_cs.bin',
    'waterfall_fft_stage_ab_cs.bin','waterfall_fft_stage_ba_cs.bin',
    'waterfall_postproc.bin','waterfall_resolve_cs.bin','waterfall_row_cs.bin'
)
foreach ($name in $requiredShaders) {
    if (-not ($shaderRows | Where-Object { [IO.Path]::GetFileName($_.path) -ieq $name })) { Fail "required shader missing: $name" }
}
foreach ($row in $shaderRows) {
    $safe = Safe-Name ($row.path -replace '/','__')
    $asm = Join-Path "$Root/dxbc" ($safe + '.asm.txt')
    Require-File $asm 100
    if (-not (Select-String -LiteralPath $asm -Pattern 'cs_[456]_[0-9]|Compute Shader|DXBC' -Quiet)) {
        Fail "shader disassembly lacks a recognized DXBC compute profile: $($row.path)"
    }
}
$asmFiles = @(Get-ChildItem -LiteralPath "$Root/dxbc" -File -Filter *.asm.txt)
if ($asmFiles.Count -ne $shaderRows.Count) { Fail "shader coverage mismatch: payload=$($shaderRows.Count), output=$($asmFiles.Count)" }

$summary = [ordered]@{
    result = 'PASS'
    verified_utc = [DateTime]::UtcNow.ToString('o')
    package_sha256 = $prov.package_sha256
    thetis_sha256 = $prov.thetis_exe_sha256
    managed_assemblies = $managed.Count
    native_modules = $native.Count
    native_functions_failed = 0
    waterfall_shaders = $shaderRows.Count
}
$summary | ConvertTo-Json | Set-Content -LiteralPath "$Root/metadata/STRICT_VERIFICATION.json" -Encoding utf8
Write-Host "STRICT VERIFY PASS: managed=$($managed.Count), native=$($native.Count), shaders=$($shaderRows.Count), native failures=0"
