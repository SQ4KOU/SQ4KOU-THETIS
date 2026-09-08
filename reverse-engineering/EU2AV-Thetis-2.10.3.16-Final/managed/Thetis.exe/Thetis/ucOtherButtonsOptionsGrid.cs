using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class ucOtherButtonsOptionsGrid : UserControl
{
	public class MacroButtonEventArgs : EventArgs
	{
		public OtherButtonId Id { get; }

		public int BitGroup { get; }

		public int BitNumber { get; }

		public bool IsChecked { get; }

		public ButtonTS Button { get; }

		public CheckBoxTS CheckBox { get; }

		public MacroButtonEventArgs(OtherButtonId id, int bitGroup, int bitNumber, ButtonTS button, CheckBoxTS checkBox, bool isChecked)
		{
			Id = id;
			BitGroup = bitGroup;
			BitNumber = bitNumber;
			Button = button;
			CheckBox = checkBox;
			IsChecked = isChecked;
		}
	}

	private List<CheckBoxTS> _check_boxes;

	private List<ButtonTS> _buttons;

	private bool _init;

	private Dictionary<OtherButtonId, CheckBoxTS> _checkbox_by_id;

	private Dictionary<int, List<(int bit, CheckBoxTS cb)>> _checkbox_by_group;

	private TableLayoutPanel _table;

	private ToolTip _tooltip;

	private OtherButtonMacroSettings[] _macro_settings;

	private IContainer components;

	private ScrollableControl scrollableControl1;

	public event EventHandler CheckboxChanged;

	public event EventHandler<MacroButtonEventArgs> MacroSetupClicked;

	public ucOtherButtonsOptionsGrid()
	{
		_macro_settings = new OtherButtonMacroSettings[32];
		for (int i = 0; i < _macro_settings.Length; i++)
		{
			_macro_settings[i] = new OtherButtonMacroSettings();
			_macro_settings[i].Number = i;
		}
		_init = false;
		InitializeComponent();
		base.Size = new Size(173, 182);
		scrollableControl1.Location = new Point(0, 0);
		scrollableControl1.Size = new Size(170, 178);
		scrollableControl1.AutoScroll = true;
		_tooltip = new ToolTip();
		_tooltip.AutomaticDelay = 300;
		_tooltip.AutoPopDelay = 8000;
		_tooltip.InitialDelay = 500;
		_tooltip.ReshowDelay = 100;
		_tooltip.ShowAlways = true;
		_check_boxes = new List<CheckBoxTS>();
		_buttons = new List<ButtonTS>();
		_checkbox_by_id = new Dictionary<OtherButtonId, CheckBoxTS>();
		_checkbox_by_group = new Dictionary<int, List<(int, CheckBoxTS)>>();
		initialise_checkboxes();
	}

	private void initialise_checkboxes()
	{
		_init = false;
		for (int i = 0; i < _check_boxes.Count; i++)
		{
			_check_boxes[i].CheckedChanged -= checkbox_checked_changed;
		}
		for (int j = 0; j < _buttons.Count; j++)
		{
			_buttons[j].Click -= button_clicked;
		}
		_check_boxes.Clear();
		_buttons.Clear();
		_checkbox_by_id.Clear();
		_checkbox_by_group.Clear();
		int num = 24;
		int num2 = 0;
		int num3 = 2;
		int num4 = num + num2 + num3;
		if (_table == null)
		{
			_table = new TableLayoutPanel();
			_table.Name = "tbl_other_buttons";
			_table.AutoSize = false;
			_table.Dock = DockStyle.Top;
			_table.ColumnCount = 4;
			_table.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
			_table.ColumnStyles.Clear();
			_table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			_table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, num4));
			_table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			_table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, num4));
			_table.Padding = new Padding(0, 0, num3, 0);
			scrollableControl1.Controls.Add(_table);
		}
		else
		{
			_table.SuspendLayout();
			_table.Controls.Clear();
			_table.RowStyles.Clear();
			_table.ColumnStyles.Clear();
			_table.ColumnCount = 4;
			_table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			_table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, num4));
			_table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			_table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, num4));
			_table.Padding = new Padding(0, 0, num3, 0);
			_table.RowCount = 0;
			_table.ResumeLayout(performLayout: false);
		}
		SuspendLayout();
		scrollableControl1.SuspendLayout();
		_table.SuspendLayout();
		bool visible = scrollableControl1.Visible;
		scrollableControl1.Visible = false;
		_table.Width = scrollableControl1.ClientSize.Width;
		int num5 = 0;
		int num6 = 0;
		(OtherButtonId, int, int, string, string, string, string)[] checkBoxData = OtherButtonIdHelpers.CheckBoxData;
		for (int k = 0; k < checkBoxData.Length; k++)
		{
			if (checkBoxData[k].Item1 == OtherButtonId.INFO_TEXT)
			{
				if (num6 != 0)
				{
					num6 = 0;
					num5++;
				}
				LabelTS labelTS = new LabelTS();
				labelTS.Name = "lbl_" + k;
				labelTS.AutoSize = true;
				labelTS.Margin = new Padding(0, 2, 0, 0);
				labelTS.Font = new Font(Font, FontStyle.Bold);
				labelTS.Text = checkBoxData[k].Item4;
				_table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
				_table.Controls.Add(labelTS, 0, num5);
				_table.SetColumnSpan(labelTS, 4);
				_table.RowCount = num5 + 1;
				num5++;
				continue;
			}
			if (checkBoxData[k].Item1 == OtherButtonId.SPLITTER)
			{
				if (num6 != 0)
				{
					num6 = 0;
					num5++;
				}
				PanelTS panelTS = new PanelTS();
				panelTS.Name = "sep_" + k;
				panelTS.Height = 1;
				panelTS.Dock = DockStyle.Fill;
				panelTS.Margin = new Padding(0, 2, 0, 4);
				panelTS.BackColor = SystemColors.ControlDark;
				_table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
				_table.Controls.Add(panelTS, 0, num5);
				_table.SetColumnSpan(panelTS, 4);
				_table.RowCount = num5 + 1;
				num5++;
				continue;
			}
			CheckBoxTS checkBoxTS = new CheckBoxTS();
			int item = (int)checkBoxData[k].Item1;
			checkBoxTS.Name = "chkOtherButton_" + item;
			checkBoxTS.AutoSize = false;
			checkBoxTS.TextAlign = ContentAlignment.MiddleLeft;
			checkBoxTS.AutoEllipsis = true;
			checkBoxTS.Margin = new Padding(0, 0, 0, 0);
			checkBoxTS.Text = OtherButtonIdHelpers.OtherButtonIDToText(checkBoxData[k].Item1);
			checkBoxTS.Tag = (checkBoxData[k].Item1, checkBoxData[k].Item2, checkBoxData[k].Item3);
			_tooltip.SetToolTip(checkBoxTS, checkBoxData[k].Item7);
			int num7 = checkBoxTS.GetPreferredSize(Size.Empty).Height;
			checkBoxTS.MinimumSize = new Size(0, num7);
			checkBoxTS.Height = num7;
			checkBoxTS.Dock = DockStyle.Fill;
			checkBoxTS.CheckedChanged += checkbox_checked_changed;
			bool num8 = checkBoxData[k].Item1 >= OtherButtonId._MACRO_0 && checkBoxData[k].Item1 <= OtherButtonId._MACRO_30;
			ButtonTS buttonTS = null;
			if (num8)
			{
				buttonTS = new ButtonTS();
				ButtonTS buttonTS2 = buttonTS;
				item = (int)checkBoxData[k].Item1;
				buttonTS2.Name = "btnOtherButtonMacroButton_" + item;
				buttonTS.AutoSize = false;
				buttonTS.Size = new Size(num, num7);
				buttonTS.MinimumSize = new Size(num, num7);
				buttonTS.MaximumSize = new Size(num, num7);
				buttonTS.TextAlign = ContentAlignment.MiddleCenter;
				buttonTS.Margin = new Padding(num2, 0, num3, 0);
				buttonTS.Anchor = AnchorStyles.Left;
				buttonTS.Text = "...";
				buttonTS.Tag = (checkBoxData[k].Item1, checkBoxData[k].Item2, checkBoxData[k].Item3);
				_tooltip.SetToolTip(buttonTS, checkBoxData[k].Item7);
				buttonTS.Click += button_clicked;
			}
			if (num6 == 0)
			{
				_table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
				_table.RowCount = num5 + 1;
			}
			int num9 = ((num6 != 0) ? 2 : 0);
			_table.Controls.Add(checkBoxTS, num9, num5);
			if (!num8)
			{
				_table.SetColumnSpan(checkBoxTS, 2);
			}
			_check_boxes.Add(checkBoxTS);
			_checkbox_by_id[checkBoxData[k].Item1] = checkBoxTS;
			if (num8)
			{
				_table.Controls.Add(buttonTS, num9 + 1, num5);
				_buttons.Add(buttonTS);
			}
			if (checkBoxData[k].Item2 >= 0 && checkBoxData[k].Item3 >= 0)
			{
				if (!_checkbox_by_group.TryGetValue(checkBoxData[k].Item2, out List<(int, CheckBoxTS)> value))
				{
					value = new List<(int, CheckBoxTS)>();
					_checkbox_by_group[checkBoxData[k].Item2] = value;
				}
				value.Add((checkBoxData[k].Item3, checkBoxTS));
			}
			num6++;
			if (num6 > 1)
			{
				num6 = 0;
				num5++;
			}
		}
		_table.Height = _table.PreferredSize.Height;
		scrollableControl1.Visible = visible;
		_table.ResumeLayout(performLayout: true);
		scrollableControl1.ResumeLayout(performLayout: true);
		ResumeLayout(performLayout: true);
		_init = true;
	}

	private void checkbox_checked_changed(object sender, EventArgs e)
	{
		if (_init && CheckboxChanged != null)
		{
			CheckboxChanged(this, EventArgs.Empty);
		}
	}

	private void button_clicked(object sender, EventArgs e)
	{
		if (_init)
		{
			ButtonTS buttonTS = (ButtonTS)sender;
			(OtherButtonId, int, int) tuple = ((OtherButtonId, int, int))buttonTS.Tag;
			_checkbox_by_id.TryGetValue(tuple.Item1, out var value);
			bool isChecked = value?.Checked ?? false;
			MacroButtonEventArgs e2 = new MacroButtonEventArgs(tuple.Item1, tuple.Item2, tuple.Item3, buttonTS, value, isChecked);
			if (MacroSetupClicked != null)
			{
				MacroSetupClicked(this, e2);
			}
		}
	}

	public int GetBitfield(int bit_group)
	{
		int num = 0;
		if (!_checkbox_by_group.TryGetValue(bit_group, out List<(int, CheckBoxTS)> value))
		{
			return 0;
		}
		for (int i = 0; i < value.Count; i++)
		{
			if (value[i].Item2.Checked)
			{
				num |= 1 << value[i].Item1;
			}
		}
		return num;
	}

	public void SetBitfield(int bit_group, int value)
	{
		bool init = _init;
		_init = false;
		if (_checkbox_by_group.TryGetValue(bit_group, out List<(int, CheckBoxTS)> value2))
		{
			for (int i = 0; i < value2.Count; i++)
			{
				bool flag = (value & (1 << value2[i].Item1)) != 0;
				value2[i].Item2.Checked = flag;
			}
		}
		_init = init;
	}

	public int GetCheckedCount(int bit_group)
	{
		int num = 0;
		if (!_checkbox_by_group.TryGetValue(bit_group, out List<(int, CheckBoxTS)> value))
		{
			return 0;
		}
		for (int i = 0; i < value.Count; i++)
		{
			if (value[i].Item2.Checked)
			{
				num++;
			}
		}
		return num;
	}

	public OtherButtonMacroSettings GetMacroSettings(int macro)
	{
		if (macro < 0 || macro > _macro_settings.Length - 1)
		{
			return null;
		}
		return _macro_settings[macro];
	}

	public void SetMacroSettings(int macro, OtherButtonMacroSettings settings)
	{
		if (macro >= 0 && macro <= _macro_settings.Length - 1)
		{
			_macro_settings[macro] = new OtherButtonMacroSettings(settings);
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
		this.scrollableControl1 = new System.Windows.Forms.ScrollableControl();
		base.SuspendLayout();
		this.scrollableControl1.AutoScroll = true;
		this.scrollableControl1.Location = new System.Drawing.Point(0, 0);
		this.scrollableControl1.Name = "scrollableControl1";
		this.scrollableControl1.Size = new System.Drawing.Size(268, 267);
		this.scrollableControl1.TabIndex = 0;
		this.scrollableControl1.Text = "scrollableControl1";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.scrollableControl1);
		base.Name = "ucOtherButtonsOptionsGrid";
		base.Size = new System.Drawing.Size(723, 725);
		base.ResumeLayout(false);
	}
}
