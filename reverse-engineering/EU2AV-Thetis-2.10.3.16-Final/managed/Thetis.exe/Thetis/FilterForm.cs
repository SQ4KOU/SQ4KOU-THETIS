using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class FilterForm : Form
{
	private Console console;

	private FilterPreset[] preset;

	private bool rx2;

	private ComboBox comboDSPMode;

	private RadioButtonTS radFilter1;

	private RadioButtonTS radFilter2;

	private RadioButtonTS radFilter3;

	private RadioButtonTS radFilter4;

	private RadioButtonTS radFilter5;

	private RadioButtonTS radFilter6;

	private RadioButtonTS radFilter7;

	private RadioButtonTS radFilter8;

	private RadioButtonTS radFilter9;

	private RadioButtonTS radFilter10;

	private RadioButtonTS radFilterVar1;

	private RadioButtonTS radFilterVar2;

	private TextBox txtName;

	private Label lblMode;

	private Label lblName;

	private NumericUpDown udLow;

	private NumericUpDown udHigh;

	private Label lblLow;

	private Label label1;

	private GroupBox groupBox1;

	private GroupBox groupBox2;

	private PictureBox picDisplay;

	private Label lblWidth;

	private NumericUpDown udWidth;

	private Container components;

	private Filter current_filter;

	private DSPMode dsp_mode = DSPMode.FIRST;

	private bool _filter_updating;

	private bool drag_low;

	private bool drag_high;

	private bool drag_filter;

	private int drag_filter_low = -1;

	private int drag_filter_high = -1;

	private int drag_filter_start = -1;

	public Filter CurrentFilter
	{
		get
		{
			return current_filter;
		}
		set
		{
			current_filter = value;
			switch (current_filter)
			{
			case Filter.F1:
				radFilter1.Checked = true;
				break;
			case Filter.F2:
				radFilter2.Checked = true;
				break;
			case Filter.F3:
				radFilter3.Checked = true;
				break;
			case Filter.F4:
				radFilter4.Checked = true;
				break;
			case Filter.F5:
				radFilter5.Checked = true;
				break;
			case Filter.F6:
				radFilter6.Checked = true;
				break;
			case Filter.F7:
				radFilter7.Checked = true;
				break;
			case Filter.F8:
				radFilter8.Checked = true;
				break;
			case Filter.F9:
				radFilter9.Checked = true;
				break;
			case Filter.F10:
				radFilter10.Checked = true;
				break;
			case Filter.VAR1:
				radFilterVar1.Checked = true;
				break;
			case Filter.VAR2:
				radFilterVar2.Checked = true;
				break;
			}
			GetFilterInfo();
		}
	}

	public DSPMode DSPMode
	{
		get
		{
			return dsp_mode;
		}
		set
		{
			dsp_mode = value;
			switch (dsp_mode)
			{
			case DSPMode.LSB:
				comboDSPMode.Text = "LSB";
				break;
			case DSPMode.USB:
				comboDSPMode.Text = "USB";
				break;
			case DSPMode.DSB:
				comboDSPMode.Text = "DSB";
				break;
			case DSPMode.CWL:
				comboDSPMode.Text = "CWL";
				break;
			case DSPMode.CWU:
				comboDSPMode.Text = "CWU";
				break;
			case DSPMode.FM:
				comboDSPMode.Text = "FMN";
				break;
			case DSPMode.AM:
				comboDSPMode.Text = "AM";
				break;
			case DSPMode.SAM:
				comboDSPMode.Text = "SAM";
				break;
			case DSPMode.DIGL:
				comboDSPMode.Text = "DIGL";
				break;
			case DSPMode.DIGU:
				comboDSPMode.Text = "DIGU";
				break;
			}
			radFilter1.Text = preset[(int)value].GetName(Filter.F1);
			radFilter2.Text = preset[(int)value].GetName(Filter.F2);
			radFilter3.Text = preset[(int)value].GetName(Filter.F3);
			radFilter4.Text = preset[(int)value].GetName(Filter.F4);
			radFilter5.Text = preset[(int)value].GetName(Filter.F5);
			radFilter6.Text = preset[(int)value].GetName(Filter.F6);
			radFilter7.Text = preset[(int)value].GetName(Filter.F7);
			radFilter8.Text = preset[(int)value].GetName(Filter.F8);
			radFilter9.Text = preset[(int)value].GetName(Filter.F9);
			radFilter10.Text = preset[(int)value].GetName(Filter.F10);
			radFilterVar1.Text = preset[(int)value].GetName(Filter.VAR1);
			radFilterVar2.Text = preset[(int)value].GetName(Filter.VAR2);
			GetFilterInfo();
		}
	}

	public FilterForm(Console c, FilterPreset[] fp, bool _rx2)
	{
		console = c;
		preset = fp;
		InitializeComponent();
		comboDSPMode.SelectedIndex = 0;
		radFilter1.Checked = true;
		rx2 = _rx2;
		if (rx2)
		{
			radFilter8.Enabled = false;
			radFilter9.Enabled = false;
			radFilter10.Enabled = false;
		}
		Common.RestoreForm(this, "FilterForm", restore_size: false);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.FilterForm));
		this.comboDSPMode = new System.Windows.Forms.ComboBox();
		this.radFilter1 = new System.Windows.Forms.RadioButtonTS();
		this.radFilter2 = new System.Windows.Forms.RadioButtonTS();
		this.radFilter3 = new System.Windows.Forms.RadioButtonTS();
		this.radFilter4 = new System.Windows.Forms.RadioButtonTS();
		this.radFilter5 = new System.Windows.Forms.RadioButtonTS();
		this.radFilter6 = new System.Windows.Forms.RadioButtonTS();
		this.radFilter7 = new System.Windows.Forms.RadioButtonTS();
		this.radFilter8 = new System.Windows.Forms.RadioButtonTS();
		this.radFilter9 = new System.Windows.Forms.RadioButtonTS();
		this.radFilter10 = new System.Windows.Forms.RadioButtonTS();
		this.radFilterVar1 = new System.Windows.Forms.RadioButtonTS();
		this.radFilterVar2 = new System.Windows.Forms.RadioButtonTS();
		this.lblMode = new System.Windows.Forms.Label();
		this.txtName = new System.Windows.Forms.TextBox();
		this.lblName = new System.Windows.Forms.Label();
		this.udLow = new System.Windows.Forms.NumericUpDown();
		this.udHigh = new System.Windows.Forms.NumericUpDown();
		this.lblLow = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.udWidth = new System.Windows.Forms.NumericUpDown();
		this.lblWidth = new System.Windows.Forms.Label();
		this.picDisplay = new System.Windows.Forms.PictureBox();
		((System.ComponentModel.ISupportInitialize)this.udLow).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udHigh).BeginInit();
		this.groupBox1.SuspendLayout();
		this.groupBox2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.udWidth).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.picDisplay).BeginInit();
		base.SuspendLayout();
		this.comboDSPMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboDSPMode.Items.AddRange(new object[9] { "LSB", "USB", "DSB", "CWL", "CWU", "AM", "SAM", "DIGL", "DIGU" });
		this.comboDSPMode.Location = new System.Drawing.Point(64, 16);
		this.comboDSPMode.Name = "comboDSPMode";
		this.comboDSPMode.Size = new System.Drawing.Size(64, 21);
		this.comboDSPMode.TabIndex = 0;
		this.comboDSPMode.SelectedIndexChanged += new System.EventHandler(comboDSPMode_SelectedIndexChanged);
		this.radFilter1.Appearance = System.Windows.Forms.Appearance.Button;
		this.radFilter1.Image = null;
		this.radFilter1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radFilter1.Location = new System.Drawing.Point(8, 48);
		this.radFilter1.Name = "radFilter1";
		this.radFilter1.Size = new System.Drawing.Size(48, 18);
		this.radFilter1.TabIndex = 37;
		this.radFilter1.Text = "6.0k";
		this.radFilter1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radFilter1.CheckedChanged += new System.EventHandler(radFilter_CheckedChanged);
		this.radFilter2.Appearance = System.Windows.Forms.Appearance.Button;
		this.radFilter2.Image = null;
		this.radFilter2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radFilter2.Location = new System.Drawing.Point(56, 48);
		this.radFilter2.Name = "radFilter2";
		this.radFilter2.Size = new System.Drawing.Size(48, 18);
		this.radFilter2.TabIndex = 39;
		this.radFilter2.Text = "4.0k";
		this.radFilter2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radFilter2.CheckedChanged += new System.EventHandler(radFilter_CheckedChanged);
		this.radFilter3.Appearance = System.Windows.Forms.Appearance.Button;
		this.radFilter3.Image = null;
		this.radFilter3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radFilter3.Location = new System.Drawing.Point(104, 48);
		this.radFilter3.Name = "radFilter3";
		this.radFilter3.Size = new System.Drawing.Size(48, 18);
		this.radFilter3.TabIndex = 38;
		this.radFilter3.Text = "2.6k";
		this.radFilter3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radFilter3.CheckedChanged += new System.EventHandler(radFilter_CheckedChanged);
		this.radFilter4.Appearance = System.Windows.Forms.Appearance.Button;
		this.radFilter4.Image = null;
		this.radFilter4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radFilter4.Location = new System.Drawing.Point(8, 66);
		this.radFilter4.Name = "radFilter4";
		this.radFilter4.Size = new System.Drawing.Size(48, 18);
		this.radFilter4.TabIndex = 40;
		this.radFilter4.Text = "2.1k";
		this.radFilter4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radFilter4.CheckedChanged += new System.EventHandler(radFilter_CheckedChanged);
		this.radFilter5.Appearance = System.Windows.Forms.Appearance.Button;
		this.radFilter5.Image = null;
		this.radFilter5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radFilter5.Location = new System.Drawing.Point(56, 66);
		this.radFilter5.Name = "radFilter5";
		this.radFilter5.Size = new System.Drawing.Size(48, 18);
		this.radFilter5.TabIndex = 41;
		this.radFilter5.Text = "1.0k";
		this.radFilter5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radFilter5.CheckedChanged += new System.EventHandler(radFilter_CheckedChanged);
		this.radFilter6.Appearance = System.Windows.Forms.Appearance.Button;
		this.radFilter6.Image = null;
		this.radFilter6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radFilter6.Location = new System.Drawing.Point(104, 66);
		this.radFilter6.Name = "radFilter6";
		this.radFilter6.Size = new System.Drawing.Size(48, 18);
		this.radFilter6.TabIndex = 42;
		this.radFilter6.Text = "500";
		this.radFilter6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radFilter6.CheckedChanged += new System.EventHandler(radFilter_CheckedChanged);
		this.radFilter7.Appearance = System.Windows.Forms.Appearance.Button;
		this.radFilter7.Image = null;
		this.radFilter7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radFilter7.Location = new System.Drawing.Point(8, 84);
		this.radFilter7.Name = "radFilter7";
		this.radFilter7.Size = new System.Drawing.Size(48, 18);
		this.radFilter7.TabIndex = 43;
		this.radFilter7.Text = "250";
		this.radFilter7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radFilter7.CheckedChanged += new System.EventHandler(radFilter_CheckedChanged);
		this.radFilter8.Appearance = System.Windows.Forms.Appearance.Button;
		this.radFilter8.Image = null;
		this.radFilter8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radFilter8.Location = new System.Drawing.Point(56, 84);
		this.radFilter8.Name = "radFilter8";
		this.radFilter8.Size = new System.Drawing.Size(48, 18);
		this.radFilter8.TabIndex = 44;
		this.radFilter8.Text = "100";
		this.radFilter8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radFilter8.CheckedChanged += new System.EventHandler(radFilter_CheckedChanged);
		this.radFilter9.Appearance = System.Windows.Forms.Appearance.Button;
		this.radFilter9.Image = null;
		this.radFilter9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radFilter9.Location = new System.Drawing.Point(104, 84);
		this.radFilter9.Name = "radFilter9";
		this.radFilter9.Size = new System.Drawing.Size(48, 18);
		this.radFilter9.TabIndex = 45;
		this.radFilter9.Text = "50";
		this.radFilter9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radFilter9.CheckedChanged += new System.EventHandler(radFilter_CheckedChanged);
		this.radFilter10.Appearance = System.Windows.Forms.Appearance.Button;
		this.radFilter10.Image = null;
		this.radFilter10.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radFilter10.Location = new System.Drawing.Point(8, 102);
		this.radFilter10.Name = "radFilter10";
		this.radFilter10.Size = new System.Drawing.Size(48, 18);
		this.radFilter10.TabIndex = 46;
		this.radFilter10.Text = "25";
		this.radFilter10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radFilter10.CheckedChanged += new System.EventHandler(radFilter_CheckedChanged);
		this.radFilterVar1.Appearance = System.Windows.Forms.Appearance.Button;
		this.radFilterVar1.Image = null;
		this.radFilterVar1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radFilterVar1.Location = new System.Drawing.Point(56, 102);
		this.radFilterVar1.Name = "radFilterVar1";
		this.radFilterVar1.Size = new System.Drawing.Size(48, 18);
		this.radFilterVar1.TabIndex = 47;
		this.radFilterVar1.Text = "Var 1";
		this.radFilterVar1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radFilterVar1.CheckedChanged += new System.EventHandler(radFilter_CheckedChanged);
		this.radFilterVar2.Appearance = System.Windows.Forms.Appearance.Button;
		this.radFilterVar2.Image = null;
		this.radFilterVar2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.radFilterVar2.Location = new System.Drawing.Point(104, 102);
		this.radFilterVar2.Name = "radFilterVar2";
		this.radFilterVar2.Size = new System.Drawing.Size(48, 18);
		this.radFilterVar2.TabIndex = 48;
		this.radFilterVar2.Text = "Var 2";
		this.radFilterVar2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.radFilterVar2.CheckedChanged += new System.EventHandler(radFilter_CheckedChanged);
		this.lblMode.Location = new System.Drawing.Point(24, 16);
		this.lblMode.Name = "lblMode";
		this.lblMode.Size = new System.Drawing.Size(40, 23);
		this.lblMode.TabIndex = 49;
		this.lblMode.Text = "Mode:";
		this.txtName.Location = new System.Drawing.Point(72, 16);
		this.txtName.MaxLength = 6;
		this.txtName.Name = "txtName";
		this.txtName.Size = new System.Drawing.Size(56, 20);
		this.txtName.TabIndex = 50;
		this.txtName.LostFocus += new System.EventHandler(txtName_LostFocus);
		this.lblName.Location = new System.Drawing.Point(8, 16);
		this.lblName.Name = "lblName";
		this.lblName.Size = new System.Drawing.Size(48, 23);
		this.lblName.TabIndex = 51;
		this.lblName.Text = "Name:";
		this.udLow.Location = new System.Drawing.Point(72, 64);
		this.udLow.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.udLow.Minimum = new decimal(new int[4] { 10000, 0, 0, -2147483648 });
		this.udLow.Name = "udLow";
		this.udLow.Size = new System.Drawing.Size(64, 20);
		this.udLow.TabIndex = 52;
		this.udLow.ValueChanged += new System.EventHandler(udLow_ValueChanged);
		this.udLow.LostFocus += new System.EventHandler(udLow_LostFocus);
		this.udHigh.Location = new System.Drawing.Point(72, 40);
		this.udHigh.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.udHigh.Minimum = new decimal(new int[4] { 10000, 0, 0, -2147483648 });
		this.udHigh.Name = "udHigh";
		this.udHigh.Size = new System.Drawing.Size(64, 20);
		this.udHigh.TabIndex = 53;
		this.udHigh.ValueChanged += new System.EventHandler(udHigh_ValueChanged);
		this.udHigh.LostFocus += new System.EventHandler(udHigh_LostFocus);
		this.lblLow.Location = new System.Drawing.Point(8, 64);
		this.lblLow.Name = "lblLow";
		this.lblLow.Size = new System.Drawing.Size(48, 23);
		this.lblLow.TabIndex = 54;
		this.lblLow.Text = "Low:";
		this.label1.Location = new System.Drawing.Point(8, 40);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(48, 23);
		this.label1.TabIndex = 55;
		this.label1.Text = "High:";
		this.groupBox1.Controls.Add(this.radFilter10);
		this.groupBox1.Controls.Add(this.radFilter2);
		this.groupBox1.Controls.Add(this.radFilterVar1);
		this.groupBox1.Controls.Add(this.comboDSPMode);
		this.groupBox1.Controls.Add(this.radFilter1);
		this.groupBox1.Controls.Add(this.radFilterVar2);
		this.groupBox1.Controls.Add(this.lblMode);
		this.groupBox1.Controls.Add(this.radFilter3);
		this.groupBox1.Controls.Add(this.radFilter4);
		this.groupBox1.Controls.Add(this.radFilter5);
		this.groupBox1.Controls.Add(this.radFilter6);
		this.groupBox1.Controls.Add(this.radFilter7);
		this.groupBox1.Controls.Add(this.radFilter8);
		this.groupBox1.Controls.Add(this.radFilter9);
		this.groupBox1.Location = new System.Drawing.Point(8, 8);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(160, 128);
		this.groupBox1.TabIndex = 56;
		this.groupBox1.TabStop = false;
		this.groupBox2.Controls.Add(this.udWidth);
		this.groupBox2.Controls.Add(this.lblWidth);
		this.groupBox2.Controls.Add(this.udHigh);
		this.groupBox2.Controls.Add(this.lblLow);
		this.groupBox2.Controls.Add(this.label1);
		this.groupBox2.Controls.Add(this.txtName);
		this.groupBox2.Controls.Add(this.lblName);
		this.groupBox2.Controls.Add(this.udLow);
		this.groupBox2.Location = new System.Drawing.Point(176, 8);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(160, 128);
		this.groupBox2.TabIndex = 57;
		this.groupBox2.TabStop = false;
		this.udWidth.Location = new System.Drawing.Point(72, 88);
		this.udWidth.Maximum = new decimal(new int[4] { 20000, 0, 0, 0 });
		this.udWidth.Minimum = new decimal(new int[4] { 10, 0, 0, 0 });
		this.udWidth.Name = "udWidth";
		this.udWidth.Size = new System.Drawing.Size(64, 20);
		this.udWidth.TabIndex = 56;
		this.udWidth.Value = new decimal(new int[4] { 10, 0, 0, 0 });
		this.udWidth.ValueChanged += new System.EventHandler(udWidth_ValueChanged);
		this.lblWidth.Location = new System.Drawing.Point(8, 88);
		this.lblWidth.Name = "lblWidth";
		this.lblWidth.Size = new System.Drawing.Size(64, 23);
		this.lblWidth.TabIndex = 57;
		this.lblWidth.Text = "Width:";
		this.picDisplay.BackColor = System.Drawing.SystemColors.ControlText;
		this.picDisplay.Location = new System.Drawing.Point(8, 144);
		this.picDisplay.Name = "picDisplay";
		this.picDisplay.Size = new System.Drawing.Size(328, 50);
		this.picDisplay.TabIndex = 58;
		this.picDisplay.TabStop = false;
		this.picDisplay.Paint += new System.Windows.Forms.PaintEventHandler(picDisplay_Paint);
		this.picDisplay.MouseDown += new System.Windows.Forms.MouseEventHandler(picDisplay_MouseDown);
		this.picDisplay.MouseMove += new System.Windows.Forms.MouseEventHandler(picDisplay_MouseMove);
		this.picDisplay.MouseUp += new System.Windows.Forms.MouseEventHandler(picDisplay_MouseUp);
		base.ClientSize = new System.Drawing.Size(344, 206);
		base.Controls.Add(this.picDisplay);
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.groupBox1);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.Name = "FilterForm";
		this.Text = "Filter Setup";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(FilterForm_FormClosing);
		((System.ComponentModel.ISupportInitialize)this.udLow).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udHigh).EndInit();
		this.groupBox1.ResumeLayout(false);
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.udWidth).EndInit();
		((System.ComponentModel.ISupportInitialize)this.picDisplay).EndInit();
		base.ResumeLayout(false);
	}

	private void GetFilterInfo()
	{
		DSPMode dSPMode = DSPMode.FIRST;
		Filter filter = Filter.FIRST;
		dSPMode = (DSPMode)Enum.Parse(typeof(DSPMode), comboDSPMode.Text);
		filter = current_filter;
		txtName.Text = preset[(int)dSPMode].GetName(filter);
		UpdateFilter(preset[(int)dSPMode].GetLow(filter), preset[(int)dSPMode].GetHigh(filter));
	}

	private int HzToPixel(float freq)
	{
		int num = (int)((double)(-10000 * console.SampleRateRX1) / 48000.0);
		int num2 = (int)((double)(10000 * console.SampleRateRX1) / 48000.0);
		return picDisplay.Width / 2 + (int)(freq / (float)(num2 - num) * (float)picDisplay.Width);
	}

	private float PixelToHz(float x)
	{
		int num = (int)((double)(-10000 * console.SampleRateRX1) / 48000.0);
		int num2 = (int)((double)(10000 * console.SampleRateRX1) / 48000.0);
		return (float)((double)num + (double)x * (double)(num2 - num) / (double)picDisplay.Width);
	}

	private void UpdateFilter(int low, int high)
	{
		if (!_filter_updating)
		{
			_filter_updating = true;
			if ((decimal)low < udLow.Minimum)
			{
				low = (int)udLow.Minimum;
			}
			if ((decimal)low > udLow.Maximum)
			{
				low = (int)udLow.Maximum;
			}
			if ((decimal)high < udHigh.Minimum)
			{
				high = (int)udHigh.Minimum;
			}
			if ((decimal)high > udHigh.Maximum)
			{
				high = (int)udHigh.Maximum;
			}
			bool num = udLow.Value != (decimal)low;
			bool flag = udHigh.Value != (decimal)high;
			udLow.Value = low;
			udHigh.Value = high;
			udWidth.Value = high - low;
			_filter_updating = false;
			if (num)
			{
				udLow_ValueChanged(this, EventArgs.Empty);
			}
			if (flag)
			{
				udHigh_ValueChanged(this, EventArgs.Empty);
			}
			if (num | flag)
			{
				udWidth_ValueChanged(this, EventArgs.Empty);
			}
		}
	}

	private void radFilter_CheckedChanged(object sender, EventArgs e)
	{
		RadioButtonTS radioButtonTS = (RadioButtonTS)sender;
		if (((RadioButtonTS)sender).Checked)
		{
			string text = radioButtonTS.Name.Substring(radioButtonTS.Name.IndexOf("Filter") + 6);
			CurrentFilter = (Filter)Enum.Parse(value: text.StartsWith("V") ? text.ToUpper() : ("F" + text), enumType: typeof(Filter));
			radioButtonTS.BackColor = console.ButtonSelectedColor;
		}
		else
		{
			radioButtonTS.BackColor = SystemColors.Control;
		}
	}

	private void comboDSPMode_SelectedIndexChanged(object sender, EventArgs e)
	{
		DSPMode = (DSPMode)Enum.Parse(typeof(DSPMode), comboDSPMode.Text);
	}

	private void txtName_LostFocus(object sender, EventArgs e)
	{
		string name = preset[(int)dsp_mode].GetName(current_filter);
		preset[(int)dsp_mode].SetName(current_filter, txtName.Text);
		GetFilterInfo();
		if (!rx2)
		{
			if (console.RX1DSPMode == dsp_mode)
			{
				console.UpdateRX1FilterNames(current_filter, name, txtName.Text);
			}
		}
		else if (console.RX2DSPMode == dsp_mode)
		{
			console.UpdateRX2FilterNames(current_filter, name, txtName.Text);
		}
		switch (current_filter)
		{
		case Filter.F1:
			radFilter1.Text = preset[(int)dsp_mode].GetName(Filter.F1);
			break;
		case Filter.F2:
			radFilter2.Text = preset[(int)dsp_mode].GetName(Filter.F2);
			break;
		case Filter.F3:
			radFilter3.Text = preset[(int)dsp_mode].GetName(Filter.F3);
			break;
		case Filter.F4:
			radFilter4.Text = preset[(int)dsp_mode].GetName(Filter.F4);
			break;
		case Filter.F5:
			radFilter5.Text = preset[(int)dsp_mode].GetName(Filter.F5);
			break;
		case Filter.F6:
			radFilter6.Text = preset[(int)dsp_mode].GetName(Filter.F6);
			break;
		case Filter.F7:
			radFilter7.Text = preset[(int)dsp_mode].GetName(Filter.F7);
			break;
		case Filter.F8:
			radFilter8.Text = preset[(int)dsp_mode].GetName(Filter.F8);
			break;
		case Filter.F9:
			radFilter9.Text = preset[(int)dsp_mode].GetName(Filter.F9);
			break;
		case Filter.F10:
			radFilter10.Text = preset[(int)dsp_mode].GetName(Filter.F10);
			break;
		case Filter.VAR1:
			radFilterVar1.Text = preset[(int)dsp_mode].GetName(Filter.VAR1);
			break;
		case Filter.VAR2:
			radFilterVar2.Text = preset[(int)dsp_mode].GetName(Filter.VAR2);
			break;
		}
	}

	private void udLow_ValueChanged(object sender, EventArgs e)
	{
		if (_filter_updating)
		{
			return;
		}
		if (udLow.Value + 10m > udHigh.Value)
		{
			udLow.Value = udHigh.Value - 10m;
		}
		preset[(int)dsp_mode].SetLow(current_filter, (int)udLow.Value);
		if (!rx2)
		{
			if (console.RX1DSPMode == dsp_mode && console.RX1Filter == current_filter)
			{
				console.UpdateRX1FilterPresetLow((int)udLow.Value);
			}
		}
		else if (console.RX2DSPMode == dsp_mode && console.RX2Filter == current_filter)
		{
			console.UpdateRX2FilterPresetLow((int)udLow.Value);
		}
		UpdateFilter((int)udLow.Value, (int)udHigh.Value);
		picDisplay.Invalidate();
	}

	private void udHigh_ValueChanged(object sender, EventArgs e)
	{
		if (_filter_updating)
		{
			return;
		}
		if (udHigh.Value - 10m < udLow.Value)
		{
			udHigh.Value = udLow.Value + 10m;
		}
		preset[(int)dsp_mode].SetHigh(current_filter, (int)udHigh.Value);
		if (!rx2)
		{
			if (console.RX1DSPMode == dsp_mode && console.RX1Filter == current_filter)
			{
				console.UpdateRX1FilterPresetHigh((int)udHigh.Value);
			}
		}
		else if (console.RX2DSPMode == dsp_mode && console.RX2Filter == current_filter)
		{
			console.UpdateRX2FilterPresetHigh((int)udHigh.Value);
		}
		UpdateFilter((int)udLow.Value, (int)udHigh.Value);
		picDisplay.Invalidate();
	}

	private void udLow_LostFocus(object sender, EventArgs e)
	{
		udLow_ValueChanged(sender, e);
	}

	private void udHigh_LostFocus(object sender, EventArgs e)
	{
		udHigh_ValueChanged(sender, e);
	}

	private void picDisplay_Paint(object sender, PaintEventArgs e)
	{
		e.Graphics.FillRectangle(new SolidBrush(Display.DisplayBackgroundColor), 0, 0, picDisplay.Width, picDisplay.Height);
		e.Graphics.FillRectangle(new SolidBrush(Display.DisplayFilterColor), HzToPixel((int)udLow.Value), 0, Math.Max(1, HzToPixel((int)udHigh.Value) - HzToPixel((int)udLow.Value)), picDisplay.Height);
		e.Graphics.DrawLine(new Pen(Display.GridZeroColor, 1f), picDisplay.Width / 2, 0, picDisplay.Width / 2, picDisplay.Height);
	}

	private void picDisplay_MouseMove(object sender, MouseEventArgs e)
	{
		int num = HzToPixel((float)udLow.Value);
		int num2 = HzToPixel((float)udHigh.Value);
		if (Math.Abs(e.X - num) < 2 || Math.Abs(e.X - num2) < 2)
		{
			Cursor = Cursors.SizeWE;
		}
		else if (e.X > num && e.X < num2)
		{
			Cursor = Cursors.NoMoveHoriz;
		}
		else
		{
			Cursor = Cursors.Arrow;
		}
		if (drag_low)
		{
			udLow.Value = Math.Max(Math.Min(udLow.Maximum, (int)PixelToHz(e.X)), udLow.Minimum);
		}
		if (drag_high)
		{
			udHigh.Value = Math.Max(Math.Min(udHigh.Maximum, (int)PixelToHz(e.X)), udHigh.Minimum);
		}
		if (drag_filter)
		{
			int num3 = (int)(PixelToHz(e.X) - PixelToHz(drag_filter_start));
			udLow.Value = Math.Max(Math.Min(udLow.Maximum, drag_filter_low + num3), udLow.Minimum);
			udHigh.Value = Math.Max(Math.Min(udHigh.Maximum, drag_filter_high + num3), udHigh.Minimum);
		}
	}

	private void picDisplay_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			int num = HzToPixel((float)udLow.Value);
			int num2 = HzToPixel((float)udHigh.Value);
			if (Math.Abs(e.X - num) < 2)
			{
				drag_low = true;
			}
			else if (Math.Abs(e.X - num2) < 2)
			{
				drag_high = true;
			}
			else if (e.X > num && e.X < num2)
			{
				drag_filter = true;
				drag_filter_low = (int)udLow.Value;
				drag_filter_high = (int)udHigh.Value;
				drag_filter_start = e.X;
			}
		}
	}

	private void picDisplay_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			drag_low = false;
			drag_high = false;
			drag_filter = false;
			drag_filter_low = -1;
			drag_filter_high = -1;
			drag_filter_start = -1;
		}
	}

	private void udWidth_ValueChanged(object sender, EventArgs e)
	{
		if (!_filter_updating && udWidth.Focused)
		{
			int num = 0;
			int num2 = 0;
			switch (comboDSPMode.Text)
			{
			case "CWL":
				num = (int)((decimal)(-console.CWPitch) - udWidth.Value / 2m);
				num2 = (int)((decimal)(-console.CWPitch) + udWidth.Value / 2m);
				break;
			case "CWU":
				num = (int)((decimal)console.CWPitch - udWidth.Value / 2m);
				num2 = (int)((decimal)console.CWPitch + udWidth.Value / 2m);
				break;
			case "DIGL":
				num = (int)((decimal)(-console.DIGLClickTuneOffset) - udWidth.Value / 2m);
				num2 = (int)((decimal)(-console.DIGLClickTuneOffset) + udWidth.Value / 2m);
				break;
			case "DIGU":
				num = (int)((decimal)console.DIGUClickTuneOffset - udWidth.Value / 2m);
				num2 = (int)((decimal)console.DIGUClickTuneOffset + udWidth.Value / 2m);
				break;
			case "LSB":
				num2 = -console.DefaultLowCut;
				num = num2 - (int)udWidth.Value;
				break;
			case "USB":
				num = console.DefaultLowCut;
				num2 = num + (int)udWidth.Value;
				break;
			case "SAM":
			case "FMN":
			case "AM":
				num = -(int)udWidth.Value / 2;
				num2 = (int)udWidth.Value / 2;
				break;
			}
			UpdateFilter(num, num2);
		}
	}

	private void FilterForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		Common.SaveForm(this, "FilterForm");
	}
}
