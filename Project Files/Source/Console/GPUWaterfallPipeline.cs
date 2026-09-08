using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace Thetis;

public class GPUWaterfallPipeline : IDisposable
{
	public const int MaxFftSize = 262144;

	private const int MaxDisplayWidth = 8192;

	private SharpDX.Direct3D11.Device _device;

	private DeviceContext _context;

	private int _fftSize;

	private int _displayWidth;

	private float _sampleRate;

	private uint _bits;

	private SharpDX.Direct3D11.Buffer _iqBuffer;

	private ShaderResourceView _iqSRV;

	private SharpDX.Direct3D11.Buffer _iqStagingBuffer;

	private SharpDX.Direct3D11.Buffer _windowBuffer;

	private ShaderResourceView _windowSRV;

	private SharpDX.Direct3D11.Buffer _fftBufferA;

	private UnorderedAccessView _fftAUAV;

	private SharpDX.Direct3D11.Buffer _fftBufferB;

	private UnorderedAccessView _fftBUAV;

	private SharpDX.Direct3D11.Buffer _magBuffer;

	private UnorderedAccessView _magUAV;

	private ShaderResourceView _magSRV;

	private SharpDX.Direct3D11.Buffer _stagingMagBuffer;

	private SharpDX.Direct3D11.Buffer _constantBuffer;

	private ComputeShader _bitReverseCS;

	private ComputeShader _fftStageABCS;

	private ComputeShader _fftStageBACS;

	private ComputeShader _magnitudeCS;

	private float[] _window;

	private float[] _iqUpload;

	private float[] _magReadback;

	private float _windowPower = 1f;

	private float _windowCoherentGain = 1f;

	private bool _initialized;

	private GPUWaterfallMagnitudeMode _magnitudeMode = GPUWaterfallMagnitudeMode.PeakHoldPower;

	private int _lanczosWindow = 3;

	private GPUWaterfallResamplingMode _resamplingMode = GPUWaterfallResamplingMode.Quality;

	private GPUWaterfallWindowType _windowType = GPUWaterfallWindowType.Nuttall;

	private double _kaiserBeta = 6.0;

	private bool _pendingReadback;

	private int _firstBin;

	private int _binCount;

	private float _displayLowFreq;

	private float _displayHighFreq;

	public const int NarrowWidth = 1024;

	private SharpDX.Direct3D11.Buffer _magBufferNarrow;

	private UnorderedAccessView _magUAVNarrow;

	private SharpDX.Direct3D11.Buffer _stagingMagBufferNarrow;

	private float[] _magReadbackNarrow;

	private bool _narrowEnabled;

	private float _narrowLow;

	private float _narrowHigh;

	private bool _narrowRowReady;

	private bool _narrowStaged;

	private bool _doNotWaitBroken;

	private int _readbackFailStreak;

	private const int READBACK_FAIL_WATCHDOG = 120;

	private int _readbackErrorSkip;

	public bool IsInitialized => _initialized;

	public int FFTSize => _fftSize;

	public int DisplayWidth => _displayWidth;

	public float SampleRate => _sampleRate;

	public ShaderResourceView MagSpectrumView => _magSRV;

	public GPUWaterfallMagnitudeMode MagnitudeMode
	{
		get
		{
			return _magnitudeMode;
		}
		set
		{
			if (_magnitudeMode != value)
			{
				_magnitudeMode = value;
				if (_initialized)
				{
					LogGPU($"Magnitude mode changed to {value}");
				}
			}
		}
	}

	public int LanczosWindow
	{
		get
		{
			return _lanczosWindow;
		}
		set
		{
			if (_lanczosWindow != value)
			{
				_lanczosWindow = value;
				if (_initialized)
				{
					LogGPU($"Lanczos window changed to {value}");
				}
			}
		}
	}

	public GPUWaterfallResamplingMode ResamplingMode
	{
		get
		{
			return _resamplingMode;
		}
		set
		{
			if (_resamplingMode != value)
			{
				_resamplingMode = value;
				if (_initialized)
				{
					LogGPU($"Resampling mode changed to {value}");
				}
			}
		}
	}

