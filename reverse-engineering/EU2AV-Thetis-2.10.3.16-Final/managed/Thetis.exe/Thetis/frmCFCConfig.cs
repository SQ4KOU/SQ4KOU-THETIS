using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Thetis;

public class frmCFCConfig : Form
{
	private int _selected_index_eq;

	private int _selected_index_comp;

	private System.Threading.Timer _timer;

	private bool _busy;

	private double[] _CFCCompValues;

	private double[] _cfc_data;

	private bool _ignore_udpates;

	private bool _ignore_unselected;

	private bool _active;

	private IContainer components;

	private LabelTS labelTS673;

	private NumericUpDownTS nudCFC_c;

	private LabelTS labelTS672;

	private LabelTS labelTS670;

	private LabelTS labelTS671;

	private NumericUpDownTS nudCFC_posteqgain;

	private LabelTS labelTS668;

	private LabelTS labelTS669;

	private NumericUpDownTS udCFC_low;

	private CheckBoxTS chkCFC_UseQFactors;

	private ButtonTS btnResetComp;

	private LabelTS labelTS665;

	private NumericUpDownTS nudCFC_gain;

	private NumericUpDownTS nudCFC_q;

	private LabelTS labelTS666;

	private LabelTS labelTS663;

	private ucParametricEq ucCFC_eq;

	private NumericUpDownTS udCFC_high;

	private LabelTS labelTS661;

	private RadioButtonTS radCFC_5;

	private CheckBoxTS chkCFC_PanaEQ_live;

	private RadioButtonTS radCFC_18;

	private LabelTS labelTS662;

	private RadioButtonTS radCFC_10;

	private NumericUpDownTS nudCFC_precomp;

	private NumericUpDownTS nudCFC_selected_band;

	private LabelTS labelTS667;

	private LabelTS labelTS664;

	private NumericUpDownTS nudCFC_f;

	private ucParametricEq ucCFC_comp;

	private ButtonTS btnResetEQ;

	private LabelTS labelTS1;

	private NumericUpDownTS nudCFC_cq;

	private LinkLabel lblOGGuide;

	private CheckBoxTS chkLogScale;

	private int SelectedIndex
	{
		get
		{
			if (_selected_index_comp != -1)
			{
				return _selected_index_comp;
			}
			if (_selected_index_eq != -1)
			{
				return _selected_index_eq;
			}
			return -1;
		}
	}

	public string ConfigData
	{
		get
		{
			try
			{
				string obj = ucCFC_comp.SaveToJson();
				string text = ucCFC_eq.SaveToJson();
				return Common.Compress_gzip(obj + "<SEP>" + text);
			}
			catch
			{
				return "";
			}
		}
		set
		{
			try
			{
				string text = Common.Decompress_gzip(value);
				if (!string.IsNullOrEmpty(text))
				{
					string[] array = text.Split(new string[1] { "<SEP>" }, 2, StringSplitOptions.None);
					if (array.Length == 2)
					{
						string json = array[0];
						string json2 = array[1];
						if (ucCFC_comp.LoadFromJson(json) | ucCFC_eq.LoadFromJson(json2))
						{
							_ignore_udpates = true;
							switch (ucCFC_comp.BandCount)
							{
							case 5:
								radCFC_5.Checked = true;
								nudCFC_selected_band.Maximum = 5m;
								break;
							case 10:
								radCFC_10.Checked = true;
								nudCFC_selected_band.Maximum = 10m;
								break;
							case 18:
								radCFC_18.Checked = true;
								nudCFC_selected_band.Maximum = 18m;
								break;
							}
							chkCFC_UseQFactors.Checked = ucCFC_comp.ParametricEQ;
							udCFC_low.Value = (decimal)ucCFC_comp.FrequencyMinHz;
							udCFC_high.Value = (decimal)ucCFC_comp.FrequencyMaxHz;
							_ignore_udpates = false;
							setCFCProfile();
							_selected_index_comp = -1;
							_selected_index_eq = -1;
							updateSelected(null);
							return;
						}
					}
				}
			}
			catch
			{
			}
			radCFC_10.Checked = true;
			ucCFC_comp.BandCount = 10;
			nudCFC_selected_band.Maximum = 10m;
			ucCFC_comp.GlobalGainDb = 0.0;
			ucCFC_comp.ResetPoints();
			ucCFC_eq.BandCount = 10;
			ucCFC_eq.GlobalGainDb = 0.0;
			ucCFC_eq.ResetPoints();
			_selected_index_comp = -1;
			_selected_index_eq = -1;
			updateSelected(null);
		}
	}

	public bool Active
	{
		get
		{
			return _active;
		}
		set
		{
			_active = value;
			if (_active)
			{
				setCFCProfile();
			}
		}
	}

	public frmCFCConfig()
	{
		_CFCCompValues = new double[1025];
		_cfc_data = null;
		_ignore_udpates = false;
		_ignore_unselected = false;
		_selected_index_eq = -1;
		_selected_index_comp = -1;
		_busy = false;
		_active = false;
		InitializeComponent();
		Common.RestoreForm(this, "CFCConfig", restore_size: false);
		Common.ForceFormOnScreen(this);
		ucCFC_comp.GetDefaults(out var F, out var G, out var Q, out var global_preamp_db, out var min_hz, out var max_hz, out var _, out var _);
		ucCFC_comp.SetPointsData(F, G, Q);
		ucCFC_comp.FrequencyMinHz = min_hz;
		ucCFC_comp.FrequencyMaxHz = max_hz;
		ucCFC_comp.GlobalGainDb = global_preamp_db;
		ucCFC_eq.SetPointsData(F, G, Q);
		ucCFC_eq.FrequencyMinHz = min_hz;
		ucCFC_eq.FrequencyMaxHz = max_hz;
		ucCFC_eq.GlobalGainDb = global_preamp_db;
		udCFC_low.Value = (decimal)min_hz;
		udCFC_high.Value = (decimal)max_hz;
		updateSelected(null);
		setTimer();
	}

	private void radCFC_bands_CheckedChanged(object sender, EventArgs e)
	{
		int num = 10;
		if (radCFC_5.Checked)
		{
			num = 5;
		}
		else if (radCFC_18.Checked)
		{
			num = 18;
		}
		nudCFC_selected_band.Maximum = num;
		ucCFC_comp.BandCount = num;
		ucCFC_eq.BandCount = num;
	}

