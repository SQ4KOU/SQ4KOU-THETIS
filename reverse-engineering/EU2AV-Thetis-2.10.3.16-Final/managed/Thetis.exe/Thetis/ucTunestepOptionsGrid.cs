using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Thetis;

public class ucTunestepOptionsGrid : UserControl
{
	private List<CheckBox> _check_boxes;

	private bool _init;

	private IContainer components;

	private PanelTS pnlButtonBox_tunestep_toggles;

	private CheckBoxTS chkButtonBox_tunestep_25;

	private CheckBoxTS chkButtonBox_tunestep_24;

	private CheckBoxTS chkButtonBox_tunestep_23;

	private CheckBoxTS chkButtonBox_tunestep_22;

	private CheckBoxTS chkButtonBox_tunestep_21;

	private CheckBoxTS chkButtonBox_tunestep_20;

	private CheckBoxTS chkButtonBox_tunestep_19;

	private CheckBoxTS chkButtonBox_tunestep_18;

	private CheckBoxTS chkButtonBox_tunestep_16;

	private CheckBoxTS chkButtonBox_tunestep_15;

	private CheckBoxTS chkButtonBox_tunestep_14;

	private CheckBoxTS chkButtonBox_tunestep_13;

	private CheckBoxTS chkButtonBox_tunestep_12;

	private CheckBoxTS chkButtonBox_tunestep_11;

	private CheckBoxTS chkButtonBox_tunestep_10;

	private CheckBoxTS chkButtonBox_tunestep_9;

	private CheckBoxTS chkButtonBox_tunestep_7;

	private CheckBoxTS chkButtonBox_tunestep_6;

	private CheckBoxTS chkButtonBox_tunestep_5;

	private CheckBoxTS chkButtonBox_tunestep_4;

	private CheckBoxTS chkButtonBox_tunestep_3;

	private CheckBoxTS chkButtonBox_tunestep_2;

	private CheckBoxTS chkButtonBox_tunestep_1;

	private CheckBoxTS chkButtonBox_tunestep_0;

	private CheckBoxTS chkButtonBox_tunestep_26;

	private CheckBoxTS chkButtonBox_tunestep_17;

	private CheckBoxTS chkButtonBox_tunestep_8;

	public int Bitfield
	{
		get
		{
			int num = 0;
			for (int i = 0; i < _check_boxes.Count; i++)
			{
				if (_check_boxes[i].Checked)
				{
					num |= 1 << i;
				}
			}
			return num;
		}
		set
		{
			for (int i = 0; i < _check_boxes.Count; i++)
			{
				CheckBox checkBox = _check_boxes[i];
				bool flag = (value & (1 << i)) != 0;
				checkBox.Checked = flag;
			}
		}
	}

	public event EventHandler CheckboxChanged;

	public ucTunestepOptionsGrid()
	{
		_init = false;
		InitializeComponent();
		initialize_checkboxes();
		hook_up_checkbox_events();
	}

	private void initialize_checkboxes()
	{
		_check_boxes = (from c in pnlButtonBox_tunestep_toggles.Controls.OfType<CheckBox>()
			orderby int.Parse(c.Name.Split('_').Last())
			select c).ToList();
	}

	public void Init(List<TuneStep> tune_steps)
	{
		if (_init)
		{
			return;
		}
		int count = tune_steps.Count;
		for (int i = 0; i < _check_boxes.Count; i++)
		{
			CheckBox checkBox = _check_boxes[i];
			if (i < count)
			{
				checkBox.Text = tune_steps[i].Name.Replace("Hz", "");
				checkBox.Visible = true;
			}
			else
			{
				checkBox.Visible = false;
			}
		}
		_init = true;
	}

	private void hook_up_checkbox_events()
	{
		foreach (CheckBox check_box in _check_boxes)
		{
			check_box.CheckedChanged += checkbox_checked_changed;
		}
	}

	private void checkbox_checked_changed(object sender, EventArgs e)
	{
		CheckboxChanged?.Invoke(this, EventArgs.Empty);
	}

