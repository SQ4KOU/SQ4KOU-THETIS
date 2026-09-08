using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class ShutdownForm : Form
{
	private IContainer components;

	private LabelTS labelTS1;

	public ShutdownForm()
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
		this.labelTS1 = new System.Windows.Forms.LabelTS();
		base.SuspendLayout();
		this.labelTS1.AutoSize = true;
		this.labelTS1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelTS1.Image = null;
		this.labelTS1.Location = new System.Drawing.Point(49, 33);
		this.labelTS1.Name = "labelTS1";
		this.labelTS1.Size = new System.Drawing.Size(344, 24);
		this.labelTS1.TabIndex = 0;
		this.labelTS1.Text = "Please wait... Thetis closing down...";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.Window;
		base.ClientSize = new System.Drawing.Size(439, 88);
		base.ControlBox = false;
		base.Controls.Add(this.labelTS1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "ShutdownForm";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		this.Text = "Closing";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
