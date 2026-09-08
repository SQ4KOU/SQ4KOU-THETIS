using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Thetis;

public class DisplaySettingsForm : Form
{
	private ButtonTS btnClose;

	private Console console;

	private ComboBoxTS comboRX1Meter;

	private LabelTS labelTS1;

	private ComboBoxTS comboRX2Meter;

	private LabelTS labelTS2;

	private ComboBoxTS comboRX1Display;

	private ComboBoxTS comboRX2Display;

	private LabelTS labelTS3;

	private LabelTS labelTS4;

	private ComboBoxTS comboTXMeter;

	private LabelTS labelTS5;

	private CheckBoxTS chkRX1Avg;

	private CheckBoxTS chkRX1Peak;

	private CheckBoxTS chkRX2Avg;

	private CheckBoxTS chkRX2Peak;

	private IContainer components;

	public DisplaySettingsForm(Console c)
	{
		InitializeComponent();
		console = c;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.DisplaySettingsForm));
		this.labelTS5 = new System.Windows.Forms.LabelTS();
		this.comboTXMeter = new System.Windows.Forms.ComboBoxTS();
		this.labelTS4 = new System.Windows.Forms.LabelTS();
		this.labelTS3 = new System.Windows.Forms.LabelTS();
		this.comboRX2Display = new System.Windows.Forms.ComboBoxTS();
		this.comboRX1Display = new System.Windows.Forms.ComboBoxTS();
		this.labelTS2 = new System.Windows.Forms.LabelTS();
		this.comboRX2Meter = new System.Windows.Forms.ComboBoxTS();
		this.labelTS1 = new System.Windows.Forms.LabelTS();
		this.comboRX1Meter = new System.Windows.Forms.ComboBoxTS();
		this.btnClose = new System.Windows.Forms.ButtonTS();
		this.chkRX1Avg = new System.Windows.Forms.CheckBoxTS();
		this.chkRX1Peak = new System.Windows.Forms.CheckBoxTS();
		this.chkRX2Avg = new System.Windows.Forms.CheckBoxTS();
		this.chkRX2Peak = new System.Windows.Forms.CheckBoxTS();
		base.SuspendLayout();
		this.labelTS5.AutoSize = true;
		this.labelTS5.ForeColor = System.Drawing.SystemColors.ControlLight;
		this.labelTS5.Image = null;
		this.labelTS5.Location = new System.Drawing.Point(323, 21);
		this.labelTS5.Name = "labelTS5";
		this.labelTS5.Size = new System.Drawing.Size(51, 13);
		this.labelTS5.TabIndex = 10;
		this.labelTS5.Text = "TX Meter";
		this.comboTXMeter.FormattingEnabled = true;
		this.comboTXMeter.Location = new System.Drawing.Point(323, 40);
		this.comboTXMeter.Name = "comboTXMeter";
		this.comboTXMeter.Size = new System.Drawing.Size(121, 21);
		this.comboTXMeter.TabIndex = 9;
		this.comboTXMeter.SelectedIndexChanged += new System.EventHandler(ComboTXMeter_SelectedIndexChanged);
		this.labelTS4.AutoSize = true;
		this.labelTS4.ForeColor = System.Drawing.SystemColors.ControlLight;
		this.labelTS4.Image = null;
		this.labelTS4.Location = new System.Drawing.Point(174, 88);
		this.labelTS4.Name = "labelTS4";
		this.labelTS4.Size = new System.Drawing.Size(95, 13);
		this.labelTS4.TabIndex = 8;
		this.labelTS4.Text = "RX2 Display Mode";
		this.labelTS3.AutoSize = true;
		this.labelTS3.ForeColor = System.Drawing.SystemColors.ControlLight;
		this.labelTS3.Image = null;
		this.labelTS3.Location = new System.Drawing.Point(19, 88);
		this.labelTS3.Name = "labelTS3";
		this.labelTS3.Size = new System.Drawing.Size(95, 13);
		this.labelTS3.TabIndex = 7;
		this.labelTS3.Text = "RX1 Display Mode";
		this.comboRX2Display.FormattingEnabled = true;
		this.comboRX2Display.Location = new System.Drawing.Point(174, 107);
		this.comboRX2Display.Name = "comboRX2Display";
		this.comboRX2Display.Size = new System.Drawing.Size(121, 21);
		this.comboRX2Display.TabIndex = 6;
		this.comboRX2Display.SelectedIndexChanged += new System.EventHandler(ComboRX2Display_SelectedIndexChanged);
		this.comboRX1Display.FormattingEnabled = true;
		this.comboRX1Display.Location = new System.Drawing.Point(19, 107);
		this.comboRX1Display.Name = "comboRX1Display";
		this.comboRX1Display.Size = new System.Drawing.Size(121, 21);
		this.comboRX1Display.TabIndex = 5;
		this.comboRX1Display.SelectedIndexChanged += new System.EventHandler(ComboRX1Display_SelectedIndexChanged);
		this.labelTS2.AutoSize = true;
		this.labelTS2.ForeColor = System.Drawing.SystemColors.ControlLight;
		this.labelTS2.Image = null;
		this.labelTS2.Location = new System.Drawing.Point(174, 21);
		this.labelTS2.Name = "labelTS2";
		this.labelTS2.Size = new System.Drawing.Size(58, 13);
		this.labelTS2.TabIndex = 4;
		this.labelTS2.Text = "RX2 Meter";
		this.comboRX2Meter.FormattingEnabled = true;
		this.comboRX2Meter.Location = new System.Drawing.Point(174, 40);
		this.comboRX2Meter.Name = "comboRX2Meter";
		this.comboRX2Meter.Size = new System.Drawing.Size(116, 21);
		this.comboRX2Meter.TabIndex = 3;
		this.comboRX2Meter.SelectedIndexChanged += new System.EventHandler(ComboRX2Meter_SelectedIndexChanged);
		this.labelTS1.AutoSize = true;
		this.labelTS1.ForeColor = System.Drawing.SystemColors.ControlLight;
		this.labelTS1.Image = null;
		this.labelTS1.Location = new System.Drawing.Point(19, 21);
		this.labelTS1.Name = "labelTS1";
		this.labelTS1.Size = new System.Drawing.Size(58, 13);
		this.labelTS1.TabIndex = 2;
		this.labelTS1.Text = "RX1 Meter";
		this.comboRX1Meter.FormattingEnabled = true;
		this.comboRX1Meter.Location = new System.Drawing.Point(19, 40);
		this.comboRX1Meter.Name = "comboRX1Meter";
		this.comboRX1Meter.Size = new System.Drawing.Size(122, 21);
		this.comboRX1Meter.TabIndex = 1;
		this.comboRX1Meter.SelectedIndexChanged += new System.EventHandler(ComboRX1Meter_SelectedIndexChanged);
		this.btnClose.Image = null;
		this.btnClose.Location = new System.Drawing.Point(326, 153);
		this.btnClose.Name = "btnClose";
		this.btnClose.Selectable = true;
		this.btnClose.Size = new System.Drawing.Size(109, 41);
		this.btnClose.TabIndex = 0;
		this.btnClose.Text = "Close";
		this.btnClose.UseVisualStyleBackColor = true;
		this.btnClose.Click += new System.EventHandler(BtnClose_Click);
		this.chkRX1Avg.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkRX1Avg.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkRX1Avg.Image = null;
		this.chkRX1Avg.Location = new System.Drawing.Point(19, 153);
		this.chkRX1Avg.Name = "chkRX1Avg";
		this.chkRX1Avg.Size = new System.Drawing.Size(50, 41);
		this.chkRX1Avg.TabIndex = 11;
		this.chkRX1Avg.Text = "Avg";
		this.chkRX1Avg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkRX1Avg.UseVisualStyleBackColor = true;
		this.chkRX1Avg.CheckedChanged += new System.EventHandler(ChkRX1Avg_CheckedChanged);
		this.chkRX1Peak.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkRX1Peak.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkRX1Peak.Image = null;
		this.chkRX1Peak.Location = new System.Drawing.Point(90, 153);
		this.chkRX1Peak.Name = "chkRX1Peak";
		this.chkRX1Peak.Size = new System.Drawing.Size(50, 41);
		this.chkRX1Peak.TabIndex = 12;
		this.chkRX1Peak.Text = "Peak";
		this.chkRX1Peak.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkRX1Peak.UseVisualStyleBackColor = true;
		this.chkRX1Peak.CheckedChanged += new System.EventHandler(ChkRX1Peak_CheckedChanged);
		this.chkRX2Avg.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkRX2Avg.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkRX2Avg.Image = null;
		this.chkRX2Avg.Location = new System.Drawing.Point(174, 153);
		this.chkRX2Avg.Name = "chkRX2Avg";
		this.chkRX2Avg.Size = new System.Drawing.Size(50, 41);
		this.chkRX2Avg.TabIndex = 13;
		this.chkRX2Avg.Text = "Avg";
		this.chkRX2Avg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkRX2Avg.UseVisualStyleBackColor = true;
		this.chkRX2Avg.CheckedChanged += new System.EventHandler(ChkRX2Avg_CheckedChanged);
		this.chkRX2Peak.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkRX2Peak.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkRX2Peak.Image = null;
		this.chkRX2Peak.Location = new System.Drawing.Point(245, 153);
		this.chkRX2Peak.Name = "chkRX2Peak";
		this.chkRX2Peak.Size = new System.Drawing.Size(50, 41);
		this.chkRX2Peak.TabIndex = 14;
		this.chkRX2Peak.Text = "Peak";
		this.chkRX2Peak.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkRX2Peak.UseVisualStyleBackColor = true;
		this.chkRX2Peak.CheckedChanged += new System.EventHandler(ChkRX2Peak_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.FromArgb(80, 80, 80);
		base.ClientSize = new System.Drawing.Size(454, 204);
		base.Controls.Add(this.chkRX2Peak);
		base.Controls.Add(this.chkRX2Avg);
		base.Controls.Add(this.chkRX1Peak);
		base.Controls.Add(this.chkRX1Avg);
		base.Controls.Add(this.labelTS5);
		base.Controls.Add(this.comboTXMeter);
		base.Controls.Add(this.labelTS4);
		base.Controls.Add(this.labelTS3);
		base.Controls.Add(this.comboRX2Display);
		base.Controls.Add(this.comboRX1Display);
		base.Controls.Add(this.labelTS2);
		base.Controls.Add(this.comboRX2Meter);
		base.Controls.Add(this.labelTS1);
		base.Controls.Add(this.comboRX1Meter);
		base.Controls.Add(this.btnClose);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "DisplaySettingsForm";
		this.Text = "Display Settings";
		base.TopMost = true;
		base.Activated += new System.EventHandler(DisplaySettingsForm_Activated);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(DisplaySettingsForm_FormClosing);
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	public void RepopulateForm()
	{
		comboRX1Meter.Items.Clear();
		comboRX1Meter.Items.AddRange(console.MeterRXModeItems.Cast<object>().ToArray());
		comboRX1Meter.Text = console.CurrentMeterRXModeText;
		comboRX2Meter.Items.Clear();
		comboRX2Meter.Items.AddRange(console.RX2MeterModeItems.Cast<object>().ToArray());
		comboRX2Meter.Text = console.RX2MeterModeText;
		comboRX1Display.Items.Clear();
		comboRX1Display.Items.AddRange(console.DisplayModeItems.Cast<object>().ToArray());
		comboRX1Display.Text = console.DisplayModeText;
		comboRX2Display.Items.Clear();
		comboRX2Display.Items.AddRange(console.DisplayRX2ModeItems.Cast<object>().ToArray());
		comboRX2Display.Text = console.DisplayRX2ModeText;
		comboTXMeter.Items.Clear();
		comboTXMeter.Items.AddRange(console.MeterTXModeItems.Cast<object>().ToArray());
		comboTXMeter.Text = console.CurrentMeterTXModeText;
		if (console.CATDisplayAvg == 1)
		{
			chkRX1Avg.Checked = true;
		}
		else
		{
			chkRX1Avg.Checked = false;
		}
		if (console.CATRX2DisplayAvg == 1)
		{
			chkRX2Avg.Checked = true;
		}
		else
		{
			chkRX2Avg.Checked = false;
		}
		if (console.CATDispPeak == "1")
		{
			chkRX1Peak.Checked = true;
		}
		else
		{
			chkRX1Peak.Checked = false;
		}
		if (console.CATRX2DispPeak == "1")
		{
			chkRX2Peak.Checked = true;
		}
		else
		{
			chkRX2Peak.Checked = false;
		}
	}

	private void BtnClose_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void DisplaySettingsForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		Hide();
		e.Cancel = true;
		Common.SaveForm(this, "DisplaySettingsForm");
	}

	private void DisplaySettingsForm_Activated(object sender, EventArgs e)
	{
		RepopulateForm();
	}

	private void ComboRX1Meter_SelectedIndexChanged(object sender, EventArgs e)
	{
		console.CurrentMeterRXModeText = comboRX1Meter.Text;
	}

	private void ComboRX2Meter_SelectedIndexChanged(object sender, EventArgs e)
	{
		console.RX2MeterModeText = comboRX2Meter.Text;
	}

	private void ComboTXMeter_SelectedIndexChanged(object sender, EventArgs e)
	{
		console.CurrentMeterTXModeText = comboTXMeter.Text;
	}

	private void ComboRX1Display_SelectedIndexChanged(object sender, EventArgs e)
	{
		console.DisplayModeText = comboRX1Display.Text;
	}

	private void ComboRX2Display_SelectedIndexChanged(object sender, EventArgs e)
	{
		console.DisplayRX2ModeText = comboRX2Display.Text;
	}

	private void ChkRX1Avg_CheckedChanged(object sender, EventArgs e)
	{
		if (chkRX1Avg.Checked)
		{
			console.CATDisplayAvg = 1;
		}
		else
		{
			console.CATDisplayAvg = 0;
		}
	}

	private void ChkRX2Avg_CheckedChanged(object sender, EventArgs e)
	{
		if (chkRX2Avg.Checked)
		{
			console.CATRX2DisplayAvg = 1;
		}
		else
		{
			console.CATRX2DisplayAvg = 0;
		}
	}

	private void ChkRX1Peak_CheckedChanged(object sender, EventArgs e)
	{
		if (chkRX1Peak.Checked)
		{
			console.CATDispPeak = "1";
		}
		else
		{
			console.CATDispPeak = "0";
		}
	}

	private void ChkRX2Peak_CheckedChanged(object sender, EventArgs e)
	{
		if (chkRX2Peak.Checked)
		{
			console.CATRX2DispPeak = "1";
		}
		else
		{
			console.CATRX2DispPeak = "0";
		}
	}
}
