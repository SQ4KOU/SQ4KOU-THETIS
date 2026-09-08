using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class ModeDependentSettingsForm : Form
{
	private IContainer components;

	private ButtonTS btnClose;

	private Console console;

	public ModeDependentSettingsForm(Console c)
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.ModeDependentSettingsForm));
		this.btnClose = new System.Windows.Forms.ButtonTS();
		base.SuspendLayout();
		this.btnClose.Image = null;
		this.btnClose.Location = new System.Drawing.Point(140, 166);
		this.btnClose.Name = "btnClose";
		this.btnClose.Selectable = true;
		this.btnClose.Size = new System.Drawing.Size(75, 23);
		this.btnClose.TabIndex = 0;
		this.btnClose.Text = "Close";
		this.btnClose.UseVisualStyleBackColor = true;
		this.btnClose.Click += new System.EventHandler(BtnClose_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.FromArgb(80, 80, 80);
		base.ClientSize = new System.Drawing.Size(354, 191);
		base.Controls.Add(this.btnClose);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "ModeDependentSettingsForm";
		this.Text = "ModeDependentSettingsForm (RX1)";
		base.TopMost = true;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(ModeDependentSettingsForm_FormClosing);
		base.ResumeLayout(false);
	}

	private void BtnClose_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void ModeDependentSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		Hide();
		e.Cancel = true;
		Common.SaveForm(this, "BandButtonsPopup");
	}
}
