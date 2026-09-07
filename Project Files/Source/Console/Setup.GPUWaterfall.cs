using System;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis
{
    public partial class Setup
    {
        // SQ4KOU V4 - Waterfall Pro page modelled on Thetis 2.10.3.16 Extended Final.
        // Built programmatically so the very large setup.designer.cs remains untouched.
        private TabPage tpWaterfall;
        private GroupBoxTS grpWaterfallPro;
        private GroupBoxTS grpWFNoiseFloor;
        private GroupBoxTS grpWFEnhancement;
        private GroupBoxTS grpGPUWaterfallFFT;
        private GroupBoxTS grpGPUAcceleration;

        private ComboBoxTS comboWFNFMode;
        private NumericUpDownTS udWFNFLow;
        private NumericUpDownTS udWFNFHigh;
        private CheckBoxTS chkWFNFAutoHigh;
        private NumericUpDownTS udWFNFAutoHighDb;
        private ComboBoxTS comboWFAGCSmooth;
        private ComboBoxTS comboWFDetect;
        private CheckBoxTS chkWFZoomAdaptive;

        private ComboBoxTS comboWFRender;
        private LabelTS lblWFPipeline;
        private ComboBoxTS comboWFToneMap;
        private ComboBoxTS comboWFTemporal;
        private TrackBarTS tbWFPaletteSharp;
        private TrackBarTS tbWFPaletteContrast;
        private LabelTS lblWFPaletteSharpValue;
        private LabelTS lblWFPaletteContrastValue;

        private CheckBoxTS chkGPUWaterfallFFT;
        private ComboBoxTS comboGPUWaterfallFFTSize;
        private CheckBoxTS chkGPUWaterfallAutoOverlap;
        private NumericUpDownTS udGPUWaterfallOverlap;
        private ComboBoxTS comboGPUWaterfallWindow;
        private ComboBoxTS comboGPUWaterfallMagnitudeMode;
        private ComboBoxTS comboGPUWaterfallResampling;
        private ComboBoxTS comboGPUWaterfallLanczos;

        private ComboBoxTS comboGPUAcceleration;
        private LabelTS lblGPUAdapter;
        private LabelTS lblGPUFeatureLevel;
        private LabelTS lblGPUCapabilities;
        private LabelTS lblGPURX1Runtime;
        private LabelTS lblGPURX2Runtime;
        private LabelTS lblGPUDropped;
        private LabelTS lblGPUCalibration;
        private ButtonTS btnGPUTest;
        private ButtonTS btnGPUDefaults;
        private ButtonTS btnGPUResetCalibration;
        private System.Windows.Forms.Timer gpuWaterfallStatusTimer;

        private static LabelTS GPUWFLabel(string text, int x, int y, int width)
        {
            LabelTS l = new LabelTS();
            l.Text = text;
            l.Location = new Point(x, y + 3);
            l.Size = new Size(width, 20);
            return l;
        }

        private static ComboBoxTS GPUWFCombo(string name, int x, int y, int width, params object[] items)
        {
            ComboBoxTS c = new ComboBoxTS();
            c.Name = name;
            c.DropDownStyle = ComboBoxStyle.DropDownList;
            c.Location = new Point(x, y);
            c.Size = new Size(width, 22);
            c.Items.AddRange(items);
            return c;
        }

        private void InitGPUWaterfallSetupUI()
        {
            if (tpWaterfall != null || tcDisplay == null) return;

            // V4 controls use new DB names, so old V3 test values cannot silently
            // override the Extended reference defaults on the first V4 run.
            Display.ApplyGPUWaterfallEU2AVDefaults();
            WaterfallEnhancer.SetQuality(WaterfallEnhancer.QualityLevel.Vivid); // Render: Medium
            WaterfallEnhancer.SetGamma(1.0f);                                  // Tone Map: Off

            tpWaterfall = new TabPage();
            tpWaterfall.Name = "tpWaterfallV4";
            tpWaterfall.Text = "Waterfall";
            tpWaterfall.BackColor = SystemColors.Control;
            tpWaterfall.Padding = new Padding(6);
            tpWaterfall.AutoScroll = true;

            grpWaterfallPro = new GroupBoxTS();
            grpWaterfallPro.Name = "grpWaterfallProV4";
            grpWaterfallPro.Text = "Waterfall Pro";
            grpWaterfallPro.Location = new Point(8, 6);
            grpWaterfallPro.Size = new Size(694, 365);

            BuildWFNoiseFloorGroup();
            BuildWFEnhancementGroup();
            BuildGPUFFTGroup();
            BuildGPUAccelerationGroup();

            grpWaterfallPro.Controls.Add(grpWFNoiseFloor);
            grpWaterfallPro.Controls.Add(grpWFEnhancement);
            grpWaterfallPro.Controls.Add(grpGPUWaterfallFFT);
            grpWaterfallPro.Controls.Add(grpGPUAcceleration);
            tpWaterfall.Controls.Add(grpWaterfallPro);
            tcDisplay.TabPages.Add(tpWaterfall);

            gpuWaterfallStatusTimer = new System.Windows.Forms.Timer();
            gpuWaterfallStatusTimer.Interval = 750;
            gpuWaterfallStatusTimer.Tick += delegate { UpdateGPUWaterfallDiagnostics(); };
            gpuWaterfallStatusTimer.Start();

            this.VisibleChanged += delegate
            {
                if (gpuWaterfallStatusTimer != null)
                    gpuWaterfallStatusTimer.Enabled = this.Visible;
            };

            UpdateGPUWaterfallSetupEnableState();
            UpdateGPUWaterfallDiagnostics();
        }

        private void BuildWFNoiseFloorGroup()
        {
            grpWFNoiseFloor = new GroupBoxTS();
            grpWFNoiseFloor.Name = "grpWFNoiseFloorV4";
            grpWFNoiseFloor.Text = "Noise Floor Pro";
            grpWFNoiseFloor.Location = new Point(12, 22);
            grpWFNoiseFloor.Size = new Size(320, 180);

            grpWFNoiseFloor.Controls.Add(GPUWFLabel("NF Mode:", 12, 22, 85));
            comboWFNFMode = GPUWFCombo("comboWFNFModeV4", 102, 20, 130, "Off", "Percentile");
            comboWFNFMode.SelectedIndex = Display.GPUWaterfallNFMode;
            comboWFNFMode.SelectedIndexChanged += delegate { Display.GPUWaterfallNFMode = comboWFNFMode.SelectedIndex; };
            grpWFNoiseFloor.Controls.Add(comboWFNFMode);

            grpWFNoiseFloor.Controls.Add(GPUWFLabel("NF Low %:", 12, 52, 85));
            udWFNFLow = new NumericUpDownTS();
            udWFNFLow.Name = "udWFNFLowV4";
            udWFNFLow.Minimum = 0; udWFNFLow.Maximum = 49; udWFNFLow.Value = Display.GPUWaterfallNFLowPercent;
            udWFNFLow.Location = new Point(102, 50); udWFNFLow.Size = new Size(55, 22);
            udWFNFLow.ValueChanged += delegate { Display.GPUWaterfallNFLowPercent = (int)udWFNFLow.Value; };
            grpWFNoiseFloor.Controls.Add(udWFNFLow);
            grpWFNoiseFloor.Controls.Add(GPUWFLabel("High:", 170, 52, 45));
            udWFNFHigh = new NumericUpDownTS();
            udWFNFHigh.Name = "udWFNFHighV4";
            udWFNFHigh.Minimum = 51; udWFNFHigh.Maximum = 100; udWFNFHigh.Value = Display.GPUWaterfallNFHighPercent;
            udWFNFHigh.Location = new Point(220, 50); udWFNFHigh.Size = new Size(55, 22);
            udWFNFHigh.ValueChanged += delegate { Display.GPUWaterfallNFHighPercent = (int)udWFNFHigh.Value; };
            grpWFNoiseFloor.Controls.Add(udWFNFHigh);

            chkWFNFAutoHigh = new CheckBoxTS();
            chkWFNFAutoHigh.Name = "chkWFNFAutoHighV4";
            chkWFNFAutoHigh.Text = "Auto High";
            chkWFNFAutoHigh.Location = new Point(12, 82); chkWFNFAutoHigh.Size = new Size(90, 22);
            chkWFNFAutoHigh.Checked = Display.GPUWaterfallNFAutoHigh;
            chkWFNFAutoHigh.CheckedChanged += delegate { Display.GPUWaterfallNFAutoHigh = chkWFNFAutoHigh.Checked; udWFNFAutoHighDb.Enabled = chkWFNFAutoHigh.Checked; };
            grpWFNoiseFloor.Controls.Add(chkWFNFAutoHigh);
            grpWFNoiseFloor.Controls.Add(GPUWFLabel("+dB:", 120, 82, 38));
            udWFNFAutoHighDb = new NumericUpDownTS();
            udWFNFAutoHighDb.Name = "udWFNFAutoHighDbV4";
            udWFNFAutoHighDb.Minimum = -30; udWFNFAutoHighDb.Maximum = 30; udWFNFAutoHighDb.DecimalPlaces = 1; udWFNFAutoHighDb.Increment = 0.5M;
            udWFNFAutoHighDb.Value = (decimal)Display.GPUWaterfallNFAutoHighDb;
            udWFNFAutoHighDb.Location = new Point(160, 80); udWFNFAutoHighDb.Size = new Size(65, 22);
            udWFNFAutoHighDb.ValueChanged += delegate { Display.GPUWaterfallNFAutoHighDb = (float)udWFNFAutoHighDb.Value; };
            grpWFNoiseFloor.Controls.Add(udWFNFAutoHighDb);

            grpWFNoiseFloor.Controls.Add(GPUWFLabel("AGC Smooth:", 12, 112, 85));
            comboWFAGCSmooth = GPUWFCombo("comboWFAGCSmoothV4", 102, 110, 130, "Off", "Fast", "Medium", "Slow");
            comboWFAGCSmooth.SelectedIndex = Display.GPUWaterfallNFAGCSmooth;
            comboWFAGCSmooth.SelectedIndexChanged += delegate { Display.GPUWaterfallNFAGCSmooth = comboWFAGCSmooth.SelectedIndex; };
            grpWFNoiseFloor.Controls.Add(comboWFAGCSmooth);

            grpWFNoiseFloor.Controls.Add(GPUWFLabel("WF Detect:", 12, 142, 85));
            comboWFDetect = GPUWFCombo("comboWFDetectV4", 102, 140, 130, "Peak", "Average");
            comboWFDetect.SelectedIndex = Display.GPUWaterfallDetectMode;
            comboWFDetect.SelectedIndexChanged += delegate { Display.GPUWaterfallDetectMode = comboWFDetect.SelectedIndex; };
            grpWFNoiseFloor.Controls.Add(comboWFDetect);

            chkWFZoomAdaptive = new CheckBoxTS();
            chkWFZoomAdaptive.Name = "chkWFZoomAdaptiveV4";
            chkWFZoomAdaptive.Text = "Zoom Adaptive";
            chkWFZoomAdaptive.Location = new Point(235, 140); chkWFZoomAdaptive.Size = new Size(82, 22);
            chkWFZoomAdaptive.Checked = Display.GPUWaterfallZoomAdaptive;
            chkWFZoomAdaptive.CheckedChanged += delegate { Display.GPUWaterfallZoomAdaptive = chkWFZoomAdaptive.Checked; };
            grpWFNoiseFloor.Controls.Add(chkWFZoomAdaptive);
        }

        private void BuildWFEnhancementGroup()
        {
            grpWFEnhancement = new GroupBoxTS();
            grpWFEnhancement.Name = "grpWFEnhancementV4";
            grpWFEnhancement.Text = "Enhancement";
            grpWFEnhancement.Location = new Point(344, 22);
            grpWFEnhancement.Size = new Size(334, 180);

            grpWFEnhancement.Controls.Add(GPUWFLabel("Render:", 12, 22, 65));
            comboWFRender = GPUWFCombo("comboWaterfallRenderQualityV4", 82, 20, 115, "Low", "Medium", "High", "Ultra");
            comboWFRender.SelectedIndex = 1;
            comboWFRender.SelectedIndexChanged += delegate
            {
                WaterfallEnhancer.SetQuality((WaterfallEnhancer.QualityLevel)Math.Max(0, Math.Min(3, comboWFRender.SelectedIndex)));
                UpdateGPUWaterfallDiagnostics();
            };
            grpWFEnhancement.Controls.Add(comboWFRender);

            lblWFPipeline = GPUWFLabel("CPU pipeline, 8-bit, Linear", 202, 22, 126);
            lblWFPipeline.Name = "lblWFPipelineV4";
            grpWFEnhancement.Controls.Add(lblWFPipeline);

            grpWFEnhancement.Controls.Add(GPUWFLabel("Tone Map:", 12, 54, 65));
            comboWFToneMap = GPUWFCombo("comboWFToneMapV4", 82, 52, 115, "Off", "Soft", "Strong");
            comboWFToneMap.SelectedIndex = 0;
            comboWFToneMap.SelectedIndexChanged += delegate
            {
                float gamma = comboWFToneMap.SelectedIndex == 1 ? 0.85f : (comboWFToneMap.SelectedIndex == 2 ? 0.70f : 1.0f);
                WaterfallEnhancer.SetGamma(gamma);
            };
            grpWFEnhancement.Controls.Add(comboWFToneMap);

            grpWFEnhancement.Controls.Add(GPUWFLabel("Temporal:", 12, 84, 65));
            comboWFTemporal = GPUWFCombo("comboWFTemporalV4", 82, 82, 115, "Off", "Fast", "Smooth");
            comboWFTemporal.SelectedIndex = Display.GPUWaterfallTemporalMode;
            comboWFTemporal.SelectedIndexChanged += delegate { Display.GPUWaterfallTemporalMode = comboWFTemporal.SelectedIndex; };
            grpWFEnhancement.Controls.Add(comboWFTemporal);

            grpWFEnhancement.Controls.Add(GPUWFLabel("Pal Sharp:", 12, 114, 65));
            tbWFPaletteSharp = new TrackBarTS();
            tbWFPaletteSharp.Name = "tbWFPaletteSharpV4";
            tbWFPaletteSharp.Minimum = 0; tbWFPaletteSharp.Maximum = 100; tbWFPaletteSharp.TickFrequency = 20;
            tbWFPaletteSharp.Value = (int)Math.Round(Display.GPUWaterfallPaletteSharpness * 100.0f);
            tbWFPaletteSharp.Location = new Point(82, 108); tbWFPaletteSharp.Size = new Size(190, 32);
            lblWFPaletteSharpValue = GPUWFLabel(tbWFPaletteSharp.Value.ToString(), 278, 114, 42);
            tbWFPaletteSharp.ValueChanged += delegate { Display.GPUWaterfallPaletteSharpness = tbWFPaletteSharp.Value / 100.0f; lblWFPaletteSharpValue.Text = tbWFPaletteSharp.Value.ToString(); };
            grpWFEnhancement.Controls.Add(tbWFPaletteSharp); grpWFEnhancement.Controls.Add(lblWFPaletteSharpValue);

            grpWFEnhancement.Controls.Add(GPUWFLabel("Pal Contrast:", 12, 146, 70));
            tbWFPaletteContrast = new TrackBarTS();
            tbWFPaletteContrast.Name = "tbWFPaletteContrastV4";
            tbWFPaletteContrast.Minimum = 0; tbWFPaletteContrast.Maximum = 100; tbWFPaletteContrast.TickFrequency = 20;
            tbWFPaletteContrast.Value = (int)Math.Round(Display.GPUWaterfallPaletteContrast * 100.0f);
            tbWFPaletteContrast.Location = new Point(82, 140); tbWFPaletteContrast.Size = new Size(190, 32);
            lblWFPaletteContrastValue = GPUWFLabel(tbWFPaletteContrast.Value.ToString(), 278, 146, 42);
            tbWFPaletteContrast.ValueChanged += delegate { Display.GPUWaterfallPaletteContrast = tbWFPaletteContrast.Value / 100.0f; lblWFPaletteContrastValue.Text = tbWFPaletteContrast.Value.ToString(); };
            grpWFEnhancement.Controls.Add(tbWFPaletteContrast); grpWFEnhancement.Controls.Add(lblWFPaletteContrastValue);
        }

        private void BuildGPUFFTGroup()
        {
            grpGPUWaterfallFFT = new GroupBoxTS();
            grpGPUWaterfallFFT.Name = "grpGPUWaterfallFFTV4";
            grpGPUWaterfallFFT.Text = "GPU FFT Waterfall";
            grpGPUWaterfallFFT.Location = new Point(12, 210);
            grpGPUWaterfallFFT.Size = new Size(320, 142);

            chkGPUWaterfallFFT = new CheckBoxTS();
            chkGPUWaterfallFFT.Name = "chkGPUWaterfallFFTV4";
            chkGPUWaterfallFFT.Text = "GPU FFT";
            chkGPUWaterfallFFT.Location = new Point(12, 22); chkGPUWaterfallFFT.Size = new Size(78, 22);
            chkGPUWaterfallFFT.Checked = Display.WaterfallUseGPU;
            chkGPUWaterfallFFT.CheckedChanged += delegate { Display.WaterfallUseGPU = chkGPUWaterfallFFT.Checked; UpdateGPUWaterfallSetupEnableState(); };
            grpGPUWaterfallFFT.Controls.Add(chkGPUWaterfallFFT);

            grpGPUWaterfallFFT.Controls.Add(GPUWFLabel("FFT size:", 94, 22, 58));
            comboGPUWaterfallFFTSize = GPUWFCombo("comboGPUWaterfallFFTSizeV4", 154, 20, 95,
                "1024", "2048", "4096", "8192", "16384", "32768", "65536", "131072", "262144");
            comboGPUWaterfallFFTSize.Text = Display.GPUWaterfallFFTSize.ToString();
            comboGPUWaterfallFFTSize.SelectedIndexChanged += delegate { int v; if (Int32.TryParse(comboGPUWaterfallFFTSize.Text, out v)) Display.GPUWaterfallFFTSize = v; };
            grpGPUWaterfallFFT.Controls.Add(comboGPUWaterfallFFTSize);

            grpGPUWaterfallFFT.Controls.Add(GPUWFLabel("Overlap %:", 12, 54, 68));
            udGPUWaterfallOverlap = new NumericUpDownTS();
            udGPUWaterfallOverlap.Name = "udGPUWaterfallOverlapV4";
            udGPUWaterfallOverlap.Minimum = 0; udGPUWaterfallOverlap.Maximum = 95; udGPUWaterfallOverlap.Value = (decimal)Display.GPUWaterfallOverlapPercent;
            udGPUWaterfallOverlap.Location = new Point(82, 52); udGPUWaterfallOverlap.Size = new Size(55, 22);
            udGPUWaterfallOverlap.ValueChanged += delegate { Display.GPUWaterfallOverlapPercent = (float)udGPUWaterfallOverlap.Value; };
            grpGPUWaterfallFFT.Controls.Add(udGPUWaterfallOverlap);
            chkGPUWaterfallAutoOverlap = new CheckBoxTS();
            chkGPUWaterfallAutoOverlap.Name = "chkGPUWaterfallAutoOverlapV4";
            chkGPUWaterfallAutoOverlap.Text = "Auto";
            chkGPUWaterfallAutoOverlap.Location = new Point(145, 52); chkGPUWaterfallAutoOverlap.Size = new Size(55, 22);
            chkGPUWaterfallAutoOverlap.Checked = Display.GPUWaterfallAutoOverlap;
            chkGPUWaterfallAutoOverlap.CheckedChanged += delegate { Display.GPUWaterfallAutoOverlap = chkGPUWaterfallAutoOverlap.Checked; UpdateGPUWaterfallSetupEnableState(); };
            grpGPUWaterfallFFT.Controls.Add(chkGPUWaterfallAutoOverlap);

            grpGPUWaterfallFFT.Controls.Add(GPUWFLabel("Window:", 12, 84, 60));
            comboGPUWaterfallWindow = GPUWFCombo("comboGPUWaterfallWindowV4", 82, 82, 118, "Hann", "Hamming", "Blackman-Harris", "Kaiser");
            comboGPUWaterfallWindow.SelectedIndex = Math.Max(0, Math.Min(3, Display.GPUWaterfallWindowType));
            comboGPUWaterfallWindow.SelectedIndexChanged += delegate { Display.GPUWaterfallWindowType = comboGPUWaterfallWindow.SelectedIndex; };
            grpGPUWaterfallFFT.Controls.Add(comboGPUWaterfallWindow);

            grpGPUWaterfallFFT.Controls.Add(GPUWFLabel("Mag:", 12, 114, 40));
            comboGPUWaterfallMagnitudeMode = GPUWFCombo("comboGPUWaterfallMagnitudeModeV4", 52, 112, 100, "Peak Amp", "PSD");
            comboGPUWaterfallMagnitudeMode.SelectedIndex = Math.Max(0, Math.Min(1, Display.GPUWaterfallMagnitudeMode));
            comboGPUWaterfallMagnitudeMode.SelectedIndexChanged += delegate { Display.GPUWaterfallMagnitudeMode = comboGPUWaterfallMagnitudeMode.SelectedIndex; };
            grpGPUWaterfallFFT.Controls.Add(comboGPUWaterfallMagnitudeMode);

            grpGPUWaterfallFFT.Controls.Add(GPUWFLabel("Upsample:", 158, 114, 62));
            comboGPUWaterfallResampling = GPUWFCombo("comboGPUWaterfallResamplingV4", 220, 112, 92, "Linear", "Power Avg", "Peak", "Lanczos");
            comboGPUWaterfallResampling.SelectedIndex = Math.Max(0, Math.Min(3, Display.GPUWaterfallResamplingMode));
            comboGPUWaterfallResampling.SelectedIndexChanged += delegate { Display.GPUWaterfallResamplingMode = comboGPUWaterfallResampling.SelectedIndex; UpdateGPUWaterfallSetupEnableState(); };
            grpGPUWaterfallFFT.Controls.Add(comboGPUWaterfallResampling);

            comboGPUWaterfallLanczos = GPUWFCombo("comboGPUWaterfallLanczosV4", 258, 82, 54, "2", "3", "4");
            comboGPUWaterfallLanczos.Text = Display.GPUWaterfallLanczosWindow.ToString();
            comboGPUWaterfallLanczos.SelectedIndexChanged += delegate { int v; if (Int32.TryParse(comboGPUWaterfallLanczos.Text, out v)) Display.GPUWaterfallLanczosWindow = v; };
            grpGPUWaterfallFFT.Controls.Add(comboGPUWaterfallLanczos);
        }

        private void BuildGPUAccelerationGroup()
        {
            grpGPUAcceleration = new GroupBoxTS();
            grpGPUAcceleration.Name = "grpGPUAccelerationV4";
            grpGPUAcceleration.Text = "GPU Acceleration";
            grpGPUAcceleration.Location = new Point(344, 210);
            grpGPUAcceleration.Size = new Size(334, 142);

            comboGPUAcceleration = GPUWFCombo("comboGPUAccelerationV4", 12, 20, 175, "Auto (GPU preferred)", "Level 0 (CPU)");
            comboGPUAcceleration.SelectedIndex = Display.WaterfallUseGPU ? 0 : 1;
            comboGPUAcceleration.SelectedIndexChanged += delegate
            {
                Display.WaterfallUseGPU = comboGPUAcceleration.SelectedIndex == 0;
                chkGPUWaterfallFFT.Checked = Display.WaterfallUseGPU;
                UpdateGPUWaterfallSetupEnableState();
            };
            grpGPUAcceleration.Controls.Add(comboGPUAcceleration);

            lblGPUAdapter = GPUWFLabel("GPU: detecting...", 12, 47, 310); lblGPUAdapter.Name = "lblGPUAdapterV4";
            lblGPUFeatureLevel = GPUWFLabel("Feature Level: ...", 12, 66, 150); lblGPUFeatureLevel.Name = "lblGPUFeatureLevelV4";
            lblGPUCapabilities = GPUWFLabel("Capabilities: ...", 12, 85, 310); lblGPUCapabilities.Name = "lblGPUCapabilitiesV4";
            lblGPURX1Runtime = GPUWFLabel("RX1: ...", 12, 104, 150); lblGPURX1Runtime.Name = "lblGPURX1RuntimeV4";
            lblGPURX2Runtime = GPUWFLabel("RX2: ...", 165, 104, 157); lblGPURX2Runtime.Name = "lblGPURX2RuntimeV4";
            grpGPUAcceleration.Controls.Add(lblGPUAdapter); grpGPUAcceleration.Controls.Add(lblGPUFeatureLevel);
            grpGPUAcceleration.Controls.Add(lblGPUCapabilities); grpGPUAcceleration.Controls.Add(lblGPURX1Runtime); grpGPUAcceleration.Controls.Add(lblGPURX2Runtime);

            btnGPUTest = new ButtonTS();
            btnGPUTest.Name = "btnGPUTestV4"; btnGPUTest.Text = "Test GPU";
            btnGPUTest.Location = new Point(205, 18); btnGPUTest.Size = new Size(75, 26);
            btnGPUTest.Click += delegate
            {
                bool ok = Display.TestGPUWaterfallHardware();
                MessageBox.Show(ok ? "DirectCompute GPU test: PASS\n" + Display.GPUWaterfallAdapterName : "DirectCompute GPU test: FAIL\nCPU fallback remains available.",
                    "GPU Waterfall", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                UpdateGPUWaterfallDiagnostics();
            };
            grpGPUAcceleration.Controls.Add(btnGPUTest);

            btnGPUDefaults = new ButtonTS();
            btnGPUDefaults.Name = "btnGPUDefaultsV4"; btnGPUDefaults.Text = "EU2AV defaults";
            btnGPUDefaults.Location = new Point(205, 44); btnGPUDefaults.Size = new Size(112, 24);
            btnGPUDefaults.Click += delegate { ApplyGPUWaterfallDefaultsToControls(); };
            grpGPUAcceleration.Controls.Add(btnGPUDefaults);

            btnGPUResetCalibration = new ButtonTS();
            btnGPUResetCalibration.Name = "btnGPUResetCalibrationV4"; btnGPUResetCalibration.Text = "Reset A/B";
            btnGPUResetCalibration.Location = new Point(205, 70); btnGPUResetCalibration.Size = new Size(112, 24);
            btnGPUResetCalibration.Click += delegate { Display.ResetGPUWaterfallCalibration(); UpdateGPUWaterfallDiagnostics(); };
            grpGPUAcceleration.Controls.Add(btnGPUResetCalibration);

            lblGPUDropped = GPUWFLabel("Dropped RX1/RX2: 0 / 0", 12, 123, 190); lblGPUDropped.Name = "lblGPUDroppedV4";
            lblGPUCalibration = GPUWFLabel("A/B: 0.00 / 0.00 dB", 205, 123, 125); lblGPUCalibration.Name = "lblGPUCalibrationV4";
            grpGPUAcceleration.Controls.Add(lblGPUDropped); grpGPUAcceleration.Controls.Add(lblGPUCalibration);
        }

        private void ApplyGPUWaterfallDefaultsToControls()
        {
            Display.ApplyGPUWaterfallEU2AVDefaults();
            if (chkGPUWaterfallFFT != null) chkGPUWaterfallFFT.Checked = true;
            if (comboGPUAcceleration != null) comboGPUAcceleration.SelectedIndex = 0;
            if (comboGPUWaterfallFFTSize != null) comboGPUWaterfallFFTSize.Text = "16384";
            if (udGPUWaterfallOverlap != null) udGPUWaterfallOverlap.Value = 0;
            if (chkGPUWaterfallAutoOverlap != null) chkGPUWaterfallAutoOverlap.Checked = false;
            if (comboGPUWaterfallWindow != null) comboGPUWaterfallWindow.SelectedIndex = 1;
            if (comboGPUWaterfallMagnitudeMode != null) comboGPUWaterfallMagnitudeMode.SelectedIndex = 0;
            if (comboGPUWaterfallResampling != null) comboGPUWaterfallResampling.SelectedIndex = 0;
            if (comboWFNFMode != null) comboWFNFMode.SelectedIndex = 1;
            if (udWFNFLow != null) udWFNFLow.Value = 1;
            if (udWFNFHigh != null) udWFNFHigh.Value = 99;
            if (chkWFNFAutoHigh != null) chkWFNFAutoHigh.Checked = true;
            if (udWFNFAutoHighDb != null) udWFNFAutoHighDb.Value = 0;
            if (comboWFAGCSmooth != null) comboWFAGCSmooth.SelectedIndex = 1;
            if (comboWFDetect != null) comboWFDetect.SelectedIndex = 0;
            if (chkWFZoomAdaptive != null) chkWFZoomAdaptive.Checked = false;
            if (comboWFRender != null) comboWFRender.SelectedIndex = 1;
            if (comboWFToneMap != null) comboWFToneMap.SelectedIndex = 0;
            if (comboWFTemporal != null) comboWFTemporal.SelectedIndex = 0;
            if (tbWFPaletteSharp != null) tbWFPaletteSharp.Value = 0;
            if (tbWFPaletteContrast != null) tbWFPaletteContrast.Value = 0;
            UpdateGPUWaterfallSetupEnableState();
            UpdateGPUWaterfallDiagnostics();
        }

        private void UpdateGPUWaterfallSetupEnableState()
        {
            if (chkGPUWaterfallFFT == null) return;
            bool enabled = chkGPUWaterfallFFT.Checked;
            comboGPUWaterfallFFTSize.Enabled = enabled;
            chkGPUWaterfallAutoOverlap.Enabled = enabled;
            udGPUWaterfallOverlap.Enabled = enabled && !chkGPUWaterfallAutoOverlap.Checked;
            comboGPUWaterfallWindow.Enabled = enabled;
            comboGPUWaterfallMagnitudeMode.Enabled = enabled;
            comboGPUWaterfallResampling.Enabled = enabled;
            comboGPUWaterfallLanczos.Enabled = enabled && comboGPUWaterfallResampling.SelectedIndex == 3;
        }

        private void UpdateGPUWaterfallDiagnostics()
        {
            if (lblGPUAdapter == null) return;

            lblGPUAdapter.Text = "GPU: " + Display.GPUWaterfallAdapterName;
            lblGPUFeatureLevel.Text = "Feature Level: " + Display.GPUWaterfallFeatureLevel;
            lblGPUCapabilities.Text = Display.GPUWaterfallCapabilities;
            lblGPURX1Runtime.Text = "RX1: " + Display.GPUWaterfallRuntimeTextRX1;
            lblGPURX2Runtime.Text = "RX2: " + Display.GPUWaterfallRuntimeTextRX2;
            lblGPUDropped.Text = "Dropped RX1/RX2: " + Display.GPUWaterfallDroppedSamplesRX1 + " / " + Display.GPUWaterfallDroppedSamplesRX2;
            lblGPUCalibration.Text = "A/B: " + Display.GPUWaterfallCalibrationOffsetRX1.ToString("0.00") + " / " + Display.GPUWaterfallCalibrationOffsetRX2.ToString("0.00") + " dB";

            string depth;
            switch (WaterfallEnhancer.Depth)
            {
                case WaterfallEnhancer.ColorDepth.Bit10: depth = "10-bit"; break;
                case WaterfallEnhancer.ColorDepth.Bit16: depth = "16-bit"; break;
                default: depth = "8-bit"; break;
            }
            string resample = comboGPUWaterfallResampling == null || comboGPUWaterfallResampling.Text.Length == 0 ? "Linear" : comboGPUWaterfallResampling.Text;
            string pipeline = (Display.GPUWaterfallRuntimeLevelRX1 >= 2 || Display.GPUWaterfallRuntimeLevelRX2 >= 2) ? "GPU FFT pipeline" : "CPU pipeline";
            if (lblWFPipeline != null) lblWFPipeline.Text = pipeline + ", " + depth + ", " + resample;
        }
    }
}
