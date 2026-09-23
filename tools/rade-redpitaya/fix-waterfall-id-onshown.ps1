$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$path = Join-Path $repo 'Project Files\Source\Console\Console.WaterfallIDMenuFix.cs'
if (-not (Test-Path $path)) { throw "Missing Waterfall ID menu source: $path" }

$text = [System.IO.File]::ReadAllText($path)
if ($text -match [regex]::Escape('_sq4kouWaterfallIdMenuHook')) {
    Write-Host 'WATERFALL_ID_ONSHOWN_BUILD_FIX=ALREADY_APPLIED'
    exit 0
}

$pattern = '(?s)\s*protected\s+override\s+void\s+OnShown\s*\(\s*EventArgs\s+e\s*\)\s*\{\s*base\.OnShown\s*\(\s*e\s*\)\s*;\s*EnsureWaterfallIDMenuVisible\s*\(\s*\)\s*;\s*(?:Ensure3DPanadapterMenuVisible\s*\(\s*\)\s*;\s*)?\}'
$matches = [regex]::Matches($text, $pattern)
if ($matches.Count -ne 1) { throw "Expected exactly one Waterfall ID OnShown override, found $($matches.Count)" }

$replacement = @'

        // SQ4KOU merged-build lifecycle adapter: Console already owns the native OnShown
        // override. Use a one-shot Application.Idle hook so the menu repair executes only
        // after the main form is visible, without declaring a second Form.OnShown override.
        private static readonly bool _sq4kouWaterfallIdMenuHook = InstallWaterfallIdMenuHook();

        private static bool InstallWaterfallIdMenuHook()
        {
            Application.Idle += Sq4kouWaterfallIdMenuIdle;
            return true;
        }

        private static void Sq4kouWaterfallIdMenuIdle(object sender, EventArgs e)
        {
            foreach (Form form in Application.OpenForms)
            {
                Console console = form as Console;
                if (console == null || !console.Visible)
                    continue;

                console.EnsureWaterfallIDMenuVisible();
                console.Ensure3DPanadapterMenuVisible();
                Application.Idle -= Sq4kouWaterfallIdMenuIdle;
                break;
            }
        }
'@

$text = [regex]::Replace($text, $pattern, $replacement, 1)
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($path, $text, $utf8NoBom)

$verify = [System.IO.File]::ReadAllText($path)
if ($verify -match 'protected\s+override\s+void\s+OnShown\s*\(') { throw 'Duplicate OnShown override still present after build fix' }
foreach ($token in @('_sq4kouWaterfallIdMenuHook','InstallWaterfallIdMenuHook','Sq4kouWaterfallIdMenuIdle','EnsureWaterfallIDMenuVisible','Ensure3DPanadapterMenuVisible')) {
    if ($verify -notmatch [regex]::Escape($token)) { throw "Missing post-fix token: $token" }
}
Write-Host 'WATERFALL_ID_ONSHOWN_BUILD_FIX=PASS'
