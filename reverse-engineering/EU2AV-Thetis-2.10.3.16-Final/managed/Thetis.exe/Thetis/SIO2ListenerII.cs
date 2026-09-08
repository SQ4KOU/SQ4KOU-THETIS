using System;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Thetis;

public class SIO2ListenerII
{
	public SDRSerialPort2 SIO2;

	private Console console;

	private ASCIIEncoding AE = new ASCIIEncoding();

	private bool Fpass = true;

	private bool cat2_enabled;

	private CATParser parser;

	private StringBuilder CommBuffer = new StringBuilder();

	public bool UseForKeyPTT
	{
		set
		{
			if (SIO2 != null)
			{
				SIO2.UseForKeyPTT = value;
			}
		}
	}

	public bool UseForPaddles
	{
		set
		{
			if (SIO2 != null)
			{
				SIO2.UseForPaddles = value;
			}
		}
	}

	public bool PTTOnDTR
	{
		set
		{
			if (SIO2 != null)
			{
				SIO2.PTTOnDTR = value;
			}
		}
	}

	public bool PTTOnRTS
	{
		set
		{
			if (SIO2 != null)
			{
				SIO2.PTTOnRTS = value;
			}
		}
	}

	public bool KeyOnDTR
	{
		set
		{
			if (SIO2 != null)
			{
				SIO2.KeyOnDTR = value;
			}
		}
	}

	public bool KeyOnRTS
	{
		set
		{
			if (SIO2 != null)
			{
				SIO2.KeyOnRTS = value;
			}
		}
	}

	public SIO2ListenerII(Console c)
	{
		console = c;
		console.Closing += console_Closing;
		parser = new CATParser(console);
		SDRSerialPort2.serial_rx_event += SerialRX2EventHandler;
		if (!console.CAT2Enabled)
		{
			return;
		}
		try
		{
			enableCAT2();
		}
		catch (Exception ex)
		{
			console.CAT2Enabled = false;
			if (!console.IsSetupFormNull)
			{
				console.SetupForm.copyCAT2PropsToDialogVars();
			}
			MessageBox.Show("Could not initialize CAT control.  Exception was:\n\n " + ex.Message + "\n\nCAT control has been disabled.", "Error Initializing CAT control", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public void enableCAT2()
	{
		if (!cat2_enabled)
		{
			cat2_enabled = true;
			int cAT2Port = console.CAT2Port;
			SIO2 = new SDRSerialPort2(cAT2Port);
			SIO2.setCommParms(console.CAT2BaudRate, console.CAT2Parity, console.CAT2DataBits, console.CAT2StopBits);
			Initialize();
		}
	}

	public void disableCAT2()
	{
		if (cat2_enabled)
		{
			cat2_enabled = false;
			if (SIO2 != null)
			{
				SIO2.Destroy();
				SIO2 = null;
			}
			Fpass = true;
		}
	}

	private void Initialize()
	{
		if (Fpass)
		{
			SIO2.Create();
			Fpass = false;
		}
	}

	private void console_Closing(object sender, CancelEventArgs e)
	{
		if (SIO2 != null)
		{
			SIO2.Destroy();
		}
	}

	private void console_Activated(object sender, EventArgs e)
	{
		if (console.CAT2Enabled)
		{
			enableCAT2();
		}
	}

	private void SerialRX2EventHandler(object sender, SerialRXEvent e)
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
					SIO2.put(text);
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
