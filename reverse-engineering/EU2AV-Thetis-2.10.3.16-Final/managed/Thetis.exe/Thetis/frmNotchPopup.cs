using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class frmNotchPopup : Form
{
	public delegate void NotchDeleteHandler(int notch_index);

	public delegate void NotchBWChangeHandler(int notch_index, double width);

	public delegate void NotchActiveChangedHandler(int notch_index, bool active);

	private int _notch_index;

	private DateTime _deactivate_time;

	private IContainer components;

	private ButtonTS btnDelete;

	private CheckBoxTS chkActive;

	private ButtonTS btn25;

	private ButtonTS btn50;

	private ButtonTS btn200;

	private ButtonTS btn100;

	private LabelTS lblWidth;

	private TrackBarTS trkWidth;

	public DateTime DeactivateTime => _deactivate_time;

	private event NotchDeleteHandler deleteEvents;

	private event NotchBWChangeHandler bwChangeEvents;

	private event NotchActiveChangedHandler activeEvents;

	public event NotchDeleteHandler NotchDeleteEvent
	{
		add
		{
			deleteEvents += value;
		}
		remove
		{
			deleteEvents -= value;
		}
	}

	public event NotchBWChangeHandler NotchBWChangedEvent
	{
		add
		{
			bwChangeEvents += value;
		}
		remove
		{
			bwChangeEvents -= value;
		}
	}

	public event NotchActiveChangedHandler NotchActiveChangedEvent
	{
		add
		{
			activeEvents += value;
		}
		remove
		{
			activeEvents -= value;
		}
	}

	public frmNotchPopup()
	{
		_notch_index = -1;
		InitializeComponent();
		_deactivate_time = DateTime.UtcNow;
	}

	public void Show(MNotch notch, int minWidth, int maxWidth, bool top, int notch_index = -1)
	{
		_deactivate_time = DateTime.UtcNow;
		if (notch != null)
		{
			_notch_index = notch_index;
			if (top)
			{
				Win32.SetWindowPos(base.Handle.ToInt32(), -1, base.Left, base.Top, base.Width, base.Height, 0);
			}
			else
			{
				Win32.SetWindowPos(base.Handle.ToInt32(), -2, base.Left, base.Top, base.Width, base.Height, 0);
			}
			trkWidth.Minimum = minWidth;
			if ((int)notch.FWidth > maxWidth)
			{
				trkWidth.Maximum = (int)notch.FWidth;
			}
			else
			{
				trkWidth.Maximum = maxWidth;
			}
			trkWidth.TickFrequency = 10;
			trkWidth.TickStyle = TickStyle.None;
			trkWidth.Value = (int)notch.FWidth;
			chkActive.Checked = notch.Active;
			setText(trkWidth.Value);
			Show();
		}
	}

	private void FrmNotchPopup_Deactivate(object sender, EventArgs e)
	{
		_notch_index = -1;
		_deactivate_time = DateTime.UtcNow;
		Hide();
	}

	private void BtnDelete_Click(object sender, EventArgs e)
	{
		deleteEvents(_notch_index);
		_notch_index = -1;
		_deactivate_time = DateTime.UtcNow;
		Hide();
	}

	private void setBW(int width)
	{
		trkWidth.Value = width;
		setText(width);
		bwChangeEvents(_notch_index, width);
	}

	private void Btn25_Click(object sender, EventArgs e)
	{
		setBW(25);
	}

	private void Btn50_Click(object sender, EventArgs e)
	{
		setBW(50);
	}

	private void Btn100_Click(object sender, EventArgs e)
	{
		setBW(100);
	}

	private void Btn200_Click(object sender, EventArgs e)
	{
		setBW(200);
	}

	private void TrkWidth_Scroll(object sender, EventArgs e)
	{
		setText(trkWidth.Value);
		bwChangeEvents(_notch_index, trkWidth.Value);
	}

	private void setText(int v)
	{
		lblWidth.Text = v + " Hz";
	}

	private void ChkActive_CheckedChanged(object sender, EventArgs e)
	{
		activeEvents(_notch_index, chkActive.Checked);
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
		this.trkWidth = new System.Windows.Forms.TrackBarTS();
		this.lblWidth = new System.Windows.Forms.LabelTS();
		this.btn200 = new System.Windows.Forms.ButtonTS();
		this.btn100 = new System.Windows.Forms.ButtonTS();
		this.btn50 = new System.Windows.Forms.ButtonTS();
		this.btn25 = new System.Windows.Forms.ButtonTS();
		this.chkActive = new System.Windows.Forms.CheckBoxTS();
		this.btnDelete = new System.Windows.Forms.ButtonTS();
		((System.ComponentModel.ISupportInitialize)this.trkWidth).BeginInit();
		base.SuspendLayout();
		this.trkWidth.AutoSize = false;
		this.trkWidth.Location = new System.Drawing.Point(5, 86);
		this.trkWidth.Name = "trkWidth";
		this.trkWidth.Size = new System.Drawing.Size(126, 20);
		this.trkWidth.TabIndex = 8;
		this.trkWidth.Scroll += new System.EventHandler(TrkWidth_Scroll);
		this.lblWidth.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.lblWidth.Image = null;
		this.lblWidth.Location = new System.Drawing.Point(5, 62);
		this.lblWidth.Name = "lblWidth";
		this.lblWidth.Size = new System.Drawing.Size(127, 21);
		this.lblWidth.TabIndex = 7;
		this.lblWidth.Text = "10000 Hz";
		this.lblWidth.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
		this.btn200.Image = null;
		this.btn200.Location = new System.Drawing.Point(71, 141);
		this.btn200.Name = "btn200";
		this.btn200.Size = new System.Drawing.Size(60, 23);
		this.btn200.TabIndex = 5;
		this.btn200.Text = "200Hz";
		this.btn200.UseVisualStyleBackColor = true;
		this.btn200.Click += new System.EventHandler(Btn200_Click);
		this.btn100.Image = null;
		this.btn100.Location = new System.Drawing.Point(5, 141);
		this.btn100.Name = "btn100";
		this.btn100.Size = new System.Drawing.Size(60, 23);
		this.btn100.TabIndex = 4;
		this.btn100.Text = "100Hz";
		this.btn100.UseVisualStyleBackColor = true;
		this.btn100.Click += new System.EventHandler(Btn100_Click);
		this.btn50.Image = null;
		this.btn50.Location = new System.Drawing.Point(71, 112);
		this.btn50.Name = "btn50";
		this.btn50.Size = new System.Drawing.Size(60, 23);
		this.btn50.TabIndex = 3;
		this.btn50.Text = "50Hz";
		this.btn50.UseVisualStyleBackColor = true;
		this.btn50.Click += new System.EventHandler(Btn50_Click);
		this.btn25.Image = null;
		this.btn25.Location = new System.Drawing.Point(5, 112);
		this.btn25.Name = "btn25";
		this.btn25.Size = new System.Drawing.Size(60, 23);
		this.btn25.TabIndex = 2;
		this.btn25.Text = "25Hz";
		this.btn25.UseVisualStyleBackColor = true;
		this.btn25.Click += new System.EventHandler(Btn25_Click);
		this.chkActive.AutoSize = true;
		this.chkActive.Image = null;
		this.chkActive.Location = new System.Drawing.Point(40, 42);
		this.chkActive.Name = "chkActive";
		this.chkActive.Size = new System.Drawing.Size(56, 17);
		this.chkActive.TabIndex = 1;
		this.chkActive.Text = "Active";
		this.chkActive.UseVisualStyleBackColor = true;
		this.chkActive.CheckedChanged += new System.EventHandler(ChkActive_CheckedChanged);
		this.btnDelete.BackColor = System.Drawing.Color.FromArgb(192, 0, 0);
		this.btnDelete.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
		this.btnDelete.Image = null;
		this.btnDelete.Location = new System.Drawing.Point(3, 4);
		this.btnDelete.Name = "btnDelete";
		this.btnDelete.Size = new System.Drawing.Size(129, 31);
		this.btnDelete.TabIndex = 0;
		this.btnDelete.Text = "Delete";
		this.btnDelete.UseVisualStyleBackColor = false;
		this.btnDelete.Click += new System.EventHandler(BtnDelete_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(136, 169);
		base.ControlBox = false;
		base.Controls.Add(this.trkWidth);
		base.Controls.Add(this.lblWidth);
		base.Controls.Add(this.btn200);
		base.Controls.Add(this.btn100);
		base.Controls.Add(this.btn50);
		base.Controls.Add(this.btn25);
		base.Controls.Add(this.chkActive);
		base.Controls.Add(this.btnDelete);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "frmNotchPopup";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		this.Text = "NotchPopup";
		base.Deactivate += new System.EventHandler(FrmNotchPopup_Deactivate);
		((System.ComponentModel.ISupportInitialize)this.trkWidth).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
