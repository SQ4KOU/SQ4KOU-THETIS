from pathlib import Path

root = Path(__file__).resolve().parents[2]
console = root / "Project Files" / "Source" / "Console"
display_path = console / "Display.GPUWaterfall.cs"
setup_path = console / "setup.cs"


def replace_once(text: str, old: str, new: str, label: str) -> str:
    count = text.count(old)
    if count == 0:
        if new in text:
            print(f"{label}=ALREADY_PRESENT")
            return text
        raise RuntimeError(f"{label}: target not found")
    if count != 1:
        raise RuntimeError(f"{label}: expected one target, found {count}")
    print(f"{label}=PATCHED")
    return text.replace(old, new, 1)


display = display_path.read_text(encoding="utf-8-sig")

old_builtin = """                _gpuPaletteUpload[n] = r; _gpuPaletteUpload[n + 1] = g; _gpuPaletteUpload[n + 2] = b; _gpuPaletteUpload[n + 3] = 1f;"""
new_builtin = """                // SQ4KOU_GPU_PALETTE_NORMALIZED: D2D shader LUT is float4 in 0..1, while WaterfallPalette.Sample returns RGB in 0..255.
                _gpuPaletteUpload[n] = r / 255f; _gpuPaletteUpload[n + 1] = g / 255f; _gpuPaletteUpload[n + 2] = b / 255f; _gpuPaletteUpload[n + 3] = 1f;"""
display = replace_once(display, old_builtin, new_builtin, "GPU_BUILTIN_LUT_NORMALIZATION")

old_legacy = """                _gpuPaletteUpload[n] = r;
                _gpuPaletteUpload[n + 1] = g;
                _gpuPaletteUpload[n + 2] = b;
                _gpuPaletteUpload[n + 3] = 1f;"""
new_legacy = """                _gpuPaletteUpload[n] = r / 255f;
                _gpuPaletteUpload[n + 1] = g / 255f;
                _gpuPaletteUpload[n + 2] = b / 255f;
                _gpuPaletteUpload[n + 3] = 1f;"""
display = replace_once(display, old_legacy, new_legacy, "GPU_LEGACY_LUT_NORMALIZATION")

display_path.write_text(display, encoding="utf-8")

setup = setup_path.read_text(encoding="utf-8-sig")

rx1_old = """            else if (comboColorPalette.Text == \"DeepBlue 256\")
            {
                console.RX1ColourScheme = ColorScheme.DeepBlue;
                clrbtnWaterfallLow.Visible = false;
            }"""
rx1_new = rx1_old + """
            // SQ4KOU_GPU_PALETTE_256_ALIAS: recovered 2.10.3.16 Final UI names mapped to existing schemes.
            else if (comboColorPalette.Text == \"Enhanced 256\")
            {
                console.RX1ColourScheme = ColorScheme.enhanced;
                clrbtnWaterfallLow.Visible = false;
            }
            else if (comboColorPalette.Text == \"BlackWhite 256\")
            {
                console.RX1ColourScheme = ColorScheme.BLACKWHITE;
                clrbtnWaterfallLow.Visible = false;
            }"""
setup = replace_once(setup, rx1_old, rx1_new, "RX1_GPU_PALETTE_MAPPING")

rx2_old = """            else if (comboRX2ColorPalette.Text == \"DeepBlue 256\")
            {
                console.RX2ColourScheme = ColorScheme.DeepBlue;
                clrbtnRX2WaterfallLow.Visible = false;
            }"""
rx2_new = rx2_old + """
            else if (comboRX2ColorPalette.Text == \"Enhanced 256\")
            {
                console.RX2ColourScheme = ColorScheme.enhanced;
                clrbtnRX2WaterfallLow.Visible = false;
            }
            else if (comboRX2ColorPalette.Text == \"BlackWhite 256\")
            {
                console.RX2ColourScheme = ColorScheme.BLACKWHITE;
                clrbtnRX2WaterfallLow.Visible = false;
            }"""
setup = replace_once(setup, rx2_old, rx2_new, "RX2_GPU_PALETTE_MAPPING")

tx_old = """            else if (comboColorPalette_tx.Text == \"DeepBlue 256\")
            {
                console.TXColourScheme = ColorScheme.DeepBlue;
                clrbtnWaterfallLow_tx.Visible = false;
            }"""
tx_new = tx_old + """
            else if (comboColorPalette_tx.Text == \"Enhanced 256\")
            {
                console.TXColourScheme = ColorScheme.enhanced;
                clrbtnWaterfallLow_tx.Visible = false;
            }
            else if (comboColorPalette_tx.Text == \"BlackWhite 256\")
            {
                console.TXColourScheme = ColorScheme.BLACKWHITE;
                clrbtnWaterfallLow_tx.Visible = false;
            }"""
setup = replace_once(setup, tx_old, tx_new, "TX_GPU_PALETTE_MAPPING")

setup_path.write_text(setup, encoding="utf-8")

# Deterministic post-check: fail rather than silently produce a partial patch.
display_check = display_path.read_text(encoding="utf-8")
setup_check = setup_path.read_text(encoding="utf-8")
required = [
    (display_check, "SQ4KOU_GPU_PALETTE_NORMALIZED"),
    (display_check, "_gpuPaletteUpload[n] = r / 255f"),
    (setup_check, 'comboColorPalette.Text == "Enhanced 256"'),
    (setup_check, 'comboColorPalette.Text == "BlackWhite 256"'),
    (setup_check, 'comboRX2ColorPalette.Text == "Enhanced 256"'),
    (setup_check, 'comboRX2ColorPalette.Text == "BlackWhite 256"'),
    (setup_check, 'comboColorPalette_tx.Text == "Enhanced 256"'),
    (setup_check, 'comboColorPalette_tx.Text == "BlackWhite 256"'),
]
for text, token in required:
    if token not in text:
        raise RuntimeError(f"post-check missing: {token}")

print("SQ4KOU_GPU_PALETTE_FIX=PASS")
