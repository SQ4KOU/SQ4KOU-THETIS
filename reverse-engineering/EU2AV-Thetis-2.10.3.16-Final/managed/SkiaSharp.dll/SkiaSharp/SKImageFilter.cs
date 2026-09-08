using System;

namespace SkiaSharp;

public class SKImageFilter : SKObject, ISKReferenceCounted
{
	internal SKImageFilter(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	[Obsolete("Use SetMatrix(in SKMatrix) instead.", true)]
	public static SKImageFilter CreateMatrix(SKMatrix matrix)
	{
		return CreateMatrix(in matrix);
	}

	[Obsolete("Use SetMatrix(in SKMatrix, SKSamplingOptions, SKImageFilter) instead.", true)]
	public static SKImageFilter CreateMatrix(SKMatrix matrix, SKFilterQuality quality, SKImageFilter? input)
	{
		return CreateMatrix(in matrix, quality.ToSamplingOptions(), input);
	}

	public static SKImageFilter CreateMatrix(in SKMatrix matrix)
	{
		return CreateMatrix(in matrix, SKSamplingOptions.Default, null);
	}

	public static SKImageFilter CreateMatrix(in SKMatrix matrix, SKSamplingOptions sampling)
	{
		return CreateMatrix(in matrix, sampling, null);
	}

	public unsafe static SKImageFilter CreateMatrix(in SKMatrix matrix, SKSamplingOptions sampling, SKImageFilter? input)
	{
		fixed (SKMatrix* cmatrix = &matrix)
		{
			return GetObject(SkiaApi.sk_imagefilter_new_matrix_transform(cmatrix, &sampling, input?.Handle ?? IntPtr.Zero));
		}
	}

	public unsafe static SKImageFilter CreateBlur(float sigmaX, float sigmaY)
	{
		return CreateBlur(sigmaX, sigmaY, SKShaderTileMode.Decal, null, null);
	}

	public unsafe static SKImageFilter CreateBlur(float sigmaX, float sigmaY, SKImageFilter? input)
	{
		return CreateBlur(sigmaX, sigmaY, SKShaderTileMode.Decal, input, null);
	}

	public unsafe static SKImageFilter CreateBlur(float sigmaX, float sigmaY, SKImageFilter? input, SKRect cropRect)
	{
		return CreateBlur(sigmaX, sigmaY, SKShaderTileMode.Decal, input, &cropRect);
	}

	public unsafe static SKImageFilter CreateBlur(float sigmaX, float sigmaY, SKShaderTileMode tileMode)
	{
		return CreateBlur(sigmaX, sigmaY, tileMode, null, null);
	}

	public unsafe static SKImageFilter CreateBlur(float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter? input)
	{
		return CreateBlur(sigmaX, sigmaY, tileMode, input, null);
	}

	public unsafe static SKImageFilter CreateBlur(float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter? input, SKRect cropRect)
	{
		return CreateBlur(sigmaX, sigmaY, tileMode, input, &cropRect);
	}

	private unsafe static SKImageFilter CreateBlur(float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter? input, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_blur(sigmaX, sigmaY, tileMode, input?.Handle ?? IntPtr.Zero, cropRect));
	}

	public unsafe static SKImageFilter CreateColorFilter(SKColorFilter cf)
	{
		return CreateColorFilter(cf, null, null);
	}

	public unsafe static SKImageFilter CreateColorFilter(SKColorFilter cf, SKImageFilter? input)
	{
		return CreateColorFilter(cf, input, null);
	}

	public unsafe static SKImageFilter CreateColorFilter(SKColorFilter cf, SKImageFilter? input, SKRect cropRect)
	{
		return CreateColorFilter(cf, input, &cropRect);
	}

	private unsafe static SKImageFilter CreateColorFilter(SKColorFilter cf, SKImageFilter? input, SKRect* cropRect)
	{
		if (cf == null)
		{
			throw new ArgumentNullException("cf");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_color_filter(cf.Handle, input?.Handle ?? IntPtr.Zero, cropRect));
	}

