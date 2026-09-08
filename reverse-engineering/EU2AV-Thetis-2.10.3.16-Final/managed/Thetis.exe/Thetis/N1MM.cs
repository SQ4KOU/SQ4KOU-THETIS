using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Thetis;

public static class N1MM
{
	private struct ReceiverStoredData
	{
		public float[] spectrum_data;

		public double LowFreq;

		public double HighFreq;

		public bool Enabled;

		public int Width;

		public float Scale;

		public bool DataReady;

		public string ID;
	}

	private const double KEEP_TIME = 0.1;

	private static ReceiverStoredData[] RXstoredData;

	private static int m_nDestinationPort;

	private static string m_sDestinationIP;

	private static int m_nSendRate;

	private static bool m_bStarted;

	private static Task udp_send_task;

	private static CancellationTokenSource cancelTokenSource;

	private static readonly object m_objLock;

	public static int SendRate
	{
		get
		{
			return m_nSendRate;
		}
		set
		{
			m_nSendRate = value;
		}
	}

	public static bool IsStarted => m_bStarted;

	public static string DestinationIP
	{
		get
		{
			return m_sDestinationIP;
		}
		set
		{
			string[] array = value.Split(':');
			string ipString = "";
			if (array.Length == 1)
			{
				ipString = value;
			}
			else if (array.Length > 1)
			{
				ipString = array[0];
				if (int.TryParse(array[1], out var result))
				{
					if (result < 0 || result > 65535)
					{
						return;
					}
					DestinationPort = result;
				}
			}
			if (IPAddress.TryParse(ipString, out var address))
			{
				m_sDestinationIP = address.ToString();
			}
		}
	}

	public static int DestinationPort
	{
		get
		{
			return m_nDestinationPort;
		}
		set
		{
			m_nDestinationPort = value;
		}
	}

	static N1MM()
	{
		m_nDestinationPort = 13064;
		m_sDestinationIP = "255.255.255.255";
		m_nSendRate = 8;
		m_bStarted = false;
		m_objLock = new object();
		setMaxRXs(2);
	}

	public static string GetID(int rx)
	{
		if (RXstoredData == null || rx > RXstoredData.Length || rx < 1)
		{
			return string.Empty;
		}
		return RXstoredData[rx - 1].ID;
	}

	public static void SetID(int rx, string id)
	{
		if (RXstoredData != null && rx <= RXstoredData.Length && rx >= 1 && !string.IsNullOrEmpty(id))
		{
			RXstoredData[rx - 1].ID = id;
		}
	}

	public static bool IsEnabled(int rx)
	{
		if (RXstoredData == null || rx > RXstoredData.Length || rx < 1)
		{
			return false;
		}
		return RXstoredData[rx - 1].Enabled;
	}

	public static void SetEnabled(int rx, bool enable)
	{
		if (RXstoredData != null && rx <= RXstoredData.Length && rx >= 1)
		{
			RXstoredData[rx - 1].Enabled = enable;
			Resize(rx);
		}
	}

	private static void setLowFrequencyMHz(int rx, double freq)
	{
		if (RXstoredData != null && rx <= RXstoredData.Length && rx >= 1)
		{
			RXstoredData[rx - 1].LowFreq = freq;
		}
	}

	private static void setHighFrequencyMHz(int rx, double freq)
	{
		if (RXstoredData != null && rx <= RXstoredData.Length && rx >= 1)
		{
			RXstoredData[rx - 1].HighFreq = freq;
		}
	}

	public static void Stop()
	{
		if (cancelTokenSource != null)
		{
			cancelTokenSource.Cancel();
			cancelTokenSource.Dispose();
			cancelTokenSource = null;
		}
		m_bStarted = false;
	}

	private static void setMaxRXs(int rxNumber)
	{
		RXstoredData = new ReceiverStoredData[rxNumber];
		for (int i = 0; i < RXstoredData.Length; i++)
		{
			RXstoredData[i].ID = string.Empty;
			RXstoredData[i].DataReady = false;
			RXstoredData[i].Enabled = false;
		}
	}

	public static void Resize()
	{
		if (!Display.IsDX2DSetup)
		{
			return;
		}
		for (int i = 0; i < RXstoredData.Length; i++)
		{
			if (RXstoredData[i].Enabled)
			{
				Resize(i + 1);
			}
		}
	}

	public static void Resize(int rx)
	{
		if (!Display.IsDX2DSetup || rx < 1 || rx > 2)
		{
			return;
		}
		int num = Display.Target.Width / Display.Decimation;
		if (RXstoredData[rx - 1].spectrum_data == null || RXstoredData[rx - 1].Width < num)
		{
			RXstoredData[rx - 1].spectrum_data = new float[num];
		}
		RXstoredData[rx - 1].Width = num;
		if (!IsEnabled(rx))
		{
			return;
		}
		double num2 = ((rx == 1) ? (Display.RXDisplayHigh - Display.RXDisplayLow) : (Display.RX2DisplayHigh - Display.RX2DisplayLow));
		double num3 = ((rx == 1) ? Display.CentreFreqRX1 : Display.CentreFreqRX2);
		double num4 = Math.Round(num3 - num2 / 2.0 * 1E-06, 6);
		double num5 = Math.Round(num3 + num2 / 2.0 * 1E-06, 6);
		int num6 = 0;
		switch (rx)
		{
		case 1:
			if (Display.RX1DSPMode == DSPMode.CWL)
			{
				num6 = Display.CWPitch;
			}
			else if (Display.RX1DSPMode == DSPMode.CWU)
			{
				num6 = -Display.CWPitch;
			}
			break;
		case 2:
			if (Display.RX2DSPMode == DSPMode.CWL)
			{
				num6 = Display.CWPitch;
			}
			else if (Display.RX2DSPMode == DSPMode.CWU)
			{
				num6 = -Display.CWPitch;
			}
			break;
		}
		num4 += (double)num6 * 1E-06;
		num5 += (double)num6 * 1E-06;
		setLowFrequencyMHz(rx, num4);
		setHighFrequencyMHz(rx, num5);
	}

