param(
    [Parameter(Mandatory=$true)][string]$ThetisExe,
    [Parameter(Mandatory=$true)][string]$WorkDir
)
$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Force -Path $WorkDir | Out-Null

function Add-KV([System.Data.DataTable]$t, [string]$k, [string]$v) {
    $r = $t.NewRow(); $r['Key'] = $k; $r['Value'] = $v; $t.Rows.Add($r)
}

function New-LegacyFixture([string]$path) {
    $ds = New-Object System.Data.DataSet 'Data'

    $tx = New-Object System.Data.DataTable 'TXProfile'
    [void]$tx.Columns.Add('Name',[string])
    [void]$tx.Columns.Add('DXOn',[bool])
    $c = $tx.Columns.Add('DXLevel',[int]); $c.AllowDBNull = $true
    [void]$tx.Columns.Add('MicGain',[int])
    [void]$tx.Columns.Add('LegacyOnlySetting',[string])
    $r = $tx.NewRow()
    $r['Name']='LEGACY CUSTOM'; $r['DXOn']=$true; $r['DXLevel']=[DBNull]::Value
    $r['MicGain']=73; $r['LegacyOnlySetting']='KEEP_PROFILE_EXTRA'
    $tx.Rows.Add($r); $ds.Tables.Add($tx)

    $def = New-Object System.Data.DataTable 'TXProfileDef'
    [void]$def.Columns.Add('Name',[string])
    [void]$def.Columns.Add('DXOn',[bool])
    $c = $def.Columns.Add('DXLevel',[int]); $c.AllowDBNull = $true
    [void]$def.Columns.Add('MicGain',[int])
    [void]$def.Columns.Add('LegacyOnlySetting',[string])
    $r = $def.NewRow(); $r['Name']='Default'; $r['DXOn']=$false; $r['DXLevel']=3; $r['MicGain']=10; $r['LegacyOnlySetting']='KEEP_DEF_EXTRA'
    $def.Rows.Add($r); $ds.Tables.Add($def)

    $state = New-Object System.Data.DataTable 'State'
    [void]$state.Columns.Add('Key',[string]); [void]$state.Columns.Add('Value',[string])
    Add-KV $state 'VersionNumber' '2.10.3'
    Add-KV $state 'Version' 'legacy-test'
    Add-KV $state 'DatabaseSchemaVersion' '0'
    Add-KV $state 'LegacyStateKey' 'KEEP_STATE_VALUE'
    $ds.Tables.Add($state)

    $options = New-Object System.Data.DataTable 'Options'
    [void]$options.Columns.Add('Key',[string]); [void]$options.Columns.Add('Value',[string])
    Add-KV $options 'LegacyOptionKey' 'KEEP_OPTION_VALUE'
    $ds.Tables.Add($options)

    $cfc = New-Object System.Data.DataTable 'CFCConfig'
    [void]$cfc.Columns.Add('Key',[string]); [void]$cfc.Columns.Add('Value',[string])
    Add-KV $cfc 'LegacyCFCKey' 'KEEP_CFC_VALUE'
    $ds.Tables.Add($cfc)

    $wide = New-Object System.Data.DataTable 'WideBand'
    [void]$wide.Columns.Add('Key',[string]); [void]$wide.Columns.Add('Value',[string])
    Add-KV $wide 'LegacyWideBandKey' 'KEEP_WIDEBAND_VALUE'
    $ds.Tables.Add($wide)

    $groups = New-Object System.Data.DataTable 'GroupList'
    [void]$groups.Columns.Add('GroupID',[int]); [void]$groups.Columns.Add('GroupName',[string]); [void]$groups.Columns.Add('LegacyGroupField',[string])
    $r=$groups.NewRow(); $r['GroupID']=77; $r['GroupName']='LEGACY-GROUP'; $r['LegacyGroupField']='KEEP_GROUP_EXTRA'; $groups.Rows.Add($r)
    $ds.Tables.Add($groups)

    $mem = New-Object System.Data.DataTable 'Memory'
    [void]$mem.Columns.Add('GroupID',[int]); [void]$mem.Columns.Add('Freq',[double]); [void]$mem.Columns.Add('Comments',[string]); [void]$mem.Columns.Add('LegacyMemoryField',[string])
    $r=$mem.NewRow(); $r['GroupID']=77; $r['Freq']=7.123456; $r['Comments']='LEGACY MEMORY'; $r['LegacyMemoryField']='KEEP_MEMORY_EXTRA'; $mem.Rows.Add($r)
    $ds.Tables.Add($mem)

    $legacy = New-Object System.Data.DataTable 'LegacyOnlyTable'
    [void]$legacy.Columns.Add('Key',[string]); [void]$legacy.Columns.Add('Value',[string]); [void]$legacy.Columns.Add('Extra',[string])
    $r=$legacy.NewRow(); $r['Key']='ONLY'; $r['Value']='KEEP_TABLE'; $r['Extra']='KEEP_EXTRA'; $legacy.Rows.Add($r)
    $ds.Tables.Add($legacy)

    $ds.WriteXml($path, [System.Data.XmlWriteMode]::WriteSchema)
}

