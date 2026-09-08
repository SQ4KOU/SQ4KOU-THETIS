using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thetis;

public static class Common
{
	private class HighlightData
	{
		public Color BackgroundColour { get; set; }

		public Color ForegroundColour { get; set; }

		public FlatStyle FlatStyle { get; set; }

		public Image BackgroundImage { get; set; }
	}

	private struct RECT
	{
		public int left;

		public int top;

		public int right;

		public int bottom;
	}

	private enum MonitorDpiType
	{
		MDT_EFFECTIVE_DPI,
		MDT_ANGULAR_DPI,
		MDT_RAW_DPI
	}

	[Flags]
	public enum ExecutionState : uint
	{
		ES_CONTINUOUS = 0x80000000u,
		ES_SYSTEM_REQUIRED = 1u,
		ES_DISPLAY_REQUIRED = 2u
	}

	public const MessageBoxOptions MB_TOPMOST = (MessageBoxOptions)262144;

	private static Dictionary<string, HighlightData> _hightlightData = new Dictionary<string, HighlightData>();

	private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

	private static string m_sLogPath = "";

	private static string m_sVersionNumber = "";

	private static string m_sFileVersion = "";

	private static string m_sRevision = "";

	private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;

	private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

	private const uint MONITOR_DEFAULTTONEAREST = 2u;

	private static HiPerfTimer _timer = null;

	private static TimeSpan _previousCpuTime;

	private static double _previousElapsedSeconds;

	private static ExecutionState _previous_sleep_state;

	private static ExecutionState _previous_display_state;

	private static bool _sleep_prevented = false;

	private static bool _display_prevented = false;

	public static bool ShiftKeyDown
	{
		get
		{
			if (!Keyboard.IsKeyDown(Keys.LShiftKey))
			{
				return Keyboard.IsKeyDown(Keys.RShiftKey);
			}
			return true;
		}
	}

	public static bool CtrlKeyDown
	{
		get
		{
			if (!Keyboard.IsKeyDown(Keys.LControlKey))
			{
				return Keyboard.IsKeyDown(Keys.RControlKey);
			}
			return true;
		}
	}

	public static bool AltlKeyDown
	{
		get
		{
			if (!Keyboard.IsKeyDown(Keys.Menu) && !Keyboard.IsKeyDown(Keys.LMenu))
			{
				return Keyboard.IsKeyDown(Keys.RMenu);
			}
			return true;
		}
	}

	public static bool Is64Bit => IntPtr.Size == 8;

	public static bool IsSleepPrevented => _sleep_prevented;

	public static bool IsScreenSaverPrevented => _display_prevented;

	public static void HightlightControl(Control c, bool bHighlight, bool bFromFinder = false)
	{
		string fullName = c.GetFullName();
		bool flag = false;
		HighlightData highlightData;
		if (!_hightlightData.ContainsKey(fullName))
		{
			highlightData = new HighlightData();
			highlightData.BackgroundColour = c.BackColor;
			highlightData.ForegroundColour = c.ForeColor;
			highlightData.BackgroundImage = c.BackgroundImage;
			highlightData.FlatStyle = FlatStyle.Flat;
			_hightlightData.Add(fullName, highlightData);
			flag = true;
		}
		highlightData = _hightlightData[fullName];
		c.BackColor = (bHighlight ? Color.Yellow : highlightData.BackgroundColour);
		c.ForeColor = (bHighlight ? Color.Black : highlightData.ForegroundColour);
		c.BackgroundImage = (bHighlight ? null : highlightData.BackgroundImage);
		if (c.GetType() == typeof(CheckBoxTS))
		{
			CheckBoxTS checkBoxTS = c as CheckBoxTS;
			if (flag)
			{
				highlightData.FlatStyle = checkBoxTS.FlatStyle;
			}
			checkBoxTS.FlatStyle = ((!bHighlight) ? highlightData.FlatStyle : FlatStyle.Flat);
		}
		else if (c.GetType() == typeof(ComboBoxTS))
		{
			ComboBoxTS comboBoxTS = c as ComboBoxTS;
			if (flag)
			{
				highlightData.FlatStyle = comboBoxTS.FlatStyle;
			}
			comboBoxTS.FlatStyle = ((!bHighlight) ? highlightData.FlatStyle : FlatStyle.Flat);
		}
		else if (c.GetType() == typeof(RadioButtonTS))
		{
			RadioButtonTS radioButtonTS = c as RadioButtonTS;
			if (flag)
			{
				highlightData.FlatStyle = radioButtonTS.FlatStyle;
			}
			radioButtonTS.FlatStyle = ((!bHighlight) ? highlightData.FlatStyle : FlatStyle.Flat);
		}
		if (!bHighlight && _hightlightData.ContainsKey(fullName))
		{
			_hightlightData.Remove(fullName);
		}
		c.Refresh();
	}

	[DllImport("dwmapi.dll")]
	private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

	public static Size DropShadowSize(Form f)
	{
		if (!f.Visible)
		{
			return new Size(0, 0);
		}
		RECT pvAttribute;
		return (Environment.OSVersion.Version.Major < 6) ? new Size(0, 0) : ((DwmGetWindowAttribute(f.Handle, 9, out pvAttribute, Marshal.SizeOf(typeof(RECT))) != 0) ? new Size(0, 0) : new Size(f.Width - (pvAttribute.right - pvAttribute.left), f.Height - (pvAttribute.bottom - pvAttribute.top)));
	}

	public static void ControlList(Control c, ref List<Control> a)
	{
		if (c.Controls.Count > 0)
		{
			foreach (Control control in c.Controls)
			{
				ControlList(control, ref a);
			}
		}
		if (c.GetType() == typeof(CheckBoxTS) || c.GetType() == typeof(CheckBoxTS) || c.GetType() == typeof(ComboBoxTS) || c.GetType() == typeof(ComboBox) || c.GetType() == typeof(NumericUpDownTS) || c.GetType() == typeof(NumericUpDown) || c.GetType() == typeof(RadioButtonTS) || c.GetType() == typeof(RadioButton) || c.GetType() == typeof(TextBoxTS) || c.GetType() == typeof(TextBox) || c.GetType() == typeof(TrackBarTS) || c.GetType() == typeof(TrackBar) || c.GetType() == typeof(ColorButton))
		{
			a.Add(c);
		}
	}

