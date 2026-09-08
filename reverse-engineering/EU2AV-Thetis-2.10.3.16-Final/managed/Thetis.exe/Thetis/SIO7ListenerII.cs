using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Thetis;

public class SIO7ListenerII
{
	public SDRSerialPort7 SIO7;

	private Console console;

	private ASCIIEncoding AE = new ASCIIEncoding();

	private bool Fpass = true;

	private bool cat7_enabled;

	private CATParser parser;

	private StringBuilder CommBuffer = new StringBuilder();

	public SIO7ListenerII(Console c)
	{
		console = c;
		console.Closing += console_Closing;
		parser = new CATParser(console);
		SDRSerialPort7.serial_rx_event += SerialRX7EventHandler;
		if (!console.GanymedeCATEnabled)
		{
			return;
		}
		try
		{
			enableCAT7();
		}
		catch (Exception ex)
		{
			console.GanymedeCATEnabled = false;
			if (!console.IsSetupFormNull)
			{
				console.SetupForm.copyGanymedeCATPropsToDialogVars();
			}
			MessageBox.Show("Could not initialize Ganymede CAT control.  Exception was:\n\n " + ex.Message + "\n\nCAT control has been disabled.", "Error Initializing CAT control", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public void enableCAT7()
	{
		if (!cat7_enabled)
		{
			cat7_enabled = true;
			int ganymedeCATPort = console.GanymedeCATPort;
			SIO7 = new SDRSerialPort7(ganymedeCATPort);
			SIO7.setCommParms(9600, Parity.None, 8, StopBits.One);
			Initialize();
		}
	}

	public void disableCAT7()
	{
		if (cat7_enabled)
		{
			cat7_enabled = false;
			if (SIO7 != null)
			{
				SIO7.Destroy();
				SIO7 = null;
			}
			Fpass = true;
		}
	}

	private void Initialize()
	{
		if (Fpass)
		{
			SIO7.Create();
			Fpass = false;
		}
	}

	private void console_Closing(object sender, CancelEventArgs e)
	{
		if (SIO7 != null)
		{
			SIO7.Destroy();
		}
	}

	private void console_Activated(object sender, EventArgs e)
	{
		if (console.GanymedeCATEnabled)
		{
			enableCAT7();
		}
	}

	private void SerialRX7EventHandler(object source, SerialRXEvent e)
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
					SIO7.put(text);
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
