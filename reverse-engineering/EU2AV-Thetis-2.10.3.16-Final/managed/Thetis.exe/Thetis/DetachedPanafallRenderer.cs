using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace Thetis;

internal sealed class DetachedPanafallRenderer : IDisposable
{
	private readonly SharpDX.Direct3D11.Device _device;

	private readonly SharpDX.Direct2D1.Device _d2dDevice;

	private readonly SharpDX.Direct2D1.Factory1 _d2dFactory;

	private readonly SharpDX.DXGI.Factory1 _dxgiFactory;

	private SwapChain _swapChain;

	private SwapChain1 _swapChain1;

	private int _bufferCount = 2;

	private Surface _surface;

	private SharpDX.Direct2D1.DeviceContext _ctx;

	private Bitmap1 _targetBitmap;

	private SharpDX.Direct2D1.Bitmap _backgroundBitmap;

	private WaterfallGPURenderer _wfGPU;

	private int _width;

	private int _height;

	private int _panadapterHeight;

	private int _waterfallHeight;

	private float _splitPerc = 0.5f;

	private bool _disposed;

	private int _uploadedPaletteVersion = -1;

	private ColorScheme _uploadedScheme = (ColorScheme)(-1);

	private volatile bool _resizePending;

	private int _pendingWidth;

	private int _pendingHeight;

	private volatile bool _splitResizePending;

	private SolidColorBrush _gridBrush;

	private SolidColorBrush _spectrumBrush;

	private SolidColorBrush _filterBrush;

	private SolidColorBrush _textBrush;

	private SolidColorBrush _bgFillBrush;

	private SolidColorBrush _splitBrush;

	private PathGeometry _spectrumPath;

	private TextFormat _dbTextFormat;

	private long _lastHeartbeatMs;

	private bool _hwndChanged;

	private int _wfGPUPipeWidth;

	private long _lastSrgbWatchdogMs;

	private readonly Dictionary<string, float> _textWidthCache = new Dictionary<string, float>();

	private SharpDX.DirectWrite.Factory _dwFactory;

	private bool _needsRecreate;

	private long _renderFrameCount;

	private Result _lastPresentResult = Result.Ok;

	private IntPtr _hwnd = IntPtr.Zero;

	private bool _flipModel;

	private bool _allowTearing;

	private int _stillDrawingStreak;

	private bool _cursorTracking;

	private bool _cursorAiming;

	private float _cursorX = -1f;

	private float _cursorY = -1f;

	private float _cursorScale = 1f;

	private int _logicalW = 64;

	private int _logicalH = 64;

	private bool _aimTargetSubRX;

	private bool _wfGPUResizePending;

	private long _lastWfGPUResizeRequestMs;

	internal int SplitBarY => _panadapterHeight;

	public bool IsInitialized
	{
		get
		{
			if (_ctx != null)
			{
				return !_disposed;
			}
			return false;
		}
	}

	internal string DebugSize => _width + "x" + _height + " (panadapter=" + _panadapterHeight + " waterfall=" + _waterfallHeight + ")";

	private int WaterfallPipeWidth => Math.Max(64, Display.SharedDisplayTargetWidth);

	public DetachedPanafallRenderer()
	{
		_device = Display.SharedD3DDevice;
		_d2dDevice = Display.SharedD2DDevice;
		_d2dFactory = Display.SharedD2DFactory;
		_dxgiFactory = Display.SharedDXGIFactory;
	}

	public void UpdateHwnd(IntPtr hwnd)
	{
		if (!(hwnd == IntPtr.Zero) && !(hwnd == _hwnd))
		{
			GPUWaterfallLogger.Log("DetachedForm", "UpdateHwnd: 0x" + _hwnd.ToInt64().ToString("X") + " -> 0x" + hwnd.ToInt64().ToString("X"));
			_hwnd = hwnd;
			_hwndChanged = true;
		}
	}

