using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class frmBandwidth : Form
{
	private IContainer components;

	private ucBandwidthView ucBandwidthView;

	private RadioButtonTS radKB;

	private RadioButtonTS radMbit;

	private Timer timerReadBandwidth;

	private CheckBoxTS chkOnTop;

	public frmBandwidth()
	{
		InitializeComponent();
		timerReadBandwidth.Interval = 500;
		timerReadBandwidth.Enabled = false;
		ucBandwidthView.SmoothingFactor = 0.7;
		ucBandwidthView.EnableSmoothing = true;
	}

	private void timerReadBandwidth_Tick(object sender, EventArgs e)
	{
		double outboundBps = NetworkIO.GetOutboundBps();
		double inboundBps = NetworkIO.GetInboundBps();
		ucBandwidthView.PushSample(inboundBps, outboundBps);
	}

	public void RecoverShow()
	{
		Common.RestoreForm(this, base.Name, restore_size: true);
		timerReadBandwidth.Enabled = true;
		Show();
	}

	private void frmBandwidth_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason == CloseReason.UserClosing)
		{
			timerReadBandwidth.Enabled = false;
			Hide();
			ucBandwidthView.Reset();
			e.Cancel = true;
		}
		Common.SaveForm(this, base.Name);
	}

	private void radUnits_CheckedChanged(object sender, EventArgs e)
	{
		ucBandwidthView.DisplayUnits = ((!radKB.Checked) ? ucBandwidthView.BandwidthUnits.Mbitps : ucBandwidthView.BandwidthUnits.KBps);
	}

	private void chkOnTop_CheckedChanged(object sender, EventArgs e)
	{
		base.TopMost = chkOnTop.Checked;
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
		this.timerReadBandwidth = new System.Windows.Forms.Timer(this.components);
		this.radMbit = new System.Windows.Forms.RadioButtonTS();
		this.radKB = new System.Windows.Forms.RadioButtonTS();
		this.ucBandwidthView = new Thetis.ucBandwidthView();
		this.chkOnTop = new System.Windows.Forms.CheckBoxTS();
		base.SuspendLayout();
		this.timerReadBandwidth.Interval = 500;
		this.timerReadBandwidth.Tick += new System.EventHandler(timerReadBandwidth_Tick);
		this.radMbit.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.radMbit.AutoSize = true;
		this.radMbit.Checked = true;
		this.radMbit.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.radMbit.Image = null;
		this.radMbit.Location = new System.Drawing.Point(63, 139);
		this.radMbit.Name = "radMbit";
		this.radMbit.Size = new System.Drawing.Size(55, 17);
		this.radMbit.TabIndex = 2;
		this.radMbit.TabStop = true;
		this.radMbit.Text = "Mbit/s";
		this.radMbit.UseVisualStyleBackColor = true;
		this.radMbit.CheckedChanged += new System.EventHandler(radUnits_CheckedChanged);
		this.radKB.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.radKB.AutoSize = true;
		this.radKB.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.radKB.Image = null;
		this.radKB.Location = new System.Drawing.Point(9, 139);
		this.radKB.Name = "radKB";
		this.radKB.Size = new System.Drawing.Size(48, 17);
		this.radKB.TabIndex = 1;
		this.radKB.Text = "kB/s";
		this.radKB.UseVisualStyleBackColor = true;
		this.radKB.CheckedChanged += new System.EventHandler(radUnits_CheckedChanged);
		this.ucBandwidthView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.ucBandwidthView.BackColor = System.Drawing.Color.Black;
		this.ucBandwidthView.DisplayUnits = Thetis.ucBandwidthView.BandwidthUnits.Mbitps;
		this.ucBandwidthView.EnableSmoothing = false;
		this.ucBandwidthView.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
		this.ucBandwidthView.ForeColor = System.Drawing.Color.White;
		this.ucBandwidthView.HistorySeconds = 60;
		this.ucBandwidthView.Location = new System.Drawing.Point(0, 6);
		this.ucBandwidthView.Name = "ucBandwidthView";
		this.ucBandwidthView.ShowGrid = true;
		this.ucBandwidthView.Size = new System.Drawing.Size(304, 127);
		this.ucBandwidthView.SmoothingFactor = 0.2;
		this.ucBandwidthView.TabIndex = 0;
		this.chkOnTop.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.chkOnTop.AutoSize = true;
		this.chkOnTop.BackColor = System.Drawing.Color.Transparent;
		this.chkOnTop.Checked = true;
		this.chkOnTop.CheckState = System.Windows.Forms.CheckState.Checked;
		this.chkOnTop.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.chkOnTop.Image = null;
		this.chkOnTop.Location = new System.Drawing.Point(230, 139);
		this.chkOnTop.Name = "chkOnTop";
		this.chkOnTop.Size = new System.Drawing.Size(62, 17);
		this.chkOnTop.TabIndex = 3;
		this.chkOnTop.Text = "On Top";
		this.chkOnTop.UseVisualStyleBackColor = false;
		this.chkOnTop.CheckedChanged += new System.EventHandler(chkOnTop_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(304, 161);
		base.Controls.Add(this.chkOnTop);
		base.Controls.Add(this.radMbit);
		base.Controls.Add(this.radKB);
		base.Controls.Add(this.ucBandwidthView);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		this.MaximumSize = new System.Drawing.Size(640, 480);
		this.MinimumSize = new System.Drawing.Size(320, 200);
		base.Name = "frmBandwidth";
		this.Text = "Bandwidth";
		base.TopMost = true;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmBandwidth_FormClosing);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
