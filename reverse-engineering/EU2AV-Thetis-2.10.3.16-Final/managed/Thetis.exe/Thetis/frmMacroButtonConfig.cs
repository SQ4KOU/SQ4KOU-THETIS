using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CatAtonic;
using Thetis.Properties;

namespace Thetis;

public class frmMacroButtonConfig : Form
{
	private class clsContainerComboboxItem
	{
		public string Text { get; set; }

		public string ID { get; set; }

		public int ListIndex { get; set; }

		public override string ToString()
		{
			return ListIndex + " - " + Text;
		}
	}

	private string _base_title;

	private OtherButtonMacroSettings _settings;

	private CATScriptInterpreter _si;

	private CATTester _cat_tester;

	private Console _console;

	private IContainer components;

	private LabelTS labelTS1;

	private LabelTS labelTS2;

	private TextBoxTS txtON;

	private TextBoxTS txtOFF;

	private CheckBoxTS chkClosesParent;

	private CheckBoxTS chkClosesContainer_1;

	private ComboBoxTS comboCloseContainer_1;

	private ComboBoxTS comboOpenContainer_1;

	private CheckBoxTS chkOpensContainer_1;

	private CheckBoxTS chkUseParent_loc_size_1;

	private CheckBoxTS chkUseParent_loc_size_2;

	private ComboBoxTS comboOpenContainer_2;

	private CheckBoxTS chkOpensContainer_2;

	private ComboBoxTS comboCloseContainer_2;

	private CheckBoxTS chkClosesContainer_2;

	private CheckBoxTS chkUseParent_loc_size_3;

	private ComboBoxTS comboOpenContainer_3;

	private CheckBoxTS chkOpensContainer_3;

	private ComboBoxTS comboCloseContainer_3;

	private CheckBoxTS chkClosesContainer_3;

	private CheckBoxTS chkUseParent_loc_size_4;

	private ComboBoxTS comboOpenContainer_4;

	private CheckBoxTS chkOpensContainer_4;

	private ComboBoxTS comboCloseContainer_4;

	private CheckBoxTS chkClosesContainer_4;

	private RadioButtonTS radButtonState_off;

	private RadioButtonTS radButtonState_on;

	private RadioButtonTS radButtonState_led;

	private RadioButtonTS radButtonState_toggle;

	private RadioButtonTS radButtonState_container_visible;

	private ComboBoxTS comboButtonState_container_visibility;

	private TextBoxTS txtButtonState_led_4char;

	private GroupBoxTS groupBoxTS1;

	private TextBoxTS txtNotes;

	private LabelTS labelTS3;

	private GroupBoxTS groupBoxTS2;

	private TextBoxTS txtMMIO_message_1;

	private TextBoxTS txtMMIO_4char_1;

	private CheckBoxTS chkSendMssageViaMMIO_1;

	private LabelTS labelTS6;

	private TextBoxTS txtMMIO_message_4;

	private TextBoxTS txtMMIO_4char_4;

	private CheckBoxTS chkSendMssageViaMMIO_4;

	private TextBoxTS txtMMIO_message_3;

	private TextBoxTS txtMMIO_4char_3;

	private CheckBoxTS chkSendMssageViaMMIO_3;

	private TextBoxTS txtMMIO_message_2;

	private TextBoxTS txtMMIO_4char_2;

	private CheckBoxTS chkSendMssageViaMMIO_2;

	private LabelTS labelTS5;

	private LabelTS labelTS4;

	private GroupBoxTS groupBoxTS3;

	private TextBoxTS txtCatMacro;

	private RadioButtonTS radButtonState_catstate;

	private ButtonTS btnOK;

	private LabelTS labelTS7;

	private TextBoxTS txtButtonState_cat_on_reply;

	private PictureBox picError;

	private LabelTS lblErrorText;

	private ButtonTS btnCancel;

	private TextBoxTS txtTokens;

	private CheckBoxTS chkCatSend_1;

	private PictureBox pictureBox1;

	private ToolTip toolTip1;

	private TabControl tabControl1;

	private TabPage tpMMIO;

	private TabPage tpCAT;

	private LabelTS labelTS8;

	private TextBoxTS txtMMIO_message_off_4;

	private TextBoxTS txtMMIO_message_off_3;

	private TextBoxTS txtMMIO_message_off_2;

	private TextBoxTS txtMMIO_message_off_1;

	private TabPage tpButtonState;

	private ButtonTS btnCatTest;

	private CheckBoxTS chkRunStateCommandOnVisible;

	public frmMacroButtonConfig(CATScriptInterpreter si)
	{
		InitializeComponent();
		_console = null;
		_cat_tester = null;
		_base_title = Text;
		picError.Visible = false;
		lblErrorText.Visible = false;
		_settings = null;
		_si = si;
	}

	public DialogResult InitAndShow(OtherButtonMacroSettings settings, Dictionary<string, string> containers, ref OtherButtonMacroSettings working_set, Console c)
	{
		_console = c;
		_settings = working_set;
		Text = _base_title + $" - Macro {_settings.Number + 1}";
		txtON.Text = _settings.OnText;
		txtOFF.Text = _settings.OffText;
		txtNotes.Text = _settings.Notes;
		chkClosesParent.Checked = _settings.ClosesParent;
		chkClosesContainer_1.Checked = _settings.ClosesContainer[0];
		chkClosesContainer_2.Checked = _settings.ClosesContainer[1];
		chkClosesContainer_3.Checked = _settings.ClosesContainer[2];
		chkClosesContainer_4.Checked = _settings.ClosesContainer[3];
		chkOpensContainer_1.Checked = _settings.OpensContainer[0];
		chkOpensContainer_2.Checked = _settings.OpensContainer[1];
		chkOpensContainer_3.Checked = _settings.OpensContainer[2];
		chkOpensContainer_4.Checked = _settings.OpensContainer[3];
		chkUseParent_loc_size_1.Checked = _settings.OpenUsesLocation[0];
		chkUseParent_loc_size_2.Checked = _settings.OpenUsesLocation[1];
		chkUseParent_loc_size_3.Checked = _settings.OpenUsesLocation[2];
		chkUseParent_loc_size_4.Checked = _settings.OpenUsesLocation[3];
		chkSendMssageViaMMIO_1.Checked = _settings.SendsViaMMIO[0];
		chkSendMssageViaMMIO_2.Checked = _settings.SendsViaMMIO[1];
		chkSendMssageViaMMIO_3.Checked = _settings.SendsViaMMIO[2];
		chkSendMssageViaMMIO_4.Checked = _settings.SendsViaMMIO[3];
		txtMMIO_4char_1.Text = _settings.MMICFourChar[0];
		txtMMIO_4char_2.Text = _settings.MMICFourChar[1];
		txtMMIO_4char_3.Text = _settings.MMICFourChar[2];
		txtMMIO_4char_4.Text = _settings.MMICFourChar[3];
		txtMMIO_message_1.Text = _settings.MMIOMessageON[0];
		txtMMIO_message_2.Text = _settings.MMIOMessageON[1];
		txtMMIO_message_3.Text = _settings.MMIOMessageON[2];
		txtMMIO_message_4.Text = _settings.MMIOMessageON[3];
		txtMMIO_message_off_1.Text = _settings.MMIOMessageOFF[0];
		txtMMIO_message_off_2.Text = _settings.MMIOMessageOFF[1];
		txtMMIO_message_off_3.Text = _settings.MMIOMessageOFF[2];
		txtMMIO_message_off_4.Text = _settings.MMIOMessageOFF[3];
		txtButtonState_led_4char.Text = _settings.LedIndiciatorFourChar;
		txtButtonState_cat_on_reply.Text = _settings.ButtonStateCatReply;
		chkRunStateCommandOnVisible.Checked = _settings.RunStateCommandOnVisible;
		switch (_settings.ButtonStateType)
		{
		case OtherButtonMacroSettings.OB_ButtonState.OFF:
			radButtonState_off.Checked = true;
			break;
		case OtherButtonMacroSettings.OB_ButtonState.ON:
			radButtonState_on.Checked = true;
			break;
		case OtherButtonMacroSettings.OB_ButtonState.TOGGLE:
			radButtonState_toggle.Checked = true;
			break;
		case OtherButtonMacroSettings.OB_ButtonState.LED:
			radButtonState_led.Checked = true;
			break;
		case OtherButtonMacroSettings.OB_ButtonState.CONT_VIS:
			radButtonState_container_visible.Checked = true;
			break;
		case OtherButtonMacroSettings.OB_ButtonState.CAT:
			radButtonState_catstate.Checked = true;
			break;
		}
		chkCatSend_1.Checked = _settings.CatMacroSend[0];
		txtCatMacro.Text = _settings.CatMacro;
		int num = 1;
		foreach (KeyValuePair<string, string> container in containers)
		{
			clsContainerComboboxItem clsContainerComboboxItem2 = new clsContainerComboboxItem();
			clsContainerComboboxItem2.ID = container.Key;
			clsContainerComboboxItem2.ListIndex = num;
			clsContainerComboboxItem2.Text = container.Value;
			int selectedIndex = comboCloseContainer_1.Items.Add(clsContainerComboboxItem2);
			if (clsContainerComboboxItem2.ID == _settings.CloseContainerID[0])
			{
				comboCloseContainer_1.SelectedIndex = selectedIndex;
			}
			selectedIndex = comboCloseContainer_2.Items.Add(clsContainerComboboxItem2);
			if (clsContainerComboboxItem2.ID == _settings.CloseContainerID[1])
			{
				comboCloseContainer_2.SelectedIndex = selectedIndex;
			}
			selectedIndex = comboCloseContainer_3.Items.Add(clsContainerComboboxItem2);
			if (clsContainerComboboxItem2.ID == _settings.CloseContainerID[2])
			{
				comboCloseContainer_3.SelectedIndex = selectedIndex;
			}
			selectedIndex = comboCloseContainer_4.Items.Add(clsContainerComboboxItem2);
			if (clsContainerComboboxItem2.ID == _settings.CloseContainerID[3])
			{
				comboCloseContainer_4.SelectedIndex = selectedIndex;
			}
			selectedIndex = comboOpenContainer_1.Items.Add(clsContainerComboboxItem2);
			if (clsContainerComboboxItem2.ID == _settings.OpenContainerID[0])
			{
				comboOpenContainer_1.SelectedIndex = selectedIndex;
			}
			selectedIndex = comboOpenContainer_2.Items.Add(clsContainerComboboxItem2);
			if (clsContainerComboboxItem2.ID == _settings.OpenContainerID[1])
			{
				comboOpenContainer_2.SelectedIndex = selectedIndex;
			}
			selectedIndex = comboOpenContainer_3.Items.Add(clsContainerComboboxItem2);
			if (clsContainerComboboxItem2.ID == _settings.OpenContainerID[2])
			{
				comboOpenContainer_3.SelectedIndex = selectedIndex;
			}
			selectedIndex = comboOpenContainer_4.Items.Add(clsContainerComboboxItem2);
			if (clsContainerComboboxItem2.ID == _settings.OpenContainerID[3])
			{
				comboOpenContainer_4.SelectedIndex = selectedIndex;
			}
			comboButtonState_container_visibility.Items.Add(clsContainerComboboxItem2);
			if (clsContainerComboboxItem2.ID == _settings.ContainerVisibleID)
			{
				comboButtonState_container_visibility.SelectedIndex = selectedIndex;
			}
			num++;
		}
		if (comboCloseContainer_1.Items.Count > 0 && comboCloseContainer_1.SelectedIndex == -1)
		{
			comboCloseContainer_1.SelectedIndex = 0;
		}
		if (comboCloseContainer_2.Items.Count > 0 && comboCloseContainer_2.SelectedIndex == -1)
		{
			comboCloseContainer_2.SelectedIndex = 0;
		}
		if (comboCloseContainer_3.Items.Count > 0 && comboCloseContainer_3.SelectedIndex == -1)
		{
			comboCloseContainer_3.SelectedIndex = 0;
		}
		if (comboCloseContainer_4.Items.Count > 0 && comboCloseContainer_4.SelectedIndex == -1)
		{
			comboCloseContainer_4.SelectedIndex = 0;
		}
		if (comboOpenContainer_1.Items.Count > 0 && comboOpenContainer_1.SelectedIndex == -1)
		{
			comboOpenContainer_1.SelectedIndex = 0;
		}
		if (comboOpenContainer_2.Items.Count > 0 && comboOpenContainer_2.SelectedIndex == -1)
		{
			comboOpenContainer_2.SelectedIndex = 0;
		}
		if (comboOpenContainer_3.Items.Count > 0 && comboOpenContainer_3.SelectedIndex == -1)
		{
			comboOpenContainer_3.SelectedIndex = 0;
		}
		if (comboOpenContainer_4.Items.Count > 0 && comboOpenContainer_4.SelectedIndex == -1)
		{
			comboOpenContainer_4.SelectedIndex = 0;
		}
		if (comboButtonState_container_visibility.Items.Count > 0 && comboButtonState_container_visibility.SelectedIndex == -1)
		{
			comboButtonState_container_visibility.SelectedIndex = 0;
		}
		updateUseParent(0);
		updateUseParent(1);
		updateUseParent(2);
		updateUseParent(3);
		DialogResult result = ShowDialog();
		if (_cat_tester != null && !_cat_tester.IsDisposed)
		{
			_cat_tester.Close();
		}
		return result;
	}

