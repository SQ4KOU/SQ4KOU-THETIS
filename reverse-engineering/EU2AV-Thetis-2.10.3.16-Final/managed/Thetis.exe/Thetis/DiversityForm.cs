using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Timers;
using System.Windows.Forms;

namespace Thetis;

public class DiversityForm : Form
{
	[Serializable]
	private class memorySettings
	{
		public bool enabled;

		public int receiverSource;

		public bool rx1RefSource = true;

		public double rx1Gain = 1.0;

		public double rx2Gain = 1.0;

		public double phase;

		public bool lockPhase;

		public bool lockGain;

		public double gainMulti = 1.0;

		public double angle;

		public double r;
	}

	private Point last_Phase1 = new Point(100, 250);

	private Point last_Phase2 = new Point(100, 100);

	private double angle;

	private double angle_A;

	private double r_A;

	private double steering_angle;

	private double cross_fire;

	private double fine_null;

	private double m_dGainMulti = 1.0;

	private Color topColor = Color.FromArgb(0, 140, 180);

	private Color bottomColor = Color.FromArgb(0, 40, 80);

	private Color lineColor = Color.FromArgb(0, 255, 255);

	private double locked_r;

	private double locked_angle;

	private Console console;

	private PictureBox picRadar;

	private CheckBox chkAuto;

	private TextBox textBox1;

	private NumericUpDownTS udR;

	private NumericUpDownTS udAngle;

	private CheckBox chkEnable;

	private GroupBoxTS panelDivControls;

	private LabelTS labelTS6;

	private ButtonTS btnShift180;

	private ButtonTS btnShiftUp45;

	private GroupBoxTS groupBox_refMerc;

	private RadioButtonTS radioButtonMerc1;

	private RadioButtonTS radioButtonMerc2;

	private LabelTS labelTS3;

	private NumericUpDownTS udR2;

	private NumericUpDownTS udR1;

	private NumericUpDownTS udCalib;

	private LabelTS labelTS5;

	private LabelTS labelTS9;

	private NumericUpDownTS udAngle0;

	private ButtonTS btnShiftDwn45;

	private LabelTS labelDirection;

	private double d_lambda;

	private LabelTS labelTS30;

	private LabelTS labelTS33;

	private LabelTS labelTS40;

	private LabelTS label_d;

	private LabelTS labelTS41;

	private NumericUpDownTS udAntSpacing;

	private CheckBoxTS chkCrossFire;

	private NumericUpDownTS udFineNull;

	private LabelTS labelTS4;

	private GroupBoxTS grpRxSource;

	private RadioButtonTS radRxSourceRx1Rx2;

	private RadioButtonTS radRxSource2;

	private RadioButtonTS radRxSource1;

	private GroupBoxTS groupBoxTS1;

	private CheckBoxTS chkLockR;

	private CheckBoxTS chkLockAngle;

	private CheckBoxTS chkEnableDiversity;

	private bool FormAutoShown;

	private System.Timers.Timer AutoHideTimer;

	private LabelTS labelTS1;

	private NumericUpDownTS udGainMulti;

	private CheckBoxTS chkAlwaysOnTop;

	private ToolTip toolTip1;

	private CheckBoxTS chkNoAttLink;

	private CheckBoxTS chkVFOSync;

	private GroupBoxTS groupBoxTS2;

	private ButtonTS btnM8;

	private ButtonTS btnM7;

	private ButtonTS btnM6;

	private ButtonTS btnM5;

	private ButtonTS btnM4;

	private ButtonTS btnM3;

	private ButtonTS btnM2;

	private ButtonTS btnM1;

	private Label label1;

	private TextBoxTS txtMemoryDataHidden;

	private ButtonTS btnShiftDown10;

	private ButtonTS btnShift90;

	private ButtonTS btnShiftUp10;

	private IContainer components;

	private Label label2;

	private bool _initalising;

	private int _hover_memory_index = -1;

	private int _mouse_down_memory_index = -1;

	private bool _dark_mode;

	private memorySettings[] _memories = new memorySettings[8];

	private Font _font = new Font("Microsoft Sans Serif", 10f, FontStyle.Bold);

	private bool mouse_down;

	private bool updateR2 = true;

	private bool updateR1 = true;

	private int _extDivOutput;

	public bool DarkMode
	{
		get
		{
			return _dark_mode;
		}
		set
		{
			_dark_mode = value;
			if (base.IsHandleCreated)
			{
				bool flag = Common.UseImmersiveDarkMode(base.Handle, _dark_mode);
				if (_dark_mode & flag)
				{
					BackColor = Color.FromArgb(64, 64, 64);
					picRadar.BackColor = Color.FromArgb(64, 64, 64);
				}
				else
				{
					BackColor = SystemColors.Control;
					picRadar.BackColor = SystemColors.Control;
				}
				applyControlStyles(this, _dark_mode & flag);
			}
		}
	}

	public bool DiversityRXRef
	{
		get
		{
			if (radioButtonMerc1.Checked)
			{
				return true;
			}
			return false;
		}
		set
		{
			if (value)
			{
				radioButtonMerc1.Checked = true;
			}
			else
			{
				radioButtonMerc2.Checked = true;
			}
		}
	}

	public int DiversityRXSource
	{
		get
		{
			if (radRxSourceRx1Rx2.Checked)
			{
				return 0;
			}
			if (radRxSource1.Checked)
			{
				return 1;
			}
			return 2;
		}
		set
		{
			switch (value)
			{
			case 0:
				radRxSourceRx1Rx2.Checked = true;
				break;
			case 1:
				radRxSource1.Checked = true;
				break;
			default:
				radRxSource2.Checked = true;
				break;
			}
		}
	}

	public decimal CATDiversityGain
	{
		get
		{
			if (radioButtonMerc1.Checked)
			{
				return DiversityR2Gain;
			}
			return DiversityGain;
		}
		set
		{
			if (radioButtonMerc1.Checked)
			{
				DiversityR2Gain = value;
			}
			else
			{
				DiversityGain = value;
			}
		}
	}

	public decimal DiversityGain
	{
		get
		{
			return udR1.Value;
		}
		set
		{
			bool flag = chkLockR.Checked;
			chkLockR.Checked = false;
			decimal val = Math.Min(value, udR1.Maximum);
			val = Math.Max(val, udR1.Minimum);
			_initalising = true;
			udR1.Value = val;
			_initalising = false;
			udR1_ValueChanged(this, EventArgs.Empty);
			chkLockR.Checked = flag;
		}
	}

	public decimal DiversityR2Gain
	{
		get
		{
			return udR2.Value;
		}
		set
		{
			bool flag = chkLockR.Checked;
			chkLockR.Checked = false;
			decimal val = Math.Min(value, udR2.Maximum);
			val = Math.Max(val, udR2.Minimum);
			_initalising = true;
			udR2.Value = val;
			_initalising = false;
			udR2_ValueChanged(this, EventArgs.Empty);
			chkLockR.Checked = flag;
		}
	}

	public decimal DiversityPhase
	{
		get
		{
			return udFineNull.Value;
		}
		set
		{
			bool flag = chkLockAngle.Checked;
			chkLockAngle.Checked = false;
			udFineNull.Value = value;
			udFineNull_ValueChanged(this, EventArgs.Empty);
			chkLockAngle.Checked = flag;
		}
	}

	public bool DiversityEnabled
	{
		get
		{
			if (chkEnableDiversity.Checked)
			{
				return true;
			}
			return false;
		}
		set
		{
			chkEnableDiversity.Checked = value;
		}
	}

	public int EXTDIVOutput => _extDivOutput;