	public static SKImageFilter CreateCompose(SKImageFilter outer, SKImageFilter inner)
	{
		if (outer == null)
		{
			throw new ArgumentNullException("outer");
		}
		if (inner == null)
		{
			throw new ArgumentNullException("inner");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_compose(outer.Handle, inner.Handle));
	}

	public unsafe static SKImageFilter CreateDisplacementMapEffect(SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement)
	{
		return CreateDisplacementMapEffect(xChannelSelector, yChannelSelector, scale, displacement, null, null);
	}

	public unsafe static SKImageFilter CreateDisplacementMapEffect(SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter? input)
	{
		return CreateDisplacementMapEffect(xChannelSelector, yChannelSelector, scale, displacement, input, null);
	}

	public unsafe static SKImageFilter CreateDisplacementMapEffect(SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter? input, SKRect cropRect)
	{
		return CreateDisplacementMapEffect(xChannelSelector, yChannelSelector, scale, displacement, input, &cropRect);
	}

	private unsafe static SKImageFilter CreateDisplacementMapEffect(SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter? input, SKRect* cropRect)
	{
		if (displacement == null)
		{
			throw new ArgumentNullException("displacement");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_displacement_map_effect(xChannelSelector, yChannelSelector, scale, displacement.Handle, input?.Handle ?? IntPtr.Zero, cropRect));
	}

	public unsafe static SKImageFilter CreateDropShadow(float dx, float dy, float sigmaX, float sigmaY, SKColor color)
	{
		return CreateDropShadow(dx, dy, sigmaX, sigmaY, color, null, null);
	}

	public unsafe static SKImageFilter CreateDropShadow(float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input)
	{
		return CreateDropShadow(dx, dy, sigmaX, sigmaY, color, input, null);
	}

	public unsafe static SKImageFilter CreateDropShadow(float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input, SKRect cropRect)
	{
		return CreateDropShadow(dx, dy, sigmaX, sigmaY, color, input, &cropRect);
	}

