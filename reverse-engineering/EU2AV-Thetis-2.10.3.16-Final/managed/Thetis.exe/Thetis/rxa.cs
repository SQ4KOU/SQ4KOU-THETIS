using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class rxa : Form
{
	private int fwid;

	private int stid;

	private int chid;

	public bool update;

	private Size console_basis_size = new Size(100, 100);

	private Size gr_display_size_basis = new Size(100, 100);

	private Size pic_display_size_basis = new Size(100, 100);

	private IContainer components;

	private NumericUpDownTS udRXAFreq;

	private LabelTS labelTS1;

	private LabelTS labelTS2;

	private NumericUpDownTS udRXAVolume;

	private NumericUpDownTS udRXAMode;

	private LabelTS labelTS3;

	private LabelTS labelTS4;

	private NumericUpDownTS udRXAAGCGain;

	private LabelTS labelTS5;

	private PanelTS panelPanDisplay;

	private PanDisplay panDisplay;

	public long RXFreq
	{
		get
		{
			return (long)((double)udRXAFreq.Value * 1000000.0);
		}
		set
		{
			udRXAFreq.Value = (decimal)((double)value * 1E-06);
		}
	}

	public PanDisplay pDisplay
	{
		get
		{
			return panDisplay;
		}
		set
		{
			panDisplay = value;
		}
	}

	public rxa(int i)
	{
		InitializeComponent();
		fwid = i;
		stid = i - 2;
		chid = 2 * stid;
		panDisplay.init = true;
		panDisplay.DisplayID = stid;
		Text = "Using Rx" + fwid;
		base.Name = "rxa" + fwid;
		create_rxa();
		Common.RestoreForm(this, base.Name, restore_size: false);
		ForceRxa();
		console_basis_size = base.Size;
		gr_display_size_basis = panelPanDisplay.Size;
		pic_display_size_basis = panDisplay.Size;
	}

	private unsafe void create_rxa()
	{
		NetworkIO.SetDDCRate(fwid, 48000);
		NetworkIO.EnableRx(fwid, 1);
		cmaster.SetXcmInrate(stid, 48000);
		cmaster.SetRunPanadapter(stid, run: true);
		cmaster.SetAAudioMixState(null, 0, chid, state: true);
		cmaster.SetAAudioMixWhat(null, 0, chid, state: true);
		WDSP.SetChannelState(chid, 1, 0);
		WDSP.SetChannelState(chid + 1, 0, 0);
	}

	private void ForceRxa()
	{
		EventArgs empty = EventArgs.Empty;
		udRXAAGCGain_ValueChanged(this, empty);
		udRXAVolume_ValueChanged(this, empty);
		udRXAMode_ValueChanged(this, empty);
		panDisplay.initAnalyzer();
		udRXAFreq_ValueChanged(this, empty);
	}

	private void udRXAFreq_ValueChanged(object sender, EventArgs e)
	{
		NetworkIO.SetVFOfreq(fwid, NetworkIO.Freq2PhaseWord((int)(1000000.0 * (double)udRXAFreq.Value)), 0);
		panDisplay.VFOHz = RXFreq;
	}

	private void udRXAAGCGain_ValueChanged(object sender, EventArgs e)
	{
		WDSP.SetRXAAGCTop(chid, (double)udRXAAGCGain.Value);
	}

	private void udRXAVolume_ValueChanged(object sender, EventArgs e)
	{
		WDSP.SetRXAPanelGain1(chid, 0.01 * (double)udRXAVolume.Value);
	}

	private void udRXAMode_ValueChanged(object sender, EventArgs e)
	{
		int num = (int)udRXAMode.Value;
		DSPMode mode = DSPMode.LSB;
		switch (num)
		{
		case 0:
			mode = DSPMode.LSB;
			WDSP.SetRXABandpassFreqs(chid, -3100.0, -200.0);
			WDSP.RXANBPSetFreqs(chid, -3100.0, -200.0);
			WDSP.SetRXASNBAOutputBandwidth(chid, -3100.0, -200.0);
			break;
		case 1:
			mode = DSPMode.USB;
			WDSP.SetRXABandpassFreqs(chid, 200.0, 3100.0);
			WDSP.RXANBPSetFreqs(chid, 200.0, 3100.0);
			WDSP.SetRXASNBAOutputBandwidth(chid, 200.0, 3100.0);
			break;
		case 10:
			mode = DSPMode.SAM;
			WDSP.SetRXABandpassFreqs(chid, -10000.0, 10000.0);
			WDSP.RXANBPSetFreqs(chid, -10000.0, 10000.0);
			WDSP.SetRXASNBAOutputBandwidth(chid, -10000.0, 10000.0);
			break;
		}
		WDSP.SetRXAMode(chid, mode);
	}

	private void rxa_FormClosing(object sender, FormClosingEventArgs e)
	{
		Hide();
		e.Cancel = true;
		Common.SaveForm(this, base.Name);
	}

	private void panDisplay_Resize(object sender, EventArgs e)
	{
	}

	private void rxa_Resize(object sender, EventArgs e)
	{
		if (base.Width < console_basis_size.Width)
		{
			base.Width = console_basis_size.Width;
			return;
		}
		if (base.Height < console_basis_size.Height)
		{
			base.Height = console_basis_size.Height;
			return;
		}
		int num = base.Width - console_basis_size.Width;
		int num2 = Math.Max(base.Height - console_basis_size.Height, 0);
		panDisplay.pauseDisplayThread = true;
		panelPanDisplay.Size = new Size(gr_display_size_basis.Width + num, gr_display_size_basis.Height + num2);
		panDisplay.Size = new Size(pic_display_size_basis.Width + num, pic_display_size_basis.Height + num2);
		panDisplay.Init();
		panDisplay.UpdateGraphicsBuffer();
		panDisplay.pauseDisplayThread = false;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.rxa));
		this.panelPanDisplay = new System.Windows.Forms.PanelTS();
		this.panDisplay = new Thetis.PanDisplay();
		this.udRXAAGCGain = new System.Windows.Forms.NumericUpDownTS();
		this.labelTS5 = new System.Windows.Forms.LabelTS();
		this.labelTS4 = new System.Windows.Forms.LabelTS();
		this.udRXAMode = new System.Windows.Forms.NumericUpDownTS();
		this.labelTS3 = new System.Windows.Forms.LabelTS();
		this.udRXAVolume = new System.Windows.Forms.NumericUpDownTS();
		this.labelTS2 = new System.Windows.Forms.LabelTS();
		this.labelTS1 = new System.Windows.Forms.LabelTS();
		this.udRXAFreq = new System.Windows.Forms.NumericUpDownTS();
		this.panelPanDisplay.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.panDisplay).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udRXAAGCGain).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udRXAMode).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udRXAVolume).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udRXAFreq).BeginInit();
		base.SuspendLayout();
		this.panelPanDisplay.AutoScrollMargin = new System.Drawing.Size(0, 0);
		this.panelPanDisplay.AutoScrollMinSize = new System.Drawing.Size(0, 0);
		this.panelPanDisplay.BackColor = System.Drawing.Color.Transparent;
		this.panelPanDisplay.Controls.Add(this.panDisplay);
		this.panelPanDisplay.Location = new System.Drawing.Point(0, 0);
		this.panelPanDisplay.Name = "panelPanDisplay";
		this.panelPanDisplay.Size = new System.Drawing.Size(700, 285);
		this.panelPanDisplay.TabIndex = 10;
		this.panDisplay.AGCHang = new System.Drawing.Rectangle(0, 0, 0, 0);
		this.panDisplay.AGCKnee = new System.Drawing.Rectangle(0, 0, 0, 0);
		this.panDisplay.AlexPreampOffset = 0f;
		this.panDisplay.AverageOn = true;
		this.panDisplay.AvTau = 0.12;
		this.panDisplay.BackColor = System.Drawing.Color.Black;
		this.panDisplay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.panDisplay.BandEdgeColor = System.Drawing.Color.Red;
		this.panDisplay.ClickTuneFilter = true;
		this.panDisplay.ColorScheme = Thetis.ColorScheme.enhanced;
		this.panDisplay.cOnsole = null;
		this.panDisplay.CurrentClickTuneMode = Thetis.ClickTuneMode.Off;
		this.panDisplay.CurrentDisplayMode = Thetis.DisplayMode.PANADAPTER;
		this.panDisplay.CurrentModel = Thetis.HPSDRModel.HERMES;
		this.panDisplay.CurrentRegion = Thetis.FRSRegion.US;
		this.panDisplay.CWPitch = 600;
		this.panDisplay.DataLineColor = System.Drawing.Color.White;
		this.panDisplay.DataReady = false;
		this.panDisplay.DataType = 1;
		this.panDisplay.DBMScalePanRect = new System.Drawing.Rectangle(0, 0, 35, 15);
		this.panDisplay.DisplayAGCHangLine = true;
		this.panDisplay.DisplayAvgBlocks = 5;
		this.panDisplay.DisplayBackgroundColor = System.Drawing.Color.FromArgb(16, 0, 0, 0);
		this.panDisplay.DisplayCursorX = 0;
		this.panDisplay.DisplayCursorY = 0;
		this.panDisplay.DisplayDuplex = false;
		this.panDisplay.DisplayFilterColor = System.Drawing.Color.FromArgb(65, 255, 255, 255);
		this.panDisplay.DisplayID = 0;
		this.panDisplay.DisplayLabelAlign = Thetis.DisplayLabelAlignment.LEFT;
		this.panDisplay.DisplayLineWidth = 1f;
		this.panDisplay.FFTSize = 4096;
		this.panDisplay.FilterRect = new System.Drawing.Rectangle(0, 0, 0, 0);
		this.panDisplay.FrameRate = 15;
		this.panDisplay.FREQ = 0.0;
		this.panDisplay.FreqDiff = 0;
		this.panDisplay.FreqOffset = 0.0;
		this.panDisplay.FreqRulerPosition = 0.5f;
		this.panDisplay.FreqScalePanRect = new System.Drawing.Rectangle(0, 0, 0, 0);
		this.panDisplay.GridColor = System.Drawing.Color.FromArgb(42, 255, 255, 255);
		this.panDisplay.GridControl = true;
		this.panDisplay.GridPenDark = System.Drawing.Color.FromArgb(16, 255, 255, 255);
		this.panDisplay.GridTextColor = System.Drawing.Color.Yellow;
		this.panDisplay.GridZeroColor = System.Drawing.Color.Red;
		this.panDisplay.HGridColor = System.Drawing.Color.FromArgb(16, 255, 255, 255);
		this.panDisplay.HighFreq = 0;
		this.panDisplay.HighSWR = false;
		this.panDisplay.KaiserPi = 14.0;
		this.panDisplay.LinCor = 2;
		this.panDisplay.LinLogCor = -14;
		this.panDisplay.Location = new System.Drawing.Point(0, 0);
		this.panDisplay.LowFreq = 0;
		this.panDisplay.MaxX = 0f;
		this.panDisplay.MaxY = 0f;
		this.panDisplay.MOX = false;
		this.panDisplay.Name = "panDisplay";
		this.panDisplay.NReceivers = 2;
		this.panDisplay.PanFill = true;
		this.panDisplay.PanFillColor = System.Drawing.Color.FromArgb(100, 0, 0, 127);
		this.panDisplay.PanRect = new System.Drawing.Rectangle(0, 0, 0, 0);
		this.panDisplay.PanSlider = 0.5;
		this.panDisplay.PeakOn = false;
		this.panDisplay.Pixels = 2048;
		this.panDisplay.PreampOffset = 0f;
		this.panDisplay.ReverseWaterfall = false;
		this.panDisplay.RIT = 0;
		this.panDisplay.RX1HangSpectrumLine = true;
		this.panDisplay.RXDisplayCalOffset = 0f;
		this.panDisplay.RXDSPMode = Thetis.DSPMode.USB;
		this.panDisplay.RXFFTSizeOffset = 0f;
		this.panDisplay.RXFilterHigh = 0;
		this.panDisplay.RXFilterLow = 0;
		this.panDisplay.SampleRate = 192000;
		this.panDisplay.SecScalePanRect = new System.Drawing.Rectangle(0, 0, 0, 0);
		this.panDisplay.ShowAGC = true;
		this.panDisplay.ShowCTHLine = false;
		this.panDisplay.ShowCWZeroLine = false;
		this.panDisplay.ShowFreqOffset = false;
		this.panDisplay.Size = new System.Drawing.Size(700, 285);
		this.panDisplay.SpectrumGridMax = -50;
		this.panDisplay.SpectrumGridMin = -170;
		this.panDisplay.SpectrumGridStep = 10;
		this.panDisplay.SpectrumLine = true;
		this.panDisplay.SplitDisplay = false;
		this.panDisplay.SplitEnabled = false;
		this.panDisplay.SubRX1Enabled = false;
		this.panDisplay.SubRXFilterColor = System.Drawing.Color.Blue;
		this.panDisplay.SubRXZeroLine = System.Drawing.Color.LightSkyBlue;
		this.panDisplay.TabIndex = 0;
		this.panDisplay.TabStop = false;
		this.panDisplay.Target = null;
		this.panDisplay.TopSize = 0;
		this.panDisplay.VFOASub = 0L;
		this.panDisplay.VFOHz = 10000000L;
		this.panDisplay.WaterfallAGC = false;
		this.panDisplay.WaterfallAvgBlocks = 18;
		this.panDisplay.WaterfallDataReady = false;
		this.panDisplay.WaterfallHighColor = System.Drawing.Color.Yellow;
		this.panDisplay.WaterfallHighThreshold = -80f;
		this.panDisplay.WaterfallLowColor = System.Drawing.Color.Black;
		this.panDisplay.WaterfallLowThreshold = -130f;
		this.panDisplay.WaterfallMidColor = System.Drawing.Color.Red;
		this.panDisplay.WaterfallRect = new System.Drawing.Rectangle(0, 0, 0, 0);
		this.panDisplay.WaterfallUpdatePeriod = 100;
		this.panDisplay.WindowType = 6;
		this.panDisplay.ZoomFactor = 0.5;
		this.panDisplay.Resize += new System.EventHandler(panDisplay_Resize);
		this.udRXAAGCGain.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udRXAAGCGain.Location = new System.Drawing.Point(60, 343);
		this.udRXAAGCGain.Maximum = new decimal(new int[4] { 120, 0, 0, 0 });
		this.udRXAAGCGain.Minimum = new decimal(new int[4] { 20, 0, 0, -2147483648 });
		this.udRXAAGCGain.Name = "udRXAAGCGain";
		this.udRXAAGCGain.Size = new System.Drawing.Size(120, 20);
		this.udRXAAGCGain.TabIndex = 9;
		this.udRXAAGCGain.TinyStep = false;
		this.udRXAAGCGain.Value = new decimal(new int[4] { 100, 0, 0, 0 });
		this.udRXAAGCGain.ValueChanged += new System.EventHandler(udRXAAGCGain_ValueChanged);
		this.labelTS5.AutoSize = true;
		this.labelTS5.Image = null;
		this.labelTS5.Location = new System.Drawing.Point(-3, 345);
		this.labelTS5.Name = "labelTS5";
		this.labelTS5.Size = new System.Drawing.Size(54, 13);
		this.labelTS5.TabIndex = 8;
		this.labelTS5.Text = "AGC Gain";
		this.labelTS4.AutoSize = true;
		this.labelTS4.Image = null;
		this.labelTS4.Location = new System.Drawing.Point(55, 419);
		this.labelTS4.Name = "labelTS4";
		this.labelTS4.Size = new System.Drawing.Size(132, 13);
		this.labelTS4.TabIndex = 7;
		this.labelTS4.Text = "(LSB=0, USB=1, SAM=10)";
		this.udRXAMode.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udRXAMode.Location = new System.Drawing.Point(60, 395);
		this.udRXAMode.Maximum = new decimal(new int[4] { 10, 0, 0, 0 });
		this.udRXAMode.Minimum = new decimal(new int[4]);
		this.udRXAMode.Name = "udRXAMode";
		this.udRXAMode.Size = new System.Drawing.Size(120, 20);
		this.udRXAMode.TabIndex = 6;
		this.udRXAMode.TinyStep = false;
		this.udRXAMode.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udRXAMode.ValueChanged += new System.EventHandler(udRXAMode_ValueChanged);
		this.labelTS3.AutoSize = true;
		this.labelTS3.Image = null;
		this.labelTS3.Location = new System.Drawing.Point(-3, 397);
		this.labelTS3.Name = "labelTS3";
		this.labelTS3.Size = new System.Drawing.Size(34, 13);
		this.labelTS3.TabIndex = 5;
		this.labelTS3.Text = "Mode";
		this.udRXAVolume.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udRXAVolume.Location = new System.Drawing.Point(60, 369);
		this.udRXAVolume.Maximum = new decimal(new int[4] { 100, 0, 0, 0 });
		this.udRXAVolume.Minimum = new decimal(new int[4]);
		this.udRXAVolume.Name = "udRXAVolume";
		this.udRXAVolume.Size = new System.Drawing.Size(120, 20);
		this.udRXAVolume.TabIndex = 4;
		this.udRXAVolume.TinyStep = false;
		this.udRXAVolume.Value = new decimal(new int[4] { 25, 0, 0, 0 });
		this.udRXAVolume.ValueChanged += new System.EventHandler(udRXAVolume_ValueChanged);
		this.labelTS2.AutoSize = true;
		this.labelTS2.Image = null;
		this.labelTS2.Location = new System.Drawing.Point(-3, 371);
		this.labelTS2.Name = "labelTS2";
		this.labelTS2.Size = new System.Drawing.Size(42, 13);
		this.labelTS2.TabIndex = 3;
		this.labelTS2.Text = "Volume";
		this.labelTS1.AutoSize = true;
		this.labelTS1.Image = null;
		this.labelTS1.Location = new System.Drawing.Point(-3, 319);
		this.labelTS1.Name = "labelTS1";
		this.labelTS1.Size = new System.Drawing.Size(57, 13);
		this.labelTS1.TabIndex = 1;
		this.labelTS1.Text = "Frequency";
		this.udRXAFreq.DecimalPlaces = 6;
		this.udRXAFreq.Increment = new decimal(new int[4] { 1, 0, 0, 393216 });
		this.udRXAFreq.Location = new System.Drawing.Point(60, 317);
		this.udRXAFreq.Maximum = new decimal(new int[4] { 54, 0, 0, 0 });
		this.udRXAFreq.Minimum = new decimal(new int[4]);
		this.udRXAFreq.Name = "udRXAFreq";
		this.udRXAFreq.Size = new System.Drawing.Size(120, 20);
		this.udRXAFreq.TabIndex = 0;
		this.udRXAFreq.TinyStep = false;
		this.udRXAFreq.Value = new decimal(new int[4] { 142, 0, 0, 65536 });
		this.udRXAFreq.ValueChanged += new System.EventHandler(udRXAFreq_ValueChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(721, 451);
		base.Controls.Add(this.panelPanDisplay);
		base.Controls.Add(this.udRXAAGCGain);
		base.Controls.Add(this.labelTS5);
		base.Controls.Add(this.labelTS4);
		base.Controls.Add(this.udRXAMode);
		base.Controls.Add(this.labelTS3);
		base.Controls.Add(this.udRXAVolume);
		base.Controls.Add(this.labelTS2);
		base.Controls.Add(this.labelTS1);
		base.Controls.Add(this.udRXAFreq);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "rxa";
		this.Text = "rxa";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(rxa_FormClosing);
		base.Resize += new System.EventHandler(rxa_Resize);
		this.panelPanDisplay.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.panDisplay).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udRXAAGCGain).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udRXAMode).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udRXAVolume).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udRXAFreq).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
