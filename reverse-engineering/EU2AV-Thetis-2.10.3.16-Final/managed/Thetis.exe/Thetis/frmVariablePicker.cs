using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class frmVariablePicker : Form
{
	private class clsVariableListItems
	{
		internal enum VariableListItemType
		{
			MMIO = 0,
			VARS = 1,
			CAT = 2,
			TEXT_ONLY = 999
		}

		private Guid _guid;

		private string _variable;

		private VariableListItemType _type;

		public Guid Guid
		{
			get
			{
				return _guid;
			}
			set
			{
				_guid = value;
			}
		}

		public string Variable
		{
			get
			{
				return _variable;
			}
			set
			{
				_variable = value;
			}
		}

		public VariableListItemType ListItemType
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		public clsVariableListItems(Guid guid, string variable, VariableListItemType listItemType)
		{
			_guid = guid;
			_variable = variable;
			_type = listItemType;
		}

		public override string ToString()
		{
			return _variable;
		}
	}

	private bool _textoverlay_led_picker;

	private Guid _guid;

	private string _variable;

	private IContainer components;

	private ListBox lstVariables;

	private ButtonTS btnCancel;

	private ButtonTS btnSelect;

	private ButtonTS btnDefault;

	public Guid Guid
	{
		get
		{
			return _guid;
		}
		set
		{
			_guid = value;
		}
	}

	public string Variable
	{
		get
		{
			return _variable;
		}
		set
		{
			_variable = value;
		}
	}

	public frmVariablePicker()
	{
		_textoverlay_led_picker = false;
		InitializeComponent();
		lstVariables.DrawMode = DrawMode.OwnerDrawFixed;
		Size size = TextRenderer.MeasureText("Ag", lstVariables.Font);
		int num = 2;
		int num2 = size.Height + num;
		if (num2 < 20)
		{
			num2 = 20;
		}
		lstVariables.ItemHeight = num2;
		lstVariables.DrawItem += list_box_DrawItem;
	}

	private Color colour_for_type(clsVariableListItems.VariableListItemType t)
	{
		return t switch
		{
			clsVariableListItems.VariableListItemType.MMIO => Color.Teal, 
			clsVariableListItems.VariableListItemType.VARS => Color.Tan, 
			clsVariableListItems.VariableListItemType.CAT => Color.Gray, 
			clsVariableListItems.VariableListItemType.TEXT_ONLY => Color.Transparent, 
			_ => Color.Gray, 
		};
	}

	private void list_box_DrawItem(object sender, DrawItemEventArgs e)
	{
		if (e.Index < 0)
		{
			return;
		}
		e.DrawBackground();
		clsVariableListItems clsVariableListItems2 = (clsVariableListItems)((ListBox)sender).Items[e.Index];
		Rectangle rect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y + 4, 6, e.Bounds.Height - 8);
		Color color = colour_for_type(clsVariableListItems2.ListItemType);
		if (color.A > 0)
		{
			using (Brush brush = new SolidBrush(color))
			{
				e.Graphics.FillRectangle(brush, rect);
			}
			using Pen pen = new Pen(Color.Black);
			e.Graphics.DrawRectangle(pen, rect);
		}
		Point pt = new Point(rect.Right + 6, e.Bounds.Y + (e.Bounds.Height - e.Font.Height) / 2);
		TextRenderer.DrawText(e.Graphics, clsVariableListItems2.ToString(), e.Font, pt, e.ForeColor, TextFormatFlags.NoPadding);
		e.DrawFocusRectangle();
	}

	private void btnSelect_Click(object sender, EventArgs e)
	{
		if (!(lstVariables.SelectedItem is clsVariableListItems clsVariableListItems2))
		{
			_guid = Guid.Empty;
			_variable = (_textoverlay_led_picker ? "" : "--DEFAULT--");
		}
		else
		{
			_guid = (_textoverlay_led_picker ? Guid.Empty : clsVariableListItems2.Guid);
			_variable = clsVariableListItems2.Variable;
		}
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		_guid = Guid.Empty;
		_variable = (_textoverlay_led_picker ? "" : "--DEFAULT--");
	}

	public void Init(int variable, Guid g, string current, bool textoverlay_led_picker = false)
	{
		_textoverlay_led_picker = textoverlay_led_picker;
		btnDefault.Visible = !textoverlay_led_picker;
		string text = (textoverlay_led_picker ? "Variable Picker to Clipboard" : ("Variable Picker (" + variable + ")"));
		Text = text;
		lstVariables.Items.Clear();
		int selectedIndex = -1;
		if (textoverlay_led_picker)
		{
			int num = -1;
			selectedIndex = -1;
			g = Guid.Empty;
		}
		else
		{
			int num = lstVariables.Items.Add(new clsVariableListItems(Guid.Empty, "--DEFAULT--", clsVariableListItems.VariableListItemType.TEXT_ONLY));
			if (g == Guid.Empty)
			{
				selectedIndex = num;
			}
		}
		foreach (KeyValuePair<Guid, MultiMeterIO.clsMMIO> datum in MultiMeterIO.Data)
		{
			MultiMeterIO.clsMMIO value = datum.Value;
			foreach (KeyValuePair<string, object> item in value.Variables())
			{
				int num = lstVariables.Items.Add(new clsVariableListItems(value.Guid, item.Key, clsVariableListItems.VariableListItemType.MMIO));
				if (value.Guid == g && current == item.Key)
				{
					selectedIndex = num;
				}
			}
		}
		if (textoverlay_led_picker)
		{
			foreach (string availableReading in MeterManager.ReadingsCustom(1).GetAvailableReadings())
			{
				lstVariables.Items.Add(new clsVariableListItems(Guid.Empty, availableReading, clsVariableListItems.VariableListItemType.VARS));
			}
			foreach (string item2 in MeterManager.CatVariables())
			{
				lstVariables.Items.Add(new clsVariableListItems(Guid.Empty, item2.Trim('%'), clsVariableListItems.VariableListItemType.CAT));
			}
			lstVariables.SelectedIndex = 0;
		}
		else
		{
			lstVariables.SelectedIndex = selectedIndex;
		}
	}

	private void btnDefault_Click(object sender, EventArgs e)
	{
		_guid = Guid.Empty;
		_variable = "--DEFAULT--";
	}

	private void lstVariables_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		ListBox listBox = (ListBox)sender;
		int num = listBox.IndexFromPoint(e.Location);
		if (num != -1 && listBox.Items[num] is clsVariableListItems)
		{
			btnSelect_Click(this, EventArgs.Empty);
			base.DialogResult = DialogResult.OK;
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
		this.lstVariables = new System.Windows.Forms.ListBox();
		this.btnCancel = new System.Windows.Forms.ButtonTS();
		this.btnSelect = new System.Windows.Forms.ButtonTS();
		this.btnDefault = new System.Windows.Forms.ButtonTS();
		base.SuspendLayout();
		this.lstVariables.FormattingEnabled = true;
		this.lstVariables.Location = new System.Drawing.Point(12, 12);
		this.lstVariables.Name = "lstVariables";
		this.lstVariables.Size = new System.Drawing.Size(205, 290);
		this.lstVariables.TabIndex = 0;
		this.lstVariables.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(lstVariables_MouseDoubleClick);
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Image = null;
		this.btnCancel.Location = new System.Drawing.Point(165, 308);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Selectable = true;
		this.btnCancel.Size = new System.Drawing.Size(52, 23);
		this.btnCancel.TabIndex = 4;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.btnSelect.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnSelect.Image = null;
		this.btnSelect.Location = new System.Drawing.Point(107, 308);
		this.btnSelect.Name = "btnSelect";
		this.btnSelect.Selectable = true;
		this.btnSelect.Size = new System.Drawing.Size(52, 23);
		this.btnSelect.TabIndex = 3;
		this.btnSelect.Text = "Select";
		this.btnSelect.UseVisualStyleBackColor = true;
		this.btnSelect.Click += new System.EventHandler(btnSelect_Click);
		this.btnDefault.DialogResult = System.Windows.Forms.DialogResult.Ignore;
		this.btnDefault.Image = null;
		this.btnDefault.Location = new System.Drawing.Point(12, 308);
		this.btnDefault.Name = "btnDefault";
		this.btnDefault.Selectable = true;
		this.btnDefault.Size = new System.Drawing.Size(52, 23);
		this.btnDefault.TabIndex = 5;
		this.btnDefault.Text = "Default";
		this.btnDefault.UseVisualStyleBackColor = true;
		this.btnDefault.Click += new System.EventHandler(btnDefault_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(229, 338);
		base.Controls.Add(this.btnDefault);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnSelect);
		base.Controls.Add(this.lstVariables);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Name = "frmVariablePicker";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Variable Picker";
		base.ResumeLayout(false);
	}
}