	public GPUWaterfallWindowType WindowType
	{
		get
		{
			return _windowType;
		}
		set
		{
			if (_windowType != value && _initialized)
			{
				_windowType = value;
				ComputeWindow();
				UploadWindow();
				LogGPU($"Window changed to {value} (power={_windowPower})");
			}
		}
	}

	public double KaiserBeta
	{
		get
		{
			return _kaiserBeta;
		}
		set
		{
			double num = value;
			if (num < 0.0)
			{
				num = 0.0;
			}
			if (num > 20.0)
			{
				num = 20.0;
			}
			if (!(Math.Abs(_kaiserBeta - num) < 0.01) && _initialized)
			{
				_kaiserBeta = num;
				if (_windowType == GPUWaterfallWindowType.Kaiser)
				{
					ComputeWindow();
					UploadWindow();
					LogGPU($"Kaiser beta changed to {num:F1} (power={_windowPower})");
				}
			}
		}
	}

	public float[] NarrowRow
	{
		get
		{
			if (!_narrowEnabled || !_narrowRowReady)
			{
				return null;
			}
			return _magReadbackNarrow;
		}
	}

	public GPUWaterfallPipeline(SharpDX.Direct3D11.Device device, int fftSize, int displayWidth, float sampleRate)
	{
		_device = device ?? throw new ArgumentNullException("device");
		_context = device.ImmediateContext;
		Resize(fftSize, displayWidth, sampleRate);
	}

	public bool Resize(int fftSize, int displayWidth, float sampleRate)
	{
		if (_device == null)
		{
			return false;
		}
		_pendingReadback = false;
		_narrowRowReady = false;
		if (!IsPowerOfTwo(fftSize) || fftSize < 1024 || fftSize > 262144)
		{
			LogGPU($"Resize: unsupported FFT size {fftSize}");
			_initialized = false;
			return false;
		}
		if (displayWidth <= 0 || displayWidth > 8192)
		{
			LogGPU($"Resize: unsupported display width {displayWidth} (max {8192})");
			_initialized = false;
			return false;
		}
		bool flag = _fftSize != fftSize || _displayWidth != displayWidth || !_initialized;
		bool flag2 = Math.Abs(_sampleRate - sampleRate) >= 1f;
		if (!flag && !flag2)
		{
			return true;
		}
		_doNotWaitBroken = false;
		_fftSize = fftSize;
		_displayWidth = displayWidth;
		_sampleRate = sampleRate;
		_bits = (uint)Log2(fftSize);
		try
		{
			if (!LoadShaders())
			{
				return false;
			}
			if (_iqBuffer == null && !CreateBuffers())
			{
				return false;
			}
			if (flag && !CreateViews(fftSize, displayWidth))
			{
				return false;
			}
			ComputeWindow();
			UploadWindow();
			_initialized = true;
			LogGPU($"Pipeline initialized: FFT={_fftSize}, displayWidth={_displayWidth}, SR={_sampleRate}");
			return true;
		}
		catch (Exception arg)
		{
			LogGPU($"GPUWaterfallPipeline.Resize failed: {arg}");
			DisposeResources();
			_initialized = false;
			return false;
		}
	}

