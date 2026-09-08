using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class VFOSettingsPopup : Form
{
	private Console console;

	private TextBoxTS txtBoxTuneStep;

	private ButtonTS buttonMinus;

	private ButtonTS buttonPlus;

	private LabelTS labelTS1;

	private ButtonTS buttonClose;

	private IContainer components;

	public VFOSettingsPopup(Console c)
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.VFOSettingsPopup));
		this.buttonClose = new System.Windows.Forms.ButtonTS();
		this.labelTS1 = new System.Windows.Forms.LabelTS();
		this.buttonPlus = new System.Windows.Forms.ButtonTS();
		this.buttonMinus = new System.Windows.Forms.ButtonTS();
		this.txtBoxTuneStep = new System.Windows.Forms.TextBoxTS();
		base.SuspendLayout();
		this.buttonClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.buttonClose.Image = null;
		this.buttonClose.Location = new System.Drawing.Point(238, 74);
		this.buttonClose.Name = "buttonClose";
		this.buttonClose.Selectable = true;
		this.buttonClose.Size = new System.Drawing.Size(75, 32);
		this.buttonClose.TabIndex = 4;
		this.buttonClose.Text = "Close";
		this.buttonClose.UseVisualStyleBackColor = true;
		this.buttonClose.Click += new System.EventHandler(ButtonClose_Click);
		this.labelTS1.AutoSize = true;
		this.labelTS1.ForeColor = System.Drawing.SystemColors.ControlLight;
		this.labelTS1.Image = null;
		this.labelTS1.Location = new System.Drawing.Point(12, 30);
		this.labelTS1.Name = "labelTS1";
		this.labelTS1.Size = new System.Drawing.Size(57, 13);
		this.labelTS1.TabIndex = 3;
		this.labelTS1.Text = "Tune Step";
		this.buttonPlus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.buttonPlus.Image = null;
		this.buttonPlus.Location = new System.Drawing.Point(261, 24);
		this.buttonPlus.Name = "buttonPlus";
		this.buttonPlus.Selectable = true;
		this.buttonPlus.Size = new System.Drawing.Size(52, 32);
		this.buttonPlus.TabIndex = 2;
		this.buttonPlus.Text = "+";
		this.buttonPlus.UseVisualStyleBackColor = true;
		this.buttonPlus.Click += new System.EventHandler(ButtonPlus_Click);
		this.buttonMinus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.buttonMinus.Image = null;
		this.buttonMinus.Location = new System.Drawing.Point(75, 24);
		this.buttonMinus.Name = "buttonMinus";
		this.buttonMinus.Selectable = true;
		this.buttonMinus.Size = new System.Drawing.Size(52, 32);
		this.buttonMinus.TabIndex = 1;
		this.buttonMinus.Text = "-";
		this.buttonMinus.UseVisualStyleBackColor = true;
		this.buttonMinus.Click += new System.EventHandler(ButtonMinus_Click);
		this.txtBoxTuneStep.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txtBoxTuneStep.Location = new System.Drawing.Point(133, 27);
		this.txtBoxTuneStep.Name = "txtBoxTuneStep";
		this.txtBoxTuneStep.Size = new System.Drawing.Size(122, 26);
		this.txtBoxTuneStep.TabIndex = 0;
		this.txtBoxTuneStep.MouseDown += new System.Windows.Forms.MouseEventHandler(TextBoxTuneStep_MouseDown);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.FromArgb(80, 80, 80);
		base.ClientSize = new System.Drawing.Size(334, 127);
		base.Controls.Add(this.buttonClose);
		base.Controls.Add(this.labelTS1);
		base.Controls.Add(this.buttonPlus);
		base.Controls.Add(this.buttonMinus);
		base.Controls.Add(this.txtBoxTuneStep);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "VFOSettingsPopup";
		this.Text = "VFO Settings";
		base.TopMost = true;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(VFOSettingsPopup_FormClosing);
		base.Load += new System.EventHandler(VFOSettingsPopup_Load);
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	private void VFOSettingsPopup_FormClosing(object sender, FormClosingEventArgs e)
	{
		Hide();
		e.Cancel = true;
		Common.SaveForm(this, "VFOSettingsPopup");
	}

	private void TextBoxTuneStep_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			ButtonPlus_Click(null, null);
		}
	}

	private void ButtonMinus_Click(object sender, EventArgs e)
	{
		List<TuneStep> tuneStepList = console.TuneStepList;
		int tuneStepIndex = console.TuneStepIndex;
		tuneStepIndex = (tuneStepIndex - 1 + tuneStepList.Count) % tuneStepList.Count;
		txtBoxTuneStep.Text = tuneStepList[tuneStepIndex].Name;
		console.TuneStepIndex = tuneStepIndex;
	}

	private void ButtonPlus_Click(object sender, EventArgs e)
	{
		List<TuneStep> tuneStepList = console.TuneStepList;
		int tuneStepIndex = console.TuneStepIndex;
		tuneStepIndex = (tuneStepIndex + 1) % tuneStepList.Count;
		txtBoxTuneStep.Text = tuneStepList[tuneStepIndex].Name;
		console.TuneStepIndex = tuneStepIndex;
	}

	private void VFOSettingsPopup_Load(object sender, EventArgs e)
	{
		List<TuneStep> tuneStepList = console.TuneStepList;
		int tuneStepIndex = console.TuneStepIndex;
		txtBoxTuneStep.Text = tuneStepList[tuneStepIndex].Name;
	}

	private void ButtonClose_Click(object sender, EventArgs e)
	{
		Close();
	}
}
