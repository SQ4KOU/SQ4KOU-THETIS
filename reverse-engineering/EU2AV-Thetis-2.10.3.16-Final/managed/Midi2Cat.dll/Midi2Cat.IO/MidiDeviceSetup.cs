using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Midi2Cat.Data;
using Midi2Cat.Helpers;

namespace Midi2Cat.IO;

public class MidiDeviceSetup : UserControl
{
	private const string MapCtrl2CmdMsg1 = "You must now give this control a name and then select one of the options in the \"Control Type\" dropdown box above.\n\nChoose the option that most closely matches the type of control you moved, pressed or slid.";

	private const string showAdvancedSettings = "Show Advanced Options";

	private const string hideAdvancedSettings = "Hide Advanced Options";

	private string DbFile;

	private bool AddingControl;

	private int AddingControlId = int.MinValue;

	private bool DialogValid;

	private EnumsDB _enumsDb;

	private Midi2CatDatabase _db;

	private MidiDevice midiDevice;

	private CatCmd CatCmdToUse;

	private ControlType ControlTypeToUse;

	private MidiDiagList _diagList;

	private DateTime ignoreMidiMessagesUntil = DateTime.Now.AddSeconds(5.0);

	private IContainer components;

	private Label label3;

	private Label label4;

	private Label midiOutStatusLabel;

	private Label midiInStatusLabel;

	private ListBox errorListBox;

	private TabControl tabControl;

	private TabPage debugTab;

	private TabPage mapInDialogTabPage;

	private Panel mapInCtrl2CmdPanel;

	private ListBox mappingAvailableCmdsLB;

	private Label mappingMessageLabel2;

	private Label mappingValueLabel2;

	private Label label14;

	private Button mappingBackButton2;

	private Button mappingDoneButton2;

	private TextBox mappingControlNameTB2;

	private Label label15;

	private Label label17;

	private Label mappingMinValueLabel2;

	private Label label19;

	private Label mappingMaxValueLabel2;

	private Label label21;

	private Label mappingControlIdLabel2;

	private Label label23;

	private ComboBox mappingControlTypeCB2;

	private Panel mappingPromptPanel2;

	private Label label12;

	private Label mappingPromptLabel2;

	private TabPage mappedControlsTab;

	private DataGridView mapControlToCommandGrid;

	private BindingSource controllerMappingBindingSource1;

	private BindingSource controlTypesBindingSource;

	private BindingSource catCmdsBindingSource;

	private DataGridViewComboBoxColumn dataGridViewComboBoxColumn1;

	private DataGridViewComboBoxColumn dataGridViewComboBoxColumn2;

	private DataGridViewComboBoxColumn dataGridViewComboBoxColumn3;

	private DataGridViewComboBoxColumn dataGridViewComboBoxColumn4;

	private BindingSource catCmdsFilteredBindingSource;

	private DataGridViewComboBoxColumn dataGridViewComboBoxColumn5;

	private DataGridViewComboBoxColumn dataGridViewComboBoxColumn6;

	private DataGridViewComboBoxColumn dataGridViewComboBoxColumn7;

	private Label label1;

	private Panel promptPanel;

	private Label label5;

	private Label label2;

	private TabPage commandsTabPage;

	private DataGridView mappedCommandsGridView;

	private BindingSource mappedCommandsBindingSource;

	private DataGridViewTextBoxColumn cmdIdDataGridViewTextBoxColumn;

	private DataGridViewTextBoxColumn descriptionDataGridViewTextBoxColumn;

	private DataGridViewTextBoxColumn controlTypeDataGridViewTextBoxColumn;

	private DataGridViewTextBoxColumn controllerDataGridViewTextBoxColumn;

	private DataGridViewTextBoxColumn controlNameDataGridViewTextBoxColumn;

	private DataGridViewLinkColumn removeColumn;

	private DataGridView midiDiagDataGrid;

	private BindingSource midiDiagListBindingSource;

	private DataGridViewTextBoxColumn SeqNumColumn;

	private DataGridViewTextBoxColumn deviceDataGridViewTextBoxColumn;

	private DataGridViewTextBoxColumn statusDataGridViewTextBoxColumn;

	private DataGridViewTextBoxColumn controlIdDataGridViewTextBoxColumn;

	private DataGridViewTextBoxColumn dataDataGridViewTextBoxColumn;

	private DataGridViewTextBoxColumn channelDataGridViewTextBoxColumn;

	private DataGridViewTextBoxColumn voiceDataGridViewTextBoxColumn;

	private LinkLabel advancedLinkLabel;

	private Panel advancedMappingPanel;

	private TextBox buttonUpEventTB;

	private Label label9;

	private TextBox buttonDownEventTB;

	private Label label8;

	private Label label7;

	private Label label6;

	private Label eventMappingPrompt;

	private TextBox newValueReceivedEventTB;

	private Label label10;

	private ToolStrip toolStrip;

	private ToolStripDropDownButton fileButton;

	private ToolStripMenuItem loadMappingToolStripMenuItem;

	private ToolStripMenuItem saveMappingToolStripMenuItem;

	private ToolStripSeparator toolStripMenuItem1;

	private ToolStripMenuItem importMappingsToolStripMenuItem;

	private ToolStripMenuItem exportMappingsToolStripMenuItem;

	private ToolStripMenuItem organiseMappingsToolStripMenuItem;

	private ToolStripLabel LoadedMappingLabel;

	private DataGridViewTextBoxColumn midiControlIdDataGridViewTextBoxColumn;

	private DataGridViewTextBoxColumn midiControlNameDataGridViewTextBoxColumn;

	private DataGridViewComboBoxColumn midiControlTypeColumn;

	private DataGridViewTextBoxColumn minValueDataGridViewTextBoxColumn;

	private DataGridViewTextBoxColumn maxValueDataGridViewTextBoxColumn;

	private DataGridViewComboBoxColumn CatCmdIdColumn;

	private DataGridViewLinkColumn EditColumn;

	private DataGridViewLinkColumn deleteColumn;

	public Midi2CatDatabase DB
	{
		get
		{
			if (_db == null)
			{
				_db = new Midi2CatDatabase(DbFile);
				_db.BindToDataSource(controllerMappingBindingSource1, DeviceName);
				_enumsDb = new EnumsDB();
				_enumsDb.BindToDataSource(controlTypesBindingSource, "ControlTypes");
				midiControlTypeColumn.DataSource = controlTypesBindingSource;
				midiControlTypeColumn.ValueMember = "ControlId";
				midiControlTypeColumn.DisplayMember = "ControlDescription";
				_enumsDb.BindToDataSource(catCmdsBindingSource, "CatCmds");
				CatCmdIdColumn.DataSource = catCmdsBindingSource;
				CatCmdIdColumn.ValueMember = "CmdId";
				CatCmdIdColumn.DisplayMember = "CmdDescription";
				_enumsDb.BindToDataSource(catCmdsFilteredBindingSource, "CatCmds");
				mappingAvailableCmdsLB.DataSource = catCmdsFilteredBindingSource;
				mappingAvailableCmdsLB.DisplayMember = "CmdDescription";
				mappingAvailableCmdsLB.ValueMember = "CmdId";
				mappingControlTypeCB2.DataBindings.Add(new Binding("SelectedValue", controllerMappingBindingSource1, "MidiControlType", formattingEnabled: true));
				mappingControlTypeCB2.DataSource = controlTypesBindingSource;
				mappingControlTypeCB2.DisplayMember = "ControlDescription";
				mappingControlTypeCB2.ValueMember = "ControlId";
				MappedCommands dataSource = new MappedCommands(_enumsDb.ds.Tables["CatCmds"], _db.ds.Tables[DeviceName]);
				mappedCommandsBindingSource.DataSource = dataSource;
				_diagList = new MidiDiagList();
				midiDiagListBindingSource.DataSource = _diagList;
			}
			return _db;
		}
	}

	private string DeviceName { get; set; }

	private int DeviceIndex { get; set; }

	public MidiDeviceSetup(string DbFile, string deviceName, int deviceIndex)
	{
		InitializeComponent();
		this.DbFile = DbFile;
		DeviceName = deviceName;
		DeviceIndex = deviceIndex;
		mapInCtrl2CmdPanel.Parent = this;
		mapInCtrl2CmdPanel.Visible = false;
		tabControl.TabPages.Remove(mapInDialogTabPage);
		advancedLinkLabel.Text = "Show Advanced Options";
		OpenMidiDevice();
	}