	private void udCFC_low_ValueChanged(object sender, EventArgs e)
	{
		if (udCFC_low.Value > udCFC_high.Value - 1000m)
		{
			udCFC_low.Value = udCFC_high.Value - 1000m;
			return;
		}
		ucCFC_comp.FrequencyMinHz = (int)udCFC_low.Value;
		ucCFC_eq.FrequencyMinHz = ucCFC_comp.FrequencyMinHz;
	}

	private void udCFC_high_ValueChanged(object sender, EventArgs e)
	{
		if (udCFC_high.Value < udCFC_low.Value + 1000m)
		{
			udCFC_high.Value = udCFC_low.Value + 1000m;
			return;
		}
		ucCFC_comp.FrequencyMaxHz = (int)udCFC_high.Value;
		ucCFC_eq.FrequencyMaxHz = ucCFC_comp.FrequencyMaxHz;
	}

	private void nudCFC_f_ValueChanged(object sender, EventArgs e)
	{
		if (!_ignore_udpates)
		{
			int selectedIndex = ucCFC_comp.SelectedIndex;
			ucCFC_comp.GetPointData(selectedIndex, out var frequency_hz, out var gain_db, out var q);
			frequency_hz = (double)nudCFC_f.Value;
			ucCFC_comp.SetPointData(selectedIndex, frequency_hz, gain_db, q);
		}
	}

	private void nudCFC_precomp_ValueChanged(object sender, EventArgs e)
	{
		if (!_ignore_udpates)
		{
			ucCFC_comp.GlobalGainDb = (double)nudCFC_precomp.Value;
		}
	}

	private void nudCFC_c_ValueChanged(object sender, EventArgs e)
	{
		if (!_ignore_udpates)
		{
			int selectedIndex = ucCFC_comp.SelectedIndex;
			ucCFC_comp.GetPointData(selectedIndex, out var frequency_hz, out var gain_db, out var q);
			gain_db = (double)nudCFC_c.Value;
			ucCFC_comp.SetPointData(selectedIndex, frequency_hz, gain_db, q);
		}
	}

	private void nudCFC_posteqgain_ValueChanged(object sender, EventArgs e)
	{
		if (!_ignore_udpates)
		{
			ucCFC_eq.GlobalGainDb = (double)nudCFC_posteqgain.Value;
		}
	}

	private void nudCFC_gain_ValueChanged(object sender, EventArgs e)
	{
		if (!_ignore_udpates)
		{
			int selectedIndex = ucCFC_eq.SelectedIndex;
			ucCFC_eq.GetPointData(selectedIndex, out var frequency_hz, out var gain_db, out var q);
			gain_db = (double)nudCFC_gain.Value;
			ucCFC_eq.SetPointData(selectedIndex, frequency_hz, gain_db, q);
		}
	}

	private void nudCFC_q_ValueChanged(object sender, EventArgs e)
	{
		if (!_ignore_udpates)
		{
			int selectedIndex = ucCFC_eq.SelectedIndex;
			ucCFC_eq.GetPointData(selectedIndex, out var frequency_hz, out var gain_db, out var q);
			q = (double)nudCFC_q.Value;
			ucCFC_eq.SetPointData(selectedIndex, frequency_hz, gain_db, q);
		}
	}

	private void nudCFC_cq_ValueChanged(object sender, EventArgs e)
	{
		if (!_ignore_udpates)
		{
			int selectedIndex = ucCFC_comp.SelectedIndex;
			ucCFC_comp.GetPointData(selectedIndex, out var frequency_hz, out var gain_db, out var q);
			q = (double)nudCFC_cq.Value;
			ucCFC_comp.SetPointData(selectedIndex, frequency_hz, gain_db, q);
		}
	}

	private void ucCFC_comp_GlobalGainChanged(object sender, ucParametricEq.EqDraggingEventArgs e)
	{
		if (!e.IsDragging || chkCFC_PanaEQ_live.Checked)
		{
			setCFCProfile();
		}
		else
		{
			setCFCProfile(-1, just_text: true);
		}
	}

	private void ucCFC_comp_PointDataChanged(object sender, ucParametricEq.EqPointDataChangedEventArgs e)
	{
		ucCFC_eq.SetPointHz(e.BandId, e.FrequencyHz, e.IsDragging);
		int indexFromBandId = ucCFC_eq.GetIndexFromBandId(e.BandId);
		ucCFC_eq.SelectedIndex = indexFromBandId;
		if (!e.IsDragging || chkCFC_PanaEQ_live.Checked)
		{
			setCFCProfile(e.Index);
		}
		else
		{
			setCFCProfile(e.Index, just_text: true);
		}
	}

	private void ucCFC_comp_PointsChanged(object sender, ucParametricEq.EqDraggingEventArgs e)
	{
		if (!e.IsDragging)
		{
			setCFCProfile();
		}
	}

	private void ucCFC_comp_PointSelected(object sender, ucParametricEq.EqPointSelectionChangedEventArgs e)
	{
		_selected_index_comp = e.Index;
		int indexFromBandId = ucCFC_eq.GetIndexFromBandId(e.BandId);
		ucCFC_eq.SelectedIndex = indexFromBandId;
		updateSelected(e);
	}

	private void ucCFC_comp_PointUnselected(object sender, ucParametricEq.EqPointSelectionChangedEventArgs e)
	{
		if (!_ignore_unselected)
		{
			_selected_index_comp = -1;
			ucCFC_eq.SelectedIndex = -1;
			updateSelected(null);
		}
	}

	private void ucCFC_eq_GlobalGainChanged(object sender, ucParametricEq.EqDraggingEventArgs e)
	{
		if (!e.IsDragging || chkCFC_PanaEQ_live.Checked)
		{
			setCFCProfile();
		}
		else
		{
			setCFCProfile(-1, just_text: true);
		}
	}

	private void ucCFC_eq_PointDataChanged(object sender, ucParametricEq.EqPointDataChangedEventArgs e)
	{
		ucCFC_comp.SetPointHz(e.BandId, e.FrequencyHz, e.IsDragging);
		int indexFromBandId = ucCFC_comp.GetIndexFromBandId(e.BandId);
		ucCFC_comp.SelectedIndex = indexFromBandId;
		if (!e.IsDragging || chkCFC_PanaEQ_live.Checked)
		{
			setCFCProfile(e.Index);
		}
		else
		{
			setCFCProfile(e.Index, just_text: true);
		}
	}