	public static void SaveForm(Form form, string tablename)
	{
		if (DB.ds == null)
		{
			return;
		}
		List<string> list = new List<string>();
		List<Control> a = new List<Control>();
		ControlList(form, ref a);
		foreach (Control item in a)
		{
			if (!(item is CheckBoxTS checkBoxTS))
			{
				if (!(item is ComboBoxTS comboBoxTS))
				{
					if (!(item is NumericUpDownTS numericUpDownTS))
					{
						if (!(item is RadioButtonTS radioButtonTS))
						{
							if (!(item is TextBoxTS textBoxTS))
							{
								if (!(item is TrackBarTS trackBarTS))
								{
									if (item is ColorButton { Color: var color } colorButton)
									{
										list.Add($"{colorButton.Name}/{color.R}.{color.G}.{color.B}.{color.A}");
									}
								}
								else
								{
									list.Add($"{trackBarTS.Name}/{trackBarTS.Value}");
								}
							}
							else
							{
								list.Add(textBoxTS.Name + "/" + textBoxTS.Text);
							}
						}
						else
						{
							list.Add($"{radioButtonTS.Name}/{radioButtonTS.Checked}");
						}
					}
					else
					{
						list.Add($"{numericUpDownTS.Name}/{numericUpDownTS.Value}");
					}
				}
				else
				{
					list.Add(comboBoxTS.Name + "/" + comboBoxTS.Text);
				}
			}
			else
			{
				list.Add($"{checkBoxTS.Name}/{checkBoxTS.Checked}");
			}
		}
		list.Add($"Top/{form.Top}");
		list.Add($"Left/{form.Left}");
		list.Add($"Width/{form.Width}");
		list.Add($"Height/{form.Height}");
		DB.SaveVars(tablename, list);
	}

	public static void RestoreForm(Form form, string tablename, bool restore_size)
	{
		if (DB.ds == null)
		{
			return;
		}
		List<Control> a = new List<Control>();
		ControlList(form, ref a);
		Dictionary<string, Control> dictionary = new Dictionary<string, Control>();
		foreach (Control item in a)
		{
			dictionary.Add(item.Name, item);
		}
		a.Clear();
		List<string> list = DB.GetVars(tablename).OfType<string>().ToList();
		list.Sort();
		foreach (string item2 in list)
		{
			string[] array = item2.Split('/');
			if (array.Length < 2)
			{
				continue;
			}
			string text = array[0];
			string text2 = array[1];
			switch (text)
			{
			case "Top":
			case "Left":
			case "Width":
			case "Height":
			{
				int num = int.Parse(text2);
				switch (text)
				{
				case "Top":
					form.StartPosition = FormStartPosition.Manual;
					form.Top = num;
					break;
				case "Left":
					form.StartPosition = FormStartPosition.Manual;
					form.Left = num;
					break;
				case "Width":
					if (restore_size)
					{
						form.Width = num;
					}
					break;
				case "Height":
					if (restore_size)
					{
						form.Height = num;
					}
					break;
				}
				continue;
			}
			}
			if (!dictionary.TryGetValue(text, out var value))
			{
				continue;
			}
			if (text.StartsWith("chk") && value is CheckBoxTS checkBoxTS)
			{
				checkBoxTS.Checked = bool.Parse(text2);
			}
			else if (text.StartsWith("combo") && value is ComboBoxTS comboBoxTS)
			{
				comboBoxTS.Text = text2;
			}
			else if (text.StartsWith("ud") && value is NumericUpDownTS numericUpDownTS)
			{
				decimal val = decimal.Parse(text2);
				numericUpDownTS.Value = Math.Max(numericUpDownTS.Minimum, Math.Min(val, numericUpDownTS.Maximum));
			}
			else if (text.StartsWith("rad") && value is RadioButtonTS radioButtonTS)
			{
				radioButtonTS.Checked = bool.Parse(text2);
			}
			else if (text.StartsWith("txt") && value is TextBoxTS textBoxTS)
			{
				textBoxTS.Text = text2;
			}
			else if (text.StartsWith("tb") && value is TrackBarTS trackBarTS)
			{
				int val2 = int.Parse(text2);
				trackBarTS.Value = Math.Max(trackBarTS.Minimum, Math.Min(val2, trackBarTS.Maximum));
			}
			else if (text.StartsWith("clrbtn") && value is ColorButton colorButton)
			{
				string[] array2 = text2.Split('.');
				if (array2.Length == 4 && int.TryParse(array2[0], out var result) && int.TryParse(array2[1], out var result2) && int.TryParse(array2[2], out var result3) && int.TryParse(array2[3], out var result4))
				{
					colorButton.Color = Color.FromArgb(result4, result, result2, result3);
				}
			}
		}
		ForceFormOnScreen(form);
	}

	public static (bool resized, bool relocated) ForceFormOnScreen(Form f, bool shrink_to_fit = false, bool keep_on_screen = false)
	{
		if (f == null)
		{
			return (resized: false, relocated: false);
		}
		(Rectangle adjusted, bool resized, bool repositioned) tuple = SafeScreens.EnsureRectangleWithinNearestScreen(null, f, keep_on_screen, use_working_area: true);
		bool item = tuple.resized;
		bool item2 = tuple.repositioned;
		return (resized: item, relocated: item2);
	}

	public static void TabControlInsert(TabControl tc, TabPage tp, int index)
	{
		tc.SuspendLayout();
		TabPage[] array = new TabPage[tc.TabPages.Count + 1];
		for (int i = 0; i < tc.TabPages.Count + 1; i++)
		{
			if (i < index)
			{
				array[i] = tc.TabPages[i];
			}
			else if (i == index)
			{
				array[i] = tp;
			}
			else if (i > index)
			{
				array[i] = tc.TabPages[i - 1];
			}
		}
		while (tc.TabPages.Count > 0)
		{
			tc.TabPages.RemoveAt(0);
		}
		for (int j = 0; j < array.Length; j++)
		{
			tc.TabPages.Add(array[j]);
		}
		tc.ResumeLayout();
	}

	public static string[] SortedComPorts()
	{
		string[] portNames = SerialPort.GetPortNames();
		Array.Sort(portNames, delegate(string strA, string strB)
		{
			try
			{
				int num = int.Parse(strA.Substring(3));
				int value = int.Parse(strB.Substring(3));
				return num.CompareTo(value);
			}
			catch (Exception)
			{
				return strA.CompareTo(strB);
			}
		});
		return portNames;
	}

