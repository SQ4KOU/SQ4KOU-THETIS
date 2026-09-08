using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Thetis.Properties;

namespace Thetis;

public class ucMeter : UserControl
{
	public const int MIN_CONTAINER_WIDTH = 24;

	public const int MIN_CONTAINER_HEIGHT = 24;

	private int _sequence;

	private bool _dragging;

	private bool _resizing;

	private bool _floating;

	private Point _point;

	private Point _clientPos;

	private Size _size;

	private Point _dockedLocation;

	private Size _dockedSize;

	private Cursor _cursor;

	private int _rx;

	private Point _delta;

	private Axis _axisLock;

	private bool _pinOnTop;

	private Console _console;

	private bool _mox;

	private string _id;

	private bool _border;

	private bool _no_controls;

	private bool _locked;

	private bool _enabled;

	private bool _show_on_rx;

	private bool _show_on_tx;

	private bool _hidden_by_macro;

	private bool _container_minimises;

	private bool _container_hides_when_rx_not_used;

	private string _notes;

	private int _height;

	private bool _autoHeight;

	private ToolTip _tool_tip;

	private IWin32Window _tool_tip_owner;

	private Guid _touch_guid;

	private IContainer components;

	private Panel pnlBar;

	private ButtonTS btnFloat;

	private PictureBox pbGrab;

	private LabelTS lblRX;

	private ButtonTS btnAxis;

	private ButtonTS btnPin;

	private ButtonTS btnSettings;

	private Panel pnlContainer;

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public Console Console
	{
		set
		{
			_console = value;
			if (_console != null)
			{
				if (_touch_guid != Guid.Empty)
				{
					TouchHandler.DisableTouchSupport(_touch_guid);
				}
				if (_console.TouchSupport)
				{
					_touch_guid = TouchHandler.EnableTouchSupport(pnlContainer, HandleTouchDown, HandleTouchMove, HandleTouchUp, 7);
				}
				else
				{
					_touch_guid = Guid.Empty;
				}
				_mox = (_console.RX2Enabled && _console.VFOBTX && _console.MOX) || (!_console.RX2Enabled && _console.MOX);
				setTitle();
				addDelegates();
			}
		}
	}

	public int Sequence
	{
		get
		{
			return _sequence;
		}
		set
		{
			_sequence = value;
		}
	}