	public bool VFOSync
	{
		get
		{
			return chkVFOSync.Checked;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke((Action)delegate
				{
					chkVFOSync.Checked = value;
				});
			}
			else
			{
				chkVFOSync.Checked = value;
			}
		}
	}

	public DiversityForm(Console c)
	{
		_initalising = true;
		InitializeComponent();
		Common.DoubleBufferAll(this, enabled: true);
		setNewNaming(new_naming: true);
		console = c;
		udR1.Maximum = udGainMulti.Maximum;
		udR2.Maximum = udGainMulti.Maximum;
		chkVFOSync.Checked = console.VFOSync;
		Common.RestoreForm(this, "DiversityForm", restore_size: true);
		try
		{
			_memories = DeserializeStringToObject<memorySettings[]>(txtMemoryDataHidden.Text);
		}
		catch
		{
			initMemories();
		}
		bool flag = chkLockAngle.Checked;
		bool flag2 = chkLockR.Checked;
		_initalising = true;
		chkLockAngle.Checked = false;
		chkLockR.Checked = false;
		_initalising = false;
		EventArgs empty = EventArgs.Empty;
		udGainMulti_ValueChanged(this, empty);
		radRxSource1_CheckedChanged(this, empty);
		radRxSource2_CheckedChanged(this, empty);
		radRxSourceRx1Rx2_CheckedChanged(this, empty);
		radioButtonMerc1_CheckedChanged(this, empty);
		radioButtonMerc2_CheckedChanged(this, empty);
		_initalising = true;
		chkLockAngle.Checked = flag;
		chkLockR.Checked = flag2;
		_initalising = false;
		chkLockR_CheckedChanged(this, empty);
		chkLockAngle_CheckedChanged(this, empty);
		WDSP.SetEXTDIVNr(0, 2);
		UpdateDiversity();
		chkEnableDiversity_CheckedChanged(this, empty);
		AutoHideTimer = new System.Timers.Timer();
		AutoHideTimer.Elapsed += Callback;
		AutoHideTimer.Enabled = false;
		if (console != null && !console.IsSetupFormNull)
		{
			DarkMode = console.SetupForm.DarkMode;
		}
	}

	private void applyControlStyles(Control parent, bool dark_mode)
	{
		Color foreColor = (dark_mode ? Color.White : Color.Black);
		foreach (Control control in parent.Controls)
		{
			control.ForeColor = foreColor;
			if (control is ButtonTS buttonTS)
			{
				if (dark_mode)
				{
					buttonTS.FlatStyle = FlatStyle.Flat;
					buttonTS.UseVisualStyleBackColor = false;
				}
				else
				{
					buttonTS.FlatStyle = FlatStyle.Standard;
					buttonTS.UseVisualStyleBackColor = true;
				}
			}
			if (control is CheckBoxTS checkBoxTS)
			{
				checkBoxTS.ForeColor = foreColor;
				if (checkBoxTS.Appearance == Appearance.Button)
				{
					if (dark_mode)
					{
						checkBoxTS.FlatStyle = FlatStyle.Flat;
						checkBoxTS.UseVisualStyleBackColor = false;
						checkBoxTS.FlatAppearance.CheckedBackColor = Color.LimeGreen;
					}
					else
					{
						checkBoxTS.FlatStyle = FlatStyle.Standard;
						checkBoxTS.UseVisualStyleBackColor = true;
						if (checkBoxTS == chkVFOSync)
						{
							if (checkBoxTS.Checked)
							{
								checkBoxTS.BackColor = Color.LimeGreen;
							}
							else
							{
								checkBoxTS.BackColor = Color.Empty;
							}
						}
						else if (checkBoxTS == chkEnableDiversity)
						{
							if (checkBoxTS.Checked)
							{
								checkBoxTS.BackColor = Color.LimeGreen;
							}
							else
							{
								checkBoxTS.BackColor = Color.Red;
							}
						}
					}
				}
				else
				{
					checkBoxTS.UseVisualStyleBackColor = true;
				}
			}
			if (control is NumericUpDownTS numericUpDownTS)
			{
				numericUpDownTS.ForeColor = foreColor;
				Color backColor = (numericUpDownTS.BackColor = (dark_mode ? Color.FromArgb(64, 64, 64) : SystemColors.Window));
				for (int i = 0; i < numericUpDownTS.Controls.Count; i++)
				{
					if (numericUpDownTS.Controls[i] is TextBox textBox)
					{
						textBox.BackColor = backColor;
						textBox.ForeColor = foreColor;
					}
				}
			}
			if (control.HasChildren)
			{
				applyControlStyles(control, dark_mode);
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			console.Diversity2 = false;
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.DiversityForm));
		this.picRadar = new System.Windows.Forms.PictureBox();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		this.btnM8 = new System.Windows.Forms.ButtonTS();
		this.btnM7 = new System.Windows.Forms.ButtonTS();
		this.btnM6 = new System.Windows.Forms.ButtonTS();
		this.btnM5 = new System.Windows.Forms.ButtonTS();
		this.btnM4 = new System.Windows.Forms.ButtonTS();
		this.btnM3 = new System.Windows.Forms.ButtonTS();
		this.btnM2 = new System.Windows.Forms.ButtonTS();
		this.btnM1 = new System.Windows.Forms.ButtonTS();
		this.chkVFOSync = new System.Windows.Forms.CheckBoxTS();
		this.chkNoAttLink = new System.Windows.Forms.CheckBoxTS();
		this.chkAlwaysOnTop = new System.Windows.Forms.CheckBoxTS();
		this.groupBoxTS2 = new System.Windows.Forms.GroupBoxTS();
		this.txtMemoryDataHidden = new System.Windows.Forms.TextBoxTS();
		this.labelTS9 = new System.Windows.Forms.LabelTS();
		this.udAngle0 = new System.Windows.Forms.NumericUpDownTS();
		this.chkAuto = new System.Windows.Forms.CheckBox();
		this.chkCrossFire = new System.Windows.Forms.CheckBoxTS();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.labelDirection = new System.Windows.Forms.LabelTS();
		this.udCalib = new System.Windows.Forms.NumericUpDownTS();
		this.udAngle = new System.Windows.Forms.NumericUpDownTS();
		this.chkEnable = new System.Windows.Forms.CheckBox();
		this.groupBoxTS1 = new System.Windows.Forms.GroupBoxTS();
		this.labelTS41 = new System.Windows.Forms.LabelTS();
		this.labelTS30 = new System.Windows.Forms.LabelTS();
		this.labelTS40 = new System.Windows.Forms.LabelTS();
		this.label_d = new System.Windows.Forms.LabelTS();
		this.udAntSpacing = new System.Windows.Forms.NumericUpDownTS();
		this.labelTS5 = new System.Windows.Forms.LabelTS();
		this.udR = new System.Windows.Forms.NumericUpDownTS();
		this.panelDivControls = new System.Windows.Forms.GroupBoxTS();
		this.label2 = new System.Windows.Forms.Label();
		this.btnShiftDown10 = new System.Windows.Forms.ButtonTS();
		this.btnShift90 = new System.Windows.Forms.ButtonTS();
		this.btnShiftUp10 = new System.Windows.Forms.ButtonTS();
		this.label1 = new System.Windows.Forms.Label();
		this.chkEnableDiversity = new System.Windows.Forms.CheckBoxTS();
		this.grpRxSource = new System.Windows.Forms.GroupBoxTS();
		this.radRxSourceRx1Rx2 = new System.Windows.Forms.RadioButtonTS();
		this.radRxSource2 = new System.Windows.Forms.RadioButtonTS();
		this.radRxSource1 = new System.Windows.Forms.RadioButtonTS();
		this.btnShiftDwn45 = new System.Windows.Forms.ButtonTS();
		this.btnShift180 = new System.Windows.Forms.ButtonTS();
		this.labelTS6 = new System.Windows.Forms.LabelTS();
		this.groupBox_refMerc = new System.Windows.Forms.GroupBoxTS();
		this.labelTS1 = new System.Windows.Forms.LabelTS();
		this.udGainMulti = new System.Windows.Forms.NumericUpDownTS();
		this.chkLockAngle = new System.Windows.Forms.CheckBoxTS();
		this.chkLockR = new System.Windows.Forms.CheckBoxTS();
		this.labelTS4 = new System.Windows.Forms.LabelTS();
		this.labelTS33 = new System.Windows.Forms.LabelTS();
		this.udFineNull = new System.Windows.Forms.NumericUpDownTS();
		this.labelTS3 = new System.Windows.Forms.LabelTS();
		this.udR2 = new System.Windows.Forms.NumericUpDownTS();
		this.udR1 = new System.Windows.Forms.NumericUpDownTS();
		this.radioButtonMerc2 = new System.Windows.Forms.RadioButtonTS();
		this.radioButtonMerc1 = new System.Windows.Forms.RadioButtonTS();
		this.btnShiftUp45 = new System.Windows.Forms.ButtonTS();
		((System.ComponentModel.ISupportInitialize)this.picRadar).BeginInit();
		this.groupBoxTS2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.udAngle0).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udCalib).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udAngle).BeginInit();
		this.groupBoxTS1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.udAntSpacing).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udR).BeginInit();
		this.panelDivControls.SuspendLayout();
		this.grpRxSource.SuspendLayout();
		this.groupBox_refMerc.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.udGainMulti).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udFineNull).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udR2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udR1).BeginInit();
		base.SuspendLayout();
		this.picRadar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.picRadar.BackColor = System.Drawing.SystemColors.Control;
		this.picRadar.Location = new System.Drawing.Point(4, 289);
		this.picRadar.Name = "picRadar";
		this.picRadar.Size = new System.Drawing.Size(305, 305);
		this.picRadar.TabIndex = 0;
		this.picRadar.TabStop = false;
		this.picRadar.Paint += new System.Windows.Forms.PaintEventHandler(picRadar_Paint);
		this.picRadar.MouseDown += new System.Windows.Forms.MouseEventHandler(picRadar_MouseDown);
		this.picRadar.MouseLeave += new System.EventHandler(picRadar_MouseLeave);
		this.picRadar.MouseMove += new System.Windows.Forms.MouseEventHandler(picRadar_MouseMove);
		this.picRadar.MouseUp += new System.Windows.Forms.MouseEventHandler(picRadar_MouseUp);
		this.btnM8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.btnM8.Image = null;
		this.btnM8.Location = new System.Drawing.Point(51, 198);
		this.btnM8.Name = "btnM8";
		this.btnM8.Selectable = true;
		this.btnM8.Size = new System.Drawing.Size(36, 23);
		this.btnM8.TabIndex = 112;
		this.btnM8.Text = "M8";
		this.toolTip1.SetToolTip(this.btnM8, "Memory. Shift click to store. Ctrl click to remove.");
		this.btnM8.UseVisualStyleBackColor = false;
		this.btnM8.Click += new System.EventHandler(btnMemory_Click);
		this.btnM7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.btnM7.Image = null;
		this.btnM7.Location = new System.Drawing.Point(6, 198);
		this.btnM7.Name = "btnM7";
		this.btnM7.Selectable = true;
		this.btnM7.Size = new System.Drawing.Size(36, 23);
		this.btnM7.TabIndex = 111;
		this.btnM7.Text = "M7";
		this.toolTip1.SetToolTip(this.btnM7, "Memory. Shift click to store. Ctrl click to remove.");
		this.btnM7.UseVisualStyleBackColor = false;
		this.btnM7.Click += new System.EventHandler(btnMemory_Click);
		this.btnM6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.btnM6.Image = null;
		this.btnM6.Location = new System.Drawing.Point(51, 173);
		this.btnM6.Name = "btnM6";
		this.btnM6.Selectable = true;
		this.btnM6.Size = new System.Drawing.Size(36, 23);
		this.btnM6.TabIndex = 110;
		this.btnM6.Text = "M6";
		this.toolTip1.SetToolTip(this.btnM6, "Memory. Shift click to store. Ctrl click to remove.");
		this.btnM6.UseVisualStyleBackColor = false;
		this.btnM6.Click += new System.EventHandler(btnMemory_Click);
		this.btnM5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.btnM5.Image = null;
		this.btnM5.Location = new System.Drawing.Point(6, 173);
		this.btnM5.Name = "btnM5";
		this.btnM5.Selectable = true;
		this.btnM5.Size = new System.Drawing.Size(36, 23);
		this.btnM5.TabIndex = 109;
		this.btnM5.Text = "M5";
		this.toolTip1.SetToolTip(this.btnM5, "Memory. Shift click to store. Ctrl click to remove.");
		this.btnM5.UseVisualStyleBackColor = false;
		this.btnM5.Click += new System.EventHandler(btnMemory_Click);
		this.btnM4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.btnM4.Image = null;
		this.btnM4.Location = new System.Drawing.Point(51, 147);
		this.btnM4.Name = "btnM4";
		this.btnM4.Selectable = true;
		this.btnM4.Size = new System.Drawing.Size(36, 23);
		this.btnM4.TabIndex = 108;
		this.btnM4.Text = "M4";
		this.toolTip1.SetToolTip(this.btnM4, "Memory. Shift click to store. Ctrl click to remove.");
		this.btnM4.UseVisualStyleBackColor = false;
		this.btnM4.Click += new System.EventHandler(btnMemory_Click);
		this.btnM3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.btnM3.Image = null;
		this.btnM3.Location = new System.Drawing.Point(6, 147);
		this.btnM3.Name = "btnM3";
		this.btnM3.Selectable = true;
		this.btnM3.Size = new System.Drawing.Size(36, 23);
		this.btnM3.TabIndex = 107;
		this.btnM3.Text = "M3";
		this.toolTip1.SetToolTip(this.btnM3, "Memory. Shift click to store. Ctrl click to remove.");
		this.btnM3.UseVisualStyleBackColor = false;
		this.btnM3.Click += new System.EventHandler(btnMemory_Click);
		this.btnM2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.btnM2.Image = null;
		this.btnM2.Location = new System.Drawing.Point(51, 122);
		this.btnM2.Name = "btnM2";
		this.btnM2.Selectable = true;
		this.btnM2.Size = new System.Drawing.Size(36, 23);
		this.btnM2.TabIndex = 106;
		this.btnM2.Text = "M2";
		this.toolTip1.SetToolTip(this.btnM2, "Memory. Shift click to store. Ctrl click to remove.");
		this.btnM2.UseVisualStyleBackColor = false;
		this.btnM2.Click += new System.EventHandler(btnMemory_Click);
		this.btnM1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.btnM1.Image = null;
		this.btnM1.Location = new System.Drawing.Point(6, 122);
		this.btnM1.Name = "btnM1";
		this.btnM1.Selectable = true;
		this.btnM1.Size = new System.Drawing.Size(36, 23);
		this.btnM1.TabIndex = 105;
		this.btnM1.Text = "M1";
		this.toolTip1.SetToolTip(this.btnM1, "Memory. Shift click to store. Ctrl click to remove.");
		this.btnM1.UseVisualStyleBackColor = false;
		this.btnM1.Click += new System.EventHandler(btnMemory_Click);
		this.chkVFOSync.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkVFOSync.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.chkVFOSync.Image = null;
		this.chkVFOSync.Location = new System.Drawing.Point(204, 48);
		this.chkVFOSync.Name = "chkVFOSync";
		this.chkVFOSync.Size = new System.Drawing.Size(94, 23);
		this.chkVFOSync.TabIndex = 104;
		this.chkVFOSync.Text = "VFO Sync";
		this.chkVFOSync.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip1.SetToolTip(this.chkVFOSync, "Enable VFO sync");
		this.chkVFOSync.UseVisualStyleBackColor = false;
		this.chkVFOSync.CheckedChanged += new System.EventHandler(chkVFOSync_CheckedChanged);
		this.chkNoAttLink.AutoSize = true;
		this.chkNoAttLink.Image = null;
		this.chkNoAttLink.Location = new System.Drawing.Point(204, 78);
		this.chkNoAttLink.Name = "chkNoAttLink";
		this.chkNoAttLink.Size = new System.Drawing.Size(83, 17);
		this.chkNoAttLink.TabIndex = 103;
		this.chkNoAttLink.Text = "No ATT link";
		this.toolTip1.SetToolTip(this.chkNoAttLink, "Normally if RX1+RX2 are in use the attenuators will be linked. Select this if you dont want that to happen.");
		this.chkNoAttLink.UseVisualStyleBackColor = true;
		this.chkNoAttLink.CheckedChanged += new System.EventHandler(chkNoAttLink_CheckedChanged);
		this.chkAlwaysOnTop.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.chkAlwaysOnTop.AutoSize = true;
		this.chkAlwaysOnTop.Image = null;
		this.chkAlwaysOnTop.Location = new System.Drawing.Point(247, 5);
		this.chkAlwaysOnTop.Name = "chkAlwaysOnTop";
		this.chkAlwaysOnTop.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.chkAlwaysOnTop.Size = new System.Drawing.Size(62, 17);
		this.chkAlwaysOnTop.TabIndex = 102;
		this.chkAlwaysOnTop.Text = "On Top";
		this.chkAlwaysOnTop.UseVisualStyleBackColor = true;
		this.chkAlwaysOnTop.CheckedChanged += new System.EventHandler(chkAlwaysOnTop_CheckedChanged);
		this.groupBoxTS2.Controls.Add(this.txtMemoryDataHidden);
		this.groupBoxTS2.Controls.Add(this.labelTS9);
		this.groupBoxTS2.Controls.Add(this.udAngle0);
		this.groupBoxTS2.Controls.Add(this.chkAuto);
		this.groupBoxTS2.Controls.Add(this.chkCrossFire);
		this.groupBoxTS2.Controls.Add(this.textBox1);
		this.groupBoxTS2.Controls.Add(this.labelDirection);
		this.groupBoxTS2.Controls.Add(this.udCalib);
		this.groupBoxTS2.Controls.Add(this.udAngle);
		this.groupBoxTS2.Controls.Add(this.chkEnable);
		this.groupBoxTS2.Controls.Add(this.groupBoxTS1);
		this.groupBoxTS2.Controls.Add(this.labelTS5);
		this.groupBoxTS2.Controls.Add(this.udR);
		this.groupBoxTS2.Location = new System.Drawing.Point(396, 10);
		this.groupBoxTS2.Name = "groupBoxTS2";
		this.groupBoxTS2.Size = new System.Drawing.Size(315, 256);
		this.groupBoxTS2.TabIndex = 101;
		this.groupBoxTS2.TabStop = false;
		this.groupBoxTS2.Text = "hidden";
		this.groupBoxTS2.Visible = false;
		this.txtMemoryDataHidden.Location = new System.Drawing.Point(10, 228);
		this.txtMemoryDataHidden.Name = "txtMemoryDataHidden";
		this.txtMemoryDataHidden.Size = new System.Drawing.Size(100, 20);
		this.txtMemoryDataHidden.TabIndex = 107;
		this.labelTS9.AutoSize = true;
		this.labelTS9.Image = null;
		this.labelTS9.Location = new System.Drawing.Point(24, 21);
		this.labelTS9.Name = "labelTS9";
		this.labelTS9.Size = new System.Drawing.Size(76, 13);
		this.labelTS9.TabIndex = 64;
		this.labelTS9.Text = "Direction (deg)";
		this.udAngle0.BackColor = System.Drawing.Color.White;
		this.udAngle0.DecimalPlaces = 1;
		this.udAngle0.Font = new System.Drawing.Font("Microsoft Sans Serif", 16f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.udAngle0.Increment = new decimal(new int[4] { 1, 0, 0, 65536 });
		this.udAngle0.Location = new System.Drawing.Point(81, 43);
		this.udAngle0.Maximum = new decimal(new int[4] { 361, 0, 0, 0 });
		this.udAngle0.Minimum = new decimal(new int[4] { 1, 0, 0, -2147483648 });
		this.udAngle0.Name = "udAngle0";
		this.udAngle0.Size = new System.Drawing.Size(74, 32);
		this.udAngle0.TabIndex = 64;
		this.udAngle0.TinyStep = false;
		this.udAngle0.Value = new decimal(new int[4]);
		this.udAngle0.ValueChanged += new System.EventHandler(udAngle0_ValueChanged);
		this.chkAuto.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
		this.chkAuto.Enabled = false;
		this.chkAuto.Location = new System.Drawing.Point(209, 164);
		this.chkAuto.Name = "chkAuto";
		this.chkAuto.Size = new System.Drawing.Size(48, 24);
		this.chkAuto.TabIndex = 1;
		this.chkAuto.Text = "Auto";
		this.chkCrossFire.AutoSize = true;
		this.chkCrossFire.Image = null;
		this.chkCrossFire.Location = new System.Drawing.Point(168, 228);
		this.chkCrossFire.Name = "chkCrossFire";
		this.chkCrossFire.Size = new System.Drawing.Size(89, 17);
		this.chkCrossFire.TabIndex = 100;
		this.chkCrossFire.Text = "Enable X-Fire";
		this.chkCrossFire.UseVisualStyleBackColor = true;
		this.chkCrossFire.Visible = false;
		this.chkCrossFire.CheckedChanged += new System.EventHandler(chkCrossFire_CheckedChanged);
		this.textBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
		this.textBox1.Enabled = false;
		this.textBox1.Location = new System.Drawing.Point(194, 130);
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(100, 20);
		this.textBox1.TabIndex = 4;
		this.labelDirection.AutoSize = true;
		this.labelDirection.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
		this.labelDirection.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.labelDirection.Font = new System.Drawing.Font("Microsoft Sans Serif", 20f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelDirection.Image = null;
		this.labelDirection.Location = new System.Drawing.Point(10, 43);
		this.labelDirection.Name = "labelDirection";
		this.labelDirection.Size = new System.Drawing.Size(63, 33);
		this.labelDirection.TabIndex = 65;
		this.labelDirection.Text = "NW";
		this.labelDirection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.udCalib.DecimalPlaces = 3;
		this.udCalib.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.udCalib.Increment = new decimal(new int[4] { 1, 0, 0, 196608 });
		this.udCalib.Location = new System.Drawing.Point(13, 189);
		this.udCalib.Maximum = new decimal(new int[4] { 4, 0, 0, 0 });
		this.udCalib.Minimum = new decimal(new int[4] { 4, 0, 0, -2147483648 });
		this.udCalib.Name = "udCalib";
		this.udCalib.Size = new System.Drawing.Size(60, 23);
		this.udCalib.TabIndex = 60;
		this.udCalib.TinyStep = false;
		this.udCalib.Value = new decimal(new int[4]);
		this.udCalib.ValueChanged += new System.EventHandler(udCalib_ValueChanged);
		this.udAngle.DecimalPlaces = 3;
		this.udAngle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.udAngle.Increment = new decimal(new int[4] { 1, 0, 0, 196608 });
		this.udAngle.Location = new System.Drawing.Point(209, 87);
		this.udAngle.Maximum = new decimal(new int[4] { 65, 0, 0, 65536 });
		this.udAngle.Minimum = new decimal(new int[4] { 65, 0, 0, -2147418112 });
		this.udAngle.Name = "udAngle";
		this.udAngle.Size = new System.Drawing.Size(60, 23);
		this.udAngle.TabIndex = 6;
		this.udAngle.TinyStep = false;
		this.udAngle.Value = new decimal(new int[4]);
		this.udAngle.ValueChanged += new System.EventHandler(udTheta_ValueChanged);
		this.chkEnable.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkEnable.Location = new System.Drawing.Point(221, 39);
		this.chkEnable.Name = "chkEnable";
		this.chkEnable.Size = new System.Drawing.Size(48, 24);
		this.chkEnable.TabIndex = 48;
		this.chkEnable.Text = "Enable";
		this.chkEnable.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkEnable.CheckedChanged += new System.EventHandler(chkEnable_CheckedChanged);
		this.groupBoxTS1.Controls.Add(this.labelTS41);
		this.groupBoxTS1.Controls.Add(this.labelTS30);
		this.groupBoxTS1.Controls.Add(this.labelTS40);
		this.groupBoxTS1.Controls.Add(this.label_d);
		this.groupBoxTS1.Controls.Add(this.udAntSpacing);
		this.groupBoxTS1.Location = new System.Drawing.Point(10, 90);
		this.groupBoxTS1.Name = "groupBoxTS1";
		this.groupBoxTS1.Size = new System.Drawing.Size(142, 75);
		this.groupBoxTS1.TabIndex = 100;
		this.groupBoxTS1.TabStop = false;
		this.groupBoxTS1.Text = "Antenna Spacing";
		this.labelTS41.AutoSize = true;
		this.labelTS41.Image = null;
		this.labelTS41.Location = new System.Drawing.Point(9, 30);
		this.labelTS41.Name = "labelTS41";
		this.labelTS41.Size = new System.Drawing.Size(55, 13);
		this.labelTS41.TabIndex = 98;
		this.labelTS41.Text = "D (meters)";
		this.labelTS30.AutoSize = true;
		this.labelTS30.Font = new System.Drawing.Font("Symbol", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 2);
		this.labelTS30.Image = null;
		this.labelTS30.Location = new System.Drawing.Point(29, 56);
		this.labelTS30.Name = "labelTS30";
		this.labelTS30.Size = new System.Drawing.Size(21, 13);
		this.labelTS30.TabIndex = 87;
		this.labelTS30.Text = "l =";
		this.labelTS40.AutoSize = true;
		this.labelTS40.Image = null;
		this.labelTS40.Location = new System.Drawing.Point(9, 56);
		this.labelTS40.Name = "labelTS40";
		this.labelTS40.Size = new System.Drawing.Size(23, 13);
		this.labelTS40.TabIndex = 95;
		this.labelTS40.Text = " D/";
		this.label_d.AutoSize = true;
		this.label_d.BackColor = System.Drawing.Color.Transparent;
		this.label_d.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.label_d.Image = null;
		this.label_d.Location = new System.Drawing.Point(52, 56);
		this.label_d.Name = "label_d";
		this.label_d.Size = new System.Drawing.Size(15, 15);
		this.label_d.TabIndex = 96;
		this.label_d.Text = "0";
		this.udAntSpacing.BackColor = System.Drawing.Color.White;
		this.udAntSpacing.DecimalPlaces = 2;
		this.udAntSpacing.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.udAntSpacing.ForeColor = System.Drawing.Color.Black;
		this.udAntSpacing.Increment = new decimal(new int[4] { 1, 0, 0, 131072 });
		this.udAntSpacing.Location = new System.Drawing.Point(66, 26);
		this.udAntSpacing.Maximum = new decimal(new int[4] { 500, 0, 0, 0 });
		this.udAntSpacing.Minimum = new decimal(new int[4]);
		this.udAntSpacing.Name = "udAntSpacing";
		this.udAntSpacing.Size = new System.Drawing.Size(60, 23);
		this.udAntSpacing.TabIndex = 97;
		this.udAntSpacing.TinyStep = false;
		this.udAntSpacing.Value = new decimal(new int[4]);
		this.udAntSpacing.ValueChanged += new System.EventHandler(udAntSpacing_ValueChanged_1);
		this.labelTS5.AutoSize = true;
		this.labelTS5.Image = null;
		this.labelTS5.Location = new System.Drawing.Point(10, 173);
		this.labelTS5.Name = "labelTS5";
		this.labelTS5.Size = new System.Drawing.Size(72, 13);
		this.labelTS5.TabIndex = 61;
		this.labelTS5.Text = "calib direction";
		this.udR.DecimalPlaces = 3;
		this.udR.Increment = new decimal(new int[4] { 1, 0, 0, 196608 });
		this.udR.Location = new System.Drawing.Point(122, 189);
		this.udR.Maximum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udR.Minimum = new decimal(new int[4]);
		this.udR.Name = "udR";
		this.udR.Size = new System.Drawing.Size(56, 20);
		this.udR.TabIndex = 5;
		this.udR.TinyStep = false;
		this.udR.Value = new decimal(new int[4]);
		this.udR.ValueChanged += new System.EventHandler(udR_ValueChanged);
		this.panelDivControls.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		this.panelDivControls.Controls.Add(this.label2);
		this.panelDivControls.Controls.Add(this.btnShiftDown10);
		this.panelDivControls.Controls.Add(this.btnShift90);
		this.panelDivControls.Controls.Add(this.btnShiftUp10);
		this.panelDivControls.Controls.Add(this.label1);
		this.panelDivControls.Controls.Add(this.btnM8);
		this.panelDivControls.Controls.Add(this.btnM7);
		this.panelDivControls.Controls.Add(this.btnM6);
		this.panelDivControls.Controls.Add(this.btnM5);
		this.panelDivControls.Controls.Add(this.btnM4);
		this.panelDivControls.Controls.Add(this.btnM3);
		this.panelDivControls.Controls.Add(this.btnM2);
		this.panelDivControls.Controls.Add(this.btnM1);
		this.panelDivControls.Controls.Add(this.chkVFOSync);
		this.panelDivControls.Controls.Add(this.chkNoAttLink);
		this.panelDivControls.Controls.Add(this.chkEnableDiversity);
		this.panelDivControls.Controls.Add(this.grpRxSource);
		this.panelDivControls.Controls.Add(this.btnShiftDwn45);
		this.panelDivControls.Controls.Add(this.btnShift180);
		this.panelDivControls.Controls.Add(this.labelTS6);
		this.panelDivControls.Controls.Add(this.groupBox_refMerc);
		this.panelDivControls.Controls.Add(this.btnShiftUp45);
		this.panelDivControls.ImeMode = System.Windows.Forms.ImeMode.AlphaFull;
		this.panelDivControls.Location = new System.Drawing.Point(4, 22);
		this.panelDivControls.Name = "panelDivControls";
		this.panelDivControls.Size = new System.Drawing.Size(305, 262);
		this.panelDivControls.TabIndex = 51;
		this.panelDivControls.TabStop = false;
		this.panelDivControls.Enter += new System.EventHandler(panelDivControls_Enter);
		this.label2.AutoSize = true;
		this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label2.Location = new System.Drawing.Point(5, 240);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(84, 13);
		this.label2.TabIndex = 117;
		this.label2.Text = "ctrl click to clear";
		this.btnShiftDown10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Bold);
		this.btnShiftDown10.Image = null;
		this.btnShiftDown10.Location = new System.Drawing.Point(47, 87);
		this.btnShiftDown10.Name = "btnShiftDown10";
		this.btnShiftDown10.Selectable = true;
		this.btnShiftDown10.Size = new System.Drawing.Size(40, 26);
		this.btnShiftDown10.TabIndex = 116;
		this.btnShiftDown10.Text = "-10";
		this.btnShiftDown10.UseVisualStyleBackColor = false;
		this.btnShiftDown10.Click += new System.EventHandler(btnShiftDown10_Click);
		this.btnShift90.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Bold);
		this.btnShift90.Image = null;
		this.btnShift90.Location = new System.Drawing.Point(47, 57);
		this.btnShift90.Name = "btnShift90";
		this.btnShift90.Selectable = true;
		this.btnShift90.Size = new System.Drawing.Size(40, 26);
		this.btnShift90.TabIndex = 114;
		this.btnShift90.Text = "90";
		this.btnShift90.UseVisualStyleBackColor = false;
		this.btnShift90.Click += new System.EventHandler(btnShift90_Click);
		this.btnShiftUp10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Bold);
		this.btnShiftUp10.Image = null;
		this.btnShiftUp10.Location = new System.Drawing.Point(47, 27);
		this.btnShiftUp10.Name = "btnShiftUp10";
		this.btnShiftUp10.Selectable = true;
		this.btnShiftUp10.Size = new System.Drawing.Size(40, 26);
		this.btnShiftUp10.TabIndex = 115;
		this.btnShiftUp10.Text = "+10";
		this.btnShiftUp10.UseVisualStyleBackColor = false;
		this.btnShiftUp10.Click += new System.EventHandler(btnShiftUp10_Click);
		this.label1.AutoSize = true;
		this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label1.Location = new System.Drawing.Point(2, 224);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(89, 13);
		this.label1.TabIndex = 113;
		this.label1.Text = "shift click to store";
		this.chkEnableDiversity.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkEnableDiversity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.chkEnableDiversity.Image = null;
		this.chkEnableDiversity.Location = new System.Drawing.Point(204, 19);
		this.chkEnableDiversity.Name = "chkEnableDiversity";
		this.chkEnableDiversity.Size = new System.Drawing.Size(94, 23);
		this.chkEnableDiversity.TabIndex = 101;
		this.chkEnableDiversity.Text = "Enabled";
		this.chkEnableDiversity.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkEnableDiversity.UseVisualStyleBackColor = false;
		this.chkEnableDiversity.CheckedChanged += new System.EventHandler(chkEnableDiversity_CheckedChanged);
		this.grpRxSource.Controls.Add(this.radRxSourceRx1Rx2);
		this.grpRxSource.Controls.Add(this.radRxSource2);
		this.grpRxSource.Controls.Add(this.radRxSource1);
		this.grpRxSource.Location = new System.Drawing.Point(93, 19);
		this.grpRxSource.Name = "grpRxSource";
		this.grpRxSource.Size = new System.Drawing.Size(105, 90);
		this.grpRxSource.TabIndex = 52;
		this.grpRxSource.TabStop = false;
		this.grpRxSource.Text = "Receiver Source";
		this.radRxSourceRx1Rx2.AutoSize = true;
		this.radRxSourceRx1Rx2.Checked = true;
		this.radRxSourceRx1Rx2.Image = null;
		this.radRxSourceRx1Rx2.Location = new System.Drawing.Point(6, 22);
		this.radRxSourceRx1Rx2.Name = "radRxSourceRx1Rx2";
		this.radRxSourceRx1Rx2.Size = new System.Drawing.Size(67, 17);
		this.radRxSourceRx1Rx2.TabIndex = 2;
		this.radRxSourceRx1Rx2.TabStop = true;
		this.radRxSourceRx1Rx2.Text = "Sync1+2";
		this.radRxSourceRx1Rx2.UseVisualStyleBackColor = true;
		this.radRxSourceRx1Rx2.CheckedChanged += new System.EventHandler(radRxSourceRx1Rx2_CheckedChanged);
		this.radRxSource2.AutoSize = true;
		this.radRxSource2.Image = null;
		this.radRxSource2.Location = new System.Drawing.Point(6, 66);
		this.radRxSource2.Name = "radRxSource2";
		this.radRxSource2.Size = new System.Drawing.Size(55, 17);
		this.radRxSource2.TabIndex = 1;
		this.radRxSource2.Text = "Sync2";
		this.radRxSource2.UseVisualStyleBackColor = true;
		this.radRxSource2.CheckedChanged += new System.EventHandler(radRxSource2_CheckedChanged);
		this.radRxSource1.AutoSize = true;
		this.radRxSource1.Image = null;
		this.radRxSource1.Location = new System.Drawing.Point(6, 43);
		this.radRxSource1.Name = "radRxSource1";
		this.radRxSource1.Size = new System.Drawing.Size(55, 17);
		this.radRxSource1.TabIndex = 0;
		this.radRxSource1.Text = "Sync1";
		this.radRxSource1.UseVisualStyleBackColor = true;
		this.radRxSource1.CheckedChanged += new System.EventHandler(radRxSource1_CheckedChanged);
		this.btnShiftDwn45.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Bold);
		this.btnShiftDwn45.Image = null;
		this.btnShiftDwn45.Location = new System.Drawing.Point(6, 87);
		this.btnShiftDwn45.Name = "btnShiftDwn45";
		this.btnShiftDwn45.Selectable = true;
		this.btnShiftDwn45.Size = new System.Drawing.Size(40, 26);
		this.btnShiftDwn45.TabIndex = 62;
		this.btnShiftDwn45.Text = "-45";
		this.btnShiftDwn45.UseVisualStyleBackColor = false;
		this.btnShiftDwn45.Click += new System.EventHandler(btnShiftDwn45_Click);
		this.btnShift180.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Bold);
		this.btnShift180.Image = null;
		this.btnShift180.Location = new System.Drawing.Point(6, 57);
		this.btnShift180.Name = "btnShift180";
		this.btnShift180.Selectable = true;
		this.btnShift180.Size = new System.Drawing.Size(40, 26);
		this.btnShift180.TabIndex = 57;
		this.btnShift180.Text = "180";
		this.btnShift180.UseVisualStyleBackColor = false;
		this.btnShift180.Click += new System.EventHandler(btnShift180_Click);
		this.labelTS6.AutoSize = true;
		this.labelTS6.Image = null;
		this.labelTS6.Location = new System.Drawing.Point(33, 10);
		this.labelTS6.Name = "labelTS6";
		this.labelTS6.Size = new System.Drawing.Size(26, 13);
		this.labelTS6.TabIndex = 54;
		this.labelTS6.Text = "shift";
		this.groupBox_refMerc.Controls.Add(this.labelTS1);
		this.groupBox_refMerc.Controls.Add(this.udGainMulti);
		this.groupBox_refMerc.Controls.Add(this.chkLockAngle);
		this.groupBox_refMerc.Controls.Add(this.chkLockR);
		this.groupBox_refMerc.Controls.Add(this.labelTS4);
		this.groupBox_refMerc.Controls.Add(this.labelTS33);
		this.groupBox_refMerc.Controls.Add(this.udFineNull);
		this.groupBox_refMerc.Controls.Add(this.labelTS3);
		this.groupBox_refMerc.Controls.Add(this.udR2);
		this.groupBox_refMerc.Controls.Add(this.udR1);
		this.groupBox_refMerc.Controls.Add(this.radioButtonMerc2);
		this.groupBox_refMerc.Controls.Add(this.radioButtonMerc1);
		this.groupBox_refMerc.Location = new System.Drawing.Point(93, 115);
		this.groupBox_refMerc.Name = "groupBox_refMerc";
		this.groupBox_refMerc.Size = new System.Drawing.Size(205, 140);
		this.groupBox_refMerc.TabIndex = 59;
		this.groupBox_refMerc.TabStop = false;
		this.groupBox_refMerc.Text = "Reference Source";
		this.groupBox_refMerc.Enter += new System.EventHandler(groupBox_refMerc_Enter);
		this.labelTS1.AutoSize = true;
		this.labelTS1.Image = null;
		this.labelTS1.Location = new System.Drawing.Point(20, 115);
		this.labelTS1.Name = "labelTS1";
		this.labelTS1.Size = new System.Drawing.Size(56, 13);
		this.labelTS1.TabIndex = 106;
		this.labelTS1.Text = "Gain multi:";
		this.udGainMulti.BackColor = System.Drawing.Color.White;
		this.udGainMulti.DecimalPlaces = 2;
		this.udGainMulti.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.udGainMulti.ForeColor = System.Drawing.Color.Black;
		this.udGainMulti.Increment = new decimal(new int[4] { 1, 0, 0, 131072 });
		this.udGainMulti.Location = new System.Drawing.Point(82, 111);
		this.udGainMulti.Maximum = new decimal(new int[4] { 10, 0, 0, 0 });
		this.udGainMulti.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udGainMulti.Name = "udGainMulti";
		this.udGainMulti.Size = new System.Drawing.Size(56, 23);
		this.udGainMulti.TabIndex = 105;
		this.udGainMulti.TinyStep = false;
		this.udGainMulti.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udGainMulti.ValueChanged += new System.EventHandler(udGainMulti_ValueChanged);
		this.chkLockAngle.AutoSize = true;
		this.chkLockAngle.Image = null;
		this.chkLockAngle.Location = new System.Drawing.Point(115, 84);
		this.chkLockAngle.Name = "chkLockAngle";
		this.chkLockAngle.Size = new System.Drawing.Size(83, 17);
		this.chkLockAngle.TabIndex = 104;
		this.chkLockAngle.Text = "Lock Phase";
		this.chkLockAngle.UseVisualStyleBackColor = true;
		this.chkLockAngle.CheckedChanged += new System.EventHandler(chkLockAngle_CheckedChanged);
		this.chkLockR.AutoSize = true;
		this.chkLockR.Image = null;
		this.chkLockR.Location = new System.Drawing.Point(29, 84);
		this.chkLockR.Name = "chkLockR";
		this.chkLockR.Size = new System.Drawing.Size(75, 17);
		this.chkLockR.TabIndex = 103;
		this.chkLockR.Text = "Lock Gain";
		this.chkLockR.UseVisualStyleBackColor = true;
		this.chkLockR.CheckedChanged += new System.EventHandler(chkLockR_CheckedChanged);
		this.labelTS4.AutoSize = true;
		this.labelTS4.Image = null;
		this.labelTS4.Location = new System.Drawing.Point(136, 11);
		this.labelTS4.Name = "labelTS4";
		this.labelTS4.Size = new System.Drawing.Size(37, 13);
		this.labelTS4.TabIndex = 102;
		this.labelTS4.Text = "Phase";
		this.labelTS33.AutoSize = true;
		this.labelTS33.Image = null;
		this.labelTS33.Location = new System.Drawing.Point(240, -13);
		this.labelTS33.Name = "labelTS33";
		this.labelTS33.Size = new System.Drawing.Size(9, 13);
		this.labelTS33.TabIndex = 93;
		this.labelTS33.Text = "i";
		this.udFineNull.BackColor = System.Drawing.Color.White;
		this.udFineNull.DecimalPlaces = 2;
		this.udFineNull.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.udFineNull.ForeColor = System.Drawing.Color.Black;
		this.udFineNull.Increment = new decimal(new int[4] { 5, 0, 0, 131072 });
		this.udFineNull.Location = new System.Drawing.Point(128, 28);
		this.udFineNull.Maximum = new decimal(new int[4] { 180, 0, 0, 0 });
		this.udFineNull.Minimum = new decimal(new int[4] { 180, 0, 0, -2147483648 });
		this.udFineNull.Name = "udFineNull";
		this.udFineNull.Size = new System.Drawing.Size(67, 23);
		this.udFineNull.TabIndex = 101;
		this.udFineNull.TinyStep = false;
		this.udFineNull.Value = new decimal(new int[4]);
		this.udFineNull.ValueChanged += new System.EventHandler(udFineNull_ValueChanged);
		this.labelTS3.AutoSize = true;
		this.labelTS3.Image = null;
		this.labelTS3.Location = new System.Drawing.Point(73, 11);
		this.labelTS3.Name = "labelTS3";
		this.labelTS3.Size = new System.Drawing.Size(29, 13);
		this.labelTS3.TabIndex = 51;
		this.labelTS3.Text = "Gain";
		this.udR2.BackColor = System.Drawing.Color.White;
		this.udR2.DecimalPlaces = 3;
		this.udR2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.udR2.ForeColor = System.Drawing.Color.Black;
		this.udR2.Increment = new decimal(new int[4] { 1, 0, 0, 196608 });
		this.udR2.Location = new System.Drawing.Point(65, 53);
		this.udR2.Maximum = new decimal(new int[4] { 5, 0, 0, 0 });
		this.udR2.Minimum = new decimal(new int[4]);
		this.udR2.Name = "udR2";
		this.udR2.Size = new System.Drawing.Size(56, 23);
		this.udR2.TabIndex = 11;
		this.udR2.TinyStep = false;
		this.udR2.Value = new decimal(new int[4] { 10, 0, 0, 65536 });
		this.udR2.ValueChanged += new System.EventHandler(udR2_ValueChanged);
		this.udR1.BackColor = System.Drawing.Color.White;
		this.udR1.DecimalPlaces = 3;
		this.udR1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.udR1.ForeColor = System.Drawing.Color.Black;
		this.udR1.Increment = new decimal(new int[4] { 1, 0, 0, 196608 });
		this.udR1.Location = new System.Drawing.Point(66, 28);
		this.udR1.Maximum = new decimal(new int[4] { 5, 0, 0, 0 });
		this.udR1.Minimum = new decimal(new int[4]);
		this.udR1.Name = "udR1";
		this.udR1.Size = new System.Drawing.Size(56, 23);
		this.udR1.TabIndex = 10;
		this.udR1.TinyStep = false;
		this.udR1.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udR1.ValueChanged += new System.EventHandler(udR1_ValueChanged);
		this.radioButtonMerc2.AutoSize = true;
		this.radioButtonMerc2.Image = null;
		this.radioButtonMerc2.Location = new System.Drawing.Point(6, 55);
		this.radioButtonMerc2.Name = "radioButtonMerc2";
		this.radioButtonMerc2.Size = new System.Drawing.Size(55, 17);
		this.radioButtonMerc2.TabIndex = 1;
		this.radioButtonMerc2.Text = "Sync2";
		this.radioButtonMerc2.UseVisualStyleBackColor = true;
		this.radioButtonMerc2.CheckedChanged += new System.EventHandler(radioButtonMerc2_CheckedChanged);
		this.radioButtonMerc1.AutoSize = true;
		this.radioButtonMerc1.Checked = true;
		this.radioButtonMerc1.Image = null;
		this.radioButtonMerc1.Location = new System.Drawing.Point(6, 30);
		this.radioButtonMerc1.Name = "radioButtonMerc1";
		this.radioButtonMerc1.Size = new System.Drawing.Size(55, 17);
		this.radioButtonMerc1.TabIndex = 0;
		this.radioButtonMerc1.TabStop = true;
		this.radioButtonMerc1.Text = "Sync1";
		this.radioButtonMerc1.UseVisualStyleBackColor = true;
		this.radioButtonMerc1.CheckedChanged += new System.EventHandler(radioButtonMerc1_CheckedChanged);
		this.btnShiftUp45.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Bold);
		this.btnShiftUp45.Image = null;
		this.btnShiftUp45.Location = new System.Drawing.Point(6, 27);
		this.btnShiftUp45.Name = "btnShiftUp45";
		this.btnShiftUp45.Selectable = true;
		this.btnShiftUp45.Size = new System.Drawing.Size(40, 26);
		this.btnShiftUp45.TabIndex = 58;
		this.btnShiftUp45.Text = "+45";
		this.btnShiftUp45.UseVisualStyleBackColor = false;
		this.btnShiftUp45.Click += new System.EventHandler(btnShiftUp45_Click);
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.BackColor = System.Drawing.SystemColors.Control;
		base.ClientSize = new System.Drawing.Size(313, 598);
		base.Controls.Add(this.chkAlwaysOnTop);
		base.Controls.Add(this.groupBoxTS2);
		base.Controls.Add(this.picRadar);
		base.Controls.Add(this.panelDivControls);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		this.MinimumSize = new System.Drawing.Size(329, 637);
		base.Name = "DiversityForm";
		base.Opacity = 0.0;
		this.Text = "Phasing Control";
		base.Closing += new System.ComponentModel.CancelEventHandler(DiversityForm_Closing);
		base.Load += new System.EventHandler(DiversityForm_Load);
		base.Resize += new System.EventHandler(DiversityForm_Resize);
		((System.ComponentModel.ISupportInitialize)this.picRadar).EndInit();
		this.groupBoxTS2.ResumeLayout(false);
		this.groupBoxTS2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.udAngle0).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udCalib).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udAngle).EndInit();
		this.groupBoxTS1.ResumeLayout(false);
		this.groupBoxTS1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.udAntSpacing).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udR).EndInit();
		this.panelDivControls.ResumeLayout(false);
		this.panelDivControls.PerformLayout();
		this.grpRxSource.ResumeLayout(false);
		this.grpRxSource.PerformLayout();
		this.groupBox_refMerc.ResumeLayout(false);
		this.groupBox_refMerc.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.udGainMulti).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udFineNull).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udR2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udR1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	public string SerializeObjectToString<T>(T obj)
	{
		using MemoryStream memoryStream = new MemoryStream();
		new BinaryFormatter().Serialize(memoryStream, obj);
		return Convert.ToBase64String(memoryStream.ToArray()).Replace("/", "[backslash]");
	}

	public T DeserializeStringToObject<T>(string str)
	{
		str = str.Replace("[backslash]", "/");
		using MemoryStream serializationStream = new MemoryStream(Convert.FromBase64String(str));
		return (T)new BinaryFormatter().Deserialize(serializationStream);
	}

	private void initMemories()
	{
		for (int i = 0; i < _memories.Length; i++)
		{
			_memories[i] = new memorySettings();
		}
	}

	private void picRadar_Paint(object sender, PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		int num = Math.Min(picRadar.ClientSize.Width, picRadar.ClientSize.Height);
		Pen pen = new Pen(lineColor);
		Pen pen2 = new Pen(Brushes.White);
		graphics.CompositingQuality = CompositingQuality.HighQuality;
		graphics.InterpolationMode = InterpolationMode.Bicubic;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		graphics.FillEllipse(new LinearGradientBrush(new Point(num / 2, 0), new Point(num / 2, num - 1), topColor, bottomColor), 0, 0, num - 1, num - 1);
		graphics.DrawEllipse(pen, 0, 0, num - 1, num - 1);
		int num2 = num / 2;
		graphics.DrawEllipse(pen, (num - num2) / 2, (num - num2) / 2, num2, num2);
		graphics.DrawLine(pen, new Point(0, num / 2), new Point(num - 1, num / 2));
		graphics.DrawLine(pen, new Point(num / 2, 0), new Point(num / 2, num - 1));
		Point point;
		for (int i = 0; i < _memories.Length; i++)
		{
			if (_memories[i].enabled)
			{
				point = PolarToXY(_memories[i].r, 0.0 - _memories[i].angle);
				bool num3 = i == _hover_memory_index;
				Brush brush = (num3 ? Brushes.LimeGreen : Brushes.Orange);
				int num4 = (num3 ? 6 : 4);
				int num5 = num4 * 2;
				graphics.FillEllipse(brush, point.X - num4, point.Y - num4, num5, num5);
				graphics.DrawString((i + 1).ToString(), _font, brush, new PointF(point.X + 2, point.Y + 2));
			}
		}
		point = getControlHandlePoint();
		graphics.FillEllipse(Brushes.White, point.X - 4, point.Y - 4, 8, 8);
		graphics.DrawLine(pen2, new Point(num / 2, num / 2), new Point(point.X, point.Y));
	}

	private Point getControlHandlePoint()
	{
		double r = (double)udR.Value;
		double num = (double)udAngle.Value;
		if (chkLockR.Checked && chkLockAngle.Checked)
		{
			return PolarToXY(locked_r, 0.0 - locked_angle);
		}
		if (chkLockR.Checked)
		{
			return PolarToXY(locked_r, 0.0 - num);
		}
		if (chkLockAngle.Checked)
		{
			return PolarToXY(r, 0.0 - locked_angle);
		}
		return PolarToXY(r, 0.0 - num);
	}

	private Point PolarToXY(double r, double angle)
	{
		int num = Math.Min(picRadar.Width, picRadar.Height);
		if (r > 1.0)
		{
			r = 1.0;
		}
		return new Point((int)(r * Math.Cos(angle) * (double)num / 2.0) + num / 2, (int)(r * Math.Sin(angle) * (double)num / 2.0) + num / 2);
	}

	private void picRadar_MouseMove(object sender, MouseEventArgs e)
	{
		if (_initalising)
		{
			return;
		}
		if (!mouse_down && e.Button == MouseButtons.None)
		{
			updateHoverMemory(e.Location);
		}
		int val = picRadar.Width;
		int val2 = picRadar.Height;
		int num = Math.Min(val, val2);
		int num2 = e.X - num / 2;
		int num3 = e.Y - num / 2;
		double num4 = (double)num2 / (double)(num / 2);
		double num5 = (0.0 - (double)num3) / (double)(num / 2);
		double num6 = Math.Min(Math.Sqrt(Math.Pow(num4, 2.0) + Math.Pow(num5, 2.0)), 2.0);
		if (num6 > 1.0)
		{
			num6 = 1.0;
		}
		angle = Math.Atan2(num5, num4);
		if (!mouse_down || (chkLockR.Checked && chkLockAngle.Checked))
		{
			return;
		}
		if (chkLockR.Checked)
		{
			locked_angle = angle;
			udR.Value = (decimal)locked_r;
			udAngle0.Value = (decimal)ConvertAngleToAngle0(angle);
			udAngle.Value = (decimal)angle;
			udFineNull.Value = (decimal)(180.0 * angle / Math.PI);
		}
		else if (chkLockAngle.Checked)
		{
			locked_r = num6;
			udR.Value = (decimal)num6;
			if (radioButtonMerc1.Checked)
			{
				udR2.Value = Math.Round(udR.Value * (decimal)m_dGainMulti, 3);
			}
			else
			{
				udR1.Value = Math.Round(udR.Value * (decimal)m_dGainMulti, 3);
			}
			udAngle0.Value = (decimal)ConvertAngleToAngle0(angle);
			udAngle.Value = (decimal)locked_angle;
			udFineNull.Value = (decimal)(180.0 * locked_angle / Math.PI);
		}
		else
		{
			locked_r = num6;
			if (radioButtonMerc1.Checked)
			{
				locked_angle = angle;
				udR.Value = (decimal)num6;
				udR2.Value = Math.Round(udR.Value * (decimal)m_dGainMulti, 3);
				udAngle.Value = (decimal)angle;
				udFineNull.Value = (decimal)(180.0 * (double)udAngle.Value / Math.PI);
				decimal num7 = (decimal)ConvertAngleToAngle0(angle);
				if (num7 >= 360m)
				{
					num7 = 0m;
				}
				if (num7 <= -1m)
				{
					num7 = 359m;
				}
				udAngle0.Value = num7;
			}
			if (radioButtonMerc2.Checked)
			{
				locked_angle = angle;
				udR.Value = (decimal)num6;
				udR1.Value = Math.Round(udR.Value * (decimal)m_dGainMulti, 3);
				udAngle.Value = (decimal)angle;
				udFineNull.Value = (decimal)(180.0 * (double)udAngle.Value / Math.PI);
				decimal num8 = (decimal)ConvertAngleToAngle0(angle);
				if (num8 >= 360m)
				{
					num8 = 0m;
				}
				if (num8 <= -1m)
				{
					num8 = 359m;
				}
				udAngle0.Value = num8;
			}
		}
		picRadar.Invalidate();
	}

	private void picRadar_MouseDown(object sender, MouseEventArgs e)
	{
		if (_initalising)
		{
			return;
		}
		updateHoverMemory(e.Location);
		_mouse_down_memory_index = -1;
		if (e.Button == MouseButtons.Left && _hover_memory_index >= 0 && _memories[_hover_memory_index].enabled && !Common.ShiftKeyDown && !Common.CtrlKeyDown)
		{
			_mouse_down_memory_index = _hover_memory_index;
			return;
		}
		mouse_down = true;
		if (_hover_memory_index >= 0)
		{
			_hover_memory_index = -1;
			picRadar.Cursor = Cursors.Default;
			picRadar.Invalidate();
		}
		picRadar_MouseMove(sender, e);
	}

	private void picRadar_MouseUp(object sender, MouseEventArgs e)
	{
		if (_initalising)
		{
			return;
		}
		updateHoverMemory(e.Location);
		if (e.Button == MouseButtons.Left && _mouse_down_memory_index >= 0)
		{
			int mouse_down_memory_index = _mouse_down_memory_index;
			_mouse_down_memory_index = -1;
			if (mouse_down_memory_index == _hover_memory_index)
			{
				recallMemory(mouse_down_memory_index);
			}
		}
		else
		{
			mouse_down = false;
		}
	}

	private void udR_ValueChanged(object sender, EventArgs e)
	{
		if (!_initalising)
		{
			UpdateDiversity();
		}
	}

	private void udTheta_ValueChanged(object sender, EventArgs e)
	{
		if (!_initalising)
		{
			UpdateDiversity();
		}
	}

	private void UpdateDirection()
	{
		decimal value = udAngle0.Value;
		if (((value > 337m) & (value <= 360m)) | ((value >= 0m) & (value <= 22m)))
		{
			labelDirection.Text = "N";
		}
		else if ((value > 22m) & (value <= 67m))
		{
			labelDirection.Text = "NE";
		}
		else if ((value > 67m) & (value <= 112m))
		{
			labelDirection.Text = "E";
		}
		else if ((value > 112m) & (value < 157m))
		{
			labelDirection.Text = "SE";
		}
		else if ((value > 157m) & (value <= 202m))
		{
			labelDirection.Text = "S";
		}
		else if ((value > 202m) & (value <= 247m))
		{
			labelDirection.Text = "SW";
		}
		else if ((value > 247m) & (value <= 292m))
		{
			labelDirection.Text = "W";
		}
		else if ((value > 292m) & (value <= 337m))
		{
			labelDirection.Text = "NW";
		}
	}

	private unsafe void UpdateDiversity()
	{
		double num = Math.PI;
		UpdateDirection();
		_ = (double)udR.Value;
		_ = 1.0;
		double num2 = (double)udAngle.Value;
		double num3 = (double)udCalib.Value;
		double num4 = 299790000.0 / (console.VFOAFreq * 1000000.0);
		d_lambda = (double)udAntSpacing.Value / num4;
		label_d.Text = d_lambda.ToString("0.00");
		double[] array = new double[2];
		double[] array2 = new double[2];
		if (radioButtonMerc1.Checked)
		{
			angle_A = cross_fire + fine_null + 2.0 * num * d_lambda * Math.Cos(num2 + num3);
			r_A = (double)udR2.Value;
			double num5 = r_A * Math.Cos(angle_A);
			double num6 = r_A * Math.Sin(angle_A);
			array[0] = 1.0;
			array2[0] = 0.0;
			array[1] = num5;
			array2[1] = num6;
			fixed (double* irotate = &array[0])
			{
				fixed (double* qrotate = &array2[0])
				{
					WDSP.SetEXTDIVRotate(0, 2, irotate, qrotate);
				}
			}
		}
		if (radioButtonMerc2.Checked)
		{
			angle_A = num + fine_null + 2.0 * num * d_lambda * Math.Cos(num2 + num3);
			r_A = (double)udR1.Value;
			double num7 = r_A * Math.Cos(angle_A);
			double num8 = r_A * Math.Sin(angle_A);
			array[1] = 1.0;
			array2[1] = 0.0;
			array[0] = num7;
			array2[0] = num8;
			fixed (double* irotate2 = &array[0])
			{
				fixed (double* qrotate2 = &array2[0])
				{
					WDSP.SetEXTDIVRotate(0, 2, irotate2, qrotate2);
				}
			}
		}
		picRadar.Invalidate();
	}

	private void chkEnable_CheckedChanged(object sender, EventArgs e)
	{
		if (!_initalising)
		{
			if (chkEnable.Checked)
			{
				chkEnable.BackColor = console.ButtonSelectedColor;
			}
			else
			{
				chkEnable.BackColor = SystemColors.Control;
			}
			if (chkEnable.Checked && !console.RX2Enabled)
			{
				console.RX2Enabled = true;
			}
		}
	}

	private void DiversityForm_Closing(object sender, CancelEventArgs e)
	{
		txtMemoryDataHidden.Text = SerializeObjectToString(_memories);
		Common.SaveForm(this, "DiversityForm");
	}

	private void btnShiftUp45_Click(object sender, EventArgs e)
	{
		stepAngle(45.0);
	}

	private void btnShift180_Click(object sender, EventArgs e)
	{
		stepAngle(180.0);
	}

	private void btnShiftDwn45_Click(object sender, EventArgs e)
	{
		stepAngle(-45.0);
	}

	private void radioButtonMerc1_CheckedChanged(object sender, EventArgs e)
	{
		if (_initalising)
		{
			return;
		}
		if (radioButtonMerc1.Checked)
		{
			udR1.Visible = false;
			udR2.Visible = true;
			updateR2 = false;
			udR2_ValueChanged(this, EventArgs.Empty);
			if (chkLockR.Checked)
			{
				udR1.Value = udR.Value;
			}
			UpdateDiversity();
		}
		console.DiversityRXRef = radioButtonMerc1.Checked;
	}

	private void radioButtonMerc2_CheckedChanged(object sender, EventArgs e)
	{
		if (!_initalising && radioButtonMerc2.Checked)
		{
			udR1.Visible = true;
			udR2.Visible = false;
			updateR1 = false;
			udR1_ValueChanged(this, EventArgs.Empty);
			UpdateDiversity();
		}
	}

	private void chkLockAngle_CheckedChanged(object sender, EventArgs e)
	{
		if (!_initalising)
		{
			locked_angle = (double)udAngle.Value;
		}
	}

	private void chkLockR_CheckedChanged(object sender, EventArgs e)
	{
		if (!_initalising)
		{
			locked_r = (double)udR.Value;
		}
	}

	private void groupBox_refMerc_Enter(object sender, EventArgs e)
	{
	}

	private void groupBox_udPhase_Enter(object sender, EventArgs e)
	{
	}

	private void udR2_ValueChanged(object sender, EventArgs e)
	{
		if (_initalising || radioButtonMerc2.Checked)
		{
			return;
		}
		if (chkLockR.Checked)
		{
			r_A = locked_r;
			if (udR2.Value != (decimal)locked_r)
			{
				udR2.Value = (decimal)Math.Round(locked_r * m_dGainMulti, 3);
			}
		}
		udR.Value = Math.Round(udR2.Value / (decimal)m_dGainMulti, 3);
		UpdateDiversity();
		if (updateR2)
		{
			switch (console.RX1Band)
			{
			case Band.B160M:
				console.DiversityR2Gain160m = udR2.Value;
				break;
			case Band.B80M:
				console.DiversityR2Gain80m = udR2.Value;
				break;
			case Band.B60M:
				console.DiversityR2Gain60m = udR2.Value;
				break;
			case Band.B40M:
				console.DiversityR2Gain40m = udR2.Value;
				break;
			case Band.B30M:
				console.DiversityR2Gain30m = udR2.Value;
				break;
			case Band.B20M:
				console.DiversityR2Gain20m = udR2.Value;
				break;
			case Band.B17M:
				console.DiversityR2Gain17m = udR2.Value;
				break;
			case Band.B15M:
				console.DiversityR2Gain15m = udR2.Value;
				break;
			case Band.B12M:
				console.DiversityR2Gain12m = udR2.Value;
				break;
			case Band.B10M:
				console.DiversityR2Gain10m = udR2.Value;
				break;
			case Band.B6M:
				console.DiversityR2Gain6m = udR2.Value;
				break;
			case Band.WWV:
				console.DiversityR2GainWWV = udR2.Value;
				break;
			case Band.GEN:
				console.DiversityR2GainGEN = udR2.Value;
				break;
			default:
				console.DiversityR2GainXVTR = udR2.Value;
				break;
			}
		}
		updateR2 = true;
	}

	private void udR1_ValueChanged(object sender, EventArgs e)
	{
		if (_initalising || radioButtonMerc1.Checked)
		{
			return;
		}
		if (chkLockR.Checked)
		{
			r_A = locked_r;
			if ((decimal)locked_r != udR1.Value)
			{
				udR1.Value = (decimal)Math.Round(locked_r * m_dGainMulti, 3);
			}
		}
		udR.Value = Math.Round(udR1.Value / (decimal)m_dGainMulti, 3);
		UpdateDiversity();
		if (updateR1)
		{
			switch (console.RX1Band)
			{
			case Band.B160M:
				console.DiversityGain160m = udR1.Value;
				break;
			case Band.B80M:
				console.DiversityGain80m = udR1.Value;
				break;
			case Band.B60M:
				console.DiversityGain60m = udR1.Value;
				break;
			case Band.B40M:
				console.DiversityGain40m = udR1.Value;
				break;
			case Band.B30M:
				console.DiversityGain30m = udR1.Value;
				break;
			case Band.B20M:
				console.DiversityGain20m = udR1.Value;
				break;
			case Band.B17M:
				console.DiversityGain17m = udR1.Value;
				break;
			case Band.B15M:
				console.DiversityGain15m = udR1.Value;
				break;
			case Band.B12M:
				console.DiversityGain12m = udR1.Value;
				break;
			case Band.B10M:
				console.DiversityGain10m = udR1.Value;
				break;
			case Band.B6M:
				console.DiversityGain6m = udR1.Value;
				break;
			case Band.WWV:
				console.DiversityGainWWV = udR1.Value;
				break;
			case Band.GEN:
				console.DiversityGainGEN = udR1.Value;
				break;
			default:
				console.DiversityGainXVTR = udR1.Value;
				break;
			}
		}
		updateR1 = true;
	}

	private void udCalib_ValueChanged(object sender, EventArgs e)
	{
		if (!_initalising)
		{
			UpdateDiversity();
		}
	}

	private void udAngle0_ValueChanged(object sender, EventArgs e)
	{
		if (!_initalising)
		{
			if (udAngle0.Value >= 360m)
			{
				udAngle0.Value = 0m;
			}
			if (udAngle0.Value <= -1m)
			{
				udAngle0.Value = 359m;
			}
			udAngle.Value = (decimal)ConvertAngle0ToAngle((double)udAngle0.Value);
			angle = (double)udAngle.Value;
			UpdateDiversity();
		}
	}

	private double ConvertAngleToAngle0(double e)
	{
		double num = e * 180.0 / Math.PI;
		double result = 0.0;
		if (((num >= 0.0) & (num <= 90.0)) | ((num <= -90.0) & (num >= -181.0)))
		{
			result = 90.0 - num;
		}
		if ((num < 0.0) & (num >= -90.0))
		{
			result = 90.0 - num;
		}
		if ((num > 90.0) & (num <= 181.0))
		{
			result = 450.0 - num;
		}
		return result;
	}

	private double ConvertAngle0ToAngle(double e)
	{
		double num = 0.0;
		if (e == 360.0)
		{
			num = 0.0;
		}
		if (e <= 270.0)
		{
			num = 90.0 - e;
		}
		if ((e > 270.0) & (e < 360.0))
		{
			num = 450.0 - e;
		}
		return num * Math.PI / 180.0;
	}

	private void panelDivControls_Enter(object sender, EventArgs e)
	{
	}

	private void udAntSpacing_ValueChanged_1(object sender, EventArgs e)
	{
		if (!_initalising)
		{
			udAngle.Value = (decimal)ConvertAngle0ToAngle((double)udAngle0.Value);
			angle = (double)udAngle.Value;
			UpdateDiversity();
		}
	}

	private double CalcVrms(double a, double b)
	{
		double num = Math.PI;
		steering_angle = b;
		double num2 = 1000000.0 * console.VFOAFreq;
		double num3 = 1.0 / (20.0 * num2);
		double num4 = ((!radioButtonMerc1.Checked) ? (cross_fire + Math.Cos(a + steering_angle)) : (cross_fire + Math.Cos(a + steering_angle)));
		double num5 = 0.0;
		double num6 = 2.0 * num * num2 * num3;
		for (int i = 0; i < 20; i++)
		{
			double num7 = Math.Sin((double)i * num6);
			double num8 = Math.Sin((double)i * num6 + num4 - 2.0 * num * d_lambda);
			double num9 = num7 + num8;
			num5 += num9 * num9;
		}
		return num5 / 20.0 * 5.5;
	}

	private void chkCrossFire_CheckedChanged(object sender, EventArgs e)
	{
		if (!_initalising)
		{
			if (chkCrossFire.Checked)
			{
				cross_fire = Math.PI;
			}
			else
			{
				cross_fire = 0.0;
			}
			udAngle.Value = (decimal)ConvertAngle0ToAngle((double)udAngle0.Value);
			angle = (double)udAngle.Value;
			UpdateDiversity();
		}
	}

	private void udFineNull_ValueChanged(object sender, EventArgs e)
	{
		if (_initalising)
		{
			return;
		}
		if (chkLockAngle.Checked)
		{
			angle_A = locked_angle;
			double num = 180.0 * locked_angle / Math.PI;
			if ((decimal)num != udFineNull.Value)
			{
				udFineNull.Value = (decimal)num;
			}
		}
		else
		{
			udAngle.Value = (decimal)(Math.PI * (double)udFineNull.Value / 180.0);
			fine_null = Math.PI * (double)udFineNull.Value / 180.0;
		}
		UpdateDiversity();
		switch (console.RX1Band)
		{
		case Band.B160M:
			console.DiversityPhase160m = udFineNull.Value;
			break;
		case Band.B80M:
			console.DiversityPhase80m = udFineNull.Value;
			break;
		case Band.B60M:
			console.DiversityPhase60m = udFineNull.Value;
			break;
		case Band.B40M:
			console.DiversityPhase40m = udFineNull.Value;
			break;
		case Band.B30M:
			console.DiversityPhase30m = udFineNull.Value;
			break;
		case Band.B20M:
			console.DiversityPhase20m = udFineNull.Value;
			break;
		case Band.B17M:
			console.DiversityPhase17m = udFineNull.Value;
			break;
		case Band.B15M:
			console.DiversityPhase15m = udFineNull.Value;
			break;
		case Band.B12M:
			console.DiversityPhase12m = udFineNull.Value;
			break;
		case Band.B10M:
			console.DiversityPhase10m = udFineNull.Value;
			break;
		case Band.B6M:
			console.DiversityPhase6m = udFineNull.Value;
			break;
		case Band.WWV:
			console.DiversityPhaseWWV = udFineNull.Value;
			break;
		case Band.GEN:
			console.DiversityPhaseGEN = udFineNull.Value;
			break;
		default:
			console.DiversityPhaseXVTR = udFineNull.Value;
			break;
		}
	}

	private void radRxSource1_CheckedChanged(object sender, EventArgs e)
	{
		if (!_initalising && radRxSource1.Checked)
		{
			_extDivOutput = 0;
			WDSP.SetEXTDIVOutput(0, 0);
			if (!console.IsSetupFormNull)
			{
				console.SetupForm.UpdateDDCTab();
			}
		}
	}

	private void radRxSource2_CheckedChanged(object sender, EventArgs e)
	{
		if (!_initalising && radRxSource2.Checked)
		{
			_extDivOutput = 1;
			WDSP.SetEXTDIVOutput(0, 1);
			if (!console.IsSetupFormNull)
			{
				console.SetupForm.UpdateDDCTab();
			}
		}
	}

	private void radRxSourceRx1Rx2_CheckedChanged(object sender, EventArgs e)
	{
		if (!_initalising && radRxSourceRx1Rx2.Checked)
		{
			_extDivOutput = 2;
			WDSP.SetEXTDIVOutput(0, 2);
			if (!console.IsSetupFormNull)
			{
				console.SetupForm.UpdateDDCTab();
			}
		}
	}

	private void chkEnableDiversity_CheckedChanged(object sender, EventArgs e)
	{
		if (!_initalising)
		{
			console.Diversity2 = chkEnableDiversity.Checked;
			if (chkEnableDiversity.Checked)
			{
				chkEnableDiversity.BackColor = Color.LimeGreen;
				chkEnableDiversity.Text = "Enabled";
			}
			else
			{
				chkEnableDiversity.BackColor = Color.Red;
				chkEnableDiversity.Text = "Disabled";
			}
		}
	}

	public void FormEncoderEvent()
	{
		if (!base.Visible)
		{
			FormAutoShown = true;
			Show();
		}
		if (FormAutoShown)
		{
			AutoHideTimer.Enabled = false;
			AutoHideTimer.AutoReset = false;
			AutoHideTimer.Interval = 10000.0;
			AutoHideTimer.Enabled = true;
		}
	}

	private void Callback(object source, ElapsedEventArgs e)
	{
		FormAutoShown = false;
		AutoHideTimer.Enabled = false;
		Hide();
	}

	private void DiversityForm_Load(object sender, EventArgs e)
	{
		if (!console.IsSetupFormNull && console.SetupForm.AndromedaDiversityFormLandscape)
		{
			picRadar.Anchor = AnchorStyles.None;
			picRadar.Size = new Size(226, 226);
			base.Size = new Size(750, 280);
			picRadar.Location = new Point(470, 1);
		}
	}

	private void DiversityForm_Resize(object sender, EventArgs e)
	{
		picRadar.Invalidate();
	}

	private void udGainMulti_ValueChanged(object sender, EventArgs e)
	{
		if (_initalising)
		{
			return;
		}
		m_dGainMulti = (double)Math.Round(udGainMulti.Value, 2);
		udR1.Maximum = (decimal)m_dGainMulti;
		udR2.Maximum = (decimal)m_dGainMulti;
		if (chkLockR.Checked)
		{
			if (udGainMulti.Value != (decimal)m_dGainMulti)
			{
				udGainMulti.Value = (decimal)m_dGainMulti;
			}
			return;
		}
		double num = (double)udR1.Value;
		double num2 = (double)udR2.Value;
		num /= m_dGainMulti;
		num2 /= m_dGainMulti;
		if (radioButtonMerc1.Checked)
		{
			_initalising = true;
			udR1.Value = (decimal)Math.Round(num * m_dGainMulti, 3);
			udR2.Value = (decimal)Math.Round(num2 * m_dGainMulti, 3);
			_initalising = false;
			udR2_ValueChanged(this, EventArgs.Empty);
		}
		else if (radioButtonMerc2.Checked)
		{
			_initalising = true;
			udR2.Value = (decimal)Math.Round(num2 * m_dGainMulti, 3);
			udR1.Value = (decimal)Math.Round(num * m_dGainMulti, 3);
			_initalising = false;
			udR1_ValueChanged(this, EventArgs.Empty);
		}
	}

	private void chkAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
	{
		base.TopMost = chkAlwaysOnTop.Checked;
	}

	private void chkNoAttLink_CheckedChanged(object sender, EventArgs e)
	{
		console.DiversityAttLink = !chkNoAttLink.Checked;
	}

	private void chkVFOSync_CheckedChanged(object sender, EventArgs e)
	{
		console.VFOSync = chkVFOSync.Checked;
		chkVFOSync.BackColor = (chkVFOSync.Checked ? Color.LimeGreen : Color.Empty);
	}

	private void btnMemory_Click(object sender, EventArgs e)
	{
		int num = int.Parse(((Control)sender).Name.Substring(4)) - 1;
		if (Common.ShiftKeyDown)
		{
			_memories[num].enabled = true;
			_memories[num].gainMulti = (double)udGainMulti.Value;
			_memories[num].receiverSource = ((!radRxSourceRx1Rx2.Checked) ? (radRxSource1.Checked ? 1 : 2) : 0);
			_memories[num].rx1RefSource = radioButtonMerc1.Checked;
			_memories[num].rx1Gain = (double)udR1.Value;
			_memories[num].rx2Gain = (double)udR2.Value;
			_memories[num].phase = (double)udFineNull.Value;
			_memories[num].lockPhase = chkLockAngle.Checked;
			_memories[num].lockGain = chkLockR.Checked;
			_memories[num].angle = (double)udAngle.Value;
			_memories[num].r = (double)udR.Value;
			picRadar.Invalidate();
		}
		else if (Common.CtrlKeyDown)
		{
			_memories[num].enabled = false;
			picRadar.Invalidate();
		}
		else
		{
			if (!_memories[num].enabled)
			{
				return;
			}
			memorySettings memorySettings2 = _memories[num];
			if (memorySettings2 != null)
			{
				chkLockAngle.Checked = false;
				chkLockR.Checked = false;
				_initalising = true;
				udGainMulti.Value = (decimal)memorySettings2.gainMulti;
				_initalising = false;
				udGainMulti_ValueChanged(this, EventArgs.Empty);
				switch (_memories[num].receiverSource)
				{
				case 0:
					radRxSourceRx1Rx2.Checked = true;
					break;
				case 1:
					radRxSource1.Checked = true;
					break;
				case 2:
					radRxSource2.Checked = true;
					break;
				}
				radioButtonMerc1.Checked = memorySettings2.rx1RefSource;
				radioButtonMerc2.Checked = !memorySettings2.rx1RefSource;
				udR1.Value = (decimal)memorySettings2.rx1Gain;
				udR2.Value = (decimal)memorySettings2.rx2Gain;
				udFineNull.Value = (decimal)memorySettings2.phase;
				UpdateDiversity();
				chkLockAngle.Checked = memorySettings2.lockPhase;
				chkLockR.Checked = memorySettings2.lockGain;
			}
		}
	}

	private double NormalizeAngle(double angle)
	{
		double num = Math.PI;
		angle %= 2.0 * num;
		if (angle > num)
		{
			angle -= 2.0 * num;
		}
		else if (angle <= 0.0 - num)
		{
			angle += 2.0 * num;
		}
		return angle;
	}

	private void stepAngle(double degrees)
	{
		double num = Math.PI;
		double num2 = degrees * (num / 180.0);
		if (!chkLockAngle.Checked)
		{
			double num3 = (double)udAngle.Value;
			num3 += num2;
			num3 = NormalizeAngle(num3);
			udAngle.Value = (decimal)num3;
			udAngle0.Value = (decimal)ConvertAngleToAngle0(num3);
			udFineNull.Value = (decimal)(180.0 * num3 / Math.PI);
			UpdateDiversity();
		}
	}

	private void btnShiftUp10_Click(object sender, EventArgs e)
	{
		stepAngle(10.0);
	}

	private void btnShift90_Click(object sender, EventArgs e)
	{
		stepAngle(90.0);
	}

	private void btnShiftDown10_Click(object sender, EventArgs e)
	{
		stepAngle(-10.0);
	}

	private void udZoom_ValueChanged(object sender, EventArgs e)
	{
	}

	private void setNewNaming(bool new_naming)
	{
		if (new_naming)
		{
			grpRxSource.Text = "RX1 Source";
			radRxSourceRx1Rx2.Text = "Sync1+2";
			radRxSource1.Text = "Sync1";
			radRxSource2.Text = "Sync2";
			radioButtonMerc1.Text = "Sync1";
			radioButtonMerc2.Text = "Sync2";
		}
		else
		{
			grpRxSource.Text = "Receiver Source";
			radRxSourceRx1Rx2.Text = "Rx1+Rx2";
			radRxSource1.Text = "Rx1";
			radRxSource2.Text = "Rx2";
			radioButtonMerc1.Text = "Rx1";
			radioButtonMerc2.Text = "Rx2";
		}
	}

	private int getMemoryIndexAtPoint(Point pt, int hit_radius_px)
	{
		for (int i = 0; i < _memories.Length; i++)
		{
			if (_memories[i].enabled)
			{
				Point point = PolarToXY(_memories[i].r, 0.0 - _memories[i].angle);
				int num = pt.X - point.X;
				int num2 = pt.Y - point.Y;
				if (num * num + num2 * num2 <= hit_radius_px * hit_radius_px)
				{
					return i;
				}
			}
		}
		return -1;
	}

	private void updateHoverMemory(Point pt)
	{
		int hit_radius_px = 8;
		int memoryIndexAtPoint = getMemoryIndexAtPoint(pt, hit_radius_px);
		if (memoryIndexAtPoint != _hover_memory_index)
		{
			_hover_memory_index = memoryIndexAtPoint;
			picRadar.Cursor = ((_hover_memory_index >= 0) ? Cursors.Hand : Cursors.Default);
			picRadar.Invalidate();
		}
	}

	private void recallMemory(int index)
	{
		if (index < 0 || index >= _memories.Length || !_memories[index].enabled)
		{
			return;
		}
		memorySettings memorySettings2 = _memories[index];
		if (memorySettings2 != null)
		{
			chkLockAngle.Checked = false;
			chkLockR.Checked = false;
			_initalising = true;
			udGainMulti.Value = (decimal)memorySettings2.gainMulti;
			_initalising = false;
			udGainMulti_ValueChanged(this, EventArgs.Empty);
			switch (memorySettings2.receiverSource)
			{
			case 0:
				radRxSourceRx1Rx2.Checked = true;
				break;
			case 1:
				radRxSource1.Checked = true;
				break;
			case 2:
				radRxSource2.Checked = true;
				break;
			}
			radioButtonMerc1.Checked = memorySettings2.rx1RefSource;
			radioButtonMerc2.Checked = !memorySettings2.rx1RefSource;
			udR1.Value = (decimal)memorySettings2.rx1Gain;
			udR2.Value = (decimal)memorySettings2.rx2Gain;
			udFineNull.Value = (decimal)memorySettings2.phase;
			UpdateDiversity();
			chkLockAngle.Checked = memorySettings2.lockPhase;
			chkLockR.Checked = memorySettings2.lockGain;
		}
	}

	private void picRadar_MouseLeave(object sender, EventArgs e)
	{
		if (_hover_memory_index >= 0)
		{
			_hover_memory_index = -1;
			picRadar.Cursor = Cursors.Default;
			picRadar.Invalidate();
		}
	}
}
