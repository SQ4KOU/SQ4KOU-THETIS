using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace Thetis;

public class WaterfallGPURenderer : IDisposable
{
	private SharpDX.Direct3D11.Device _device;

	private SharpDX.Direct2D1.DeviceContext _d2dDC;

	private Format _format;

	private bool _isLinearOutput;

	private int _width;

	private int _height;

	private int _ditherRowY;

	private readonly HiPerfTimer _temporalTimer = new HiPerfTimer();

	private bool _temporalTimerValid;

	private Texture2D _waterfallTexture;

	private Surface _waterfallSurface;

	private Bitmap1 _waterfallBitmap;

	private UnorderedAccessView _waterfallUAV;

	private Texture2D _scrollTempTexture;

	private Texture2D _rowTexture;

	private UnorderedAccessView _rowUAV;

	private SharpDX.Direct3D11.Buffer _spectrumBuffer;

	private ShaderResourceView _spectrumSRV;

	private SharpDX.Direct3D11.Buffer _paletteBuffer;

	private ShaderResourceView _paletteSRV;

	private int _paletteCapacity;

	private int _paletteSize;

	private SharpDX.Direct3D11.Buffer _prevPctBuffer;

	private UnorderedAccessView _prevPctUAV;

	private SharpDX.Direct3D11.Buffer _constantBuffer;

	private ComputeShader _computeShader;

	private bool _initialized;

	private static byte[] _shaderBytecode;

	public bool IsInitialized => _initialized;

	public int Width => _width;

	public int Height => _height;

	public bool IsLinearOutput => _isLinearOutput;

	public WaterfallGPURenderer(SharpDX.Direct3D11.Device device, SharpDX.Direct2D1.DeviceContext d2dDC, int width, int height, Format format)
	{
		_device = device;
		_d2dDC = d2dDC;
		_paletteCapacity = 1024;
		Resize(d2dDC, width, height, format);
	}

