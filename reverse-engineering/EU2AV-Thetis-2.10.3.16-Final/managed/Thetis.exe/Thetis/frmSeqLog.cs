using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Thetis.Properties;

namespace Thetis;

public class frmSeqLog : Form
{
	public delegate void ClearButtonHandler();

	private int m_nTabMainTop;

	private StringBuilder log = new StringBuilder();

	private IContainer components;

	private ButtonTS btnClear;

	private LabelTS labelTS1;

	private LabelTS labelTS2;

	private LabelTS labelTS3;

	private LabelTS labelTS4;

	private LabelTS labelTS5;

	private ButtonTS btnCopyToClipboard;

	private ButtonTS btnCopyImageToClipboard;

	private PanelTS panelTS1;

	private PanelTS panelTS2;

	private TabControl tabMain;

	private TabPage tabPage1;

	private TextBoxTS txtLog;

	private TabPage tabPage2;

	private GroupBoxTS groupDumpCap;

	private CheckBoxTS chkKillOnNegativeOnly;

	private LabelTS labelTS8;

	private NumericUpDownTS udInterface;

	private CheckBoxTS chkDumpCapEnabled;

	private LabelTS labelTS7;

	private ButtonTS btnSetWireSharkFolder;

	private TextBoxTS txtWireSharkFolder;

	private CheckBoxTS chkClearRingBufferFolderOnRestart;

	private PanelTS panelTS3;

	private ButtonTS btnShowDumpCapFolder;

	private LabelTS labelTS9;

	private CheckBoxTS chkStatusBarWarningNegativeOnly;

	public bool StatusBarWarningOnNegativeOnly
	{
		get
		{
			return chkStatusBarWarningNegativeOnly.Checked;
		}
		set
		{
			chkStatusBarWarningNegativeOnly.Checked = value;
		}
	}

	private event ClearButtonHandler clearButtonEvents;

	public event ClearButtonHandler ClearButtonEvent
	{
		add
		{
			clearButtonEvents += value;
		}
		remove
		{
			clearButtonEvents -= value;
		}
	}

	public frmSeqLog()
	{
		InitializeComponent();
		m_nTabMainTop = tabMain.Top;
		btnClear.Text = "Close";
		btnCopyImageToClipboard.Enabled = false;
		btnCopyToClipboard.Enabled = false;
		tabMain.SelectedIndex = 0;
	}

	public void InitAndShow()
	{
		tabMain.SelectedIndex = 0;
		Show();
	}

	public void SetWireSharkPath(string sPath)
	{
		setupControlsDumpCap(sPath);
	}

	private void btnClear_Click(object sender, EventArgs e)
	{
		btnClear.Text = "Close";
		btnCopyImageToClipboard.Enabled = false;
		btnCopyToClipboard.Enabled = false;
		txtLog.Text = "";
		log.Clear();
		clearButtonEvents();
		Close();
	}

	public void LogString(string s)
	{
		log.Insert(0, s + Environment.NewLine);
		if (log.Length > 16000)
		{
			log.Remove(16000, log.Length - 16000);
		}
		txtLog.Text = log.ToString();
		btnClear.Text = "Clear Log + Close";
		btnCopyImageToClipboard.Enabled = true;
		btnCopyToClipboard.Enabled = true;
	}

	private void frmSeqLog_FormClosing(object sender, FormClosingEventArgs e)
	{
		Hide();
		e.Cancel = true;
	}

	private void btnCopyToClipboard_Click(object sender, EventArgs e)
	{
		Clipboard.SetText(txtLog.Text);
	}

	private void btnCopyImageToClipboard_Click(object sender, EventArgs e)
	{
		using Bitmap bitmap = new Bitmap(txtLog.Width, txtLog.Height);
		txtLog.DrawToBitmap(bitmap, new Rectangle(new Point(0, 0), txtLog.Size));
		Clipboard.SetImage(bitmap);
	}