	public static string RevToString(uint rev)
	{
		return (byte)(rev >> 24) + "." + (byte)(rev >> 16) + "." + (byte)(rev >> 8) + "." + (byte)rev;
	}

	public static void SetLogPath(string sPath)
	{
		m_sLogPath = sPath;
	}

	public static void LogString(string entry)
	{
		if (m_sLogPath == "" || entry == "")
		{
			return;
		}
		try
		{
			using StreamWriter streamWriter = File.AppendText(m_sLogPath + "\\ErrorLog.txt");
			streamWriter.Write("\r\nEntry : ");
			streamWriter.WriteLine(DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString());
			streamWriter.WriteLine(entry);
			streamWriter.WriteLine("-------------------------------");
		}
		catch
		{
		}
	}

	public static void LogException(Exception e)
	{
		if (m_sLogPath == "" || e == null)
		{
			return;
		}
		try
		{
			using StreamWriter streamWriter = File.AppendText(m_sLogPath + "\\ErrorLog.txt");
			streamWriter.Write("\r\nEntry : ");
			streamWriter.WriteLine(DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString());
			streamWriter.WriteLine(e.Message);
			if (e.StackTrace != "")
			{
				streamWriter.WriteLine("---------stacktrace------------");
				streamWriter.WriteLine(e.StackTrace);
			}
			streamWriter.WriteLine("-------------------------------");
		}
		catch
		{
		}
	}

	public static string GetVerNum(bool include_revision = false, bool include_build = false)
	{
		if (string.IsNullOrEmpty(m_sVersionNumber))
		{
			setupVersions();
		}
		string text = m_sVersionNumber;
		if (include_revision)
		{
			string text2 = m_sRevision;
			if (text2 == ".0")
			{
				text2 = "";
			}
			text = text + "." + text2;
		}
		if (include_build)
		{
			text += " EU2AV";
		}
		return text;
	}

	public static string GetFileVersion()
	{
		if (m_sFileVersion != "")
		{
			return m_sFileVersion;
		}
		setupVersions();
		return m_sFileVersion;
	}

	public static string GetRevision()
	{
		if (m_sRevision != "")
		{
			return m_sRevision;
		}
		setupVersions();
		return m_sRevision;
	}

	private static void setupVersions()
	{
		if (!(m_sVersionNumber != "") || !(m_sFileVersion != ""))
		{
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
			m_sVersionNumber = versionInfo.FileVersion.Substring(0, versionInfo.FileVersion.LastIndexOf("."));
			m_sFileVersion = versionInfo.FileVersion;
			m_sRevision = versionInfo.FileVersion.Substring(versionInfo.FileVersion.LastIndexOf(".") + 1);
		}
	}

	public static bool IsAdministrator()
	{
		using WindowsIdentity ntIdentity = WindowsIdentity.GetCurrent();
		return new WindowsPrincipal(ntIdentity).IsInRole(WindowsBuiltInRole.Administrator);
	}

	public static void DoubleBuffered(Control control, bool enabled)
	{
		PropertyInfo property = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
		if (property != null)
		{
			property.SetValue(control, enabled, null);
		}
	}

	public static int FiveDigitHash(string str)
	{
		if (str == "")
		{
			return 0;
		}
		uint num = 0u;
		byte[] bytes = Encoding.Unicode.GetBytes(str);
		foreach (byte b in bytes)
		{
			num += b;
			num += num << 10;
			num ^= num >> 6;
		}
		num += num << 3;
		num ^= num >> 11;
		num += num << 15;
		return (int)(num % 99999);
	}

	public static string ColourToString(Color c)
	{
		return c.A + "." + c.R + "." + c.G + "." + c.B;
	}

	public static Color ColourFromString(string str)
	{
		string[] array = str.Split('.');
		if (array.Length == 4)
		{
			int result = 0;
			int result2 = 0;
			int result3 = 0;
			int result4 = 0;
			bool flag = int.TryParse(array[0], out result);
			if (flag)
			{
				flag = int.TryParse(array[1], out result2);
			}
			if (flag)
			{
				flag = int.TryParse(array[2], out result3);
			}
			if (flag)
			{
				flag = int.TryParse(array[3], out result4);
			}
			if (flag)
			{
				return Color.FromArgb(result, result2, result3, result4);
			}
		}
		return Color.Empty;
	}

	public static double UVfromDBM(double dbm)
	{
		return Math.Sqrt(Math.Pow(10.0, dbm / 10.0) * 50.0 * 0.001) * 1000000.0;
	}

	public static string SMeterFromDBM(double dbm, bool bAboveS9Frequency)
	{
		string text = (bAboveS9Frequency ? ((dbm <= -144.0) ? "S 0" : (((dbm > -144.0) & (dbm <= -138.0)) ? "S 1" : (((dbm > -138.0) & (dbm <= -132.0)) ? "S 2" : (((dbm > -132.0) & (dbm <= -126.0)) ? "S 3" : (((dbm > -126.0) & (dbm <= -120.0)) ? "S 4" : (((dbm > -120.0) & (dbm <= -114.0)) ? "S 5" : (((dbm > -114.0) & (dbm <= -108.0)) ? "S 6" : (((dbm > -108.0) & (dbm <= -102.0)) ? "S 7" : (((dbm > -102.0) & (dbm <= -96.0)) ? "S 8" : (((dbm > -96.0) & (dbm <= -90.0)) ? "S 9" : (((dbm > -90.0) & (dbm <= -86.0)) ? "S 9 + 5" : (((dbm > -86.0) & (dbm <= -80.0)) ? "S 9 + 10" : (((dbm > -80.0) & (dbm <= -76.0)) ? "S 9 + 15" : (((dbm > -76.0) & (dbm <= -66.0)) ? "S 9 + 20" : (((dbm > -66.0) & (dbm <= -56.0)) ? "S 9 + 30" : (((dbm > -56.0) & (dbm <= -46.0)) ? "S 9 + 40" : ((!((dbm > -46.0) & (dbm <= -36.0))) ? "S 9 + 60" : "S 9 + 50"))))))))))))))))) : ((dbm <= -124.0) ? "S 0" : (((dbm > -124.0) & (dbm <= -118.0)) ? "S 1" : (((dbm > -118.0) & (dbm <= -112.0)) ? "S 2" : (((dbm > -112.0) & (dbm <= -106.0)) ? "S 3" : (((dbm > -106.0) & (dbm <= -100.0)) ? "S 4" : (((dbm > -100.0) & (dbm <= -94.0)) ? "S 5" : (((dbm > -94.0) & (dbm <= -88.0)) ? "S 6" : (((dbm > -88.0) & (dbm <= -82.0)) ? "S 7" : (((dbm > -82.0) & (dbm <= -76.0)) ? "S 8" : (((dbm > -76.0) & (dbm <= -70.0)) ? "S 9" : (((dbm > -70.0) & (dbm <= -66.0)) ? "S 9 + 5" : (((dbm > -66.0) & (dbm <= -60.0)) ? "S 9 + 10" : (((dbm > -60.0) & (dbm <= -56.0)) ? "S 9 + 15" : (((dbm > -56.0) & (dbm <= -46.0)) ? "S 9 + 20" : (((dbm > -46.0) & (dbm <= -36.0)) ? "S 9 + 30" : (((dbm > -36.0) & (dbm <= -26.0)) ? "S 9 + 40" : ((!((dbm > -26.0) & (dbm <= -16.0))) ? "S 9 + 60" : "S 9 + 50"))))))))))))))))));
		return "    " + text;
	}