function Read-DS([string]$path) {
    $d = New-Object System.Data.DataSet
    [void]$d.ReadXml($path)
    return $d
}

function Assert([bool]$condition, [string]$message) {
    if (-not $condition) { throw "ASSERT FAILED: $message" }
}

function Get-RowByKey([System.Data.DataTable]$t, [string]$key) {
    $escaped = $key.Replace("'", "''")
    $rows = $t.Select("Key = '$escaped'")
    if ($rows.Count -eq 0) { return $null }
    return $rows[0]
}

function Assert-LegacyContent([string]$path, [int]$expectedSchema) {
    $d = Read-DS $path
    Assert ($d.Tables.Contains('TXProfile')) 'TXProfile missing'
    $p = $d.Tables['TXProfile'].Select("Name = 'LEGACY CUSTOM'")
    Assert ($p.Count -eq 1) 'legacy custom TX profile missing'
    $p = $p[0]
    foreach ($col in @('DXLevel','Tune_Power','Tune_Meter_Type')) {
        Assert ($d.Tables['TXProfile'].Columns.Contains($col)) "current TX field $col missing"
        Assert (-not $p.IsNull($col)) "current TX field $col remained DBNull"
    }
    Assert ([int]$p['MicGain'] -eq 73) 'old MicGain overwritten'
    Assert ([string]$p['LegacyOnlySetting'] -eq 'KEEP_PROFILE_EXTRA') 'legacy-only TX column/value lost'

    Assert ($d.Tables.Contains('LegacyOnlyTable')) 'legacy-only table lost'
    Assert ([string]$d.Tables['LegacyOnlyTable'].Rows[0]['Extra'] -eq 'KEEP_EXTRA') 'legacy-only table value lost'

    $s = Get-RowByKey $d.Tables['State'] 'DatabaseSchemaVersion'
    Assert ($null -ne $s) 'schema version state key missing'
    Assert ([int]$s['Value'] -eq $expectedSchema) 'schema version not promoted'
    $s = Get-RowByKey $d.Tables['State'] 'LegacyStateKey'
    Assert ($null -ne $s -and [string]$s['Value'] -eq 'KEEP_STATE_VALUE') 'legacy State key/value lost'

    Assert ($d.Tables.Contains('Options')) 'Options missing'
    $o = Get-RowByKey $d.Tables['Options'] 'LegacyOptionKey'
    Assert ($null -ne $o -and [string]$o['Value'] -eq 'KEEP_OPTION_VALUE') 'legacy Option key/value lost'

    Assert ($d.Tables.Contains('CFCConfig')) 'CFCConfig missing'
    $o = Get-RowByKey $d.Tables['CFCConfig'] 'LegacyCFCKey'
    Assert ($null -ne $o -and [string]$o['Value'] -eq 'KEEP_CFC_VALUE') 'CFCConfig data lost'

    Assert ($d.Tables.Contains('WideBand')) 'WideBand missing'
    $o = Get-RowByKey $d.Tables['WideBand'] 'LegacyWideBandKey'
    Assert ($null -ne $o -and [string]$o['Value'] -eq 'KEEP_WIDEBAND_VALUE') 'WideBand data lost'

    Assert ($d.Tables.Contains('GroupList')) 'GroupList missing'
    $g=$d.Tables['GroupList'].Select('GroupID = 77')
    Assert ($g.Count -eq 1 -and [string]$g[0]['LegacyGroupField'] -eq 'KEEP_GROUP_EXTRA') 'GroupList legacy row/extra lost'

    Assert ($d.Tables.Contains('Memory')) 'Memory missing'
    $foundMemory=$false
    foreach($m in $d.Tables['Memory'].Rows) {
        if ([string]$m['Comments'] -eq 'LEGACY MEMORY') {
            Assert ([string]$m['LegacyMemoryField'] -eq 'KEEP_MEMORY_EXTRA') 'Memory legacy extra lost'
            Assert ([math]::Abs([double]$m['Freq'] - 7.123456) -lt 0.0000001) 'Memory frequency changed'
            $foundMemory=$true
        }
    }
    Assert $foundMemory 'legacy Memory row lost'
}

