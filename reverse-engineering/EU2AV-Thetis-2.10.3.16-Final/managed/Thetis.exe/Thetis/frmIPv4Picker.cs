using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Thetis;

public class frmIPv4Picker : Form
{
	private string _ip = "";

	private int _port = -1;

	private bool _bPortOk;

	private string _sOldPort = "";

	private IContainer components;

	private ComboBoxTS comboAddresses;

	private ButtonTS btnSelect;

	private ButtonTS btnCancel;

	public string IP => _ip;

	public int Port => _port;

	public frmIPv4Picker()
	{
		InitializeComponent();
	}

	private void btnSelect_Click(object sender, EventArgs e)
	{
		if (comboAddresses.Text.Contains(":"))
		{
			string[] array = comboAddresses.Text.Split(':');
			if (array.Length == 2)
			{
				int port = _port;
				_bPortOk = int.TryParse(array[1], out _port);
				if (_bPortOk)
				{
					_ip = array[0] + ":" + array[1];
					return;
				}
				_port = port;
			}
		}
		if (_port != -1 && _bPortOk)
		{
			_ip = comboAddresses.Text + ":" + _port;
		}
		else if (_sOldPort != "")
		{
			_ip = comboAddresses.Text + ":" + _sOldPort.ToString();
		}
		else
		{
			_ip = comboAddresses.Text;
		}
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		_ip = "";
	}

	public void Init(string sIPPort, bool addBroadcast = false)
	{
		_port = -1;
		_ip = "";
		_bPortOk = false;
		_sOldPort = "";
		comboAddresses.Items.Clear();
		try
		{
			string[] array = sIPPort.Split(':');
			string text = "";
			string text2 = "";
			bool enabled = false;
			if (array.Length == 1)
			{
				text = sIPPort;
			}
			else if (array.Length > 1)
			{
				text = array[0];
				int.TryParse(_sOldPort = array[1], out _port);
			}
			if (IPAddress.TryParse(text, out var address))
			{
				text = address.ToString();
			}
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface networkInterface in allNetworkInterfaces)
			{
				if (networkInterface.OperationalStatus != OperationalStatus.Up)
				{
					continue;
				}
				foreach (UnicastIPAddressInformation unicastAddress in networkInterface.GetIPProperties().UnicastAddresses)
				{
					if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
					{
						string text3 = unicastAddress.Address.ToString();
						int selectedIndex = comboAddresses.Items.Add(text3);
						if (text3 == text)
						{
							comboAddresses.SelectedIndex = selectedIndex;
						}
						if (text3 == "255.255.255.255")
						{
							addBroadcast = false;
						}
						enabled = true;
					}
				}
			}
			if (addBroadcast)
			{
				comboAddresses.Items.Add("255.255.255.255");
			}
			btnSelect.Enabled = enabled;
		}
		catch
		{
			btnSelect.Enabled = false;
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
		this.btnCancel = new System.Windows.Forms.ButtonTS();
		this.btnSelect = new System.Windows.Forms.ButtonTS();
		this.comboAddresses = new System.Windows.Forms.ComboBoxTS();
		base.SuspendLayout();
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Image = null;
		this.btnCancel.Location = new System.Drawing.Point(81, 167);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(52, 23);
		this.btnCancel.TabIndex = 2;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.btnSelect.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnSelect.Image = null;
		this.btnSelect.Location = new System.Drawing.Point(12, 167);
		this.btnSelect.Name = "btnSelect";
		this.btnSelect.Size = new System.Drawing.Size(52, 23);
		this.btnSelect.TabIndex = 1;
		this.btnSelect.Text = "Select";
		this.btnSelect.UseVisualStyleBackColor = true;
		this.btnSelect.Click += new System.EventHandler(btnSelect_Click);
		this.comboAddresses.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
		this.comboAddresses.FormattingEnabled = true;
		this.comboAddresses.Items.AddRange(new object[4] { "192", "168", "0", "26" });
		this.comboAddresses.Location = new System.Drawing.Point(12, 12);
		this.comboAddresses.Name = "comboAddresses";
		this.comboAddresses.Size = new System.Drawing.Size(125, 150);
		this.comboAddresses.TabIndex = 0;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(149, 202);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnSelect);
		base.Controls.Add(this.comboAddresses);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Name = "frmIPv4Picker";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "IPv4 Address";
		base.ResumeLayout(false);
	}
}