	public static string SMeterFromDBM_Spaceless(double dbm, bool bAboveS9Frequency)
	{
		if (bAboveS9Frequency)
		{
			if (dbm <= -144.0)
			{
				return "S0";
			}
			if ((dbm > -144.0) & (dbm <= -138.0))
			{
				return "S1";
			}
			if ((dbm > -138.0) & (dbm <= -132.0))
			{
				return "S2";
			}
			if ((dbm > -132.0) & (dbm <= -126.0))
			{
				return "S3";
			}
			if ((dbm > -126.0) & (dbm <= -120.0))
			{
				return "S4";
			}
			if ((dbm > -120.0) & (dbm <= -114.0))
			{
				return "S5";
			}
			if ((dbm > -114.0) & (dbm <= -108.0))
			{
				return "S6";
			}
			if ((dbm > -108.0) & (dbm <= -102.0))
			{
				return "S7";
			}
			if ((dbm > -102.0) & (dbm <= -96.0))
			{
				return "S8";
			}
			if ((dbm > -96.0) & (dbm <= -90.0))
			{
				return "S9";
			}
			if ((dbm > -90.0) & (dbm <= -86.0))
			{
				return "S9+5";
			}
			if ((dbm > -86.0) & (dbm <= -80.0))
			{
				return "S9+10";
			}
			if ((dbm > -80.0) & (dbm <= -76.0))
			{
				return "S9+15";
			}
			if ((dbm > -76.0) & (dbm <= -66.0))
			{
				return "S9+20";
			}
			if ((dbm > -66.0) & (dbm <= -56.0))
			{
				return "S9+30";
			}
			if ((dbm > -56.0) & (dbm <= -46.0))
			{
				return "S9+40";
			}
			if ((dbm > -46.0) & (dbm <= -36.0))
			{
				return "S9+50";
			}
			return "S9+60";
		}
		if (dbm <= -124.0)
		{
			return "S0";
		}
		if ((dbm > -124.0) & (dbm <= -118.0))
		{
			return "S1";
		}
		if ((dbm > -118.0) & (dbm <= -112.0))
		{
			return "S2";
		}
		if ((dbm > -112.0) & (dbm <= -106.0))
		{
			return "S3";
		}
		if ((dbm > -106.0) & (dbm <= -100.0))
		{
			return "S4";
		}
		if ((dbm > -100.0) & (dbm <= -94.0))
		{
			return "S5";
		}
		if ((dbm > -94.0) & (dbm <= -88.0))
		{
			return "S6";
		}
		if ((dbm > -88.0) & (dbm <= -82.0))
		{
			return "S7";
		}
		if ((dbm > -82.0) & (dbm <= -76.0))
		{
			return "S8";
		}
		if ((dbm > -76.0) & (dbm <= -70.0))
		{
			return "S9";
		}
		if ((dbm > -70.0) & (dbm <= -66.0))
		{
			return "S9+5";
		}
		if ((dbm > -66.0) & (dbm <= -60.0))
		{
			return "S9+10";
		}
		if ((dbm > -60.0) & (dbm <= -56.0))
		{
			return "S9+15";
		}
		if ((dbm > -56.0) & (dbm <= -46.0))
		{
			return "S9+20";
		}
		if ((dbm > -46.0) & (dbm <= -36.0))
		{
			return "S9+30";
		}
		if ((dbm > -36.0) & (dbm <= -26.0))
		{
			return "S9+40";
		}
		if ((dbm > -26.0) & (dbm <= -16.0))
		{
			return "S9+50";
		}
		return "S9+60";
	}

	public static double GetSMeterUnits(double dbm, bool bAboveS9Frequency)
	{
		if (bAboveS9Frequency)
		{
			return 9.0 + (dbm + 93.0) / 6.0;
		}
		return 9.0 + (dbm + 73.0) / 6.0;
	}

