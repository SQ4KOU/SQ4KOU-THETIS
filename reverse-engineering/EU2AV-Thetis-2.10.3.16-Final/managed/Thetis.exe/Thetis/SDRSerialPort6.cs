using System;
using System.IO.Ports;
using System.Text;

namespace Thetis;

public class SDRSerialPort6
{
	private SerialPort commPort;

	private bool isOpen;

	private bool bitBangOnly;

	private bool use_for_cat_ptt;

	private bool use_for_keyptt;

	private bool use_for_paddles;

	private bool ptt_on_dtr;

	private bool ptt_on_rts;

	private bool key_on_dtr;

	private bool key_on_rts;

	public SerialPort BasePort => commPort;

	public bool IsOpen => commPort.IsOpen;

	public bool UseForCATPTT
	{
		get
		{
			return use_for_cat_ptt;
		}
		set
		{
			use_for_cat_ptt = value;
		}
	}

	public bool UseForKeyPTT
	{
		get
		{
			return use_for_keyptt;
		}
		set
		{
			use_for_keyptt = value;
		}
	}

	public bool UseForPaddles
	{
		get
		{
			return use_for_paddles;
		}
		set
		{
			use_for_paddles = value;
		}
	}

	public bool PTTOnDTR
	{
		get
		{
			return ptt_on_dtr;
		}
		set
		{
			ptt_on_dtr = value;
		}
	}

	public bool PTTOnRTS
	{
		get
		{
			return ptt_on_rts;
		}
		set
		{
			ptt_on_rts = value;
		}
	}

	public bool KeyOnDTR
	{
		get
		{
			return key_on_dtr;
		}
		set
		{
			key_on_dtr = value;
		}
	}

	public bool KeyOnRTS
	{
		get
		{
			return key_on_rts;
		}
		set
		{
			key_on_rts = value;
		}
	}

	public static event SerialRXEventHandler serial_rx_event;

	public void Open()
	{
		commPort.Open();
	}

	public void Close()
	{
		commPort.Close();
	}

	public static Parity StringToParity(string s)
	{
		return s switch
		{
			"none" => Parity.None, 
			"odd" => Parity.Odd, 
			"even" => Parity.Even, 
			"space" => Parity.Space, 
			"mark" => Parity.Mark, 
			_ => Parity.None, 
		};
	}

	public static StopBits StringToStopBits(string s)
	{
		return s switch
		{
			"0" => StopBits.None, 
			"1" => StopBits.One, 
			"1.5" => StopBits.OnePointFive, 
			"2" => StopBits.Two, 
			_ => StopBits.One, 
		};
	}

	public SDRSerialPort6(int portidx)
	{
		commPort = new SerialPort();
		commPort.Encoding = Encoding.ASCII;
		commPort.RtsEnable = true;
		commPort.DtrEnable = true;
		commPort.DataReceived += SerialReceivedData;
		commPort.PinChanged += SerialPinChanged;
		commPort.PortName = "COM" + portidx;
		commPort.Handshake = Handshake.None;
		commPort.Parity = Parity.None;
		commPort.StopBits = StopBits.One;
		commPort.DataBits = 8;
		commPort.BaudRate = 9600;
		commPort.ReadTimeout = 5000;
		commPort.WriteTimeout = 500;
		commPort.ReceivedBytesThreshold = 1;
	}

	public void setCommParms(int baudrate, Parity p, int databits, StopBits stop)
	{
		if (!commPort.IsOpen)
		{
			commPort.BaudRate = baudrate;
			commPort.Parity = p;
			commPort.StopBits = stop;
			commPort.DataBits = databits;
		}
	}

	public uint put(string s)
	{
		if (bitBangOnly)
		{
			return 0u;
		}
		commPort.Write(s);
		return (uint)s.Length;
	}

	public int Create()
	{
		return Create(bit_bang_only: false);
	}

	public int Create(bool bit_bang_only)
	{
		bitBangOnly = bit_bang_only;
		if (isOpen)
		{
			return -1;
		}
		commPort.Open();
		isOpen = commPort.IsOpen;
		if (isOpen)
		{
			return 0;
		}
		return -1;
	}

	public void Destroy()
	{
		try
		{
			commPort.Close();
		}
		catch (Exception)
		{
		}
		isOpen = false;
	}

	public bool isCTS()
	{
		if (!isOpen)
		{
			return false;
		}
		return commPort.CtsHolding;
	}

	public bool isDSR()
	{
		if (!isOpen)
		{
			return false;
		}
		return commPort.DsrHolding;
	}

	public bool isRI()
	{
		_ = isOpen;
		return false;
	}

	public bool isRLSD()
	{
		if (!isOpen)
		{
			return false;
		}
		return commPort.CDHolding;
	}

	public void setDTR(bool v)
	{
		if (isOpen)
		{
			commPort.DtrEnable = v;
		}
	}

	private void SerialErrorReceived(object source, SerialErrorReceivedEventArgs e)
	{
	}

	private void SerialPinChanged(object source, SerialPinChangedEventArgs e)
	{
		if (!use_for_keyptt && !use_for_paddles && !use_for_cat_ptt)
		{
			return;
		}
		if (use_for_keyptt)
		{
			switch (e.EventType)
			{
			case SerialPinChange.DsrChanged:
				if (ptt_on_dtr)
				{
					CWInput.KeyerPTT = commPort.DsrHolding;
				}
				if (key_on_dtr)
				{
					NetworkIO.SetCWX(Convert.ToInt32(commPort.DsrHolding));
				}
				break;
			case SerialPinChange.CtsChanged:
				if (ptt_on_rts)
				{
					CWInput.KeyerPTT = commPort.CtsHolding;
				}
				if (key_on_rts)
				{
					NetworkIO.SetCWX(Convert.ToInt32(commPort.CtsHolding));
				}
				break;
			}
		}
		else if (use_for_paddles)
		{
			switch (e.EventType)
			{
			case SerialPinChange.DsrChanged:
				NetworkIO.SetCWDot(Convert.ToInt32(commPort.DsrHolding));
				break;
			case SerialPinChange.CtsChanged:
				NetworkIO.SetCWDash(Convert.ToInt32(commPort.CtsHolding));
				break;
			}
		}
		if (!use_for_cat_ptt)
		{
			return;
		}
		switch (e.EventType)
		{
		case SerialPinChange.DsrChanged:
			if (ptt_on_dtr)
			{
				CWInput.CATPTT = commPort.DsrHolding;
			}
			break;
		case SerialPinChange.CtsChanged:
			if (ptt_on_rts)
			{
				CWInput.CATPTT = commPort.CtsHolding;
			}
			break;
		}
	}

	private void SerialReceivedData(object source, SerialDataReceivedEventArgs e)
	{
		serial_rx_event(this, new SerialRXEvent(commPort.ReadExisting()));
	}
}
