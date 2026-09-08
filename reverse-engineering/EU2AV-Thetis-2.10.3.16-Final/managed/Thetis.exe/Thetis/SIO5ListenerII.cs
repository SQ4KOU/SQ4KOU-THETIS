using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Thetis;

public class SIO5ListenerII
{
	public SDRSerialPort5 SIO5;

	private Console console;

	private ASCIIEncoding AE = new ASCIIEncoding();

	private bool Fpass = true;

	private bool cat5_enabled;

	private CATParser parser;

	private StringBuilder CommBuffer = new StringBuilder();

	public SIO5ListenerII(Console c)
	{
		console = c;
		console.Closing += console_Closing;
		parser = new CATParser(console);
		SDRSerialPort5.serial_rx_event += SerialRX5EventHandler;
		if (!console.AndromedaCATEnabled)
		{
			return;
		}
		try
		{
			enableCAT5();
		}
		catch (Exception ex)
		{
			console.AndromedaCATEnabled = false;
			if (!console.IsSetupFormNull)
			{
				console.SetupForm.copyAndromedaCATPropsToDialogVars();
			}
			MessageBox.Show("Could not initialize Andromeda CAT control.  Exception was:\n\n " + ex.Message + "\n\nCAT control has been disabled.", "Error Initializing CAT control", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public void enableCAT5()
	{
		if (!cat5_enabled)
		{
			cat5_enabled = true;
			int andromedaCATPort = console.AndromedaCATPort;
			SIO5 = new SDRSerialPort5(andromedaCATPort);
			SIO5.setCommParms(9600, Parity.None, 8, StopBits.One);
			Initialize();
		}
	}

	public void disableCAT5()
	{
		if (cat5_enabled)
		{
			cat5_enabled = false;
			if (SIO5 != null)
			{
				SIO5.Destroy();
				SIO5 = null;
			}
			Fpass = true;
		}
	}

	private void Initialize()
	{
		if (Fpass)
		{
			SIO5.Create();
			Fpass = false;
		}
	}

	private void console_Closing(object sender, CancelEventArgs e)
	{
		if (SIO5 != null)
		{
			SIO5.Destroy();
		}
	}

	private void console_Activated(object sender, EventArgs e)
	{
		if (console.AndromedaCATEnabled)
		{
			enableCAT5();
		}
	}

	private void SerialRX5EventHandler(object source, SerialRXEvent e)
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
					SIO5.put(text);
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