	public int GetCheckedCount()
	{
		int num = 0;
		foreach (CheckBoxTS check_box in _check_boxes)
		{
			if (check_box.Checked)
			{
				num++;
			}
		}
		return num;
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
		this.pnlButtonBox_tunestep_toggles = new System.Windows.Forms.PanelTS();
		this.chkButtonBox_tunestep_26 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_17 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_8 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_25 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_24 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_23 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_22 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_21 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_20 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_19 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_18 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_16 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_15 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_14 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_13 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_12 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_11 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_10 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_9 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_7 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_6 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_5 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_4 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_3 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_2 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_1 = new System.Windows.Forms.CheckBoxTS();
		this.chkButtonBox_tunestep_0 = new System.Windows.Forms.CheckBoxTS();
		this.pnlButtonBox_tunestep_toggles.SuspendLayout();
		base.SuspendLayout();
		this.pnlButtonBox_tunestep_toggles.AutoScrollMargin = new System.Drawing.Size(0, 0);
		this.pnlButtonBox_tunestep_toggles.AutoScrollMinSize = new System.Drawing.Size(0, 0);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_26);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_17);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_8);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_25);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_24);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_23);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_22);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_21);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_20);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_19);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_18);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_16);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_15);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_14);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_13);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_12);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_11);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_10);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_9);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_7);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_6);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_5);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_4);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_3);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_2);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_1);
		this.pnlButtonBox_tunestep_toggles.Controls.Add(this.chkButtonBox_tunestep_0);
		this.pnlButtonBox_tunestep_toggles.Location = new System.Drawing.Point(0, 0);
		this.pnlButtonBox_tunestep_toggles.Name = "pnlButtonBox_tunestep_toggles";
		this.pnlButtonBox_tunestep_toggles.Size = new System.Drawing.Size(157, 182);
		this.pnlButtonBox_tunestep_toggles.TabIndex = 111;
		this.chkButtonBox_tunestep_26.AutoSize = true;
		this.chkButtonBox_tunestep_26.Image = null;
		this.chkButtonBox_tunestep_26.Location = new System.Drawing.Point(109, 163);
		this.chkButtonBox_tunestep_26.Name = "chkButtonBox_tunestep_26";
		this.chkButtonBox_tunestep_26.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_26.TabIndex = 27;
		this.chkButtonBox_tunestep_26.Text = "Rx 1";
		this.chkButtonBox_tunestep_26.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_17.AutoSize = true;
		this.chkButtonBox_tunestep_17.Image = null;
		this.chkButtonBox_tunestep_17.Location = new System.Drawing.Point(56, 163);
		this.chkButtonBox_tunestep_17.Name = "chkButtonBox_tunestep_17";
		this.chkButtonBox_tunestep_17.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_17.TabIndex = 26;
		this.chkButtonBox_tunestep_17.Text = "Rx 1";
		this.chkButtonBox_tunestep_17.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_8.AutoSize = true;
		this.chkButtonBox_tunestep_8.Image = null;
		this.chkButtonBox_tunestep_8.Location = new System.Drawing.Point(3, 163);
		this.chkButtonBox_tunestep_8.Name = "chkButtonBox_tunestep_8";
		this.chkButtonBox_tunestep_8.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_8.TabIndex = 25;
		this.chkButtonBox_tunestep_8.Text = "Rx 1";
		this.chkButtonBox_tunestep_8.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_25.AutoSize = true;
		this.chkButtonBox_tunestep_25.Image = null;
		this.chkButtonBox_tunestep_25.Location = new System.Drawing.Point(109, 143);
		this.chkButtonBox_tunestep_25.Name = "chkButtonBox_tunestep_25";
		this.chkButtonBox_tunestep_25.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_25.TabIndex = 24;
		this.chkButtonBox_tunestep_25.Text = "Rx 1";
		this.chkButtonBox_tunestep_25.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_24.AutoSize = true;
		this.chkButtonBox_tunestep_24.Image = null;
		this.chkButtonBox_tunestep_24.Location = new System.Drawing.Point(109, 123);
		this.chkButtonBox_tunestep_24.Name = "chkButtonBox_tunestep_24";
		this.chkButtonBox_tunestep_24.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_24.TabIndex = 23;
		this.chkButtonBox_tunestep_24.Text = "Rx 1";
		this.chkButtonBox_tunestep_24.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_23.AutoSize = true;
		this.chkButtonBox_tunestep_23.Image = null;
		this.chkButtonBox_tunestep_23.Location = new System.Drawing.Point(109, 103);
		this.chkButtonBox_tunestep_23.Name = "chkButtonBox_tunestep_23";
		this.chkButtonBox_tunestep_23.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_23.TabIndex = 22;
		this.chkButtonBox_tunestep_23.Text = "Rx 1";
		this.chkButtonBox_tunestep_23.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_22.AutoSize = true;
		this.chkButtonBox_tunestep_22.Image = null;
		this.chkButtonBox_tunestep_22.Location = new System.Drawing.Point(109, 83);
		this.chkButtonBox_tunestep_22.Name = "chkButtonBox_tunestep_22";
		this.chkButtonBox_tunestep_22.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_22.TabIndex = 21;
		this.chkButtonBox_tunestep_22.Text = "Rx 1";
		this.chkButtonBox_tunestep_22.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_21.AutoSize = true;
		this.chkButtonBox_tunestep_21.Image = null;
		this.chkButtonBox_tunestep_21.Location = new System.Drawing.Point(109, 63);
		this.chkButtonBox_tunestep_21.Name = "chkButtonBox_tunestep_21";
		this.chkButtonBox_tunestep_21.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_21.TabIndex = 20;
		this.chkButtonBox_tunestep_21.Text = "Rx 1";
		this.chkButtonBox_tunestep_21.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_20.AutoSize = true;
		this.chkButtonBox_tunestep_20.Image = null;
		this.chkButtonBox_tunestep_20.Location = new System.Drawing.Point(109, 43);
		this.chkButtonBox_tunestep_20.Name = "chkButtonBox_tunestep_20";
		this.chkButtonBox_tunestep_20.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_20.TabIndex = 19;
		this.chkButtonBox_tunestep_20.Text = "Rx 1";
		this.chkButtonBox_tunestep_20.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_19.AutoSize = true;
		this.chkButtonBox_tunestep_19.Image = null;
		this.chkButtonBox_tunestep_19.Location = new System.Drawing.Point(109, 23);
		this.chkButtonBox_tunestep_19.Name = "chkButtonBox_tunestep_19";
		this.chkButtonBox_tunestep_19.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_19.TabIndex = 18;
		this.chkButtonBox_tunestep_19.Text = "Rx 1";
		this.chkButtonBox_tunestep_19.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_18.AutoSize = true;
		this.chkButtonBox_tunestep_18.Image = null;
		this.chkButtonBox_tunestep_18.Location = new System.Drawing.Point(109, 3);
		this.chkButtonBox_tunestep_18.Name = "chkButtonBox_tunestep_18";
		this.chkButtonBox_tunestep_18.Size = new System.Drawing.Size(53, 17);
		this.chkButtonBox_tunestep_18.TabIndex = 17;
		this.chkButtonBox_tunestep_18.Text = "12.5k";
		this.chkButtonBox_tunestep_18.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_16.AutoSize = true;
		this.chkButtonBox_tunestep_16.Image = null;
		this.chkButtonBox_tunestep_16.Location = new System.Drawing.Point(56, 143);
		this.chkButtonBox_tunestep_16.Name = "chkButtonBox_tunestep_16";
		this.chkButtonBox_tunestep_16.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_16.TabIndex = 16;
		this.chkButtonBox_tunestep_16.Text = "Rx 1";
		this.chkButtonBox_tunestep_16.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_15.AutoSize = true;
		this.chkButtonBox_tunestep_15.Image = null;
		this.chkButtonBox_tunestep_15.Location = new System.Drawing.Point(56, 123);
		this.chkButtonBox_tunestep_15.Name = "chkButtonBox_tunestep_15";
		this.chkButtonBox_tunestep_15.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_15.TabIndex = 15;
		this.chkButtonBox_tunestep_15.Text = "Rx 1";
		this.chkButtonBox_tunestep_15.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_14.AutoSize = true;
		this.chkButtonBox_tunestep_14.Image = null;
		this.chkButtonBox_tunestep_14.Location = new System.Drawing.Point(56, 103);
		this.chkButtonBox_tunestep_14.Name = "chkButtonBox_tunestep_14";
		this.chkButtonBox_tunestep_14.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_14.TabIndex = 14;
		this.chkButtonBox_tunestep_14.Text = "Rx 1";
		this.chkButtonBox_tunestep_14.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_13.AutoSize = true;
		this.chkButtonBox_tunestep_13.Image = null;
		this.chkButtonBox_tunestep_13.Location = new System.Drawing.Point(56, 83);
		this.chkButtonBox_tunestep_13.Name = "chkButtonBox_tunestep_13";
		this.chkButtonBox_tunestep_13.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_13.TabIndex = 13;
		this.chkButtonBox_tunestep_13.Text = "Rx 1";
		this.chkButtonBox_tunestep_13.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_12.AutoSize = true;
		this.chkButtonBox_tunestep_12.Image = null;
		this.chkButtonBox_tunestep_12.Location = new System.Drawing.Point(56, 63);
		this.chkButtonBox_tunestep_12.Name = "chkButtonBox_tunestep_12";
		this.chkButtonBox_tunestep_12.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_12.TabIndex = 12;
		this.chkButtonBox_tunestep_12.Text = "Rx 1";
		this.chkButtonBox_tunestep_12.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_11.AutoSize = true;
		this.chkButtonBox_tunestep_11.Image = null;
		this.chkButtonBox_tunestep_11.Location = new System.Drawing.Point(56, 43);
		this.chkButtonBox_tunestep_11.Name = "chkButtonBox_tunestep_11";
		this.chkButtonBox_tunestep_11.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_11.TabIndex = 11;
		this.chkButtonBox_tunestep_11.Text = "Rx 1";
		this.chkButtonBox_tunestep_11.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_10.AutoSize = true;
		this.chkButtonBox_tunestep_10.Image = null;
		this.chkButtonBox_tunestep_10.Location = new System.Drawing.Point(56, 23);
		this.chkButtonBox_tunestep_10.Name = "chkButtonBox_tunestep_10";
		this.chkButtonBox_tunestep_10.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_10.TabIndex = 10;
		this.chkButtonBox_tunestep_10.Text = "Rx 1";
		this.chkButtonBox_tunestep_10.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_9.AutoSize = true;
		this.chkButtonBox_tunestep_9.Image = null;
		this.chkButtonBox_tunestep_9.Location = new System.Drawing.Point(56, 3);
		this.chkButtonBox_tunestep_9.Name = "chkButtonBox_tunestep_9";
		this.chkButtonBox_tunestep_9.Size = new System.Drawing.Size(53, 17);
		this.chkButtonBox_tunestep_9.TabIndex = 9;
		this.chkButtonBox_tunestep_9.Text = "12.5k";
		this.chkButtonBox_tunestep_9.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_7.AutoSize = true;
		this.chkButtonBox_tunestep_7.Image = null;
		this.chkButtonBox_tunestep_7.Location = new System.Drawing.Point(3, 143);
		this.chkButtonBox_tunestep_7.Name = "chkButtonBox_tunestep_7";
		this.chkButtonBox_tunestep_7.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_7.TabIndex = 8;
		this.chkButtonBox_tunestep_7.Text = "Rx 1";
		this.chkButtonBox_tunestep_7.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_6.AutoSize = true;
		this.chkButtonBox_tunestep_6.Image = null;
		this.chkButtonBox_tunestep_6.Location = new System.Drawing.Point(3, 123);
		this.chkButtonBox_tunestep_6.Name = "chkButtonBox_tunestep_6";
		this.chkButtonBox_tunestep_6.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_6.TabIndex = 7;
		this.chkButtonBox_tunestep_6.Text = "Rx 1";
		this.chkButtonBox_tunestep_6.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_5.AutoSize = true;
		this.chkButtonBox_tunestep_5.Image = null;
		this.chkButtonBox_tunestep_5.Location = new System.Drawing.Point(3, 103);
		this.chkButtonBox_tunestep_5.Name = "chkButtonBox_tunestep_5";
		this.chkButtonBox_tunestep_5.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_5.TabIndex = 6;
		this.chkButtonBox_tunestep_5.Text = "Rx 1";
		this.chkButtonBox_tunestep_5.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_4.AutoSize = true;
		this.chkButtonBox_tunestep_4.Image = null;
		this.chkButtonBox_tunestep_4.Location = new System.Drawing.Point(3, 83);
		this.chkButtonBox_tunestep_4.Name = "chkButtonBox_tunestep_4";
		this.chkButtonBox_tunestep_4.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_4.TabIndex = 5;
		this.chkButtonBox_tunestep_4.Text = "Rx 1";
		this.chkButtonBox_tunestep_4.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_3.AutoSize = true;
		this.chkButtonBox_tunestep_3.Image = null;
		this.chkButtonBox_tunestep_3.Location = new System.Drawing.Point(3, 63);
		this.chkButtonBox_tunestep_3.Name = "chkButtonBox_tunestep_3";
		this.chkButtonBox_tunestep_3.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_3.TabIndex = 4;
		this.chkButtonBox_tunestep_3.Text = "Rx 1";
		this.chkButtonBox_tunestep_3.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_2.AutoSize = true;
		this.chkButtonBox_tunestep_2.Image = null;
		this.chkButtonBox_tunestep_2.Location = new System.Drawing.Point(3, 43);
		this.chkButtonBox_tunestep_2.Name = "chkButtonBox_tunestep_2";
		this.chkButtonBox_tunestep_2.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_2.TabIndex = 3;
		this.chkButtonBox_tunestep_2.Text = "Rx 1";
		this.chkButtonBox_tunestep_2.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_1.AutoSize = true;
		this.chkButtonBox_tunestep_1.Image = null;
		this.chkButtonBox_tunestep_1.Location = new System.Drawing.Point(3, 23);
		this.chkButtonBox_tunestep_1.Name = "chkButtonBox_tunestep_1";
		this.chkButtonBox_tunestep_1.Size = new System.Drawing.Size(48, 17);
		this.chkButtonBox_tunestep_1.TabIndex = 2;
		this.chkButtonBox_tunestep_1.Text = "Rx 1";
		this.chkButtonBox_tunestep_1.UseVisualStyleBackColor = true;
		this.chkButtonBox_tunestep_0.AutoSize = true;
		this.chkButtonBox_tunestep_0.Image = null;
		this.chkButtonBox_tunestep_0.Location = new System.Drawing.Point(3, 3);
		this.chkButtonBox_tunestep_0.Name = "chkButtonBox_tunestep_0";
		this.chkButtonBox_tunestep_0.Size = new System.Drawing.Size(53, 17);
		this.chkButtonBox_tunestep_0.TabIndex = 1;
		this.chkButtonBox_tunestep_0.Text = "12.5k";
		this.chkButtonBox_tunestep_0.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.pnlButtonBox_tunestep_toggles);
		base.Name = "ucTunestepOptionsGrid";
		base.Size = new System.Drawing.Size(157, 182);
		this.pnlButtonBox_tunestep_toggles.ResumeLayout(false);
		this.pnlButtonBox_tunestep_toggles.PerformLayout();
		base.ResumeLayout(false);
	}
}
