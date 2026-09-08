using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class wideband : Form
{
	private ToolStripMenuItem ContextMenuToolStripMenuItem;

	private Color ContextMenuToolStripMenuItemFontColor = Color.Empty;

	private IContainer components;

	private PanelTS panelwbDisplay;

	private wbDisplay wbdisplay;

	private ContextMenuStrip contextMenuStripWideBand;

	private ToolStripMenuItem canceltoolStripMenuItem1;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem wbAvgtoolStripMenuItem;

	private ToolStripMenuItem wbUpdatetoolStripMenuItem;

	private ToolStripComboBox wbUpdatetoolStripComboBox;

	private ToolStripMenuItem wbFrameSizetoolStripMenuItem;

	private ToolStripComboBox wbFrameSizetoolStripComboBox;

	public wbDisplay WBdisplay
	{
		get
		{
			return wbdisplay;
		}
		set
		{
			wbdisplay = value;
		}
	}

	public wideband(int i)
	{
		InitializeComponent();
		MaximumSize = new Size(4096, 4096);
		Common.DoubleBufferAll(this, enabled: true);
		wbdisplay.init = true;
		wbdisplay.ADC = i;
		wbdisplay.create_wideband(i);
		wbdisplay.initWideband();
		GetWideBand();
	}

	private void wideband_Resize(object sender, EventArgs e)
	{
		wbdisplay.pauseDisplayThread = true;
		if (base.WindowState != FormWindowState.Minimized)
		{
			wbdisplay.Init();
			wbdisplay.UpdateGraphicsBuffer();
			wbdisplay.pauseDisplayThread = false;
		}
	}

	private void wideband_FormClosing(object sender, FormClosingEventArgs e)
	{
		wbdisplay.Cancel_Display();
		NetworkIO.SetWBEnable(0, 0);
		Console.getConsole().wbClosing();
		Hide();
		e.Cancel = true;
		SaveWideBand();
	}

	private void contextMenuStripWideBand_Opening(object sender, CancelEventArgs e)
	{
		if (wbdisplay.mouseRegion == DisplayRegion.dBmScalePanadapterRegion)
		{
			e.Cancel = true;
		}
		wbAvgtoolStripMenuItem.Checked = wbdisplay.AverageOn;
		wbUpdatetoolStripComboBox.Text = wbdisplay.UpdateRate;
		wbFrameSizetoolStripComboBox.Text = wbdisplay.FrameSize;
	}

	private void ToolStripMenuItem_MouseEnter(object sender, EventArgs e)
	{
		if (sender != null && !(sender.GetType() != typeof(ToolStripMenuItem)))
		{
			ToolStripMenuItem toolStripMenuItem = (ContextMenuToolStripMenuItem = (ToolStripMenuItem)sender);
			ContextMenuToolStripMenuItemFontColor = toolStripMenuItem.ForeColor;
			toolStripMenuItem.ForeColor = Color.Black;
		}
	}

	private void ToolStripMenuItem_MouseLeave(object sender, EventArgs e)
	{
		if (sender != null && !(sender.GetType() != typeof(ToolStripMenuItem)))
		{
			ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
			if (ContextMenuToolStripMenuItem != null)
			{
				toolStripMenuItem.ForeColor = ContextMenuToolStripMenuItemFontColor;
				ContextMenuToolStripMenuItem = null;
				ContextMenuToolStripMenuItemFontColor = Color.Empty;
			}
		}
	}

	private void ContextMenuStrip_Closing(object sender, ToolStripDropDownClosingEventArgs e)
	{
		if (sender != null && !(sender.GetType() != typeof(ContextMenuStrip)))
		{
			_ = (ContextMenuStrip)sender;
			if (ContextMenuToolStripMenuItem != null)
			{
				ContextMenuToolStripMenuItem.ForeColor = ContextMenuToolStripMenuItemFontColor;
				ContextMenuToolStripMenuItem = null;
				ContextMenuToolStripMenuItemFontColor = Color.Empty;
			}
		}
	}

	private void wbAvgtoolStripMenuItem_Click(object sender, EventArgs e)
	{
		wbdisplay.AverageOn = !wbAvgtoolStripMenuItem.Checked;
	}

	private void wbdisplay_Resize(object sender, EventArgs e)
	{
	}

	private void wbUpdatetoolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (wbUpdatetoolStripComboBox.SelectedIndex >= 0)
		{
			wbdisplay.UpdateRate = wbUpdatetoolStripComboBox.Text;
		}
	}

	private void wbFrameSizetoolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (wbFrameSizetoolStripComboBox.SelectedIndex >= 0)
		{
			wbdisplay.FrameSize = wbFrameSizetoolStripComboBox.Text;
		}
	}

	public void SaveWideBand()
	{
		List<string> list = new List<string>();
		list.Add("average_on/" + wbdisplay.AverageOn);
		list.Add("update_rate/" + wbdisplay.UpdateRate);
		list.Add("frame_size/" + wbdisplay.FrameSize);
		list.Add("spectrum_grid_max/" + wbdisplay.SpectrumGridMax);
		list.Add("spectrum_grid_min/" + wbdisplay.SpectrumGridMin);
		list.Add("Top/" + base.Top);
		list.Add("Left/" + base.Left);
		list.Add("Width/" + base.Width);
		list.Add("Height/" + base.Height);
		DB.SaveVars("WideBand", list);
	}

	public void GetWideBand()
	{
		List<string> vars = DB.GetVars("WideBand");
		vars.Sort();
		foreach (string item in vars)
		{
			string[] array = item.Split('/');
			if (array.Length > 2)
			{
				for (int i = 2; i < array.Length; i++)
				{
					ref string reference = ref array[1];
					reference = reference + "/" + array[i];
				}
			}
			string text = array[0];
			string text2 = array[1];
			switch (text)
			{
			case "Top":
			{
				base.StartPosition = FormStartPosition.Manual;
				int top = int.Parse(text2);
				base.Top = top;
				break;
			}
			case "Left":
			{
				base.StartPosition = FormStartPosition.Manual;
				int left = int.Parse(text2);
				base.Left = left;
				break;
			}
			case "Width":
			{
				int num2 = int.Parse(text2);
				base.Width = num2;
				break;
			}
			case "Height":
			{
				int num = int.Parse(text2);
				base.Height = num;
				break;
			}
			case "average_on":
				wbdisplay.AverageOn = bool.Parse(text2);
				break;
			case "update_rate":
				wbdisplay.UpdateRate = text2;
				break;
			case "frame_size":
				wbdisplay.FrameSize = text2;
				break;
			case "spectrum_grid_max":
				wbdisplay.SpectrumGridMax = int.Parse(text2);
				break;
			case "spectrum_grid_min":
				wbdisplay.SpectrumGridMin = int.Parse(text2);
				break;
			}
		}
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
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.wideband));
		this.contextMenuStripWideBand = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.canceltoolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.wbAvgtoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.wbUpdatetoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.wbUpdatetoolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
		this.wbFrameSizetoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.wbFrameSizetoolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
		this.panelwbDisplay = new System.Windows.Forms.PanelTS();
		this.wbdisplay = new Thetis.wbDisplay();
		this.contextMenuStripWideBand.SuspendLayout();
		this.panelwbDisplay.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.wbdisplay).BeginInit();
		base.SuspendLayout();
		this.contextMenuStripWideBand.BackColor = System.Drawing.Color.Black;
		this.contextMenuStripWideBand.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.contextMenuStripWideBand.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.contextMenuStripWideBand.Items.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.canceltoolStripMenuItem1, this.toolStripSeparator1, this.wbAvgtoolStripMenuItem, this.wbUpdatetoolStripMenuItem, this.wbFrameSizetoolStripMenuItem });
		this.contextMenuStripWideBand.Name = "contextMenuStripWideBand";
		this.contextMenuStripWideBand.ShowCheckMargin = true;
		this.contextMenuStripWideBand.ShowImageMargin = false;
		this.contextMenuStripWideBand.Size = new System.Drawing.Size(136, 98);
		this.contextMenuStripWideBand.Closing += new System.Windows.Forms.ToolStripDropDownClosingEventHandler(ContextMenuStrip_Closing);
		this.contextMenuStripWideBand.Opening += new System.ComponentModel.CancelEventHandler(contextMenuStripWideBand_Opening);
		this.canceltoolStripMenuItem1.BackColor = System.Drawing.Color.Black;
		this.canceltoolStripMenuItem1.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.canceltoolStripMenuItem1.ForeColor = System.Drawing.Color.White;
		this.canceltoolStripMenuItem1.Name = "canceltoolStripMenuItem1";
		this.canceltoolStripMenuItem1.Size = new System.Drawing.Size(135, 22);
		this.canceltoolStripMenuItem1.Text = "Cancel";
		this.canceltoolStripMenuItem1.MouseEnter += new System.EventHandler(ToolStripMenuItem_MouseEnter);
		this.canceltoolStripMenuItem1.MouseLeave += new System.EventHandler(ToolStripMenuItem_MouseLeave);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(132, 6);
		this.wbAvgtoolStripMenuItem.BackColor = System.Drawing.Color.Black;
		this.wbAvgtoolStripMenuItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.wbAvgtoolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.wbAvgtoolStripMenuItem.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.wbAvgtoolStripMenuItem.ForeColor = System.Drawing.Color.Goldenrod;
		this.wbAvgtoolStripMenuItem.Name = "wbAvgtoolStripMenuItem";
		this.wbAvgtoolStripMenuItem.Size = new System.Drawing.Size(135, 22);
		this.wbAvgtoolStripMenuItem.Text = "Average";
		this.wbAvgtoolStripMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.wbAvgtoolStripMenuItem.Click += new System.EventHandler(wbAvgtoolStripMenuItem_Click);
		this.wbAvgtoolStripMenuItem.MouseEnter += new System.EventHandler(ToolStripMenuItem_MouseEnter);
		this.wbAvgtoolStripMenuItem.MouseLeave += new System.EventHandler(ToolStripMenuItem_MouseLeave);
		this.wbUpdatetoolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
		this.wbUpdatetoolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.wbUpdatetoolStripComboBox });
		this.wbUpdatetoolStripMenuItem.ForeColor = System.Drawing.Color.Goldenrod;
		this.wbUpdatetoolStripMenuItem.Name = "wbUpdatetoolStripMenuItem";
		this.wbUpdatetoolStripMenuItem.Size = new System.Drawing.Size(135, 22);
		this.wbUpdatetoolStripMenuItem.Text = "Update Rate";
		this.wbUpdatetoolStripMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.wbUpdatetoolStripMenuItem.MouseEnter += new System.EventHandler(ToolStripMenuItem_MouseEnter);
		this.wbUpdatetoolStripMenuItem.MouseLeave += new System.EventHandler(ToolStripMenuItem_MouseLeave);
		this.wbUpdatetoolStripComboBox.AutoSize = false;
		this.wbUpdatetoolStripComboBox.DropDownWidth = 20;
		this.wbUpdatetoolStripComboBox.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.wbUpdatetoolStripComboBox.Items.AddRange(new object[11]
		{
			"10", "15", "20", "25", "30", "35", "40", "45", "50", "55",
			"60"
		});
		this.wbUpdatetoolStripComboBox.Name = "wbUpdatetoolStripComboBox";
		this.wbUpdatetoolStripComboBox.Size = new System.Drawing.Size(40, 21);
		this.wbUpdatetoolStripComboBox.SelectedIndexChanged += new System.EventHandler(wbUpdatetoolStripComboBox_SelectedIndexChanged);
		this.wbUpdatetoolStripComboBox.MouseEnter += new System.EventHandler(ToolStripMenuItem_MouseEnter);
		this.wbUpdatetoolStripComboBox.MouseLeave += new System.EventHandler(ToolStripMenuItem_MouseLeave);
		this.wbFrameSizetoolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.wbFrameSizetoolStripComboBox });
		this.wbFrameSizetoolStripMenuItem.ForeColor = System.Drawing.Color.Goldenrod;
		this.wbFrameSizetoolStripMenuItem.Name = "wbFrameSizetoolStripMenuItem";
		this.wbFrameSizetoolStripMenuItem.Size = new System.Drawing.Size(135, 22);
		this.wbFrameSizetoolStripMenuItem.Text = "Frame Size";
		this.wbFrameSizetoolStripMenuItem.MouseEnter += new System.EventHandler(ToolStripMenuItem_MouseEnter);
		this.wbFrameSizetoolStripMenuItem.MouseLeave += new System.EventHandler(ToolStripMenuItem_MouseLeave);
		this.wbFrameSizetoolStripComboBox.AutoSize = false;
		this.wbFrameSizetoolStripComboBox.DropDownWidth = 50;
		this.wbFrameSizetoolStripComboBox.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.wbFrameSizetoolStripComboBox.Items.AddRange(new object[2] { "8", "32" });
		this.wbFrameSizetoolStripComboBox.Name = "wbFrameSizetoolStripComboBox";
		this.wbFrameSizetoolStripComboBox.Size = new System.Drawing.Size(40, 21);
		this.wbFrameSizetoolStripComboBox.SelectedIndexChanged += new System.EventHandler(wbFrameSizetoolStripComboBox_SelectedIndexChanged);
		this.wbFrameSizetoolStripComboBox.MouseEnter += new System.EventHandler(ToolStripMenuItem_MouseEnter);
		this.wbFrameSizetoolStripComboBox.MouseLeave += new System.EventHandler(ToolStripMenuItem_MouseLeave);
		this.panelwbDisplay.AutoScrollMargin = new System.Drawing.Size(0, 0);
		this.panelwbDisplay.AutoScrollMinSize = new System.Drawing.Size(0, 0);
		this.panelwbDisplay.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		this.panelwbDisplay.BackColor = System.Drawing.Color.Transparent;
		this.panelwbDisplay.Controls.Add(this.wbdisplay);
		this.panelwbDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panelwbDisplay.Location = new System.Drawing.Point(0, 0);
		this.panelwbDisplay.Name = "panelwbDisplay";
		this.panelwbDisplay.Size = new System.Drawing.Size(801, 302);
		this.panelwbDisplay.TabIndex = 0;
		this.wbdisplay.ADC = 0;
		this.wbdisplay.AGCHang = new System.Drawing.Rectangle(0, 0, 0, 0);
		this.wbdisplay.AGCKnee = new System.Drawing.Rectangle(0, 0, 0, 0);
		this.wbdisplay.AlexPreampOffset = 0f;
		this.wbdisplay.AverageOn = true;
		this.wbdisplay.AvTau = 0.12;
		this.wbdisplay.BackColor = System.Drawing.Color.Black;
		this.wbdisplay.BandEdgeColor = System.Drawing.Color.Red;
		this.wbdisplay.ClickTuneFilter = true;
		this.wbdisplay.ColorScheme = Thetis.ColorScheme.enhanced;
		this.wbdisplay.cOnsole = null;
		this.wbdisplay.ContextMenuStrip = this.contextMenuStripWideBand;
		this.wbdisplay.CurrentClickTuneMode = Thetis.ClickTuneMode.Off;
		this.wbdisplay.CurrentDisplayMode = Thetis.DisplayMode.PANADAPTER;
		this.wbdisplay.CurrentModel = Thetis.HPSDRModel.HERMES;
		this.wbdisplay.CurrentRegion = Thetis.FRSRegion.US;
		this.wbdisplay.CWPitch = 600;
		this.wbdisplay.DataLineColor = System.Drawing.Color.White;
		this.wbdisplay.DataReady = false;
		this.wbdisplay.DBMScalePanRect = new System.Drawing.Rectangle(65, 0, 35, 30);
		this.wbdisplay.DisplayAGCHangLine = true;
		this.wbdisplay.DisplayAvgBlocks = 5;
		this.wbdisplay.DisplayBackgroundColor = System.Drawing.Color.Black;
		this.wbdisplay.DisplayCursorX = 0;
		this.wbdisplay.DisplayCursorY = 0;
		this.wbdisplay.DisplayDuplex = false;
		this.wbdisplay.DisplayFilterColor = System.Drawing.Color.FromArgb(65, 255, 255, 255);
		this.wbdisplay.DisplayLabelAlign = Thetis.DisplayLabelAlignment.LEFT;
		this.wbdisplay.DisplayLineWidth = 1f;
		this.wbdisplay.Dock = System.Windows.Forms.DockStyle.Fill;
		this.wbdisplay.FFTSize = 16384;
		this.wbdisplay.FilterRect = new System.Drawing.Rectangle(0, 0, 0, 0);
		this.wbdisplay.FrameRate = 15;
		this.wbdisplay.FrameSize = "32";
		this.wbdisplay.FREQ = 0.0;
		this.wbdisplay.FreqDiff = 0;
		this.wbdisplay.FreqRulerPosition = 1f;
		this.wbdisplay.FreqScalePanRect = new System.Drawing.Rectangle(0, 282, 801, 20);
		this.wbdisplay.GridColor = System.Drawing.Color.FromArgb(65, 255, 255, 255);
		this.wbdisplay.GridControl = true;
		this.wbdisplay.GridPenDark = System.Drawing.Color.FromArgb(65, 255, 255, 255);
		this.wbdisplay.GridTextColor = System.Drawing.Color.Yellow;
		this.wbdisplay.GridZeroColor = System.Drawing.Color.Red;
		this.wbdisplay.HGridColor = System.Drawing.Color.White;
		this.wbdisplay.HighSWR = false;
		this.wbdisplay.KaiserPi = 14.0;
		this.wbdisplay.LinCor = 2;
		this.wbdisplay.LinLogCor = -14;
		this.wbdisplay.Location = new System.Drawing.Point(0, 0);
		this.wbdisplay.MaxX = 0f;
		this.wbdisplay.MaxY = 0f;
		this.wbdisplay.MOX = false;
		this.wbdisplay.Name = "wbdisplay";
		this.wbdisplay.NReceivers = 2;
		this.wbdisplay.PanFill = true;
		this.wbdisplay.PanFillColor = System.Drawing.Color.FromArgb(100, 0, 0, 127);
		this.wbdisplay.PanRect = new System.Drawing.Rectangle(0, 0, 801, 282);
		this.wbdisplay.PanSlider = 0.5;
		this.wbdisplay.PeakOn = false;
		this.wbdisplay.Pixels = 801;
		this.wbdisplay.PreampOffset = 0f;
		this.wbdisplay.ReverseWaterfall = false;
		this.wbdisplay.RIT = 0;
		this.wbdisplay.RX1HangSpectrumLine = true;
		this.wbdisplay.RXDisplayCalOffset = -40.1f;
		this.wbdisplay.RXDSPMode = Thetis.DSPMode.USB;
		this.wbdisplay.RXFFTSizeOffset = 0f;
		this.wbdisplay.RXFilterHigh = 0;
		this.wbdisplay.RXFilterLow = 0;
		this.wbdisplay.SampleRate = 122880000;
		this.wbdisplay.SecScalePanRect = new System.Drawing.Rectangle(0, 302, 45, 0);
		this.wbdisplay.ShowAGC = true;
		this.wbdisplay.ShowCTHLine = false;
		this.wbdisplay.ShowCWZeroLine = false;
		this.wbdisplay.ShowFreqOffset = false;
		this.wbdisplay.Size = new System.Drawing.Size(801, 302);
		this.wbdisplay.SpectrumGridMax = -50;
		this.wbdisplay.SpectrumGridMin = -170;
		this.wbdisplay.SpectrumGridStep = 10;
		this.wbdisplay.SpectrumLine = true;
		this.wbdisplay.SplitDisplay = false;
		this.wbdisplay.SplitEnabled = false;
		this.wbdisplay.SubRX1Enabled = false;
		this.wbdisplay.SubRXFilterColor = System.Drawing.Color.Blue;
		this.wbdisplay.SubRXZeroLine = System.Drawing.Color.LightSkyBlue;
		this.wbdisplay.TabIndex = 0;
		this.wbdisplay.TabStop = false;
		this.wbdisplay.Target = null;
		this.wbdisplay.TopSize = 0;
		this.wbdisplay.UpdateRate = "15";
		this.wbdisplay.VFOASub = 0L;
		this.wbdisplay.VFOHz = 10000000L;
		this.wbdisplay.WaterfallAGC = false;
		this.wbdisplay.WaterfallAvgBlocks = 18;
		this.wbdisplay.WaterfallDataReady = false;
		this.wbdisplay.WaterfallHighColor = System.Drawing.Color.Yellow;
		this.wbdisplay.WaterfallHighThreshold = -80f;
		this.wbdisplay.WaterfallLowColor = System.Drawing.Color.Black;
		this.wbdisplay.WaterfallLowThreshold = -130f;
		this.wbdisplay.WaterfallMidColor = System.Drawing.Color.Red;
		this.wbdisplay.WaterfallRect = new System.Drawing.Rectangle(0, 302, 801, 1);
		this.wbdisplay.WaterfallUpdatePeriod = 100;
		this.wbdisplay.WindowType = 6;
		this.wbdisplay.ZoomSlider = 0.0;
		this.wbdisplay.Resize += new System.EventHandler(wbdisplay_Resize);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(801, 302);
		base.Controls.Add(this.panelwbDisplay);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "wideband";
		this.Text = "wideband";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(wideband_FormClosing);
		base.Resize += new System.EventHandler(wideband_Resize);
		this.contextMenuStripWideBand.ResumeLayout(false);
		this.panelwbDisplay.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.wbdisplay).EndInit();
		base.ResumeLayout(false);
	}
}