	public bool Resize(SharpDX.Direct2D1.DeviceContext dc, int width, int height, Format format)
	{
		_d2dDC = dc;
		if (width <= 0 || height <= 0)
		{
			return false;
		}
		if (format != Format.B8G8R8A8_UNorm && format != Format.R16G16B16A16_Float)
		{
			LogGPU($"Resize: unsupported format {format}");
			return false;
		}
		if (_format != format)
		{
			DisposeResources();
		}
		if (_width == width && _height == height && _initialized && _format == format)
		{
			return true;
		}
		_format = format;
		_isLinearOutput = format == Format.R16G16B16A16_Float;
		DisposeResources();
		_width = width;
		_height = height;
		_ditherRowY = 0;
		_temporalTimerValid = false;
		try
		{
			if (_shaderBytecode == null)
			{
				_shaderBytecode = LoadShaderBytecode();
			}
			if (_shaderBytecode == null || _shaderBytecode.Length == 0)
			{
				return false;
			}
			_computeShader = new ComputeShader(_device, _shaderBytecode);
			Texture2DDescription texture2DDescription = new Texture2DDescription
			{
				Width = _width,
				Height = _height,
				MipLevels = 1,
				ArraySize = 1,
				Format = _format,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				BindFlags = (BindFlags.ShaderResource | BindFlags.UnorderedAccess),
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None
			};
			_waterfallTexture = new Texture2D(_device, texture2DDescription);
			_waterfallSurface = _waterfallTexture.QueryInterface<Surface>();
			if (_waterfallSurface == null)
			{
				LogGPU($"Resize ({_width}x{_height}): QueryInterface<Surface>() returned null");
				return false;
			}
			_waterfallUAV = new UnorderedAccessView(_device, _waterfallTexture, new UnorderedAccessViewDescription
			{
				Format = _format,
				Dimension = UnorderedAccessViewDimension.Texture2D,
				Texture2D = 
				{
					MipSlice = 0
				}
			});
			Size2F dotsPerInch = _d2dDC.DotsPerInch;
			SharpDX.Direct2D1.AlphaMode alphaMode = ((_format != Format.R16G16B16A16_Float) ? SharpDX.Direct2D1.AlphaMode.Premultiplied : SharpDX.Direct2D1.AlphaMode.Ignore);
			BitmapProperties1 bitmapProperties = new BitmapProperties1(new PixelFormat(_format, alphaMode), dotsPerInch.Width, dotsPerInch.Height, BitmapOptions.None);
			_waterfallBitmap = new Bitmap1(_d2dDC, _waterfallSurface, bitmapProperties);
			Texture2DDescription description = texture2DDescription;
			description.BindFlags = BindFlags.None;
			_scrollTempTexture = new Texture2D(_device, description);
			Texture2DDescription description2 = texture2DDescription;
			description2.Height = 1;
			_rowTexture = new Texture2D(_device, description2);
			_rowUAV = new UnorderedAccessView(_device, _rowTexture, new UnorderedAccessViewDescription
			{
				Format = _format,
				Dimension = UnorderedAccessViewDimension.Texture2D,
				Texture2D = 
				{
					MipSlice = 0
				}
			});
			_spectrumBuffer = new SharpDX.Direct3D11.Buffer(_device, new BufferDescription
			{
				SizeInBytes = _width * 4,
				Usage = ResourceUsage.Dynamic,
				BindFlags = BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.Write,
				OptionFlags = ResourceOptionFlags.BufferStructured,
				StructureByteStride = 4
			});
			_spectrumSRV = new ShaderResourceView(_device, _spectrumBuffer, new ShaderResourceViewDescription
			{
				Format = Format.Unknown,
				Dimension = ShaderResourceViewDimension.Buffer,
				Buffer = 
				{
					ElementCount = _width
				}
			});
			_paletteBuffer = new SharpDX.Direct3D11.Buffer(_device, new BufferDescription
			{
				SizeInBytes = _paletteCapacity * 4 * 4,
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.BufferStructured,
				StructureByteStride = 16
			});
			_paletteSRV = new ShaderResourceView(_device, _paletteBuffer, new ShaderResourceViewDescription
			{
				Format = Format.Unknown,
				Dimension = ShaderResourceViewDimension.Buffer,
				Buffer = 
				{
					ElementCount = _paletteCapacity
				}
			});
			_prevPctBuffer = new SharpDX.Direct3D11.Buffer(_device, new BufferDescription
			{
				SizeInBytes = _width * 4 * 4,
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.UnorderedAccess,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.BufferStructured,
				StructureByteStride = 16
			});
			_prevPctUAV = new UnorderedAccessView(_device, _prevPctBuffer, new UnorderedAccessViewDescription
			{
				Format = Format.Unknown,
				Dimension = UnorderedAccessViewDimension.Buffer,
				Buffer = 
				{
					ElementCount = _width
				}
			});
			int sizeInBytes = Marshal.SizeOf(typeof(WaterfallRowParams));
			_constantBuffer = new SharpDX.Direct3D11.Buffer(_device, new BufferDescription
			{
				SizeInBytes = sizeInBytes,
				Usage = ResourceUsage.Dynamic,
				BindFlags = BindFlags.ConstantBuffer,
				CpuAccessFlags = CpuAccessFlags.Write
			});
			Clear();
			_initialized = true;
			return true;
		}
		catch (Exception ex)
		{
			string text = $"WaterfallGPURenderer.Resize failed ({_width}x{_height}): " + ex;
			LogTool.AddLogEntry(text, "GPU");
			LogGPU(text);
			DisposeResources();
			_initialized = false;
			return false;
		}
	}

	public void SetPalette(float[] rgba, int count)
	{
		if (!_initialized || rgba == null || count <= 0)
		{
			return;
		}
		if (count > _paletteCapacity)
		{
			count = _paletteCapacity;
		}
		_paletteSize = count;
		GCHandle gCHandle = GCHandle.Alloc(rgba, GCHandleType.Pinned);
		try
		{
			DataBox source = new DataBox(gCHandle.AddrOfPinnedObject(), 0, 0);
			_device.ImmediateContext.UpdateSubresource(source, _paletteBuffer);
		}
		finally
		{
			gCHandle.Free();
		}
	}

	public void ProcessRow(float[] spectrum, int spectrumLength, int decimation, float lowThreshold, float highThreshold, float gamma, float invGamma, int toneMapMode, float saturationBoost, float contrastBoost, bool ditherEnabled, int ditherLevels, float temporalAlphaRef, float motionThreshold, bool isPaletteScheme, bool applyGammaToPercent, float paletteSharpness, float paletteContrast)
	{
		if (_initialized)
		{
			int count = Math.Min(spectrumLength, _width);
			_device.ImmediateContext.MapSubresource(_spectrumBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out var stream);
			if (stream != null)
			{
				stream.WriteRange(spectrum, 0, count);
				_device.ImmediateContext.UnmapSubresource(_spectrumBuffer, 0);
			}
			ProcessRowCore(_spectrumSRV, spectrumLength, decimation, lowThreshold, highThreshold, gamma, invGamma, toneMapMode, saturationBoost, contrastBoost, ditherEnabled, ditherLevels, temporalAlphaRef, motionThreshold, isPaletteScheme, applyGammaToPercent, paletteSharpness, paletteContrast);
		}
	}

