using System.IO.Ports;

namespace Thetis;

public class SerialPortPTT
{
	private int portNum;

	private bool rtsIsPTT;

	private bool dtrIsPTT;

	private SDRSerialPort commPort;

	private bool Initialized;

	public bool RTSIsPTT
	{
		get
		{
			return rtsIsPTT;
		}
		set
		{
			rtsIsPTT = value;
		}
	}

	public bool DTRIsPTT
	{
		get
		{
			return dtrIsPTT;
		}
		set
		{
			dtrIsPTT = value;
		}
	}

	public static event SerialRXEventHandler serial_rx_event;

	public SerialPortPTT(int portidx, bool rts_is_ptt, bool dtr_is_ptt)
	{
		portNum = portidx;
		rtsIsPTT = rts_is_ptt;
		dtrIsPTT = dtr_is_ptt;
	}

	public void Init()
	{
		lock (this)
		{
			if (Initialized || portNum == 0)
			{
				return;
			}
			try
			{
				commPort = new SDRSerialPort(portNum);
				commPort.Create(bit_bang_only: true);
				Initialized = true;
			}
			catch
			{
			}
		}
	}

	public bool isPTT()
	{
		try
		{
			if (!Initialized)
			{
				return false;
			}
			if (rtsIsPTT && commPort.isCTS())
			{
				return true;
			}
			if (dtrIsPTT && commPort.isDSR())
			{
				return true;
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	public bool isCTS()
	{
		try
		{
			return commPort.isCTS();
		}
		catch
		{
			return false;
		}
	}

	public bool isDSR()
	{
		try
		{
			return commPort.isDSR();
		}
		catch
		{
			return false;
		}
	}

	public void setDTR(bool v)
	{
		try
		{
			commPort.setDTR(v);
		}
		catch
		{
		}
	}

	public void Destroy()
	{
		lock (this)
		{
			if (!Initialized)
			{
				return;
			}
			Initialized = false;
		}
		try
		{
			if (commPort != null)
			{
				commPort.Destroy();
				commPort = null;
			}
		}
		catch
		{
		}
	}

	private void SerialReceivedData(object source, SerialDataReceivedEventArgs e)
	{
		serial_rx_event(this, new SerialRXEvent(commPort.BasePort.ReadExisting()));
	}
}