	private void txtON_TextChanged(object sender, EventArgs e)
	{
		_settings.OnText = txtON.Text;
	}

	private void txtOFF_TextChanged(object sender, EventArgs e)
	{
		_settings.OffText = txtOFF.Text;
	}

	private void txtNotes_TextChanged(object sender, EventArgs e)
	{
		_settings.Notes = txtNotes.Text;
	}

	private void chkClosesParent_CheckedChanged(object sender, EventArgs e)
	{
		_settings.ClosesParent = chkClosesParent.Checked;
	}

	private int getIndexFromName(object sender)
	{
		if (!(sender.GetType() == typeof(CheckBoxTS)) && !(sender.GetType() == typeof(TextBoxTS)) && !(sender.GetType() == typeof(ComboBoxTS)))
		{
			return -1;
		}
		if (!(sender is Control { Name: var name }))
		{
			return -1;
		}
		if (string.IsNullOrEmpty(name))
		{
			return -1;
		}
		int num = name.LastIndexOf('_');
		if (num < 0 || num == name.Length - 1)
		{
			return -1;
		}
		if (int.TryParse(name.Substring(num + 1).Trim(), out var result))
		{
			return result - 1;
		}
		return -1;
	}

	private void chkClosesContainer_n_CheckedChanged(object sender, EventArgs e)
	{
		int indexFromName = getIndexFromName(sender);
		if (indexFromName != -1)
		{
			_settings.ClosesContainer[indexFromName] = (sender as CheckBoxTS).Checked;
			updateUseParent(indexFromName);
		}
	}

	private void comboOpenContainer_n_SelectedIndexChanged(object sender, EventArgs e)
	{
		int indexFromName = getIndexFromName(sender);
		if (indexFromName != -1)
		{
			clsContainerComboboxItem clsContainerComboboxItem2 = (sender as ComboBoxTS).SelectedItem as clsContainerComboboxItem;
			_settings.OpenContainerID[indexFromName] = clsContainerComboboxItem2.ID;
			updateUseParent(indexFromName);
		}
	}

	private void chkOpensContainer_n_CheckedChanged(object sender, EventArgs e)
	{
		int indexFromName = getIndexFromName(sender);
		if (indexFromName != -1)
		{
			_settings.OpensContainer[indexFromName] = (sender as CheckBoxTS).Checked;
			updateUseParent(indexFromName);
		}
	}

	private void comboCloseContainer_n_SelectedIndexChanged(object sender, EventArgs e)
	{
		int indexFromName = getIndexFromName(sender);
		if (indexFromName != -1)
		{
			clsContainerComboboxItem clsContainerComboboxItem2 = (sender as ComboBoxTS).SelectedItem as clsContainerComboboxItem;
			_settings.CloseContainerID[indexFromName] = clsContainerComboboxItem2.ID;
			updateUseParent(indexFromName);
		}
	}

	private void updateUseParent(int idx)
	{
		switch (idx)
		{
		case 0:
		{
			clsContainerComboboxItem clsContainerComboboxItem2 = comboCloseContainer_1.SelectedItem as clsContainerComboboxItem;
			clsContainerComboboxItem clsContainerComboboxItem3 = comboOpenContainer_1.SelectedItem as clsContainerComboboxItem;
			string obj4 = ((clsContainerComboboxItem2 == null) ? "" : clsContainerComboboxItem2.ID);
			string text = ((clsContainerComboboxItem3 == null) ? "" : clsContainerComboboxItem3.ID);
			bool enabled = obj4 == "" || text == "" || !_settings.ClosesContainer[0] || !(clsContainerComboboxItem2.ID == clsContainerComboboxItem3.ID) || !_settings.OpensContainer[0];
			chkUseParent_loc_size_1.Enabled = enabled;
			break;
		}
		case 1:
		{
			clsContainerComboboxItem clsContainerComboboxItem2 = comboCloseContainer_2.SelectedItem as clsContainerComboboxItem;
			clsContainerComboboxItem clsContainerComboboxItem3 = comboOpenContainer_2.SelectedItem as clsContainerComboboxItem;
			string obj2 = ((clsContainerComboboxItem2 == null) ? "" : clsContainerComboboxItem2.ID);
			string text = ((clsContainerComboboxItem3 == null) ? "" : clsContainerComboboxItem3.ID);
			bool enabled = obj2 == "" || text == "" || !_settings.ClosesContainer[1] || !(clsContainerComboboxItem2.ID == clsContainerComboboxItem3.ID) || !_settings.OpensContainer[1];
			chkUseParent_loc_size_2.Enabled = enabled;
			break;
		}
		case 2:
		{
			clsContainerComboboxItem clsContainerComboboxItem2 = comboCloseContainer_3.SelectedItem as clsContainerComboboxItem;
			clsContainerComboboxItem clsContainerComboboxItem3 = comboOpenContainer_3.SelectedItem as clsContainerComboboxItem;
			string obj3 = ((clsContainerComboboxItem2 == null) ? "" : clsContainerComboboxItem2.ID);
			string text = ((clsContainerComboboxItem3 == null) ? "" : clsContainerComboboxItem3.ID);
			bool enabled = obj3 == "" || text == "" || !_settings.ClosesContainer[2] || !(clsContainerComboboxItem2.ID == clsContainerComboboxItem3.ID) || !_settings.OpensContainer[2];
			chkUseParent_loc_size_3.Enabled = enabled;
			break;
		}
		case 3:
		{
			clsContainerComboboxItem clsContainerComboboxItem2 = comboCloseContainer_4.SelectedItem as clsContainerComboboxItem;
			clsContainerComboboxItem clsContainerComboboxItem3 = comboOpenContainer_4.SelectedItem as clsContainerComboboxItem;
			string obj = ((clsContainerComboboxItem2 == null) ? "" : clsContainerComboboxItem2.ID);
			string text = ((clsContainerComboboxItem3 == null) ? "" : clsContainerComboboxItem3.ID);
			bool enabled = obj == "" || text == "" || !_settings.ClosesContainer[3] || !(clsContainerComboboxItem2.ID == clsContainerComboboxItem3.ID) || !_settings.OpensContainer[3];
			chkUseParent_loc_size_4.Enabled = enabled;
			break;
		}
		}
	}

