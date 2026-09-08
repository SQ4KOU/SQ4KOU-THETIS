using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp;

public class SKImage : SKObject, ISKReferenceCounted
{
	public int Width => SkiaApi.sk_image_get_width(Handle);

	public int Height => SkiaApi.sk_image_get_height(Handle);

	public uint UniqueId => SkiaApi.sk_image_get_unique_id(Handle);

	public SKAlphaType AlphaType => SkiaApi.sk_image_get_alpha_type(Handle);

	public SKColorType ColorType => SkiaApi.sk_image_get_color_type(Handle).FromNative();

	public SKColorSpace ColorSpace => SKColorSpace.GetObject(SkiaApi.sk_image_get_colorspace(Handle));

	public bool IsAlphaOnly => SkiaApi.sk_image_is_alpha_only(Handle);

	public SKData EncodedData => SKData.GetObject(SkiaApi.sk_image_ref_encoded(Handle));

	public SKImageInfo Info => new SKImageInfo(Width, Height, ColorType, AlphaType, ColorSpace);

	public bool IsTextureBacked => SkiaApi.sk_image_is_texture_backed(Handle);

	public bool IsLazyGenerated => SkiaApi.sk_image_is_lazy_generated(Handle);

	internal SKImage(IntPtr x, bool owns)
		: base(x, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public unsafe static SKImage Create(SKImageInfo info)
	{
		IntPtr addr = Marshal.AllocCoTaskMem(info.BytesSize);
		using SKPixmap sKPixmap = new SKPixmap(info, addr);
		return GetObject(SkiaApi.sk_image_new_raster(sKPixmap.Handle, DelegateProxies.SKImageRasterReleaseProxyForCoTaskMem, null));
	}

	public static SKImage FromPixelCopy(SKImageInfo info, SKStream pixels)
	{
		return FromPixelCopy(info, pixels, info.RowBytes);
	}

	public static SKImage FromPixelCopy(SKImageInfo info, SKStream pixels, int rowBytes)
	{
		if (pixels == null)
		{
			throw new ArgumentNullException("pixels");
		}
		using SKData data = SKData.Create(pixels);
		return FromPixels(info, data, rowBytes);
	}

	public static SKImage FromPixelCopy(SKImageInfo info, Stream pixels)
	{
		return FromPixelCopy(info, pixels, info.RowBytes);
	}

	public static SKImage FromPixelCopy(SKImageInfo info, Stream pixels, int rowBytes)
	{
		if (pixels == null)
		{
			throw new ArgumentNullException("pixels");
		}
		using SKData data = SKData.Create(pixels);
		return FromPixels(info, data, rowBytes);
	}

	public static SKImage FromPixelCopy(SKImageInfo info, byte[] pixels)
	{
		return FromPixelCopy(info, pixels, info.RowBytes);
	}

	public static SKImage FromPixelCopy(SKImageInfo info, byte[] pixels, int rowBytes)
	{
		if (pixels == null)
		{
			throw new ArgumentNullException("pixels");
		}
		using SKData data = SKData.CreateCopy(pixels);
		return FromPixels(info, data, rowBytes);
	}

	public static SKImage FromPixelCopy(SKImageInfo info, IntPtr pixels)
	{
		return FromPixelCopy(info, pixels, info.RowBytes);
	}

	public unsafe static SKImage FromPixelCopy(SKImageInfo info, IntPtr pixels, int rowBytes)
	{
		if (pixels == IntPtr.Zero)
		{
			throw new ArgumentNullException("pixels");
		}
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		return GetObject(SkiaApi.sk_image_new_raster_copy(&sKImageInfoNative, (void*)pixels, (IntPtr)rowBytes));
	}

	public static SKImage FromPixelCopy(SKPixmap pixmap)
	{
		if (pixmap == null)
		{
			throw new ArgumentNullException("pixmap");
		}
		return GetObject(SkiaApi.sk_image_new_raster_copy_with_pixmap(pixmap.Handle));
	}

	public static SKImage FromPixelCopy(SKImageInfo info, ReadOnlySpan<byte> pixels)
	{
		return FromPixelCopy(info, pixels, info.RowBytes);
	}

	public static SKImage FromPixelCopy(SKImageInfo info, ReadOnlySpan<byte> pixels, int rowBytes)
	{
		if (pixels == null)
		{
			throw new ArgumentNullException("pixels");
		}
		using SKData data = SKData.CreateCopy(pixels);
		return FromPixels(info, data, rowBytes);
	}

	public static SKImage FromPixels(SKImageInfo info, SKData data)
	{
		return FromPixels(info, data, info.RowBytes);
	}

	public unsafe static SKImage FromPixels(SKImageInfo info, SKData data, int rowBytes)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref info);
		return GetObject(SkiaApi.sk_image_new_raster_data(&sKImageInfoNative, data.Handle, (IntPtr)rowBytes));
	}

	public static SKImage FromPixels(SKImageInfo info, IntPtr pixels)
	{
		using SKPixmap pixmap = new SKPixmap(info, pixels, info.RowBytes);
		return FromPixels(pixmap, null, null);
	}

	public static SKImage FromPixels(SKImageInfo info, IntPtr pixels, int rowBytes)
	{
		using SKPixmap pixmap = new SKPixmap(info, pixels, rowBytes);
		return FromPixels(pixmap, null, null);
	}

	public static SKImage FromPixels(SKPixmap pixmap)
	{
		return FromPixels(pixmap, null, null);
	}

	public static SKImage FromPixels(SKPixmap pixmap, SKImageRasterReleaseDelegate releaseProc)
	{
		return FromPixels(pixmap, releaseProc, null);
	}

	public unsafe static SKImage FromPixels(SKPixmap pixmap, SKImageRasterReleaseDelegate releaseProc, object releaseContext)
	{
		if (pixmap == null)
		{
			throw new ArgumentNullException("pixmap");
		}
		SKImageRasterReleaseDelegate sKImageRasterReleaseDelegate = ((releaseProc != null && releaseContext != null) ? ((SKImageRasterReleaseDelegate)delegate(IntPtr addr, object _)
		{
			releaseProc(addr, releaseContext);
		}) : releaseProc);
		DelegateProxies.Create(sKImageRasterReleaseDelegate, out var _, out var contextPtr);
		SKImageRasterReleaseProxyDelegate releaseProc2 = ((sKImageRasterReleaseDelegate != null) ? DelegateProxies.SKImageRasterReleaseProxy : null);
		return GetObject(SkiaApi.sk_image_new_raster(pixmap.Handle, releaseProc2, (void*)contextPtr));
	}

	public static SKImage FromEncodedData(SKData data, SKRectI subset)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return FromEncodedData(data)?.Subset(subset);
	}

	public static SKImage FromEncodedData(SKData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		IntPtr handle = SkiaApi.sk_image_new_from_encoded(data.Handle);
		return GetObject(handle);
	}

	public static SKImage FromEncodedData(ReadOnlySpan<byte> data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length == 0)
		{
			throw new ArgumentException("The data buffer was empty.");
		}
		using SKData data2 = SKData.CreateCopy(data);
		return FromEncodedData(data2);
	}

	public static SKImage FromEncodedData(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length == 0)
		{
			throw new ArgumentException("The data buffer was empty.");
		}
		using SKData data2 = SKData.CreateCopy(data);
		return FromEncodedData(data2);
	}

	public static SKImage FromEncodedData(SKStream data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		using SKData sKData = SKData.Create(data);
		if (sKData == null)
		{
			return null;
		}
		return FromEncodedData(sKData);
	}

	public static SKImage FromEncodedData(Stream data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		using SKData sKData = SKData.Create(data);
		if (sKData == null)
		{
			return null;
		}
		return FromEncodedData(sKData);
	}

	public static SKImage FromEncodedData(string filename)
	{
		if (filename == null)
		{
			throw new ArgumentNullException("filename");
		}
		using SKData sKData = SKData.Create(filename);
		if (sKData == null)
		{
			return null;
		}
		return FromEncodedData(sKData);
	}

	public static SKImage FromBitmap(SKBitmap bitmap)
	{
		if (bitmap == null)
		{
			throw new ArgumentNullException("bitmap");
		}
		SKImage result = GetObject(SkiaApi.sk_image_new_from_bitmap(bitmap.Handle));
		GC.KeepAlive(bitmap);
		return result;
	}

	public static SKImage FromTexture(GRContext context, GRBackendTexture texture, SKColorType colorType)
	{
		return FromTexture((GRRecordingContext)context, texture, colorType);
	}

	public static SKImage FromTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType)
	{
		return FromTexture((GRRecordingContext)context, texture, origin, colorType);
	}

	public static SKImage FromTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha)
	{
		return FromTexture((GRRecordingContext)context, texture, origin, colorType, alpha);
	}

	public static SKImage FromTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace)
	{
		return FromTexture((GRRecordingContext)context, texture, origin, colorType, alpha, colorspace);
	}

	public static SKImage FromTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc)
	{
		return FromTexture((GRRecordingContext)context, texture, origin, colorType, alpha, colorspace, releaseProc);
	}

	public static SKImage FromTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc, object releaseContext)
	{
		return FromTexture((GRRecordingContext)context, texture, origin, colorType, alpha, colorspace, releaseProc, releaseContext);
	}

	public static SKImage FromTexture(GRRecordingContext context, GRBackendTexture texture, SKColorType colorType)
	{
		return FromTexture(context, texture, GRSurfaceOrigin.BottomLeft, colorType, SKAlphaType.Premul, null, null, null);
	}

	public static SKImage FromTexture(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType)
	{
		return FromTexture(context, texture, origin, colorType, SKAlphaType.Premul, null, null, null);
	}

	public static SKImage FromTexture(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha)
	{
		return FromTexture(context, texture, origin, colorType, alpha, null, null, null);
	}

	public static SKImage FromTexture(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace)
	{
		return FromTexture(context, texture, origin, colorType, alpha, colorspace, null, null);
	}

	public static SKImage FromTexture(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc)
	{
		return FromTexture(context, texture, origin, colorType, alpha, colorspace, releaseProc, null);
	}

	public unsafe static SKImage FromTexture(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc, object releaseContext)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		IntPtr colorSpace = colorspace?.Handle ?? IntPtr.Zero;
		SKImageTextureReleaseDelegate sKImageTextureReleaseDelegate = ((releaseProc != null && releaseContext != null) ? ((SKImageTextureReleaseDelegate)delegate
		{
			releaseProc(releaseContext);
		}) : releaseProc);
		DelegateProxies.Create(sKImageTextureReleaseDelegate, out var _, out var contextPtr);
		SKImageTextureReleaseProxyDelegate releaseProc2 = ((sKImageTextureReleaseDelegate != null) ? DelegateProxies.SKImageTextureReleaseProxy : null);
		return GetObject(SkiaApi.sk_image_new_from_texture(context.Handle, texture.Handle, origin, colorType.ToNative(), alpha, colorSpace, releaseProc2, (void*)contextPtr));
	}

	public static SKImage FromAdoptedTexture(GRContext context, GRBackendTexture texture, SKColorType colorType)
	{
		return FromAdoptedTexture((GRRecordingContext)context, texture, colorType);
	}

	public static SKImage FromAdoptedTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType)
	{
		return FromAdoptedTexture((GRRecordingContext)context, texture, origin, colorType);
	}

	public static SKImage FromAdoptedTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha)
	{
		return FromAdoptedTexture((GRRecordingContext)context, texture, origin, colorType, alpha);
	}

	public static SKImage FromAdoptedTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace)
	{
		return FromAdoptedTexture((GRRecordingContext)context, texture, origin, colorType, alpha, colorspace);
	}

	public static SKImage FromAdoptedTexture(GRRecordingContext context, GRBackendTexture texture, SKColorType colorType)
	{
		return FromAdoptedTexture(context, texture, GRSurfaceOrigin.BottomLeft, colorType, SKAlphaType.Premul, null);
	}

	public static SKImage FromAdoptedTexture(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType)
	{
		return FromAdoptedTexture(context, texture, origin, colorType, SKAlphaType.Premul, null);
	}

	public static SKImage FromAdoptedTexture(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha)
	{
		return FromAdoptedTexture(context, texture, origin, colorType, alpha, null);
	}

	public static SKImage FromAdoptedTexture(GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		IntPtr colorSpace = colorspace?.Handle ?? IntPtr.Zero;
		return GetObject(SkiaApi.sk_image_new_from_adopted_texture(context.Handle, texture.Handle, origin, colorType.ToNative(), alpha, colorSpace));
	}

	public unsafe static SKImage FromPicture(SKPicture picture, SKSizeI dimensions)
	{
		return FromPicture(picture, dimensions, null, null, useFloatingPointBitDepth: false, SKColorSpace.CreateSrgb(), null);
	}

	public unsafe static SKImage FromPicture(SKPicture picture, SKSizeI dimensions, SKMatrix matrix)
	{
		return FromPicture(picture, dimensions, &matrix, null, useFloatingPointBitDepth: false, SKColorSpace.CreateSrgb(), null);
	}

	public unsafe static SKImage FromPicture(SKPicture picture, SKSizeI dimensions, SKPaint paint)
	{
		return FromPicture(picture, dimensions, null, paint, useFloatingPointBitDepth: false, SKColorSpace.CreateSrgb(), null);
	}

	public unsafe static SKImage FromPicture(SKPicture picture, SKSizeI dimensions, SKMatrix matrix, SKPaint paint)
	{
		return FromPicture(picture, dimensions, &matrix, paint, useFloatingPointBitDepth: false, SKColorSpace.CreateSrgb(), null);
	}

	private unsafe static SKImage FromPicture(SKPicture picture, SKSizeI dimensions, SKMatrix* matrix, SKPaint paint, bool useFloatingPointBitDepth, SKColorSpace colorspace, SKSurfaceProperties props)
	{
		if (picture == null)
		{
			throw new ArgumentNullException("picture");
		}
		IntPtr paint2 = paint?.Handle ?? IntPtr.Zero;
		IntPtr colorSpace = colorspace?.Handle ?? IntPtr.Zero;
		IntPtr props2 = props?.Handle ?? IntPtr.Zero;
		return GetObject(SkiaApi.sk_image_new_from_picture(picture.Handle, &dimensions, matrix, paint2, useFloatingPointBitDepth, colorSpace, props2));
	}

	public SKData Encode()
	{
		if (EncodedData != null)
		{
			return EncodedData;
		}
		return Encode(SKEncodedImageFormat.Png, 100);
	}

	public SKData Encode(SKEncodedImageFormat format, int quality)
	{
		SKImage sKImage = ToRasterImage(ensurePixelData: true);
		try
		{
			using SKPixmap sKPixmap = sKImage.PeekPixels();
			return sKPixmap?.Encode(format, quality);
		}
		finally
		{
			if (this != sKImage)
			{
				sKImage.Dispose();
			}
		}
	}

	public unsafe SKShader ToShader()
	{
		return ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, SKSamplingOptions.Default, null);
	}

	public unsafe SKShader ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY)
	{
		return ToShader(tileX, tileY, SKSamplingOptions.Default, null);
	}

	public unsafe SKShader ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKMatrix localMatrix)
	{
		return ToShader(tileX, tileY, SKSamplingOptions.Default, &localMatrix);
	}

	public unsafe SKShader ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling)
	{
		return ToShader(tileX, tileY, sampling, null);
	}

	[Obsolete("Use ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling) instead.")]
	public unsafe SKShader ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKFilterQuality quality)
	{
		return ToShader(tileX, tileY, quality.ToSamplingOptions(), null);
	}

	public unsafe SKShader ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling, SKMatrix localMatrix)
	{
		return ToShader(tileX, tileY, sampling, &localMatrix);
	}

	[Obsolete("Use ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling, SKMatrix localMatrix) instead.")]
	public unsafe SKShader ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKFilterQuality quality, SKMatrix localMatrix)
	{
		return ToShader(tileX, tileY, quality.ToSamplingOptions(), &localMatrix);
	}

	private unsafe SKShader ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling, SKMatrix* localMatrix)
	{
		return SKShader.GetObject(SkiaApi.sk_image_make_shader(Handle, tileX, tileY, &sampling, localMatrix));
	}

	public unsafe SKShader ToRawShader()
	{
		return ToRawShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, SKSamplingOptions.Default, null);
	}

	public unsafe SKShader ToRawShader(SKShaderTileMode tileX, SKShaderTileMode tileY)
	{
		return ToRawShader(tileX, tileY, SKSamplingOptions.Default, null);
	}

	public unsafe SKShader ToRawShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKMatrix localMatrix)
	{
		return ToRawShader(tileX, tileY, SKSamplingOptions.Default, &localMatrix);
	}

	public unsafe SKShader ToRawShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling)
	{
		return ToRawShader(tileX, tileY, sampling, null);
	}

	public unsafe SKShader ToRawShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling, SKMatrix localMatrix)
	{
		return ToRawShader(tileX, tileY, sampling, &localMatrix);
	}

	private unsafe SKShader ToRawShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling, SKMatrix* localMatrix)
	{
		return SKShader.GetObject(SkiaApi.sk_image_make_raw_shader(Handle, tileX, tileY, &sampling, localMatrix));
	}

	public bool PeekPixels(SKPixmap pixmap)
	{
		if (pixmap == null)
		{
			throw new ArgumentNullException("pixmap");
		}
		bool flag = SkiaApi.sk_image_peek_pixels(Handle, pixmap.Handle);
		if (flag)
		{
			pixmap.pixelSource = this;
		}
		return flag;
	}

	public SKPixmap PeekPixels()
	{
		SKPixmap sKPixmap = new SKPixmap();
		if (!PeekPixels(sKPixmap))
		{
			sKPixmap.Dispose();
			sKPixmap = null;
		}
		return sKPixmap;
	}

	public bool IsValid(GRContext context)
	{
		return IsValid((GRRecordingContext)context);
	}

	public bool IsValid(GRRecordingContext context)
	{
		return SkiaApi.sk_image_is_valid(Handle, context?.Handle ?? IntPtr.Zero);
	}

	public bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels)
	{
		return ReadPixels(dstInfo, dstPixels, dstInfo.RowBytes, 0, 0, SKImageCachingHint.Allow);
	}

	public bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes)
	{
		return ReadPixels(dstInfo, dstPixels, dstRowBytes, 0, 0, SKImageCachingHint.Allow);
	}

	public bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
	{
		return ReadPixels(dstInfo, dstPixels, dstRowBytes, srcX, srcY, SKImageCachingHint.Allow);
	}

	public unsafe bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKImageCachingHint cachingHint)
	{
		SKImageInfoNative sKImageInfoNative = SKImageInfoNative.FromManaged(ref dstInfo);
		bool result = SkiaApi.sk_image_read_pixels(Handle, &sKImageInfoNative, (void*)dstPixels, (IntPtr)dstRowBytes, srcX, srcY, cachingHint);
		GC.KeepAlive(this);
		return result;
	}

	public bool ReadPixels(SKPixmap pixmap)
	{
		return ReadPixels(pixmap, 0, 0, SKImageCachingHint.Allow);
	}

	public bool ReadPixels(SKPixmap pixmap, int srcX, int srcY)
	{
		return ReadPixels(pixmap, srcX, srcY, SKImageCachingHint.Allow);
	}

	public bool ReadPixels(SKPixmap pixmap, int srcX, int srcY, SKImageCachingHint cachingHint)
	{
		if (pixmap == null)
		{
			throw new ArgumentNullException("pixmap");
		}
		bool result = SkiaApi.sk_image_read_pixels_into_pixmap(Handle, pixmap.Handle, srcX, srcY, cachingHint);
		GC.KeepAlive(this);
		return result;
	}

	[Obsolete("Use ScalePixels(SKPixmap dst, SKSamplingOptions sampling) instead.")]
	public bool ScalePixels(SKPixmap dst, SKFilterQuality quality)
	{
		return ScalePixels(dst, quality.ToSamplingOptions());
	}

	[Obsolete("Use ScalePixels(SKPixmap dst, SKSamplingOptions sampling, SKImageCachingHint cachingHint) instead.")]
	public bool ScalePixels(SKPixmap dst, SKFilterQuality quality, SKImageCachingHint cachingHint)
	{
		return ScalePixels(dst, quality.ToSamplingOptions(), cachingHint);
	}

	public bool ScalePixels(SKPixmap dst, SKSamplingOptions sampling)
	{
		return ScalePixels(dst, sampling, SKImageCachingHint.Allow);
	}

	public unsafe bool ScalePixels(SKPixmap dst, SKSamplingOptions sampling, SKImageCachingHint cachingHint)
	{
		if (dst == null)
		{
			throw new ArgumentNullException("dst");
		}
		return SkiaApi.sk_image_scale_pixels(Handle, dst.Handle, &sampling, cachingHint);
	}

	public unsafe SKImage Subset(SKRectI subset)
	{
		return GetObject(SkiaApi.sk_image_make_subset_raster(Handle, &subset));
	}

	public unsafe SKImage Subset(GRRecordingContext context, SKRectI subset)
	{
		return GetObject(SkiaApi.sk_image_make_subset(Handle, context?.Handle ?? IntPtr.Zero, &subset));
	}

	public SKImage ToRasterImage()
	{
		return ToRasterImage(ensurePixelData: false);
	}

	public SKImage ToRasterImage(bool ensurePixelData)
	{
		if (!ensurePixelData)
		{
			return GetObject(SkiaApi.sk_image_make_non_texture_image(Handle));
		}
		return GetObject(SkiaApi.sk_image_make_raster_image(Handle));
	}

	public SKImage ToTextureImage(GRContext context)
	{
		return ToTextureImage(context, mipmapped: false, budgeted: true);
	}

	public SKImage ToTextureImage(GRContext context, bool mipmapped)
	{
		return ToTextureImage(context, mipmapped, budgeted: true);
	}

	public SKImage ToTextureImage(GRContext context, bool mipmapped, bool budgeted)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		return GetObject(SkiaApi.sk_image_make_texture_image(Handle, context.Handle, mipmapped, budgeted));
	}

	public SKImage ApplyImageFilter(SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPoint outOffset)
	{
		SKImage result = ApplyImageFilter(filter, subset, clipBounds, out outSubset, out SKPointI outOffset2);
		outOffset = outOffset2;
		return result;
	}

	public unsafe SKImage ApplyImageFilter(SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPointI outOffset)
	{
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		fixed (SKRectI* outSubset2 = &outSubset)
		{
			fixed (SKPointI* outOffset2 = &outOffset)
			{
				return GetObject(SkiaApi.sk_image_make_with_filter_raster(Handle, filter.Handle, &subset, &clipBounds, outSubset2, outOffset2));
			}
		}
	}

	public SKImage ApplyImageFilter(GRContext context, SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPointI outOffset)
	{
		return ApplyImageFilter((GRRecordingContext)context, filter, subset, clipBounds, out outSubset, out outOffset);
	}

	public unsafe SKImage ApplyImageFilter(GRRecordingContext context, SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPointI outOffset)
	{
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		fixed (SKRectI* outSubset2 = &outSubset)
		{
			fixed (SKPointI* outOffset2 = &outOffset)
			{
				return GetObject(SkiaApi.sk_image_make_with_filter(Handle, context?.Handle ?? IntPtr.Zero, filter.Handle, &subset, &clipBounds, outSubset2, outOffset2));
			}
		}
	}

	internal static SKImage GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKImage(h, o));
	}
}