	public void ProcessRow(ShaderResourceView spectrumSrv, int spectrumLength, int decimation, float lowThreshold, float highThreshold, float gamma, float invGamma, int toneMapMode, float saturationBoost, float contrastBoost, bool ditherEnabled, int ditherLevels, float temporalAlphaRef, float motionThreshold, bool isPaletteScheme, bool applyGammaToPercent, float paletteSharpness, float paletteContrast)
	{
		if (_initialized && spectrumSrv != null && !spectrumSrv.IsDisposed)
		{
			ProcessRowCore(spectrumSrv, spectrumLength, decimation, lowThreshold, highThreshold, gamma, invGamma, toneMapMode, saturationBoost, contrastBoost, ditherEnabled, ditherLevels, temporalAlphaRef, motionThreshold, isPaletteScheme, applyGammaToPercent, paletteSharpness, paletteContrast);
		}
	}

	private void ProcessRowCore(ShaderResourceView spectrumSrv, int spectrumLength, int decimation, float lowThreshold, float highThreshold, float gamma, float invGamma, int toneMapMode, float saturationBoost, float contrastBoost, bool ditherEnabled, int ditherLevels, float temporalAlphaRef, float motionThreshold, bool isPaletteScheme, bool applyGammaToPercent, float paletteSharpness, float paletteContrast)
	{
		float dt = 0f;
		float tau = 0f;
		if (_temporalTimerValid)
		{
			dt = (float)_temporalTimer.Elapsed;
		}
		if (temporalAlphaRef > 0f && temporalAlphaRef < 1f)
		{
			tau = -1f / 60f / (float)Math.Log(1.0 - (double)temporalAlphaRef);
		}
		_temporalTimer.Reset();
		_temporalTimerValid = true;
		WaterfallRowParams value = new WaterfallRowParams
		{
			Width = _width,
			Decimation = decimation,
			PaletteSize = ((_paletteSize > 0) ? _paletteSize : _paletteCapacity),
			RowYForDither = (_ditherRowY & 7),
			ToneMapMode = toneMapMode,
			DitherEnabled = (ditherEnabled ? 1 : 0),
			DitherLevels = ditherLevels,
			Pad0 = 0,
			IsPaletteScheme = (isPaletteScheme ? 1 : 0),
			ApplyGammaToPercent = (applyGammaToPercent ? 1 : 0),
			IsLinearOutput = (_isLinearOutput ? 1 : 0),
			Pad2 = 0,
			LowThreshold = lowThreshold,
			HighThreshold = highThreshold,
			Dt = dt,
			Tau = tau,
			Gamma = gamma,
			InvGamma = invGamma,
			MotionThreshold = motionThreshold,
			QualityContrast = contrastBoost,
			SaturationBoost = saturationBoost,
			ContrastBoost = contrastBoost,
			PaletteSharpness = paletteSharpness,
			PaletteContrast = paletteContrast
		};
		_device.ImmediateContext.MapSubresource(_constantBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out var stream);
		if (stream != null)
		{
			stream.Write(value);
			_device.ImmediateContext.UnmapSubresource(_constantBuffer, 0);
		}
		SharpDX.Direct3D11.DeviceContext immediateContext = _device.ImmediateContext;
		immediateContext.ComputeShader.Set(_computeShader);
		immediateContext.ComputeShader.SetShaderResource(0, spectrumSrv);
		immediateContext.ComputeShader.SetShaderResource(1, _paletteSRV);
		immediateContext.ComputeShader.SetUnorderedAccessView(0, _rowUAV);
		immediateContext.ComputeShader.SetUnorderedAccessView(1, _prevPctUAV);
		immediateContext.ComputeShader.SetConstantBuffer(0, _constantBuffer);
		int threadGroupCountX = (_width + 255) / 256;
		immediateContext.Dispatch(threadGroupCountX, 1, 1);
		immediateContext.ComputeShader.SetUnorderedAccessView(0, null);
		immediateContext.ComputeShader.SetUnorderedAccessView(1, null);
		immediateContext.Flush();
		_ditherRowY++;
	}

	public void AdvanceRow(int horizontalShiftPixels, bool insertedNewRow)
	{
		if (_initialized)
		{
			SharpDX.Direct3D11.DeviceContext immediateContext = _device.ImmediateContext;
			immediateContext.CopySubresourceRegion(_waterfallTexture, 0, null, _scrollTempTexture, 0);
			immediateContext.ClearUnorderedAccessView(_waterfallUAV, new RawVector4(0f, 0f, 0f, 1f));
			int num = Math.Abs(horizontalShiftPixels);
			int num2 = _width - num;
			int dstY = (insertedNewRow ? 1 : 0);
			int num3 = (insertedNewRow ? (_height - 1) : _height);
			if (num2 > 0 && num3 > 0)
			{
				int num4 = ((horizontalShiftPixels < 0) ? (-horizontalShiftPixels) : 0);
				int dstX = ((horizontalShiftPixels > 0) ? horizontalShiftPixels : 0);
				immediateContext.CopySubresourceRegion(_scrollTempTexture, 0, new ResourceRegion(num4, 0, 0, num4 + num2, num3, 1), _waterfallTexture, 0, dstX, dstY);
			}
			if (insertedNewRow)
			{
				immediateContext.CopySubresourceRegion(_rowTexture, 0, null, _waterfallTexture, 0);
			}
			immediateContext.Flush();
		}
	}

