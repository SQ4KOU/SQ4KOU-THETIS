using System;
using System.IO;

namespace SkiaSharp;

public class SKPixmap : SKObject
{
	private const string UnableToCreateInstanceMessage = "Unable to create a new SKPixmap instance.";

	internal SKObject? pixelSource;

	public unsafe SKImageInfo Info
	{
		get
		{
			SKImageInfoNative native = default(SKImageInfoNative);
			SkiaApi.sk_pixmap_get_info(Handle, &native);
			return SKImageInfoNative.ToManaged(ref native);
		}
	}

	public int Width => Info.Width;

	public int Height => Info.Height;

	public SKSizeI Size
	{
		get
		{
			SKImageInfo info = Info;
			return new SKSizeI(info.Width, info.Height);
		}
	}

	public SKRectI Rect => SKRectI.Create(Size);

	public SKColorType ColorType => Info.ColorType;

	public SKAlphaType AlphaType => Info.AlphaType;

	public SKColorSpace? ColorSpace => SKColorSpace.GetObject(SkiaApi.sk_pixmap_get_colorspace(Handle));

	public int BytesPerPixel => Info.BytesPerPixel;

	public int BitShiftPerPixel => Info.BitShiftPerPixel;

	public int RowBytes => (int)SkiaApi.sk_pixmap_get_row_bytes(Handle);

	public int BytesSize => Info.BytesSize;

	public long BytesSize64 => Info.BytesSize64;

	internal SKPixmap(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKPixmap()
		: this(SkiaApi.sk_pixmap_new(), owns: true)
	{
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKPixmap instance.");
		}
	}

	public SKPixmap(SKImageInfo info, IntPtr addr)
		: this(info, addr, info.RowBytes)
	{
	}