	private unsafe static SKImageFilter CreateDropShadow(float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_drop_shadow(dx, dy, sigmaX, sigmaY, (uint)color, input?.Handle ?? IntPtr.Zero, cropRect));
	}

	public unsafe static SKImageFilter CreateDropShadowOnly(float dx, float dy, float sigmaX, float sigmaY, SKColor color)
	{
		return CreateDropShadowOnly(dx, dy, sigmaX, sigmaY, color, null, null);
	}

	public unsafe static SKImageFilter CreateDropShadowOnly(float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input)
	{
		return CreateDropShadowOnly(dx, dy, sigmaX, sigmaY, color, input, null);
	}

	public unsafe static SKImageFilter CreateDropShadowOnly(float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input, SKRect cropRect)
	{
		return CreateDropShadowOnly(dx, dy, sigmaX, sigmaY, color, input, &cropRect);
	}

	private unsafe static SKImageFilter CreateDropShadowOnly(float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_drop_shadow_only(dx, dy, sigmaX, sigmaY, (uint)color, input?.Handle ?? IntPtr.Zero, cropRect));
	}

	public unsafe static SKImageFilter CreateDistantLitDiffuse(SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd)
	{
		return CreateDistantLitDiffuse(direction, lightColor, surfaceScale, kd, null, null);
	}

	public unsafe static SKImageFilter CreateDistantLitDiffuse(SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input)
	{
		return CreateDistantLitDiffuse(direction, lightColor, surfaceScale, kd, input, null);
	}

	public unsafe static SKImageFilter CreateDistantLitDiffuse(SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect cropRect)
	{
		return CreateDistantLitDiffuse(direction, lightColor, surfaceScale, kd, input, &cropRect);
	}

	private unsafe static SKImageFilter CreateDistantLitDiffuse(SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_distant_lit_diffuse(&direction, (uint)lightColor, surfaceScale, kd, input?.Handle ?? IntPtr.Zero, cropRect));
	}

	public unsafe static SKImageFilter CreatePointLitDiffuse(SKPoint3 location, SKColor lightColor, float surfaceScale, float kd)
	{
		return CreatePointLitDiffuse(location, lightColor, surfaceScale, kd, null, null);
	}

	public unsafe static SKImageFilter CreatePointLitDiffuse(SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input)
	{
		return CreatePointLitDiffuse(location, lightColor, surfaceScale, kd, input, null);
	}

	public unsafe static SKImageFilter CreatePointLitDiffuse(SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect cropRect)
	{
		return CreatePointLitDiffuse(location, lightColor, surfaceScale, kd, input, &cropRect);
	}

	private unsafe static SKImageFilter CreatePointLitDiffuse(SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_point_lit_diffuse(&location, (uint)lightColor, surfaceScale, kd, input?.Handle ?? IntPtr.Zero, cropRect));
	}

	public unsafe static SKImageFilter CreateSpotLitDiffuse(SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd)
	{
		return CreateSpotLitDiffuse(location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, kd, null, null);
	}

	public unsafe static SKImageFilter CreateSpotLitDiffuse(SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input)
	{
		return CreateSpotLitDiffuse(location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, kd, input, null);
	}

	public unsafe static SKImageFilter CreateSpotLitDiffuse(SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect cropRect)
	{
		return CreateSpotLitDiffuse(location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, kd, input, &cropRect);
	}

	private unsafe static SKImageFilter CreateSpotLitDiffuse(SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_spot_lit_diffuse(&location, &target, specularExponent, cutoffAngle, (uint)lightColor, surfaceScale, kd, input?.Handle ?? IntPtr.Zero, cropRect));
	}

	public unsafe static SKImageFilter CreateDistantLitSpecular(SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess)
	{
		return CreateDistantLitSpecular(direction, lightColor, surfaceScale, ks, shininess, null, null);
	}

	public unsafe static SKImageFilter CreateDistantLitSpecular(SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input)
	{
		return CreateDistantLitSpecular(direction, lightColor, surfaceScale, ks, shininess, input, null);
	}

	public unsafe static SKImageFilter CreateDistantLitSpecular(SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect cropRect)
	{
		return CreateDistantLitSpecular(direction, lightColor, surfaceScale, ks, shininess, input, &cropRect);
	}

	private unsafe static SKImageFilter CreateDistantLitSpecular(SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_distant_lit_specular(&direction, (uint)lightColor, surfaceScale, ks, shininess, input?.Handle ?? IntPtr.Zero, cropRect));
	}

	public unsafe static SKImageFilter CreatePointLitSpecular(SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess)
	{
		return CreatePointLitSpecular(location, lightColor, surfaceScale, ks, shininess, null, null);
	}

	public unsafe static SKImageFilter CreatePointLitSpecular(SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input)
	{
		return CreatePointLitSpecular(location, lightColor, surfaceScale, ks, shininess, input, null);
	}

	public unsafe static SKImageFilter CreatePointLitSpecular(SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect cropRect)
	{
		return CreatePointLitSpecular(location, lightColor, surfaceScale, ks, shininess, input, &cropRect);
	}

	private unsafe static SKImageFilter CreatePointLitSpecular(SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_point_lit_specular(&location, (uint)lightColor, surfaceScale, ks, shininess, input?.Handle ?? IntPtr.Zero, cropRect));
	}

	public unsafe static SKImageFilter CreateSpotLitSpecular(SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess)
	{
		return CreateSpotLitSpecular(location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, ks, shininess, null, null);
	}

	public unsafe static SKImageFilter CreateSpotLitSpecular(SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input)
	{
		return CreateSpotLitSpecular(location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, ks, shininess, input, null);
	}

	public unsafe static SKImageFilter CreateSpotLitSpecular(SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect cropRect)
	{
		return CreateSpotLitSpecular(location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, ks, shininess, input, &cropRect);
	}

	private unsafe static SKImageFilter CreateSpotLitSpecular(SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_spot_lit_specular(&location, &target, specularExponent, cutoffAngle, (uint)lightColor, surfaceScale, ks, shininess, input?.Handle ?? IntPtr.Zero, cropRect));
	}

	public unsafe static SKImageFilter CreateMatrixConvolution(SKSizeI kernelSize, ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha)
	{
		return CreateMatrixConvolution(kernelSize, kernel, gain, bias, kernelOffset, tileMode, convolveAlpha, null, null);
	}

	public unsafe static SKImageFilter CreateMatrixConvolution(SKSizeI kernelSize, ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter? input)
	{
		return CreateMatrixConvolution(kernelSize, kernel, gain, bias, kernelOffset, tileMode, convolveAlpha, input, null);
	}

	public unsafe static SKImageFilter CreateMatrixConvolution(SKSizeI kernelSize, ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter? input, SKRect cropRect)
	{
		return CreateMatrixConvolution(kernelSize, kernel, gain, bias, kernelOffset, tileMode, convolveAlpha, input, &cropRect);
	}

	private unsafe static SKImageFilter CreateMatrixConvolution(SKSizeI kernelSize, ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter? input, SKRect* cropRect)
	{
		if (kernel.Length != kernelSize.Width * kernelSize.Height)
		{
			throw new ArgumentException("Kernel length must match the dimensions of the kernel size (Width * Height).", "kernel");
		}
		fixed (float* kernel2 = kernel)
		{
			return GetObject(SkiaApi.sk_imagefilter_new_matrix_convolution(&kernelSize, kernel2, gain, bias, &kernelOffset, tileMode, convolveAlpha, input?.Handle ?? IntPtr.Zero, cropRect));
		}
	}

	public unsafe static SKImageFilter CreateMerge(SKImageFilter? first, SKImageFilter? second)
	{
		return CreateMerge(first, second, null);
	}

	public unsafe static SKImageFilter CreateMerge(SKImageFilter? first, SKImageFilter? second, SKRect cropRect)
	{
		return CreateMerge(first, second, &cropRect);
	}

	private unsafe static SKImageFilter CreateMerge(SKImageFilter? first, SKImageFilter? second, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_merge_simple(first?.Handle ?? IntPtr.Zero, second?.Handle ?? IntPtr.Zero, cropRect));
	}

	public unsafe static SKImageFilter CreateMerge(ReadOnlySpan<SKImageFilter> filters)
	{
		return CreateMerge(filters, null);
	}

	public unsafe static SKImageFilter CreateMerge(ReadOnlySpan<SKImageFilter> filters, SKRect cropRect)
	{
		return CreateMerge(filters, &cropRect);
	}

	public unsafe static SKImageFilter CreateMerge(ReadOnlySpan<SKImageFilter> filters, SKRect* cropRect)
	{
		IntPtr[] array = new IntPtr[filters.Length];
		for (int i = 0; i < filters.Length; i++)
		{
			array[i] = filters[i]?.Handle ?? IntPtr.Zero;
		}
		fixed (IntPtr* cfilters = array)
		{
			return GetObject(SkiaApi.sk_imagefilter_new_merge(cfilters, filters.Length, cropRect));
		}
	}

	public unsafe static SKImageFilter CreateDilate(float radiusX, float radiusY)
	{
		return CreateDilate(radiusX, radiusY, null, null);
	}

	public unsafe static SKImageFilter CreateDilate(float radiusX, float radiusY, SKImageFilter? input)
	{
		return CreateDilate(radiusX, radiusY, input, null);
	}

	public unsafe static SKImageFilter CreateDilate(float radiusX, float radiusY, SKImageFilter? input, SKRect cropRect)
	{
		return CreateDilate(radiusX, radiusY, input, &cropRect);
	}

	private unsafe static SKImageFilter CreateDilate(float radiusX, float radiusY, SKImageFilter? input, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_dilate(radiusX, radiusY, input?.Handle ?? IntPtr.Zero, cropRect));
	}

	public unsafe static SKImageFilter CreateErode(float radiusX, float radiusY)
	{
		return CreateErode(radiusX, radiusY, null, null);
	}

	public unsafe static SKImageFilter CreateErode(float radiusX, float radiusY, SKImageFilter? input)
	{
		return CreateErode(radiusX, radiusY, input, null);
	}

	public unsafe static SKImageFilter CreateErode(float radiusX, float radiusY, SKImageFilter? input, SKRect cropRect)
	{
		return CreateErode(radiusX, radiusY, input, &cropRect);
	}

	private unsafe static SKImageFilter CreateErode(float radiusX, float radiusY, SKImageFilter? input, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_erode(radiusX, radiusY, input?.Handle ?? IntPtr.Zero, cropRect));
	}

	public unsafe static SKImageFilter CreateOffset(float radiusX, float radiusY)
	{
		return CreateOffset(radiusX, radiusY, null, null);
	}

	public unsafe static SKImageFilter CreateOffset(float radiusX, float radiusY, SKImageFilter? input)
	{
		return CreateOffset(radiusX, radiusY, input, null);
	}

	public unsafe static SKImageFilter CreateOffset(float radiusX, float radiusY, SKImageFilter? input, SKRect cropRect)
	{
		return CreateOffset(radiusX, radiusY, input, &cropRect);
	}

	private unsafe static SKImageFilter CreateOffset(float radiusX, float radiusY, SKImageFilter? input, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_offset(radiusX, radiusY, input?.Handle ?? IntPtr.Zero, cropRect));
	}

	public static SKImageFilter CreatePicture(SKPicture picture)
	{
		if (picture == null)
		{
			throw new ArgumentNullException("picture");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_picture(picture.Handle));
	}

	public unsafe static SKImageFilter CreatePicture(SKPicture picture, SKRect cropRect)
	{
		if (picture == null)
		{
			throw new ArgumentNullException("picture");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_picture_with_rect(picture.Handle, &cropRect));
	}

	public static SKImageFilter CreateTile(SKRect src, SKRect dst)
	{
		return CreateTile(src, dst, null);
	}

	public unsafe static SKImageFilter CreateTile(SKRect src, SKRect dst, SKImageFilter? input)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_tile(&src, &dst, input.Handle));
	}

	public unsafe static SKImageFilter CreateBlendMode(SKBlendMode mode, SKImageFilter? background)
	{
		return CreateBlendMode(mode, background, null, null);
	}

	public unsafe static SKImageFilter CreateBlendMode(SKBlendMode mode, SKImageFilter? background, SKImageFilter? foreground)
	{
		return CreateBlendMode(mode, background, foreground, null);
	}

	public unsafe static SKImageFilter CreateBlendMode(SKBlendMode mode, SKImageFilter? background, SKImageFilter? foreground, SKRect cropRect)
	{
		return CreateBlendMode(mode, background, foreground, &cropRect);
	}

	private unsafe static SKImageFilter CreateBlendMode(SKBlendMode mode, SKImageFilter? background, SKImageFilter? foreground, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_blend(mode, background?.Handle ?? IntPtr.Zero, foreground?.Handle ?? IntPtr.Zero, cropRect));
	}

	public unsafe static SKImageFilter CreateBlendMode(SKBlender blender, SKImageFilter? background)
	{
		return CreateBlendMode(blender, background, null, null);
	}

	public unsafe static SKImageFilter CreateBlendMode(SKBlender blender, SKImageFilter? background, SKImageFilter? foreground)
	{
		return CreateBlendMode(blender, background, foreground, null);
	}

	public unsafe static SKImageFilter CreateBlendMode(SKBlender blender, SKImageFilter? background, SKImageFilter? foreground, SKRect cropRect)
	{
		return CreateBlendMode(blender, background, foreground, &cropRect);
	}

	private unsafe static SKImageFilter CreateBlendMode(SKBlender blender, SKImageFilter? background, SKImageFilter? foreground, SKRect* cropRect)
	{
		if (blender == null)
		{
			throw new ArgumentNullException("blender");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_blender(blender.Handle, background?.Handle ?? IntPtr.Zero, foreground?.Handle ?? IntPtr.Zero, cropRect));
	}

	public unsafe static SKImageFilter CreateArithmetic(float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter? background)
	{
		return CreateArithmetic(k1, k2, k3, k4, enforcePMColor, background, null, null);
	}

	public unsafe static SKImageFilter CreateArithmetic(float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter? background, SKImageFilter? foreground)
	{
		return CreateArithmetic(k1, k2, k3, k4, enforcePMColor, background, foreground, null);
	}

	public unsafe static SKImageFilter CreateArithmetic(float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter? background, SKImageFilter? foreground, SKRect cropRect)
	{
		return CreateArithmetic(k1, k2, k3, k4, enforcePMColor, background, foreground, &cropRect);
	}

	private unsafe static SKImageFilter CreateArithmetic(float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter? background, SKImageFilter? foreground, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_arithmetic(k1, k2, k3, k4, enforcePMColor, background?.Handle ?? IntPtr.Zero, foreground?.Handle ?? IntPtr.Zero, cropRect));
	}

	public static SKImageFilter CreateImage(SKImage image)
	{
		return CreateImage(image, new SKSamplingOptions(SKCubicResampler.Mitchell));
	}

	public unsafe static SKImageFilter CreateImage(SKImage image, SKSamplingOptions sampling)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_image_simple(image.Handle, &sampling));
	}

	public unsafe static SKImageFilter CreateImage(SKImage image, SKRect src, SKRect dst, SKSamplingOptions sampling)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		return GetObject(SkiaApi.sk_imagefilter_new_image(image.Handle, &src, &dst, &sampling));
	}

	[Obsolete("Use CreateImage(SKImage, SKRect, SKRect, SKSamplingOptions) instead.", true)]
	public static SKImageFilter CreateImage(SKImage image, SKRect src, SKRect dst, SKFilterQuality filterQuality)
	{
		return CreateImage(image, src, dst, filterQuality.ToSamplingOptions());
	}

	public unsafe static SKImageFilter CreateMagnifier(SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling)
	{
		return CreateMagnifier(lensBounds, zoomAmount, inset, sampling, null, null);
	}

	public unsafe static SKImageFilter CreateMagnifier(SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling, SKImageFilter? input)
	{
		return CreateMagnifier(lensBounds, zoomAmount, inset, sampling, input, null);
	}

	public unsafe static SKImageFilter CreateMagnifier(SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling, SKImageFilter? input, SKRect cropRect)
	{
		return CreateMagnifier(lensBounds, zoomAmount, inset, sampling, input, &cropRect);
	}

	private unsafe static SKImageFilter CreateMagnifier(SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling, SKImageFilter? input, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_magnifier(&lensBounds, zoomAmount, inset, &sampling, input?.Handle ?? IntPtr.Zero, cropRect));
	}

	[Obsolete("Use CreateShader(SKShader) instead.", true)]
	public unsafe static SKImageFilter CreatePaint(SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		return CreateShader(paint.Shader, paint.IsDither, null);
	}

	[Obsolete("Use CreateShader(SKShader, bool, SKRect) instead.", true)]
	public unsafe static SKImageFilter CreatePaint(SKPaint paint, SKRect cropRect)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		return CreateShader(paint.Shader, paint.IsDither, &cropRect);
	}

	public unsafe static SKImageFilter CreateShader(SKShader? shader)
	{
		return CreateShader(shader, dither: false, null);
	}

	public unsafe static SKImageFilter CreateShader(SKShader? shader, bool dither)
	{
		return CreateShader(shader, dither, null);
	}

	public unsafe static SKImageFilter CreateShader(SKShader? shader, bool dither, SKRect cropRect)
	{
		return CreateShader(shader, dither, &cropRect);
	}

	private unsafe static SKImageFilter CreateShader(SKShader? shader, bool dither, SKRect* cropRect)
	{
		return GetObject(SkiaApi.sk_imagefilter_new_shader(shader?.Handle ?? IntPtr.Zero, dither, cropRect));
	}

	internal static SKImageFilter GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKImageFilter(h, o));
	}
}