	public float[] Process(float[] i, float[] q, int count)
	{
		if (!_initialized || i == null || q == null || count <= 0)
		{
			return null;
		}
		float[] array = null;
		bool flag = false;
		if (_pendingReadback)
		{
			int num = ReadbackMag();
			if (num > 0)
			{
				_readbackFailStreak = 0;
				_pendingReadback = false;
				array = _magReadback;
				flag = num == 2;
				if (_narrowEnabled && _narrowStaged && _stagingMagBufferNarrow != null)
				{
					try
					{
						_context.MapSubresource(_stagingMagBufferNarrow, MapMode.Read, SharpDX.Direct3D11.MapFlags.None, out var stream);
						if (stream != null)
						{
							try
							{
								if (_magReadbackNarrow == null || _magReadbackNarrow.Length < 1024)
								{
									_magReadbackNarrow = new float[1024];
								}
								stream.ReadRange(_magReadbackNarrow, 0, 1024);
								_narrowRowReady = true;
								_narrowStaged = false;
							}
							finally
							{
								try
								{
									_context.UnmapSubresource(_stagingMagBufferNarrow, 0);
								}
								catch
								{
								}
							}
						}
					}
					catch
					{
					}
				}
			}
			else if (++_readbackFailStreak == 120)
			{
				LogGPU($"ReadbackMag: {120} consecutive failed readbacks — forcing pipeline rebuild");
				_readbackFailStreak = 0;
				_initialized = false;
				Resize(_fftSize, _displayWidth, _sampleRate);
			}
		}
		if (count < _fftSize)
		{
			if (array == null)
			{
				LogGPU($"Process: not enough samples ({count} < {_fftSize})");
			}
		}
		else if (!flag)
		{
			UploadIQ(i, q, _fftSize);
			RunFFT();
			_context.CopyResource(_magBuffer, _stagingMagBuffer);
			if (_narrowEnabled && _magBufferNarrow != null && _stagingMagBufferNarrow != null)
			{
				_context.CopyResource(_magBufferNarrow, _stagingMagBufferNarrow);
				_narrowStaged = true;
			}
			_pendingReadback = true;
		}
		return array;
	}

	public void SetSpan(int firstBin, int binCount)
	{
		if (_initialized)
		{
			if (firstBin < 0)
			{
				firstBin = 0;
			}
			if (binCount <= 0)
			{
				binCount = _fftSize;
			}
			if (firstBin + binCount > _fftSize)
			{
				binCount = _fftSize - firstBin;
			}
			_firstBin = firstBin;
			_binCount = binCount;
		}
	}

	public void SetFrequencySpan(float lowFreq, float highFreq)
	{
		_displayLowFreq = lowFreq;
		_displayHighFreq = highFreq;
	}

	public void SetNarrowSpan(bool enabled, float lowFreq, float highFreq)
	{
		_narrowEnabled = enabled && highFreq > lowFreq;
		_narrowLow = lowFreq;
		_narrowHigh = highFreq;
		if (!_narrowEnabled)
		{
			_narrowRowReady = false;
			_narrowStaged = false;
		}
	}

