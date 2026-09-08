using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class frmInfoBarPopup : Form
{
	public class PopupActionSelected : EventArgs
	{
		public ucInfoBar.ActionTypes Action;

		public bool ButtonState;

		public MouseButtons Button;
	}

	private bool _bHasButtons;

	private Dictionary<ucInfoBar.ActionTypes, ucInfoBar.ActionState> _states;

	private IContainer components;

	private CheckBoxTS chkButton1;

	private CheckBoxTS chkButton2;

	private CheckBoxTS chkButton3;

	private CheckBoxTS chkButton4;

	private CheckBoxTS chkButton5;

	private CheckBoxTS chkButton6;

	private CheckBoxTS chkButton7;

	private CheckBoxTS chkButton8;

	private ToolTip toolTip1;

	public bool HasButtons => _bHasButtons;

	public event EventHandler<PopupActionSelected> ActionClicked;

	public frmInfoBarPopup()
	{
		InitializeComponent();
	}

	public void SetStates(Dictionary<ucInfoBar.ActionTypes, ucInfoBar.ActionState> states, ucInfoBar.ActionState b1, ucInfoBar.ActionState b2)
	{
		if (states == null)
		{
			return;
		}
		int num = 0;
		_states = states;
		Dictionary<string, CheckBoxTS> checkboxesDictionary = getCheckboxesDictionary();
		foreach (CheckBoxTS value2 in checkboxesDictionary.Values)
		{
			value2.Visible = false;
		}
		_bHasButtons = false;
		foreach (KeyValuePair<ucInfoBar.ActionTypes, ucInfoBar.ActionState> state in _states)
		{
			ucInfoBar.ActionState value = state.Value;
			if (value != null && value.Action != b1.Action && value.Action != b2.Action)
			{
				string key = "chkButton" + (num + 1);
				if (checkboxesDictionary.ContainsKey(key))
				{
					CheckBoxTS checkBoxTS = checkboxesDictionary[key];
					checkBoxTS.Tag = (int)value.Action;
					checkBoxTS.Text = value.DisplayString;
					checkBoxTS.Checked = value.Checked;
					toolTip1.SetToolTip(checkBoxTS, value.TipString);
					checkBoxTS.Visible = true;
					num++;
				}
			}
		}
		if (num > 0)
		{
			_bHasButtons = true;
			base.Height = num * (chkButton1.Size.Height + 1) + 4;
		}
	}

	private Dictionary<string, CheckBoxTS> getCheckboxesDictionary()
	{
		Dictionary<string, CheckBoxTS> dictionary = new Dictionary<string, CheckBoxTS>();
		foreach (Control control in base.Controls)
		{
			if (control.GetType() == typeof(CheckBoxTS))
			{
				dictionary.Add(control.Name, (CheckBoxTS)control);
			}
		}
		return dictionary;
	}

	private void chkButton1_MouseUp(object sender, MouseEventArgs e)
	{
		CheckBoxTS checkBoxTS = sender as CheckBoxTS;
		_ = e?.Button;
		if (checkBoxTS != null)
		{
			int.Parse(checkBoxTS.Name.Substring(9, 1));
			ActionClicked?.Invoke(sender, new PopupActionSelected
			{
				Action = (ucInfoBar.ActionTypes)checkBoxTS.Tag,
				ButtonState = checkBoxTS.Checked,
				Button = e.Button
			});
		}
	}

	public CheckBoxTS GetPopupButton(int index)
	{
		Dictionary<string, CheckBoxTS> checkboxesDictionary = getCheckboxesDictionary();
		if (checkboxesDictionary == null)
		{
			return null;
		}
		string key = "chkButton" + (index + 1);
		if (checkboxesDictionary.ContainsKey(key))
		{
			return checkboxesDictionary[key];
		}
		return null;
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
		this.chkButton8 = new System.Windows.Forms.CheckBoxTS();
		this.chkButton7 = new System.Windows.Forms.CheckBoxTS();
		this.chkButton6 = new System.Windows.Forms.CheckBoxTS();
		this.chkButton5 = new System.Windows.Forms.CheckBoxTS();
		this.chkButton4 = new System.Windows.Forms.CheckBoxTS();
		this.chkButton3 = new System.Windows.Forms.CheckBoxTS();
		this.chkButton2 = new System.Windows.Forms.CheckBoxTS();
		this.chkButton1 = new System.Windows.Forms.CheckBoxTS();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		base.SuspendLayout();
		this.chkButton8.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkButton8.FlatAppearance.BorderSize = 0;
		this.chkButton8.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver;
		this.chkButton8.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
		this.chkButton8.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
		this.chkButton8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.chkButton8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.chkButton8.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.chkButton8.Image = null;
		this.chkButton8.ImeMode = System.Windows.Forms.ImeMode.Off;
		this.chkButton8.Location = new System.Drawing.Point(3, 171);
		this.chkButton8.Margin = new System.Windows.Forms.Padding(0);
		this.chkButton8.Name = "chkButton8";
		this.chkButton8.Size = new System.Drawing.Size(50, 23);
		this.chkButton8.TabIndex = 40;
		this.chkButton8.Text = "Peak";
		this.chkButton8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkButton8.MouseUp += new System.Windows.Forms.MouseEventHandler(chkButton1_MouseUp);
		this.chkButton7.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkButton7.FlatAppearance.BorderSize = 0;
		this.chkButton7.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver;
		this.chkButton7.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
		this.chkButton7.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
		this.chkButton7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.chkButton7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.chkButton7.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.chkButton7.Image = null;
		this.chkButton7.ImeMode = System.Windows.Forms.ImeMode.Off;
		this.chkButton7.Location = new System.Drawing.Point(3, 147);
		this.chkButton7.Margin = new System.Windows.Forms.Padding(0);
		this.chkButton7.Name = "chkButton7";
		this.chkButton7.Size = new System.Drawing.Size(50, 23);
		this.chkButton7.TabIndex = 39;
		this.chkButton7.Text = "Peak";
		this.chkButton7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkButton7.MouseUp += new System.Windows.Forms.MouseEventHandler(chkButton1_MouseUp);
		this.chkButton6.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkButton6.FlatAppearance.BorderSize = 0;
		this.chkButton6.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver;
		this.chkButton6.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
		this.chkButton6.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
		this.chkButton6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.chkButton6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.chkButton6.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.chkButton6.Image = null;
		this.chkButton6.ImeMode = System.Windows.Forms.ImeMode.Off;
		this.chkButton6.Location = new System.Drawing.Point(3, 123);
		this.chkButton6.Margin = new System.Windows.Forms.Padding(0);
		this.chkButton6.Name = "chkButton6";
		this.chkButton6.Size = new System.Drawing.Size(50, 23);
		this.chkButton6.TabIndex = 38;
		this.chkButton6.Text = "Peak";
		this.chkButton6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkButton6.MouseUp += new System.Windows.Forms.MouseEventHandler(chkButton1_MouseUp);
		this.chkButton5.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkButton5.FlatAppearance.BorderSize = 0;
		this.chkButton5.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver;
		this.chkButton5.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
		this.chkButton5.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
		this.chkButton5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.chkButton5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.chkButton5.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.chkButton5.Image = null;
		this.chkButton5.ImeMode = System.Windows.Forms.ImeMode.Off;
		this.chkButton5.Location = new System.Drawing.Point(3, 99);
		this.chkButton5.Margin = new System.Windows.Forms.Padding(0);
		this.chkButton5.Name = "chkButton5";
		this.chkButton5.Size = new System.Drawing.Size(50, 23);
		this.chkButton5.TabIndex = 37;
		this.chkButton5.Text = "Peak";
		this.chkButton5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkButton5.MouseUp += new System.Windows.Forms.MouseEventHandler(chkButton1_MouseUp);
		this.chkButton4.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkButton4.FlatAppearance.BorderSize = 0;
		this.chkButton4.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver;
		this.chkButton4.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
		this.chkButton4.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
		this.chkButton4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.chkButton4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.chkButton4.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.chkButton4.Image = null;
		this.chkButton4.ImeMode = System.Windows.Forms.ImeMode.Off;
		this.chkButton4.Location = new System.Drawing.Point(3, 75);
		this.chkButton4.Margin = new System.Windows.Forms.Padding(0);
		this.chkButton4.Name = "chkButton4";
		this.chkButton4.Size = new System.Drawing.Size(50, 23);
		this.chkButton4.TabIndex = 36;
		this.chkButton4.Text = "Peak";
		this.chkButton4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkButton4.MouseUp += new System.Windows.Forms.MouseEventHandler(chkButton1_MouseUp);
		this.chkButton3.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkButton3.FlatAppearance.BorderSize = 0;
		this.chkButton3.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver;
		this.chkButton3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
		this.chkButton3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
		this.chkButton3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.chkButton3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.chkButton3.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.chkButton3.Image = null;
		this.chkButton3.ImeMode = System.Windows.Forms.ImeMode.Off;
		this.chkButton3.Location = new System.Drawing.Point(3, 51);
		this.chkButton3.Margin = new System.Windows.Forms.Padding(0);
		this.chkButton3.Name = "chkButton3";
		this.chkButton3.Size = new System.Drawing.Size(50, 23);
		this.chkButton3.TabIndex = 35;
		this.chkButton3.Text = "Peak";
		this.chkButton3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkButton3.MouseUp += new System.Windows.Forms.MouseEventHandler(chkButton1_MouseUp);
		this.chkButton2.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkButton2.FlatAppearance.BorderSize = 0;
		this.chkButton2.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver;
		this.chkButton2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
		this.chkButton2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
		this.chkButton2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.chkButton2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.chkButton2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.chkButton2.Image = null;
		this.chkButton2.ImeMode = System.Windows.Forms.ImeMode.Off;
		this.chkButton2.Location = new System.Drawing.Point(3, 27);
		this.chkButton2.Margin = new System.Windows.Forms.Padding(0);
		this.chkButton2.Name = "chkButton2";
		this.chkButton2.Size = new System.Drawing.Size(50, 23);
		this.chkButton2.TabIndex = 34;
		this.chkButton2.Text = "Peak";
		this.chkButton2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkButton2.MouseUp += new System.Windows.Forms.MouseEventHandler(chkButton1_MouseUp);
		this.chkButton1.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkButton1.FlatAppearance.BorderSize = 0;
		this.chkButton1.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver;
		this.chkButton1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
		this.chkButton1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
		this.chkButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.chkButton1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.chkButton1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.chkButton1.Image = null;
		this.chkButton1.ImeMode = System.Windows.Forms.ImeMode.Off;
		this.chkButton1.Location = new System.Drawing.Point(3, 3);
		this.chkButton1.Margin = new System.Windows.Forms.Padding(0);
		this.chkButton1.Name = "chkButton1";
		this.chkButton1.Size = new System.Drawing.Size(50, 23);
		this.chkButton1.TabIndex = 33;
		this.chkButton1.Text = "Peak";
		this.chkButton1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.chkButton1.MouseUp += new System.Windows.Forms.MouseEventHandler(chkButton1_MouseUp);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(56, 320);
		base.Controls.Add(this.chkButton8);
		base.Controls.Add(this.chkButton7);
		base.Controls.Add(this.chkButton6);
		base.Controls.Add(this.chkButton5);
		base.Controls.Add(this.chkButton4);
		base.Controls.Add(this.chkButton3);
		base.Controls.Add(this.chkButton2);
		base.Controls.Add(this.chkButton1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "frmInfoBarPopup";
		base.ShowInTaskbar = false;
		this.Text = "frmInfoBarPopup";
		base.ResumeLayout(false);
	}
}
