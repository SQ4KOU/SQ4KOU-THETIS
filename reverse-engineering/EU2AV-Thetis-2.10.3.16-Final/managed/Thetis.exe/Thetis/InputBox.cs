using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class InputBox : Form
{
	private string retval;

	private TextBox textbox;

	private Label label;

	private Button btnOK;

	private Button btnCancel;

	private Container components;

	public InputBox()
	{
		InitializeComponent();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.InputBox));
		this.textbox = new System.Windows.Forms.TextBox();
		this.label = new System.Windows.Forms.Label();
		this.btnOK = new System.Windows.Forms.Button();
		this.btnCancel = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.textbox.Location = new System.Drawing.Point(8, 64);
		this.textbox.Name = "textbox";
		this.textbox.Size = new System.Drawing.Size(192, 20);
		this.textbox.TabIndex = 0;
		this.label.Location = new System.Drawing.Point(8, 16);
		this.label.Name = "label";
		this.label.Size = new System.Drawing.Size(192, 23);
		this.label.TabIndex = 1;
		this.btnOK.Location = new System.Drawing.Point(224, 16);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(72, 23);
		this.btnOK.TabIndex = 2;
		this.btnOK.Text = "OK";
		this.btnOK.Click += new System.EventHandler(btnOK_Click);
		this.btnCancel.Location = new System.Drawing.Point(224, 48);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(72, 23);
		this.btnCancel.TabIndex = 3;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		base.AcceptButton = this.btnOK;
		base.ClientSize = new System.Drawing.Size(304, 102);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnOK);
		base.Controls.Add(this.label);
		base.Controls.Add(this.textbox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "InputBox";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	public static string Show(string title, string label, string textbox, bool to_top = false)
	{
		InputBox inputBox = new InputBox();
		inputBox.TopMost = to_top;
		inputBox.Text = title;
		inputBox.label.Text = label;
		inputBox.textbox.Text = textbox;
		inputBox.ShowDialog();
		string result = inputBox.retval;
		inputBox.Dispose();
		return result;
	}

	private void btnOK_Click(object sender, EventArgs e)
	{
		retval = textbox.Text;
		Close();
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		retval = "";
		Close();
	}
}
