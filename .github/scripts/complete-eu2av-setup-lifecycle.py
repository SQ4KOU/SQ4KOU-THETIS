from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
CONSOLE = ROOT / "Project Files" / "Source" / "Console"
SETUP = CONSOLE / "Setup.GPUWaterfall.cs"
DISPLAY = CONSOLE / "display.cs"
POST = CONSOLE / "Display.GPUPostProcessing.cs"


def read(path):
    return path.read_text(encoding="utf-8-sig")


def write(path, text):
    path.write_text(text, encoding="utf-8", newline="\n")


def replace_method(text, signature, replacement):
    pos = text.find(signature)
    if pos < 0:
        raise RuntimeError(f"Method not found: {signature}")
    line_start = text.rfind("\n", 0, pos) + 1
    brace = text.find("{", pos)
    if brace < 0:
        raise RuntimeError(f"Opening brace not found: {signature}")
    depth = 0
    end = -1
    for i in range(brace, len(text)):
        c = text[i]
        if c == "{":
            depth += 1
        elif c == "}":
            depth -= 1
            if depth == 0:
                end = i
                break
    if end < 0:
        raise RuntimeError(f"Closing brace not found: {signature}")
    return text[:line_start] + replacement.rstrip() + "\n" + text[end + 1:]


def replace_delegate(text, marker, replacement):
    pos = text.find(marker)
    if pos < 0:
        raise RuntimeError(f"Delegate marker not found: {marker}")
    line_start = text.rfind("\n", 0, pos) + 1
    brace = text.find("{", pos)
    if brace < 0:
        raise RuntimeError("Delegate opening brace not found")
    depth = 0
    end = -1
    for i in range(brace, len(text)):
        c = text[i]
        if c == "{":
            depth += 1
        elif c == "}":
            depth -= 1
            if depth == 0:
                end = i
                break
    if end < 0:
        raise RuntimeError("Delegate closing brace not found")
    semi = text.find(";", end)
    if semi < 0 or semi - end > 8:
        raise RuntimeError("Delegate semicolon not found")
    return text[:line_start] + replacement.rstrip() + "\n" + text[semi + 1:]


setup = read(SETUP)

# Complete the recovered General-tab controls. The resampling ComboBox was only
# declared in the partial port, which made the setting permanently null/inert.
if "private CheckBoxTS chkAutoThreshold;" not in setup:
    field_anchor = "        private ComboBoxTS comboGPUWaterfallResampling;"
    if field_anchor not in setup:
        raise RuntimeError("Setup field anchor not found")
    fields = """        private ComboBoxTS comboGPUWaterfallResampling;
        private CheckBoxTS chkAutoThreshold;
        private LabelTS lblAutoThHint;
        private NumericUpDownTS udAutoThFine;
        private LabelTS lblAutoThFineHint;
        private LabelTS lblGPUWaterfallResampling;"""
    setup = setup.replace(field_anchor, fields, 1)

