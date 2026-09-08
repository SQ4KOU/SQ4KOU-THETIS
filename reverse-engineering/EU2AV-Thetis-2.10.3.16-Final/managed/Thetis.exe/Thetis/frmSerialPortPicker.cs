using System;
using System.ComponentModel;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace Thetis;

public class frmSerialPortPicker : Form
{
	private int[] _baudRates = new int[15]
	{
		110, 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400,
		57600, 115200, 230400, 460800, 921600
	};

	private int[] _dataBits = new int[4] { 5, 6, 7, 8 };

	private StopBits[] _stopBits = new StopBits[4]
	{
		StopBits.One,
		StopBits.OnePointFive,
		StopBits.Two,
		StopBits.None
	};

	private Parity[] _parity = new Parity[5]
	{
		Parity.None,
		Parity.Odd,
		Parity.Even,
		Parity.Mark,
		Parity.Space
	};

	private string _com_port_setting;

	private int _baud_rate_setting;

	private int _data_bits_setting;

	private StopBits _stop_bits_setting;

	private Parity _parity_setting;

	private IContainer components;

	private ButtonTS btnCancel;

	private ButtonTS btnSelect;

	private ComboBoxTS comboComPort;

	private ComboBoxTS comboBaudRate;

	private ComboBoxTS comboDataBits;

	private ComboBoxTS comboStopBits;

	private ComboBoxTS comboParity;

	private LabelTS labelTS1;

	private LabelTS labelTS2;

	private LabelTS labelTS3;

	private LabelTS labelTS4;

	private LabelTS labelTS5;

	public string ComPort
	{
		get
		{
			return _com_port_setting;
		}
		set
		{
			_com_port_setting = value;
		}
	}

	public int BaudRate
	{
		get
		{
			return _baud_rate_setting;
		}
		set
		{
			_baud_rate_setting = value;
		}
	}

	public int DataBits
	{
		get
		{
			return _data_bits_setting;
		}
		set
		{
			_data_bits_setting = value;
		}
	}

	public StopBits StopBits
	{
		get
		{
			return _stop_bits_setting;
		}
		set
		{
			_stop_bits_setting = value;
		}
	}

	public Parity Parity
	{
		get
		{
			return _parity_setting;
		}
		set
		{
			_parity_setting = value;
		}
	}

	public frmSerialPortPicker()
	{
		InitializeComponent();
	}

	public void Init()
	{
		btnSelect.Enabled = false;
		comboComPort.Items.Clear();
		comboBaudRate.Items.Clear();
		comboDataBits.Items.Clear();
		comboStopBits.Items.Clear();
		comboParity.Items.Clear();
		string[] portNames = SerialPort.GetPortNames();
		foreach (string text in portNames)
		{
			int selectedIndex = comboComPort.Items.Add(text);
			if (text == _com_port_setting)
			{
				comboComPort.SelectedIndex = selectedIndex;
			}
		}
		if (comboComPort.SelectedIndex == -1)
		{
			updateCombos(enabled: false);
		}
	}

	private void updateCombos(bool enabled)
	{
		comboBaudRate.Enabled = enabled;
		comboDataBits.Enabled = enabled;
		comboStopBits.Enabled = enabled;
		comboParity.Enabled = enabled;
	}

	private void btnSelect_Click(object sender, EventArgs e)
	{
		bool flag = IsComPortAvailable(_com_port_setting);
		if (!flag)
		{
			MessageBox.Show(_com_port_setting + " can not be opened, it is probably already in use.");
		}
		if (flag)
		{
			flag = IsBaudRateSupported(_com_port_setting, _baud_rate_setting);
			if (!flag)
			{
				MessageBox.Show(_com_port_setting + " can not be opened with that baud rate.");
			}
		}
		if (flag)
		{
			flag = IsDataBitsSupported(_com_port_setting, _baud_rate_setting, _data_bits_setting);
			if (!flag)
			{
				MessageBox.Show(_com_port_setting + " can not be opened with those data bits.");
			}
		}
		if (flag)
		{
			flag = IsStopBitsSupported(_com_port_setting, _baud_rate_setting, _data_bits_setting, _stop_bits_setting);
			if (!flag)
			{
				MessageBox.Show(_com_port_setting + " can not be opened with those stop bits.");
			}
		}
		if (flag)
		{
			flag = IsParitySupported(_com_port_setting, _baud_rate_setting, _data_bits_setting, _stop_bits_setting, _parity_setting);
			if (!flag)
			{
				MessageBox.Show(_com_port_setting + " can not be opened with that parity.");
			}
		}
		if (flag)
		{
			base.DialogResult = DialogResult.OK;
			Close();
		}
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
	}

