using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class cwedit : Form
{
	private IContainer components;

	private TextBox txtElements;

	private TextBox txtComments;

	private Label label2;

	private Label label3;

	private Label label4;

	private Button saveButton;

	private Label label5;

	private ToolTip toolTip1;

	private Button cancelButton;

	private TextBox txtOriginal;

	private TextBox txtCurrent;

	private Console console;

	private string sedit;

	private string id;

	private string els;

	private string cmnts;

	public cwedit(Console c)
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
		this.components = new System.ComponentModel.Container();
		this.saveButton = new System.Windows.Forms.Button();
		this.txtElements = new System.Windows.Forms.TextBox();
		this.txtComments = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		this.cancelButton = new System.Windows.Forms.Button();
		this.txtOriginal = new System.Windows.Forms.TextBox();
		this.txtCurrent = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.saveButton.Location = new System.Drawing.Point(192, 200);
		this.saveButton.Name = "saveButton";
		this.saveButton.Size = new System.Drawing.Size(64, 24);
		this.saveButton.TabIndex = 0;
		this.saveButton.Text = "Save";
		this.toolTip1.SetToolTip(this.saveButton, " Save new definition and exit.");
		this.saveButton.Click += new System.EventHandler(saveButton_Click);
		this.txtElements.Font = new System.Drawing.Font("Courier New", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txtElements.Location = new System.Drawing.Point(40, 152);
		this.txtElements.MaxLength = 10;
		this.txtElements.Name = "txtElements";
		this.txtElements.Size = new System.Drawing.Size(80, 21);
		this.txtElements.TabIndex = 1;
		this.txtElements.Text = "---...---";
		this.toolTip1.SetToolTip(this.txtElements, " The Morse dots and dashes up to nine.");
		this.txtElements.Leave += new System.EventHandler(txtElements_Leave);
		this.txtComments.Font = new System.Drawing.Font("Courier New", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txtComments.Location = new System.Drawing.Point(40, 208);
		this.txtComments.MaxLength = 11;
		this.txtComments.Name = "txtComments";
		this.txtComments.Size = new System.Drawing.Size(80, 21);
		this.txtComments.TabIndex = 2;
		this.txtComments.Text = "0123456789";
		this.toolTip1.SetToolTip(this.txtComments, " Any comments up to ten characters.");
		this.txtComments.Leave += new System.EventHandler(txtComments_Leave);
		this.label2.Location = new System.Drawing.Point(40, 176);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(56, 16);
		this.label2.TabIndex = 4;
		this.label2.Text = "Elements";
		this.label3.Location = new System.Drawing.Point(40, 232);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(72, 16);
		this.label3.TabIndex = 5;
		this.label3.Text = "Comments";
		this.label4.Location = new System.Drawing.Point(40, 52);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(120, 16);
		this.label4.TabIndex = 6;
		this.label4.Text = "Original Definition";
		this.label5.Location = new System.Drawing.Point(40, 104);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(96, 16);
		this.label5.TabIndex = 8;
		this.label5.Text = "Current Definition";
		this.cancelButton.Location = new System.Drawing.Point(192, 160);
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.Size = new System.Drawing.Size(64, 24);
		this.cancelButton.TabIndex = 9;
		this.cancelButton.Text = "Cancel";
		this.toolTip1.SetToolTip(this.cancelButton, "Cancel and quite without changes.");
		this.cancelButton.Click += new System.EventHandler(cancelButton_Click);
		this.txtOriginal.Font = new System.Drawing.Font("Courier New", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txtOriginal.Location = new System.Drawing.Point(40, 32);
		this.txtOriginal.Name = "txtOriginal";
		this.txtOriginal.ReadOnly = true;
		this.txtOriginal.Size = new System.Drawing.Size(216, 21);
		this.txtOriginal.TabIndex = 10;
		this.txtOriginal.Text = "txtOriginal";
		this.toolTip1.SetToolTip(this.txtOriginal, " The original definition line.");
		this.txtCurrent.Font = new System.Drawing.Font("Courier New", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txtCurrent.Location = new System.Drawing.Point(40, 80);
		this.txtCurrent.Name = "txtCurrent";
		this.txtCurrent.ReadOnly = true;
		this.txtCurrent.Size = new System.Drawing.Size(216, 21);
		this.txtCurrent.TabIndex = 11;
		this.txtCurrent.Text = "txtCurrent";
		this.toolTip1.SetToolTip(this.txtCurrent, " The current definition line.");
		base.AutoScaleDimensions = new System.Drawing.SizeF(5f, 13f);
		base.ClientSize = new System.Drawing.Size(290, 258);
		base.ControlBox = false;
		base.Controls.Add(this.txtCurrent);
		base.Controls.Add(this.txtOriginal);
		base.Controls.Add(this.cancelButton);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.txtComments);
		base.Controls.Add(this.txtElements);
		base.Controls.Add(this.saveButton);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Name = "cwedit";
		this.Text = " CW definition editor ...";
		base.Load += new System.EventHandler(cwedit_Load);
		base.ResumeLayout(false);
	}

	private void extract_fields()
	{
		id = sedit.Substring(0, 5);
		els = sedit.Substring(5, 9);
		cmnts = sedit.Substring(16, 10);
	}

	private void make_current()
	{
		txtCurrent.Text = id + els + "| " + cmnts;
	}

	private void cwedit_Load(object sender, EventArgs e)
	{
		sedit = console.CWXForm.editline;
		txtOriginal.Text = sedit;
		extract_fields();
		txtElements.Text = els;
		txtComments.Text = cmnts;
		make_current();
	}

	private void saveButton_Click(object sender, EventArgs e)
	{
		console.CWXForm.editline = txtCurrent.Text;
		Close();
	}

	private void cancelButton_Click(object sender, EventArgs e)
	{
		if (MessageBox.Show("Do you want to exit without saving?", " CW Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
		{
			console.CWXForm.editline = "";
			Close();
		}
	}

	private string slen(string s, int len)
	{
		if (s.Length < len)
		{
			return s.PadRight(len, ' ');
		}
		if (s.Length > len)
		{
			return s.Substring(0, len);
		}
		return s;
	}

	private void txtComments_Leave(object sender, EventArgs e)
	{
		string s = txtComments.Text;
		s = slen(s, 10);
		txtComments.Text = s;
		cmnts = s;
		make_current();
	}

	private void txtElements_Leave(object sender, EventArgs e)
	{
		string s = txtElements.Text;
		s = slen(s, 9);
		txtElements.Text = s;
		els = s;
		make_current();
	}
}
