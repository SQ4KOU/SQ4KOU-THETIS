using System;
using System.IO.Ports;

namespace Thetis;

internal class CWInput
{
	private static SDRSerialPort primary_com_port;

	private static SDRSerialPort secondary_com_port;

	private static string primary_input = "Radio";

	private static string secondary_input = "None";

	private static KeyerLine secondary_ptt_line = KeyerLine.None;

	private static KeyerLine secondary_key_line = KeyerLine.None;

	private static bool keyerptt = false;

	private static bool cat_ptt = false;

	public static string PrimaryInput => primary_input;

	public static string SecondaryInput => secondary_input;

	public static KeyerLine SecondaryPTTLine
	{
		get
		{
			return secondary_ptt_line;
		}
		set
		{
			secondary_ptt_line = value;
			if (secondary_input.ToUpper().StartsWith("COM") && secondary_com_port != null)
			{
				switch (value)
				{
				case KeyerLine.DTR:
					secondary_com_port.PTTOnRTS = false;
					secondary_com_port.PTTOnDTR = true;
					break;
				case KeyerLine.RTS:
					secondary_com_port.PTTOnDTR = false;
					secondary_com_port.PTTOnRTS = true;
					break;
				case KeyerLine.None:
					secondary_com_port.PTTOnRTS = false;
					secondary_com_port.PTTOnDTR = false;
					break;
				}
			}
		}
	}

	public static KeyerLine SecondaryKeyLine
	{
		get
		{
			return secondary_key_line;
		}
		set
		{
			secondary_key_line = value;
			if (secondary_input.ToUpper().StartsWith("COM") && secondary_com_port != null)
			{
				switch (value)
				{
				case KeyerLine.DTR:
					secondary_com_port.KeyOnRTS = false;
					secondary_com_port.KeyOnDTR = true;
					break;
				case KeyerLine.RTS:
					secondary_com_port.KeyOnDTR = false;
					secondary_com_port.KeyOnRTS = true;
					break;
				case KeyerLine.None:
					secondary_com_port.KeyOnRTS = false;
					secondary_com_port.KeyOnDTR = false;
					break;
				}
			}
		}
	}

	public static bool KeyerPTT
	{
		get
		{
			return keyerptt;
		}
		set
		{
			keyerptt = value;
		}
	}

	public static bool CATPTT
	{
		get
		{
			return cat_ptt;
		}
		set
		{
			cat_ptt = value;
		}
	}

	public static event SerialRXEventHandler serial_rx_event;

	public static bool SetPrimaryInput(string s)
	{
		if (s.ToUpper().StartsWith("COM") && s.Length > 3)
		{
			int result = 0;
			if (!int.TryParse(s.Substring(3, s.Length - 3), out result))
			{
				return false;
			}
			if (primary_com_port != null)
			{
				if (primary_com_port.IsOpen)
				{
					primary_com_port.Close();
				}
				primary_com_port = null;
			}
			primary_com_port = new SDRSerialPort(result);
			try
			{
				primary_com_port.Open();
			}
			catch (Exception)
			{
				primary_com_port = null;
				return false;
			}
			if (!primary_com_port.IsOpen)
			{
				primary_com_port = null;
				return false;
			}
			primary_com_port.UseForPaddles = true;
			primary_input = s;
			return true;
		}
		switch (s)
		{
		case "Radio":
		case "CAT":
		case "None":
			if (primary_com_port != null)
			{
				if (primary_com_port.IsOpen)
				{
					primary_com_port.Close();
				}
				primary_com_port = null;
			}
			primary_input = s;
			break;
		}
		return true;
	}

	public static bool SetSecondaryInput(string s)
	{
		if (s.ToUpper().StartsWith("COM") && s.Length > 3)
		{
			int result = 0;
			if (!int.TryParse(s.Substring(3, s.Length - 3), out result))
			{
				return false;
			}
			if (secondary_com_port != null)
			{
				if (secondary_com_port.IsOpen)
				{
					secondary_com_port.Close();
				}
				secondary_com_port = null;
			}
			secondary_com_port = new SDRSerialPort(result);
			try
			{
				secondary_com_port.Open();
			}
			catch (Exception)
			{
			}
			if (!secondary_com_port.IsOpen)
			{
				secondary_com_port = null;
				return false;
			}
			secondary_com_port.UseForKeyPTT = true;
			SecondaryKeyLine = secondary_key_line;
			SecondaryPTTLine = secondary_ptt_line;
			secondary_input = s;
			return true;
		}
		switch (s)
		{
		case "Radio":
		case "CAT":
		case "None":
			if (secondary_com_port != null)
			{
				if (secondary_com_port.IsOpen)
				{
					secondary_com_port.Close();
				}
				secondary_com_port = null;
			}
			primary_input = s;
			break;
		}
		return true;
	}

	private void SerialReceivedData(object source, SerialDataReceivedEventArgs e)
	{
		serial_rx_event(this, new SerialRXEvent(primary_com_port.BasePort.ReadExisting()));
	}
}
