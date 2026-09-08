using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;

namespace Thetis;

public class ucInfoBar : UserControl
{
	public enum ActionTypes
	{
		Blobs,
		ActivePeaks,
		CursorInfo,
		ShowSpots,
		DisplayFill,
		CFC,
		CFCeq,
		Leveler,
		DisplayPause,
		LAST
	}

	public class InfoBarAction : EventArgs
	{
		public bool ButtonState;

		public MouseButtons Button;

		public ActionTypes Action { get; set; }
	}

	public class ActionState
	{
		public bool Checked;

		public ActionTypes Action;

		public string DisplayString => Action switch
		{
			ActionTypes.Blobs => "Blobs", 
			ActionTypes.ActivePeaks => "Peak", 
			ActionTypes.CFC => "CFC", 
			ActionTypes.CursorInfo => "Info", 
			ActionTypes.Leveler => "Lev", 
			ActionTypes.CFCeq => "CFCeq", 
			ActionTypes.ShowSpots => "Spots", 
			ActionTypes.DisplayFill => "Fill", 
			ActionTypes.DisplayPause => "Pause", 
			_ => "?", 
		};

		public string TipString => Action switch
		{
			ActionTypes.Blobs => "Show peak blobs", 
			ActionTypes.ActivePeaks => "Show active peak hold", 
			ActionTypes.CFC => "Enable CFC", 
			ActionTypes.CursorInfo => "Show information on cursor", 
			ActionTypes.Leveler => "Enable the leveler", 
			ActionTypes.CFCeq => "Enable Post CFC EQ", 
			ActionTypes.ShowSpots => "Show spots", 
			ActionTypes.DisplayFill => "Fill the panadaptor", 
			ActionTypes.DisplayPause => "Pause the display", 
			_ => "", 
		};
	}

	private const int WM_SETREDRAW = 11;

	private const int MAX_FLIP = 2;

	private Console _console;

	private bool _mox;

	private bool _psEnabled;

	private System.Timers.Timer _psTimer;

	private System.Timers.Timer _warningTimer;

	private bool _preventClickEvents;

	private bool _shutDown;

	private int _currentFlip;

	private bool _hideFeedback;

	private bool _dragging;

	private int _startX;

	private float _splitterRatio = 1f;

	private bool _okToResize;

	private string[] _left1;

	private string[] _left2;

	private string[] _left3;

	private string[] _right1;

	private string[] _right2;

	private string[] _right3;

	private string[,] _leftToolTip;

	private string[,] _rightToolTip;

	private int[] _left1BaseWidth;

	private int[] _left2BaseWidth;

	private int[] _left3BaseWidth;

	private int[] _left1Width;

	private int[] _left2Width;

	private int[] _left3Width;

	private int[] _right1BaseWidth;

	private int[] _right2BaseWidth;

	private int[] _right3BaseWidth;

	private int[] _right1Width;

	private int[] _right2Width;

	private int[] _right3Width;

	private frmInfoBarPopup _frmInfoBarPopup_Button1;

	private ToolStripDropDown _toolStripForm_Button1;

	private ToolStripControlHost _host_Button1;

	private frmInfoBarPopup _frmInfoBarPopup_Button2;

	private ToolStripDropDown _toolStripForm_Button2;

	private ToolStripControlHost _host_Button2;

	private Cursor _oldCursor;

	private Font _normalPSFont;

	private Font _smallPSFont;

	private Dictionary<ActionTypes, ActionState> _button1Actions = new Dictionary<ActionTypes, ActionState>();

	private Dictionary<ActionTypes, ActionState> _button2Actions = new Dictionary<ActionTypes, ActionState>();

	private ActionState _button1Action = new ActionState();

	private ActionState _button2Action = new ActionState();

	private Color _lastColor = Color.SeaGreen;

	private bool _bCorrectionsBeingApplied;

	private bool _bCalibrationAttemptsChanged;

	private bool _bFeedbackLevelOk;

	private Color _feedbackColour = Color.Black;

	private int _nFeedbackLevel;

	private string _psStateText = "";

	private bool _useSmallFonts;

	private IContainer components;

	private CheckBoxTS chkButton1;

	private CheckBoxTS chkButton2;

	private LabelTS lblPS;

	private LabelTS lblFB;

	private LabelTS lblLeft1;

	private LabelTS lblLeft2;

	private LabelTS lblLeft3;

	private LabelTS lblRight3;

	private LabelTS lblRight2;

	private LabelTS lblRight1;

	private LabelTS lblWarning;

	private ToolTip toolTip1;

	private LabelTS lblSplitter;

	private LabelTS lblPageNo;

	public ActionTypes Button1Action
	{
		get
		{
			return _button1Action.Action;
		}
		set
		{
			_button1Action.Action = value;
		}
	}

	public ActionTypes Button2Action
	{
		get
		{
			return _button2Action.Action;
		}
		set
		{
			_button2Action.Action = value;
		}
	}

	public override Color BackColor
	{
		get
		{
			return base.BackColor;
		}
		set
		{
			base.BackColor = value;
			lblPageNo.BackColor = value;
			lblLeft1.BackColor = value;
			lblLeft2.BackColor = value;
			lblLeft3.BackColor = value;
			lblRight1.BackColor = value;
			lblRight2.BackColor = value;
			lblRight3.BackColor = value;
			lblWarning.BackColor = value;
			if (_frmInfoBarPopup_Button1 != null)
			{
				_frmInfoBarPopup_Button1.BackColor = value;
			}
			if (_frmInfoBarPopup_Button2 != null)
			{
				_frmInfoBarPopup_Button2.BackColor = value;
			}
		}
	}

	public override Color ForeColor
	{
		get
		{
			return base.ForeColor;
		}
		set
		{
			base.ForeColor = value;
			lblPageNo.ForeColor = value;
			lblLeft1.ForeColor = value;
			lblLeft2.ForeColor = value;
			lblLeft3.ForeColor = value;
			lblRight1.ForeColor = value;
			lblRight2.ForeColor = value;
			lblRight3.ForeColor = value;
		}
	}

	public bool PSAEnabled
	{
		set
		{
			_psEnabled = value;
			if (!_psEnabled)
			{
				setPSboolsToFalse();
			}
			updatePSDisplay();
		}
	}

