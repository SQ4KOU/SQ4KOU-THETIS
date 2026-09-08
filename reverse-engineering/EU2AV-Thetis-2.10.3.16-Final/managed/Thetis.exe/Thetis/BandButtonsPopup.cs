using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class BandButtonsPopup : Form
{
	private Console console;

	private int currentBandGroup;

	private ButtonTS btnClose;

	private GroupBoxTS groupBoxTS1;

	private RadioButtonTS radBtn15;

	private RadioButtonTS radBtn14;

	private RadioButtonTS radBtn13;

	private RadioButtonTS radBtn12;

	private RadioButtonTS radBtn11;

	private RadioButtonTS radBtn10;

	private RadioButtonTS radBtn9;

	private RadioButtonTS radBtn8;

	private RadioButtonTS radBtn7;

	private RadioButtonTS radBtn6;

	private RadioButtonTS radBtn5;

	private RadioButtonTS radBtn4;

	private RadioButtonTS radBtn3;

	private RadioButtonTS radBtn2;

	private RadioButtonTS radBtn1;

	private IContainer components;

	public BandButtonsPopup(Console c)
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.BandButtonsPopup));
		this.groupBoxTS1 = new System.Windows.Forms.GroupBoxTS();
		this.radBtn15 = new System.Windows.Forms.RadioButtonTS();
		this.radBtn14 = new System.Windows.Forms.RadioButtonTS();
		this.radBtn13 = new System.Windows.Forms.RadioButtonTS();
		this.radBtn12 = new System.Windows.Forms.RadioButtonTS();
		this.radBtn11 = new System.Windows.Forms.RadioButtonTS();
		this.radBtn10 = new System.Windows.Forms.RadioButtonTS();
		this.radBtn9 = new System.Windows.Forms.RadioButtonTS();
		this.radBtn8 = new System.Windows.Forms.RadioButtonTS();
		this.radBtn7 = new System.Windows.Forms.RadioButtonTS();
		this.radBtn6 = new System.Windows.Forms.RadioButtonTS();
		this.radBtn5 = new System.Windows.Forms.RadioButtonTS();
		this.radBtn4 = new System.Windows.Forms.RadioButtonTS();
		this.radBtn3 = new System.Windows.Forms.RadioButtonTS();
		this.radBtn2 = new System.Windows.Forms.RadioButtonTS();
		this.radBtn1 = new System.Windows.Forms.RadioButtonTS();
		this.btnClose = new System.Windows.Forms.ButtonTS();
		this.groupBoxTS1.SuspendLayout();
		base.SuspendLayout();
		this.groupBoxTS1.Controls.Add(this.radBtn15);
		this.groupBoxTS1.Controls.Add(this.radBtn14);
		this.groupBoxTS1.Controls.Add(this.radBtn13);
		this.groupBoxTS1.Controls.Add(this.radBtn12);
		this.groupBoxTS1.Controls.Add(this.radBtn11);
		this.groupBoxTS1.Controls.Add(this.radBtn10);
		this.groupBoxTS1.Controls.Add(this.radBtn9);
		this.groupBoxTS1.Controls.Add(this.radBtn8);
		this.groupBoxTS1.Controls.Add(this.radBtn7);
		this.groupBoxTS1.Controls.Add(this.radBtn6);
		this.groupBoxTS1.Controls.Add(this.radBtn5);
		this.groupBoxTS1.Controls.Add(this.radBtn4);
		this.groupBoxTS1.Controls.Add(this.radBtn3);
		this.groupBoxTS1.Controls.Add(this.radBtn2);
		this.groupBoxTS1.Controls.Add(this.radBtn1);
		this.groupBoxTS1.Location = new System.Drawing.Point(12, 12);
		this.groupBoxTS1.Name = "groupBoxTS1";
		this.groupBoxTS1.Size = new System.Drawing.Size(328, 302);
		this.groupBoxTS1.TabIndex = 3;
		this.groupBoxTS1.TabStop = false;
		this.radBtn15.Appearance = System.Windows.Forms.Appearance.Button;
		this.radBtn15.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
		this.radBtn15.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.radBtn15.Image = null;
		this.radBtn15.Location = new System.Drawing.Point(218, 251);
		this.radBtn15.Name = "radBtn15";
		this.radBtn15.Size = new System.Drawing.Size(100, 40);
		this.radBtn15.TabIndex = 14;
		this.radBtn15.TabStop = true;
		this.radBtn15.Text = "F15";
		this.radBtn15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radBtn15.UseVisualStyleBackColor = true;
		this.radBtn15.Click += new System.EventHandler(radBtn15_Click);
		this.radBtn14.Appearance = System.Windows.Forms.Appearance.Button;
		this.radBtn14.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
		this.radBtn14.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.radBtn14.Image = null;
		this.radBtn14.Location = new System.Drawing.Point(112, 251);
		this.radBtn14.Name = "radBtn14";
		this.radBtn14.Size = new System.Drawing.Size(100, 40);
		this.radBtn14.TabIndex = 13;
		this.radBtn14.TabStop = true;
		this.radBtn14.Text = "F14";
		this.radBtn14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radBtn14.UseVisualStyleBackColor = true;
		this.radBtn14.Click += new System.EventHandler(radBtn14_Click);
		this.radBtn13.Appearance = System.Windows.Forms.Appearance.Button;
		this.radBtn13.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
		this.radBtn13.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.radBtn13.Image = null;
		this.radBtn13.Location = new System.Drawing.Point(6, 251);
		this.radBtn13.Name = "radBtn13";
		this.radBtn13.Size = new System.Drawing.Size(100, 40);
		this.radBtn13.TabIndex = 12;
		this.radBtn13.TabStop = true;
		this.radBtn13.Text = "F13";
		this.radBtn13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radBtn13.UseVisualStyleBackColor = true;
		this.radBtn13.Click += new System.EventHandler(radBtn13_Click);
		this.radBtn12.Appearance = System.Windows.Forms.Appearance.Button;
		this.radBtn12.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
		this.radBtn12.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.radBtn12.Image = null;
		this.radBtn12.Location = new System.Drawing.Point(218, 193);
		this.radBtn12.Name = "radBtn12";
		this.radBtn12.Size = new System.Drawing.Size(100, 40);
		this.radBtn12.TabIndex = 11;
		this.radBtn12.TabStop = true;
		this.radBtn12.Text = "F12";
		this.radBtn12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radBtn12.UseVisualStyleBackColor = true;
		this.radBtn12.Click += new System.EventHandler(radBtn12_Click);
		this.radBtn11.Appearance = System.Windows.Forms.Appearance.Button;
		this.radBtn11.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
		this.radBtn11.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.radBtn11.Image = null;
		this.radBtn11.Location = new System.Drawing.Point(112, 193);
		this.radBtn11.Name = "radBtn11";
		this.radBtn11.Size = new System.Drawing.Size(100, 40);
		this.radBtn11.TabIndex = 10;
		this.radBtn11.TabStop = true;
		this.radBtn11.Text = "F11";
		this.radBtn11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radBtn11.UseVisualStyleBackColor = true;
		this.radBtn11.Click += new System.EventHandler(radBtn11_Click);
		this.radBtn10.Appearance = System.Windows.Forms.Appearance.Button;
		this.radBtn10.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
		this.radBtn10.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.radBtn10.Image = null;
		this.radBtn10.Location = new System.Drawing.Point(6, 193);
		this.radBtn10.Name = "radBtn10";
		this.radBtn10.Size = new System.Drawing.Size(100, 40);
		this.radBtn10.TabIndex = 9;
		this.radBtn10.TabStop = true;
		this.radBtn10.Text = "F10";
		this.radBtn10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radBtn10.UseVisualStyleBackColor = true;
		this.radBtn10.Click += new System.EventHandler(radBtn10_Click);
		this.radBtn9.Appearance = System.Windows.Forms.Appearance.Button;
		this.radBtn9.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
		this.radBtn9.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.radBtn9.Image = null;
		this.radBtn9.Location = new System.Drawing.Point(218, 135);
		this.radBtn9.Name = "radBtn9";
		this.radBtn9.Size = new System.Drawing.Size(100, 40);
		this.radBtn9.TabIndex = 8;
		this.radBtn9.TabStop = true;
		this.radBtn9.Text = "F9";
		this.radBtn9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radBtn9.UseVisualStyleBackColor = true;
		this.radBtn9.Click += new System.EventHandler(radBtn9_Click);
		this.radBtn8.Appearance = System.Windows.Forms.Appearance.Button;
		this.radBtn8.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
		this.radBtn8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.radBtn8.Image = null;
		this.radBtn8.Location = new System.Drawing.Point(112, 135);
		this.radBtn8.Name = "radBtn8";
		this.radBtn8.Size = new System.Drawing.Size(100, 40);
		this.radBtn8.TabIndex = 7;
		this.radBtn8.TabStop = true;
		this.radBtn8.Text = "F8";
		this.radBtn8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radBtn8.UseVisualStyleBackColor = true;
		this.radBtn8.Click += new System.EventHandler(radBtn8_Click);
		this.radBtn7.Appearance = System.Windows.Forms.Appearance.Button;
		this.radBtn7.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
		this.radBtn7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.radBtn7.Image = null;
		this.radBtn7.Location = new System.Drawing.Point(6, 135);
		this.radBtn7.Name = "radBtn7";
		this.radBtn7.Size = new System.Drawing.Size(100, 40);
		this.radBtn7.TabIndex = 6;
		this.radBtn7.TabStop = true;
		this.radBtn7.Text = "F7";
		this.radBtn7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radBtn7.UseVisualStyleBackColor = true;
		this.radBtn7.Click += new System.EventHandler(radBtn7_Click);
		this.radBtn6.Appearance = System.Windows.Forms.Appearance.Button;
		this.radBtn6.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
		this.radBtn6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.radBtn6.Image = null;
		this.radBtn6.Location = new System.Drawing.Point(218, 77);
		this.radBtn6.Name = "radBtn6";
		this.radBtn6.Size = new System.Drawing.Size(100, 40);
		this.radBtn6.TabIndex = 5;
		this.radBtn6.TabStop = true;
		this.radBtn6.Text = "F6";
		this.radBtn6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radBtn6.UseVisualStyleBackColor = true;
		this.radBtn6.Click += new System.EventHandler(radBtn6_Click);
		this.radBtn5.Appearance = System.Windows.Forms.Appearance.Button;
		this.radBtn5.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
		this.radBtn5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.radBtn5.Image = null;
		this.radBtn5.Location = new System.Drawing.Point(112, 77);
		this.radBtn5.Name = "radBtn5";
		this.radBtn5.Size = new System.Drawing.Size(100, 40);
		this.radBtn5.TabIndex = 4;
		this.radBtn5.TabStop = true;
		this.radBtn5.Text = "F5";
		this.radBtn5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radBtn5.UseVisualStyleBackColor = true;
		this.radBtn5.Click += new System.EventHandler(radBtn5_Click);
		this.radBtn4.Appearance = System.Windows.Forms.Appearance.Button;
		this.radBtn4.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
		this.radBtn4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.radBtn4.Image = null;
		this.radBtn4.Location = new System.Drawing.Point(6, 77);
		this.radBtn4.Name = "radBtn4";
		this.radBtn4.Size = new System.Drawing.Size(100, 40);
		this.radBtn4.TabIndex = 3;
		this.radBtn4.TabStop = true;
		this.radBtn4.Text = "F4";
		this.radBtn4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radBtn4.UseVisualStyleBackColor = true;
		this.radBtn4.Click += new System.EventHandler(radBtn4_Click);
		this.radBtn3.Appearance = System.Windows.Forms.Appearance.Button;
		this.radBtn3.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
		this.radBtn3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.radBtn3.Image = null;
		this.radBtn3.Location = new System.Drawing.Point(218, 19);
		this.radBtn3.Name = "radBtn3";
		this.radBtn3.Size = new System.Drawing.Size(100, 40);
		this.radBtn3.TabIndex = 2;
		this.radBtn3.TabStop = true;
		this.radBtn3.Text = "F3";
		this.radBtn3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radBtn3.UseVisualStyleBackColor = true;
		this.radBtn3.Click += new System.EventHandler(radBtn3_Click);
		this.radBtn2.Appearance = System.Windows.Forms.Appearance.Button;
		this.radBtn2.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
		this.radBtn2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.radBtn2.Image = null;
		this.radBtn2.Location = new System.Drawing.Point(112, 19);
		this.radBtn2.Name = "radBtn2";
		this.radBtn2.Size = new System.Drawing.Size(100, 40);
		this.radBtn2.TabIndex = 1;
		this.radBtn2.TabStop = true;
		this.radBtn2.Text = "F2";
		this.radBtn2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radBtn2.UseVisualStyleBackColor = true;
		this.radBtn2.Click += new System.EventHandler(radBtn2_Click);
		this.radBtn1.Appearance = System.Windows.Forms.Appearance.Button;
		this.radBtn1.BackColor = System.Drawing.SystemColors.Control;
		this.radBtn1.FlatAppearance.CheckedBackColor = System.Drawing.Color.SkyBlue;
		this.radBtn1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.radBtn1.ForeColor = System.Drawing.SystemColors.ControlText;
		this.radBtn1.Image = null;
		this.radBtn1.Location = new System.Drawing.Point(6, 19);
		this.radBtn1.Name = "radBtn1";
		this.radBtn1.Size = new System.Drawing.Size(100, 40);
		this.radBtn1.TabIndex = 0;
		this.radBtn1.TabStop = true;
		this.radBtn1.Text = "F1";
		this.radBtn1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radBtn1.UseVisualStyleBackColor = true;
		this.radBtn1.Click += new System.EventHandler(radBtn1_Click);
		this.btnClose.Image = null;
		this.btnClose.Location = new System.Drawing.Point(346, 263);
		this.btnClose.Name = "btnClose";
		this.btnClose.Selectable = true;
		this.btnClose.Size = new System.Drawing.Size(100, 40);
		this.btnClose.TabIndex = 0;
		this.btnClose.Text = "Close";
		this.btnClose.UseVisualStyleBackColor = true;
		this.btnClose.Click += new System.EventHandler(btnClose_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.FromArgb(80, 80, 80);
		base.ClientSize = new System.Drawing.Size(466, 326);
		base.Controls.Add(this.groupBoxTS1);
		base.Controls.Add(this.btnClose);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "BandButtonsPopup";
		this.Text = "Select Operating Band";
		base.TopMost = true;
		base.Activated += new System.EventHandler(BandButtonsPopup_Activated);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(BandButtonsPopup_FormClosing);
		this.groupBoxTS1.ResumeLayout(false);
		base.ResumeLayout(false);
	}

	public void RepopulateForm()
	{
		currentBandGroup = console.BandGroup;
		if (console.ShowRX1)
		{
			Text = "set RX1 Band";
			Band rX1Band = console.RX1Band;
			switch (currentBandGroup)
			{
			case 0:
				radBtn1.Text = "160";
				radBtn2.Text = "80";
				radBtn3.Text = "60";
				radBtn4.Text = "40";
				radBtn5.Text = "30";
				radBtn6.Text = "20";
				radBtn7.Text = "17";
				radBtn8.Text = "15";
				radBtn9.Text = "12";
				radBtn10.Text = "10";
				radBtn11.Text = "6";
				radBtn12.Text = "LF MF";
				radBtn13.Text = "VHF+";
				radBtn14.Text = "WWV";
				radBtn15.Text = "SWL";
				radBtn12.Enabled = false;
				radBtn13.Enabled = true;
				break;
			case 1:
				radBtn1.Text = console.GetVHFText(0);
				radBtn2.Text = console.GetVHFText(1);
				radBtn3.Text = console.GetVHFText(2);
				radBtn4.Text = console.GetVHFText(3);
				radBtn5.Text = console.GetVHFText(4);
				radBtn6.Text = console.GetVHFText(5);
				radBtn7.Text = console.GetVHFText(6);
				radBtn8.Text = console.GetVHFText(7);
				radBtn9.Text = console.GetVHFText(8);
				radBtn10.Text = console.GetVHFText(9);
				radBtn11.Text = console.GetVHFText(10);
				radBtn12.Text = console.GetVHFText(11);
				radBtn13.Text = "HF";
				radBtn14.Text = console.GetVHFText(12);
				radBtn15.Text = console.GetVHFText(13);
				radBtn12.Enabled = true;
				radBtn13.Enabled = true;
				break;
			case 2:
				radBtn1.Text = "L/MW";
				radBtn2.Text = "120m";
				radBtn3.Text = "90m";
				radBtn4.Text = "61m";
				radBtn5.Text = "49m";
				radBtn6.Text = "41m";
				radBtn7.Text = "31m";
				radBtn8.Text = "25m";
				radBtn9.Text = "22m";
				radBtn10.Text = "19m";
				radBtn11.Text = "16m";
				radBtn12.Text = "14m";
				radBtn13.Text = "HF";
				radBtn14.Text = "13m";
				radBtn15.Text = "11m";
				radBtn12.Enabled = true;
				radBtn13.Enabled = true;
				break;
			}
			switch (rX1Band)
			{
			case Band.B160M:
			case Band.VHF0:
			case Band.BLMF:
				radBtn1.Checked = true;
				break;
			case Band.B80M:
			case Band.VHF1:
			case Band.B120M:
				radBtn2.Checked = true;
				break;
			case Band.B60M:
			case Band.VHF2:
			case Band.B90M:
				radBtn3.Checked = true;
				break;
			case Band.B40M:
			case Band.VHF3:
			case Band.B61M:
				radBtn4.Checked = true;
				break;
			case Band.B30M:
			case Band.VHF4:
			case Band.B49M:
				radBtn5.Checked = true;
				break;
			case Band.B20M:
			case Band.VHF5:
			case Band.B41M:
				radBtn6.Checked = true;
				break;
			case Band.B17M:
			case Band.VHF6:
			case Band.B31M:
				radBtn7.Checked = true;
				break;
			case Band.B15M:
			case Band.VHF7:
			case Band.B25M:
				radBtn8.Checked = true;
				break;
			case Band.B12M:
			case Band.VHF8:
			case Band.B22M:
				radBtn9.Checked = true;
				break;
			case Band.B10M:
			case Band.VHF9:
			case Band.B19M:
				radBtn10.Checked = true;
				break;
			case Band.B6M:
			case Band.VHF10:
			case Band.B16M:
				radBtn11.Checked = true;
				break;
			case Band.GEN:
			case Band.VHF11:
			case Band.B14M:
				radBtn12.Checked = true;
				break;
			case Band.WWV:
			case Band.VHF12:
			case Band.B13M:
				radBtn14.Checked = true;
				break;
			case Band.VHF13:
			case Band.B11M:
				radBtn15.Checked = true;
				break;
			case Band.B2M:
				break;
			}
		}
		else
		{
			Text = "set RX2 Band";
			Band rX1Band = console.RX2Band;
			radBtn1.Text = "160";
			radBtn2.Text = "80";
			radBtn3.Text = "60";
			radBtn4.Text = "40";
			radBtn5.Text = "30";
			radBtn6.Text = "20";
			radBtn7.Text = "17";
			radBtn8.Text = "15";
			radBtn9.Text = "12";
			radBtn10.Text = "10";
			radBtn11.Text = "6";
			radBtn12.Text = "--";
			radBtn13.Text = "--";
			radBtn14.Text = "WWV";
			radBtn15.Text = "GEN";
			radBtn12.Enabled = false;
			radBtn13.Enabled = false;
			switch (rX1Band)
			{
			case Band.B160M:
				radBtn1.Checked = true;
				break;
			case Band.B80M:
				radBtn2.Checked = true;
				break;
			case Band.B60M:
				radBtn3.Checked = true;
				break;
			case Band.B40M:
				radBtn4.Checked = true;
				break;
			case Band.B30M:
				radBtn5.Checked = true;
				break;
			case Band.B20M:
				radBtn6.Checked = true;
				break;
			case Band.B17M:
				radBtn7.Checked = true;
				break;
			case Band.B15M:
				radBtn8.Checked = true;
				break;
			case Band.B12M:
				radBtn9.Checked = true;
				break;
			case Band.B10M:
				radBtn10.Checked = true;
				break;
			case Band.B6M:
				radBtn11.Checked = true;
				break;
			case Band.WWV:
				radBtn14.Checked = true;
				break;
			case Band.GEN:
				radBtn15.Checked = true;
				break;
			case Band.B2M:
				break;
			}
		}
	}

	private void btnClose_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void BandButtonsPopup_FormClosing(object sender, FormClosingEventArgs e)
	{
		Hide();
		e.Cancel = true;
		Common.SaveForm(this, "BandButtonsPopup");
	}

	private void BandButtonsPopup_Activated(object sender, EventArgs e)
	{
		RepopulateForm();
	}

	private void radBtn1_Click(object sender, EventArgs e)
	{
		if (!console.ShowRX1)
		{
			console.RX2Band = Band.B160M;
		}
		else if (currentBandGroup == 0)
		{
			console.SetCATBand(Band.B160M);
		}
		else if (currentBandGroup == 1)
		{
			console.SetCATBand(Band.VHF0);
		}
	}

	private void radBtn2_Click(object sender, EventArgs e)
	{
		if (!console.ShowRX1)
		{
			console.RX2Band = Band.B80M;
		}
		else if (currentBandGroup == 0)
		{
			console.SetCATBand(Band.B80M);
		}
		else if (currentBandGroup == 1)
		{
			console.SetCATBand(Band.VHF1);
		}
	}

	private void radBtn3_Click(object sender, EventArgs e)
	{
		if (!console.ShowRX1)
		{
			console.RX2Band = Band.B60M;
		}
		else if (currentBandGroup == 0)
		{
			console.SetCATBand(Band.B60M);
		}
		else if (currentBandGroup == 1)
		{
			console.SetCATBand(Band.VHF2);
		}
	}

	private void radBtn4_Click(object sender, EventArgs e)
	{
		if (!console.ShowRX1)
		{
			console.RX2Band = Band.B40M;
		}
		else if (currentBandGroup == 0)
		{
			console.SetCATBand(Band.B40M);
		}
		else if (currentBandGroup == 1)
		{
			console.SetCATBand(Band.VHF3);
		}
	}

	private void radBtn5_Click(object sender, EventArgs e)
	{
		if (!console.ShowRX1)
		{
			console.RX2Band = Band.B30M;
		}
		else if (currentBandGroup == 0)
		{
			console.SetCATBand(Band.B30M);
		}
		else if (currentBandGroup == 1)
		{
			console.SetCATBand(Band.VHF4);
		}
	}

	private void radBtn6_Click(object sender, EventArgs e)
	{
		if (!console.ShowRX1)
		{
			console.RX2Band = Band.B20M;
		}
		else if (currentBandGroup == 0)
		{
			console.SetCATBand(Band.B20M);
		}
		else if (currentBandGroup == 1)
		{
			console.SetCATBand(Band.VHF5);
		}
	}

	private void radBtn7_Click(object sender, EventArgs e)
	{
		if (!console.ShowRX1)
		{
			console.RX2Band = Band.B17M;
		}
		else if (currentBandGroup == 0)
		{
			console.SetCATBand(Band.B17M);
		}
		else if (currentBandGroup == 1)
		{
			console.SetCATBand(Band.VHF6);
		}
	}

	private void radBtn8_Click(object sender, EventArgs e)
	{
		if (!console.ShowRX1)
		{
			console.RX2Band = Band.B15M;
		}
		else if (currentBandGroup == 0)
		{
			console.SetCATBand(Band.B15M);
		}
		else if (currentBandGroup == 1)
		{
			console.SetCATBand(Band.VHF7);
		}
	}

	private void radBtn9_Click(object sender, EventArgs e)
	{
		if (!console.ShowRX1)
		{
			console.RX2Band = Band.B12M;
		}
		else if (currentBandGroup == 0)
		{
			console.SetCATBand(Band.B12M);
		}
		else if (currentBandGroup == 1)
		{
			console.SetCATBand(Band.VHF8);
		}
	}

	private void radBtn10_Click(object sender, EventArgs e)
	{
		if (!console.ShowRX1)
		{
			console.RX2Band = Band.B10M;
		}
		else if (currentBandGroup == 0)
		{
			console.SetCATBand(Band.B10M);
		}
		else if (currentBandGroup == 1)
		{
			console.SetCATBand(Band.VHF9);
		}
	}

	private void radBtn11_Click(object sender, EventArgs e)
	{
		if (!console.ShowRX1)
		{
			console.RX2Band = Band.B6M;
		}
		else if (currentBandGroup == 0)
		{
			console.SetCATBand(Band.B6M);
		}
		else if (currentBandGroup == 1)
		{
			console.SetCATBand(Band.VHF10);
		}
	}

	private void radBtn12_Click(object sender, EventArgs e)
	{
		if (console.ShowRX1 && currentBandGroup == 1)
		{
			console.SetCATBand(Band.VHF11);
		}
	}

	private void radBtn13_Click(object sender, EventArgs e)
	{
		if (console.ShowRX1)
		{
			if (currentBandGroup == 0)
			{
				console.BandGroup = 1;
			}
			else if (currentBandGroup == 1)
			{
				console.BandGroup = 0;
			}
			else if (currentBandGroup == 2)
			{
				console.BandGroup = 0;
			}
			RepopulateForm();
		}
	}

	private void radBtn14_Click(object sender, EventArgs e)
	{
		if (!console.ShowRX1)
		{
			console.RX2Band = Band.WWV;
		}
		else if (currentBandGroup == 0)
		{
			console.SetCATBand(Band.WWV);
		}
		else if (currentBandGroup == 1)
		{
			console.SetCATBand(Band.VHF12);
		}
	}

	private void radBtn15_Click(object sender, EventArgs e)
	{
		if (!console.ShowRX1)
		{
			console.RX2Band = Band.GEN;
		}
		else if (currentBandGroup == 0)
		{
			console.BandGroup = 2;
			RepopulateForm();
		}
		else if (currentBandGroup == 1)
		{
			console.SetCATBand(Band.VHF13);
		}
	}
}