	private void RunFFT()
	{
		int num = _firstBin;
		int num2 = ((_binCount > 0) ? _binCount : _fftSize);
		if (num < 0)
		{
			num = 0;
		}
		if (num + num2 > _fftSize)
		{
			num2 = _fftSize - num;
		}
		float binWidth = _sampleRate / (float)_fftSize;
		FFTParams data = new FFTParams
		{
			N = (uint)_fftSize,
			Bits = _bits,
			Stage = 0u,
			DisplayWidth = (uint)_displayWidth,
			FirstBin = num,
			BinCount = (uint)num2,
			SampleRate = _sampleRate,
			WindowPower = _windowPower,
			CoherentGain = _windowCoherentGain,
			MagnitudeMode = (int)_magnitudeMode,
			DisplayLowFreq = _displayLowFreq,
			DisplayHighFreq = _displayHighFreq,
			BinWidth = binWidth,
			Padding = 0u,
			LanczosWindow = _lanczosWindow,
			ResamplingMode = (int)_resamplingMode
		};
		_context.ComputeShader.Set(_bitReverseCS);
		_context.ComputeShader.SetConstantBuffer(0, _constantBuffer);
		_context.ComputeShader.SetShaderResource(0, _iqSRV);
		_context.ComputeShader.SetShaderResource(1, _windowSRV);
		_context.ComputeShader.SetUnorderedAccessView(0, _fftAUAV);
		_context.ComputeShader.SetUnorderedAccessView(1, null);
		_context.ComputeShader.SetUnorderedAccessView(2, null);
		_context.UpdateSubresource(ref data, _constantBuffer);
		_context.Dispatch((int)Math.Ceiling((double)_fftSize / 256.0), 1, 1);
		for (uint num3 = 1u; num3 <= _bits; num3++)
		{
			data.Stage = num3;
			_context.UpdateSubresource(ref data, _constantBuffer);
			bool flag = (num3 & 1) == 1;
			_context.ComputeShader.Set(flag ? _fftStageABCS : _fftStageBACS);
			_context.ComputeShader.SetUnorderedAccessView(0, _fftAUAV);
			_context.ComputeShader.SetUnorderedAccessView(1, _fftBUAV);
			_context.ComputeShader.SetUnorderedAccessView(2, null);
			_context.Dispatch((int)Math.Ceiling((double)(_fftSize >> 1) / 256.0), 1, 1);
		}
		if ((_bits & 1) == 1)
		{
			_context.ComputeShader.SetUnorderedAccessView(0, null);
			_context.ComputeShader.SetUnorderedAccessView(1, null);
			_context.CopyResource(_fftBufferB, _fftBufferA);
		}
		_context.ComputeShader.Set(_magnitudeCS);
		_context.ComputeShader.SetUnorderedAccessView(0, _fftAUAV);
		_context.ComputeShader.SetUnorderedAccessView(1, null);
		_context.ComputeShader.SetUnorderedAccessView(2, _magUAV);
		_context.UpdateSubresource(ref data, _constantBuffer);
		_context.Dispatch((int)Math.Ceiling((double)_displayWidth / 256.0), 1, 1);
		if (_narrowEnabled && _magBufferNarrow != null && _magUAVNarrow != null)
		{
			data.DisplayWidth = 1024u;
			data.DisplayLowFreq = _narrowLow;
			data.DisplayHighFreq = _narrowHigh;
			_context.ComputeShader.SetUnorderedAccessView(2, _magUAVNarrow);
			_context.UpdateSubresource(ref data, _constantBuffer);
			_context.Dispatch((int)Math.Ceiling(4.0), 1, 1);
		}
		_context.ComputeShader.SetUnorderedAccessView(0, null);
		_context.ComputeShader.SetUnorderedAccessView(1, null);
		_context.ComputeShader.SetUnorderedAccessView(2, null);
		_context.ComputeShader.SetShaderResource(0, null);
		_context.ComputeShader.SetShaderResource(1, null);
	}

	private unsafe void UploadIQ(float[] i, float[] q, int n)
	{
		if (_iqUpload == null || _iqUpload.Length < n * 2)
		{
			_iqUpload = new float[524288];
		}
		fixed (float* ptr = i)
		{
			fixed (float* ptr2 = q)
			{
				fixed (float* iqUpload = _iqUpload)
				{
					for (int j = 0; j < n; j++)
					{
						iqUpload[j * 2] = ptr[j];
						iqUpload[j * 2 + 1] = ptr2[j];
					}
				}
			}
		}
		_context.MapSubresource(_iqStagingBuffer, MapMode.Write, SharpDX.Direct3D11.MapFlags.None, out var stream);
		if (stream != null)
		{
			stream.WriteRange(_iqUpload, 0, n * 2);
			_context.UnmapSubresource(_iqStagingBuffer, 0);
			ResourceRegion value = new ResourceRegion(0, 0, 0, n * 2 * 4, 1, 1);
			_context.CopySubresourceRegion(_iqStagingBuffer, 0, value, _iqBuffer, 0);
		}
	}