	public unsafe static void CopyData(int rx, float[] newData)
	{
		if (!m_bStarted || RXstoredData == null || rx > RXstoredData.Length || rx < 1 || !RXstoredData[rx - 1].Enabled || RXstoredData[rx - 1].DataReady)
		{
			return;
		}
		lock (m_objLock)
		{
			fixed (float* ptr = &newData[0])
			{
				void* srcptr = ptr;
				fixed (float* ptr2 = &RXstoredData[rx - 1].spectrum_data[0])
				{
					void* destptr = ptr2;
					Win32.memcpy(destptr, srcptr, RXstoredData[rx - 1].Width * 4);
				}
			}
			RXstoredData[rx - 1].DataReady = true;
		}
	}

	public static void Start()
	{
		if (cancelTokenSource != null)
		{
			return;
		}
		cancelTokenSource = new CancellationTokenSource();
		m_bStarted = true;
		udp_send_task = Task.Factory.StartNew(delegate
		{
			while (cancelTokenSource != null && !cancelTokenSource.IsCancellationRequested)
			{
				lock (m_objLock)
				{
					try
					{
						sendUDPData();
					}
					catch
					{
					}
				}
				Thread.Sleep(1000 / m_nSendRate);
			}
		}, cancelTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
	}

	public static void SetScale(int rx, float fScale)
	{
		if (RXstoredData != null && rx <= RXstoredData.Length && rx >= 1)
		{
			RXstoredData[rx - 1].Scale = fScale;
		}
	}

	private static void sendUDPData()
	{
		if (RXstoredData == null)
		{
			return;
		}
		for (int i = 0; i < RXstoredData.Length; i++)
		{
			ReceiverStoredData receiverStoredData = RXstoredData[i];
			if (receiverStoredData.spectrum_data == null || !receiverStoredData.Enabled || !receiverStoredData.DataReady)
			{
				continue;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.PreserveWhitespace = false;
			XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", null, null);
			xmlDeclaration.Encoding = "UTF-8";
			xmlDeclaration.Standalone = "yes";
			xmlDocument.AppendChild(xmlDeclaration);
			XmlElement xmlElement = xmlDocument.CreateElement("Spectrum");
			xmlDocument.AppendChild(xmlElement);
			XmlElement xmlElement2 = xmlDocument.CreateElement("app");
			xmlElement2.InnerText = "WaterfallBandmap";
			XmlElement xmlElement3 = xmlDocument.CreateElement("Name");
			if (string.IsNullOrEmpty(receiverStoredData.ID))
			{
				xmlElement3.InnerText = "Thetis_" + (i + 1);
			}
			else
			{
				xmlElement3.InnerText = receiverStoredData.ID;
			}
			XmlElement xmlElement4 = xmlDocument.CreateElement("LowScopeFrequency");
			xmlElement4.InnerText = (receiverStoredData.LowFreq * 1000.0).ToString("0.000");
			XmlElement xmlElement5 = xmlDocument.CreateElement("HighScopeFrequency");
			xmlElement5.InnerText = (receiverStoredData.HighFreq * 1000.0).ToString("0.000");
			XmlElement xmlElement6 = xmlDocument.CreateElement("ScalingFactor");
			xmlElement6.InnerText = receiverStoredData.Scale.ToString("0.00");
			XmlElement xmlElement7 = xmlDocument.CreateElement("DataCount");
			xmlElement7.InnerText = receiverStoredData.Width.ToString();
			XmlElement xmlElement8 = xmlDocument.CreateElement("SpectrumData");
			string text = "";
			double num = double.MaxValue;
			for (int j = 0; j < receiverStoredData.Width; j++)
			{
				if ((double)receiverStoredData.spectrum_data[j] < num)
				{
					num = receiverStoredData.spectrum_data[j];
				}
			}
			num = Math.Abs(num);
			for (int k = 0; k < receiverStoredData.Width; k++)
			{
				text = text + ((int)receiverStoredData.spectrum_data[k] + (int)num) + ",";
			}
			if (text != "")
			{
				text = text.Substring(0, text.Length - 1);
			}
			xmlElement8.InnerText = text;
			xmlElement.AppendChild(xmlElement2);
			xmlElement.AppendChild(xmlElement3);
			xmlElement.AppendChild(xmlElement4);
			xmlElement.AppendChild(xmlElement5);
			xmlElement.AppendChild(xmlElement6);
			xmlElement.AppendChild(xmlElement7);
			xmlElement.AppendChild(xmlElement8);
			byte[] bytes = Encoding.UTF8.GetBytes(xmlDocument.OuterXml);
			using (UdpClient udpClient = new UdpClient())
			{
				udpClient.Send(bytes, bytes.Length, m_sDestinationIP, m_nDestinationPort);
				udpClient.Close();
			}
			RXstoredData[i].DataReady = false;
		}
	}
}
