using System;

namespace SkiaSharp;

public class GRRecordingContext : SKObject, ISKReferenceCounted
{
	public virtual GRBackend Backend => SkiaApi.gr_recording_context_get_backend(Handle).FromNative();

	public virtual bool IsAbandoned => SkiaApi.gr_recording_context_is_abandoned(Handle);

	public int MaxTextureSize => SkiaApi.gr_recording_context_max_texture_size(Handle);

	public int MaxRenderTargetSize => SkiaApi.gr_recording_context_max_render_target_size(Handle);

	internal GRRecordingContext(IntPtr h, bool owns)
		: base(h, owns)
	{
	}

	public int GetMaxSurfaceSampleCount(SKColorType colorType)
	{
		return SkiaApi.gr_recording_context_get_max_surface_sample_count_for_color_type(Handle, colorType.ToNative());
	}

	internal static GRRecordingContext GetObject(IntPtr handle, bool owns = true, bool unrefExisting = true)
	{
		IntPtr intPtr = SkiaApi.gr_recording_context_get_direct_context(handle);
		if (intPtr != IntPtr.Zero)
		{
			return GRContext.GetObject(intPtr, owns: false, unrefExisting: false);
		}
		return SKObject.GetOrAddObject(handle, owns, unrefExisting, (IntPtr h, bool o) => new GRRecordingContext(h, o));
	}
}
