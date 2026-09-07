using System;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis
{
    public partial class Setup
    {
        // Thetis 2.10.3.16 Extended-style Waterfall page, implemented programmatically
        // so the large WinForms designer file and the existing SQ4KOU setup layout remain untouched.
        private TabPage tpWaterfall;
        private GroupBoxTS grpGPUWaterfallFFT;
        private GroupBoxTS grpGPUWaterfallDiagnostics;
        private CheckBoxTS chkGPUWaterfallFFT;
        private ComboBoxTS comboGPUWaterfallFFTSize;
        private CheckBoxTS chkGPUWaterfallAutoOverlap;
        private NumericUpDownTS udGPUWaterfallOverlap;
        private ComboBoxTS comboGPUWaterfallWindow;
        private NumericUpDownTS udGPUWaterfallKaiserBeta;
        private ComboBoxTS comboGPUWaterfallMagnitudeMode;
        private ComboBoxTS comboGPUWaterfallLanczos;
        private ComboBoxTS comboGPUWaterfallResampling;
        private LabelTS lblGPUWaterfallDroppedRX1;
        private LabelTS lblGPUWaterfallDroppedRX2;
        private LabelTS lblGPUWaterfallCalRX1;
        private LabelTS lblGPUWaterfallCalRX2;
        private LabelTS lblGPUWaterfallStatus;
        private ButtonTS btnGPUWaterfallResetCalibration;
        private System.Windows.Forms.Timer gpuWaterfallStatusTimer;

        private static LabelTS GPUWFLabel(string text, int x, int y, int width)
        {
            LabelTS label = new LabelTS();
            label.Text = text;
            label.Location = new Point(x, y + 4);
            label.Size = new Size(width, 20);
            return label;
        }

        private void InitGPUWaterfallSetupUI()
        {
            if (tpWaterfall != null || tcDisplay == null) return;

            tpWaterfall = new TabPage();
            tpWaterfall.Name = "tpWaterfall";
            tpWaterfall.Text = "Waterfall";
            tpWaterfall.BackColor = SystemColors.Control;
            tpWaterfall.Padding = new Padding(8);
            tpWaterfall.AutoScroll = true;

            grpGPUWaterfallFFT = new GroupBoxTS();
            grpGPUWaterfallFFT.Name = "grpGPUWaterfallFFT";
            grpGPUWaterfallFFT.Text = "GPU FFT Waterfall";
            grpGPUWaterfallFFT.Location = new Point(10, 10);
            grpGPUWaterfallFFT.Size = new Size(470, 330);

            chkGPUWaterfallFFT = new CheckBoxTS();
            chkGPUWaterfallFFT.Name = "chkGPUWaterfallFFT";
            chkGPUWaterfallFFT.Text = "Enable GPU FFT Waterfall (DirectCompute)";
            chkGPUWaterfallFFT.Location = new Point(18, 28);
            chkGPUWaterfallFFT.Size = new Size(310, 22);
            chkGPUWaterfallFFT.Checked = Display.WaterfallUseGPU;
            chkGPUWaterfallFFT.CheckedChanged += delegate
            {
                Display.WaterfallUseGPU = chkGPUWaterfallFFT.Checked;
                UpdateGPUWaterfallSetupEnableState();
            };
            grpGPUWaterfallFFT.Controls.Add(chkGPUWaterfallFFT);

            grpGPUWaterfallFFT.Controls.Add(GPUWFLabel("FFT size:", 18, 62, 130));
            comboGPUWaterfallFFTSize = new ComboBoxTS();
            comboGPUWaterfallFFTSize.Name = "comboGPUWaterfallFFTSize";
            comboGPUWaterfallFFTSize.DropDownStyle = ComboBoxStyle.DropDownList;
            comboGPUWaterfallFFTSize.Items.AddRange(new object[] {
                "1024", "2048", "4096", "8192", "16384", "32768", "65536", "131072", "262144"
            });
            comboGPUWaterfallFFTSize.Location = new Point(160, 60);
            comboGPUWaterfallFFTSize.Size = new Size(135, 22);
            comboGPUWaterfallFFTSize.Text = Display.GPUWaterfallFFTSize.ToString();
            comboGPUWaterfallFFTSize.SelectedIndexChanged += delegate
            {
                int value;
                if (Int32.TryParse(comboGPUWaterfallFFTSize.Text, out value))
                    Display.GPUWaterfallFFTSize = value;
            };
            grpGPUWaterfallFFT.Controls.Add(comboGPUWaterfallFFTSize);

            chkGPUWaterfallAutoOverlap = new CheckBoxTS();
            chkGPUWaterfallAutoOverlap.Name = "chkGPUWaterfallAutoOverlap";
            chkGPUWaterfallAutoOverlap.Text = "Auto Overlap";
            chkGPUWaterfallAutoOverlap.Location = new Point(18, 96);
            chkGPUWaterfallAutoOverlap.Size = new Size(130, 22);
            chkGPUWaterfallAutoOverlap.Checked = Display.GPUWaterfallAutoOverlap;
            chkGPUWaterfallAutoOverlap.CheckedChanged += delegate
            {
                Display.GPUWaterfallAutoOverlap = chkGPUWaterfallAutoOverlap.Checked;
                UpdateGPUWaterfallSetupEnableState();
            };
            grpGPUWaterfallFFT.Controls.Add(chkGPUWaterfallAutoOverlap);

            grpGPUWaterfallFFT.Controls.Add(GPUWFLabel("Overlap %:", 160, 96, 88));
            udGPUWaterfallOverlap = new NumericUpDownTS();
            udGPUWaterfallOverlap.Name = "udGPUWaterfallOverlap";
            udGPUWaterfallOverlap.Minimum = 0;
            udGPUWaterfallOverlap.Maximum = 95;
            udGPUWaterfallOverlap.DecimalPlaces = 0;
            udGPUWaterfallOverlap.Increment = 1;
            udGPUWaterfallOverlap.Value = (decimal)Display.GPUWaterfallOverlapPercent;
            udGPUWaterfallOverlap.Location = new Point(250, 94);
            udGPUWaterfallOverlap.Size = new Size(70, 22);
            udGPUWaterfallOverlap.ValueChanged += delegate
            {
                Display.GPUWaterfallOverlapPercent = (float)udGPUWaterfallOverlap.Value;
            };
            grpGPUWaterfallFFT.Controls.Add(udGPUWaterfallOverlap);

            grpGPUWaterfallFFT.Controls.Add(GPUWFLabel("Window:", 18, 132, 130));
            comboGPUWaterfallWindow = new ComboBoxTS();
            comboGPUWaterfallWindow.Name = "comboGPUWaterfallWindow";
            comboGPUWaterfallWindow.DropDownStyle = ComboBoxStyle.DropDownList;
            comboGPUWaterfallWindow.Items.AddRange(new object[] { "Hann", "Hamming", "Blackman-Harris", "Kaiser" });
            comboGPUWaterfallWindow.Location = new Point(160, 130);
            comboGPUWaterfallWindow.Size = new Size(160, 22);
            comboGPUWaterfallWindow.SelectedIndex = Math.Max(0, Math.Min(3, Display.GPUWaterfallWindowType));
            comboGPUWaterfallWindow.SelectedIndexChanged += delegate
            {
                Display.GPUWaterfallWindowType = comboGPUWaterfallWindow.SelectedIndex;
                UpdateGPUWaterfallSetupEnableState();
            };
            grpGPUWaterfallFFT.Controls.Add(comboGPUWaterfallWindow);

            grpGPUWaterfallFFT.Controls.Add(GPUWFLabel("Kaiser Beta:", 18, 166, 130));
            udGPUWaterfallKaiserBeta = new NumericUpDownTS();
            udGPUWaterfallKaiserBeta.Name = "udGPUWaterfallKaiserBeta";
            udGPUWaterfallKaiserBeta.Minimum = 0;
            udGPUWaterfallKaiserBeta.Maximum = 20;
            udGPUWaterfallKaiserBeta.DecimalPlaces = 1;
            udGPUWaterfallKaiserBeta.Increment = 0.1M;
            udGPUWaterfallKaiserBeta.Value = (decimal)Display.GPUWaterfallKaiserBeta;
            udGPUWaterfallKaiserBeta.Location = new Point(160, 164);
            udGPUWaterfallKaiserBeta.Size = new Size(90, 22);
            udGPUWaterfallKaiserBeta.ValueChanged += delegate
            {
                Display.GPUWaterfallKaiserBeta = (float)udGPUWaterfallKaiserBeta.Value;
            };
            grpGPUWaterfallFFT.Controls.Add(udGPUWaterfallKaiserBeta);

            grpGPUWaterfallFFT.Controls.Add(GPUWFLabel("Magnitude:", 18, 200, 130));
            comboGPUWaterfallMagnitudeMode = new ComboBoxTS();
            comboGPUWaterfallMagnitudeMode.Name = "comboGPUWaterfallMagnitudeMode";
            comboGPUWaterfallMagnitudeMode.DropDownStyle = ComboBoxStyle.DropDownList;
            comboGPUWaterfallMagnitudeMode.Items.AddRange(new object[] { "Amplitude dBFS", "PSD dBFS/Hz" });
            comboGPUWaterfallMagnitudeMode.Location = new Point(160, 198);
            comboGPUWaterfallMagnitudeMode.Size = new Size(160, 22);
            comboGPUWaterfallMagnitudeMode.SelectedIndex = Math.Max(0, Math.Min(1, Display.GPUWaterfallMagnitudeMode));
            comboGPUWaterfallMagnitudeMode.SelectedIndexChanged += delegate
            {
                Display.GPUWaterfallMagnitudeMode = comboGPUWaterfallMagnitudeMode.SelectedIndex;
            };
            grpGPUWaterfallFFT.Controls.Add(comboGPUWaterfallMagnitudeMode);

            grpGPUWaterfallFFT.Controls.Add(GPUWFLabel("Resampling:", 18, 234, 130));
            comboGPUWaterfallResampling = new ComboBoxTS();
            comboGPUWaterfallResampling.Name = "comboGPUWaterfallResampling";
            comboGPUWaterfallResampling.DropDownStyle = ComboBoxStyle.DropDownList;
            comboGPUWaterfallResampling.Items.AddRange(new object[] { "Linear", "Power Average", "Peak", "Lanczos" });
            comboGPUWaterfallResampling.Location = new Point(160, 232);
            comboGPUWaterfallResampling.Size = new Size(160, 22);
            comboGPUWaterfallResampling.SelectedIndex = Math.Max(0, Math.Min(3, Display.GPUWaterfallResamplingMode));
            comboGPUWaterfallResampling.SelectedIndexChanged += delegate
            {
                Display.GPUWaterfallResamplingMode = comboGPUWaterfallResampling.SelectedIndex;
                UpdateGPUWaterfallSetupEnableState();
            };
            grpGPUWaterfallFFT.Controls.Add(comboGPUWaterfallResampling);

            grpGPUWaterfallFFT.Controls.Add(GPUWFLabel("Lanczos window:", 18, 268, 130));
            comboGPUWaterfallLanczos = new ComboBoxTS();
            comboGPUWaterfallLanczos.Name = "comboGPUWaterfallLanczos";
            comboGPUWaterfallLanczos.DropDownStyle = ComboBoxStyle.DropDownList;
            comboGPUWaterfallLanczos.Items.AddRange(new object[] { "2", "3", "4" });
            comboGPUWaterfallLanczos.Location = new Point(160, 266);
            comboGPUWaterfallLanczos.Size = new Size(90, 22);
            comboGPUWaterfallLanczos.Text = Display.GPUWaterfallLanczosWindow.ToString();
            comboGPUWaterfallLanczos.SelectedIndexChanged += delegate
            {
                int value;
                if (Int32.TryParse(comboGPUWaterfallLanczos.Text, out value))
                    Display.GPUWaterfallLanczosWindow = value;
            };
            grpGPUWaterfallFFT.Controls.Add(comboGPUWaterfallLanczos);

            LabelTS note = new LabelTS();
            note.Text = "Recommended start: FFT 32768, Auto Overlap, Hann, Power Average.";
            note.Location = new Point(18, 300);
            note.Size = new Size(425, 20);
            grpGPUWaterfallFFT.Controls.Add(note);

            grpGPUWaterfallDiagnostics = new GroupBoxTS();
            grpGPUWaterfallDiagnostics.Name = "grpGPUWaterfallDiagnostics";
            grpGPUWaterfallDiagnostics.Text = "GPU Waterfall diagnostics";
            grpGPUWaterfallDiagnostics.Location = new Point(490, 10);
            grpGPUWaterfallDiagnostics.Size = new Size(210, 260);

            lblGPUWaterfallStatus = GPUWFLabel("GPU FFT: enabled", 14, 28, 180);
            lblGPUWaterfallStatus.Name = "lblGPUWaterfallStatus";
            grpGPUWaterfallDiagnostics.Controls.Add(lblGPUWaterfallStatus);

            lblGPUWaterfallDroppedRX1 = GPUWFLabel("RX1 dropped: 0", 14, 62, 180);
            lblGPUWaterfallDroppedRX1.Name = "lblGPUWaterfallDroppedRX1";
            grpGPUWaterfallDiagnostics.Controls.Add(lblGPUWaterfallDroppedRX1);

            lblGPUWaterfallDroppedRX2 = GPUWFLabel("RX2 dropped: 0", 14, 88, 180);
            lblGPUWaterfallDroppedRX2.Name = "lblGPUWaterfallDroppedRX2";
            grpGPUWaterfallDiagnostics.Controls.Add(lblGPUWaterfallDroppedRX2);

            lblGPUWaterfallCalRX1 = GPUWFLabel("RX1 A/B cal: 0.00 dB", 14, 122, 180);
            lblGPUWaterfallCalRX1.Name = "lblGPUWaterfallCalRX1";
            grpGPUWaterfallDiagnostics.Controls.Add(lblGPUWaterfallCalRX1);

            lblGPUWaterfallCalRX2 = GPUWFLabel("RX2 A/B cal: 0.00 dB", 14, 148, 180);
            lblGPUWaterfallCalRX2.Name = "lblGPUWaterfallCalRX2";
            grpGPUWaterfallDiagnostics.Controls.Add(lblGPUWaterfallCalRX2);

            btnGPUWaterfallResetCalibration = new ButtonTS();
            btnGPUWaterfallResetCalibration.Name = "btnGPUWaterfallResetCalibration";
            btnGPUWaterfallResetCalibration.Text = "Reset A/B calibration";
            btnGPUWaterfallResetCalibration.Location = new Point(14, 188);
            btnGPUWaterfallResetCalibration.Size = new Size(175, 28);
            btnGPUWaterfallResetCalibration.Click += delegate
            {
                Display.ResetGPUWaterfallCalibration();
                UpdateGPUWaterfallDiagnostics();
            };
            grpGPUWaterfallDiagnostics.Controls.Add(btnGPUWaterfallResetCalibration);

            tpWaterfall.Controls.Add(grpGPUWaterfallFFT);
            tpWaterfall.Controls.Add(grpGPUWaterfallDiagnostics);
            tcDisplay.TabPages.Add(tpWaterfall);

            gpuWaterfallStatusTimer = new System.Windows.Forms.Timer();
            gpuWaterfallStatusTimer.Interval = 1000;
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

        private void UpdateGPUWaterfallSetupEnableState()
        {
            if (chkGPUWaterfallFFT == null) return;
            bool enabled = chkGPUWaterfallFFT.Checked;
            comboGPUWaterfallFFTSize.Enabled = enabled;
            chkGPUWaterfallAutoOverlap.Enabled = enabled;
            udGPUWaterfallOverlap.Enabled = enabled && !chkGPUWaterfallAutoOverlap.Checked;
            comboGPUWaterfallWindow.Enabled = enabled;
            udGPUWaterfallKaiserBeta.Enabled = enabled && comboGPUWaterfallWindow.SelectedIndex == 3;
            comboGPUWaterfallMagnitudeMode.Enabled = enabled;
            comboGPUWaterfallResampling.Enabled = enabled;
            comboGPUWaterfallLanczos.Enabled = enabled && comboGPUWaterfallResampling.SelectedIndex == 3;
        }

        private void UpdateGPUWaterfallDiagnostics()
        {
            if (lblGPUWaterfallStatus == null) return;
            lblGPUWaterfallStatus.Text = Display.WaterfallUseGPU ? "GPU FFT: enabled" : "GPU FFT: CPU fallback";
            lblGPUWaterfallDroppedRX1.Text = "RX1 dropped: " + Display.GPUWaterfallDroppedSamplesRX1.ToString();
            lblGPUWaterfallDroppedRX2.Text = "RX2 dropped: " + Display.GPUWaterfallDroppedSamplesRX2.ToString();
            lblGPUWaterfallCalRX1.Text = "RX1 A/B cal: " + Display.GPUWaterfallCalibrationOffsetRX1.ToString("0.00") + " dB";
            lblGPUWaterfallCalRX2.Text = "RX2 A/B cal: " + Display.GPUWaterfallCalibrationOffsetRX2.ToString("0.00") + " dB";
        }
    }
}
