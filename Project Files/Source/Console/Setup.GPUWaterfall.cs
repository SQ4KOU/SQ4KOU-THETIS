using System;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis
{
    public partial class Setup
    {

        private TabPage _tpWaterfall;
        private GroupBoxTS wfProGroup;
        private bool _renderQualityItemsUpdating;
        private bool _renderFilterPending;
        private bool _paletteItemsUpdating;
        private static readonly string[] _wfPaletteItemsGPU = new string[5] { "Console 256", "Thermal 256", "DeepBlue 256", "Enhanced 256", "BlackWhite 256" };
        private static readonly string[] _wfPaletteItemsAll = new string[12] { "Console 256", "Thermal 256", "DeepBlue 256", "Enhanced 256", "BlackWhite 256", "Enhanced", "Spectran", "BlackWhite", "LinLog", "LinRad", "LinAuto", "Custom" };
        private LabelTS lblNFProSep; private ComboBoxTS comboNFMode; private LabelTS lblNFMode;
        private NumericUpDownTS udNFLowPct; private LabelTS lblNFLowPct; private NumericUpDownTS udNFHighPct; private LabelTS lblNFHighPct;
        private CheckBoxTS chkAutoHigh; private NumericUpDownTS udAutoHighMargin; private LabelTS lblAutoHighMargin;
        private ComboBoxTS comboAGCSmooth; private LabelTS lblAGCSmooth; private ComboBoxTS comboWFDetector; private LabelTS lblWFDetector;
        private CheckBoxTS chkZoomAdaptive; private LabelTS lblZoomHint; private ComboBoxTS comboToneMap; private LabelTS lblToneMap; private LabelTS lblToneMapSep;
        private ComboBoxTS comboTemporal; private LabelTS lblTemporal; private LabelTS lblGPUSep; private LabelTS lblPalSharp; private TrackBarTS tbPalSharp;
        private LabelTS lblPalSharpVal; private LabelTS lblPalContrast; private TrackBarTS tbPalContrast; private LabelTS lblPalContrastVal;
        private ComboBoxTS comboGPU; private LabelTS lblGPU; private LabelTS lblGPUInfo; private ButtonTS btnTestGPU; private System.Windows.Forms.Timer _gpuStatusTimer;
        private LabelTS lblWaterfallRenderQuality; private ComboBoxTS comboWaterfallRenderQuality; private LabelTS lblWaterfallRenderQualityHint;
        private CheckBoxTS chkGPUWaterfallFFT; private LabelTS lblGPUWaterfallFFTSize; private ComboBoxTS comboGPUWaterfallFFTSize;
        private LabelTS lblGPUWaterfallWindow; private ComboBoxTS comboGPUWaterfallWindow; private LabelTS lblGPUWaterfallKaiserBeta; private NumericUpDownTS udGPUWaterfallKaiserBeta;
        private LabelTS lblGPUWaterfallMagnitudeMode; private ComboBoxTS comboGPUWaterfallMagnitudeMode; private LabelTS lblGPUWaterfallOverlap; private NumericUpDownTS udGPUWaterfallOverlap;
        private CheckBoxTS chkGPUWaterfallAutoOverlap; private LabelTS lblGPUWaterfallEffectiveOverlap; private LabelTS lblGPUWaterfallLanczos; private ComboBoxTS comboGPUWaterfallLanczos;
        private ComboBoxTS comboGPUWaterfallResampling;


        // SQ4KOU test: exact Setup > Display > Waterfall window recovered from
        // Thetis 2.10.3.16 Extended Final. No visual reconstruction.
        private void InitGPUWaterfallSetupUI()
        {
            InitWaterfallTab();
            if (_tpWaterfall != null && grpDisplayDriverEngine != null)
                InitNoiseFloorProControls(grpDisplayDriverEngine, 0);
        }


	private void InitWaterfallTab()
	{
		if (tcDisplay != null && _tpWaterfall == null)
		{
			_tpWaterfall = new TabPage();
			_tpWaterfall.BackColor = SystemColors.Control;
			_tpWaterfall.Location = new Point(4, 22);
			_tpWaterfall.Name = "tpWaterfall";
			_tpWaterfall.Padding = new Padding(3);
			_tpWaterfall.Size = new Size(721, 403);
			_tpWaterfall.Text = "Waterfall";
			_tpWaterfall.AutoScroll = false;
			_tpWaterfall.UseVisualStyleBackColor = true;
			tcDisplay.Controls.Add(_tpWaterfall);
			tcDisplay.Controls.SetChildIndex(_tpWaterfall, 1);
		}
	}


	private void InitNoiseFloorProControls(GroupBox grp, int startY)
	{
		if (comboNFMode != null)
		{
			return;
		}
		wfProGroup = new GroupBoxTS();
		wfProGroup.Text = "Waterfall Pro";
		wfProGroup.Location = new Point(8, 8);
		wfProGroup.Size = new Size(700, 380);
		int num = 22;
		lblNFProSep = new LabelTS();
		lblNFProSep.Text = "── Noise Floor Pro ──";
		lblNFProSep.Location = new Point(12, num);
		lblNFProSep.Size = new Size(160, 16);
		lblNFProSep.ForeColor = Color.SlateGray;
		wfProGroup.Controls.Add(lblNFProSep);
		num += 24;
		lblNFMode = new LabelTS();
		lblNFMode.Text = "NF Mode:";
		lblNFMode.Location = new Point(12, num + 3);
		lblNFMode.Size = new Size(80, 16);
		wfProGroup.Controls.Add(lblNFMode);
		comboNFMode = new ComboBoxTS();
		comboNFMode.Name = "comboNFMode";
		comboNFMode.DropDownStyle = ComboBoxStyle.DropDownList;
		comboNFMode.Items.AddRange(new object[2] { "Average", "Percentile" });
		comboNFMode.Location = new Point(92, num);
		comboNFMode.Size = new Size(100, 21);
		comboNFMode.SelectedIndex = 0;
		comboNFMode.SelectedIndexChanged += comboNFMode_SelectedIndexChanged;
		wfProGroup.Controls.Add(comboNFMode);
		num += 28;
		lblNFLowPct = new LabelTS();
		lblNFLowPct.Text = "NF Low %:";
		lblNFLowPct.Location = new Point(12, num + 3);
		lblNFLowPct.Size = new Size(80, 16);
		wfProGroup.Controls.Add(lblNFLowPct);
		udNFLowPct = new NumericUpDownTS();
		udNFLowPct.Name = "udNFLowPct";
		udNFLowPct.Minimum = 1m;
		udNFLowPct.Maximum = 25m;
		udNFLowPct.DecimalPlaces = 0;
		udNFLowPct.Value = 10m;
		udNFLowPct.Location = new Point(92, num);
		udNFLowPct.Size = new Size(46, 21);
		udNFLowPct.ValueChanged += udNFLowPct_ValueChanged;
		wfProGroup.Controls.Add(udNFLowPct);
		lblNFHighPct = new LabelTS();
		lblNFHighPct.Text = "High %:";
		lblNFHighPct.Location = new Point(144, num + 3);
		lblNFHighPct.Size = new Size(42, 16);
		wfProGroup.Controls.Add(lblNFHighPct);
		udNFHighPct = new NumericUpDownTS();
		udNFHighPct.Name = "udNFHighPct";
		udNFHighPct.Minimum = 90m;
		udNFHighPct.Maximum = 99m;
		udNFHighPct.DecimalPlaces = 0;
		udNFHighPct.Value = 99m;
		udNFHighPct.Location = new Point(188, num);
		udNFHighPct.Size = new Size(42, 21);
		udNFHighPct.ValueChanged += udNFHighPct_ValueChanged;
		wfProGroup.Controls.Add(udNFHighPct);
		num += 28;
		chkAutoHigh = new CheckBoxTS();
		chkAutoHigh.Name = "chkAutoHigh";
		chkAutoHigh.Text = "Auto High";
		chkAutoHigh.AutoSize = true;
		chkAutoHigh.Location = new Point(12, num + 1);
		chkAutoHigh.Checked = false;
		chkAutoHigh.CheckedChanged += chkAutoHigh_CheckedChanged;
		wfProGroup.Controls.Add(chkAutoHigh);
		lblAutoHighMargin = new LabelTS();
		lblAutoHighMargin.Text = "+dB:";
		lblAutoHighMargin.Location = new Point(90, num + 3);
		lblAutoHighMargin.Size = new Size(28, 16);
		wfProGroup.Controls.Add(lblAutoHighMargin);
		udAutoHighMargin = new NumericUpDownTS();
		udAutoHighMargin.Name = "udAutoHighMargin";
		udAutoHighMargin.Minimum = 0m;
		udAutoHighMargin.Maximum = 20m;
		udAutoHighMargin.DecimalPlaces = 0;
		udAutoHighMargin.Value = 6m;
		udAutoHighMargin.Location = new Point(120, num);
		udAutoHighMargin.Size = new Size(40, 21);
		udAutoHighMargin.ValueChanged += udAutoHighMargin_ValueChanged;
		wfProGroup.Controls.Add(udAutoHighMargin);
		num += 28;
		lblAGCSmooth = new LabelTS();
		lblAGCSmooth.Text = "AGC Smooth:";
		lblAGCSmooth.Location = new Point(12, num + 3);
		lblAGCSmooth.Size = new Size(80, 16);
		wfProGroup.Controls.Add(lblAGCSmooth);
		comboAGCSmooth = new ComboBoxTS();
		comboAGCSmooth.Name = "comboAGCSmooth";
		comboAGCSmooth.DropDownStyle = ComboBoxStyle.DropDownList;
		comboAGCSmooth.Items.AddRange(new object[3] { "Slow", "Medium", "Fast" });
		comboAGCSmooth.Location = new Point(92, num);
		comboAGCSmooth.Size = new Size(80, 21);
		comboAGCSmooth.SelectedIndex = 1;
		comboAGCSmooth.SelectedIndexChanged += comboAGCSmooth_SelectedIndexChanged;
		wfProGroup.Controls.Add(comboAGCSmooth);
		num += 28;
		lblWFDetector = new LabelTS();
		lblWFDetector.Text = "WF Detect:";
		lblWFDetector.Location = new Point(12, num + 3);
		lblWFDetector.Size = new Size(80, 16);
		wfProGroup.Controls.Add(lblWFDetector);
		comboWFDetector = new ComboBoxTS();
		comboWFDetector.Name = "comboWFDetector";
		comboWFDetector.DropDownStyle = ComboBoxStyle.DropDownList;
		comboWFDetector.Items.AddRange(new object[3] { "Peak", "Average", "Sample" });
		comboWFDetector.Location = new Point(92, num);
		comboWFDetector.Size = new Size(80, 21);
		comboWFDetector.SelectedIndex = 0;
		comboWFDetector.SelectedIndexChanged += comboWFDetector_SelectedIndexChanged;
		wfProGroup.Controls.Add(comboWFDetector);
		num += 28;
		chkZoomAdaptive = new CheckBoxTS();
		chkZoomAdaptive.Name = "chkZoomAdaptive";
		chkZoomAdaptive.Text = "Zoom Adaptive";
		chkZoomAdaptive.AutoSize = true;
		chkZoomAdaptive.Location = new Point(12, num + 1);
		chkZoomAdaptive.Checked = true;
		chkZoomAdaptive.CheckedChanged += chkZoomAdaptive_CheckedChanged;
		wfProGroup.Controls.Add(chkZoomAdaptive);
		lblZoomHint = new LabelTS();
		lblZoomHint.Text = "(auto-adjusts Tone Map + Temporal)";
		lblZoomHint.Location = new Point(132, num + 3);
		lblZoomHint.Size = new Size(180, 16);
		lblZoomHint.ForeColor = Color.SlateGray;
		wfProGroup.Controls.Add(lblZoomHint);
		num += 36;
		LabelTS labelTS = new LabelTS();
		labelTS.Text = "── GPU FFT Waterfall ──";
		labelTS.Location = new Point(12, num);
		labelTS.Size = new Size(160, 16);
		labelTS.ForeColor = Color.SlateGray;
		wfProGroup.Controls.Add(labelTS);
		num += 24;
		chkGPUWaterfallFFT = new CheckBoxTS();
		chkGPUWaterfallFFT.Name = "chkGPUWaterfallFFT";
		chkGPUWaterfallFFT.Text = "GPU FFT Waterfall";
		chkGPUWaterfallFFT.AutoSize = true;
		chkGPUWaterfallFFT.Location = new Point(12, num);
		chkGPUWaterfallFFT.Visible = false;
		chkGPUWaterfallFFT.CheckedChanged += chkGPUWaterfallFFT_CheckedChanged;
		wfProGroup.Controls.Add(chkGPUWaterfallFFT);
		chkGPUWaterfallFFT.BringToFront();
		num += 24;
		int num2 = num;
		lblGPUWaterfallFFTSize = new LabelTS();
		lblGPUWaterfallFFTSize.Text = "FFT size:";
		lblGPUWaterfallFFTSize.Location = new Point(12, num2 + 3);
		lblGPUWaterfallFFTSize.Size = new Size(52, 16);
		wfProGroup.Controls.Add(lblGPUWaterfallFFTSize);
		lblGPUWaterfallFFTSize.BringToFront();
		comboGPUWaterfallFFTSize = new ComboBoxTS();
		comboGPUWaterfallFFTSize.Name = "comboGPUWaterfallFFTSize";
		comboGPUWaterfallFFTSize.DropDownStyle = ComboBoxStyle.DropDownList;
		comboGPUWaterfallFFTSize.Items.AddRange(new object[9] { "1024", "2048", "4096", "8192", "16384", "32768", "65536", "131072", "262144" });
		comboGPUWaterfallFFTSize.Location = new Point(70, num2);
		comboGPUWaterfallFFTSize.Size = new Size(70, 21);
		comboGPUWaterfallFFTSize.SelectedIndex = 4;
		comboGPUWaterfallFFTSize.SelectedIndexChanged += comboGPUWaterfallFFTSize_SelectedIndexChanged;
		wfProGroup.Controls.Add(comboGPUWaterfallFFTSize);
		comboGPUWaterfallFFTSize.BringToFront();
		lblGPUWaterfallOverlap = new LabelTS();
		lblGPUWaterfallOverlap.Text = "Overlap %:";
		lblGPUWaterfallOverlap.Location = new Point(150, num2 + 3);
		lblGPUWaterfallOverlap.Size = new Size(60, 16);
		wfProGroup.Controls.Add(lblGPUWaterfallOverlap);
		lblGPUWaterfallOverlap.BringToFront();
		udGPUWaterfallOverlap = new NumericUpDownTS();
		udGPUWaterfallOverlap.Name = "udGPUWaterfallOverlap";
		udGPUWaterfallOverlap.Minimum = 0m;
		udGPUWaterfallOverlap.Maximum = 95m;
		udGPUWaterfallOverlap.DecimalPlaces = 0;
		udGPUWaterfallOverlap.Value = 85m;
		udGPUWaterfallOverlap.Increment = 1m;
		udGPUWaterfallOverlap.Location = new Point(212, num2);
		udGPUWaterfallOverlap.Size = new Size(50, 21);
		udGPUWaterfallOverlap.ValueChanged += udGPUWaterfallOverlap_ValueChanged;
		wfProGroup.Controls.Add(udGPUWaterfallOverlap);
		udGPUWaterfallOverlap.BringToFront();
		chkGPUWaterfallAutoOverlap = new CheckBoxTS();
		chkGPUWaterfallAutoOverlap.Name = "chkGPUWaterfallAutoOverlap";
		chkGPUWaterfallAutoOverlap.Text = "Auto";
		chkGPUWaterfallAutoOverlap.AutoSize = true;
		chkGPUWaterfallAutoOverlap.Location = new Point(267, num2 + 2);
		chkGPUWaterfallAutoOverlap.Checked = false;
		chkGPUWaterfallAutoOverlap.CheckedChanged += chkGPUWaterfallAutoOverlap_CheckedChanged;
		wfProGroup.Controls.Add(chkGPUWaterfallAutoOverlap);
		chkGPUWaterfallAutoOverlap.BringToFront();
		lblGPUWaterfallEffectiveOverlap = new LabelTS();
		lblGPUWaterfallEffectiveOverlap.Text = "";
		lblGPUWaterfallEffectiveOverlap.Location = new Point(317, num2 + 4);
		lblGPUWaterfallEffectiveOverlap.Size = new Size(80, 16);
		lblGPUWaterfallEffectiveOverlap.ForeColor = Color.SlateGray;
		wfProGroup.Controls.Add(lblGPUWaterfallEffectiveOverlap);
		lblGPUWaterfallEffectiveOverlap.BringToFront();
		num += 24;
		lblGPUWaterfallWindow = new LabelTS();
		lblGPUWaterfallWindow.Text = "Window:";
		lblGPUWaterfallWindow.Location = new Point(12, num + 3);
		lblGPUWaterfallWindow.Size = new Size(48, 16);
		wfProGroup.Controls.Add(lblGPUWaterfallWindow);
		lblGPUWaterfallWindow.BringToFront();
		comboGPUWaterfallWindow = new ComboBoxTS();
		comboGPUWaterfallWindow.Name = "comboGPUWaterfallWindow";
		comboGPUWaterfallWindow.DropDownStyle = ComboBoxStyle.DropDownList;
		ComboBox.ObjectCollection items = comboGPUWaterfallWindow.Items;
		object[] names = Enum.GetNames(typeof(GPUWaterfallWindowType));
		items.AddRange(names);
		comboGPUWaterfallWindow.Location = new Point(70, num);
		comboGPUWaterfallWindow.Size = new Size(100, 21);
		comboGPUWaterfallWindow.SelectedIndex = 4;
		comboGPUWaterfallWindow.SelectedIndexChanged += comboGPUWaterfallWindow_SelectedIndexChanged;
		wfProGroup.Controls.Add(comboGPUWaterfallWindow);
		comboGPUWaterfallWindow.BringToFront();
		lblGPUWaterfallKaiserBeta = new LabelTS();
		lblGPUWaterfallKaiserBeta.Text = "Beta:";
		lblGPUWaterfallKaiserBeta.Location = new Point(176, num + 3);
		lblGPUWaterfallKaiserBeta.Size = new Size(34, 16);
		lblGPUWaterfallKaiserBeta.Visible = false;
		wfProGroup.Controls.Add(lblGPUWaterfallKaiserBeta);
		lblGPUWaterfallKaiserBeta.BringToFront();
		udGPUWaterfallKaiserBeta = new NumericUpDownTS();
		udGPUWaterfallKaiserBeta.Name = "udGPUWaterfallKaiserBeta";
		udGPUWaterfallKaiserBeta.Minimum = 0m;
		udGPUWaterfallKaiserBeta.Maximum = 20m;
		udGPUWaterfallKaiserBeta.DecimalPlaces = 1;
		udGPUWaterfallKaiserBeta.Value = 6.0m;
		udGPUWaterfallKaiserBeta.Increment = 0.5m;
		udGPUWaterfallKaiserBeta.Location = new Point(210, num);
		udGPUWaterfallKaiserBeta.Size = new Size(50, 21);
		udGPUWaterfallKaiserBeta.ValueChanged += udGPUWaterfallKaiserBeta_ValueChanged;
		udGPUWaterfallKaiserBeta.Visible = false;
		wfProGroup.Controls.Add(udGPUWaterfallKaiserBeta);
		udGPUWaterfallKaiserBeta.BringToFront();
		num += 24;
		lblGPUWaterfallMagnitudeMode = new LabelTS();
		lblGPUWaterfallMagnitudeMode.Text = "Mag mode:";
		lblGPUWaterfallMagnitudeMode.Location = new Point(12, num + 3);
		lblGPUWaterfallMagnitudeMode.Size = new Size(58, 16);
		wfProGroup.Controls.Add(lblGPUWaterfallMagnitudeMode);
		lblGPUWaterfallMagnitudeMode.BringToFront();
		comboGPUWaterfallMagnitudeMode = new ComboBoxTS();
		comboGPUWaterfallMagnitudeMode.Name = "comboGPUWaterfallMagnitudeMode";
		comboGPUWaterfallMagnitudeMode.DropDownStyle = ComboBoxStyle.DropDownList;
		comboGPUWaterfallMagnitudeMode.Items.AddRange(new object[3] { "Peak Amp", "Avg Power", "Peak Power" });
		comboGPUWaterfallMagnitudeMode.Location = new Point(74, num);
		comboGPUWaterfallMagnitudeMode.Size = new Size(96, 21);
		comboGPUWaterfallMagnitudeMode.SelectedIndex = 2;
		comboGPUWaterfallMagnitudeMode.SelectedIndexChanged += comboGPUWaterfallMagnitudeMode_SelectedIndexChanged;
		wfProGroup.Controls.Add(comboGPUWaterfallMagnitudeMode);
		comboGPUWaterfallMagnitudeMode.BringToFront();
		lblGPUWaterfallLanczos = new LabelTS();
		lblGPUWaterfallLanczos.Text = "Upsample:";
		lblGPUWaterfallLanczos.Location = new Point(180, num + 3);
		lblGPUWaterfallLanczos.Size = new Size(55, 16);
		wfProGroup.Controls.Add(lblGPUWaterfallLanczos);
		lblGPUWaterfallLanczos.BringToFront();
		comboGPUWaterfallLanczos = new ComboBoxTS();
		comboGPUWaterfallLanczos.Name = "comboGPUWaterfallLanczos";
		comboGPUWaterfallLanczos.DropDownStyle = ComboBoxStyle.DropDownList;
		comboGPUWaterfallLanczos.Items.AddRange(new object[4] { "Linear", "Lanczos 2", "Lanczos 3", "Lanczos 4" });
		comboGPUWaterfallLanczos.Location = new Point(237, num);
		comboGPUWaterfallLanczos.Size = new Size(80, 21);
		comboGPUWaterfallLanczos.SelectedIndex = 2;
		comboGPUWaterfallLanczos.SelectedIndexChanged += comboGPUWaterfallLanczos_SelectedIndexChanged;
		wfProGroup.Controls.Add(comboGPUWaterfallLanczos);
		comboGPUWaterfallLanczos.BringToFront();
		int num3 = 22;
		lblWaterfallRenderQuality = new LabelTS();
		lblWaterfallRenderQuality.Text = "Render:";
		lblWaterfallRenderQuality.Location = new Point(340, num3 + 3);
		lblWaterfallRenderQuality.Size = new Size(44, 16);
		wfProGroup.Controls.Add(lblWaterfallRenderQuality);
		lblWaterfallRenderQuality.BringToFront();
		comboWaterfallRenderQuality = new ComboBoxTS();
		comboWaterfallRenderQuality.Name = "comboWaterfallRenderQuality";
		comboWaterfallRenderQuality.DropDownStyle = ComboBoxStyle.DropDownList;
		comboWaterfallRenderQuality.Items.AddRange(new object[3] { "Low", "Medium", "High" });
		comboWaterfallRenderQuality.Location = new Point(388, num3);
		comboWaterfallRenderQuality.Size = new Size(90, 21);
		comboWaterfallRenderQuality.SelectedIndex = 2;
		comboWaterfallRenderQuality.SelectedIndexChanged += comboWaterfallRenderQuality_SelectedIndexChanged;
		wfProGroup.Controls.Add(comboWaterfallRenderQuality);
		comboWaterfallRenderQuality.BringToFront();
		lblWaterfallRenderQualityHint = new LabelTS();
		lblWaterfallRenderQualityHint.Text = "GPU pipeline";
		lblWaterfallRenderQualityHint.Location = new Point(482, num3 + 3);
		lblWaterfallRenderQualityHint.Size = new Size(150, 16);
		lblWaterfallRenderQualityHint.ForeColor = Color.SlateGray;
		wfProGroup.Controls.Add(lblWaterfallRenderQualityHint);
		lblWaterfallRenderQualityHint.BringToFront();
		num3 += 24;
		lblToneMapSep = new LabelTS();
		lblToneMapSep.Text = "── Enhancement ──";
		lblToneMapSep.Location = new Point(340, num3);
		lblToneMapSep.Size = new Size(120, 16);
		lblToneMapSep.ForeColor = Color.SlateGray;
		wfProGroup.Controls.Add(lblToneMapSep);
		num3 += 24;
		lblToneMap = new LabelTS();
		lblToneMap.Text = "Tone Map:";
		lblToneMap.Location = new Point(340, num3 + 3);
		lblToneMap.Size = new Size(80, 16);
		wfProGroup.Controls.Add(lblToneMap);
		comboToneMap = new ComboBoxTS();
		comboToneMap.Name = "comboToneMap";
		comboToneMap.DropDownStyle = ComboBoxStyle.DropDownList;
		comboToneMap.Items.AddRange(new object[3] { "Off", "Reinhard", "ACES" });
		comboToneMap.Location = new Point(420, num3);
		comboToneMap.Size = new Size(90, 21);
		comboToneMap.SelectedIndex = 0;
		comboToneMap.SelectedIndexChanged += comboToneMap_SelectedIndexChanged;
		wfProGroup.Controls.Add(comboToneMap);
		num3 += 28;
		lblTemporal = new LabelTS();
		lblTemporal.Text = "Temporal:";
		lblTemporal.Location = new Point(340, num3 + 3);
		lblTemporal.Size = new Size(80, 16);
		wfProGroup.Controls.Add(lblTemporal);
		comboTemporal = new ComboBoxTS();
		comboTemporal.Name = "comboTemporal";
		comboTemporal.DropDownStyle = ComboBoxStyle.DropDownList;
		comboTemporal.Items.AddRange(new object[4] { "Off", "Light", "Medium", "Strong" });
		comboTemporal.Location = new Point(420, num3);
		comboTemporal.Size = new Size(90, 21);
		comboTemporal.SelectedIndex = 0;
		comboTemporal.SelectedIndexChanged += comboTemporal_SelectedIndexChanged;
		wfProGroup.Controls.Add(comboTemporal);
		num3 += 36;
		lblPalSharp = new LabelTS();
		lblPalSharp.Text = "Pal Sharp:";
		lblPalSharp.Location = new Point(340, num3 + 3);
		lblPalSharp.Size = new Size(80, 16);
		wfProGroup.Controls.Add(lblPalSharp);
		tbPalSharp = new TrackBarTS();
		tbPalSharp.Name = "tbPalSharp";
		tbPalSharp.Minimum = 0;
		tbPalSharp.Maximum = 150;
		tbPalSharp.Value = 0;
		tbPalSharp.TickFrequency = 25;
		tbPalSharp.Location = new Point(420, num3 - 2);
		tbPalSharp.Size = new Size(140, 28);
		tbPalSharp.Scroll += tbPalSharp_Scroll;
		wfProGroup.Controls.Add(tbPalSharp);
		lblPalSharpVal = new LabelTS();
		lblPalSharpVal.Text = "0";
		lblPalSharpVal.Location = new Point(564, num3 + 3);
		lblPalSharpVal.Size = new Size(30, 16);
		wfProGroup.Controls.Add(lblPalSharpVal);
		num3 += 47;
		lblPalContrast = new LabelTS();
		lblPalContrast.Text = "Pal Contrast:";
		lblPalContrast.Location = new Point(340, num3 + 3);
		lblPalContrast.Size = new Size(80, 16);
		wfProGroup.Controls.Add(lblPalContrast);
		tbPalContrast = new TrackBarTS();
		tbPalContrast.Name = "tbPalContrast";
		tbPalContrast.Minimum = 0;
		tbPalContrast.Maximum = 150;
		tbPalContrast.Value = 0;
		tbPalContrast.TickFrequency = 25;
		tbPalContrast.Location = new Point(420, num3 - 2);
		tbPalContrast.Size = new Size(140, 28);
		tbPalContrast.Scroll += tbPalContrast_Scroll;
		wfProGroup.Controls.Add(tbPalContrast);
		lblPalContrastVal = new LabelTS();
		lblPalContrastVal.Text = "0";
		lblPalContrastVal.Location = new Point(564, num3 + 3);
		lblPalContrastVal.Size = new Size(30, 16);
		wfProGroup.Controls.Add(lblPalContrastVal);
		num3 += 69;
		lblGPUSep = new LabelTS();
		lblGPUSep.Text = "── GPU Acceleration ──";
		lblGPUSep.Location = new Point(340, num3);
		lblGPUSep.Size = new Size(160, 16);
		lblGPUSep.ForeColor = Color.SlateGray;
		wfProGroup.Controls.Add(lblGPUSep);
		num3 += 24;
		lblGPU = new LabelTS();
		lblGPU.Text = "Mode:";
		lblGPU.Location = new Point(340, num3 + 3);
		lblGPU.Size = new Size(80, 16);
		wfProGroup.Controls.Add(lblGPU);
		comboGPU = new ComboBoxTS();
		comboGPU.Name = "comboGPU";
		comboGPU.DropDownStyle = ComboBoxStyle.DropDownList;
		comboGPU.Items.AddRange(new object[4] { "Auto", "Level 0 (CPU)", "Level 1 (Basic)", "Level 2 (Advanced)" });
		comboGPU.Location = new Point(420, num3);
		comboGPU.Size = new Size(130, 21);
		comboGPU.SelectedIndex = 0;
		comboGPU.SelectedIndexChanged += comboGPU_SelectedIndexChanged;
		wfProGroup.Controls.Add(comboGPU);
		if (_gpuStatusTimer == null)
		{
			_gpuStatusTimer = new System.Windows.Forms.Timer();
			_gpuStatusTimer.Interval = 2000;
			_gpuStatusTimer.Tick += delegate
			{
				try
				{
					UpdateGPUInfoLabel();
				}
				catch
				{
				}
				try
				{
					UpdateWaterfallPaletteItems(Display.GPUEffectsEnabled);
				}
				catch
				{
				}
			};
			_gpuStatusTimer.Start();
		}
		num3 += 28;
		lblGPUInfo = new LabelTS();
		lblGPUInfo.Text = "Detecting...";
		lblGPUInfo.Location = new Point(340, num3);
		lblGPUInfo.Size = new Size(330, 36);
		lblGPUInfo.ForeColor = Color.SlateGray;
		wfProGroup.Controls.Add(lblGPUInfo);
		num3 += 40;
		btnTestGPU = new ButtonTS();
		btnTestGPU.Name = "btnTestGPU";
		btnTestGPU.Text = "Test GPU";
		btnTestGPU.Location = new Point(340, num3);
		btnTestGPU.Size = new Size(80, 24);
		btnTestGPU.Click += btnTestGPU_Click;
		wfProGroup.Controls.Add(btnTestGPU);
		if (chkGPUWaterfallFFT != null)
		{
			chkGPUWaterfallFFT.Checked = Display.GPUWaterfallPipelineEnabled;
		}
		if (comboGPUWaterfallFFTSize != null)
		{
			string value = Display.GPUWaterfallFFTSize.ToString();
			int num4 = comboGPUWaterfallFFTSize.Items.IndexOf(value);
			if (num4 >= 0)
			{
				comboGPUWaterfallFFTSize.SelectedIndex = num4;
			}
		}
		if (comboGPUWaterfallWindow != null)
		{
			comboGPUWaterfallWindow.SelectedIndex = (int)Display.GPUWaterfallWindowType;
			UpdateKaiserBetaVisibility();
		}
		if (udGPUWaterfallKaiserBeta != null)
		{
			udGPUWaterfallKaiserBeta.Value = (decimal)Display.GPUWaterfallKaiserBeta;
		}
		if (comboGPUWaterfallMagnitudeMode != null)
		{
			comboGPUWaterfallMagnitudeMode.SelectedIndex = (int)Display.GPUWaterfallMagnitudeMode;
		}
		if (udGPUWaterfallOverlap != null)
		{
			udGPUWaterfallOverlap.Value = Display.GPUWaterfallOverlapPercent;
		}
		if (chkGPUWaterfallAutoOverlap != null)
		{
			chkGPUWaterfallAutoOverlap.Checked = Display.GPUWaterfallAutoOverlap;
		}
		if (comboGPUWaterfallLanczos != null)
		{
			int gPUWaterfallLanczosWindow = Display.GPUWaterfallLanczosWindow;
			int num5 = ((gPUWaterfallLanczosWindow > 1) ? (gPUWaterfallLanczosWindow - 1) : 0);
			if (num5 >= 0 && num5 < comboGPUWaterfallLanczos.Items.Count)
			{
				comboGPUWaterfallLanczos.SelectedIndex = num5;
			}
		}
		if (comboGPUWaterfallResampling != null)
		{
			int gPUWaterfallResamplingMode = (int)Display.GPUWaterfallResamplingMode;
			if (gPUWaterfallResamplingMode >= 0 && gPUWaterfallResamplingMode < comboGPUWaterfallResampling.Items.Count)
			{
				comboGPUWaterfallResampling.SelectedIndex = gPUWaterfallResamplingMode;
			}
		}
		try
		{
			if (_tpWaterfall != null)
			{
				_tpWaterfall.Controls.Add(wfProGroup);
				wfProGroup.Location = new Point(8, 8);
				wfProGroup.BringToFront();
			}
			else
			{
				grp.Parent.Controls.Add(wfProGroup);
				wfProGroup.BringToFront();
			}
		}
		catch (Exception ex)
		{
			LogTool.AddLogEntry("Waterfall Pro group reparent failed: " + ex.Message, "SETUP");
			grp.Parent.Controls.Add(wfProGroup);
			wfProGroup.BringToFront();
		}
		Display.GPUWaterfallEffectiveOverlapChanged -= OnGPUWaterfallEffectiveOverlapChanged;
		Display.GPUWaterfallEffectiveOverlapChanged += OnGPUWaterfallEffectiveOverlapChanged;
	}


	private void UpdateNFLowHighEnabledState()
	{
		bool enabled = comboNFMode != null && comboNFMode.SelectedIndex == 1;
		if (udNFLowPct != null)
		{
			udNFLowPct.Enabled = enabled;
		}
		if (udNFHighPct != null)
		{
			udNFHighPct.Enabled = enabled;
		}
	}


	private void UpdateGPUInfoLabel()
	{
		try
		{
			string text = Display.GPUName ?? "unknown";
			int gPUDetectionLevel = Display.GPUDetectionLevel;
			string text2 = GPUDetector.FeaturesList ?? "";
			if (string.IsNullOrEmpty(text2))
			{
				text2 = "(none detected)";
			}
			string text3 = ((!GPUDetector.HasDeviceContext) ? "Pending... (connect radio to detect)" : ((!Display.GPUEffectsEnabled) ? "INACTIVE (CPU mode)" : "ACTIVE ✓ (GPU post-processing ON)"));
			lblGPUInfo.Text = text + "\nLevel " + gPUDetectionLevel + ": " + text2 + "\n" + text3;
		}
		catch
		{
			lblGPUInfo.Text = "(info unavailable)";
		}
	}


	private void ApplyGPUSelection(int selectedIndex)
	{
		int gPUDetectionLevel = Display.GPUDetectionLevel;
		int num = selectedIndex switch
		{
			1 => 0, 
			2 => 1, 
			3 => 2, 
			_ => gPUDetectionLevel, 
		};
		bool hasDeviceContext = GPUDetector.HasDeviceContext;
		Display.GPUEffectsEnabled = num >= 1 && GPUDetector.HasBuiltInEffects;
		Display.AutoEnableGPU = num >= 1;
		if (hasDeviceContext || selectedIndex != 0)
		{
			UpdateWaterfallRenderQualityItems(num);
		}
		else
		{
			_renderFilterPending = true;
		}
		UpdateWaterfallPaletteItems(num >= 1);
		SyncGPUWaterfallPipelineEnabled(num);
		if (hasDeviceContext)
		{
			if (num >= 2 && !GPUDetector.HasCustomShaders)
			{
				MessageBox.Show("Custom HLSL shaders (Level 2) are not available in this build.\nUsing Level 1 (Built-in Effects) instead.", "GPU Acceleration", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			}
			else if (num >= 1 && !GPUDetector.HasBuiltInEffects)
			{
				MessageBox.Show("Built-in D2D Effects are not available on this system.\nUsing CPU post-processing (Level 0).", "GPU Acceleration", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			}
		}
	}


	private void UpdateWaterfallRenderQualityItems(int target)
	{
		if (comboWaterfallRenderQuality == null)
		{
			return;
		}
		_renderFilterPending = false;
		_renderQualityItemsUpdating = true;
		try
		{
			bool flag = target >= 1;
			string value = comboWaterfallRenderQuality.SelectedItem as string;
			if (string.IsNullOrEmpty(value))
			{
				value = (flag ? "High" : "Medium");
			}
			comboWaterfallRenderQuality.BeginUpdate();
			comboWaterfallRenderQuality.Items.Clear();
			if (flag)
			{
				comboWaterfallRenderQuality.Items.Add("High");
			}
			else
			{
				comboWaterfallRenderQuality.Items.Add("Low");
				comboWaterfallRenderQuality.Items.Add("Medium");
			}
			int num = comboWaterfallRenderQuality.Items.IndexOf(value);
			if (num < 0)
			{
				num = ((!flag) ? comboWaterfallRenderQuality.Items.IndexOf("Medium") : 0);
			}
			comboWaterfallRenderQuality.SelectedIndex = num;
			comboWaterfallRenderQuality.EndUpdate();
		}
		finally
		{
			_renderQualityItemsUpdating = false;
		}
		Display.WaterfallQuality = SelectedRenderQuality();
		SyncGPUWaterfallPipelineEnabled(target);
		UpdateWaterfallRenderQualityHint();
	}


	private void UpdateOnePaletteCombo(ComboBoxTS combo, bool gpuMode)
	{
		if (combo == null)
		{
			return;
		}
		string[] array = (gpuMode ? _wfPaletteItemsGPU : _wfPaletteItemsAll);
		if (combo.Items.Count == array.Length)
		{
			bool flag = true;
			for (int i = 0; i < array.Length; i++)
			{
				if (!string.Equals(combo.Items[i] as string, array[i]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				if (gpuMode && Array.IndexOf(_wfPaletteItemsGPU, combo.Text) < 0)
				{
					combo.Text = "Console 256";
				}
				return;
			}
		}
		string value = combo.Text;
		combo.Items.Clear();
		ComboBox.ObjectCollection items = combo.Items;
		object[] items2 = array;
		items.AddRange(items2);
		if (Array.IndexOf(array, value) >= 0)
		{
			combo.Text = value;
		}
		else
		{
			combo.Text = "Console 256";
		}
	}


	private void UpdateWaterfallPaletteItems(bool gpuMode)
	{
		if (_paletteItemsUpdating)
		{
			return;
		}
		_paletteItemsUpdating = true;
		try
		{
			UpdateOnePaletteCombo(comboColorPalette, gpuMode);
			UpdateOnePaletteCombo(comboRX2ColorPalette, gpuMode);
			UpdateOnePaletteCombo(comboColorPalette_tx, gpuMode);
		}
		finally
		{
			_paletteItemsUpdating = false;
		}
	}


	private void SyncGPUWaterfallPipelineEnabled(int target)
	{
		Display.GPUWaterfallPipelineEnabled = target >= 1 && Display.WaterfallQuality == Display.WaterfallRenderQuality.High;
	}


	private int CurrentGPUTarget()
	{
		if (comboGPU == null)
		{
			return 0;
		}
		return comboGPU.SelectedIndex switch
		{
			1 => 0, 
			2 => 1, 
			3 => 2, 
			_ => Display.GPUDetectionLevel, 
		};
	}


	private Display.WaterfallRenderQuality SelectedRenderQuality()
	{
		return (comboWaterfallRenderQuality?.SelectedItem as string) switch
		{
			"Low" => Display.WaterfallRenderQuality.Low, 
			"Medium" => Display.WaterfallRenderQuality.Medium, 
			"High" => Display.WaterfallRenderQuality.High, 
			_ => Display.WaterfallRenderQuality.High, 
		};
	}


	private void UpdateWaterfallRenderQualityHint()
	{
		if (lblWaterfallRenderQualityHint != null && comboWaterfallRenderQuality != null)
		{
			bool flag = WaterfallEnhancer.Depth == WaterfallEnhancer.ColorDepth.Bit16;
			bool gPUEffectsEnabled = Display.GPUEffectsEnabled;
			switch (comboWaterfallRenderQuality.SelectedItem as string)
			{
			case "Low":
				lblWaterfallRenderQualityHint.Text = (gPUEffectsEnabled ? "GPU pipeline, NN" : "CPU pipeline, 8-bit, NN");
				break;
			case "Medium":
				lblWaterfallRenderQualityHint.Text = (gPUEffectsEnabled ? "GPU pipeline, Linear" : "CPU pipeline, 8-bit, Linear");
				break;
			case "High":
				lblWaterfallRenderQualityHint.Text = (flag ? "GPU pipeline + FFT, 16-bit" : "GPU pipeline + FFT, 8-bit");
				break;
			default:
				lblWaterfallRenderQualityHint.Text = "";
				break;
			}
		}
	}


	private void UpdateKaiserBetaVisibility()
	{
		if (lblGPUWaterfallKaiserBeta != null && udGPUWaterfallKaiserBeta != null && comboGPUWaterfallWindow.SelectedItem != null)
		{
			bool visible = comboGPUWaterfallWindow.SelectedItem.ToString() == GPUWaterfallWindowType.Kaiser.ToString();
			lblGPUWaterfallKaiserBeta.Visible = visible;
			udGPUWaterfallKaiserBeta.Visible = visible;
		}
	}


	private void btnTestGPU_Click(object sender, EventArgs e)
	{
		UpdateGPUInfoLabel();
		MessageBox.Show("GPU: " + (Display.GPUName ?? "unknown") + "\nDetected Level: " + Display.GPUDetectionLevel + "\nFeatures: " + (GPUDetector.FeaturesList ?? "(none)") + "\nHasDeviceContext: " + GPUDetector.HasDeviceContext + "\nHasBuiltInEffects: " + GPUDetector.HasBuiltInEffects + "\nHasCustomShaders: " + GPUDetector.HasCustomShaders + "\n\nDirect3D11 / DirectCompute detection result.", "GPU Test", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
	}


	private void comboGPU_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!initializing)
		{
			ApplyGPUSelection(comboGPU.SelectedIndex);
		}
	}


	private void comboNFMode_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!initializing)
		{
			Display.NFMode = ((comboNFMode.SelectedIndex == 1) ? NoiseFloorPro.DetectionMode.Percentile : NoiseFloorPro.DetectionMode.Average);
			UpdateNFLowHighEnabledState();
		}
	}


	private void udNFLowPct_ValueChanged(object sender, EventArgs e)
	{
		if (!initializing)
		{
			Display.NFLowPct = (float)udNFLowPct.Value;
		}
	}


	private void udNFHighPct_ValueChanged(object sender, EventArgs e)
	{
		if (!initializing)
		{
			Display.NFHighPct = (float)udNFHighPct.Value;
		}
	}


	private void chkAutoHigh_CheckedChanged(object sender, EventArgs e)
	{
		if (!initializing)
		{
			Display.AutoHighEnabledRX1 = chkAutoHigh.Checked;
			Display.AutoHighEnabledRX2 = chkAutoHigh.Checked;
		}
	}


	private void udAutoHighMargin_ValueChanged(object sender, EventArgs e)
	{
		if (!initializing)
		{
			Display.AutoHighMarginDb = (float)udAutoHighMargin.Value;
		}
	}


	private void comboAGCSmooth_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!initializing)
		{
			float waterfallAgcSmoothing = 0.4f;
			switch (comboAGCSmooth.SelectedIndex)
			{
			case 0:
				waterfallAgcSmoothing = 0.2f;
				break;
			case 1:
				waterfallAgcSmoothing = 0.4f;
				break;
			case 2:
				waterfallAgcSmoothing = 0.6f;
				break;
			}
			Display.WaterfallAgcSmoothing = waterfallAgcSmoothing;
		}
	}


	private void comboWFDetector_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!initializing)
		{
			int num = comboWFDetector.SelectedIndex switch
			{
				1 => 2, 
				2 => 3, 
				_ => 0, 
			};
			if (comboDispWFDetector != null && comboDispWFDetector.SelectedIndex != num)
			{
				comboDispWFDetector.SelectedIndex = num;
			}
			if (comboRX2DispWFDetector != null && comboRX2DispWFDetector.SelectedIndex != num)
			{
				comboRX2DispWFDetector.SelectedIndex = num;
			}
			if (console != null && console.specRX != null)
			{
				console.specRX.GetSpecRX(0).DetTypeWF = num;
				console.specRX.GetSpecRX(1).DetTypeWF = num;
			}
		}
	}


	private void chkZoomAdaptive_CheckedChanged(object sender, EventArgs e)
	{
		if (!initializing)
		{
			bool flag = (Display.ZoomAdaptiveEnabled = chkZoomAdaptive.Checked);
			if (comboToneMap != null)
			{
				comboToneMap.Enabled = !flag;
			}
			if (comboTemporal != null)
			{
				comboTemporal.Enabled = !flag;
			}
		}
	}


	private void comboToneMap_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!initializing)
		{
			WaterfallEnhancer.SetToneMap(comboToneMap.SelectedIndex switch
			{
				1 => WaterfallEnhancer.ToneMapMode.Reinhard, 
				2 => WaterfallEnhancer.ToneMapMode.ACES, 
				_ => WaterfallEnhancer.ToneMapMode.None, 
			});
		}
	}


	private void comboTemporal_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!initializing)
		{
			float num = (Display.TemporalStrength = comboTemporal.SelectedIndex switch
			{
				1 => 0.15f, 
				2 => 0.3f, 
				3 => 0.45f, 
				_ => 0f, 
			});
			Display.TemporalEnabled = num > 0f;
		}
	}


	private void tbPalSharp_Scroll(object sender, EventArgs e)
	{
		float paletteSharpness = (float)tbPalSharp.Value / 100f;
		lblPalSharpVal.Text = tbPalSharp.Value.ToString();
		WaterfallEnhancer.SetPaletteSharpness(paletteSharpness);
	}


	private void tbPalContrast_Scroll(object sender, EventArgs e)
	{
		float paletteContrast = (float)tbPalContrast.Value / 100f;
		lblPalContrastVal.Text = tbPalContrast.Value.ToString();
		WaterfallEnhancer.SetPaletteContrast(paletteContrast);
	}


	private void comboWaterfallRenderQuality_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!initializing && !_renderQualityItemsUpdating && comboWaterfallRenderQuality != null && comboWaterfallRenderQuality.SelectedIndex >= 0)
		{
			Display.WaterfallQuality = SelectedRenderQuality();
			SyncGPUWaterfallPipelineEnabled(CurrentGPUTarget());
			UpdateWaterfallRenderQualityHint();
		}
	}


	private void chkGPUWaterfallFFT_CheckedChanged(object sender, EventArgs e)
	{
		if (!initializing)
		{
			Display.GPUWaterfallPipelineEnabled = chkGPUWaterfallFFT.Checked;
		}
	}


	private void comboGPUWaterfallFFTSize_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!initializing && comboGPUWaterfallFFTSize.SelectedItem != null && int.TryParse(comboGPUWaterfallFFTSize.SelectedItem.ToString(), out var result))
		{
			Display.GPUWaterfallFFTSize = result;
		}
	}


	private void comboGPUWaterfallWindow_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!initializing && comboGPUWaterfallWindow.SelectedItem != null)
		{
			if (Enum.TryParse<GPUWaterfallWindowType>(comboGPUWaterfallWindow.SelectedItem.ToString(), out var result))
			{
				Display.GPUWaterfallWindowType = (int)result;
			}
			UpdateKaiserBetaVisibility();
		}
	}


	private void udGPUWaterfallKaiserBeta_ValueChanged(object sender, EventArgs e)
	{
		if (!initializing)
		{
			Display.GPUWaterfallKaiserBeta = (float)udGPUWaterfallKaiserBeta.Value;
		}
	}


	private void comboGPUWaterfallMagnitudeMode_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!initializing && comboGPUWaterfallMagnitudeMode.SelectedItem != null)
		{
			Display.GPUWaterfallMagnitudeMode = comboGPUWaterfallMagnitudeMode.SelectedIndex;
		}
	}


	private void udGPUWaterfallOverlap_ValueChanged(object sender, EventArgs e)
	{
		if (!initializing)
		{
			Display.GPUWaterfallOverlapPercent = (int)udGPUWaterfallOverlap.Value;
		}
	}


	private void chkGPUWaterfallAutoOverlap_CheckedChanged(object sender, EventArgs e)
	{
		if (!initializing)
		{
			Display.GPUWaterfallAutoOverlap = chkGPUWaterfallAutoOverlap.Checked;
		}
	}


	private void comboGPUWaterfallLanczos_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!initializing && comboGPUWaterfallLanczos != null && comboGPUWaterfallLanczos.SelectedIndex >= 0)
		{
			int selectedIndex = comboGPUWaterfallLanczos.SelectedIndex;
			Display.GPUWaterfallLanczosWindow = ((selectedIndex != 0) ? (selectedIndex + 1) : 0);
		}
	}


	private void comboGPUWaterfallResampling_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!initializing && comboGPUWaterfallResampling != null && comboGPUWaterfallResampling.SelectedIndex >= 0)
		{
			Display.GPUWaterfallResamplingMode = comboGPUWaterfallResampling.SelectedIndex;
		}
	}


	private void OnGPUWaterfallEffectiveOverlapChanged(int rx, double overlap)
	{
		if (lblGPUWaterfallEffectiveOverlap == null || lblGPUWaterfallEffectiveOverlap.IsDisposed || !base.IsHandleCreated || base.IsDisposed)
		{
			return;
		}
		if (base.InvokeRequired)
		{
			try
			{
				BeginInvoke((Action)delegate
				{
					OnGPUWaterfallEffectiveOverlapChanged(rx, overlap);
				});
				return;
			}
			catch
			{
				return;
			}
		}
		try
		{
			lblGPUWaterfallEffectiveOverlap.Text = $"Eff: {overlap * 100.0:F0}%";
		}
		catch
		{
		}
	}
    }
}
