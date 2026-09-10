$ErrorActionPreference = 'Stop'
$root = Join-Path $PSScriptRoot '../..'
$actual = Get-Content (Join-Path $root 'Project Files/Source/Console/WaterfallPalette.cs') -Raw
$reference = Get-Content (Join-Path $root '.github/recovered/eu2av-2.10.3.16/WaterfallPalette.EU2AV.2.10.3.16.cs.txt') -Raw
$reference = $reference.Replace('namespace Thetis;', "namespace RecoveredFinal {") + "`n}"
Add-Type -TypeDefinition $actual -IgnoreWarnings
Add-Type -TypeDefinition $reference -IgnoreWarnings
$count = 0
foreach ($name in @('ConsoleStops', 'ThermalStops', 'DeepBlueStops', 'EnhancedStops', 'GrayscaleStops')) {
    $a = [Thetis.WaterfallPalette]::new()
    $b = [RecoveredFinal.WaterfallPalette]::new()
    $a.Build([Thetis.WaterfallPalette]::$name)
    $b.Build([RecoveredFinal.WaterfallPalette]::$name)
    $samples = @(0..255 | ForEach-Object { [float]($_ / 255.0) })
    $samples += @([float]::NaN, [float]::NegativeInfinity, [float]::PositiveInfinity, [float]-1, [float]2, [float]0.501)
    foreach ($p in $samples) {
        [float]$ar=0; [float]$ag=0; [float]$ab=0
        [float]$br=0; [float]$bg=0; [float]$bb=0
        $a.Sample($p,[ref]$ar,[ref]$ag,[ref]$ab)
        $b.Sample($p,[ref]$br,[ref]$bg,[ref]$bb)
        if ($ar -ne $br -or $ag -ne $bg -or $ab -ne $bb) { throw "Final palette mismatch: $name at $p" }
        foreach ($v in @($ar,$ag,$ab)) {
            if ([float]::IsNaN($v) -or $v -lt 0 -or $v -gt 255) { throw "Invalid RGB value: $v" }
        }
        $count++
    }
}
# This discriminates the recovered Oklab ramp from the removed linear grayscale alias.
$g = [Thetis.WaterfallPalette]::new()
$g.Build([Thetis.WaterfallPalette]::GrayscaleStops)
[float]$r=0; [float]$green=0; [float]$blue=0
$g.Sample([float]0.5,[ref]$r,[ref]$green,[ref]$blue)
if ($r -lt 98 -or $r -gt 100) { throw "Grayscale256 is not the recovered Oklab ramp: $r" }
Write-Host "FINAL_PALETTE_PARITY=PASS ($count samples, exact float equality on every RGB channel)"