	public unsafe SKPixmap(SKImageInfo info, IntPtr addr, int rowBytes)
		: this(IntPtr.Zero, owns: true)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		Handle = SkiaApi.sk_pixmap_new_with_params(&sKImageInfoNative, (void*)addr, (IntPtr)rowBytes);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new SKPixmap instance.");
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_pixmap_destructor(Handle);
	}

	protected override void DisposeManaged()
	{
		base.DisposeManaged();
		pixelSource = null;
	}

	public void Reset()
	{
		SkiaApi.sk_pixmap_reset(Handle);
		pixelSource = null;
	}

	public unsafe void Reset(SKImageInfo info, IntPtr addr, int rowBytes)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		SkiaApi.sk_pixmap_reset_with_params(Handle, &sKImageInfoNative, (void*)addr, (IntPtr)rowBytes);
		pixelSource = null;
	}

	public unsafe IntPtr GetPixels()
	{
		return (IntPtr)SkiaApi.sk_pixmap_get_writable_addr(Handle);
	}

	public unsafe IntPtr GetPixels(int x, int y)
	{
		return (IntPtr)SkiaApi.sk_pixmap_get_writeable_addr_with_xy(Handle, x, y);
	}

	public Span<byte> GetPixelSpan()
	{
		return GetPixelSpan<byte>(0, 0);
	}

	public Span<byte> GetPixelSpan(int x, int y)
	{
		return GetPixelSpan<byte>(x, y);
	}

	public Span<T> GetPixelSpan<T>() where T : unmanaged
	{
		return GetPixelSpan<T>(0, 0);
	}

	public unsafe Span<T> GetPixelSpan<T>(int x, int y) where T : unmanaged
	{
		SKImageInfo info = Info;
		if (info.IsEmpty)
		{
			return null;
		}
		int bytesPerPixel = info.BytesPerPixel;
		if (bytesPerPixel <= 0)
		{
			return null;
		}
		int num = 0;
		int num2 = 0;
		if (typeof(T) == typeof(byte))
		{
			num = info.BytesSize;
			if (x != 0 || y != 0)
			{
				num2 = info.GetPixelBytesOffset(x, y);
			}
		}
		else
		{
			int num3 = sizeof(T);
			if (bytesPerPixel != num3)
			{
				throw new ArgumentException($"Size of T ({num3}) is not the same as the size of each pixel ({bytesPerPixel}).", "T");
			}
			num = info.Width * info.Height;
			if (x != 0 || y != 0)
			{
				num2 = y * info.Height + x;
			}
		}
		void* pointer = SkiaApi.sk_pixmap_get_writable_addr(Handle);
		Span<T> result = new Span<T>(pointer, num);
		if (num2 != 0)
		{
			result = result.Slice(num2);
			return result;
		}
		return result;
	}

	public SKColor GetPixelColor(int x, int y)
	{
		return SkiaApi.sk_pixmap_get_pixel_color(Handle, x, y);
	}

	public unsafe SKColorF GetPixelColorF(int x, int y)
	{
		SKColorF result = default(SKColorF);
		SkiaApi.sk_pixmap_get_pixel_color4f(Handle, x, y, &result);
		return result;
	}

	public float GetPixelAlpha(int x, int y)
	{
		return SkiaApi.sk_pixmap_get_pixel_alphaf(Handle, x, y);
	}

	[Obsolete("Use ScalePixels(SKPixmap destination, SKSamplingOptions sampling) instead.")]
	public bool ScalePixels(SKPixmap destination, SKFilterQuality quality)
	{
		return ScalePixels(destination, quality.ToSamplingOptions());
	}

	public bool ScalePixels(SKPixmap destination)
	{
		return ScalePixels(destination, SKSamplingOptions.Default);
	}

	public unsafe bool ScalePixels(SKPixmap destination, SKSamplingOptions sampling)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		return SkiaApi.sk_pixmap_scale_pixels(Handle, destination.Handle, &sampling);
	}

	public unsafe bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref dstInfo);
		return SkiaApi.sk_pixmap_read_pixels(Handle, &sKImageInfoNative, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY);
	}

	public bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes)
	{
		return ReadPixels(dstInfo, dstPixels, dstRowBytes, 0, 0);
	}

	public bool ReadPixels(SKPixmap pixmap, int srcX, int srcY)
	{
		return ReadPixels(pixmap.Info, pixmap.GetPixels(), pixmap.RowBytes, srcX, srcY);
	}

	public bool ReadPixels(SKPixmap pixmap)
	{
		return ReadPixels(pixmap.Info, pixmap.GetPixels(), pixmap.RowBytes, 0, 0);
	}

	public SKData? Encode(SKEncodedImageFormat encoder, int quality)
	{
		using SKDynamicMemoryWStream sKDynamicMemoryWStream = new SKDynamicMemoryWStream();
		return Encode(sKDynamicMemoryWStream, encoder, quality) ? sKDynamicMemoryWStream.DetachAsData() : null;
	}

	public bool Encode(Stream dst, SKEncodedImageFormat encoder, int quality)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		using SKManagedWStream dst2 = new SKManagedWStream(dst);
		return Encode(dst2, encoder, quality);
	}

	public bool Encode(SKWStream dst, SKEncodedImageFormat encoder, int quality)
	{
		switch (encoder)
		{
		case SKEncodedImageFormat.Jpeg:
			return Encode(dst, new SKJpegEncoderOptions(quality));
		case SKEncodedImageFormat.Png:
			return Encode(dst, SKPngEncoderOptions.Default);
		case SKEncodedImageFormat.Webp:
			if (quality == 100)
			{
				return Encode(dst, new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossless, 75f));
			}
			return Encode(dst, new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, quality));
		default:
			return false;
		}
	}

	public SKData? Encode(SKWebpEncoderOptions options)
	{
		using SKDynamicMemoryWStream sKDynamicMemoryWStream = new SKDynamicMemoryWStream();
		return Encode(sKDynamicMemoryWStream, options) ? sKDynamicMemoryWStream.DetachAsData() : null;
	}

	public bool Encode(Stream dst, SKWebpEncoderOptions options)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		using SKManagedWStream dst2 = new SKManagedWStream(dst);
		return Encode(dst2, options);
	}

	public unsafe bool Encode(SKWStream dst, SKWebpEncoderOptions options)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		return SkiaApi.sk_webpencoder_encode(dst.Handle, Handle, &options);
	}

	public SKData? Encode(SKJpegEncoderOptions options)
	{
		using SKDynamicMemoryWStream sKDynamicMemoryWStream = new SKDynamicMemoryWStream();
		return Encode(sKDynamicMemoryWStream, options) ? sKDynamicMemoryWStream.DetachAsData() : null;
	}

	public bool Encode(Stream dst, SKJpegEncoderOptions options)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		using SKManagedWStream dst2 = new SKManagedWStream(dst);
		return Encode(dst2, options);
	}

	public unsafe bool Encode(SKWStream dst, SKJpegEncoderOptions options)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		return SkiaApi.sk_jpegencoder_encode(dst.Handle, Handle, &options);
	}

	public SKData? Encode(SKPngEncoderOptions options)
	{
		using SKDynamicMemoryWStream sKDynamicMemoryWStream = new SKDynamicMemoryWStream();
		return Encode(sKDynamicMemoryWStream, options) ? sKDynamicMemoryWStream.DetachAsData() : null;
	}

	public bool Encode(Stream dst, SKPngEncoderOptions options)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		using SKManagedWStream dst2 = new SKManagedWStream(dst);
		return Encode(dst2, options);
	}

	public unsafe bool Encode(SKWStream dst, SKPngEncoderOptions options)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		return SkiaApi.sk_pngencoder_encode(dst.Handle, Handle, &options);
	}

	public SKPixmap? ExtractSubset(SKRectI subset)
	{
		SKPixmap sKPixmap = new SKPixmap();
		if (!ExtractSubset(sKPixmap, subset))
		{
			sKPixmap.Dispose();
			sKPixmap = null;
		}
		return sKPixmap;
	}

	public unsafe bool ExtractSubset(SKPixmap result, SKRectI subset)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		return SkiaApi.sk_pixmap_extract_subset(Handle, result.Handle, &subset);
	}

	public bool Erase(SKColor color)
	{
		return Erase(color, Rect);
	}

	public unsafe bool Erase(SKColor color, SKRectI subset)
	{
		return SkiaApi.sk_pixmap_erase_color(Handle, (uint)color, &subset);
	}

	public bool Erase(SKColorF color)
	{
		return Erase(color, Rect);
	}

	public unsafe bool Erase(SKColorF color, SKRectI subset)
	{
		return SkiaApi.sk_pixmap_erase_color4f(Handle, &color, &subset);
	}

	public bool ComputeIsOpaque()
	{
		return SkiaApi.sk_pixmap_compute_is_opaque(Handle);
	}

	public SKPixmap WithColorType(SKColorType newColorType)
	{
		return new SKPixmap(Info.WithColorType(newColorType), GetPixels(), RowBytes);
	}

	public SKPixmap WithColorSpace(SKColorSpace newColorSpace)
	{
		return new SKPixmap(Info.WithColorSpace(newColorSpace), GetPixels(), RowBytes);
	}

	public SKPixmap WithAlphaType(SKAlphaType newAlphaType)
	{
		return new SKPixmap(Info.WithAlphaType(newAlphaType), GetPixels(), RowBytes);
	}
}