	private void ucCFC_eq_PointsChanged(object sender, ucParametricEq.EqDraggingEventArgs e)
	{
		if (!e.IsDragging)
		{
			setCFCProfile();
		}
	}

	private void ucCFC_eq_PointSelected(object sender, ucParametricEq.EqPointSelectionChangedEventArgs e)
	{
		_selected_index_eq = e.Index;
		int indexFromBandId = ucCFC_comp.GetIndexFromBandId(e.BandId);
		ucCFC_comp.SelectedIndex = indexFromBandId;
		updateSelected(e);
	}

	private void ucCFC_eq_PointUnselected(object sender, ucParametricEq.EqPointSelectionChangedEventArgs e)
	{
		if (!_ignore_unselected)
		{
			_selected_index_eq = -1;
			ucCFC_comp.SelectedIndex = -1;
			updateSelected(null);
		}
	}

	private void updateSelected(ucParametricEq.EqPointSelectionChangedEventArgs e)
	{
		bool enabled = _selected_index_comp != -1;
		nudCFC_f.Enabled = enabled;
		nudCFC_precomp.Enabled = enabled;
		nudCFC_c.Enabled = enabled;
		nudCFC_cq.Enabled = enabled;
		nudCFC_posteqgain.Enabled = enabled;
		nudCFC_gain.Enabled = enabled;
		nudCFC_q.Enabled = enabled;
		if (SelectedIndex != -1 && e != null)
		{
			nudCFC_f.Value = (decimal)e.FrequencyHz;
		}
	}

	private unsafe void setCFCProfile(int index = -1, bool just_text = false)
	{
		ucCFC_comp.GetPointsData(out var frequency_hz, out var gain_db, out var q);
		ucCFC_eq.GetPointsData(out var _, out var gain_db2, out var q2);
		double num = ucCFC_comp.GlobalGainDb;
		double num2 = ucCFC_eq.GlobalGainDb;
		_ignore_udpates = true;
		nudCFC_precomp.Value = (decimal)num;
		nudCFC_posteqgain.Value = (decimal)num2;
		if (index >= 0 && index < frequency_hz.Length)
		{
			nudCFC_selected_band.Value = index + 1;
			nudCFC_f.Value = (decimal)frequency_hz[index];
			nudCFC_c.Value = (decimal)gain_db[index];
			nudCFC_cq.Value = (decimal)q[index];
			nudCFC_gain.Value = (decimal)gain_db2[index];
			nudCFC_q.Value = (decimal)q2[index];
		}
		_ignore_udpates = false;
		if (just_text || !_active)
		{
			return;
		}
		WDSP.SetTXACFCOMPPrecomp(WDSP.id(1u, 0u), num);
		WDSP.SetTXACFCOMPPrePeq(WDSP.id(1u, 0u), num2);
		int nfreqs = frequency_hz.Length;
		bool num3 = ucCFC_comp.ParametricEQ && ucCFC_eq.ParametricEQ;
		fixed (double* f = &frequency_hz[0])
		{
			fixed (double* g = &gain_db[0])
			{
				fixed (double* e = &gain_db2[0])
				{
					if (num3)
					{
						fixed (double* qg = &q[0])
						{
							fixed (double* qe = &q2[0])
							{
								WDSP.SetTXACFCOMPprofile(WDSP.id(1u, 0u), nfreqs, f, g, e, qg, qe);
							}
						}
					}
					else
					{
						WDSP.SetTXACFCOMPprofile(WDSP.id(1u, 0u), nfreqs, f, g, e, null, null);
					}
				}
			}
		}
	}

	private unsafe void timerTick(object state)
	{
		if (_busy || base.Disposing || base.IsDisposed)
		{
			return;
		}
		_busy = true;
		int num = 0;
		fixed (double* comp_values = &_CFCCompValues[0])
		{
			WDSP.GetTXACFCOMPDisplayCompression(WDSP.id(1u, 0u), comp_values, &num);
		}
		if (num == 1)
		{
			double frequencyMinHz = ucCFC_comp.FrequencyMinHz;
			double frequencyMaxHz = ucCFC_comp.FrequencyMaxHz;
			float num2 = (float)_CFCCompValues.Length / 48000f;
			int num3 = (int)(frequencyMaxHz * (double)num2);
			int num4 = (int)(frequencyMinHz * (double)num2);
			int num5 = num3 - num4 + 1;
			if (_cfc_data == null || _cfc_data.Length != num5)
			{
				_cfc_data = new double[num5];
			}
			int num6 = 0;
			for (int i = num4; i <= num3; i++)
			{
				_cfc_data[num6] = _CFCCompValues[i];
				num6++;
			}
			ucCFC_comp.DrawBarChart(_cfc_data);
		}
		_busy = false;
	}

	private void frmCFCConfig_VisibleChanged(object sender, EventArgs e)
	{
		setTimer();
	}

	private void setTimer()
	{
		_timer?.Dispose();
		if (base.Visible)
		{
			_timer = new System.Threading.Timer(timerTick, null, 100, 50);
		}
	}

	private void btnResetComp_Click(object sender, EventArgs e)
	{
		ucCFC_comp.SelectedIndex = -1;
		ucCFC_comp.GlobalGainDb = 0.0;
		ucCFC_comp.ResetPoints();
	}

	private void btnResetEQ_Click(object sender, EventArgs e)
	{
		ucCFC_eq.SelectedIndex = -1;
		ucCFC_eq.GlobalGainDb = 0.0;
		ucCFC_eq.ResetPoints();
	}

	private void nudCFC_selected_band_ValueChanged(object sender, EventArgs e)
	{
		if (!_ignore_udpates)
		{
			_ignore_unselected = true;
			ucCFC_comp.SelectedIndex = (int)(nudCFC_selected_band.Value - 1m);
			ucCFC_eq.SelectedIndex = ucCFC_comp.SelectedIndex;
			_ignore_unselected = false;
			setCFCProfile(ucCFC_eq.SelectedIndex, just_text: true);
		}
	}

	private void frmCFCConfig_FormClosing(object sender, FormClosingEventArgs e)
	{
		Hide();
		e.Cancel = true;
		Common.SaveForm(this, "CFCConfig");
	}

	private void chkCFC_UseQFactors_CheckedChanged(object sender, EventArgs e)
	{
		ucCFC_comp.ParametricEQ = chkCFC_UseQFactors.Checked;
		ucCFC_eq.ParametricEQ = ucCFC_comp.ParametricEQ;
		setCFCProfile();
	}