if "private void InitGeneralTabWaterfallControls()" not in setup:
    anchor = "\tprivate void InitWaterfallTab()"
    if anchor not in setup:
        anchor = "        private void InitWaterfallTab()"
    if anchor not in setup:
        raise RuntimeError("InitWaterfallTab anchor not found")
    general_method = r'''        private void InitGeneralTabWaterfallControls()
        {
            if (comboGPUWaterfallResampling != null || tpDisplayGeneral == null)
                return;

            GroupBoxTS groupBoxTS = new GroupBoxTS();
            groupBoxTS.Text = "Waterfall thresholds (auto)";
            groupBoxTS.Location = new Point(392, 148);
            groupBoxTS.Size = new Size(170, 112);
            groupBoxTS.Name = "grpWaterfallThresholdsAuto";

            chkAutoThreshold = new CheckBoxTS();
            chkAutoThreshold.Name = "chkAutoThreshold";
            chkAutoThreshold.Text = "Auto";
            chkAutoThreshold.AutoSize = true;
            chkAutoThreshold.Location = new Point(8, 19);
            chkAutoThreshold.Checked = Display.AutoThresholdEnabled;
            chkAutoThreshold.CheckedChanged += chkAutoThreshold_CheckedChanged;

            lblAutoThHint = new LabelTS();
            lblAutoThHint.AutoSize = true;
            lblAutoThHint.Location = new Point(76, 20);
            lblAutoThHint.Text = "thresholds";

            udAutoThFine = new NumericUpDownTS();
            udAutoThFine.Name = "udAutoThFine";
            udAutoThFine.Location = new Point(48, 44);
            udAutoThFine.Size = new Size(56, 20);
            udAutoThFine.Minimum = -20m;
            udAutoThFine.Maximum = 20m;
            udAutoThFine.DecimalPlaces = 0;
            udAutoThFine.Increment = 1m;
            decimal fine = (decimal)Display.AutoThresholdFineOffset;
            if (fine < udAutoThFine.Minimum) fine = udAutoThFine.Minimum;
            if (fine > udAutoThFine.Maximum) fine = udAutoThFine.Maximum;
            udAutoThFine.Value = fine;
            udAutoThFine.ValueChanged += udAutoThFine_ValueChanged;

            lblAutoThFineHint = new LabelTS();
            lblAutoThFineHint.AutoSize = true;
            lblAutoThFineHint.Location = new Point(8, 46);
            lblAutoThFineHint.Text = "Fine";

            lblGPUWaterfallResampling = new LabelTS();
            lblGPUWaterfallResampling.AutoSize = true;
            lblGPUWaterfallResampling.Location = new Point(8, 75);
            lblGPUWaterfallResampling.Text = "GPU scale";

            comboGPUWaterfallResampling = new ComboBoxTS();
            comboGPUWaterfallResampling.Name = "comboGPUWaterfallResampling";
            comboGPUWaterfallResampling.DropDownStyle = ComboBoxStyle.DropDownList;
            comboGPUWaterfallResampling.Location = new Point(80, 72);
            comboGPUWaterfallResampling.Size = new Size(82, 21);
            comboGPUWaterfallResampling.Items.AddRange(new object[2] { "Fast", "Quality" });
            int resampling = (int)Display.GPUWaterfallResamplingMode;
            if (resampling < 0) resampling = 0;
            if (resampling > 1) resampling = 1;
            comboGPUWaterfallResampling.SelectedIndex = resampling;
            comboGPUWaterfallResampling.SelectedIndexChanged += comboGPUWaterfallResampling_SelectedIndexChanged;

            groupBoxTS.Controls.Add(chkAutoThreshold);
            groupBoxTS.Controls.Add(lblAutoThHint);
            groupBoxTS.Controls.Add(lblAutoThFineHint);
            groupBoxTS.Controls.Add(udAutoThFine);
            groupBoxTS.Controls.Add(lblGPUWaterfallResampling);
            groupBoxTS.Controls.Add(comboGPUWaterfallResampling);
            tpDisplayGeneral.Controls.Add(groupBoxTS);
            groupBoxTS.BringToFront();
        }

'''
    setup = setup.replace(anchor, general_method + anchor, 1)

# Ensure the recovered General-tab controls are actually created.
old_init = """        private void InitGPUWaterfallSetupUI()
        {
            InitWaterfallTab();
            if (_tpWaterfall != null && grpDisplayDriverEngine != null)
                InitNoiseFloorProControls(grpDisplayDriverEngine, 0);
        }"""
if old_init in setup:
    new_init = """        private void InitGPUWaterfallSetupUI()
        {
            InitWaterfallTab();
            InitGeneralTabWaterfallControls();
            if (_tpWaterfall != null && grpDisplayDriverEngine != null)
                InitNoiseFloorProControls(grpDisplayDriverEngine, 0);
        }"""
    setup = setup.replace(old_init, new_init, 1)
elif "InitGeneralTabWaterfallControls();" not in setup[setup.find("private void InitGPUWaterfallSetupUI()"):setup.find("private void InitWaterfallTab()")]:
    raise RuntimeError("Could not wire InitGeneralTabWaterfallControls")

