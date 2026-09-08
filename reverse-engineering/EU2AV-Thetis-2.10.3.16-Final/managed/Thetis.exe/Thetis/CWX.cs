using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Thetis;

public class CWX : Form
{
	private delegate void TimeProc(int id, int msg, int user, int param1, int param2);

	public enum TimerMode
	{
		OneShot,
		Periodic
	}

	public struct TimerCaps
	{
		public int periodMin;

		public int periodMax;
	}

	private const byte EL_UNDERFLOW = 128;

	private const byte EL_PTT = 16;

	private const byte EL_PAUSE = 8;

	private const byte EL_END = 4;

	private const byte EL_KEYUP = 2;

	private const byte EL_KEYDOWN = 3;

	private const int NKEYS = 120;

	private const int NKPL = 60;

	private const char EMPTY_CODE = '_';

	private IContainer components;

	private Console console;

	public static Mutex keydisplay = new Mutex();

	private int cwxwpm;

	private uint[] mbits = new uint[64];

	private string[] a2m2 = new string[64];

	private bool quit;

	private bool kquit;

	private int pause;

	public static Mutex cwfifo2 = new Mutex();

	private byte[] fifo2 = new byte[32768];

	private int infifo2;

	private int pin2;

	private int pout2;

	public static Mutex cwfifo = new Mutex();

	private byte[] elfifo = new byte[32768];

	private int infifo;

	private int pin;

	private int pout;

	private int tel;

	private int ttx;

	private int ttdel;

	private int tpause;

	private string tqq;

	private bool altkey;

	private int kkk;

	private bool keying;

	private bool ptt;

	private int newptt;

	private int pttdelay;

	private bool pause_checked;

	private bool stopThreads;

	private bool _threadsStarted;

	private int kylx = 12;

	private int kyty = 180;

	private int kyysz = 82;

	private int kyxsz = 665;

	private char[] kbufold = new char[120];

	private char[] kbufnew = new char[120];

	private LabelTS label4;

	private ButtonTS stopButton;

	private LabelTS label5;

	private LabelTS label6;

	private ButtonTS s1;

	private TextBoxTS txt1;

	private ButtonTS s2;

	private TextBoxTS txt2;

	private ButtonTS s3;

	private TextBoxTS txt3;

	private ButtonTS s4;

	private TextBoxTS txt4;

	private ButtonTS s5;

	private TextBoxTS txt5;

	private ButtonTS s6;

	private TextBoxTS txt6;

	private LabelTS speedLabel;

	private ButtonTS notesButton;

	private ComboBoxTS cbMorse;

	private NumericUpDownTS udDelay;

	private LabelTS repeatdelayLabel;

	private TextBoxTS txt7;

	private ButtonTS s7;

	private ButtonTS s8;

	private TextBoxTS txt8;

	private ButtonTS s9;

	private TextBoxTS txt9;

	private LabelTS dropdelaylabel;

	private NumericUpDownTS udDrop;

	private ButtonTS keyButton;

	private LabelTS label7;

	private TextBoxTS txtdummy1;

	private CheckBoxTS chkPause;

	private ButtonTS clearButton;

	private ButtonTS keyboardButton;

	private Panel pttLed;

	private Panel keyLed;

	private ToolTip toolTip1;

	private Panel keyboardLed;

	private ButtonTS expandButton;

	private NumericUpDownTS udPtt;

	private LabelTS pttdelaylabel;

	private CheckBoxTS chkAlwaysOnTop;

	private NumericUpDownTS udWPM;

	private CheckBoxTS chkForceToCWmode;

	private CheckBoxTS chkFocusRequired;

	private ASCIIEncoding AE = new ASCIIEncoding();

	private const int TIMERR_NOERROR = 0;

	private int timerID;

	private TimeProc timeProcPeriodic;

	private bool setptt_memory;

	private bool setkey_memory;

	private string sfile = "morsedef.txt";

	private RingBufferByte rb = new RingBufferByte(2048);

	private volatile bool stopSending;

	private bool _shown;

	private readonly object m_objLock = new object();

	public string editline;

	public int WPM
	{
		get
		{
			return cwxwpm;
		}
		set
		{
			int val = Math.Max((int)udWPM.Minimum, value);
			val = Math.Min((int)udWPM.Maximum, val);
			udWPM.Value = val;
		}
	}

	public int PTTDelayMs
	{
		get
		{
			return pttdelay;
		}
		set
		{
			int val = Math.Max((int)udPtt.Minimum, value);
			val = Math.Min((int)udPtt.Maximum, val);
			udPtt.Value = val;
		}
	}

	public int Characters2Send => infifo;

	public int PendingRemoteCharacters => rb.ReadSpace();

	public int StartQueue
	{
		set
		{
			queue_start(value);
		}
	}

	public bool IsShown => _shown;

	public bool ForceToCWmode => chkForceToCWmode.Checked;

	[DllImport("winmm.dll")]
	private static extern int timeGetDevCaps(ref TimerCaps caps, int sizeOfTimerCaps);

	[DllImport("winmm.dll")]
	private static extern int timeSetEvent(int delay, int resolution, TimeProc proc, int user, int mode);

	[DllImport("winmm.dll")]
	private static extern int timeKillEvent(int id);

	private void setup_timer()
	{
		tel = wpmrate();
		if (timerID != 0)
		{
			timeKillEvent(timerID);
		}
		timerID = timeSetEvent(tel, 1, timeProcPeriodic, 0, 1);
		_ = timerID;
	}

	private void setptt(bool state)
	{
		if (setptt_memory != state)
		{
			ptt = state;
			if (state)
			{
				pttLed.BackColor = Color.Red;
			}
			else
			{
				pttLed.BackColor = Color.Black;
			}
			setptt_memory = state;
		}
	}

	private void setkey(bool state)
	{
		if (setkey_memory != state)
		{
			NetworkIO.SetCWX(Convert.ToInt32(state));
			if (state)
			{
				keyLed.BackColor = Color.Yellow;
			}
			else
			{
				keyLed.BackColor = Color.Black;
			}
			setkey_memory = state;
		}
	}

	private void quitshut()
	{
		clear_fifo();
		clear_fifo2();
		setkey(state: false);
		setptt(state: false);
		ttx = 0;
		pause = 0;
		newptt = 0;
		keying = false;
		NetworkIO.SendHighPriority(1);
	}

	private void clear_fifo()
	{
		cwfifo.WaitOne();
		infifo = 0;
		pin = 0;
		pout = 0;
		cwfifo.ReleaseMutex();
	}

	private void push_fifo(byte data)
	{
		cwfifo.WaitOne();
		elfifo.SetValue(data, pin);
		pin++;
		if (pin >= elfifo.Length)
		{
			pin = 0;
		}
		infifo++;
		cwfifo.ReleaseMutex();
	}

	private byte pop_fifo()
	{
		byte result;
		if (infifo < 1)
		{
			result = 128;
		}
		else
		{
			cwfifo.WaitOne();
			result = (byte)elfifo.GetValue(pout);
			pout++;
			if (pout >= elfifo.Length)
			{
				pout = 0;
			}
			infifo--;
			cwfifo.ReleaseMutex();
		}
		return result;
	}

	private void clear_fifo2()
	{
		cwfifo2.WaitOne();
		infifo2 = 0;
		pin2 = 0;
		pout2 = 0;
		cwfifo2.ReleaseMutex();
	}

	private void push_fifo2(byte data)
	{
		cwfifo2.WaitOne();
		fifo2.SetValue(data, pin2);
		pin2++;
		if (pin2 >= fifo2.Length)
		{
			pin2 = 0;
		}
		infifo2++;
		cwfifo2.ReleaseMutex();
	}

	private byte pop_fifo2()
	{
		byte result;
		if (infifo2 < 1)
		{
			result = 128;
		}
		else
		{
			cwfifo2.WaitOne();
			result = (byte)fifo2.GetValue(pout2);
			pout2++;
			if (pout2 >= fifo2.Length)
			{
				pout2 = 0;
			}
			infifo2--;
			cwfifo2.ReleaseMutex();
		}
		return result;
	}

	private int wpmrate()
	{
		return 1200 / cwxwpm;
	}

