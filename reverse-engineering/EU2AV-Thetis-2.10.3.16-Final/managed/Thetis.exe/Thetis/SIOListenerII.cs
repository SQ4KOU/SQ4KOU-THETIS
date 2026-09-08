using System;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Thetis;

public class SIOListenerII
{
	public SDRSerialPort SIO;

	private Console console;

	private ASCIIEncoding AE = new ASCIIEncoding();

	private bool Fpass = true;

	private bool cat_enabled;

	private CATParser parser;

	private StringBuilder CommBuffer = new StringBuilder();

	public bool UseForCATPTT
	{
		set
		{
			if (SIO != null)
			{
				SIO.UseForCATPTT = value;
			}
		}
	}

	public bool UseForKeyPTT
	{
		set
		{
			if (SIO != null)
			{
				SIO.UseForKeyPTT = value;
			}
		}
	}

	public bool UseForPaddles
	{
		set
		{
			if (SIO != null)
			{
				SIO.UseForPaddles = value;
			}
		}
	}

	public bool PTTOnDTR
	{
		set
		{
			if (SIO != null)
			{
				SIO.PTTOnDTR = value;
			}
		}
	}

	public bool PTTOnRTS
	{
		set
		{
			if (SIO != null)
			{
				SIO.PTTOnRTS = value;
			}
		}
	}

	public bool KeyOnDTR
	{
		set
		{
			if (SIO != null)
			{
				SIO.KeyOnDTR = value;
			}
		}
	}

	public bool KeyOnRTS
	{
		set
		{
			if (SIO != null)
			{
				SIO.KeyOnRTS = value;
			}
		}
	}

	public SIOListenerII(Console c)
	{
		console = c;
		console.Closing += console_Closing;
		parser = new CATParser(console);
		SDRSerialPort.serial_rx_event += SerialRXEventHandler;
		if (!console.CATEnabled)
		{
			return;
		}
		try
		{
			enableCAT();
		}
		catch (Exception ex)
		{
			console.CATEnabled = false;
			if (!console.IsSetupFormNull)
			{
				console.SetupForm.copyCATPropsToDialogVars();
			}
			MessageBox.Show("Could not initialize CAT control.  Exception was:\n\n " + ex.Message + "\n\nCAT control has been disabled.", "Error Initializing CAT control", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public void enableCAT()
	{
		if (!cat_enabled)
		{
			cat_enabled = true;
			int cATPort = console.CATPort;
			SIO = new SDRSerialPort(cATPort);
			SIO.setCommParms(console.CATBaudRate, console.CATParity, console.CATDataBits, console.CATStopBits);
			Initialize();
		}
	}

	public void disableCAT()
	{
		if (cat_enabled)
		{
			cat_enabled = false;
			if (SIO != null)
			{
				SIO.Destroy();
				SIO = null;
			}
			Fpass = true;
		}
	}

	private void Initialize()
	{
		if (Fpass)
		{
			SIO.Create();
			Fpass = false;
		}
	}

	private void console_Closing(object sender, CancelEventArgs e)
	{
		if (SIO != null)
		{
			SIO.Destroy();
		}
	}

	private void console_Activated(object sender, EventArgs e)
	{
		if (console.CATEnabled)
		{
			enableCAT();
		}
	}

	private void SerialRXEventHandler(object source, SerialRXEvent e)
	{
		CommBuffer.Append(e.buffer);
		if (parser == null)
		{
			return;
		}
		try
		{
			Match match = new Regex(".*?;").Match(CommBuffer.ToString());
			while (match.Success)
			{
				string text = parser.Get(match.Value);
				if (text.Length > 0)
				{
					SIO.put(text);
				}
				CommBuffer = CommBuffer.Replace(match.Value, "", 0, match.Length);
				match = match.NextMatch();
			}
		}
		catch (Exception)
		{
		}
	}
}
