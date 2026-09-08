using System;

namespace SkiaSharp;

public class GRBackendRenderTarget : SKObject, ISKSkipObjectRegistration
{
	public bool IsValid => SkiaApi.gr_backendrendertarget_is_valid(Handle);

	public int Width => SkiaApi.gr_backendrendertarget_get_width(Handle);

	public int Height => SkiaApi.gr_backendrendertarget_get_height(Handle);

	public int SampleCount => SkiaApi.gr_backendrendertarget_get_samples(Handle);

	public int StencilBits => SkiaApi.gr_backendrendertarget_get_stencils(Handle);

	public GRBackend Backend => SkiaApi.gr_backendrendertarget_get_backend(Handle).FromNative();

	public SKSizeI Size => new SKSizeI(Width, Height);

	public SKRectI Rect => new SKRectI(0, 0, Width, Height);

	internal GRBackendRenderTarget(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public GRBackendRenderTarget(int width, int height, int sampleCount, int stencilBits, GRGlFramebufferInfo glInfo)
		: this(IntPtr.Zero, owns: true)
	{
		CreateGl(width, height, sampleCount, stencilBits, glInfo);
	}

	[Obsolete("Use GRBackendRenderTarget(int width, int height, GRVkImageInfo vkImageInfo) instead.")]
	public GRBackendRenderTarget(int width, int height, int sampleCount, GRVkImageInfo vkImageInfo)
		: this(width, height, vkImageInfo)
	{
	}

	public GRBackendRenderTarget(int width, int height, GRVkImageInfo vkImageInfo)
		: this(IntPtr.Zero, owns: true)
	{
		CreateVulkan(width, height, vkImageInfo);
	}

	public GRBackendRenderTarget(int width, int height, GRD3DTextureResourceInfo d3dTextureInfo)
		: this(IntPtr.Zero, owns: true)
	{
		CreateDirect3D(width, height, d3dTextureInfo);
	}

	public unsafe GRBackendRenderTarget(int width, int height, GRMtlTextureInfo mtlInfo)
		: this(IntPtr.Zero, owns: true)
	{
		GRMtlTextureInfoNative gRMtlTextureInfoNative = mtlInfo.ToNative();
		Handle = SkiaApi.gr_backendrendertarget_new_metal(width, height, &gRMtlTextureInfoNative);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new GRBackendRenderTarget instance.");
		}
	}

	private unsafe void CreateGl(int width, int height, int sampleCount, int stencilBits, GRGlFramebufferInfo glInfo)
	{
		Handle = SkiaApi.gr_backendrendertarget_new_gl(width, height, sampleCount, stencilBits, &glInfo);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new GRBackendRenderTarget instance.");
		}
	}

	private unsafe void CreateVulkan(int width, int height, GRVkImageInfo vkImageInfo)
	{
		Handle = SkiaApi.gr_backendrendertarget_new_vulkan(width, height, &vkImageInfo);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new GRBackendRenderTarget instance.");
		}
	}

	private unsafe void CreateDirect3D(int width, int height, GRD3DTextureResourceInfo d3dTextureInfo)
	{
		GRD3DTextureResourceInfoNative gRD3DTextureResourceInfoNative = d3dTextureInfo.ToNative();
		Handle = SkiaApi.gr_backendrendertarget_new_direct3d(width, height, &gRD3DTextureResourceInfoNative);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new GRBackendRenderTarget instance.");
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.gr_backendrendertarget_delete(Handle);
	}

	public GRGlFramebufferInfo GetGlFramebufferInfo()
	{
		if (!GetGlFramebufferInfo(out var glInfo))
		{
			return default(GRGlFramebufferInfo);
		}
		return glInfo;
	}

	public unsafe bool GetGlFramebufferInfo(out GRGlFramebufferInfo glInfo)
	{
		fixed (GRGlFramebufferInfo* glInfo2 = &glInfo)
		{
			return SkiaApi.gr_backendrendertarget_get_gl_framebufferinfo(Handle, glInfo2);
		}
	}
}