	private void help()
	{
		MessageBox.Show(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("                  Memory and Keyboard Keyer Notes\n" + "\n", "Radio must be running in a valid cw mode and frequency.\n"), "Speed is for this form only (may change later).\n"), "PTT Delay (ms) is time from PTT to first key down.\n"), "Drop Delay (ms) is time for radio to drop out of transmit when keying stops.\n"), "Drop Delay cannot be set less than PTT Delay * 1.5 and should\n"), "be kept high enough so that PTT does not drop out between words.\n"), "Note that weight settings on Setup Form do not affect CWX at this time.\n"), "\n"), "Messages may be edited while sending but take effect after restarting the message.\n"), "\n"), "\n"), "In the keyer memories special characters are:\n\n"), "    # sends a long dash a bit longer than a zero\n"), "    $ send a long space as above\n"), "    \" at the message end means loop it continuously\n"), "      and Repeat Delay sets time between repeats in seconds\n"), "\n"), "    + is AR        ( is KN         * is SK\n"), "    ! is SN        = is BT         \\ is BK\n\n"), "The other specials can be changed in morsedef.txt.\n"), "They are & ' ) : ; < > [  ] and ^\n"), "All others without regular Morse defs send a space.\n"), "\nPress button in lower right corner to show keyboard and more memories.\n\n"), "Keyboard button must have the focus for characters\n"), "to be entered in the keyboard buffer. Cyan indicator on.\n\n"), "Keyboard sending will pause if the checkbox is checked.\n"), "F1 flips the pause state.  F2 clears the keyboard.\n"), "Alt 1 thru Alt 9 will load memories 1 to 9 into keyboard buffer.\n"), "Right click on a message button will also load it into the keyboard\n"), "\n"), "Esc stops any output and clears the keyboard buffer.\n"), "\n"), "\n"), "    << Sugar Land, Texas 2006-02-16 - Richard Allen, W5SXD >>\n"), "  CWX Notes ...");
	}

	private void notesButton_Click(object sender, EventArgs e)
	{
		Thread thread = new Thread(help);
		thread.Name = "help thread";
		thread.IsBackground = true;
		thread.Priority = ThreadPriority.Normal;
		thread.Start();
	}

	private void build_mbits2()
	{
		for (int i = 0; i < 64; i++)
		{
			uint num = 0u;
			uint num2 = 0u;
			uint num3 = 2147483648u;
			string text = ((string)a2m2.GetValue(i)).Substring(5, 9);
			int length = text.Length;
			for (int j = 0; j < length; j++)
			{
				if (string.CompareOrdinal(text, j, "-", 0, 1) == 0)
				{
					num += 4;
					num2 |= num3;
					num3 >>= 1;
					num2 |= num3;
					num3 >>= 1;
					num2 |= num3;
					num3 >>= 1;
					num3 >>= 1;
				}
				else if (string.CompareOrdinal(text, j, ".", 0, 1) == 0)
				{
					num += 2;
					num2 |= num3;
					num3 >>= 1;
					num3 >>= 1;
				}
			}
			num = ((i != 0) ? (num + 2) : 4u);
			num2 &= 0xFFFFFFE0u;
			num2 += num & 0x1F;
			mbits.SetValue(num2, i);
		}
	}

	private void load_alpha()
	{
		if (!File.Exists(console.AppDataPath + sfile))
		{
			using StreamWriter streamWriter = new StreamWriter(console.AppDataPath + sfile);
			streamWriter.WriteLine("32| |*        | space     ");
			streamWriter.WriteLine("33|!|...-.    | [SN]      ");
			streamWriter.WriteLine("34|\"|*        | loop      ");
			streamWriter.WriteLine("35|#|*        | long dash ");
			streamWriter.WriteLine("36|$|*        | long space");
			streamWriter.WriteLine("37|%|.-...    | [AS]      ");
			streamWriter.WriteLine("38|&|.........| 0123456789");
			streamWriter.WriteLine("39|'|         |           ");
			streamWriter.WriteLine("40|(|-.--.    | [KN]      ");
			streamWriter.WriteLine("41|)|         |           ");
			streamWriter.WriteLine("42|*|...-.-   | [SK]      ");
			streamWriter.WriteLine("43|+|.-.-.    | [AR]      ");
			streamWriter.WriteLine("44|,|--..--   |           ");
			streamWriter.WriteLine("45|-|-....-   |           ");
			streamWriter.WriteLine("46|.|.-.-.-   |           ");
			streamWriter.WriteLine("47|/|-..-.    |           ");
			streamWriter.WriteLine("48|0|-----    |           ");
			streamWriter.WriteLine("49|1|.----    |           ");
			streamWriter.WriteLine("50|2|..---    |           ");
			streamWriter.WriteLine("51|3|...--    |           ");
			streamWriter.WriteLine("52|4|....-    |           ");
			streamWriter.WriteLine("53|5|.....    |           ");
			streamWriter.WriteLine("54|6|-....    |           ");
			streamWriter.WriteLine("55|7|--...    |           ");
			streamWriter.WriteLine("56|8|---..    |           ");
			streamWriter.WriteLine("57|9|----.    |           ");
			streamWriter.WriteLine("58|:|         |           ");
			streamWriter.WriteLine("59|;|         |           ");
			streamWriter.WriteLine("60|<|         |           ");
			streamWriter.WriteLine("61|=|-...-    | [BT]      ");
			streamWriter.WriteLine("62|>|         |           ");
			streamWriter.WriteLine("63|?|..--..   |           ");
			streamWriter.WriteLine("64|@|.--.-.   |           ");
			streamWriter.WriteLine("65|A|.-       |           ");
			streamWriter.WriteLine("66|B|-...     |           ");
			streamWriter.WriteLine("67|C|-.-.     |           ");
			streamWriter.WriteLine("68|D|-..      |           ");
			streamWriter.WriteLine("69|E|.        |           ");
			streamWriter.WriteLine("70|F|..-.     |           ");
			streamWriter.WriteLine("71|G|--.      |           ");
			streamWriter.WriteLine("72|H|....     |           ");
			streamWriter.WriteLine("73|I|..       |           ");
			streamWriter.WriteLine("74|J|.---     |           ");
			streamWriter.WriteLine("75|K|-.-      |           ");
			streamWriter.WriteLine("76|L|.-..     |           ");
			streamWriter.WriteLine("77|M|--       |           ");
			streamWriter.WriteLine("78|N|-.       |           ");
			streamWriter.WriteLine("79|O|---      |           ");
			streamWriter.WriteLine("80|P|.--.     |           ");
			streamWriter.WriteLine("81|Q|--.-     |           ");
			streamWriter.WriteLine("82|R|.-.      |           ");
			streamWriter.WriteLine("83|S|...      |           ");
			streamWriter.WriteLine("84|T|-        |           ");
			streamWriter.WriteLine("85|U|..-      |           ");
			streamWriter.WriteLine("86|V|...-     |           ");
			streamWriter.WriteLine("87|W|.--      |           ");
			streamWriter.WriteLine("88|X|-..-     |           ");
			streamWriter.WriteLine("89|Y|-.--     |           ");
			streamWriter.WriteLine("90|Z|--..     |           ");
			streamWriter.WriteLine("91|[|         |           ");
			streamWriter.WriteLine("92|\\|-...-.-  | [BK]      ");
			streamWriter.WriteLine("93|]|         |           ");
			streamWriter.WriteLine("94|^|         |           ");
			streamWriter.WriteLine("95|_|*        | reserved  ");
		}
		using StreamReader streamReader = new StreamReader(console.AppDataPath + sfile);
		int num = 0;
		cbMorse.Items.Clear();
		string text;
		while ((text = streamReader.ReadLine()) != null)
		{
			cbMorse.Items.Add(text);
			a2m2.SetValue(text, num);
			num++;
		}
		if (num != 64)
		{
			MessageBox.Show(string.Concat(sfile + " has incorrect length and may be corrupt\n", "delete it and let it be rebuilt ..."));
		}
	}

	public string RemoteMessage(byte[] msg)
	{
		rb.Write(msg, msg.Length);
		return "";
	}

	public string RemoteMessage(char msg)
	{
		loadchar(msg);
		return "";
	}

	private void SendBufferMessage()
	{
		while (true)
		{
			Thread.Sleep(10);
			if (stopSending)
			{
				continue;
			}
			if (rb.ReadSpace() > 0)
			{
				byte[] array = new byte[1];
				rb.Read(array, array.Length);
				char cc = (char)array[0];
				loadchar(cc);
				console?.CWXRemoteCharacterStartedHandlers?.Invoke(rb.ReadSpace(), infifo);
				while (infifo > 2)
				{
					Thread.Sleep(2);
				}
			}
			Thread.Sleep(2);
		}
	}

	public void CWXStop()
	{
		stopSending = true;
		rb.Reset();
		stopSending = false;
	}

	public void AbortSending()
	{
		stopSending = true;
		rb.Reset();
		clear_show();
		quit = true;
		kquit = true;
		quitshut();
		stopSending = false;
	}

	public CWX(Console c)
	{
		InitializeComponent();
		console = c;
		c.GlobalKeyPressDownHandlers = (Console.GlobalKeyPress)Delegate.Combine(c.GlobalKeyPressDownHandlers, new Console.GlobalKeyPress(onGlobalKeyDown));
		c.GlobalKeyPressUpHandlers = (Console.GlobalKeyPress)Delegate.Combine(c.GlobalKeyPressUpHandlers, new Console.GlobalKeyPress(onGlobalKeyUp));
		txtdummy1.Hide();
		clear_keys();
		txt1.Text = "### test de w5sxd/b el29ep.$$\"";
		txt2.Text = "cq cq test w5sxd test";
		txt3.Text = "5nn stx";
		txt4.Text = "k5sdr de w5sxd (";
		txt5.Text = "cq cq cq de w5sxd w5sxd w5sxd +k";
		txt6.Text = "The quick brown fox jumped over the lazy dog. 0123456789 ";
		txt7.Text = "?";
		txt8.Text = "agn";
		txt9.Text = "n6vs";
		Common.RestoreForm(this, "CWX", restore_size: true);
		cwxwpm = (int)udWPM.Value;
		tpause = (int)udDelay.Value * 1000;
		if (tpause < 1)
		{
			tpause = tel;
		}
		ttdel = (int)udDrop.Value;
		pttdelay = (int)udPtt.Value;
		timeProcPeriodic = TimerPeriodicEventCallback;
		setup_timer();
		load_alpha();
		build_mbits2();
		stopThreads = false;
		_threadsStarted = false;
		startThreads();
		Thread thread = new Thread(SendBufferMessage);
		thread.Name = "CAT Read Thread";
		thread.IsBackground = true;
		thread.Priority = ThreadPriority.Highest;
		thread.Start();
	}

	private void startThreads()
	{
		if (!stopThreads && !_threadsStarted)
		{
			Thread thread = new Thread(keyboardFifo);
			thread.Name = "keyboard fifo pop thread";
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.Normal;
			thread.Start();
			Thread thread2 = new Thread(keyboardDisplay);
			thread2.Name = "keyboard edit box handler thread";
			thread2.IsBackground = true;
			thread2.Priority = ThreadPriority.Normal;
			thread2.Start();
			_threadsStarted = true;
		}
	}

	protected override void Dispose(bool disposing)
	{
		timeKillEvent(timerID);
		if (disposing)
		{
			if (console != null)
			{
				Console obj = console;
				obj.GlobalKeyPressDownHandlers = (Console.GlobalKeyPress)Delegate.Remove(obj.GlobalKeyPressDownHandlers, new Console.GlobalKeyPress(onGlobalKeyDown));
				Console obj2 = console;
				obj2.GlobalKeyPressUpHandlers = (Console.GlobalKeyPress)Delegate.Remove(obj2.GlobalKeyPressUpHandlers, new Console.GlobalKeyPress(onGlobalKeyUp));
			}
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.CWX));
		this.pttLed = new System.Windows.Forms.Panel();
		this.keyLed = new System.Windows.Forms.Panel();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		this.keyboardLed = new System.Windows.Forms.Panel();
		this.pttdelaylabel = new System.Windows.Forms.LabelTS();
		this.expandButton = new System.Windows.Forms.ButtonTS();
		this.keyboardButton = new System.Windows.Forms.ButtonTS();
		this.clearButton = new System.Windows.Forms.ButtonTS();
		this.chkPause = new System.Windows.Forms.CheckBoxTS();
		this.txt9 = new System.Windows.Forms.TextBoxTS();
		this.txt8 = new System.Windows.Forms.TextBoxTS();
		this.txt7 = new System.Windows.Forms.TextBoxTS();
		this.txt6 = new System.Windows.Forms.TextBoxTS();
		this.txt5 = new System.Windows.Forms.TextBoxTS();
		this.txt4 = new System.Windows.Forms.TextBoxTS();
		this.txt3 = new System.Windows.Forms.TextBoxTS();
		this.txt2 = new System.Windows.Forms.TextBoxTS();
		this.txt1 = new System.Windows.Forms.TextBoxTS();
		this.keyButton = new System.Windows.Forms.ButtonTS();
		this.dropdelaylabel = new System.Windows.Forms.LabelTS();
		this.s9 = new System.Windows.Forms.ButtonTS();
		this.s8 = new System.Windows.Forms.ButtonTS();
		this.s7 = new System.Windows.Forms.ButtonTS();
		this.stopButton = new System.Windows.Forms.ButtonTS();
		this.repeatdelayLabel = new System.Windows.Forms.LabelTS();
		this.cbMorse = new System.Windows.Forms.ComboBoxTS();
		this.notesButton = new System.Windows.Forms.ButtonTS();
		this.speedLabel = new System.Windows.Forms.LabelTS();
		this.s6 = new System.Windows.Forms.ButtonTS();
		this.s5 = new System.Windows.Forms.ButtonTS();
		this.s4 = new System.Windows.Forms.ButtonTS();
		this.s3 = new System.Windows.Forms.ButtonTS();
		this.s2 = new System.Windows.Forms.ButtonTS();
		this.s1 = new System.Windows.Forms.ButtonTS();
		this.chkForceToCWmode = new System.Windows.Forms.CheckBoxTS();
		this.chkAlwaysOnTop = new System.Windows.Forms.CheckBoxTS();
		this.udWPM = new System.Windows.Forms.NumericUpDownTS();
		this.udPtt = new System.Windows.Forms.NumericUpDownTS();
		this.txtdummy1 = new System.Windows.Forms.TextBoxTS();
		this.label7 = new System.Windows.Forms.LabelTS();
		this.udDrop = new System.Windows.Forms.NumericUpDownTS();
		this.label6 = new System.Windows.Forms.LabelTS();
		this.label5 = new System.Windows.Forms.LabelTS();
		this.label4 = new System.Windows.Forms.LabelTS();
		this.udDelay = new System.Windows.Forms.NumericUpDownTS();
		this.chkFocusRequired = new System.Windows.Forms.CheckBoxTS();
		((System.ComponentModel.ISupportInitialize)this.udWPM).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udPtt).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udDrop).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udDelay).BeginInit();
		base.SuspendLayout();
		this.pttLed.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pttLed.Location = new System.Drawing.Point(10, 8);
		this.pttLed.Name = "pttLed";
		this.pttLed.Size = new System.Drawing.Size(24, 13);
		this.pttLed.TabIndex = 49;
		this.toolTip1.SetToolTip(this.pttLed, " PTT status");
		this.keyLed.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.keyLed.Location = new System.Drawing.Point(10, 24);
		this.keyLed.Name = "keyLed";
		this.keyLed.Size = new System.Drawing.Size(24, 13);
		this.keyLed.TabIndex = 50;
		this.toolTip1.SetToolTip(this.keyLed, "Key status");
		this.keyboardLed.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.keyboardLed.Location = new System.Drawing.Point(344, 157);
		this.keyboardLed.Name = "keyboardLed";
		this.keyboardLed.Size = new System.Drawing.Size(24, 13);
		this.keyboardLed.TabIndex = 52;
		this.toolTip1.SetToolTip(this.keyboardLed, " Keyboard active indicator.");
		this.pttdelaylabel.Image = null;
		this.pttdelaylabel.Location = new System.Drawing.Point(456, 32);
		this.pttdelaylabel.Name = "pttdelaylabel";
		this.pttdelaylabel.Size = new System.Drawing.Size(64, 16);
		this.pttdelaylabel.TabIndex = 55;
		this.pttdelaylabel.Text = "PTT Delay";
		this.toolTip1.SetToolTip(this.pttdelaylabel, "Set delay from PTT to key down in milliseconds.");
		this.pttdelaylabel.Visible = false;
		this.expandButton.BackColor = System.Drawing.Color.RoyalBlue;
		this.expandButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.expandButton.Image = null;
		this.expandButton.Location = new System.Drawing.Point(688, 266);
		this.expandButton.Name = "expandButton";
		this.expandButton.Selectable = true;
		this.expandButton.Size = new System.Drawing.Size(8, 8);
		this.expandButton.TabIndex = 53;
		this.toolTip1.SetToolTip(this.expandButton, "Contract Form");
		this.expandButton.UseVisualStyleBackColor = false;
		this.expandButton.Click += new System.EventHandler(expandButton_Click);
		this.keyboardButton.Cursor = System.Windows.Forms.Cursors.Hand;
		this.keyboardButton.Image = null;
		this.keyboardButton.Location = new System.Drawing.Point(216, 152);
		this.keyboardButton.Name = "keyboardButton";
		this.keyboardButton.Selectable = true;
		this.keyboardButton.Size = new System.Drawing.Size(112, 23);
		this.keyboardButton.TabIndex = 45;
		this.keyboardButton.Text = "Keyboard";
		this.toolTip1.SetToolTip(this.keyboardButton, " Enable keyboard.  This must be selected for keyboard to work.");
		this.keyboardButton.Enter += new System.EventHandler(keyboardButton_Enter);
		this.keyboardButton.KeyPress += new System.Windows.Forms.KeyPressEventHandler(keyboardButton_KeyPress);
		this.keyboardButton.Leave += new System.EventHandler(keyboardButton_Leave);
		this.clearButton.Image = null;
		this.clearButton.Location = new System.Drawing.Point(120, 152);
		this.clearButton.Name = "clearButton";
		this.clearButton.Selectable = true;
		this.clearButton.Size = new System.Drawing.Size(75, 23);
		this.clearButton.TabIndex = 46;
		this.clearButton.Text = "Clear (F12)";
		this.toolTip1.SetToolTip(this.clearButton, " Clear the keyboard buffer.");
		this.clearButton.Click += new System.EventHandler(clearButton_Click);
		this.chkPause.Image = null;
		this.chkPause.Location = new System.Drawing.Point(16, 152);
		this.chkPause.Name = "chkPause";
		this.chkPause.Size = new System.Drawing.Size(98, 20);
		this.chkPause.TabIndex = 43;
		this.chkPause.Text = "Pause (F11)";
		this.toolTip1.SetToolTip(this.chkPause, " Pause keyboard transmission.");
		this.chkPause.CheckedChanged += new System.EventHandler(chkPause_CheckedChanged);
		this.txt9.Location = new System.Drawing.Point(495, 120);
		this.txt9.Name = "txt9";
		this.txt9.Size = new System.Drawing.Size(176, 20);
		this.txt9.TabIndex = 34;
		this.toolTip1.SetToolTip(this.txt9, "Message edit box.");
		this.txt8.Location = new System.Drawing.Point(495, 88);
		this.txt8.Name = "txt8";
		this.txt8.Size = new System.Drawing.Size(176, 20);
		this.txt8.TabIndex = 32;
		this.toolTip1.SetToolTip(this.txt8, "Message edit box.");
		this.txt7.Location = new System.Drawing.Point(495, 56);
		this.txt7.Name = "txt7";
		this.txt7.Size = new System.Drawing.Size(176, 20);
		this.txt7.TabIndex = 29;
		this.toolTip1.SetToolTip(this.txt7, "Message edit box.");
		this.txt6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txt6.Location = new System.Drawing.Point(272, 121);
		this.txt6.Name = "txt6";
		this.txt6.Size = new System.Drawing.Size(175, 20);
		this.txt6.TabIndex = 13;
		this.toolTip1.SetToolTip(this.txt6, "Message edit box.");
		this.txt5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txt5.Location = new System.Drawing.Point(271, 88);
		this.txt5.Name = "txt5";
		this.txt5.Size = new System.Drawing.Size(176, 20);
		this.txt5.TabIndex = 11;
		this.toolTip1.SetToolTip(this.txt5, "Message edit box.");
		this.txt4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txt4.Location = new System.Drawing.Point(271, 56);
		this.txt4.Name = "txt4";
		this.txt4.Size = new System.Drawing.Size(176, 20);
		this.txt4.TabIndex = 9;
		this.toolTip1.SetToolTip(this.txt4, "Message edit box.");
		this.txt3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txt3.Location = new System.Drawing.Point(48, 120);
		this.txt3.Name = "txt3";
		this.txt3.Size = new System.Drawing.Size(172, 20);
		this.txt3.TabIndex = 7;
		this.toolTip1.SetToolTip(this.txt3, "Message edit box.");
		this.txt2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txt2.Location = new System.Drawing.Point(47, 88);
		this.txt2.Name = "txt2";
		this.txt2.Size = new System.Drawing.Size(172, 20);
		this.txt2.TabIndex = 5;
		this.toolTip1.SetToolTip(this.txt2, "Message edit box.");
		this.txt1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txt1.Location = new System.Drawing.Point(47, 56);
		this.txt1.Name = "txt1";
		this.txt1.Size = new System.Drawing.Size(172, 20);
		this.txt1.TabIndex = 3;
		this.txt1.Text = "cq cq test w5sxd test";
		this.toolTip1.SetToolTip(this.txt1, "Message edit box.");
		this.keyButton.Image = null;
		this.keyButton.Location = new System.Drawing.Point(128, 8);
		this.keyButton.Name = "keyButton";
		this.keyButton.Selectable = true;
		this.keyButton.Size = new System.Drawing.Size(40, 24);
		this.keyButton.TabIndex = 37;
		this.keyButton.Text = "Key";
		this.toolTip1.SetToolTip(this.keyButton, "Turn on transmitter and key it. (60 second timeout)");
		this.keyButton.Click += new System.EventHandler(keyButton_Click);
		this.dropdelaylabel.Image = null;
		this.dropdelaylabel.Location = new System.Drawing.Point(384, 32);
		this.dropdelaylabel.Name = "dropdelaylabel";
		this.dropdelaylabel.Size = new System.Drawing.Size(64, 16);
		this.dropdelaylabel.TabIndex = 36;
		this.dropdelaylabel.Text = "Drop Delay";
		this.toolTip1.SetToolTip(this.dropdelaylabel, " Set break in drop out in milliseconds. Minimum allowed is PTT Delay * 1.5 .");
		this.dropdelaylabel.Visible = false;
		this.s9.Image = null;
		this.s9.Location = new System.Drawing.Point(456, 120);
		this.s9.Name = "s9";
		this.s9.Selectable = true;
		this.s9.Size = new System.Drawing.Size(33, 20);
		this.s9.TabIndex = 33;
		this.s9.Text = "F9";
		this.toolTip1.SetToolTip(this.s9, "Start message 9.");
		this.s9.Click += new System.EventHandler(s9_Click);
		this.s9.MouseDown += new System.Windows.Forms.MouseEventHandler(s9_MouseDown);
		this.s8.Image = null;
		this.s8.Location = new System.Drawing.Point(456, 88);
		this.s8.Name = "s8";
		this.s8.Selectable = true;
		this.s8.Size = new System.Drawing.Size(33, 20);
		this.s8.TabIndex = 31;
		this.s8.Text = "F8";
		this.toolTip1.SetToolTip(this.s8, "Start message 8.");
		this.s8.Click += new System.EventHandler(s8_Click);
		this.s8.MouseDown += new System.Windows.Forms.MouseEventHandler(s8_MouseDown);
		this.s7.Image = null;
		this.s7.Location = new System.Drawing.Point(456, 56);
		this.s7.Name = "s7";
		this.s7.Selectable = true;
		this.s7.Size = new System.Drawing.Size(33, 20);
		this.s7.TabIndex = 30;
		this.s7.Text = "F7";
		this.toolTip1.SetToolTip(this.s7, "Start message 7.");
		this.s7.Click += new System.EventHandler(s7_Click);
		this.s7.MouseDown += new System.Windows.Forms.MouseEventHandler(s7_MouseDown);
		this.stopButton.Image = null;
		this.stopButton.Location = new System.Drawing.Point(48, 8);
		this.stopButton.Name = "stopButton";
		this.stopButton.Selectable = true;
		this.stopButton.Size = new System.Drawing.Size(72, 24);
		this.stopButton.TabIndex = 26;
		this.stopButton.Text = "Stop (Esc)";
		this.toolTip1.SetToolTip(this.stopButton, "Stop all keying.");
		this.stopButton.Click += new System.EventHandler(stopButton_Click);
		this.repeatdelayLabel.Image = null;
		this.repeatdelayLabel.Location = new System.Drawing.Point(304, 32);
		this.repeatdelayLabel.Name = "repeatdelayLabel";
		this.repeatdelayLabel.Size = new System.Drawing.Size(80, 16);
		this.repeatdelayLabel.TabIndex = 48;
		this.repeatdelayLabel.Text = "Repeat Delay";
		this.toolTip1.SetToolTip(this.repeatdelayLabel, " Set repeat message delay in seconds.");
		this.cbMorse.Cursor = System.Windows.Forms.Cursors.Default;
		this.cbMorse.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cbMorse.DropDownWidth = 208;
		this.cbMorse.Font = new System.Drawing.Font("Courier New", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.cbMorse.Location = new System.Drawing.Point(472, 152);
		this.cbMorse.Name = "cbMorse";
		this.cbMorse.Size = new System.Drawing.Size(208, 23);
		this.cbMorse.TabIndex = 19;
		this.toolTip1.SetToolTip(this.cbMorse, "View and right click to edit Morse definition table.");
		this.cbMorse.SelectedIndexChanged += new System.EventHandler(CbMorse_SelectedIndexChanged);
		this.cbMorse.MouseDown += new System.Windows.Forms.MouseEventHandler(cbMorse_MouseDown);
		this.notesButton.Image = null;
		this.notesButton.Location = new System.Drawing.Point(176, 8);
		this.notesButton.Name = "notesButton";
		this.notesButton.Selectable = true;
		this.notesButton.Size = new System.Drawing.Size(48, 24);
		this.notesButton.TabIndex = 17;
		this.notesButton.Text = "Notes";
		this.toolTip1.SetToolTip(this.notesButton, "Show program notes.");
		this.notesButton.Click += new System.EventHandler(notesButton_Click);
		this.speedLabel.Image = null;
		this.speedLabel.Location = new System.Drawing.Point(232, 32);
		this.speedLabel.Name = "speedLabel";
		this.speedLabel.Size = new System.Drawing.Size(72, 16);
		this.speedLabel.TabIndex = 15;
		this.speedLabel.Text = "Speed WPM";
		this.toolTip1.SetToolTip(this.speedLabel, " Set memory keyer (not paddle) speed in words per minute. (PARIS method)");
		this.s6.Image = null;
		this.s6.Location = new System.Drawing.Point(232, 120);
		this.s6.Name = "s6";
		this.s6.Selectable = true;
		this.s6.Size = new System.Drawing.Size(33, 20);
		this.s6.TabIndex = 14;
		this.s6.Text = "F6";
		this.toolTip1.SetToolTip(this.s6, "Start message 6.");
		this.s6.Click += new System.EventHandler(s6_Click);
		this.s6.MouseDown += new System.Windows.Forms.MouseEventHandler(s6_MouseDown);
		this.s5.Image = null;
		this.s5.Location = new System.Drawing.Point(232, 88);
		this.s5.Name = "s5";
		this.s5.Selectable = true;
		this.s5.Size = new System.Drawing.Size(33, 20);
		this.s5.TabIndex = 12;
		this.s5.Text = "F5";
		this.toolTip1.SetToolTip(this.s5, "Start message 5.");
		this.s5.Click += new System.EventHandler(s5_Click);
		this.s5.MouseDown += new System.Windows.Forms.MouseEventHandler(s5_MouseDown);
		this.s4.Image = null;
		this.s4.Location = new System.Drawing.Point(232, 56);
		this.s4.Name = "s4";
		this.s4.Selectable = true;
		this.s4.Size = new System.Drawing.Size(33, 20);
		this.s4.TabIndex = 10;
		this.s4.Text = "F4";
		this.toolTip1.SetToolTip(this.s4, "Start message 4.");
		this.s4.Click += new System.EventHandler(s4_Click);
		this.s4.MouseDown += new System.Windows.Forms.MouseEventHandler(s4_MouseDown);
		this.s3.Image = null;
		this.s3.Location = new System.Drawing.Point(8, 120);
		this.s3.Name = "s3";
		this.s3.Selectable = true;
		this.s3.Size = new System.Drawing.Size(33, 20);
		this.s3.TabIndex = 8;
		this.s3.Text = "F3";
		this.toolTip1.SetToolTip(this.s3, "Start message 3.");
		this.s3.Click += new System.EventHandler(s3_Click);
		this.s3.MouseDown += new System.Windows.Forms.MouseEventHandler(s3_MouseDown);
		this.s2.Image = null;
		this.s2.Location = new System.Drawing.Point(8, 88);
		this.s2.Name = "s2";
		this.s2.Selectable = true;
		this.s2.Size = new System.Drawing.Size(33, 20);
		this.s2.TabIndex = 6;
		this.s2.Text = "F2";
		this.toolTip1.SetToolTip(this.s2, "Start message 2.");
		this.s2.Click += new System.EventHandler(s2_Click);
		this.s2.MouseDown += new System.Windows.Forms.MouseEventHandler(s2_MouseDown);
		this.s1.Image = null;
		this.s1.Location = new System.Drawing.Point(8, 56);
		this.s1.Name = "s1";
		this.s1.Selectable = true;
		this.s1.Size = new System.Drawing.Size(34, 20);
		this.s1.TabIndex = 4;
		this.s1.Text = "F1";
		this.toolTip1.SetToolTip(this.s1, "Start message 1.");
		this.s1.Click += new System.EventHandler(s1_Click);
		this.s1.MouseDown += new System.Windows.Forms.MouseEventHandler(s1_MouseDown);
		this.chkForceToCWmode.AutoSize = true;
		this.chkForceToCWmode.Checked = true;
		this.chkForceToCWmode.CheckState = System.Windows.Forms.CheckState.Checked;
		this.chkForceToCWmode.Image = null;
		this.chkForceToCWmode.Location = new System.Drawing.Point(528, 32);
		this.chkForceToCWmode.Name = "chkForceToCWmode";
		this.chkForceToCWmode.Size = new System.Drawing.Size(115, 17);
		this.chkForceToCWmode.TabIndex = 58;
		this.chkForceToCWmode.Text = "Force to CW mode";
		this.chkForceToCWmode.UseVisualStyleBackColor = true;
		this.chkAlwaysOnTop.Image = null;
		this.chkAlwaysOnTop.Location = new System.Drawing.Point(528, 8);
		this.chkAlwaysOnTop.Name = "chkAlwaysOnTop";
		this.chkAlwaysOnTop.Size = new System.Drawing.Size(104, 24);
		this.chkAlwaysOnTop.TabIndex = 57;
		this.chkAlwaysOnTop.Text = "Always On Top";
		this.chkAlwaysOnTop.CheckedChanged += new System.EventHandler(chkAlwaysOnTop_CheckedChanged);
		this.udWPM.ImeMode = System.Windows.Forms.ImeMode.NoControl;
		this.udWPM.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udWPM.Location = new System.Drawing.Point(240, 8);
		this.udWPM.Maximum = new decimal(new int[4] { 99, 0, 0, 0 });
		this.udWPM.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udWPM.Name = "udWPM";
		this.udWPM.Size = new System.Drawing.Size(56, 20);
		this.udWPM.TabIndex = 56;
		this.udWPM.TinyStep = false;
		this.udWPM.Value = new decimal(new int[4] { 22, 0, 0, 0 });
		this.udWPM.ValueChanged += new System.EventHandler(udWPM_ValueChanged);
		this.udWPM.LostFocus += new System.EventHandler(udWPM_LostFocus);
		this.udPtt.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udPtt.Location = new System.Drawing.Point(456, 8);
		this.udPtt.Maximum = new decimal(new int[4] { 2000, 0, 0, 0 });
		this.udPtt.Minimum = new decimal(new int[4] { 50, 0, 0, 0 });
		this.udPtt.Name = "udPtt";
		this.udPtt.Size = new System.Drawing.Size(56, 20);
		this.udPtt.TabIndex = 54;
		this.udPtt.TinyStep = false;
		this.udPtt.Value = new decimal(new int[4] { 50, 0, 0, 0 });
		this.udPtt.Visible = false;
		this.udPtt.ValueChanged += new System.EventHandler(udPtt_ValueChanged);
		this.udPtt.LostFocus += new System.EventHandler(udPtt_LostFocus);
		this.txtdummy1.Location = new System.Drawing.Point(12, 180);
		this.txtdummy1.Multiline = true;
		this.txtdummy1.Name = "txtdummy1";
		this.txtdummy1.Size = new System.Drawing.Size(665, 82);
		this.txtdummy1.TabIndex = 42;
		this.txtdummy1.Text = "the actual text box will be a graphic here and this one disabled";
		this.label7.Image = null;
		this.label7.Location = new System.Drawing.Point(376, 352);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(256, 32);
		this.label7.TabIndex = 47;
		this.label7.Text = "label7";
		this.udDrop.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udDrop.Location = new System.Drawing.Point(384, 8);
		this.udDrop.Maximum = new decimal(new int[4] { 5000, 0, 0, 0 });
		this.udDrop.Minimum = new decimal(new int[4]);
		this.udDrop.Name = "udDrop";
		this.udDrop.Size = new System.Drawing.Size(56, 20);
		this.udDrop.TabIndex = 35;
		this.udDrop.TinyStep = false;
		this.udDrop.Value = new decimal(new int[4] { 300, 0, 0, 0 });
		this.udDrop.Visible = false;
		this.udDrop.ValueChanged += new System.EventHandler(udDrop_ValueChanged);
		this.udDrop.LostFocus += new System.EventHandler(udDrop_LostFocus);
		this.label6.Image = null;
		this.label6.Location = new System.Drawing.Point(56, 352);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(256, 32);
		this.label6.TabIndex = 28;
		this.label6.Text = "label6";
		this.label5.Image = null;
		this.label5.Location = new System.Drawing.Point(376, 304);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(256, 32);
		this.label5.TabIndex = 27;
		this.label5.Text = "label5";
		this.label4.Image = null;
		this.label4.Location = new System.Drawing.Point(56, 304);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(256, 32);
		this.label4.TabIndex = 25;
		this.label4.Text = "label4";
		this.udDelay.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udDelay.Location = new System.Drawing.Point(312, 8);
		this.udDelay.Maximum = new decimal(new int[4] { 3600, 0, 0, 0 });
		this.udDelay.Minimum = new decimal(new int[4]);
		this.udDelay.Name = "udDelay";
		this.udDelay.Size = new System.Drawing.Size(56, 20);
		this.udDelay.TabIndex = 20;
		this.udDelay.TinyStep = false;
		this.udDelay.Value = new decimal(new int[4] { 3, 0, 0, 0 });
		this.udDelay.ValueChanged += new System.EventHandler(udDelay_ValueChanged);
		this.udDelay.LostFocus += new System.EventHandler(udDelay_LostFocus);
		this.chkFocusRequired.AutoSize = true;
		this.chkFocusRequired.Checked = true;
		this.chkFocusRequired.CheckState = System.Windows.Forms.CheckState.Checked;
		this.chkFocusRequired.Image = null;
		this.chkFocusRequired.Location = new System.Drawing.Point(637, 12);
		this.chkFocusRequired.Name = "chkFocusRequired";
		this.chkFocusRequired.Size = new System.Drawing.Size(55, 17);
		this.chkFocusRequired.TabIndex = 59;
		this.chkFocusRequired.Text = "Focus";
		this.toolTip1.SetToolTip(this.chkFocusRequired, "Window focus is needed for Fn keys and Alt Digits");
		this.chkFocusRequired.UseVisualStyleBackColor = true;
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		base.ClientSize = new System.Drawing.Size(704, 281);
		base.Controls.Add(this.chkFocusRequired);
		base.Controls.Add(this.chkForceToCWmode);
		base.Controls.Add(this.chkAlwaysOnTop);
		base.Controls.Add(this.udWPM);
		base.Controls.Add(this.pttdelaylabel);
		base.Controls.Add(this.udPtt);
		base.Controls.Add(this.expandButton);
		base.Controls.Add(this.keyboardLed);
		base.Controls.Add(this.keyLed);
		base.Controls.Add(this.pttLed);
		base.Controls.Add(this.keyboardButton);
		base.Controls.Add(this.clearButton);
		base.Controls.Add(this.chkPause);
		base.Controls.Add(this.txtdummy1);
		base.Controls.Add(this.txt9);
		base.Controls.Add(this.txt8);
		base.Controls.Add(this.txt7);
		base.Controls.Add(this.txt6);
		base.Controls.Add(this.txt5);
		base.Controls.Add(this.txt4);
		base.Controls.Add(this.txt3);
		base.Controls.Add(this.txt2);
		base.Controls.Add(this.txt1);
		base.Controls.Add(this.label7);
		base.Controls.Add(this.keyButton);
		base.Controls.Add(this.dropdelaylabel);
		base.Controls.Add(this.udDrop);
		base.Controls.Add(this.s9);
		base.Controls.Add(this.s8);
		base.Controls.Add(this.s7);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.stopButton);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.repeatdelayLabel);
		base.Controls.Add(this.udDelay);
		base.Controls.Add(this.cbMorse);
		base.Controls.Add(this.notesButton);
		base.Controls.Add(this.speedLabel);
		base.Controls.Add(this.s6);
		base.Controls.Add(this.s5);
		base.Controls.Add(this.s4);
		base.Controls.Add(this.s3);
		base.Controls.Add(this.s2);
		base.Controls.Add(this.s1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.KeyPreview = true;
		base.Name = "CWX";
		this.Text = "   CW Memories and Keyboard ...";
		base.Closing += new System.ComponentModel.CancelEventHandler(CWX_Closing);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(CWX_FormClosing);
		base.Load += new System.EventHandler(CWX_Load);
		base.Paint += new System.Windows.Forms.PaintEventHandler(CWX_Paint);
		base.KeyDown += new System.Windows.Forms.KeyEventHandler(CWX_KeyDown_1);
		base.KeyUp += new System.Windows.Forms.KeyEventHandler(CWX_KeyUp_1);
		base.MouseMove += new System.Windows.Forms.MouseEventHandler(CWX_MouseMove);
		((System.ComponentModel.ISupportInitialize)this.udWPM).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udPtt).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udDrop).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udDelay).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	private void expandButton_Click(object sender, EventArgs e)
	{
		if (base.Width > 500)
		{
			base.Width = 466;
			base.Height = 190;
			expandButton.Left = 432;
			expandButton.Top = 132;
			toolTip1.SetToolTip(expandButton, "Expand Form");
		}
		else
		{
			base.Width = 720;
			base.Height = 320;
			expandButton.Left = 688;
			expandButton.Top = 266;
			toolTip1.SetToolTip(expandButton, "Compress Form");
		}
	}

	private void keyboardButton_Leave(object sender, EventArgs e)
	{
		keyboardButton.ForeColor = Color.Gray;
		keyboardButton.Text = "Keys Off";
		keyboardLed.BackColor = Color.Black;
	}

	private void keyboardButton_Enter(object sender, EventArgs e)
	{
		keyboardButton.ForeColor = Color.Black;
		keyboardButton.Text = "KEYS ACTIVE";
		keyboardLed.BackColor = Color.Cyan;
	}

	private void CWX_KeyUp_1(object sender, KeyEventArgs e)
	{
		kkk++;
		label6.Text = kkk + " " + e.KeyCode.ToString() + " " + e.KeyData.ToString() + " " + e.KeyValue.ToString("x");
		if (e.KeyCode.ToString().Equals("Menu"))
		{
			altkey = false;
		}
	}

	private void CWX_KeyDown_1(object sender, KeyEventArgs e)
	{
		if (!console.PowerOn)
		{
			return;
		}
		char c = (char)e.KeyValue;
		label5.Text = "KeyDown " + c + " " + e.KeyCode.ToString() + " " + e.KeyData.ToString() + " " + e.KeyValue.ToString("x");
		switch (c)
		{
		case 'z':
			chkPause.Checked = !chkPause.Checked;
			break;
		case '{':
			clear_show();
			break;
		case '\u001b':
			clear_show();
			quit = true;
			kquit = true;
			break;
		case 'p':
		case 'q':
		case 'r':
		case 's':
		case 't':
		case 'u':
		case 'v':
		case 'w':
		case 'x':
			queue_start(c - 111);
			break;
		default:
			if (e.KeyCode.ToString().Equals("Menu"))
			{
				altkey = true;
			}
			break;
		}
		if (altkey)
		{
			if (e.KeyCode.ToString().Equals("D1"))
			{
				msg2keys(1);
			}
			else if (e.KeyCode.ToString().Equals("D2"))
			{
				msg2keys(2);
			}
			else if (e.KeyCode.ToString().Equals("D3"))
			{
				msg2keys(3);
			}
			else if (e.KeyCode.ToString().Equals("D4"))
			{
				msg2keys(4);
			}
			else if (e.KeyCode.ToString().Equals("D5"))
			{
				msg2keys(5);
			}
			else if (e.KeyCode.ToString().Equals("D6"))
			{
				msg2keys(6);
			}
			else if (e.KeyCode.ToString().Equals("D7"))
			{
				msg2keys(7);
			}
			else if (e.KeyCode.ToString().Equals("D8"))
			{
				msg2keys(8);
			}
			else if (e.KeyCode.ToString().Equals("D9"))
			{
				msg2keys(9);
			}
		}
	}

	private void keyboardButton_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (console.PowerOn)
		{
			process_key(e.KeyChar);
		}
	}

	public void KeyAction()
	{
		if (!_shown)
		{
			return;
		}
		if (keying)
		{
			quitshut();
		}
		else if (console.RX1DSPMode != DSPMode.CWL && console.RX1DSPMode != DSPMode.CWU)
		{
			MessageBox.Show("Console is not in CW mode.  Please switch to either CWL or CWU and try again.", "CWX Error: Wrong Mode", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		else if (checkPTT())
		{
			quit = true;
			kquit = true;
			while (quit)
			{
				Thread.Sleep(10);
			}
			pause = 60000 / tel;
			tqq = " . ";
			setptt(state: true);
			setkey(state: true);
			keying = true;
		}
	}

	private void keyButton_Click(object sender, EventArgs e)
	{
		KeyAction();
	}

	private bool checkPTT(bool bShowWarning = true)
	{
		if (console.DisablePTT)
		{
			if (bShowWarning)
			{
				MessageBox.Show("Console has PTT disabled.  Please enable it and try again.", "PTT disabled", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			}
			return false;
		}
		return true;
	}

	private void CWX_Load(object sender, EventArgs e)
	{
		cbMorse.SelectedIndex = 0;
	}

	private void CWX_Closing(object sender, CancelEventArgs e)
	{
		quitshut();
		Thread.Sleep(100);
		stopThreads = true;
		if (timerID != 0)
		{
			timeKillEvent(timerID);
		}
		Thread.Sleep(200);
		_threadsStarted = false;
		stopThreads = false;
		Common.SaveForm(this, "CWX");
	}

	public new void Show()
	{
		if (!_shown)
		{
			setup_timer();
			startThreads();
		}
		_shown = true;
		base.Show();
		if (console != null)
		{
			console.CWXShownHandlers?.Invoke(_shown);
		}
	}

	private void TimerPeriodicEventCallback(int id, int msg, int user, int param1, int param2)
	{
		process_element();
	}

	public void PressFNkey(int fn_number)
	{
		if (_shown && fn_number >= 1 && fn_number <= 9)
		{
			queue_start(fn_number);
		}
	}

	private void s1_Click(object sender, EventArgs e)
	{
		queue_start(1);
	}

	private void s2_Click(object sender, EventArgs e)
	{
		queue_start(2);
	}

	private void s3_Click(object sender, EventArgs e)
	{
		queue_start(3);
	}

	private void s4_Click(object sender, EventArgs e)
	{
		queue_start(4);
	}

	private void s5_Click(object sender, EventArgs e)
	{
		queue_start(5);
	}

	private void s6_Click(object sender, EventArgs e)
	{
		queue_start(6);
	}

	private void s7_Click(object sender, EventArgs e)
	{
		queue_start(7);
	}

	private void s8_Click(object sender, EventArgs e)
	{
		queue_start(8);
	}

	private void s9_Click(object sender, EventArgs e)
	{
		queue_start(9);
	}

	private void cbMorse_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button.Equals(MouseButtons.Right))
		{
			editit();
		}
	}

	private void s1_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button.Equals(MouseButtons.Right))
		{
			msg2keys(1);
		}
	}

	private void s2_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button.Equals(MouseButtons.Right))
		{
			msg2keys(2);
		}
	}

	private void s3_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button.Equals(MouseButtons.Right))
		{
			msg2keys(3);
		}
	}

	private void s4_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button.Equals(MouseButtons.Right))
		{
			msg2keys(4);
		}
	}

	private void s5_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button.Equals(MouseButtons.Right))
		{
			msg2keys(5);
		}
	}

	private void s6_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button.Equals(MouseButtons.Right))
		{
			msg2keys(6);
		}
	}

	private void s7_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button.Equals(MouseButtons.Right))
		{
			msg2keys(7);
		}
	}

	private void s8_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button.Equals(MouseButtons.Right))
		{
			msg2keys(8);
		}
	}

	private void s9_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button.Equals(MouseButtons.Right))
		{
			msg2keys(9);
		}
	}

	public void StopAction()
	{
		stopActionCore();
	}

	private void stopActionCore()
	{
		if (_shown)
		{
			clear_show();
			quit = true;
			kquit = true;
		}
	}

	private void stopButton_Click(object sender, EventArgs e)
	{
		StopAction();
	}

	private void udWPM_ValueChanged(object sender, EventArgs e)
	{
		int num = cwxwpm;
		cwxwpm = (int)udWPM.Value;
		setup_timer();
		if (num != cwxwpm)
		{
			console?.CWXSpeedChangedHandlers?.Invoke(num, cwxwpm);
		}
	}

	private void udWPM_LostFocus(object sender, EventArgs e)
	{
		udWPM_ValueChanged(sender, e);
	}

	private void udDelay_ValueChanged(object sender, EventArgs e)
	{
		tpause = (int)udDelay.Value * 1000;
		if (tpause < 1)
		{
			tpause = tel;
		}
	}

	private void udDelay_LostFocus(object sender, EventArgs e)
	{
		udDelay_ValueChanged(sender, e);
	}

	private void udDrop_ValueChanged(object sender, EventArgs e)
	{
		ttdel = (int)udDrop.Value;
	}

	private void udDrop_LostFocus(object sender, EventArgs e)
	{
		udDrop_ValueChanged(sender, e);
	}

	private void udPtt_ValueChanged(object sender, EventArgs e)
	{
		int num = pttdelay;
		pttdelay = (int)udPtt.Value;
		if (num != pttdelay)
		{
			console?.CWXDelayChangedHandlers?.Invoke(num, pttdelay);
		}
	}

	private void udPtt_LostFocus(object sender, EventArgs e)
	{
		udPtt_ValueChanged(sender, e);
	}

	private void CWX_MouseMove(object sender, MouseEventArgs e)
	{
		label5.Text = e.X + " " + e.Y;
	}

	private void CWX_Paint(object sender, PaintEventArgs e)
	{
		show_keys(e.Graphics);
	}

	private void chkPause_CheckedChanged(object sender, EventArgs e)
	{
		pause_checked = chkPause.Checked;
	}

	private void chkAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
	{
		base.TopMost = chkAlwaysOnTop.Checked;
	}

	private void clear_keys()
	{
		keydisplay.WaitOne();
		for (int i = 0; i < 120; i++)
		{
			kbufnew.SetValue('_', i);
			kbufold.SetValue('_', i);
		}
		keydisplay.ReleaseMutex();
	}

	private void show_keys(Graphics formGraphics = null)
	{
		int num = kylx + kyxsz + 1;
		int num2 = kyty + kyysz + 1;
		lock (m_objLock)
		{
			int num3 = kyty + 2;
			int num4 = 11;
			int num5 = 19;
			if (base.Disposing || base.IsDisposed)
			{
				return;
			}
			if (formGraphics == null)
			{
				formGraphics = CreateGraphics();
			}
			Font font = new Font("Courier New", 14f, FontStyle.Bold);
			SolidBrush solidBrush = new SolidBrush(Color.Black);
			SolidBrush solidBrush2 = new SolidBrush(Color.Gray);
			SolidBrush solidBrush3 = new SolidBrush(Color.White);
			formGraphics.FillRectangle(solidBrush3, new Rectangle(kylx, kyty, kyxsz + 1, kyysz + 1));
			Pen pen = new Pen(Color.Gray, 1f);
			formGraphics.DrawLine(pen, kylx, kyty, num, kyty);
			formGraphics.DrawLine(pen, num, kyty, num, num2);
			formGraphics.DrawLine(pen, num, num2, kylx, num2);
			formGraphics.DrawLine(pen, kylx, num2, kylx, kyty);
			pen.Dispose();
			keydisplay.WaitOne();
			int num6 = kylx;
			for (int i = 0; i < 120; i++)
			{
				string s = kbufold.GetValue(i).ToString();
				formGraphics.DrawString(s, font, solidBrush2, num6, num3);
				if (i % 60 == 59)
				{
					num6 = kylx;
					num3 += num5;
				}
				else
				{
					num6 += num4;
				}
			}
			num6 = kylx;
			for (int i = 0; i < 120; i++)
			{
				string s = kbufnew.GetValue(i).ToString();
				formGraphics.DrawString(s, font, solidBrush, num6, num3);
				if (i % 60 == 59)
				{
					num6 = kylx;
					num3 += num5;
				}
				else
				{
					num6 += num4;
				}
			}
			keydisplay.ReleaseMutex();
			font.Dispose();
			solidBrush3.Dispose();
			solidBrush.Dispose();
			solidBrush2.Dispose();
			formGraphics.Dispose();
		}
	}

	private void clearButton_Click(object sender, EventArgs e)
	{
		clear_show();
	}

	private void clear_show()
	{
		clear_keys();
		show_keys();
	}

	private void editit()
	{
		clear_show();
		quitshut();
		editline = cbMorse.Text;
		if (editline[5] == '*')
		{
			MessageBox.Show("Definitions that start with '*' cannot be edited");
			return;
		}
		if (editline.Length != 26)
		{
			MessageBox.Show("Selected line has invalid length");
			return;
		}
		new cwedit(console).ShowDialog();
		if (editline.Length == 26)
		{
			insert_and_reload(editline);
		}
		else if (editline.Length > 0)
		{
			MessageBox.Show("Edited line has invalid length and is not saved.");
		}
	}

	private void insert_and_reload(string s)
	{
		int num = int.Parse(s.Substring(0, 2));
		num -= 32;
		if (num < 0 || num > 63)
		{
			MessageBox.Show("Edited line cannot be found in a2m2.");
			return;
		}
		a2m2[num] = s;
		write_a2m2();
		load_alpha();
		build_mbits2();
	}

	private void write_a2m2()
	{
		if (File.Exists(console.AppDataPath + sfile))
		{
			File.Delete(console.AppDataPath + sfile);
		}
		using StreamWriter streamWriter = new StreamWriter(console.AppDataPath + sfile);
		for (int i = 0; i < 64; i++)
		{
			streamWriter.WriteLine(a2m2[i]);
		}
	}

	private void process_element()
	{
		if (quit)
		{
			quitshut();
			quit = false;
		}
		else
		{
			if (!checkPTT(bShowWarning: false))
			{
				return;
			}
			if (newptt > 0)
			{
				newptt--;
				ttx = ttdel / tel;
				if (newptt <= 0)
				{
					setkey(state: true);
				}
				return;
			}
			if (pause > 0)
			{
				pause--;
				if (pause <= 0)
				{
					loadmsg(tqq);
					push_fifo(4);
				}
				return;
			}
			if (infifo >= 1)
			{
				byte b = pop_fifo();
				if (b == 128)
				{
					return;
				}
				if (b == 4)
				{
					quitshut();
					return;
				}
				if (b == 8)
				{
					ttx = 0;
					pause = tpause / tel;
					if (pause < 1)
					{
						pause = tel;
					}
				}
				else
				{
					if (b == 16)
					{
						setptt(state: true);
						ttx = ttdel / tel;
					}
					if (b != 3 && b != 2)
					{
						return;
					}
					if (b == 3)
					{
						if (!ptt)
						{
							newptt = pttdelay / tel;
						}
						setptt(state: true);
						ttx = ttdel / tel;
						if (newptt <= 0)
						{
							setkey(state: true);
						}
						return;
					}
					setkey(state: false);
				}
			}
			if (ttx > 0)
			{
				ttx--;
			}
			if (ttx <= 0)
			{
				setkey(state: false);
				setptt(state: false);
			}
		}
	}

	private void keyboardFifo()
	{
		while (!stopThreads)
		{
			if (infifo2 > 0)
			{
				byte b = pop_fifo2();
				if (b >= 97 && b <= 122)
				{
					b = (byte)(b - 97 + 65);
				}
				char cc = (char)b;
				if (kquit)
				{
					clear_fifo2();
					kquit = false;
				}
				if (b >= 32)
				{
					loadchar(cc);
					while (infifo > 2)
					{
						Thread.Sleep(10);
					}
				}
			}
			else
			{
				Thread.Sleep(20);
			}
		}
	}

	private void keyboardDisplay()
	{
		while (!stopThreads)
		{
			while (pause_checked)
			{
				Thread.Sleep(100);
			}
			char num = (char)kbufnew.GetValue(0);
			char c = num;
			if (num != '_')
			{
				keydisplay.WaitOne();
				for (int i = 0; i < 119; i++)
				{
					kbufold.SetValue(kbufold.GetValue(i + 1), i);
				}
				kbufold.SetValue(c, 119);
				for (int i = 0; i < 119; i++)
				{
					kbufnew.SetValue(kbufnew.GetValue(i + 1), i);
				}
				kbufnew.SetValue('_', 119);
				keydisplay.ReleaseMutex();
				push_fifo2((byte)c);
				show_keys();
				while (infifo > 0)
				{
					Thread.Sleep(10);
				}
			}
			else
			{
				Thread.Sleep(20);
			}
		}
	}

	private void queue_start(int qmsg)
	{
		if (console.RX1DSPMode != DSPMode.CWL && console.RX1DSPMode != DSPMode.CWU)
		{
			MessageBox.Show("Console is not in CW mode.  Please switch to either CWL or CWU and try again.", "CWX Error: Wrong Mode", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		kquit = true;
		quit = true;
		while (quit)
		{
			Thread.Sleep(10);
		}
		switch (qmsg)
		{
		case 1:
			tqq = txt1.Text;
			break;
		case 2:
			tqq = txt2.Text;
			break;
		case 3:
			tqq = txt3.Text;
			break;
		case 4:
			tqq = txt4.Text;
			break;
		case 5:
			tqq = txt5.Text;
			break;
		case 6:
			tqq = txt6.Text;
			break;
		case 7:
			tqq = txt7.Text;
			break;
		case 8:
			tqq = txt8.Text;
			break;
		case 9:
			tqq = txt9.Text;
			break;
		default:
			tqq = "?bad msg?";
			break;
		}
		loadmsg(tqq);
		push_fifo(4);
	}

	private void loadchar(char cc)
	{
		uint num = 0u;
		int num2 = cc - 32;
		if (cc >= 'a' && cc <= 'z')
		{
			num2 -= 32;
		}
		if (num2 < 0 || num2 > 63)
		{
			return;
		}
		num2 &= 0x3F;
		if (num2 == 2 || num2 == 3 || num2 == 4)
		{
			return;
		}
		num = (uint)mbits.GetValue(num2);
		for (uint num3 = num & 0x1F; num3 != 0; num3--)
		{
			if ((num & 0x80000000u) != 0)
			{
				push_fifo(3);
			}
			else
			{
				push_fifo(2);
			}
			num <<= 1;
		}
	}

	private void loadmsg(string t)
	{
		int length = t.Length;
		if (length < 1)
		{
			t = "?";
			length = t.Length;
		}
		string obj = t.ToUpper();
		char[] array = new char[length + 1];
		obj.CopyTo(0, array, 0, length);
		length = t.Length;
		clear_fifo();
		for (int num = pttdelay / tel; num > 0; num--)
		{
			push_fifo(16);
		}
		int num2 = 0;
		bool flag = false;
		while (length > 0)
		{
			char num3 = (char)array.GetValue(num2);
			uint num4 = 0u;
			int num5 = num3 - 32;
			num5 &= 0x3F;
			if (num5 == 2)
			{
				push_fifo(8);
				flag = true;
				break;
			}
			num4 = num5 switch
			{
				3 => 4294967063u, 
				4 => 23u, 
				_ => (uint)mbits.GetValue(num5), 
			};
			for (uint num6 = num4 & 0x1F; num6 != 0; num6--)
			{
				if ((num4 & 0x80000000u) != 0)
				{
					push_fifo(3);
				}
				else
				{
					push_fifo(2);
				}
				num4 <<= 1;
			}
			num2++;
			length--;
		}
		if (!flag)
		{
			push_fifo(4);
		}
	}

	private void process_key(char key)
	{
		if (key >= ' ' && key <= '~')
		{
			if (key >= 'a' && key <= 'z')
			{
				key = (char)(key - 97);
				key = (char)(key + 65);
			}
			insert_key(key);
			show_keys();
		}
		else if (key == '\b')
		{
			backspace();
		}
	}

	private void CbMorse_SelectedIndexChanged(object sender, EventArgs e)
	{
	}

	private void insert_key(char key)
	{
		keydisplay.WaitOne();
		for (int i = 0; i < 120; i++)
		{
			if ((char)kbufnew.GetValue(i) == '_')
			{
				kbufnew.SetValue(key, i);
				keydisplay.ReleaseMutex();
				return;
			}
		}
		kbufnew.SetValue(key, 119);
		keydisplay.ReleaseMutex();
	}

	private void CWX_FormClosing(object sender, FormClosingEventArgs e)
	{
		clear_show();
		quitshut();
		_shown = false;
		e.Cancel = true;
		Hide();
		if (console != null)
		{
			console.CWXShownHandlers?.Invoke(_shown);
		}
	}

	private void backspace()
	{
		for (int num = 119; num >= 0; num--)
		{
			if ((char)kbufnew.GetValue(num) != '_')
			{
				kbufnew.SetValue('_', num);
				show_keys();
				break;
			}
		}
	}

	private void msg2keys(int nmsg)
	{
		char[] array = new char[5];
		string text = nmsg switch
		{
			1 => txt1.Text, 
			2 => txt2.Text, 
			3 => txt3.Text, 
			4 => txt4.Text, 
			5 => txt5.Text, 
			6 => txt6.Text, 
			7 => txt7.Text, 
			8 => txt8.Text, 
			9 => txt9.Text, 
			_ => "?bad msg?", 
		};
		insert_key(' ');
		int length = text.Length;
		for (int i = 0; i < length; i++)
		{
			text.Substring(i, 1).CopyTo(0, array, 0, 1);
			char key = (char)array.GetValue(0);
			insert_key(key);
		}
		show_keys();
	}

	public void StopEverything(bool bPowerState = false)
	{
		keyButton.Enabled = bPowerState;
		keyboardButton.Enabled = bPowerState;
		s1.Enabled = bPowerState;
		s2.Enabled = bPowerState;
		s3.Enabled = bPowerState;
		s4.Enabled = bPowerState;
		s5.Enabled = bPowerState;
		s6.Enabled = bPowerState;
		s7.Enabled = bPowerState;
		s8.Enabled = bPowerState;
		s9.Enabled = bPowerState;
		stopActionCore();
	}

	private void onGlobalKeyDown(Keys keycode)
	{
		if (_shown && !chkFocusRequired.Checked && !Focused)
		{
			CWX_KeyDown_1(this, new KeyEventArgs(keycode));
		}
	}

	private void onGlobalKeyUp(Keys keycode)
	{
		if (_shown && !chkFocusRequired.Checked && !Focused)
		{
			CWX_KeyUp_1(this, new KeyEventArgs(keycode));
		}
	}
}
