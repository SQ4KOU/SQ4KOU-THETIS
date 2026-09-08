using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thetis;

public class PSForm : Form
{
	private enum eCMDState
	{
		OFF,
		TurnOnAutoCalibrate,
		AutoCalibrate,
		TurnOnSingleCalibrate,
		SingleCalibrate,
		StayON,
		TurnOFF,
		IntiateRestoredCorrection
	}

	private enum eAAState
	{
		Monitor,
		SetNewValues,
		RestoreOperation
	}

	private Console console;

	private int _gcolor = -16711936;

	private static bool _autoON = false;

	private static bool _singlecalON = false;

	private static bool _restoreON = false;

	private static bool _OFF = true;

	private eAAState _autoAttenuateState;

	private static double _PShwpeak;

	private static double _GetPSpeakval;

	public static AmpView ampv = null;

	public static Thread ampvThread = null;

	private int _save_autoON;

	private int _save_singlecalON;

	private int _deltadB;

	private const int AUTO_ATT_REQUIRED_STRIKES = 3;

	private int _autoAttOutOfRangeCount;

	private int _restoreSettleTicks;

	private bool _power;

	private bool _psFormReady;

	private static eCMDState _cmdstate = eCMDState.OFF;

	private static bool _topmost = false;

	private Thread _ps_thread;

	private bool m_bQuckAttenuate;

	private int _ints = 16;

	private int _spi = 256;

	private volatile bool _bPSRunning;

	private volatile bool _ps_closing;

	private static bool _psenabled = false;

	private static bool _autocal_enabled = false;

	private static bool _autoattenuate = true;

	private static bool _ttgenON = false;

	private static int _txachannel = WDSP.id(1u, 0u);

	private readonly object _objLocker = new object();

	private static bool _mox = false;

	private readonly ManualResetEventSlim _ampViewDone = new ManualResetEventSlim(initialState: false);

	private bool _performing_single_cal;

	private int _performing_single_cal_retries;

	private HPSDRModel _lastModel = HPSDRModel.FIRST;

	private static readonly object _psAutoAttenLogLock = new object();

	private bool _advancedON;

	private LabelTS lblPSState;

	private static bool _psCacheLoaded = false;

	private static readonly object _psLogLock = new object();

	private static StreamWriter _psLog = null;

	private static string _psLogPath = null;

	private static int _psLogWrites = 0;

	private const long PS_LOG_MAX_BYTES = 10485760L;

	private IContainer components;

	private GroupBoxTS grpPSInfo;

	private LabelTS lblPSInfo15;

	private LabelTS labelTS146;

	private LabelTS lblPSInfo3;

	private LabelTS lblPSInfo2;

	private LabelTS lblPSInfo1;

	private LabelTS lblPSInfo0;

	private LabelTS labelTS143;

	private LabelTS labelTS144;

	private LabelTS labelTS142;

	private LabelTS labelTS141;

	private ButtonTS btnPSReset;

	private ButtonTS btnPSCalibrate;

	private LabelTS labelTS140;

	private NumericUpDownTS udPSCalWait;

	private LabelTS labelTS2;

	private NumericUpDownTS udPSPhnum;

	private LabelTS labelTS4;

	private NumericUpDownTS udPSMoxDelay;

	private ButtonTS btnPSSave;

	private ButtonTS btnPSRestore;

	private LabelTS lblPSInfoFB;

	private LabelTS labelTS8;

	private LabelTS lblPSInfoCO;

	private LabelTS labelTS9;

	private ButtonTS btnPSTwoToneGen;

	private LabelTS lblPSfb2;

	private LabelTS labelTS1;

	private LabelTS labelTS5;

	private TextBoxTS txtPSpeak;

	private TextBoxTS GetPSpeak;

	private LabelTS labelTS3;

	private ButtonTS btnPSResetEngine;

	private ComboBoxTS comboPSTint;

	private NumericUpDownTS udPSOutlierSigma;

	private LabelTS lblPSOutlierSigma;

	private CheckBoxTS chkPSOutlierEnable;

	private NumericUpDownTS udPSTargetFeedback;

	private NumericUpDownTS udPSFBTargetOffset;

	private LabelTS lblPSFBTargetOffset;

	private LabelTS lblPSTargetFeedback;

	private CheckBoxTS chkPSEQ;

	private CheckBoxTS chkPSPin;

	private NumericUpDownTS udPSDCBCap;

	private LabelTS lblPSDCBCap;

	private CheckBoxTS chkPSDCB;

	private NumericUpDownTS udPSPinAlpha;

	private LabelTS lblPSPinAlpha;

	private NumericUpDownTS udPSEMAAlpha;

	private LabelTS lblPSEMAAlpha;

	private ButtonTS btnPSAmpView;

	private CheckBoxTS chkPSAutoAttenuate;

	private LabelTS lblPSInfo5;

	private LabelTS labelTS13;

	private LabelTS lblPSInfo13;

	private LabelTS labelTS11;

	private LabelTS lblPSInfo6;

	private LabelTS labelTS7;

	private CheckBoxTS checkLoopback;

	private CheckBoxTS chkPSStbl;

	private ButtonTS btnPSAdvanced;

	private CheckBoxTS chkPSOnTop;

	private CheckBoxTS chkQuickAttenuate;

	private ButtonTS btnDefaultPeaks;

	private CheckBoxTS chkAdvancedViewHidden;

	private PictureBox pbWarningSetPk;

	private ToolTip toolTip1;

	private CheckBoxTS chkShow2ToneMeasurements;

	public bool QuickAttenuate
	{
		get
		{
			return m_bQuckAttenuate;
		}
		set
		{
			m_bQuckAttenuate = value;
		}
	}

	public int Ints
	{
		get
		{
			return _ints;
		}
		set
		{
			_ints = value;
		}
	}

	public int Spi
	{
		get
		{
			return _spi;
		}
		set
		{
			_spi = value;
		}
	}

	public ToolTip ToolTip => toolTip1;

	public unsafe bool PSEnabled
	{
		get
		{
			return _psenabled;
		}
		set
		{
			_psenabled = value;
			PsCalLog($"PS enable={value}");
			NetworkIO.NotifyRxReconfig();
			if (value)
			{
				OnPSContextChanged();
			}
			else
			{
				SaveCurveCache();
			}
			if (_psenabled)
			{
				console.UpdateDDCs(console.RX2Enabled);
				NetworkIO.SetPureSignal(1);
				NetworkIO.SendHighPriority(1);
				console.UpdateAAudioMixerStates();
				cmaster.LoadRouterControlBit(null, 0, 0, 1);
				console.radio.GetDSPTX(0).PSRunCal = true;
			}
			else
			{
				console.UpdateDDCs(console.RX2Enabled);
				NetworkIO.SetPureSignal(0);
				NetworkIO.SendHighPriority(1);
				console.UpdateAAudioMixerStates();
				cmaster.LoadRouterControlBit(null, 0, 0, 0);
				console.radio.GetDSPTX(0).PSRunCal = false;
			}
			if (console.path_Illustrator != null)
			{
				console.path_Illustrator.pi_Changed();
			}
		}
	}

	public bool AutoCalEnabled
	{
		get
		{
			return _autocal_enabled;
		}
		set
		{
			_autocal_enabled = value;
			if (_autocal_enabled)
			{
				_autoON = true;
				console.PSState = true;
			}
			else
			{
				_OFF = true;
				console.PSState = false;
			}
		}
	}

	public bool AutoAttenuate
	{
		get
		{
			return _autoattenuate;
		}
		set
		{
			_autoattenuate = value;
			if (_autoattenuate)
			{
				console.ATTOnTX = _autoattenuate;
			}
			else if (!console.IsSetupFormNull)
			{
				console.ATTOnTX = console.SetupForm.ATTOnTXChecked;
			}
			else
			{
				console.ATTOnTX = _autoattenuate;
			}
		}
	}

	public bool TTgenON
	{
		get
		{
			return _ttgenON;
		}
		set
		{
			_ttgenON = value;
			if (_ttgenON)
			{
				btnPSTwoToneGen.BackColor = Color.FromArgb(_gcolor);
			}
			else
			{
				btnPSTwoToneGen.BackColor = SystemColors.Control;
			}
		}
	}

	public int TXAchannel
	{
		get
		{
			return _txachannel;
		}
		set
		{
			_txachannel = value;
		}
	}

	public bool Mox
	{
		get
		{
			return _mox;
		}
		set
		{
			_mox = value;
			puresignal.SetPSMox(_txachannel, value);
		}
	}

	private int FBTargetDefaultOffset
	{
		get
		{
			if (HardwareSpecific.Model != HPSDRModel.ANVELINAPRO3)
			{
				return 0;
			}
			return 10;
		}
	}

	public PSForm(Console c)
	{
		InitializeComponent();
		Common.DoubleBufferAll(this, enabled: true);
		lblPSState = new LabelTS();
		lblPSState.AutoSize = true;
		lblPSState.ForeColor = Color.White;
		lblPSState.BackColor = Color.Transparent;
		lblPSState.Location = new Point(250, 40);
		lblPSState.Name = "lblPSState";
		lblPSState.Text = "";
		toolTip1.SetToolTip(lblPSState, "PureSignal engine state");
		base.Controls.Add(lblPSState);
		lblPSState.BringToFront();
		chkPSEQ.Visible = false;
		chkPSDCB.Visible = true;
		udPSDCBCap.Visible = true;
		lblPSDCBCap.Visible = true;
		chkPSOutlierEnable.Visible = false;
		udPSOutlierSigma.Visible = false;
		lblPSOutlierSigma.Visible = false;
		udPSFBTargetOffset.Visible = false;
		lblPSFBTargetOffset.Visible = false;
		udPSTargetFeedback.Visible = false;
		lblPSTargetFeedback.Visible = false;
		txtPSpeak.Text = "";
		console = c;
		Common.RestoreForm(this, "PureSignal", restore_size: false);
		_advancedON = chkAdvancedViewHidden.Checked;
		Console obj = console;
		obj.PowerChangeHanders = (Console.PowerChanged)Delegate.Combine(obj.PowerChangeHanders, new Console.PowerChanged(onPowerOn));
		console.ConsoleClosingHandlersAsync += onConsoleClosingAsync;
		_power = console.PowerOn;
		startPSThread();
		_psFormReady = true;
	}

	private void startPSThread()
	{
		if (_ps_thread == null || !_ps_thread.IsAlive)
		{
			_ps_thread = new Thread(PSLoop)
			{
				Name = "PureSignal Thread",
				Priority = ThreadPriority.AboveNormal,
				IsBackground = true
			};
			_ps_thread.Start();
		}
	}

	public void StopPSThread()
	{
		_ps_closing = true;
		_bPSRunning = false;
		if (_ps_thread != null && _ps_thread.IsAlive)
		{
			_ps_thread.Join(1000);
		}
		if (console != null)
		{
			Console obj = console;
			obj.PowerChangeHanders = (Console.PowerChanged)Delegate.Remove(obj.PowerChangeHanders, new Console.PowerChanged(onPowerOn));
			console.ConsoleClosingHandlersAsync -= onConsoleClosingAsync;
		}
	}

	private async Task onConsoleClosingAsync()
	{
		_ps_closing = true;
		await Task.Delay(100);
	}

	private void onPowerOn(bool oldPower, bool newPower)
	{
		_power = newPower;
	}

	private void PSLoop()
	{
		_bPSRunning = true;
		int num = 0;
		while (_bPSRunning && !_ps_closing)
		{
			int millisecondsTimeout;
			if (!_ps_closing && _power && !base.IsDisposed && base.IsHandleCreated)
			{
				timer1code();
				if (num == 0)
				{
					timer2code();
				}
				num++;
				if (m_bQuckAttenuate || num == 10)
				{
					num = 0;
				}
				millisecondsTimeout = 10;
			}
			else
			{
				num = 0;
				millisecondsTimeout = 100;
			}
			Thread.Sleep(millisecondsTimeout);
		}
	}

	private void psdefpeak(double value)
	{
		string text = value.ToString();
		if (txtPSpeak.Text != text)
		{
			txtPSpeak.Text = value.ToString();
		}
		else
		{
			PSpeak_TextChanged(this, EventArgs.Empty);
		}
		UpdateWarningSetPk();
	}

	private void PSForm_Load(object sender, EventArgs e)
	{
		SetupForm();
	}

	public unsafe void SetupForm()
	{
		if (_ttgenON)
		{
			btnPSTwoToneGen.BackColor = Color.FromArgb(_gcolor);
		}
		fixed (double* pShwpeak = &_PShwpeak)
		{
			puresignal.GetPSHWPeak(_txachannel, pShwpeak);
		}
		txtPSpeak.Text = _PShwpeak.ToString();
		setAdvancedView();
	}

	private void PSForm_Closing(object sender, FormClosingEventArgs e)
	{
		Hide();
		e.Cancel = true;
		Common.SaveForm(this, "PureSignal");
	}

