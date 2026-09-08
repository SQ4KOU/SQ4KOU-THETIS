using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class ucSignalSelect : UserControl
{
	public class SignalTypeChangedEventArgs : EventArgs
	{
		public Reading SignalType { get; }

		public SignalTypeChangedEventArgs(Reading signalType)
		{
			SignalType = signalType;
		}
	}

	private IContainer components;

	private RadioButtonTS radSig;

	private RadioButtonTS radSigAvg;

	private RadioButtonTS radSigMaxBin;

	private ToolTip toolTip1;

	public Reading SignalType
	{
		get
		{
			return getSignalTypeFromSelection();
		}
		set
		{
			switch (value)
			{
			case Reading.AVG_SIGNAL_STRENGTH:
				radSigAvg.Checked = true;
				break;
			case Reading.SIGNAL_MAX_BIN:
				radSigMaxBin.Checked = true;
				break;
			default:
				radSig.Checked = true;
				break;
			}
		}
	}

	public event EventHandler<SignalTypeChangedEventArgs> SignalTypeChanged;

	public ucSignalSelect()
	{
		InitializeComponent();
	}

	private void radSig_CheckedChanged(object sender, EventArgs e)
	{
		if (sender is RadioButton { Checked: not false })
		{
			onSignalTypeChanged(getSignalTypeFromSelection());
		}
	}

	private void radSigAvg_CheckedChanged(object sender, EventArgs e)
	{
		if (sender is RadioButton { Checked: not false })
		{
			onSignalTypeChanged(getSignalTypeFromSelection());
		}
	}

	private Reading getSignalTypeFromSelection()
	{
		if (radSigAvg.Checked)
		{
			return Reading.AVG_SIGNAL_STRENGTH;
		}
		if (radSigMaxBin.Checked)
		{
			return Reading.SIGNAL_MAX_BIN;
		}
		return Reading.SIGNAL_STRENGTH;
	}

	private void onSignalTypeChanged(Reading signalType)
	{
		SignalTypeChanged?.Invoke(this, new SignalTypeChangedEventArgs(signalType));
	}

	private void radSigMaxBin_CheckedChanged(object sender, EventArgs e)
	{
		if (sender is RadioButton { Checked: not false })
		{
			onSignalTypeChanged(getSignalTypeFromSelection());
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
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.ucSignalSelect));
		this.radSig = new System.Windows.Forms.RadioButtonTS();
		this.radSigAvg = new System.Windows.Forms.RadioButtonTS();
		this.radSigMaxBin = new System.Windows.Forms.RadioButtonTS();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		base.SuspendLayout();
		this.radSig.AutoSize = true;
		this.radSig.Image = null;
		this.radSig.Location = new System.Drawing.Point(3, 3);
		this.radSig.Name = "radSig";
		this.radSig.Size = new System.Drawing.Size(40, 17);
		this.radSig.TabIndex = 0;
		this.radSig.TabStop = true;
		this.radSig.Text = "Sig";
		this.toolTip1.SetToolTip(this.radSig, "Signal Peak");
		this.radSig.UseVisualStyleBackColor = true;
		this.radSig.CheckedChanged += new System.EventHandler(radSig_CheckedChanged);
		this.radSigAvg.AutoSize = true;
		this.radSigAvg.Image = null;
		this.radSigAvg.Location = new System.Drawing.Point(49, 3);
		this.radSigAvg.Name = "radSigAvg";
		this.radSigAvg.Size = new System.Drawing.Size(44, 17);
		this.radSigAvg.TabIndex = 1;
		this.radSigAvg.TabStop = true;
		this.radSigAvg.Text = "Avg";
		this.toolTip1.SetToolTip(this.radSigAvg, "Signal Average");
		this.radSigAvg.UseVisualStyleBackColor = true;
		this.radSigAvg.CheckedChanged += new System.EventHandler(radSigAvg_CheckedChanged);
		this.radSigMaxBin.AutoSize = true;
		this.radSigMaxBin.Image = null;
		this.radSigMaxBin.Location = new System.Drawing.Point(99, 3);
		this.radSigMaxBin.Name = "radSigMaxBin";
		this.radSigMaxBin.Size = new System.Drawing.Size(63, 17);
		this.radSigMaxBin.TabIndex = 2;
		this.radSigMaxBin.TabStop = true;
		this.radSigMaxBin.Text = "Max Bin";
		this.toolTip1.SetToolTip(this.radSigMaxBin, resources.GetString("radSigMaxBin.ToolTip"));
		this.radSigMaxBin.UseVisualStyleBackColor = true;
		this.radSigMaxBin.CheckedChanged += new System.EventHandler(radSigMaxBin_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.radSigMaxBin);
		base.Controls.Add(this.radSigAvg);
		base.Controls.Add(this.radSig);
		base.Name = "ucSignalSelect";
		base.Size = new System.Drawing.Size(162, 24);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