	private void ComputeWindow()
	{
		if (_window == null || _window.Length < 262144)
		{
			_window = new float[262144];
		}
		Array.Clear(_window, 0, _window.Length);
		double num = _fftSize - 1;
		double num2 = 0.0;
		double num3 = 0.0;
		for (int i = 0; i < _fftSize; i++)
		{
			double num4 = Math.PI * 2.0 * (double)i / num;
			double num7;
			switch (_windowType)
			{
			case GPUWaterfallWindowType.Hann:
				num7 = 0.5 - 0.5 * Math.Cos(num4);
				break;
			case GPUWaterfallWindowType.Hamming:
				num7 = 0.54 - 0.46 * Math.Cos(num4);
				break;
			case GPUWaterfallWindowType.Blackman:
				num7 = 0.42 - 0.5 * Math.Cos(num4) + 0.08 * Math.Cos(2.0 * num4);
				break;
			case GPUWaterfallWindowType.BlackmanHarris:
				num7 = 0.4243801 - 0.4973406 * Math.Cos(num4) + 0.0782793 * Math.Cos(2.0 * num4);
				break;
			default:
				num7 = 0.3635819 - 0.4891775 * Math.Cos(num4) + 0.1365995 * Math.Cos(2.0 * num4) - 0.0106411 * Math.Cos(3.0 * num4);
				break;
			case GPUWaterfallWindowType.Kaiser:
			{
				double num5 = num * 0.5;
				double num6 = ((double)i - num5) / num5;
				num7 = BesselI0(_kaiserBeta * Math.Sqrt(Math.Max(0.0, 1.0 - num6 * num6))) / BesselI0(_kaiserBeta);
				break;
			}
			}
			_window[i] = (float)num7;
			num3 += num7;
			num2 += num7 * num7;
		}
		_windowPower = (float)(num2 / (double)_fftSize);
		_windowCoherentGain = (float)(num3 / (double)_fftSize);
	}

	private static double BesselI0(double x)
	{
		double num = Math.Abs(x);
		if (num < 3.75)
		{
			double num2 = x / 3.75;
			num2 *= num2;
			return 1.0 + num2 * (3.5156229 + num2 * (3.0899424 + num2 * (1.2067492 + num2 * (0.2659732 + num2 * (0.0360768 + num2 * 0.0045813)))));
		}
		double num3 = 3.75 / num;
		return (0.39894228 + num3 * (0.01328592 + num3 * (0.00225319 + num3 * (-0.00157565 + num3 * (0.00916281 + num3 * (-0.02057706 + num3 * (0.02635537 + num3 * (-0.01647633 + num3 * 0.00392377)))))))) * Math.Exp(num) / Math.Sqrt(num);
	}

	private void UploadWindow()
	{
		if (_windowBuffer == null || _window == null || _fftSize <= 0)
		{
			return;
		}
		try
		{
			BufferDescription description = new BufferDescription
			{
				SizeInBytes = 1048576,
				Usage = ResourceUsage.Staging,
				BindFlags = BindFlags.None,
				CpuAccessFlags = CpuAccessFlags.Write,
				OptionFlags = ResourceOptionFlags.None,
				StructureByteStride = 0
			};
			using SharpDX.Direct3D11.Buffer buffer = new SharpDX.Direct3D11.Buffer(_device, description);
			_context.MapSubresource(buffer, MapMode.Write, SharpDX.Direct3D11.MapFlags.None, out var stream);
			if (stream != null)
			{
				stream.WriteRange(_window, 0, _fftSize);
				_context.UnmapSubresource(buffer, 0);
			}
			ResourceRegion value = new ResourceRegion(0, 0, 0, _fftSize * 4, 1, 1);
			_context.CopySubresourceRegion(buffer, 0, value, _windowBuffer, 0);
		}
		catch (Exception ex)
		{
			LogGPU("UploadWindow failed: " + ex.Message);
		}
	}

