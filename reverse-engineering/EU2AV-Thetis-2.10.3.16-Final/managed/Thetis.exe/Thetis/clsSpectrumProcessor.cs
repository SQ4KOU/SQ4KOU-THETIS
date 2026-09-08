using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Thetis;

public sealed class clsSpectrumProcessor : IDisposable
{
	private enum SpectrumSourceType
	{
		Receiver,
		Transmitter
	}

	private sealed class SpectrumEndpoint
	{
		private readonly object _syncRoot = new object();

		private readonly SpectrumSourceType _sourceType;

		private readonly int _sourceId;

		private readonly int _displayId;

		private readonly int _maxFftSize;

		private readonly SpecHPSDR _spec;

		private float[] _latestPixels;

		private float[] _workingPixels;

		private int _pixels;

		private int _frameRate;

		private int _configuredSampleRate;

		private int _fftSize;

		private int _blockSize;

		private int _windowType;

		private int _detectorTypePan;

		private int _averageMode;

		private double _averageTau;

		private bool _averageOn;

		private bool _peakOn;

		private bool _normOneHzPan;

		private double _zoomSlider;

		private double _panSlider;

		private bool _enabled;

		private bool _hasData;

		private bool _shutdown;

		private int _dataIndex;

		private long _nextReadUtcTicks;

		public SpectrumSourceType SourceType => _sourceType;

		public SpectrumEndpoint(SpectrumSourceType sourceType, int sourceId, int pixels, int frameRate, int fftSize)
		{
			_sourceType = sourceType;
			_sourceId = sourceId;
			_pixels = pixels;
			_frameRate = frameRate;
			_configuredSampleRate = 192000;
			_fftSize = NormalizeFftSize(fftSize);
			_blockSize = ResolveDefaultBlockSize(_configuredSampleRate);
			_windowType = 4;
			_detectorTypePan = 0;
			_averageMode = 0;
			_averageTau = 0.12;
			_averageOn = false;
			_peakOn = false;
			_normOneHzPan = false;
			_zoomSlider = 0.0;
			_panSlider = 0.5;
			_maxFftSize = 262144;
			_enabled = true;
			EnsurePixelBuffersLocked(_pixels);
			ClearPixelBuffersLocked();
			_displayId = cmaster.AllocAnalyzer((int)_sourceType, _sourceId, _maxFftSize);
			if (_displayId < 0)
			{
				throw new InvalidOperationException("Unable to allocate analyzer for source type " + _sourceType.ToString() + " id " + _sourceId + ".");
			}
			_spec = new SpecHPSDR(_displayId)
			{
				Update = false
			};
			Refresh();
			cmaster.RunAnalyzer(_displayId, 1);
		}

		public bool MatchesReceiverEvent(int rx)
		{
			if (_sourceType == SpectrumSourceType.Receiver)
			{
				return _sourceId + 1 == rx;
			}
			return false;
		}

		public void SetPixels(int pixels)
		{
			lock (_syncRoot)
			{
				if (!_shutdown && pixels != _pixels)
				{
					_pixels = pixels;
					ClearPixelBuffersLocked();
					_spec.Pixels = _pixels;
					_spec.resetPixelBuffers();
					_hasData = false;
					_nextReadUtcTicks = 0L;
				}
			}
		}

		public void SetFrameRate(int frameRate)
		{
			lock (_syncRoot)
			{
				if (!_shutdown && frameRate != _frameRate)
				{
					_frameRate = frameRate;
					_spec.FrameRate = _frameRate;
					_spec.resetPixelBuffers();
					_hasData = false;
					_nextReadUtcTicks = 0L;
				}
			}
		}

		public void SetFFTSize(int fftSize)
		{
			lock (_syncRoot)
			{
				if (!_shutdown && fftSize != _fftSize)
				{
					_fftSize = fftSize;
					RefreshLocked();
				}
			}
		}

		public void SetSampleRate(int sampleRate)
		{
			lock (_syncRoot)
			{
				if (!_shutdown && sampleRate != _configuredSampleRate)
				{
					_configuredSampleRate = sampleRate;
					_blockSize = ResolveDefaultBlockSize(_configuredSampleRate);
					RefreshLocked();
				}
			}
		}

		public void SetZoomSlider(double zoomSlider)
		{
			lock (_syncRoot)
			{
				if (!_shutdown && !(Math.Abs(_zoomSlider - zoomSlider) < 1E-06))
				{
					_zoomSlider = zoomSlider;
					_spec.ZoomSlider = _zoomSlider;
					_spec.resetPixelBuffers();
					ClearPixelBuffersLocked();
					_hasData = false;
					_nextReadUtcTicks = 0L;
				}
			}
		}

		public void SetPanSlider(double panSlider)
		{
			lock (_syncRoot)
			{
				if (!_shutdown && !(Math.Abs(_panSlider - panSlider) < 1E-06))
				{
					_panSlider = panSlider;
					_spec.PanSlider = _panSlider;
					_spec.resetPixelBuffers();
					ClearPixelBuffersLocked();
					_hasData = false;
					_nextReadUtcTicks = 0L;
				}
			}
		}

		public void SetEnabled(bool enabled)
		{
			lock (_syncRoot)
			{
				if (!_shutdown && enabled != _enabled)
				{
					_enabled = enabled;
					cmaster.RunAnalyzer(_displayId, _enabled ? 1 : 0);
					_nextReadUtcTicks = 0L;
				}
			}
		}