	public CheckBoxTS Button1 => chkButton1;

	public CheckBoxTS Button2 => chkButton2;

	public int CurrentFlip
	{
		get
		{
			return _currentFlip;
		}
		set
		{
			_currentFlip = value;
			if (_currentFlip < 0 || _currentFlip > 1)
			{
				_currentFlip = 0;
			}
		}
	}

	public bool SwapRedBlue
	{
		get
		{
			return puresignal.InvertRedBlue;
		}
		set
		{
			bool num = puresignal.InvertRedBlue != value;
			puresignal.InvertRedBlue = value;
			setToolTips();
			if (num)
			{
				SwapRedBlueChanged?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	public bool HideFeedback
	{
		get
		{
			return _hideFeedback;
		}
		set
		{
			bool num = _hideFeedback != value;
			_hideFeedback = value;
			setToolTips();
			if (num)
			{
				HideFeedbackChanged?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	public float SplitterRatio
	{
		get
		{
			return _splitterRatio;
		}
		set
		{
			_splitterRatio = value;
		}
	}

	public event EventHandler<InfoBarAction> Button1Clicked;

	public event EventHandler<InfoBarAction> Button2Clicked;

	public event EventHandler<InfoBarAction> Button1MouseDown;

	public event EventHandler<InfoBarAction> Button2MouseDown;

	public event EventHandler SwapRedBlueChanged;

	public event EventHandler HideFeedbackChanged;

	[DllImport("user32.dll")]
	private static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

	public ucInfoBar()
	{
		InitializeComponent();
		_oldCursor = Cursor.Current;
		_normalPSFont = new Font(lblPS.Font.FontFamily, 9f, FontStyle.Bold);
		_smallPSFont = new Font(lblPS.Font.FontFamily, 6.75f, FontStyle.Regular);
		_psTimer = new System.Timers.Timer();
		_psTimer.AutoReset = false;
		_psTimer.Interval = 50.0;
		_psTimer.Elapsed += onTick;
		_warningTimer = new System.Timers.Timer();
		_warningTimer.AutoReset = false;
		_warningTimer.Interval = 2000.0;
		_warningTimer.Elapsed += onWarning;
		_left1 = new string[2];
		_left2 = new string[2];
		_left3 = new string[2];
		_right1 = new string[2];
		_right2 = new string[2];
		_right3 = new string[2];
		_left1BaseWidth = new int[2];
		_left2BaseWidth = new int[2];
		_left3BaseWidth = new int[2];
		_left1Width = new int[2];
		_left2Width = new int[2];
		_left3Width = new int[2];
		_right1BaseWidth = new int[2];
		_right2BaseWidth = new int[2];
		_right3BaseWidth = new int[2];
		_right1Width = new int[2];
		_right2Width = new int[2];
		_right3Width = new int[2];
		_leftToolTip = new string[2, 3];
		_rightToolTip = new string[2, 3];
		for (int i = 0; i < 2; i++)
		{
			_left1BaseWidth[i] = lblLeft1.Width;
			_left2BaseWidth[i] = lblLeft2.Width;
			_left3BaseWidth[i] = lblLeft3.Width;
			_left1Width[i] = lblLeft1.Width;
			_left2Width[i] = lblLeft2.Width;
			_left3Width[i] = lblLeft3.Width;
			_right1BaseWidth[i] = lblRight1.Width;
			_right2BaseWidth[i] = lblRight2.Width;
			_right3BaseWidth[i] = lblRight3.Width;
			_right1Width[i] = lblRight1.Width;
			_right2Width[i] = lblRight2.Width;
			_right3Width[i] = lblRight3.Width;
			for (int j = 0; j < 3; j++)
			{
				_leftToolTip[i, j] = "";
				_rightToolTip[i, j] = "";
			}
		}
		for (int k = 0; k < 9; k++)
		{
			_button1Actions.Add((ActionTypes)k, new ActionState
			{
				Action = (ActionTypes)k
			});
			_button2Actions.Add((ActionTypes)k, new ActionState
			{
				Action = (ActionTypes)k
			});
		}
		_button1Action = new ActionState
		{
			Action = ActionTypes.Blobs
		};
		_button2Action = new ActionState
		{
			Action = ActionTypes.ActivePeaks
		};
		_frmInfoBarPopup_Button1 = new frmInfoBarPopup();
		_frmInfoBarPopup_Button1.ActionClicked += OnActionClicked_Button1;
		_frmInfoBarPopup_Button1.TopLevel = false;
		_frmInfoBarPopup_Button2 = new frmInfoBarPopup();
		_frmInfoBarPopup_Button2.ActionClicked += OnActionClicked_Button2;
		_frmInfoBarPopup_Button2.TopLevel = false;
		_host_Button1 = new ToolStripControlHost(_frmInfoBarPopup_Button1);
		_toolStripForm_Button1 = new ToolStripDropDown();
		_host_Button2 = new ToolStripControlHost(_frmInfoBarPopup_Button2);
		_toolStripForm_Button2 = new ToolStripDropDown();
		lblSplitter.BackColor = Color.Silver;
		lblFB.Font = _normalPSFont;
		_okToResize = true;
		repositionControls();
	}

	private string actionString(ActionTypes action)
	{
		return new ActionState
		{
			Action = action
		}.DisplayString;
	}

	private void OnActionClicked_Button1(object sender, frmInfoBarPopup.PopupActionSelected e)
	{
		if (e.Button == MouseButtons.Left)
		{
			doAction(1, e.Action, e.ButtonState, e.Button);
		}
		else if (e.Button == MouseButtons.Right && !Common.ShiftKeyDown)
		{
			replaceMainButton(1, e.Action, e.ButtonState, e.Button);
		}
		_toolStripForm_Button1.Hide();
	}

	private void OnActionClicked_Button2(object sender, frmInfoBarPopup.PopupActionSelected e)
	{
		if (e.Button == MouseButtons.Left)
		{
			doAction(2, e.Action, e.ButtonState, e.Button);
		}
		else if (e.Button == MouseButtons.Right && !Common.ShiftKeyDown)
		{
			replaceMainButton(2, e.Action, e.ButtonState, e.Button);
		}
		_toolStripForm_Button2.Hide();
	}

	private void doAction(int button, ActionTypes action, bool bState, MouseButtons mouseButton)
	{
		if (!_preventClickEvents)
		{
			switch (button)
			{
			case 1:
				_button1Actions[action].Checked = bState;
				Button1Clicked?.Invoke(this, new InfoBarAction
				{
					Action = action,
					ButtonState = bState,
					Button = mouseButton
				});
				break;
			case 2:
				_button2Actions[action].Checked = bState;
				Button2Clicked?.Invoke(this, new InfoBarAction
				{
					Action = action,
					ButtonState = bState,
					Button = mouseButton
				});
				break;
			}
		}
	}

	private void addPopup(frmInfoBarPopup frm, ToolStripControlHost host, ToolStripDropDown dropDown)
	{
		host.AutoSize = false;
		host.Margin = Padding.Empty;
		host.Padding = Padding.Empty;
		host.Width = frm.Width;
		host.Height = frm.Height;
		dropDown.AutoSize = false;
		dropDown.Margin = Padding.Empty;
		dropDown.Padding = Padding.Empty;
		dropDown.Width = host.Width;
		dropDown.Height = host.Height;
		dropDown.Items.Add(host);
		dropDown.Closed += OnPopupClosed;
	}

	~ucInfoBar()
	{
		ShutDown();
	}

	private void OnPopupClosed(object sender, ToolStripDropDownClosedEventArgs e)
	{
	}

	public void ShutDown()
	{
		if (_console != null)
		{
			Console console = _console;
			console.MoxChangeHandlers = (Console.MoxChanged)Delegate.Remove(console.MoxChangeHandlers, new Console.MoxChanged(OnMoxChangeHandler));
		}
		_shutDown = true;
		if (_psTimer != null)
		{
			_psTimer.Stop();
			_psTimer.Elapsed -= onTick;
			_psTimer = null;
		}
		if (_warningTimer != null)
		{
			_warningTimer.Stop();
			_warningTimer.Elapsed -= onWarning;
			_warningTimer = null;
		}
		if (_frmInfoBarPopup_Button1 != null)
		{
			_frmInfoBarPopup_Button1.Close();
			_frmInfoBarPopup_Button1 = null;
		}
	}

	private void onWarning(object sender, ElapsedEventArgs e)
	{
		_warningTimer.Interval = 2000.0;
		if (!_shutDown && !base.IsDisposed && !base.Disposing && !lblWarning.IsDisposed && !lblWarning.Disposing)
		{
			lblWarning.Visible = false;
		}
	}

	private void onTick(object sender, ElapsedEventArgs e)
	{
		if (!_psEnabled || _shutDown || base.IsDisposed || base.Disposing)
		{
			return;
		}
		Color backColor = lblFB.BackColor;
		int num = (int)((double)(int)backColor.R * 0.95);
		int num2 = (int)((double)(int)backColor.G * 0.95);
		int num3 = (int)((double)(int)backColor.B * 0.95);
		bool flag = false;
		if (_lastColor == Color.Red)
		{
			flag = num <= 128;
		}
		else if (_lastColor == Color.SeaGreen)
		{
			flag = num2 <= Color.SeaGreen.G;
			if (num < Color.SeaGreen.R)
			{
				num = Color.SeaGreen.R;
			}
			if (num2 < Color.SeaGreen.G)
			{
				num2 = Color.SeaGreen.G;
			}
			if (num3 < Color.SeaGreen.B)
			{
				num3 = Color.SeaGreen.B;
			}
		}
		else if (_lastColor == Color.Yellow)
		{
			flag = num <= 96 && num3 <= 96;
		}
		else if (_lastColor == Color.DodgerBlue)
		{
			flag = num3 <= 128;
		}
		else if (_lastColor == Color.Lime)
		{
			flag = num2 <= Color.SeaGreen.G;
			if (num < Color.SeaGreen.R)
			{
				num = Color.SeaGreen.R;
			}
			if (num2 < Color.SeaGreen.G)
			{
				num2 = Color.SeaGreen.G;
			}
			if (num3 < Color.SeaGreen.B)
			{
				num3 = Color.SeaGreen.B;
			}
		}
		lblFB.BackColor = Color.FromArgb(255, num, num2, num3);
		if (flag)
		{
			_feedbackColour = Color.SeaGreen;
			if (_useSmallFonts)
			{
				lblFB.Text = "FB";
			}
			else
			{
				lblFB.Text = "Feedback";
			}
			_psTimer.Stop();
		}
		else if (_psTimer != null)
		{
			_psTimer.Start();
		}
	}

	public void LateInit(Console c)
	{
		_console = c;
		Console console = _console;
		console.MoxChangeHandlers = (Console.MoxChanged)Delegate.Combine(console.MoxChangeHandlers, new Console.MoxChanged(OnMoxChangeHandler));
		for (int i = 0; i < 2; i++)
		{
			_left1[i] = "";
			_left2[i] = "";
			_left3[i] = "";
			_right1[i] = "";
			_right2[i] = "";
			_right3[i] = "";
		}
		lblFB.ForeColor = Color.Black;
		lblPS.ForeColor = Color.Black;
		_shutDown = false;
		lblWarning.Visible = false;
		_preventClickEvents = false;
		PSAEnabled = false;
		_frmInfoBarPopup_Button1.SetStates(_button1Actions, _button1Action, _button2Action);
		_frmInfoBarPopup_Button2.SetStates(_button2Actions, _button1Action, _button2Action);
		addPopup(_frmInfoBarPopup_Button1, _host_Button1, _toolStripForm_Button1);
		addPopup(_frmInfoBarPopup_Button2, _host_Button2, _toolStripForm_Button2);
		updateLabels();
		setToolTips();
	}

	private void OnMoxChangeHandler(int rx, bool oldMox, bool newMox)
	{
		_mox = newMox;
		if (!_mox)
		{
			setPSboolsToFalse();
		}
		updatePSDisplay();
	}

	private void setPSboolsToFalse()
	{
		_bCalibrationAttemptsChanged = false;
		_bCorrectionsBeingApplied = false;
		_bFeedbackLevelOk = false;
	}

	private void chkButton1_CheckedChanged(object sender, EventArgs e)
	{
		if (!_preventClickEvents)
		{
			Button1Clicked?.Invoke(this, new InfoBarAction
			{
				Action = _button1Action.Action,
				ButtonState = chkButton1.Checked,
				Button = MouseButtons.None
			});
		}
	}

	private void chkButton2_CheckedChanged(object sender, EventArgs e)
	{
		if (!_preventClickEvents)
		{
			Button2Clicked?.Invoke(this, new InfoBarAction
			{
				Action = _button2Action.Action,
				ButtonState = chkButton2.Checked,
				Button = MouseButtons.None
			});
		}
	}

	public void UpdateButtonState(ActionTypes action, bool bEnabled, bool bIncludePopup = true)
	{
		if (_button1Actions.ContainsKey(action) && _button2Actions.ContainsKey(action))
		{
			_preventClickEvents = true;
			_button1Actions[action].Checked = bEnabled;
			_button2Actions[action].Checked = bEnabled;
			if (bIncludePopup)
			{
				_frmInfoBarPopup_Button1.SetStates(_button1Actions, _button1Action, _button2Action);
				_frmInfoBarPopup_Button2.SetStates(_button2Actions, _button1Action, _button2Action);
			}
			if (_button1Action.Action == action)
			{
				chkButton1.Text = actionString(action);
				chkButton1.Checked = bEnabled;
				toolTip1.SetToolTip(chkButton1, _button1Actions[action].TipString);
			}
			if (_button2Action.Action == action)
			{
				chkButton2.Text = actionString(action);
				chkButton2.Checked = bEnabled;
				toolTip1.SetToolTip(chkButton2, _button2Actions[action].TipString);
			}
			_preventClickEvents = false;
		}
	}

	public void Left1(int flipLayer, string value, int width = -1)
	{
		if (flipLayer >= 0 && flipLayer <= 1)
		{
			_left1[flipLayer] = value;
			if (width == -1)
			{
				_left1Width[flipLayer] = _left1BaseWidth[flipLayer];
			}
			else
			{
				_left1Width[flipLayer] = width;
			}
			if (_currentFlip == flipLayer)
			{
				lblLeft1.Text = _left1[_currentFlip];
				repositionControls();
			}
		}
	}

	public void Left2(int flipLayer, string value, int width = -1)
	{
		if (flipLayer >= 0 && flipLayer <= 1)
		{
			_left2[flipLayer] = value;
			if (width == -1)
			{
				_left2Width[flipLayer] = _left2BaseWidth[flipLayer];
			}
			else
			{
				_left2Width[flipLayer] = width;
			}
			if (_currentFlip == flipLayer)
			{
				lblLeft2.Text = _left2[_currentFlip];
				repositionControls();
			}
		}
	}

	public void Left3(int flipLayer, string value, int width = -1)
	{
		if (flipLayer >= 0 && flipLayer <= 1)
		{
			_left3[flipLayer] = value;
			if (width == -1)
			{
				_left3Width[flipLayer] = _left3BaseWidth[flipLayer];
			}
			else
			{
				_left3Width[flipLayer] = width;
			}
			if (_currentFlip == flipLayer)
			{
				lblLeft3.Text = _left3[_currentFlip];
				repositionControls();
			}
		}
	}

	public void Right1(int flipLayer, string value, int width = -1)
	{
		if (flipLayer >= 0 && flipLayer <= 1)
		{
			_right1[flipLayer] = value;
			if (width == -1)
			{
				_right1Width[flipLayer] = _right1BaseWidth[flipLayer];
			}
			else
			{
				_right1Width[flipLayer] = width;
			}
			if (_currentFlip == flipLayer)
			{
				lblRight1.Text = _right1[_currentFlip];
				repositionControls();
			}
		}
	}

	public void Right2(int flipLayer, string value, int width = -1)
	{
		if (flipLayer >= 0 && flipLayer <= 1)
		{
			_right2[flipLayer] = value;
			if (width == -1)
			{
				_right2Width[flipLayer] = _right2BaseWidth[flipLayer];
			}
			else
			{
				_right2Width[flipLayer] = width;
			}
			if (_currentFlip == flipLayer)
			{
				lblRight2.Text = _right2[_currentFlip];
				repositionControls();
			}
		}
	}

	public void Right3(int flipLayer, string value, int width = -1)
	{
		if (flipLayer >= 0 && flipLayer <= 1)
		{
			_right3[flipLayer] = value;
			if (width == -1)
			{
				_right3Width[flipLayer] = _right3BaseWidth[flipLayer];
			}
			else
			{
				_right3Width[flipLayer] = width;
			}
			if (_currentFlip == flipLayer)
			{
				lblRight3.Text = _right3[_currentFlip];
				repositionControls();
			}
		}
	}

	public void SetToolTipLeft(int flipLayer, int labelIndex, string text)
	{
		if (flipLayer >= 0 && flipLayer <= 1 && labelIndex >= 1 && labelIndex <= 3)
		{
			LabelTS labelTS = null;
			_leftToolTip[flipLayer, labelIndex - 1] = text;
			switch (labelIndex)
			{
			case 1:
				labelTS = lblLeft1;
				break;
			case 2:
				labelTS = lblLeft2;
				break;
			case 3:
				labelTS = lblLeft3;
				break;
			}
			if (_currentFlip == flipLayer && labelTS != null)
			{
				toolTip1.SetToolTip(labelTS, text);
			}
		}
	}

	public void SetToolTipRight(int flipLayer, int labelIndex, string text)
	{
		if (flipLayer >= 0 && flipLayer <= 1 && labelIndex >= 1 && labelIndex <= 3)
		{
			LabelTS labelTS = null;
			_rightToolTip[flipLayer, labelIndex - 1] = text;
			switch (labelIndex)
			{
			case 1:
				labelTS = lblRight1;
				break;
			case 2:
				labelTS = lblRight2;
				break;
			case 3:
				labelTS = lblRight3;
				break;
			}
			if (_currentFlip == flipLayer && labelTS != null)
			{
				toolTip1.SetToolTip(labelTS, text);
			}
		}
	}

	public void PSInfo(int level, bool bFeedbackLevelOk, bool bCorrectionsBeingApplied, bool bCalibrationAttemptsChanged, Color feedbackColour)
	{
		if (!_shutDown)
		{
			_bCalibrationAttemptsChanged = bCalibrationAttemptsChanged;
			if (_bCalibrationAttemptsChanged && _mox)
			{
				_nFeedbackLevel = level;
				_feedbackColour = feedbackColour;
				_bCorrectionsBeingApplied = bCorrectionsBeingApplied;
				_bFeedbackLevelOk = bFeedbackLevelOk;
				updatePSDisplay();
				_psTimer.Start();
			}
		}
	}

	public void PSState(string state)
	{
		if (!_shutDown && _psStateText != state)
		{
			_psStateText = state;
			updatePSDisplay();
		}
	}

	private void updatePSDisplay()
	{
		if (!_psEnabled)
		{
			lblFB.BackColor = Color.FromArgb(255, Color.DimGray);
			lblPS.BackColor = Color.FromArgb(255, Color.DimGray);
			_lastColor = Color.DimGray;
			if (_useSmallFonts)
			{
				lblFB.Text = "FB";
			}
			else
			{
				lblFB.Text = "Feedback";
			}
			lblPS.Text = "Pure Signal2";
		}
		else if (_mox)
		{
			if (_bCorrectionsBeingApplied)
			{
				lblPS.Text = (_useSmallFonts ? "Correct" : "Correcting");
				lblPS.BackColor = Color.FromArgb(255, Color.Lime);
			}
			else
			{
				lblPS.Text = (string.IsNullOrEmpty(_psStateText) ? "Pure Signal2" : _psStateText);
				lblPS.BackColor = Color.FromArgb(255, Color.SeaGreen);
			}
			lblFB.BackColor = _feedbackColour;
			_lastColor = _feedbackColour;
			if (_hideFeedback || !_bCalibrationAttemptsChanged)
			{
				if (_useSmallFonts)
				{
					lblFB.Text = "FB";
				}
				else
				{
					lblFB.Text = "Feedback";
				}
			}
			else
			{
				lblFB.Text = _nFeedbackLevel.ToString();
			}
		}
		else
		{
			_psTimer.Stop();
			_lastColor = Color.SeaGreen;
			_feedbackColour = Color.SeaGreen;
			lblPS.Text = (string.IsNullOrEmpty(_psStateText) ? "Pure Signal2" : _psStateText);
			lblPS.BackColor = Color.FromArgb(255, Color.SeaGreen);
			lblFB.BackColor = Color.SeaGreen;
			if (_useSmallFonts)
			{
				lblFB.Text = "FB";
			}
			else
			{
				lblFB.Text = "Feedback";
			}
		}
	}

	public void Warning(string msg, bool red_warning = false, int show_duration = 2000)
	{
		if (!_shutDown)
		{
			_warningTimer.Stop();
			lblWarning.ForeColor = (red_warning ? Color.Red : Color.Yellow);
			lblWarning.Text = msg;
			lblWarning.Visible = true;
			_warningTimer.Interval = show_duration;
			_warningTimer.Start();
		}
	}

	private void InfoBar_Resize(object sender, EventArgs e)
	{
		int num = (int)((float)base.Width * 0.7f);
		int num2 = base.Width - 88 - num;
		float num3 = (float)num2 - (float)num2 * _splitterRatio;
		lblSplitter.Left = base.Width - 88 - (int)num3;
		repositionControls();
	}

	private void InfoBar_Click(object sender, EventArgs e)
	{
		flip();
	}

	private void flip()
	{
		_currentFlip++;
		if (_currentFlip > 1)
		{
			_currentFlip = 0;
		}
		updateLabels();
	}

	private void updateLabels()
	{
		lblPageNo.Text = _currentFlip + 1 + "/" + 2;
		lblLeft1.Text = _left1[_currentFlip];
		lblLeft2.Text = _left2[_currentFlip];
		lblLeft3.Text = _left3[_currentFlip];
		lblRight1.Text = _right1[_currentFlip];
		lblRight2.Text = _right2[_currentFlip];
		lblRight3.Text = _right3[_currentFlip];
		for (int i = 0; i < 3; i++)
		{
			SetToolTipLeft(_currentFlip, i + 1, _leftToolTip[_currentFlip, i]);
			SetToolTipRight(_currentFlip, i + 1, _rightToolTip[_currentFlip, i]);
		}
		repositionControls();
	}

	private void chkButton1_MouseDown(object sender, MouseEventArgs e)
	{
		if (IsRightButton(e))
		{
			if (Common.ShiftKeyDown)
			{
				Button1MouseDown?.Invoke(this, new InfoBarAction
				{
					Action = _button1Action.Action,
					ButtonState = chkButton1.Checked,
					Button = e.Button
				});
			}
			else if (_frmInfoBarPopup_Button1 != null && _frmInfoBarPopup_Button1 != null && _frmInfoBarPopup_Button1.HasButtons)
			{
				_toolStripForm_Button1.Show(this, new Point(chkButton1.Left + chkButton1.Width / 2 - _frmInfoBarPopup_Button1.Width / 2, chkButton1.Top + chkButton1.Height));
			}
		}
	}

	private void chkButton2_MouseDown(object sender, MouseEventArgs e)
	{
		if (IsRightButton(e))
		{
			if (Common.ShiftKeyDown)
			{
				Button2MouseDown?.Invoke(this, new InfoBarAction
				{
					Action = _button2Action.Action,
					ButtonState = chkButton2.Checked,
					Button = e.Button
				});
			}
			else if (_frmInfoBarPopup_Button2 != null && _frmInfoBarPopup_Button2 != null && _frmInfoBarPopup_Button2.HasButtons)
			{
				_toolStripForm_Button2.Show(this, new Point(chkButton2.Left + chkButton2.Width / 2 - _frmInfoBarPopup_Button2.Width / 2, chkButton2.Top + chkButton2.Height));
			}
		}
	}

	private bool IsRightButton(MouseEventArgs e)
	{
		return e.Button == MouseButtons.Right;
	}

	private void lblFB_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			SwapRedBlue = !puresignal.InvertRedBlue;
		}
		else if (e.Button == MouseButtons.Right)
		{
			HideFeedback = !HideFeedback;
		}
	}

	private void setToolTips()
	{
		string text = "";
		if (!HideFeedback)
		{
			text = "Showing level, ";
		}
		if (puresignal.InvertRedBlue)
		{
			toolTip1.SetToolTip(lblFB, text + "Blue 0-90, Yellow 91-128, Green 129-181, Red 182+");
		}
		else
		{
			toolTip1.SetToolTip(lblFB, text + "Red 0-90, Yellow 91-128, Green 129-181, Blue 182+");
		}
	}

	private void replaceMainButton(int button, ActionTypes action, bool bState, MouseButtons mouseButton)
	{
		_preventClickEvents = true;
		switch (button)
		{
		case 1:
			_button1Action.Action = action;
			break;
		case 2:
			_button2Action.Action = action;
			break;
		}
		_preventClickEvents = false;
		UpdateButtonState(action, bState);
	}

	public CheckBoxTS GetPopupButton(int infoBarButton, int index)
	{
		switch (infoBarButton)
		{
		case 1:
			if (_frmInfoBarPopup_Button1 == null)
			{
				return null;
			}
			return _frmInfoBarPopup_Button1.GetPopupButton(index);
		case 2:
			if (_frmInfoBarPopup_Button2 == null)
			{
				return null;
			}
			return _frmInfoBarPopup_Button2.GetPopupButton(index);
		default:
			return null;
		}
	}

	private void lblSplitter_MouseDown(object sender, MouseEventArgs e)
	{
		if (!_dragging)
		{
			_dragging = true;
			_startX = e.X;
		}
	}

	private void lblSplitter_MouseEnter(object sender, EventArgs e)
	{
		lblSplitter.BackColor = Color.White;
		_oldCursor = Cursor.Current;
		Cursor = Cursors.SizeWE;
	}

	private void lblSplitter_MouseHover(object sender, EventArgs e)
	{
		lblSplitter.BackColor = Color.White;
	}

	private void lblSplitter_MouseLeave(object sender, EventArgs e)
	{
		lblSplitter.BackColor = Color.Silver;
		Cursor = _oldCursor;
	}

	private void lblSplitter_MouseMove(object sender, MouseEventArgs e)
	{
		if (_dragging)
		{
			SendMessage(base.Handle, 11, wParam: false, 0);
			int num = e.X - _startX;
			_ = lblSplitter.Left;
			int num2 = num + lblSplitter.Left;
			int num3 = (int)((float)base.Width * 0.7f);
			if (num2 < num3)
			{
				num2 = num3;
			}
			if (num2 > base.Width - 88 - 5)
			{
				num2 = base.Width - 88 - 5;
			}
			lblSplitter.Left = num2;
			_splitterRatio = (float)(num2 - num3) / (float)(base.Width - 88 - num3);
			repositionControls();
			SendMessage(base.Handle, 11, wParam: true, 0);
			Refresh();
		}
	}

	private void repositionControls()
	{
		if (!_okToResize || _shutDown || _currentFlip < 0 || _currentFlip > 1 || lblFB == null || lblPS == null || lblLeft1 == null || lblLeft2 == null || lblLeft3 == null || lblRight1 == null || lblRight2 == null || lblRight3 == null || lblWarning == null || lblSplitter == null || _left1Width == null || _left2Width == null || _left3Width == null || _right1Width == null || _right2Width == null || _right3Width == null)
		{
			return;
		}
		int num = lblSplitter.Left + lblSplitter.Width;
		int num2 = base.Width - num;
		int num3 = (int)Math.Ceiling((float)num2 / 2f);
		lblFB.Left = num;
		lblFB.Width = num3;
		lblPS.Left = num + num3;
		lblPS.Width = num3;
		lblLeft1.Width = _left1Width[_currentFlip];
		lblLeft2.Width = _left2Width[_currentFlip];
		lblLeft3.Width = _left3Width[_currentFlip];
		lblLeft2.Left = lblLeft1.Left + lblLeft1.Width;
		lblLeft3.Left = lblLeft2.Left + lblLeft2.Width;
		lblRight1.Width = _right1Width[_currentFlip];
		lblRight2.Width = _right2Width[_currentFlip];
		lblRight3.Width = _right3Width[_currentFlip];
		int num4 = lblRight1.Width + lblRight2.Width + lblRight3.Width + 4;
		lblRight1.Left = lblFB.Left - num4;
		lblRight2.Left = lblRight1.Left + lblRight1.Width;
		lblRight3.Left = lblRight1.Left + lblRight1.Width + lblRight2.Width;
		lblWarning.Width = lblSplitter.Left - lblWarning.Left - 4;
		_useSmallFonts = num2 <= 180;
		if (_useSmallFonts)
		{
			if (lblPS.Font != _smallPSFont)
			{
				lblPS.Font = _smallPSFont;
			}
			if (lblFB.Text == "Feedback")
			{
				lblFB.Text = "FB";
			}
			if (lblPS.Text == "Correcting")
			{
				lblPS.Text = "Correct";
			}
		}
		else
		{
			if (lblPS.Font != _normalPSFont)
			{
				lblPS.Font = _normalPSFont;
			}
			if (lblFB.Text == "FB")
			{
				lblFB.Text = "Feedback";
			}
			if (lblPS.Text == "Correct")
			{
				lblPS.Text = "Correcting";
			}
		}
		lblLeft1.Visible = !lblSplitter.Bounds.IntersectsWith(lblLeft1.Bounds);
		lblLeft2.Visible = lblLeft1.Visible && !lblSplitter.Bounds.IntersectsWith(lblLeft2.Bounds);
		lblLeft3.Visible = lblLeft2.Visible && !lblSplitter.Bounds.IntersectsWith(lblLeft3.Bounds);
		lblRight1.Visible = (!(lblLeft3.Text != "") || !lblRight1.Bounds.IntersectsWith(lblLeft3.Bounds)) && (!(lblLeft2.Text != "") || !lblRight1.Bounds.IntersectsWith(lblLeft2.Bounds)) && (!(lblLeft1.Text != "") || !lblRight1.Bounds.IntersectsWith(lblLeft1.Bounds));
		lblRight2.Visible = (!(lblLeft3.Text != "") || !lblRight2.Bounds.IntersectsWith(lblLeft3.Bounds)) && (!(lblLeft2.Text != "") || !lblRight2.Bounds.IntersectsWith(lblLeft2.Bounds)) && (!(lblLeft1.Text != "") || !lblRight2.Bounds.IntersectsWith(lblLeft1.Bounds));
		lblRight3.Visible = (!(lblLeft3.Text != "") || !lblRight3.Bounds.IntersectsWith(lblLeft3.Bounds)) && (!(lblLeft2.Text != "") || !lblRight3.Bounds.IntersectsWith(lblLeft2.Bounds)) && (!(lblLeft1.Text != "") || !lblRight3.Bounds.IntersectsWith(lblLeft1.Bounds));
	}

	private void lblSplitter_MouseUp(object sender, MouseEventArgs e)
	{
		_dragging = false;
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
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		this.lblFB = new System.Windows.Forms.LabelTS();
		this.lblPS = new System.Windows.Forms.LabelTS();
		this.chkButton2 = new System.Windows.Forms.CheckBoxTS();
		this.chkButton1 = new System.Windows.Forms.CheckBoxTS();
		this.lblPageNo = new System.Windows.Forms.LabelTS();
		this.lblSplitter = new System.Windows.Forms.LabelTS();
		this.lblWarning = new System.Windows.Forms.LabelTS();
		this.lblRight3 = new System.Windows.Forms.LabelTS();
		this.lblRight2 = new System.Windows.Forms.LabelTS();
		this.lblRight1 = new System.Windows.Forms.LabelTS();
		this.lblLeft3 = new System.Windows.Forms.LabelTS();
		this.lblLeft2 = new System.Windows.Forms.LabelTS();
		this.lblLeft1 = new System.Windows.Forms.LabelTS();
		base.SuspendLayout();
		this.lblFB.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.lblFB.BackColor = System.Drawing.Color.FromArgb(0, 64, 0);
		this.lblFB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.lblFB.ForeColor = System.Drawing.Color.Black;
		this.lblFB.Image = null;
		this.lblFB.Location = new System.Drawing.Point(801, 0);
		this.lblFB.MinimumSize = new System.Drawing.Size(44, 24);
		this.lblFB.Name = "lblFB";
		this.lblFB.Size = new System.Drawing.Size(44, 24);
		this.lblFB.TabIndex = 34;
		this.lblFB.Text = "FB";
		this.lblFB.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip1.SetToolTip(this.lblFB, "Feedback level in order. Blue > 181, Green > 128, Yellow > 90, Red >= 0");
		this.lblFB.MouseDown += new System.Windows.Forms.MouseEventHandler(lblFB_MouseDown);
		this.lblPS.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.lblPS.BackColor = System.Drawing.Color.FromArgb(0, 64, 0);
		this.lblPS.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lblPS.ForeColor = System.Drawing.Color.Black;
		this.lblPS.Image = null;
		this.lblPS.Location = new System.Drawing.Point(845, 0);
		this.lblPS.MinimumSize = new System.Drawing.Size(44, 24);
		this.lblPS.Name = "lblPS";
		this.lblPS.Size = new System.Drawing.Size(44, 24);
		this.lblPS.TabIndex = 33;
		this.lblPS.Text = "Pure Signal2";
		this.lblPS.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip1.SetToolTip(this.lblPS, "PS2 is correcting if 'Correct' is shown");
		this.chkButton2.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkButton2.FlatAppearance.BorderSize = 0;
		this.chkButton2.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver;
		this.chkButton2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
		this.chkButton2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
		this.chkButton2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.chkButton2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.chkButton2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.chkButton2.Image = null;
		this.chkButton2.ImeMode = System.Windows.Forms.ImeMode.Off;
		this.chkButton2.Location = new System.Drawing.Point(59, 1);
		this.chkButton2.Name = "chkButton2";
		this.chkButton2.Size = new System.Drawing.Size(50, 23);
		this.chkButton2.TabIndex = 32;
		this.chkButton2.Text = "Peak";
		this.chkButton2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip1.SetToolTip(this.chkButton2, "Show/hide active peaks");
		this.chkButton2.CheckedChanged += new System.EventHandler(chkButton2_CheckedChanged);
		this.chkButton2.MouseDown += new System.Windows.Forms.MouseEventHandler(chkButton2_MouseDown);
		this.chkButton1.Appearance = System.Windows.Forms.Appearance.Button;
		this.chkButton1.FlatAppearance.BorderSize = 0;
		this.chkButton1.FlatAppearance.CheckedBackColor = System.Drawing.Color.Silver;
		this.chkButton1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
		this.chkButton1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Gray;
		this.chkButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.chkButton1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold);
		this.chkButton1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.chkButton1.Image = null;
		this.chkButton1.ImeMode = System.Windows.Forms.ImeMode.Off;
		this.chkButton1.Location = new System.Drawing.Point(3, 1);
		this.chkButton1.Name = "chkButton1";
		this.chkButton1.Size = new System.Drawing.Size(50, 23);
		this.chkButton1.TabIndex = 31;
		this.chkButton1.Text = "Blobs";
		this.chkButton1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.toolTip1.SetToolTip(this.chkButton1, "Show/hide spectrum peak blobs");
		this.chkButton1.CheckedChanged += new System.EventHandler(chkButton1_CheckedChanged);
		this.chkButton1.MouseDown += new System.Windows.Forms.MouseEventHandler(chkButton1_MouseDown);
		this.lblPageNo.AutoSize = true;
		this.lblPageNo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lblPageNo.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.lblPageNo.Image = null;
		this.lblPageNo.Location = new System.Drawing.Point(115, 6);
		this.lblPageNo.Name = "lblPageNo";
		this.lblPageNo.Size = new System.Drawing.Size(24, 13);
		this.lblPageNo.TabIndex = 43;
		this.lblPageNo.Text = "2/2";
		this.lblPageNo.Click += new System.EventHandler(InfoBar_Click);
		this.lblSplitter.BackColor = System.Drawing.Color.White;
		this.lblSplitter.ForeColor = System.Drawing.Color.White;
		this.lblSplitter.Image = null;
		this.lblSplitter.Location = new System.Drawing.Point(796, 0);
		this.lblSplitter.Margin = new System.Windows.Forms.Padding(0);
		this.lblSplitter.Name = "lblSplitter";
		this.lblSplitter.Size = new System.Drawing.Size(5, 24);
		this.lblSplitter.TabIndex = 42;
		this.lblSplitter.MouseDown += new System.Windows.Forms.MouseEventHandler(lblSplitter_MouseDown);
		this.lblSplitter.MouseEnter += new System.EventHandler(lblSplitter_MouseEnter);
		this.lblSplitter.MouseLeave += new System.EventHandler(lblSplitter_MouseLeave);
		this.lblSplitter.MouseHover += new System.EventHandler(lblSplitter_MouseHover);
		this.lblSplitter.MouseMove += new System.Windows.Forms.MouseEventHandler(lblSplitter_MouseMove);
		this.lblSplitter.MouseUp += new System.Windows.Forms.MouseEventHandler(lblSplitter_MouseUp);
		this.lblWarning.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.lblWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.lblWarning.ForeColor = System.Drawing.Color.Red;
		this.lblWarning.Image = null;
		this.lblWarning.Location = new System.Drawing.Point(138, 0);
		this.lblWarning.Name = "lblWarning";
		this.lblWarning.Size = new System.Drawing.Size(654, 24);
		this.lblWarning.TabIndex = 41;
		this.lblWarning.Text = "Warning";
		this.lblWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblWarning.Visible = false;
		this.lblRight3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.lblRight3.BackColor = System.Drawing.Color.Black;
		this.lblRight3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lblRight3.ForeColor = System.Drawing.Color.DodgerBlue;
		this.lblRight3.Image = null;
		this.lblRight3.Location = new System.Drawing.Point(680, 3);
		this.lblRight3.Name = "lblRight3";
		this.lblRight3.Size = new System.Drawing.Size(112, 15);
		this.lblRight3.TabIndex = 40;
		this.lblRight3.Text = "000000000000";
		this.lblRight3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblRight3.Click += new System.EventHandler(InfoBar_Click);
		this.lblRight2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.lblRight2.BackColor = System.Drawing.Color.Black;
		this.lblRight2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lblRight2.ForeColor = System.Drawing.Color.DodgerBlue;
		this.lblRight2.Image = null;
		this.lblRight2.Location = new System.Drawing.Point(592, 3);
		this.lblRight2.Name = "lblRight2";
		this.lblRight2.Size = new System.Drawing.Size(88, 15);
		this.lblRight2.TabIndex = 39;
		this.lblRight2.Text = "000000000000";
		this.lblRight2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblRight2.Click += new System.EventHandler(InfoBar_Click);
		this.lblRight1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.lblRight1.BackColor = System.Drawing.Color.Black;
		this.lblRight1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lblRight1.ForeColor = System.Drawing.Color.DodgerBlue;
		this.lblRight1.Image = null;
		this.lblRight1.Location = new System.Drawing.Point(504, 3);
		this.lblRight1.Name = "lblRight1";
		this.lblRight1.Size = new System.Drawing.Size(88, 15);
		this.lblRight1.TabIndex = 38;
		this.lblRight1.Text = "000000000000";
		this.lblRight1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblRight1.Click += new System.EventHandler(InfoBar_Click);
		this.lblLeft3.BackColor = System.Drawing.Color.Black;
		this.lblLeft3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lblLeft3.ForeColor = System.Drawing.Color.DodgerBlue;
		this.lblLeft3.Image = null;
		this.lblLeft3.Location = new System.Drawing.Point(294, 4);
		this.lblLeft3.Name = "lblLeft3";
		this.lblLeft3.Size = new System.Drawing.Size(112, 15);
		this.lblLeft3.TabIndex = 37;
		this.lblLeft3.Text = "000000000000";
		this.lblLeft3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblLeft3.Click += new System.EventHandler(InfoBar_Click);
		this.lblLeft2.BackColor = System.Drawing.Color.Black;
		this.lblLeft2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lblLeft2.ForeColor = System.Drawing.Color.DodgerBlue;
		this.lblLeft2.Image = null;
		this.lblLeft2.Location = new System.Drawing.Point(226, 4);
		this.lblLeft2.Name = "lblLeft2";
		this.lblLeft2.Size = new System.Drawing.Size(68, 15);
		this.lblLeft2.TabIndex = 36;
		this.lblLeft2.Text = "0000sec";
		this.lblLeft2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblLeft2.Click += new System.EventHandler(InfoBar_Click);
		this.lblLeft1.BackColor = System.Drawing.Color.Black;
		this.lblLeft1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lblLeft1.ForeColor = System.Drawing.Color.DodgerBlue;
		this.lblLeft1.Image = null;
		this.lblLeft1.Location = new System.Drawing.Point(138, 4);
		this.lblLeft1.Name = "lblLeft1";
		this.lblLeft1.Size = new System.Drawing.Size(88, 15);
		this.lblLeft1.TabIndex = 35;
		this.lblLeft1.Text = "000000000000";
		this.lblLeft1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblLeft1.Click += new System.EventHandler(InfoBar_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Gray;
		base.Controls.Add(this.lblPageNo);
		base.Controls.Add(this.lblSplitter);
		base.Controls.Add(this.lblWarning);
		base.Controls.Add(this.lblRight3);
		base.Controls.Add(this.lblRight2);
		base.Controls.Add(this.lblRight1);
		base.Controls.Add(this.lblLeft3);
		base.Controls.Add(this.lblLeft2);
		base.Controls.Add(this.lblLeft1);
		base.Controls.Add(this.lblFB);
		base.Controls.Add(this.lblPS);
		base.Controls.Add(this.chkButton2);
		base.Controls.Add(this.chkButton1);
		base.Name = "ucInfoBar";
		base.Size = new System.Drawing.Size(889, 24);
		base.Click += new System.EventHandler(InfoBar_Click);
		base.Resize += new System.EventHandler(InfoBar_Resize);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
