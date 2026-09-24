using System;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis
{
    public partial class Setup
    {
        private TabPage _tpWaterfall;
        private GroupBoxTS wfProGroup;
        private System.Windows.Forms.Timer _gpuStatusTimer;

        private ComboBoxTS comboNFMode, comboAGCSmooth, comboWFDetector;
        private NumericUpDownTS udNFLowPct, udNFHighPct, udAutoHighMargin;
        private CheckBoxTS chkAutoHigh, chkZoomAdaptive;

        private CheckBoxTS chkGPUWaterfallFFT, chkGPUWaterfallAutoOverlap;
        private ComboBoxTS comboGPUWaterfallFFTSize, comboGPUWaterfallWindow;
        private NumericUpDownTS udGPUWaterfallKaiserBeta, udGPUWaterfallOverlap;
        private ComboBoxTS comboGPUWaterfallMagnitudeMode, comboGPUWaterfallResampling, comboGPUWaterfallLanczos;
        private LabelTS lblGPUWaterfallKaiserBeta, lblGPUWaterfallLanczos, lblGPUWaterfallEffectiveOverlap;

        private ComboBoxTS comboWFPaletteRX1, comboWFPaletteRX2, comboWFPaletteTX;
        private bool _paletteSyncing;
        private ComboBoxTS comboToneMap, comboTemporal;
        private TrackBarTS tbPalSharp, tbPalContrast;
        private LabelTS lblPalSharpVal, lblPalContrastVal;

        private CheckBoxTS chkGPUColorCompute;
        private LabelTS lblGPUInfo;
        private ButtonTS btnTestGPU;

        private LabelTS L(string text, int x, int y, int w = 90)
        {
            LabelTS l = new LabelTS();
            l.Text = text; l.Location = new Point(x, y); l.Size = new Size(w, 16);
            return l;
        }

        private void InitGPUWaterfallSetupUI()
        {
            if (tcDisplay == null || _tpWaterfall != null) return;

            _tpWaterfall = new TabPage();
            _tpWaterfall.BackColor = SystemColors.Control;
            _tpWaterfall.Name = "tpWaterfall";
            _tpWaterfall.Padding = new Padding(3);
            _tpWaterfall.Size = new Size(721, 403);
            _tpWaterfall.Text = "Waterfall";
            tcDisplay.Controls.Add(_tpWaterfall);
            tcDisplay.Controls.SetChildIndex(_tpWaterfall, 1);

            wfProGroup = new GroupBoxTS();
            wfProGroup.Text = "Waterfall Pro / GPU";
            wfProGroup.Location = new Point(8, 8);
            wfProGroup.Size = new Size(700, 380);
            _tpWaterfall.Controls.Add(wfProGroup);

            BuildWaterfallSourceControls();
            BuildWaterfallColourControls();
            BuildWaterfallGpuControls();

            Display.GPUWaterfallEffectiveOverlapChanged -= OnGPUWaterfallEffectiveOverlapChanged;
            Display.GPUWaterfallEffectiveOverlapChanged += OnGPUWaterfallEffectiveOverlapChanged;
            UpdateNFLowHighEnabledState();
            UpdateKaiserBetaVisibility();
            UpdateLanczosControls();
            SyncPaletteMirrors();
            UpdateGPUInfoLabel();
        }

        private void Section(string text, int x, int y, int w)
        {
            LabelTS l = L(text, x, y, w);
            l.ForeColor = Color.SlateGray;
            wfProGroup.Controls.Add(l);
        }

        private void BuildWaterfallSourceControls()
        {
            int y = 22;
            Section("── Noise Floor / Detection ──", 12, y, 200); y += 24;

            wfProGroup.Controls.Add(L("NF Mode:", 12, y + 3, 70));
            comboNFMode = new ComboBoxTS();
            comboNFMode.Name = "comboNFMode";
            comboNFMode.DropDownStyle = ComboBoxStyle.DropDownList;
            comboNFMode.Items.AddRange(new object[] { "Average", "Percentile" });
            comboNFMode.Location = new Point(82, y); comboNFMode.Size = new Size(100, 21);
            comboNFMode.SelectedIndex = Display.NFMode == NoiseFloorPro.DetectionMode.Percentile ? 1 : 0;
            comboNFMode.SelectedIndexChanged += comboNFMode_SelectedIndexChanged;
            wfProGroup.Controls.Add(comboNFMode); y += 27;

            wfProGroup.Controls.Add(L("NF Low %:", 12, y + 3, 65));
            udNFLowPct = new NumericUpDownTS();
            udNFLowPct.Name = "udNFLowPct"; udNFLowPct.Minimum = 1; udNFLowPct.Maximum = 49;
            udNFLowPct.Value = (decimal)Display.NFLowPct; udNFLowPct.Location = new Point(82, y); udNFLowPct.Size = new Size(48, 21);
            udNFLowPct.ValueChanged += udNFLowPct_ValueChanged; wfProGroup.Controls.Add(udNFLowPct);
            wfProGroup.Controls.Add(L("High %:", 140, y + 3, 45));
            udNFHighPct = new NumericUpDownTS();
            udNFHighPct.Name = "udNFHighPct"; udNFHighPct.Minimum = 50; udNFHighPct.Maximum = 100;
            udNFHighPct.Value = (decimal)Math.Min(100f, Display.NFHighPct); udNFHighPct.Location = new Point(188, y); udNFHighPct.Size = new Size(48, 21);
            udNFHighPct.ValueChanged += udNFHighPct_ValueChanged; wfProGroup.Controls.Add(udNFHighPct); y += 27;

            chkAutoHigh = new CheckBoxTS();
            chkAutoHigh.Name = "chkAutoHigh"; chkAutoHigh.Text = "Auto High"; chkAutoHigh.AutoSize = true;
            chkAutoHigh.Location = new Point(12, y + 1); chkAutoHigh.Checked = Display.AutoHighEnabledRX1;
            chkAutoHigh.CheckedChanged += chkAutoHigh_CheckedChanged; wfProGroup.Controls.Add(chkAutoHigh);
            wfProGroup.Controls.Add(L("+dB:", 92, y + 3, 32));
            udAutoHighMargin = new NumericUpDownTS();
            udAutoHighMargin.Name = "udAutoHighMargin"; udAutoHighMargin.Minimum = 0; udAutoHighMargin.Maximum = 30;
            udAutoHighMargin.Value = (decimal)Display.AutoHighMarginDb; udAutoHighMargin.Location = new Point(126, y); udAutoHighMargin.Size = new Size(48, 21);
            udAutoHighMargin.ValueChanged += udAutoHighMargin_ValueChanged; wfProGroup.Controls.Add(udAutoHighMargin); y += 27;

            wfProGroup.Controls.Add(L("AGC Smooth:", 12, y + 3, 72));
            comboAGCSmooth = new ComboBoxTS();
            comboAGCSmooth.Name = "comboAGCSmooth"; comboAGCSmooth.DropDownStyle = ComboBoxStyle.DropDownList;
            comboAGCSmooth.Items.AddRange(new object[] { "Slow", "Medium", "Fast" });
            comboAGCSmooth.Location = new Point(86, y); comboAGCSmooth.Size = new Size(82, 21); comboAGCSmooth.SelectedIndex = 1;
            comboAGCSmooth.SelectedIndexChanged += comboAGCSmooth_SelectedIndexChanged; wfProGroup.Controls.Add(comboAGCSmooth);
            wfProGroup.Controls.Add(L("WF Detect:", 176, y + 3, 62));
            comboWFDetector = new ComboBoxTS();
            comboWFDetector.Name = "comboWFDetector"; comboWFDetector.DropDownStyle = ComboBoxStyle.DropDownList;
            comboWFDetector.Items.AddRange(new object[] { "Peak", "Average", "Sample" });
            comboWFDetector.Location = new Point(238, y); comboWFDetector.Size = new Size(78, 21); comboWFDetector.SelectedIndex = 0;
            comboWFDetector.SelectedIndexChanged += comboWFDetector_SelectedIndexChanged; wfProGroup.Controls.Add(comboWFDetector); y += 28;

            chkZoomAdaptive = new CheckBoxTS();
            chkZoomAdaptive.Name = "chkZoomAdaptive"; chkZoomAdaptive.Text = "Zoom Adaptive"; chkZoomAdaptive.AutoSize = true;
            chkZoomAdaptive.Location = new Point(12, y); chkZoomAdaptive.Checked = Display.ZoomAdaptiveEnabled;
            chkZoomAdaptive.CheckedChanged += chkZoomAdaptive_CheckedChanged; wfProGroup.Controls.Add(chkZoomAdaptive); y += 30;

            Section("── GPU FFT source ──", 12, y, 160); y += 22;
            chkGPUWaterfallFFT = new CheckBoxTS();
            chkGPUWaterfallFFT.Name = "chkGPUWaterfallFFT"; chkGPUWaterfallFFT.Text = "GPU FFT Waterfall"; chkGPUWaterfallFFT.AutoSize = true;
            chkGPUWaterfallFFT.Location = new Point(12, y); chkGPUWaterfallFFT.Checked = Display.GPUWaterfallPipelineEnabled;
            chkGPUWaterfallFFT.CheckedChanged += chkGPUWaterfallFFT_CheckedChanged; wfProGroup.Controls.Add(chkGPUWaterfallFFT); y += 23;

            wfProGroup.Controls.Add(L("FFT:", 12, y + 3, 30));
            comboGPUWaterfallFFTSize = new ComboBoxTS();
            comboGPUWaterfallFFTSize.Name = "comboGPUWaterfallFFTSize"; comboGPUWaterfallFFTSize.DropDownStyle = ComboBoxStyle.DropDownList;
            comboGPUWaterfallFFTSize.Items.AddRange(new object[] { "1024","2048","4096","8192","16384","32768","65536","131072","262144" });
            comboGPUWaterfallFFTSize.Location = new Point(44, y); comboGPUWaterfallFFTSize.Size = new Size(76,21);
            int fi = comboGPUWaterfallFFTSize.Items.IndexOf(Display.GPUWaterfallFFTSize.ToString()); comboGPUWaterfallFFTSize.SelectedIndex = fi >= 0 ? fi : 4;
            comboGPUWaterfallFFTSize.SelectedIndexChanged += comboGPUWaterfallFFTSize_SelectedIndexChanged; wfProGroup.Controls.Add(comboGPUWaterfallFFTSize);
            wfProGroup.Controls.Add(L("Overlap:", 126, y + 3, 50));
            udGPUWaterfallOverlap = new NumericUpDownTS();
            udGPUWaterfallOverlap.Name = "udGPUWaterfallOverlap"; udGPUWaterfallOverlap.Minimum = 0; udGPUWaterfallOverlap.Maximum = 95;
            udGPUWaterfallOverlap.Value = Display.GPUWaterfallOverlapPercent; udGPUWaterfallOverlap.Location = new Point(178,y); udGPUWaterfallOverlap.Size = new Size(48,21);
            udGPUWaterfallOverlap.ValueChanged += udGPUWaterfallOverlap_ValueChanged; wfProGroup.Controls.Add(udGPUWaterfallOverlap);
            chkGPUWaterfallAutoOverlap = new CheckBoxTS();
            chkGPUWaterfallAutoOverlap.Name = "chkGPUWaterfallAutoOverlap"; chkGPUWaterfallAutoOverlap.Text = "Auto"; chkGPUWaterfallAutoOverlap.AutoSize = true;
            chkGPUWaterfallAutoOverlap.Location = new Point(232,y+2); chkGPUWaterfallAutoOverlap.Checked = Display.GPUWaterfallAutoOverlap;
            chkGPUWaterfallAutoOverlap.CheckedChanged += chkGPUWaterfallAutoOverlap_CheckedChanged; wfProGroup.Controls.Add(chkGPUWaterfallAutoOverlap);
            lblGPUWaterfallEffectiveOverlap = L("", 278, y+3, 42); wfProGroup.Controls.Add(lblGPUWaterfallEffectiveOverlap); y += 24;

            wfProGroup.Controls.Add(L("Window:", 12, y + 3, 48));
            comboGPUWaterfallWindow = new ComboBoxTS();
            comboGPUWaterfallWindow.Name = "comboGPUWaterfallWindow"; comboGPUWaterfallWindow.DropDownStyle = ComboBoxStyle.DropDownList;
            comboGPUWaterfallWindow.Items.AddRange(Enum.GetNames(typeof(GPUWaterfallWindowType)));
            comboGPUWaterfallWindow.Location = new Point(62,y); comboGPUWaterfallWindow.Size = new Size(112,21);
            comboGPUWaterfallWindow.SelectedIndex = (int)Display.GPUWaterfallWindowType;
            comboGPUWaterfallWindow.SelectedIndexChanged += comboGPUWaterfallWindow_SelectedIndexChanged; wfProGroup.Controls.Add(comboGPUWaterfallWindow);
            lblGPUWaterfallKaiserBeta = L("Beta:", 180,y+3,32); wfProGroup.Controls.Add(lblGPUWaterfallKaiserBeta);
            udGPUWaterfallKaiserBeta = new NumericUpDownTS();
            udGPUWaterfallKaiserBeta.Name = "udGPUWaterfallKaiserBeta"; udGPUWaterfallKaiserBeta.Minimum=0; udGPUWaterfallKaiserBeta.Maximum=20; udGPUWaterfallKaiserBeta.DecimalPlaces=1; udGPUWaterfallKaiserBeta.Increment=0.5m;
            udGPUWaterfallKaiserBeta.Value=(decimal)Display.GPUWaterfallKaiserBeta; udGPUWaterfallKaiserBeta.Location=new Point(214,y); udGPUWaterfallKaiserBeta.Size=new Size(52,21);
            udGPUWaterfallKaiserBeta.ValueChanged += udGPUWaterfallKaiserBeta_ValueChanged; wfProGroup.Controls.Add(udGPUWaterfallKaiserBeta); y += 24;

            wfProGroup.Controls.Add(L("Scale:",12,y+3,38));
            comboGPUWaterfallMagnitudeMode = new ComboBoxTS();
            comboGPUWaterfallMagnitudeMode.Name="comboGPUWaterfallMagnitudeMode"; comboGPUWaterfallMagnitudeMode.DropDownStyle=ComboBoxStyle.DropDownList;
            comboGPUWaterfallMagnitudeMode.Items.AddRange(new object[]{"dBFS","PSD dBFS/Hz"}); comboGPUWaterfallMagnitudeMode.Location=new Point(52,y); comboGPUWaterfallMagnitudeMode.Size=new Size(104,21);
            comboGPUWaterfallMagnitudeMode.SelectedIndex=Math.Max(0,Math.Min(1,(int)Display.GPUWaterfallMagnitudeMode));
            comboGPUWaterfallMagnitudeMode.SelectedIndexChanged += comboGPUWaterfallMagnitudeMode_SelectedIndexChanged; wfProGroup.Controls.Add(comboGPUWaterfallMagnitudeMode);

            wfProGroup.Controls.Add(L("Resample:",164,y+3,58));
            comboGPUWaterfallResampling = new ComboBoxTS();
            comboGPUWaterfallResampling.Name="comboGPUWaterfallResampling"; comboGPUWaterfallResampling.DropDownStyle=ComboBoxStyle.DropDownList;
            comboGPUWaterfallResampling.Items.AddRange(new object[]{"Linear","Power Avg","Peak","Lanczos"}); comboGPUWaterfallResampling.Location=new Point(222,y); comboGPUWaterfallResampling.Size=new Size(96,21);
            comboGPUWaterfallResampling.SelectedIndex=Math.Max(0,Math.Min(3,(int)Display.GPUWaterfallResamplingMode));
            comboGPUWaterfallResampling.SelectedIndexChanged += comboGPUWaterfallResampling_SelectedIndexChanged; wfProGroup.Controls.Add(comboGPUWaterfallResampling); y += 24;

            lblGPUWaterfallLanczos=L("Lanczos:",164,y+3,58); wfProGroup.Controls.Add(lblGPUWaterfallLanczos);
            comboGPUWaterfallLanczos=new ComboBoxTS();
            comboGPUWaterfallLanczos.Name="comboGPUWaterfallLanczos"; comboGPUWaterfallLanczos.DropDownStyle=ComboBoxStyle.DropDownList;
            comboGPUWaterfallLanczos.Items.AddRange(new object[]{"2","3","4"}); comboGPUWaterfallLanczos.Location=new Point(222,y); comboGPUWaterfallLanczos.Size=new Size(50,21);
            comboGPUWaterfallLanczos.SelectedIndex=Math.Max(0,Math.Min(2,Display.GPUWaterfallLanczosWindow-2));
            comboGPUWaterfallLanczos.SelectedIndexChanged += comboGPUWaterfallLanczos_SelectedIndexChanged; wfProGroup.Controls.Add(comboGPUWaterfallLanczos);
        }

        private void MirrorPalette(ComboBoxTS mirror, ComboBoxTS source, string name, int x, int y)
        {
            mirror.Name = name; mirror.DropDownStyle = ComboBoxStyle.DropDownList;
            if (source != null) foreach (object item in source.Items) mirror.Items.Add(item);
            mirror.Location = new Point(x,y); mirror.Size = new Size(150,21);
            if (source != null && source.SelectedIndex >= 0 && source.SelectedIndex < mirror.Items.Count)
                mirror.SelectedIndex = source.SelectedIndex;
            wfProGroup.Controls.Add(mirror);
        }

        private void BuildWaterfallColourControls()
        {
            int x=340, y=22;
            Section("── Color schemes (native Thetis) ──",x,y,220); y+=22;
            wfProGroup.Controls.Add(L("RX1:",x,y+3,34)); comboWFPaletteRX1=new ComboBoxTS();
            MirrorPalette(comboWFPaletteRX1,comboColorPalette,"comboWFPaletteRX1",x+38,y);
            comboWFPaletteRX1.SelectedIndexChanged += (s,e)=>SetPaletteMirror(comboWFPaletteRX1,comboColorPalette); y+=24;
            wfProGroup.Controls.Add(L("RX2:",x,y+3,34)); comboWFPaletteRX2=new ComboBoxTS();
            MirrorPalette(comboWFPaletteRX2,comboRX2ColorPalette,"comboWFPaletteRX2",x+38,y);
            comboWFPaletteRX2.SelectedIndexChanged += (s,e)=>SetPaletteMirror(comboWFPaletteRX2,comboRX2ColorPalette); y+=24;
            wfProGroup.Controls.Add(L("TX:",x,y+3,34)); comboWFPaletteTX=new ComboBoxTS();
            MirrorPalette(comboWFPaletteTX,comboColorPalette_tx,"comboWFPaletteTX",x+38,y);
            comboWFPaletteTX.SelectedIndexChanged += (s,e)=>SetPaletteMirror(comboWFPaletteTX,comboColorPalette_tx); y+=30;

            Section("── Enhancement ──",x,y,140); y+=22;
            wfProGroup.Controls.Add(L("Tone Map:",x,y+3,68));
            comboToneMap=new ComboBoxTS(); comboToneMap.Name="comboToneMap"; comboToneMap.DropDownStyle=ComboBoxStyle.DropDownList;
            comboToneMap.Items.AddRange(new object[]{"Off","Reinhard","ACES"}); comboToneMap.Location=new Point(x+70,y); comboToneMap.Size=new Size(92,21);
            comboToneMap.SelectedIndex=(int)WaterfallEnhancer.ToneMap; comboToneMap.SelectedIndexChanged += comboToneMap_SelectedIndexChanged; wfProGroup.Controls.Add(comboToneMap); y+=25;
            wfProGroup.Controls.Add(L("Temporal:",x,y+3,68));
            comboTemporal=new ComboBoxTS(); comboTemporal.Name="comboTemporal"; comboTemporal.DropDownStyle=ComboBoxStyle.DropDownList;
            comboTemporal.Items.AddRange(new object[]{"Off","Light","Medium","Strong"}); comboTemporal.Location=new Point(x+70,y); comboTemporal.Size=new Size(92,21);
            comboTemporal.SelectedIndex=Display.TemporalEnabled ? (Display.TemporalStrength>=0.4f?3:(Display.TemporalStrength>=0.25f?2:1)) : 0;
            comboTemporal.SelectedIndexChanged += comboTemporal_SelectedIndexChanged; wfProGroup.Controls.Add(comboTemporal); y+=28;

            wfProGroup.Controls.Add(L("Palette sharp:",x,y+5,80));
            tbPalSharp=new TrackBarTS(); tbPalSharp.Name="tbPalSharp"; tbPalSharp.Minimum=0; tbPalSharp.Maximum=150; tbPalSharp.TickFrequency=25;
            tbPalSharp.Value=(int)(WaterfallEnhancer.PaletteSharpness*100f); tbPalSharp.Location=new Point(x+82,y); tbPalSharp.Size=new Size(140,28); tbPalSharp.Scroll += tbPalSharp_Scroll; wfProGroup.Controls.Add(tbPalSharp);
            lblPalSharpVal=L(tbPalSharp.Value.ToString(),x+225,y+5,32); wfProGroup.Controls.Add(lblPalSharpVal); y+=34;
            wfProGroup.Controls.Add(L("Palette contrast:",x,y+5,90));
            tbPalContrast=new TrackBarTS(); tbPalContrast.Name="tbPalContrast"; tbPalContrast.Minimum=0; tbPalContrast.Maximum=150; tbPalContrast.TickFrequency=25;
            tbPalContrast.Value=(int)(WaterfallEnhancer.PaletteContrast*100f); tbPalContrast.Location=new Point(x+92,y); tbPalContrast.Size=new Size(130,28); tbPalContrast.Scroll += tbPalContrast_Scroll; wfProGroup.Controls.Add(tbPalContrast);
            lblPalContrastVal=L(tbPalContrast.Value.ToString(),x+225,y+5,32); wfProGroup.Controls.Add(lblPalContrastVal);
        }

        private void BuildWaterfallGpuControls()
        {
            int x=340,y=246;
            Section("── GPU paths (independent) ──",x,y,200); y+=22;
            chkGPUColorCompute=new CheckBoxTS(); chkGPUColorCompute.Name="chkGPUColorCompute"; chkGPUColorCompute.Text="GPU color compute (HLSL)"; chkGPUColorCompute.AutoSize=true;
            chkGPUColorCompute.Location=new Point(x,y); chkGPUColorCompute.Checked=Display.GPUColorComputeEnabled; chkGPUColorCompute.CheckedChanged += chkGPUColorCompute_CheckedChanged; wfProGroup.Controls.Add(chkGPUColorCompute); y+=20;
            chkSQ4KOUWaterfallMesh=new CheckBoxTS(); chkSQ4KOUWaterfallMesh.Name="chkSQ4KOUWaterfallMesh"; chkSQ4KOUWaterfallMesh.Text="WaterfallMesh presenter"; chkSQ4KOUWaterfallMesh.AutoSize=true;
            chkSQ4KOUWaterfallMesh.Location=new Point(x,y); chkSQ4KOUWaterfallMesh.Checked=Display.SQ4KOUWaterfallMeshEnabled; chkSQ4KOUWaterfallMesh.CheckedChanged += chkSQ4KOUWaterfallMesh_CheckedChanged; wfProGroup.Controls.Add(chkSQ4KOUWaterfallMesh); y+=20;
            chkSQ4KOUDiagLog=new CheckBoxTS(); chkSQ4KOUDiagLog.Name="chkSQ4KOUDiagLog"; chkSQ4KOUDiagLog.Text="GPU diagnostic log"; chkSQ4KOUDiagLog.AutoSize=true;
            chkSQ4KOUDiagLog.Location=new Point(x,y); chkSQ4KOUDiagLog.Checked=Common.MeshDiagLogEnabled; chkSQ4KOUDiagLog.CheckedChanged += chkSQ4KOUDiagLog_CheckedChanged; wfProGroup.Controls.Add(chkSQ4KOUDiagLog);

            btnTestGPU=new ButtonTS(); btnTestGPU.Name="btnTestGPU"; btnTestGPU.Text="Test GPU"; btnTestGPU.Location=new Point(x,y+25); btnTestGPU.Size=new Size(82,24);
            btnTestGPU.Click += btnTestGPU_Click; wfProGroup.Controls.Add(btnTestGPU);
            lblGPUInfo=L("Detecting...",x+92,y-42,255); lblGPUInfo.Size=new Size(255,108); lblGPUInfo.ForeColor=Color.SlateGray; wfProGroup.Controls.Add(lblGPUInfo);

            _gpuStatusTimer=new System.Windows.Forms.Timer();
            _gpuStatusTimer.Interval=1500;
            _gpuStatusTimer.Tick += (s,e)=>{ try { SyncPaletteMirrors(); UpdateGPUInfoLabel(); } catch {} };
            _gpuStatusTimer.Start();
        }

        private void SetPaletteMirror(ComboBoxTS mirror, ComboBoxTS source)
        {
            if (_paletteSyncing || initializing || mirror==null || source==null || mirror.SelectedIndex<0) return;
            if (mirror.SelectedIndex < source.Items.Count)
            {
                _paletteSyncing=true;
                try { source.SelectedIndex=mirror.SelectedIndex; }
                finally { _paletteSyncing=false; }
            }
        }

        private void SyncPaletteMirrors()
        {
            if (_paletteSyncing) return;
            _paletteSyncing=true;
            try
            {
                SyncPaletteOne(comboWFPaletteRX1,comboColorPalette);
                SyncPaletteOne(comboWFPaletteRX2,comboRX2ColorPalette);
                SyncPaletteOne(comboWFPaletteTX,comboColorPalette_tx);
            }
            finally { _paletteSyncing=false; }
        }

        private void SyncPaletteOne(ComboBoxTS mirror, ComboBoxTS source)
        {
            if (mirror==null || source==null || mirror.Focused) return;
            if (source.SelectedIndex>=0 && source.SelectedIndex<mirror.Items.Count && mirror.SelectedIndex!=source.SelectedIndex)
                mirror.SelectedIndex=source.SelectedIndex;
        }

        private void UpdateGPUInfoLabel()
        {
            if (lblGPUInfo==null) return;
            lblGPUInfo.Text=(Display.GPUName??"unknown")+"\n"+
                "Renderer: "+Display.RenderPathString()+"\n"+
                "Source: "+Display.SQ4KOUHighResWaterfallStatusRX1+"\n"+
                "Color: "+Display.WaterfallColorComputeStatus+"\n"+
                "Presenter: "+Display.WaterfallPresenterStatusRX1;
        }

        private void btnTestGPU_Click(object sender, EventArgs e)
        {
            bool hlsl=Display.GpuComputeAvailable;
            UpdateGPUInfoLabel();
            MessageBox.Show("GPU: "+(Display.GPUName??"unknown")+"\nRenderer: "+Display.RenderPathString()+
                "\nHardware context: "+GPUDetector.HasDeviceContext+
                "\n\nGPU FFT requested: "+Display.GPUWaterfallPipelineEnabled+
                "\nGPU FFT RX1: "+Display.SQ4KOUHighResWaterfallStatusRX1+
                "\n\nHLSL requested: "+Display.GPUColorComputeEnabled+
                "\nHLSL available: "+hlsl+
                "\nHLSL probe: "+Display.GpuComputeProbeStatus+
                "\nColor path: "+Display.WaterfallColorComputeStatus+
                "\n\nWaterfallMesh requested: "+Display.SQ4KOUWaterfallMeshEnabled+
                "\nPresenter: "+Display.WaterfallPresenterStatusRX1,
                "GPU Test",MessageBoxButtons.OK,MessageBoxIcon.Information,MessageBoxDefaultButton.Button1,(MessageBoxOptions)262144);
        }

        private void chkGPUColorCompute_CheckedChanged(object sender, EventArgs e)
        { if(!initializing){ Display.GPUColorComputeEnabled=chkGPUColorCompute.Checked; UpdateGPUInfoLabel(); } }
        private void chkSQ4KOUWaterfallMesh_CheckedChanged(object sender, EventArgs e)
        { if(!initializing){ Display.SQ4KOUWaterfallMeshEnabled=chkSQ4KOUWaterfallMesh.Checked; UpdateGPUInfoLabel(); } }
        private void chkSQ4KOUDiagLog_CheckedChanged(object sender, EventArgs e)
        { if(!initializing) Common.MeshDiagLogEnabled=chkSQ4KOUDiagLog.Checked; }

        private void comboNFMode_SelectedIndexChanged(object sender, EventArgs e)
        { if(!initializing){ Display.NFMode=comboNFMode.SelectedIndex==1?NoiseFloorPro.DetectionMode.Percentile:NoiseFloorPro.DetectionMode.Average; UpdateNFLowHighEnabledState(); } }
        private void UpdateNFLowHighEnabledState()
        { bool b=comboNFMode!=null&&comboNFMode.SelectedIndex==1; if(udNFLowPct!=null)udNFLowPct.Enabled=b; if(udNFHighPct!=null)udNFHighPct.Enabled=b; }
        private void udNFLowPct_ValueChanged(object sender,EventArgs e){if(!initializing)Display.NFLowPct=(float)udNFLowPct.Value;}
        private void udNFHighPct_ValueChanged(object sender,EventArgs e){if(!initializing)Display.NFHighPct=(float)udNFHighPct.Value;}
        private void chkAutoHigh_CheckedChanged(object sender,EventArgs e){if(!initializing){Display.AutoHighEnabledRX1=chkAutoHigh.Checked;Display.AutoHighEnabledRX2=chkAutoHigh.Checked;}}
        private void udAutoHighMargin_ValueChanged(object sender,EventArgs e){if(!initializing)Display.AutoHighMarginDb=(float)udAutoHighMargin.Value;}
        private void comboAGCSmooth_SelectedIndexChanged(object sender,EventArgs e)
        { if(!initializing)Display.WaterfallAgcSmoothing=comboAGCSmooth.SelectedIndex==0?0.2f:(comboAGCSmooth.SelectedIndex==2?0.6f:0.4f); }
        private void comboWFDetector_SelectedIndexChanged(object sender,EventArgs e)
        {
            if(initializing)return;
            int d=comboWFDetector.SelectedIndex==1?2:(comboWFDetector.SelectedIndex==2?3:0);
            if(comboDispWFDetector!=null)comboDispWFDetector.SelectedIndex=d;
            if(comboRX2DispWFDetector!=null)comboRX2DispWFDetector.SelectedIndex=d;
            if(console!=null&&console.specRX!=null){console.specRX.GetSpecRX(0).DetTypeWF=d;console.specRX.GetSpecRX(1).DetTypeWF=d;}
        }
        private void chkZoomAdaptive_CheckedChanged(object sender,EventArgs e)
        { if(!initializing){Display.ZoomAdaptiveEnabled=chkZoomAdaptive.Checked;comboToneMap.Enabled=!chkZoomAdaptive.Checked;comboTemporal.Enabled=!chkZoomAdaptive.Checked;} }

        private void comboToneMap_SelectedIndexChanged(object sender,EventArgs e)
        { if(!initializing)WaterfallEnhancer.SetToneMap(comboToneMap.SelectedIndex==1?WaterfallEnhancer.ToneMapMode.Reinhard:(comboToneMap.SelectedIndex==2?WaterfallEnhancer.ToneMapMode.ACES:WaterfallEnhancer.ToneMapMode.None)); }
        private void comboTemporal_SelectedIndexChanged(object sender,EventArgs e)
        { if(!initializing){float a=comboTemporal.SelectedIndex==1?0.15f:(comboTemporal.SelectedIndex==2?0.30f:(comboTemporal.SelectedIndex==3?0.45f:0f));Display.TemporalStrength=a;Display.TemporalEnabled=a>0;}}
        private void tbPalSharp_Scroll(object sender,EventArgs e){lblPalSharpVal.Text=tbPalSharp.Value.ToString();WaterfallEnhancer.SetPaletteSharpness(tbPalSharp.Value/100f);}
        private void tbPalContrast_Scroll(object sender,EventArgs e){lblPalContrastVal.Text=tbPalContrast.Value.ToString();WaterfallEnhancer.SetPaletteContrast(tbPalContrast.Value/100f);}

        private void chkGPUWaterfallFFT_CheckedChanged(object sender,EventArgs e)
        { if(!initializing){Display.GPUWaterfallPipelineEnabled=chkGPUWaterfallFFT.Checked;UpdateGPUInfoLabel();}}
        private void comboGPUWaterfallFFTSize_SelectedIndexChanged(object sender,EventArgs e)
        { if(!initializing&&int.TryParse(comboGPUWaterfallFFTSize.Text,out int n))Display.GPUWaterfallFFTSize=n; }
        private void comboGPUWaterfallWindow_SelectedIndexChanged(object sender,EventArgs e)
        { if(!initializing&&Enum.TryParse(comboGPUWaterfallWindow.Text,out GPUWaterfallWindowType w)){Display.GPUWaterfallWindowType=w;UpdateKaiserBetaVisibility();}}
        private void UpdateKaiserBetaVisibility()
        { bool v=comboGPUWaterfallWindow!=null&&comboGPUWaterfallWindow.Text==GPUWaterfallWindowType.Kaiser.ToString();if(lblGPUWaterfallKaiserBeta!=null)lblGPUWaterfallKaiserBeta.Visible=v;if(udGPUWaterfallKaiserBeta!=null)udGPUWaterfallKaiserBeta.Visible=v;}
        private void udGPUWaterfallKaiserBeta_ValueChanged(object sender,EventArgs e){if(!initializing)Display.GPUWaterfallKaiserBeta=(double)udGPUWaterfallKaiserBeta.Value;}
        private void comboGPUWaterfallMagnitudeMode_SelectedIndexChanged(object sender,EventArgs e){if(!initializing)Display.GPUWaterfallMagnitudeMode=(GPUWaterfallMagnitudeMode)comboGPUWaterfallMagnitudeMode.SelectedIndex;}
        private void udGPUWaterfallOverlap_ValueChanged(object sender,EventArgs e){if(!initializing)Display.GPUWaterfallOverlapPercent=(int)udGPUWaterfallOverlap.Value;}
        private void chkGPUWaterfallAutoOverlap_CheckedChanged(object sender,EventArgs e){if(!initializing)Display.GPUWaterfallAutoOverlap=chkGPUWaterfallAutoOverlap.Checked;}
        private void comboGPUWaterfallResampling_SelectedIndexChanged(object sender,EventArgs e)
        { if(!initializing){Display.GPUWaterfallResamplingMode=(GPUWaterfallResamplingMode)comboGPUWaterfallResampling.SelectedIndex;UpdateLanczosControls();}}
        private void comboGPUWaterfallLanczos_SelectedIndexChanged(object sender,EventArgs e){if(!initializing)Display.GPUWaterfallLanczosWindow=comboGPUWaterfallLanczos.SelectedIndex+2;}
        private void UpdateLanczosControls()
        { bool b=comboGPUWaterfallResampling!=null&&comboGPUWaterfallResampling.SelectedIndex==(int)GPUWaterfallResamplingMode.Lanczos;if(lblGPUWaterfallLanczos!=null)lblGPUWaterfallLanczos.Enabled=b;if(comboGPUWaterfallLanczos!=null)comboGPUWaterfallLanczos.Enabled=b;}

        private void OnGPUWaterfallEffectiveOverlapChanged(int rx,double overlap)
        {
            if(lblGPUWaterfallEffectiveOverlap==null||lblGPUWaterfallEffectiveOverlap.IsDisposed||!IsHandleCreated||IsDisposed)return;
            if(InvokeRequired){try{BeginInvoke((Action)(()=>OnGPUWaterfallEffectiveOverlapChanged(rx,overlap)));}catch{}return;}
            lblGPUWaterfallEffectiveOverlap.Text=$"{overlap*100.0:F0}%";
        }
    }
}
