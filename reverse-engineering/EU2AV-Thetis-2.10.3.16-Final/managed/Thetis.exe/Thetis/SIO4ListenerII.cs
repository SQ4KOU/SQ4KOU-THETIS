using System;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Thetis;

public class SIO4ListenerII
{
	public SDRSerialPort4 SIO4;

	private Console console;

	private ASCIIEncoding AE = new ASCIIEncoding();

	private bool Fpass = true;

	private bool cat4_enabled;

	private CATParser parser;

	private StringBuilder CommBuffer = new StringBuilder();

	public bool UseForKeyPTT
	{
		set
		{
			if (SIO4 != null)
			{
				SIO4.UseForKeyPTT = value;
			}
		}
	}

	public bool UseForPaddles
	{
		set
		{
			if (SIO4 != null)
			{
				SIO4.UseForPaddles = value;
			}
		}
	}

	public bool PTTOnDTR
	{
		set
		{
			if (SIO4 != null)
			{
				SIO4.PTTOnDTR = value;
			}
		}
	}

	public bool PTTOnRTS
	{
		set
		{
			if (SIO4 != null)
			{
				SIO4.PTTOnRTS = value;
			}
		}
	}

	public bool KeyOnDTR
	{
		set
		{
			if (SIO4 != null)
			{
				SIO4.KeyOnDTR = value;
			}
		}
	}

	public bool KeyOnRTS
	{
		set
		{
			if (SIO4 != null)
			{
				SIO4.KeyOnRTS = value;
			}
		}
	}

	public SIO4ListenerII(Console c)
	{
		console = c;
		console.Closing += console_Closing;
		parser = new CATParser(console);
		SDRSerialPort4.serial_rx_event += SerialRX4EventHandler;
		if (!console.CAT4Enabled)
		{
			return;
		}
		try
		{
			enableCAT4();
		}
		catch (Exception ex)
		{
			console.CAT4Enabled = false;
			if (!console.IsSetupFormNull)
			{
				console.SetupForm.copyCAT4PropsToDialogVars();
			}
			MessageBox.Show("Could not initialize CAT control.  Exception was:\n\n " + ex.Message + "\n\nCAT control has been disabled.", "Error Initializing CAT control", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public void enableCAT4()
	{
		if (!cat4_enabled)
		{
			cat4_enabled = true;
			int cAT4Port = console.CAT4Port;
			SIO4 = new SDRSerialPort4(cAT4Port);
			SIO4.setCommParms(console.CAT4BaudRate, console.CAT4Parity, console.CAT4DataBits, console.CAT4StopBits);
			Initialize();
		}
	}

	public void disableCAT4()
	{
		if (cat4_enabled)
		{
			cat4_enabled = false;
			if (SIO4 != null)
			{
				SIO4.Destroy();
				SIO4 = null;
			}
			Fpass = true;
		}
	}

	private void Initialize()
	{
		if (Fpass)
		{
			SIO4.Create();
			Fpass = false;
		}
	}

	private void console_Closing(object sender, CancelEventArgs e)
	{
		if (SIO4 != null)
		{
			SIO4.Destroy();
		}
	}

	private void console_Activated(object sender, EventArgs e)
	{
		if (console.CAT4Enabled)
		{
			enableCAT4();
		}
	}

	private void SerialRX4EventHandler(object source, SerialRXEvent e)
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
					SIO4.put(text);
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
