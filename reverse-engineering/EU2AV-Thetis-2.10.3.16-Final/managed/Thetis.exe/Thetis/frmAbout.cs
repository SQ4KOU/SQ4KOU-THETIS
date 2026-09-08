using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Thetis;

public class frmAbout : Form
{
	private class ThetisVersionInfo
	{
		public string ReleaseVersion { get; set; }

		public string ReleaseBuild { get; set; }

		public string ReleaseURL { get; set; }

		public string ReleaseName { get; set; }

		public string DevelopmentVersion { get; set; }

		public string DevelopmentBuild { get; set; }

		public string DevelopmentURL { get; set; }

		public string DevelopmentName { get; set; }
	}

	private const string GITHUB_VERSION_JSON_RAW = "https://raw.githubusercontent.com/ramdor/Thetis/refs/heads/master/version.json";

	private ThetisVersionInfo _versionInfo;

	private CancellationTokenSource _cancellationTokenSource;

	private Task _fetchJsonTask;

	private readonly object _version_info_lock = new object();

	private string _version;

	private string _build;

	private bool _update_available;

	private Console _console;

	private bool _check_dev_version;

	private string _exe_path;

	private IContainer components;

	private ListBox lstContributors;

	private LabelTS labelTS1;

	private ButtonTS btnOK;

	private ListBox lstVersions;

	private LabelTS labelTS2;

	private LinkLabel lnkLicence;

	private ButtonTS btnCopyContributors;

	private ButtonTS btnSysInfo;

	private ButtonTS btnDXDiag;

	private ListBox lstLinks;

	private ButtonTS btnVisit;

	private LabelTS labelTS3;

	private LabelTS labelTS4;

	private ButtonTS btnUpdatedRelease;

	private ToolTip toolTip1;

	private ButtonTS btnReleaseNotes;

	private bool UpdateAvaialble
	{
		get
		{
			lock (_version_info_lock)
			{
				return _update_available;
			}
		}
	}

	public frmAbout(Console console, bool check_dev_version)
	{
		InitializeComponent();
		base.TopMost = true;
		_exe_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
		_console = console;
		_update_available = false;
		_versionInfo = null;
		_version = "";
		_build = "";
		_check_dev_version = check_dev_version;
		btnVisit.Enabled = false;
		btnUpdatedRelease.Visible = false;
		_cancellationTokenSource = new CancellationTokenSource();
		_fetchJsonTask = fetchJsonAsync(_cancellationTokenSource.Token);
	}