		public void SyncFrequencies()
		{
			lock (_syncRoot)
			{
				if (!_shutdown)
				{
					_spec.resetPixelBuffers();
					ClearPixelBuffersLocked();
					_hasData = false;
					_nextReadUtcTicks = 0L;
				}
			}
		}

		public void Refresh()
		{
			lock (_syncRoot)
			{
				if (!_shutdown)
				{
					RefreshLocked();
				}
			}
		}

		public void ResetBuffers()
		{
			lock (_syncRoot)
			{
				if (!_shutdown)
				{
					_spec.resetPixelBuffers();
					ClearPixelBuffersLocked();
					_hasData = false;
					_nextReadUtcTicks = 0L;
				}
			}
		}

		public void ClearData()
		{
			lock (_syncRoot)
			{
				ClearPixelBuffersLocked();
				_hasData = false;
				_nextReadUtcTicks = 0L;
			}
		}

		public unsafe bool ProcessFrame(long nowUtcTicks)
		{
			lock (_syncRoot)
			{
				if (_shutdown || !_enabled)
				{
					return false;
				}
				if (nowUtcTicks < _nextReadUtcTicks)
				{
					return false;
				}
				_nextReadUtcTicks = nowUtcTicks + 10000000L / (long)Math.Max(1, _frameRate);
				int flag = 0;
				fixed (float* pix = &_workingPixels[0])
				{
					SpecHPSDRDLL.GetPixels(_displayId, 0, pix, ref flag);
				}
				if (flag != 1)
				{
					return false;
				}
				float pixelOffset = GetPixelOffset();
				if (pixelOffset != 0f)
				{
					for (int i = 0; i < _pixels; i++)
					{
						_workingPixels[i] += pixelOffset;
					}
				}
				float[] latestPixels = _latestPixels;
				_latestPixels = _workingPixels;
				_workingPixels = latestPixels;
				_hasData = true;
				_dataIndex++;
				if (_dataIndex >= 1000000000)
				{
					_dataIndex = 1;
				}
				return true;
			}
		}

		public bool TryGetLatestPixels(out float[] pixels, out int dataIndex)
		{
			lock (_syncRoot)
			{
				pixels = new float[_pixels];
				Buffer.BlockCopy(_latestPixels, 0, pixels, 0, _pixels * 4);
				dataIndex = _dataIndex;
				return _hasData;
			}
		}

		public bool TryCopyLatestPixels(float[] destination, out int pixelCount, out int dataIndex)
		{
			if (destination == null)
			{
				throw new ArgumentNullException("destination");
			}
			lock (_syncRoot)
			{
				pixelCount = _pixels;
				dataIndex = _dataIndex;
				if (destination.Length < _pixels)
				{
					return false;
				}
				Buffer.BlockCopy(_latestPixels, 0, destination, 0, _pixels * 4);
				return _hasData;
			}
		}

		public bool TryGetViewport(out double zoomSlider, out double panSlider)
		{
			lock (_syncRoot)
			{
				zoomSlider = _zoomSlider;
				panSlider = _panSlider;
				return !_shutdown;
			}
		}

		public bool TryGetSampleRate(out int sampleRate)
		{
			lock (_syncRoot)
			{
				sampleRate = _configuredSampleRate;
				return !_shutdown;
			}
		}

		public bool TryGetFFTSize(out int fftSize)
		{
			lock (_syncRoot)
			{
				fftSize = _fftSize;
				return !_shutdown;
			}
		}

		public void Shutdown()
		{
			lock (_syncRoot)
			{
				if (!_shutdown)
				{
					_shutdown = true;
					_enabled = false;
					try
					{
						cmaster.RunAnalyzer(_displayId, 0);
					}
					catch
					{
					}
					try
					{
						cmaster.FreeAnalyzer(_displayId);
					}
					catch
					{
					}
					ReturnPixelBuffer(ref _latestPixels);
					ReturnPixelBuffer(ref _workingPixels);
				}
			}
		}

		private void RefreshLocked()
		{
			_spec.Update = false;
			_spec.PixelOut = 1;
			_spec.IgnoreFrequencyOffset = true;
			_spec.FrameRate = _frameRate;
			_spec.Pixels = _pixels;
			_spec.DetTypePan = _detectorTypePan;
			_spec.AvTau = _averageTau;
			_spec.FFTSize = Math.Min(_fftSize, _maxFftSize);
			_spec.BlockSize = _blockSize;
			_spec.SampleRate = _configuredSampleRate;
			_spec.WindowType = _windowType;
			_spec.AverageMode = _averageMode;
			_spec.AverageOn = _averageOn;
			_spec.PeakOn = _peakOn;
			_spec.NormOneHzPan = _normOneHzPan;
			_spec.ZoomSlider = _zoomSlider;
			_spec.PanSlider = _panSlider;
			_spec.Update = true;
			_spec.initAnalyzer();
			_spec.resetPixelBuffers();
			_hasData = false;
			_nextReadUtcTicks = 0L;
			ClearPixelBuffersLocked();
		}

		private static int ResolveDefaultBlockSize(int sampleRate)
		{
			int buffSize = cmaster.GetBuffSize(sampleRate);
			if (buffSize <= 0)
			{
				return 2048;
			}
			return buffSize;
		}