	private void MidiDeviceSetup_Load(object sender, EventArgs e)
	{
		List<ControllerMapping> mappings = DB.GetMappings(DeviceName, MappingFilter.None);
		foreach (ControllerMapping item in mappings)
		{
			if (item.CatCmdId != CatCmd.None)
			{
				_enumsDb.SetCatCmdInUse(item.CatCmdId, inUse: true);
			}
		}
		mapControlToCommandGrid.Sort(mapControlToCommandGrid.Columns[0], ListSortDirection.Ascending);
		promptPanel.Visible = mappings.Count == 0;
		LoadedMappingLabel.Text = DB.GetLoadedMappingName(DeviceName);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			CloseMidiDevice();
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	private void OpenMidiDevice()
	{
		if (midiDevice == null)
		{
			midiDevice = new MidiDevice();
			midiDevice.onMidiDebugMessage += onMidiDebugMsg;
			midiDevice.onMidiInput += OnMidiInput;
		}
		midiDevice.OpenMidiIn(DeviceIndex, DeviceName);
		ignoreMidiMessagesUntil = DateTime.Now.AddSeconds(5.0);
	}

	private void CloseMidiDevice()
	{
		if (midiDevice != null)
		{
			midiDevice.CloseMidiIn();
			midiDevice.CloseMidiOut();
			midiDevice = null;
		}
	}

	private void OnMidiInput(MidiDevice Device, int DeviceIdx, int ControlId, int Data, int Status, int Event, int Channel)
	{
		if (DateTime.Now < ignoreMidiMessagesUntil)
		{
			return;
		}
		ControlId = Device.FixBehringerCtlID(ControlId, Status);
		this.InvokeIfRequired(delegate
		{
			if (tabControl.SelectedTab == debugTab)
			{
				int num = Device.UnmapControlID(ControlId, out var byteCount);
				string controlId;
				if (ControlId != num)
				{
					string text = "X" + byteCount * 2;
					controlId = ControlId.ToString(text) + " (" + num.ToString("X2") + ")";
				}
				else
				{
					controlId = ControlId.ToString("X2");
				}
				MidiDiagList diagList = _diagList;
				MidiDiagItem obj = new MidiDiagItem
				{
					Device = DeviceIdx.ToString("X2"),
					ControlId = controlId,
					Data = Data.ToString("X2"),
					Status = Status.ToString("X2")
				};
				MidiEvent midiEvent = (MidiEvent)Event;
				obj.Voice = midiEvent.ToString().Replace("_", " ");
				obj.Channel = (Channel + 1).ToString("X2");
				diagList.Add(obj);
				if (_diagList.Count > 100)
				{
					_diagList.RemoveAt(0);
				}
				midiDiagDataGrid.CurrentCell = midiDiagDataGrid.Rows[_diagList.Count - 1].Cells[0];
			}
			if (tabControl.SelectedTab == commandsTabPage)
			{
				tabControl.SelectedTab = mappedControlsTab;
				if (mappedCommandsGridView.SelectedRows.Count == 1)
				{
					DataGridViewRow dataGridViewRow = mappedCommandsGridView.SelectedRows[0];
					int index = mappedCommandsGridView.Columns["cmdIdDataGridViewTextBoxColumn"].Index;
					CatCmdToUse = (CatCmd)dataGridViewRow.Cells[index].Value;
					int index2 = mappedCommandsGridView.Columns["controlTypeDataGridViewTextBoxColumn"].Index;
					ControlTypeToUse = (ControlType)dataGridViewRow.Cells[index2].Value;
				}
			}
			if (tabControl.SelectedTab == mappedControlsTab)
			{
				ControllerMapping controllerMapping = DB.GetMapping(DeviceName, ControlId);
				if (AddingControl && AddingControlId != ControlId)
				{
					MappingDone();
				}
				if (controllerMapping == null)
				{
					controllerMapping = new ControllerMapping
					{
						MidiControlId = ControlId,
						MaxValue = int.MinValue,
						MinValue = int.MaxValue,
						MidiControlType = ControlTypeToUse,
						MidiControlName = ""
					};
					controllerMapping.CatCmd = CatCmdDb.Get(CatCmd.None);
					DataRowView dataRowView = (DataRowView)controllerMappingBindingSource1.AddNew();
					DB.PopulateRow(dataRowView.Row, controllerMapping);
					DB.AddRow(DeviceName, dataRowView.Row);
					dataRowView.Row.AcceptChanges();
					mapControlToCommandGrid.Refresh();
					DB.SaveChanges(DeviceName);
					AddingControl = true;
					AddingControlId = ControlId;
				}
				int position = controllerMappingBindingSource1.Find("MidiControlId", controllerMapping.MidiControlId);
				controllerMappingBindingSource1.Position = position;
				ShowMapInCtrl2CmdDialog(ControlId, Data);
				ValidateDialogInput();
				SendMidiCommand(Channel, Data, Status, ControlId, controllerMapping);
			}
		});
	}

	private void onMidiDebugMsg(int Device, Direction direction, Status status, string msg1, string msg2)
	{
		string text = null;
		if (!string.IsNullOrEmpty(msg1) || !string.IsNullOrEmpty(msg2))
		{
			text = $"{status.ToString()} {direction.ToString()}:{msg1} {msg2}\n";
		}
		if (base.Visible)
		{
			switch (direction)
			{
			case Direction.In:
				midiInStatusLabel.Text = status.ToString();
				break;
			case Direction.Out:
				midiOutStatusLabel.Text = status.ToString();
				break;
			}
			if (text != null)
			{
				errorListBox.Items.Insert(0, text);
			}
			else if (errorListBox.Items.Count == 0)
			{
				errorListBox.Items.Add("No errors detected.");
			}
		}
	}

	private void ShowMapInCtrl2CmdDialog(int ctrlId, int value)
	{
		tabControl.Visible = false;
		LoadNonDataBoundControls(ctrlId, value);
		ShowAddPrompt();
		mapInCtrl2CmdPanel.Visible = true;
		ResizeDialogs();
		if (string.IsNullOrWhiteSpace(mappingControlNameTB2.Text))
		{
			mappingControlNameTB2.Focus();
		}
		mapInCtrl2CmdPanel.Focus();
	}

	private void SetMapInCtrl2CmdDialog(int ctrlId, int value)
	{
		LoadNonDataBoundControls(ctrlId, value);
		ShowAddPrompt();
		mapInCtrl2CmdPanel.Visible = true;
	}

	private void ShowAddPrompt()
	{
		if ((mappingControlTypeCB2.SelectedValue != null && (ControlType)mappingControlTypeCB2.SelectedValue == ControlType.Unknown) || string.IsNullOrWhiteSpace(mappingControlNameTB2.Text))
		{
			mappingPromptLabel2.Text = "You must now give this control a name and then select one of the options in the \"Control Type\" dropdown box above.\n\nChoose the option that most closely matches the type of control you moved, pressed or slid.";
			mappingPromptPanel2.Visible = true;
			mappingAvailableCmdsLB.Visible = false;
		}
		else
		{
			mappingPromptPanel2.Visible = false;
			mappingAvailableCmdsLB.Visible = true;
		}
		promptPanel.Visible = false;
	}

	private void LoadNonDataBoundControls(int ctrlId, int value)
	{
		DataRowView dataRowView = (DataRowView)controllerMappingBindingSource1.Current;
		CatCmd catCmd = (CatCmd)dataRowView["CatCmdId"];
		if (CatCmdToUse != CatCmd.None)
		{
			catCmd = CatCmdToUse;
			mappingControlTypeCB2.SelectedValue = ControlTypeToUse;
			if (value == 0)
			{
				CatCmdToUse = CatCmd.None;
				ControlTypeToUse = ControlType.Unknown;
			}
		}
		if (value > int.MinValue)
		{
			int num = (int)dataRowView["MinValue"];
			int num2 = (int)dataRowView["MaxValue"];
			if (value < num)
			{
				dataRowView["MinValue"] = value;
				dataRowView.Row.AcceptChanges();
			}
			if (value > num2)
			{
				dataRowView["MaxValue"] = value;
				dataRowView.Row.AcceptChanges();
			}
		}
		mappingValueLabel2.Text = value.ToString();
		mappingMessageLabel2.Text = CatCmdDb.Get(catCmd).Desc;
		mappingControlNameTB2.Text = (string)dataRowView["MidiControlName"];
		mappingMaxValueLabel2.Text = ((int)dataRowView["MaxValue"]).ToString();
		mappingMinValueLabel2.Text = ((int)dataRowView["MinValue"]).ToString();
		mappingControlIdLabel2.Text = ((int)dataRowView["MidiControlId"]).ToString();
		buttonDownEventTB.Text = _db.ConvertFromDBVal<string>(dataRowView["MidiOutCmdDown"]);
		buttonUpEventTB.Text = _db.ConvertFromDBVal<string>(dataRowView["MidiOutCmdUp"]);
		newValueReceivedEventTB.Text = _db.ConvertFromDBVal<string>(dataRowView["MidiOutCmdSetValue"]);
		if (string.IsNullOrWhiteSpace(buttonDownEventTB.Text) && string.IsNullOrWhiteSpace(buttonUpEventTB.Text) && string.IsNullOrWhiteSpace(newValueReceivedEventTB.Text))
		{
			advancedLinkLabel.Text = "Show Advanced Options";
		}
		else
		{
			advancedLinkLabel.Text = "Hide Advanced Options";
		}
		ControlType controlType = (ControlType)mappingControlTypeCB2.SelectedValue;
		buttonDownEventTB.Enabled = controlType == ControlType.Button;
		buttonUpEventTB.Enabled = controlType == ControlType.Button;
		string text = "( InUse = False AND ControlType = " + mappingControlTypeCB2.SelectedValue.ToString() + " OR ControlType = 0 )";
		string obj = text;
		int num3 = (int)catCmd;
		text = obj + " OR ( CmdId = " + num3 + ")";
		if (text != catCmdsFilteredBindingSource.Filter)
		{
			catCmdsFilteredBindingSource.Filter = text;
		}
		mappingAvailableCmdsLB.SelectedValue = catCmd;
	}

	private void HideMapInDialogs()
	{
		mapInCtrl2CmdPanel.Visible = false;
		tabControl.Visible = true;
	}

	private void MidiDeviceSetup_Resize(object sender, EventArgs e)
	{
		ResizeDialogs();
	}

	private void advancedLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		if (advancedLinkLabel.Text == "Show Advanced Options")
		{
			advancedLinkLabel.Text = "Hide Advanced Options";
		}
		else
		{
			advancedLinkLabel.Text = "Show Advanced Options";
		}
		ResizeDialogs();
	}

	private void ResizeDialogs()
	{
		if (mapInCtrl2CmdPanel.Visible)
		{
			if (advancedLinkLabel.Text == "Show Advanced Options")
			{
				advancedMappingPanel.Visible = false;
				mapInCtrl2CmdPanel.Width = 396;
				mapInCtrl2CmdPanel.Left = base.Width / 2 - mapInCtrl2CmdPanel.Width / 2;
			}
			else
			{
				advancedMappingPanel.Visible = true;
				mapInCtrl2CmdPanel.Width = base.Width - 16;
				mapInCtrl2CmdPanel.Left = 8;
			}
			mapInCtrl2CmdPanel.Top = 10;
			mapInCtrl2CmdPanel.Height = base.Height - 20;
		}
	}