	public void InitVersions(string version, string build, string db_version, string dx_version, string radio_model, string firmware_version, string protocol, string supported_protocol, string wdsp_version, string channel_master_version, string cmASIO_version, string portAudio_version, string andromeda_version)
	{
		_version = version;
		_build = build;
		cancelFetchJsonTask();
		if (_cancellationTokenSource == null)
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_fetchJsonTask = fetchJsonAsync(_cancellationTokenSource.Token);
		}
		lstLinks.ClearSelected();
		btnVisit.Enabled = false;
		if (string.IsNullOrEmpty(firmware_version))
		{
			firmware_version = "?";
		}
		if (string.IsNullOrEmpty(radio_model))
		{
			firmware_version = "?";
		}
		if (radio_model == HPSDRModel.FIRST.ToString() || radio_model == HPSDRModel.LAST.ToString())
		{
			radio_model = "?";
		}
		if (!string.IsNullOrEmpty(build))
		{
			build = build.Left(16);
			version = version + " [build " + build + "]";
		}
		lstVersions.Items.Clear();
		lstVersions.Items.Add("Version: " + version);
		lstVersions.Items.Add("Database Version: " + db_version);
		lstVersions.Items.Add("Radio Model: " + radio_model);
		lstVersions.Items.Add("Fork: Anvelina PRO3 DX by eu2av (Belarus)");
		lstVersions.Items.Add(NetworkIO.Supports24BitAudio ? "Audio: 24-bit RX/TX (TLV320AIC3204, hw_rev detected)" : "Audio: 16-bit (legacy codec)");
		if (!string.IsNullOrEmpty(andromeda_version))
		{
			lstVersions.Items.Add(andromeda_version);
		}
		lstVersions.Items.Add("Firmware Version: " + firmware_version);
		string text = ((!string.IsNullOrEmpty(supported_protocol)) ? (" (v" + supported_protocol + ")") : "");
		lstVersions.Items.Add("Protocol: " + protocol + text);
		lstVersions.Items.Add("WDSP Version: " + wdsp_version);
		lstVersions.Items.Add("ChannelMaster: " + channel_master_version);
		lstVersions.Items.Add("cmASIO Version: " + cmASIO_version);
		lstVersions.Items.Add("PortAudio Version: " + portAudio_version);
		if (!string.IsNullOrEmpty(firmware_version))
		{
			lstVersions.Items.Add("DirectX Version: " + dx_version);
		}
	}

	private void btnOK_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
	}

	private void btnCopyContributors_Click(object sender, EventArgs e)
	{
		string text = "Thetis\n";
		foreach (string item in lstVersions.Items)
		{
			text = text + item + "\n";
		}
		try
		{
			Clipboard.SetText(text);
		}
		catch
		{
		}
	}

	private void btnSysInfo_Click(object sender, EventArgs e)
	{
		Common.OpenUri("msinfo32.exe", check_uri: false);
	}

	private void btnDXDiag_Click(object sender, EventArgs e)
	{
		Common.OpenUri("dxdiag.exe", check_uri: false);
	}

	private void lnkLicence_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		lnkLicence.LinkVisited = Common.OpenUri("https://github.com/eu2av/OpenHPSDR-Thetis-Enhanced?tab=GPL-2.0-1-ov-file");
	}

	private void btnVisit_Click(object sender, EventArgs e)
	{
		if (lstLinks.SelectedItems.Count == 0)
		{
			btnVisit.Enabled = false;
			return;
		}
		switch (lstLinks.SelectedIndex)
		{
		case 0:
			Common.OpenUri("https://github.com/ramdor/Thetis/releases");
			break;
		case 1:
			Common.OpenUri("https://discord.gg/6fHCRKnDc9");
			break;
		case 2:
			Common.OpenUri("https://community.apache-labs.com/index.php");
			break;
		case 3:
			Common.OpenUri("https://apache-labs.com/");
			break;
		case 4:
			Common.OpenUri("https://github.com/TAPR/OpenHPSDR-Protocol1-Programmers");
			break;
		case 5:
			Common.OpenUri("https://github.com/TAPR/OpenHPSDR-Protocol2-Programmers");
			break;
		case 6:
			Common.OpenUri("https://community.apache-labs.com/viewforum.php?f=23");
			break;
		case 7:
			Common.OpenUri("https://community.apache-labs.com/viewtopic.php?f=27&t=3080");
			break;
		case 8:
			Common.OpenUri("https://community.apache-labs.com/viewtopic.php?f=32&t=4972");
			break;
		case 9:
			Common.OpenUri("https://github.com/laurencebarker/Saturn");
			break;
		case 10:
			Common.OpenUri("https://github.com/mi0bot/OpenHPSDR-Thetis/releases");
			break;
		case 11:
			Common.OpenUri("https://github.com/TAPR/OpenHPSDR-wdsp");
			break;
		case 12:
			Common.OpenUri("https://www.oe3ide.com/wp/software/");
			break;
		case 14:
			Common.OpenUri("file://" + _exe_path + "Thetis manual.pdf", check_uri: false);
			break;
		case 15:
			Common.OpenUri("file://" + _exe_path + "Thetis-CAT-Command-Reference-Guide-V3.pdf", check_uri: false);
			break;
		case 16:
			Common.OpenUri("file://" + _exe_path + "PureSignal.pdf", check_uri: false);
			break;
		case 17:
			Common.OpenUri("file://" + _exe_path + "Midi2Cat_Instructions_V3.pdf", check_uri: false);
			break;
		case 18:
			Common.OpenUri("file://" + _exe_path + "cmASIO Guide.pdf", check_uri: false);
			break;
		case 19:
			Common.OpenUri("file://" + _exe_path + "BehringerMods_Midi2Cat_v2.pdf", check_uri: false);
			break;
		case 20:
			Common.OpenUri("file://" + _exe_path + "APFtypes.pdf", check_uri: false);
			break;
		case 21:
			Common.OpenUri("file://" + _exe_path + "Thetis Network Settings_0.2.pdf", check_uri: false);
			break;
		}
		lstLinks.ClearSelected();
		btnVisit.Enabled = false;
	}

	private void lstLinks_SelectedIndexChanged(object sender, EventArgs e)
	{
		btnVisit.Enabled = lstLinks.SelectedItems.Count > 0;
	}

	private void handleVersionInfo()
	{
		lock (_version_info_lock)
		{
			int num = Common.CompareVersions(_version, _versionInfo.ReleaseVersion);
			bool flag = !string.IsNullOrEmpty(_versionInfo.ReleaseBuild) && _versionInfo.ReleaseBuild != _build;
			if (num < 0 || ((num == 0) & flag))
			{
				btnUpdatedRelease.Text = "Release version [" + _versionInfo.ReleaseVersion + "]\n" + _versionInfo.ReleaseName + "\nClick to view on GitHub";
				btnUpdatedRelease.Tag = _versionInfo.ReleaseURL;
				btnUpdatedRelease.Visible = true;
				_update_available = true;
			}
			else if (_check_dev_version)
			{
				int num2 = Common.CompareVersions(_version, _versionInfo.DevelopmentVersion);
				bool flag2 = !string.IsNullOrEmpty(_versionInfo.DevelopmentBuild) && _versionInfo.DevelopmentBuild != _build;
				if (num2 < 0 || ((num2 == 0) & flag2))
				{
					string text = (string.IsNullOrEmpty(_versionInfo.DevelopmentBuild) ? "" : (" " + _versionInfo.DevelopmentBuild));
					string text2 = ((_versionInfo.DevelopmentURL != null && _versionInfo.DevelopmentURL.Contains("discord", StringComparison.OrdinalIgnoreCase)) ? "Discord" : "GitHub");
					btnUpdatedRelease.Text = "Dev version [" + _versionInfo.DevelopmentVersion + text + "]\n" + _versionInfo.DevelopmentName + "\nClick to view on " + text2;
					btnUpdatedRelease.Tag = _versionInfo.DevelopmentURL;
					btnUpdatedRelease.Visible = true;
					_update_available = true;
				}
				else
				{
					btnUpdatedRelease.Visible = false;
					_update_available = false;
				}
			}
		}
	}

	private void cancelFetchJsonTask()
	{
		if (_cancellationTokenSource != null)
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = null;
		}
	}

	private async Task fetchJsonAsync(CancellationToken cancellationToken)
	{
		using HttpClient client = new HttpClient();
		client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
		{
			NoCache = true,
			NoStore = true,
			MustRevalidate = true
		};
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				HttpResponseMessage obj = await client.GetAsync("https://raw.githubusercontent.com/ramdor/Thetis/refs/heads/master/version.json", cancellationToken);
				obj.EnsureSuccessStatusCode();
				string value = await obj.Content.ReadAsStringAsync();
				lock (_version_info_lock)
				{
					_versionInfo = JsonConvert.DeserializeObject<ThetisVersionInfo>(value);
				}
				if (base.IsHandleCreated)
				{
					Invoke((Action)delegate
					{
						handleVersionInfo();
					});
				}
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (Exception)
			{
			}
			try
			{
				await Task.Delay(TimeSpan.FromMinutes(30.0), cancellationToken);
			}
			catch (TaskCanceledException)
			{
				break;
			}
		}
	}

	private void btnUpdatedRelease_Click(object sender, EventArgs e)
	{
		if (!string.IsNullOrEmpty(btnUpdatedRelease.Tag.ToString()))
		{
			Common.OpenUri(btnUpdatedRelease.Tag.ToString());
		}
	}

	private void btnReleaseNotes_Click(object sender, EventArgs e)
	{
		if (_console != null)
		{
			_console.ShowReleaseNotes();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.frmAbout));
		this.lstContributors = new System.Windows.Forms.ListBox();
		this.lstVersions = new System.Windows.Forms.ListBox();
		this.lnkLicence = new System.Windows.Forms.LinkLabel();
		this.lstLinks = new System.Windows.Forms.ListBox();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		this.btnReleaseNotes = new System.Windows.Forms.ButtonTS();
		this.btnUpdatedRelease = new System.Windows.Forms.ButtonTS();
		this.btnVisit = new System.Windows.Forms.ButtonTS();
		this.btnDXDiag = new System.Windows.Forms.ButtonTS();
		this.btnSysInfo = new System.Windows.Forms.ButtonTS();
		this.btnCopyContributors = new System.Windows.Forms.ButtonTS();
		this.labelTS4 = new System.Windows.Forms.LabelTS();
		this.labelTS3 = new System.Windows.Forms.LabelTS();
		this.labelTS2 = new System.Windows.Forms.LabelTS();
		this.btnOK = new System.Windows.Forms.ButtonTS();
		this.labelTS1 = new System.Windows.Forms.LabelTS();
		base.SuspendLayout();
		this.lstContributors.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.lstContributors.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lstContributors.FormattingEnabled = true;
		this.lstContributors.ItemHeight = 16;
		this.lstContributors.Items.AddRange(new object[25]
		{
			"NR0V, Warren (WDSP & too many other contributions to list)", "G8NJJ, Laurence (G2, Andromeda & protocols)", "N1GP, Rick (Firmware related changes)", "W4WMT, Bryan (Resampler, VAC & cmASIO)", "MI0BOT, Reid (Hermes Lite 2)", "MW0LGE, Richie (UI & various)", "W5WC, Doug (UI, ChannelMaster, various & Thetis naming)", "W2PA, Chris (QSK & MIDI)", "WD5Y, Joe (UI tweaks and fixes)", "M0YGG, Andrew (MIDI & various)",
			"", "VK6PH, Phil (Firmware, Protocols & other)", "KD5TFD, Bill (Protocol 1 initial implementation, UI & various)", "K5SO, Joe (Diversity Reception & firmware)", "WA8YWQ, Dave (various)", "KE9NS, Darrin (various)", "EU2AV, Yurij (PureSignal enhancements, feedback calibration, Anvelina PRO3 firmware update & firmware)", "", "WU2O, Scott (forum admin & ideas/feedback)", "NC3Z, Gary (forum mod)",
			"OE3IDE, Ernst (skins & primary tester)", "W1AEX, Rob (skins & audio information)", "DH1KLM, Sigi (midi, skins & UI improvements)", "", "and indirectly, all the testers"
		});
		this.lstContributors.Location = new System.Drawing.Point(19, 267);
		this.lstContributors.Name = "lstContributors";
		this.lstContributors.SelectionMode = System.Windows.Forms.SelectionMode.None;
		this.lstContributors.Size = new System.Drawing.Size(490, 162);
		this.lstContributors.TabIndex = 0;
		this.lstVersions.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lstVersions.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lstVersions.FormattingEnabled = true;
		this.lstVersions.ItemHeight = 16;
		this.lstVersions.Items.AddRange(new object[11]
		{
			"Version : 2.10.3.6 WWWWWWWWWWWWWWWW", "Database : 2.10.3", "Radio Model : ANAN7000", "Andromeda Info:", "Firmware Version : ?", "Protocol : 2 (v1.1)", "WDSP Version :", "ChannelMaster Version :", "cmASIO Version :", "PortAudio Version :",
			"DirectX Version: 12.1"
		});
		this.lstVersions.HorizontalScrollbar = true;
		this.lstVersions.Location = new System.Drawing.Point(19, 54);
		this.lstVersions.Name = "lstVersions";
		this.lstVersions.SelectionMode = System.Windows.Forms.SelectionMode.None;
		this.lstVersions.Size = new System.Drawing.Size(417, 176);
		this.lstVersions.TabIndex = 3;
		this.lnkLicence.AutoSize = true;
		this.lnkLicence.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lnkLicence.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
		this.lnkLicence.Location = new System.Drawing.Point(612, 31);
		this.lnkLicence.Name = "lnkLicence";
		this.lnkLicence.Size = new System.Drawing.Size(96, 16);
		this.lnkLicence.TabIndex = 5;
		this.lnkLicence.TabStop = true;
		this.lnkLicence.Text = "License Terms";
		this.lnkLicence.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(lnkLicence_LinkClicked);
		this.lstLinks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.lstLinks.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lstLinks.FormattingEnabled = true;
		this.lstLinks.ItemHeight = 16;
		this.lstLinks.Items.AddRange(new object[22]
		{
			"Thetis Releases on GitHub", "Join Thetis Discord Server", "Apache Labs Community Forums", "Apache Labs Home Page", "Protocol1 Programmers on GitHub", "Protocol2 Programmers on GitHub", "Firmware Discussions (all)", "Protocol1 Firmware (7000/8000)", "Protocol2 Firmware (7000/8000) RF fix", "G2 Firmware & Software \"p2app\"",
			"Thetis for Hermes Lite 2 on GitHub", "WDSP Documentation on GitHub", "OE3IDE's (Ernst) Connectors & Tools", "---MANUALS---", "Thetis", "Cat Command Reference", "PureSignal", "Midi", "cmASIO", "Behringer",
			"APF Types", "Network Settings"
		});
		this.lstLinks.Location = new System.Drawing.Point(445, 108);
		this.lstLinks.Name = "lstLinks";
		this.lstLinks.Size = new System.Drawing.Size(267, 130);
		this.lstLinks.TabIndex = 9;
		this.lstLinks.SelectedIndexChanged += new System.EventHandler(lstLinks_SelectedIndexChanged);
		this.btnReleaseNotes.BackColor = System.Drawing.SystemColors.ControlLight;
		this.btnReleaseNotes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnReleaseNotes.Image = null;
		this.btnReleaseNotes.Location = new System.Drawing.Point(524, 388);
		this.btnReleaseNotes.Name = "btnReleaseNotes";
		this.btnReleaseNotes.Selectable = true;
		this.btnReleaseNotes.Size = new System.Drawing.Size(88, 41);
		this.btnReleaseNotes.TabIndex = 14;
		this.btnReleaseNotes.Text = "Release\r\nNotes";
		this.toolTip1.SetToolTip(this.btnReleaseNotes, "Show the release notes");
		this.btnReleaseNotes.UseVisualStyleBackColor = false;
		this.btnReleaseNotes.Click += new System.EventHandler(btnReleaseNotes_Click);
		this.btnUpdatedRelease.BackColor = System.Drawing.Color.FromArgb(255, 128, 128);
		this.btnUpdatedRelease.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnUpdatedRelease.Image = null;
		this.btnUpdatedRelease.Location = new System.Drawing.Point(416, 8);
		this.btnUpdatedRelease.Name = "btnUpdatedRelease";
		this.btnUpdatedRelease.Selectable = true;
		this.btnUpdatedRelease.Size = new System.Drawing.Size(196, 58);
		this.btnUpdatedRelease.TabIndex = 13;
		this.btnUpdatedRelease.Text = "Version 2.10.3.8\r\nhas been released";
		this.toolTip1.SetToolTip(this.btnUpdatedRelease, "New release available");
		this.btnUpdatedRelease.UseVisualStyleBackColor = false;
		this.btnUpdatedRelease.Click += new System.EventHandler(btnUpdatedRelease_Click);
		this.btnVisit.BackColor = System.Drawing.SystemColors.ControlLight;
		this.btnVisit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnVisit.Image = null;
		this.btnVisit.Location = new System.Drawing.Point(624, 78);
		this.btnVisit.Name = "btnVisit";
		this.btnVisit.Selectable = true;
		this.btnVisit.Size = new System.Drawing.Size(88, 23);
		this.btnVisit.TabIndex = 10;
		this.btnVisit.Text = "View";
		this.toolTip1.SetToolTip(this.btnVisit, "Visit he selected link");
		this.btnVisit.UseVisualStyleBackColor = false;
		this.btnVisit.Click += new System.EventHandler(btnVisit_Click);
		this.btnDXDiag.BackColor = System.Drawing.SystemColors.ControlLight;
		this.btnDXDiag.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnDXDiag.Image = null;
		this.btnDXDiag.Location = new System.Drawing.Point(524, 325);
		this.btnDXDiag.Name = "btnDXDiag";
		this.btnDXDiag.Selectable = true;
		this.btnDXDiag.Size = new System.Drawing.Size(88, 23);
		this.btnDXDiag.TabIndex = 8;
		this.btnDXDiag.Text = "DxDiag";
		this.toolTip1.SetToolTip(this.btnDXDiag, "Run dxDiag");
		this.btnDXDiag.UseVisualStyleBackColor = false;
		this.btnDXDiag.Click += new System.EventHandler(btnDXDiag_Click);
		this.btnSysInfo.BackColor = System.Drawing.SystemColors.ControlLight;
		this.btnSysInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnSysInfo.Image = null;
		this.btnSysInfo.Location = new System.Drawing.Point(524, 296);
		this.btnSysInfo.Name = "btnSysInfo";
		this.btnSysInfo.Selectable = true;
		this.btnSysInfo.Size = new System.Drawing.Size(88, 23);
		this.btnSysInfo.TabIndex = 7;
		this.btnSysInfo.Text = "System Info";
		this.toolTip1.SetToolTip(this.btnSysInfo, "Show system info");
		this.btnSysInfo.UseVisualStyleBackColor = false;
		this.btnSysInfo.Click += new System.EventHandler(btnSysInfo_Click);
		this.btnCopyContributors.BackColor = System.Drawing.SystemColors.ControlLight;
		this.btnCopyContributors.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnCopyContributors.Image = null;
		this.btnCopyContributors.Location = new System.Drawing.Point(524, 267);
		this.btnCopyContributors.Name = "btnCopyContributors";
		this.btnCopyContributors.Selectable = true;
		this.btnCopyContributors.Size = new System.Drawing.Size(88, 23);
		this.btnCopyContributors.TabIndex = 6;
		this.btnCopyContributors.Text = "Copy Info";
		this.toolTip1.SetToolTip(this.btnCopyContributors, "Copy the version info to the clipboard");
		this.btnCopyContributors.UseVisualStyleBackColor = false;
		this.btnCopyContributors.Click += new System.EventHandler(btnCopyContributors_Click);
		this.labelTS4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.labelTS4.Image = null;
		this.labelTS4.Location = new System.Drawing.Point(16, 441);
		this.labelTS4.Name = "labelTS4";
		this.labelTS4.Size = new System.Drawing.Size(493, 86);
		this.labelTS4.TabIndex = 12;
		this.labelTS4.Text = resources.GetString("labelTS4.Text");
		this.labelTS3.AutoSize = true;
		this.labelTS3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.labelTS3.Image = null;
		this.labelTS3.Location = new System.Drawing.Point(442, 85);
		this.labelTS3.Name = "labelTS3";
		this.labelTS3.Size = new System.Drawing.Size(102, 16);
		this.labelTS3.TabIndex = 11;
		this.labelTS3.Text = "Links / Manuals:";
		this.labelTS2.AutoSize = true;
		this.labelTS2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.labelTS2.Image = null;
		this.labelTS2.Location = new System.Drawing.Point(16, 244);
		this.labelTS2.Name = "labelTS2";
		this.labelTS2.Size = new System.Drawing.Size(81, 16);
		this.labelTS2.TabIndex = 4;
		this.labelTS2.Text = "Contributors:";
		this.btnOK.BackColor = System.Drawing.SystemColors.ControlLight;
		this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnOK.Image = null;
		this.btnOK.Location = new System.Drawing.Point(524, 499);
		this.btnOK.Name = "btnOK";
		this.btnOK.Selectable = true;
		this.btnOK.Size = new System.Drawing.Size(88, 23);
		this.btnOK.TabIndex = 2;
		this.btnOK.Text = "OK";
		this.btnOK.UseVisualStyleBackColor = false;
		this.btnOK.Click += new System.EventHandler(btnOK_Click);
		this.labelTS1.AutoSize = true;
		this.labelTS1.Font = new System.Drawing.Font("Microsoft Sans Serif", 27.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelTS1.ForeColor = System.Drawing.Color.Teal;
		this.labelTS1.Image = null;
		this.labelTS1.Location = new System.Drawing.Point(12, 9);
		this.labelTS1.Name = "labelTS1";
		this.labelTS1.Size = new System.Drawing.Size(126, 42);
		this.labelTS1.TabIndex = 1;
		this.labelTS1.Text = "Thetis - Anvelina PRO3 DX";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.Window;
		base.ClientSize = new System.Drawing.Size(724, 534);
		base.Controls.Add(this.btnReleaseNotes);
		base.Controls.Add(this.btnUpdatedRelease);
		base.Controls.Add(this.labelTS4);
		base.Controls.Add(this.labelTS3);
		base.Controls.Add(this.btnVisit);
		base.Controls.Add(this.lstLinks);
		base.Controls.Add(this.btnDXDiag);
		base.Controls.Add(this.btnSysInfo);
		base.Controls.Add(this.btnCopyContributors);
		base.Controls.Add(this.lnkLicence);
		base.Controls.Add(this.labelTS2);
		base.Controls.Add(this.lstVersions);
		base.Controls.Add(this.btnOK);
		base.Controls.Add(this.labelTS1);
		base.Controls.Add(this.lstContributors);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmAbout";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "About Thetis";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
