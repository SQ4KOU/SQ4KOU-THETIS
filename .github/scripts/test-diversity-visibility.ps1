$ErrorActionPreference = 'Stop'
# Execute the production compact-layout methods against real Windows Forms controls.
# The harness excludes radio/DSP initialization; it tests the visibility lifecycle only.
$source = Get-Content 'Project Files/Source/Console/DiversityForm.cs' -Raw
function Get-Method([string]$signature) {
    $start = $source.IndexOf($signature)
    if ($start -lt 0) { throw "Missing production method: $signature" }
    $brace = $source.IndexOf('{', $start)
    $depth = 1
    $end = $brace + 1
    while ($depth -gt 0 -and $end -lt $source.Length) {
        if ($source[$end] -eq '{') { $depth++ }
        if ($source[$end] -eq '}') { $depth-- }
        $end++
    }
    if ($depth -ne 0) { throw "Unbalanced production method: $signature" }
    return $source.Substring($start, $end - $start)
}
$constructor = Get-Method 'public DiversityForm(Console c)'
if ($constructor.Contains('InitializeSQ4KOUCompactMode();')) {
    throw 'Visibility must not be captured while the constructor parent is hidden'
}
$fieldStart = $source.IndexOf('private bool _sq4kouCompactMode;')
$fieldEnd = $source.IndexOf('// Preserve the Phase/Gain values', $fieldStart)
$fields = $source.Substring($fieldStart, $fieldEnd - $fieldStart)
$methods = @(
    (Get-Method 'protected override void OnShown(EventArgs e)'),
    (Get-Method 'private void InitializeSQ4KOUCompactMode()'),
    (Get-Method 'private void SetSQ4KOUCompactMode(bool compact)')
) -join "`n"
$fixture = @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
public class DiversityVisibilityFixture : Form
{
    private PictureBox picRadar = new PictureBox();
    private Panel controlsPanel = new Panel();
    private GroupBox nativePanel = new GroupBox();
    private CheckBox topControl = new CheckBox();
    private GroupBox intentionallyHidden = new GroupBox();
    private Size expectedSize;
    private Rectangle expectedRadar;
    private Size expectedMinimum;
    private void EnsurePA3GHMNativePanelSize() { }
    public DiversityVisibilityFixture(bool loadLayout)
    {
        ClientSize = new Size(720, 598);
        MinimumSize = SizeFromClientSize(ClientSize);
        picRadar.Bounds = new Rectangle(4, 289, 305, 305);
        Controls.Add(controlsPanel);
        Controls.Add(nativePanel);
        Controls.Add(topControl);
        Controls.Add(intentionallyHidden);
        Controls.Add(picRadar);
        intentionallyHidden.Visible = false;
        Load += delegate {
            if (loadLayout) {
                ClientSize = new Size(900, 700);
                picRadar.Bounds = new Rectangle(470, 1, 226, 226);
            }
            expectedSize = ClientSize;
            expectedRadar = picRadar.Bounds;
            expectedMinimum = MinimumSize;
        };
    }
    private static void Check(bool result, string message)
    {
        if (!result) throw new Exception(message);
    }
    private void CheckExpanded()
    {
        Check(controlsPanel.Visible && nativePanel.Visible && topControl.Visible,
            "Expanded Diversity controls disappeared");
        Check(!intentionallyHidden.Visible, "Intentionally hidden group was exposed");
        Check(ClientSize == expectedSize, "Expanded window size was not restored");
        Check(MinimumSize == expectedMinimum, "Expanded minimum size was not restored");
        Check(picRadar.Bounds == expectedRadar, "Expanded radar layout was not restored");
    }
    public static void Run()
    {
        foreach (bool loadLayout in new bool[] { false, true })
        using (var form = new DiversityVisibilityFixture(loadLayout)) {
            Check(!form.controlsPanel.Visible, "Hidden-parent WinForms behavior not reproduced");
            form.Show();
            Application.DoEvents();
            Check(form._sq4kouCompactInitialised && form._sq4kouCompactMode,
                "First shown form must initialize compact mode");
            Check(form.ClientSize == new Size(313, 313), "Wrong compact dimensions");
            Check(form.picRadar.Visible && !form.controlsPanel.Visible && !form.nativePanel.Visible,
                "Compact mode should display the radar only");
            for (int i = 0; i < 3; i++) {
                form.SetSQ4KOUCompactMode(false);
                form.CheckExpanded();
                form.SetSQ4KOUCompactMode(true);
            }
            form.Hide();
            form.Show();
            Application.DoEvents();
            form.SetSQ4KOUCompactMode(false);
            form.CheckExpanded();
        }
    }
'@
Add-Type -TypeDefinition ($fixture + "`n" + $fields + "`n" + $methods + "`n}") -ReferencedAssemblies System.Windows.Forms,System.Drawing
[DiversityVisibilityFixture]::Run()
Write-Host 'SQ4KOU_DIVERSITY_VISIBILITY=PASS (real WinForms, initial Show, Load layout, repeated toggles, Hide/Show)'