	private void btnSetWireSharkFolder_Click(object sender, EventArgs e)
	{
		using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		if (folderBrowserDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
		{
			setupControlsDumpCap(folderBrowserDialog.SelectedPath);
		}
	}

	private void setupControlsDumpCap(string sPath)
	{
		txtWireSharkFolder.Text = sPath;
		DumpCap.WireSharkPath = sPath;
		bool flag = DumpCap.DumpCapExists();
		groupDumpCap.Enabled = flag;
		if (flag)
		{
			groupDumpCap.Text = "DumpCap [FOUND]";
		}
		else
		{
			groupDumpCap.Text = "DumpCap [NOT FOUND]";
		}
		chkDumpCapEnabled.Checked = false;
		DumpCap.Enabled = false;
		udInterface.Value = DumpCap.Interface;
		chkKillOnNegativeOnly.Checked = DumpCap.KillOnNegativeSeqOnly;
		chkClearRingBufferFolderOnRestart.Checked = DumpCap.ClearFolderOnRestart;
	}

	private void udInterface_ValueChanged(object sender, EventArgs e)
	{
		DumpCap.Interface = (int)udInterface.Value;
	}

	private void chkKillOnNegativeOnly_CheckedChanged(object sender, EventArgs e)
	{
		DumpCap.KillOnNegativeSeqOnly = chkKillOnNegativeOnly.Checked;
	}

	private void chkDumpCapEnabled_CheckedChanged(object sender, EventArgs e)
	{
		DumpCap.Enabled = chkDumpCapEnabled.Checked;
	}

	private void chkClearRingBufferFolderOnRestart_CheckedChanged(object sender, EventArgs e)
	{
		DumpCap.ClearFolderOnRestart = chkClearRingBufferFolderOnRestart.Checked;
	}

	private void btnShowDumpCapFolder_Click(object sender, EventArgs e)
	{
		DumpCap.ShowAppPathFolder();
	}

	private void chkStatusBarWarningNegativeOnly_CheckedChanged(object sender, EventArgs e)
	{
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
		this.tabMain = new System.Windows.Forms.TabControl();
		this.tabPage1 = new System.Windows.Forms.TabPage();
		this.txtLog = new System.Windows.Forms.TextBoxTS();
		this.tabPage2 = new System.Windows.Forms.TabPage();
		this.groupDumpCap = new System.Windows.Forms.GroupBoxTS();
		this.labelTS9 = new System.Windows.Forms.LabelTS();
		this.btnShowDumpCapFolder = new System.Windows.Forms.ButtonTS();
		this.chkClearRingBufferFolderOnRestart = new System.Windows.Forms.CheckBoxTS();
		this.chkKillOnNegativeOnly = new System.Windows.Forms.CheckBoxTS();
		this.labelTS8 = new System.Windows.Forms.LabelTS();
		this.udInterface = new System.Windows.Forms.NumericUpDownTS();
		this.chkDumpCapEnabled = new System.Windows.Forms.CheckBoxTS();
		this.labelTS7 = new System.Windows.Forms.LabelTS();
		this.btnSetWireSharkFolder = new System.Windows.Forms.ButtonTS();
		this.txtWireSharkFolder = new System.Windows.Forms.TextBoxTS();
		this.panelTS3 = new System.Windows.Forms.PanelTS();
		this.chkStatusBarWarningNegativeOnly = new System.Windows.Forms.CheckBoxTS();
		this.labelTS1 = new System.Windows.Forms.LabelTS();
		this.labelTS2 = new System.Windows.Forms.LabelTS();
		this.panelTS2 = new System.Windows.Forms.PanelTS();
		this.labelTS3 = new System.Windows.Forms.LabelTS();
		this.panelTS1 = new System.Windows.Forms.PanelTS();
		this.labelTS4 = new System.Windows.Forms.LabelTS();
		this.btnCopyImageToClipboard = new System.Windows.Forms.ButtonTS();
		this.labelTS5 = new System.Windows.Forms.LabelTS();
		this.btnCopyToClipboard = new System.Windows.Forms.ButtonTS();
		this.btnClear = new System.Windows.Forms.ButtonTS();
		this.tabMain.SuspendLayout();
		this.tabPage1.SuspendLayout();
		this.tabPage2.SuspendLayout();
		this.groupDumpCap.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.udInterface).BeginInit();
		this.panelTS3.SuspendLayout();
		base.SuspendLayout();
		this.tabMain.Controls.Add(this.tabPage1);
		this.tabMain.Controls.Add(this.tabPage2);
		this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tabMain.Location = new System.Drawing.Point(0, 168);
		this.tabMain.Name = "tabMain";
		this.tabMain.SelectedIndex = 0;
		this.tabMain.Size = new System.Drawing.Size(852, 546);
		this.tabMain.TabIndex = 12;
		this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
		this.tabPage1.Controls.Add(this.txtLog);
		this.tabPage1.Location = new System.Drawing.Point(4, 22);
		this.tabPage1.Name = "tabPage1";
		this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
		this.tabPage1.Size = new System.Drawing.Size(844, 520);
		this.tabPage1.TabIndex = 0;
		this.tabPage1.Text = "SEQ Errors";
		this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
		this.txtLog.Font = new System.Drawing.Font("Courier New", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txtLog.Location = new System.Drawing.Point(3, 3);
		this.txtLog.Multiline = true;
		this.txtLog.Name = "txtLog";
		this.txtLog.ReadOnly = true;
		this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
		this.txtLog.Size = new System.Drawing.Size(838, 514);
		this.txtLog.TabIndex = 1;
		this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
		this.tabPage2.Controls.Add(this.groupDumpCap);
		this.tabPage2.Controls.Add(this.labelTS7);
		this.tabPage2.Controls.Add(this.btnSetWireSharkFolder);
		this.tabPage2.Controls.Add(this.txtWireSharkFolder);
		this.tabPage2.Location = new System.Drawing.Point(4, 22);
		this.tabPage2.Name = "tabPage2";
		this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
		this.tabPage2.Size = new System.Drawing.Size(844, 520);
		this.tabPage2.TabIndex = 1;
		this.tabPage2.Text = "WireShark / DumpCap";
		this.groupDumpCap.Controls.Add(this.labelTS9);
		this.groupDumpCap.Controls.Add(this.btnShowDumpCapFolder);
		this.groupDumpCap.Controls.Add(this.chkClearRingBufferFolderOnRestart);
		this.groupDumpCap.Controls.Add(this.chkKillOnNegativeOnly);
		this.groupDumpCap.Controls.Add(this.labelTS8);
		this.groupDumpCap.Controls.Add(this.udInterface);
		this.groupDumpCap.Controls.Add(this.chkDumpCapEnabled);
		this.groupDumpCap.Location = new System.Drawing.Point(30, 85);
		this.groupDumpCap.Name = "groupDumpCap";
		this.groupDumpCap.Size = new System.Drawing.Size(539, 218);
		this.groupDumpCap.TabIndex = 3;
		this.groupDumpCap.TabStop = false;
		this.groupDumpCap.Text = "DumpCap [xxxxxxxxx]";
		this.labelTS9.Image = null;
		this.labelTS9.Location = new System.Drawing.Point(295, 29);
		this.labelTS9.Name = "labelTS9";
		this.labelTS9.Size = new System.Drawing.Size(239, 38);
		this.labelTS9.TabIndex = 6;
		this.labelTS9.Text = "obtained by running DumpCap.exe -D on command line and using number on left of list";
		this.btnShowDumpCapFolder.Image = null;
		this.btnShowDumpCapFolder.Location = new System.Drawing.Point(22, 144);
		this.btnShowDumpCapFolder.Name = "btnShowDumpCapFolder";
		this.btnShowDumpCapFolder.Size = new System.Drawing.Size(147, 50);
		this.btnShowDumpCapFolder.TabIndex = 5;
		this.btnShowDumpCapFolder.Text = "Open DumpCap Folder";
		this.btnShowDumpCapFolder.UseVisualStyleBackColor = true;
		this.btnShowDumpCapFolder.Click += new System.EventHandler(btnShowDumpCapFolder_Click);
		this.chkClearRingBufferFolderOnRestart.AutoSize = true;
		this.chkClearRingBufferFolderOnRestart.Image = null;
		this.chkClearRingBufferFolderOnRestart.Location = new System.Drawing.Point(21, 108);
		this.chkClearRingBufferFolderOnRestart.Name = "chkClearRingBufferFolderOnRestart";
		this.chkClearRingBufferFolderOnRestart.Size = new System.Drawing.Size(211, 17);
		this.chkClearRingBufferFolderOnRestart.TabIndex = 4;
		this.chkClearRingBufferFolderOnRestart.Text = "Clear DumpCap folder on Thetis startup";
		this.chkClearRingBufferFolderOnRestart.UseVisualStyleBackColor = true;
		this.chkClearRingBufferFolderOnRestart.CheckedChanged += new System.EventHandler(chkClearRingBufferFolderOnRestart_CheckedChanged);
		this.chkKillOnNegativeOnly.AutoSize = true;
		this.chkKillOnNegativeOnly.Image = null;
		this.chkKillOnNegativeOnly.Location = new System.Drawing.Point(21, 72);
		this.chkKillOnNegativeOnly.Name = "chkKillOnNegativeOnly";
		this.chkKillOnNegativeOnly.Size = new System.Drawing.Size(202, 17);
		this.chkKillOnNegativeOnly.TabIndex = 3;
		this.chkKillOnNegativeOnly.Text = "New ringbuffer files for -ve deltas only";
		this.chkKillOnNegativeOnly.UseVisualStyleBackColor = true;
		this.chkKillOnNegativeOnly.CheckedChanged += new System.EventHandler(chkKillOnNegativeOnly_CheckedChanged);
		this.labelTS8.AutoSize = true;
		this.labelTS8.Image = null;
		this.labelTS8.Location = new System.Drawing.Point(198, 34);
		this.labelTS8.Name = "labelTS8";
		this.labelTS8.Size = new System.Drawing.Size(87, 13);
		this.labelTS8.TabIndex = 2;
		this.labelTS8.Text = "Interface number";
		this.labelTS8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.udInterface.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udInterface.Location = new System.Drawing.Point(154, 31);
		this.udInterface.Maximum = new decimal(new int[4] { 32, 0, 0, 0 });
		this.udInterface.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udInterface.Name = "udInterface";
		this.udInterface.Size = new System.Drawing.Size(38, 20);
		this.udInterface.TabIndex = 1;
		this.udInterface.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udInterface.ValueChanged += new System.EventHandler(udInterface_ValueChanged);
		this.chkDumpCapEnabled.AutoSize = true;
		this.chkDumpCapEnabled.Image = null;
		this.chkDumpCapEnabled.Location = new System.Drawing.Point(22, 34);
		this.chkDumpCapEnabled.Name = "chkDumpCapEnabled";
		this.chkDumpCapEnabled.Size = new System.Drawing.Size(65, 17);
		this.chkDumpCapEnabled.TabIndex = 0;
		this.chkDumpCapEnabled.Text = "Enabled";
		this.chkDumpCapEnabled.UseVisualStyleBackColor = true;
		this.chkDumpCapEnabled.CheckedChanged += new System.EventHandler(chkDumpCapEnabled_CheckedChanged);
		this.labelTS7.AutoSize = true;
		this.labelTS7.Image = null;
		this.labelTS7.Location = new System.Drawing.Point(25, 37);
		this.labelTS7.Name = "labelTS7";
		this.labelTS7.Size = new System.Drawing.Size(92, 13);
		this.labelTS7.TabIndex = 2;
		this.labelTS7.Text = "WireShark folder :";
		this.labelTS7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.btnSetWireSharkFolder.Image = null;
		this.btnSetWireSharkFolder.Location = new System.Drawing.Point(531, 26);
		this.btnSetWireSharkFolder.Name = "btnSetWireSharkFolder";
		this.btnSetWireSharkFolder.Size = new System.Drawing.Size(102, 34);
		this.btnSetWireSharkFolder.TabIndex = 1;
		this.btnSetWireSharkFolder.Text = "Select Folder";
		this.btnSetWireSharkFolder.UseVisualStyleBackColor = true;
		this.btnSetWireSharkFolder.Click += new System.EventHandler(btnSetWireSharkFolder_Click);
		this.txtWireSharkFolder.Location = new System.Drawing.Point(123, 34);
		this.txtWireSharkFolder.Name = "txtWireSharkFolder";
		this.txtWireSharkFolder.ReadOnly = true;
		this.txtWireSharkFolder.Size = new System.Drawing.Size(401, 20);
		this.txtWireSharkFolder.TabIndex = 0;
		this.panelTS3.AutoScrollMargin = new System.Drawing.Size(0, 0);
		this.panelTS3.AutoScrollMinSize = new System.Drawing.Size(0, 0);
		this.panelTS3.Controls.Add(this.chkStatusBarWarningNegativeOnly);
		this.panelTS3.Controls.Add(this.labelTS1);
		this.panelTS3.Controls.Add(this.labelTS2);
		this.panelTS3.Controls.Add(this.panelTS2);
		this.panelTS3.Controls.Add(this.labelTS3);
		this.panelTS3.Controls.Add(this.panelTS1);
		this.panelTS3.Controls.Add(this.labelTS4);
		this.panelTS3.Controls.Add(this.btnCopyImageToClipboard);
		this.panelTS3.Controls.Add(this.labelTS5);
		this.panelTS3.Controls.Add(this.btnCopyToClipboard);
		this.panelTS3.Dock = System.Windows.Forms.DockStyle.Top;
		this.panelTS3.Location = new System.Drawing.Point(0, 0);
		this.panelTS3.Name = "panelTS3";
		this.panelTS3.Size = new System.Drawing.Size(852, 168);
		this.panelTS3.TabIndex = 13;
		this.chkStatusBarWarningNegativeOnly.AutoSize = true;
		this.chkStatusBarWarningNegativeOnly.Checked = true;
		this.chkStatusBarWarningNegativeOnly.CheckState = System.Windows.Forms.CheckState.Checked;
		this.chkStatusBarWarningNegativeOnly.Image = null;
		this.chkStatusBarWarningNegativeOnly.Location = new System.Drawing.Point(587, 140);
		this.chkStatusBarWarningNegativeOnly.Name = "chkStatusBarWarningNegativeOnly";
		this.chkStatusBarWarningNegativeOnly.Size = new System.Drawing.Size(171, 17);
		this.chkStatusBarWarningNegativeOnly.TabIndex = 12;
		this.chkStatusBarWarningNegativeOnly.Text = "Status bar warning on -VE only";
		this.chkStatusBarWarningNegativeOnly.UseVisualStyleBackColor = true;
		this.chkStatusBarWarningNegativeOnly.CheckedChanged += new System.EventHandler(chkStatusBarWarningNegativeOnly_CheckedChanged);
		this.labelTS1.AutoSize = true;
		this.labelTS1.Image = null;
		this.labelTS1.Location = new System.Drawing.Point(16, 20);
		this.labelTS1.Name = "labelTS1";
		this.labelTS1.Size = new System.Drawing.Size(621, 13);
		this.labelTS1.TabIndex = 2;
		this.labelTS1.Text = "This log shows inbound (radio->pc) UDP packet sequence errors. The numbers are a delta from the next expected packet number.";
		this.labelTS2.AutoSize = true;
		this.labelTS2.Image = null;
		this.labelTS2.Location = new System.Drawing.Point(59, 67);
		this.labelTS2.Name = "labelTS2";
		this.labelTS2.Size = new System.Drawing.Size(281, 13);
		this.labelTS2.TabIndex = 3;
		this.labelTS2.Text = "0 = there is no delta so expected packet number is correct";
		this.panelTS2.AutoScrollMargin = new System.Drawing.Size(0, 0);
		this.panelTS2.AutoScrollMinSize = new System.Drawing.Size(0, 0);
		this.panelTS2.BackgroundImage = Thetis.Properties.Resources.warning4;
		this.panelTS2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		this.panelTS2.Location = new System.Drawing.Point(35, 124);
		this.panelTS2.Name = "panelTS2";
		this.panelTS2.Size = new System.Drawing.Size(18, 16);
		this.panelTS2.TabIndex = 11;
		this.labelTS3.AutoSize = true;
		this.labelTS3.Image = null;
		this.labelTS3.Location = new System.Drawing.Point(59, 95);
		this.labelTS3.Name = "labelTS3";
		this.labelTS3.Size = new System.Drawing.Size(398, 13);
		this.labelTS3.TabIndex = 4;
		this.labelTS3.Text = "-ve = the packet arriving is older, and has a sequence number lower than expected";
		this.panelTS1.AutoScrollMargin = new System.Drawing.Size(0, 0);
		this.panelTS1.AutoScrollMinSize = new System.Drawing.Size(0, 0);
		this.panelTS1.BackColor = System.Drawing.Color.Red;
		this.panelTS1.BackgroundImage = Thetis.Properties.Resources.warning4;
		this.panelTS1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		this.panelTS1.Location = new System.Drawing.Point(35, 95);
		this.panelTS1.Name = "panelTS1";
		this.panelTS1.Size = new System.Drawing.Size(18, 16);
		this.panelTS1.TabIndex = 10;
		this.labelTS4.AutoSize = true;
		this.labelTS4.Image = null;
		this.labelTS4.Location = new System.Drawing.Point(59, 127);
		this.labelTS4.Name = "labelTS4";
		this.labelTS4.Size = new System.Drawing.Size(388, 13);
		this.labelTS4.TabIndex = 5;
		this.labelTS4.Text = "+ve = the packet arriving is greater than expected. This is likely to be packet loss";
		this.btnCopyImageToClipboard.Enabled = false;
		this.btnCopyImageToClipboard.Image = null;
		this.btnCopyImageToClipboard.Location = new System.Drawing.Point(587, 61);
		this.btnCopyImageToClipboard.Name = "btnCopyImageToClipboard";
		this.btnCopyImageToClipboard.Size = new System.Drawing.Size(97, 61);
		this.btnCopyImageToClipboard.TabIndex = 9;
		this.btnCopyImageToClipboard.Text = "Copy Image To Clipboard";
		this.btnCopyImageToClipboard.UseVisualStyleBackColor = true;
		this.btnCopyImageToClipboard.Click += new System.EventHandler(btnCopyImageToClipboard_Click);
		this.labelTS5.AutoSize = true;
		this.labelTS5.Image = null;
		this.labelTS5.Location = new System.Drawing.Point(16, 37);
		this.labelTS5.Name = "labelTS5";
		this.labelTS5.Size = new System.Drawing.Size(343, 13);
		this.labelTS5.TabIndex = 6;
		this.labelTS5.Text = "A snapshot is taken when a sequence error is detected. s0 is the latest.";
		this.btnCopyToClipboard.Enabled = false;
		this.btnCopyToClipboard.Image = null;
		this.btnCopyToClipboard.Location = new System.Drawing.Point(690, 61);
		this.btnCopyToClipboard.Name = "btnCopyToClipboard";
		this.btnCopyToClipboard.Size = new System.Drawing.Size(97, 61);
		this.btnCopyToClipboard.TabIndex = 8;
		this.btnCopyToClipboard.Text = "Copy Text To Clipboard";
		this.btnCopyToClipboard.UseVisualStyleBackColor = true;
		this.btnCopyToClipboard.Click += new System.EventHandler(btnCopyToClipboard_Click);
		this.btnClear.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.btnClear.Image = null;
		this.btnClear.Location = new System.Drawing.Point(0, 714);
		this.btnClear.Name = "btnClear";
		this.btnClear.Size = new System.Drawing.Size(852, 31);
		this.btnClear.TabIndex = 0;
		this.btnClear.Text = "HOLDER";
		this.btnClear.UseVisualStyleBackColor = true;
		this.btnClear.Click += new System.EventHandler(btnClear_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(852, 745);
		base.Controls.Add(this.tabMain);
		base.Controls.Add(this.panelTS3);
		base.Controls.Add(this.btnClear);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		this.MinimumSize = new System.Drawing.Size(868, 784);
		base.Name = "frmSeqLog";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		this.Text = "SEQ Log";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmSeqLog_FormClosing);
		this.tabMain.ResumeLayout(false);
		this.tabPage1.ResumeLayout(false);
		this.tabPage1.PerformLayout();
		this.tabPage2.ResumeLayout(false);
		this.tabPage2.PerformLayout();
		this.groupDumpCap.ResumeLayout(false);
		this.groupDumpCap.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.udInterface).EndInit();
		this.panelTS3.ResumeLayout(false);
		this.panelTS3.PerformLayout();
		base.ResumeLayout(false);
	}
}