		private float GetPixelOffset()
		{
			switch (_sourceType)
			{
			case SpectrumSourceType.Transmitter:
				return Display.TXDisplayCalOffset;
			case SpectrumSourceType.Receiver:
				if (_sourceId != 1)
				{
					return Display.RX1OffsetWithDup;
				}
				return Display.RX2OffsetWithDup;
			default:
				return 0f;
			}
		}

		private void EnsurePixelBuffersLocked(int pixels)
		{
			if (_latestPixels == null || _latestPixels.Length < pixels)
			{
				ReturnPixelBuffer(ref _latestPixels);
				_latestPixels = RentPixelBuffer(pixels);
			}
			if (_workingPixels == null || _workingPixels.Length < pixels)
			{
				ReturnPixelBuffer(ref _workingPixels);
				_workingPixels = RentPixelBuffer(pixels);
			}
		}

		private void ClearPixelBuffersLocked()
		{
			EnsurePixelBuffersLocked(_pixels);
			FillPixelBuffer(_latestPixels, _pixels);
			FillPixelBuffer(_workingPixels, _pixels);
		}

		private static float[] RentPixelBuffer(int pixels)
		{
			return PixelArrayPool.Rent(pixels);
		}

		private static void ReturnPixelBuffer(ref float[] buffer)
		{
			if (buffer != null)
			{
				PixelArrayPool.Return(buffer);
				buffer = null;
			}
		}

		private static void FillPixelBuffer(float[] data, int pixels)
		{
			for (int i = 0; i < pixels; i++)
			{
				data[i] = -200f;
			}
		}
	}

	private sealed class SpectrumTestForm : Form
	{
		private readonly clsSpectrumProcessor _processor;

		private readonly SpectrumSourceType _sourceType;

		private readonly int _sourceId;

		private readonly float? _minDbm;

		private readonly float? _maxDbm;

		private readonly SpectrumGraphPanel _graphPanel;

		private readonly System.Windows.Forms.Timer _refreshTimer;

		public SpectrumTestForm(clsSpectrumProcessor processor, SpectrumSourceType sourceType, int sourceId, float? minDbm, float? maxDbm)
		{
			_processor = processor;
			_sourceType = sourceType;
			_sourceId = sourceId;
			_minDbm = minDbm;
			_maxDbm = maxDbm;
			Text = BuildTitle(sourceType, sourceId, _minDbm, _maxDbm);
			base.StartPosition = FormStartPosition.CenterScreen;
			base.Size = new Size(900, 450);
			MinimumSize = new Size(320, 200);
			BackColor = Color.FromArgb(18, 18, 18);
			_graphPanel = new SpectrumGraphPanel(_processor, _sourceType, _sourceId, _minDbm, _maxDbm)
			{
				Dock = DockStyle.Fill
			};
			base.Controls.Add(_graphPanel);
			_refreshTimer = new System.Windows.Forms.Timer
			{
				Interval = 50
			};
			_refreshTimer.Tick += RefreshTimer_Tick;
			_refreshTimer.Start();
			base.FormClosed += SpectrumTestForm_FormClosed;
		}

		private static string BuildTitle(SpectrumSourceType sourceType, int sourceId, float? minDbm, float? maxDbm)
		{
			string text = string.Empty;
			if (minDbm.HasValue && maxDbm.HasValue && maxDbm.Value > minDbm.Value)
			{
				text = " [" + minDbm.Value.ToString("F0") + " to " + maxDbm.Value.ToString("F0") + " dBm]";
			}
			if (sourceType == SpectrumSourceType.Receiver)
			{
				return "Spectrum Test - RX" + (sourceId + 1) + text;
			}
			return "Spectrum Test - TX" + sourceId + text;
		}

		private void RefreshTimer_Tick(object sender, EventArgs e)
		{
			_graphPanel.Invalidate();
		}

