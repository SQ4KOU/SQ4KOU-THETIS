using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Thetis;

public class frmMeterDisplay : Form
{
	private Console _console;

	private int _rx;

	private string _id;

	private bool _container_minimises = true;

	private bool _container_hides_when_rx_not_used = true;

	private bool _is_enabled = true;

	private bool _floating;

	private bool _rx2_enabled;

	private bool _hidden_by_macro;

	private const uint SWP_NOSIZE = 1u;

	private const uint SWP_NOZORDER = 4u;

	private IContainer components;

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

	public int RX
	{
		get
		{
			return _rx;
		}
		set
		{
			_rx = value;
		}
	}

	public bool Floating
	{
		get
		{
			return _floating;
		}
		set
		{
			_floating = value;
		}
	}

	public bool FormEnabled
	{
		get
		{
			return _is_enabled;
		}
		set
		{
			_is_enabled = value;
		}
	}

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

	public string ID
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
			Common.RestoreForm(this, "MeterDisplay_" + _id, restore_size: true);
			Common.ForceFormOnScreen(this);
			setTitle();
		}
	}

	public frmMeterDisplay(Console c, int rx)
	{
		InitializeComponent();
		MinimumSize = new Size(24, 24);
		_id = Guid.NewGuid().ToString();
		_console = c;
		_rx = rx;
		_rx2_enabled = _console.RX2Enabled;
		_hidden_by_macro = false;
		Common.DoubleBufferAll(this, enabled: true);
		Console console = _console;
		console.WindowStateChangedHandlers = (Console.WindowStateChanged)Delegate.Combine(console.WindowStateChangedHandlers, new Console.WindowStateChanged(OnWindowStateChanged));
		Console console2 = _console;
		console2.RX2EnabledChangedHandlers = (Console.RX2EnabledChanged)Delegate.Combine(console2.RX2EnabledChangedHandlers, new Console.RX2EnabledChanged(OnRX2Enabled));
		setTitle();
	}

	private void OnRX2Enabled(bool enabled)
	{
		_rx2_enabled = enabled;
	}

	private void OnWindowStateChanged(FormWindowState state)
	{
		if (base.Disposing || base.IsDisposed || !_is_enabled || !_floating || !_container_minimises)
		{
			return;
		}
		if (state == FormWindowState.Minimized)
		{
			Hide();
			return;
		}
		bool flag = false;
		switch (_rx)
		{
		case 1:
			flag = !_hidden_by_macro;
			break;
		case 2:
			if (_rx2_enabled || !_container_hides_when_rx_not_used)
			{
				flag = !_hidden_by_macro;
			}
			break;
		}
		if (flag)
		{
			Show();
		}
	}

	private void setTitle()
	{
		Text = "Thetis Meter [" + Common.FiveDigitHash(_id).ToString("00000") + "]";
	}

	private void frmMeterDisplay_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason == CloseReason.UserClosing)
		{
			Hide();
			e.Cancel = true;
		}
		Common.SaveForm(this, "MeterDisplay_" + _id);
	}

	public void TakeOwner(ucMeter m)
	{
		_container_minimises = m.ContainerMinimises;
		_is_enabled = m.MeterEnabled;
		m.Parent = this;
		m.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		m.Location = new Point(0, 0);
		m.Size = new Size(base.Width, base.Height);
		m.BringToFront();
		m.Show();
	}

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		IntPtr handle = base.Handle;
		Rectangle bounds = base.Bounds;
		int num = bounds.X;
		int num2 = bounds.Y;
		SetWindowPos(handle, IntPtr.Zero, num + 1, num2, 0, 0, 5u);
		SetWindowPos(handle, IntPtr.Zero, num, num2, 0, 0, 5u);
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
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(400, 200);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(100, 32);
		base.Name = "frmMeterDisplay";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		this.Text = "RX";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmMeterDisplay_FormClosing);
		base.ResumeLayout(false);
	}
}
