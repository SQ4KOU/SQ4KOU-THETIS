using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Midi2Cat.IO;

public class PickDialog : Form
{
	private IContainer components;

	private Button doneButton;

	private Button button2;

	private Label promptLabel;

	private CheckedListBox mappingsLB;

	public string Prompt
	{
		set
		{
			promptLabel.Text = value;
		}
	}

	public string[] Mappings
	{
		get
		{
			List<string> list = new List<string>();
			foreach (object checkedItem in mappingsLB.CheckedItems)
			{
				list.Add((string)checkedItem);
			}
			return list.ToArray();
		}
	}

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

	public PickDialog()
	{
		InitializeComponent();
	}

	private void mappingsLB_ItemCheck(object sender, ItemCheckEventArgs e)
	{
		int count = mappingsLB.CheckedItems.Count;
		count = ((e.NewValue != CheckState.Checked) ? (count - 1) : (count + 1));
		doneButton.Enabled = count > 0;
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
		this.doneButton = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.promptLabel = new System.Windows.Forms.Label();
		this.mappingsLB = new System.Windows.Forms.CheckedListBox();
		base.SuspendLayout();
		this.doneButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.doneButton.Enabled = false;
		this.doneButton.Location = new System.Drawing.Point(12, 240);
		this.doneButton.Name = "doneButton";
		this.doneButton.Size = new System.Drawing.Size(75, 23);
		this.doneButton.TabIndex = 2;
		this.doneButton.Text = "Done";
		this.doneButton.UseVisualStyleBackColor = true;
		this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.button2.Location = new System.Drawing.Point(293, 239);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(75, 23);
		this.button2.TabIndex = 3;
		this.button2.Text = "Cancel";
		this.button2.UseVisualStyleBackColor = true;
		this.promptLabel.AutoSize = true;
		this.promptLabel.Location = new System.Drawing.Point(12, 9);
		this.promptLabel.Name = "promptLabel";
		this.promptLabel.Size = new System.Drawing.Size(138, 13);
		this.promptLabel.TabIndex = 4;
		this.promptLabel.Text = "Pick the mappings to export";
		this.mappingsLB.FormattingEnabled = true;
		this.mappingsLB.Location = new System.Drawing.Point(15, 26);
		this.mappingsLB.Name = "mappingsLB";
		this.mappingsLB.Size = new System.Drawing.Size(353, 199);
		this.mappingsLB.TabIndex = 5;
		this.mappingsLB.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(mappingsLB_ItemCheck);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(380, 268);
		base.Controls.Add(this.mappingsLB);
		base.Controls.Add(this.promptLabel);
		base.Controls.Add(this.button2);
		base.Controls.Add(this.doneButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "PickDialog";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Mapping Picker";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