		private void SpectrumTestForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			_refreshTimer.Stop();
			_refreshTimer.Dispose();
		}
	}

	private sealed class SpectrumGraphPanel : Panel
	{
		private readonly clsSpectrumProcessor _processor;

		private readonly SpectrumSourceType _sourceType;

		private readonly int _sourceId;

		private readonly float? _fixedMinDbm;

		private readonly float? _fixedMaxDbm;

		private readonly Pen _tracePen;

		private readonly Pen _gridPen;

		private readonly Brush _textBrush;

		private readonly Brush _backgroundBrush;

		private readonly Font _font;

		private float[] _pixelBuffer;

		private PointF[] _pointBuffer;

		public SpectrumGraphPanel(clsSpectrumProcessor processor, SpectrumSourceType sourceType, int sourceId, float? fixedMinDbm, float? fixedMaxDbm)
		{
			_processor = processor;
			_sourceType = sourceType;
			_sourceId = sourceId;
			_fixedMinDbm = fixedMinDbm;
			_fixedMaxDbm = fixedMaxDbm;
			DoubleBuffered = true;
			base.ResizeRedraw = true;
			_tracePen = new Pen(Color.LimeGreen, 1.5f);
			_gridPen = new Pen(Color.FromArgb(45, 90, 90, 90), 1f);
			_textBrush = Brushes.Gainsboro;
			_backgroundBrush = new SolidBrush(Color.FromArgb(18, 18, 18));
			_font = new Font("Segoe UI", 9f, FontStyle.Regular);
			_pixelBuffer = new float[1024];
			_pointBuffer = new PointF[1024];
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_tracePen.Dispose();
				_gridPen.Dispose();
				_backgroundBrush.Dispose();
				_font.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Graphics graphics = e.Graphics;
			graphics.Clear(Color.FromArgb(18, 18, 18));
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Inflate(-12, -12);
			if (clientRectangle.Width < 10 || clientRectangle.Height < 10)
			{
				return;
			}
			DrawGrid(graphics, clientRectangle);
			float[] pixels = _pixelBuffer;
			if (!TryCopyPixels(out pixels, out var pixelCount, out var dataIndex) || pixels == null || pixelCount < 2)
			{
				graphics.DrawString("Waiting for pixel data...", _font, _textBrush, clientRectangle.Left, clientRectangle.Top);
				return;
			}
			float num = float.MaxValue;
			float num2 = float.MinValue;
			for (int i = 0; i < pixelCount; i++)
			{
				if (pixels[i] < num)
				{
					num = pixels[i];
				}
				if (pixels[i] > num2)
				{
					num2 = pixels[i];
				}
			}
			if (num == float.MaxValue || num2 == float.MinValue)
			{
				graphics.DrawString("No valid dBm data.", _font, _textBrush, clientRectangle.Left, clientRectangle.Top);
				return;
			}
			float num3 = num;
			float num4 = num2;
			bool flag = _fixedMinDbm.HasValue && _fixedMaxDbm.HasValue && _fixedMaxDbm.Value > _fixedMinDbm.Value;
			if (flag)
			{
				num3 = _fixedMinDbm.Value;
				num4 = _fixedMaxDbm.Value;
			}
			if (Math.Abs(num4 - num3) < 0.001f)
			{
				num4++;
				num3--;
			}
			EnsurePointBuffer(pixelCount);
			PointF[] pointBuffer = _pointBuffer;
			float num5 = (float)clientRectangle.Width - 1f;
			float num6 = (float)clientRectangle.Height - 1f;
			float num7 = num4 - num3;
			for (int j = 0; j < pixelCount; j++)
			{
				float num8 = (float)clientRectangle.Left + num5 * (float)j / ((float)pixelCount - 1f);
				float num9 = (pixels[j] - num3) / num7;
				if (num9 < 0f)
				{
					num9 = 0f;
				}
				if (num9 > 1f)
				{
					num9 = 1f;
				}
				float num10 = (float)clientRectangle.Bottom - num9 * num6;
				pointBuffer[j] = new PointF(num8, num10);
			}
			graphics.DrawLines(_tracePen, pointBuffer);
			string text = ((_sourceType == SpectrumSourceType.Receiver) ? ("RX" + (_sourceId + 1)) : ("TX" + _sourceId));
			bool flag2 = _processor.TryGetViewport(_sourceType, _sourceId, out var zoomSlider, out var panSlider);
			bool flag3 = _processor.TryGetSampleRate(_sourceType, _sourceId, out var sampleRate);
			bool flag4 = _processor.TryGetFFTSize(_sourceType, _sourceId, out var fftSize);
			string text2 = text + "  Pixels=" + pixelCount + "  Frame=" + dataIndex + "  Min=" + num.ToString("F1") + " dBm  Max=" + num2.ToString("F1") + " dBm";
			if (flag2)
			{
				text2 = text2 + "  Zoom=" + zoomSlider.ToString("F3") + "  Pan=" + panSlider.ToString("F3");
			}
			if (flag3)
			{
				text2 = text2 + "  SR=" + sampleRate;
			}
			if (flag4)
			{
				text2 = text2 + "  FFT=" + fftSize;
			}
			if (flag)
			{
				text2 = text2 + "  Scale=" + num3.ToString("F1") + " to " + num4.ToString("F1") + " dBm";
			}
			SizeF sizeF = graphics.MeasureString(text2, _font);
			graphics.FillRectangle(rect: new RectangleF(clientRectangle.Left, clientRectangle.Top, Math.Min(sizeF.Width + 8f, clientRectangle.Width), sizeF.Height + 4f), brush: _backgroundBrush);
			graphics.DrawString(text2, _font, _textBrush, clientRectangle.Left + 2, clientRectangle.Top + 2);
		}

		private bool TryCopyPixels(out float[] pixels, out int pixelCount, out int dataIndex)
		{
			if (_pixelBuffer == null)
			{
				_pixelBuffer = new float[1024];
			}
			bool result = TryCopyPixelsWithBuffer(_pixelBuffer, out pixelCount, out dataIndex);
			if (pixelCount > _pixelBuffer.Length)
			{
				_pixelBuffer = new float[pixelCount];
				result = TryCopyPixelsWithBuffer(_pixelBuffer, out pixelCount, out dataIndex);
			}
			pixels = _pixelBuffer;
			return result;
		}

		private bool TryCopyPixelsWithBuffer(float[] destination, out int pixelCount, out int dataIndex)
		{
			if (_sourceType != SpectrumSourceType.Receiver)
			{
				return _processor.TryCopyTransmitterPixels(_sourceId, destination, out pixelCount, out dataIndex);
			}
			return _processor.TryCopyReceiverPixels(_sourceId, destination, out pixelCount, out dataIndex);
		}

		private void EnsurePointBuffer(int pixelCount)
		{
			if (_pointBuffer == null || _pointBuffer.Length != pixelCount)
			{
				_pointBuffer = new PointF[pixelCount];
			}
		}

		private void DrawGrid(Graphics g, Rectangle plotRect)
		{
			for (int i = 0; i <= 8; i++)
			{
				float num = (float)plotRect.Left + (float)(plotRect.Width * i) / 8f;
				g.DrawLine(_gridPen, num, plotRect.Top, num, plotRect.Bottom);
			}
			for (int j = 0; j <= 6; j++)
			{
				float num2 = (float)plotRect.Top + (float)(plotRect.Height * j) / 6f;
				g.DrawLine(_gridPen, plotRect.Left, num2, plotRect.Right, num2);
			}
		}
	}

	private const int DefaultPixels = 1024;

	private const int DefaultFrameRate = 15;

	private const int DefaultMaxFftSize = 262144;

	private const int DefaultSampleRate = 192000;

	private const int DefaultFftSize = 4096;

	private const int DefaultWindowType = 4;

	private const int DefaultDetectorTypePan = 0;

	private const int DefaultAverageMode = 0;

	private const double DefaultAverageTau = 0.12;

	private const float EmptyPixelValue = -200f;

	private const int DataIndexWrap = 1000000000;

	private static readonly ArrayPool<float> PixelArrayPool = ArrayPool<float>.Shared;

	private readonly Console _console;

	private readonly object _endpointsLock = new object();

	private readonly Dictionary<string, SpectrumEndpoint> _endpoints;

	private readonly Thread _workerThread;

	private volatile SpectrumEndpoint[] _endpointSnapshot;

	private volatile bool _workerRunning;

	private bool _disposed;

	public clsSpectrumProcessor(Console console)
	{
		if (console == null)
		{
			throw new ArgumentNullException("console");
		}
		_console = console;
		_endpoints = new Dictionary<string, SpectrumEndpoint>(StringComparer.OrdinalIgnoreCase);
		_endpointSnapshot = Array.Empty<SpectrumEndpoint>();
		_workerRunning = true;
		SubscribeDelegates();
		_workerThread = new Thread(RunWorker)
		{
			Name = "Spectrum Processor Thread",
			IsBackground = true,
			Priority = ThreadPriority.BelowNormal
		};
		_workerThread.Start();
	}

	public bool AddReceiver(int receiverId)
	{
		return AddReceiver(receiverId, 1024, 15, 4096);
	}

	public bool AddReceiver(int receiverId, int pixels, int frameRate, int fftSize)
	{
		return AddSource(SpectrumSourceType.Receiver, receiverId, pixels, frameRate, fftSize);
	}

	public bool AddTransmitter(int transmitterId)
	{
		return AddTransmitter(transmitterId, 1024, 15, 4096);
	}

	public bool AddTransmitter(int transmitterId, int pixels, int frameRate, int fftSize)
	{
		return AddSource(SpectrumSourceType.Transmitter, transmitterId, pixels, frameRate, fftSize);
	}

	private bool AddSource(SpectrumSourceType sourceType, int sourceId, int pixels, int frameRate, int fftSize)
	{
		ThrowIfDisposed();
		ValidateSourceId(sourceId);
		pixels = ClampInt(pixels, 64, 32768);
		frameRate = ClampInt(frameRate, 1, 120);
		fftSize = NormalizeFftSize(fftSize);
		string key = MakeSourceKey(sourceType, sourceId);
		SpectrumEndpoint spectrumEndpoint = new SpectrumEndpoint(sourceType, sourceId, pixels, frameRate, fftSize);
		lock (_endpointsLock)
		{
			if (_endpoints.ContainsKey(key))
			{
				spectrumEndpoint.Shutdown();
				return false;
			}
			_endpoints.Add(key, spectrumEndpoint);
			UpdateEndpointSnapshotLocked();
		}
		return true;
	}

	public bool RemoveReceiver(int receiverId)
	{
		return RemoveSource(SpectrumSourceType.Receiver, receiverId);
	}

	public bool RemoveTransmitter(int transmitterId)
	{
		return RemoveSource(SpectrumSourceType.Transmitter, transmitterId);
	}

	private bool RemoveSource(SpectrumSourceType sourceType, int sourceId)
	{
		if (!TryDetachEndpoint(sourceType, sourceId, out var endpoint))
		{
			return false;
		}
		endpoint.Shutdown();
		return true;
	}

	public void Clear()
	{
		SpectrumEndpoint[] endpointSnapshot;
		lock (_endpointsLock)
		{
			endpointSnapshot = _endpointSnapshot;
			_endpoints.Clear();
			_endpointSnapshot = Array.Empty<SpectrumEndpoint>();
		}
		for (int i = 0; i < endpointSnapshot.Length; i++)
		{
			endpointSnapshot[i].Shutdown();
		}
	}

	private bool ContainsSource(SpectrumSourceType sourceType, int sourceId)
	{
		ValidateSourceId(sourceId);
		lock (_endpointsLock)
		{
			return _endpoints.ContainsKey(MakeSourceKey(sourceType, sourceId));
		}
	}

	public bool SetReceiverPixelResolution(int receiverId, int pixels)
	{
		return SetPixelResolution(SpectrumSourceType.Receiver, receiverId, pixels);
	}

	public bool SetTransmitterPixelResolution(int transmitterId, int pixels)
	{
		return SetPixelResolution(SpectrumSourceType.Transmitter, transmitterId, pixels);
	}

	private bool SetPixelResolution(SpectrumSourceType sourceType, int sourceId, int pixels)
	{
		if (!TryGetEndpoint(sourceType, sourceId, out var endpoint))
		{
			return false;
		}
		endpoint.SetPixels(ClampInt(pixels, 64, 32768));
		return true;
	}

	public bool SetReceiverFrameRate(int receiverId, int frameRate)
	{
		return SetFrameRate(SpectrumSourceType.Receiver, receiverId, frameRate);
	}

	public bool SetTransmitterFrameRate(int transmitterId, int frameRate)
	{
		return SetFrameRate(SpectrumSourceType.Transmitter, transmitterId, frameRate);
	}

	private bool SetFrameRate(SpectrumSourceType sourceType, int sourceId, int frameRate)
	{
		if (!TryGetEndpoint(sourceType, sourceId, out var endpoint))
		{
			return false;
		}
		endpoint.SetFrameRate(ClampInt(frameRate, 1, 120));
		return true;
	}

	public bool SetReceiverFFTSize(int receiverId, int fftSize)
	{
		return SetFFTSize(SpectrumSourceType.Receiver, receiverId, fftSize);
	}

	public bool SetTransmitterFFTSize(int transmitterId, int fftSize)
	{
		return SetFFTSize(SpectrumSourceType.Transmitter, transmitterId, fftSize);
	}

	private bool SetFFTSize(SpectrumSourceType sourceType, int sourceId, int fftSize)
	{
		if (!TryGetEndpoint(sourceType, sourceId, out var endpoint))
		{
			return false;
		}
		endpoint.SetFFTSize(NormalizeFftSize(fftSize));
		return true;
	}

	public bool SetReceiverSampleRate(int receiverId, int sampleRate)
	{
		return SetSampleRate(SpectrumSourceType.Receiver, receiverId, sampleRate);
	}

	public bool SetTransmitterSampleRate(int transmitterId, int sampleRate)
	{
		return SetSampleRate(SpectrumSourceType.Transmitter, transmitterId, sampleRate);
	}

	private bool SetSampleRate(SpectrumSourceType sourceType, int sourceId, int sampleRate)
	{
		if (!TryGetEndpoint(sourceType, sourceId, out var endpoint))
		{
			return false;
		}
		endpoint.SetSampleRate(ValidateSampleRate(sampleRate));
		return true;
	}

	public bool SetReceiverZoomSlider(int receiverId, double zoomSlider)
	{
		return SetZoomSlider(SpectrumSourceType.Receiver, receiverId, zoomSlider);
	}

	public bool SetTransmitterZoomSlider(int transmitterId, double zoomSlider)
	{
		return SetZoomSlider(SpectrumSourceType.Transmitter, transmitterId, zoomSlider);
	}

	private bool SetZoomSlider(SpectrumSourceType sourceType, int sourceId, double zoomSlider)
	{
		if (!TryGetEndpoint(sourceType, sourceId, out var endpoint))
		{
			return false;
		}
		endpoint.SetZoomSlider(ClampZoomSlider(zoomSlider));
		return true;
	}

	public bool SetReceiverPanSlider(int receiverId, double panSlider)
	{
		return SetPanSlider(SpectrumSourceType.Receiver, receiverId, panSlider);
	}

	public bool SetTransmitterPanSlider(int transmitterId, double panSlider)
	{
		return SetPanSlider(SpectrumSourceType.Transmitter, transmitterId, panSlider);
	}

	private bool SetPanSlider(SpectrumSourceType sourceType, int sourceId, double panSlider)
	{
		if (!TryGetEndpoint(sourceType, sourceId, out var endpoint))
		{
			return false;
		}
		endpoint.SetPanSlider(ClampPanSlider(panSlider));
		return true;
	}

	public bool CentreReceiverPan(int receiverId)
	{
		return CentrePan(SpectrumSourceType.Receiver, receiverId);
	}

	public bool CentreTransmitterPan(int transmitterId)
	{
		return CentrePan(SpectrumSourceType.Transmitter, transmitterId);
	}

	private bool CentrePan(SpectrumSourceType sourceType, int sourceId)
	{
		return SetPanSlider(sourceType, sourceId, 0.5);
	}

	public bool SetReceiverEnabled(int receiverId, bool enabled)
	{
		return SetEnabled(SpectrumSourceType.Receiver, receiverId, enabled);
	}

	public bool SetTransmitterEnabled(int transmitterId, bool enabled)
	{
		return SetEnabled(SpectrumSourceType.Transmitter, transmitterId, enabled);
	}

	private bool SetEnabled(SpectrumSourceType sourceType, int sourceId, bool enabled)
	{
		if (!TryGetEndpoint(sourceType, sourceId, out var endpoint))
		{
			return false;
		}
		endpoint.SetEnabled(enabled);
		return true;
	}

	public bool ResetReceiverBuffers(int receiverId)
	{
		return ResetBuffers(SpectrumSourceType.Receiver, receiverId);
	}

	public bool ResetTransmitterBuffers(int transmitterId)
	{
		return ResetBuffers(SpectrumSourceType.Transmitter, transmitterId);
	}

	private bool ResetBuffers(SpectrumSourceType sourceType, int sourceId)
	{
		if (!TryGetEndpoint(sourceType, sourceId, out var endpoint))
		{
			return false;
		}
		endpoint.ResetBuffers();
		return true;
	}

	public bool TryGetReceiverPixels(int receiverId, out float[] pixels, out int dataIndex)
	{
		return TryGetLatestPixels(SpectrumSourceType.Receiver, receiverId, out pixels, out dataIndex);
	}

	public bool TryGetTransmitterPixels(int transmitterId, out float[] pixels, out int dataIndex)
	{
		return TryGetLatestPixels(SpectrumSourceType.Transmitter, transmitterId, out pixels, out dataIndex);
	}

	public bool TryCopyReceiverPixels(int receiverId, float[] destination, out int pixelCount, out int dataIndex)
	{
		return TryCopyLatestPixels(SpectrumSourceType.Receiver, receiverId, destination, out pixelCount, out dataIndex);
	}

	public bool TryCopyTransmitterPixels(int transmitterId, float[] destination, out int pixelCount, out int dataIndex)
	{
		return TryCopyLatestPixels(SpectrumSourceType.Transmitter, transmitterId, destination, out pixelCount, out dataIndex);
	}

	private bool TryGetLatestPixels(SpectrumSourceType sourceType, int sourceId, out float[] pixels, out int dataIndex)
	{
		if (!TryGetEndpoint(sourceType, sourceId, out var endpoint))
		{
			pixels = null;
			dataIndex = 0;
			return false;
		}
		return endpoint.TryGetLatestPixels(out pixels, out dataIndex);
	}

	private bool TryCopyLatestPixels(SpectrumSourceType sourceType, int sourceId, float[] destination, out int pixelCount, out int dataIndex)
	{
		if (!TryGetEndpoint(sourceType, sourceId, out var endpoint))
		{
			pixelCount = 0;
			dataIndex = 0;
			return false;
		}
		return endpoint.TryCopyLatestPixels(destination, out pixelCount, out dataIndex);
	}

	private bool TryGetViewport(SpectrumSourceType sourceType, int sourceId, out double zoomSlider, out double panSlider)
	{
		if (!TryGetEndpoint(sourceType, sourceId, out var endpoint))
		{
			zoomSlider = 0.0;
			panSlider = 0.5;
			return false;
		}
		return endpoint.TryGetViewport(out zoomSlider, out panSlider);
	}

	private bool TryGetSampleRate(SpectrumSourceType sourceType, int sourceId, out int sampleRate)
	{
		if (!TryGetEndpoint(sourceType, sourceId, out var endpoint))
		{
			sampleRate = 0;
			return false;
		}
		return endpoint.TryGetSampleRate(out sampleRate);
	}

	private bool TryGetFFTSize(SpectrumSourceType sourceType, int sourceId, out int fftSize)
	{
		if (!TryGetEndpoint(sourceType, sourceId, out var endpoint))
		{
			fftSize = 0;
			return false;
		}
		return endpoint.TryGetFFTSize(out fftSize);
	}

	public Form ShowReceiverTestForm(int receiverId, float? min_dBm = null, float? max_dBm = null)
	{
		return ShowTestForm(SpectrumSourceType.Receiver, receiverId, min_dBm, max_dBm);
	}

	public Form ShowTransmitterTestForm(int transmitterId, float? min_dBm = null, float? max_dBm = null)
	{
		return ShowTestForm(SpectrumSourceType.Transmitter, transmitterId, min_dBm, max_dBm);
	}

	private Form ShowTestForm(SpectrumSourceType sourceType, int sourceId, float? min_dBm = null, float? max_dBm = null)
	{
		ThrowIfDisposed();
		ValidateSourceId(sourceId);
		if (!ContainsSource(sourceType, sourceId))
		{
			AddSource(sourceType, sourceId, 1024, 15, 4096);
		}
		SpectrumTestForm spectrumTestForm = new SpectrumTestForm(this, sourceType, sourceId, min_dBm, max_dBm);
		spectrumTestForm.Show();
		return spectrumTestForm;
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			_workerRunning = false;
			if (_workerThread != null && _workerThread.IsAlive)
			{
				_workerThread.Join(250);
			}
			UnsubscribeDelegates();
			Clear();
		}
	}

	private void RunWorker()
	{
		while (_workerRunning)
		{
			SpectrumEndpoint[] endpointArray = GetEndpointArray();
			long ticks = DateTime.UtcNow.Ticks;
			bool flag = false;
			for (int i = 0; i < endpointArray.Length; i++)
			{
				if (endpointArray[i].ProcessFrame(ticks))
				{
					flag = true;
				}
			}
			Thread.Sleep(flag ? 1 : 10);
		}
	}

	private void SubscribeDelegates()
	{
		Console console = _console;
		console.CentreFrequencyHandlers = (Console.CentreFrequencyChanged)Delegate.Combine(console.CentreFrequencyHandlers, new Console.CentreFrequencyChanged(OnCentreFrequencyChanged));
		Console console2 = _console;
		console2.HWSampleRateChangedHandlers = (Console.HWSampleRateChanged)Delegate.Combine(console2.HWSampleRateChangedHandlers, new Console.HWSampleRateChanged(OnHWSampleRateChanged));
		Console console3 = _console;
		console3.PowerChangeHanders = (Console.PowerChanged)Delegate.Combine(console3.PowerChangeHanders, new Console.PowerChanged(OnPowerChanged));
	}

	private void UnsubscribeDelegates()
	{
		Console console = _console;
		console.CentreFrequencyHandlers = (Console.CentreFrequencyChanged)Delegate.Remove(console.CentreFrequencyHandlers, new Console.CentreFrequencyChanged(OnCentreFrequencyChanged));
		Console console2 = _console;
		console2.HWSampleRateChangedHandlers = (Console.HWSampleRateChanged)Delegate.Remove(console2.HWSampleRateChangedHandlers, new Console.HWSampleRateChanged(OnHWSampleRateChanged));
		Console console3 = _console;
		console3.PowerChangeHanders = (Console.PowerChanged)Delegate.Remove(console3.PowerChangeHanders, new Console.PowerChanged(OnPowerChanged));
	}

	private void OnCentreFrequencyChanged(int rx, double oldFreq, double newFreq, Band band, double offset)
	{
		SpectrumEndpoint[] endpointArray = GetEndpointArray();
		foreach (SpectrumEndpoint spectrumEndpoint in endpointArray)
		{
			if (spectrumEndpoint.SourceType == SpectrumSourceType.Transmitter || spectrumEndpoint.MatchesReceiverEvent(rx))
			{
				spectrumEndpoint.SyncFrequencies();
			}
		}
	}

	private void OnHWSampleRateChanged(int rx, int oldRate, int newRate)
	{
		SpectrumEndpoint[] endpointArray = GetEndpointArray();
		int sampleRate = ValidateSampleRate(newRate);
		for (int i = 0; i < endpointArray.Length; i++)
		{
			if (endpointArray[i].MatchesReceiverEvent(rx))
			{
				endpointArray[i].SetSampleRate(sampleRate);
			}
		}
	}

	private void OnPowerChanged(bool oldPower, bool newPower)
	{
		if (!(!oldPower | newPower))
		{
			SpectrumEndpoint[] endpointArray = GetEndpointArray();
			for (int i = 0; i < endpointArray.Length; i++)
			{
				endpointArray[i].ClearData();
			}
		}
	}

	private SpectrumEndpoint[] GetEndpointArray()
	{
		return _endpointSnapshot;
	}

	private bool TryGetEndpoint(SpectrumSourceType sourceType, int sourceId, out SpectrumEndpoint endpoint)
	{
		endpoint = null;
		if (sourceId < 0)
		{
			return false;
		}
		lock (_endpointsLock)
		{
			return _endpoints.TryGetValue(MakeSourceKey(sourceType, sourceId), out endpoint);
		}
	}

	private bool TryDetachEndpoint(SpectrumSourceType sourceType, int sourceId, out SpectrumEndpoint endpoint)
	{
		endpoint = null;
		if (sourceId < 0)
		{
			return false;
		}
		lock (_endpointsLock)
		{
			string key = MakeSourceKey(sourceType, sourceId);
			if (!_endpoints.TryGetValue(key, out endpoint))
			{
				return false;
			}
			_endpoints.Remove(key);
			UpdateEndpointSnapshotLocked();
			return true;
		}
	}

	private void UpdateEndpointSnapshotLocked()
	{
		if (_endpoints.Count == 0)
		{
			_endpointSnapshot = Array.Empty<SpectrumEndpoint>();
			return;
		}
		SpectrumEndpoint[] array = new SpectrumEndpoint[_endpoints.Count];
		_endpoints.Values.CopyTo(array, 0);
		_endpointSnapshot = array;
	}

	private void ThrowIfDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(GetType().Name);
		}
	}

	private static string MakeSourceKey(SpectrumSourceType sourceType, int sourceId)
	{
		return ((sourceType == SpectrumSourceType.Receiver) ? "R:" : "T:") + sourceId;
	}

	private static void ValidateSourceId(int sourceId)
	{
		if (sourceId < 0)
		{
			throw new ArgumentOutOfRangeException("sourceId", "sourceId must be zero or greater.");
		}
	}

	private static int ClampInt(int value, int minimum, int maximum)
	{
		if (value < minimum)
		{
			return minimum;
		}
		if (value > maximum)
		{
			return maximum;
		}
		return value;
	}

	private static int ValidateSampleRate(int sampleRate)
	{
		if (sampleRate <= 0)
		{
			throw new ArgumentOutOfRangeException("sampleRate", "sampleRate must be greater than zero.");
		}
		return sampleRate;
	}

	private static double ClampZoomSlider(double value)
	{
		if (double.IsNaN(value) || double.IsInfinity(value))
		{
			return 0.0;
		}
		if (value < 0.0)
		{
			return 0.0;
		}
		if (value > 1.0)
		{
			return 1.0;
		}
		return value;
	}

	private static double ClampPanSlider(double value)
	{
		if (double.IsNaN(value) || double.IsInfinity(value))
		{
			return 0.5;
		}
		if (value < 0.0)
		{
			return 0.0;
		}
		if (value > 1.0)
		{
			return 1.0;
		}
		return value;
	}

	private static int NormalizeFftSize(int fftSize)
	{
		int num = ClampInt(fftSize, 4096, 262144);
		int num2 = 4096;
		int num3 = Math.Abs(num - num2);
		for (int num4 = 8192; num4 <= 262144; num4 <<= 1)
		{
			int num5 = Math.Abs(num - num4);
			if (num5 < num3)
			{
				num2 = num4;
				num3 = num5;
			}
		}
		return num2;
	}
}
