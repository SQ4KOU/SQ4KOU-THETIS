using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Midi2Cat.Data;

namespace Midi2Cat.IO;

public class OrganiseDialog : Form
{
	private Midi2CatDatabase DB;

	private string DeviceName;

	private IContainer components;

	private Button loadButton;

	private Label label2;

	private ListBox mappingsLB;

	private Button deleteButton;

	private Button renameButton;

	private TextBox renameTB;

	public string[] ExistingMappings
	{
		set
		{
			mappingsLB.Items.Clear();
			foreach (string item in value)
			{
				mappingsLB.Items.Add(item);
			}
		}
	}

	public OrganiseDialog(Midi2CatDatabase DB, string DeviceName)
	{
		this.DB = DB;
		this.DeviceName = DeviceName;
		InitializeComponent();
	}

	private void mappingsLB_SelectedIndexChanged(object sender, EventArgs e)
	{
		EndRename();
		if (mappingsLB.SelectedIndex >= 0)
		{
			renameButton.Enabled = true;
			deleteButton.Enabled = true;
		}
		else
		{
			renameButton.Enabled = false;
			deleteButton.Enabled = false;
		}
	}

	private void deleteButton_Click(object sender, EventArgs e)
	{
		if (mappingsLB.SelectedItem != null)
		{
			DB.RemoveSavedMapping(DeviceName, (string)mappingsLB.SelectedItem);
			ExistingMappings = DB.GetSavedMappings();
		}
	}

	private void renameButton_Click(object sender, EventArgs e)
	{
		StartRename();
	}

	private void mappingsLB_DoubleClick(object sender, EventArgs e)
	{
		StartRename();
	}

	private void StartRename()
	{
		renameButton.Enabled = false;
		deleteButton.Enabled = false;
		if (mappingsLB.SelectedItem != null)
		{
			mappingsLB.BackColor = Color.Silver;
			renameTB.Left = mappingsLB.Left + 4;
			renameTB.Width = mappingsLB.Width - 6;
			renameTB.Top = mappingsLB.Top + (mappingsLB.SelectedIndex - mappingsLB.TopIndex) * mappingsLB.ItemHeight + 2;
			renameTB.Text = (string)mappingsLB.SelectedItem;
			renameTB.Visible = true;
			renameTB.Focus();
			renameTB.SelectAll();
		}
	}

	private void EndRename()
	{
		renameTB.Visible = false;
		mappingsLB.BackColor = Color.White;
	}

	private void renameTB_Leave(object sender, EventArgs e)
	{
		CommitRename();
	}

	private void renameTB_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Escape)
		{
			EndRename();
		}
		else if (e.KeyCode == Keys.Return)
		{
			CommitRename();
		}
	}

	private void CommitRename()
	{
		string text = (string)mappingsLB.SelectedItem;
		string text2 = renameTB.Text.Trim();
		if (text2.Length > 0 && text.ToLower() != text2.ToLower() && !DB.GetSavedMappings().Contains(text2))
		{
			DB.RenameSavedMapping(DeviceName, text, text2);
		}
		EndRename();
		ExistingMappings = DB.GetSavedMappings();
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
		this.loadButton = new System.Windows.Forms.Button();
		this.label2 = new System.Windows.Forms.Label();
		this.mappingsLB = new System.Windows.Forms.ListBox();
		this.deleteButton = new System.Windows.Forms.Button();
		this.renameButton = new System.Windows.Forms.Button();
		this.renameTB = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.loadButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.loadButton.Location = new System.Drawing.Point(15, 298);
		this.loadButton.Name = "loadButton";
		this.loadButton.Size = new System.Drawing.Size(75, 23);
		this.loadButton.TabIndex = 2;
		this.loadButton.Text = "Done";
		this.loadButton.UseVisualStyleBackColor = true;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(12, 9);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(53, 13);
		this.label2.TabIndex = 4;
		this.label2.Text = "Mappings";
		this.mappingsLB.FormattingEnabled = true;
		this.mappingsLB.Location = new System.Drawing.Point(12, 25);
		this.mappingsLB.Name = "mappingsLB";
		this.mappingsLB.Size = new System.Drawing.Size(356, 264);
		this.mappingsLB.TabIndex = 5;
		this.mappingsLB.SelectedIndexChanged += new System.EventHandler(mappingsLB_SelectedIndexChanged);
		this.mappingsLB.DoubleClick += new System.EventHandler(mappingsLB_DoubleClick);
		this.deleteButton.Enabled = false;
		this.deleteButton.Location = new System.Drawing.Point(212, 298);
		this.deleteButton.Name = "deleteButton";
		this.deleteButton.Size = new System.Drawing.Size(75, 23);
		this.deleteButton.TabIndex = 6;
		this.deleteButton.Text = "Delete";
		this.deleteButton.UseVisualStyleBackColor = true;
		this.deleteButton.Click += new System.EventHandler(deleteButton_Click);
		this.renameButton.Enabled = false;
		this.renameButton.Location = new System.Drawing.Point(293, 298);
		this.renameButton.Name = "renameButton";
		this.renameButton.Size = new System.Drawing.Size(75, 23);
		this.renameButton.TabIndex = 7;
		this.renameButton.Text = "Rename";
		this.renameButton.UseVisualStyleBackColor = true;
		this.renameButton.Click += new System.EventHandler(renameButton_Click);
		this.renameTB.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.renameTB.Location = new System.Drawing.Point(15, 45);
		this.renameTB.Name = "renameTB";
		this.renameTB.Size = new System.Drawing.Size(272, 13);
		this.renameTB.TabIndex = 1;
		this.renameTB.Visible = false;
		this.renameTB.KeyDown += new System.Windows.Forms.KeyEventHandler(renameTB_KeyDown);
		this.renameTB.Leave += new System.EventHandler(renameTB_Leave);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(380, 328);
		base.Controls.Add(this.renameTB);
		base.Controls.Add(this.renameButton);
		base.Controls.Add(this.deleteButton);
		base.Controls.Add(this.mappingsLB);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.loadButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "OrganiseDialog";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Organise Mappings";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
