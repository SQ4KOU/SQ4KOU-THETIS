using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Midi2Cat.IO;

namespace Thetis.Midi2Cat;

public class Midi2CatSetupForm : Form
{
	private string DbFile;

	private List<MidiDeviceSetup> Setups;

	private int startDelay = 5;

	private int MsgToShow;

	private string[] startupMessages = new string[6] { "Closing Thetis Midi Input", "Closing Thetis Midi Output", "Attaching Midi Input and Output to Midi Setup", "Midi Setup is opening Midi input and output", "Synchronising Console Controls with Midi Setup", "Initialising Database." };

	private IContainer components;

	private TabControl devicesTabControl;

	private Button saveButton;

	private Panel startupPanel;

	private Label label1;

	private Timer startTimer;

	private Label progressLabel;

	public Midi2CatSetupForm(string DbFile)
	{
		this.DbFile = DbFile;
		InitializeComponent();
	}

	private void Midi2CatSetupForm_Load(object sender, EventArgs e)
	{
		startTimer.Enabled = true;
		LoadSetup();
	}

	private void LoadSetup()
	{
		Setups = new List<MidiDeviceSetup>();
		MidiDevices midiDevices = new MidiDevices();
		int num = 0;
		foreach (string inDevice in midiDevices.InDevices)
		{
			MidiDeviceSetup midiDeviceSetup = new MidiDeviceSetup(DbFile, inDevice, num++);
			Setups.Add(midiDeviceSetup);
			TabPage tabPage = new TabPage(inDevice);
			tabPage.Controls.Add(midiDeviceSetup);
			midiDeviceSetup.Dock = DockStyle.Fill;
			devicesTabControl.TabPages.Add(tabPage);
		}
	}

	private void Midi2CatSetupForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		foreach (MidiDeviceSetup setup in Setups)
		{
			setup.Parent = null;
			setup.Dispose();
		}
	}

	private void saveButton_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void startTimer_Tick(object sender, EventArgs e)
	{
		if (startDelay >= 0)
		{
			progressLabel.Text = startupMessages[MsgToShow++];
			startDelay--;
		}
		else
		{
			startTimer.Enabled = false;
			startupPanel.Visible = false;
			devicesTabControl.Visible = true;
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
		this.devicesTabControl = new System.Windows.Forms.TabControl();
		this.saveButton = new System.Windows.Forms.Button();
		this.startupPanel = new System.Windows.Forms.Panel();
		this.progressLabel = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.startTimer = new System.Windows.Forms.Timer(this.components);
		this.startupPanel.SuspendLayout();
		base.SuspendLayout();
		this.devicesTabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.devicesTabControl.Location = new System.Drawing.Point(0, 0);
		this.devicesTabControl.Margin = new System.Windows.Forms.Padding(0);
		this.devicesTabControl.Name = "devicesTabControl";
		this.devicesTabControl.SelectedIndex = 0;
		this.devicesTabControl.Size = new System.Drawing.Size(748, 630);
		this.devicesTabControl.TabIndex = 1;
		this.devicesTabControl.Visible = false;
		this.saveButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.saveButton.Location = new System.Drawing.Point(667, 635);
		this.saveButton.Name = "saveButton";
		this.saveButton.Size = new System.Drawing.Size(75, 23);
		this.saveButton.TabIndex = 2;
		this.saveButton.Text = "Save";
		this.saveButton.UseVisualStyleBackColor = true;
		this.saveButton.Click += new System.EventHandler(saveButton_Click);
		this.startupPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.startupPanel.Controls.Add(this.progressLabel);
		this.startupPanel.Controls.Add(this.label1);
		this.startupPanel.Location = new System.Drawing.Point(162, 0);
		this.startupPanel.Name = "startupPanel";
		this.startupPanel.Size = new System.Drawing.Size(433, 83);
		this.startupPanel.TabIndex = 3;
		this.progressLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.progressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.progressLabel.Location = new System.Drawing.Point(3, 56);
		this.progressLabel.Name = "progressLabel";
		this.progressLabel.Size = new System.Drawing.Size(425, 20);
		this.progressLabel.TabIndex = 2;
		this.progressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label1.Location = new System.Drawing.Point(7, 19);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(421, 28);
		this.label1.TabIndex = 0;
		this.label1.Text = "Midi Controller Setup Is Initialising Please Wait...";
		this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.startTimer.Interval = 1000;
		this.startTimer.Tick += new System.EventHandler(startTimer_Tick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(748, 661);
		base.Controls.Add(this.startupPanel);
		base.Controls.Add(this.saveButton);
		base.Controls.Add(this.devicesTabControl);
		base.MinimizeBox = false;
		base.Name = "Midi2CatSetupForm";
		this.Text = "Midi Controller Setup";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(Midi2CatSetupForm_FormClosing);
		base.Load += new System.EventHandler(Midi2CatSetupForm_Load);
		this.startupPanel.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
