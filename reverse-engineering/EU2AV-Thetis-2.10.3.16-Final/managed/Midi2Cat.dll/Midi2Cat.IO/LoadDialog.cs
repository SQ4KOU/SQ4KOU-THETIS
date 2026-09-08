using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Midi2Cat.IO;

public class LoadDialog : Form
{
	private IContainer components;

	private TextBox textBox1;

	private Label label1;

	private Button loadButton;

	private Button button2;

	private Label label2;

	private ListBox mappingsLB;

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

	public LoadDialog()
	{
		InitializeComponent();
	}

	private void textBox1_TextChanged(object sender, EventArgs e)
	{
		loadButton.Enabled = textBox1.Text.Trim().Length > 0;
	}

	private void mappingsLB_SelectedIndexChanged(object sender, EventArgs e)
	{
		textBox1.Text = (string)mappingsLB.SelectedItem;
	}

	private void mappingsLB_DoubleClick(object sender, EventArgs e)
	{
		if (mappingsLB.SelectedItem != null)
		{
			textBox1.Text = (string)mappingsLB.SelectedItem;
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
		this.loadButton = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.label2 = new System.Windows.Forms.Label();
		this.mappingsLB = new System.Windows.Forms.ListBox();
		base.SuspendLayout();
		this.textBox1.Location = new System.Drawing.Point(53, 207);
		this.textBox1.Name = "textBox1";
		this.textBox1.ReadOnly = true;
		this.textBox1.Size = new System.Drawing.Size(315, 20);
		this.textBox1.TabIndex = 0;
		this.textBox1.TextChanged += new System.EventHandler(textBox1_TextChanged);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 210);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(35, 13);
		this.label1.TabIndex = 1;
		this.label1.Text = "Name";
		this.loadButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.loadButton.Enabled = false;
		this.loadButton.Location = new System.Drawing.Point(12, 240);
		this.loadButton.Name = "loadButton";
		this.loadButton.Size = new System.Drawing.Size(75, 23);
		this.loadButton.TabIndex = 2;
		this.loadButton.Text = "Load";
		this.loadButton.UseVisualStyleBackColor = true;
		this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.button2.Location = new System.Drawing.Point(293, 239);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(75, 23);
		this.button2.TabIndex = 3;
		this.button2.Text = "Cancel";
		this.button2.UseVisualStyleBackColor = true;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(12, 9);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(53, 13);
		this.label2.TabIndex = 4;
		this.label2.Text = "Mappings";
		this.mappingsLB.FormattingEnabled = true;
		this.mappingsLB.Location = new System.Drawing.Point(12, 27);
		this.mappingsLB.Name = "mappingsLB";
		this.mappingsLB.Size = new System.Drawing.Size(356, 173);
		this.mappingsLB.TabIndex = 5;
		this.mappingsLB.SelectedIndexChanged += new System.EventHandler(mappingsLB_SelectedIndexChanged);
		this.mappingsLB.DoubleClick += new System.EventHandler(mappingsLB_DoubleClick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(380, 268);
		base.Controls.Add(this.mappingsLB);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.button2);
		base.Controls.Add(this.loadButton);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.textBox1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "LoadDialog";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Load Mapping";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