	public void CloseAmpView()
	{
		if (ampv != null)
		{
			_ampViewDone.Reset();
			ampv.Invoke((Action)delegate
			{
				ampv.CloseDown();
			});
			_ampViewDone.Wait();
			if (ampvThread != null && ampvThread.IsAlive && !ampvThread.Join(1000))
			{
				ampvThread.Abort();
			}
			ampvThread = null;
			ampv = null;
		}
	}

	public void RunAmpv()
	{
		ampv = new AmpView(this);
		ampv.Opacity = 0.0;
		Application.Run(ampv);
		_ampViewDone.Set();
	}

	private void btnPSAmpView_Click(object sender, EventArgs e)
	{
		if (ampv == null || (ampv != null && ampv.IsDisposed))
		{
			ampvThread = new Thread(RunAmpv);
			ampvThread.SetApartmentState(ApartmentState.STA);
			ampvThread.Name = "Ampv Thread";
			ampvThread.Start();
		}
	}

	private void btnPSCalibrate_Click(object sender, EventArgs e)
	{
		if (_singlecalON)
		{
			_singlecalON = false;
			return;
		}
		console.ForcePureSignalAutoCalDisable();
		_singlecalON = true;
		console.PSState = false;
	}

	public void SingleCalrun()
	{
		btnPSCalibrate_Click(this, EventArgs.Empty);
	}

	private void btnPSReset_Click(object sender, EventArgs e)
	{
		console.ForcePureSignalAutoCalDisable();
		if (!_OFF)
		{
			_OFF = true;
		}
		console.PSState = false;
	}

	private void udPSMoxDelay_ValueChanged(object sender, EventArgs e)
	{
		puresignal.SetPSMoxDelay(_txachannel, (double)udPSMoxDelay.Value);
	}

	private void udPSCalWait_ValueChanged(object sender, EventArgs e)
	{
		puresignal.SetPSLoopDelay(_txachannel, (double)udPSCalWait.Value);
	}

	private void udPSPhnum_ValueChanged(object sender, EventArgs e)
	{
		puresignal.SetPSTXDelay(_txachannel, (double)udPSPhnum.Value * 1E-09);
	}

	private void btnPSTwoToneGen_Click(object sender, EventArgs e)
	{
		if (!_ttgenON)
		{
			btnPSTwoToneGen.BackColor = Color.FromArgb(_gcolor);
			_ttgenON = true;
			console.SetupForm.TTgenrun = true;
		}
		else
		{
			btnPSTwoToneGen.BackColor = SystemColors.Control;
			_ttgenON = false;
			console.SetupForm.TTgenrun = false;
		}
	}