# Recovered Auto Threshold handlers were absent from the partial port.
if "private void chkAutoThreshold_CheckedChanged" not in setup:
    handler_anchor = "private void comboGPUWaterfallResampling_SelectedIndexChanged"
    pos = setup.find(handler_anchor)
    if pos < 0:
        raise RuntimeError("Resampling handler anchor not found")
    line_start = setup.rfind("\n", 0, pos) + 1
    handlers = r'''        private void chkAutoThreshold_CheckedChanged(object sender, EventArgs e)
        {
            if (!initializing)
                Display.AutoThresholdEnabled = chkAutoThreshold.Checked;
        }

        private void udAutoThFine_ValueChanged(object sender, EventArgs e)
        {
            if (!initializing)
                Display.AutoThresholdFineOffset = (float)udAutoThFine.Value;
        }

'''
    setup = setup[:line_start] + handlers + setup[line_start:]

# Preserve Auto/explicit GPU intent until the real D2D DeviceContext exists.
# Use only APIs that exist in the recovered detector: Level is read-only and
# capability fallback is resolved locally for Setup selection.
apply_method = r'''        private void ApplyGPUSelection(int selectedIndex)
        {
            Display.DetectGPUCapabilitiesFromD2D();
            if (selectedIndex < 0) selectedIndex = 0;

            bool auto = selectedIndex == 0;
            bool wantsGPU = selectedIndex != 1;
            if (!GPUDetector.HasDeviceContext && wantsGPU)
            {
                _renderFilterPending = true;
                Display.AutoEnableGPU = true;
                Display.GPUEffectsEnabled = false;
                UpdateWaterfallPaletteItems(false);
                SyncGPUWaterfallPipelineEnabled(0);
                UpdateGPUInfoLabel();
                return;
            }

            int detectedLevel = Display.GPUDetectionLevel;
            int requestedLevel = selectedIndex switch
            {
                1 => 0,
                2 => 1,
                3 => 2,
                _ => detectedLevel,
            };
            int effectiveLevel = requestedLevel;
            if (effectiveLevel >= 2 && !GPUDetector.HasCustomShaders)
                effectiveLevel = GPUDetector.HasBuiltInEffects ? 1 : 0;
            if (effectiveLevel >= 1 && !GPUDetector.HasBuiltInEffects)
                effectiveLevel = 0;

            _renderFilterPending = false;
            Display.GPUEffectsEnabled = effectiveLevel >= 1 && GPUDetector.HasBuiltInEffects;
            Display.AutoEnableGPU = auto || effectiveLevel >= 1;
            UpdateWaterfallRenderQualityItems(effectiveLevel);
            UpdateWaterfallPaletteItems(effectiveLevel >= 1);
            SyncGPUWaterfallPipelineEnabled(effectiveLevel);
            UpdateGPUInfoLabel();

            if (GPUDetector.HasDeviceContext)
            {
                if (requestedLevel >= 2 && !GPUDetector.HasCustomShaders)
                {
                    MessageBox.Show("Custom HLSL shaders (Level 2) are not available.\nUsing Level 1 (Built-in Effects) instead.", "GPU Acceleration", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
                }
                else if (requestedLevel >= 1 && !GPUDetector.HasBuiltInEffects)
                {
                    MessageBox.Show("Built-in D2D Effects are not available on this system.\nUsing CPU post-processing (Level 0).", "GPU Acceleration", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
                }
            }
        }'''
setup = replace_method(setup, "private void ApplyGPUSelection(int selectedIndex)", apply_method)

# Consume a pending Auto/Level1/Level2 selection as soon as D2D becomes live.
timer_method = r'''            _gpuStatusTimer.Tick += delegate
            {
                try
                {
                    Display.DetectGPUCapabilitiesFromD2D();
                    if (_renderFilterPending && GPUDetector.HasDeviceContext && comboGPU != null)
                    {
                        _renderFilterPending = false;
                        ApplyGPUSelection(comboGPU.SelectedIndex);
                    }
                    UpdateGPUInfoLabel();
                    UpdateWaterfallPaletteItems(Display.GPUEffectsEnabled);
                }
                catch
                {
                }
            };'''
if "_gpuStatusTimer.Tick += delegate" in setup:
    setup = replace_delegate(setup, "_gpuStatusTimer.Tick += delegate", timer_method)
