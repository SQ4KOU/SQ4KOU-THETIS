using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Midi2Cat.IO;

public class SaveAsDialog : Form
{
	private IContainer components;

	private TextBox textBox1;

	private Label label1;

	private Button saveButton;

	private Button button2;

	private Label label2;

	private ListBox mappingsLB;

	private Label NoDashLab;

	public string MappingName => textBox1.Text;

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

	public SaveAsDialog()
	{
		InitializeComponent();
	}

	private void textBox1_TextChanged(object sender, EventArgs e)
	{
		if (textBox1.Text.Contains('-'))
		{
			NoDashLab.Visible = true;
			saveButton.Enabled = false;
		}
		else
		{
			NoDashLab.Visible = false;
			saveButton.Enabled = textBox1.Text.Trim().Length > 0;
		}
	}

	private void mappingsLB_SelectedIndexChanged(object sender, EventArgs e)
	{
		textBox1.Text = (string)mappingsLB.SelectedItem;
	}

	private void saveButton_Click(object sender, EventArgs e)
	{
		bool flag = true;
		string text = textBox1.Text.Trim().ToLower();
		foreach (object item in mappingsLB.Items)
		{
			if (text == ((string)item).ToLower())
			{
				if (MessageBox.Show("You are about to overwrite an existing saved mapping.\nDo you want to continue?", "Overwrite an existing saved mapping?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
				{
					flag = false;
				}
				break;
			}
		}
		if (flag)
		{
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
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.saveButton = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.label2 = new System.Windows.Forms.Label();
		this.mappingsLB = new System.Windows.Forms.ListBox();
		this.NoDashLab = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.textBox1.Location = new System.Drawing.Point(53, 207);
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(315, 20);
		this.textBox1.TabIndex = 0;
		this.textBox1.TextChanged += new System.EventHandler(textBox1_TextChanged);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 210);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(35, 13);
		this.label1.TabIndex = 1;
		this.label1.Text = "Name";
		this.saveButton.Enabled = false;
		this.saveButton.Location = new System.Drawing.Point(15, 255);
		this.saveButton.Name = "saveButton";
		this.saveButton.Size = new System.Drawing.Size(75, 23);
		this.saveButton.TabIndex = 2;
		this.saveButton.Text = "Save";
		this.saveButton.UseVisualStyleBackColor = true;
		this.saveButton.Click += new System.EventHandler(saveButton_Click);
		this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.button2.Location = new System.Drawing.Point(293, 255);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(75, 23);
		this.button2.TabIndex = 3;
		this.button2.Text = "Cancel";
		this.button2.UseVisualStyleBackColor = true;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(12, 9);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(87, 13);
		this.label2.TabIndex = 4;
		this.label2.Text = "Saved Mappings";
		this.mappingsLB.FormattingEnabled = true;
		this.mappingsLB.Location = new System.Drawing.Point(12, 27);
		this.mappingsLB.Name = "mappingsLB";
		this.mappingsLB.Size = new System.Drawing.Size(356, 173);
		this.mappingsLB.TabIndex = 5;
		this.mappingsLB.SelectedIndexChanged += new System.EventHandler(mappingsLB_SelectedIndexChanged);
		this.NoDashLab.AutoSize = true;
		this.NoDashLab.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.NoDashLab.Location = new System.Drawing.Point(56, 232);
		this.NoDashLab.Name = "NoDashLab";
		this.NoDashLab.Size = new System.Drawing.Size(295, 13);
		this.NoDashLab.TabIndex = 6;
		this.NoDashLab.Text = "Mapping names cannot contain a dash '-' charater.\r\n";
		this.NoDashLab.Visible = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(380, 286);
		base.Controls.Add(this.NoDashLab);
		base.Controls.Add(this.mappingsLB);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.button2);
		base.Controls.Add(this.saveButton);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.textBox1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "SaveAsDialog";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Save Mapping";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