	private int ReadbackMag()
	{
		if (_context == null || _magBuffer == null || _stagingMagBuffer == null)
		{
			return 0;
		}
		if (_magReadback == null || _magReadback.Length < 8192)
		{
			_magReadback = new float[8192];
		}
		if (!_doNotWaitBroken)
		{
			try
			{
				_context.MapSubresource(_stagingMagBuffer, MapMode.Read, SharpDX.Direct3D11.MapFlags.DoNotWait, out var stream);
				if (stream != null)
				{
					try
					{
						stream.ReadRange(_magReadback, 0, _displayWidth);
						return 1;
					}
					finally
					{
						try
						{
							_context.UnmapSubresource(_stagingMagBuffer, 0);
						}
						catch
						{
						}
					}
				}
			}
			catch (SharpDXException)
			{
			}
			catch (Exception ex2)
			{
				_doNotWaitBroken = true;
				LogGPU("ReadbackMag: DoNotWait disabled (" + ex2.GetType().Name + ": " + ex2.Message + ")");
			}
		}
		try
		{
			_context.MapSubresource(_stagingMagBuffer, MapMode.Read, SharpDX.Direct3D11.MapFlags.None, out var stream2);
			if (stream2 != null)
			{
				try
				{
					stream2.ReadRange(_magReadback, 0, _displayWidth);
					return 2;
				}
				finally
				{
					try
					{
						_context.UnmapSubresource(_stagingMagBuffer, 0);
					}
					catch
					{
					}
				}
			}
			if (++_readbackErrorSkip >= 30)
			{
				_readbackErrorSkip = 0;
				LogGPU("ReadbackMag: blocking map returned null stream (driver quirk)");
			}
		}
		catch (Exception ex3)
		{
			LogReadbackError(ex3);
		}
		return 0;
	}

	private void LogReadbackError(Exception ex)
	{
		if (++_readbackErrorSkip >= 30)
		{
			_readbackErrorSkip = 0;
			LogGPU("ReadbackMag failed: " + ex.GetType().Name + ": " + ex.Message);
		}
	}

	private bool LoadShaders()
	{
		byte[] array = LoadBytecode("waterfall_fft_bitreverse_cs.bin");
		byte[] array2 = LoadBytecode("waterfall_fft_stage_ab_cs.bin");
		byte[] array3 = LoadBytecode("waterfall_fft_stage_ba_cs.bin");
		byte[] array4 = LoadBytecode("waterfall_fft_magnitude_cs.bin");
		if (array == null || array2 == null || array3 == null || array4 == null)
		{
			return false;
		}
		_bitReverseCS = new ComputeShader(_device, array);
		_fftStageABCS = new ComputeShader(_device, array2);
		_fftStageBACS = new ComputeShader(_device, array3);
		_magnitudeCS = new ComputeShader(_device, array4);
		return true;
	}

