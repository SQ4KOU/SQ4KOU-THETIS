using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class frmLog : Form
{
	private const int MAX_ENTRIES = 500;

	private bool _log;

	private object _lock = new object();

	private string[] _logLines;

	private IContainer components;

	private TextBoxTS txtLog;

	private CheckBoxTS chkLog;

	private ButtonTS btnClear;

	private LabelTS labelTS1;

	public frmLog()
	{
		InitializeComponent();
	}

	private void btnClear_Click(object sender, EventArgs e)
	{
		lock (_lock)
		{
			_logLines = null;
		}
		txtLog.Lines = _logLines;
	}

	private void chkLog_CheckedChanged(object sender, EventArgs e)
	{
		_log = chkLog.Checked;
		if (_log)
		{
			lock (_lock)
			{
				_logLines = null;
			}
			txtLog.Lines = _logLines;
		}
	}

	public void Log(bool bIn, string sMessage)
	{
		if (!_log)
		{
			return;
		}
		lock (_lock)
		{
			if (_logLines == null)
			{
				_logLines = new string[0];
			}
			string[] array = ((_logLines.Length + 1 <= 500) ? new string[_logLines.Length + 1] : new string[500]);
			for (int i = 1; i < array.Length; i++)
			{
				array[i] = _logLines[i - 1];
			}
			array[0] = (bIn ? "< " : "> ") + sMessage;
			_logLines = array;
		}
		if (_log)
		{
			txtLog.Lines = _logLines;
		}
	}

	public void ShowWithTitle(string title)
	{
		Text = "Log: " + title;
		Show();
	}

	private void frmLog_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason == CloseReason.UserClosing)
		{
			Hide();
			e.Cancel = true;
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
		this.txtLog = new System.Windows.Forms.TextBoxTS();
		this.chkLog = new System.Windows.Forms.CheckBoxTS();
		this.btnClear = new System.Windows.Forms.ButtonTS();
		this.labelTS1 = new System.Windows.Forms.LabelTS();
		base.SuspendLayout();
		this.txtLog.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.txtLog.BackColor = System.Drawing.Color.White;
		this.txtLog.Location = new System.Drawing.Point(12, 40);
		this.txtLog.Multiline = true;
		this.txtLog.Name = "txtLog";
		this.txtLog.ReadOnly = true;
		this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.txtLog.Size = new System.Drawing.Size(280, 200);
		this.txtLog.TabIndex = 0;
		this.chkLog.AutoSize = true;
		this.chkLog.Image = null;
		this.chkLog.Location = new System.Drawing.Point(12, 12);
		this.chkLog.Name = "chkLog";
		this.chkLog.Size = new System.Drawing.Size(44, 17);
		this.chkLog.TabIndex = 1;
		this.chkLog.Text = "Log";
		this.chkLog.UseVisualStyleBackColor = true;
		this.chkLog.CheckedChanged += new System.EventHandler(chkLog_CheckedChanged);
		this.btnClear.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnClear.Image = null;
		this.btnClear.Location = new System.Drawing.Point(217, 246);
		this.btnClear.Name = "btnClear";
		this.btnClear.Size = new System.Drawing.Size(75, 23);
		this.btnClear.TabIndex = 2;
		this.btnClear.Text = "Clear";
		this.btnClear.UseVisualStyleBackColor = true;
		this.btnClear.Click += new System.EventHandler(btnClear_Click);
		this.labelTS1.AutoSize = true;
		this.labelTS1.Image = null;
		this.labelTS1.Location = new System.Drawing.Point(100, 13);
		this.labelTS1.Name = "labelTS1";
		this.labelTS1.Size = new System.Drawing.Size(128, 13);
		this.labelTS1.TabIndex = 3;
		this.labelTS1.Text = "< : received          > : sent";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(304, 281);
		base.Controls.Add(this.labelTS1);
		base.Controls.Add(this.btnClear);
		base.Controls.Add(this.chkLog);
		base.Controls.Add(this.txtLog);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(320, 320);
		base.Name = "frmLog";
		this.Text = "Log";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmLog_FormClosing);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