	public static void SMeterFromDBM2(double dbm, bool bAboveS9Frequency, out int S, out int over9dBm)
	{
		if (bAboveS9Frequency)
		{
			if (dbm <= -144.0)
			{
				S = 0;
				over9dBm = 0;
			}
			else if ((dbm > -144.0) & (dbm <= -138.0))
			{
				S = 1;
				over9dBm = 0;
			}
			else if ((dbm > -138.0) & (dbm <= -132.0))
			{
				S = 2;
				over9dBm = 0;
			}
			else if ((dbm > -132.0) & (dbm <= -126.0))
			{
				S = 3;
				over9dBm = 0;
			}
			else if ((dbm > -126.0) & (dbm <= -120.0))
			{
				S = 4;
				over9dBm = 0;
			}
			else if ((dbm > -120.0) & (dbm <= -114.0))
			{
				S = 5;
				over9dBm = 0;
			}
			else if ((dbm > -114.0) & (dbm <= -108.0))
			{
				S = 6;
				over9dBm = 0;
			}
			else if ((dbm > -108.0) & (dbm <= -102.0))
			{
				S = 7;
				over9dBm = 0;
			}
			else if ((dbm > -102.0) & (dbm <= -96.0))
			{
				S = 8;
				over9dBm = 0;
			}
			else if ((dbm > -96.0) & (dbm <= -90.0))
			{
				S = 9;
				over9dBm = 0;
			}
			else if ((dbm > -90.0) & (dbm <= -86.0))
			{
				S = 9;
				over9dBm = 5;
			}
			else if ((dbm > -86.0) & (dbm <= -80.0))
			{
				S = 9;
				over9dBm = 10;
			}
			else if ((dbm > -80.0) & (dbm <= -76.0))
			{
				S = 9;
				over9dBm = 15;
			}
			else if ((dbm > -76.0) & (dbm <= -66.0))
			{
				S = 9;
				over9dBm = 20;
			}
			else if ((dbm > -66.0) & (dbm <= -56.0))
			{
				S = 9;
				over9dBm = 30;
			}
			else if ((dbm > -56.0) & (dbm <= -46.0))
			{
				S = 9;
				over9dBm = 40;
			}
			else if ((dbm > -46.0) & (dbm <= -36.0))
			{
				S = 9;
				over9dBm = 50;
			}
			else
			{
				S = 9;
				over9dBm = 60;
			}
		}
		else if (dbm <= -124.0)
		{
			S = 0;
			over9dBm = 0;
		}
		else if ((dbm > -124.0) & (dbm <= -118.0))
		{
			S = 1;
			over9dBm = 0;
		}
		else if ((dbm > -118.0) & (dbm <= -112.0))
		{
			S = 2;
			over9dBm = 0;
		}
		else if ((dbm > -112.0) & (dbm <= -106.0))
		{
			S = 3;
			over9dBm = 0;
		}
		else if ((dbm > -106.0) & (dbm <= -100.0))
		{
			S = 4;
			over9dBm = 0;
		}
		else if ((dbm > -100.0) & (dbm <= -94.0))
		{
			S = 5;
			over9dBm = 0;
		}
		else if ((dbm > -94.0) & (dbm <= -88.0))
		{
			S = 6;
			over9dBm = 0;
		}
		else if ((dbm > -88.0) & (dbm <= -82.0))
		{
			S = 7;
			over9dBm = 0;
		}
		else if ((dbm > -82.0) & (dbm <= -76.0))
		{
			S = 8;
			over9dBm = 0;
		}
		else if ((dbm > -76.0) & (dbm <= -70.0))
		{
			S = 9;
			over9dBm = 0;
		}
		else if ((dbm > -70.0) & (dbm <= -66.0))
		{
			S = 9;
			over9dBm = 5;
		}
		else if ((dbm > -66.0) & (dbm <= -60.0))
		{
			S = 9;
			over9dBm = 10;
		}
		else if ((dbm > -60.0) & (dbm <= -56.0))
		{
			S = 9;
			over9dBm = 15;
		}
		else if ((dbm > -56.0) & (dbm <= -46.0))
		{
			S = 9;
			over9dBm = 20;
		}
		else if ((dbm > -46.0) & (dbm <= -36.0))
		{
			S = 9;
			over9dBm = 30;
		}
		else if ((dbm > -36.0) & (dbm <= -26.0))
		{
			S = 9;
			over9dBm = 40;
		}
		else if ((dbm > -26.0) & (dbm <= -16.0))
		{
			S = 9;
			over9dBm = 50;
		}
		else
		{
			S = 9;
			over9dBm = 60;
		}
	}

	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

	public static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
	{
		if (IsWindows10OrGreater(17763))
		{
			int attr = 19;
			if (IsWindows10OrGreater(18985))
			{
				attr = 20;
			}
			int attrValue = (enabled ? 1 : 0);
			return DwmSetWindowAttribute(handle, attr, ref attrValue, 4) == 0;
		}
		return false;
	}

	public static bool IsWindows10OrGreater(int build = -1)
	{
		if (Environment.OSVersion.Version.Major >= 10)
		{
			return Environment.OSVersion.Version.Build >= build;
		}
		return false;
	}

	public static string DateTimeStringForFile(string cultureName = "")
	{
		CultureInfo cultureInfo = ((!(cultureName == "")) ? CultureInfo.GetCultureInfo(cultureName) : CultureInfo.InstalledUICulture);
		DateTime now = DateTime.Now;
		string text = now.ToString(cultureInfo.DateTimeFormat.ShortDatePattern, cultureInfo) + "_" + now.ToString(cultureInfo.DateTimeFormat.ShortTimePattern, cultureInfo);
		text = text.Replace("/", "_");
		text = text.Replace(":", "_");
		text = text.Replace(".", "_");
		return string.Join("_", text.Split(Path.GetInvalidFileNameChars()));
	}

	public static async void FadeIn(Form frm, int msTimeToFade = 500, int steps = 20)
	{
		float stepSize = 1f / (float)steps;
		float interval = (float)msTimeToFade / (float)steps;
		while (frm.Opacity < 1.0)
		{
			await Task.Delay((int)interval);
			frm.Opacity += stepSize;
		}
		frm.Opacity = 1.0;
	}

	public static async void FadeOut(Form frm, int msTimeToFade = 500, int steps = 20)
	{
		float stepSize = 1f / (float)steps;
		float interval = (float)msTimeToFade / (float)steps;
		while (frm.Opacity > 0.0)
		{
			await Task.Delay((int)interval);
			frm.Opacity -= stepSize;
		}
		frm.Opacity = 0.0;
	}

	public static int CompareVersions(string version1, string version2)
	{
		string[] array = (from part in version1.Split('.')
			select tryParseVersionPart(part)).ToArray();
		string[] array2 = (from part in version2.Split('.')
			select tryParseVersionPart(part)).ToArray();
		int num = Math.Max(array.Length, array2.Length);
		for (int num2 = 0; num2 < num; num2++)
		{
			int num3 = ((num2 < array.Length) ? int.Parse(array[num2]) : 0);
			int value = ((num2 < array2.Length) ? int.Parse(array2[num2]) : 0);
			int num4 = num3.CompareTo(value);
			if (num4 != 0)
			{
				return num4;
			}
		}
		return 0;
	}

	private static string tryParseVersionPart(string part)
	{
		if (int.TryParse(part, out var result))
		{
			return result.ToString();
		}
		return "-1";
	}

	public static bool IsValidUri(string uri)
	{
		if (uri == "")
		{
			return false;
		}
		try
		{
			if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
			{
				return false;
			}
			if (!Uri.TryCreate(uri, UriKind.Absolute, out var result))
			{
				return false;
			}
			return result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps;
		}
		catch
		{
			return false;
		}
	}