	private bool CreateBuffers()
	{
		_iqBuffer = new SharpDX.Direct3D11.Buffer(_device, new BufferDescription
		{
			SizeInBytes = 2097152,
			Usage = ResourceUsage.Default,
			BindFlags = BindFlags.ShaderResource,
			CpuAccessFlags = CpuAccessFlags.None,
			OptionFlags = ResourceOptionFlags.BufferStructured,
			StructureByteStride = 8
		});
		_iqStagingBuffer = new SharpDX.Direct3D11.Buffer(_device, new BufferDescription
		{
			SizeInBytes = 2097152,
			Usage = ResourceUsage.Staging,
			BindFlags = BindFlags.None,
			CpuAccessFlags = CpuAccessFlags.Write,
			OptionFlags = ResourceOptionFlags.None,
			StructureByteStride = 0
		});
		_windowBuffer = new SharpDX.Direct3D11.Buffer(_device, new BufferDescription
		{
			SizeInBytes = 1048576,
			Usage = ResourceUsage.Default,
			BindFlags = BindFlags.ShaderResource,
			CpuAccessFlags = CpuAccessFlags.None,
			OptionFlags = ResourceOptionFlags.BufferStructured,
			StructureByteStride = 4
		});
		BufferDescription description = new BufferDescription
		{
			SizeInBytes = 2097152,
			Usage = ResourceUsage.Default,
			BindFlags = (BindFlags.ShaderResource | BindFlags.UnorderedAccess),
			CpuAccessFlags = CpuAccessFlags.None,
			OptionFlags = ResourceOptionFlags.BufferStructured,
			StructureByteStride = 8
		};
		_fftBufferA = new SharpDX.Direct3D11.Buffer(_device, description);
		_fftBufferB = new SharpDX.Direct3D11.Buffer(_device, description);
		_magBuffer = new SharpDX.Direct3D11.Buffer(_device, new BufferDescription
		{
			SizeInBytes = 32768,
			Usage = ResourceUsage.Default,
			BindFlags = (BindFlags.ShaderResource | BindFlags.UnorderedAccess),
			CpuAccessFlags = CpuAccessFlags.None,
			OptionFlags = ResourceOptionFlags.BufferStructured,
			StructureByteStride = 4
		});
		_stagingMagBuffer = new SharpDX.Direct3D11.Buffer(_device, new BufferDescription
		{
			SizeInBytes = 32768,
			Usage = ResourceUsage.Staging,
			BindFlags = BindFlags.None,
			CpuAccessFlags = CpuAccessFlags.Read,
			OptionFlags = ResourceOptionFlags.None,
			StructureByteStride = 0
		});
		_magBufferNarrow = new SharpDX.Direct3D11.Buffer(_device, new BufferDescription
		{
			SizeInBytes = 4096,
			Usage = ResourceUsage.Default,
			BindFlags = (BindFlags.ShaderResource | BindFlags.UnorderedAccess),
			CpuAccessFlags = CpuAccessFlags.None,
			OptionFlags = ResourceOptionFlags.BufferStructured,
			StructureByteStride = 4
		});
		_magUAVNarrow = new UnorderedAccessView(_device, _magBufferNarrow, new UnorderedAccessViewDescription
		{
			Dimension = UnorderedAccessViewDimension.Buffer,
			Format = Format.Unknown,
			Buffer = new UnorderedAccessViewDescription.BufferResource
			{
				ElementCount = 1024,
				FirstElement = 0,
				Flags = UnorderedAccessViewBufferFlags.None
			}
		});
		_stagingMagBufferNarrow = new SharpDX.Direct3D11.Buffer(_device, new BufferDescription
		{
			SizeInBytes = 4096,
			Usage = ResourceUsage.Staging,
			BindFlags = BindFlags.None,
			CpuAccessFlags = CpuAccessFlags.Read,
			OptionFlags = ResourceOptionFlags.None,
			StructureByteStride = 0
		});
		if (_magReadbackNarrow == null || _magReadbackNarrow.Length < 1024)
		{
			_magReadbackNarrow = new float[1024];
		}
		int num = Marshal.SizeOf(typeof(FFTParams));
		num = (num + 15) & -16;
		_constantBuffer = new SharpDX.Direct3D11.Buffer(_device, new BufferDescription
		{
			SizeInBytes = num,
			Usage = ResourceUsage.Default,
			BindFlags = BindFlags.ConstantBuffer,
			CpuAccessFlags = CpuAccessFlags.None,
			OptionFlags = ResourceOptionFlags.None,
			StructureByteStride = 0
		});
		if (_window == null || _window.Length < 262144)
		{
			_window = new float[262144];
		}
		if (_iqUpload == null || _iqUpload.Length < 524288)
		{
			_iqUpload = new float[524288];
		}
		if (_magReadback == null || _magReadback.Length < 8192)
		{
			_magReadback = new float[8192];
		}
		return true;
	}