	public void HighlightTXProfileSaveItems(bool bHighlight)
	{
		Common.HightlightControl(this, bHighlight);
	}

	private void lblOGGuide_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		Common.OpenUri("https://www.w1aex.com/anan/CFC_Audio_Tools/CFC_Audio_Tools.html");
	}

	private void chkLogScale_CheckedChanged(object sender, EventArgs e)
	{
		ucCFC_comp.LogScale = chkLogScale.Checked;
		ucCFC_eq.LogScale = ucCFC_comp.LogScale;
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
		this.lblOGGuide = new System.Windows.Forms.LinkLabel();
		this.chkLogScale = new System.Windows.Forms.CheckBoxTS();
		this.labelTS1 = new System.Windows.Forms.LabelTS();
		this.nudCFC_cq = new System.Windows.Forms.NumericUpDownTS();
		this.btnResetEQ = new System.Windows.Forms.ButtonTS();
		this.ucCFC_comp = new Thetis.ucParametricEq();
		this.labelTS673 = new System.Windows.Forms.LabelTS();
		this.nudCFC_c = new System.Windows.Forms.NumericUpDownTS();
		this.chkCFC_PanaEQ_live = new System.Windows.Forms.CheckBoxTS();
		this.labelTS672 = new System.Windows.Forms.LabelTS();
		this.nudCFC_f = new System.Windows.Forms.NumericUpDownTS();
		this.labelTS670 = new System.Windows.Forms.LabelTS();
		this.labelTS664 = new System.Windows.Forms.LabelTS();
		this.labelTS671 = new System.Windows.Forms.LabelTS();
		this.labelTS667 = new System.Windows.Forms.LabelTS();
		this.nudCFC_posteqgain = new System.Windows.Forms.NumericUpDownTS();
		this.nudCFC_selected_band = new System.Windows.Forms.NumericUpDownTS();
		this.labelTS668 = new System.Windows.Forms.LabelTS();
		this.nudCFC_precomp = new System.Windows.Forms.NumericUpDownTS();
		this.labelTS669 = new System.Windows.Forms.LabelTS();
		this.udCFC_low = new System.Windows.Forms.NumericUpDownTS();
		this.radCFC_10 = new System.Windows.Forms.RadioButtonTS();
		this.chkCFC_UseQFactors = new System.Windows.Forms.CheckBoxTS();
		this.labelTS662 = new System.Windows.Forms.LabelTS();
		this.btnResetComp = new System.Windows.Forms.ButtonTS();
		this.radCFC_18 = new System.Windows.Forms.RadioButtonTS();
		this.labelTS665 = new System.Windows.Forms.LabelTS();
		this.radCFC_5 = new System.Windows.Forms.RadioButtonTS();
		this.nudCFC_gain = new System.Windows.Forms.NumericUpDownTS();
		this.labelTS661 = new System.Windows.Forms.LabelTS();
		this.nudCFC_q = new System.Windows.Forms.NumericUpDownTS();
		this.udCFC_high = new System.Windows.Forms.NumericUpDownTS();
		this.labelTS666 = new System.Windows.Forms.LabelTS();
		this.ucCFC_eq = new Thetis.ucParametricEq();
		this.labelTS663 = new System.Windows.Forms.LabelTS();
		((System.ComponentModel.ISupportInitialize)this.nudCFC_cq).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.nudCFC_c).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.nudCFC_f).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.nudCFC_posteqgain).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.nudCFC_selected_band).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.nudCFC_precomp).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udCFC_low).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.nudCFC_gain).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.nudCFC_q).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udCFC_high).BeginInit();
		base.SuspendLayout();
		this.lblOGGuide.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.lblOGGuide.Location = new System.Drawing.Point(527, 666);
		this.lblOGGuide.Name = "lblOGGuide";
		this.lblOGGuide.Size = new System.Drawing.Size(89, 32);
		this.lblOGGuide.TabIndex = 205;
		this.lblOGGuide.TabStop = true;
		this.lblOGGuide.Text = "OG CFC Guide\r\nby W1AEX";
		this.lblOGGuide.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.lblOGGuide.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(lblOGGuide_LinkClicked);
		this.chkLogScale.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.chkLogScale.Image = null;
		this.chkLogScale.Location = new System.Drawing.Point(527, 155);
		this.chkLogScale.Name = "chkLogScale";
		this.chkLogScale.Size = new System.Drawing.Size(84, 17);
		this.chkLogScale.TabIndex = 206;
		this.chkLogScale.Text = "Log scale";
		this.chkLogScale.UseVisualStyleBackColor = true;
		this.chkLogScale.CheckedChanged += new System.EventHandler(chkLogScale_CheckedChanged);
		this.labelTS1.AutoSize = true;
		this.labelTS1.Image = null;
		this.labelTS1.Location = new System.Drawing.Point(423, 11);
		this.labelTS1.Name = "labelTS1";
		this.labelTS1.Size = new System.Drawing.Size(15, 13);
		this.labelTS1.TabIndex = 204;
		this.labelTS1.Text = "Q";
		this.nudCFC_cq.DecimalPlaces = 2;
		this.nudCFC_cq.Increment = new decimal(new int[4] { 1, 0, 0, 131072 });
		this.nudCFC_cq.Location = new System.Drawing.Point(444, 7);
		this.nudCFC_cq.Maximum = new decimal(new int[4] { 20, 0, 0, 0 });
		this.nudCFC_cq.Minimum = new decimal(new int[4] { 2, 0, 0, 65536 });
		this.nudCFC_cq.Name = "nudCFC_cq";
		this.nudCFC_cq.Size = new System.Drawing.Size(49, 20);
		this.nudCFC_cq.TabIndex = 203;
		this.nudCFC_cq.TinyStep = false;
		this.nudCFC_cq.Value = new decimal(new int[4] { 4, 0, 0, 0 });
		this.nudCFC_cq.ValueChanged += new System.EventHandler(nudCFC_cq_ValueChanged);
		this.btnResetEQ.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnResetEQ.Image = null;
		this.btnResetEQ.Location = new System.Drawing.Point(532, 387);
		this.btnResetEQ.Name = "btnResetEQ";
		this.btnResetEQ.Selectable = true;
		this.btnResetEQ.Size = new System.Drawing.Size(84, 32);
		this.btnResetEQ.TabIndex = 202;
		this.btnResetEQ.Text = "Reset";
		this.btnResetEQ.UseVisualStyleBackColor = true;
		this.btnResetEQ.Click += new System.EventHandler(btnResetEQ_Click);
		this.ucCFC_comp.AllowPointReorder = true;
		this.ucCFC_comp.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.ucCFC_comp.AxisTextColor = System.Drawing.Color.FromArgb(170, 170, 170);
		this.ucCFC_comp.AxisTickColor = System.Drawing.Color.FromArgb(80, 80, 80);
		this.ucCFC_comp.AxisTickLength = 6;
		this.ucCFC_comp.BackColor = System.Drawing.Color.FromArgb(25, 25, 25);
		this.ucCFC_comp.BandShadeAlpha = 70;
		this.ucCFC_comp.BandShadeColor = System.Drawing.Color.FromArgb(200, 200, 200);
		this.ucCFC_comp.BandShadeWeightCutoff = 0.002;
		this.ucCFC_comp.BarChartFillColor = System.Drawing.Color.FromArgb(0, 120, 255);
		this.ucCFC_comp.BarChartPeakColor = System.Drawing.Color.FromArgb(160, 210, 255);
		this.ucCFC_comp.DbMax = 16.0;
		this.ucCFC_comp.DbMin = 0.0;
		this.ucCFC_comp.ForeColor = System.Drawing.Color.Gainsboro;
		this.ucCFC_comp.FrequencyMaxHz = 4000.0;
		this.ucCFC_comp.FrequencyMinHz = 0.0;
		this.ucCFC_comp.GlobalGainDb = 0.0;
		this.ucCFC_comp.GlobalGainIsHorizLine = true;
		this.ucCFC_comp.Location = new System.Drawing.Point(12, 36);
		this.ucCFC_comp.MinPointSpacingHz = 5.0;
		this.ucCFC_comp.Name = "ucCFC_comp";
		this.ucCFC_comp.ParametricEQ = true;
		this.ucCFC_comp.QMax = 20.0;
		this.ucCFC_comp.QMin = 0.2;
		this.ucCFC_comp.SelectedIndex = -1;
		this.ucCFC_comp.ShowAxisScales = true;
		this.ucCFC_comp.ShowBandShading = true;
		this.ucCFC_comp.ShowDotReadings = true;
		this.ucCFC_comp.ShowDotReadingsAsComp = true;
		this.ucCFC_comp.ShowReadout = false;
		this.ucCFC_comp.Size = new System.Drawing.Size(509, 320);
		this.ucCFC_comp.TabIndex = 201;
		this.ucCFC_comp.UsePerBandColours = true;
		this.ucCFC_comp.YAxisStepDb = 2.0;
		this.ucCFC_comp.PointsChanged += new System.EventHandler<Thetis.ucParametricEq.EqDraggingEventArgs>(ucCFC_comp_PointsChanged);
		this.ucCFC_comp.GlobalGainChanged += new System.EventHandler<Thetis.ucParametricEq.EqDraggingEventArgs>(ucCFC_comp_GlobalGainChanged);
		this.ucCFC_comp.PointDataChanged += new System.EventHandler<Thetis.ucParametricEq.EqPointDataChangedEventArgs>(ucCFC_comp_PointDataChanged);
		this.ucCFC_comp.PointSelected += new System.EventHandler<Thetis.ucParametricEq.EqPointSelectionChangedEventArgs>(ucCFC_comp_PointSelected);
		this.ucCFC_comp.PointUnselected += new System.EventHandler<Thetis.ucParametricEq.EqPointSelectionChangedEventArgs>(ucCFC_comp_PointUnselected);
		this.labelTS673.AutoSize = true;
		this.labelTS673.Image = null;
		this.labelTS673.Location = new System.Drawing.Point(396, 11);
		this.labelTS673.Name = "labelTS673";
		this.labelTS673.Size = new System.Drawing.Size(20, 13);
		this.labelTS673.TabIndex = 200;
		this.labelTS673.Text = "dB";
		this.nudCFC_c.DecimalPlaces = 1;
		this.nudCFC_c.Increment = new decimal(new int[4] { 1, 0, 0, 65536 });
		this.nudCFC_c.Location = new System.Drawing.Point(341, 7);
		this.nudCFC_c.Maximum = new decimal(new int[4] { 16, 0, 0, 0 });
		this.nudCFC_c.Minimum = new decimal(new int[4]);
		this.nudCFC_c.Name = "nudCFC_c";
		this.nudCFC_c.Size = new System.Drawing.Size(49, 20);
		this.nudCFC_c.TabIndex = 198;
		this.nudCFC_c.TinyStep = false;
		this.nudCFC_c.Value = new decimal(new int[4]);
		this.nudCFC_c.ValueChanged += new System.EventHandler(nudCFC_c_ValueChanged);
		this.chkCFC_PanaEQ_live.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.chkCFC_PanaEQ_live.Image = null;
		this.chkCFC_PanaEQ_live.Location = new System.Drawing.Point(527, 201);
		this.chkCFC_PanaEQ_live.Name = "chkCFC_PanaEQ_live";
		this.chkCFC_PanaEQ_live.Size = new System.Drawing.Size(84, 17);
		this.chkCFC_PanaEQ_live.TabIndex = 188;
		this.chkCFC_PanaEQ_live.Text = "Live Update";
		this.chkCFC_PanaEQ_live.UseVisualStyleBackColor = true;
		this.labelTS672.AutoSize = true;
		this.labelTS672.Image = null;
		this.labelTS672.Location = new System.Drawing.Point(302, 11);
		this.labelTS672.Name = "labelTS672";
		this.labelTS672.Size = new System.Drawing.Size(34, 13);
		this.labelTS672.TabIndex = 199;
		this.labelTS672.Text = "Comp";
		this.nudCFC_f.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.nudCFC_f.Location = new System.Drawing.Point(91, 7);
		this.nudCFC_f.Maximum = new decimal(new int[4] { 20000, 0, 0, 0 });
		this.nudCFC_f.Minimum = new decimal(new int[4]);
		this.nudCFC_f.Name = "nudCFC_f";
		this.nudCFC_f.Size = new System.Drawing.Size(50, 20);
		this.nudCFC_f.TabIndex = 186;
		this.nudCFC_f.TinyStep = false;
		this.nudCFC_f.Value = new decimal(new int[4] { 16000, 0, 0, 0 });
		this.nudCFC_f.ValueChanged += new System.EventHandler(nudCFC_f_ValueChanged);
		this.labelTS670.AutoSize = true;
		this.labelTS670.Image = null;
		this.labelTS670.Location = new System.Drawing.Point(163, 367);
		this.labelTS670.Name = "labelTS670";
		this.labelTS670.Size = new System.Drawing.Size(20, 13);
		this.labelTS670.TabIndex = 197;
		this.labelTS670.Text = "dB";
		this.labelTS664.AutoSize = true;
		this.labelTS664.Image = null;
		this.labelTS664.Location = new System.Drawing.Point(76, 11);
		this.labelTS664.Name = "labelTS664";
		this.labelTS664.Size = new System.Drawing.Size(10, 13);
		this.labelTS664.TabIndex = 187;
		this.labelTS664.Text = "f";
		this.labelTS671.AutoSize = true;
		this.labelTS671.Image = null;
		this.labelTS671.Location = new System.Drawing.Point(53, 367);
		this.labelTS671.Name = "labelTS671";
		this.labelTS671.Size = new System.Drawing.Size(46, 13);
		this.labelTS671.TabIndex = 196;
		this.labelTS671.Text = "Post-EQ";
		this.labelTS667.AutoSize = true;
		this.labelTS667.Image = null;
		this.labelTS667.Location = new System.Drawing.Point(9, 11);
		this.labelTS667.Name = "labelTS667";
		this.labelTS667.Size = new System.Drawing.Size(14, 13);
		this.labelTS667.TabIndex = 180;
		this.labelTS667.Text = "#";
		this.nudCFC_posteqgain.DecimalPlaces = 1;
		this.nudCFC_posteqgain.Increment = new decimal(new int[4] { 1, 0, 0, 65536 });
		this.nudCFC_posteqgain.Location = new System.Drawing.Point(108, 363);
		this.nudCFC_posteqgain.Maximum = new decimal(new int[4] { 24, 0, 0, 0 });
		this.nudCFC_posteqgain.Minimum = new decimal(new int[4] { 24, 0, 0, -2147483648 });
		this.nudCFC_posteqgain.Name = "nudCFC_posteqgain";
		this.nudCFC_posteqgain.Size = new System.Drawing.Size(49, 20);
		this.nudCFC_posteqgain.TabIndex = 195;
		this.nudCFC_posteqgain.TinyStep = false;
		this.nudCFC_posteqgain.Value = new decimal(new int[4]);
		this.nudCFC_posteqgain.ValueChanged += new System.EventHandler(nudCFC_posteqgain_ValueChanged);
		this.nudCFC_selected_band.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.nudCFC_selected_band.Location = new System.Drawing.Point(29, 7);
		this.nudCFC_selected_band.Maximum = new decimal(new int[4] { 10, 0, 0, 0 });
		this.nudCFC_selected_band.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.nudCFC_selected_band.Name = "nudCFC_selected_band";
		this.nudCFC_selected_band.ReadOnly = true;
		this.nudCFC_selected_band.Size = new System.Drawing.Size(38, 20);
		this.nudCFC_selected_band.TabIndex = 179;
		this.nudCFC_selected_band.TinyStep = false;
		this.nudCFC_selected_band.Value = new decimal(new int[4] { 10, 0, 0, 0 });
		this.nudCFC_selected_band.ValueChanged += new System.EventHandler(nudCFC_selected_band_ValueChanged);
		this.labelTS668.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.labelTS668.AutoSize = true;
		this.labelTS668.Image = null;
		this.labelTS668.Location = new System.Drawing.Point(529, 95);
		this.labelTS668.Name = "labelTS668";
		this.labelTS668.Size = new System.Drawing.Size(27, 13);
		this.labelTS668.TabIndex = 193;
		this.labelTS668.Text = "Low";
		this.nudCFC_precomp.DecimalPlaces = 1;
		this.nudCFC_precomp.Increment = new decimal(new int[4] { 1, 0, 0, 65536 });
		this.nudCFC_precomp.Location = new System.Drawing.Point(218, 7);
		this.nudCFC_precomp.Maximum = new decimal(new int[4] { 16, 0, 0, 0 });
		this.nudCFC_precomp.Minimum = new decimal(new int[4]);
		this.nudCFC_precomp.Name = "nudCFC_precomp";
		this.nudCFC_precomp.Size = new System.Drawing.Size(49, 20);
		this.nudCFC_precomp.TabIndex = 189;
		this.nudCFC_precomp.TinyStep = false;
		this.nudCFC_precomp.Value = new decimal(new int[4]);
		this.nudCFC_precomp.ValueChanged += new System.EventHandler(nudCFC_precomp_ValueChanged);
		this.labelTS669.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.labelTS669.AutoSize = true;
		this.labelTS669.Image = null;
		this.labelTS669.Location = new System.Drawing.Point(527, 121);
		this.labelTS669.Name = "labelTS669";
		this.labelTS669.Size = new System.Drawing.Size(29, 13);
		this.labelTS669.TabIndex = 194;
		this.labelTS669.Text = "High";
		this.udCFC_low.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.udCFC_low.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udCFC_low.Location = new System.Drawing.Point(562, 93);
		this.udCFC_low.Maximum = new decimal(new int[4] { 20000, 0, 0, 0 });
		this.udCFC_low.Minimum = new decimal(new int[4]);
		this.udCFC_low.Name = "udCFC_low";
		this.udCFC_low.Size = new System.Drawing.Size(50, 20);
		this.udCFC_low.TabIndex = 177;
		this.udCFC_low.TinyStep = false;
		this.udCFC_low.Value = new decimal(new int[4]);
		this.udCFC_low.ValueChanged += new System.EventHandler(udCFC_low_ValueChanged);
		this.radCFC_10.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.radCFC_10.AutoSize = true;
		this.radCFC_10.Checked = true;
		this.radCFC_10.Image = null;
		this.radCFC_10.Location = new System.Drawing.Point(553, 35);
		this.radCFC_10.Name = "radCFC_10";
		this.radCFC_10.Size = new System.Drawing.Size(64, 17);
		this.radCFC_10.TabIndex = 174;
		this.radCFC_10.TabStop = true;
		this.radCFC_10.Text = "10-band";
		this.radCFC_10.CheckedChanged += new System.EventHandler(radCFC_bands_CheckedChanged);
		this.chkCFC_UseQFactors.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.chkCFC_UseQFactors.Checked = true;
		this.chkCFC_UseQFactors.CheckState = System.Windows.Forms.CheckState.Checked;
		this.chkCFC_UseQFactors.Image = null;
		this.chkCFC_UseQFactors.Location = new System.Drawing.Point(527, 178);
		this.chkCFC_UseQFactors.Name = "chkCFC_UseQFactors";
		this.chkCFC_UseQFactors.Size = new System.Drawing.Size(94, 17);
		this.chkCFC_UseQFactors.TabIndex = 172;
		this.chkCFC_UseQFactors.Text = "Use Q Factors";
		this.chkCFC_UseQFactors.UseVisualStyleBackColor = true;
		this.chkCFC_UseQFactors.CheckedChanged += new System.EventHandler(chkCFC_UseQFactors_CheckedChanged);
		this.labelTS662.AutoSize = true;
		this.labelTS662.Image = null;
		this.labelTS662.Location = new System.Drawing.Point(159, 11);
		this.labelTS662.Name = "labelTS662";
		this.labelTS662.Size = new System.Drawing.Size(53, 13);
		this.labelTS662.TabIndex = 190;
		this.labelTS662.Text = "Pre-Comp";
		this.btnResetComp.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnResetComp.Image = null;
		this.btnResetComp.Location = new System.Drawing.Point(533, 324);
		this.btnResetComp.Name = "btnResetComp";
		this.btnResetComp.Selectable = true;
		this.btnResetComp.Size = new System.Drawing.Size(84, 32);
		this.btnResetComp.TabIndex = 173;
		this.btnResetComp.Text = "Reset";
		this.btnResetComp.UseVisualStyleBackColor = true;
		this.btnResetComp.Click += new System.EventHandler(btnResetComp_Click);
		this.radCFC_18.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.radCFC_18.AutoSize = true;
		this.radCFC_18.Image = null;
		this.radCFC_18.Location = new System.Drawing.Point(553, 58);
		this.radCFC_18.Name = "radCFC_18";
		this.radCFC_18.Size = new System.Drawing.Size(64, 17);
		this.radCFC_18.TabIndex = 175;
		this.radCFC_18.Text = "18-band";
		this.radCFC_18.CheckedChanged += new System.EventHandler(radCFC_bands_CheckedChanged);
		this.labelTS665.AutoSize = true;
		this.labelTS665.Image = null;
		this.labelTS665.Location = new System.Drawing.Point(304, 367);
		this.labelTS665.Name = "labelTS665";
		this.labelTS665.Size = new System.Drawing.Size(15, 13);
		this.labelTS665.TabIndex = 185;
		this.labelTS665.Text = "Q";
		this.radCFC_5.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.radCFC_5.AutoSize = true;
		this.radCFC_5.Image = null;
		this.radCFC_5.Location = new System.Drawing.Point(553, 12);
		this.radCFC_5.Name = "radCFC_5";
		this.radCFC_5.Size = new System.Drawing.Size(58, 17);
		this.radCFC_5.TabIndex = 176;
		this.radCFC_5.Text = "5-band";
		this.radCFC_5.CheckedChanged += new System.EventHandler(radCFC_bands_CheckedChanged);
		this.nudCFC_gain.DecimalPlaces = 1;
		this.nudCFC_gain.Increment = new decimal(new int[4] { 1, 0, 0, 65536 });
		this.nudCFC_gain.Location = new System.Drawing.Point(224, 363);
		this.nudCFC_gain.Maximum = new decimal(new int[4] { 24, 0, 0, 0 });
		this.nudCFC_gain.Minimum = new decimal(new int[4] { 24, 0, 0, -2147483648 });
		this.nudCFC_gain.Name = "nudCFC_gain";
		this.nudCFC_gain.Size = new System.Drawing.Size(49, 20);
		this.nudCFC_gain.TabIndex = 181;
		this.nudCFC_gain.TinyStep = false;
		this.nudCFC_gain.Value = new decimal(new int[4]);
		this.nudCFC_gain.ValueChanged += new System.EventHandler(nudCFC_gain_ValueChanged);
		this.labelTS661.AutoSize = true;
		this.labelTS661.Image = null;
		this.labelTS661.Location = new System.Drawing.Point(270, 11);
		this.labelTS661.Name = "labelTS661";
		this.labelTS661.Size = new System.Drawing.Size(20, 13);
		this.labelTS661.TabIndex = 191;
		this.labelTS661.Text = "dB";
		this.nudCFC_q.DecimalPlaces = 2;
		this.nudCFC_q.Increment = new decimal(new int[4] { 1, 0, 0, 131072 });
		this.nudCFC_q.Location = new System.Drawing.Point(325, 363);
		this.nudCFC_q.Maximum = new decimal(new int[4] { 20, 0, 0, 0 });
		this.nudCFC_q.Minimum = new decimal(new int[4] { 2, 0, 0, 65536 });
		this.nudCFC_q.Name = "nudCFC_q";
		this.nudCFC_q.Size = new System.Drawing.Size(49, 20);
		this.nudCFC_q.TabIndex = 184;
		this.nudCFC_q.TinyStep = false;
		this.nudCFC_q.Value = new decimal(new int[4] { 4, 0, 0, 0 });
		this.nudCFC_q.ValueChanged += new System.EventHandler(nudCFC_q_ValueChanged);
		this.udCFC_high.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.udCFC_high.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udCFC_high.Location = new System.Drawing.Point(562, 119);
		this.udCFC_high.Maximum = new decimal(new int[4] { 20000, 0, 0, 0 });
		this.udCFC_high.Minimum = new decimal(new int[4]);
		this.udCFC_high.Name = "udCFC_high";
		this.udCFC_high.Size = new System.Drawing.Size(50, 20);
		this.udCFC_high.TabIndex = 178;
		this.udCFC_high.TinyStep = false;
		this.udCFC_high.Value = new decimal(new int[4] { 16000, 0, 0, 0 });
		this.udCFC_high.ValueChanged += new System.EventHandler(udCFC_high_ValueChanged);
		this.labelTS666.AutoSize = true;
		this.labelTS666.Image = null;
		this.labelTS666.Location = new System.Drawing.Point(189, 367);
		this.labelTS666.Name = "labelTS666";
		this.labelTS666.Size = new System.Drawing.Size(29, 13);
		this.labelTS666.TabIndex = 182;
		this.labelTS666.Text = "Gain";
		this.ucCFC_eq.AllowPointReorder = true;
		this.ucCFC_eq.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.ucCFC_eq.AxisTextColor = System.Drawing.Color.FromArgb(170, 170, 170);
		this.ucCFC_eq.AxisTickColor = System.Drawing.Color.FromArgb(80, 80, 80);
		this.ucCFC_eq.AxisTickLength = 6;
		this.ucCFC_eq.BackColor = System.Drawing.Color.FromArgb(25, 25, 25);
		this.ucCFC_eq.BandShadeAlpha = 70;
		this.ucCFC_eq.BandShadeColor = System.Drawing.Color.FromArgb(200, 200, 200);
		this.ucCFC_eq.BandShadeWeightCutoff = 0.002;
		this.ucCFC_eq.BarChartFillColor = System.Drawing.Color.FromArgb(0, 120, 255);
		this.ucCFC_eq.BarChartPeakColor = System.Drawing.Color.FromArgb(160, 210, 255);
		this.ucCFC_eq.DbMax = 24.0;
		this.ucCFC_eq.DbMin = -24.0;
		this.ucCFC_eq.ForeColor = System.Drawing.Color.Gainsboro;
		this.ucCFC_eq.FrequencyMaxHz = 4000.0;
		this.ucCFC_eq.FrequencyMinHz = 0.0;
		this.ucCFC_eq.GlobalGainDb = 0.0;
		this.ucCFC_eq.Location = new System.Drawing.Point(12, 387);
		this.ucCFC_eq.MinPointSpacingHz = 5.0;
		this.ucCFC_eq.Name = "ucCFC_eq";
		this.ucCFC_eq.ParametricEQ = true;
		this.ucCFC_eq.QMax = 20.0;
		this.ucCFC_eq.QMin = 0.2;
		this.ucCFC_eq.SelectedIndex = -1;
		this.ucCFC_eq.ShowAxisScales = true;
		this.ucCFC_eq.ShowBandShading = true;
		this.ucCFC_eq.ShowDotReadings = true;
		this.ucCFC_eq.ShowReadout = false;
		this.ucCFC_eq.Size = new System.Drawing.Size(509, 320);
		this.ucCFC_eq.TabIndex = 0;
		this.ucCFC_eq.UsePerBandColours = true;
		this.ucCFC_eq.PointsChanged += new System.EventHandler<Thetis.ucParametricEq.EqDraggingEventArgs>(ucCFC_eq_PointsChanged);
		this.ucCFC_eq.GlobalGainChanged += new System.EventHandler<Thetis.ucParametricEq.EqDraggingEventArgs>(ucCFC_eq_GlobalGainChanged);
		this.ucCFC_eq.PointDataChanged += new System.EventHandler<Thetis.ucParametricEq.EqPointDataChangedEventArgs>(ucCFC_eq_PointDataChanged);
		this.ucCFC_eq.PointSelected += new System.EventHandler<Thetis.ucParametricEq.EqPointSelectionChangedEventArgs>(ucCFC_eq_PointSelected);
		this.ucCFC_eq.PointUnselected += new System.EventHandler<Thetis.ucParametricEq.EqPointSelectionChangedEventArgs>(ucCFC_eq_PointUnselected);
		this.labelTS663.AutoSize = true;
		this.labelTS663.Image = null;
		this.labelTS663.Location = new System.Drawing.Point(279, 367);
		this.labelTS663.Name = "labelTS663";
		this.labelTS663.Size = new System.Drawing.Size(20, 13);
		this.labelTS663.TabIndex = 183;
		this.labelTS663.Text = "dB";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(624, 717);
		base.Controls.Add(this.chkLogScale);
		base.Controls.Add(this.lblOGGuide);
		base.Controls.Add(this.labelTS1);
		base.Controls.Add(this.nudCFC_cq);
		base.Controls.Add(this.btnResetEQ);
		base.Controls.Add(this.ucCFC_comp);
		base.Controls.Add(this.labelTS673);
		base.Controls.Add(this.nudCFC_c);
		base.Controls.Add(this.chkCFC_PanaEQ_live);
		base.Controls.Add(this.labelTS672);
		base.Controls.Add(this.nudCFC_f);
		base.Controls.Add(this.labelTS670);
		base.Controls.Add(this.labelTS664);
		base.Controls.Add(this.labelTS671);
		base.Controls.Add(this.labelTS667);
		base.Controls.Add(this.nudCFC_posteqgain);
		base.Controls.Add(this.nudCFC_selected_band);
		base.Controls.Add(this.labelTS668);
		base.Controls.Add(this.nudCFC_precomp);
		base.Controls.Add(this.labelTS669);
		base.Controls.Add(this.udCFC_low);
		base.Controls.Add(this.radCFC_10);
		base.Controls.Add(this.chkCFC_UseQFactors);
		base.Controls.Add(this.labelTS662);
		base.Controls.Add(this.btnResetComp);
		base.Controls.Add(this.radCFC_18);
		base.Controls.Add(this.labelTS665);
		base.Controls.Add(this.radCFC_5);
		base.Controls.Add(this.nudCFC_gain);
		base.Controls.Add(this.labelTS661);
		base.Controls.Add(this.nudCFC_q);
		base.Controls.Add(this.udCFC_high);
		base.Controls.Add(this.labelTS666);
		base.Controls.Add(this.ucCFC_eq);
		base.Controls.Add(this.labelTS663);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		this.MaximumSize = new System.Drawing.Size(4096, 756);
		this.MinimumSize = new System.Drawing.Size(640, 756);
		base.Name = "frmCFCConfig";
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
		this.Text = "CFC Config";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmCFCConfig_FormClosing);
		base.VisibleChanged += new System.EventHandler(frmCFCConfig_VisibleChanged);
		((System.ComponentModel.ISupportInitialize)this.nudCFC_cq).EndInit();
		((System.ComponentModel.ISupportInitialize)this.nudCFC_c).EndInit();
		((System.ComponentModel.ISupportInitialize)this.nudCFC_f).EndInit();
		((System.ComponentModel.ISupportInitialize)this.nudCFC_posteqgain).EndInit();
		((System.ComponentModel.ISupportInitialize)this.nudCFC_selected_band).EndInit();
		((System.ComponentModel.ISupportInitialize)this.nudCFC_precomp).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udCFC_low).EndInit();
		((System.ComponentModel.ISupportInitialize)this.nudCFC_gain).EndInit();
		((System.ComponentModel.ISupportInitialize)this.nudCFC_q).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udCFC_high).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