	public void UpdateDeviceContext(SharpDX.Direct2D1.DeviceContext dc)
	{
		if (dc != null)
		{
			_d2dDC = dc;
			if (_waterfallBitmap != null && !_waterfallBitmap.IsDisposed)
			{
				Utilities.Dispose(ref _waterfallBitmap);
			}
			if (_waterfallSurface != null && !_waterfallSurface.IsDisposed)
			{
				Size2F dotsPerInch = _d2dDC.DotsPerInch;
				SharpDX.Direct2D1.AlphaMode alphaMode = ((_format != Format.R16G16B16A16_Float) ? SharpDX.Direct2D1.AlphaMode.Premultiplied : SharpDX.Direct2D1.AlphaMode.Ignore);
				BitmapProperties1 bitmapProperties = new BitmapProperties1(new PixelFormat(_format, alphaMode), dotsPerInch.Width, dotsPerInch.Height, BitmapOptions.None);
				_waterfallBitmap = new Bitmap1(_d2dDC, _waterfallSurface, bitmapProperties);
			}
		}
	}

	public void Draw(int xOffset, int yOffset, float opacity, Brush clearBrush, bool linearInterpolation = false)
	{
		if (_initialized && _waterfallBitmap != null && !_waterfallBitmap.IsDisposed && _d2dDC != null && !_d2dDC.IsDisposed)
		{
			_d2dDC.DrawBitmap(_waterfallBitmap, new RectangleF(xOffset, yOffset, _width, _height), opacity, linearInterpolation ? BitmapInterpolationMode.Linear : BitmapInterpolationMode.NearestNeighbor);
		}
	}

	public void DrawScaled(int xOffset, int yOffset, int destWidth, int destHeight, float opacity, bool linearInterpolation)
	{
		if (_initialized && _waterfallBitmap != null && !_waterfallBitmap.IsDisposed && _d2dDC != null && !_d2dDC.IsDisposed && destWidth > 0 && destHeight > 0)
		{
			_d2dDC.DrawBitmap(_waterfallBitmap, new RectangleF(xOffset, yOffset, destWidth, destHeight), opacity, linearInterpolation ? BitmapInterpolationMode.Linear : BitmapInterpolationMode.NearestNeighbor);
		}
	}

	public void Clear()
	{
		if (_initialized)
		{
			_device.ImmediateContext.ClearUnorderedAccessView(_waterfallUAV, new RawVector4(0f, 0f, 0f, 1f));
			_device.ImmediateContext.ClearUnorderedAccessView(_prevPctUAV, new RawVector4(0f, 0f, 0f, 0f));
			_ditherRowY = 0;
			_temporalTimerValid = false;
		}
	}

	public void Dispose()
	{
		DisposeResources();
		GC.SuppressFinalize(this);
	}

	private void DisposeResources()
	{
		_initialized = false;
		Utilities.Dispose(ref _waterfallBitmap);
		Utilities.Dispose(ref _waterfallSurface);
		Utilities.Dispose(ref _waterfallUAV);
		Utilities.Dispose(ref _waterfallTexture);
		Utilities.Dispose(ref _scrollTempTexture);
		Utilities.Dispose(ref _rowUAV);
		Utilities.Dispose(ref _rowTexture);
		Utilities.Dispose(ref _spectrumSRV);
		Utilities.Dispose(ref _spectrumBuffer);
		Utilities.Dispose(ref _paletteSRV);
		Utilities.Dispose(ref _paletteBuffer);
		Utilities.Dispose(ref _prevPctUAV);
		Utilities.Dispose(ref _prevPctBuffer);
		Utilities.Dispose(ref _constantBuffer);
		Utilities.Dispose(ref _computeShader);
	}

	private static byte[] LoadShaderBytecode()
	{
		byte[] b = GPUWaterfallShaderLoader.Load("waterfall_row_cs.bin");
		if (b == null || b.Length == 0) LogGPU("Shader binary not found: waterfall_row_cs.bin");
		return b;
	}

	private static void LogGPU(string message)
	{
		GPUWaterfallLogger.Log("GPU-REN", message);
	}
}