	private bool IsBaudRateSupported(string portName, int baudRate)
	{
		try
		{
			using (SerialPort serialPort = new SerialPort(portName, baudRate))
			{
				serialPort.ReadTimeout = 100;
				serialPort.WriteTimeout = 100;
				serialPort.Open();
				Thread.Sleep(100);
				serialPort.Close();
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private bool IsDataBitsSupported(string portName, int baudRate, int dataBits)
	{
		try
		{
			using (SerialPort serialPort = new SerialPort(portName, baudRate))
			{
				serialPort.ReadTimeout = 100;
				serialPort.WriteTimeout = 100;
				serialPort.DataBits = dataBits;
				serialPort.Open();
				Thread.Sleep(100);
				serialPort.Close();
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private bool IsStopBitsSupported(string portName, int baudRate, int dataBits, StopBits stopBits)
	{
		try
		{
			using (SerialPort serialPort = new SerialPort(portName, baudRate))
			{
				serialPort.ReadTimeout = 100;
				serialPort.WriteTimeout = 100;
				serialPort.DataBits = dataBits;
				serialPort.StopBits = stopBits;
				serialPort.Open();
				Thread.Sleep(100);
				serialPort.Close();
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private bool IsParitySupported(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
	{
		try
		{
			using (SerialPort serialPort = new SerialPort(portName, baudRate))
			{
				serialPort.ReadTimeout = 100;
				serialPort.WriteTimeout = 100;
				serialPort.DataBits = dataBits;
				serialPort.StopBits = stopBits;
				serialPort.Parity = parity;
				serialPort.Open();
				Thread.Sleep(100);
				serialPort.Close();
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static bool IsComPortAvailable(string portName)
	{
		try
		{
			using (SerialPort serialPort = new SerialPort(portName))
			{
				serialPort.ReadTimeout = 100;
				serialPort.WriteTimeout = 100;
				serialPort.Open();
				Thread.Sleep(100);
				serialPort.Close();
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private void comboComPort_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboComPort.SelectedIndex != -1)
		{
			btnSelect.Enabled = true;
			setupSubCombos(comboComPort.SelectedItem.ToString());
			_com_port_setting = comboComPort.SelectedItem.ToString();
		}
	}

	private void setupSubCombos(string com_port)
	{
		comboBaudRate.Items.Clear();
		comboDataBits.Items.Clear();
		comboStopBits.Items.Clear();
		comboParity.Items.Clear();
		if (string.IsNullOrEmpty(com_port))
		{
			return;
		}
		updateCombos(enabled: true);
		int[] baudRates = _baudRates;
		foreach (int num in baudRates)
		{
			int selectedIndex = comboBaudRate.Items.Add(num);
			if (num == _baud_rate_setting)
			{
				comboBaudRate.SelectedIndex = selectedIndex;
			}
		}
		baudRates = _dataBits;
		foreach (int num2 in baudRates)
		{
			int selectedIndex2 = comboDataBits.Items.Add(num2);
			if (num2 == _data_bits_setting)
			{
				comboDataBits.SelectedIndex = selectedIndex2;
			}
		}
		StopBits[] stopBits = _stopBits;
		foreach (StopBits stopBits2 in stopBits)
		{
			string item = "";
			switch (stopBits2)
			{
			case StopBits.One:
				item = "1";
				break;
			case StopBits.Two:
				item = "2";
				break;
			case StopBits.OnePointFive:
				item = "1.5";
				break;
			case StopBits.None:
				item = "None";
				break;
			}
			int selectedIndex3 = comboStopBits.Items.Add(item);
			if (stopBits2 == _stop_bits_setting)
			{
				comboStopBits.SelectedIndex = selectedIndex3;
			}
		}
		Parity[] parity = _parity;
		foreach (Parity parity2 in parity)
		{
			string item2 = "";
			switch (parity2)
			{
			case Parity.Odd:
				item2 = "Odd";
				break;
			case Parity.Even:
				item2 = "Even";
				break;
			case Parity.Mark:
				item2 = "Mark";
				break;
			case Parity.Space:
				item2 = "Space";
				break;
			case Parity.None:
				item2 = "None";
				break;
			}
			int selectedIndex4 = comboParity.Items.Add(item2);
			if (parity2 == _parity_setting)
			{
				comboParity.SelectedIndex = selectedIndex4;
			}
		}
	}

	private void comboBaudRate_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboBaudRate.SelectedIndex != -1)
		{
			int.TryParse(comboBaudRate.SelectedItem.ToString(), out var result);
			_baud_rate_setting = result;
		}
	}

	private void comboDataBits_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboDataBits.SelectedIndex != -1)
		{
			int.TryParse(comboDataBits.SelectedItem.ToString(), out var result);
			_data_bits_setting = result;
		}
	}

	private void comboStopBits_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboStopBits.SelectedIndex != -1)
		{
			switch (comboStopBits.SelectedItem.ToString())
			{
			case "1":
				_stop_bits_setting = StopBits.One;
				break;
			case "2":
				_stop_bits_setting = StopBits.Two;
				break;
			case "1.5":
				_stop_bits_setting = StopBits.OnePointFive;
				break;
			case "None":
				_stop_bits_setting = StopBits.None;
				break;
			}
		}
	}

	private void comboParity_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (comboParity.SelectedIndex != -1)
		{
			switch (comboParity.SelectedItem.ToString())
			{
			case "Odd":
				_parity_setting = Parity.Odd;
				break;
			case "Even":
				_parity_setting = Parity.Even;
				break;
			case "Mark":
				_parity_setting = Parity.Mark;
				break;
			case "Space":
				_parity_setting = Parity.Space;
				break;
			case "None":
				_parity_setting = Parity.None;
				break;
			}
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
		this.labelTS5 = new System.Windows.Forms.LabelTS();
		this.labelTS4 = new System.Windows.Forms.LabelTS();
		this.labelTS3 = new System.Windows.Forms.LabelTS();
		this.labelTS2 = new System.Windows.Forms.LabelTS();
		this.labelTS1 = new System.Windows.Forms.LabelTS();
		this.comboParity = new System.Windows.Forms.ComboBoxTS();
		this.comboStopBits = new System.Windows.Forms.ComboBoxTS();
		this.comboDataBits = new System.Windows.Forms.ComboBoxTS();
		this.comboBaudRate = new System.Windows.Forms.ComboBoxTS();
		this.comboComPort = new System.Windows.Forms.ComboBoxTS();
		this.btnCancel = new System.Windows.Forms.ButtonTS();
		this.btnSelect = new System.Windows.Forms.ButtonTS();
		base.SuspendLayout();
		this.labelTS5.AutoSize = true;
		this.labelTS5.Image = null;
		this.labelTS5.Location = new System.Drawing.Point(37, 133);
		this.labelTS5.Name = "labelTS5";
		this.labelTS5.Size = new System.Drawing.Size(36, 13);
		this.labelTS5.TabIndex = 16;
		this.labelTS5.Text = "Parity:";
		this.labelTS4.AutoSize = true;
		this.labelTS4.Image = null;
		this.labelTS4.Location = new System.Drawing.Point(21, 106);
		this.labelTS4.Name = "labelTS4";
		this.labelTS4.Size = new System.Drawing.Size(52, 13);
		this.labelTS4.TabIndex = 15;
		this.labelTS4.Text = "Stop Bits:";
		this.labelTS3.AutoSize = true;
		this.labelTS3.Image = null;
		this.labelTS3.Location = new System.Drawing.Point(20, 79);
		this.labelTS3.Name = "labelTS3";
		this.labelTS3.Size = new System.Drawing.Size(53, 13);
		this.labelTS3.TabIndex = 14;
		this.labelTS3.Text = "Data Bits:";
		this.labelTS2.AutoSize = true;
		this.labelTS2.Image = null;
		this.labelTS2.Location = new System.Drawing.Point(12, 52);
		this.labelTS2.Name = "labelTS2";
		this.labelTS2.Size = new System.Drawing.Size(61, 13);
		this.labelTS2.TabIndex = 13;
		this.labelTS2.Text = "Baud Rate:";
		this.labelTS1.AutoSize = true;
		this.labelTS1.Image = null;
		this.labelTS1.Location = new System.Drawing.Point(20, 25);
		this.labelTS1.Name = "labelTS1";
		this.labelTS1.Size = new System.Drawing.Size(53, 13);
		this.labelTS1.TabIndex = 12;
		this.labelTS1.Text = "Com Port:";
		this.comboParity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboParity.FormattingEnabled = true;
		this.comboParity.Location = new System.Drawing.Point(79, 130);
		this.comboParity.Name = "comboParity";
		this.comboParity.Size = new System.Drawing.Size(121, 21);
		this.comboParity.TabIndex = 11;
		this.comboParity.SelectedIndexChanged += new System.EventHandler(comboParity_SelectedIndexChanged);
		this.comboStopBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboStopBits.FormattingEnabled = true;
		this.comboStopBits.Location = new System.Drawing.Point(79, 103);
		this.comboStopBits.Name = "comboStopBits";
		this.comboStopBits.Size = new System.Drawing.Size(121, 21);
		this.comboStopBits.TabIndex = 10;
		this.comboStopBits.SelectedIndexChanged += new System.EventHandler(comboStopBits_SelectedIndexChanged);
		this.comboDataBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboDataBits.FormattingEnabled = true;
		this.comboDataBits.Location = new System.Drawing.Point(79, 76);
		this.comboDataBits.Name = "comboDataBits";
		this.comboDataBits.Size = new System.Drawing.Size(121, 21);
		this.comboDataBits.TabIndex = 9;
		this.comboDataBits.SelectedIndexChanged += new System.EventHandler(comboDataBits_SelectedIndexChanged);
		this.comboBaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBaudRate.FormattingEnabled = true;
		this.comboBaudRate.Location = new System.Drawing.Point(79, 49);
		this.comboBaudRate.Name = "comboBaudRate";
		this.comboBaudRate.Size = new System.Drawing.Size(121, 21);
		this.comboBaudRate.TabIndex = 8;
		this.comboBaudRate.SelectedIndexChanged += new System.EventHandler(comboBaudRate_SelectedIndexChanged);
		this.comboComPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboComPort.FormattingEnabled = true;
		this.comboComPort.Location = new System.Drawing.Point(79, 22);
		this.comboComPort.Name = "comboComPort";
		this.comboComPort.Size = new System.Drawing.Size(121, 21);
		this.comboComPort.TabIndex = 7;
		this.comboComPort.SelectedIndexChanged += new System.EventHandler(comboComPort_SelectedIndexChanged);
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Image = null;
		this.btnCancel.Location = new System.Drawing.Point(175, 180);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Selectable = true;
		this.btnCancel.Size = new System.Drawing.Size(52, 23);
		this.btnCancel.TabIndex = 6;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.btnSelect.Image = null;
		this.btnSelect.Location = new System.Drawing.Point(117, 180);
		this.btnSelect.Name = "btnSelect";
		this.btnSelect.Selectable = true;
		this.btnSelect.Size = new System.Drawing.Size(52, 23);
		this.btnSelect.TabIndex = 5;
		this.btnSelect.Text = "Select";
		this.btnSelect.UseVisualStyleBackColor = true;
		this.btnSelect.Click += new System.EventHandler(btnSelect_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(234, 211);
		base.Controls.Add(this.labelTS5);
		base.Controls.Add(this.labelTS4);
		base.Controls.Add(this.labelTS3);
		base.Controls.Add(this.labelTS2);
		base.Controls.Add(this.labelTS1);
		base.Controls.Add(this.comboParity);
		base.Controls.Add(this.comboStopBits);
		base.Controls.Add(this.comboDataBits);
		base.Controls.Add(this.comboBaudRate);
		base.Controls.Add(this.comboComPort);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnSelect);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		this.MaximumSize = new System.Drawing.Size(250, 320);
		this.MinimumSize = new System.Drawing.Size(250, 100);
		base.Name = "frmSerialPortPicker";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Serial Port Picker";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