	private void mappingControlTypeCB2_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!mapInCtrl2CmdPanel.Visible)
		{
			return;
		}
		if (mappingControlTypeCB2.SelectedIndex == 0)
		{
			mappingPromptPanel2.Visible = true;
			mappingAvailableCmdsLB.Visible = false;
		}
		else
		{
			mappingPromptPanel2.Visible = false;
			mappingAvailableCmdsLB.Visible = true;
		}
		if (mappingControlTypeCB2.SelectedValue != null)
		{
			ControlType controlType = (ControlType)mappingControlTypeCB2.SelectedValue;
			buttonDownEventTB.Enabled = controlType == ControlType.Button;
			buttonUpEventTB.Enabled = controlType == ControlType.Button;
			string text = "( InUse = False AND ControlType = " + mappingControlTypeCB2.SelectedValue.ToString() + " OR ControlType = 0 )";
			if (mappingAvailableCmdsLB.SelectedValue != null)
			{
				text = text + " OR ( CmdId = " + mappingAvailableCmdsLB.SelectedValue.ToString() + ")";
			}
			if (catCmdsFilteredBindingSource.Filter != text)
			{
				catCmdsFilteredBindingSource.Filter = text;
			}
		}
	}

	private void mappingAvailableCmdsLB_SelectedIndexChanged(object sender, EventArgs e)
	{
		ValidateDialogInput();
	}

	private void mappingDoneButton2_Click(object sender, EventArgs e)
	{
		MappingDone();
		HideMapInDialogs();
	}

	private void MappingDone()
	{
		if (AddingControl && AddingControlId > int.MinValue && !DialogValid)
		{
			RemoveInvalidMidiAddedControlType();
			return;
		}
		DataRowView dataRowView = (DataRowView)controllerMappingBindingSource1.Current;
		if (dataRowView != null && DialogValid)
		{
			DataRowView dataRowView2 = (DataRowView)mappingAvailableCmdsLB.SelectedItem;
			dataRowView2["InUse"] = true;
			dataRowView2.Row.AcceptChanges();
			dataRowView["CatCmdId"] = (CatCmd)dataRowView2["CmdId"];
			dataRowView["MidiControlName"] = mappingControlNameTB2.Text;
			dataRowView["MidiControlType"] = (CatCmd)mappingControlTypeCB2.SelectedValue;
			dataRowView["MidiOutCmdDown"] = buttonDownEventTB.Text.Trim().ToUpper();
			dataRowView["MidiOutCmdUp"] = buttonUpEventTB.Text.Trim().ToUpper();
			dataRowView["MidiOutCmdSetValue"] = newValueReceivedEventTB.Text.Trim().ToUpper();
			dataRowView.Row.AcceptChanges();
			DB.SaveChanges(DeviceName);
			AddingControl = false;
			AddingControlId = int.MinValue;
		}
	}

	private void mappingBackButton2_Click(object sender, EventArgs e)
	{
		if (AddingControl && AddingControlId > int.MinValue)
		{
			RemoveInvalidMidiAddedControlType();
		}
		HideMapInDialogs();
	}

	private void mappingControlNameTB2_KeyPress(object sender, KeyPressEventArgs e)
	{
		ValidateDialogInput();
	}

	private void mapControlToCommandGrid_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex >= 0)
		{
			_ = e.ColumnIndex;
			_ = 0;
		}
	}

	private void mapControlToCommandGrid_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
		{
			_ = mapControlToCommandGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
		}
		else if (e.RowIndex >= 0 && e.ColumnIndex < 0)
		{
			mapControlToCommandGrid.CurrentCell = mapControlToCommandGrid.Rows[e.RowIndex].Cells[0];
		}
	}

	private void mapControlToCommandGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
	{
		DataGridViewCell dataGridViewCell = mapControlToCommandGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
		DataGridViewRow owningRow = dataGridViewCell.OwningRow;
		DataGridViewColumn owningColumn = dataGridViewCell.OwningColumn;
		if (!owningColumn.IsDataBound || !(owningColumn.Name == "CatCmdIdColumn") || !(mapControlToCommandGrid.CurrentCell.EditType == typeof(DataGridViewComboBoxEditingControl)))
		{
			return;
		}
		DataGridViewComboBoxCell dataGridViewComboBoxCell = (DataGridViewComboBoxCell)mapControlToCommandGrid[e.ColumnIndex, e.RowIndex];
		dataGridViewComboBoxCell.DataSource = catCmdsFilteredBindingSource;
		object value = owningRow.Cells["midiControlTypeColumn"].Value;
		string text = "( ControlType = " + value.ToString() + " OR ControlType = 0 )";
		if (!string.IsNullOrEmpty(dataGridViewCell.Value.ToString()))
		{
			text = text + " OR ( CmdId = " + dataGridViewCell.Value.ToString() + ")";
		}
		catCmdsFilteredBindingSource.Filter = text;
		bool flag = false;
		foreach (DataRowView item in catCmdsFilteredBindingSource)
		{
			if (item["CmdId"].ToString() == dataGridViewCell.Value.ToString())
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			DataRowView obj = (DataRowView)mapControlToCommandGrid.CurrentRow.DataBoundItem;
			obj.BeginEdit();
			obj.Row["CatCmdId"] = 0;
			obj.Row.EndEdit();
		}
		dataGridViewComboBoxCell.DataSource = catCmdsFilteredBindingSource;
	}

	private void mapControlToCommandGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
	{
		DataGridViewCell dataGridViewCell = mapControlToCommandGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
		DataGridViewRow owningRow = dataGridViewCell.OwningRow;
		DataGridViewColumn owningColumn = dataGridViewCell.OwningColumn;
		if (owningColumn.IsDataBound && owningColumn.Name == "CatCmdIdColumn")
		{
			if (mapControlToCommandGrid.CurrentCell.EditType == typeof(DataGridViewComboBoxEditingControl))
			{
				((DataGridViewComboBoxCell)mapControlToCommandGrid[e.ColumnIndex, e.RowIndex]).DataSource = catCmdsBindingSource;
			}
		}
		else if (owningColumn.IsDataBound && owningColumn.Name == "midiControlTypeColumn")
		{
			ControlType controlType = (ControlType)owningRow.Cells["midiControlTypeColumn"].Value;
			if (CatCmdDb.Get((CatCmd)owningRow.Cells["CatCmdIdColumn"].Value).ControlType != controlType)
			{
				DataRowView obj = (DataRowView)mapControlToCommandGrid.CurrentRow.DataBoundItem;
				obj.BeginEdit();
				obj.Row["CatCmdId"] = 0;
				obj.EndEdit();
			}
		}
	}

	private void mapControlToCommandGrid_Click(object sender, EventArgs e)
	{
		if (mapControlToCommandGrid.IsCurrentCellInEditMode)
		{
			mapControlToCommandGrid.EndEdit();
		}
	}

	private void mapControlToCommandGrid_DataError(object sender, DataGridViewDataErrorEventArgs e)
	{
	}

	private void mapControlToCommandGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
	{
		DataGridViewCell dataGridViewCell = mapControlToCommandGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
		_ = dataGridViewCell.OwningRow;
		DataGridViewColumn owningColumn = dataGridViewCell.OwningColumn;
		string value = dataGridViewCell.Value.ToString();
		string value2 = (string)dataGridViewCell.Tag;
		if (owningColumn.Name == "CatCmdIdColumn")
		{
			CatCmd catCmd = (CatCmd)Convert.ToInt32(value);
			CatCmd catCmd2 = (CatCmd)Convert.ToInt32(value2);
			_enumsDb.SetCatCmdInUse(catCmd2, inUse: false);
			_enumsDb.SetCatCmdInUse(catCmd, inUse: true);
		}
		DB.SaveChanges(DeviceName);
	}

	private void mapControlToCommandGrid_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
	{
		ShowMapInCtrl2CmdDialog((int)mapControlToCommandGrid.CurrentRow.Cells[0].Value, int.MinValue);
	}

	private void validateDialogInput(object sender, CancelEventArgs e)
	{
		ValidateDialogInput();
	}

	private void ValidateDialogInput()
	{
		DialogValid = !string.IsNullOrWhiteSpace(mappingControlNameTB2.Text) && mappingControlTypeCB2.Text != "Unknown" && mappingAvailableCmdsLB.SelectedItem != null;
		mappingDoneButton2.Enabled = DialogValid;
	}

	private void RemoveInvalidMidiAddedControlType()
	{
		if (AddingControl && AddingControlId >= int.MinValue)
		{
			DB.DeleteRow(DeviceName, AddingControlId);
			DB.SaveChanges(DeviceName);
			AddingControl = false;
			AddingControlId = int.MinValue;
		}
	}

	private void mapControlToCommandGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
		{
			DataGridViewCell dataGridViewCell = mapControlToCommandGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
			if (dataGridViewCell.OwningColumn.Name == "EditColumn")
			{
				ShowMapInCtrl2CmdDialog((int)mapControlToCommandGrid.CurrentRow.Cells[0].Value, int.MinValue);
			}
			if (dataGridViewCell.OwningColumn.Name == "deleteColumn")
			{
				mapControlToCommandGrid.Rows.RemoveAt(e.RowIndex);
				DB.SaveChanges(DeviceName);
			}
		}
	}

	private void mappedCommandsGridView_CellClick(object sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex < 0 || e.ColumnIndex < 0)
		{
			return;
		}
		try
		{
			if (mappedCommandsGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].OwningColumn.Name == "removeColumn")
			{
				DataGridViewRow dataGridViewRow = mappedCommandsGridView.Rows[e.RowIndex];
				int index = mappedCommandsGridView.Columns["controllerDataGridViewTextBoxColumn"].Index;
				int index2 = mappedCommandsGridView.Columns["cmdIdDataGridViewTextBoxColumn"].Index;
				string midiDeviceName = (string)dataGridViewRow.Cells[index].Value;
				int catCmd = (int)dataGridViewRow.Cells[index2].Value;
				DB.RemoveMapping(midiDeviceName, catCmd);
				DB.SaveChanges(DeviceName);
				_enumsDb.SetCatCmdInUse((CatCmd)catCmd, inUse: false);
				MappedCommands dataSource = new MappedCommands(_enumsDb.ds.Tables["CatCmds"], _db.ds.Tables[DeviceName]);
				mappedCommandsBindingSource.DataSource = dataSource;
			}
		}
		catch
		{
		}
	}

	private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (tabControl.SelectedTab == commandsTabPage)
		{
			MappedCommands dataSource = new MappedCommands(_enumsDb.ds.Tables["CatCmds"], _db.ds.Tables[DeviceName]);
			mappedCommandsBindingSource.DataSource = dataSource;
		}
		else if (tabControl.SelectedTab == mappedControlsTab)
		{
			CatCmdToUse = CatCmd.None;
		}
		else if (tabControl.SelectedTab == debugTab)
		{
			CatCmdToUse = CatCmd.None;
		}
	}

	private void validateDialogInput(object sender, EventArgs e)
	{
		ShowAddPrompt();
		ValidateDialogInput();
		eventMappingPrompt.Text = "";
		string[] array2;
		if (!string.IsNullOrWhiteSpace(newValueReceivedEventTB.Text))
		{
			string[] array = midiDevice.ValidateMidiMessages(newValueReceivedEventTB.Text);
			if (array.Length == 0)
			{
				eventMappingPrompt.Text += "New Value Received Event(s) formatted correctly.\n";
			}
			else
			{
				eventMappingPrompt.Text += "New Value Received Event(s) NOT formatted correctly.\n";
				array2 = array;
				foreach (string text in array2)
				{
					Label label = eventMappingPrompt;
					label.Text = label.Text + text + "\n";
				}
			}
		}
		if (!string.IsNullOrWhiteSpace(buttonDownEventTB.Text))
		{
			string[] array3 = midiDevice.ValidateMidiMessages(buttonDownEventTB.Text);
			if (array3.Length == 0)
			{
				eventMappingPrompt.Text += "Button Down Event(s) formatted correctly.\n";
			}
			else
			{
				eventMappingPrompt.Text += "Button Down Event(s) NOT formatted correctly.\n";
				array2 = array3;
				foreach (string text2 in array2)
				{
					Label label2 = eventMappingPrompt;
					label2.Text = label2.Text + text2 + "\n";
				}
			}
		}
		if (string.IsNullOrWhiteSpace(buttonUpEventTB.Text))
		{
			return;
		}
		string[] array4 = midiDevice.ValidateMidiMessages(buttonUpEventTB.Text);
		if (array4.Length == 0)
		{
			eventMappingPrompt.Text += "Button Up Event(s) formatted correctly.";
			return;
		}
		eventMappingPrompt.Text += "Button Up Event NOT formatted correctly.\n";
		array2 = array4;
		foreach (string text3 in array2)
		{
			Label label3 = eventMappingPrompt;
			label3.Text = label3.Text + text3 + "\n";
		}
	}

	private void mappedCommandsGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
	{
		e.ThrowException = false;
	}

	private void SendMidiCommand(int inChannel, int inValue, int inStatus, int inControl, ControllerMapping mapping)
	{
		if (!string.IsNullOrWhiteSpace(mapping.MidiOutCmdDown) && inValue > 0)
		{
			midiDevice.SendMsg(inChannel, inValue, inStatus, inControl, mapping.MidiOutCmdDown);
		}
		else if (!string.IsNullOrWhiteSpace(mapping.MidiOutCmdUp) && inValue <= 0)
		{
			midiDevice.SendMsg(inChannel, inValue, inStatus, inControl, mapping.MidiOutCmdUp);
		}
		else if (!string.IsNullOrWhiteSpace(mapping.MidiOutCmdSetValue))
		{
			midiDevice.SendMsg(inChannel, inValue, inStatus, inControl, mapping.MidiOutCmdSetValue);
		}
	}

	private void mapControlToCommandGrid_Leave(object sender, EventArgs e)
	{
	}

	private void saveMappingToolStripMenuItem_Click(object sender, EventArgs e)
	{
		SaveAsDialog saveAsDialog = new SaveAsDialog();
		saveAsDialog.ExistingMappings = DB.GetSavedMappings();
		if (saveAsDialog.ShowDialog() == DialogResult.OK)
		{
			DB.SaveMappingAs(DeviceName, saveAsDialog.MappingName, replace: true);
			LoadedMappingLabel.Text = DB.GetLoadedMappingName(DeviceName);
		}
	}

	private void loadMappingToolStripMenuItem_Click(object sender, EventArgs e)
	{
		LoadDialog loadDialog = new LoadDialog();
		loadDialog.ExistingMappings = DB.GetSavedMappings();
		if (loadDialog.ShowDialog() == DialogResult.OK)
		{
			tabControl.SelectedTab = mappedControlsTab;
			DB.LoadMapping(DeviceName, loadDialog.MappingName);
			LoadedMappingLabel.Text = DB.GetLoadedMappingName(DeviceName);
			promptPanel.Visible = false;
		}
	}

	private void exportMappingsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		PickDialog pickDialog = new PickDialog();
		pickDialog.Prompt = "Pick the mappings to export";
		string[] savedMappings = DB.GetSavedMappings();
		if (savedMappings.Length == 0)
		{
			MessageBox.Show("You can only export saved mappings,\nYou must save your current mapping before it can be exported", "Unable To Export", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		pickDialog.ExistingMappings = savedMappings;
		if (pickDialog.ShowDialog() == DialogResult.OK)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.FileName = "";
			saveFileDialog.Filter = "Midi2Cat Files | *.m2c";
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				DB.ExportMappings(saveFileDialog.FileName, pickDialog.Mappings);
			}
		}
	}

	private void importMappingsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.FileName = "";
		openFileDialog.Filter = "Midi2Cat Files | *.m2c";
		openFileDialog.Multiselect = false;
		if (openFileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		if (!DB.ImportMappings(openFileDialog.FileName))
		{
			MessageBox.Show("The import file is invalid or corrupt", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		PickDialog pickDialog = new PickDialog();
		pickDialog.Prompt = "Pick the mappings to import from the file.";
		pickDialog.ExistingMappings = DB.GetImportedMappings();
		if (pickDialog.ShowDialog() == DialogResult.OK)
		{
			DB.AddFromImport(pickDialog.Mappings);
		}
	}

	private void organiseMappingsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OrganiseDialog organiseDialog = new OrganiseDialog(DB, DeviceName);
		organiseDialog.ExistingMappings = DB.GetSavedMappings();
		organiseDialog.ShowDialog();
		LoadedMappingLabel.Text = DB.GetLoadedMappingName(DeviceName);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Midi2Cat.IO.MidiDeviceSetup));
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.midiOutStatusLabel = new System.Windows.Forms.Label();
		this.midiInStatusLabel = new System.Windows.Forms.Label();
		this.errorListBox = new System.Windows.Forms.ListBox();
		this.tabControl = new System.Windows.Forms.TabControl();
		this.mappedControlsTab = new System.Windows.Forms.TabPage();
		this.promptPanel = new System.Windows.Forms.Panel();
		this.label5 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.mapControlToCommandGrid = new System.Windows.Forms.DataGridView();
		this.controllerMappingBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
		this.commandsTabPage = new System.Windows.Forms.TabPage();
		this.mappedCommandsGridView = new System.Windows.Forms.DataGridView();
		this.cmdIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.descriptionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.controlTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.controllerDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.controlNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.removeColumn = new System.Windows.Forms.DataGridViewLinkColumn();
		this.mappedCommandsBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.debugTab = new System.Windows.Forms.TabPage();
		this.midiDiagDataGrid = new System.Windows.Forms.DataGridView();
		this.SeqNumColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.deviceDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.statusDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.controlIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.channelDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.voiceDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.midiDiagListBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.mapInDialogTabPage = new System.Windows.Forms.TabPage();
		this.mapInCtrl2CmdPanel = new System.Windows.Forms.Panel();
		this.advancedMappingPanel = new System.Windows.Forms.Panel();
		this.newValueReceivedEventTB = new System.Windows.Forms.TextBox();
		this.label10 = new System.Windows.Forms.Label();
		this.eventMappingPrompt = new System.Windows.Forms.Label();
		this.buttonUpEventTB = new System.Windows.Forms.TextBox();
		this.label9 = new System.Windows.Forms.Label();
		this.buttonDownEventTB = new System.Windows.Forms.TextBox();
		this.label8 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.mappingPromptPanel2 = new System.Windows.Forms.Panel();
		this.label1 = new System.Windows.Forms.Label();
		this.mappingPromptLabel2 = new System.Windows.Forms.Label();
		this.mappingControlTypeCB2 = new System.Windows.Forms.ComboBox();
		this.controlTypesBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.mappingAvailableCmdsLB = new System.Windows.Forms.ListBox();
		this.catCmdsFilteredBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.mappingMessageLabel2 = new System.Windows.Forms.Label();
		this.mappingValueLabel2 = new System.Windows.Forms.Label();
		this.label14 = new System.Windows.Forms.Label();
		this.mappingBackButton2 = new System.Windows.Forms.Button();
		this.mappingDoneButton2 = new System.Windows.Forms.Button();
		this.mappingControlNameTB2 = new System.Windows.Forms.TextBox();
		this.label15 = new System.Windows.Forms.Label();
		this.label17 = new System.Windows.Forms.Label();
		this.mappingMinValueLabel2 = new System.Windows.Forms.Label();
		this.label19 = new System.Windows.Forms.Label();
		this.mappingMaxValueLabel2 = new System.Windows.Forms.Label();
		this.label21 = new System.Windows.Forms.Label();
		this.mappingControlIdLabel2 = new System.Windows.Forms.Label();
		this.label23 = new System.Windows.Forms.Label();
		this.label12 = new System.Windows.Forms.Label();
		this.advancedLinkLabel = new System.Windows.Forms.LinkLabel();
		this.dataGridViewComboBoxColumn1 = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.dataGridViewComboBoxColumn2 = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.dataGridViewComboBoxColumn3 = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.dataGridViewComboBoxColumn4 = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.dataGridViewComboBoxColumn5 = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.dataGridViewComboBoxColumn6 = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.dataGridViewComboBoxColumn7 = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.catCmdsBindingSource = new System.Windows.Forms.BindingSource(this.components);
		this.toolStrip = new System.Windows.Forms.ToolStrip();
		this.fileButton = new System.Windows.Forms.ToolStripDropDownButton();
		this.loadMappingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.saveMappingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.organiseMappingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
		this.importMappingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.exportMappingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.LoadedMappingLabel = new System.Windows.Forms.ToolStripLabel();
		this.midiControlIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.midiControlNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.midiControlTypeColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.minValueDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.maxValueDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.CatCmdIdColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.EditColumn = new System.Windows.Forms.DataGridViewLinkColumn();
		this.deleteColumn = new System.Windows.Forms.DataGridViewLinkColumn();
		this.tabControl.SuspendLayout();
		this.mappedControlsTab.SuspendLayout();
		this.promptPanel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.mapControlToCommandGrid).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.controllerMappingBindingSource1).BeginInit();
		this.commandsTabPage.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.mappedCommandsGridView).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.mappedCommandsBindingSource).BeginInit();
		this.debugTab.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.midiDiagDataGrid).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.midiDiagListBindingSource).BeginInit();
		this.mapInDialogTabPage.SuspendLayout();
		this.mapInCtrl2CmdPanel.SuspendLayout();
		this.advancedMappingPanel.SuspendLayout();
		this.mappingPromptPanel2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.controlTypesBindingSource).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.catCmdsFilteredBindingSource).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.catCmdsBindingSource).BeginInit();
		this.toolStrip.SuspendLayout();
		base.SuspendLayout();
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(3, 24);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(82, 13);
		this.label3.TabIndex = 3;
		this.label3.Text = "Midi Out Status:";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(3, 7);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(74, 13);
		this.label4.TabIndex = 4;
		this.label4.Text = "Midi In Status:";
		this.midiOutStatusLabel.AutoSize = true;
		this.midiOutStatusLabel.Location = new System.Drawing.Point(88, 24);
		this.midiOutStatusLabel.Name = "midiOutStatusLabel";
		this.midiOutStatusLabel.Size = new System.Drawing.Size(39, 13);
		this.midiOutStatusLabel.TabIndex = 5;
		this.midiOutStatusLabel.Text = "Closed";
		this.midiInStatusLabel.AutoSize = true;
		this.midiInStatusLabel.Location = new System.Drawing.Point(88, 7);
		this.midiInStatusLabel.Name = "midiInStatusLabel";
		this.midiInStatusLabel.Size = new System.Drawing.Size(39, 13);
		this.midiInStatusLabel.TabIndex = 6;
		this.midiInStatusLabel.Text = "Closed";
		this.errorListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.errorListBox.FormattingEnabled = true;
		this.errorListBox.HorizontalScrollbar = true;
		this.errorListBox.Location = new System.Drawing.Point(6, 46);
		this.errorListBox.Name = "errorListBox";
		this.errorListBox.Size = new System.Drawing.Size(614, 56);
		this.errorListBox.TabIndex = 8;
		this.tabControl.Controls.Add(this.mappedControlsTab);
		this.tabControl.Controls.Add(this.commandsTabPage);
		this.tabControl.Controls.Add(this.debugTab);
		this.tabControl.Controls.Add(this.mapInDialogTabPage);
		this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tabControl.Location = new System.Drawing.Point(0, 25);
		this.tabControl.Name = "tabControl";
		this.tabControl.SelectedIndex = 0;
		this.tabControl.Size = new System.Drawing.Size(634, 666);
		this.tabControl.TabIndex = 12;
		this.tabControl.SelectedIndexChanged += new System.EventHandler(tabControl_SelectedIndexChanged);
		this.mappedControlsTab.Controls.Add(this.promptPanel);
		this.mappedControlsTab.Controls.Add(this.mapControlToCommandGrid);
		this.mappedControlsTab.Location = new System.Drawing.Point(4, 22);
		this.mappedControlsTab.Name = "mappedControlsTab";
		this.mappedControlsTab.Padding = new System.Windows.Forms.Padding(3);
		this.mappedControlsTab.Size = new System.Drawing.Size(626, 640);
		this.mappedControlsTab.TabIndex = 4;
		this.mappedControlsTab.Text = "Mapped Controls";
		this.mappedControlsTab.UseVisualStyleBackColor = true;
		this.promptPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.promptPanel.Controls.Add(this.label5);
		this.promptPanel.Controls.Add(this.label2);
		this.promptPanel.Location = new System.Drawing.Point(6, 29);
		this.promptPanel.Name = "promptPanel";
		this.promptPanel.Size = new System.Drawing.Size(339, 235);
		this.promptPanel.TabIndex = 1;
		this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label5.Location = new System.Drawing.Point(14, 57);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(314, 170);
		this.label5.TabIndex = 1;
		this.label5.Text = resources.GetString("label5.Text");
		this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label2.Location = new System.Drawing.Point(14, 19);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(314, 22);
		this.label2.TabIndex = 0;
		this.label2.Text = "Map Your Controller";
		this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.mapControlToCommandGrid.AllowUserToAddRows = false;
		this.mapControlToCommandGrid.AllowUserToDeleteRows = false;
		this.mapControlToCommandGrid.AllowUserToResizeRows = false;
		this.mapControlToCommandGrid.AutoGenerateColumns = false;
		this.mapControlToCommandGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		this.mapControlToCommandGrid.Columns.AddRange(this.midiControlIdDataGridViewTextBoxColumn, this.midiControlNameDataGridViewTextBoxColumn, this.midiControlTypeColumn, this.minValueDataGridViewTextBoxColumn, this.maxValueDataGridViewTextBoxColumn, this.CatCmdIdColumn, this.EditColumn, this.deleteColumn);
		this.mapControlToCommandGrid.DataSource = this.controllerMappingBindingSource1;
		this.mapControlToCommandGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.mapControlToCommandGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
		this.mapControlToCommandGrid.Location = new System.Drawing.Point(3, 3);
		this.mapControlToCommandGrid.MultiSelect = false;
		this.mapControlToCommandGrid.Name = "mapControlToCommandGrid";
		this.mapControlToCommandGrid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
		this.mapControlToCommandGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
		this.mapControlToCommandGrid.Size = new System.Drawing.Size(620, 634);
		this.mapControlToCommandGrid.TabIndex = 0;
		this.mapControlToCommandGrid.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(mapControlToCommandGrid_CellBeginEdit);
		this.mapControlToCommandGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(mapControlToCommandGrid_CellContentClick);
		this.mapControlToCommandGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(mapControlToCommandGrid_CellEndEdit);
		this.mapControlToCommandGrid.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(mapControlToCommandGrid_CellMouseEnter);
		this.mapControlToCommandGrid.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(mapControlToCommandGrid_CellMouseLeave);
		this.mapControlToCommandGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(mapControlToCommandGrid_DataError);
		this.mapControlToCommandGrid.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(mapControlToCommandGrid_RowHeaderMouseClick);
		this.mapControlToCommandGrid.Click += new System.EventHandler(mapControlToCommandGrid_Click);
		this.mapControlToCommandGrid.Leave += new System.EventHandler(mapControlToCommandGrid_Leave);
		this.controllerMappingBindingSource1.DataSource = typeof(Midi2Cat.Data.ControllerMapping);
		this.commandsTabPage.Controls.Add(this.mappedCommandsGridView);
		this.commandsTabPage.Location = new System.Drawing.Point(4, 22);
		this.commandsTabPage.Name = "commandsTabPage";
		this.commandsTabPage.Padding = new System.Windows.Forms.Padding(3);
		this.commandsTabPage.Size = new System.Drawing.Size(626, 640);
		this.commandsTabPage.TabIndex = 5;
		this.commandsTabPage.Text = "Commands";
		this.commandsTabPage.UseVisualStyleBackColor = true;
		this.mappedCommandsGridView.AllowUserToAddRows = false;
		this.mappedCommandsGridView.AllowUserToDeleteRows = false;
		this.mappedCommandsGridView.AllowUserToResizeRows = false;
		this.mappedCommandsGridView.AutoGenerateColumns = false;
		this.mappedCommandsGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.mappedCommandsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		this.mappedCommandsGridView.Columns.AddRange(this.cmdIdDataGridViewTextBoxColumn, this.descriptionDataGridViewTextBoxColumn, this.controlTypeDataGridViewTextBoxColumn, this.controllerDataGridViewTextBoxColumn, this.controlNameDataGridViewTextBoxColumn, this.removeColumn);
		this.mappedCommandsGridView.DataSource = this.mappedCommandsBindingSource;
		this.mappedCommandsGridView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.mappedCommandsGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
		this.mappedCommandsGridView.Location = new System.Drawing.Point(3, 3);
		this.mappedCommandsGridView.MultiSelect = false;
		this.mappedCommandsGridView.Name = "mappedCommandsGridView";
		this.mappedCommandsGridView.ReadOnly = true;
		this.mappedCommandsGridView.RowHeadersVisible = false;
		this.mappedCommandsGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.mappedCommandsGridView.Size = new System.Drawing.Size(620, 634);
		this.mappedCommandsGridView.TabIndex = 0;
		this.mappedCommandsGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(mappedCommandsGridView_CellClick);
		this.mappedCommandsGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(mappedCommandsGridView_DataError);
		this.cmdIdDataGridViewTextBoxColumn.DataPropertyName = "CmdId";
		this.cmdIdDataGridViewTextBoxColumn.HeaderText = "Id";
		this.cmdIdDataGridViewTextBoxColumn.Name = "cmdIdDataGridViewTextBoxColumn";
		this.cmdIdDataGridViewTextBoxColumn.ReadOnly = true;
		this.cmdIdDataGridViewTextBoxColumn.Width = 50;
		this.descriptionDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.descriptionDataGridViewTextBoxColumn.DataPropertyName = "Description";
		this.descriptionDataGridViewTextBoxColumn.HeaderText = "Description";
		this.descriptionDataGridViewTextBoxColumn.Name = "descriptionDataGridViewTextBoxColumn";
		this.descriptionDataGridViewTextBoxColumn.ReadOnly = true;
		this.controlTypeDataGridViewTextBoxColumn.DataPropertyName = "ControlType";
		this.controlTypeDataGridViewTextBoxColumn.HeaderText = "Control Type";
		this.controlTypeDataGridViewTextBoxColumn.Name = "controlTypeDataGridViewTextBoxColumn";
		this.controlTypeDataGridViewTextBoxColumn.ReadOnly = true;
		this.controllerDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.controllerDataGridViewTextBoxColumn.DataPropertyName = "Controller";
		this.controllerDataGridViewTextBoxColumn.FillWeight = 90f;
		this.controllerDataGridViewTextBoxColumn.HeaderText = "Controller";
		this.controllerDataGridViewTextBoxColumn.Name = "controllerDataGridViewTextBoxColumn";
		this.controllerDataGridViewTextBoxColumn.ReadOnly = true;
		this.controlNameDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.controlNameDataGridViewTextBoxColumn.DataPropertyName = "ControlName";
		this.controlNameDataGridViewTextBoxColumn.HeaderText = "Control Name";
		this.controlNameDataGridViewTextBoxColumn.Name = "controlNameDataGridViewTextBoxColumn";
		this.controlNameDataGridViewTextBoxColumn.ReadOnly = true;
		this.removeColumn.DataPropertyName = "Remove";
		this.removeColumn.HeaderText = "Unmap";
		this.removeColumn.Name = "removeColumn";
		this.removeColumn.ReadOnly = true;
		this.removeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.removeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.removeColumn.Width = 60;
		this.mappedCommandsBindingSource.DataSource = typeof(Midi2Cat.Data.MappedCommands);
		this.debugTab.Controls.Add(this.midiDiagDataGrid);
		this.debugTab.Controls.Add(this.errorListBox);
		this.debugTab.Controls.Add(this.label3);
		this.debugTab.Controls.Add(this.label4);
		this.debugTab.Controls.Add(this.midiInStatusLabel);
		this.debugTab.Controls.Add(this.midiOutStatusLabel);
		this.debugTab.Location = new System.Drawing.Point(4, 22);
		this.debugTab.Name = "debugTab";
		this.debugTab.Padding = new System.Windows.Forms.Padding(3);
		this.debugTab.Size = new System.Drawing.Size(626, 640);
		this.debugTab.TabIndex = 2;
		this.debugTab.Text = "Diagnostics";
		this.debugTab.UseVisualStyleBackColor = true;
		this.midiDiagDataGrid.AllowUserToAddRows = false;
		this.midiDiagDataGrid.AllowUserToDeleteRows = false;
		this.midiDiagDataGrid.AllowUserToResizeColumns = false;
		this.midiDiagDataGrid.AllowUserToResizeRows = false;
		this.midiDiagDataGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.midiDiagDataGrid.AutoGenerateColumns = false;
		this.midiDiagDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.midiDiagDataGrid.Columns.AddRange(this.SeqNumColumn, this.deviceDataGridViewTextBoxColumn, this.statusDataGridViewTextBoxColumn, this.controlIdDataGridViewTextBoxColumn, this.dataDataGridViewTextBoxColumn, this.channelDataGridViewTextBoxColumn, this.voiceDataGridViewTextBoxColumn);
		this.midiDiagDataGrid.DataSource = this.midiDiagListBindingSource;
		this.midiDiagDataGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
		this.midiDiagDataGrid.Location = new System.Drawing.Point(6, 108);
		this.midiDiagDataGrid.MultiSelect = false;
		this.midiDiagDataGrid.Name = "midiDiagDataGrid";
		this.midiDiagDataGrid.ReadOnly = true;
		this.midiDiagDataGrid.RowHeadersVisible = false;
		this.midiDiagDataGrid.RowTemplate.ReadOnly = true;
		this.midiDiagDataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.midiDiagDataGrid.Size = new System.Drawing.Size(614, 544);
		this.midiDiagDataGrid.TabIndex = 12;
		this.SeqNumColumn.DataPropertyName = "SeqNum";
		this.SeqNumColumn.HeaderText = "#";
		this.SeqNumColumn.Name = "SeqNumColumn";
		this.SeqNumColumn.ReadOnly = true;
		this.SeqNumColumn.Width = 50;
		this.deviceDataGridViewTextBoxColumn.DataPropertyName = "Device";
		dataGridViewCellStyle.NullValue = null;
		this.deviceDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle;
		this.deviceDataGridViewTextBoxColumn.HeaderText = "Controller #";
		this.deviceDataGridViewTextBoxColumn.Name = "deviceDataGridViewTextBoxColumn";
		this.deviceDataGridViewTextBoxColumn.ReadOnly = true;
		this.deviceDataGridViewTextBoxColumn.Width = 75;
		this.statusDataGridViewTextBoxColumn.DataPropertyName = "Status";
		this.statusDataGridViewTextBoxColumn.HeaderText = "Status";
		this.statusDataGridViewTextBoxColumn.Name = "statusDataGridViewTextBoxColumn";
		this.statusDataGridViewTextBoxColumn.ReadOnly = true;
		this.statusDataGridViewTextBoxColumn.Width = 75;
		this.controlIdDataGridViewTextBoxColumn.DataPropertyName = "ControlId";
		this.controlIdDataGridViewTextBoxColumn.HeaderText = "Control #";
		this.controlIdDataGridViewTextBoxColumn.Name = "controlIdDataGridViewTextBoxColumn";
		this.controlIdDataGridViewTextBoxColumn.ReadOnly = true;
		this.controlIdDataGridViewTextBoxColumn.Width = 75;
		this.dataDataGridViewTextBoxColumn.DataPropertyName = "Data";
		this.dataDataGridViewTextBoxColumn.HeaderText = "Data";
		this.dataDataGridViewTextBoxColumn.Name = "dataDataGridViewTextBoxColumn";
		this.dataDataGridViewTextBoxColumn.ReadOnly = true;
		this.dataDataGridViewTextBoxColumn.Width = 75;
		this.channelDataGridViewTextBoxColumn.DataPropertyName = "Channel";
		this.channelDataGridViewTextBoxColumn.HeaderText = "Channel";
		this.channelDataGridViewTextBoxColumn.Name = "channelDataGridViewTextBoxColumn";
		this.channelDataGridViewTextBoxColumn.ReadOnly = true;
		this.channelDataGridViewTextBoxColumn.Width = 75;
		this.voiceDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.voiceDataGridViewTextBoxColumn.DataPropertyName = "Voice";
		this.voiceDataGridViewTextBoxColumn.HeaderText = "Event";
		this.voiceDataGridViewTextBoxColumn.Name = "voiceDataGridViewTextBoxColumn";
		this.voiceDataGridViewTextBoxColumn.ReadOnly = true;
		this.midiDiagListBindingSource.DataSource = typeof(Midi2Cat.Data.MidiDiagList);
		this.mapInDialogTabPage.Controls.Add(this.mapInCtrl2CmdPanel);
		this.mapInDialogTabPage.Location = new System.Drawing.Point(4, 22);
		this.mapInDialogTabPage.Name = "mapInDialogTabPage";
		this.mapInDialogTabPage.Padding = new System.Windows.Forms.Padding(3);
		this.mapInDialogTabPage.Size = new System.Drawing.Size(626, 640);
		this.mapInDialogTabPage.TabIndex = 3;
		this.mapInDialogTabPage.Text = "Maping Dialogs";
		this.mapInDialogTabPage.UseVisualStyleBackColor = true;
		this.mapInCtrl2CmdPanel.BackColor = System.Drawing.SystemColors.Control;
		this.mapInCtrl2CmdPanel.Controls.Add(this.advancedMappingPanel);
		this.mapInCtrl2CmdPanel.Controls.Add(this.mappingPromptPanel2);
		this.mapInCtrl2CmdPanel.Controls.Add(this.mappingControlTypeCB2);
		this.mapInCtrl2CmdPanel.Controls.Add(this.mappingAvailableCmdsLB);
		this.mapInCtrl2CmdPanel.Controls.Add(this.mappingMessageLabel2);
		this.mapInCtrl2CmdPanel.Controls.Add(this.mappingValueLabel2);
		this.mapInCtrl2CmdPanel.Controls.Add(this.label14);
		this.mapInCtrl2CmdPanel.Controls.Add(this.mappingBackButton2);
		this.mapInCtrl2CmdPanel.Controls.Add(this.mappingDoneButton2);
		this.mapInCtrl2CmdPanel.Controls.Add(this.mappingControlNameTB2);
		this.mapInCtrl2CmdPanel.Controls.Add(this.label15);
		this.mapInCtrl2CmdPanel.Controls.Add(this.label17);
		this.mapInCtrl2CmdPanel.Controls.Add(this.mappingMinValueLabel2);
		this.mapInCtrl2CmdPanel.Controls.Add(this.label19);
		this.mapInCtrl2CmdPanel.Controls.Add(this.mappingMaxValueLabel2);
		this.mapInCtrl2CmdPanel.Controls.Add(this.label21);
		this.mapInCtrl2CmdPanel.Controls.Add(this.mappingControlIdLabel2);
		this.mapInCtrl2CmdPanel.Controls.Add(this.label23);
		this.mapInCtrl2CmdPanel.Controls.Add(this.label12);
		this.mapInCtrl2CmdPanel.Controls.Add(this.advancedLinkLabel);
		this.mapInCtrl2CmdPanel.Location = new System.Drawing.Point(6, 16);
		this.mapInCtrl2CmdPanel.MinimumSize = new System.Drawing.Size(404, 306);
		this.mapInCtrl2CmdPanel.Name = "mapInCtrl2CmdPanel";
		this.mapInCtrl2CmdPanel.Size = new System.Drawing.Size(615, 565);
		this.mapInCtrl2CmdPanel.TabIndex = 1;
		this.advancedMappingPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.advancedMappingPanel.BackColor = System.Drawing.Color.White;
		this.advancedMappingPanel.Controls.Add(this.newValueReceivedEventTB);
		this.advancedMappingPanel.Controls.Add(this.label10);
		this.advancedMappingPanel.Controls.Add(this.eventMappingPrompt);
		this.advancedMappingPanel.Controls.Add(this.buttonUpEventTB);
		this.advancedMappingPanel.Controls.Add(this.label9);
		this.advancedMappingPanel.Controls.Add(this.buttonDownEventTB);
		this.advancedMappingPanel.Controls.Add(this.label8);
		this.advancedMappingPanel.Controls.Add(this.label6);
		this.advancedMappingPanel.Controls.Add(this.label7);
		this.advancedMappingPanel.Location = new System.Drawing.Point(396, 7);
		this.advancedMappingPanel.Name = "advancedMappingPanel";
		this.advancedMappingPanel.Size = new System.Drawing.Size(216, 510);
		this.advancedMappingPanel.TabIndex = 24;
		this.newValueReceivedEventTB.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.newValueReceivedEventTB.Location = new System.Drawing.Point(2, 340);
		this.newValueReceivedEventTB.Name = "newValueReceivedEventTB";
		this.newValueReceivedEventTB.Size = new System.Drawing.Size(207, 20);
		this.newValueReceivedEventTB.TabIndex = 31;
		this.newValueReceivedEventTB.TextChanged += new System.EventHandler(validateDialogInput);
		this.label10.AutoSize = true;
		this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label10.Location = new System.Drawing.Point(3, 324);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(142, 13);
		this.label10.TabIndex = 30;
		this.label10.Text = "New Value Received Event:";
		this.eventMappingPrompt.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.eventMappingPrompt.Location = new System.Drawing.Point(0, 446);
		this.eventMappingPrompt.Name = "eventMappingPrompt";
		this.eventMappingPrompt.Size = new System.Drawing.Size(210, 64);
		this.eventMappingPrompt.TabIndex = 29;
		this.eventMappingPrompt.Text = "Your mappings are formatted correctly.";
		this.buttonUpEventTB.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.buttonUpEventTB.Location = new System.Drawing.Point(2, 420);
		this.buttonUpEventTB.Name = "buttonUpEventTB";
		this.buttonUpEventTB.Size = new System.Drawing.Size(207, 20);
		this.buttonUpEventTB.TabIndex = 28;
		this.buttonUpEventTB.TextChanged += new System.EventHandler(validateDialogInput);
		this.label9.AutoSize = true;
		this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label9.Location = new System.Drawing.Point(3, 404);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(89, 13);
		this.label9.TabIndex = 27;
		this.label9.Text = "Button Up Event:";
		this.buttonDownEventTB.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.buttonDownEventTB.Location = new System.Drawing.Point(2, 379);
		this.buttonDownEventTB.Name = "buttonDownEventTB";
		this.buttonDownEventTB.Size = new System.Drawing.Size(207, 20);
		this.buttonDownEventTB.TabIndex = 26;
		this.buttonDownEventTB.TextChanged += new System.EventHandler(validateDialogInput);
		this.label8.AutoSize = true;
		this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label8.Location = new System.Drawing.Point(3, 363);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(103, 13);
		this.label8.TabIndex = 25;
		this.label8.Text = "Button Down Event:";
		this.label6.Location = new System.Drawing.Point(3, 7);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(209, 13);
		this.label6.TabIndex = 23;
		this.label6.Text = "Advanced Options";
		this.label6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label7.Location = new System.Drawing.Point(3, 34);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(209, 290);
		this.label7.TabIndex = 24;
		this.label7.Text = resources.GetString("label7.Text");
		this.mappingPromptPanel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.mappingPromptPanel2.Controls.Add(this.label1);
		this.mappingPromptPanel2.Controls.Add(this.mappingPromptLabel2);
		this.mappingPromptPanel2.Location = new System.Drawing.Point(30, 124);
		this.mappingPromptPanel2.Name = "mappingPromptPanel2";
		this.mappingPromptPanel2.Size = new System.Drawing.Size(333, 165);
		this.mappingPromptPanel2.TabIndex = 20;
		this.label1.BackColor = System.Drawing.Color.White;
		this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label1.Location = new System.Drawing.Point(13, 6);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(308, 41);
		this.label1.TabIndex = 5;
		this.label1.Text = "You operated one of the controls on your controller.";
		this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.mappingPromptLabel2.BackColor = System.Drawing.Color.White;
		this.mappingPromptLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.mappingPromptLabel2.Location = new System.Drawing.Point(13, 47);
		this.mappingPromptLabel2.Name = "mappingPromptLabel2";
		this.mappingPromptLabel2.Size = new System.Drawing.Size(308, 109);
		this.mappingPromptLabel2.TabIndex = 4;
		this.mappingPromptLabel2.Text = "You operated one of the controls on your controller.";
		this.mappingControlTypeCB2.DataSource = this.controlTypesBindingSource;
		this.mappingControlTypeCB2.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.mappingControlTypeCB2.Location = new System.Drawing.Point(99, 66);
		this.mappingControlTypeCB2.Name = "mappingControlTypeCB2";
		this.mappingControlTypeCB2.Size = new System.Drawing.Size(110, 21);
		this.mappingControlTypeCB2.TabIndex = 2;
		this.mappingControlTypeCB2.SelectedIndexChanged += new System.EventHandler(mappingControlTypeCB2_SelectedIndexChanged);
		this.mappingControlTypeCB2.Leave += new System.EventHandler(validateDialogInput);
		this.mappingControlTypeCB2.Validating += new System.ComponentModel.CancelEventHandler(validateDialogInput);
		this.controlTypesBindingSource.DataSource = typeof(Midi2Cat.Data.EnumsDB);
		this.mappingAvailableCmdsLB.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.mappingAvailableCmdsLB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.mappingAvailableCmdsLB.DataSource = this.catCmdsFilteredBindingSource;
		this.mappingAvailableCmdsLB.DisplayMember = "ds";
		this.mappingAvailableCmdsLB.FormattingEnabled = true;
		this.mappingAvailableCmdsLB.Location = new System.Drawing.Point(21, 112);
		this.mappingAvailableCmdsLB.Name = "mappingAvailableCmdsLB";
		this.mappingAvailableCmdsLB.Size = new System.Drawing.Size(369, 405);
		this.mappingAvailableCmdsLB.TabIndex = 3;
		this.mappingAvailableCmdsLB.SelectedIndexChanged += new System.EventHandler(mappingAvailableCmdsLB_SelectedIndexChanged);
		this.mappingAvailableCmdsLB.Validating += new System.ComponentModel.CancelEventHandler(validateDialogInput);
		this.catCmdsFilteredBindingSource.DataSource = typeof(Midi2Cat.Data.EnumsDB);
		this.catCmdsFilteredBindingSource.Filter = "";
		this.mappingMessageLabel2.AutoSize = true;
		this.mappingMessageLabel2.Location = new System.Drawing.Point(214, 70);
		this.mappingMessageLabel2.Name = "mappingMessageLabel2";
		this.mappingMessageLabel2.Size = new System.Drawing.Size(36, 13);
		this.mappingMessageLabel2.TabIndex = 17;
		this.mappingMessageLabel2.Text = "Msg...";
		this.mappingValueLabel2.AutoSize = true;
		this.mappingValueLabel2.Location = new System.Drawing.Point(175, 14);
		this.mappingValueLabel2.Name = "mappingValueLabel2";
		this.mappingValueLabel2.Size = new System.Drawing.Size(34, 13);
		this.mappingValueLabel2.TabIndex = 16;
		this.mappingValueLabel2.Text = "Value";
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(140, 14);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(37, 13);
		this.label14.TabIndex = 15;
		this.label14.Text = "Value:";
		this.mappingBackButton2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.mappingBackButton2.Location = new System.Drawing.Point(519, 534);
		this.mappingBackButton2.Name = "mappingBackButton2";
		this.mappingBackButton2.Size = new System.Drawing.Size(75, 23);
		this.mappingBackButton2.TabIndex = 14;
		this.mappingBackButton2.Text = "Back";
		this.mappingBackButton2.UseVisualStyleBackColor = true;
		this.mappingBackButton2.Click += new System.EventHandler(mappingBackButton2_Click);
		this.mappingDoneButton2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.mappingDoneButton2.Location = new System.Drawing.Point(17, 534);
		this.mappingDoneButton2.Name = "mappingDoneButton2";
		this.mappingDoneButton2.Size = new System.Drawing.Size(75, 23);
		this.mappingDoneButton2.TabIndex = 13;
		this.mappingDoneButton2.Text = "Done";
		this.mappingDoneButton2.UseVisualStyleBackColor = true;
		this.mappingDoneButton2.Click += new System.EventHandler(mappingDoneButton2_Click);
		this.mappingControlNameTB2.Location = new System.Drawing.Point(99, 38);
		this.mappingControlNameTB2.Name = "mappingControlNameTB2";
		this.mappingControlNameTB2.Size = new System.Drawing.Size(291, 20);
		this.mappingControlNameTB2.TabIndex = 1;
		this.mappingControlNameTB2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(mappingControlNameTB2_KeyPress);
		this.mappingControlNameTB2.Leave += new System.EventHandler(validateDialogInput);
		this.mappingControlNameTB2.Validating += new System.ComponentModel.CancelEventHandler(validateDialogInput);
		this.label15.AutoSize = true;
		this.label15.Location = new System.Drawing.Point(18, 42);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(74, 13);
		this.label15.TabIndex = 11;
		this.label15.Text = "Control Name:";
		this.label17.AutoSize = true;
		this.label17.Location = new System.Drawing.Point(18, 69);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(73, 13);
		this.label17.TabIndex = 9;
		this.label17.Text = "Control Type: ";
		this.mappingMinValueLabel2.AutoSize = true;
		this.mappingMinValueLabel2.Location = new System.Drawing.Point(360, 14);
		this.mappingMinValueLabel2.Name = "mappingMinValueLabel2";
		this.mappingMinValueLabel2.Size = new System.Drawing.Size(34, 13);
		this.mappingMinValueLabel2.TabIndex = 8;
		this.mappingMinValueLabel2.Text = "Value";
		this.label19.AutoSize = true;
		this.label19.Location = new System.Drawing.Point(306, 14);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(57, 13);
		this.label19.TabIndex = 7;
		this.label19.Text = "Min Value:";
		this.mappingMaxValueLabel2.AutoSize = true;
		this.mappingMaxValueLabel2.Location = new System.Drawing.Point(271, 14);
		this.mappingMaxValueLabel2.Name = "mappingMaxValueLabel2";
		this.mappingMaxValueLabel2.Size = new System.Drawing.Size(34, 13);
		this.mappingMaxValueLabel2.TabIndex = 6;
		this.mappingMaxValueLabel2.Text = "Value";
		this.label21.AutoSize = true;
		this.label21.Location = new System.Drawing.Point(214, 14);
		this.label21.Name = "label21";
		this.label21.Size = new System.Drawing.Size(60, 13);
		this.label21.TabIndex = 5;
		this.label21.Text = "Max Value:";
		this.mappingControlIdLabel2.AutoSize = true;
		this.mappingControlIdLabel2.Location = new System.Drawing.Point(74, 14);
		this.mappingControlIdLabel2.Name = "mappingControlIdLabel2";
		this.mappingControlIdLabel2.Size = new System.Drawing.Size(57, 13);
		this.mappingControlIdLabel2.TabIndex = 4;
		this.mappingControlIdLabel2.Text = "Control ID:";
		this.label23.AutoSize = true;
		this.label23.Location = new System.Drawing.Point(18, 14);
		this.label23.Name = "label23";
		this.label23.Size = new System.Drawing.Size(60, 13);
		this.label23.TabIndex = 3;
		this.label23.Text = "Control ID: ";
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(20, 99);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(105, 13);
		this.label12.TabIndex = 22;
		this.label12.Text = "Available Commands";
		this.advancedLinkLabel.AutoSize = true;
		this.advancedLinkLabel.Location = new System.Drawing.Point(268, 98);
		this.advancedLinkLabel.Name = "advancedLinkLabel";
		this.advancedLinkLabel.Size = new System.Drawing.Size(120, 13);
		this.advancedLinkLabel.TabIndex = 26;
		this.advancedLinkLabel.TabStop = true;
		this.advancedLinkLabel.Text = "Hide Advanced Options";
		this.advancedLinkLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.advancedLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(advancedLinkLabel_LinkClicked);
		this.dataGridViewComboBoxColumn1.DataPropertyName = "CatCmdId";
		dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(255, 128, 128);
		this.dataGridViewComboBoxColumn1.DefaultCellStyle = dataGridViewCellStyle2;
		this.dataGridViewComboBoxColumn1.HeaderText = "Cat Cmd";
		this.dataGridViewComboBoxColumn1.Name = "dataGridViewComboBoxColumn1";
		this.dataGridViewComboBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGridViewComboBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.dataGridViewComboBoxColumn2.DataPropertyName = "CatCmdId";
		this.dataGridViewComboBoxColumn2.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.dataGridViewComboBoxColumn2.HeaderText = "Cat Cmd";
		this.dataGridViewComboBoxColumn2.Name = "dataGridViewComboBoxColumn2";
		this.dataGridViewComboBoxColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGridViewComboBoxColumn2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.dataGridViewComboBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.dataGridViewComboBoxColumn3.DataPropertyName = "CatCmdId";
		this.dataGridViewComboBoxColumn3.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.dataGridViewComboBoxColumn3.HeaderText = "Cat Cmd";
		this.dataGridViewComboBoxColumn3.Name = "dataGridViewComboBoxColumn3";
		this.dataGridViewComboBoxColumn3.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGridViewComboBoxColumn3.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.dataGridViewComboBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.dataGridViewComboBoxColumn4.DataPropertyName = "CatCmdId";
		this.dataGridViewComboBoxColumn4.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.dataGridViewComboBoxColumn4.HeaderText = "Cat Cmd";
		this.dataGridViewComboBoxColumn4.Name = "dataGridViewComboBoxColumn4";
		this.dataGridViewComboBoxColumn4.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGridViewComboBoxColumn4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.dataGridViewComboBoxColumn5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.dataGridViewComboBoxColumn5.DataPropertyName = "CatCmdId";
		this.dataGridViewComboBoxColumn5.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
		this.dataGridViewComboBoxColumn5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.dataGridViewComboBoxColumn5.HeaderText = "Cat Cmd";
		this.dataGridViewComboBoxColumn5.Name = "dataGridViewComboBoxColumn5";
		this.dataGridViewComboBoxColumn5.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGridViewComboBoxColumn5.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.dataGridViewComboBoxColumn6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.dataGridViewComboBoxColumn6.DataPropertyName = "CatCmdId";
		this.dataGridViewComboBoxColumn6.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
		this.dataGridViewComboBoxColumn6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.dataGridViewComboBoxColumn6.HeaderText = "Cat Cmd";
		this.dataGridViewComboBoxColumn6.Name = "dataGridViewComboBoxColumn6";
		this.dataGridViewComboBoxColumn6.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGridViewComboBoxColumn6.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.dataGridViewComboBoxColumn7.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.dataGridViewComboBoxColumn7.DataPropertyName = "CatCmdId";
		this.dataGridViewComboBoxColumn7.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
		this.dataGridViewComboBoxColumn7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.dataGridViewComboBoxColumn7.HeaderText = "Cat Cmd";
		this.dataGridViewComboBoxColumn7.Name = "dataGridViewComboBoxColumn7";
		this.dataGridViewComboBoxColumn7.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGridViewComboBoxColumn7.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.catCmdsBindingSource.DataSource = typeof(Midi2Cat.Data.EnumsDB);
		this.catCmdsBindingSource.Filter = "";
		this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.fileButton, this.LoadedMappingLabel });
		this.toolStrip.Location = new System.Drawing.Point(0, 0);
		this.toolStrip.Name = "toolStrip";
		this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
		this.toolStrip.Size = new System.Drawing.Size(634, 25);
		this.toolStrip.TabIndex = 13;
		this.toolStrip.Text = "toolStrip1";
		this.fileButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[6] { this.loadMappingToolStripMenuItem, this.saveMappingToolStripMenuItem, this.organiseMappingsToolStripMenuItem, this.toolStripMenuItem1, this.importMappingsToolStripMenuItem, this.exportMappingsToolStripMenuItem });
		this.fileButton.ImageTransparentColor = System.Drawing.Color.Lime;
		this.fileButton.Name = "fileButton";
		this.fileButton.Size = new System.Drawing.Size(119, 22);
		this.fileButton.Text = "Manage Mappings";
		this.loadMappingToolStripMenuItem.Name = "loadMappingToolStripMenuItem";
		this.loadMappingToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.loadMappingToolStripMenuItem.Text = "Load Mapping";
		this.loadMappingToolStripMenuItem.Click += new System.EventHandler(loadMappingToolStripMenuItem_Click);
		this.saveMappingToolStripMenuItem.Name = "saveMappingToolStripMenuItem";
		this.saveMappingToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.saveMappingToolStripMenuItem.Text = "Save Mapping As";
		this.saveMappingToolStripMenuItem.Click += new System.EventHandler(saveMappingToolStripMenuItem_Click);
		this.organiseMappingsToolStripMenuItem.Name = "organiseMappingsToolStripMenuItem";
		this.organiseMappingsToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.organiseMappingsToolStripMenuItem.Text = "Organise Mappings";
		this.organiseMappingsToolStripMenuItem.Click += new System.EventHandler(organiseMappingsToolStripMenuItem_Click);
		this.toolStripMenuItem1.Name = "toolStripMenuItem1";
		this.toolStripMenuItem1.Size = new System.Drawing.Size(174, 6);
		this.importMappingsToolStripMenuItem.Name = "importMappingsToolStripMenuItem";
		this.importMappingsToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.importMappingsToolStripMenuItem.Text = "Import Mappings";
		this.importMappingsToolStripMenuItem.Click += new System.EventHandler(importMappingsToolStripMenuItem_Click);
		this.exportMappingsToolStripMenuItem.Name = "exportMappingsToolStripMenuItem";
		this.exportMappingsToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
		this.exportMappingsToolStripMenuItem.Text = "Export Mappings";
		this.exportMappingsToolStripMenuItem.Click += new System.EventHandler(exportMappingsToolStripMenuItem_Click);
		this.LoadedMappingLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
		this.LoadedMappingLabel.Name = "LoadedMappingLabel";
		this.LoadedMappingLabel.Size = new System.Drawing.Size(94, 22);
		this.LoadedMappingLabel.Text = "LoadedMapping";
		this.midiControlIdDataGridViewTextBoxColumn.DataPropertyName = "MidiControlId";
		this.midiControlIdDataGridViewTextBoxColumn.HeaderText = "Id";
		this.midiControlIdDataGridViewTextBoxColumn.Name = "midiControlIdDataGridViewTextBoxColumn";
		this.midiControlIdDataGridViewTextBoxColumn.Width = 30;
		this.midiControlNameDataGridViewTextBoxColumn.DataPropertyName = "MidiControlName";
		this.midiControlNameDataGridViewTextBoxColumn.HeaderText = "Control Name";
		this.midiControlNameDataGridViewTextBoxColumn.Name = "midiControlNameDataGridViewTextBoxColumn";
		this.midiControlTypeColumn.DataPropertyName = "MidiControlType";
		this.midiControlTypeColumn.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
		this.midiControlTypeColumn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.midiControlTypeColumn.HeaderText = "Control Type";
		this.midiControlTypeColumn.Name = "midiControlTypeColumn";
		this.midiControlTypeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.midiControlTypeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.minValueDataGridViewTextBoxColumn.DataPropertyName = "MinValue";
		this.minValueDataGridViewTextBoxColumn.HeaderText = "Min";
		this.minValueDataGridViewTextBoxColumn.Name = "minValueDataGridViewTextBoxColumn";
		this.minValueDataGridViewTextBoxColumn.ReadOnly = true;
		this.minValueDataGridViewTextBoxColumn.Width = 30;
		this.maxValueDataGridViewTextBoxColumn.DataPropertyName = "MaxValue";
		this.maxValueDataGridViewTextBoxColumn.HeaderText = "Max";
		this.maxValueDataGridViewTextBoxColumn.Name = "maxValueDataGridViewTextBoxColumn";
		this.maxValueDataGridViewTextBoxColumn.ReadOnly = true;
		this.maxValueDataGridViewTextBoxColumn.Width = 30;
		this.CatCmdIdColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.CatCmdIdColumn.DataPropertyName = "CatCmdId";
		this.CatCmdIdColumn.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
		this.CatCmdIdColumn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.CatCmdIdColumn.HeaderText = "Cat Cmd";
		this.CatCmdIdColumn.Name = "CatCmdIdColumn";
		this.CatCmdIdColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.CatCmdIdColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.EditColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
		this.EditColumn.HeaderText = "Edit";
		this.EditColumn.Name = "EditColumn";
		this.EditColumn.ReadOnly = true;
		this.EditColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.EditColumn.Text = "Edit";
		this.EditColumn.TrackVisitedState = false;
		this.EditColumn.UseColumnTextForLinkValue = true;
		this.EditColumn.Width = 30;
		this.deleteColumn.HeaderText = "Delete";
		this.deleteColumn.Name = "deleteColumn";
		this.deleteColumn.Text = "Delete";
		this.deleteColumn.UseColumnTextForLinkValue = true;
		this.deleteColumn.Width = 45;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.Window;
		base.Controls.Add(this.tabControl);
		base.Controls.Add(this.toolStrip);
		base.Name = "MidiDeviceSetup";
		base.Size = new System.Drawing.Size(634, 691);
		base.Load += new System.EventHandler(MidiDeviceSetup_Load);
		base.Resize += new System.EventHandler(MidiDeviceSetup_Resize);
		this.tabControl.ResumeLayout(false);
		this.mappedControlsTab.ResumeLayout(false);
		this.promptPanel.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.mapControlToCommandGrid).EndInit();
		((System.ComponentModel.ISupportInitialize)this.controllerMappingBindingSource1).EndInit();
		this.commandsTabPage.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.mappedCommandsGridView).EndInit();
		((System.ComponentModel.ISupportInitialize)this.mappedCommandsBindingSource).EndInit();
		this.debugTab.ResumeLayout(false);
		this.debugTab.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.midiDiagDataGrid).EndInit();
		((System.ComponentModel.ISupportInitialize)this.midiDiagListBindingSource).EndInit();
		this.mapInDialogTabPage.ResumeLayout(false);
		this.mapInCtrl2CmdPanel.ResumeLayout(false);
		this.mapInCtrl2CmdPanel.PerformLayout();
		this.advancedMappingPanel.ResumeLayout(false);
		this.advancedMappingPanel.PerformLayout();
		this.mappingPromptPanel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.controlTypesBindingSource).EndInit();
		((System.ComponentModel.ISupportInitialize)this.catCmdsFilteredBindingSource).EndInit();
		((System.ComponentModel.ISupportInitialize)this.catCmdsBindingSource).EndInit();
		this.toolStrip.ResumeLayout(false);
		this.toolStrip.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