	private bool CreateViews(int fftSize, int displayWidth)
	{
		Utilities.Dispose(ref _iqSRV);
		Utilities.Dispose(ref _windowSRV);
		Utilities.Dispose(ref _fftAUAV);
		Utilities.Dispose(ref _fftBUAV);
		Utilities.Dispose(ref _magUAV);
		Utilities.Dispose(ref _magSRV);
		_iqSRV = new ShaderResourceView(_device, _iqBuffer, new ShaderResourceViewDescription
		{
			Dimension = ShaderResourceViewDimension.Buffer,
			Format = Format.Unknown,
			Buffer = new ShaderResourceViewDescription.BufferResource
			{
				ElementCount = fftSize
			}
		});
		_windowSRV = new ShaderResourceView(_device, _windowBuffer, new ShaderResourceViewDescription
		{
			Dimension = ShaderResourceViewDimension.Buffer,
			Format = Format.Unknown,
			Buffer = new ShaderResourceViewDescription.BufferResource
			{
				ElementCount = fftSize
			}
		});
		_fftAUAV = new UnorderedAccessView(_device, _fftBufferA, new UnorderedAccessViewDescription
		{
			Dimension = UnorderedAccessViewDimension.Buffer,
			Format = Format.Unknown,
			Buffer = new UnorderedAccessViewDescription.BufferResource
			{
				ElementCount = fftSize,
				FirstElement = 0,
				Flags = UnorderedAccessViewBufferFlags.None
			}
		});
		_fftBUAV = new UnorderedAccessView(_device, _fftBufferB, new UnorderedAccessViewDescription
		{
			Dimension = UnorderedAccessViewDimension.Buffer,
			Format = Format.Unknown,
			Buffer = new UnorderedAccessViewDescription.BufferResource
			{
				ElementCount = fftSize,
				FirstElement = 0,
				Flags = UnorderedAccessViewBufferFlags.None
			}
		});
		_magUAV = new UnorderedAccessView(_device, _magBuffer, new UnorderedAccessViewDescription
		{
			Dimension = UnorderedAccessViewDimension.Buffer,
			Format = Format.Unknown,
			Buffer = new UnorderedAccessViewDescription.BufferResource
			{
				ElementCount = displayWidth,
				FirstElement = 0,
				Flags = UnorderedAccessViewBufferFlags.None
			}
		});
		_magSRV = new ShaderResourceView(_device, _magBuffer, new ShaderResourceViewDescription
		{
			Dimension = ShaderResourceViewDimension.Buffer,
			Format = Format.Unknown,
			Buffer = new ShaderResourceViewDescription.BufferResource
			{
				ElementCount = displayWidth
			}
		});
		return true;
	}

	private void DisposeResources()
	{
		_initialized = false;
		_pendingReadback = false;
		Utilities.Dispose(ref _iqSRV);
		Utilities.Dispose(ref _iqBuffer);
		Utilities.Dispose(ref _iqStagingBuffer);
		Utilities.Dispose(ref _windowSRV);
		Utilities.Dispose(ref _windowBuffer);
		Utilities.Dispose(ref _fftAUAV);
		Utilities.Dispose(ref _fftBUAV);
		Utilities.Dispose(ref _fftBufferA);
		Utilities.Dispose(ref _fftBufferB);
		Utilities.Dispose(ref _magSRV);
		Utilities.Dispose(ref _magUAV);
		Utilities.Dispose(ref _magBuffer);
		Utilities.Dispose(ref _stagingMagBuffer);
		Utilities.Dispose(ref _magUAVNarrow);
		Utilities.Dispose(ref _magBufferNarrow);
		Utilities.Dispose(ref _stagingMagBufferNarrow);
		_narrowRowReady = false;
		_narrowStaged = false;
		Utilities.Dispose(ref _constantBuffer);
	}

	public void Dispose()
	{
		DisposeResources();
		Utilities.Dispose(ref _bitReverseCS);
		Utilities.Dispose(ref _fftStageABCS);
		Utilities.Dispose(ref _fftStageBACS);
		Utilities.Dispose(ref _magnitudeCS);
	}

	private static byte[] LoadBytecode(string filename)
	{
		byte[] b = GPUWaterfallShaderLoader.Load(filename);
		if (b == null || b.Length == 0) LogGPU("Shader binary not found: " + filename);
		return b;
	}

	private static int Log2(int n)
	{
		int num = 0;
		while (n > 1)
		{
			n >>= 1;
			num++;
		}
		return num;
	}

	private static bool IsPowerOfTwo(int n)
	{
		if (n > 0)
		{
			return (n & (n - 1)) == 0;
		}
		return false;
	}

	private static void LogGPU(string message)
	{
		GPUWaterfallLogger.Log("GPU-FFT", message);
	}
}
