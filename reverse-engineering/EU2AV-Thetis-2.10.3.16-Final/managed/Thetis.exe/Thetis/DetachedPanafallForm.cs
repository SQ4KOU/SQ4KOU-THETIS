using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Thetis;

public class DetachedPanafallForm : Form
{
	private enum DetachedAimMode
	{
		Off,
		VFOA,
		VFOB
	}

	private DetachedAimMode _aimMode;

	private DetachedPanafallRenderer _renderer;

	private bool _teardownRequested;

	private bool _initialised;

	private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

	private bool _draggingSplit;

	private bool _draggingDbPan;

	private bool _draggingDbZoom;

	private Point _dbDragStart;

	private float _dbDragStartMax;

	private float _dbDragStartMin;

	private bool _draggingSpectrum;

	private int _spectrumDragLastX;

	private bool _spectrumMouseDown;

	private int _spectrumMouseDownX;

	private double _dragStartCentre;

	private bool _draggingFilter;

	private int _filterDragLastX;

	private int _filterDragMode;

	private bool _hiDpiSurfaceOn;

	private IContainer components;

	private Panel pnlRender;

	private Label lblStatus;

	private Button btnPin;

	private bool _cursorAiming => Display.CurrentClickTuneMode != ClickTuneMode.Off;

	internal DetachedPanafallRenderer Renderer => _renderer;

	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);

	private void ApplyDarkTitleBar()
	{
		try
		{
			if (base.IsHandleCreated)
			{
				int pvAttribute = 1;
				DwmSetWindowAttribute(base.Handle, 20, ref pvAttribute, 4);
			}
		}
		catch
		{
		}
	}

	private void btnPin_Click(object sender, EventArgs e)
	{
		base.TopMost = !base.TopMost;
		btnPin.ForeColor = (base.TopMost ? Color.Gold : Color.LightGray);
	}

	public DetachedPanafallForm()
	{
		InitializeComponent();
		MaximumSize = new Size(8192, 8192);
		Common.DoubleBufferAll(this, enabled: false);
		try
		{
			base.Icon = Console.getConsole().Icon;
		}
		catch
		{
		}
	}

	private void DetachedPanafallForm_Load(object sender, EventArgs e)
	{
		try
		{
			ApplyDarkTitleBar();
			RestorePosition();
			InitRenderer();
		}
		catch (Exception ex)
		{
			GPUWaterfallLogger.Log("DetachedForm", "Load failed: " + ex);
			MessageBox.Show("Failed to initialise Detached Panafall:\n\n" + ex.Message, "Thetis Detached Panafall", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}
	}

	private void InitRenderer()
	{
		if (_initialised)
		{
			return;
		}
		if (!base.IsHandleCreated || !pnlRender.IsHandleCreated)
		{
			GPUWaterfallLogger.Log("DetachedForm", "InitRenderer deferred: handle not created yet");
			return;
		}
		int num = pnlRender.Width;
		int num2 = pnlRender.Height;
		if (num < 64 || num2 < 64)
		{
			GPUWaterfallLogger.Log("DetachedForm", "InitRenderer deferred: size too small " + num + "x" + num2);
			return;
		}
		if (Display.HiDpiPhysicalRender && !_hiDpiSurfaceOn)
		{
			SetHiDpiSurface(on: true);
		}
		try
		{
			lock (Display.DX2RenderLock)
			{
				_renderer = new DetachedPanafallRenderer();
				_renderer.Init(pnlRender.Handle, num, num2);
				Display.SetDetachedRenderer(_renderer);
			}
			_initialised = true;
			GPUWaterfallLogger.Log("DetachedForm", "InitRenderer done, size=" + num + "x" + num2);
		}
		catch (Exception ex)
		{
			GPUWaterfallLogger.Log("DetachedForm", "InitRenderer failed: " + ex);
			try
			{
				if (_renderer != null)
				{
					_renderer.Dispose();
				}
			}
			catch
			{
			}
			_renderer = null;
			_initialised = false;
		}
	}

	internal void SetHiDpiSurface(bool on)
	{
		if (!base.IsHandleCreated || !pnlRender.IsHandleCreated)
		{
			return;
		}
		if (on)
		{
			HiDpiSurfaces.Enable(pnlRender, delegate
			{
				Display.RequestDetachedResize(pnlRender.Width, pnlRender.Height);
			});
			_hiDpiSurfaceOn = true;
		}
		else
		{
			HiDpiSurfaces.Disable(pnlRender);
			_hiDpiSurfaceOn = false;
		}
		if (_renderer != null)
		{
			_renderer.UpdateHwnd(pnlRender.Handle);
		}
		Display.RequestDetachedResize(pnlRender.Width, pnlRender.Height);
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);
		if (!base.Visible || _initialised)
		{
			return;
		}
		BeginInvoke((MethodInvoker)delegate
		{
			if (base.Visible && !_initialised)
			{
				InitRenderer();
			}
		});
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		if (base.WindowState == FormWindowState.Minimized)
		{
			base.WindowState = FormWindowState.Normal;
		}
		if (_initialised)
		{
			return;
		}
		BeginInvoke((MethodInvoker)delegate
		{
			if (base.Visible && !_initialised)
			{
				InitRenderer();
			}
		});
	}

	private void pnlRender_Resize(object sender, EventArgs e)
	{
		if (!_initialised || _renderer == null || base.WindowState == FormWindowState.Minimized)
		{
			return;
		}
		int num = pnlRender.Width;
		int num2 = pnlRender.Height;
		if (num < 64 || num2 < 64)
		{
			return;
		}
		try
		{
			Display.RequestDetachedResize(num, num2);
		}
		catch (Exception ex)
		{
			GPUWaterfallLogger.Log("DetachedResize", "pnlRender_Resize exception: " + ex);
		}
	}

	private void DetachedPanafallForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		bool flag = e.CloseReason == CloseReason.ApplicationExitCall || e.CloseReason == CloseReason.FormOwnerClosing || e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing;
		SavePosition();
		Display.SetDetachedRenderer(null);
		_teardownRequested = true;
		_renderer = null;
		_initialised = false;
		Console.getConsole()?.DetachedPanafallClosing();
		if (flag)
		{
			GPUWaterfallLogger.Log("DetachedForm", "FormClosing appShuttingDown=" + flag + " reason=" + e.CloseReason.ToString() + " — allowing close");
		}
		else
		{
			e.Cancel = true;
			Hide();
		}
	}

	public void SavePosition()
	{
		List<string> list = new List<string>();
		if (base.WindowState != FormWindowState.Minimized)
		{
			list.Add("Top/" + base.Top);
			list.Add("Left/" + base.Left);
			list.Add("Width/" + base.Width);
			list.Add("Height/" + base.Height);
		}
		list.Add("WindowState/" + (int)base.WindowState);
		try
		{
			DB.SaveVars("DetachedPanafall", list);
		}
		catch
		{
		}
	}

	public void RestorePosition()
	{
		List<string> list = null;
		try
		{
			list = DB.GetVars("DetachedPanafall");
		}
		catch
		{
		}
		if (list == null || list.Count == 0)
		{
			return;
		}
		foreach (string item in list)
		{
			string[] array = item.Split('/');
			if (array.Length < 2)
			{
				continue;
			}
			string text = array[0];
			string s = array[1];
			switch (text)
			{
			case "Top":
			{
				base.StartPosition = FormStartPosition.Manual;
				if (int.TryParse(s, out var result5))
				{
					base.Top = result5;
				}
				break;
			}
			case "Left":
			{
				base.StartPosition = FormStartPosition.Manual;
				if (int.TryParse(s, out var result4))
				{
					base.Left = result4;
				}
				break;
			}
			case "Width":
			{
				if (int.TryParse(s, out var result2) && result2 >= 320)
				{
					base.Width = result2;
				}
				break;
			}
			case "Height":
			{
				if (int.TryParse(s, out var result3) && result3 >= 200)
				{
					base.Height = result3;
				}
				break;
			}
			case "WindowState":
			{
				if (int.TryParse(s, out var result))
				{
					base.WindowState = (FormWindowState)result;
				}
				break;
			}
			}
		}
	}

	private void pnlRender_MouseDown(object sender, MouseEventArgs e)
	{
		if (_renderer == null)
		{
			return;
		}
		if (_renderer.IsOverSplitBar(e.X, e.Y))
		{
			_draggingSplit = true;
		}
		else if (_renderer.IsOverDbScale(e.X, e.Y))
		{
			_dbDragStart = e.Location;
			_dbDragStartMax = Display.SpectrumGridMax;
			_dbDragStartMin = Display.SpectrumGridMin;
			if (e.Button == MouseButtons.Left)
			{
				_draggingDbPan = true;
			}
			else if (e.Button == MouseButtons.Right)
			{
				_draggingDbZoom = true;
			}
		}
		else if (e.Button == MouseButtons.Right && !_renderer.IsOverDbScale(e.X, e.Y) && !_renderer.IsOverSplitBar(e.X, e.Y))
		{
			Console console = Console.getConsole();
			if (console != null)
			{
				switch (Display.CurrentClickTuneMode)
				{
				case ClickTuneMode.Off:
					console.CurrentClickTuneMode = ClickTuneMode.VFOA;
					break;
				case ClickTuneMode.VFOA:
					console.CurrentClickTuneMode = ((console.chkVFOSplit.Checked || console.chkEnableMultiRX.Checked) ? ClickTuneMode.VFOB : ClickTuneMode.Off);
					break;
				case ClickTuneMode.VFOB:
					console.CurrentClickTuneMode = ClickTuneMode.Off;
					break;
				}
			}
			_renderer.SetCursor(e.X, e.Y, _cursorAiming);
		}
		else if (e.Button == MouseButtons.Left && !_renderer.IsOverDbScale(e.X, e.Y) && !_renderer.IsOverSplitBar(e.X, e.Y))
		{
			int num = FilterHitTest(e.X);
			bool flag = Console.getConsole()?.ClickTuneDisplay ?? false;
			if (num == 1 || num == 2)
			{
				_draggingFilter = true;
				_filterDragMode = num;
				_filterDragLastX = e.X;
			}
			else if (!flag && num == 3)
			{
				_draggingFilter = true;
				_filterDragMode = 3;
				_filterDragLastX = e.X;
			}
			else if (flag)
			{
				_draggingSpectrum = true;
				_spectrumDragLastX = e.X;
				_spectrumMouseDown = true;
				_spectrumMouseDownX = e.X;
			}
		}
	}

	private int FilterHitTest(int x)
	{
		int rXDisplayLow = Display.RXDisplayLow;
		int num = Display.RXDisplayHigh - rXDisplayLow;
		if (num <= 0 || pnlRender.Width <= 0)
		{
			return 0;
		}
		int freqDiff = Display.FreqDiff;
		float num2 = Display.RX1FilterLow;
		float num3 = Display.RX1FilterHigh;
		float val = (num2 - (float)rXDisplayLow - (float)freqDiff) / (float)num * (float)pnlRender.Width;
		float val2 = (num3 - (float)rXDisplayLow - (float)freqDiff) / (float)num * (float)pnlRender.Width;
		float num4 = Math.Min(val, val2);
		float num5 = Math.Max(val, val2);
		float num6 = 10f;
		if ((float)x < num4 - num6 || (float)x > num5 + num6)
		{
			return 0;
		}
		if ((float)x < num4 + num6)
		{
			return 1;
		}
		if ((float)x > num5 - num6)
		{
			return 2;
		}
		return 3;
	}

	private bool IsOverFilterBand(int x)
	{
		return FilterHitTest(x) != 0;
	}

	private void DragFilter(int deltaX, int mode)
	{
		int rXDisplayLow = Display.RXDisplayLow;
		int rXDisplayHigh = Display.RXDisplayHigh;
		int num = rXDisplayHigh - rXDisplayLow;
		if (num <= 0 || pnlRender.Width <= 0)
		{
			return;
		}
		double num2 = (double)num / (double)pnlRender.Width;
		int num3 = (int)Math.Round((double)deltaX * num2);
		int rX1FilterLow = Display.RX1FilterLow;
		int rX1FilterHigh = Display.RX1FilterHigh;
		int freqDiff = Display.FreqDiff;
		int num4 = Math.Min(rX1FilterLow, rX1FilterHigh);
		int num5 = Math.Max(rX1FilterLow, rX1FilterHigh);
		int num6 = 10;
		switch (mode)
		{
		case 1:
			num4 += num3;
			if (num4 < rXDisplayLow + freqDiff)
			{
				num4 = rXDisplayLow + freqDiff;
			}
			if (num4 > num5 - num6)
			{
				num4 = num5 - num6;
			}
			if (rX1FilterLow <= rX1FilterHigh)
			{
				Display.RX1FilterLow = num4;
			}
			else
			{
				Display.RX1FilterHigh = num4;
			}
			return;
		case 2:
			num5 += num3;
			if (num5 > rXDisplayHigh + freqDiff)
			{
				num5 = rXDisplayHigh + freqDiff;
			}
			if (num5 < num4 + num6)
			{
				num5 = num4 + num6;
			}
			if (rX1FilterLow <= rX1FilterHigh)
			{
				Display.RX1FilterHigh = num5;
			}
			else
			{
				Display.RX1FilterLow = num5;
			}
			return;
		}
		int num7 = rX1FilterLow + num3;
		int num8 = rX1FilterHigh + num3;
		if (num7 < rXDisplayLow)
		{
			num8 += rXDisplayLow - num7;
			num7 = rXDisplayLow;
		}
		if (num8 > rXDisplayHigh)
		{
			num7 -= num8 - rXDisplayHigh;
			num8 = rXDisplayHigh;
		}
		Display.RX1FilterLow = num7;
		Display.RX1FilterHigh = num8;
	}

	private void pnlRender_MouseMove(object sender, MouseEventArgs e)
	{
		if (_renderer == null)
		{
			return;
		}
		if (_draggingSplit)
		{
			float splitPerc = (float)e.Y / (float)pnlRender.Height;
			_renderer.SetSplitPerc(splitPerc);
			return;
		}
		if (_draggingDbPan)
		{
			double num = (double)(e.Y - _dbDragStart.Y) / 10.0 * (double)Display.SpectrumGridStep;
			float num2 = _dbDragStartMax + (float)num;
			float num3 = _dbDragStartMin + (float)num;
			if (num3 < -200f)
			{
				num3 = -200f;
				if (num2 - num3 < 24f)
				{
					num2 = num3 + 24f;
				}
			}
			if (num2 > 200f)
			{
				num2 = 200f;
				if (num2 - num3 < 24f)
				{
					num3 = num2 - 24f;
				}
			}
			ApplyGridMaxMin(num2, num3);
			return;
		}
		if (_draggingDbZoom)
		{
			double num4 = (double)(e.Y - _dbDragStart.Y) / 10.0 * (double)Display.SpectrumGridStep;
			float num5 = _dbDragStartMax + (float)num4;
			float num6 = Display.SpectrumGridMin;
			if (num5 - num6 < 24f)
			{
				num5 = num6 + 24f;
			}
			ApplyGridMaxMin(num5, num6);
			return;
		}
		if (_draggingFilter)
		{
			DragFilter(e.X - _filterDragLastX, _filterDragMode);
			_filterDragLastX = e.X;
			return;
		}
		if (_draggingSpectrum)
		{
			Console console = Console.getConsole();
			if (console != null && !console.MOX)
			{
				int rXDisplayLow = Display.RXDisplayLow;
				int num7 = Display.RXDisplayHigh - rXDisplayLow;
				if (num7 > 0 && pnlRender.Width > 0)
				{
					double num8 = (double)num7 / (double)pnlRender.Width;
					double num9 = (double)_spectrumDragLastX * num8 + (double)rXDisplayLow;
					double num10 = (double)e.X * num8 + (double)rXDisplayLow;
					double num11 = num9 - num10;
					_spectrumDragLastX = e.X;
					console.VFOAFreq -= num11 * 1E-06;
				}
			}
			_renderer.SetCursor(e.X, e.Y, _cursorAiming);
			return;
		}
		_renderer.SetCursor(e.X, e.Y, _cursorAiming);
		_renderer.SetAimTarget(Display.CurrentClickTuneMode == ClickTuneMode.VFOB);
		try
		{
			int rXDisplayLow2 = Display.RXDisplayLow;
			int num12 = Display.RXDisplayHigh - rXDisplayLow2;
			if (num12 > 0 && pnlRender.Width > 0)
			{
				Display.MouseFrequency = (double)rXDisplayLow2 + (double)e.X / (double)pnlRender.Width * (double)num12;
			}
		}
		catch
		{
		}
		UpdateHoverCursor(e.X, e.Y);
	}

	private void pnlRender_MouseUp(object sender, MouseEventArgs e)
	{
		_draggingSplit = false;
		_draggingDbPan = false;
		_draggingDbZoom = false;
		_draggingSpectrum = false;
		_draggingFilter = false;
		_spectrumMouseDown = false;
		_renderer?.SetCursor(e.X, e.Y, _cursorAiming);
		UpdateHoverCursor(e.X, e.Y);
	}

	private void pnlRender_MouseClick(object sender, MouseEventArgs e)
	{
		if (_renderer == null || e.Button != MouseButtons.Left || _renderer.IsOverDbScale(e.X, e.Y) || _renderer.IsOverSplitBar(e.X, e.Y) || IsOverFilterBand(e.X))
		{
			return;
		}
		Console console = Console.getConsole();
		if (console == null || console.MOX)
		{
			return;
		}
		bool clickTuneDisplay = console.ClickTuneDisplay;
		if (!_cursorAiming && !clickTuneDisplay)
		{
			return;
		}
		int rXDisplayLow = Display.RXDisplayLow;
		int num = Display.RXDisplayHigh - rXDisplayLow;
		if (num > 0 && pnlRender.Width > 0)
		{
			double num2 = (double)rXDisplayLow + (double)e.X / (double)pnlRender.Width * (double)num;
			double value = (console.ClickTuneDisplay ? console.CentreFrequency : console.VFOAFreq) + num2 * 1E-06;
			if (Display.CurrentClickTuneMode == ClickTuneMode.VFOB && Display.SubRX1Enabled)
			{
				Display.VFOASub = (long)(Math.Round(value, 6) * 1000000.0);
			}
			else
			{
				console.VFOAFreq = Math.Round(value, 6);
			}
		}
	}

	private void pnlRender_MouseLeave(object sender, EventArgs e)
	{
		_draggingSplit = false;
		_draggingDbPan = false;
		_draggingDbZoom = false;
		_draggingSpectrum = false;
		_draggingFilter = false;
		_spectrumMouseDown = false;
		_renderer?.SetCursor(-1f, -1f, aiming: false);
		pnlRender.Cursor = Cursors.Default;
	}

	private void pnlRender_MouseWheel(object sender, MouseEventArgs e)
	{
		if (_renderer == null || e.Delta == 0 || _renderer.IsOverDbScale(e.X, e.Y) || _renderer.IsOverSplitBar(e.X, e.Y))
		{
			return;
		}
		Console console = Console.getConsole();
		if (console != null && !console.MOX)
		{
			int num = console.CurrentTuneStepHz;
			if (num <= 0)
			{
				num = 10;
			}
			if (Common.ShiftKeyDown && num >= 10)
			{
				num /= 10;
			}
			int num_steps = ((!console.WheelReverse) ? ((e.Delta > 0) ? 1 : (-1)) : ((e.Delta <= 0) ? 1 : (-1)));
			console.VFOAFreq = console.SnapTune(console.VFOAFreq, num, num_steps);
		}
	}

	private void ApplyGridMaxMin(float max, float min)
	{
		Display.SpectrumGridMax = (int)Math.Round(max);
		Display.SpectrumGridMin = (int)Math.Round(min);
		Console console = Console.getConsole();
		if (console != null && !console.IsSetupFormNull)
		{
			try
			{
				console.SetupForm.DisplayGridMax = max;
				console.SetupForm.DisplayGridMin = min;
			}
			catch
			{
			}
		}
	}

	private void UpdateHoverCursor(int x, int y)
	{
		if (_renderer == null)
		{
			return;
		}
		if (_draggingSplit || _renderer.IsOverSplitBar(x, y))
		{
			pnlRender.Cursor = Cursors.SizeNS;
			return;
		}
		if (_renderer.IsOverDbScale(x, y))
		{
			pnlRender.Cursor = ((_draggingDbPan || _draggingDbZoom) ? Cursors.SizeNS : Cursors.Hand);
			return;
		}
		bool flag = Console.getConsole()?.ClickTuneDisplay ?? false;
		int num = FilterHitTest(x);
		if (num == 1 || num == 2)
		{
			pnlRender.Cursor = Cursors.SizeWE;
		}
		else if (_draggingSpectrum)
		{
			pnlRender.Cursor = Cursors.SizeAll;
		}
		else if (flag)
		{
			pnlRender.Cursor = Cursors.SizeAll;
		}
		else if (num == 3)
		{
			pnlRender.Cursor = Cursors.SizeWE;
		}
		else
		{
			pnlRender.Cursor = Cursors.Cross;
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
		this.pnlRender = new System.Windows.Forms.Panel();
		this.lblStatus = new System.Windows.Forms.Label();
		this.btnPin = new System.Windows.Forms.Button();
		this.pnlRender.SuspendLayout();
		base.SuspendLayout();
		this.pnlRender.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.pnlRender.BackColor = System.Drawing.Color.Black;
		this.pnlRender.Location = new System.Drawing.Point(0, 0);
		this.pnlRender.Name = "pnlRender";
		this.pnlRender.Size = new System.Drawing.Size(800, 450);
		this.pnlRender.TabIndex = 0;
		this.pnlRender.Resize += new System.EventHandler(pnlRender_Resize);
		this.pnlRender.MouseDown += new System.Windows.Forms.MouseEventHandler(pnlRender_MouseDown);
		this.pnlRender.MouseMove += new System.Windows.Forms.MouseEventHandler(pnlRender_MouseMove);
		this.pnlRender.MouseUp += new System.Windows.Forms.MouseEventHandler(pnlRender_MouseUp);
		this.pnlRender.MouseClick += new System.Windows.Forms.MouseEventHandler(pnlRender_MouseClick);
		this.pnlRender.MouseLeave += new System.EventHandler(pnlRender_MouseLeave);
		this.pnlRender.MouseWheel += new System.Windows.Forms.MouseEventHandler(pnlRender_MouseWheel);
		this.lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.lblStatus.AutoSize = true;
		this.lblStatus.BackColor = System.Drawing.Color.Transparent;
		this.lblStatus.ForeColor = System.Drawing.Color.DimGray;
		this.lblStatus.Location = new System.Drawing.Point(700, 8);
		this.lblStatus.Name = "lblStatus";
		this.lblStatus.Size = new System.Drawing.Size(68, 13);
		this.lblStatus.TabIndex = 1;
		this.lblStatus.Text = "RX1 mirror";
		this.btnPin.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnPin.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
		this.btnPin.FlatAppearance.BorderSize = 0;
		this.btnPin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnPin.Font = new System.Drawing.Font("Segoe UI", 10f);
		this.btnPin.ForeColor = System.Drawing.Color.LightGray;
		this.btnPin.Location = new System.Drawing.Point(760, 4);
		this.btnPin.Name = "btnPin";
		this.btnPin.Size = new System.Drawing.Size(32, 24);
		this.btnPin.TabIndex = 2;
		this.btnPin.Text = "\ud83d\udccc";
		this.btnPin.UseVisualStyleBackColor = false;
		this.btnPin.Click += new System.EventHandler(btnPin_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(800, 450);
		base.Controls.Add(this.btnPin);
		base.Controls.Add(this.lblStatus);
		base.Controls.Add(this.pnlRender);
		this.MinimumSize = new System.Drawing.Size(320, 200);
		base.Name = "DetachedPanafallForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation;
		this.Text = "Detached Panafall";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(DetachedPanafallForm_FormClosing);
		base.Load += new System.EventHandler(DetachedPanafallForm_Load);
		this.pnlRender.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