	public void Init(IntPtr hwnd, int width, int height)
	{
		if (_device == null || _d2dDevice == null || _d2dFactory == null || _dxgiFactory == null)
		{
			throw new InvalidOperationException("DetachedPanafallRenderer: shared DX resources not initialised.");
		}
		_hwnd = hwnd;
		_logicalW = Math.Max(64, width);
		_logicalH = Math.Max(64, height);
		int width2 = _logicalW;
		int height2 = _logicalH;
		if (Display.HiDpiPhysicalRender)
		{
			Size physicalClientSize = HiDpiSurfaces.GetPhysicalClientSize(hwnd);
			if (physicalClientSize.Width >= 64 && physicalClientSize.Height >= 64)
			{
				width2 = physicalClientSize.Width;
				height2 = physicalClientSize.Height;
			}
		}
		_width = width2;
		_height = height2;
		_cursorScale = Math.Max(1f, (float)_width / (float)_logicalW);
		RecomputeSplit();
		Format format = WaterfallPixelWriter.DxgiFormat;
		Format format2 = ((format == Format.R16G16B16A16_Float) ? Format.B8G8R8A8_UNorm : format);
		SharpDX.DXGI.Factory4 comObject = _dxgiFactory.QueryInterfaceOrNull<SharpDX.DXGI.Factory4>();
		bool flag = comObject != null;
		Utilities.Dispose(ref comObject);
		SwapEffect swapEffect = (flag ? SwapEffect.FlipDiscard : SwapEffect.Discard);
		int bufferCount = ((!flag) ? 1 : 2);
		bool flag2 = false;
		SharpDX.DXGI.Factory5 comObject2 = _dxgiFactory.QueryInterfaceOrNull<SharpDX.DXGI.Factory5>();
		if (comObject2 != null)
		{
			try
			{
				int num = Marshal.SizeOf(typeof(int));
				IntPtr intPtr = Marshal.AllocHGlobal(num);
				try
				{
					comObject2.CheckFeatureSupport(SharpDX.DXGI.Feature.PresentAllowTearing, intPtr, num);
					flag2 = Marshal.ReadInt32(intPtr) == 1;
				}
				finally
				{
					Marshal.FreeHGlobal(intPtr);
				}
			}
			catch
			{
			}
			Utilities.Dispose(ref comObject2);
		}
		_allowTearing = flag2;
		int numerator = 60;
		try
		{
			numerator = Math.Max(1, Console.getConsole().DisplayFPS);
		}
		catch
		{
		}
		ModeDescription modeDescription = new ModeDescription(_width, _height, new Rational(numerator, 1), format2);
		modeDescription.ScanlineOrdering = DisplayModeScanlineOrder.Progressive;
		modeDescription.Scaling = DisplayModeScaling.Stretched;
		SwapChainDescription description = new SwapChainDescription
		{
			BufferCount = bufferCount,
			ModeDescription = modeDescription,
			IsWindowed = true,
			OutputHandle = hwnd,
			SampleDescription = new SampleDescription(1, 0),
			SwapEffect = swapEffect,
			Usage = Usage.RenderTargetOutput,
			Flags = (flag2 ? SwapChainFlags.AllowTearing : SwapChainFlags.None)
		};
		_dxgiFactory.MakeWindowAssociation(hwnd, WindowAssociationFlags.IgnoreAll);
		try
		{
			_swapChain = new SwapChain(_dxgiFactory, _device, description);
		}
		catch (Exception)
		{
			if (!flag)
			{
				throw;
			}
			swapEffect = SwapEffect.Discard;
			bufferCount = 1;
			description.BufferCount = 1;
			description.SwapEffect = SwapEffect.Discard;
			description.Flags = SwapChainFlags.None;
			_allowTearing = false;
			_swapChain = new SwapChain(_dxgiFactory, _device, description);
		}
		_bufferCount = bufferCount;
		_flipModel = swapEffect != SwapEffect.Discard;
		_swapChain1 = _swapChain.QueryInterface<SwapChain1>();
		ApplySRGBColorSpaceForFloatSwapChain(_swapChain1);
		if (format2 != format)
		{
			try
			{
				_swapChain1.ResizeBuffers(bufferCount, _width, _height, format, _allowTearing ? SwapChainFlags.AllowTearing : SwapChainFlags.None);
			}
			catch (Exception ex2)
			{
				GPUWaterfallLogger.Log("DetachedForm", "float ResizeBuffers failed, falling back to UNorm: " + ex2.Message);
				format = format2;
			}
		}
		ApplySRGBColorSpaceForFloatSwapChain(_swapChain1);
		_surface = _swapChain1.GetBackBuffer<Surface>(0);
		_ctx = new SharpDX.Direct2D1.DeviceContext(_d2dDevice, DeviceContextOptions.None);
		BitmapProperties1 bitmapProperties = new BitmapProperties1(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Ignore), 96f, 96f, BitmapOptions.Target | BitmapOptions.CannotDraw);
		_targetBitmap = new Bitmap1(_ctx, _surface, bitmapProperties);
		_ctx.Target = _targetBitmap;
		_ctx.DotsPerInch = new Size2F(96f, 96f);
		_ctx.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale;
		BuildBackgroundBitmap();
		CreateBrushes();
		_wfGPU = new WaterfallGPURenderer(width: _wfGPUPipeWidth = Math.Max(64, Display.SharedDisplayTargetWidth), device: _device, d2dDC: _ctx, height: _waterfallHeight, format: format);
	}

	private void ApplySRGBColorSpaceForFloatSwapChain(SwapChain1 sc1, bool quiet = false)
	{
		if (WaterfallPixelWriter.DxgiFormat != Format.R16G16B16A16_Float || sc1 == null)
		{
			return;
		}
		SwapChain3 comObject = sc1.QueryInterfaceOrNull<SwapChain3>();
		if (comObject == null)
		{
			if (!quiet)
			{
				GPUWaterfallLogger.Log("DetachedPresent", "ApplySRGB: SwapChain3 not available");
			}
			return;
		}
		try
		{
			SwapChainColorSpaceSupportFlags swapChainColorSpaceSupportFlags = comObject.CheckColorSpaceSupport(ColorSpaceType.RgbFullG22NoneP709);
			if ((swapChainColorSpaceSupportFlags & SwapChainColorSpaceSupportFlags.Present) != SwapChainColorSpaceSupportFlags.None)
			{
				comObject.ColorSpace1 = ColorSpaceType.RgbFullG22NoneP709;
				if (!quiet)
				{
					GPUWaterfallLogger.Log("DetachedPresent", "ApplySRGB: color space set to sRGB (G22)");
				}
			}
			else if (!quiet)
			{
				GPUWaterfallLogger.Log("DetachedPresent", "ApplySRGB: sRGB NOT supported, flags=" + swapChainColorSpaceSupportFlags.ToString() + " — background will look washed out");
			}
		}
		catch (Exception ex)
		{
			if (!quiet)
			{
				GPUWaterfallLogger.Log("DetachedPresent", "ApplySRGB failed: " + ex.Message);
			}
		}
		finally
		{
			Utilities.Dispose(ref comObject);
		}
	}

	private void BuildBackgroundBitmap()
	{
		Utilities.Dispose(ref _backgroundBitmap);
		_backgroundBitmap = null;
		System.Drawing.Image sharedBackgroundImageSource = Display.SharedBackgroundImageSource;
		if (sharedBackgroundImageSource == null)
		{
			return;
		}
		try
		{
			if (sharedBackgroundImageSource.Width <= 0 || sharedBackgroundImageSource.Height <= 0)
			{
				return;
			}
			using System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(sharedBackgroundImageSource);
			if (bitmap.Width > 0 && bitmap.Height > 0)
			{
				_backgroundBitmap = Display.SDXBitmapFromSysBitmapPublic(_ctx, bitmap);
			}
		}
		catch
		{
		}
	}

	private void CreateBrushes()
	{
		Utilities.Dispose(ref _gridBrush);
		Utilities.Dispose(ref _spectrumBrush);
		Utilities.Dispose(ref _filterBrush);
		Utilities.Dispose(ref _textBrush);
		Utilities.Dispose(ref _bgFillBrush);
		Utilities.Dispose(ref _spectrumPath);
		Utilities.Dispose(ref _dbTextFormat);
		_gridBrush = new SolidColorBrush(_ctx, new RawColor4(0.25f, 0.25f, 0.25f, 1f));
		System.Drawing.Color c = (Display.MOX ? Display.TXDataLineColor : Display.DataLineColor);
		_spectrumBrush = new SolidColorBrush(_ctx, ToRawColor(c));
		_filterBrush = new SolidColorBrush(_ctx, new RawColor4(0.85f, 0.55f, 0.15f, 0.45f));
		_textBrush = new SolidColorBrush(_ctx, new RawColor4(0.8f, 0.8f, 0.8f, 1f));
		_bgFillBrush = new SolidColorBrush(_ctx, new RawColor4(0.04f, 0.04f, 0.04f, 1f));
		_splitBrush = new SolidColorBrush(_ctx, new RawColor4(0.7f, 0.7f, 0.7f, 0.6f));
		Utilities.Dispose(ref _dbTextFormat);
		if (_dwFactory == null)
		{
			_dwFactory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);
		}
		_dbTextFormat = new TextFormat(_dwFactory, "Consolas", 11f * _cursorScale)
		{
			TextAlignment = TextAlignment.Trailing,
			ParagraphAlignment = ParagraphAlignment.Center,
			WordWrapping = WordWrapping.NoWrap
		};
		_textWidthCache.Clear();
	}

	private float MeasureTextWidth(string s)
	{
		if (_textWidthCache.TryGetValue(s, out var value))
		{
			return value;
		}
		try
		{
			using TextLayout textLayout = new TextLayout(_dwFactory, s, _dbTextFormat, 100000f, 100000f);
			value = textLayout.Metrics.Width;
		}
		catch
		{
			value = (float)s.Length * 5.5f * _cursorScale;
		}
		if (_textWidthCache.Count > 512)
		{
			_textWidthCache.Clear();
		}
		_textWidthCache[s] = value;
		return value;
	}

	private void RecomputeSplit()
	{
		_panadapterHeight = Math.Max(40, (int)((float)_height * _splitPerc));
		_waterfallHeight = Math.Max(40, _height - _panadapterHeight);
	}

	internal void SetSplitPerc(float perc)
	{
		if (perc < 0.1f)
		{
			perc = 0.1f;
		}
		if (perc > 0.9f)
		{
			perc = 0.9f;
		}
		if (!(Math.Abs(perc - _splitPerc) < 0.001f))
		{
			_splitPerc = perc;
			int waterfallHeight = _waterfallHeight;
			int panadapterHeight = _panadapterHeight;
			RecomputeSplit();
			if (_waterfallHeight != waterfallHeight || _panadapterHeight != panadapterHeight)
			{
				_splitResizePending = true;
			}
		}
	}

	internal float GetSplitPerc()
	{
		return _splitPerc;
	}

	internal bool IsOverDbScale(int x, int y)
	{
		int num = (int)((float)x * _cursorScale);
		int num2 = (int)((float)y * _cursorScale);
		if (num2 >= 0 && num2 < _panadapterHeight && num >= 0)
		{
			return num < 48;
		}
		return false;
	}

	internal bool IsOverSplitBar(int x, int y)
	{
		int num = (int)((float)y * _cursorScale);
		int num2 = _panadapterHeight - 3;
		int num3 = _panadapterHeight + 5;
		if (num >= num2)
		{
			return num <= num3;
		}
		return false;
	}

	public void RequestResize(int width, int height)
	{
		width = Math.Max(64, width);
		height = Math.Max(64, height);
		if (width != _width || height != _height || _resizePending)
		{
			_pendingWidth = width;
			_pendingHeight = height;
			_resizePending = true;
		}
	}

	public void OnBackgroundChanged()
	{
		if (_ctx == null)
		{
			return;
		}
		try
		{
			BuildBackgroundBitmap();
		}
		catch
		{
		}
	}

	public void OnFormatChanged()
	{
		if (!_disposed)
		{
			RequestResize(_width, _height);
		}
	}

	private void RecreateRenderTarget()
	{
		GPUWaterfallLogger.Log("DetachedRender", "RecreateRenderTarget: rebuilding after device loss");
		try
		{
			if (_ctx != null)
			{
				_ctx.Target = null;
			}
			Utilities.Dispose(ref _targetBitmap);
			Utilities.Dispose(ref _ctx);
			Utilities.Dispose(ref _surface);
			try
			{
				_device.ImmediateContext.ClearState();
				_device.ImmediateContext.Flush();
			}
			catch
			{
			}
			_surface = _swapChain1.GetBackBuffer<Surface>(0);
			_ctx = new SharpDX.Direct2D1.DeviceContext(_d2dDevice, DeviceContextOptions.None);
			BitmapProperties1 bitmapProperties = new BitmapProperties1(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Ignore), 96f, 96f, BitmapOptions.Target | BitmapOptions.CannotDraw);
			_targetBitmap = new Bitmap1(_ctx, _surface, bitmapProperties);
			_ctx.Target = _targetBitmap;
			_ctx.DotsPerInch = new Size2F(96f, 96f);
			_ctx.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale;
			CreateBrushes();
			BuildBackgroundBitmap();
			ScheduleWfGPUResize();
			_uploadedPaletteVersion = -1;
			_needsRecreate = false;
			GPUWaterfallLogger.Log("DetachedRender", "RecreateRenderTarget: success");
		}
		catch (Exception ex)
		{
			GPUWaterfallLogger.Log("DetachedRender", "RecreateRenderTarget FAILED: " + ex.Message);
		}
	}

	public void Render()
	{
		if (_disposed || _ctx == null)
		{
			return;
		}
		if (_needsRecreate)
		{
			RecreateRenderTarget();
		}
		_renderFrameCount++;
		long num = Stopwatch.GetTimestamp() * 1000 / Stopwatch.Frequency;
		if (num - _lastHeartbeatMs > 60000)
		{
			_lastHeartbeatMs = num;
			SharedWaterfallState.PerRxSnapshot rX = SharedWaterfallState.RX1;
			float num2 = float.NaN;
			float num3 = float.NaN;
			float[] panadapterRow = rX.PanadapterRow;
			if (panadapterRow != null && rX.PanadapterWidth >= 2)
			{
				int num4 = Math.Min(rX.PanadapterWidth, panadapterRow.Length);
				int num5 = num4 / 2;
				num2 = panadapterRow[0];
				num3 = panadapterRow[num5];
				for (int i = 1; i < num5; i++)
				{
					if (panadapterRow[i] > num2)
					{
						num2 = panadapterRow[i];
					}
				}
				for (int j = num5 + 1; j < num4; j++)
				{
					if (panadapterRow[j] > num3)
					{
						num3 = panadapterRow[j];
					}
				}
			}
			GPUWaterfallLogger.Log("DetachedRender", "heartbeat frames=" + _renderFrameCount + " size=" + DebugSize + " snapValid=" + rX.Valid + " fresh=" + SharedWaterfallState.ProduceForDetachedFresh + " panRowW=" + rX.PanadapterWidth + " max1=" + num2.ToString("F0") + " max2=" + num3.ToString("F0"));
		}
		if (_splitResizePending)
		{
			_splitResizePending = false;
			ScheduleWfGPUResize();
		}
		if (_wfGPU != null && !_wfGPUResizePending && WaterfallPipeWidth != _wfGPUPipeWidth)
		{
			ScheduleWfGPUResize();
		}
		if (_wfGPUResizePending && Environment.TickCount - _lastWfGPUResizeRequestMs >= 300)
		{
			_wfGPUResizePending = false;
			try
			{
				if (_wfGPU != null && _ctx != null)
				{
					_wfGPUPipeWidth = WaterfallPipeWidth;
					_wfGPU.Resize(_ctx, _wfGPUPipeWidth, _waterfallHeight, WaterfallPixelWriter.DxgiFormat);
				}
				_uploadedPaletteVersion = -1;
			}
			catch (Exception ex)
			{
				GPUWaterfallLogger.Log("DetachedRender", "deferred wfGPU resize exception: " + ex);
			}
		}
		SharedWaterfallState.PerRxSnapshot rX2 = SharedWaterfallState.RX1;
		if (!rX2.Valid)
		{
			return;
		}
		if (_uploadedScheme != rX2.Scheme || _uploadedPaletteVersion != rX2.PaletteVersion)
		{
			UploadPalette(rX2.Scheme);
			_uploadedScheme = rX2.Scheme;
			_uploadedPaletteVersion = rX2.PaletteVersion;
		}
		_ctx.BeginDraw();
		try
		{
			_ctx.Transform = Matrix3x2.Identity;
			DrawBackground();
			DrawPanadapter(rX2);
			DrawWaterfall(rX2);
			DrawSplitBar();
			DrawCTUNCursor(rX2);
		}
		finally
		{
			try
			{
				_ctx.EndDraw();
			}
			catch (SharpDXException ex2) when (ex2.ResultCode == SharpDX.Direct2D1.ResultCode.RecreateTarget)
			{
				GPUWaterfallLogger.Log("DetachedRender", "EndDraw RecreateTarget");
				_needsRecreate = true;
			}
		}
		try
		{
			if (WaterfallPixelWriter.DxgiFormat == Format.R16G16B16A16_Float && _swapChain1 != null)
			{
				long num6 = Stopwatch.GetTimestamp() * 1000 / Stopwatch.Frequency;
				if (num6 - _lastSrgbWatchdogMs > 2000)
				{
					_lastSrgbWatchdogMs = num6;
					ApplySRGBColorSpaceForFloatSwapChain(_swapChain1, quiet: true);
				}
			}
			PresentFlags flags = (_flipModel ? (_allowTearing ? PresentFlags.AllowTearing : PresentFlags.DoNotWait) : PresentFlags.None);
			Result result = _swapChain1.TryPresent(0, flags);
			if (result != Result.Ok && result != SharpDX.DXGI.ResultCode.WasStillDrawing && result != 142213121)
			{
				Result result2 = result;
				GPUWaterfallLogger.Log("DetachedPresent", "result " + result2.ToString());
			}
			if (result != _lastPresentResult)
			{
				_lastPresentResult = result;
				if (result != Result.Ok)
				{
					Result result2 = result;
					GPUWaterfallLogger.Log("DetachedPresent", "state changed to " + result2.ToString());
				}
			}
			if (result == SharpDX.DXGI.ResultCode.WasStillDrawing)
			{
				if (_flipModel && ++_stillDrawingStreak == 120)
				{
					GPUWaterfallLogger.Log("DetachedPresent", "WAS_STILL_DRAWING x120 — recreating swap chain with legacy Discard effect");
					RecreateSwapChainLegacy();
				}
			}
			else
			{
				_stillDrawingStreak = 0;
			}
		}
		catch (SharpDXException ex3)
		{
			GPUWaterfallLogger.Log("DetachedPresent", "exception: " + ex3.Message);
		}
	}

	private void RecreateSwapChainLegacy()
	{
		try
		{
			_stillDrawingStreak = 0;
			_flipModel = false;
			_bufferCount = 1;
			if (_ctx != null)
			{
				_ctx.Target = null;
			}
			Utilities.Dispose(ref _targetBitmap);
			Utilities.Dispose(ref _ctx);
			Utilities.Dispose(ref _surface);
			Utilities.Dispose(ref _swapChain1);
			Utilities.Dispose(ref _swapChain);
			try
			{
				_device.ImmediateContext.ClearState();
				_device.ImmediateContext.Flush();
			}
			catch
			{
			}
			Format dxgiFormat = WaterfallPixelWriter.DxgiFormat;
			Format format = ((dxgiFormat == Format.R16G16B16A16_Float) ? Format.B8G8R8A8_UNorm : dxgiFormat);
			int numerator = 60;
			try
			{
				numerator = Math.Max(1, Console.getConsole().DisplayFPS);
			}
			catch
			{
			}
			ModeDescription modeDescription = new ModeDescription(_width, _height, new Rational(numerator, 1), format);
			modeDescription.ScanlineOrdering = DisplayModeScanlineOrder.Progressive;
			modeDescription.Scaling = DisplayModeScaling.Stretched;
			SwapChainDescription description = new SwapChainDescription
			{
				BufferCount = 1,
				ModeDescription = modeDescription,
				IsWindowed = true,
				OutputHandle = _hwnd,
				SampleDescription = new SampleDescription(1, 0),
				SwapEffect = SwapEffect.Discard,
				Usage = Usage.RenderTargetOutput,
				Flags = SwapChainFlags.None
			};
			_swapChain = new SwapChain(_dxgiFactory, _device, description);
			_swapChain1 = _swapChain.QueryInterface<SwapChain1>();
			if (format != dxgiFormat)
			{
				GPUWaterfallLogger.Log("DetachedPresent", "legacy Discard chain kept at UNorm (float would be scRGB on a non-flip chain)");
			}
			_surface = _swapChain1.GetBackBuffer<Surface>(0);
			_ctx = new SharpDX.Direct2D1.DeviceContext(_d2dDevice, DeviceContextOptions.None);
			BitmapProperties1 bitmapProperties = new BitmapProperties1(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Ignore), 96f, 96f, BitmapOptions.Target | BitmapOptions.CannotDraw);
			_targetBitmap = new Bitmap1(_ctx, _surface, bitmapProperties);
			_ctx.Target = _targetBitmap;
			_ctx.DotsPerInch = new Size2F(96f, 96f);
			_ctx.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale;
			CreateBrushes();
			BuildBackgroundBitmap();
			ScheduleWfGPUResize();
			_uploadedPaletteVersion = -1;
			_lastPresentResult = Result.Ok;
			GPUWaterfallLogger.Log("DetachedPresent", "legacy Discard swap chain created OK");
		}
		catch (Exception ex)
		{
			GPUWaterfallLogger.Log("DetachedPresent", "legacy swap chain recreate failed: " + ex.Message);
		}
	}

	private void RecreateSwapChainForHwnd()
	{
		try
		{
			_stillDrawingStreak = 0;
			if (_ctx != null)
			{
				_ctx.Target = null;
			}
			Utilities.Dispose(ref _targetBitmap);
			Utilities.Dispose(ref _ctx);
			Utilities.Dispose(ref _surface);
			Utilities.Dispose(ref _swapChain1);
			Utilities.Dispose(ref _swapChain);
			try
			{
				_device.ImmediateContext.ClearState();
				_device.ImmediateContext.Flush();
			}
			catch
			{
			}
			Format dxgiFormat = WaterfallPixelWriter.DxgiFormat;
			Format format = ((dxgiFormat == Format.R16G16B16A16_Float) ? Format.B8G8R8A8_UNorm : dxgiFormat);
			int numerator = 60;
			try
			{
				numerator = Math.Max(1, Console.getConsole().DisplayFPS);
			}
			catch
			{
			}
			ModeDescription modeDescription = new ModeDescription(_width, _height, new Rational(numerator, 1), format);
			modeDescription.ScanlineOrdering = DisplayModeScanlineOrder.Progressive;
			modeDescription.Scaling = DisplayModeScaling.Stretched;
			SwapChainDescription description = new SwapChainDescription
			{
				BufferCount = _bufferCount,
				ModeDescription = modeDescription,
				IsWindowed = true,
				OutputHandle = _hwnd,
				SampleDescription = new SampleDescription(1, 0),
				SwapEffect = (_flipModel ? SwapEffect.FlipDiscard : SwapEffect.Discard),
				Usage = Usage.RenderTargetOutput,
				Flags = ((_flipModel && _allowTearing) ? SwapChainFlags.AllowTearing : SwapChainFlags.None)
			};
			_dxgiFactory.MakeWindowAssociation(_hwnd, WindowAssociationFlags.IgnoreAll);
			try
			{
				_swapChain = new SwapChain(_dxgiFactory, _device, description);
			}
			catch (Exception)
			{
				if (!_flipModel)
				{
					throw;
				}
				_flipModel = false;
				_allowTearing = false;
				_bufferCount = 1;
				description.BufferCount = 1;
				description.SwapEffect = SwapEffect.Discard;
				description.Flags = SwapChainFlags.None;
				_swapChain = new SwapChain(_dxgiFactory, _device, description);
			}
			_swapChain1 = _swapChain.QueryInterface<SwapChain1>();
			ApplySRGBColorSpaceForFloatSwapChain(_swapChain1);
			if (format != dxgiFormat)
			{
				try
				{
					_swapChain1.ResizeBuffers(_bufferCount, _width, _height, dxgiFormat, _allowTearing ? SwapChainFlags.AllowTearing : SwapChainFlags.None);
				}
				catch (Exception ex2)
				{
					GPUWaterfallLogger.Log("DetachedPresent", "HWND rebuild: float ResizeBuffers failed (8-bit kept): " + ex2.Message);
				}
			}
			ApplySRGBColorSpaceForFloatSwapChain(_swapChain1);
			_surface = _swapChain1.GetBackBuffer<Surface>(0);
			_ctx = new SharpDX.Direct2D1.DeviceContext(_d2dDevice, DeviceContextOptions.None);
			BitmapProperties1 bitmapProperties = new BitmapProperties1(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Ignore), 96f, 96f, BitmapOptions.Target | BitmapOptions.CannotDraw);
			_targetBitmap = new Bitmap1(_ctx, _surface, bitmapProperties);
			_ctx.Target = _targetBitmap;
			_ctx.DotsPerInch = new Size2F(96f, 96f);
			_ctx.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale;
			CreateBrushes();
			BuildBackgroundBitmap();
			ScheduleWfGPUResize();
			_uploadedPaletteVersion = -1;
			_lastPresentResult = Result.Ok;
			GPUWaterfallLogger.Log("DetachedPresent", "swap chain recreated for new HWND OK (flip=" + _flipModel + ", tearing=" + _allowTearing + ")");
		}
		catch (Exception ex3)
		{
			GPUWaterfallLogger.Log("DetachedPresent", "HWND swap chain recreate failed: " + ex3.Message);
		}
	}

	private void DrawSplitBar()
	{
		if (_splitBrush != null)
		{
			float y = _panadapterHeight;
			_ctx.DrawLine(new RawVector2(0f, y), new RawVector2(_width, y), _splitBrush, 1f);
		}
	}

	private void DrawBackground()
	{
		_ctx.Clear(new RawColor4(0f, 0f, 0f, 1f));
		if (_backgroundBitmap != null)
		{
			float width = _backgroundBitmap.Size.Width;
			float height = _backgroundBitmap.Size.Height;
			if (width > 0f && height > 0f)
			{
				float val = (float)_width / width;
				float val2 = (float)_height / height;
				float num = Math.Max(val, val2);
				float num2 = width * num;
				float num3 = height * num;
				float num4 = ((float)_width - num2) * 0.5f;
				float num5 = ((float)_height - num3) * 0.5f;
				_ctx.DrawBitmap(_backgroundBitmap, new RawRectangleF(num4, num5, num4 + num2, num5 + num3), 1f, BitmapInterpolationMode.Linear);
			}
		}
		else
		{
			_ctx.FillRectangle(new RawRectangleF(0f, 0f, _width, _height), _bgFillBrush);
		}
	}

	private void DrawPanadapter(SharedWaterfallState.PerRxSnapshot snap)
	{
		float num = 0f;
		float num2 = _panadapterHeight;
		float num3 = num2 - num;
		float[] panadapterRow = snap.PanadapterRow;
		if (panadapterRow == null || snap.PanadapterWidth <= 1)
		{
			return;
		}
		int panadapterWidth = snap.PanadapterWidth;
		float num4 = (float)_width / (float)panadapterWidth;
		int spectrumGridMax = Display.SpectrumGridMax;
		int spectrumGridMin = Display.SpectrumGridMin;
		int num5 = Display.SpectrumGridStep;
		if (num5 < 1)
		{
			num5 = 1;
		}
		int num6 = spectrumGridMax - spectrumGridMin;
		if (num6 <= 0)
		{
			num6 = 100;
		}
		float num7 = num3 / (float)num6;
		int num8 = (int)((double)num5 * (double)num3 / (double)num6);
		if (num8 < 14)
		{
			num8 = 14;
		}
		int num9 = (int)snap.RXDisplayLowHz;
		int num10 = (int)snap.RXDisplayHighHz - num9;
		int[] array = new int[4] { 10, 20, 25, 50 };
		int num11 = 1;
		int num12 = 0;
		int num13 = 50;
		if (num10 > 0)
		{
			while (num10 / num13 > 10)
			{
				num13 = array[num12] * (int)Math.Pow(10.0, num11);
				num12 = (num12 + 1) % 4;
				if (num12 == 0)
				{
					num11++;
				}
			}
		}
		System.Drawing.Color c = (Display.MOX ? Display.TXFilterColor : Display.DisplayFilterColor);
		_ = Display.GridZeroColor;
		int freqDiff = Display.FreqDiff;
		if (num10 > 0)
		{
			bool mOX = Display.MOX;
			int num14 = (mOX ? Display.TXFilterLow : Display.RX1FilterLow);
			int num15 = (mOX ? Display.TXFilterHigh : Display.RX1FilterHigh);
			long num16 = -freqDiff;
			if (mOX)
			{
				int num17 = (Display.DisplayDuplex ? Display.XIT : 0);
				num16 = (Display.SplitEnabled ? (num17 + (Display.DisplayDuplex ? (Display.VFOASub - Display.VFOA) : 0)) : (-freqDiff + num17));
			}
			float num18 = (float)(num14 - num9 + num16) / (float)num10 * (float)_width;
			float num19 = (float)(num15 - num9 + num16) / (float)num10 * (float)_width;
			if (num19 < num18)
			{
				float num20 = num18;
				num18 = num19;
				num19 = num20;
			}
			if (num19 > 0f && num18 < (float)_width)
			{
				float left = Math.Max(0f, num18);
				float right = Math.Min(_width, num19);
				using SolidColorBrush brush = new SolidColorBrush(_ctx, ToRawColor(c));
				_ctx.FillRectangle(new RawRectangleF(left, num + (float)num8, right, num2), brush);
			}
			if (Display.SubRX1Enabled && !Display.MOX)
			{
				long num21 = Display.VFOASub - Display.VFOA;
				float num22 = (float)(num14 - num9 - freqDiff + num21) / (float)num10 * (float)_width;
				float num23 = (float)(num15 - num9 - freqDiff + num21) / (float)num10 * (float)_width;
				if (num23 < num22)
				{
					float num24 = num22;
					num22 = num23;
					num23 = num24;
				}
				if (num23 > 0f && num22 < (float)_width)
				{
					float left2 = Math.Max(0f, num22);
					float right2 = Math.Min(_width, num23);
					System.Drawing.Color subRXFilterColor = Display.SubRXFilterColor;
					using SolidColorBrush brush2 = new SolidColorBrush(_ctx, ToRawColor(subRXFilterColor));
					_ctx.FillRectangle(new RawRectangleF(left2, num + (float)num8, right2, num2), brush2);
				}
				int num25 = (int)((double)(num21 - num9 - freqDiff) / (double)num10 * (double)_width);
				if (num25 >= 0 && num25 <= _width)
				{
					System.Drawing.Color subRXZeroLine = Display.SubRXZeroLine;
					using SolidColorBrush brush3 = new SolidColorBrush(_ctx, ToRawColor(subRXZeroLine));
					_ctx.DrawLine(new RawVector2(num25, num + (float)num8), new RawVector2(num25, num2), brush3, 2f);
				}
			}
		}
		int num26 = num6 / num5;
		System.Drawing.Color hGridColor = Display.HGridColor;
		System.Drawing.Color gridTextColor = Display.GridTextColor;
		using (SolidColorBrush solidColorBrush = new SolidColorBrush(_ctx, ToRawColor(hGridColor)))
		{
			using SolidColorBrush defaultForegroundBrush = new SolidColorBrush(_ctx, ToRawColor(gridTextColor));
			for (int i = 1; i < num26; i++)
			{
				int num27 = spectrumGridMax - i * num5;
				float num28 = (float)(spectrumGridMax - num27) * num7;
				if (num28 < (float)num8 || num28 > num3)
				{
					continue;
				}
				solidColorBrush.Opacity = 0.45f;
				_ctx.DrawLine(new RawVector2(0f, num28), new RawVector2(_width, num28), solidColorBrush, 1f);
				solidColorBrush.Opacity = 1f;
				if (i != 1)
				{
					string text = num27.ToString();
					float num29 = num28 - 8f * _cursorScale;
					if (num29 + 12f * _cursorScale < num3)
					{
						_ctx.DrawText(text, _dbTextFormat, new RawRectangleF(2f, num29, 60f * _cursorScale, num29 + 14f * _cursorScale), defaultForegroundBrush);
					}
				}
			}
		}
		if (num10 > 0 && num13 > 0)
		{
			System.Drawing.Color gridColor = Display.GridColor;
			using SolidColorBrush solidColorBrush2 = new SolidColorBrush(_ctx, ToRawColor(gridColor));
			using SolidColorBrush defaultForegroundBrush2 = new SolidColorBrush(_ctx, ToRawColor(gridTextColor));
			int num30 = num10 / num13 + 1;
			long vFOA = Display.VFOA;
			long num31 = vFOA / num13 * num13;
			long num32 = vFOA - num31;
			for (int j = -1; j < num30 + 1; j++)
			{
				int num33 = j * num13 + num9 / num13 * num13;
				double num34 = (double)(num31 + num33) / 1000000.0;
				int num35 = (int)((double)(num33 - num32 - num9) / (double)num10 * (double)_width);
				if (num35 < 0 || num35 > _width)
				{
					continue;
				}
				solidColorBrush2.Opacity = 0.3f;
				_ctx.DrawLine(new RawVector2(num35, num + (float)num8), new RawVector2(num35, num2), solidColorBrush2, 1f);
				solidColorBrush2.Opacity = 1f;
				if (Display.GridControlMajor && Display.GridControlMinor)
				{
					float num36 = (float)((int)((float)((j + 1) * num13 + num9 / num13 * num13 - num32 - num9) / (float)num10 * (float)_width) - num35) / 5f;
					solidColorBrush2.Opacity = 0.12f;
					for (int k = 1; k < 5; k++)
					{
						float num37 = (float)num35 + (float)k * num36;
						if (num37 > 0f && num37 < (float)_width)
						{
							_ctx.DrawLine(new RawVector2(num37, num + (float)num8), new RawVector2(num37, num2), solidColorBrush2, 1f);
						}
					}
					solidColorBrush2.Opacity = 1f;
				}
				if (Display.ShowFrequencyNumbers)
				{
					string text2 = ((!(Math.Abs(num34 * 1000.0 - Math.Round(num34 * 1000.0)) < 0.0001)) ? num34.ToString("f4") : num34.ToString("f3"));
					float num38 = MeasureTextWidth(text2) + 4f * _cursorScale;
					float num39 = (float)num35 - num38 / 2f;
					if (num39 < 1f)
					{
						num39 = 1f;
					}
					if (num39 + num38 > (float)(_width - 1))
					{
						num39 = (float)_width - num38 - 1f;
					}
					_ctx.DrawText(text2, _dbTextFormat, new RawRectangleF(num39, 3f, num39 + num38, 3f + 14f * _cursorScale), defaultForegroundBrush2);
				}
			}
		}
		if (Display.ShowZeroLine && num10 > 0)
		{
			System.Drawing.Color c2 = (Display.MOX ? Display.TXGridZeroColor : Display.GridZeroColor);
			int cWSideToneShift = Display.getCWSideToneShift(1);
			int num40 = (int)((double)(-Display.FreqDiff - -cWSideToneShift - num9) / (double)num10 * (double)_width);
			if (num40 >= 0 && num40 <= _width)
			{
				using SolidColorBrush brush4 = new SolidColorBrush(_ctx, ToRawColor(c2));
				_ctx.DrawLine(new RawVector2(num40, num + (float)num8), new RawVector2(num40, num2), brush4, 2f);
			}
		}
		Utilities.Dispose(ref _spectrumPath);
		_spectrumPath = new PathGeometry(_d2dFactory);
		GeometrySink comObject = _spectrumPath.Open();
		try
		{
			bool flag = true;
			for (int l = 0; l < panadapterWidth; l++)
			{
				float num41 = panadapterRow[l] + snap.RXDisplayOffsetDb;
				float num42 = ((float)spectrumGridMax - num41) * num7;
				if (num42 < num)
				{
					num42 = num;
				}
				if (num42 > num2)
				{
					num42 = num2;
				}
				float x = (float)l * num4;
				if (flag)
				{
					comObject.BeginFigure(new RawVector2(x, num42), FigureBegin.Filled);
					flag = false;
				}
				else
				{
					comObject.AddLine(new RawVector2(x, num42));
				}
			}
			if (!flag)
			{
				comObject.AddLine(new RawVector2((float)(panadapterWidth - 1) * num4, num2));
				comObject.AddLine(new RawVector2(0f, num2));
				comObject.EndFigure(FigureEnd.Closed);
			}
		}
		finally
		{
			comObject.Close();
			Utilities.Dispose(ref comObject);
		}
		System.Drawing.Color c3 = (Display.MOX ? Display.TXDataLineColor : Display.DataLineColor);
		using (SolidColorBrush brush5 = new SolidColorBrush(_ctx, ToRawColor(c3)))
		{
			_ctx.DrawGeometry(_spectrumPath, brush5, 1.5f);
		}
		DrawCursorOverlay(snap, spectrumGridMax, spectrumGridMin, num6, num7, num9, num10, num8);
	}

	private void DrawCursorOverlay(SharedWaterfallState.PerRxSnapshot snap, int gridMax, int gridMin, int yRange, float dbmToPixel, int low, int width, int topBand)
	{
		if (!_cursorAiming || _cursorX < 0f || _cursorY < 0f)
		{
			return;
		}
		System.Drawing.Color c = (_aimTargetSubRX ? Display.SubRXZeroLine : System.Drawing.Color.White);
		using (SolidColorBrush solidColorBrush = new SolidColorBrush(_ctx, ToRawColor(c)))
		{
			solidColorBrush.Opacity = 0.45f;
			_ctx.DrawLine(new RawVector2(_cursorX, 0f), new RawVector2(_cursorX, _height), solidColorBrush, 1f);
			_ctx.DrawLine(new RawVector2(0f, _cursorY), new RawVector2(_width, _cursorY), solidColorBrush, 1f);
			solidColorBrush.Opacity = 1f;
		}
		if (_cursorY >= (float)_panadapterHeight || width <= 0)
		{
			return;
		}
		long vFOA = Display.VFOA;
		double num = (double)low + (double)_cursorX / (double)_width * (double)width;
		double num2 = ((double)vFOA + num) / 1000000.0;
		double num3 = (float)gridMax - _cursorY / dbmToPixel;
		string text = num2.ToString("f6") + " MHz";
		int num4 = text.IndexOf('.');
		if (num4 >= 0 && num4 + 4 < text.Length - 4)
		{
			text = text.Substring(0, num4 + 4) + " " + text.Substring(num4 + 4);
		}
		string text2 = num3.ToString("f1") + " dBm";
		float num5 = Math.Max(MeasureTextWidth(text), MeasureTextWidth(text2)) + 12f * _cursorScale;
		float num6 = 32f * _cursorScale;
		float num7 = _cursorX + 12f;
		float num8 = _cursorY - 34f;
		if (num7 + num5 > (float)(_width - 2))
		{
			num7 = _cursorX - num5 - 12f;
		}
		if (num8 < 2f)
		{
			num8 = _cursorY + 12f;
		}
		using SolidColorBrush brush = new SolidColorBrush(_ctx, new RawColor4(0f, 0f, 0f, 0.65f));
		using SolidColorBrush brush2 = new SolidColorBrush(_ctx, new RawColor4(1f, 1f, 1f, 0.4f));
		using SolidColorBrush defaultForegroundBrush = new SolidColorBrush(_ctx, new RawColor4(1f, 1f, 1f, 0.95f));
		RoundedRectangle roundedRect = new RoundedRectangle
		{
			Rect = new RawRectangleF(num7, num8, num7 + num5, num8 + num6),
			RadiusX = 4f,
			RadiusY = 4f
		};
		_ctx.FillRoundedRectangle(roundedRect, brush);
		_ctx.DrawRoundedRectangle(roundedRect, brush2, 1f);
		_ctx.DrawText(text, _dbTextFormat, new RawRectangleF(num7 + 6f * _cursorScale, num8 + 4f * _cursorScale, num7 + num5 - 4f, num8 + 18f * _cursorScale), defaultForegroundBrush);
		_ctx.DrawText(text2, _dbTextFormat, new RawRectangleF(num7 + 6f * _cursorScale, num8 + 17f * _cursorScale, num7 + num5 - 4f, num8 + 31f * _cursorScale), defaultForegroundBrush);
	}

	private static RawColor4 ToRawColor(System.Drawing.Color c)
	{
		return new RawColor4((float)(int)c.R / 255f, (float)(int)c.G / 255f, (float)(int)c.B / 255f, (float)(int)c.A / 255f);
	}

	internal void SetCursor(float x, float y, bool aiming)
	{
		_cursorX = ((x < 0f) ? (-1f) : (x * _cursorScale));
		_cursorY = ((y < 0f) ? (-1f) : (y * _cursorScale));
		_cursorTracking = x >= 0f && y >= 0f;
		_cursorAiming = aiming;
	}

	internal void SetAimTarget(bool subRX)
	{
		_aimTargetSubRX = subRX;
	}

	private void DrawCTUNCursor(SharedWaterfallState.PerRxSnapshot snap)
	{
		ClickTuneMode currentClickTuneMode = Display.CurrentClickTuneMode;
		if (currentClickTuneMode == ClickTuneMode.Off || !_cursorTracking || _cursorX < 0f)
		{
			return;
		}
		System.Drawing.Color c = ((currentClickTuneMode == ClickTuneMode.VFOA) ? Display.GridTextColor : System.Drawing.Color.Red);
		using SolidColorBrush brush = new SolidColorBrush(_ctx, ToRawColor(c));
		_ctx.DrawLine(new RawVector2(_cursorX, 0f), new RawVector2(_cursorX, _height), brush, 1.5f);
	}

	private void DrawTextRightAligned(string text, float rightX, float y, SolidColorBrush brush)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		try
		{
			_ctx.DrawText(text, _dbTextFormat, new RawRectangleF(rightX - 44f * _cursorScale, y, rightX, y + 14f * _cursorScale), brush);
		}
		catch
		{
		}
	}

	private void DrawWaterfall(SharedWaterfallState.PerRxSnapshot snap)
	{
		if (_wfGPU == null || !_wfGPU.IsInitialized)
		{
			return;
		}
		float invGamma = ((snap.Gamma != 0f) ? (1f / snap.Gamma) : 1f);
		bool produceForDetachedFresh = SharedWaterfallState.ProduceForDetachedFresh;
		if (produceForDetachedFresh)
		{
			GPUWaterfallPipeline sharedGPUFFTRX = Display.SharedGPUFFTRX1;
			int waterfallPipeWidth = WaterfallPipeWidth;
			if (sharedGPUFFTRX != null && sharedGPUFFTRX.MagSpectrumView != null && !sharedGPUFFTRX.MagSpectrumView.IsDisposed)
			{
				_wfGPU.ProcessRow(sharedGPUFFTRX.MagSpectrumView, waterfallPipeWidth, 1, snap.LowThreshold - snap.GpuCalOffset - snap.RXDisplayOffsetDb, snap.HighThreshold - snap.GpuCalOffset - snap.RXDisplayOffsetDb, snap.Gamma, invGamma, snap.EffectiveToneMap, WaterfallEnhancer.SaturationBoost, WaterfallEnhancer.ContrastBoost, WaterfallEnhancer.DitherEnabled, WaterfallEnhancer.Levels, snap.EffectiveTemporalAlpha, 0.05f, snap.Scheme != ColorScheme.Custom, snap.Scheme == ColorScheme.Custom, WaterfallEnhancer.PaletteSharpness, WaterfallEnhancer.PaletteContrast);
			}
			else
			{
				float[] sharedCurrentWaterfallDataRX = Display.SharedCurrentWaterfallDataRX1;
				int sharedDisplayTargetWidth = Display.SharedDisplayTargetWidth;
				if (sharedCurrentWaterfallDataRX != null && sharedDisplayTargetWidth > 0)
				{
					_wfGPU.ProcessRow(sharedCurrentWaterfallDataRX, Math.Min(sharedDisplayTargetWidth, sharedCurrentWaterfallDataRX.Length), 1, snap.LowThreshold, snap.HighThreshold, snap.Gamma, invGamma, snap.EffectiveToneMap, WaterfallEnhancer.SaturationBoost, WaterfallEnhancer.ContrastBoost, WaterfallEnhancer.DitherEnabled, WaterfallEnhancer.Levels, snap.EffectiveTemporalAlpha, 0.05f, snap.Scheme != ColorScheme.Custom, snap.Scheme == ColorScheme.Custom, WaterfallEnhancer.PaletteSharpness, WaterfallEnhancer.PaletteContrast);
				}
			}
		}
		_wfGPU.AdvanceRow(0, produceForDetachedFresh);
		_wfGPU.DrawScaled(0, _panadapterHeight, _width, _waterfallHeight, 1f, linearInterpolation: true);
		if (!Display.ShowRXFilterOnWaterfall)
		{
			return;
		}
		int num = (int)snap.RXDisplayLowHz;
		int num2 = (int)snap.RXDisplayHighHz - num;
		if (num2 <= 0)
		{
			return;
		}
		int freqDiff = Display.FreqDiff;
		bool mOX = Display.MOX;
		int num3 = (mOX ? Display.TXFilterLow : Display.RX1FilterLow);
		int num4 = (mOX ? Display.TXFilterHigh : Display.RX1FilterHigh);
		long num5 = -freqDiff;
		if (mOX)
		{
			int num6 = (Display.DisplayDuplex ? Display.XIT : 0);
			num5 = (Display.SplitEnabled ? (num6 + (Display.DisplayDuplex ? (Display.VFOASub - Display.VFOA) : 0)) : (-freqDiff + num6));
		}
		float num7 = (float)(num3 - num + num5) / (float)num2 * (float)_width;
		float num8 = (float)(num4 - num + num5) / (float)num2 * (float)_width;
		if (num8 < num7)
		{
			float num9 = num7;
			num7 = num8;
			num8 = num9;
		}
		if (num8 > 0f && num7 < (float)_width)
		{
			float left = Math.Max(0f, num7);
			float right = Math.Min(_width, num8);
			System.Drawing.Color c = (mOX ? Display.TXFilterColor : Display.DisplayFilterColor);
			using SolidColorBrush brush = new SolidColorBrush(_ctx, ToRawColor(c));
			_ctx.FillRectangle(new RawRectangleF(left, _panadapterHeight, right, _height), brush);
		}
		if (!Display.SubRX1Enabled || Display.MOX)
		{
			return;
		}
		long num10 = Display.VFOASub - Display.VFOA;
		float num11 = (float)(num3 - num - freqDiff + num10) / (float)num2 * (float)_width;
		float num12 = (float)(num4 - num - freqDiff + num10) / (float)num2 * (float)_width;
		if (num12 < num11)
		{
			float num13 = num11;
			num11 = num12;
			num12 = num13;
		}
		if (!(num12 > 0f) || !(num11 < (float)_width))
		{
			return;
		}
		float left2 = Math.Max(0f, num11);
		float right2 = Math.Min(_width, num12);
		System.Drawing.Color subRXFilterColor = Display.SubRXFilterColor;
		using SolidColorBrush brush2 = new SolidColorBrush(_ctx, ToRawColor(subRXFilterColor));
		_ctx.FillRectangle(new RawRectangleF(left2, _panadapterHeight, right2, _height), brush2);
	}

	private void UploadPalette(ColorScheme scheme)
	{
		if (_wfGPU == null)
		{
			return;
		}
		if (scheme == ColorScheme.Custom)
		{
			System.Drawing.Color[] rX1WaterfallGradient = Display.RX1WaterfallGradient;
			if (rX1WaterfallGradient != null && rX1WaterfallGradient.Length != 0)
			{
				Display.UploadCustomGradientToGPU(_wfGPU, rX1WaterfallGradient);
			}
		}
		else
		{
			Display.UploadPaletteToGPU(_wfGPU, Display.GetPaletteForScheme(scheme));
		}
	}

	public void Resize(int width, int height)
	{
		GPUWaterfallLogger.Log("DetachedResize", "Resize() entry: " + width + "x" + height + ", current=" + DebugSize + ", disposed=" + _disposed);
		if (_disposed || _swapChain1 == null || _ctx == null)
		{
			GPUWaterfallLogger.Log("DetachedResize", "Resize() abort: disposed=" + _disposed + " swapChain=" + (_swapChain1 != null) + " ctx=" + (_ctx != null));
			return;
		}
		_logicalW = Math.Max(64, width);
		_logicalH = Math.Max(64, height);
		int width2 = _logicalW;
		int height2 = _logicalH;
		if (Display.HiDpiPhysicalRender)
		{
			Size physicalClientSize = HiDpiSurfaces.GetPhysicalClientSize(_hwnd);
			if (physicalClientSize.Width >= 64 && physicalClientSize.Height >= 64)
			{
				width2 = physicalClientSize.Width;
				height2 = physicalClientSize.Height;
			}
		}
		_width = width2;
		_height = height2;
		_cursorScale = Math.Max(1f, (float)_width / (float)_logicalW);
		RecomputeSplit();
		if (_hwndChanged)
		{
			_hwndChanged = false;
			GPUWaterfallLogger.Log("DetachedResize", "HWND changed — full swap chain rebuild (" + _width + "x" + _height + ")");
			RecreateSwapChainForHwnd();
			return;
		}
		_ctx.Target = null;
		Utilities.Dispose(ref _targetBitmap);
		Utilities.Dispose(ref _ctx);
		Utilities.Dispose(ref _surface);
		try
		{
			_device.ImmediateContext.ClearState();
			_device.ImmediateContext.Flush();
		}
		catch (Exception ex)
		{
			GPUWaterfallLogger.Log("DetachedResize", "ClearState/Flush exception: " + ex);
		}
		try
		{
			_swapChain1.ResizeBuffers(_bufferCount, _width, _height, _swapChain.Description.ModeDescription.Format, _allowTearing ? SwapChainFlags.AllowTearing : SwapChainFlags.None);
		}
		catch (Exception ex2)
		{
			GPUWaterfallLogger.Log("DetachedResize", "ResizeBuffers FAILED: " + ex2.Message + " — rebuilding as legacy Discard");
			RecreateSwapChainLegacy();
			return;
		}
		Utilities.Dispose(ref _surface);
		_surface = _swapChain1.GetBackBuffer<Surface>(0);
		_ctx = new SharpDX.Direct2D1.DeviceContext(_d2dDevice, DeviceContextOptions.None);
		BitmapProperties1 bitmapProperties = new BitmapProperties1(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Ignore), 96f, 96f, BitmapOptions.Target | BitmapOptions.CannotDraw);
		_targetBitmap = new Bitmap1(_ctx, _surface, bitmapProperties);
		_ctx.Target = _targetBitmap;
		_ctx.DotsPerInch = new Size2F(96f, 96f);
		_ctx.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale;
		CreateBrushes();
		BuildBackgroundBitmap();
		ScheduleWfGPUResize();
		_uploadedPaletteVersion = -1;
		GPUWaterfallLogger.Log("DetachedResize", "Resize() success, new size=" + DebugSize);
	}

	private void ScheduleWfGPUResize()
	{
		_wfGPUResizePending = true;
		_lastWfGPUResizeRequestMs = Environment.TickCount;
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			Utilities.Dispose(ref _spectrumPath);
			Utilities.Dispose(ref _gridBrush);
			Utilities.Dispose(ref _spectrumBrush);
			Utilities.Dispose(ref _filterBrush);
			Utilities.Dispose(ref _textBrush);
			Utilities.Dispose(ref _bgFillBrush);
			Utilities.Dispose(ref _splitBrush);
			Utilities.Dispose(ref _dbTextFormat);
			Utilities.Dispose(ref _dwFactory);
			Utilities.Dispose(ref _backgroundBitmap);
			Utilities.Dispose(ref _wfGPU);
			if (_ctx != null)
			{
				_ctx.Target = null;
			}
			Utilities.Dispose(ref _targetBitmap);
			Utilities.Dispose(ref _ctx);
			Utilities.Dispose(ref _surface);
			Utilities.Dispose(ref _swapChain1);
			Utilities.Dispose(ref _swapChain);
		}
	}
}