else:
    raise RuntimeError("GPU status timer not found")

# Sanity checks before writing Setup.
for token in (
    "comboGPUWaterfallResampling = new ComboBoxTS()",
    "chkAutoThreshold = new CheckBoxTS()",
    "Display.AutoThresholdEnabled",
    "Display.AutoThresholdFineOffset",
    "Display.GPUWaterfallResamplingMode",
    "_renderFilterPending && GPUDetector.HasDeviceContext",
    "Display.DetectGPUCapabilitiesFromD2D()",
    "SyncGPUWaterfallPipelineEnabled(effectiveLevel)",
):
    if token not in setup:
        raise RuntimeError("Setup integration token missing: " + token)
if "PullPaletteTelemetry" in setup:
    raise RuntimeError("Invalid stale palette telemetry call remains")
if "GPUDetector.SetLevel" in setup:
    raise RuntimeError("Invalid detector mutator remains")
write(SETUP, setup)

# Lifecycle bridge: managed D3D11 waterfall surfaces are bound to the D2D
# DeviceContext and must be rebound on target recreation; all managed GPU
# resources must be released on DX2D shutdown.
post = read(POST)
if "private static void OnD2DDeviceContextRecreated" not in post:
    close = post.rfind("\n    }\n}")
    if close < 0:
        raise RuntimeError("Display.GPUPostProcessing class close not found")
    lifecycle = r'''
        private static void OnD2DDeviceContextRecreated(SharpDX.Direct2D1.DeviceContext dc)
        {
            if (dc == null || dc.IsDisposed) return;
            try { if (_waterfallGPU1 != null) _waterfallGPU1.UpdateDeviceContext(dc); } catch { }
            try { if (_waterfallGPU2 != null) _waterfallGPU2.UpdateDeviceContext(dc); } catch { }
            WaterfallEffect.Reset();
            DetectGPUCapabilitiesFromD2D();
        }

        private static void ShutdownManagedGPUWaterfallResources()
        {
            try { if (_waterfallGPU1 != null) _waterfallGPU1.Dispose(); } catch { }
            _waterfallGPU1 = null;
            try { if (_waterfallGPU2 != null) _waterfallGPU2.Dispose(); } catch { }
            _waterfallGPU2 = null;
            try { if (_gpuFFT1 != null) _gpuFFT1.Dispose(); } catch { }
            _gpuFFT1 = null;
            try { if (_gpuFFT2 != null) _gpuFFT2.Dispose(); } catch { }
            _gpuFFT2 = null;
            _gpuEffectsEnabled = false;
            ResetTemporalWaterfallState();
            WaterfallEffect.Reset();
        }
'''
    post = post[:close] + lifecycle + post[close:]
write(POST, post)

# Wire lifecycle into the actual D2D target creation/shutdown paths.
display = read(DISPLAY)
if "OnD2DDeviceContextRecreated(ctx);" not in display:
    target_anchor = "                ctx.Target = _d2dTargetBitmap;"
    if target_anchor not in display:
        raise RuntimeError("D2D target bind anchor not found")
    display = display.replace(target_anchor, target_anchor + "\n                OnD2DDeviceContextRecreated(ctx);", 1)

if "ShutdownManagedGPUWaterfallResources();" not in display:
    shutdown_anchor = """            lock (_objDX2Lock)
            {
                if (!_bDX2Setup) return;"""
    if shutdown_anchor not in display:
        raise RuntimeError("ShutdownDX2D lock anchor not found")
    shutdown_new = """            lock (_objDX2Lock)
            {
                ShutdownManagedGPUWaterfallResources();
                if (!_bDX2Setup) return;"""
    display = display.replace(shutdown_anchor, shutdown_new, 1)

for token in (
    "OnD2DDeviceContextRecreated(ctx);",
    "ShutdownManagedGPUWaterfallResources();",
):
    if token not in display:
        raise RuntimeError("Display lifecycle token missing: " + token)
write(DISPLAY, display)

print("COMPLETE_EU2AV_SETUP_LIFECYCLE=PASS")