	public static bool OpenUri(string uri, bool check_uri = true)
	{
		try
		{
			if (check_uri && !IsValidUri(uri))
			{
				return false;
			}
			Task.Run(() => Process.Start(uri));
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static int FindNextPowerOf2(int n)
	{
		n--;
		n |= n >> 1;
		n |= n >> 2;
		n |= n >> 4;
		n |= n >> 8;
		n |= n >> 16;
		return ++n;
	}

	public static int FindPreviousPowerOf2(int n)
	{
		n |= n >> 1;
		n |= n >> 2;
		n |= n >> 4;
		n |= n >> 8;
		n |= n >> 16;
		return n - (n >> 1);
	}

	public static bool IsIpv4Valid(string ip, int port)
	{
		if (!IPAddress.TryParse(ip, out var address))
		{
			return false;
		}
		if (address.AddressFamily != AddressFamily.InterNetwork)
		{
			return false;
		}
		if (port < 1 || port > 65535)
		{
			return false;
		}
		string pattern = "^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
		if (!Regex.IsMatch(ip, pattern))
		{
			return false;
		}
		return true;
	}

	public static string SerializeToBase64<T>(T obj)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (GZipStream serializationStream = new GZipStream(memoryStream, CompressionMode.Compress))
		{
			((IFormatter)new BinaryFormatter()).Serialize((Stream)serializationStream, (object)obj);
		}
		return Convert.ToBase64String(memoryStream.ToArray());
	}

	public static T DeserializeFromBase64<T>(string base64String)
	{
		using MemoryStream stream = new MemoryStream(Convert.FromBase64String(base64String));
		using GZipStream serializationStream = new GZipStream(stream, CompressionMode.Decompress);
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		TypeRenameBinder binder = TypeRenameBinder.Create().Add("Thetis.MeterManager+clsFilterItem+DisplayMode", typeof(MeterManager.clsFilterItem.FIDisplayMode)).Add("Thetis.MeterManager+clsFilterItem+WaterfallPalette", typeof(MeterManager.clsFilterItem.FIWaterfallPalette));
		binaryFormatter.Binder = binder;
		return (T)binaryFormatter.Deserialize(serializationStream);
	}

	public static bool HasArg(string[] args, string arg)
	{
		if (args == null || args.Length < 1 || string.IsNullOrEmpty(arg))
		{
			return false;
		}
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i].Contains(arg, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public static string ArgParam(string[] args, string arg)
	{
		string result = string.Empty;
		if (args == null || args.Length < 1 || string.IsNullOrEmpty(arg))
		{
			return result;
		}
		foreach (string text in args)
		{
			if (text.Contains(arg, StringComparison.OrdinalIgnoreCase))
			{
				string text2 = text.Trim();
				int num = text2.IndexOf(":");
				if (num != -1)
				{
					result = text2.Substring(num + 1);
				}
				break;
			}
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetLuminance(Color c)
	{
		int num = rGBtoLin(c.R);
		int num2 = rGBtoLin(c.G);
		int num3 = rGBtoLin(c.B);
		return (num + num + num3 + num2 + num2 + num2) / 6;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int rGBtoLin(int col)
	{
		float num = (float)col / 255f;
		if ((double)num <= 0.04045)
		{
			return (int)((double)num / 12.92 * 255.0);
		}
		return (int)(Math.Pow(((double)num + 0.055) / 1.055, 2.4) * 255.0);
	}

	public static void DoubleBufferAll(Control control, bool enabled)
	{
		DoubleBuffered(control, enabled);
		foreach (Control control2 in control.Controls)
		{
			DoubleBufferAll(control2, enabled);
		}
	}

	public static bool IsValidFilename(string filename)
	{
		char[] invalidChars = Path.GetInvalidFileNameChars();
		if (filename.Any((char ch) => invalidChars.Contains(ch)))
		{
			return false;
		}
		if (filename.Length >= 260)
		{
			return false;
		}
		return true;
	}

	public static bool IsValidPath(string path)
	{
		char[] invalidChars = Path.GetInvalidPathChars();
		if (path.Any((char ch) => invalidChars.Contains(ch)))
		{
			return false;
		}
		if (path.Length >= 260)
		{
			return false;
		}
		return true;
	}

	public static void DebugPrintCallStack(bool only_with_line = true)
	{
		StackFrame[] frames = new StackTrace(fNeedFileInfo: true).GetFrames();
		for (int i = 0; i < frames.Length; i++)
		{
			if (frames[i].GetFileLineNumber() == 0)
			{
			}
		}
	}

	public static string FourChar(string data1, int data2, Guid guid)
	{
		string s = $"{data1}:{data2}:{guid}";
		using SHA256 sHA = SHA256.Create();
		return convertToFourChar(Convert.ToBase64String(sHA.ComputeHash(Encoding.UTF8.GetBytes(s))));
	}

	private static string convertToFourChar(string base64Hash)
	{
		char[] array = new char[4];
		int[] array2 = new int[4];
		for (int i = 0; i < base64Hash.Length; i++)
		{
			array2[i % 4] = (array2[i % 4] + base64Hash[i]) % "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".Length;
		}
		for (int j = 0; j < 4; j++)
		{
			array[j] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"[array2[j]];
		}
		return new string(array);
	}

	public static bool CanCreateFile(string filePath)
	{
		try
		{
			string directoryName = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(directoryName))
			{
				return false;
			}
			if (!hasWritePermissionOnDir(directoryName))
			{
				return false;
			}
			if (File.Exists(filePath))
			{
				if (new FileInfo(filePath).IsReadOnly)
				{
					return false;
				}
				if (!isFileWritable(filePath))
				{
					return false;
				}
			}
			string path = Path.Combine(directoryName, Path.GetRandomFileName());
			File.Create(path).Close();
			File.Delete(path);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static bool hasWritePermissionOnDir(string path)
	{
		try
		{
			File.Create(Path.Combine(path, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose).Close();
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static bool isFileWritable(string filePath)
	{
		try
		{
			File.Open(filePath, FileMode.Open, FileAccess.Write).Close();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool GetComPortNumber(string comport, out int portNumber)
	{
		string text = comport.ToLower();
		if (!text.StartsWith("com"))
		{
			portNumber = 0;
			return false;
		}
		return int.TryParse(text.Substring(3), out portNumber);
	}

	[DllImport("kernel32.dll")]
	private static extern bool SetProcessPriorityBoost(IntPtr processHandle, bool disablePriorityBoost);

	public static void DisableForegroundPriorityBoost()
	{
		try
		{
			SetProcessPriorityBoost(Process.GetCurrentProcess().Handle, disablePriorityBoost: true);
		}
		catch
		{
		}
	}

	public static string GetCpuName()
	{
		try
		{
			ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
			ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
			string result = "Unknown CPU";
			using (ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = managementObjectCollection.GetEnumerator())
			{
				if (managementObjectEnumerator.MoveNext())
				{
					ManagementObject managementObject = (ManagementObject)managementObjectEnumerator.Current;
					result = ((managementObject["Name"] != null) ? managementObject["Name"].ToString().Trim() : string.Empty);
				}
			}
			managementObjectCollection.Dispose();
			managementObjectSearcher.Dispose();
			return result;
		}
		catch
		{
			return "Unknown CPU";
		}
	}

	public static List<string> GetGpuNames()
	{
		List<string> list = new List<string>();
		try
		{
			using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
			{
				using ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
				foreach (ManagementObject item in managementObjectCollection)
				{
					string text = ((item["Name"] != null) ? item["Name"].ToString().Trim() : string.Empty);
					if (text != string.Empty)
					{
						list.Add(text);
					}
				}
			}
			if (list.Count == 0)
			{
				list.Add("Unknown GPU(s)");
			}
			return list;
		}
		catch
		{
			list.Clear();
			list.Add("Unknown GPU(s)");
			return list;
		}
	}

	public static string GetTotalRam()
	{
		try
		{
			using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
			using ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
			using ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = managementObjectCollection.GetEnumerator();
			if (managementObjectEnumerator.MoveNext())
			{
				ManagementObject managementObject = (ManagementObject)managementObjectEnumerator.Current;
				if (ulong.TryParse((managementObject["TotalPhysicalMemory"] != null) ? managementObject["TotalPhysicalMemory"].ToString() : "0", out var result))
				{
					return ((double)result / 1024.0 / 1024.0 / 1024.0).ToString("F2") + " GiB";
				}
			}
		}
		catch
		{
		}
		return "Unknown";
	}

	public static string GetInstalledRam()
	{
		try
		{
			using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
			using ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
			ulong num = 0uL;
			foreach (ManagementObject item in managementObjectCollection)
			{
				num += (ulong)item["Capacity"];
			}
			return ((double)num / 1024.0 / 1024.0 / 1024.0).ToString("F2") + " GiB";
		}
		catch
		{
			return "Unknown";
		}
	}

	[DllImport("Shcore.dll")]
	private static extern int GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);

	[DllImport("User32.dll")]
	private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

	public static int GetScalingForWindow(IntPtr hwnd)
	{
		OperatingSystem oSVersion = Environment.OSVersion;
		Version version = oSVersion.Version;
		if (oSVersion.Platform == PlatformID.Win32NT && (version.Major > 6 || (version.Major == 6 && version.Minor >= 3)))
		{
			try
			{
				if (GetDpiForMonitor(MonitorFromWindow(hwnd, 2u), MonitorDpiType.MDT_EFFECTIVE_DPI, out var dpiX, out var _) == 0)
				{
					return (int)(dpiX * 100 / 96);
				}
			}
			catch (DllNotFoundException)
			{
			}
			catch (EntryPointNotFoundException)
			{
			}
		}
		using Graphics graphics = Graphics.FromHwnd(hwnd);
		return (int)(graphics.DpiX * 100f / 96f);
	}

	public static double ProcessCPUUsage()
	{
		if (_timer == null)
		{
			_timer = new HiPerfTimer();
			_timer.Start();
			_previousCpuTime = Process.GetCurrentProcess().TotalProcessorTime;
			_previousElapsedSeconds = _timer.Elapsed;
		}
		TimeSpan totalProcessorTime = Process.GetCurrentProcess().TotalProcessorTime;
		double elapsed = _timer.Elapsed;
		TimeSpan timeSpan = totalProcessorTime - _previousCpuTime;
		double num = elapsed - _previousElapsedSeconds;
		_previousCpuTime = totalProcessorTime;
		_previousElapsedSeconds = elapsed;
		if (num <= 0.0)
		{
			return 0.0;
		}
		return timeSpan.TotalSeconds / (num * (double)Environment.ProcessorCount) * 100.0;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);

	public static void PreventSleep()
	{
		try
		{
			_previous_sleep_state = SetThreadExecutionState(ExecutionState.ES_CONTINUOUS | ExecutionState.ES_SYSTEM_REQUIRED);
			_sleep_prevented = true;
		}
		catch
		{
		}
	}

	public static void PreventScreenSaver()
	{
		try
		{
			_previous_display_state = SetThreadExecutionState(ExecutionState.ES_CONTINUOUS | ExecutionState.ES_DISPLAY_REQUIRED);
			_display_prevented = true;
		}
		catch
		{
		}
	}

	public static ExecutionState ResumeSleep()
	{
		try
		{
			if (!_sleep_prevented)
			{
				return (ExecutionState)0u;
			}
			ExecutionState result = SetThreadExecutionState(_previous_sleep_state);
			_previous_sleep_state = (ExecutionState)0u;
			_sleep_prevented = false;
			return result;
		}
		catch
		{
			return (ExecutionState)0u;
		}
	}

	public static ExecutionState ResumeScreenSaver()
	{
		try
		{
			if (!_display_prevented)
			{
				return (ExecutionState)0u;
			}
			ExecutionState result = SetThreadExecutionState(_previous_display_state);
			_previous_display_state = (ExecutionState)0u;
			_display_prevented = false;
			return result;
		}
		catch
		{
			return (ExecutionState)0u;
		}
	}

	public static string GenerateKeyBase64()
	{
		byte[] array = new byte[32];
		using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
		{
			randomNumberGenerator.GetBytes(array);
		}
		return Convert.ToBase64String(array);
	}

	public static string EncryptAndCombineIvToBase64(string plaintext, byte[] key)
	{
		if (string.IsNullOrEmpty(plaintext) || key == null)
		{
			return string.Empty;
		}
		try
		{
			using Aes aes = Aes.Create();
			aes.Key = key;
			aes.GenerateIV();
			using ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);
			using MemoryStream memoryStream = new MemoryStream();
			using CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
			byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
			cryptoStream.Write(bytes, 0, bytes.Length);
			cryptoStream.FlushFinalBlock();
			byte[] array = memoryStream.ToArray();
			byte[] array2 = new byte[aes.IV.Length + array.Length];
			Buffer.BlockCopy(aes.IV, 0, array2, 0, aes.IV.Length);
			Buffer.BlockCopy(array, 0, array2, aes.IV.Length, array.Length);
			return Convert.ToBase64String(array2);
		}
		catch (Exception)
		{
			return string.Empty;
		}
	}

	public static string DecryptFromCombinedIvBase64(string combinedBase64, byte[] key)
	{
		if (string.IsNullOrEmpty(combinedBase64) || key == null)
		{
			return string.Empty;
		}
		try
		{
			byte[] array = Convert.FromBase64String(combinedBase64);
			using Aes aes = Aes.Create();
			aes.Key = key;
			int num = aes.BlockSize / 8;
			byte[] array2 = new byte[num];
			byte[] array3 = new byte[array.Length - num];
			Buffer.BlockCopy(array, 0, array2, 0, num);
			Buffer.BlockCopy(array, num, array3, 0, array3.Length);
			aes.IV = array2;
			using ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
			using MemoryStream stream = new MemoryStream(array3);
			using CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);
			using MemoryStream memoryStream = new MemoryStream();
			cryptoStream.CopyTo(memoryStream);
			byte[] bytes = memoryStream.ToArray();
			return Encoding.UTF8.GetString(bytes);
		}
		catch (Exception)
		{
			return string.Empty;
		}
	}

	public static string Compress_gzip(string uncompressed_input)
	{
		if (string.IsNullOrEmpty(uncompressed_input))
		{
			return null;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(uncompressed_input);
		using MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionLevel.Optimal, leaveOpen: true))
		{
			gZipStream.Write(bytes, 0, bytes.Length);
		}
		return Convert.ToBase64String(memoryStream.ToArray()).Replace('+', '-').Replace('/', '_')
			.TrimEnd('=');
	}

	public static string Decompress_gzip(string compressed_input)
	{
		if (string.IsNullOrEmpty(compressed_input))
		{
			return null;
		}
		string text = compressed_input.Replace('-', '+').Replace('_', '/');
		switch (text.Length % 4)
		{
		case 2:
			text += "==";
			break;
		case 3:
			text += "=";
			break;
		case 1:
			throw new FormatException("Invalid Base64URL length");
		}
		using MemoryStream stream = new MemoryStream(Convert.FromBase64String(text));
		using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
		using MemoryStream memoryStream = new MemoryStream();
		byte[] array = new byte[8192];
		int count;
		while ((count = gZipStream.Read(array, 0, array.Length)) > 0)
		{
			memoryStream.Write(array, 0, count);
		}
		byte[] bytes = memoryStream.ToArray();
		return Encoding.UTF8.GetString(bytes);
	}

	[DllImport("kernel32.dll")]
	private static extern bool GetDiskFreeSpaceExW([MarshalAs(UnmanagedType.LPWStr)] string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

	public static bool TryGetDriveTotalAndFreeBytes(string folderPath, out ulong totalBytes, out ulong freeBytes)
	{
		totalBytes = 0uL;
		freeBytes = 0uL;
		try
		{
			if (string.IsNullOrWhiteSpace(folderPath))
			{
				return false;
			}
			string text = folderPath.Trim();
			try
			{
				text = Path.GetFullPath(text);
			}
			catch
			{
			}
			if (text.StartsWith("\\\\?\\UNC\\", StringComparison.OrdinalIgnoreCase))
			{
				text = "\\\\" + text.Substring(8);
			}
			else if (text.StartsWith("\\\\?\\", StringComparison.OrdinalIgnoreCase))
			{
				text = text.Substring(4);
			}
			string text2 = text;
			try
			{
				if (File.Exists(text2))
				{
					string directoryName = Path.GetDirectoryName(text2);
					if (!string.IsNullOrWhiteSpace(directoryName))
					{
						text2 = directoryName;
					}
				}
			}
			catch
			{
			}
			string text3 = text2;
			for (int i = 0; i < 64; i++)
			{
				bool flag = false;
				try
				{
					flag = Directory.Exists(text3);
				}
				catch
				{
					flag = false;
				}
				if (flag)
				{
					break;
				}
				string text4 = null;
				try
				{
					text4 = Path.GetDirectoryName(text3);
				}
				catch
				{
				}
				if (string.IsNullOrWhiteSpace(text4) || string.Equals(text4, text3, StringComparison.Ordinal))
				{
					break;
				}
				text3 = text4;
			}
			bool diskFreeSpaceExW = GetDiskFreeSpaceExW(text3, out var lpFreeBytesAvailable, out var lpTotalNumberOfBytes, out var lpTotalNumberOfFreeBytes);
			if (!diskFreeSpaceExW)
			{
				string text5 = null;
				try
				{
					text5 = Path.GetPathRoot(text);
				}
				catch
				{
				}
				if (!string.IsNullOrWhiteSpace(text5))
				{
					diskFreeSpaceExW = GetDiskFreeSpaceExW(text5, out lpFreeBytesAvailable, out lpTotalNumberOfBytes, out lpTotalNumberOfFreeBytes);
				}
				if (!diskFreeSpaceExW && text.StartsWith("\\\\", StringComparison.Ordinal))
				{
					string uncShareRoot = getUncShareRoot(text);
					if (!string.IsNullOrWhiteSpace(uncShareRoot))
					{
						diskFreeSpaceExW = GetDiskFreeSpaceExW(uncShareRoot, out lpFreeBytesAvailable, out lpTotalNumberOfBytes, out lpTotalNumberOfFreeBytes);
					}
				}
			}
			if (!diskFreeSpaceExW)
			{
				return false;
			}
			totalBytes = lpTotalNumberOfBytes;
			freeBytes = lpFreeBytesAvailable;
			return true;
		}
		catch
		{
			totalBytes = 0uL;
			freeBytes = 0uL;
			return false;
		}
	}

	private static string getUncShareRoot(string uncPath)
	{
		if (string.IsNullOrWhiteSpace(uncPath))
		{
			return null;
		}
		if (!uncPath.StartsWith("\\\\", StringComparison.Ordinal))
		{
			return null;
		}
		int num = uncPath.IndexOf('\\', 2);
		if (num < 0)
		{
			return null;
		}
		int num2 = uncPath.IndexOf('\\', num + 1);
		if (num2 < 0)
		{
			num2 = uncPath.Length;
		}
		string text = uncPath.Substring(0, num2);
		if (!text.EndsWith("\\", StringComparison.Ordinal))
		{
			text += "\\";
		}
		return text;
	}
}