$fixture = Join-Path $WorkDir 'legacy-original.xml'
$normalizeFile = Join-Path $WorkDir 'legacy-normalize.xml'
$freshFile = Join-Path $WorkDir 'fresh-current.xml'
$candidateFile = Join-Path $WorkDir 'legacy-import-candidate.xml'
New-LegacyFixture $fixture
Copy-Item $fixture $normalizeFile -Force

$asm = [System.Reflection.Assembly]::LoadFrom((Resolve-Path $ThetisExe))
$dbType = $asm.GetType('Thetis.DB', $true)
$flags = [System.Reflection.BindingFlags]'Public,NonPublic,Static'
$fileProp = $dbType.GetProperty('FileName', $flags)
$initMethod = $dbType.GetMethod('Init', $flags)
$compatMethod = $dbType.GetMethod('IsDatabaseCompatible', $flags)
$importMethod = $dbType.GetMethod('ImportAndMergeDatabase', $flags)
$preserveMethod = $dbType.GetMethod('ValidateDatabasePreservesSource', $flags)
$currentSchema = [int]$dbType.GetField('CurrentDatabaseSchemaVersion', $flags).GetRawConstantValue()
$writeOne = $dbType.GetMethods($flags) | Where-Object { $_.Name -eq 'WriteDB' -and $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType -eq [string] } | Select-Object -First 1
Assert ($null -ne $writeOne) 'WriteDB(string) reflection method missing'

$fileProp.SetValue($null, $normalizeFile, $null)
$ok = [bool]$initMethod.Invoke($null, @())
Assert $ok 'DB.Init failed for legacy fixture'
$args = [object[]]@([string]$null)
$ok = [bool]$compatMethod.Invoke($null, $args)
if (-not $ok) { throw "IsDatabaseCompatible failed after normalization: $($args[0])" }
Assert-LegacyContent $normalizeFile $currentSchema
$args = [object[]]@($fixture, $normalizeFile, [string]$null)
$ok = [bool]$preserveMethod.Invoke($null, $args)
if (-not $ok) { throw "Preservation validator failed normalized DB: $($args[2])" }
Write-Host 'DIRECT NORMALIZATION TEST: PASS'

if (Test-Path $freshFile) { Remove-Item $freshFile -Force }
$fileProp.SetValue($null, $freshFile, $null)
$ok = [bool]$initMethod.Invoke($null, @())
Assert $ok 'fresh DB.Init failed'
$importArgs = [object[]]@($fixture, [string]$null, $true)
$ok = [bool]$importMethod.Invoke($null, $importArgs)
if (-not $ok) { throw "ImportAndMergeDatabase failed: $($importArgs[1])" }
$ok = [bool]$writeOne.Invoke($null, @($candidateFile))
Assert $ok 'WriteDB(candidate) failed'
Assert-LegacyContent $candidateFile $currentSchema
$preserveArgs = [object[]]@($fixture, $candidateFile, [string]$null)
$ok = [bool]$preserveMethod.Invoke($null, $preserveArgs)
if (-not $ok) { throw "Preservation validator failed candidate DB: $($preserveArgs[2])" }
Write-Host 'CANDIDATE IMPORT TEST: PASS'
Write-Host 'DB COMPATIBILITY REBUILD SELF-TEST: PASS'