	private void chkUseParentCoodsForOpen_n_CheckedChanged(object sender, EventArgs e)
	{
		int indexFromName = getIndexFromName(sender);
		if (indexFromName != -1)
		{
			_settings.OpenUsesLocation[indexFromName] = (sender as CheckBoxTS).Checked;
		}
	}

	private void chkSendMssageViaMMIO_n_CheckedChanged(object sender, EventArgs e)
	{
		int indexFromName = getIndexFromName(sender);
		if (indexFromName != -1)
		{
			_settings.SendsViaMMIO[indexFromName] = (sender as CheckBoxTS).Checked;
		}
	}

	private void txtMMIO_4char_n_TextChanged(object sender, EventArgs e)
	{
		int indexFromName = getIndexFromName(sender);
		if (indexFromName != -1)
		{
			_settings.MMICFourChar[indexFromName] = (sender as TextBoxTS).Text;
		}
	}

	private void txtMMIO_message_n_TextChanged(object sender, EventArgs e)
	{
		int indexFromName = getIndexFromName(sender);
		if (indexFromName != -1)
		{
			_settings.MMIOMessageON[indexFromName] = (sender as TextBoxTS).Text;
		}
	}

	private void txtMMIO_message_n_off_TextChanged(object sender, EventArgs e)
	{
		int indexFromName = getIndexFromName(sender);
		if (indexFromName != -1)
		{
			_settings.MMIOMessageOFF[indexFromName] = (sender as TextBoxTS).Text;
		}
	}

	private void radButtonState_n_CheckedChanged(object sender, EventArgs e)
	{
		if (!(sender as RadioButtonTS).Checked || !(sender is Control { Name: var name }))
		{
			return;
		}
		int num = name.LastIndexOf('_');
		if (num >= 0 && num != name.Length - 1)
		{
			switch (name.Substring(num + 1).Trim().ToLower())
			{
			case "off":
				_settings.ButtonStateType = OtherButtonMacroSettings.OB_ButtonState.OFF;
				break;
			case "on":
				_settings.ButtonStateType = OtherButtonMacroSettings.OB_ButtonState.ON;
				break;
			case "toggle":
				_settings.ButtonStateType = OtherButtonMacroSettings.OB_ButtonState.TOGGLE;
				break;
			case "led":
				_settings.ButtonStateType = OtherButtonMacroSettings.OB_ButtonState.LED;
				break;
			case "visible":
				_settings.ButtonStateType = OtherButtonMacroSettings.OB_ButtonState.CONT_VIS;
				break;
			case "catstate":
				_settings.ButtonStateType = OtherButtonMacroSettings.OB_ButtonState.CAT;
				break;
			}
		}
	}

	private void txtButtonState_led_4char_TextChanged(object sender, EventArgs e)
	{
		_settings.LedIndiciatorFourChar = txtButtonState_led_4char.Text;
	}