	public string ID
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value.Replace("|", "");
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public Panel DisplayContainer => pnlContainer;

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public Point DockedLocation
	{
		get
		{
			return _dockedLocation;
		}
		set
		{
			_dockedLocation = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public Size DockedSize
	{
		get
		{
			return _dockedSize;
		}
		set
		{
			_dockedSize = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool Floating
	{
		get
		{
			return _floating;
		}
		set
		{
			_floating = value;
			setTopBarButtons();
			setTopMost();
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public int RX
	{
		get
		{
			return _rx;
		}
		set
		{
			_rx = value;
			setTitle();
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public Point Delta
	{
		get
		{
			return _delta;
		}
		set
		{
			_delta = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool PinOnTop
	{
		get
		{
			return _pinOnTop;
		}
		set
		{
			_pinOnTop = value;
			setPinOnTopButton();
			setTopMost();
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public Axis AxisLock
	{
		get
		{
			return _axisLock;
		}
		set
		{
			_axisLock = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool UCBorder
	{
		get
		{
			return _border;
		}
		set
		{
			_border = value;
			setupBorder();
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool Locked
	{
		get
		{
			return _locked;
		}
		set
		{
			_locked = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool MeterEnabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			_enabled = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool HiddenByMacro
	{
		get
		{
			return _hidden_by_macro;
		}
		set
		{
			_hidden_by_macro = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShowOnRX
	{
		get
		{
			return _show_on_rx;
		}
		set
		{
			_show_on_rx = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShowOnTX
	{
		get
		{
			return _show_on_tx;
		}
		set
		{
			_show_on_tx = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ContainerMinimises
	{
		get
		{
			return _container_minimises;
		}
		set
		{
			_container_minimises = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ContainerHidesWhenRXNotUsed
	{
		get
		{
			return _container_hides_when_rx_not_used;
		}
		set
		{
			_container_hides_when_rx_not_used = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public string Notes
	{
		get
		{
			return _notes;
		}
		set
		{
			_notes = value.Replace("|", "");
			setTitle();
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool NoControls
	{
		get
		{
			return _no_controls;
		}
		set
		{
			_no_controls = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool AutoHeight
	{
		get
		{
			return _autoHeight;
		}
		set
		{
			_autoHeight = value;
			if (_autoHeight && !_resizing)
			{
				forceResize();
			}
		}
	}

	public bool IsTopMost
	{
		get
		{
			if (base.Parent != null)
			{
				if (base.Parent is frmMeterDisplay frmMeterDisplay2)
				{
					return frmMeterDisplay2.TopMost;
				}
				return false;
			}
			return false;
		}
	}

	[Browsable(true)]
	[Category("Action")]
	public event EventHandler FloatingDockedClicked;

	[Browsable(true)]
	[Category("Action")]
	public event EventHandler SettingsClicked;

	public event EventHandler DockedMoved;

	public ucMeter()
	{
		InitializeComponent();
		Common.DoubleBufferAll(this, enabled: true);
		_sequence = 0;
		pnlContainer.Location = new Point(0, 0);
		pnlContainer.Size = new Size(base.ClientSize.Width, base.ClientSize.Height);
		_height = 24;
		_autoHeight = false;
		_touch_guid = Guid.Empty;
		_console = null;
		_id = Guid.NewGuid().ToString();
		_border = true;
		_no_controls = false;
		_enabled = true;
		_show_on_rx = true;
		_show_on_tx = true;
		_hidden_by_macro = false;
		_container_minimises = true;
		_container_hides_when_rx_not_used = true;
		_notes = "";
		_tool_tip = new ToolTip();
		_tool_tip_owner = null;
		_tool_tip.InitialDelay = 0;
		_tool_tip.ReshowDelay = 0;
		_tool_tip.UseFading = true;
		_tool_tip.ShowAlways = true;
		base.Name = "UCMeter_" + _id;
		btnFloat.BackgroundImageLayout = ImageLayout.Center;
		_axisLock = Axis.TOPLEFT;
		_delta = new Point(0, 0);
		_pinOnTop = false;
		storeLocation();
		setTopBarButtons();
		setTitle();
		setupBorder();
		btnAxis.BringToFront();
		btnFloat.BringToFront();
		btnPin.BringToFront();
		btnSettings.BringToFront();
		btnAxis.Hide();
		_cursor = Cursor.Current;
		pnlBar.Hide();
		pbGrab.Hide();
	}

	~ucMeter()
	{
		if (_touch_guid != Guid.Empty)
		{
			TouchHandler.DisableTouchSupport(_touch_guid);
		}
	}

	private void HandleTouchDown(int x, int y)
	{
		if (pnlBar.Visible && pnlBar.Bounds.Contains(x, y))
		{
			pnlBar_MouseDown(this, new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
		}
		else if (pbGrab.Visible && pbGrab.Bounds.Contains(x, y))
		{
			pbGrab_MouseEnter(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
			pbGrab_MouseDown(this, new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
		}
		else if (lblRX.Visible && lblRX.Bounds.Contains(x, y))
		{
			lblRX_MouseDown(this, new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
		}
		else if (pnlContainer.Bounds.Contains(x, y))
		{
			pnlContainer_MouseMove(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
		}
	}

	private void HandleTouchMove(int x, int y)
	{
		if (pnlBar.Visible && pnlBar.Bounds.Contains(x, y))
		{
			pnlBar_MouseMove(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
		}
		else if (pbGrab.Visible && pbGrab.Bounds.Contains(x, y))
		{
			pbGrab_MouseMove(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
		}
		else if (lblRX.Visible && lblRX.Bounds.Contains(x, y))
		{
			lblRX_MouseMove(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
		}
		if (pnlContainer.Bounds.Contains(x, y))
		{
			pnlContainer_MouseMove(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
		}
	}

	private void HandleTouchUp(int x, int y)
	{
		if (pnlBar.Visible && pnlBar.Bounds.Contains(x, y))
		{
			pnlBar_MouseUp(this, new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
			pnlBar_MouseLeave(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
		}
		else if (pbGrab.Visible && pbGrab.Bounds.Contains(x, y))
		{
			pbGrab_MouseUp(this, new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
			pbGrab_MouseLeave(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
		}
		else if (lblRX.Visible && lblRX.Bounds.Contains(x, y))
		{
			lblRX_MouseUp(this, new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
			lblRX_MouseLeave(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
		}
		else if (pnlContainer.Bounds.Contains(x, y))
		{
			pnlContainer_MouseLeave(this, new MouseEventArgs(MouseButtons.None, 0, x, y, 0));
		}
	}

	private void addDelegates()
	{
		if (_console != null)
		{
			Console console = _console;
			console.MoxChangeHandlers = (Console.MoxChanged)Delegate.Combine(console.MoxChangeHandlers, new Console.MoxChanged(OnMoxChangeHandler));
		}
	}

	public void RemoveDelegates()
	{
		if (_console != null)
		{
			Console console = _console;
			console.MoxChangeHandlers = (Console.MoxChanged)Delegate.Remove(console.MoxChangeHandlers, new Console.MoxChanged(OnMoxChangeHandler));
		}
	}

	private void OnMoxChangeHandler(int rx, bool oldMox, bool newMox)
	{
		if (rx == _rx)
		{
			_mox = newMox;
			setTitle();
		}
	}

	private void pnlBar_MouseDown(object sender, MouseEventArgs e)
	{
		if (_floating)
		{
			_point = base.Parent.PointToClient(Cursor.Position);
		}
		else
		{
			BringToFront();
			_point.X = e.X;
			_point.Y = e.Y;
		}
		_dragging = true;
	}

	public void Repaint()
	{
		if (!_floating && base.Parent != null)
		{
			base.Parent.Invalidate(base.Bounds, invalidateChildren: true);
			base.Parent.Update();
		}
	}

	private void pnlBar_MouseLeave(object sender, EventArgs e)
	{
		uiComponentMouseLeave();
	}

	private void pnlBar_MouseUp(object sender, MouseEventArgs e)
	{
		_point = Point.Empty;
		_dragging = false;
		hideToolTip();
		DockedMoved?.Invoke(this, e);
	}

	private void pnlBar_MouseMove(object sender, MouseEventArgs e)
	{
		if (!_dragging)
		{
			return;
		}
		if (_floating)
		{
			Point point = base.Parent.PointToClient(Cursor.Position);
			int num = point.X - _point.X;
			int num2 = point.Y - _point.Y;
			Point point2 = new Point(base.Parent.Left + num, base.Parent.Top + num2);
			if (Common.CtrlKeyDown)
			{
				point2.X = roundToNearestTen(point2.X);
				point2.Y = roundToNearestTen(point2.Y);
			}
			if (base.Parent != null && point2 != base.Parent.Location)
			{
				base.Parent.Location = point2;
				showToolTip($"{point2.X}, {point2.Y}", base.Parent);
			}
			return;
		}
		Point location = e.Location;
		int num3 = location.X - _point.X;
		int num4 = location.Y - _point.Y;
		num3 += base.Location.X;
		num4 += base.Location.Y;
		if (Common.CtrlKeyDown)
		{
			num3 = roundToNearestTen(num3);
			num4 = roundToNearestTen(num4);
		}
		if (num3 < 0)
		{
			num3 = 0;
		}
		if (num4 < 0)
		{
			num4 = 0;
		}
		if (num3 > base.Parent.ClientSize.Width - base.Width)
		{
			num3 = base.Parent.ClientSize.Width - base.Width;
		}
		if (num4 > base.Parent.ClientSize.Height - base.Height)
		{
			num4 = base.Parent.ClientSize.Height - base.Height;
		}
		Point point3 = new Point(num3, num4);
		if (point3 != base.Location)
		{
			base.Location = point3;
			Repaint();
			showToolTip($"{point3.X}, {point3.Y}", this);
		}
	}

	private void showToolTip(string msg, Control window, bool is_resize = false)
	{
		_tool_tip_owner = window;
		if (is_resize)
		{
			_tool_tip.Show(msg + "\nctrl to lock", _tool_tip_owner, new Point(window.Size.Width, window.Size.Height - pnlBar.Height * 2));
		}
		else
		{
			_tool_tip.Show(msg + "\nctrl to lock", _tool_tip_owner, new Point(0, pnlBar.Height));
		}
	}

	private void hideToolTip()
	{
		if (_tool_tip != null && _tool_tip_owner != null)
		{
			_tool_tip.Hide(_tool_tip_owner);
			_tool_tip_owner = null;
		}
	}

	private int roundToNearestTen(int number)
	{
		return (number + 5) / 10 * 10;
	}

	private void pbGrab_MouseDown(object sender, MouseEventArgs e)
	{
		_clientPos = base.Parent.PointToClient(Cursor.Position);
		_size.Width = base.Size.Width;
		_size.Height = base.Size.Height;
		_resizing = true;
		BringToFront();
	}

	private void pbGrab_MouseUp(object sender, MouseEventArgs e)
	{
		_clientPos = Point.Empty;
		_resizing = false;
		hideToolTip();
		forceResize();
	}

	private void forceResize(bool shrink = false)
	{
		_resizing = true;
		if (_autoHeight)
		{
			if (_floating)
			{
				if (base.Parent != null && base.Parent.ClientSize.Height != _height)
				{
					resize(base.Parent.ClientSize.Width, _height, shrink);
				}
			}
			else if (base.Size.Height != _height)
			{
				resize(base.Size.Width, _height);
			}
		}
		else if (_floating)
		{
			if (base.Parent != null)
			{
				resize(base.Parent.ClientSize.Width, base.Parent.ClientSize.Height, shrink);
			}
		}
		else
		{
			resize(base.Size.Width, base.Size.Height);
		}
		_resizing = false;
	}

	public void ChangeHeight(int height)
	{
		if (!_autoHeight)
		{
			return;
		}
		_height = height;
		if (_resizing || _dragging)
		{
			return;
		}
		if (_floating)
		{
			if (base.Parent == null || !base.Parent.IsHandleCreated)
			{
				return;
			}
			bool shrink = false;
			int num = Screen.FromControl(base.Parent).WorkingArea.Height;
			if (num < height)
			{
				Point point = new Point(base.Parent.Location.X, 0);
				if (point != base.Parent.Location)
				{
					base.Parent.Location = point;
				}
				shrink = true;
			}
			height = Math.Min(height, num);
			if (base.Parent.ClientSize.Height != height)
			{
				_height = height;
				forceResize(shrink);
			}
		}
		else if (base.Size.Height != height)
		{
			forceResize();
		}
	}

	private void pbGrab_MouseMove(object sender, MouseEventArgs e)
	{
		if (!_resizing)
		{
			return;
		}
		Point point = base.Parent.PointToClient(Cursor.Position);
		int num = point.X - _clientPos.X;
		int num2 = point.Y - _clientPos.Y;
		int number = _size.Width + num;
		int number2 = _size.Height + num2;
		if (Common.CtrlKeyDown)
		{
			number = roundToNearestTen(number);
			number2 = roundToNearestTen(number2);
		}
		resize(number, number2);
		if (_floating)
		{
			if (base.Parent != null)
			{
				showToolTip($"{base.Parent.Size.Width}, {base.Parent.Size.Height}", base.Parent, is_resize: true);
			}
		}
		else
		{
			showToolTip($"{base.Size.Width}, {base.Size.Height}", this, is_resize: true);
		}
	}

	private void resize(int x, int y, bool shrink = false)
	{
		if (base.Parent == null)
		{
			return;
		}
		if (x < 24)
		{
			x = 24;
		}
		if (y < 24)
		{
			y = 24;
		}
		if (_floating)
		{
			base.Parent.ClientSize = new Size(x, y);
			base.Parent.PerformLayout();
			if (base.Parent != null)
			{
				Common.ForceFormOnScreen((Form)base.Parent, shrink);
			}
			return;
		}
		if (base.Left + x > base.Parent.ClientSize.Width)
		{
			x = base.Parent.ClientSize.Width - base.Left;
		}
		if (base.Top + y > base.Parent.ClientSize.Height)
		{
			y = base.Parent.ClientSize.Height - base.Top;
		}
		Size size = new Size(x, y);
		if (size != base.Size)
		{
			base.Size = size;
			PerformLayout();
			Repaint();
		}
	}

	private void storeLocation()
	{
		_dockedLocation = base.Location;
		_dockedSize = base.Size;
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void RestoreLocation()
	{
		bool flag = false;
		if (_dockedLocation != base.Location)
		{
			base.Location = _dockedLocation;
			flag = true;
		}
		if (_dockedSize != base.Size)
		{
			base.Size = _dockedSize;
			flag = true;
		}
		if (flag)
		{
			PerformLayout();
			Repaint();
		}
	}

	private void setTopBarButtons()
	{
		if (_floating)
		{
			btnFloat.BackgroundImage = Resources.dockIcon_dock;
			btnPin.Left = btnAxis.Left;
			btnAxis.Visible = false;
			btnPin.Visible = true;
		}
		else
		{
			btnFloat.BackgroundImage = Resources.dockIcon_float;
			btnPin.Left = btnAxis.Left - btnPin.Width;
			btnAxis.Visible = true;
			btnPin.Visible = false;
		}
		setAxisButton();
		setPinOnTopButton();
	}

	private void setTitle()
	{
		string text = (_mox ? "TX" : "RX");
		string firstLineOrWholeString = getFirstLineOrWholeString(_notes);
		lblRX.Text = text + _rx + ((firstLineOrWholeString != "") ? (" " + firstLineOrWholeString) : "");
	}

	private string getFirstLineOrWholeString(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		return input.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None)[0];
	}

	private void setupBorder()
	{
		base.BorderStyle = (_border ? BorderStyle.FixedSingle : BorderStyle.None);
	}

	private void btnFloat_Click(object sender, EventArgs e)
	{
		FloatingDockedClicked?.Invoke(this, e);
	}

	private void pbGrab_MouseEnter(object sender, EventArgs e)
	{
		_cursor = Cursor.Current;
		Cursor = Cursors.SizeNWSE;
	}

	private void pbGrab_MouseLeave(object sender, EventArgs e)
	{
		Cursor = _cursor;
		if (!_resizing && (!pbGrab.ClientRectangle.Contains(pbGrab.PointToClient(Control.MousePosition)) || !base.ClientRectangle.Contains(PointToClient(Control.MousePosition))))
		{
			mouseLeave();
		}
	}

	private void mouseLeave()
	{
		if (pnlBar.Visible)
		{
			pnlBar.Hide();
		}
		if (pbGrab.Visible)
		{
			pbGrab.Hide();
		}
	}

	private void btnFloat_MouseLeave(object sender, EventArgs e)
	{
		uiComponentMouseLeave();
	}

	private void lblRX_MouseDown(object sender, MouseEventArgs e)
	{
		if (_floating)
		{
			_point = base.Parent.PointToClient(Cursor.Position);
		}
		else
		{
			BringToFront();
			_point.X = e.X;
			_point.Y = e.Y;
		}
		_dragging = true;
	}

	private void lblRX_MouseUp(object sender, MouseEventArgs e)
	{
		_point = Point.Empty;
		_dragging = false;
		hideToolTip();
		DockedMoved?.Invoke(this, e);
	}

	private void lblRX_MouseMove(object sender, MouseEventArgs e)
	{
		if (!_dragging)
		{
			return;
		}
		Point point = base.Parent.PointToClient(Cursor.Position);
		int num = point.X - _point.X;
		int num2 = point.Y - _point.Y;
		if (_floating)
		{
			Point point2 = new Point(base.Parent.Left + num, base.Parent.Top + num2);
			if (Common.CtrlKeyDown)
			{
				point2.X = roundToNearestTen(point2.X);
				point2.Y = roundToNearestTen(point2.Y);
			}
			if (point2 != base.Parent.Location)
			{
				base.Parent.Location = point2;
			}
			if (base.Parent != null)
			{
				showToolTip($"{point2.X}, {point2.Y}", base.Parent);
			}
			return;
		}
		num -= lblRX.Left;
		num2 -= lblRX.Top;
		if (Common.CtrlKeyDown)
		{
			num = roundToNearestTen(num);
			num2 = roundToNearestTen(num2);
		}
		if (num < 0)
		{
			num = 0;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		if (num > base.Parent.ClientSize.Width - base.Width)
		{
			num = base.Parent.ClientSize.Width - base.Width;
		}
		if (num2 > base.Parent.ClientSize.Height - base.Height)
		{
			num2 = base.Parent.ClientSize.Height - base.Height;
		}
		Point point3 = new Point(num, num2);
		if (point3 != base.Location)
		{
			base.Location = point3;
			Repaint();
		}
		showToolTip($"{point3.X}, {point3.Y}", this);
	}

	private void lblRX_MouseLeave(object sender, EventArgs e)
	{
		uiComponentMouseLeave();
	}

	private void ucMeter_LocationChanged(object sender, EventArgs e)
	{
		if (!_floating && _dragging && _dockedLocation != base.Location)
		{
			_dockedLocation = base.Location;
		}
	}

	private void ucMeter_SizeChanged(object sender, EventArgs e)
	{
		if (!_floating && _resizing)
		{
			_dockedSize = base.Size;
		}
	}

	private void btnAxis_Click(object sender, EventArgs e)
	{
		MouseEventArgs obj = (MouseEventArgs)e;
		int axisLock = (int)_axisLock;
		axisLock = ((obj.Button != MouseButtons.Right) ? (axisLock + 1) : (axisLock - 1));
		if (axisLock > 7)
		{
			axisLock = 0;
		}
		if (axisLock < 0)
		{
			axisLock = 7;
		}
		_axisLock = (Axis)axisLock;
		setAxisButton();
		if (_console != null)
		{
			Delta = new Point(_console.HDelta, _console.VDelta);
			DockedLocation = new Point(base.Left, base.Top);
		}
	}

	private void setAxisButton()
	{
		switch (_axisLock)
		{
		case Axis.LEFT:
			btnAxis.BackgroundImage = Resources.arrow_left;
			break;
		case Axis.TOPLEFT:
			btnAxis.BackgroundImage = Resources.arrow_topleft;
			break;
		case Axis.TOP:
			btnAxis.BackgroundImage = Resources.arrow_up;
			break;
		case Axis.TOPRIGHT:
			btnAxis.BackgroundImage = Resources.arrow_topright;
			break;
		case Axis.RIGHT:
			btnAxis.BackgroundImage = Resources.arrow_right;
			break;
		case Axis.BOTTOMRIGHT:
			btnAxis.BackgroundImage = Resources.arrow_bottomright;
			break;
		case Axis.BOTTOM:
			btnAxis.BackgroundImage = Resources.down;
			break;
		case Axis.BOTTOMLEFT:
			btnAxis.BackgroundImage = Resources.arrow_bottomleft;
			break;
		}
	}

	private void setPinOnTopButton()
	{
		btnPin.BackgroundImage = (_pinOnTop ? Resources.pin_on_top : Resources.pin_not_on_top);
	}

	private void btnPin_Click(object sender, EventArgs e)
	{
		_pinOnTop = !_pinOnTop;
		setPinOnTopButton();
		setTopMost();
	}

	private void setTopMost()
	{
		if (_floating && base.Parent != null && base.Parent is frmMeterDisplay frmMeterDisplay2)
		{
			frmMeterDisplay2.TopMost = _pinOnTop;
		}
	}

	public override string ToString()
	{
		return ID + "|" + RX + "|" + DockedLocation.X + "|" + DockedLocation.Y + "|" + DockedSize.Width + "|" + DockedSize.Height + "|" + Floating.ToString().ToLower() + "|" + Delta.X + "|" + Delta.Y + "|" + AxisLock.ToString() + "|" + PinOnTop.ToString().ToLower() + "|" + UCBorder.ToString().ToLower() + "|" + Common.ColourToString(BackColor) + "|" + NoControls.ToString().ToLower() + "|" + MeterEnabled.ToString().ToLower() + "|" + Notes + "|" + ContainerMinimises.ToString().ToLower() + "|" + AutoHeight.ToString().ToLower() + "|" + ShowOnRX.ToString().ToLower() + "|" + ShowOnTX.ToString().ToLower() + "|" + Locked.ToString().ToLower() + "|" + ContainerHidesWhenRXNotUsed.ToString().ToLower() + "|" + HiddenByMacro.ToString().ToLower();
	}

	public bool TryParse(string str)
	{
		bool flag = false;
		int result = 0;
		int result2 = 0;
		int result3 = 0;
		int result4 = 0;
		int result5 = 0;
		bool result6 = false;
		bool result7 = false;
		bool result8 = false;
		bool result9 = false;
		bool result10 = true;
		bool result11 = true;
		bool result12 = false;
		bool result13 = true;
		bool result14 = true;
		bool result15 = false;
		bool result16 = true;
		bool result17 = false;
		if (str != "")
		{
			string[] array = str.Split('|');
			if (array.Length >= 13)
			{
				flag = array[0] != "";
				if (flag)
				{
					ID = array[0];
				}
				if (flag)
				{
					int.TryParse(array[1], out result5);
				}
				if (flag)
				{
					RX = result5;
				}
				if (flag)
				{
					flag = int.TryParse(array[2], out result);
				}
				if (flag)
				{
					flag = int.TryParse(array[3], out result2);
				}
				if (flag)
				{
					flag = int.TryParse(array[4], out result3);
				}
				if (flag)
				{
					flag = int.TryParse(array[5], out result4);
				}
				if (flag)
				{
					DockedLocation = new Point(result, result2);
					DockedSize = new Size(result3, result4);
				}
				if (flag)
				{
					flag = bool.TryParse(array[6], out result6);
				}
				if (flag)
				{
					Floating = result6;
				}
				if (flag)
				{
					flag = int.TryParse(array[7], out result);
				}
				if (flag)
				{
					flag = int.TryParse(array[8], out result2);
				}
				if (flag)
				{
					Delta = new Point(result, result2);
				}
				if (flag)
				{
					try
					{
						AxisLock = (Axis)Enum.Parse(typeof(Axis), array[9]);
					}
					catch
					{
						flag = false;
					}
				}
				if (flag)
				{
					flag = bool.TryParse(array[10], out result7);
				}
				if (flag)
				{
					PinOnTop = result7;
				}
				if (flag)
				{
					flag = bool.TryParse(array[11], out result8);
				}
				if (flag)
				{
					UCBorder = result8;
				}
				Color color = Common.ColourFromString(array[12]);
				flag = color != Color.Empty;
				if (flag)
				{
					BackColor = color;
				}
				if (flag && array.Length > 13)
				{
					flag = bool.TryParse(array[13], out result9);
					if (flag)
					{
						NoControls = result9;
					}
				}
				if (flag && array.Length > 14)
				{
					flag = bool.TryParse(array[14], out result10);
					if (flag)
					{
						MeterEnabled = result10;
					}
				}
				if (flag && array.Length > 15)
				{
					Notes = array[15];
				}
				if (flag && array.Length > 16)
				{
					if (flag)
					{
						flag = bool.TryParse(array[16], out result11);
					}
					if (flag)
					{
						ContainerMinimises = result11;
					}
				}
				if (flag && array.Length > 17)
				{
					flag = bool.TryParse(array[17], out result12);
					if (flag)
					{
						AutoHeight = result12;
					}
				}
				if (flag && array.Length > 18)
				{
					if (bool.TryParse(array[18], out result13))
					{
						ShowOnRX = result13;
					}
					flag = bool.TryParse(array[19], out result14);
					if (flag)
					{
						ShowOnTX = result14;
					}
				}
				if (flag && array.Length > 20)
				{
					flag = bool.TryParse(array[20], out result15);
					if (flag)
					{
						Locked = result15;
					}
				}
				if (flag && array.Length > 21)
				{
					flag = bool.TryParse(array[21], out result16);
					if (flag)
					{
						ContainerHidesWhenRXNotUsed = result16;
					}
				}
				if (flag && array.Length > 22)
				{
					flag = bool.TryParse(array[22], out result17);
					if (flag)
					{
						HiddenByMacro = result17;
					}
				}
			}
		}
		return flag;
	}

	private void btnAxis_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			btnAxis_Click(sender, e);
		}
	}

	private void btnAxis_MouseLeave(object sender, EventArgs e)
	{
		uiComponentMouseLeave();
	}

	private void btnPin_MouseLeave(object sender, EventArgs e)
	{
		uiComponentMouseLeave();
	}

	private void btnSettings_Click(object sender, EventArgs e)
	{
		SettingsClicked?.Invoke(this, e);
	}

	private void btnSettings_MouseLeave(object sender, EventArgs e)
	{
		uiComponentMouseLeave();
	}

	private void uiComponentMouseLeave()
	{
		if (!_dragging && (!pnlBar.ClientRectangle.Contains(pnlBar.PointToClient(Control.MousePosition)) || !base.ClientRectangle.Contains(PointToClient(Control.MousePosition))))
		{
			mouseLeave();
		}
	}

	private void ucMeter_MouseLeave(object sender, EventArgs e)
	{
		if (!_dragging && !_resizing && !pnlContainer.ClientRectangle.Contains(PointToClient(Control.MousePosition)))
		{
			mouseLeave();
		}
	}

	private void pnlContainer_MouseMove(object sender, MouseEventArgs e)
	{
		bool flag = _no_controls && !Common.ShiftKeyDown;
		if (!_dragging)
		{
			bool flag2 = !flag && pnlBar.ClientRectangle.Contains(pnlBar.PointToClient(Control.MousePosition));
			if (flag2 && !pnlBar.Visible)
			{
				pnlBar.BringToFront();
				pnlBar.Show();
			}
			else if (!flag2 && pnlBar.Visible)
			{
				pnlBar.Hide();
			}
		}
		if (!_resizing)
		{
			bool flag3 = !flag && pbGrab.ClientRectangle.Contains(pbGrab.PointToClient(Control.MousePosition));
			if (flag3 && !pbGrab.Visible)
			{
				pbGrab.BringToFront();
				pbGrab.Show();
			}
			else if (!flag3 && pbGrab.Visible)
			{
				pbGrab.Hide();
			}
		}
	}

	private void pnlContainer_MouseLeave(object sender, EventArgs e)
	{
		if (!_dragging && !_resizing && !pnlContainer.ClientRectangle.Contains(pnlContainer.PointToClient(Control.MousePosition)))
		{
			mouseLeave();
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
		this.pnlBar = new System.Windows.Forms.Panel();
		this.btnSettings = new System.Windows.Forms.ButtonTS();
		this.btnPin = new System.Windows.Forms.ButtonTS();
		this.btnAxis = new System.Windows.Forms.ButtonTS();
		this.lblRX = new System.Windows.Forms.LabelTS();
		this.btnFloat = new System.Windows.Forms.ButtonTS();
		this.pbGrab = new System.Windows.Forms.PictureBox();
		this.pnlContainer = new System.Windows.Forms.Panel();
		this.pnlBar.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.pbGrab).BeginInit();
		base.SuspendLayout();
		this.pnlBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.pnlBar.BackColor = System.Drawing.Color.DimGray;
		this.pnlBar.Controls.Add(this.btnSettings);
		this.pnlBar.Controls.Add(this.btnPin);
		this.pnlBar.Controls.Add(this.btnAxis);
		this.pnlBar.Controls.Add(this.lblRX);
		this.pnlBar.Controls.Add(this.btnFloat);
		this.pnlBar.Location = new System.Drawing.Point(0, 0);
		this.pnlBar.Margin = new System.Windows.Forms.Padding(0);
		this.pnlBar.Name = "pnlBar";
		this.pnlBar.Size = new System.Drawing.Size(400, 18);
		this.pnlBar.TabIndex = 0;
		this.pnlBar.MouseDown += new System.Windows.Forms.MouseEventHandler(pnlBar_MouseDown);
		this.pnlBar.MouseLeave += new System.EventHandler(pnlBar_MouseLeave);
		this.pnlBar.MouseMove += new System.Windows.Forms.MouseEventHandler(pnlBar_MouseMove);
		this.pnlBar.MouseUp += new System.Windows.Forms.MouseEventHandler(pnlBar_MouseUp);
		this.btnSettings.BackColor = System.Drawing.Color.Transparent;
		this.btnSettings.BackgroundImage = Thetis.Properties.Resources.gear;
		this.btnSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.btnSettings.FlatAppearance.BorderColor = System.Drawing.Color.Black;
		this.btnSettings.FlatAppearance.BorderSize = 0;
		this.btnSettings.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
		this.btnSettings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
		this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.btnSettings.Image = null;
		this.btnSettings.Location = new System.Drawing.Point(3, 0);
		this.btnSettings.Margin = new System.Windows.Forms.Padding(0);
		this.btnSettings.Name = "btnSettings";
		this.btnSettings.Selectable = false;
		this.btnSettings.Size = new System.Drawing.Size(18, 18);
		this.btnSettings.TabIndex = 4;
		this.btnSettings.UseVisualStyleBackColor = false;
		this.btnSettings.Click += new System.EventHandler(btnSettings_Click);
		this.btnSettings.MouseLeave += new System.EventHandler(btnSettings_MouseLeave);
		this.btnPin.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnPin.BackColor = System.Drawing.Color.Transparent;
		this.btnPin.BackgroundImage = Thetis.Properties.Resources.pin_not_on_top;
		this.btnPin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.btnPin.FlatAppearance.BorderColor = System.Drawing.Color.Black;
		this.btnPin.FlatAppearance.BorderSize = 0;
		this.btnPin.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
		this.btnPin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
		this.btnPin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnPin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.btnPin.Image = null;
		this.btnPin.Location = new System.Drawing.Point(334, 0);
		this.btnPin.Margin = new System.Windows.Forms.Padding(0);
		this.btnPin.Name = "btnPin";
		this.btnPin.Selectable = false;
		this.btnPin.Size = new System.Drawing.Size(18, 18);
		this.btnPin.TabIndex = 3;
		this.btnPin.UseVisualStyleBackColor = false;
		this.btnPin.Click += new System.EventHandler(btnPin_Click);
		this.btnPin.MouseLeave += new System.EventHandler(btnPin_MouseLeave);
		this.btnAxis.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnAxis.BackColor = System.Drawing.Color.Transparent;
		this.btnAxis.BackgroundImage = Thetis.Properties.Resources.dot;
		this.btnAxis.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.btnAxis.FlatAppearance.BorderColor = System.Drawing.Color.Black;
		this.btnAxis.FlatAppearance.BorderSize = 0;
		this.btnAxis.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
		this.btnAxis.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
		this.btnAxis.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnAxis.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.btnAxis.Image = null;
		this.btnAxis.Location = new System.Drawing.Point(358, 0);
		this.btnAxis.Margin = new System.Windows.Forms.Padding(0);
		this.btnAxis.Name = "btnAxis";
		this.btnAxis.Selectable = false;
		this.btnAxis.Size = new System.Drawing.Size(18, 18);
		this.btnAxis.TabIndex = 2;
		this.btnAxis.UseVisualStyleBackColor = false;
		this.btnAxis.Click += new System.EventHandler(btnAxis_Click);
		this.btnAxis.MouseLeave += new System.EventHandler(btnAxis_MouseLeave);
		this.btnAxis.MouseUp += new System.Windows.Forms.MouseEventHandler(btnAxis_MouseUp);
		this.lblRX.AutoSize = true;
		this.lblRX.BackColor = System.Drawing.Color.DimGray;
		this.lblRX.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.lblRX.ForeColor = System.Drawing.Color.White;
		this.lblRX.Image = null;
		this.lblRX.Location = new System.Drawing.Point(24, 3);
		this.lblRX.Name = "lblRX";
		this.lblRX.Size = new System.Drawing.Size(31, 13);
		this.lblRX.TabIndex = 1;
		this.lblRX.Text = "RX0";
		this.lblRX.MouseDown += new System.Windows.Forms.MouseEventHandler(lblRX_MouseDown);
		this.lblRX.MouseLeave += new System.EventHandler(lblRX_MouseLeave);
		this.lblRX.MouseMove += new System.Windows.Forms.MouseEventHandler(lblRX_MouseMove);
		this.lblRX.MouseUp += new System.Windows.Forms.MouseEventHandler(lblRX_MouseUp);
		this.btnFloat.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnFloat.BackColor = System.Drawing.Color.Transparent;
		this.btnFloat.BackgroundImage = Thetis.Properties.Resources.dockIcon_dock;
		this.btnFloat.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.btnFloat.FlatAppearance.BorderColor = System.Drawing.Color.Black;
		this.btnFloat.FlatAppearance.BorderSize = 0;
		this.btnFloat.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
		this.btnFloat.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
		this.btnFloat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnFloat.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.btnFloat.Image = null;
		this.btnFloat.Location = new System.Drawing.Point(380, 0);
		this.btnFloat.Margin = new System.Windows.Forms.Padding(0);
		this.btnFloat.Name = "btnFloat";
		this.btnFloat.Selectable = false;
		this.btnFloat.Size = new System.Drawing.Size(18, 18);
		this.btnFloat.TabIndex = 0;
		this.btnFloat.UseVisualStyleBackColor = false;
		this.btnFloat.Click += new System.EventHandler(btnFloat_Click);
		this.btnFloat.MouseLeave += new System.EventHandler(btnFloat_MouseLeave);
		this.pbGrab.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.pbGrab.BackColor = System.Drawing.Color.Transparent;
		this.pbGrab.Image = Thetis.Properties.Resources.resizegrab;
		this.pbGrab.Location = new System.Drawing.Point(384, 184);
		this.pbGrab.Name = "pbGrab";
		this.pbGrab.Size = new System.Drawing.Size(16, 16);
		this.pbGrab.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
		this.pbGrab.TabIndex = 1;
		this.pbGrab.TabStop = false;
		this.pbGrab.MouseDown += new System.Windows.Forms.MouseEventHandler(pbGrab_MouseDown);
		this.pbGrab.MouseEnter += new System.EventHandler(pbGrab_MouseEnter);
		this.pbGrab.MouseLeave += new System.EventHandler(pbGrab_MouseLeave);
		this.pbGrab.MouseMove += new System.Windows.Forms.MouseEventHandler(pbGrab_MouseMove);
		this.pbGrab.MouseUp += new System.Windows.Forms.MouseEventHandler(pbGrab_MouseUp);
		this.pnlContainer.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.pnlContainer.Location = new System.Drawing.Point(102, 75);
		this.pnlContainer.Name = "pnlContainer";
		this.pnlContainer.Size = new System.Drawing.Size(200, 67);
		this.pnlContainer.TabIndex = 2;
		this.pnlContainer.MouseLeave += new System.EventHandler(pnlContainer_MouseLeave);
		this.pnlContainer.MouseMove += new System.Windows.Forms.MouseEventHandler(pnlContainer_MouseMove);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Black;
		base.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		base.Controls.Add(this.pnlContainer);
		base.Controls.Add(this.pbGrab);
		base.Controls.Add(this.pnlBar);
		base.Name = "ucMeter";
		base.Size = new System.Drawing.Size(400, 200);
		base.LocationChanged += new System.EventHandler(ucMeter_LocationChanged);
		base.SizeChanged += new System.EventHandler(ucMeter_SizeChanged);
		base.MouseLeave += new System.EventHandler(ucMeter_MouseLeave);
		this.pnlBar.ResumeLayout(false);
		this.pnlBar.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.pbGrab).EndInit();
		base.ResumeLayout(false);
	}
}
