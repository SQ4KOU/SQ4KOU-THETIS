using System;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Thetis;

public class SIO3ListenerII
{
	public SDRSerialPort3 SIO3;

	private Console console;

	private ASCIIEncoding AE = new ASCIIEncoding();

	private bool Fpass = true;

	private bool cat3_enabled;

	private CATParser parser;

	private StringBuilder CommBuffer = new StringBuilder();

	public bool UseForKeyPTT
	{
		set
		{
			if (SIO3 != null)
			{
				SIO3.UseForKeyPTT = value;
			}
		}
	}

	public bool UseForPaddles
	{
		set
		{
			if (SIO3 != null)
			{
				SIO3.UseForPaddles = value;
			}
		}
	}

	public bool PTTOnDTR
	{
		set
		{
			if (SIO3 != null)
			{
				SIO3.PTTOnDTR = value;
			}
		}
	}

	public bool PTTOnRTS
	{
		set
		{
			if (SIO3 != null)
			{
				SIO3.PTTOnRTS = value;
			}
		}
	}

	public bool KeyOnDTR
	{
		set
		{
			if (SIO3 != null)
			{
				SIO3.KeyOnDTR = value;
			}
		}
	}

	public bool KeyOnRTS
	{
		set
		{
			if (SIO3 != null)
			{
				SIO3.KeyOnRTS = value;
			}
		}
	}

	public SIO3ListenerII(Console c)
	{
		console = c;
		console.Closing += console_Closing;
		parser = new CATParser(console);
		SDRSerialPort3.serial_rx_event += SerialRX3EventHandler;
		if (!console.CAT3Enabled)
		{
			return;
		}
		try
		{
			enableCAT3();
		}
		catch (Exception ex)
		{
			console.CAT3Enabled = false;
			if (!console.IsSetupFormNull)
			{
				console.SetupForm.copyCAT3PropsToDialogVars();
			}
			MessageBox.Show("Could not initialize CAT control.  Exception was:\n\n " + ex.Message + "\n\nCAT control has been disabled.", "Error Initializing CAT control", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public void enableCAT3()
	{
		if (!cat3_enabled)
		{
			cat3_enabled = true;
			int cAT3Port = console.CAT3Port;
			SIO3 = new SDRSerialPort3(cAT3Port);
			SIO3.setCommParms(console.CAT3BaudRate, console.CAT3Parity, console.CAT3DataBits, console.CAT3StopBits);
			Initialize();
		}
	}

	public void disableCAT3()
	{
		if (cat3_enabled)
		{
			cat3_enabled = false;
			if (SIO3 != null)
			{
				SIO3.Destroy();
				SIO3 = null;
			}
			Fpass = true;
		}
	}

	private void Initialize()
	{
		if (Fpass)
		{
			SIO3.Create();
			Fpass = false;
		}
	}

	private void console_Closing(object sender, CancelEventArgs e)
	{
		if (SIO3 != null)
		{
			SIO3.Destroy();
		}
	}

	private void console_Activated(object sender, EventArgs e)
	{
		if (console.CAT3Enabled)
		{
			enableCAT3();
		}
	}

	private void SerialRX3EventHandler(object source, SerialRXEvent e)
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
					SIO3.put(text);
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
