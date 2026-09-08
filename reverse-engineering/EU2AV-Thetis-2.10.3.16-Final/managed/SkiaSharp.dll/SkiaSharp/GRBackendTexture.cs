using System;

namespace SkiaSharp;

public class GRBackendTexture : SKObject, ISKSkipObjectRegistration
{
	public bool IsValid => SkiaApi.gr_backendtexture_is_valid(Handle);

	public int Width => SkiaApi.gr_backendtexture_get_width(Handle);

	public int Height => SkiaApi.gr_backendtexture_get_height(Handle);

	public bool HasMipMaps => SkiaApi.gr_backendtexture_has_mipmaps(Handle);

	public GRBackend Backend => SkiaApi.gr_backendtexture_get_backend(Handle).FromNative();

	public SKSizeI Size => new SKSizeI(Width, Height);

	public SKRectI Rect => new SKRectI(0, 0, Width, Height);

	internal GRBackendTexture(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public GRBackendTexture(int width, int height, bool mipmapped, GRGlTextureInfo glInfo)
		: this(IntPtr.Zero, owns: true)
	{
		CreateGl(width, height, mipmapped, glInfo);
	}

	public GRBackendTexture(int width, int height, GRVkImageInfo vkInfo)
		: this(IntPtr.Zero, owns: true)
	{
		CreateVulkan(width, height, vkInfo);
	}

	public GRBackendTexture(int width, int height, GRD3DTextureResourceInfo d3dTextureInfo)
		: this(IntPtr.Zero, owns: true)
	{
		CreateDirect3D(width, height, d3dTextureInfo);
	}

	public unsafe GRBackendTexture(int width, int height, bool mipmapped, GRMtlTextureInfo mtlInfo)
		: this(IntPtr.Zero, owns: true)
	{
		GRMtlTextureInfoNative gRMtlTextureInfoNative = mtlInfo.ToNative();
		Handle = SkiaApi.gr_backendtexture_new_metal(width, height, mipmapped, &gRMtlTextureInfoNative);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new GRBackendTexture instance.");
		}
	}

	private unsafe void CreateGl(int width, int height, bool mipmapped, GRGlTextureInfo glInfo)
	{
		Handle = SkiaApi.gr_backendtexture_new_gl(width, height, mipmapped, &glInfo);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new GRBackendTexture instance.");
		}
	}

	private unsafe void CreateVulkan(int width, int height, GRVkImageInfo vkInfo)
	{
		Handle = SkiaApi.gr_backendtexture_new_vulkan(width, height, &vkInfo);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new GRBackendTexture instance.");
		}
	}

	private unsafe void CreateDirect3D(int width, int height, GRD3DTextureResourceInfo d3dTextureInfo)
	{
		GRD3DTextureResourceInfoNative gRD3DTextureResourceInfoNative = d3dTextureInfo.ToNative();
		Handle = SkiaApi.gr_backendtexture_new_direct3d(width, height, &gRD3DTextureResourceInfoNative);
		if (Handle == IntPtr.Zero)
		{
			throw new InvalidOperationException("Unable to create a new GRBackendTexture instance.");
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.gr_backendtexture_delete(Handle);
	}

	public GRGlTextureInfo GetGlTextureInfo()
	{
		if (!GetGlTextureInfo(out var glInfo))
		{
			return default(GRGlTextureInfo);
		}
		return glInfo;
	}

	public unsafe bool GetGlTextureInfo(out GRGlTextureInfo glInfo)
	{
		fixed (GRGlTextureInfo* glInfo2 = &glInfo)
		{
			return SkiaApi.gr_backendtexture_get_gl_textureinfo(Handle, glInfo2);
		}
	}
}