	private void comboButtonState_container_visibility_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboButtonState_container_visibility.SelectedIndex != -1)
		{
			clsContainerComboboxItem clsContainerComboboxItem2 = comboButtonState_container_visibility.SelectedItem as clsContainerComboboxItem;
			_settings.ContainerVisibleID = ((clsContainerComboboxItem2 == null) ? "" : clsContainerComboboxItem2.ID);
		}
	}

	private void txtButtonState_cat_on_reply_TextChanged(object sender, EventArgs e)
	{
		_settings.ButtonStateCatReply = txtButtonState_cat_on_reply.Text;
	}

	private void txtCatMacro_TextChanged(object sender, EventArgs e)
	{
		ScriptResult scriptResult = _si.run(txtCatMacro.Text);
		if (!scriptResult.is_valid)
		{
			lblErrorText.Text = scriptResult.error_message;
			txtTokens.Text = "";
		}
		else
		{
			string text = "";
			int num = 1;
			foreach (ScriptCommand command in scriptResult.commands)
			{
				text = text + $"{num}) " + command.text + Environment.NewLine;
				num++;
			}
			txtTokens.Text = text;
		}
		lblErrorText.Visible = !scriptResult.is_valid;
		picError.Visible = !scriptResult.is_valid;
		_settings.CatMacro = txtCatMacro.Text;
	}

	private void chkCatSend_CheckedChanged(object sender, EventArgs e)
	{
		int indexFromName = getIndexFromName(sender);
		if (indexFromName != -1)
		{
			_settings.CatMacroSend[indexFromName] = (sender as CheckBoxTS).Checked;
		}
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
	}

	private void btnOK_Click(object sender, EventArgs e)
	{
	}

	private void btnCatTest_Click(object sender, EventArgs e)
	{
		if (_cat_tester == null || _cat_tester.IsDisposed)
		{
			_cat_tester = new CATTester(_console);
		}
		_cat_tester.Show();
		_cat_tester.Focus();
	}

	private void chkRunStateCommandOnVisible_CheckedChanged(object sender, EventArgs e)
	{
		_settings.RunStateCommandOnVisible = chkRunStateCommandOnVisible.Checked;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.frmMacroButtonConfig));
		this.groupBoxTS3 = new System.Windows.Forms.GroupBoxTS();
		this.btnCatTest = new System.Windows.Forms.ButtonTS();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.chkCatSend_1 = new System.Windows.Forms.CheckBoxTS();
		this.txtTokens = new System.Windows.Forms.TextBoxTS();
		this.lblErrorText = new System.Windows.Forms.LabelTS();
		this.picError = new System.Windows.Forms.PictureBox();
		this.txtCatMacro = new System.Windows.Forms.TextBoxTS();
		this.btnCancel = new System.Windows.Forms.ButtonTS();
		this.btnOK = new System.Windows.Forms.ButtonTS();
		this.groupBoxTS2 = new System.Windows.Forms.GroupBoxTS();
		this.labelTS8 = new System.Windows.Forms.LabelTS();
		this.txtMMIO_message_off_4 = new System.Windows.Forms.TextBoxTS();
		this.txtMMIO_message_off_3 = new System.Windows.Forms.TextBoxTS();
		this.txtMMIO_message_off_2 = new System.Windows.Forms.TextBoxTS();
		this.txtMMIO_message_off_1 = new System.Windows.Forms.TextBoxTS();
		this.txtMMIO_message_4 = new System.Windows.Forms.TextBoxTS();
		this.txtMMIO_4char_4 = new System.Windows.Forms.TextBoxTS();
		this.chkSendMssageViaMMIO_4 = new System.Windows.Forms.CheckBoxTS();
		this.txtMMIO_message_3 = new System.Windows.Forms.TextBoxTS();
		this.txtMMIO_4char_3 = new System.Windows.Forms.TextBoxTS();
		this.chkSendMssageViaMMIO_3 = new System.Windows.Forms.CheckBoxTS();
		this.txtMMIO_message_2 = new System.Windows.Forms.TextBoxTS();
		this.txtMMIO_4char_2 = new System.Windows.Forms.TextBoxTS();
		this.chkSendMssageViaMMIO_2 = new System.Windows.Forms.CheckBoxTS();
		this.labelTS5 = new System.Windows.Forms.LabelTS();
		this.labelTS4 = new System.Windows.Forms.LabelTS();
		this.txtMMIO_message_1 = new System.Windows.Forms.TextBoxTS();
		this.txtMMIO_4char_1 = new System.Windows.Forms.TextBoxTS();
		this.chkSendMssageViaMMIO_1 = new System.Windows.Forms.CheckBoxTS();
		this.labelTS3 = new System.Windows.Forms.LabelTS();
		this.txtNotes = new System.Windows.Forms.TextBoxTS();
		this.groupBoxTS1 = new System.Windows.Forms.GroupBoxTS();
		this.chkRunStateCommandOnVisible = new System.Windows.Forms.CheckBoxTS();
		this.labelTS7 = new System.Windows.Forms.LabelTS();
		this.txtButtonState_cat_on_reply = new System.Windows.Forms.TextBoxTS();
		this.radButtonState_catstate = new System.Windows.Forms.RadioButtonTS();
		this.labelTS6 = new System.Windows.Forms.LabelTS();
		this.radButtonState_off = new System.Windows.Forms.RadioButtonTS();
		this.txtButtonState_led_4char = new System.Windows.Forms.TextBoxTS();
		this.radButtonState_on = new System.Windows.Forms.RadioButtonTS();
		this.comboButtonState_container_visibility = new System.Windows.Forms.ComboBoxTS();
		this.radButtonState_container_visible = new System.Windows.Forms.RadioButtonTS();
		this.radButtonState_toggle = new System.Windows.Forms.RadioButtonTS();
		this.radButtonState_led = new System.Windows.Forms.RadioButtonTS();
		this.chkUseParent_loc_size_4 = new System.Windows.Forms.CheckBoxTS();
		this.comboOpenContainer_4 = new System.Windows.Forms.ComboBoxTS();
		this.chkOpensContainer_4 = new System.Windows.Forms.CheckBoxTS();
		this.comboCloseContainer_4 = new System.Windows.Forms.ComboBoxTS();
		this.chkClosesContainer_4 = new System.Windows.Forms.CheckBoxTS();
		this.chkUseParent_loc_size_3 = new System.Windows.Forms.CheckBoxTS();
		this.comboOpenContainer_3 = new System.Windows.Forms.ComboBoxTS();
		this.chkOpensContainer_3 = new System.Windows.Forms.CheckBoxTS();
		this.comboCloseContainer_3 = new System.Windows.Forms.ComboBoxTS();
		this.chkClosesContainer_3 = new System.Windows.Forms.CheckBoxTS();
		this.chkUseParent_loc_size_2 = new System.Windows.Forms.CheckBoxTS();
		this.comboOpenContainer_2 = new System.Windows.Forms.ComboBoxTS();
		this.chkOpensContainer_2 = new System.Windows.Forms.CheckBoxTS();
		this.comboCloseContainer_2 = new System.Windows.Forms.ComboBoxTS();
		this.chkClosesContainer_2 = new System.Windows.Forms.CheckBoxTS();
		this.chkUseParent_loc_size_1 = new System.Windows.Forms.CheckBoxTS();
		this.comboOpenContainer_1 = new System.Windows.Forms.ComboBoxTS();
		this.chkOpensContainer_1 = new System.Windows.Forms.CheckBoxTS();
		this.comboCloseContainer_1 = new System.Windows.Forms.ComboBoxTS();
		this.chkClosesContainer_1 = new System.Windows.Forms.CheckBoxTS();
		this.chkClosesParent = new System.Windows.Forms.CheckBoxTS();
		this.txtOFF = new System.Windows.Forms.TextBoxTS();
		this.txtON = new System.Windows.Forms.TextBoxTS();
		this.labelTS2 = new System.Windows.Forms.LabelTS();
		this.labelTS1 = new System.Windows.Forms.LabelTS();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		this.tabControl1 = new System.Windows.Forms.TabControl();
		this.tpButtonState = new System.Windows.Forms.TabPage();
		this.tpMMIO = new System.Windows.Forms.TabPage();
		this.tpCAT = new System.Windows.Forms.TabPage();
		this.groupBoxTS3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.picError).BeginInit();
		this.groupBoxTS2.SuspendLayout();
		this.groupBoxTS1.SuspendLayout();
		this.tabControl1.SuspendLayout();
		this.tpButtonState.SuspendLayout();
		this.tpMMIO.SuspendLayout();
		this.tpCAT.SuspendLayout();
		base.SuspendLayout();
		this.groupBoxTS3.Controls.Add(this.btnCatTest);
		this.groupBoxTS3.Controls.Add(this.pictureBox1);
		this.groupBoxTS3.Controls.Add(this.chkCatSend_1);
		this.groupBoxTS3.Controls.Add(this.txtTokens);
		this.groupBoxTS3.Controls.Add(this.lblErrorText);
		this.groupBoxTS3.Controls.Add(this.picError);
		this.groupBoxTS3.Controls.Add(this.txtCatMacro);
		this.groupBoxTS3.Location = new System.Drawing.Point(6, 6);
		this.groupBoxTS3.Name = "groupBoxTS3";
		this.groupBoxTS3.Size = new System.Drawing.Size(761, 303);
		this.groupBoxTS3.TabIndex = 37;
		this.groupBoxTS3.TabStop = false;
		this.groupBoxTS3.Text = "CAT Macro";
		this.btnCatTest.Image = null;
		this.btnCatTest.Location = new System.Drawing.Point(291, 19);
		this.btnCatTest.Name = "btnCatTest";
		this.btnCatTest.Selectable = true;
		this.btnCatTest.Size = new System.Drawing.Size(75, 23);
		this.btnCatTest.TabIndex = 40;
		this.btnCatTest.Text = "Cat Tester";
		this.toolTip1.SetToolTip(this.btnCatTest, "Show the cat tester");
		this.btnCatTest.UseVisualStyleBackColor = true;
		this.btnCatTest.Click += new System.EventHandler(btnCatTest_Click);
		this.pictureBox1.Image = Thetis.Properties.Resources.info;
		this.pictureBox1.Location = new System.Drawing.Point(379, 12);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(32, 32);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
		this.pictureBox1.TabIndex = 38;
		this.pictureBox1.TabStop = false;
		this.toolTip1.SetToolTip(this.pictureBox1, resources.GetString("pictureBox1.ToolTip"));
		this.chkCatSend_1.AutoSize = true;
		this.chkCatSend_1.Image = null;
		this.chkCatSend_1.Location = new System.Drawing.Point(24, 28);
		this.chkCatSend_1.Name = "chkCatSend_1";
		this.chkCatSend_1.Size = new System.Drawing.Size(98, 17);
		this.chkCatSend_1.TabIndex = 39;
		this.chkCatSend_1.Text = "Run Cat Macro";
		this.chkCatSend_1.UseVisualStyleBackColor = true;
		this.chkCatSend_1.CheckedChanged += new System.EventHandler(chkCatSend_CheckedChanged);
		this.txtTokens.BackColor = System.Drawing.Color.Black;
		this.txtTokens.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txtTokens.ForeColor = System.Drawing.Color.Lime;
		this.txtTokens.Location = new System.Drawing.Point(532, 51);
		this.txtTokens.Multiline = true;
		this.txtTokens.Name = "txtTokens";
		this.txtTokens.ReadOnly = true;
		this.txtTokens.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.txtTokens.Size = new System.Drawing.Size(213, 243);
		this.txtTokens.TabIndex = 38;
		this.lblErrorText.AutoSize = true;
		this.lblErrorText.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lblErrorText.Image = null;
		this.lblErrorText.Location = new System.Drawing.Point(459, 24);
		this.lblErrorText.Name = "lblErrorText";
		this.lblErrorText.Size = new System.Drawing.Size(44, 20);
		this.lblErrorText.TabIndex = 37;
		this.lblErrorText.Text = "Error";
		this.picError.Image = Thetis.Properties.Resources.warning4;
		this.picError.Location = new System.Drawing.Point(421, 13);
		this.picError.Name = "picError";
		this.picError.Size = new System.Drawing.Size(32, 32);
		this.picError.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
		this.picError.TabIndex = 36;
		this.picError.TabStop = false;
		this.txtCatMacro.BackColor = System.Drawing.Color.Black;
		this.txtCatMacro.Font = new System.Drawing.Font("Courier New", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.txtCatMacro.ForeColor = System.Drawing.Color.Lime;
		this.txtCatMacro.Location = new System.Drawing.Point(20, 51);
		this.txtCatMacro.Multiline = true;
		this.txtCatMacro.Name = "txtCatMacro";
		this.txtCatMacro.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.txtCatMacro.Size = new System.Drawing.Size(499, 243);
		this.txtCatMacro.TabIndex = 35;
		this.txtCatMacro.Text = resources.GetString("txtCatMacro.Text");
		this.txtCatMacro.TextChanged += new System.EventHandler(txtCatMacro_TextChanged);
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Image = null;
		this.btnCancel.Location = new System.Drawing.Point(699, 577);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Selectable = true;
		this.btnCancel.Size = new System.Drawing.Size(95, 50);
		this.btnCancel.TabIndex = 38;
		this.btnCancel.Text = "&Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOK.Image = null;
		this.btnOK.Location = new System.Drawing.Point(595, 577);
		this.btnOK.Name = "btnOK";
		this.btnOK.Selectable = true;
		this.btnOK.Size = new System.Drawing.Size(98, 50);
		this.btnOK.TabIndex = 33;
		this.btnOK.Text = "&OK";
		this.btnOK.UseVisualStyleBackColor = true;
		this.btnOK.Click += new System.EventHandler(btnOK_Click);
		this.groupBoxTS2.Controls.Add(this.labelTS8);
		this.groupBoxTS2.Controls.Add(this.txtMMIO_message_off_4);
		this.groupBoxTS2.Controls.Add(this.txtMMIO_message_off_3);
		this.groupBoxTS2.Controls.Add(this.txtMMIO_message_off_2);
		this.groupBoxTS2.Controls.Add(this.txtMMIO_message_off_1);
		this.groupBoxTS2.Controls.Add(this.txtMMIO_message_4);
		this.groupBoxTS2.Controls.Add(this.txtMMIO_4char_4);
		this.groupBoxTS2.Controls.Add(this.chkSendMssageViaMMIO_4);
		this.groupBoxTS2.Controls.Add(this.txtMMIO_message_3);
		this.groupBoxTS2.Controls.Add(this.txtMMIO_4char_3);
		this.groupBoxTS2.Controls.Add(this.chkSendMssageViaMMIO_3);
		this.groupBoxTS2.Controls.Add(this.txtMMIO_message_2);
		this.groupBoxTS2.Controls.Add(this.txtMMIO_4char_2);
		this.groupBoxTS2.Controls.Add(this.chkSendMssageViaMMIO_2);
		this.groupBoxTS2.Controls.Add(this.labelTS5);
		this.groupBoxTS2.Controls.Add(this.labelTS4);
		this.groupBoxTS2.Controls.Add(this.txtMMIO_message_1);
		this.groupBoxTS2.Controls.Add(this.txtMMIO_4char_1);
		this.groupBoxTS2.Controls.Add(this.chkSendMssageViaMMIO_1);
		this.groupBoxTS2.Location = new System.Drawing.Point(6, 6);
		this.groupBoxTS2.Name = "groupBoxTS2";
		this.groupBoxTS2.Size = new System.Drawing.Size(751, 135);
		this.groupBoxTS2.TabIndex = 36;
		this.groupBoxTS2.TabStop = false;
		this.groupBoxTS2.Text = "MMIO Actions";
		this.labelTS8.AutoSize = true;
		this.labelTS8.Image = null;
		this.labelTS8.Location = new System.Drawing.Point(524, 11);
		this.labelTS8.Name = "labelTS8";
		this.labelTS8.Size = new System.Drawing.Size(112, 13);
		this.labelTS8.TabIndex = 37;
		this.labelTS8.Text = "OFF raw text message";
		this.txtMMIO_message_off_4.Location = new System.Drawing.Point(462, 105);
		this.txtMMIO_message_off_4.MaxLength = 255;
		this.txtMMIO_message_off_4.Name = "txtMMIO_message_off_4";
		this.txtMMIO_message_off_4.Size = new System.Drawing.Size(232, 20);
		this.txtMMIO_message_off_4.TabIndex = 48;
		this.txtMMIO_message_off_4.Text = "WWWWWWWWWWWWWWWWWWWW";
		this.txtMMIO_message_off_4.TextChanged += new System.EventHandler(txtMMIO_message_n_off_TextChanged);
		this.txtMMIO_message_off_3.Location = new System.Drawing.Point(462, 79);
		this.txtMMIO_message_off_3.MaxLength = 255;
		this.txtMMIO_message_off_3.Name = "txtMMIO_message_off_3";
		this.txtMMIO_message_off_3.Size = new System.Drawing.Size(232, 20);
		this.txtMMIO_message_off_3.TabIndex = 47;
		this.txtMMIO_message_off_3.Text = "%x4Kn.power%";
		this.txtMMIO_message_off_3.TextChanged += new System.EventHandler(txtMMIO_message_n_off_TextChanged);
		this.txtMMIO_message_off_2.Location = new System.Drawing.Point(462, 53);
		this.txtMMIO_message_off_2.MaxLength = 255;
		this.txtMMIO_message_off_2.Name = "txtMMIO_message_off_2";
		this.txtMMIO_message_off_2.Size = new System.Drawing.Size(232, 20);
		this.txtMMIO_message_off_2.TabIndex = 46;
		this.txtMMIO_message_off_2.Text = "%vfoa_freq%";
		this.txtMMIO_message_off_2.TextChanged += new System.EventHandler(txtMMIO_message_n_off_TextChanged);
		this.txtMMIO_message_off_1.Location = new System.Drawing.Point(462, 27);
		this.txtMMIO_message_off_1.MaxLength = 255;
		this.txtMMIO_message_off_1.Name = "txtMMIO_message_off_1";
		this.txtMMIO_message_off_1.Size = new System.Drawing.Size(232, 20);
		this.txtMMIO_message_off_1.TabIndex = 45;
		this.txtMMIO_message_off_1.Text = "Buton [ID] pressed. [STATE] [OLDSTATE]";
		this.txtMMIO_message_off_1.TextChanged += new System.EventHandler(txtMMIO_message_n_off_TextChanged);
		this.txtMMIO_message_4.Location = new System.Drawing.Point(224, 105);
		this.txtMMIO_message_4.MaxLength = 255;
		this.txtMMIO_message_4.Name = "txtMMIO_message_4";
		this.txtMMIO_message_4.Size = new System.Drawing.Size(232, 20);
		this.txtMMIO_message_4.TabIndex = 44;
		this.txtMMIO_message_4.Text = "WWWWWWWWWWWWWWWWWWWW";
		this.txtMMIO_message_4.TextChanged += new System.EventHandler(txtMMIO_message_n_TextChanged);
		this.txtMMIO_4char_4.Location = new System.Drawing.Point(146, 104);
		this.txtMMIO_4char_4.MaxLength = 4;
		this.txtMMIO_4char_4.Name = "txtMMIO_4char_4";
		this.txtMMIO_4char_4.Size = new System.Drawing.Size(59, 20);
		this.txtMMIO_4char_4.TabIndex = 43;
		this.txtMMIO_4char_4.Text = "WWWW";
		this.txtMMIO_4char_4.TextChanged += new System.EventHandler(txtMMIO_4char_n_TextChanged);
		this.chkSendMssageViaMMIO_4.AutoSize = true;
		this.chkSendMssageViaMMIO_4.Image = null;
		this.chkSendMssageViaMMIO_4.Location = new System.Drawing.Point(27, 107);
		this.chkSendMssageViaMMIO_4.Name = "chkSendMssageViaMMIO_4";
		this.chkSendMssageViaMMIO_4.Size = new System.Drawing.Size(113, 17);
		this.chkSendMssageViaMMIO_4.TabIndex = 42;
		this.chkSendMssageViaMMIO_4.Text = "Send message via";
		this.chkSendMssageViaMMIO_4.UseVisualStyleBackColor = true;
		this.chkSendMssageViaMMIO_4.CheckedChanged += new System.EventHandler(chkSendMssageViaMMIO_n_CheckedChanged);
		this.txtMMIO_message_3.Location = new System.Drawing.Point(224, 79);
		this.txtMMIO_message_3.MaxLength = 255;
		this.txtMMIO_message_3.Name = "txtMMIO_message_3";
		this.txtMMIO_message_3.Size = new System.Drawing.Size(232, 20);
		this.txtMMIO_message_3.TabIndex = 41;
		this.txtMMIO_message_3.Text = "%x4Kn.power%";
		this.txtMMIO_message_3.TextChanged += new System.EventHandler(txtMMIO_message_n_TextChanged);
		this.txtMMIO_4char_3.Location = new System.Drawing.Point(146, 78);
		this.txtMMIO_4char_3.MaxLength = 4;
		this.txtMMIO_4char_3.Name = "txtMMIO_4char_3";
		this.txtMMIO_4char_3.Size = new System.Drawing.Size(59, 20);
		this.txtMMIO_4char_3.TabIndex = 40;
		this.txtMMIO_4char_3.Text = "WWWW";
		this.txtMMIO_4char_3.TextChanged += new System.EventHandler(txtMMIO_4char_n_TextChanged);
		this.chkSendMssageViaMMIO_3.AutoSize = true;
		this.chkSendMssageViaMMIO_3.Image = null;
		this.chkSendMssageViaMMIO_3.Location = new System.Drawing.Point(27, 81);
		this.chkSendMssageViaMMIO_3.Name = "chkSendMssageViaMMIO_3";
		this.chkSendMssageViaMMIO_3.Size = new System.Drawing.Size(113, 17);
		this.chkSendMssageViaMMIO_3.TabIndex = 39;
		this.chkSendMssageViaMMIO_3.Text = "Send message via";
		this.chkSendMssageViaMMIO_3.UseVisualStyleBackColor = true;
		this.chkSendMssageViaMMIO_3.CheckedChanged += new System.EventHandler(chkSendMssageViaMMIO_n_CheckedChanged);
		this.txtMMIO_message_2.Location = new System.Drawing.Point(224, 53);
		this.txtMMIO_message_2.MaxLength = 255;
		this.txtMMIO_message_2.Name = "txtMMIO_message_2";
		this.txtMMIO_message_2.Size = new System.Drawing.Size(232, 20);
		this.txtMMIO_message_2.TabIndex = 38;
		this.txtMMIO_message_2.Text = "%vfoa_freq%";
		this.txtMMIO_message_2.TextChanged += new System.EventHandler(txtMMIO_message_n_TextChanged);
		this.txtMMIO_4char_2.Location = new System.Drawing.Point(146, 52);
		this.txtMMIO_4char_2.MaxLength = 4;
		this.txtMMIO_4char_2.Name = "txtMMIO_4char_2";
		this.txtMMIO_4char_2.Size = new System.Drawing.Size(59, 20);
		this.txtMMIO_4char_2.TabIndex = 37;
		this.txtMMIO_4char_2.Text = "WWWW";
		this.txtMMIO_4char_2.TextChanged += new System.EventHandler(txtMMIO_4char_n_TextChanged);
		this.chkSendMssageViaMMIO_2.AutoSize = true;
		this.chkSendMssageViaMMIO_2.Image = null;
		this.chkSendMssageViaMMIO_2.Location = new System.Drawing.Point(27, 55);
		this.chkSendMssageViaMMIO_2.Name = "chkSendMssageViaMMIO_2";
		this.chkSendMssageViaMMIO_2.Size = new System.Drawing.Size(113, 17);
		this.chkSendMssageViaMMIO_2.TabIndex = 36;
		this.chkSendMssageViaMMIO_2.Text = "Send message via";
		this.chkSendMssageViaMMIO_2.UseVisualStyleBackColor = true;
		this.chkSendMssageViaMMIO_2.CheckedChanged += new System.EventHandler(chkSendMssageViaMMIO_n_CheckedChanged);
		this.labelTS5.AutoSize = true;
		this.labelTS5.Image = null;
		this.labelTS5.Location = new System.Drawing.Point(156, 10);
		this.labelTS5.Name = "labelTS5";
		this.labelTS5.Size = new System.Drawing.Size(34, 13);
		this.labelTS5.TabIndex = 35;
		this.labelTS5.Text = "4char";
		this.labelTS4.AutoSize = true;
		this.labelTS4.Image = null;
		this.labelTS4.Location = new System.Drawing.Point(280, 11);
		this.labelTS4.Name = "labelTS4";
		this.labelTS4.Size = new System.Drawing.Size(108, 13);
		this.labelTS4.TabIndex = 34;
		this.labelTS4.Text = "ON raw text message";
		this.txtMMIO_message_1.Location = new System.Drawing.Point(224, 27);
		this.txtMMIO_message_1.MaxLength = 255;
		this.txtMMIO_message_1.Name = "txtMMIO_message_1";
		this.txtMMIO_message_1.Size = new System.Drawing.Size(232, 20);
		this.txtMMIO_message_1.TabIndex = 33;
		this.txtMMIO_message_1.Text = "Buton [ID] pressed. [STATE] [OLDSTATE]";
		this.txtMMIO_message_1.TextChanged += new System.EventHandler(txtMMIO_message_n_TextChanged);
		this.txtMMIO_4char_1.Location = new System.Drawing.Point(146, 26);
		this.txtMMIO_4char_1.MaxLength = 4;
		this.txtMMIO_4char_1.Name = "txtMMIO_4char_1";
		this.txtMMIO_4char_1.Size = new System.Drawing.Size(59, 20);
		this.txtMMIO_4char_1.TabIndex = 32;
		this.txtMMIO_4char_1.Text = "WWWW";
		this.txtMMIO_4char_1.TextChanged += new System.EventHandler(txtMMIO_4char_n_TextChanged);
		this.chkSendMssageViaMMIO_1.AutoSize = true;
		this.chkSendMssageViaMMIO_1.Image = null;
		this.chkSendMssageViaMMIO_1.Location = new System.Drawing.Point(27, 29);
		this.chkSendMssageViaMMIO_1.Name = "chkSendMssageViaMMIO_1";
		this.chkSendMssageViaMMIO_1.Size = new System.Drawing.Size(113, 17);
		this.chkSendMssageViaMMIO_1.TabIndex = 0;
		this.chkSendMssageViaMMIO_1.Text = "Send message via";
		this.chkSendMssageViaMMIO_1.UseVisualStyleBackColor = true;
		this.chkSendMssageViaMMIO_1.CheckedChanged += new System.EventHandler(chkSendMssageViaMMIO_n_CheckedChanged);
		this.labelTS3.AutoSize = true;
		this.labelTS3.Image = null;
		this.labelTS3.Location = new System.Drawing.Point(366, 16);
		this.labelTS3.Name = "labelTS3";
		this.labelTS3.Size = new System.Drawing.Size(38, 13);
		this.labelTS3.TabIndex = 35;
		this.labelTS3.Text = "Notes:";
		this.txtNotes.Location = new System.Drawing.Point(410, 13);
		this.txtNotes.Multiline = true;
		this.txtNotes.Name = "txtNotes";
		this.txtNotes.Size = new System.Drawing.Size(346, 49);
		this.txtNotes.TabIndex = 34;
		this.txtNotes.TextChanged += new System.EventHandler(txtNotes_TextChanged);
		this.groupBoxTS1.BackColor = System.Drawing.Color.Transparent;
		this.groupBoxTS1.Controls.Add(this.chkRunStateCommandOnVisible);
		this.groupBoxTS1.Controls.Add(this.labelTS7);
		this.groupBoxTS1.Controls.Add(this.txtButtonState_cat_on_reply);
		this.groupBoxTS1.Controls.Add(this.radButtonState_catstate);
		this.groupBoxTS1.Controls.Add(this.labelTS6);
		this.groupBoxTS1.Controls.Add(this.radButtonState_off);
		this.groupBoxTS1.Controls.Add(this.txtButtonState_led_4char);
		this.groupBoxTS1.Controls.Add(this.radButtonState_on);
		this.groupBoxTS1.Controls.Add(this.comboButtonState_container_visibility);
		this.groupBoxTS1.Controls.Add(this.radButtonState_container_visible);
		this.groupBoxTS1.Controls.Add(this.radButtonState_toggle);
		this.groupBoxTS1.Controls.Add(this.radButtonState_led);
		this.groupBoxTS1.Location = new System.Drawing.Point(10, 12);
		this.groupBoxTS1.Name = "groupBoxTS1";
		this.groupBoxTS1.Size = new System.Drawing.Size(362, 227);
		this.groupBoxTS1.TabIndex = 32;
		this.groupBoxTS1.TabStop = false;
		this.groupBoxTS1.Text = "Button State";
		this.chkRunStateCommandOnVisible.AutoSize = true;
		this.chkRunStateCommandOnVisible.Image = null;
		this.chkRunStateCommandOnVisible.Location = new System.Drawing.Point(67, 187);
		this.chkRunStateCommandOnVisible.Name = "chkRunStateCommandOnVisible";
		this.chkRunStateCommandOnVisible.Size = new System.Drawing.Size(282, 17);
		this.chkRunStateCommandOnVisible.TabIndex = 39;
		this.chkRunStateCommandOnVisible.Text = "Run state cmd when parent container becomes visible";
		this.toolTip1.SetToolTip(this.chkRunStateCommandOnVisible, "Run the state command, when the parent container becomes visible.\r\nThis ensures button state accuracy.");
		this.chkRunStateCommandOnVisible.UseVisualStyleBackColor = true;
		this.chkRunStateCommandOnVisible.CheckedChanged += new System.EventHandler(chkRunStateCommandOnVisible_CheckedChanged);
		this.labelTS7.AutoSize = true;
		this.labelTS7.Image = null;
		this.labelTS7.Location = new System.Drawing.Point(32, 164);
		this.labelTS7.Name = "labelTS7";
		this.labelTS7.Size = new System.Drawing.Size(29, 13);
		this.labelTS7.TabIndex = 39;
		this.labelTS7.Text = "ON=";
		this.txtButtonState_cat_on_reply.Location = new System.Drawing.Point(67, 161);
		this.txtButtonState_cat_on_reply.MaxLength = 20;
		this.txtButtonState_cat_on_reply.Name = "txtButtonState_cat_on_reply";
		this.txtButtonState_cat_on_reply.Size = new System.Drawing.Size(200, 20);
		this.txtButtonState_cat_on_reply.TabIndex = 38;
		this.txtButtonState_cat_on_reply.Text = "PS1;";
		this.txtButtonState_cat_on_reply.TextChanged += new System.EventHandler(txtButtonState_cat_on_reply_TextChanged);
		this.radButtonState_catstate.AutoSize = true;
		this.radButtonState_catstate.Image = null;
		this.radButtonState_catstate.Location = new System.Drawing.Point(17, 138);
		this.radButtonState_catstate.Name = "radButtonState_catstate";
		this.radButtonState_catstate.Size = new System.Drawing.Size(200, 17);
		this.radButtonState_catstate.TabIndex = 37;
		this.radButtonState_catstate.TabStop = true;
		this.radButtonState_catstate.Text = "CAT   (eg. use [STATE]PS; in macro)";
		this.radButtonState_catstate.UseVisualStyleBackColor = true;
		this.radButtonState_catstate.CheckedChanged += new System.EventHandler(radButtonState_n_CheckedChanged);
		this.labelTS6.AutoSize = true;
		this.labelTS6.Image = null;
		this.labelTS6.Location = new System.Drawing.Point(175, 92);
		this.labelTS6.Name = "labelTS6";
		this.labelTS6.Size = new System.Drawing.Size(34, 13);
		this.labelTS6.TabIndex = 36;
		this.labelTS6.Text = "4char";
		this.radButtonState_off.AutoSize = true;
		this.radButtonState_off.Image = null;
		this.radButtonState_off.Location = new System.Drawing.Point(17, 46);
		this.radButtonState_off.Name = "radButtonState_off";
		this.radButtonState_off.Size = new System.Drawing.Size(39, 17);
		this.radButtonState_off.TabIndex = 25;
		this.radButtonState_off.TabStop = true;
		this.radButtonState_off.Text = "Off";
		this.radButtonState_off.UseVisualStyleBackColor = true;
		this.radButtonState_off.CheckedChanged += new System.EventHandler(radButtonState_n_CheckedChanged);
		this.txtButtonState_led_4char.Location = new System.Drawing.Point(110, 89);
		this.txtButtonState_led_4char.MaxLength = 4;
		this.txtButtonState_led_4char.Name = "txtButtonState_led_4char";
		this.txtButtonState_led_4char.Size = new System.Drawing.Size(59, 20);
		this.txtButtonState_led_4char.TabIndex = 31;
		this.txtButtonState_led_4char.Text = "WWWW";
		this.txtButtonState_led_4char.TextChanged += new System.EventHandler(txtButtonState_led_4char_TextChanged);
		this.radButtonState_on.AutoSize = true;
		this.radButtonState_on.Image = null;
		this.radButtonState_on.Location = new System.Drawing.Point(17, 23);
		this.radButtonState_on.Name = "radButtonState_on";
		this.radButtonState_on.Size = new System.Drawing.Size(39, 17);
		this.radButtonState_on.TabIndex = 26;
		this.radButtonState_on.TabStop = true;
		this.radButtonState_on.Text = "On";
		this.radButtonState_on.UseVisualStyleBackColor = true;
		this.radButtonState_on.CheckedChanged += new System.EventHandler(radButtonState_n_CheckedChanged);
		this.comboButtonState_container_visibility.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboButtonState_container_visibility.FormattingEnabled = true;
		this.comboButtonState_container_visibility.Location = new System.Drawing.Point(129, 114);
		this.comboButtonState_container_visibility.Name = "comboButtonState_container_visibility";
		this.comboButtonState_container_visibility.Size = new System.Drawing.Size(140, 21);
		this.comboButtonState_container_visibility.TabIndex = 30;
		this.comboButtonState_container_visibility.SelectedIndexChanged += new System.EventHandler(comboButtonState_container_visibility_SelectedIndexChanged);
		this.radButtonState_container_visible.AutoSize = true;
		this.radButtonState_container_visible.Image = null;
		this.radButtonState_container_visible.Location = new System.Drawing.Point(17, 115);
		this.radButtonState_container_visible.Name = "radButtonState_container_visible";
		this.radButtonState_container_visible.Size = new System.Drawing.Size(106, 17);
		this.radButtonState_container_visible.TabIndex = 29;
		this.radButtonState_container_visible.TabStop = true;
		this.radButtonState_container_visible.Text = "Container visibilty";
		this.radButtonState_container_visible.UseVisualStyleBackColor = true;
		this.radButtonState_container_visible.CheckedChanged += new System.EventHandler(radButtonState_n_CheckedChanged);
		this.radButtonState_toggle.AutoSize = true;
		this.radButtonState_toggle.Image = null;
		this.radButtonState_toggle.Location = new System.Drawing.Point(17, 69);
		this.radButtonState_toggle.Name = "radButtonState_toggle";
		this.radButtonState_toggle.Size = new System.Drawing.Size(58, 17);
		this.radButtonState_toggle.TabIndex = 28;
		this.radButtonState_toggle.TabStop = true;
		this.radButtonState_toggle.Text = "Toggle";
		this.radButtonState_toggle.UseVisualStyleBackColor = true;
		this.radButtonState_toggle.CheckedChanged += new System.EventHandler(radButtonState_n_CheckedChanged);
		this.radButtonState_led.AutoSize = true;
		this.radButtonState_led.Image = null;
		this.radButtonState_led.Location = new System.Drawing.Point(17, 92);
		this.radButtonState_led.Name = "radButtonState_led";
		this.radButtonState_led.Size = new System.Drawing.Size(87, 17);
		this.radButtonState_led.TabIndex = 27;
		this.radButtonState_led.TabStop = true;
		this.radButtonState_led.Text = "Led Indicator";
		this.radButtonState_led.UseVisualStyleBackColor = true;
		this.radButtonState_led.CheckedChanged += new System.EventHandler(radButtonState_n_CheckedChanged);
		this.chkUseParent_loc_size_4.AutoSize = true;
		this.chkUseParent_loc_size_4.Image = null;
		this.chkUseParent_loc_size_4.Location = new System.Drawing.Point(551, 178);
		this.chkUseParent_loc_size_4.Name = "chkUseParent_loc_size_4";
		this.chkUseParent_loc_size_4.Size = new System.Drawing.Size(209, 17);
		this.chkUseParent_loc_size_4.TabIndex = 24;
		this.chkUseParent_loc_size_4.Text = "use closed container for location + size";
		this.chkUseParent_loc_size_4.UseVisualStyleBackColor = true;
		this.chkUseParent_loc_size_4.CheckedChanged += new System.EventHandler(chkUseParentCoodsForOpen_n_CheckedChanged);
		this.comboOpenContainer_4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboOpenContainer_4.FormattingEnabled = true;
		this.comboOpenContainer_4.Location = new System.Drawing.Point(394, 176);
		this.comboOpenContainer_4.Name = "comboOpenContainer_4";
		this.comboOpenContainer_4.Size = new System.Drawing.Size(140, 21);
		this.comboOpenContainer_4.TabIndex = 23;
		this.comboOpenContainer_4.SelectedIndexChanged += new System.EventHandler(comboOpenContainer_n_SelectedIndexChanged);
		this.chkOpensContainer_4.AutoSize = true;
		this.chkOpensContainer_4.Image = null;
		this.chkOpensContainer_4.Location = new System.Drawing.Point(284, 178);
		this.chkOpensContainer_4.Name = "chkOpensContainer_4";
		this.chkOpensContainer_4.Size = new System.Drawing.Size(104, 17);
		this.chkOpensContainer_4.TabIndex = 22;
		this.chkOpensContainer_4.Text = "Opens container";
		this.chkOpensContainer_4.UseVisualStyleBackColor = true;
		this.chkOpensContainer_4.CheckedChanged += new System.EventHandler(chkOpensContainer_n_CheckedChanged);
		this.comboCloseContainer_4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboCloseContainer_4.FormattingEnabled = true;
		this.comboCloseContainer_4.Location = new System.Drawing.Point(125, 176);
		this.comboCloseContainer_4.Name = "comboCloseContainer_4";
		this.comboCloseContainer_4.Size = new System.Drawing.Size(140, 21);
		this.comboCloseContainer_4.TabIndex = 21;
		this.comboCloseContainer_4.SelectedIndexChanged += new System.EventHandler(comboCloseContainer_n_SelectedIndexChanged);
		this.chkClosesContainer_4.AutoSize = true;
		this.chkClosesContainer_4.Image = null;
		this.chkClosesContainer_4.Location = new System.Drawing.Point(15, 178);
		this.chkClosesContainer_4.Name = "chkClosesContainer_4";
		this.chkClosesContainer_4.Size = new System.Drawing.Size(104, 17);
		this.chkClosesContainer_4.TabIndex = 20;
		this.chkClosesContainer_4.Text = "Closes container";
		this.chkClosesContainer_4.UseVisualStyleBackColor = true;
		this.chkClosesContainer_4.CheckedChanged += new System.EventHandler(chkClosesContainer_n_CheckedChanged);
		this.chkUseParent_loc_size_3.AutoSize = true;
		this.chkUseParent_loc_size_3.Image = null;
		this.chkUseParent_loc_size_3.Location = new System.Drawing.Point(551, 151);
		this.chkUseParent_loc_size_3.Name = "chkUseParent_loc_size_3";
		this.chkUseParent_loc_size_3.Size = new System.Drawing.Size(209, 17);
		this.chkUseParent_loc_size_3.TabIndex = 19;
		this.chkUseParent_loc_size_3.Text = "use closed container for location + size";
		this.chkUseParent_loc_size_3.UseVisualStyleBackColor = true;
		this.chkUseParent_loc_size_3.CheckedChanged += new System.EventHandler(chkUseParentCoodsForOpen_n_CheckedChanged);
		this.comboOpenContainer_3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboOpenContainer_3.FormattingEnabled = true;
		this.comboOpenContainer_3.Location = new System.Drawing.Point(394, 149);
		this.comboOpenContainer_3.Name = "comboOpenContainer_3";
		this.comboOpenContainer_3.Size = new System.Drawing.Size(140, 21);
		this.comboOpenContainer_3.TabIndex = 18;
		this.comboOpenContainer_3.SelectedIndexChanged += new System.EventHandler(comboOpenContainer_n_SelectedIndexChanged);
		this.chkOpensContainer_3.AutoSize = true;
		this.chkOpensContainer_3.Image = null;
		this.chkOpensContainer_3.Location = new System.Drawing.Point(284, 151);
		this.chkOpensContainer_3.Name = "chkOpensContainer_3";
		this.chkOpensContainer_3.Size = new System.Drawing.Size(104, 17);
		this.chkOpensContainer_3.TabIndex = 17;
		this.chkOpensContainer_3.Text = "Opens container";
		this.chkOpensContainer_3.UseVisualStyleBackColor = true;
		this.chkOpensContainer_3.CheckedChanged += new System.EventHandler(chkOpensContainer_n_CheckedChanged);
		this.comboCloseContainer_3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboCloseContainer_3.FormattingEnabled = true;
		this.comboCloseContainer_3.Location = new System.Drawing.Point(125, 149);
		this.comboCloseContainer_3.Name = "comboCloseContainer_3";
		this.comboCloseContainer_3.Size = new System.Drawing.Size(140, 21);
		this.comboCloseContainer_3.TabIndex = 16;
		this.comboCloseContainer_3.SelectedIndexChanged += new System.EventHandler(comboCloseContainer_n_SelectedIndexChanged);
		this.chkClosesContainer_3.AutoSize = true;
		this.chkClosesContainer_3.Image = null;
		this.chkClosesContainer_3.Location = new System.Drawing.Point(15, 151);
		this.chkClosesContainer_3.Name = "chkClosesContainer_3";
		this.chkClosesContainer_3.Size = new System.Drawing.Size(104, 17);
		this.chkClosesContainer_3.TabIndex = 15;
		this.chkClosesContainer_3.Text = "Closes container";
		this.chkClosesContainer_3.UseVisualStyleBackColor = true;
		this.chkClosesContainer_3.CheckedChanged += new System.EventHandler(chkClosesContainer_n_CheckedChanged);
		this.chkUseParent_loc_size_2.AutoSize = true;
		this.chkUseParent_loc_size_2.Image = null;
		this.chkUseParent_loc_size_2.Location = new System.Drawing.Point(551, 124);
		this.chkUseParent_loc_size_2.Name = "chkUseParent_loc_size_2";
		this.chkUseParent_loc_size_2.Size = new System.Drawing.Size(209, 17);
		this.chkUseParent_loc_size_2.TabIndex = 14;
		this.chkUseParent_loc_size_2.Text = "use closed container for location + size";
		this.chkUseParent_loc_size_2.UseVisualStyleBackColor = true;
		this.chkUseParent_loc_size_2.CheckedChanged += new System.EventHandler(chkUseParentCoodsForOpen_n_CheckedChanged);
		this.comboOpenContainer_2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboOpenContainer_2.FormattingEnabled = true;
		this.comboOpenContainer_2.Location = new System.Drawing.Point(394, 122);
		this.comboOpenContainer_2.Name = "comboOpenContainer_2";
		this.comboOpenContainer_2.Size = new System.Drawing.Size(140, 21);
		this.comboOpenContainer_2.TabIndex = 13;
		this.comboOpenContainer_2.SelectedIndexChanged += new System.EventHandler(comboOpenContainer_n_SelectedIndexChanged);
		this.chkOpensContainer_2.AutoSize = true;
		this.chkOpensContainer_2.Image = null;
		this.chkOpensContainer_2.Location = new System.Drawing.Point(284, 124);
		this.chkOpensContainer_2.Name = "chkOpensContainer_2";
		this.chkOpensContainer_2.Size = new System.Drawing.Size(104, 17);
		this.chkOpensContainer_2.TabIndex = 12;
		this.chkOpensContainer_2.Text = "Opens container";
		this.chkOpensContainer_2.UseVisualStyleBackColor = true;
		this.chkOpensContainer_2.CheckedChanged += new System.EventHandler(chkOpensContainer_n_CheckedChanged);
		this.comboCloseContainer_2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboCloseContainer_2.FormattingEnabled = true;
		this.comboCloseContainer_2.Location = new System.Drawing.Point(125, 122);
		this.comboCloseContainer_2.Name = "comboCloseContainer_2";
		this.comboCloseContainer_2.Size = new System.Drawing.Size(140, 21);
		this.comboCloseContainer_2.TabIndex = 11;
		this.comboCloseContainer_2.SelectedIndexChanged += new System.EventHandler(comboCloseContainer_n_SelectedIndexChanged);
		this.chkClosesContainer_2.AutoSize = true;
		this.chkClosesContainer_2.Image = null;
		this.chkClosesContainer_2.Location = new System.Drawing.Point(15, 124);
		this.chkClosesContainer_2.Name = "chkClosesContainer_2";
		this.chkClosesContainer_2.Size = new System.Drawing.Size(104, 17);
		this.chkClosesContainer_2.TabIndex = 10;
		this.chkClosesContainer_2.Text = "Closes container";
		this.chkClosesContainer_2.UseVisualStyleBackColor = true;
		this.chkClosesContainer_2.CheckedChanged += new System.EventHandler(chkClosesContainer_n_CheckedChanged);
		this.chkUseParent_loc_size_1.AutoSize = true;
		this.chkUseParent_loc_size_1.Image = null;
		this.chkUseParent_loc_size_1.Location = new System.Drawing.Point(551, 97);
		this.chkUseParent_loc_size_1.Name = "chkUseParent_loc_size_1";
		this.chkUseParent_loc_size_1.Size = new System.Drawing.Size(209, 17);
		this.chkUseParent_loc_size_1.TabIndex = 9;
		this.chkUseParent_loc_size_1.Text = "use closed container for location + size";
		this.chkUseParent_loc_size_1.UseVisualStyleBackColor = true;
		this.chkUseParent_loc_size_1.CheckedChanged += new System.EventHandler(chkUseParentCoodsForOpen_n_CheckedChanged);
		this.comboOpenContainer_1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboOpenContainer_1.FormattingEnabled = true;
		this.comboOpenContainer_1.Location = new System.Drawing.Point(394, 95);
		this.comboOpenContainer_1.Name = "comboOpenContainer_1";
		this.comboOpenContainer_1.Size = new System.Drawing.Size(140, 21);
		this.comboOpenContainer_1.TabIndex = 8;
		this.comboOpenContainer_1.SelectedIndexChanged += new System.EventHandler(comboOpenContainer_n_SelectedIndexChanged);
		this.chkOpensContainer_1.AutoSize = true;
		this.chkOpensContainer_1.Image = null;
		this.chkOpensContainer_1.Location = new System.Drawing.Point(284, 97);
		this.chkOpensContainer_1.Name = "chkOpensContainer_1";
		this.chkOpensContainer_1.Size = new System.Drawing.Size(104, 17);
		this.chkOpensContainer_1.TabIndex = 7;
		this.chkOpensContainer_1.Text = "Opens container";
		this.chkOpensContainer_1.UseVisualStyleBackColor = true;
		this.chkOpensContainer_1.CheckedChanged += new System.EventHandler(chkOpensContainer_n_CheckedChanged);
		this.comboCloseContainer_1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboCloseContainer_1.FormattingEnabled = true;
		this.comboCloseContainer_1.Location = new System.Drawing.Point(125, 95);
		this.comboCloseContainer_1.Name = "comboCloseContainer_1";
		this.comboCloseContainer_1.Size = new System.Drawing.Size(140, 21);
		this.comboCloseContainer_1.TabIndex = 6;
		this.comboCloseContainer_1.SelectedIndexChanged += new System.EventHandler(comboCloseContainer_n_SelectedIndexChanged);
		this.chkClosesContainer_1.AutoSize = true;
		this.chkClosesContainer_1.Image = null;
		this.chkClosesContainer_1.Location = new System.Drawing.Point(15, 97);
		this.chkClosesContainer_1.Name = "chkClosesContainer_1";
		this.chkClosesContainer_1.Size = new System.Drawing.Size(104, 17);
		this.chkClosesContainer_1.TabIndex = 5;
		this.chkClosesContainer_1.Text = "Closes container";
		this.chkClosesContainer_1.UseVisualStyleBackColor = true;
		this.chkClosesContainer_1.CheckedChanged += new System.EventHandler(chkClosesContainer_n_CheckedChanged);
		this.chkClosesParent.AutoSize = true;
		this.chkClosesParent.Image = null;
		this.chkClosesParent.Location = new System.Drawing.Point(15, 72);
		this.chkClosesParent.Name = "chkClosesParent";
		this.chkClosesParent.Size = new System.Drawing.Size(137, 17);
		this.chkClosesParent.TabIndex = 4;
		this.chkClosesParent.Text = "Closes parent container";
		this.chkClosesParent.UseVisualStyleBackColor = true;
		this.chkClosesParent.CheckedChanged += new System.EventHandler(chkClosesParent_CheckedChanged);
		this.txtOFF.Location = new System.Drawing.Point(94, 39);
		this.txtOFF.MaxLength = 40;
		this.txtOFF.Name = "txtOFF";
		this.txtOFF.Size = new System.Drawing.Size(232, 20);
		this.txtOFF.TabIndex = 3;
		this.txtOFF.Text = "WWWWWWWWWWWWWWWWWWWW";
		this.txtOFF.TextChanged += new System.EventHandler(txtOFF_TextChanged);
		this.txtON.Location = new System.Drawing.Point(94, 13);
		this.txtON.MaxLength = 40;
		this.txtON.Name = "txtON";
		this.txtON.Size = new System.Drawing.Size(232, 20);
		this.txtON.TabIndex = 2;
		this.txtON.TextChanged += new System.EventHandler(txtON_TextChanged);
		this.labelTS2.AutoSize = true;
		this.labelTS2.Image = null;
		this.labelTS2.Location = new System.Drawing.Point(12, 42);
		this.labelTS2.Name = "labelTS2";
		this.labelTS2.Size = new System.Drawing.Size(76, 13);
		this.labelTS2.TabIndex = 1;
		this.labelTS2.Text = "OFF state text:";
		this.labelTS1.AutoSize = true;
		this.labelTS1.Image = null;
		this.labelTS1.Location = new System.Drawing.Point(16, 16);
		this.labelTS1.Name = "labelTS1";
		this.labelTS1.Size = new System.Drawing.Size(72, 13);
		this.labelTS1.TabIndex = 0;
		this.labelTS1.Text = "ON state text:";
		this.tabControl1.Controls.Add(this.tpButtonState);
		this.tabControl1.Controls.Add(this.tpMMIO);
		this.tabControl1.Controls.Add(this.tpCAT);
		this.tabControl1.Location = new System.Drawing.Point(12, 211);
		this.tabControl1.Name = "tabControl1";
		this.tabControl1.SelectedIndex = 0;
		this.tabControl1.Size = new System.Drawing.Size(786, 364);
		this.tabControl1.TabIndex = 38;
		this.tpButtonState.BackColor = System.Drawing.SystemColors.Control;
		this.tpButtonState.Controls.Add(this.groupBoxTS1);
		this.tpButtonState.Location = new System.Drawing.Point(4, 22);
		this.tpButtonState.Name = "tpButtonState";
		this.tpButtonState.Size = new System.Drawing.Size(778, 338);
		this.tpButtonState.TabIndex = 2;
		this.tpButtonState.Text = "Button State";
		this.tpMMIO.BackColor = System.Drawing.SystemColors.Control;
		this.tpMMIO.Controls.Add(this.groupBoxTS2);
		this.tpMMIO.Location = new System.Drawing.Point(4, 22);
		this.tpMMIO.Name = "tpMMIO";
		this.tpMMIO.Padding = new System.Windows.Forms.Padding(3);
		this.tpMMIO.Size = new System.Drawing.Size(778, 338);
		this.tpMMIO.TabIndex = 0;
		this.tpMMIO.Text = "MMIO Actions";
		this.tpCAT.BackColor = System.Drawing.SystemColors.Control;
		this.tpCAT.Controls.Add(this.groupBoxTS3);
		this.tpCAT.Location = new System.Drawing.Point(4, 22);
		this.tpCAT.Name = "tpCAT";
		this.tpCAT.Padding = new System.Windows.Forms.Padding(3);
		this.tpCAT.Size = new System.Drawing.Size(778, 338);
		this.tpCAT.TabIndex = 1;
		this.tpCAT.Text = "CAT Macro";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(803, 633);
		base.ControlBox = false;
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.tabControl1);
		base.Controls.Add(this.btnOK);
		base.Controls.Add(this.labelTS3);
		base.Controls.Add(this.txtNotes);
		base.Controls.Add(this.chkUseParent_loc_size_4);
		base.Controls.Add(this.comboOpenContainer_4);
		base.Controls.Add(this.chkOpensContainer_4);
		base.Controls.Add(this.comboCloseContainer_4);
		base.Controls.Add(this.chkClosesContainer_4);
		base.Controls.Add(this.chkUseParent_loc_size_3);
		base.Controls.Add(this.comboOpenContainer_3);
		base.Controls.Add(this.chkOpensContainer_3);
		base.Controls.Add(this.comboCloseContainer_3);
		base.Controls.Add(this.chkClosesContainer_3);
		base.Controls.Add(this.chkUseParent_loc_size_2);
		base.Controls.Add(this.comboOpenContainer_2);
		base.Controls.Add(this.chkOpensContainer_2);
		base.Controls.Add(this.comboCloseContainer_2);
		base.Controls.Add(this.chkClosesContainer_2);
		base.Controls.Add(this.chkUseParent_loc_size_1);
		base.Controls.Add(this.comboOpenContainer_1);
		base.Controls.Add(this.chkOpensContainer_1);
		base.Controls.Add(this.comboCloseContainer_1);
		base.Controls.Add(this.chkClosesContainer_1);
		base.Controls.Add(this.chkClosesParent);
		base.Controls.Add(this.txtOFF);
		base.Controls.Add(this.txtON);
		base.Controls.Add(this.labelTS2);
		base.Controls.Add(this.labelTS1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmMacroButtonConfig";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Macro Button Configuration";
		this.groupBoxTS3.ResumeLayout(false);
		this.groupBoxTS3.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.picError).EndInit();
		this.groupBoxTS2.ResumeLayout(false);
		this.groupBoxTS2.PerformLayout();
		this.groupBoxTS1.ResumeLayout(false);
		this.groupBoxTS1.PerformLayout();
		this.tabControl1.ResumeLayout(false);
		this.tpButtonState.ResumeLayout(false);
		this.tpMMIO.ResumeLayout(false);
		this.tpCAT.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
