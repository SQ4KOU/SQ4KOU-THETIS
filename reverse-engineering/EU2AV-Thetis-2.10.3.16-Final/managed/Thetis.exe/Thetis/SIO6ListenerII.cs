using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Thetis;

public class SIO6ListenerII
{
	public SDRSerialPort6 SIO6;

	private Console console;

	private ASCIIEncoding AE = new ASCIIEncoding();

	private bool Fpass = true;

	private bool cat6_enabled;

	private CATParser parser;

	private StringBuilder CommBuffer = new StringBuilder();

	public SIO6ListenerII(Console c)
	{
		console = c;
		console.Closing += console_Closing;
		parser = new CATParser(console);
		SDRSerialPort6.serial_rx_event += SerialRX6EventHandler;
		if (!console.AriesCATEnabled)
		{
			return;
		}
		try
		{
			enableCAT6();
		}
		catch (Exception ex)
		{
			console.AriesCATEnabled = false;
			if (!console.IsSetupFormNull)
			{
				console.SetupForm.copyAriesCATPropsToDialogVars();
			}
			MessageBox.Show("Could not initialize Aries CAT control.  Exception was:\n\n " + ex.Message + "\n\nCAT control has been disabled.", "Error Initializing CAT control", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public void enableCAT6()
	{
		if (!cat6_enabled)
		{
			cat6_enabled = true;
			int ariesCATPort = console.AriesCATPort;
			SIO6 = new SDRSerialPort6(ariesCATPort);
			SIO6.setCommParms(9600, Parity.None, 8, StopBits.One);
			Initialize();
		}
	}

	public void disableCAT6()
	{
		if (cat6_enabled)
		{
			cat6_enabled = false;
			if (SIO6 != null)
			{
				SIO6.Destroy();
				SIO6 = null;
			}
			Fpass = true;
		}
	}

	private void Initialize()
	{
		if (Fpass)
		{
			SIO6.Create();
			Fpass = false;
		}
	}

	private void console_Closing(object sender, CancelEventArgs e)
	{
		if (SIO6 != null)
		{
			SIO6.Destroy();
		}
	}

	private void console_Activated(object sender, EventArgs e)
	{
		if (console.AriesCATEnabled)
		{
			enableCAT6();
		}
	}

	private void SerialRX6EventHandler(object source, SerialRXEvent e)
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
					SIO6.put(text);
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
