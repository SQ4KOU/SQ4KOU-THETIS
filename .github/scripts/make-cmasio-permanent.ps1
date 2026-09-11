$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$path = 'Project Files\Source\Console\console.cs'
$text = [System.IO.File]::ReadAllText((Resolve-Path $path))
$enc = New-Object System.Text.UTF8Encoding($true)

$oldCall = 'if (!IsSetupFormNull) SetupForm.SetupCMAsio(_portAudioIssue, Common.HasArg(args, "-cmasioconfig"));'
$newCall = @'
// SQ4KOU_CMASIO_PERMANENT: Setup > Audio > cmASIO is always available.
            // Do not depend on the -cmasioconfig launch argument.
            if (!IsSetupFormNull) SetupForm.SetupCMAsio(_portAudioIssue, true);
'@

$count = ([regex]::Matches($text, [regex]::Escape($oldCall))).Count
if ($count -ne 1) { throw "cmASIO startup call anchor: expected 1, found $count" }
$text = $text.Replace($oldCall, $newCall.TrimEnd("`r","`n"))

$helpPattern = '(?m)^\s*s \+= "  -cmasioconfig\s+show the cmASIO setup tab in audio setup\\n";\r?\n'
$matches = [regex]::Matches($text, $helpPattern)
if ($matches.Count -ne 1) { throw "cmASIO help-line anchor: expected 1, found $($matches.Count)" }
$text = [regex]::Replace($text, $helpPattern, '', 1)

[System.IO.File]::WriteAllText((Resolve-Path $path), $text, $enc)

$verify = [System.IO.File]::ReadAllText((Resolve-Path $path))
if (-not $verify.Contains('SQ4KOU_CMASIO_PERMANENT')) { throw 'Permanent cmASIO marker missing' }
if (-not $verify.Contains('SetupForm.SetupCMAsio(_portAudioIssue, true);')) { throw 'Permanent SetupCMAsio call missing' }
if ($verify.Contains('Common.HasArg(args, "-cmasioconfig")')) { throw 'Startup argument still controls cmASIO setup' }
if ($verify.Contains('-cmasioconfig      show the cmASIO setup tab')) { throw 'Obsolete cmASIO help entry still present' }

Write-Host 'CMASIO_PERMANENT_CONSOLE_PATCH=PASS'