	private void btnPSSave_Click(object sender, EventArgs e)
	{
		Directory.CreateDirectory(console.AppDataPath + "PureSignal\\");
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.InitialDirectory = console.AppDataPath + "PureSignal\\";
		saveFileDialog.RestoreDirectory = true;
		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			puresignal.PSSaveCorr(_txachannel, saveFileDialog.FileName);
		}
	}

	private void btnPSRestore_Click(object sender, EventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.InitialDirectory = console.AppDataPath + "PureSignal\\";
		openFileDialog.RestoreDirectory = true;
		if (openFileDialog.ShowDialog() == DialogResult.OK)
		{
			console.ForcePureSignalAutoCalDisable();
			_OFF = false;
			puresignal.PSRestoreCorr(_txachannel, openFileDialog.FileName);
			_restoreON = true;
		}
	}

	public void SetDefaultPeaks()
	{
		psdefpeak(HardwareSpecific.PSDefaultPeak);
	}

	private unsafe void timer1code()
	{
		if (!_bPSRunning)
		{
			return;
		}
		if (HardwareSpecific.Model != _lastModel)
		{
			_lastModel = HardwareSpecific.Model;
			udPSFBTargetOffset.Value = FBTargetDefaultOffset;
			ApplyFBTarget();
		}
		puresignal.GetInfo(_txachannel);
		bool infoChanged = puresignal.HasInfoChanged;
		int i0 = puresignal.Info[0];
		int i1 = puresignal.Info[1];
		int i2 = puresignal.Info[2];
		int i3 = puresignal.Info[3];
		int fb = puresignal.FeedbackLevel;
		int calCount = puresignal.CalibrationCount;
		int i6 = puresignal.Info[6];
		int i13 = puresignal.Info[13];
		int i15 = puresignal.Info[15];
		bool corrApplied = puresignal.CorrectionsBeingApplied;
		bool correcting = puresignal.Correcting;
		bool calAttemptsChanged = puresignal.CalibrationAttemptsChanged;
		int num = puresignal.Info[7];
		int num2 = puresignal.Info[8];
		int num3 = puresignal.Info[9];
		int num4 = puresignal.Info[10];
		int num5 = puresignal.Info[11];
		int num6 = puresignal.Info[12];
		int num7 = puresignal.Info[13];
		int num8 = puresignal.Info[14];
		Color fbColor = puresignal.FeedbackColourLevel;
		if (calAttemptsChanged)
		{
			double m = 0.0;
			double c = 0.0;
			double s = 0.0;
			double m2 = 0.0;
			double m3 = 0.0;
			double c2;
			double s2;
			try
			{
				puresignal.GetPSCurveVals(_txachannel, 0, out m, out c, out s);
				puresignal.GetPSCurveVals(_txachannel, 8, out m2, out c2, out s2);
				puresignal.GetPSCurveVals(_txachannel, 15, out m3, out s2, out c2);
			}
			catch
			{
			}
			double m4 = 0.0;
			double m5 = 0.0;
			double m6 = 0.0;
			try
			{
				puresignal.GetPSCurveVals(_txachannel, 1, out m4, out c2, out s2);
				puresignal.GetPSCurveVals(_txachannel, 2, out m5, out s2, out c2);
				puresignal.GetPSCurveVals(_txachannel, 3, out m6, out c2, out s2);
			}
			catch
			{
			}
			double num9 = 0.0;
			try
			{
				num9 = puresignal.GetPSXYLag(_txachannel);
			}
			catch
			{
			}
			PsCalLog($"cal;fb={fb};n={calCount};i0={i0};i1={i1};i2={i2};i3={i3};i6={i6};i7={num};pm={num5};i13={i13};i14={(corrApplied ? 1 : 0)};i15={i15};m0={m:F4};c0={c:F4};s0={s:F4};m8={m2:F4};m15={m3:F4};ym0avg={num3};ym0med={num4};ym0max={num2};rmid={num6};m1={m4:F4};m2={m5:F4};m3={m6:F4};ym0min={num7};ym0mad={num8};xylag={num9:F1}");
		}
		bool fbOK = puresignal.IsFeedbackLevelOK;
		bool autocalEnabled = _autocal_enabled;
		fixed (double* getPSpeakval = &_GetPSpeakval)
		{
			puresignal.GetPSMaxTX(_txachannel, getPSpeakval);
		}
		string peakText = _GetPSpeakval.ToString();
		if (base.IsHandleCreated && !base.IsDisposed && !base.Disposing)
		{
			try
			{
				BeginInvoke((Action)delegate
				{
					if (infoChanged)
					{
						lblPSInfo0.Text = i0.ToString();
						lblPSInfo1.Text = i1.ToString();
						lblPSInfo2.Text = i2.ToString();
						lblPSInfo3.Text = i3.ToString();
						lblPSfb2.Text = fb.ToString();
						lblPSInfo5.Text = calCount.ToString();
						lblPSInfo6.Text = i6.ToString();
						if (i6 == 2)
						{
							if (lblPSInfo6.BackColor != Color.Red)
							{
								lblPSInfo6.BackColor = Color.Red;
								lblPSInfo6.ForeColor = Color.White;
							}
						}
						else if (lblPSInfo6.BackColor != Color.Bisque)
						{
							lblPSInfo6.BackColor = Color.Bisque;
							lblPSInfo6.ForeColor = Color.Black;
						}
						lblPSInfo13.Text = i13.ToString();
						lblPSInfo15.Text = i15.ToString();
					}
					string text = PSStateText(i15, corrApplied);
					if (i15 == 4)
					{
						int pSCollectProgress = puresignal.GetPSCollectProgress(_txachannel);
						text = $"PS: collecting {pSCollectProgress}/16";
					}
					lblPSState.Text = text;
					console.InfoBarPSState(text);
					if (corrApplied)
					{
						btnPSSave.Enabled = true;
						if (correcting)
						{
							if (lblPSInfoCO.BackColor != Color.Lime)
							{
								lblPSInfoCO.BackColor = Color.Lime;
							}
						}
						else if (lblPSInfoCO.BackColor != Color.Yellow)
						{
							lblPSInfoCO.BackColor = Color.Yellow;
						}
					}
					else
					{
						btnPSSave.Enabled = false;
						if (lblPSInfoCO.BackColor != Color.Black)
						{
							lblPSInfoCO.BackColor = Color.Black;
						}
					}
					if (calAttemptsChanged)
					{
						if (lblPSInfoFB.BackColor != fbColor)
						{
							lblPSInfoFB.BackColor = fbColor;
						}
					}
					else if (lblPSInfoFB.BackColor.R > 0 || lblPSInfoFB.BackColor.G > 0 || lblPSInfoFB.BackColor.B > 0)
					{
						int red = Math.Max(0, lblPSInfoFB.BackColor.R - 5);
						int green = Math.Max(0, lblPSInfoFB.BackColor.G - 5);
						int blue = Math.Max(0, lblPSInfoFB.BackColor.B - 5);
						Color color = Color.FromArgb(red, green, blue);
						if (lblPSInfoFB.BackColor != color)
						{
							lblPSInfoFB.BackColor = color;
						}
					}
					if (autocalEnabled && infoChanged)
					{
						console.InfoBarFeedbackLevel(fb, fbOK, corrApplied, calAttemptsChanged, fbColor);
					}
					if (GetPSpeak.Text != peakText)
					{
						GetPSpeak.Text = peakText;
					}
				});
			}
			catch
			{
			}
		}
		switch (_cmdstate)
		{
		case eCMDState.OFF:
			puresignal.SetPSControl(_txachannel, 1, 0, 0, 0);
			if (PSEnabled)
			{
				PSEnabled = false;
			}
			btnPSCalibrate.BackColor = SystemColors.Control;
			if (_restoreON)
			{
				_cmdstate = eCMDState.IntiateRestoredCorrection;
			}
			else if (_autoON)
			{
				_cmdstate = eCMDState.TurnOnAutoCalibrate;
			}
			else if (_singlecalON)
			{
				_cmdstate = eCMDState.TurnOnSingleCalibrate;
			}
			_OFF = false;
			break;
		case eCMDState.TurnOnAutoCalibrate:
			puresignal.SetPSControl(_txachannel, 1, 0, 1, 0);
			if (!PSEnabled)
			{
				PSEnabled = true;
			}
			btnPSCalibrate.BackColor = SystemColors.Control;
			_cmdstate = eCMDState.AutoCalibrate;
			break;
		case eCMDState.AutoCalibrate:
			if (_OFF)
			{
				_cmdstate = eCMDState.TurnOFF;
			}
			else if (_restoreON)
			{
				_cmdstate = eCMDState.IntiateRestoredCorrection;
			}
			else if (_singlecalON)
			{
				_cmdstate = eCMDState.TurnOnSingleCalibrate;
			}
			break;
		case eCMDState.TurnOnSingleCalibrate:
			_autoON = false;
			_performing_single_cal = true;
			puresignal.SetPSControl(_txachannel, 1, 1, 0, 0);
			if (!PSEnabled)
			{
				PSEnabled = true;
			}
			btnPSCalibrate.BackColor = Color.FromArgb(_gcolor);
			_cmdstate = eCMDState.SingleCalibrate;
			break;
		case eCMDState.SingleCalibrate:
			_singlecalON = false;
			if (_OFF)
			{
				_cmdstate = eCMDState.TurnOFF;
			}
			else if (_restoreON)
			{
				_cmdstate = eCMDState.IntiateRestoredCorrection;
			}
			else if (_autoON)
			{
				_cmdstate = eCMDState.TurnOnAutoCalibrate;
			}
			else if (puresignal.CorrectionsBeingApplied)
			{
				_cmdstate = eCMDState.StayON;
			}
			break;
		case eCMDState.StayON:
			if (PSEnabled)
			{
				PSEnabled = false;
			}
			btnPSCalibrate.BackColor = SystemColors.Control;
			if (_OFF)
			{
				_cmdstate = eCMDState.TurnOFF;
			}
			else if (_restoreON)
			{
				_cmdstate = eCMDState.IntiateRestoredCorrection;
			}
			else if (_autoON)
			{
				_cmdstate = eCMDState.TurnOnAutoCalibrate;
			}
			else if (_singlecalON)
			{
				_cmdstate = eCMDState.TurnOnSingleCalibrate;
			}
			else if (_performing_single_cal)
			{
				_performing_single_cal = false;
				if (!puresignal.IsFeedbackLevelOKRange && _performing_single_cal_retries < 5)
				{
					_performing_single_cal_retries++;
					_singlecalON = true;
				}
				else
				{
					_performing_single_cal_retries = 0;
				}
			}
			break;
		case eCMDState.TurnOFF:
			if (!_autocal_enabled)
			{
				_autoON = false;
			}
			puresignal.SetPSControl(_txachannel, 1, 0, 0, 0);
			if (!PSEnabled)
			{
				PSEnabled = true;
			}
			btnPSCalibrate.BackColor = SystemColors.Control;
			_OFF = false;
			if (_restoreON)
			{
				_cmdstate = eCMDState.IntiateRestoredCorrection;
			}
			else if (_autoON)
			{
				_cmdstate = eCMDState.TurnOnAutoCalibrate;
			}
			else if (_singlecalON)
			{
				_cmdstate = eCMDState.TurnOnSingleCalibrate;
			}
			else if (!puresignal.CorrectionsBeingApplied && puresignal.State == puresignal.EngineState.LRESET)
			{
				_cmdstate = eCMDState.OFF;
			}
			break;
		case eCMDState.IntiateRestoredCorrection:
			_autoON = false;
			puresignal.SetPSControl(_txachannel, 0, 0, 0, 1);
			if (!PSEnabled)
			{
				PSEnabled = true;
			}
			btnPSCalibrate.BackColor = SystemColors.Control;
			_restoreON = false;
			if (puresignal.State == puresignal.EngineState.LSTAYON)
			{
				_cmdstate = eCMDState.StayON;
			}
			break;
		}
	}

	private void timer2code()
	{
		if (!_bPSRunning)
		{
			return;
		}
		switch (_autoAttenuateState)
		{
		case eAAState.Monitor:
		{
			if (!_autoattenuate || !puresignal.CalibrationAttemptsChanged)
			{
				break;
			}
			if (puresignal.NeedToRecalibrate(console.SetupForm.ATTOnTX))
			{
				_autoAttOutOfRangeCount++;
				LogAutoAttenuate($"MONITOR-PENDING FB={puresignal.FeedbackLevel} ATT={console.SetupForm.ATTOnTX} strike={_autoAttOutOfRangeCount}/{3} {PSInfoBits()}");
			}
			else
			{
				_autoAttOutOfRangeCount = 0;
			}
			if (_autoAttOutOfRangeCount < 3)
			{
				break;
			}
			_autoAttOutOfRangeCount = 0;
			if (!console.ATTOnTX)
			{
				AutoAttenuate = true;
			}
			_autoAttenuateState = eAAState.SetNewValues;
			double num;
			if (puresignal.IsFeedbackLevelOK)
			{
				num = 20.0 * Math.Log10((double)puresignal.FeedbackLevel / (double)puresignal.TargetFeedbackLevel);
				if (double.IsNaN(num))
				{
					num = 31.1;
				}
				if (num < -100.0)
				{
					num = -100.0;
				}
				if (num > 100.0)
				{
					num = 100.0;
				}
			}
			else
			{
				num = 31.1;
			}
			_deltadB = (int)Math.Round(num, MidpointRounding.AwayFromZero);
			int aTTOnTX2 = console.SetupForm.ATTOnTX;
			int num2 = ((aTTOnTX2 + _deltadB > 0) ? (aTTOnTX2 + _deltadB) : 0);
			if (num2 > 31)
			{
				num2 = 31;
			}
			if (num2 == aTTOnTX2)
			{
				LogAutoAttenuate($"MONITOR-SKIP FB={puresignal.FeedbackLevel} ATT={aTTOnTX2} ddB={num:F2} (at limit, no reset) {PSInfoBits()}");
				break;
			}
			_save_autoON = ((_cmdstate == eCMDState.AutoCalibrate) ? 1 : 0);
			_save_singlecalON = ((_cmdstate == eCMDState.SingleCalibrate) ? 1 : 0);
			puresignal.SetPSControl(_txachannel, 1, 0, 0, 0);
			LogAutoAttenuate($"MONITOR FB={puresignal.FeedbackLevel} ATT={console.SetupForm.ATTOnTX} ddB={num:F2} delta={_deltadB} OK={puresignal.IsFeedbackLevelOKRange} {PSInfoBits()}");
			break;
		}
		case eAAState.SetNewValues:
		{
			_autoAttenuateState = eAAState.RestoreOperation;
			int aTTOnTX = console.SetupForm.ATTOnTX;
			int newAtten;
			if (aTTOnTX + _deltadB > 0)
			{
				newAtten = aTTOnTX + _deltadB;
			}
			else
			{
				newAtten = 0;
			}
			if (aTTOnTX != newAtten)
			{
				try
				{
					if (console.SetupForm != null && console.SetupForm.InvokeRequired)
					{
						console.SetupForm.Invoke((Action)delegate
						{
							console.SetupForm.ATTOnTX = newAtten;
						});
					}
					else
					{
						console.SetupForm.ATTOnTX = newAtten;
					}
				}
				catch
				{
					console.SetupForm.ATTOnTX = newAtten;
				}
				LogAutoAttenuate($"SETATT  old={aTTOnTX} new={newAtten} delta={_deltadB} FB={puresignal.FeedbackLevel} {PSInfoBits()}");
				_restoreSettleTicks = (m_bQuckAttenuate ? 10 : 0);
			}
			else
			{
				LogAutoAttenuate($"SETATT  no change old={aTTOnTX} new={newAtten} delta={_deltadB} FB={puresignal.FeedbackLevel} {PSInfoBits()}");
			}
			break;
		}
		case eAAState.RestoreOperation:
			if (_restoreSettleTicks > 0)
			{
				_restoreSettleTicks--;
				break;
			}
			_autoAttenuateState = eAAState.Monitor;
			puresignal.SetPSControl(_txachannel, 0, _save_singlecalON, _save_autoON, 0);
			LogAutoAttenuate($"RESUME  FB={puresignal.FeedbackLevel} ATT={console.SetupForm.ATTOnTX} {PSInfoBits()}");
			break;
		}
	}

	private string PSInfoBits()
	{
		return $"I0={puresignal.Info[0]} I1={puresignal.Info[1]} I2={puresignal.Info[2]} I3={puresignal.Info[3]} I6={puresignal.Info[6]} C={puresignal.CalibrationCount} A={puresignal.Info[7]}";
	}

	private void LogAutoAttenuate(string message)
	{
		try
		{
			string path = Path.Combine(Application.StartupPath, "PSAutoAttenuate.log");
			string contents = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} {message}{Environment.NewLine}";
			lock (_psAutoAttenLogLock)
			{
				File.AppendAllText(path, contents);
			}
		}
		catch
		{
		}
	}

	private void PSpeak_TextChanged(object sender, EventArgs e)
	{
		if (double.TryParse(txtPSpeak.Text, out var result))
		{
			_PShwpeak = result;
			puresignal.SetPSHWPeak(_txachannel, _PShwpeak);
			UpdateWarningSetPk();
		}
	}

	public void UpdateWarningSetPk()
	{
		pbWarningSetPk.Visible = _PShwpeak != HardwareSpecific.PSDefaultPeak;
	}

	private void chkPSAutoAttenuate_CheckedChanged(object sender, EventArgs e)
	{
		AutoAttenuate = chkPSAutoAttenuate.Checked;
	}

	private void checkLoopback_CheckedChanged(object sender, EventArgs e)
	{
		if (checkLoopback.Checked && (console.SampleRateRX1 != 192000 || console.SampleRateRX2 != 192000))
		{
			MessageBox.Show("This feature can only be used with sample rates set to 192KHz.", "Sample Rate Issue", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			checkLoopback.Checked = false;
		}
		else
		{
			cmaster.PSLoopback = checkLoopback.Checked;
		}
	}

	private void chkPSStbl_CheckedChanged(object sender, EventArgs e)
	{
		if (chkPSStbl.Checked)
		{
			puresignal.SetPSStabilize(_txachannel, 1);
		}
		else
		{
			puresignal.SetPSStabilize(_txachannel, 0);
		}
	}

	private void udPSEMAAlpha_ValueChanged(object sender, EventArgs e)
	{
		puresignal.SetPSEMAAlpha(_txachannel, (double)udPSEMAAlpha.Value);
	}

	private void udPSPinAlpha_ValueChanged(object sender, EventArgs e)
	{
		puresignal.SetPSPinAlpha(_txachannel, (double)udPSPinAlpha.Value);
	}

	private void chkPSPin_CheckedChanged(object sender, EventArgs e)
	{
		puresignal.SetPSPinMode(_txachannel, chkPSPin.Checked ? 1 : 0);
	}

	private void chkPSEQ_CheckedChanged(object sender, EventArgs e)
	{
		puresignal.SetPSEQEnable(_txachannel, chkPSEQ.Checked ? 1 : 0);
	}

	private void comboPSTint_SelectedIndexChanged(object sender, EventArgs e)
	{
		switch (comboPSTint.SelectedIndex)
		{
		case 0:
		{
			puresignal.SetPSIntsAndSpi(_txachannel, 16, 256);
			_ints = 16;
			_spi = 256;
			ButtonTS buttonTS4 = btnPSSave;
			bool enabled = (btnPSRestore.Enabled = true);
			buttonTS4.Enabled = enabled;
			break;
		}
		case 1:
		{
			puresignal.SetPSIntsAndSpi(_txachannel, 8, 512);
			_ints = 8;
			_spi = 512;
			ButtonTS buttonTS3 = btnPSSave;
			bool enabled = (btnPSRestore.Enabled = false);
			buttonTS3.Enabled = enabled;
			break;
		}
		case 2:
		{
			puresignal.SetPSIntsAndSpi(_txachannel, 4, 1024);
			_ints = 4;
			_spi = 1024;
			ButtonTS buttonTS2 = btnPSSave;
			bool enabled = (btnPSRestore.Enabled = false);
			buttonTS2.Enabled = enabled;
			break;
		}
		default:
		{
			puresignal.SetPSIntsAndSpi(_txachannel, 16, 256);
			_ints = 16;
			_spi = 256;
			ButtonTS buttonTS = btnPSSave;
			bool enabled = (btnPSRestore.Enabled = true);
			buttonTS.Enabled = enabled;
			break;
		}
		}
	}

	private void udPSOutlierSigma_ValueChanged(object sender, EventArgs e)
	{
		if (_psFormReady && console != null && console.SetupForm != null)
		{
			console.SetupForm.PSOutlierSigma = (double)udPSOutlierSigma.Value;
		}
		if (chkPSOutlierEnable.Checked)
		{
			puresignal.SetPSOutlierSigma(_txachannel, (double)udPSOutlierSigma.Value);
		}
	}

	private void chkPSOutlierEnable_CheckedChanged(object sender, EventArgs e)
	{
		if (_psFormReady && console != null && console.SetupForm != null)
		{
			console.SetupForm.PSOutlierEnable = chkPSOutlierEnable.Checked;
		}
		if (chkPSOutlierEnable.Checked)
		{
			puresignal.SetPSOutlierSigma(_txachannel, (double)udPSOutlierSigma.Value);
		}
		else
		{
			puresignal.SetPSOutlierSigma(_txachannel, 0.0);
		}
	}

	private void udPSTargetFeedback_ValueChanged(object sender, EventArgs e)
	{
		int num = (int)udPSTargetFeedback.Value;
		puresignal.TargetFeedbackLevel = (int)Math.Round(152.0 * Math.Pow(10.0, (double)(-num) / 20.0));
		if (_psFormReady && console != null && console.SetupForm != null)
		{
			console.SetupForm.PSTargetFeedbackLevel = num;
		}
	}

	private void ApplyFBTarget()
	{
	}

	private void ApplyFBTarget_disabled()
	{
		if (NetworkIO.Supports24BitAudio && _psFormReady && console != null && !console.IsSetupFormNull)
		{
			int num = (int)udPSFBTargetOffset.Value;
			if (num < 0)
			{
				num = 0;
			}
			if (num > 31)
			{
				num = 31;
			}
			if (console.SetupForm.ATTOnTX != num)
			{
				console.SetupForm.ATTOnTX = num;
			}
		}
	}

	private void udPSFBTargetOffset_ValueChanged(object sender, EventArgs e)
	{
		ApplyFBTarget();
	}

	private void chkPSDCB_CheckedChanged(object sender, EventArgs e)
	{
		puresignal.SetPSDCBEnable(_txachannel, chkPSDCB.Checked ? 1 : 0);
	}

	private void udPSDCBCap_ValueChanged(object sender, EventArgs e)
	{
		puresignal.SetPSDCBCap(_txachannel, (double)udPSDCBCap.Value);
	}

	private void btnPSResetEngine_Click(object sender, EventArgs e)
	{
		SetPSAdvancedDefaults();
	}

	private void SetPSAdvancedDefaults()
	{
		chkPSStbl.Checked = true;
		udPSEMAAlpha.Value = 1.00m;
		udPSPinAlpha.Value = 0.10m;
		chkPSPin.Checked = true;
		chkPSEQ.Checked = true;
		comboPSTint.SelectedIndex = 0;
		udPSOutlierSigma.Value = udPSOutlierSigma.Minimum;
		chkPSOutlierEnable.Checked = false;
		chkPSOutlierEnable.Enabled = false;
		udPSOutlierSigma.Enabled = false;
		lblPSOutlierSigma.Enabled = false;
		udPSTargetFeedback.Value = 0m;
		console.SetupForm.PSTargetFeedbackLevel = 0;
		puresignal.TargetFeedbackLevel = 152;
		udPSFBTargetOffset.Value = FBTargetDefaultOffset;
		ApplyFBTarget();
		chkPSDCB.Checked = true;
		udPSDCBCap.Value = 0.25m;
		puresignal.ResetPSAdvancedParams(_txachannel);
	}

	private void btnPSAdvanced_Click(object sender, EventArgs e)
	{
		_advancedON = !_advancedON;
		setAdvancedView();
	}

	private void setAdvancedView()
	{
		if (_advancedON)
		{
			console.psform.ClientSize = new Size(560, 60);
		}
		else
		{
			console.psform.ClientSize = new Size(560, 300);
		}
		chkAdvancedViewHidden.Checked = _advancedON;
	}

	private void chkPSOnTop_CheckedChanged(object sender, EventArgs e)
	{
		_topmost = chkPSOnTop.Checked;
		base.TopMost = _topmost;
	}

	public void ShowAtStartup_LinearityForm()
	{
		base.Opacity = 0.0;
		SetupForm();
		Show();
		Common.FadeIn(this);
	}

	public void ShowAtStartup_AmpViewForm()
	{
		btnPSAmpView_Click(this, EventArgs.Empty);
	}

	private static string PSStateText(int state, bool corrApplied)
	{
		switch (state)
		{
		case 0:
			return "PS: reset";
		case 1:
			return "PS: wait TX";
		case 2:
			return "PS: mox delay";
		case 3:
			return "PS: setup";
		case 4:
			return "PS: collecting";
		case 5:
			return "PS: check";
		case 6:
			return "PS: calculating";
		case 7:
			if (!corrApplied)
			{
				return "PS: idle";
			}
			return "PS: tracking";
		case 8:
			return "PS: correcting";
		case 9:
			return "PS: turn on";
		default:
			return "PS: " + state;
		}
	}

	public void OnPSContextChanged()
	{
		if (!_bPSRunning || (!_autoON && _cmdstate == eCMDState.OFF && !puresignal.CorrectionsBeingApplied))
		{
			return;
		}
		try
		{
			puresignal.SetPSContext(_txachannel, (int)console.TXBand, console.TXFreq, 0);
		}
		catch
		{
		}
	}

	public void SaveCurveCache()
	{
		try
		{
			Directory.CreateDirectory(console.AppDataPath + "PureSignal\\");
			int num = puresignal.PSCacheSaveFileATT(_txachannel, console.AppDataPath + "PureSignal\\ps_curve_cache.bin", console.SetupForm.ATTOnTX);
			PsCalLog($"CACHE save slots={num}");
		}
		catch
		{
		}
	}

	private void PsCalLog(string details)
	{
		try
		{
			lock (_psLogLock)
			{
				if (_psLog == null)
				{
					string obj = console.AppDataPath + "PureSignal\\";
					Directory.CreateDirectory(obj);
					_psLogPath = obj + "ps_calib_log.csv";
					_psLog = new StreamWriter(_psLogPath, append: true)
					{
						AutoFlush = true
					};
					_psLog.WriteLine("time;band;freq_mhz;att;details");
				}
				if ((_psLogWrites++ & 0x3F) == 0 && _psLog.BaseStream.Length > 10485760)
				{
					_psLog.Close();
					string text = _psLogPath + ".1";
					try
					{
						if (File.Exists(text))
						{
							File.Delete(text);
						}
						File.Move(_psLogPath, text);
					}
					catch
					{
					}
					_psLog = new StreamWriter(_psLogPath, append: false)
					{
						AutoFlush = true
					};
					_psLog.WriteLine("time;band;freq_mhz;att;details");
				}
				string text2 = "";
				string text3 = "";
				string text4 = "";
				try
				{
					text2 = console.TXBand.ToString();
					text3 = console.VFOAFreq.ToString("F6");
					if (!console.IsSetupFormNull)
					{
						text4 = console.SetupForm.ATTOnTX.ToString();
					}
				}
				catch
				{
				}
				_psLog.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff};{text2};{text3};{text4};{details}");
			}
		}
		catch
		{
		}
	}

	public void HardResetPS()
	{
		try
		{
			puresignal.SetPSDCBHw24(_txachannel, NetworkIO.Supports24BitAudio ? 1 : 0);
		}
		catch
		{
		}
		PsCalLog("SESSION power-on");
		if (!_psCacheLoaded)
		{
			_psCacheLoaded = true;
			try
			{
				int num = puresignal.PSCacheLoadFile(_txachannel, console.AppDataPath + "PureSignal\\ps_curve_cache.bin");
				PsCalLog($"CACHE load slots={num}");
			}
			catch
			{
			}
		}
		if (!puresignal.TryPSHardReset(_txachannel))
		{
			try
			{
				puresignal.SetPSControl(_txachannel, 1, 0, 0, 0);
			}
			catch
			{
			}
		}
		ForcePS();
	}

	public void ForcePS()
	{
		EventArgs empty = EventArgs.Empty;
		if (!_autoON)
		{
			puresignal.SetPSControl(_txachannel, 1, 0, 0, 0);
		}
		else
		{
			puresignal.SetPSControl(_txachannel, 0, 0, 1, 0);
		}
		if (!_ttgenON)
		{
			WDSP.SetTXAPostGenRun(_txachannel, 0);
		}
		else
		{
			WDSP.SetTXAPostGenMode(_txachannel, 1);
			WDSP.SetTXAPostGenRun(_txachannel, 1);
		}
		udPSCalWait_ValueChanged(this, empty);
		udPSPhnum_ValueChanged(this, empty);
		udPSMoxDelay_ValueChanged(this, empty);
		chkPSAutoAttenuate_CheckedChanged(this, empty);
		chkPSStbl_CheckedChanged(this, empty);
		udPSEMAAlpha_ValueChanged(this, empty);
		udPSPinAlpha_ValueChanged(this, empty);
		chkPSPin_CheckedChanged(this, empty);
		chkPSEQ_CheckedChanged(this, empty);
		comboPSTint_SelectedIndexChanged(this, empty);
		udPSTargetFeedback.Value = console.SetupForm.PSTargetFeedbackLevel;
		udPSTargetFeedback_ValueChanged(this, empty);
		udPSOutlierSigma.Value = (decimal)console.SetupForm.PSOutlierSigma;
		chkPSOutlierEnable.Checked = console.SetupForm.PSOutlierEnable;
		chkPSOutlierEnable_CheckedChanged(this, empty);
		udPSOutlierSigma_ValueChanged(this, empty);
		chkPSDCB_CheckedChanged(this, empty);
		udPSDCBCap_ValueChanged(this, empty);
		chkPSOnTop_CheckedChanged(this, empty);
		chkQuickAttenuate_CheckedChanged(this, empty);
		chkShow2ToneMeasurements_CheckedChanged(this, empty);
	}

	private void chkQuickAttenuate_CheckedChanged(object sender, EventArgs e)
	{
		QuickAttenuate = chkQuickAttenuate.Checked;
	}

	private void btnDefaultPeaks_Click(object sender, EventArgs e)
	{
		SetDefaultPeaks();
	}

	private void chkShow2ToneMeasurements_CheckedChanged(object sender, EventArgs e)
	{
		Display.ShowIMDMeasurments = chkShow2ToneMeasurements.Checked;
	}

	public void FixAmpViewOnTop()
	{
		if (ampv != null && !ampv.IsDisposed)
		{
			ampv.FixOnTop();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.PSForm));
		this.chkPSOnTop = new System.Windows.Forms.CheckBoxTS();
		this.btnPSResetEngine = new System.Windows.Forms.ButtonTS();
		this.comboPSTint = new System.Windows.Forms.ComboBoxTS();
		this.udPSOutlierSigma = new System.Windows.Forms.NumericUpDownTS();
		this.lblPSOutlierSigma = new System.Windows.Forms.LabelTS();
		this.chkPSOutlierEnable = new System.Windows.Forms.CheckBoxTS();
		this.udPSTargetFeedback = new System.Windows.Forms.NumericUpDownTS();
		this.udPSFBTargetOffset = new System.Windows.Forms.NumericUpDownTS();
		this.lblPSFBTargetOffset = new System.Windows.Forms.LabelTS();
		this.lblPSTargetFeedback = new System.Windows.Forms.LabelTS();
		this.chkPSEQ = new System.Windows.Forms.CheckBoxTS();
		this.chkPSPin = new System.Windows.Forms.CheckBoxTS();
		this.udPSDCBCap = new System.Windows.Forms.NumericUpDownTS();
		this.lblPSDCBCap = new System.Windows.Forms.LabelTS();
		this.chkPSDCB = new System.Windows.Forms.CheckBoxTS();
		this.udPSPinAlpha = new System.Windows.Forms.NumericUpDownTS();
		this.lblPSPinAlpha = new System.Windows.Forms.LabelTS();
		this.udPSEMAAlpha = new System.Windows.Forms.NumericUpDownTS();
		this.lblPSEMAAlpha = new System.Windows.Forms.LabelTS();
		this.btnPSRestore = new System.Windows.Forms.ButtonTS();
		this.btnPSSave = new System.Windows.Forms.ButtonTS();
		this.btnPSAdvanced = new System.Windows.Forms.ButtonTS();
		this.chkPSStbl = new System.Windows.Forms.CheckBoxTS();
		this.chkPSAutoAttenuate = new System.Windows.Forms.CheckBoxTS();
		this.btnPSAmpView = new System.Windows.Forms.ButtonTS();
		this.btnPSTwoToneGen = new System.Windows.Forms.ButtonTS();
		this.labelTS8 = new System.Windows.Forms.LabelTS();
		this.lblPSInfoFB = new System.Windows.Forms.LabelTS();
		this.lblPSInfoCO = new System.Windows.Forms.LabelTS();
		this.labelTS9 = new System.Windows.Forms.LabelTS();
		this.labelTS4 = new System.Windows.Forms.LabelTS();
		this.udPSMoxDelay = new System.Windows.Forms.NumericUpDownTS();
		this.labelTS2 = new System.Windows.Forms.LabelTS();
		this.udPSPhnum = new System.Windows.Forms.NumericUpDownTS();
		this.grpPSInfo = new System.Windows.Forms.GroupBoxTS();
		this.btnDefaultPeaks = new System.Windows.Forms.ButtonTS();
		this.checkLoopback = new System.Windows.Forms.CheckBoxTS();
		this.lblPSInfo5 = new System.Windows.Forms.LabelTS();
		this.labelTS13 = new System.Windows.Forms.LabelTS();
		this.lblPSInfo13 = new System.Windows.Forms.LabelTS();
		this.labelTS11 = new System.Windows.Forms.LabelTS();
		this.lblPSInfo6 = new System.Windows.Forms.LabelTS();
		this.labelTS7 = new System.Windows.Forms.LabelTS();
		this.GetPSpeak = new System.Windows.Forms.TextBoxTS();
		this.labelTS3 = new System.Windows.Forms.LabelTS();
		this.txtPSpeak = new System.Windows.Forms.TextBoxTS();
		this.labelTS5 = new System.Windows.Forms.LabelTS();
		this.lblPSfb2 = new System.Windows.Forms.LabelTS();
		this.labelTS1 = new System.Windows.Forms.LabelTS();
		this.lblPSInfo15 = new System.Windows.Forms.LabelTS();
		this.labelTS146 = new System.Windows.Forms.LabelTS();
		this.lblPSInfo3 = new System.Windows.Forms.LabelTS();
		this.lblPSInfo2 = new System.Windows.Forms.LabelTS();
		this.lblPSInfo1 = new System.Windows.Forms.LabelTS();
		this.lblPSInfo0 = new System.Windows.Forms.LabelTS();
		this.labelTS143 = new System.Windows.Forms.LabelTS();
		this.labelTS144 = new System.Windows.Forms.LabelTS();
		this.labelTS142 = new System.Windows.Forms.LabelTS();
		this.labelTS141 = new System.Windows.Forms.LabelTS();
		this.btnPSReset = new System.Windows.Forms.ButtonTS();
		this.btnPSCalibrate = new System.Windows.Forms.ButtonTS();
		this.labelTS140 = new System.Windows.Forms.LabelTS();
		this.udPSCalWait = new System.Windows.Forms.NumericUpDownTS();
		this.chkQuickAttenuate = new System.Windows.Forms.CheckBoxTS();
		this.chkAdvancedViewHidden = new System.Windows.Forms.CheckBoxTS();
		this.pbWarningSetPk = new System.Windows.Forms.PictureBox();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		this.chkShow2ToneMeasurements = new System.Windows.Forms.CheckBoxTS();
		((System.ComponentModel.ISupportInitialize)this.udPSMoxDelay).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udPSPhnum).BeginInit();
		this.grpPSInfo.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.udPSCalWait).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udPSTargetFeedback).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.udPSFBTargetOffset).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.pbWarningSetPk).BeginInit();
		base.SuspendLayout();
		this.chkPSOnTop.AutoSize = true;
		this.chkPSOnTop.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.chkPSOnTop.Image = null;
		this.chkPSOnTop.Location = new System.Drawing.Point(450, 276);
		this.chkPSOnTop.Name = "chkPSOnTop";
		this.chkPSOnTop.Size = new System.Drawing.Size(98, 17);
		this.chkPSOnTop.TabIndex = 48;
		this.chkPSOnTop.Text = "Always On Top";
		this.chkPSOnTop.UseVisualStyleBackColor = true;
		this.chkPSOnTop.CheckedChanged += new System.EventHandler(chkPSOnTop_CheckedChanged);
		this.btnPSRestore.BackColor = System.Drawing.SystemColors.Control;
		this.btnPSRestore.ForeColor = System.Drawing.SystemColors.ControlText;
		this.btnPSRestore.Image = null;
		this.btnPSRestore.Location = new System.Drawing.Point(399, 12);
		this.btnPSRestore.Name = "btnPSRestore";
		this.btnPSRestore.Selectable = true;
		this.btnPSRestore.Size = new System.Drawing.Size(71, 20);
		this.btnPSRestore.TabIndex = 0;
		this.btnPSRestore.Text = "Restore";
		this.btnPSRestore.UseVisualStyleBackColor = false;
		this.btnPSRestore.Click += new System.EventHandler(btnPSRestore_Click);
		this.btnPSSave.BackColor = System.Drawing.SystemColors.Control;
		this.btnPSSave.ForeColor = System.Drawing.SystemColors.ControlText;
		this.btnPSSave.Image = null;
		this.btnPSSave.Location = new System.Drawing.Point(322, 12);
		this.btnPSSave.Name = "btnPSSave";
		this.btnPSSave.Selectable = true;
		this.btnPSSave.Size = new System.Drawing.Size(71, 20);
		this.btnPSSave.TabIndex = 4;
		this.btnPSSave.Text = "Save";
		this.btnPSSave.UseVisualStyleBackColor = false;
		this.btnPSSave.Click += new System.EventHandler(btnPSSave_Click);
		this.btnPSAdvanced.BackColor = System.Drawing.SystemColors.Control;
		this.btnPSAdvanced.Image = null;
		this.btnPSAdvanced.Location = new System.Drawing.Point(245, 12);
		this.btnPSAdvanced.Name = "btnPSAdvanced";
		this.btnPSAdvanced.Selectable = true;
		this.btnPSAdvanced.Size = new System.Drawing.Size(71, 20);
		this.btnPSAdvanced.TabIndex = 46;
		this.btnPSAdvanced.Text = "Advanced";
		this.btnPSAdvanced.UseVisualStyleBackColor = false;
		this.btnPSAdvanced.Click += new System.EventHandler(btnPSAdvanced_Click);
		this.lblPSEMAAlpha.AutoSize = true;
		this.lblPSEMAAlpha.ForeColor = System.Drawing.Color.White;
		this.lblPSEMAAlpha.Image = null;
		this.lblPSEMAAlpha.Location = new System.Drawing.Point(434, 65);
		this.lblPSEMAAlpha.Name = "lblPSEMAAlpha";
		this.lblPSEMAAlpha.Size = new System.Drawing.Size(45, 13);
		this.lblPSEMAAlpha.TabIndex = 60;
		this.lblPSEMAAlpha.Text = "EMA α";
		this.udPSEMAAlpha.DecimalPlaces = 2;
		this.udPSEMAAlpha.Increment = new decimal(new int[4] { 5, 0, 0, 131072 });
		this.udPSEMAAlpha.Location = new System.Drawing.Point(490, 63);
		this.udPSEMAAlpha.Maximum = new decimal(new int[4] { 200, 0, 0, 131072 });
		this.udPSEMAAlpha.Minimum = new decimal(new int[4]);
		this.udPSEMAAlpha.Name = "udPSEMAAlpha";
		this.udPSEMAAlpha.Size = new System.Drawing.Size(50, 20);
		this.udPSEMAAlpha.TabIndex = 61;
		this.udPSEMAAlpha.Value = new decimal(new int[4] { 100, 0, 0, 131072 });
		this.toolTip1.SetToolTip(this.udPSEMAAlpha, "EMA alpha across calibration cycles (0 = frozen, 1 = no smoothing, range 0.00–2.00).");
		this.udPSEMAAlpha.ValueChanged += new System.EventHandler(udPSEMAAlpha_ValueChanged);
		this.lblPSPinAlpha.AutoSize = true;
		this.lblPSPinAlpha.ForeColor = System.Drawing.Color.White;
		this.lblPSPinAlpha.Image = null;
		this.lblPSPinAlpha.Location = new System.Drawing.Point(434, 88);
		this.lblPSPinAlpha.Name = "lblPSPinAlpha";
		this.lblPSPinAlpha.Size = new System.Drawing.Size(40, 13);
		this.lblPSPinAlpha.TabIndex = 62;
		this.lblPSPinAlpha.Text = "Pin α";
		this.udPSPinAlpha.DecimalPlaces = 2;
		this.udPSPinAlpha.Increment = new decimal(new int[4] { 5, 0, 0, 131072 });
		this.udPSPinAlpha.Location = new System.Drawing.Point(490, 86);
		this.udPSPinAlpha.Maximum = new decimal(new int[4] { 200, 0, 0, 131072 });
		this.udPSPinAlpha.Minimum = new decimal(new int[4]);
		this.udPSPinAlpha.Name = "udPSPinAlpha";
		this.udPSPinAlpha.Size = new System.Drawing.Size(50, 20);
		this.udPSPinAlpha.TabIndex = 63;
		this.udPSPinAlpha.Value = new decimal(new int[4] { 10, 0, 0, 131072 });
		this.toolTip1.SetToolTip(this.udPSPinAlpha, "COS/SIN start-point pin smoothing. Range 0.00–2.00.");
		this.udPSPinAlpha.ValueChanged += new System.EventHandler(udPSPinAlpha_ValueChanged);
		this.chkPSEQ.AutoSize = true;
		this.chkPSEQ.Checked = true;
		this.chkPSEQ.CheckState = System.Windows.Forms.CheckState.Checked;
		this.chkPSEQ.ForeColor = System.Drawing.SystemColors.HighlightText;
		this.chkPSEQ.Image = null;
		this.chkPSEQ.Location = new System.Drawing.Point(434, 155);
		this.chkPSEQ.Name = "chkPSEQ";
		this.chkPSEQ.Size = new System.Drawing.Size(40, 17);
		this.chkPSEQ.TabIndex = 64;
		this.chkPSEQ.Text = "EQ";
		this.toolTip1.SetToolTip(this.chkPSEQ, "Enable density equalization of feedback samples before NURBS fitting.");
		this.chkPSEQ.UseVisualStyleBackColor = true;
		this.chkPSEQ.CheckedChanged += new System.EventHandler(chkPSEQ_CheckedChanged);
		this.chkPSPin.AutoSize = true;
		this.chkPSPin.Checked = true;
		this.chkPSPin.CheckState = System.Windows.Forms.CheckState.Checked;
		this.chkPSPin.ForeColor = System.Drawing.SystemColors.HighlightText;
		this.chkPSPin.Image = null;
		this.chkPSPin.Location = new System.Drawing.Point(480, 155);
		this.chkPSPin.Name = "chkPSPin";
		this.chkPSPin.Size = new System.Drawing.Size(40, 17);
		this.chkPSPin.TabIndex = 70;
		this.chkPSPin.Text = "Pin";
		this.toolTip1.SetToolTip(this.chkPSPin, "Pin COS/SIN NURBS curves at the low-drive start point.");
		this.chkPSPin.UseVisualStyleBackColor = true;
		this.chkPSPin.CheckedChanged += new System.EventHandler(chkPSPin_CheckedChanged);
		this.udPSOutlierSigma.DecimalPlaces = 1;
		this.udPSOutlierSigma.Increment = new decimal(new int[4] { 1, 0, 0, 65536 });
		this.udPSOutlierSigma.Location = new System.Drawing.Point(505, 109);
		this.udPSOutlierSigma.Maximum = new decimal(new int[4] { 50, 0, 0, 65536 });
		this.udPSOutlierSigma.Minimum = new decimal(new int[4] { 1, 0, 0, 65536 });
		this.udPSOutlierSigma.Name = "udPSOutlierSigma";
		this.udPSOutlierSigma.Size = new System.Drawing.Size(50, 20);
		this.udPSOutlierSigma.TabIndex = 65;
		this.udPSOutlierSigma.Value = new decimal(new int[4] { 25, 0, 0, 65536 });
		this.toolTip1.SetToolTip(this.udPSOutlierSigma, "Outlier-rejection sigma. Range 0.1–5.0 (lower = more aggressive culling). Default 5.0 for Orion MK2 rigs.");
		this.udPSOutlierSigma.ValueChanged += new System.EventHandler(udPSOutlierSigma_ValueChanged);
		this.lblPSOutlierSigma.AutoSize = true;
		this.lblPSOutlierSigma.ForeColor = System.Drawing.Color.White;
		this.lblPSOutlierSigma.Image = null;
		this.lblPSOutlierSigma.Location = new System.Drawing.Point(485, 111);
		this.lblPSOutlierSigma.Name = "lblPSOutlierSigma";
		this.lblPSOutlierSigma.Size = new System.Drawing.Size(14, 13);
		this.lblPSOutlierSigma.TabIndex = 69;
		this.lblPSOutlierSigma.Text = "σ";
		this.chkPSOutlierEnable.AutoSize = true;
		this.chkPSOutlierEnable.ForeColor = System.Drawing.SystemColors.HighlightText;
		this.chkPSOutlierEnable.Image = null;
		this.chkPSOutlierEnable.Location = new System.Drawing.Point(434, 111);
		this.chkPSOutlierEnable.Name = "chkPSOutlierEnable";
		this.chkPSOutlierEnable.Size = new System.Drawing.Size(54, 17);
		this.chkPSOutlierEnable.TabIndex = 72;
		this.chkPSOutlierEnable.Text = "Outlier";
		this.toolTip1.SetToolTip(this.chkPSOutlierEnable, "Enable robust outlier rejection before cubic-spline fitting. Default ON at sigma 5.0 for Orion MK2 rigs (ANAN-7000/8000/Anvelina PRO3).");
		this.chkPSOutlierEnable.UseVisualStyleBackColor = true;
		this.chkPSOutlierEnable.CheckedChanged += new System.EventHandler(chkPSOutlierEnable_CheckedChanged);
		this.udPSTargetFeedback.Location = new System.Drawing.Point(490, 131);
		this.udPSTargetFeedback.Maximum = new decimal(new int[4] { 20, 0, 0, 0 });
		this.udPSTargetFeedback.Minimum = new decimal(new int[4]);
		this.udPSTargetFeedback.Name = "udPSTargetFeedback";
		this.udPSTargetFeedback.Size = new System.Drawing.Size(50, 20);
		this.udPSTargetFeedback.TabIndex = 73;
		this.udPSTargetFeedback.Value = new decimal(new int[4]);
		this.toolTip1.SetToolTip(this.udPSTargetFeedback, "Extra PS feedback attenuation vs stock, dB (eu2av). 0 = exact stock (auto-att target 152). 10 = auto-attenuate settles with 10 dB MORE attenuation (feedback ~10 dB weaker - headroom against RX overload with a PA). The indicator zones and auto-att thresholds follow synchronously.");
		this.udPSTargetFeedback.ValueChanged += new System.EventHandler(udPSTargetFeedback_ValueChanged);
		this.lblPSTargetFeedback.AutoSize = true;
		this.lblPSTargetFeedback.ForeColor = System.Drawing.Color.White;
		this.lblPSTargetFeedback.Image = null;
		this.lblPSTargetFeedback.Location = new System.Drawing.Point(434, 133);
		this.lblPSTargetFeedback.Name = "lblPSTargetFeedback";
		this.lblPSTargetFeedback.Size = new System.Drawing.Size(54, 13);
		this.lblPSTargetFeedback.TabIndex = 74;
		this.lblPSTargetFeedback.Text = "ATT +dB";
		this.chkPSDCB.AutoSize = true;
		this.chkPSDCB.ForeColor = System.Drawing.SystemColors.HighlightText;
		this.chkPSDCB.Image = null;
		this.chkPSDCB.Location = new System.Drawing.Point(434, 178);
		this.chkPSDCB.Name = "chkPSDCB";
		this.chkPSDCB.Size = new System.Drawing.Size(46, 17);
		this.chkPSDCB.TabIndex = 66;
		this.chkPSDCB.Text = "DCB";
		this.toolTip1.SetToolTip(this.chkPSDCB, "Enable DC-balance / anchor correction for noisy low-level feedback.");
		this.chkPSDCB.UseVisualStyleBackColor = true;
		this.chkPSDCB.CheckedChanged += new System.EventHandler(chkPSDCB_CheckedChanged);
		this.udPSDCBCap.DecimalPlaces = 2;
		this.udPSDCBCap.Increment = new decimal(new int[4] { 5, 0, 0, 131072 });
		this.udPSDCBCap.Location = new System.Drawing.Point(510, 176);
		this.udPSDCBCap.Maximum = new decimal(new int[4] { 100, 0, 0, 131072 });
		this.udPSDCBCap.Minimum = new decimal(new int[4]);
		this.udPSDCBCap.Name = "udPSDCBCap";
		this.udPSDCBCap.Size = new System.Drawing.Size(40, 20);
		this.udPSDCBCap.TabIndex = 67;
		this.udPSDCBCap.Value = new decimal(new int[4] { 25, 0, 0, 131072 });
		this.toolTip1.SetToolTip(this.udPSDCBCap, "DC-balance cap (max anchor magnitude).");
		this.udPSDCBCap.ValueChanged += new System.EventHandler(udPSDCBCap_ValueChanged);
		this.lblPSDCBCap.AutoSize = true;
		this.lblPSDCBCap.ForeColor = System.Drawing.Color.White;
		this.lblPSDCBCap.Image = null;
		this.lblPSDCBCap.Location = new System.Drawing.Point(480, 178);
		this.lblPSDCBCap.Name = "lblPSDCBCap";
		this.lblPSDCBCap.Size = new System.Drawing.Size(26, 13);
		this.lblPSDCBCap.TabIndex = 70;
		this.lblPSDCBCap.Text = "Cap";
		this.chkPSStbl.AutoSize = true;
		this.chkPSStbl.Checked = true;
		this.chkPSStbl.CheckState = System.Windows.Forms.CheckState.Checked;
		this.chkPSStbl.ForeColor = System.Drawing.SystemColors.HighlightText;
		this.chkPSStbl.Image = null;
		this.chkPSStbl.Location = new System.Drawing.Point(434, 201);
		this.chkPSStbl.Name = "chkPSStbl";
		this.chkPSStbl.Size = new System.Drawing.Size(53, 17);
		this.chkPSStbl.TabIndex = 44;
		this.chkPSStbl.Text = "STBL";
		this.toolTip1.SetToolTip(this.chkPSStbl, "Enable EMA smoothing of calibration curves across cycles.");
		this.chkPSStbl.UseVisualStyleBackColor = true;
		this.chkPSStbl.CheckedChanged += new System.EventHandler(chkPSStbl_CheckedChanged);
		this.btnPSResetEngine.BackColor = System.Drawing.SystemColors.Control;
		this.btnPSResetEngine.ForeColor = System.Drawing.SystemColors.ControlText;
		this.btnPSResetEngine.Image = null;
		this.btnPSResetEngine.Location = new System.Drawing.Point(434, 224);
		this.btnPSResetEngine.Name = "btnPSResetEngine";
		this.btnPSResetEngine.Selectable = true;
		this.btnPSResetEngine.Size = new System.Drawing.Size(100, 20);
		this.btnPSResetEngine.TabIndex = 68;
		this.btnPSResetEngine.Text = "Reset PSA defaults";
		this.toolTip1.SetToolTip(this.btnPSResetEngine, "Restore WDSP 2.00 recommended PureSignal default parameters.");
		this.btnPSResetEngine.UseVisualStyleBackColor = false;
		this.btnPSResetEngine.Click += new System.EventHandler(btnPSResetEngine_Click);
		this.comboPSTint.ForeColor = System.Drawing.Color.Black;
		this.comboPSTint.FormattingEnabled = true;
		this.comboPSTint.Items.AddRange(new object[3] { "0.5", "1.1", "2.5" });
		this.comboPSTint.Location = new System.Drawing.Point(434, 250);
		this.comboPSTint.Name = "comboPSTint";
		this.comboPSTint.Size = new System.Drawing.Size(100, 21);
		this.comboPSTint.TabIndex = 71;
		this.comboPSTint.Text = "0.5";
		this.toolTip1.SetToolTip(this.comboPSTint, "Bucket configuration: 0.5=16x256, 1.1=8x512, 2.5=4x1024.");
		this.comboPSTint.SelectedIndexChanged += new System.EventHandler(comboPSTint_SelectedIndexChanged);
		this.chkPSAutoAttenuate.AutoSize = true;
		this.chkPSAutoAttenuate.Checked = true;
		this.chkPSAutoAttenuate.CheckState = System.Windows.Forms.CheckState.Checked;
		this.chkPSAutoAttenuate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.chkPSAutoAttenuate.ForeColor = System.Drawing.SystemColors.HighlightText;
		this.chkPSAutoAttenuate.Image = null;
		this.chkPSAutoAttenuate.Location = new System.Drawing.Point(208, 61);
		this.chkPSAutoAttenuate.Name = "chkPSAutoAttenuate";
		this.chkPSAutoAttenuate.Size = new System.Drawing.Size(97, 17);
		this.chkPSAutoAttenuate.TabIndex = 41;
		this.chkPSAutoAttenuate.Text = "Auto-Attenuate";
		this.toolTip1.SetToolTip(this.chkPSAutoAttenuate, "Automatically adjust attenuator for optimum feedback level. (Recommended)");
		this.chkPSAutoAttenuate.UseVisualStyleBackColor = true;
		this.chkPSAutoAttenuate.CheckedChanged += new System.EventHandler(chkPSAutoAttenuate_CheckedChanged);
		this.btnPSAmpView.BackColor = System.Drawing.SystemColors.Control;
		this.btnPSAmpView.ForeColor = System.Drawing.SystemColors.ControlText;
		this.btnPSAmpView.Image = null;
		this.btnPSAmpView.Location = new System.Drawing.Point(168, 12);
		this.btnPSAmpView.Name = "btnPSAmpView";
		this.btnPSAmpView.Selectable = true;
		this.btnPSAmpView.Size = new System.Drawing.Size(71, 20);
		this.btnPSAmpView.TabIndex = 40;
		this.btnPSAmpView.Text = "AmpView";
		this.btnPSAmpView.UseVisualStyleBackColor = false;
		this.btnPSAmpView.Click += new System.EventHandler(btnPSAmpView_Click);
		this.btnPSTwoToneGen.BackColor = System.Drawing.SystemColors.Control;
		this.btnPSTwoToneGen.ForeColor = System.Drawing.SystemColors.ControlText;
		this.btnPSTwoToneGen.Image = null;
		this.btnPSTwoToneGen.Location = new System.Drawing.Point(14, 12);
		this.btnPSTwoToneGen.Name = "btnPSTwoToneGen";
		this.btnPSTwoToneGen.Selectable = true;
		this.btnPSTwoToneGen.Size = new System.Drawing.Size(71, 20);
		this.btnPSTwoToneGen.TabIndex = 37;
		this.btnPSTwoToneGen.Text = "Two-tone";
		this.toolTip1.SetToolTip(this.btnPSTwoToneGen, "Generate and TX a Two Tone Signal");
		this.btnPSTwoToneGen.UseVisualStyleBackColor = false;
		this.btnPSTwoToneGen.Click += new System.EventHandler(btnPSTwoToneGen_Click);
		this.labelTS8.AutoSize = true;
		this.labelTS8.ForeColor = System.Drawing.Color.White;
		this.labelTS8.Image = null;
		this.labelTS8.Location = new System.Drawing.Point(32, 40);
		this.labelTS8.Name = "labelTS8";
		this.labelTS8.Size = new System.Drawing.Size(84, 13);
		this.labelTS8.TabIndex = 10;
		this.labelTS8.Text = "Feedback Level";
		this.toolTip1.SetToolTip(this.labelTS8, "Indicates, by color, correct/incorrect RF feedback level");
		this.lblPSInfoFB.BackColor = System.Drawing.Color.Black;
		this.lblPSInfoFB.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblPSInfoFB.ForeColor = System.Drawing.Color.Black;
		this.lblPSInfoFB.Image = null;
		this.lblPSInfoFB.Location = new System.Drawing.Point(14, 41);
		this.lblPSInfoFB.Name = "lblPSInfoFB";
		this.lblPSInfoFB.Size = new System.Drawing.Size(12, 12);
		this.lblPSInfoFB.TabIndex = 11;
		this.toolTip1.SetToolTip(this.lblPSInfoFB, "Indicates, by color, correct/incorrect RF feedback level");
		this.lblPSInfoCO.BackColor = System.Drawing.Color.Black;
		this.lblPSInfoCO.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblPSInfoCO.ForeColor = System.Drawing.Color.Black;
		this.lblPSInfoCO.Image = null;
		this.lblPSInfoCO.Location = new System.Drawing.Point(168, 41);
		this.lblPSInfoCO.Name = "lblPSInfoCO";
		this.lblPSInfoCO.Size = new System.Drawing.Size(12, 12);
		this.lblPSInfoCO.TabIndex = 13;
		this.toolTip1.SetToolTip(this.lblPSInfoCO, "If green, a correction solution is in place and PureSignal is correcting");
		this.labelTS9.AutoSize = true;
		this.labelTS9.ForeColor = System.Drawing.Color.White;
		this.labelTS9.Image = null;
		this.labelTS9.Location = new System.Drawing.Point(186, 40);
		this.labelTS9.Name = "labelTS9";
		this.labelTS9.Size = new System.Drawing.Size(55, 13);
		this.labelTS9.TabIndex = 12;
		this.labelTS9.Text = "Correcting";
		this.toolTip1.SetToolTip(this.labelTS9, "If green, a correction solution is in place and PureSignal is correcting");
		this.labelTS4.AutoSize = true;
		this.labelTS4.ForeColor = System.Drawing.Color.White;
		this.labelTS4.Image = null;
		this.labelTS4.Location = new System.Drawing.Point(11, 63);
		this.labelTS4.Name = "labelTS4";
		this.labelTS4.Size = new System.Drawing.Size(82, 13);
		this.labelTS4.TabIndex = 30;
		this.labelTS4.Text = "MOX Wait (sec)";
		this.udPSMoxDelay.DecimalPlaces = 1;
		this.udPSMoxDelay.Increment = new decimal(new int[4] { 1, 0, 0, 65536 });
		this.udPSMoxDelay.Location = new System.Drawing.Point(99, 61);
		this.udPSMoxDelay.Maximum = new decimal(new int[4] { 10, 0, 0, 65536 });
		this.udPSMoxDelay.Minimum = new decimal(new int[4] { 1, 0, 0, 65536 });
		this.udPSMoxDelay.Name = "udPSMoxDelay";
		this.udPSMoxDelay.Size = new System.Drawing.Size(51, 20);
		this.udPSMoxDelay.TabIndex = 29;
		this.udPSMoxDelay.TinyStep = false;
		this.toolTip1.SetToolTip(this.udPSMoxDelay, "Settling time between assertion of MOX and collection of feedback");
		this.udPSMoxDelay.Value = new decimal(new int[4] { 2, 0, 0, 65536 });
		this.udPSMoxDelay.ValueChanged += new System.EventHandler(udPSMoxDelay_ValueChanged);
		this.labelTS2.AutoSize = true;
		this.labelTS2.ForeColor = System.Drawing.Color.White;
		this.labelTS2.Image = null;
		this.labelTS2.Location = new System.Drawing.Point(11, 117);
		this.labelTS2.Name = "labelTS2";
		this.labelTS2.Size = new System.Drawing.Size(80, 13);
		this.labelTS2.TabIndex = 26;
		this.labelTS2.Text = "AMP Delay (ns)";
		this.udPSPhnum.Increment = new decimal(new int[4] { 20, 0, 0, 0 });
		this.udPSPhnum.Location = new System.Drawing.Point(99, 115);
		this.udPSPhnum.Maximum = new decimal(new int[4] { 25000000, 0, 0, 0 });
		this.udPSPhnum.Minimum = new decimal(new int[4]);
		this.udPSPhnum.Name = "udPSPhnum";
		this.udPSPhnum.Size = new System.Drawing.Size(80, 20);
		this.udPSPhnum.TabIndex = 25;
		this.udPSPhnum.TinyStep = false;
		this.toolTip1.SetToolTip(this.udPSPhnum, "Compensation delay for the analog PA chain");
		this.udPSPhnum.Value = new decimal(new int[4] { 150, 0, 0, 0 });
		this.udPSPhnum.ValueChanged += new System.EventHandler(udPSPhnum_ValueChanged);
		this.grpPSInfo.Controls.Add(this.btnDefaultPeaks);
		this.grpPSInfo.Controls.Add(this.checkLoopback);
		this.grpPSInfo.Controls.Add(this.lblPSInfo5);
		this.grpPSInfo.Controls.Add(this.labelTS13);
		this.grpPSInfo.Controls.Add(this.lblPSInfo13);
		this.grpPSInfo.Controls.Add(this.labelTS11);
		this.grpPSInfo.Controls.Add(this.lblPSInfo6);
		this.grpPSInfo.Controls.Add(this.labelTS7);
		this.grpPSInfo.Controls.Add(this.GetPSpeak);
		this.grpPSInfo.Controls.Add(this.labelTS3);
		this.grpPSInfo.Controls.Add(this.txtPSpeak);
		this.grpPSInfo.Controls.Add(this.labelTS5);
		this.grpPSInfo.Controls.Add(this.lblPSfb2);
		this.grpPSInfo.Controls.Add(this.labelTS1);
		this.grpPSInfo.Controls.Add(this.lblPSInfo15);
		this.grpPSInfo.Controls.Add(this.labelTS146);
		this.grpPSInfo.Controls.Add(this.lblPSInfo3);
		this.grpPSInfo.Controls.Add(this.lblPSInfo2);
		this.grpPSInfo.Controls.Add(this.lblPSInfo1);
		this.grpPSInfo.Controls.Add(this.lblPSInfo0);
		this.grpPSInfo.Controls.Add(this.labelTS143);
		this.grpPSInfo.Controls.Add(this.labelTS144);
		this.grpPSInfo.Controls.Add(this.labelTS142);
		this.grpPSInfo.Controls.Add(this.labelTS141);
		this.grpPSInfo.ForeColor = System.Drawing.Color.White;
		this.grpPSInfo.Location = new System.Drawing.Point(14, 145);
		this.grpPSInfo.Name = "grpPSInfo";
		this.grpPSInfo.Size = new System.Drawing.Size(358, 148);
		this.grpPSInfo.TabIndex = 21;
		this.grpPSInfo.TabStop = false;
		this.grpPSInfo.Text = "Calibration Information";
		this.btnDefaultPeaks.BackColor = System.Drawing.Color.White;
		this.btnDefaultPeaks.ForeColor = System.Drawing.Color.Black;
		this.btnDefaultPeaks.Image = null;
		this.btnDefaultPeaks.Location = new System.Drawing.Point(277, 117);
		this.btnDefaultPeaks.Name = "btnDefaultPeaks";
		this.btnDefaultPeaks.Selectable = true;
		this.btnDefaultPeaks.Size = new System.Drawing.Size(67, 23);
		this.btnDefaultPeaks.TabIndex = 41;
		this.btnDefaultPeaks.Text = "Default";
		this.toolTip1.SetToolTip(this.btnDefaultPeaks, "Set the default peak level of expected digital TX feedback for the current hardware");
		this.btnDefaultPeaks.UseVisualStyleBackColor = false;
		this.btnDefaultPeaks.Click += new System.EventHandler(btnDefaultPeaks_Click);
		this.checkLoopback.AutoSize = true;
		this.checkLoopback.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.checkLoopback.ForeColor = System.Drawing.SystemColors.HighlightText;
		this.checkLoopback.Image = null;
		this.checkLoopback.Location = new System.Drawing.Point(9, 121);
		this.checkLoopback.Name = "checkLoopback";
		this.checkLoopback.Size = new System.Drawing.Size(188, 17);
		this.checkLoopback.TabIndex = 40;
		this.checkLoopback.Text = "Display PS-RX and PS-TX spectra";
		this.toolTip1.SetToolTip(this.checkLoopback, "Use top and bottom panadapters to display the two feedback streams.");
		this.checkLoopback.UseVisualStyleBackColor = true;
		this.checkLoopback.CheckedChanged += new System.EventHandler(checkLoopback_CheckedChanged);
		this.lblPSInfo5.AutoSize = true;
		this.lblPSInfo5.BackColor = System.Drawing.Color.Bisque;
		this.lblPSInfo5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblPSInfo5.ForeColor = System.Drawing.Color.Black;
		this.lblPSInfo5.Image = null;
		this.lblPSInfo5.Location = new System.Drawing.Point(194, 72);
		this.lblPSInfo5.Name = "lblPSInfo5";
		this.lblPSInfo5.Size = new System.Drawing.Size(2, 15);
		this.lblPSInfo5.TabIndex = 21;
		this.toolTip1.SetToolTip(this.lblPSInfo5, "Indicator:  cumulative number of new correction solutions.");
		this.labelTS13.AutoSize = true;
		this.labelTS13.Image = null;
		this.labelTS13.Location = new System.Drawing.Point(128, 72);
		this.labelTS13.Name = "labelTS13";
		this.labelTS13.Size = new System.Drawing.Size(40, 13);
		this.labelTS13.TabIndex = 20;
		this.labelTS13.Text = "cor.cnt";
		this.toolTip1.SetToolTip(this.labelTS13, "Indicator:  cumulative number of new correction solutions.");
		this.lblPSInfo13.AutoSize = true;
		this.lblPSInfo13.BackColor = System.Drawing.Color.Bisque;
		this.lblPSInfo13.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblPSInfo13.ForeColor = System.Drawing.Color.Black;
		this.lblPSInfo13.Image = null;
		this.lblPSInfo13.Location = new System.Drawing.Point(194, 48);
		this.lblPSInfo13.Name = "lblPSInfo13";
		this.lblPSInfo13.Size = new System.Drawing.Size(2, 15);
		this.lblPSInfo13.TabIndex = 19;
		this.toolTip1.SetToolTip(this.lblPSInfo13, "Indicator:  number of rejected sample sets (Normal <= 2)");
		this.labelTS11.AutoSize = true;
		this.labelTS11.Image = null;
		this.labelTS11.Location = new System.Drawing.Point(128, 48);
		this.labelTS11.Name = "labelTS11";
		this.labelTS11.Size = new System.Drawing.Size(37, 13);
		this.labelTS11.TabIndex = 18;
		this.labelTS11.Text = "dg.cnt";
		this.toolTip1.SetToolTip(this.labelTS11, "Indicator:  number of rejected sample sets (Normal <= 2)");
		this.lblPSInfo6.AutoSize = true;
		this.lblPSInfo6.BackColor = System.Drawing.Color.Bisque;
		this.lblPSInfo6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblPSInfo6.ForeColor = System.Drawing.Color.Black;
		this.lblPSInfo6.Image = null;
		this.lblPSInfo6.Location = new System.Drawing.Point(194, 24);
		this.lblPSInfo6.Name = "lblPSInfo6";
		this.lblPSInfo6.Size = new System.Drawing.Size(2, 15);
		this.lblPSInfo6.TabIndex = 17;
		this.toolTip1.SetToolTip(this.lblPSInfo6, "Indicator:  code indicating evaluation of correction solution. (Normal = 0)");
		this.labelTS7.AutoSize = true;
		this.labelTS7.Image = null;
		this.labelTS7.Location = new System.Drawing.Point(128, 24);
		this.labelTS7.Name = "labelTS7";
		this.labelTS7.Size = new System.Drawing.Size(41, 13);
		this.labelTS7.TabIndex = 16;
		this.labelTS7.Text = "sln.chk";
		this.toolTip1.SetToolTip(this.labelTS7, "Indicator:  code indicating evaluation of correction solution. (Normal = 0)");
		this.GetPSpeak.BackColor = System.Drawing.Color.Bisque;
		this.GetPSpeak.Location = new System.Drawing.Point(287, 69);
		this.GetPSpeak.Name = "GetPSpeak";
		this.GetPSpeak.ReadOnly = true;
		this.GetPSpeak.Size = new System.Drawing.Size(57, 20);
		this.GetPSpeak.TabIndex = 15;
		this.toolTip1.SetToolTip(this.GetPSpeak, "Indicator:  Peak level of measured digital TX feedback.");
		this.labelTS3.AutoSize = true;
		this.labelTS3.Image = null;
		this.labelTS3.Location = new System.Drawing.Point(250, 72);
		this.labelTS3.Name = "labelTS3";
		this.labelTS3.Size = new System.Drawing.Size(37, 13);
		this.labelTS3.TabIndex = 14;
		this.labelTS3.Text = "GetPk";
		this.txtPSpeak.BackColor = System.Drawing.Color.Bisque;
		this.txtPSpeak.Location = new System.Drawing.Point(287, 93);
		this.txtPSpeak.Name = "txtPSpeak";
		this.txtPSpeak.Size = new System.Drawing.Size(57, 20);
		this.txtPSpeak.TabIndex = 13;
		this.toolTip1.SetToolTip(this.txtPSpeak, "Indicator:  Peak level of expected digital TX feedback.  (Should be close to GetPk; Can be set manually for non-recognized hardware/firmware.)");
		this.txtPSpeak.TextChanged += new System.EventHandler(PSpeak_TextChanged);
		this.labelTS5.AutoSize = true;
		this.labelTS5.Image = null;
		this.labelTS5.Location = new System.Drawing.Point(250, 96);
		this.labelTS5.Name = "labelTS5";
		this.labelTS5.Size = new System.Drawing.Size(36, 13);
		this.labelTS5.TabIndex = 12;
		this.labelTS5.Text = "SetPk";
		this.lblPSfb2.AutoSize = true;
		this.lblPSfb2.BackColor = System.Drawing.Color.Bisque;
		this.lblPSfb2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblPSfb2.ForeColor = System.Drawing.Color.Black;
		this.lblPSfb2.Image = null;
		this.lblPSfb2.Location = new System.Drawing.Point(309, 48);
		this.lblPSfb2.Name = "lblPSfb2";
		this.lblPSfb2.Size = new System.Drawing.Size(2, 15);
		this.lblPSfb2.TabIndex = 11;
		this.toolTip1.SetToolTip(this.lblPSfb2, "Indicator:  RF feedback level; drives red/yellow/green indicator.");
		this.labelTS1.AutoSize = true;
		this.labelTS1.Image = null;
		this.labelTS1.Location = new System.Drawing.Point(250, 48);
		this.labelTS1.Name = "labelTS1";
		this.labelTS1.Size = new System.Drawing.Size(40, 13);
		this.labelTS1.TabIndex = 10;
		this.labelTS1.Text = "feedbk";
		this.toolTip1.SetToolTip(this.labelTS1, "Indicator:  RF feedback level; drives red/yellow/green indicator.");
		this.lblPSInfo15.AutoSize = true;
		this.lblPSInfo15.BackColor = System.Drawing.Color.Bisque;
		this.lblPSInfo15.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblPSInfo15.ForeColor = System.Drawing.Color.Black;
		this.lblPSInfo15.Image = null;
		this.lblPSInfo15.Location = new System.Drawing.Point(309, 24);
		this.lblPSInfo15.Name = "lblPSInfo15";
		this.lblPSInfo15.Size = new System.Drawing.Size(2, 15);
		this.lblPSInfo15.TabIndex = 9;
		this.toolTip1.SetToolTip(this.lblPSInfo15, "Indicator:  indicates what PureSignal is doing at the present time.");
		this.labelTS146.AutoSize = true;
		this.labelTS146.Image = null;
		this.labelTS146.Location = new System.Drawing.Point(250, 24);
		this.labelTS146.Name = "labelTS146";
		this.labelTS146.Size = new System.Drawing.Size(30, 13);
		this.labelTS146.TabIndex = 8;
		this.labelTS146.Text = "state";
		this.toolTip1.SetToolTip(this.labelTS146, "Indicator:  indicates what PureSignal is doing at the present time.");
		this.lblPSInfo3.AutoSize = true;
		this.lblPSInfo3.BackColor = System.Drawing.Color.Bisque;
		this.lblPSInfo3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblPSInfo3.ForeColor = System.Drawing.Color.Black;
		this.lblPSInfo3.Image = null;
		this.lblPSInfo3.Location = new System.Drawing.Point(72, 96);
		this.lblPSInfo3.Name = "lblPSInfo3";
		this.lblPSInfo3.Size = new System.Drawing.Size(2, 15);
		this.lblPSInfo3.TabIndex = 7;
		this.toolTip1.SetToolTip(this.lblPSInfo3, "Indicator:  build of sine correction curve.  (Normal = 0)");
		this.lblPSInfo2.AutoSize = true;
		this.lblPSInfo2.BackColor = System.Drawing.Color.Bisque;
		this.lblPSInfo2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblPSInfo2.ForeColor = System.Drawing.Color.Black;
		this.lblPSInfo2.Image = null;
		this.lblPSInfo2.Location = new System.Drawing.Point(72, 72);
		this.lblPSInfo2.Name = "lblPSInfo2";
		this.lblPSInfo2.Size = new System.Drawing.Size(2, 15);
		this.lblPSInfo2.TabIndex = 6;
		this.toolTip1.SetToolTip(this.lblPSInfo2, "Indicator:  build of cosine correction curve.  (Normal = 0)");
		this.lblPSInfo1.AutoSize = true;
		this.lblPSInfo1.BackColor = System.Drawing.Color.Bisque;
		this.lblPSInfo1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblPSInfo1.ForeColor = System.Drawing.Color.Black;
		this.lblPSInfo1.Image = null;
		this.lblPSInfo1.Location = new System.Drawing.Point(72, 48);
		this.lblPSInfo1.Name = "lblPSInfo1";
		this.lblPSInfo1.Size = new System.Drawing.Size(2, 15);
		this.lblPSInfo1.TabIndex = 5;
		this.toolTip1.SetToolTip(this.lblPSInfo1, "Indicator:  build of magnitude correction curve.  (Normal = 0)");
		this.lblPSInfo0.AutoSize = true;
		this.lblPSInfo0.BackColor = System.Drawing.Color.Bisque;
		this.lblPSInfo0.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblPSInfo0.ForeColor = System.Drawing.Color.Black;
		this.lblPSInfo0.Image = null;
		this.lblPSInfo0.Location = new System.Drawing.Point(72, 24);
		this.lblPSInfo0.Name = "lblPSInfo0";
		this.lblPSInfo0.Size = new System.Drawing.Size(2, 15);
		this.lblPSInfo0.TabIndex = 4;
		this.toolTip1.SetToolTip(this.lblPSInfo0, "Indicator:  build of feedback magnitude curve.  (Normal = 0)");
		this.labelTS143.AutoSize = true;
		this.labelTS143.Image = null;
		this.labelTS143.Location = new System.Drawing.Point(6, 96);
		this.labelTS143.Name = "labelTS143";
		this.labelTS143.Size = new System.Drawing.Size(38, 13);
		this.labelTS143.TabIndex = 3;
		this.labelTS143.Text = "bldr.cs";
		this.toolTip1.SetToolTip(this.labelTS143, "Indicator:  build of sine correction curve.  (Normal = 0)");
		this.labelTS144.AutoSize = true;
		this.labelTS144.Image = null;
		this.labelTS144.Location = new System.Drawing.Point(6, 72);
		this.labelTS144.Name = "labelTS144";
		this.labelTS144.Size = new System.Drawing.Size(39, 13);
		this.labelTS144.TabIndex = 2;
		this.labelTS144.Text = "bldr.cc";
		this.toolTip1.SetToolTip(this.labelTS144, "Indicator:  build of cosine correction curve.  (Normal = 0)");
		this.labelTS142.AutoSize = true;
		this.labelTS142.Image = null;
		this.labelTS142.Location = new System.Drawing.Point(6, 48);
		this.labelTS142.Name = "labelTS142";
		this.labelTS142.Size = new System.Drawing.Size(41, 13);
		this.labelTS142.TabIndex = 1;
		this.labelTS142.Text = "bldr.cm";
		this.toolTip1.SetToolTip(this.labelTS142, "Indicator:  build of magnitude correction curve.  (Normal = 0)");
		this.labelTS141.AutoSize = true;
		this.labelTS141.Image = null;
		this.labelTS141.Location = new System.Drawing.Point(6, 24);
		this.labelTS141.Name = "labelTS141";
		this.labelTS141.Size = new System.Drawing.Size(35, 13);
		this.labelTS141.TabIndex = 0;
		this.labelTS141.Text = "bldr.rx";
		this.toolTip1.SetToolTip(this.labelTS141, "Indicator:  build of feedback magnitude curve.  (Normal = 0)");
		this.btnPSReset.BackColor = System.Drawing.SystemColors.Control;
		this.btnPSReset.Image = null;
		this.btnPSReset.Location = new System.Drawing.Point(476, 12);
		this.btnPSReset.Name = "btnPSReset";
		this.btnPSReset.Selectable = true;
		this.btnPSReset.Size = new System.Drawing.Size(71, 20);
		this.btnPSReset.TabIndex = 20;
		this.btnPSReset.Text = "OFF";
		this.btnPSReset.UseVisualStyleBackColor = false;
		this.btnPSReset.Click += new System.EventHandler(btnPSReset_Click);
		this.btnPSCalibrate.BackColor = System.Drawing.SystemColors.Control;
		this.btnPSCalibrate.Image = null;
		this.btnPSCalibrate.Location = new System.Drawing.Point(91, 12);
		this.btnPSCalibrate.Name = "btnPSCalibrate";
		this.btnPSCalibrate.Selectable = true;
		this.btnPSCalibrate.Size = new System.Drawing.Size(71, 20);
		this.btnPSCalibrate.TabIndex = 19;
		this.btnPSCalibrate.Text = "Single Cal";
		this.toolTip1.SetToolTip(this.btnPSCalibrate, "Perform a singal calibration. This will happen up to 5 times in a row");
		this.btnPSCalibrate.UseVisualStyleBackColor = false;
		this.btnPSCalibrate.Click += new System.EventHandler(btnPSCalibrate_Click);
		this.labelTS140.AutoSize = true;
		this.labelTS140.ForeColor = System.Drawing.Color.White;
		this.labelTS140.Image = null;
		this.labelTS140.Location = new System.Drawing.Point(11, 90);
		this.labelTS140.Name = "labelTS140";
		this.labelTS140.Size = new System.Drawing.Size(78, 13);
		this.labelTS140.TabIndex = 17;
		this.labelTS140.Text = "CAL Wait (sec)";
		this.udPSCalWait.DecimalPlaces = 1;
		this.udPSCalWait.Increment = new decimal(new int[4] { 1, 0, 0, 65536 });
		this.udPSCalWait.Location = new System.Drawing.Point(99, 88);
		this.udPSCalWait.Maximum = new decimal(new int[4] { 100, 0, 0, 0 });
		this.udPSCalWait.Minimum = new decimal(new int[4]);
		this.udPSCalWait.Name = "udPSCalWait";
		this.udPSCalWait.Size = new System.Drawing.Size(51, 20);
		this.udPSCalWait.TabIndex = 16;
		this.udPSCalWait.TinyStep = false;
		this.toolTip1.SetToolTip(this.udPSCalWait, "Time to wait between calculating correction solutions.  (Zero for fastest response.)");
		this.udPSCalWait.Value = new decimal(new int[4]);
		this.udPSCalWait.ValueChanged += new System.EventHandler(udPSCalWait_ValueChanged);
		this.chkQuickAttenuate.AutoSize = true;
		this.chkQuickAttenuate.ForeColor = System.Drawing.SystemColors.HighlightText;
		this.chkQuickAttenuate.Image = null;
		this.chkQuickAttenuate.Location = new System.Drawing.Point(208, 108);
		this.chkQuickAttenuate.Name = "chkQuickAttenuate";
		this.chkQuickAttenuate.Size = new System.Drawing.Size(154, 17);
		this.chkQuickAttenuate.TabIndex = 49;
		this.chkQuickAttenuate.Text = "Quick Attenuate Response";
		this.toolTip1.SetToolTip(this.chkQuickAttenuate, "Apply auto attenuation changes at a faster interval");
		this.chkQuickAttenuate.UseVisualStyleBackColor = true;
		this.chkQuickAttenuate.CheckedChanged += new System.EventHandler(chkQuickAttenuate_CheckedChanged);
		this.chkAdvancedViewHidden.AutoSize = true;
		this.chkAdvancedViewHidden.BackColor = System.Drawing.SystemColors.ControlLightLight;
		this.chkAdvancedViewHidden.Image = null;
		this.chkAdvancedViewHidden.Location = new System.Drawing.Point(397, 189);
		this.chkAdvancedViewHidden.Name = "chkAdvancedViewHidden";
		this.chkAdvancedViewHidden.Size = new System.Drawing.Size(150, 17);
		this.chkAdvancedViewHidden.TabIndex = 50;
		this.chkAdvancedViewHidden.Text = "chkAdvancedViewHidden";
		this.chkAdvancedViewHidden.UseVisualStyleBackColor = false;
		this.chkAdvancedViewHidden.Visible = false;
		this.pbWarningSetPk.Image = (System.Drawing.Image)resources.GetObject("pbWarningSetPk.Image");
		this.pbWarningSetPk.Location = new System.Drawing.Point(323, 35);
		this.pbWarningSetPk.Name = "pbWarningSetPk";
		this.pbWarningSetPk.Size = new System.Drawing.Size(20, 20);
		this.pbWarningSetPk.TabIndex = 51;
		this.pbWarningSetPk.TabStop = false;
		this.toolTip1.SetToolTip(this.pbWarningSetPk, resources.GetString("pbWarningSetPk.ToolTip"));
		this.pbWarningSetPk.Visible = false;
		this.chkShow2ToneMeasurements.AutoSize = true;
		this.chkShow2ToneMeasurements.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.chkShow2ToneMeasurements.Image = null;
		this.chkShow2ToneMeasurements.Location = new System.Drawing.Point(389, 38);
		this.chkShow2ToneMeasurements.Name = "chkShow2ToneMeasurements";
		this.chkShow2ToneMeasurements.Size = new System.Drawing.Size(158, 17);
		this.chkShow2ToneMeasurements.TabIndex = 52;
		this.chkShow2ToneMeasurements.Text = "Show 2Tone measurements";
		this.chkShow2ToneMeasurements.UseVisualStyleBackColor = true;
		this.chkShow2ToneMeasurements.CheckedChanged += new System.EventHandler(chkShow2ToneMeasurements_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(560, 303);
		base.Controls.Add(this.chkShow2ToneMeasurements);
		base.Controls.Add(this.pbWarningSetPk);
		base.Controls.Add(this.chkAdvancedViewHidden);
		base.Controls.Add(this.chkQuickAttenuate);
		base.Controls.Add(this.chkPSOnTop);
		base.Controls.Add(this.btnPSResetEngine);
		base.Controls.Add(this.comboPSTint);
		base.Controls.Add(this.udPSOutlierSigma);
		base.Controls.Add(this.lblPSOutlierSigma);
		base.Controls.Add(this.chkPSOutlierEnable);
		base.Controls.Add(this.udPSTargetFeedback);
		base.Controls.Add(this.udPSFBTargetOffset);
		base.Controls.Add(this.lblPSFBTargetOffset);
		this.udPSFBTargetOffset.Increment = new decimal(new int[4] { 1, 0, 0, 0 });
		this.udPSFBTargetOffset.Location = new System.Drawing.Point(208, 86);
		this.udPSFBTargetOffset.Maximum = new decimal(new int[4] { 20, 0, 0, 0 });
		this.udPSFBTargetOffset.Minimum = new decimal(new int[4]);
		this.udPSFBTargetOffset.Name = "udPSFBTargetOffset";
		this.udPSFBTargetOffset.Size = new System.Drawing.Size(48, 20);
		this.udPSFBTargetOffset.TabIndex = 30;
		this.udPSFBTargetOffset.TinyStep = false;
		this.toolTip1.SetToolTip(this.udPSFBTargetOffset, "ATT RX (PS feedback path attenuator), dB: 0 = all models (original), 10 = Anvelina PRO3. Auto-attenuate walks from this point.");
		this.udPSFBTargetOffset.Value = new decimal(new int[4]);
		this.udPSFBTargetOffset.ValueChanged += new System.EventHandler(udPSFBTargetOffset_ValueChanged);
		this.lblPSFBTargetOffset.AutoSize = true;
		this.lblPSFBTargetOffset.ForeColor = System.Drawing.Color.White;
		this.lblPSFBTargetOffset.Image = null;
		this.lblPSFBTargetOffset.Location = new System.Drawing.Point(262, 90);
		this.lblPSFBTargetOffset.Name = "lblPSFBTargetOffset";
		this.lblPSFBTargetOffset.Size = new System.Drawing.Size(82, 13);
		this.lblPSFBTargetOffset.TabIndex = 31;
		this.lblPSFBTargetOffset.Text = "ATT RX (dB)";
		base.Controls.Add(this.lblPSTargetFeedback);
		base.Controls.Add(this.chkPSEQ);
		base.Controls.Add(this.chkPSPin);
		base.Controls.Add(this.udPSDCBCap);
		base.Controls.Add(this.lblPSDCBCap);
		base.Controls.Add(this.chkPSDCB);
		base.Controls.Add(this.udPSPinAlpha);
		base.Controls.Add(this.lblPSPinAlpha);
		base.Controls.Add(this.udPSEMAAlpha);
		base.Controls.Add(this.lblPSEMAAlpha);
		base.Controls.Add(this.btnPSRestore);
		base.Controls.Add(this.btnPSSave);
		base.Controls.Add(this.btnPSAdvanced);
		base.Controls.Add(this.chkPSStbl);
		base.Controls.Add(this.chkPSAutoAttenuate);
		base.Controls.Add(this.btnPSAmpView);
		base.Controls.Add(this.btnPSTwoToneGen);
		base.Controls.Add(this.labelTS8);
		base.Controls.Add(this.lblPSInfoFB);
		base.Controls.Add(this.lblPSInfoCO);
		base.Controls.Add(this.labelTS9);
		base.Controls.Add(this.labelTS4);
		base.Controls.Add(this.udPSMoxDelay);
		base.Controls.Add(this.labelTS2);
		base.Controls.Add(this.udPSPhnum);
		base.Controls.Add(this.grpPSInfo);
		base.Controls.Add(this.btnPSReset);
		base.Controls.Add(this.btnPSCalibrate);
		base.Controls.Add(this.labelTS140);
		base.Controls.Add(this.udPSCalWait);
		this.ForeColor = System.Drawing.SystemColors.ControlText;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "PSForm";
		this.Text = "PureSignal 2.0";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(PSForm_Closing);
		base.Load += new System.EventHandler(PSForm_Load);
		((System.ComponentModel.ISupportInitialize)this.udPSMoxDelay).EndInit();
		((System.ComponentModel.ISupportInitialize)this.udPSPhnum).EndInit();
		this.grpPSInfo.ResumeLayout(false);
		this.grpPSInfo.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.udPSCalWait).EndInit();
		((System.ComponentModel.ISupportInitialize)this.pbWarningSetPk).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
