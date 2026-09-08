using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class AndromedaEditForm : Form
{
	private readonly Console console;

	private Button BtnClose;

	private TabControl tabControl1;

	private TabPage tabPage1;

	private DataGridView EncoderDataGridView;

	private TabPage tabPage2;

	private DataGridView ButtonDataGridView;

	private TabPage tabPage3;

	private TabPage tabPage4;

	private IContainer components;

	private DataGridView IndicatorDataGridView;

	private DataGridView MenuDataGridView;

	private DataSet UserData;

	private DataSet StringData;

	private Button btnSave;

	private Button btnReset;

	private int NumEncoders;

	private int NumPushbuttons;

	private Button btnDelete;

	private Button btnInsert;

	private int NumMenuItems;

	private bool FormInitialised;

	private Button btnG2Reset;

	private bool IsVisible;

	public bool AndromedaEditorVisible => IsVisible;

	public AndromedaEditForm(Console c)
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.AndromedaEditForm));
		this.BtnClose = new System.Windows.Forms.Button();
		this.tabControl1 = new System.Windows.Forms.TabControl();
		this.tabPage1 = new System.Windows.Forms.TabPage();
		this.EncoderDataGridView = new System.Windows.Forms.DataGridView();
		this.tabPage2 = new System.Windows.Forms.TabPage();
		this.ButtonDataGridView = new System.Windows.Forms.DataGridView();
		this.tabPage3 = new System.Windows.Forms.TabPage();
		this.IndicatorDataGridView = new System.Windows.Forms.DataGridView();
		this.tabPage4 = new System.Windows.Forms.TabPage();
		this.MenuDataGridView = new System.Windows.Forms.DataGridView();
		this.btnDelete = new System.Windows.Forms.Button();
		this.btnInsert = new System.Windows.Forms.Button();
		this.btnSave = new System.Windows.Forms.Button();
		this.btnReset = new System.Windows.Forms.Button();
		this.btnG2Reset = new System.Windows.Forms.Button();
		this.tabControl1.SuspendLayout();
		this.tabPage1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.EncoderDataGridView).BeginInit();
		this.tabPage2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.ButtonDataGridView).BeginInit();
		this.tabPage3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.IndicatorDataGridView).BeginInit();
		this.tabPage4.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.MenuDataGridView).BeginInit();
		base.SuspendLayout();
		this.BtnClose.Location = new System.Drawing.Point(390, 348);
		this.BtnClose.Name = "BtnClose";
		this.BtnClose.Size = new System.Drawing.Size(98, 38);
		this.BtnClose.TabIndex = 0;
		this.BtnClose.Text = "Close";
		this.BtnClose.UseVisualStyleBackColor = true;
		this.BtnClose.Click += new System.EventHandler(BtnClose_Click);
		this.tabControl1.Controls.Add(this.tabPage1);
		this.tabControl1.Controls.Add(this.tabPage2);
		this.tabControl1.Controls.Add(this.tabPage3);
		this.tabControl1.Controls.Add(this.tabPage4);
		this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.tabControl1.Location = new System.Drawing.Point(12, 12);
		this.tabControl1.Name = "tabControl1";
		this.tabControl1.SelectedIndex = 0;
		this.tabControl1.Size = new System.Drawing.Size(780, 330);
		this.tabControl1.TabIndex = 1;
		this.tabControl1.SelectedIndexChanged += new System.EventHandler(TabControl1_SelectedIndexChanged);
		this.tabPage1.Controls.Add(this.EncoderDataGridView);
		this.tabPage1.Location = new System.Drawing.Point(4, 25);
		this.tabPage1.Name = "tabPage1";
		this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
		this.tabPage1.Size = new System.Drawing.Size(772, 301);
		this.tabPage1.TabIndex = 0;
		this.tabPage1.Text = "Encoders";
		this.tabPage1.UseVisualStyleBackColor = true;
		this.EncoderDataGridView.AllowUserToAddRows = false;
		this.EncoderDataGridView.AllowUserToDeleteRows = false;
		this.EncoderDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.EncoderDataGridView.Location = new System.Drawing.Point(0, 0);
		this.EncoderDataGridView.Name = "EncoderDataGridView";
		this.EncoderDataGridView.RowHeadersWidth = 60;
		this.EncoderDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.EncoderDataGridView.Size = new System.Drawing.Size(769, 302);
		this.EncoderDataGridView.TabIndex = 0;
		this.tabPage2.Controls.Add(this.ButtonDataGridView);
		this.tabPage2.Location = new System.Drawing.Point(4, 25);
		this.tabPage2.Name = "tabPage2";
		this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
		this.tabPage2.Size = new System.Drawing.Size(772, 301);
		this.tabPage2.TabIndex = 1;
		this.tabPage2.Text = "Pushbuttons";
		this.tabPage2.UseVisualStyleBackColor = true;
		this.ButtonDataGridView.AllowUserToAddRows = false;
		this.ButtonDataGridView.AllowUserToDeleteRows = false;
		this.ButtonDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.ButtonDataGridView.Location = new System.Drawing.Point(0, 0);
		this.ButtonDataGridView.Name = "ButtonDataGridView";
		this.ButtonDataGridView.RowHeadersWidth = 60;
		this.ButtonDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.ButtonDataGridView.Size = new System.Drawing.Size(769, 310);
		this.ButtonDataGridView.TabIndex = 0;
		this.tabPage3.Controls.Add(this.IndicatorDataGridView);
		this.tabPage3.Location = new System.Drawing.Point(4, 25);
		this.tabPage3.Name = "tabPage3";
		this.tabPage3.Size = new System.Drawing.Size(772, 301);
		this.tabPage3.TabIndex = 2;
		this.tabPage3.Text = "Indicators";
		this.tabPage3.UseVisualStyleBackColor = true;
		this.IndicatorDataGridView.AllowUserToAddRows = false;
		this.IndicatorDataGridView.AllowUserToDeleteRows = false;
		this.IndicatorDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.IndicatorDataGridView.Location = new System.Drawing.Point(0, 0);
		this.IndicatorDataGridView.Name = "IndicatorDataGridView";
		this.IndicatorDataGridView.RowHeadersWidth = 60;
		this.IndicatorDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.IndicatorDataGridView.Size = new System.Drawing.Size(769, 301);
		this.IndicatorDataGridView.TabIndex = 0;
		this.tabPage4.Controls.Add(this.MenuDataGridView);
		this.tabPage4.Location = new System.Drawing.Point(4, 25);
		this.tabPage4.Name = "tabPage4";
		this.tabPage4.Size = new System.Drawing.Size(772, 301);
		this.tabPage4.TabIndex = 3;
		this.tabPage4.Text = "Menus";
		this.tabPage4.UseVisualStyleBackColor = true;
		this.MenuDataGridView.AllowUserToAddRows = false;
		this.MenuDataGridView.AllowUserToDeleteRows = false;
		this.MenuDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.MenuDataGridView.Location = new System.Drawing.Point(0, 0);
		this.MenuDataGridView.Name = "MenuDataGridView";
		this.MenuDataGridView.RowHeadersWidth = 60;
		this.MenuDataGridView.Size = new System.Drawing.Size(769, 301);
		this.MenuDataGridView.TabIndex = 0;
		this.MenuDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(MenuDataGridView_CellValueChanged);
		this.btnDelete.Location = new System.Drawing.Point(682, 348);
		this.btnDelete.Name = "btnDelete";
		this.btnDelete.Size = new System.Drawing.Size(86, 38);
		this.btnDelete.TabIndex = 5;
		this.btnDelete.Text = "Delete Menu";
		this.btnDelete.UseVisualStyleBackColor = true;
		this.btnDelete.Visible = false;
		this.btnDelete.Click += new System.EventHandler(btnDelete_Click);
		this.btnInsert.Location = new System.Drawing.Point(570, 348);
		this.btnInsert.Name = "btnInsert";
		this.btnInsert.Size = new System.Drawing.Size(86, 38);
		this.btnInsert.TabIndex = 4;
		this.btnInsert.Text = "Insert Menu";
		this.btnInsert.UseVisualStyleBackColor = true;
		this.btnInsert.Visible = false;
		this.btnInsert.Click += new System.EventHandler(BtnInsert_Click);
		this.btnSave.Location = new System.Drawing.Point(273, 348);
		this.btnSave.Name = "btnSave";
		this.btnSave.Size = new System.Drawing.Size(100, 38);
		this.btnSave.TabIndex = 2;
		this.btnSave.Text = "Save";
		this.btnSave.UseVisualStyleBackColor = true;
		this.btnSave.Click += new System.EventHandler(BtnSave_Click);
		this.btnReset.Location = new System.Drawing.Point(33, 348);
		this.btnReset.Name = "btnReset";
		this.btnReset.Size = new System.Drawing.Size(114, 38);
		this.btnReset.TabIndex = 3;
		this.btnReset.Text = "Reset Andromeda Data";
		this.btnReset.UseVisualStyleBackColor = true;
		this.btnReset.Click += new System.EventHandler(BtnReset_Click);
		this.btnG2Reset.Location = new System.Drawing.Point(161, 348);
		this.btnG2Reset.Name = "btnG2Reset";
		this.btnG2Reset.Size = new System.Drawing.Size(96, 38);
		this.btnG2Reset.TabIndex = 6;
		this.btnG2Reset.Text = "Reset G2 Panel Data";
		this.btnG2Reset.UseVisualStyleBackColor = true;
		this.btnG2Reset.Click += new System.EventHandler(btnG2Reset_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(804, 392);
		base.Controls.Add(this.btnG2Reset);
		base.Controls.Add(this.btnDelete);
		base.Controls.Add(this.btnReset);
		base.Controls.Add(this.btnInsert);
		base.Controls.Add(this.btnSave);
		base.Controls.Add(this.tabControl1);
		base.Controls.Add(this.BtnClose);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "AndromedaEditForm";
		this.Text = "Andromeda Settings Editor";
		base.Activated += new System.EventHandler(AndromedaEditForm_Activated);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(AndromedaEditForm_FormClosing);
		base.Load += new System.EventHandler(AndromedaEditForm_Load);
		this.tabControl1.ResumeLayout(false);
		this.tabPage1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.EncoderDataGridView).EndInit();
		this.tabPage2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.ButtonDataGridView).EndInit();
		this.tabPage3.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.IndicatorDataGridView).EndInit();
		this.tabPage4.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.MenuDataGridView).EndInit();
		base.ResumeLayout(false);
	}

	private void BtnClose_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void AndromedaEditForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		Hide();
		IsVisible = false;
		e.Cancel = true;
	}

	private void AndromedaEditForm_Activated(object sender, EventArgs e)
	{
		UserData = console.AndromedaSettings;
		StringData = console.AndromedaComboStrings;
		if (!FormInitialised)
		{
			EncoderDataGridView.AutoGenerateColumns = false;
			EncoderDataGridView.DataSource = UserData.Tables[2];
			EncoderDataGridView.TopLeftHeaderCell.Value = "Encoder";
			NumEncoders = UserData.Tables[2].Rows.Count;
			DataGridViewComboBoxColumn dataGridViewColumn = new DataGridViewComboBoxColumn
			{
				DataPropertyName = "Encoder Action",
				HeaderText = "Encoder Action",
				Width = 200,
				DataSource = StringData.Tables["Encoder Combo Strings"],
				ValueMember = "ActionId",
				DisplayMember = "ActionString",
				DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
			};
			EncoderDataGridView.Columns.Add(dataGridViewColumn);
			DataGridViewComboBoxColumn dataGridViewColumn2 = new DataGridViewComboBoxColumn
			{
				DataPropertyName = "Encoder RX Selector",
				HeaderText = "Selected RX",
				Width = 160,
				DataSource = StringData.Tables["Encoder RX Override Strings"],
				ValueMember = "OvrId",
				DisplayMember = "OvrString",
				DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
			};
			EncoderDataGridView.Columns.Add(dataGridViewColumn2);
			DisplayRowNumbers(EncoderDataGridView);
			ButtonDataGridView.AutoGenerateColumns = false;
			ButtonDataGridView.DataSource = UserData.Tables[1];
			ButtonDataGridView.TopLeftHeaderCell.Value = "Button";
			NumPushbuttons = UserData.Tables[1].Rows.Count;
			DataGridViewComboBoxColumn dataGridViewColumn3 = new DataGridViewComboBoxColumn
			{
				DataPropertyName = "Pushbutton Action",
				HeaderText = "Pushbutton Action",
				Width = 240,
				DataSource = StringData.Tables["Pushbutton Combo Strings"],
				ValueMember = "ActionId",
				DisplayMember = "ActionString",
				DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
			};
			ButtonDataGridView.Columns.Add(dataGridViewColumn3);
			DataGridViewComboBoxColumn dataGridViewColumn4 = new DataGridViewComboBoxColumn
			{
				DataPropertyName = "Pushbutton RX Selector",
				HeaderText = "Selected RX",
				Width = 160,
				DataSource = StringData.Tables["Pushbutton RX Override Strings"],
				ValueMember = "OvrId",
				DisplayMember = "OvrString",
				DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
			};
			ButtonDataGridView.Columns.Add(dataGridViewColumn4);
			DisplayRowNumbers(ButtonDataGridView);
			IndicatorDataGridView.AutoGenerateColumns = false;
			IndicatorDataGridView.DataSource = UserData.Tables[0];
			IndicatorDataGridView.TopLeftHeaderCell.Value = "Indicator";
			DataGridViewComboBoxColumn dataGridViewColumn5 = new DataGridViewComboBoxColumn
			{
				DataPropertyName = "Indicator Action",
				HeaderText = "Indicators shows:",
				Width = 200,
				DataSource = StringData.Tables["Indicator Combo Strings"],
				ValueMember = "ActionId",
				DisplayMember = "ActionString",
				DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
			};
			IndicatorDataGridView.Columns.Add(dataGridViewColumn5);
			DataGridViewComboBoxColumn dataGridViewColumn6 = new DataGridViewComboBoxColumn
			{
				DataPropertyName = "Indicator RX Selector",
				HeaderText = "Selected RX",
				Width = 200,
				DataSource = StringData.Tables["Indicator RX Override Strings"],
				ValueMember = "OvrId",
				DisplayMember = "OvrString",
				DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
			};
			IndicatorDataGridView.Columns.Add(dataGridViewColumn6);
			DisplayRowNumbers(IndicatorDataGridView);
			MenuDataGridView.AutoGenerateColumns = false;
			MenuDataGridView.DataSource = UserData.Tables[4];
			NumMenuItems = UserData.Tables[4].Rows.Count;
			MenuDataGridView.TopLeftHeaderCell.Value = "Menu";
			DataGridViewComboBoxColumn dataGridViewColumn7 = new DataGridViewComboBoxColumn
			{
				DataPropertyName = "Menu Action",
				HeaderText = "Menu button action",
				Width = 250,
				DataSource = StringData.Tables["Pushbutton Combo Strings"],
				ValueMember = "ActionId",
				DisplayMember = "ActionString",
				DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
			};
			MenuDataGridView.Columns.Add(dataGridViewColumn7);
			DataGridViewTextBoxColumn dataGridViewColumn8 = new DataGridViewTextBoxColumn
			{
				DataPropertyName = "Menu Text",
				HeaderText = "Button Text:",
				Width = 220
			};
			MenuDataGridView.Columns.Add(dataGridViewColumn8);
			DataGridViewComboBoxColumn dataGridViewColumn9 = new DataGridViewComboBoxColumn
			{
				DataPropertyName = "Menu RX Selector",
				HeaderText = "Selected RX",
				Width = 120,
				DataSource = StringData.Tables["Encoder RX Override Strings"],
				ValueMember = "OvrId",
				DisplayMember = "OvrString",
				DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
			};
			MenuDataGridView.Columns.Add(dataGridViewColumn9);
			DataGridViewTextBoxColumn dataGridViewColumn10 = new DataGridViewTextBoxColumn
			{
				DataPropertyName = "Menu Number",
				HeaderText = "Link to Menu:",
				Width = 80
			};
			MenuDataGridView.Columns.Add(dataGridViewColumn10);
			DisplayMenuNumbers(MenuDataGridView);
		}
		FormInitialised = true;
		IsVisible = true;
	}

	private void DisplayRowNumbers(DataGridView dgv)
	{
		foreach (DataGridViewRow item in (IEnumerable)dgv.Rows)
		{
			item.HeaderCell.Value = (item.Index + 1).ToString();
		}
	}

	private void DisplayMenuNumbers(DataGridView dgv)
	{
		foreach (DataGridViewRow item in (IEnumerable)dgv.Rows)
		{
			item.HeaderCell.Value = (item.Index / 8 + 1).ToString();
		}
	}

	public void SetEncoderNumber(int Encoder)
	{
		if (Encoder < NumEncoders)
		{
			EncoderDataGridView.CurrentCell = EncoderDataGridView[1, Encoder];
		}
	}

	public void SetPushbuttonNumber(int Button)
	{
		if (Button < NumPushbuttons)
		{
			ButtonDataGridView.CurrentCell = ButtonDataGridView[1, Button];
		}
	}

	private void BtnSave_Click(object sender, EventArgs e)
	{
		console.AndromedaSettings = UserData;
	}

	private void BtnReset_Click(object sender, EventArgs e)
	{
		console.ResetAndromedaDataset();
		AndromedaEditForm_Activated(null, null);
	}

	private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
	{
		switch (tabControl1.SelectedIndex)
		{
		case 0:
			DisplayRowNumbers(EncoderDataGridView);
			btnInsert.Visible = false;
			btnDelete.Visible = false;
			break;
		case 1:
			DisplayRowNumbers(ButtonDataGridView);
			btnInsert.Visible = false;
			btnDelete.Visible = false;
			break;
		case 2:
			DisplayRowNumbers(IndicatorDataGridView);
			btnInsert.Visible = false;
			btnDelete.Visible = false;
			break;
		case 3:
			DisplayMenuNumbers(MenuDataGridView);
			btnInsert.Visible = true;
			btnDelete.Visible = true;
			break;
		}
	}

	private void btnDelete_Click(object sender, EventArgs e)
	{
		int num = MenuDataGridView.CurrentRow.Index / 8 + 1;
		int index = (num - 1) * 8;
		for (int i = 0; i < 8; i++)
		{
			UserData.Tables[4].Rows[index].Delete();
		}
		int count = UserData.Tables[4].Rows.Count;
		int num2 = count / 8;
		for (int i = 0; i < count; i++)
		{
			int num3 = (int)UserData.Tables[4].Rows[i]["Menu Number"];
			if (num3 > num2)
			{
				UserData.Tables[4].Rows[i]["Menu Number"] = 1;
			}
			else if (num3 > num)
			{
				UserData.Tables[4].Rows[i]["Menu Number"] = num3 - 1;
			}
		}
		MenuDataGridView.Refresh();
		DisplayMenuNumbers(MenuDataGridView);
	}

	private void MenuDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
	{
		int columnIndex = e.ColumnIndex;
		if (columnIndex != 0)
		{
			return;
		}
		int rowIndex = e.RowIndex;
		int num = (int)MenuDataGridView.Rows[rowIndex].Cells[columnIndex].Value;
		int count = StringData.Tables["Pushbutton Combo Strings"].Rows.Count;
		for (int i = 0; i < count; i++)
		{
			if ((int)StringData.Tables["Pushbutton Combo Strings"].Rows[i]["ActionId"] == num)
			{
				UserData.Tables[4].Rows[rowIndex]["Menu Text"] = StringData.Tables["Pushbutton Combo Strings"].Rows[i]["MenuText"];
			}
		}
	}

	private void BtnInsert_Click(object sender, EventArgs e)
	{
		int count;
		int num = (count = UserData.Tables[4].Rows.Count);
		int index = MenuDataGridView.CurrentRow.Index;
		int num2 = index / 8 + 1;
		int num3 = (num2 - 1) * 8;
		if (index - num3 >= 4)
		{
			num3 += 8;
		}
		if (num3 < num)
		{
			for (int i = 0; i < 8; i++)
			{
				DataRow dataRow = UserData.Tables[4].NewRow();
				dataRow["Menu button Number"] = 0;
				dataRow["Menu Action"] = 0;
				dataRow["Menu Text"] = "---";
				dataRow["Menu RX Selector"] = 0;
				dataRow["Menu Number"] = 0;
				UserData.Tables[4].Rows.InsertAt(dataRow, num3);
			}
			for (int i = 0; i < num; i++)
			{
				count = (int)UserData.Tables[4].Rows[i]["Menu Number"];
				if (count >= num2)
				{
					UserData.Tables[4].Rows[i]["Menu Number"] = count + 1;
				}
			}
		}
		else
		{
			UserData.Tables[4].Rows.Add(0, 0, "----", 0, 0);
			UserData.Tables[4].Rows.Add(0, 0, "----", 0, 0);
			UserData.Tables[4].Rows.Add(0, 0, "----", 0, 0);
			UserData.Tables[4].Rows.Add(0, 0, "----", 0, 0);
			UserData.Tables[4].Rows.Add(0, 0, "----", 0, 0);
			UserData.Tables[4].Rows.Add(0, 0, "----", 0, 0);
			UserData.Tables[4].Rows.Add(0, 0, "----", 0, 0);
			UserData.Tables[4].Rows.Add(0, 0, "----", 0, 0);
		}
		MenuDataGridView.Refresh();
		DisplayMenuNumbers(MenuDataGridView);
	}

	private void AndromedaEditForm_Load(object sender, EventArgs e)
	{
	}

	private void btnG2Reset_Click(object sender, EventArgs e)
	{
		console.ResetG2PanelDataset();
		AndromedaEditForm_Activated(null, null);
	}
}
