using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

internal class SkiaApi
{
	private class Delegates
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_backendrendertarget_delete(IntPtr rendertarget);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate GRBackendNative gr_backendrendertarget_get_backend(IntPtr rendertarget);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool gr_backendrendertarget_get_gl_framebufferinfo(IntPtr rendertarget, GRGlFramebufferInfo* glInfo);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int gr_backendrendertarget_get_height(IntPtr rendertarget);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int gr_backendrendertarget_get_samples(IntPtr rendertarget);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int gr_backendrendertarget_get_stencils(IntPtr rendertarget);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int gr_backendrendertarget_get_width(IntPtr rendertarget);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool gr_backendrendertarget_is_valid(IntPtr rendertarget);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_backendrendertarget_new_direct3d(int width, int height, GRD3DTextureResourceInfoNative* d3dInfo);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_backendrendertarget_new_gl(int width, int height, int samples, int stencils, GRGlFramebufferInfo* glInfo);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_backendrendertarget_new_metal(int width, int height, GRMtlTextureInfoNative* mtlInfo);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_backendrendertarget_new_vulkan(int width, int height, GRVkImageInfo* vkImageInfo);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_backendtexture_delete(IntPtr texture);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate GRBackendNative gr_backendtexture_get_backend(IntPtr texture);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool gr_backendtexture_get_gl_textureinfo(IntPtr texture, GRGlTextureInfo* glInfo);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int gr_backendtexture_get_height(IntPtr texture);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int gr_backendtexture_get_width(IntPtr texture);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool gr_backendtexture_has_mipmaps(IntPtr texture);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool gr_backendtexture_is_valid(IntPtr texture);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_backendtexture_new_direct3d(int width, int height, GRD3DTextureResourceInfoNative* d3dInfo);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_backendtexture_new_gl(int width, int height, [MarshalAs(UnmanagedType.I1)] bool mipmapped, GRGlTextureInfo* glInfo);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_backendtexture_new_metal(int width, int height, [MarshalAs(UnmanagedType.I1)] bool mipmapped, GRMtlTextureInfoNative* mtlInfo);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_backendtexture_new_vulkan(int width, int height, GRVkImageInfo* vkInfo);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_direct_context_abandon_context(IntPtr context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_direct_context_dump_memory_statistics(IntPtr context, IntPtr dump);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_direct_context_flush(IntPtr context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_direct_context_flush_and_submit(IntPtr context, [MarshalAs(UnmanagedType.I1)] bool syncCpu);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_direct_context_flush_image(IntPtr context, IntPtr image);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_direct_context_flush_surface(IntPtr context, IntPtr surface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_direct_context_free_gpu_resources(IntPtr context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr gr_direct_context_get_resource_cache_limit(IntPtr context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void gr_direct_context_get_resource_cache_usage(IntPtr context, int* maxResources, IntPtr* maxResourceBytes);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool gr_direct_context_is_abandoned(IntPtr context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr gr_direct_context_make_direct3d(GRD3DBackendContextNative d3dBackendContext);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_direct_context_make_direct3d_with_options(GRD3DBackendContextNative d3dBackendContext, GRContextOptionsNative* options);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr gr_direct_context_make_gl(IntPtr glInterface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_direct_context_make_gl_with_options(IntPtr glInterface, GRContextOptionsNative* options);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_direct_context_make_metal(void* device, void* queue);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_direct_context_make_metal_with_options(void* device, void* queue, GRContextOptionsNative* options);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr gr_direct_context_make_vulkan(GRVkBackendContextNative vkBackendContext);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_direct_context_make_vulkan_with_options(GRVkBackendContextNative vkBackendContext, GRContextOptionsNative* options);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_direct_context_perform_deferred_cleanup(IntPtr context, long ms);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_direct_context_purge_unlocked_resources(IntPtr context, [MarshalAs(UnmanagedType.I1)] bool scratchResourcesOnly);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_direct_context_purge_unlocked_resources_bytes(IntPtr context, IntPtr bytesToPurge, [MarshalAs(UnmanagedType.I1)] bool preferScratchResources);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_direct_context_release_resources_and_abandon_context(IntPtr context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_direct_context_reset_context(IntPtr context, uint state);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_direct_context_set_resource_cache_limit(IntPtr context, IntPtr maxResourceBytes);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool gr_direct_context_submit(IntPtr context, [MarshalAs(UnmanagedType.I1)] bool syncCpu);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_glinterface_assemble_gl_interface(void* ctx, GRGlGetProcProxyDelegate get);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_glinterface_assemble_gles_interface(void* ctx, GRGlGetProcProxyDelegate get);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_glinterface_assemble_interface(void* ctx, GRGlGetProcProxyDelegate get);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr gr_glinterface_assemble_webgl_interface(void* ctx, GRGlGetProcProxyDelegate get);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr gr_glinterface_create_native_interface();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool gr_glinterface_has_extension(IntPtr glInterface, [MarshalAs(UnmanagedType.LPStr)] string extension);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_glinterface_unref(IntPtr glInterface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool gr_glinterface_validate(IntPtr glInterface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate GRBackendNative gr_recording_context_get_backend(IntPtr context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr gr_recording_context_get_direct_context(IntPtr context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int gr_recording_context_get_max_surface_sample_count_for_color_type(IntPtr context, SKColorTypeNative colorType);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool gr_recording_context_is_abandoned(IntPtr context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int gr_recording_context_max_render_target_size(IntPtr context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int gr_recording_context_max_texture_size(IntPtr context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_recording_context_unref(IntPtr context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void gr_vk_extensions_delete(IntPtr extensions);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool gr_vk_extensions_has_extension(IntPtr extensions, [MarshalAs(UnmanagedType.LPStr)] string ext, uint minVersion);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void gr_vk_extensions_init(IntPtr extensions, GRVkGetProcProxyDelegate getProc, void* userData, IntPtr instance, IntPtr physDev, uint instanceExtensionCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] instanceExtensions, uint deviceExtensionCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] deviceExtensions);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr gr_vk_extensions_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_bitmap_destructor(IntPtr cbitmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_bitmap_erase(IntPtr cbitmap, uint color);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_bitmap_erase_rect(IntPtr cbitmap, uint color, SKRectI* rect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_bitmap_extract_alpha(IntPtr cbitmap, IntPtr dst, IntPtr paint, SKPointI* offset);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_bitmap_extract_subset(IntPtr cbitmap, IntPtr dst, SKRectI* subset);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void* sk_bitmap_get_addr(IntPtr cbitmap, int x, int y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate ushort* sk_bitmap_get_addr_16(IntPtr cbitmap, int x, int y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate uint* sk_bitmap_get_addr_32(IntPtr cbitmap, int x, int y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate byte* sk_bitmap_get_addr_8(IntPtr cbitmap, int x, int y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_bitmap_get_byte_count(IntPtr cbitmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_bitmap_get_info(IntPtr cbitmap, SKImageInfoNative* info);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate uint sk_bitmap_get_pixel_color(IntPtr cbitmap, int x, int y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_bitmap_get_pixel_colors(IntPtr cbitmap, uint* colors);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void* sk_bitmap_get_pixels(IntPtr cbitmap, IntPtr* length);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_bitmap_get_row_bytes(IntPtr cbitmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_bitmap_install_pixels(IntPtr cbitmap, SKImageInfoNative* cinfo, void* pixels, IntPtr rowBytes, SKBitmapReleaseProxyDelegate releaseProc, void* context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_bitmap_install_pixels_with_pixmap(IntPtr cbitmap, IntPtr cpixmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_bitmap_is_immutable(IntPtr cbitmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_bitmap_is_null(IntPtr cbitmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_bitmap_make_shader(IntPtr cbitmap, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions* sampling, SKMatrix* cmatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_bitmap_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_bitmap_notify_pixels_changed(IntPtr cbitmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_bitmap_peek_pixels(IntPtr cbitmap, IntPtr cpixmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_bitmap_ready_to_draw(IntPtr cbitmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_bitmap_reset(IntPtr cbitmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_bitmap_set_immutable(IntPtr cbitmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_bitmap_set_pixels(IntPtr cbitmap, void* pixels);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_bitmap_swap(IntPtr cbitmap, IntPtr cother);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_bitmap_try_alloc_pixels(IntPtr cbitmap, SKImageInfoNative* requestedInfo, IntPtr rowBytes);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_bitmap_try_alloc_pixels_with_flags(IntPtr cbitmap, SKImageInfoNative* requestedInfo, uint flags);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_blender_new_arithmetic(float k1, float k2, float k3, float k4, [MarshalAs(UnmanagedType.I1)] bool enforcePremul);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_blender_new_mode(SKBlendMode mode);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_blender_ref(IntPtr blender);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_blender_unref(IntPtr blender);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_clear(IntPtr ccanvas, uint color);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_clear_color4f(IntPtr ccanvas, SKColorF color);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_clip_path_with_operation(IntPtr ccanvas, IntPtr cpath, SKClipOperation op, [MarshalAs(UnmanagedType.I1)] bool doAA);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_clip_rect_with_operation(IntPtr ccanvas, SKRect* crect, SKClipOperation op, [MarshalAs(UnmanagedType.I1)] bool doAA);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_clip_region(IntPtr ccanvas, IntPtr region, SKClipOperation op);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_clip_rrect_with_operation(IntPtr ccanvas, IntPtr crect, SKClipOperation op, [MarshalAs(UnmanagedType.I1)] bool doAA);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_concat(IntPtr ccanvas, SKMatrix44* cmatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_destroy(IntPtr ccanvas);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_discard(IntPtr ccanvas);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_annotation(IntPtr t, SKRect* rect, void* key, IntPtr value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_arc(IntPtr ccanvas, SKRect* oval, float startAngle, float sweepAngle, [MarshalAs(UnmanagedType.I1)] bool useCenter, IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_atlas(IntPtr ccanvas, IntPtr atlas, SKRotationScaleMatrix* xform, SKRect* tex, uint* colors, int count, SKBlendMode mode, SKSamplingOptions* sampling, SKRect* cullRect, IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_draw_circle(IntPtr ccanvas, float cx, float cy, float rad, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_draw_color(IntPtr ccanvas, uint color, SKBlendMode cmode);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_draw_color4f(IntPtr ccanvas, SKColorF color, SKBlendMode cmode);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_drawable(IntPtr ccanvas, IntPtr cdrawable, SKMatrix* cmatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_draw_drrect(IntPtr ccanvas, IntPtr outer, IntPtr inner, IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_image(IntPtr ccanvas, IntPtr cimage, float x, float y, SKSamplingOptions* sampling, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_image_lattice(IntPtr ccanvas, IntPtr image, SKLatticeInternal* lattice, SKRect* dst, SKFilterMode mode, IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_image_nine(IntPtr ccanvas, IntPtr image, SKRectI* center, SKRect* dst, SKFilterMode mode, IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_image_rect(IntPtr ccanvas, IntPtr cimage, SKRect* csrcR, SKRect* cdstR, SKSamplingOptions* sampling, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_draw_line(IntPtr ccanvas, float x0, float y0, float x1, float y1, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_link_destination_annotation(IntPtr t, SKRect* rect, IntPtr value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_named_destination_annotation(IntPtr t, SKPoint* point, IntPtr value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_oval(IntPtr ccanvas, SKRect* crect, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_draw_paint(IntPtr ccanvas, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_patch(IntPtr ccanvas, SKPoint* cubics, uint* colors, SKPoint* texCoords, SKBlendMode mode, IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_draw_path(IntPtr ccanvas, IntPtr cpath, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_picture(IntPtr ccanvas, IntPtr cpicture, SKMatrix* cmatrix, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_draw_point(IntPtr ccanvas, float x, float y, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_points(IntPtr ccanvas, SKPointMode pointMode, IntPtr count, SKPoint* points, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_rect(IntPtr ccanvas, SKRect* crect, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_draw_region(IntPtr ccanvas, IntPtr cregion, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_round_rect(IntPtr ccanvas, SKRect* crect, float rx, float ry, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_draw_rrect(IntPtr ccanvas, IntPtr crect, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_simple_text(IntPtr ccanvas, void* text, IntPtr byte_length, SKTextEncoding encoding, float x, float y, IntPtr cfont, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_draw_text_blob(IntPtr ccanvas, IntPtr text, float x, float y, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_draw_url_annotation(IntPtr t, SKRect* rect, IntPtr value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_draw_vertices(IntPtr ccanvas, IntPtr vertices, SKBlendMode mode, IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_canvas_get_device_clip_bounds(IntPtr ccanvas, SKRectI* cbounds);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_canvas_get_local_clip_bounds(IntPtr ccanvas, SKRect* cbounds);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_get_matrix(IntPtr ccanvas, SKMatrix44* cmatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_canvas_get_save_count(IntPtr ccanvas);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_canvas_is_clip_empty(IntPtr ccanvas);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_canvas_is_clip_rect(IntPtr ccanvas);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_canvas_new_from_bitmap(IntPtr bitmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_canvas_new_from_raster(SKImageInfoNative* cinfo, void* pixels, IntPtr rowBytes, IntPtr props);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_canvas_quick_reject(IntPtr ccanvas, SKRect* crect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_reset_matrix(IntPtr ccanvas);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_restore(IntPtr ccanvas);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_restore_to_count(IntPtr ccanvas, int saveCount);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_rotate_degrees(IntPtr ccanvas, float degrees);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_rotate_radians(IntPtr ccanvas, float radians);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_canvas_save(IntPtr ccanvas);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int sk_canvas_save_layer(IntPtr ccanvas, SKRect* crect, IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int sk_canvas_save_layer_rec(IntPtr ccanvas, SKCanvasSaveLayerRecNative* crec);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_scale(IntPtr ccanvas, float sx, float sy);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_canvas_set_matrix(IntPtr ccanvas, SKMatrix44* cmatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_skew(IntPtr ccanvas, float sx, float sy);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_canvas_translate(IntPtr ccanvas, float dx, float dy);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_get_recording_context(IntPtr canvas);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_get_surface(IntPtr canvas);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_nodraw_canvas_destroy(IntPtr t);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_nodraw_canvas_new(int width, int height);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_nway_canvas_add_canvas(IntPtr t, IntPtr canvas);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_nway_canvas_destroy(IntPtr t);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_nway_canvas_new(int width, int height);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_nway_canvas_remove_all(IntPtr t);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_nway_canvas_remove_canvas(IntPtr t, IntPtr canvas);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_overdraw_canvas_destroy(IntPtr canvas);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_overdraw_canvas_new(IntPtr canvas);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_codec_destroy(IntPtr codec);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKEncodedImageFormat sk_codec_get_encoded_format(IntPtr codec);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_codec_get_frame_count(IntPtr codec);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_codec_get_frame_info(IntPtr codec, SKCodecFrameInfo* frameInfo);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_codec_get_frame_info_for_index(IntPtr codec, int index, SKCodecFrameInfo* frameInfo);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_codec_get_info(IntPtr codec, SKImageInfoNative* info);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKEncodedOrigin sk_codec_get_origin(IntPtr codec);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate SKCodecResult sk_codec_get_pixels(IntPtr codec, SKImageInfoNative* info, void* pixels, IntPtr rowBytes, SKCodecOptionsInternal* options);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_codec_get_repetition_count(IntPtr codec);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_codec_get_scaled_dimensions(IntPtr codec, float desiredScale, SKSizeI* dimensions);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKCodecScanlineOrder sk_codec_get_scanline_order(IntPtr codec);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int sk_codec_get_scanlines(IntPtr codec, void* dst, int countLines, IntPtr rowBytes);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_codec_get_valid_subset(IntPtr codec, SKRectI* desiredSubset);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate SKCodecResult sk_codec_incremental_decode(IntPtr codec, int* rowsDecoded);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_codec_min_buffered_bytes_needed();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_codec_new_from_data(IntPtr data);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_codec_new_from_stream(IntPtr stream, SKCodecResult* result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_codec_next_scanline(IntPtr codec);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_codec_output_scanline(IntPtr codec, int inputScanline);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_codec_skip_scanlines(IntPtr codec, int countLines);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate SKCodecResult sk_codec_start_incremental_decode(IntPtr codec, SKImageInfoNative* info, void* pixels, IntPtr rowBytes, SKCodecOptionsInternal* options);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate SKCodecResult sk_codec_start_scanline_decode(IntPtr codec, SKImageInfoNative* info, SKCodecOptionsInternal* options);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_colorfilter_new_color_matrix(float* array);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_colorfilter_new_compose(IntPtr outer, IntPtr inner);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_colorfilter_new_high_contrast(SKHighContrastConfig* config);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_colorfilter_new_hsla_matrix(float* array);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_colorfilter_new_lerp(float weight, IntPtr filter0, IntPtr filter1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_colorfilter_new_lighting(uint mul, uint add);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_colorfilter_new_linear_to_srgb_gamma();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_colorfilter_new_luma_color();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_colorfilter_new_mode(uint c, SKBlendMode mode);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_colorfilter_new_srgb_to_linear_gamma();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_colorfilter_new_table(byte* table);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_colorfilter_new_table_argb(byte* tableA, byte* tableR, byte* tableG, byte* tableB);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_colorfilter_unref(IntPtr filter);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_color4f_from_color(uint color, SKColorF* color4f);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate uint sk_color4f_to_color(SKColorF* color4f);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_colorspace_equals(IntPtr src, IntPtr dst);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_colorspace_gamma_close_to_srgb(IntPtr colorspace);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_colorspace_gamma_is_linear(IntPtr colorspace);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_colorspace_icc_profile_delete(IntPtr profile);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate byte* sk_colorspace_icc_profile_get_buffer(IntPtr profile, uint* size);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_colorspace_icc_profile_get_to_xyzd50(IntPtr profile, SKColorSpaceXyz* toXYZD50);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_colorspace_icc_profile_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_colorspace_icc_profile_parse(void* buffer, IntPtr length, IntPtr profile);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_colorspace_is_numerical_transfer_fn(IntPtr colorspace, SKColorSpaceTransferFn* transferFn);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_colorspace_is_srgb(IntPtr colorspace);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_colorspace_make_linear_gamma(IntPtr colorspace);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_colorspace_make_srgb_gamma(IntPtr colorspace);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_colorspace_new_icc(IntPtr profile);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_colorspace_new_rgb(SKColorSpaceTransferFn* transferFn, SKColorSpaceXyz* toXYZD50);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_colorspace_new_srgb();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_colorspace_new_srgb_linear();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_colorspace_primaries_to_xyzd50(SKColorSpacePrimaries* primaries, SKColorSpaceXyz* toXYZD50);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_colorspace_ref(IntPtr colorspace);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_colorspace_to_profile(IntPtr colorspace, IntPtr profile);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_colorspace_to_xyzd50(IntPtr colorspace, SKColorSpaceXyz* toXYZD50);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate float sk_colorspace_transfer_fn_eval(SKColorSpaceTransferFn* transferFn, float x);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_colorspace_transfer_fn_invert(SKColorSpaceTransferFn* src, SKColorSpaceTransferFn* dst);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_colorspace_transfer_fn_named_2dot2(SKColorSpaceTransferFn* transferFn);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_colorspace_transfer_fn_named_hlg(SKColorSpaceTransferFn* transferFn);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_colorspace_transfer_fn_named_linear(SKColorSpaceTransferFn* transferFn);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_colorspace_transfer_fn_named_pq(SKColorSpaceTransferFn* transferFn);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_colorspace_transfer_fn_named_rec2020(SKColorSpaceTransferFn* transferFn);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_colorspace_transfer_fn_named_srgb(SKColorSpaceTransferFn* transferFn);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_colorspace_unref(IntPtr colorspace);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_colorspace_xyz_concat(SKColorSpaceXyz* a, SKColorSpaceXyz* b, SKColorSpaceXyz* result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_colorspace_xyz_invert(SKColorSpaceXyz* src, SKColorSpaceXyz* dst);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_colorspace_xyz_named_adobe_rgb(SKColorSpaceXyz* xyz);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_colorspace_xyz_named_display_p3(SKColorSpaceXyz* xyz);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_colorspace_xyz_named_rec2020(SKColorSpaceXyz* xyz);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_colorspace_xyz_named_srgb(SKColorSpaceXyz* xyz);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_colorspace_xyz_named_xyz(SKColorSpaceXyz* xyz);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate byte* sk_data_get_bytes(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void* sk_data_get_data(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_data_get_size(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_data_new_empty();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_data_new_from_file(void* path);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_data_new_from_stream(IntPtr stream, IntPtr length);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_data_new_subset(IntPtr src, IntPtr offset, IntPtr length);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_data_new_uninitialized(IntPtr size);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_data_new_with_copy(void* src, IntPtr length);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_data_new_with_proc(void* ptr, IntPtr length, SKDataReleaseProxyDelegate proc, void* ctx);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_data_ref(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_data_unref(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_document_abort(IntPtr document);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_document_begin_page(IntPtr document, float width, float height, SKRect* content);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_document_close(IntPtr document);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_document_create_pdf_from_stream(IntPtr stream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_document_create_pdf_from_stream_with_metadata(IntPtr stream, SKDocumentPdfMetadataInternal* metadata);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_document_create_xps_from_stream(IntPtr stream, float dpi);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_document_end_page(IntPtr document);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_document_unref(IntPtr document);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_drawable_approximate_bytes_used(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_drawable_draw(IntPtr param0, IntPtr param1, SKMatrix* param2);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_drawable_get_bounds(IntPtr param0, SKRect* param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate uint sk_drawable_get_generation_id(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_drawable_new_picture_snapshot(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_drawable_notify_drawing_changed(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_drawable_unref(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_font_break_text(IntPtr font, void* text, IntPtr byteLength, SKTextEncoding encoding, float maxWidth, float* measuredWidth, IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_font_delete(IntPtr font);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKFontEdging sk_font_get_edging(IntPtr font);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKFontHinting sk_font_get_hinting(IntPtr font);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate float sk_font_get_metrics(IntPtr font, SKFontMetrics* metrics);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_font_get_path(IntPtr font, ushort glyph, IntPtr path);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_font_get_paths(IntPtr font, ushort* glyphs, int count, SKGlyphPathProxyDelegate glyphPathProc, void* context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_font_get_pos(IntPtr font, ushort* glyphs, int count, SKPoint* pos, SKPoint* origin);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate float sk_font_get_scale_x(IntPtr font);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate float sk_font_get_size(IntPtr font);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate float sk_font_get_skew_x(IntPtr font);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_font_get_typeface(IntPtr font);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_font_get_widths_bounds(IntPtr font, ushort* glyphs, int count, float* widths, SKRect* bounds, IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_font_get_xpos(IntPtr font, ushort* glyphs, int count, float* xpos, float origin);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_font_is_baseline_snap(IntPtr font);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_font_is_embedded_bitmaps(IntPtr font);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_font_is_embolden(IntPtr font);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_font_is_force_auto_hinting(IntPtr font);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_font_is_linear_metrics(IntPtr font);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_font_is_subpixel(IntPtr font);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate float sk_font_measure_text(IntPtr font, void* text, IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_font_measure_text_no_return(IntPtr font, void* text, IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, IntPtr paint, float* measuredWidth);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_font_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_font_new_with_values(IntPtr typeface, float size, float scaleX, float skewX);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_font_set_baseline_snap(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_font_set_edging(IntPtr font, SKFontEdging value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_font_set_embedded_bitmaps(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_font_set_embolden(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_font_set_force_auto_hinting(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_font_set_hinting(IntPtr font, SKFontHinting value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_font_set_linear_metrics(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_font_set_scale_x(IntPtr font, float value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_font_set_size(IntPtr font, float value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_font_set_skew_x(IntPtr font, float value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_font_set_subpixel(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_font_set_typeface(IntPtr font, IntPtr value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int sk_font_text_to_glyphs(IntPtr font, void* text, IntPtr byteLength, SKTextEncoding encoding, ushort* glyphs, int maxGlyphCount);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate ushort sk_font_unichar_to_glyph(IntPtr font, int uni);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_font_unichars_to_glyphs(IntPtr font, int* uni, int count, ushort* glyphs);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_text_utils_get_path(void* text, IntPtr length, SKTextEncoding encoding, float x, float y, IntPtr font, IntPtr path);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_text_utils_get_pos_path(void* text, IntPtr length, SKTextEncoding encoding, SKPoint* pos, IntPtr font, IntPtr path);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKColorTypeNative sk_colortype_get_default_8888();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_nvrefcnt_get_ref_count(IntPtr refcnt);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_nvrefcnt_safe_ref(IntPtr refcnt);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_nvrefcnt_safe_unref(IntPtr refcnt);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_nvrefcnt_unique(IntPtr refcnt);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_refcnt_get_ref_count(IntPtr refcnt);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_refcnt_safe_ref(IntPtr refcnt);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_refcnt_safe_unref(IntPtr refcnt);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_refcnt_unique(IntPtr refcnt);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_version_get_increment();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_version_get_milestone();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void* sk_version_get_string();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_graphics_dump_memory_statistics(IntPtr dump);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_graphics_get_font_cache_count_limit();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_graphics_get_font_cache_count_used();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_graphics_get_font_cache_limit();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_graphics_get_font_cache_used();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_graphics_get_resource_cache_single_allocation_byte_limit();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_graphics_get_resource_cache_total_byte_limit();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_graphics_get_resource_cache_total_bytes_used();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_graphics_init();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_graphics_purge_all_caches();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_graphics_purge_font_cache();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_graphics_purge_resource_cache();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_graphics_set_font_cache_count_limit(int count);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_graphics_set_font_cache_limit(IntPtr bytes);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_graphics_set_resource_cache_single_allocation_byte_limit(IntPtr newLimit);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_graphics_set_resource_cache_total_byte_limit(IntPtr newLimit);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKAlphaType sk_image_get_alpha_type(IntPtr image);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKColorTypeNative sk_image_get_color_type(IntPtr image);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_image_get_colorspace(IntPtr image);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_image_get_height(IntPtr cimage);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate uint sk_image_get_unique_id(IntPtr cimage);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_image_get_width(IntPtr cimage);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_image_is_alpha_only(IntPtr image);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_image_is_lazy_generated(IntPtr image);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_image_is_texture_backed(IntPtr image);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_image_is_valid(IntPtr image, IntPtr context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_image_make_non_texture_image(IntPtr cimage);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_image_make_raster_image(IntPtr cimage);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_image_make_raw_shader(IntPtr image, SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions* sampling, SKMatrix* cmatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_image_make_shader(IntPtr image, SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions* sampling, SKMatrix* cmatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_image_make_subset(IntPtr cimage, IntPtr context, SKRectI* subset);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_image_make_subset_raster(IntPtr cimage, SKRectI* subset);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_image_make_texture_image(IntPtr cimage, IntPtr context, [MarshalAs(UnmanagedType.I1)] bool mipmapped, [MarshalAs(UnmanagedType.I1)] bool budgeted);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_image_make_with_filter(IntPtr cimage, IntPtr context, IntPtr filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_image_make_with_filter_raster(IntPtr cimage, IntPtr filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_image_new_from_adopted_texture(IntPtr context, IntPtr texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, IntPtr colorSpace);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_image_new_from_bitmap(IntPtr cbitmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_image_new_from_encoded(IntPtr cdata);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_image_new_from_picture(IntPtr picture, SKSizeI* dimensions, SKMatrix* cmatrix, IntPtr paint, [MarshalAs(UnmanagedType.I1)] bool useFloatingPointBitDepth, IntPtr colorSpace, IntPtr props);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_image_new_from_texture(IntPtr context, IntPtr texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, IntPtr colorSpace, SKImageTextureReleaseProxyDelegate releaseProc, void* releaseContext);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_image_new_raster(IntPtr pixmap, SKImageRasterReleaseProxyDelegate releaseProc, void* context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_image_new_raster_copy(SKImageInfoNative* cinfo, void* pixels, IntPtr rowBytes);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_image_new_raster_copy_with_pixmap(IntPtr pixmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_image_new_raster_data(SKImageInfoNative* cinfo, IntPtr pixels, IntPtr rowBytes);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_image_peek_pixels(IntPtr image, IntPtr pixmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_image_read_pixels(IntPtr image, SKImageInfoNative* dstInfo, void* dstPixels, IntPtr dstRowBytes, int srcX, int srcY, SKImageCachingHint cachingHint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_image_read_pixels_into_pixmap(IntPtr image, IntPtr dst, int srcX, int srcY, SKImageCachingHint cachingHint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_image_ref(IntPtr cimage);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_image_ref_encoded(IntPtr cimage);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_image_scale_pixels(IntPtr image, IntPtr dst, SKSamplingOptions* sampling, SKImageCachingHint cachingHint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_image_unref(IntPtr cimage);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_arithmetic(float k1, float k2, float k3, float k4, [MarshalAs(UnmanagedType.I1)] bool enforcePMColor, IntPtr background, IntPtr foreground, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_blend(SKBlendMode mode, IntPtr background, IntPtr foreground, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_blender(IntPtr blender, IntPtr background, IntPtr foreground, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_blur(float sigmaX, float sigmaY, SKShaderTileMode tileMode, IntPtr input, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_color_filter(IntPtr cf, IntPtr input, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_imagefilter_new_compose(IntPtr outer, IntPtr inner);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_dilate(float radiusX, float radiusY, IntPtr input, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_displacement_map_effect(SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, IntPtr displacement, IntPtr color, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_distant_lit_diffuse(SKPoint3* direction, uint lightColor, float surfaceScale, float kd, IntPtr input, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_distant_lit_specular(SKPoint3* direction, uint lightColor, float surfaceScale, float ks, float shininess, IntPtr input, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_drop_shadow(float dx, float dy, float sigmaX, float sigmaY, uint color, IntPtr input, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_drop_shadow_only(float dx, float dy, float sigmaX, float sigmaY, uint color, IntPtr input, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_erode(float radiusX, float radiusY, IntPtr input, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_image(IntPtr image, SKRect* srcRect, SKRect* dstRect, SKSamplingOptions* sampling);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_image_simple(IntPtr image, SKSamplingOptions* sampling);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_magnifier(SKRect* lensBounds, float zoomAmount, float inset, SKSamplingOptions* sampling, IntPtr input, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_matrix_convolution(SKSizeI* kernelSize, float* kernel, float gain, float bias, SKPointI* kernelOffset, SKShaderTileMode ctileMode, [MarshalAs(UnmanagedType.I1)] bool convolveAlpha, IntPtr input, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_matrix_transform(SKMatrix* cmatrix, SKSamplingOptions* sampling, IntPtr input);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_merge(IntPtr* cfilters, int count, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_merge_simple(IntPtr first, IntPtr second, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_offset(float dx, float dy, IntPtr input, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_imagefilter_new_picture(IntPtr picture);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_picture_with_rect(IntPtr picture, SKRect* targetRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_point_lit_diffuse(SKPoint3* location, uint lightColor, float surfaceScale, float kd, IntPtr input, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_point_lit_specular(SKPoint3* location, uint lightColor, float surfaceScale, float ks, float shininess, IntPtr input, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_shader(IntPtr shader, [MarshalAs(UnmanagedType.I1)] bool dither, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_spot_lit_diffuse(SKPoint3* location, SKPoint3* target, float specularExponent, float cutoffAngle, uint lightColor, float surfaceScale, float kd, IntPtr input, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_spot_lit_specular(SKPoint3* location, SKPoint3* target, float specularExponent, float cutoffAngle, uint lightColor, float surfaceScale, float ks, float shininess, IntPtr input, SKRect* cropRect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_imagefilter_new_tile(SKRect* src, SKRect* dst, IntPtr input);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_imagefilter_unref(IntPtr cfilter);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_linker_keep_alive();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_maskfilter_new_blur(SKBlurStyle param0, float sigma);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_maskfilter_new_blur_with_flags(SKBlurStyle param0, float sigma, [MarshalAs(UnmanagedType.I1)] bool respectCTM);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_maskfilter_new_clip(byte min, byte max);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_maskfilter_new_gamma(float gamma);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_maskfilter_new_shader(IntPtr cshader);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_maskfilter_new_table(byte* table);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_maskfilter_ref(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_maskfilter_unref(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_matrix_concat(SKMatrix* result, SKMatrix* first, SKMatrix* second);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_matrix_map_points(SKMatrix* matrix, SKPoint* dst, SKPoint* src, int count);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate float sk_matrix_map_radius(SKMatrix* matrix, float radius);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_matrix_map_rect(SKMatrix* matrix, SKRect* dest, SKRect* source);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_matrix_map_vector(SKMatrix* matrix, float x, float y, SKPoint* result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_matrix_map_vectors(SKMatrix* matrix, SKPoint* dst, SKPoint* src, int count);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_matrix_map_xy(SKMatrix* matrix, float x, float y, SKPoint* result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_matrix_post_concat(SKMatrix* result, SKMatrix* matrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_matrix_pre_concat(SKMatrix* result, SKMatrix* matrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_matrix_try_invert(SKMatrix* matrix, SKMatrix* result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_paint_clone(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_delete(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_paint_get_blender(IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKBlendMode sk_paint_get_blendmode(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate uint sk_paint_get_color(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_paint_get_color4f(IntPtr paint, SKColorF* color);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_paint_get_colorfilter(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_paint_get_fill_path(IntPtr cpaint, IntPtr src, IntPtr dst, SKRect* cullRect, SKMatrix* cmatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_paint_get_imagefilter(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_paint_get_maskfilter(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_paint_get_path_effect(IntPtr cpaint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_paint_get_shader(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKStrokeCap sk_paint_get_stroke_cap(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKStrokeJoin sk_paint_get_stroke_join(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate float sk_paint_get_stroke_miter(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate float sk_paint_get_stroke_width(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKPaintStyle sk_paint_get_style(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_paint_is_antialias(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_paint_is_dither(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_paint_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_reset(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_set_antialias(IntPtr param0, [MarshalAs(UnmanagedType.I1)] bool param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_set_blender(IntPtr paint, IntPtr blender);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_set_blendmode(IntPtr param0, SKBlendMode param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_set_color(IntPtr param0, uint param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_paint_set_color4f(IntPtr paint, SKColorF* color, IntPtr colorspace);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_set_colorfilter(IntPtr param0, IntPtr param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_set_dither(IntPtr param0, [MarshalAs(UnmanagedType.I1)] bool param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_set_imagefilter(IntPtr param0, IntPtr param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_set_maskfilter(IntPtr param0, IntPtr param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_set_path_effect(IntPtr cpaint, IntPtr effect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_set_shader(IntPtr param0, IntPtr param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_set_stroke_cap(IntPtr param0, SKStrokeCap param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_set_stroke_join(IntPtr param0, SKStrokeJoin param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_set_stroke_miter(IntPtr param0, float miter);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_set_stroke_width(IntPtr param0, float width);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_paint_set_style(IntPtr param0, SKPaintStyle param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_opbuilder_add(IntPtr builder, IntPtr path, SKPathOp op);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_opbuilder_destroy(IntPtr builder);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_opbuilder_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_opbuilder_resolve(IntPtr builder, IntPtr result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_path_add_arc(IntPtr cpath, SKRect* crect, float startAngle, float sweepAngle);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_add_circle(IntPtr param0, float x, float y, float radius, SKPathDirection dir);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_path_add_oval(IntPtr param0, SKRect* param1, SKPathDirection param2);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_add_path(IntPtr cpath, IntPtr other, SKPathAddMode add_mode);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_path_add_path_matrix(IntPtr cpath, IntPtr other, SKMatrix* matrix, SKPathAddMode add_mode);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_add_path_offset(IntPtr cpath, IntPtr other, float dx, float dy, SKPathAddMode add_mode);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_add_path_reverse(IntPtr cpath, IntPtr other);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_path_add_poly(IntPtr cpath, SKPoint* points, int count, [MarshalAs(UnmanagedType.I1)] bool close);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_path_add_rect(IntPtr param0, SKRect* param1, SKPathDirection param2);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_path_add_rect_start(IntPtr cpath, SKRect* crect, SKPathDirection cdir, uint startIndex);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_path_add_rounded_rect(IntPtr param0, SKRect* param1, float param2, float param3, SKPathDirection param4);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_add_rrect(IntPtr param0, IntPtr param1, SKPathDirection param2);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_add_rrect_start(IntPtr param0, IntPtr param1, SKPathDirection param2, uint param3);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_arc_to(IntPtr param0, float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_path_arc_to_with_oval(IntPtr param0, SKRect* oval, float startAngle, float sweepAngle, [MarshalAs(UnmanagedType.I1)] bool forceMoveTo);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_arc_to_with_points(IntPtr param0, float x1, float y1, float x2, float y2, float radius);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_path_clone(IntPtr cpath);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_close(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_path_compute_tight_bounds(IntPtr param0, SKRect* param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_conic_to(IntPtr param0, float x0, float y0, float x1, float y1, float w);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_path_contains(IntPtr cpath, float x, float y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int sk_path_convert_conic_to_quads(SKPoint* p0, SKPoint* p1, SKPoint* p2, float w, SKPoint* pts, int pow2);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_path_count_points(IntPtr cpath);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_path_count_verbs(IntPtr cpath);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_path_create_iter(IntPtr cpath, int forceClose);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_path_create_rawiter(IntPtr cpath);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_cubic_to(IntPtr param0, float x0, float y0, float x1, float y1, float x2, float y2);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_delete(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_path_get_bounds(IntPtr param0, SKRect* param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKPathFillType sk_path_get_filltype(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_path_get_last_point(IntPtr cpath, SKPoint* point);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_path_get_point(IntPtr cpath, int index, SKPoint* point);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int sk_path_get_points(IntPtr cpath, SKPoint* points, int max);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate uint sk_path_get_segment_masks(IntPtr cpath);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_path_is_convex(IntPtr cpath);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_path_is_line(IntPtr cpath, SKPoint* line);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_path_is_oval(IntPtr cpath, SKRect* bounds);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_path_is_rect(IntPtr cpath, SKRect* rect, byte* isClosed, SKPathDirection* direction);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_path_is_rrect(IntPtr cpath, IntPtr bounds);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate float sk_path_iter_conic_weight(IntPtr iterator);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_iter_destroy(IntPtr iterator);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_path_iter_is_close_line(IntPtr iterator);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_path_iter_is_closed_contour(IntPtr iterator);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate SKPathVerb sk_path_iter_next(IntPtr iterator, SKPoint* points);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_line_to(IntPtr param0, float x, float y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_move_to(IntPtr param0, float x, float y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_path_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_path_parse_svg_string(IntPtr cpath, [MarshalAs(UnmanagedType.LPStr)] string str);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_quad_to(IntPtr param0, float x0, float y0, float x1, float y1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_rarc_to(IntPtr param0, float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate float sk_path_rawiter_conic_weight(IntPtr iterator);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_rawiter_destroy(IntPtr iterator);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate SKPathVerb sk_path_rawiter_next(IntPtr iterator, SKPoint* points);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKPathVerb sk_path_rawiter_peek(IntPtr iterator);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_rconic_to(IntPtr param0, float dx0, float dy0, float dx1, float dy1, float w);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_rcubic_to(IntPtr param0, float dx0, float dy0, float dx1, float dy1, float dx2, float dy2);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_reset(IntPtr cpath);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_rewind(IntPtr cpath);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_rline_to(IntPtr param0, float dx, float yd);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_rmove_to(IntPtr param0, float dx, float dy);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_rquad_to(IntPtr param0, float dx0, float dy0, float dx1, float dy1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_set_filltype(IntPtr param0, SKPathFillType param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_to_svg_string(IntPtr cpath, IntPtr str);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_path_transform(IntPtr cpath, SKMatrix* cmatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_path_transform_to_dest(IntPtr cpath, SKMatrix* cmatrix, IntPtr destination);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_pathmeasure_destroy(IntPtr pathMeasure);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate float sk_pathmeasure_get_length(IntPtr pathMeasure);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_pathmeasure_get_matrix(IntPtr pathMeasure, float distance, SKMatrix* matrix, SKPathMeasureMatrixFlags flags);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_pathmeasure_get_pos_tan(IntPtr pathMeasure, float distance, SKPoint* position, SKPoint* tangent);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_pathmeasure_get_segment(IntPtr pathMeasure, float start, float stop, IntPtr dst, [MarshalAs(UnmanagedType.I1)] bool startWithMoveTo);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_pathmeasure_is_closed(IntPtr pathMeasure);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_pathmeasure_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_pathmeasure_new_with_path(IntPtr path, [MarshalAs(UnmanagedType.I1)] bool forceClosed, float resScale);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_pathmeasure_next_contour(IntPtr pathMeasure);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_pathmeasure_set_path(IntPtr pathMeasure, IntPtr path, [MarshalAs(UnmanagedType.I1)] bool forceClosed);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_pathop_as_winding(IntPtr path, IntPtr result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_pathop_op(IntPtr one, IntPtr two, SKPathOp op, IntPtr result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_pathop_simplify(IntPtr path, IntPtr result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_pathop_tight_bounds(IntPtr path, SKRect* result);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_path_effect_create_1d_path(IntPtr path, float advance, float phase, SKPath1DPathEffectStyle style);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_path_effect_create_2d_line(float width, SKMatrix* matrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_path_effect_create_2d_path(SKMatrix* matrix, IntPtr path);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_path_effect_create_compose(IntPtr outer, IntPtr inner);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_path_effect_create_corner(float radius);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_path_effect_create_dash(float* intervals, int count, float phase);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_path_effect_create_discrete(float segLength, float deviation, uint seedAssist);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_path_effect_create_sum(IntPtr first, IntPtr second);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_path_effect_create_trim(float start, float stop, SKTrimPathEffectMode mode);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_path_effect_unref(IntPtr t);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_picture_approximate_bytes_used(IntPtr picture);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_picture_approximate_op_count(IntPtr picture, [MarshalAs(UnmanagedType.I1)] bool nested);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_picture_deserialize_from_data(IntPtr data);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_picture_deserialize_from_memory(void* buffer, IntPtr length);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_picture_deserialize_from_stream(IntPtr stream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_picture_get_cull_rect(IntPtr param0, SKRect* param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_picture_get_recording_canvas(IntPtr crec);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate uint sk_picture_get_unique_id(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_picture_make_shader(IntPtr src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode mode, SKMatrix* localMatrix, SKRect* tile);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_picture_playback(IntPtr picture, IntPtr canvas);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_picture_recorder_begin_recording(IntPtr param0, SKRect* param1);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_picture_recorder_begin_recording_with_bbh_factory(IntPtr param0, SKRect* param1, IntPtr param2);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_picture_recorder_delete(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_picture_recorder_end_recording(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_picture_recorder_end_recording_as_drawable(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_picture_recorder_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_picture_ref(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_picture_serialize_to_data(IntPtr picture);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_picture_serialize_to_stream(IntPtr picture, IntPtr stream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_picture_unref(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_rtree_factory_delete(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_rtree_factory_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_color_get_bit_shift(int* a, int* r, int* g, int* b);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate uint sk_color_premultiply(uint color);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_color_premultiply_array(uint* colors, int size, uint* pmcolors);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate uint sk_color_unpremultiply(uint pmcolor);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_color_unpremultiply_array(uint* pmcolors, int size, uint* colors);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_jpegencoder_encode(IntPtr dst, IntPtr src, SKJpegEncoderOptions* options);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_pixmap_compute_is_opaque(IntPtr cpixmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_pixmap_destructor(IntPtr cpixmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_pixmap_erase_color(IntPtr cpixmap, uint color, SKRectI* subset);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_pixmap_erase_color4f(IntPtr cpixmap, SKColorF* color, SKRectI* subset);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_pixmap_extract_subset(IntPtr cpixmap, IntPtr result, SKRectI* subset);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_pixmap_get_colorspace(IntPtr cpixmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_pixmap_get_info(IntPtr cpixmap, SKImageInfoNative* cinfo);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate float sk_pixmap_get_pixel_alphaf(IntPtr cpixmap, int x, int y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate uint sk_pixmap_get_pixel_color(IntPtr cpixmap, int x, int y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_pixmap_get_pixel_color4f(IntPtr cpixmap, int x, int y, SKColorF* color);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_pixmap_get_row_bytes(IntPtr cpixmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void* sk_pixmap_get_writable_addr(IntPtr cpixmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void* sk_pixmap_get_writeable_addr_with_xy(IntPtr cpixmap, int x, int y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_pixmap_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_pixmap_new_with_params(SKImageInfoNative* cinfo, void* addr, IntPtr rowBytes);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_pixmap_read_pixels(IntPtr cpixmap, SKImageInfoNative* dstInfo, void* dstPixels, IntPtr dstRowBytes, int srcX, int srcY);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_pixmap_reset(IntPtr cpixmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_pixmap_reset_with_params(IntPtr cpixmap, SKImageInfoNative* cinfo, void* addr, IntPtr rowBytes);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_pixmap_scale_pixels(IntPtr cpixmap, IntPtr dst, SKSamplingOptions* sampling);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_pixmap_set_colorspace(IntPtr cpixmap, IntPtr colorspace);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_pngencoder_encode(IntPtr dst, IntPtr src, SKPngEncoderOptions* options);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_swizzle_swap_rb(uint* dest, uint* src, int count);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_webpencoder_encode(IntPtr dst, IntPtr src, SKWebpEncoderOptions* options);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_region_cliperator_delete(IntPtr iter);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_region_cliperator_done(IntPtr iter);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_region_cliperator_new(IntPtr region, SKRectI* clip);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_region_cliperator_next(IntPtr iter);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_region_cliperator_rect(IntPtr iter, SKRectI* rect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_region_contains(IntPtr r, IntPtr region);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_region_contains_point(IntPtr r, int x, int y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_region_contains_rect(IntPtr r, SKRectI* rect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_region_delete(IntPtr r);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_region_get_boundary_path(IntPtr r, IntPtr path);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_region_get_bounds(IntPtr r, SKRectI* rect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_region_intersects(IntPtr r, IntPtr src);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_region_intersects_rect(IntPtr r, SKRectI* rect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_region_is_complex(IntPtr r);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_region_is_empty(IntPtr r);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_region_is_rect(IntPtr r);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_region_iterator_delete(IntPtr iter);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_region_iterator_done(IntPtr iter);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_region_iterator_new(IntPtr region);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_region_iterator_next(IntPtr iter);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_region_iterator_rect(IntPtr iter, SKRectI* rect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_region_iterator_rewind(IntPtr iter);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_region_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_region_op(IntPtr r, IntPtr region, SKRegionOperation op);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_region_op_rect(IntPtr r, SKRectI* rect, SKRegionOperation op);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_region_quick_contains(IntPtr r, SKRectI* rect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_region_quick_reject(IntPtr r, IntPtr region);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_region_quick_reject_rect(IntPtr r, SKRectI* rect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_region_set_empty(IntPtr r);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_region_set_path(IntPtr r, IntPtr t, IntPtr clip);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_region_set_rect(IntPtr r, SKRectI* rect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_region_set_rects(IntPtr r, SKRectI* rects, int count);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_region_set_region(IntPtr r, IntPtr region);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_region_spanerator_delete(IntPtr iter);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_region_spanerator_new(IntPtr region, int y, int left, int right);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_region_spanerator_next(IntPtr iter, int* left, int* right);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_region_translate(IntPtr r, int x, int y);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_rrect_contains(IntPtr rrect, SKRect* rect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_rrect_delete(IntPtr rrect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate float sk_rrect_get_height(IntPtr rrect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_rrect_get_radii(IntPtr rrect, SKRoundRectCorner corner, SKPoint* radii);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_rrect_get_rect(IntPtr rrect, SKRect* rect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKRoundRectType sk_rrect_get_type(IntPtr rrect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate float sk_rrect_get_width(IntPtr rrect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_rrect_inset(IntPtr rrect, float dx, float dy);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_rrect_is_valid(IntPtr rrect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_rrect_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_rrect_new_copy(IntPtr rrect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_rrect_offset(IntPtr rrect, float dx, float dy);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_rrect_outset(IntPtr rrect, float dx, float dy);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_rrect_set_empty(IntPtr rrect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_rrect_set_nine_patch(IntPtr rrect, SKRect* rect, float leftRad, float topRad, float rightRad, float bottomRad);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_rrect_set_oval(IntPtr rrect, SKRect* rect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_rrect_set_rect(IntPtr rrect, SKRect* rect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_rrect_set_rect_radii(IntPtr rrect, SKRect* rect, SKPoint* radii);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_rrect_set_rect_xy(IntPtr rrect, SKRect* rect, float xRad, float yRad);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_rrect_transform(IntPtr rrect, SKMatrix* matrix, IntPtr dest);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_runtimeeffect_get_child_from_index(IntPtr effect, int index, SKRuntimeEffectChildNative* cchild);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_runtimeeffect_get_child_from_name(IntPtr effect, void* name, IntPtr len, SKRuntimeEffectChildNative* cchild);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_runtimeeffect_get_child_name(IntPtr effect, int index, IntPtr name);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_runtimeeffect_get_children_size(IntPtr effect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_runtimeeffect_get_uniform_byte_size(IntPtr effect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_runtimeeffect_get_uniform_from_index(IntPtr effect, int index, SKRuntimeEffectUniformNative* cuniform);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_runtimeeffect_get_uniform_from_name(IntPtr effect, void* name, IntPtr len, SKRuntimeEffectUniformNative* cuniform);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_runtimeeffect_get_uniform_name(IntPtr effect, int index, IntPtr name);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_runtimeeffect_get_uniforms_size(IntPtr effect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_runtimeeffect_make_blender(IntPtr effect, IntPtr uniforms, IntPtr* children, IntPtr childCount);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_runtimeeffect_make_color_filter(IntPtr effect, IntPtr uniforms, IntPtr* children, IntPtr childCount);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_runtimeeffect_make_for_blender(IntPtr sksl, IntPtr error);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_runtimeeffect_make_for_color_filter(IntPtr sksl, IntPtr error);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_runtimeeffect_make_for_shader(IntPtr sksl, IntPtr error);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_runtimeeffect_make_shader(IntPtr effect, IntPtr uniforms, IntPtr* children, IntPtr childCount, SKMatrix* localMatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_runtimeeffect_unref(IntPtr effect);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_shader_new_blend(SKBlendMode mode, IntPtr dst, IntPtr src);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_shader_new_blender(IntPtr blender, IntPtr dst, IntPtr src);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_shader_new_color(uint color);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_shader_new_color4f(SKColorF* color, IntPtr colorspace);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_shader_new_empty();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_shader_new_linear_gradient(SKPoint* points, uint* colors, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_shader_new_linear_gradient_color4f(SKPoint* points, SKColorF* colors, IntPtr colorspace, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_shader_new_perlin_noise_fractal_noise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKSizeI* tileSize);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_shader_new_perlin_noise_turbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKSizeI* tileSize);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_shader_new_radial_gradient(SKPoint* center, float radius, uint* colors, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_shader_new_radial_gradient_color4f(SKPoint* center, float radius, SKColorF* colors, IntPtr colorspace, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_shader_new_sweep_gradient(SKPoint* center, uint* colors, float* colorPos, int colorCount, SKShaderTileMode tileMode, float startAngle, float endAngle, SKMatrix* localMatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_shader_new_sweep_gradient_color4f(SKPoint* center, SKColorF* colors, IntPtr colorspace, float* colorPos, int colorCount, SKShaderTileMode tileMode, float startAngle, float endAngle, SKMatrix* localMatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_shader_new_two_point_conical_gradient(SKPoint* start, float startRadius, SKPoint* end, float endRadius, uint* colors, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_shader_new_two_point_conical_gradient_color4f(SKPoint* start, float startRadius, SKPoint* end, float endRadius, SKColorF* colors, IntPtr colorspace, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_shader_ref(IntPtr shader);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_shader_unref(IntPtr shader);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_shader_with_color_filter(IntPtr shader, IntPtr filter);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_shader_with_local_matrix(IntPtr shader, SKMatrix* localMatrix);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_dynamicmemorywstream_copy_to(IntPtr cstream, void* data);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_dynamicmemorywstream_destroy(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_dynamicmemorywstream_detach_as_data(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_dynamicmemorywstream_detach_as_stream(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_dynamicmemorywstream_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_dynamicmemorywstream_write_to_stream(IntPtr cstream, IntPtr dst);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_filestream_destroy(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_filestream_is_valid(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_filestream_new(void* path);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_filewstream_destroy(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_filewstream_is_valid(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_filewstream_new(void* path);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_memorystream_destroy(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_memorystream_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_memorystream_new_with_data(void* data, IntPtr length, [MarshalAs(UnmanagedType.I1)] bool copyData);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_memorystream_new_with_length(IntPtr length);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_memorystream_new_with_skdata(IntPtr data);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_memorystream_set_memory(IntPtr cmemorystream, void* data, IntPtr length, [MarshalAs(UnmanagedType.I1)] bool copyData);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_stream_asset_destroy(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_stream_destroy(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_stream_duplicate(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_stream_fork(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_stream_get_length(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void* sk_stream_get_memory_base(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_stream_get_position(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_stream_has_length(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_stream_has_position(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_stream_is_at_end(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_stream_move(IntPtr cstream, int offset);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_stream_peek(IntPtr cstream, void* buffer, IntPtr size);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_stream_read(IntPtr cstream, void* buffer, IntPtr size);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_stream_read_bool(IntPtr cstream, byte* buffer);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_stream_read_s16(IntPtr cstream, short* buffer);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_stream_read_s32(IntPtr cstream, int* buffer);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_stream_read_s8(IntPtr cstream, sbyte* buffer);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_stream_read_u16(IntPtr cstream, ushort* buffer);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_stream_read_u32(IntPtr cstream, uint* buffer);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_stream_read_u8(IntPtr cstream, byte* buffer);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_stream_rewind(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_stream_seek(IntPtr cstream, IntPtr position);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_stream_skip(IntPtr cstream, IntPtr size);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_wstream_bytes_written(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_wstream_flush(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_wstream_get_size_of_packed_uint(IntPtr value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_wstream_newline(IntPtr cstream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_wstream_write(IntPtr cstream, void* buffer, IntPtr size);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_wstream_write_16(IntPtr cstream, ushort value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_wstream_write_32(IntPtr cstream, uint value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_wstream_write_8(IntPtr cstream, byte value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_wstream_write_bigdec_as_text(IntPtr cstream, long value, int minDigits);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_wstream_write_bool(IntPtr cstream, [MarshalAs(UnmanagedType.I1)] bool value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_wstream_write_dec_as_text(IntPtr cstream, int value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_wstream_write_hex_as_text(IntPtr cstream, uint value, int minDigits);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_wstream_write_packed_uint(IntPtr cstream, IntPtr value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_wstream_write_scalar(IntPtr cstream, float value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_wstream_write_scalar_as_text(IntPtr cstream, float value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_wstream_write_stream(IntPtr cstream, IntPtr input, IntPtr length);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_wstream_write_text(IntPtr cstream, [MarshalAs(UnmanagedType.LPStr)] string value);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_string_destructor(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void* sk_string_get_c_str(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_string_get_size(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_string_new_empty();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_string_new_with_copy(void* src, IntPtr length);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_surface_draw(IntPtr surface, IntPtr canvas, float x, float y, IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_surface_get_canvas(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_surface_get_props(IntPtr surface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_surface_get_recording_context(IntPtr surface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_surface_new_backend_render_target(IntPtr context, IntPtr target, GRSurfaceOrigin origin, SKColorTypeNative colorType, IntPtr colorspace, IntPtr props);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_surface_new_backend_texture(IntPtr context, IntPtr texture, GRSurfaceOrigin origin, int samples, SKColorTypeNative colorType, IntPtr colorspace, IntPtr props);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_surface_new_image_snapshot(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_surface_new_image_snapshot_with_crop(IntPtr surface, SKRectI* bounds);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_surface_new_metal_layer(IntPtr context, void* layer, GRSurfaceOrigin origin, int sampleCount, SKColorTypeNative colorType, IntPtr colorspace, IntPtr props, void** drawable);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_surface_new_metal_view(IntPtr context, void* mtkView, GRSurfaceOrigin origin, int sampleCount, SKColorTypeNative colorType, IntPtr colorspace, IntPtr props);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_surface_new_null(int width, int height);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_surface_new_raster(SKImageInfoNative* param0, IntPtr rowBytes, IntPtr param2);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_surface_new_raster_direct(SKImageInfoNative* param0, void* pixels, IntPtr rowBytes, SKSurfaceRasterReleaseProxyDelegate releaseProc, void* context, IntPtr props);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_surface_new_render_target(IntPtr context, [MarshalAs(UnmanagedType.I1)] bool budgeted, SKImageInfoNative* cinfo, int sampleCount, GRSurfaceOrigin origin, IntPtr props, [MarshalAs(UnmanagedType.I1)] bool shouldCreateWithMips);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_surface_peek_pixels(IntPtr surface, IntPtr pixmap);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_surface_read_pixels(IntPtr surface, SKImageInfoNative* dstInfo, void* dstPixels, IntPtr dstRowBytes, int srcX, int srcY);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_surface_unref(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_surfaceprops_delete(IntPtr props);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate uint sk_surfaceprops_get_flags(IntPtr props);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKPixelGeometry sk_surfaceprops_get_pixel_geometry(IntPtr props);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_surfaceprops_new(uint flags, SKPixelGeometry geometry);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_svgcanvas_create_with_stream(SKRect* bounds, IntPtr stream);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_textblob_builder_alloc_run(IntPtr builder, IntPtr font, int count, float x, float y, SKRect* bounds, SKRunBufferInternal* runbuffer);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_textblob_builder_alloc_run_pos(IntPtr builder, IntPtr font, int count, SKRect* bounds, SKRunBufferInternal* runbuffer);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_textblob_builder_alloc_run_pos_h(IntPtr builder, IntPtr font, int count, float y, SKRect* bounds, SKRunBufferInternal* runbuffer);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_textblob_builder_alloc_run_rsxform(IntPtr builder, IntPtr font, int count, SKRect* bounds, SKRunBufferInternal* runbuffer);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_textblob_builder_alloc_run_text(IntPtr builder, IntPtr font, int count, float x, float y, int textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_textblob_builder_alloc_run_text_pos(IntPtr builder, IntPtr font, int count, int textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_textblob_builder_alloc_run_text_pos_h(IntPtr builder, IntPtr font, int count, float y, int textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_textblob_builder_alloc_run_text_rsxform(IntPtr builder, IntPtr font, int count, int textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_textblob_builder_delete(IntPtr builder);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_textblob_builder_make(IntPtr builder);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_textblob_builder_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_textblob_get_bounds(IntPtr blob, SKRect* bounds);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int sk_textblob_get_intercepts(IntPtr blob, float* bounds, float* intervals, IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate uint sk_textblob_get_unique_id(IntPtr blob);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_textblob_ref(IntPtr blob);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_textblob_unref(IntPtr blob);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_fontmgr_count_families(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_fontmgr_create_default();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_fontmgr_create_from_data(IntPtr param0, IntPtr data, int index);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_fontmgr_create_from_file(IntPtr param0, void* path, int index);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_fontmgr_create_from_stream(IntPtr param0, IntPtr stream, int index);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_fontmgr_create_styleset(IntPtr param0, int index);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_fontmgr_get_family_name(IntPtr param0, int index, IntPtr familyName);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_fontmgr_match_family(IntPtr param0, IntPtr familyName);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_fontmgr_match_family_style(IntPtr param0, IntPtr familyName, IntPtr style);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_fontmgr_match_family_style_character(IntPtr param0, IntPtr familyName, IntPtr style, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] bcp47, int bcp47Count, int character);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_fontmgr_ref_default();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_fontmgr_unref(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_fontstyle_delete(IntPtr fs);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKFontStyleSlant sk_fontstyle_get_slant(IntPtr fs);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_fontstyle_get_weight(IntPtr fs);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_fontstyle_get_width(IntPtr fs);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_fontstyle_new(int weight, int width, SKFontStyleSlant slant);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_fontstyleset_create_empty();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_fontstyleset_create_typeface(IntPtr fss, int index);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_fontstyleset_get_count(IntPtr fss);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_fontstyleset_get_style(IntPtr fss, int index, IntPtr fs, IntPtr style);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_fontstyleset_match_style(IntPtr fss, IntPtr style);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_fontstyleset_unref(IntPtr fss);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_typeface_copy_table_data(IntPtr typeface, uint tag);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_typeface_count_glyphs(IntPtr typeface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_typeface_count_tables(IntPtr typeface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_typeface_create_default();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_typeface_create_from_data(IntPtr data, int index);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_typeface_create_from_file(void* path, int index);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_typeface_create_from_name(IntPtr familyName, IntPtr style);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_typeface_create_from_stream(IntPtr stream, int index);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_typeface_get_family_name(IntPtr typeface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKFontStyleSlant sk_typeface_get_font_slant(IntPtr typeface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_typeface_get_font_weight(IntPtr typeface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_typeface_get_font_width(IntPtr typeface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_typeface_get_fontstyle(IntPtr typeface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal unsafe delegate bool sk_typeface_get_kerning_pair_adjustments(IntPtr typeface, ushort* glyphs, int count, int* adjustments);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_typeface_get_post_script_name(IntPtr typeface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_typeface_get_table_data(IntPtr typeface, uint tag, IntPtr offset, IntPtr length, void* data);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_typeface_get_table_size(IntPtr typeface, uint tag);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate int sk_typeface_get_table_tags(IntPtr typeface, uint* tags);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_typeface_get_units_per_em(IntPtr typeface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_typeface_is_fixed_pitch(IntPtr typeface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_typeface_open_stream(IntPtr typeface, int* ttcIndex);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_typeface_ref_default();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate ushort sk_typeface_unichar_to_glyph(IntPtr typeface, int unichar);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate void sk_typeface_unichars_to_glyphs(IntPtr typeface, int* unichars, int count, ushort* glyphs);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_typeface_unref(IntPtr typeface);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_vertices_make_copy(SKVertexMode vmode, int vertexCount, SKPoint* positions, SKPoint* texs, uint* colors, int indexCount, ushort* indices);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_vertices_ref(IntPtr cvertices);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_vertices_unref(IntPtr cvertices);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_compatpaint_clone(IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_compatpaint_delete(IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate int sk_compatpaint_get_filter_quality(IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_compatpaint_get_font(IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal delegate bool sk_compatpaint_get_lcd_render_text(IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKTextAlign sk_compatpaint_get_text_align(IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate SKTextEncoding sk_compatpaint_get_text_encoding(IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_compatpaint_make_font(IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_compatpaint_new();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate IntPtr sk_compatpaint_new_with_font(IntPtr font);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_compatpaint_reset(IntPtr paint);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_compatpaint_set_filter_quality(IntPtr paint, int quality);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_compatpaint_set_is_antialias(IntPtr paint, [MarshalAs(UnmanagedType.I1)] bool antialias);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_compatpaint_set_lcd_render_text(IntPtr paint, [MarshalAs(UnmanagedType.I1)] bool lcdRenderText);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_compatpaint_set_text_align(IntPtr paint, SKTextAlign align);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_compatpaint_set_text_encoding(IntPtr paint, SKTextEncoding encoding);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_manageddrawable_new(void* context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_manageddrawable_set_procs(SKManagedDrawableDelegates procs);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_manageddrawable_unref(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_managedstream_destroy(IntPtr s);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_managedstream_new(void* context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_managedstream_set_procs(SKManagedStreamDelegates procs);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_managedwstream_destroy(IntPtr s);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_managedwstream_new(void* context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_managedwstream_set_procs(SKManagedWStreamDelegates procs);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_managedtracememorydump_delete(IntPtr param0);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal unsafe delegate IntPtr sk_managedtracememorydump_new([MarshalAs(UnmanagedType.I1)] bool detailed, [MarshalAs(UnmanagedType.I1)] bool dumpWrapped, void* context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void sk_managedtracememorydump_set_procs(SKManagedTraceMemoryDumpDelegates procs);
	}

	private const string SKIA = "libSkiaSharp";

	private static readonly Lazy<IntPtr> libSkiaSharpHandle = new Lazy<IntPtr>(() => LibraryLoader.LoadLocalLibrary<SkiaApi>("libSkiaSharp"));

	private static Delegates.gr_backendrendertarget_delete gr_backendrendertarget_delete_delegate;

	private static Delegates.gr_backendrendertarget_get_backend gr_backendrendertarget_get_backend_delegate;

	private static Delegates.gr_backendrendertarget_get_gl_framebufferinfo gr_backendrendertarget_get_gl_framebufferinfo_delegate;

	private static Delegates.gr_backendrendertarget_get_height gr_backendrendertarget_get_height_delegate;

	private static Delegates.gr_backendrendertarget_get_samples gr_backendrendertarget_get_samples_delegate;

	private static Delegates.gr_backendrendertarget_get_stencils gr_backendrendertarget_get_stencils_delegate;

	private static Delegates.gr_backendrendertarget_get_width gr_backendrendertarget_get_width_delegate;

	private static Delegates.gr_backendrendertarget_is_valid gr_backendrendertarget_is_valid_delegate;

	private static Delegates.gr_backendrendertarget_new_direct3d gr_backendrendertarget_new_direct3d_delegate;

	private static Delegates.gr_backendrendertarget_new_gl gr_backendrendertarget_new_gl_delegate;

	private static Delegates.gr_backendrendertarget_new_metal gr_backendrendertarget_new_metal_delegate;

	private static Delegates.gr_backendrendertarget_new_vulkan gr_backendrendertarget_new_vulkan_delegate;

	private static Delegates.gr_backendtexture_delete gr_backendtexture_delete_delegate;

	private static Delegates.gr_backendtexture_get_backend gr_backendtexture_get_backend_delegate;

	private static Delegates.gr_backendtexture_get_gl_textureinfo gr_backendtexture_get_gl_textureinfo_delegate;

	private static Delegates.gr_backendtexture_get_height gr_backendtexture_get_height_delegate;

	private static Delegates.gr_backendtexture_get_width gr_backendtexture_get_width_delegate;

	private static Delegates.gr_backendtexture_has_mipmaps gr_backendtexture_has_mipmaps_delegate;

	private static Delegates.gr_backendtexture_is_valid gr_backendtexture_is_valid_delegate;

	private static Delegates.gr_backendtexture_new_direct3d gr_backendtexture_new_direct3d_delegate;

	private static Delegates.gr_backendtexture_new_gl gr_backendtexture_new_gl_delegate;

	private static Delegates.gr_backendtexture_new_metal gr_backendtexture_new_metal_delegate;

	private static Delegates.gr_backendtexture_new_vulkan gr_backendtexture_new_vulkan_delegate;

	private static Delegates.gr_direct_context_abandon_context gr_direct_context_abandon_context_delegate;

	private static Delegates.gr_direct_context_dump_memory_statistics gr_direct_context_dump_memory_statistics_delegate;

	private static Delegates.gr_direct_context_flush gr_direct_context_flush_delegate;

	private static Delegates.gr_direct_context_flush_and_submit gr_direct_context_flush_and_submit_delegate;

	private static Delegates.gr_direct_context_flush_image gr_direct_context_flush_image_delegate;

	private static Delegates.gr_direct_context_flush_surface gr_direct_context_flush_surface_delegate;

	private static Delegates.gr_direct_context_free_gpu_resources gr_direct_context_free_gpu_resources_delegate;

	private static Delegates.gr_direct_context_get_resource_cache_limit gr_direct_context_get_resource_cache_limit_delegate;

	private static Delegates.gr_direct_context_get_resource_cache_usage gr_direct_context_get_resource_cache_usage_delegate;

	private static Delegates.gr_direct_context_is_abandoned gr_direct_context_is_abandoned_delegate;

	private static Delegates.gr_direct_context_make_direct3d gr_direct_context_make_direct3d_delegate;

	private static Delegates.gr_direct_context_make_direct3d_with_options gr_direct_context_make_direct3d_with_options_delegate;

	private static Delegates.gr_direct_context_make_gl gr_direct_context_make_gl_delegate;

	private static Delegates.gr_direct_context_make_gl_with_options gr_direct_context_make_gl_with_options_delegate;

	private static Delegates.gr_direct_context_make_metal gr_direct_context_make_metal_delegate;

	private static Delegates.gr_direct_context_make_metal_with_options gr_direct_context_make_metal_with_options_delegate;

	private static Delegates.gr_direct_context_make_vulkan gr_direct_context_make_vulkan_delegate;

	private static Delegates.gr_direct_context_make_vulkan_with_options gr_direct_context_make_vulkan_with_options_delegate;

	private static Delegates.gr_direct_context_perform_deferred_cleanup gr_direct_context_perform_deferred_cleanup_delegate;

	private static Delegates.gr_direct_context_purge_unlocked_resources gr_direct_context_purge_unlocked_resources_delegate;

	private static Delegates.gr_direct_context_purge_unlocked_resources_bytes gr_direct_context_purge_unlocked_resources_bytes_delegate;

	private static Delegates.gr_direct_context_release_resources_and_abandon_context gr_direct_context_release_resources_and_abandon_context_delegate;

	private static Delegates.gr_direct_context_reset_context gr_direct_context_reset_context_delegate;

	private static Delegates.gr_direct_context_set_resource_cache_limit gr_direct_context_set_resource_cache_limit_delegate;

	private static Delegates.gr_direct_context_submit gr_direct_context_submit_delegate;

	private static Delegates.gr_glinterface_assemble_gl_interface gr_glinterface_assemble_gl_interface_delegate;

	private static Delegates.gr_glinterface_assemble_gles_interface gr_glinterface_assemble_gles_interface_delegate;

	private static Delegates.gr_glinterface_assemble_interface gr_glinterface_assemble_interface_delegate;

	private static Delegates.gr_glinterface_assemble_webgl_interface gr_glinterface_assemble_webgl_interface_delegate;

	private static Delegates.gr_glinterface_create_native_interface gr_glinterface_create_native_interface_delegate;

	private static Delegates.gr_glinterface_has_extension gr_glinterface_has_extension_delegate;

	private static Delegates.gr_glinterface_unref gr_glinterface_unref_delegate;

	private static Delegates.gr_glinterface_validate gr_glinterface_validate_delegate;

	private static Delegates.gr_recording_context_get_backend gr_recording_context_get_backend_delegate;

	private static Delegates.gr_recording_context_get_direct_context gr_recording_context_get_direct_context_delegate;

	private static Delegates.gr_recording_context_get_max_surface_sample_count_for_color_type gr_recording_context_get_max_surface_sample_count_for_color_type_delegate;

	private static Delegates.gr_recording_context_is_abandoned gr_recording_context_is_abandoned_delegate;

	private static Delegates.gr_recording_context_max_render_target_size gr_recording_context_max_render_target_size_delegate;

	private static Delegates.gr_recording_context_max_texture_size gr_recording_context_max_texture_size_delegate;

	private static Delegates.gr_recording_context_unref gr_recording_context_unref_delegate;

	private static Delegates.gr_vk_extensions_delete gr_vk_extensions_delete_delegate;

	private static Delegates.gr_vk_extensions_has_extension gr_vk_extensions_has_extension_delegate;

	private static Delegates.gr_vk_extensions_init gr_vk_extensions_init_delegate;

	private static Delegates.gr_vk_extensions_new gr_vk_extensions_new_delegate;

	private static Delegates.sk_bitmap_destructor sk_bitmap_destructor_delegate;

	private static Delegates.sk_bitmap_erase sk_bitmap_erase_delegate;

	private static Delegates.sk_bitmap_erase_rect sk_bitmap_erase_rect_delegate;

	private static Delegates.sk_bitmap_extract_alpha sk_bitmap_extract_alpha_delegate;

	private static Delegates.sk_bitmap_extract_subset sk_bitmap_extract_subset_delegate;

	private static Delegates.sk_bitmap_get_addr sk_bitmap_get_addr_delegate;

	private static Delegates.sk_bitmap_get_addr_16 sk_bitmap_get_addr_16_delegate;

	private static Delegates.sk_bitmap_get_addr_32 sk_bitmap_get_addr_32_delegate;

	private static Delegates.sk_bitmap_get_addr_8 sk_bitmap_get_addr_8_delegate;

	private static Delegates.sk_bitmap_get_byte_count sk_bitmap_get_byte_count_delegate;

	private static Delegates.sk_bitmap_get_info sk_bitmap_get_info_delegate;

	private static Delegates.sk_bitmap_get_pixel_color sk_bitmap_get_pixel_color_delegate;

	private static Delegates.sk_bitmap_get_pixel_colors sk_bitmap_get_pixel_colors_delegate;

	private static Delegates.sk_bitmap_get_pixels sk_bitmap_get_pixels_delegate;

	private static Delegates.sk_bitmap_get_row_bytes sk_bitmap_get_row_bytes_delegate;

	private static Delegates.sk_bitmap_install_pixels sk_bitmap_install_pixels_delegate;

	private static Delegates.sk_bitmap_install_pixels_with_pixmap sk_bitmap_install_pixels_with_pixmap_delegate;

	private static Delegates.sk_bitmap_is_immutable sk_bitmap_is_immutable_delegate;

	private static Delegates.sk_bitmap_is_null sk_bitmap_is_null_delegate;

	private static Delegates.sk_bitmap_make_shader sk_bitmap_make_shader_delegate;

	private static Delegates.sk_bitmap_new sk_bitmap_new_delegate;

	private static Delegates.sk_bitmap_notify_pixels_changed sk_bitmap_notify_pixels_changed_delegate;

	private static Delegates.sk_bitmap_peek_pixels sk_bitmap_peek_pixels_delegate;

	private static Delegates.sk_bitmap_ready_to_draw sk_bitmap_ready_to_draw_delegate;

	private static Delegates.sk_bitmap_reset sk_bitmap_reset_delegate;

	private static Delegates.sk_bitmap_set_immutable sk_bitmap_set_immutable_delegate;

	private static Delegates.sk_bitmap_set_pixels sk_bitmap_set_pixels_delegate;

	private static Delegates.sk_bitmap_swap sk_bitmap_swap_delegate;

	private static Delegates.sk_bitmap_try_alloc_pixels sk_bitmap_try_alloc_pixels_delegate;

	private static Delegates.sk_bitmap_try_alloc_pixels_with_flags sk_bitmap_try_alloc_pixels_with_flags_delegate;

	private static Delegates.sk_blender_new_arithmetic sk_blender_new_arithmetic_delegate;

	private static Delegates.sk_blender_new_mode sk_blender_new_mode_delegate;

	private static Delegates.sk_blender_ref sk_blender_ref_delegate;

	private static Delegates.sk_blender_unref sk_blender_unref_delegate;

	private static Delegates.sk_canvas_clear sk_canvas_clear_delegate;

	private static Delegates.sk_canvas_clear_color4f sk_canvas_clear_color4f_delegate;

	private static Delegates.sk_canvas_clip_path_with_operation sk_canvas_clip_path_with_operation_delegate;

	private static Delegates.sk_canvas_clip_rect_with_operation sk_canvas_clip_rect_with_operation_delegate;

	private static Delegates.sk_canvas_clip_region sk_canvas_clip_region_delegate;

	private static Delegates.sk_canvas_clip_rrect_with_operation sk_canvas_clip_rrect_with_operation_delegate;

	private static Delegates.sk_canvas_concat sk_canvas_concat_delegate;

	private static Delegates.sk_canvas_destroy sk_canvas_destroy_delegate;

	private static Delegates.sk_canvas_discard sk_canvas_discard_delegate;

	private static Delegates.sk_canvas_draw_annotation sk_canvas_draw_annotation_delegate;

	private static Delegates.sk_canvas_draw_arc sk_canvas_draw_arc_delegate;

	private static Delegates.sk_canvas_draw_atlas sk_canvas_draw_atlas_delegate;

	private static Delegates.sk_canvas_draw_circle sk_canvas_draw_circle_delegate;

	private static Delegates.sk_canvas_draw_color sk_canvas_draw_color_delegate;

	private static Delegates.sk_canvas_draw_color4f sk_canvas_draw_color4f_delegate;

	private static Delegates.sk_canvas_draw_drawable sk_canvas_draw_drawable_delegate;

	private static Delegates.sk_canvas_draw_drrect sk_canvas_draw_drrect_delegate;

	private static Delegates.sk_canvas_draw_image sk_canvas_draw_image_delegate;

	private static Delegates.sk_canvas_draw_image_lattice sk_canvas_draw_image_lattice_delegate;

	private static Delegates.sk_canvas_draw_image_nine sk_canvas_draw_image_nine_delegate;

	private static Delegates.sk_canvas_draw_image_rect sk_canvas_draw_image_rect_delegate;

	private static Delegates.sk_canvas_draw_line sk_canvas_draw_line_delegate;

	private static Delegates.sk_canvas_draw_link_destination_annotation sk_canvas_draw_link_destination_annotation_delegate;

	private static Delegates.sk_canvas_draw_named_destination_annotation sk_canvas_draw_named_destination_annotation_delegate;

	private static Delegates.sk_canvas_draw_oval sk_canvas_draw_oval_delegate;

	private static Delegates.sk_canvas_draw_paint sk_canvas_draw_paint_delegate;

	private static Delegates.sk_canvas_draw_patch sk_canvas_draw_patch_delegate;

	private static Delegates.sk_canvas_draw_path sk_canvas_draw_path_delegate;

	private static Delegates.sk_canvas_draw_picture sk_canvas_draw_picture_delegate;

	private static Delegates.sk_canvas_draw_point sk_canvas_draw_point_delegate;

	private static Delegates.sk_canvas_draw_points sk_canvas_draw_points_delegate;

	private static Delegates.sk_canvas_draw_rect sk_canvas_draw_rect_delegate;

	private static Delegates.sk_canvas_draw_region sk_canvas_draw_region_delegate;

	private static Delegates.sk_canvas_draw_round_rect sk_canvas_draw_round_rect_delegate;

	private static Delegates.sk_canvas_draw_rrect sk_canvas_draw_rrect_delegate;

	private static Delegates.sk_canvas_draw_simple_text sk_canvas_draw_simple_text_delegate;

	private static Delegates.sk_canvas_draw_text_blob sk_canvas_draw_text_blob_delegate;

	private static Delegates.sk_canvas_draw_url_annotation sk_canvas_draw_url_annotation_delegate;

	private static Delegates.sk_canvas_draw_vertices sk_canvas_draw_vertices_delegate;

	private static Delegates.sk_canvas_get_device_clip_bounds sk_canvas_get_device_clip_bounds_delegate;

	private static Delegates.sk_canvas_get_local_clip_bounds sk_canvas_get_local_clip_bounds_delegate;

	private static Delegates.sk_canvas_get_matrix sk_canvas_get_matrix_delegate;

	private static Delegates.sk_canvas_get_save_count sk_canvas_get_save_count_delegate;

	private static Delegates.sk_canvas_is_clip_empty sk_canvas_is_clip_empty_delegate;

	private static Delegates.sk_canvas_is_clip_rect sk_canvas_is_clip_rect_delegate;

	private static Delegates.sk_canvas_new_from_bitmap sk_canvas_new_from_bitmap_delegate;

	private static Delegates.sk_canvas_new_from_raster sk_canvas_new_from_raster_delegate;

	private static Delegates.sk_canvas_quick_reject sk_canvas_quick_reject_delegate;

	private static Delegates.sk_canvas_reset_matrix sk_canvas_reset_matrix_delegate;

	private static Delegates.sk_canvas_restore sk_canvas_restore_delegate;

	private static Delegates.sk_canvas_restore_to_count sk_canvas_restore_to_count_delegate;

	private static Delegates.sk_canvas_rotate_degrees sk_canvas_rotate_degrees_delegate;

	private static Delegates.sk_canvas_rotate_radians sk_canvas_rotate_radians_delegate;

	private static Delegates.sk_canvas_save sk_canvas_save_delegate;

	private static Delegates.sk_canvas_save_layer sk_canvas_save_layer_delegate;

	private static Delegates.sk_canvas_save_layer_rec sk_canvas_save_layer_rec_delegate;

	private static Delegates.sk_canvas_scale sk_canvas_scale_delegate;

	private static Delegates.sk_canvas_set_matrix sk_canvas_set_matrix_delegate;

	private static Delegates.sk_canvas_skew sk_canvas_skew_delegate;

	private static Delegates.sk_canvas_translate sk_canvas_translate_delegate;

	private static Delegates.sk_get_recording_context sk_get_recording_context_delegate;

	private static Delegates.sk_get_surface sk_get_surface_delegate;

	private static Delegates.sk_nodraw_canvas_destroy sk_nodraw_canvas_destroy_delegate;

	private static Delegates.sk_nodraw_canvas_new sk_nodraw_canvas_new_delegate;

	private static Delegates.sk_nway_canvas_add_canvas sk_nway_canvas_add_canvas_delegate;

	private static Delegates.sk_nway_canvas_destroy sk_nway_canvas_destroy_delegate;

	private static Delegates.sk_nway_canvas_new sk_nway_canvas_new_delegate;

	private static Delegates.sk_nway_canvas_remove_all sk_nway_canvas_remove_all_delegate;

	private static Delegates.sk_nway_canvas_remove_canvas sk_nway_canvas_remove_canvas_delegate;

	private static Delegates.sk_overdraw_canvas_destroy sk_overdraw_canvas_destroy_delegate;

	private static Delegates.sk_overdraw_canvas_new sk_overdraw_canvas_new_delegate;

	private static Delegates.sk_codec_destroy sk_codec_destroy_delegate;

	private static Delegates.sk_codec_get_encoded_format sk_codec_get_encoded_format_delegate;

	private static Delegates.sk_codec_get_frame_count sk_codec_get_frame_count_delegate;

	private static Delegates.sk_codec_get_frame_info sk_codec_get_frame_info_delegate;

	private static Delegates.sk_codec_get_frame_info_for_index sk_codec_get_frame_info_for_index_delegate;

	private static Delegates.sk_codec_get_info sk_codec_get_info_delegate;

	private static Delegates.sk_codec_get_origin sk_codec_get_origin_delegate;

	private static Delegates.sk_codec_get_pixels sk_codec_get_pixels_delegate;

	private static Delegates.sk_codec_get_repetition_count sk_codec_get_repetition_count_delegate;

	private static Delegates.sk_codec_get_scaled_dimensions sk_codec_get_scaled_dimensions_delegate;

	private static Delegates.sk_codec_get_scanline_order sk_codec_get_scanline_order_delegate;

	private static Delegates.sk_codec_get_scanlines sk_codec_get_scanlines_delegate;

	private static Delegates.sk_codec_get_valid_subset sk_codec_get_valid_subset_delegate;

	private static Delegates.sk_codec_incremental_decode sk_codec_incremental_decode_delegate;

	private static Delegates.sk_codec_min_buffered_bytes_needed sk_codec_min_buffered_bytes_needed_delegate;

	private static Delegates.sk_codec_new_from_data sk_codec_new_from_data_delegate;

	private static Delegates.sk_codec_new_from_stream sk_codec_new_from_stream_delegate;

	private static Delegates.sk_codec_next_scanline sk_codec_next_scanline_delegate;

	private static Delegates.sk_codec_output_scanline sk_codec_output_scanline_delegate;

	private static Delegates.sk_codec_skip_scanlines sk_codec_skip_scanlines_delegate;

	private static Delegates.sk_codec_start_incremental_decode sk_codec_start_incremental_decode_delegate;

	private static Delegates.sk_codec_start_scanline_decode sk_codec_start_scanline_decode_delegate;

	private static Delegates.sk_colorfilter_new_color_matrix sk_colorfilter_new_color_matrix_delegate;

	private static Delegates.sk_colorfilter_new_compose sk_colorfilter_new_compose_delegate;

	private static Delegates.sk_colorfilter_new_high_contrast sk_colorfilter_new_high_contrast_delegate;

	private static Delegates.sk_colorfilter_new_hsla_matrix sk_colorfilter_new_hsla_matrix_delegate;

	private static Delegates.sk_colorfilter_new_lerp sk_colorfilter_new_lerp_delegate;

	private static Delegates.sk_colorfilter_new_lighting sk_colorfilter_new_lighting_delegate;

	private static Delegates.sk_colorfilter_new_linear_to_srgb_gamma sk_colorfilter_new_linear_to_srgb_gamma_delegate;

	private static Delegates.sk_colorfilter_new_luma_color sk_colorfilter_new_luma_color_delegate;

	private static Delegates.sk_colorfilter_new_mode sk_colorfilter_new_mode_delegate;

	private static Delegates.sk_colorfilter_new_srgb_to_linear_gamma sk_colorfilter_new_srgb_to_linear_gamma_delegate;

	private static Delegates.sk_colorfilter_new_table sk_colorfilter_new_table_delegate;

	private static Delegates.sk_colorfilter_new_table_argb sk_colorfilter_new_table_argb_delegate;

	private static Delegates.sk_colorfilter_unref sk_colorfilter_unref_delegate;

	private static Delegates.sk_color4f_from_color sk_color4f_from_color_delegate;

	private static Delegates.sk_color4f_to_color sk_color4f_to_color_delegate;

	private static Delegates.sk_colorspace_equals sk_colorspace_equals_delegate;

	private static Delegates.sk_colorspace_gamma_close_to_srgb sk_colorspace_gamma_close_to_srgb_delegate;

	private static Delegates.sk_colorspace_gamma_is_linear sk_colorspace_gamma_is_linear_delegate;

	private static Delegates.sk_colorspace_icc_profile_delete sk_colorspace_icc_profile_delete_delegate;

	private static Delegates.sk_colorspace_icc_profile_get_buffer sk_colorspace_icc_profile_get_buffer_delegate;

	private static Delegates.sk_colorspace_icc_profile_get_to_xyzd50 sk_colorspace_icc_profile_get_to_xyzd50_delegate;

	private static Delegates.sk_colorspace_icc_profile_new sk_colorspace_icc_profile_new_delegate;

	private static Delegates.sk_colorspace_icc_profile_parse sk_colorspace_icc_profile_parse_delegate;

	private static Delegates.sk_colorspace_is_numerical_transfer_fn sk_colorspace_is_numerical_transfer_fn_delegate;

	private static Delegates.sk_colorspace_is_srgb sk_colorspace_is_srgb_delegate;

	private static Delegates.sk_colorspace_make_linear_gamma sk_colorspace_make_linear_gamma_delegate;

	private static Delegates.sk_colorspace_make_srgb_gamma sk_colorspace_make_srgb_gamma_delegate;

	private static Delegates.sk_colorspace_new_icc sk_colorspace_new_icc_delegate;

	private static Delegates.sk_colorspace_new_rgb sk_colorspace_new_rgb_delegate;

	private static Delegates.sk_colorspace_new_srgb sk_colorspace_new_srgb_delegate;

	private static Delegates.sk_colorspace_new_srgb_linear sk_colorspace_new_srgb_linear_delegate;

	private static Delegates.sk_colorspace_primaries_to_xyzd50 sk_colorspace_primaries_to_xyzd50_delegate;

	private static Delegates.sk_colorspace_ref sk_colorspace_ref_delegate;

	private static Delegates.sk_colorspace_to_profile sk_colorspace_to_profile_delegate;

	private static Delegates.sk_colorspace_to_xyzd50 sk_colorspace_to_xyzd50_delegate;

	private static Delegates.sk_colorspace_transfer_fn_eval sk_colorspace_transfer_fn_eval_delegate;

	private static Delegates.sk_colorspace_transfer_fn_invert sk_colorspace_transfer_fn_invert_delegate;

	private static Delegates.sk_colorspace_transfer_fn_named_2dot2 sk_colorspace_transfer_fn_named_2dot2_delegate;

	private static Delegates.sk_colorspace_transfer_fn_named_hlg sk_colorspace_transfer_fn_named_hlg_delegate;

	private static Delegates.sk_colorspace_transfer_fn_named_linear sk_colorspace_transfer_fn_named_linear_delegate;

	private static Delegates.sk_colorspace_transfer_fn_named_pq sk_colorspace_transfer_fn_named_pq_delegate;

	private static Delegates.sk_colorspace_transfer_fn_named_rec2020 sk_colorspace_transfer_fn_named_rec2020_delegate;

	private static Delegates.sk_colorspace_transfer_fn_named_srgb sk_colorspace_transfer_fn_named_srgb_delegate;

	private static Delegates.sk_colorspace_unref sk_colorspace_unref_delegate;

	private static Delegates.sk_colorspace_xyz_concat sk_colorspace_xyz_concat_delegate;

	private static Delegates.sk_colorspace_xyz_invert sk_colorspace_xyz_invert_delegate;

	private static Delegates.sk_colorspace_xyz_named_adobe_rgb sk_colorspace_xyz_named_adobe_rgb_delegate;

	private static Delegates.sk_colorspace_xyz_named_display_p3 sk_colorspace_xyz_named_display_p3_delegate;

	private static Delegates.sk_colorspace_xyz_named_rec2020 sk_colorspace_xyz_named_rec2020_delegate;

	private static Delegates.sk_colorspace_xyz_named_srgb sk_colorspace_xyz_named_srgb_delegate;

	private static Delegates.sk_colorspace_xyz_named_xyz sk_colorspace_xyz_named_xyz_delegate;

	private static Delegates.sk_data_get_bytes sk_data_get_bytes_delegate;

	private static Delegates.sk_data_get_data sk_data_get_data_delegate;

	private static Delegates.sk_data_get_size sk_data_get_size_delegate;

	private static Delegates.sk_data_new_empty sk_data_new_empty_delegate;

	private static Delegates.sk_data_new_from_file sk_data_new_from_file_delegate;

	private static Delegates.sk_data_new_from_stream sk_data_new_from_stream_delegate;

	private static Delegates.sk_data_new_subset sk_data_new_subset_delegate;

	private static Delegates.sk_data_new_uninitialized sk_data_new_uninitialized_delegate;

	private static Delegates.sk_data_new_with_copy sk_data_new_with_copy_delegate;

	private static Delegates.sk_data_new_with_proc sk_data_new_with_proc_delegate;

	private static Delegates.sk_data_ref sk_data_ref_delegate;

	private static Delegates.sk_data_unref sk_data_unref_delegate;

	private static Delegates.sk_document_abort sk_document_abort_delegate;

	private static Delegates.sk_document_begin_page sk_document_begin_page_delegate;

	private static Delegates.sk_document_close sk_document_close_delegate;

	private static Delegates.sk_document_create_pdf_from_stream sk_document_create_pdf_from_stream_delegate;

	private static Delegates.sk_document_create_pdf_from_stream_with_metadata sk_document_create_pdf_from_stream_with_metadata_delegate;

	private static Delegates.sk_document_create_xps_from_stream sk_document_create_xps_from_stream_delegate;

	private static Delegates.sk_document_end_page sk_document_end_page_delegate;

	private static Delegates.sk_document_unref sk_document_unref_delegate;

	private static Delegates.sk_drawable_approximate_bytes_used sk_drawable_approximate_bytes_used_delegate;

	private static Delegates.sk_drawable_draw sk_drawable_draw_delegate;

	private static Delegates.sk_drawable_get_bounds sk_drawable_get_bounds_delegate;

	private static Delegates.sk_drawable_get_generation_id sk_drawable_get_generation_id_delegate;

	private static Delegates.sk_drawable_new_picture_snapshot sk_drawable_new_picture_snapshot_delegate;

	private static Delegates.sk_drawable_notify_drawing_changed sk_drawable_notify_drawing_changed_delegate;

	private static Delegates.sk_drawable_unref sk_drawable_unref_delegate;

	private static Delegates.sk_font_break_text sk_font_break_text_delegate;

	private static Delegates.sk_font_delete sk_font_delete_delegate;

	private static Delegates.sk_font_get_edging sk_font_get_edging_delegate;

	private static Delegates.sk_font_get_hinting sk_font_get_hinting_delegate;

	private static Delegates.sk_font_get_metrics sk_font_get_metrics_delegate;

	private static Delegates.sk_font_get_path sk_font_get_path_delegate;

	private static Delegates.sk_font_get_paths sk_font_get_paths_delegate;

	private static Delegates.sk_font_get_pos sk_font_get_pos_delegate;

	private static Delegates.sk_font_get_scale_x sk_font_get_scale_x_delegate;

	private static Delegates.sk_font_get_size sk_font_get_size_delegate;

	private static Delegates.sk_font_get_skew_x sk_font_get_skew_x_delegate;

	private static Delegates.sk_font_get_typeface sk_font_get_typeface_delegate;

	private static Delegates.sk_font_get_widths_bounds sk_font_get_widths_bounds_delegate;

	private static Delegates.sk_font_get_xpos sk_font_get_xpos_delegate;

	private static Delegates.sk_font_is_baseline_snap sk_font_is_baseline_snap_delegate;

	private static Delegates.sk_font_is_embedded_bitmaps sk_font_is_embedded_bitmaps_delegate;

	private static Delegates.sk_font_is_embolden sk_font_is_embolden_delegate;

	private static Delegates.sk_font_is_force_auto_hinting sk_font_is_force_auto_hinting_delegate;

	private static Delegates.sk_font_is_linear_metrics sk_font_is_linear_metrics_delegate;

	private static Delegates.sk_font_is_subpixel sk_font_is_subpixel_delegate;

	private static Delegates.sk_font_measure_text sk_font_measure_text_delegate;

	private static Delegates.sk_font_measure_text_no_return sk_font_measure_text_no_return_delegate;

	private static Delegates.sk_font_new sk_font_new_delegate;

	private static Delegates.sk_font_new_with_values sk_font_new_with_values_delegate;

	private static Delegates.sk_font_set_baseline_snap sk_font_set_baseline_snap_delegate;

	private static Delegates.sk_font_set_edging sk_font_set_edging_delegate;

	private static Delegates.sk_font_set_embedded_bitmaps sk_font_set_embedded_bitmaps_delegate;

	private static Delegates.sk_font_set_embolden sk_font_set_embolden_delegate;

	private static Delegates.sk_font_set_force_auto_hinting sk_font_set_force_auto_hinting_delegate;

	private static Delegates.sk_font_set_hinting sk_font_set_hinting_delegate;

	private static Delegates.sk_font_set_linear_metrics sk_font_set_linear_metrics_delegate;

	private static Delegates.sk_font_set_scale_x sk_font_set_scale_x_delegate;

	private static Delegates.sk_font_set_size sk_font_set_size_delegate;

	private static Delegates.sk_font_set_skew_x sk_font_set_skew_x_delegate;

	private static Delegates.sk_font_set_subpixel sk_font_set_subpixel_delegate;

	private static Delegates.sk_font_set_typeface sk_font_set_typeface_delegate;

	private static Delegates.sk_font_text_to_glyphs sk_font_text_to_glyphs_delegate;

	private static Delegates.sk_font_unichar_to_glyph sk_font_unichar_to_glyph_delegate;

	private static Delegates.sk_font_unichars_to_glyphs sk_font_unichars_to_glyphs_delegate;

	private static Delegates.sk_text_utils_get_path sk_text_utils_get_path_delegate;

	private static Delegates.sk_text_utils_get_pos_path sk_text_utils_get_pos_path_delegate;

	private static Delegates.sk_colortype_get_default_8888 sk_colortype_get_default_8888_delegate;

	private static Delegates.sk_nvrefcnt_get_ref_count sk_nvrefcnt_get_ref_count_delegate;

	private static Delegates.sk_nvrefcnt_safe_ref sk_nvrefcnt_safe_ref_delegate;

	private static Delegates.sk_nvrefcnt_safe_unref sk_nvrefcnt_safe_unref_delegate;

	private static Delegates.sk_nvrefcnt_unique sk_nvrefcnt_unique_delegate;

	private static Delegates.sk_refcnt_get_ref_count sk_refcnt_get_ref_count_delegate;

	private static Delegates.sk_refcnt_safe_ref sk_refcnt_safe_ref_delegate;

	private static Delegates.sk_refcnt_safe_unref sk_refcnt_safe_unref_delegate;

	private static Delegates.sk_refcnt_unique sk_refcnt_unique_delegate;

	private static Delegates.sk_version_get_increment sk_version_get_increment_delegate;

	private static Delegates.sk_version_get_milestone sk_version_get_milestone_delegate;

	private static Delegates.sk_version_get_string sk_version_get_string_delegate;

	private static Delegates.sk_graphics_dump_memory_statistics sk_graphics_dump_memory_statistics_delegate;

	private static Delegates.sk_graphics_get_font_cache_count_limit sk_graphics_get_font_cache_count_limit_delegate;

	private static Delegates.sk_graphics_get_font_cache_count_used sk_graphics_get_font_cache_count_used_delegate;

	private static Delegates.sk_graphics_get_font_cache_limit sk_graphics_get_font_cache_limit_delegate;

	private static Delegates.sk_graphics_get_font_cache_used sk_graphics_get_font_cache_used_delegate;

	private static Delegates.sk_graphics_get_resource_cache_single_allocation_byte_limit sk_graphics_get_resource_cache_single_allocation_byte_limit_delegate;

	private static Delegates.sk_graphics_get_resource_cache_total_byte_limit sk_graphics_get_resource_cache_total_byte_limit_delegate;

	private static Delegates.sk_graphics_get_resource_cache_total_bytes_used sk_graphics_get_resource_cache_total_bytes_used_delegate;

	private static Delegates.sk_graphics_init sk_graphics_init_delegate;

	private static Delegates.sk_graphics_purge_all_caches sk_graphics_purge_all_caches_delegate;

	private static Delegates.sk_graphics_purge_font_cache sk_graphics_purge_font_cache_delegate;

	private static Delegates.sk_graphics_purge_resource_cache sk_graphics_purge_resource_cache_delegate;

	private static Delegates.sk_graphics_set_font_cache_count_limit sk_graphics_set_font_cache_count_limit_delegate;

	private static Delegates.sk_graphics_set_font_cache_limit sk_graphics_set_font_cache_limit_delegate;

	private static Delegates.sk_graphics_set_resource_cache_single_allocation_byte_limit sk_graphics_set_resource_cache_single_allocation_byte_limit_delegate;

	private static Delegates.sk_graphics_set_resource_cache_total_byte_limit sk_graphics_set_resource_cache_total_byte_limit_delegate;

	private static Delegates.sk_image_get_alpha_type sk_image_get_alpha_type_delegate;

	private static Delegates.sk_image_get_color_type sk_image_get_color_type_delegate;

	private static Delegates.sk_image_get_colorspace sk_image_get_colorspace_delegate;

	private static Delegates.sk_image_get_height sk_image_get_height_delegate;

	private static Delegates.sk_image_get_unique_id sk_image_get_unique_id_delegate;

	private static Delegates.sk_image_get_width sk_image_get_width_delegate;

	private static Delegates.sk_image_is_alpha_only sk_image_is_alpha_only_delegate;

	private static Delegates.sk_image_is_lazy_generated sk_image_is_lazy_generated_delegate;

	private static Delegates.sk_image_is_texture_backed sk_image_is_texture_backed_delegate;

	private static Delegates.sk_image_is_valid sk_image_is_valid_delegate;

	private static Delegates.sk_image_make_non_texture_image sk_image_make_non_texture_image_delegate;

	private static Delegates.sk_image_make_raster_image sk_image_make_raster_image_delegate;

	private static Delegates.sk_image_make_raw_shader sk_image_make_raw_shader_delegate;

	private static Delegates.sk_image_make_shader sk_image_make_shader_delegate;

	private static Delegates.sk_image_make_subset sk_image_make_subset_delegate;

	private static Delegates.sk_image_make_subset_raster sk_image_make_subset_raster_delegate;

	private static Delegates.sk_image_make_texture_image sk_image_make_texture_image_delegate;

	private static Delegates.sk_image_make_with_filter sk_image_make_with_filter_delegate;

	private static Delegates.sk_image_make_with_filter_raster sk_image_make_with_filter_raster_delegate;

	private static Delegates.sk_image_new_from_adopted_texture sk_image_new_from_adopted_texture_delegate;

	private static Delegates.sk_image_new_from_bitmap sk_image_new_from_bitmap_delegate;

	private static Delegates.sk_image_new_from_encoded sk_image_new_from_encoded_delegate;

	private static Delegates.sk_image_new_from_picture sk_image_new_from_picture_delegate;

	private static Delegates.sk_image_new_from_texture sk_image_new_from_texture_delegate;

	private static Delegates.sk_image_new_raster sk_image_new_raster_delegate;

	private static Delegates.sk_image_new_raster_copy sk_image_new_raster_copy_delegate;

	private static Delegates.sk_image_new_raster_copy_with_pixmap sk_image_new_raster_copy_with_pixmap_delegate;

	private static Delegates.sk_image_new_raster_data sk_image_new_raster_data_delegate;

	private static Delegates.sk_image_peek_pixels sk_image_peek_pixels_delegate;

	private static Delegates.sk_image_read_pixels sk_image_read_pixels_delegate;

	private static Delegates.sk_image_read_pixels_into_pixmap sk_image_read_pixels_into_pixmap_delegate;

	private static Delegates.sk_image_ref sk_image_ref_delegate;

	private static Delegates.sk_image_ref_encoded sk_image_ref_encoded_delegate;

	private static Delegates.sk_image_scale_pixels sk_image_scale_pixels_delegate;

	private static Delegates.sk_image_unref sk_image_unref_delegate;

	private static Delegates.sk_imagefilter_new_arithmetic sk_imagefilter_new_arithmetic_delegate;

	private static Delegates.sk_imagefilter_new_blend sk_imagefilter_new_blend_delegate;

	private static Delegates.sk_imagefilter_new_blender sk_imagefilter_new_blender_delegate;

	private static Delegates.sk_imagefilter_new_blur sk_imagefilter_new_blur_delegate;

	private static Delegates.sk_imagefilter_new_color_filter sk_imagefilter_new_color_filter_delegate;

	private static Delegates.sk_imagefilter_new_compose sk_imagefilter_new_compose_delegate;

	private static Delegates.sk_imagefilter_new_dilate sk_imagefilter_new_dilate_delegate;

	private static Delegates.sk_imagefilter_new_displacement_map_effect sk_imagefilter_new_displacement_map_effect_delegate;

	private static Delegates.sk_imagefilter_new_distant_lit_diffuse sk_imagefilter_new_distant_lit_diffuse_delegate;

	private static Delegates.sk_imagefilter_new_distant_lit_specular sk_imagefilter_new_distant_lit_specular_delegate;

	private static Delegates.sk_imagefilter_new_drop_shadow sk_imagefilter_new_drop_shadow_delegate;

	private static Delegates.sk_imagefilter_new_drop_shadow_only sk_imagefilter_new_drop_shadow_only_delegate;

	private static Delegates.sk_imagefilter_new_erode sk_imagefilter_new_erode_delegate;

	private static Delegates.sk_imagefilter_new_image sk_imagefilter_new_image_delegate;

	private static Delegates.sk_imagefilter_new_image_simple sk_imagefilter_new_image_simple_delegate;

	private static Delegates.sk_imagefilter_new_magnifier sk_imagefilter_new_magnifier_delegate;

	private static Delegates.sk_imagefilter_new_matrix_convolution sk_imagefilter_new_matrix_convolution_delegate;

	private static Delegates.sk_imagefilter_new_matrix_transform sk_imagefilter_new_matrix_transform_delegate;

	private static Delegates.sk_imagefilter_new_merge sk_imagefilter_new_merge_delegate;

	private static Delegates.sk_imagefilter_new_merge_simple sk_imagefilter_new_merge_simple_delegate;

	private static Delegates.sk_imagefilter_new_offset sk_imagefilter_new_offset_delegate;

	private static Delegates.sk_imagefilter_new_picture sk_imagefilter_new_picture_delegate;

	private static Delegates.sk_imagefilter_new_picture_with_rect sk_imagefilter_new_picture_with_rect_delegate;

	private static Delegates.sk_imagefilter_new_point_lit_diffuse sk_imagefilter_new_point_lit_diffuse_delegate;

	private static Delegates.sk_imagefilter_new_point_lit_specular sk_imagefilter_new_point_lit_specular_delegate;

	private static Delegates.sk_imagefilter_new_shader sk_imagefilter_new_shader_delegate;

	private static Delegates.sk_imagefilter_new_spot_lit_diffuse sk_imagefilter_new_spot_lit_diffuse_delegate;

	private static Delegates.sk_imagefilter_new_spot_lit_specular sk_imagefilter_new_spot_lit_specular_delegate;

	private static Delegates.sk_imagefilter_new_tile sk_imagefilter_new_tile_delegate;

	private static Delegates.sk_imagefilter_unref sk_imagefilter_unref_delegate;

	private static Delegates.sk_linker_keep_alive sk_linker_keep_alive_delegate;

	private static Delegates.sk_maskfilter_new_blur sk_maskfilter_new_blur_delegate;

	private static Delegates.sk_maskfilter_new_blur_with_flags sk_maskfilter_new_blur_with_flags_delegate;

	private static Delegates.sk_maskfilter_new_clip sk_maskfilter_new_clip_delegate;

	private static Delegates.sk_maskfilter_new_gamma sk_maskfilter_new_gamma_delegate;

	private static Delegates.sk_maskfilter_new_shader sk_maskfilter_new_shader_delegate;

	private static Delegates.sk_maskfilter_new_table sk_maskfilter_new_table_delegate;

	private static Delegates.sk_maskfilter_ref sk_maskfilter_ref_delegate;

	private static Delegates.sk_maskfilter_unref sk_maskfilter_unref_delegate;

	private static Delegates.sk_matrix_concat sk_matrix_concat_delegate;

	private static Delegates.sk_matrix_map_points sk_matrix_map_points_delegate;

	private static Delegates.sk_matrix_map_radius sk_matrix_map_radius_delegate;

	private static Delegates.sk_matrix_map_rect sk_matrix_map_rect_delegate;

	private static Delegates.sk_matrix_map_vector sk_matrix_map_vector_delegate;

	private static Delegates.sk_matrix_map_vectors sk_matrix_map_vectors_delegate;

	private static Delegates.sk_matrix_map_xy sk_matrix_map_xy_delegate;

	private static Delegates.sk_matrix_post_concat sk_matrix_post_concat_delegate;

	private static Delegates.sk_matrix_pre_concat sk_matrix_pre_concat_delegate;

	private static Delegates.sk_matrix_try_invert sk_matrix_try_invert_delegate;

	private static Delegates.sk_paint_clone sk_paint_clone_delegate;

	private static Delegates.sk_paint_delete sk_paint_delete_delegate;

	private static Delegates.sk_paint_get_blender sk_paint_get_blender_delegate;

	private static Delegates.sk_paint_get_blendmode sk_paint_get_blendmode_delegate;

	private static Delegates.sk_paint_get_color sk_paint_get_color_delegate;

	private static Delegates.sk_paint_get_color4f sk_paint_get_color4f_delegate;

	private static Delegates.sk_paint_get_colorfilter sk_paint_get_colorfilter_delegate;

	private static Delegates.sk_paint_get_fill_path sk_paint_get_fill_path_delegate;

	private static Delegates.sk_paint_get_imagefilter sk_paint_get_imagefilter_delegate;

	private static Delegates.sk_paint_get_maskfilter sk_paint_get_maskfilter_delegate;

	private static Delegates.sk_paint_get_path_effect sk_paint_get_path_effect_delegate;

	private static Delegates.sk_paint_get_shader sk_paint_get_shader_delegate;

	private static Delegates.sk_paint_get_stroke_cap sk_paint_get_stroke_cap_delegate;

	private static Delegates.sk_paint_get_stroke_join sk_paint_get_stroke_join_delegate;

	private static Delegates.sk_paint_get_stroke_miter sk_paint_get_stroke_miter_delegate;

	private static Delegates.sk_paint_get_stroke_width sk_paint_get_stroke_width_delegate;

	private static Delegates.sk_paint_get_style sk_paint_get_style_delegate;

	private static Delegates.sk_paint_is_antialias sk_paint_is_antialias_delegate;

	private static Delegates.sk_paint_is_dither sk_paint_is_dither_delegate;

	private static Delegates.sk_paint_new sk_paint_new_delegate;

	private static Delegates.sk_paint_reset sk_paint_reset_delegate;

	private static Delegates.sk_paint_set_antialias sk_paint_set_antialias_delegate;

	private static Delegates.sk_paint_set_blender sk_paint_set_blender_delegate;

	private static Delegates.sk_paint_set_blendmode sk_paint_set_blendmode_delegate;

	private static Delegates.sk_paint_set_color sk_paint_set_color_delegate;

	private static Delegates.sk_paint_set_color4f sk_paint_set_color4f_delegate;

	private static Delegates.sk_paint_set_colorfilter sk_paint_set_colorfilter_delegate;

	private static Delegates.sk_paint_set_dither sk_paint_set_dither_delegate;

	private static Delegates.sk_paint_set_imagefilter sk_paint_set_imagefilter_delegate;

	private static Delegates.sk_paint_set_maskfilter sk_paint_set_maskfilter_delegate;

	private static Delegates.sk_paint_set_path_effect sk_paint_set_path_effect_delegate;

	private static Delegates.sk_paint_set_shader sk_paint_set_shader_delegate;

	private static Delegates.sk_paint_set_stroke_cap sk_paint_set_stroke_cap_delegate;

	private static Delegates.sk_paint_set_stroke_join sk_paint_set_stroke_join_delegate;

	private static Delegates.sk_paint_set_stroke_miter sk_paint_set_stroke_miter_delegate;

	private static Delegates.sk_paint_set_stroke_width sk_paint_set_stroke_width_delegate;

	private static Delegates.sk_paint_set_style sk_paint_set_style_delegate;

	private static Delegates.sk_opbuilder_add sk_opbuilder_add_delegate;

	private static Delegates.sk_opbuilder_destroy sk_opbuilder_destroy_delegate;

	private static Delegates.sk_opbuilder_new sk_opbuilder_new_delegate;

	private static Delegates.sk_opbuilder_resolve sk_opbuilder_resolve_delegate;

	private static Delegates.sk_path_add_arc sk_path_add_arc_delegate;

	private static Delegates.sk_path_add_circle sk_path_add_circle_delegate;

	private static Delegates.sk_path_add_oval sk_path_add_oval_delegate;

	private static Delegates.sk_path_add_path sk_path_add_path_delegate;

	private static Delegates.sk_path_add_path_matrix sk_path_add_path_matrix_delegate;

	private static Delegates.sk_path_add_path_offset sk_path_add_path_offset_delegate;

	private static Delegates.sk_path_add_path_reverse sk_path_add_path_reverse_delegate;

	private static Delegates.sk_path_add_poly sk_path_add_poly_delegate;

	private static Delegates.sk_path_add_rect sk_path_add_rect_delegate;

	private static Delegates.sk_path_add_rect_start sk_path_add_rect_start_delegate;

	private static Delegates.sk_path_add_rounded_rect sk_path_add_rounded_rect_delegate;

	private static Delegates.sk_path_add_rrect sk_path_add_rrect_delegate;

	private static Delegates.sk_path_add_rrect_start sk_path_add_rrect_start_delegate;

	private static Delegates.sk_path_arc_to sk_path_arc_to_delegate;

	private static Delegates.sk_path_arc_to_with_oval sk_path_arc_to_with_oval_delegate;

	private static Delegates.sk_path_arc_to_with_points sk_path_arc_to_with_points_delegate;

	private static Delegates.sk_path_clone sk_path_clone_delegate;

	private static Delegates.sk_path_close sk_path_close_delegate;

	private static Delegates.sk_path_compute_tight_bounds sk_path_compute_tight_bounds_delegate;

	private static Delegates.sk_path_conic_to sk_path_conic_to_delegate;

	private static Delegates.sk_path_contains sk_path_contains_delegate;

	private static Delegates.sk_path_convert_conic_to_quads sk_path_convert_conic_to_quads_delegate;

	private static Delegates.sk_path_count_points sk_path_count_points_delegate;

	private static Delegates.sk_path_count_verbs sk_path_count_verbs_delegate;

	private static Delegates.sk_path_create_iter sk_path_create_iter_delegate;

	private static Delegates.sk_path_create_rawiter sk_path_create_rawiter_delegate;

	private static Delegates.sk_path_cubic_to sk_path_cubic_to_delegate;

	private static Delegates.sk_path_delete sk_path_delete_delegate;

	private static Delegates.sk_path_get_bounds sk_path_get_bounds_delegate;

	private static Delegates.sk_path_get_filltype sk_path_get_filltype_delegate;

	private static Delegates.sk_path_get_last_point sk_path_get_last_point_delegate;

	private static Delegates.sk_path_get_point sk_path_get_point_delegate;

	private static Delegates.sk_path_get_points sk_path_get_points_delegate;

	private static Delegates.sk_path_get_segment_masks sk_path_get_segment_masks_delegate;

	private static Delegates.sk_path_is_convex sk_path_is_convex_delegate;

	private static Delegates.sk_path_is_line sk_path_is_line_delegate;

	private static Delegates.sk_path_is_oval sk_path_is_oval_delegate;

	private static Delegates.sk_path_is_rect sk_path_is_rect_delegate;

	private static Delegates.sk_path_is_rrect sk_path_is_rrect_delegate;

	private static Delegates.sk_path_iter_conic_weight sk_path_iter_conic_weight_delegate;

	private static Delegates.sk_path_iter_destroy sk_path_iter_destroy_delegate;

	private static Delegates.sk_path_iter_is_close_line sk_path_iter_is_close_line_delegate;

	private static Delegates.sk_path_iter_is_closed_contour sk_path_iter_is_closed_contour_delegate;

	private static Delegates.sk_path_iter_next sk_path_iter_next_delegate;

	private static Delegates.sk_path_line_to sk_path_line_to_delegate;

	private static Delegates.sk_path_move_to sk_path_move_to_delegate;

	private static Delegates.sk_path_new sk_path_new_delegate;

	private static Delegates.sk_path_parse_svg_string sk_path_parse_svg_string_delegate;

	private static Delegates.sk_path_quad_to sk_path_quad_to_delegate;

	private static Delegates.sk_path_rarc_to sk_path_rarc_to_delegate;

	private static Delegates.sk_path_rawiter_conic_weight sk_path_rawiter_conic_weight_delegate;

	private static Delegates.sk_path_rawiter_destroy sk_path_rawiter_destroy_delegate;

	private static Delegates.sk_path_rawiter_next sk_path_rawiter_next_delegate;

	private static Delegates.sk_path_rawiter_peek sk_path_rawiter_peek_delegate;

	private static Delegates.sk_path_rconic_to sk_path_rconic_to_delegate;

	private static Delegates.sk_path_rcubic_to sk_path_rcubic_to_delegate;

	private static Delegates.sk_path_reset sk_path_reset_delegate;

	private static Delegates.sk_path_rewind sk_path_rewind_delegate;

	private static Delegates.sk_path_rline_to sk_path_rline_to_delegate;

	private static Delegates.sk_path_rmove_to sk_path_rmove_to_delegate;

	private static Delegates.sk_path_rquad_to sk_path_rquad_to_delegate;

	private static Delegates.sk_path_set_filltype sk_path_set_filltype_delegate;

	private static Delegates.sk_path_to_svg_string sk_path_to_svg_string_delegate;

	private static Delegates.sk_path_transform sk_path_transform_delegate;

	private static Delegates.sk_path_transform_to_dest sk_path_transform_to_dest_delegate;

	private static Delegates.sk_pathmeasure_destroy sk_pathmeasure_destroy_delegate;

	private static Delegates.sk_pathmeasure_get_length sk_pathmeasure_get_length_delegate;

	private static Delegates.sk_pathmeasure_get_matrix sk_pathmeasure_get_matrix_delegate;

	private static Delegates.sk_pathmeasure_get_pos_tan sk_pathmeasure_get_pos_tan_delegate;

	private static Delegates.sk_pathmeasure_get_segment sk_pathmeasure_get_segment_delegate;

	private static Delegates.sk_pathmeasure_is_closed sk_pathmeasure_is_closed_delegate;

	private static Delegates.sk_pathmeasure_new sk_pathmeasure_new_delegate;

	private static Delegates.sk_pathmeasure_new_with_path sk_pathmeasure_new_with_path_delegate;

	private static Delegates.sk_pathmeasure_next_contour sk_pathmeasure_next_contour_delegate;

	private static Delegates.sk_pathmeasure_set_path sk_pathmeasure_set_path_delegate;

	private static Delegates.sk_pathop_as_winding sk_pathop_as_winding_delegate;

	private static Delegates.sk_pathop_op sk_pathop_op_delegate;

	private static Delegates.sk_pathop_simplify sk_pathop_simplify_delegate;

	private static Delegates.sk_pathop_tight_bounds sk_pathop_tight_bounds_delegate;

	private static Delegates.sk_path_effect_create_1d_path sk_path_effect_create_1d_path_delegate;

	private static Delegates.sk_path_effect_create_2d_line sk_path_effect_create_2d_line_delegate;

	private static Delegates.sk_path_effect_create_2d_path sk_path_effect_create_2d_path_delegate;

	private static Delegates.sk_path_effect_create_compose sk_path_effect_create_compose_delegate;

	private static Delegates.sk_path_effect_create_corner sk_path_effect_create_corner_delegate;

	private static Delegates.sk_path_effect_create_dash sk_path_effect_create_dash_delegate;

	private static Delegates.sk_path_effect_create_discrete sk_path_effect_create_discrete_delegate;

	private static Delegates.sk_path_effect_create_sum sk_path_effect_create_sum_delegate;

	private static Delegates.sk_path_effect_create_trim sk_path_effect_create_trim_delegate;

	private static Delegates.sk_path_effect_unref sk_path_effect_unref_delegate;

	private static Delegates.sk_picture_approximate_bytes_used sk_picture_approximate_bytes_used_delegate;

	private static Delegates.sk_picture_approximate_op_count sk_picture_approximate_op_count_delegate;

	private static Delegates.sk_picture_deserialize_from_data sk_picture_deserialize_from_data_delegate;

	private static Delegates.sk_picture_deserialize_from_memory sk_picture_deserialize_from_memory_delegate;

	private static Delegates.sk_picture_deserialize_from_stream sk_picture_deserialize_from_stream_delegate;

	private static Delegates.sk_picture_get_cull_rect sk_picture_get_cull_rect_delegate;

	private static Delegates.sk_picture_get_recording_canvas sk_picture_get_recording_canvas_delegate;

	private static Delegates.sk_picture_get_unique_id sk_picture_get_unique_id_delegate;

	private static Delegates.sk_picture_make_shader sk_picture_make_shader_delegate;

	private static Delegates.sk_picture_playback sk_picture_playback_delegate;

	private static Delegates.sk_picture_recorder_begin_recording sk_picture_recorder_begin_recording_delegate;

	private static Delegates.sk_picture_recorder_begin_recording_with_bbh_factory sk_picture_recorder_begin_recording_with_bbh_factory_delegate;

	private static Delegates.sk_picture_recorder_delete sk_picture_recorder_delete_delegate;

	private static Delegates.sk_picture_recorder_end_recording sk_picture_recorder_end_recording_delegate;

	private static Delegates.sk_picture_recorder_end_recording_as_drawable sk_picture_recorder_end_recording_as_drawable_delegate;

	private static Delegates.sk_picture_recorder_new sk_picture_recorder_new_delegate;

	private static Delegates.sk_picture_ref sk_picture_ref_delegate;

	private static Delegates.sk_picture_serialize_to_data sk_picture_serialize_to_data_delegate;

	private static Delegates.sk_picture_serialize_to_stream sk_picture_serialize_to_stream_delegate;

	private static Delegates.sk_picture_unref sk_picture_unref_delegate;

	private static Delegates.sk_rtree_factory_delete sk_rtree_factory_delete_delegate;

	private static Delegates.sk_rtree_factory_new sk_rtree_factory_new_delegate;

	private static Delegates.sk_color_get_bit_shift sk_color_get_bit_shift_delegate;

	private static Delegates.sk_color_premultiply sk_color_premultiply_delegate;

	private static Delegates.sk_color_premultiply_array sk_color_premultiply_array_delegate;

	private static Delegates.sk_color_unpremultiply sk_color_unpremultiply_delegate;

	private static Delegates.sk_color_unpremultiply_array sk_color_unpremultiply_array_delegate;

	private static Delegates.sk_jpegencoder_encode sk_jpegencoder_encode_delegate;

	private static Delegates.sk_pixmap_compute_is_opaque sk_pixmap_compute_is_opaque_delegate;

	private static Delegates.sk_pixmap_destructor sk_pixmap_destructor_delegate;

	private static Delegates.sk_pixmap_erase_color sk_pixmap_erase_color_delegate;

	private static Delegates.sk_pixmap_erase_color4f sk_pixmap_erase_color4f_delegate;

	private static Delegates.sk_pixmap_extract_subset sk_pixmap_extract_subset_delegate;

	private static Delegates.sk_pixmap_get_colorspace sk_pixmap_get_colorspace_delegate;

	private static Delegates.sk_pixmap_get_info sk_pixmap_get_info_delegate;

	private static Delegates.sk_pixmap_get_pixel_alphaf sk_pixmap_get_pixel_alphaf_delegate;

	private static Delegates.sk_pixmap_get_pixel_color sk_pixmap_get_pixel_color_delegate;

	private static Delegates.sk_pixmap_get_pixel_color4f sk_pixmap_get_pixel_color4f_delegate;

	private static Delegates.sk_pixmap_get_row_bytes sk_pixmap_get_row_bytes_delegate;

	private static Delegates.sk_pixmap_get_writable_addr sk_pixmap_get_writable_addr_delegate;

	private static Delegates.sk_pixmap_get_writeable_addr_with_xy sk_pixmap_get_writeable_addr_with_xy_delegate;

	private static Delegates.sk_pixmap_new sk_pixmap_new_delegate;

	private static Delegates.sk_pixmap_new_with_params sk_pixmap_new_with_params_delegate;

	private static Delegates.sk_pixmap_read_pixels sk_pixmap_read_pixels_delegate;

	private static Delegates.sk_pixmap_reset sk_pixmap_reset_delegate;

	private static Delegates.sk_pixmap_reset_with_params sk_pixmap_reset_with_params_delegate;

	private static Delegates.sk_pixmap_scale_pixels sk_pixmap_scale_pixels_delegate;

	private static Delegates.sk_pixmap_set_colorspace sk_pixmap_set_colorspace_delegate;

	private static Delegates.sk_pngencoder_encode sk_pngencoder_encode_delegate;

	private static Delegates.sk_swizzle_swap_rb sk_swizzle_swap_rb_delegate;

	private static Delegates.sk_webpencoder_encode sk_webpencoder_encode_delegate;

	private static Delegates.sk_region_cliperator_delete sk_region_cliperator_delete_delegate;

	private static Delegates.sk_region_cliperator_done sk_region_cliperator_done_delegate;

	private static Delegates.sk_region_cliperator_new sk_region_cliperator_new_delegate;

	private static Delegates.sk_region_cliperator_next sk_region_cliperator_next_delegate;

	private static Delegates.sk_region_cliperator_rect sk_region_cliperator_rect_delegate;

	private static Delegates.sk_region_contains sk_region_contains_delegate;

	private static Delegates.sk_region_contains_point sk_region_contains_point_delegate;

	private static Delegates.sk_region_contains_rect sk_region_contains_rect_delegate;

	private static Delegates.sk_region_delete sk_region_delete_delegate;

	private static Delegates.sk_region_get_boundary_path sk_region_get_boundary_path_delegate;

	private static Delegates.sk_region_get_bounds sk_region_get_bounds_delegate;

	private static Delegates.sk_region_intersects sk_region_intersects_delegate;

	private static Delegates.sk_region_intersects_rect sk_region_intersects_rect_delegate;

	private static Delegates.sk_region_is_complex sk_region_is_complex_delegate;

	private static Delegates.sk_region_is_empty sk_region_is_empty_delegate;

	private static Delegates.sk_region_is_rect sk_region_is_rect_delegate;

	private static Delegates.sk_region_iterator_delete sk_region_iterator_delete_delegate;

	private static Delegates.sk_region_iterator_done sk_region_iterator_done_delegate;

	private static Delegates.sk_region_iterator_new sk_region_iterator_new_delegate;

	private static Delegates.sk_region_iterator_next sk_region_iterator_next_delegate;

	private static Delegates.sk_region_iterator_rect sk_region_iterator_rect_delegate;

	private static Delegates.sk_region_iterator_rewind sk_region_iterator_rewind_delegate;

	private static Delegates.sk_region_new sk_region_new_delegate;

	private static Delegates.sk_region_op sk_region_op_delegate;

	private static Delegates.sk_region_op_rect sk_region_op_rect_delegate;

	private static Delegates.sk_region_quick_contains sk_region_quick_contains_delegate;

	private static Delegates.sk_region_quick_reject sk_region_quick_reject_delegate;

	private static Delegates.sk_region_quick_reject_rect sk_region_quick_reject_rect_delegate;

	private static Delegates.sk_region_set_empty sk_region_set_empty_delegate;

	private static Delegates.sk_region_set_path sk_region_set_path_delegate;

	private static Delegates.sk_region_set_rect sk_region_set_rect_delegate;

	private static Delegates.sk_region_set_rects sk_region_set_rects_delegate;

	private static Delegates.sk_region_set_region sk_region_set_region_delegate;

	private static Delegates.sk_region_spanerator_delete sk_region_spanerator_delete_delegate;

	private static Delegates.sk_region_spanerator_new sk_region_spanerator_new_delegate;

	private static Delegates.sk_region_spanerator_next sk_region_spanerator_next_delegate;

	private static Delegates.sk_region_translate sk_region_translate_delegate;

	private static Delegates.sk_rrect_contains sk_rrect_contains_delegate;

	private static Delegates.sk_rrect_delete sk_rrect_delete_delegate;

	private static Delegates.sk_rrect_get_height sk_rrect_get_height_delegate;

	private static Delegates.sk_rrect_get_radii sk_rrect_get_radii_delegate;

	private static Delegates.sk_rrect_get_rect sk_rrect_get_rect_delegate;

	private static Delegates.sk_rrect_get_type sk_rrect_get_type_delegate;

	private static Delegates.sk_rrect_get_width sk_rrect_get_width_delegate;

	private static Delegates.sk_rrect_inset sk_rrect_inset_delegate;

	private static Delegates.sk_rrect_is_valid sk_rrect_is_valid_delegate;

	private static Delegates.sk_rrect_new sk_rrect_new_delegate;

	private static Delegates.sk_rrect_new_copy sk_rrect_new_copy_delegate;

	private static Delegates.sk_rrect_offset sk_rrect_offset_delegate;

	private static Delegates.sk_rrect_outset sk_rrect_outset_delegate;

	private static Delegates.sk_rrect_set_empty sk_rrect_set_empty_delegate;

	private static Delegates.sk_rrect_set_nine_patch sk_rrect_set_nine_patch_delegate;

	private static Delegates.sk_rrect_set_oval sk_rrect_set_oval_delegate;

	private static Delegates.sk_rrect_set_rect sk_rrect_set_rect_delegate;

	private static Delegates.sk_rrect_set_rect_radii sk_rrect_set_rect_radii_delegate;

	private static Delegates.sk_rrect_set_rect_xy sk_rrect_set_rect_xy_delegate;

	private static Delegates.sk_rrect_transform sk_rrect_transform_delegate;

	private static Delegates.sk_runtimeeffect_get_child_from_index sk_runtimeeffect_get_child_from_index_delegate;

	private static Delegates.sk_runtimeeffect_get_child_from_name sk_runtimeeffect_get_child_from_name_delegate;

	private static Delegates.sk_runtimeeffect_get_child_name sk_runtimeeffect_get_child_name_delegate;

	private static Delegates.sk_runtimeeffect_get_children_size sk_runtimeeffect_get_children_size_delegate;

	private static Delegates.sk_runtimeeffect_get_uniform_byte_size sk_runtimeeffect_get_uniform_byte_size_delegate;

	private static Delegates.sk_runtimeeffect_get_uniform_from_index sk_runtimeeffect_get_uniform_from_index_delegate;

	private static Delegates.sk_runtimeeffect_get_uniform_from_name sk_runtimeeffect_get_uniform_from_name_delegate;

	private static Delegates.sk_runtimeeffect_get_uniform_name sk_runtimeeffect_get_uniform_name_delegate;

	private static Delegates.sk_runtimeeffect_get_uniforms_size sk_runtimeeffect_get_uniforms_size_delegate;

	private static Delegates.sk_runtimeeffect_make_blender sk_runtimeeffect_make_blender_delegate;

	private static Delegates.sk_runtimeeffect_make_color_filter sk_runtimeeffect_make_color_filter_delegate;

	private static Delegates.sk_runtimeeffect_make_for_blender sk_runtimeeffect_make_for_blender_delegate;

	private static Delegates.sk_runtimeeffect_make_for_color_filter sk_runtimeeffect_make_for_color_filter_delegate;

	private static Delegates.sk_runtimeeffect_make_for_shader sk_runtimeeffect_make_for_shader_delegate;

	private static Delegates.sk_runtimeeffect_make_shader sk_runtimeeffect_make_shader_delegate;

	private static Delegates.sk_runtimeeffect_unref sk_runtimeeffect_unref_delegate;

	private static Delegates.sk_shader_new_blend sk_shader_new_blend_delegate;

	private static Delegates.sk_shader_new_blender sk_shader_new_blender_delegate;

	private static Delegates.sk_shader_new_color sk_shader_new_color_delegate;

	private static Delegates.sk_shader_new_color4f sk_shader_new_color4f_delegate;

	private static Delegates.sk_shader_new_empty sk_shader_new_empty_delegate;

	private static Delegates.sk_shader_new_linear_gradient sk_shader_new_linear_gradient_delegate;

	private static Delegates.sk_shader_new_linear_gradient_color4f sk_shader_new_linear_gradient_color4f_delegate;

	private static Delegates.sk_shader_new_perlin_noise_fractal_noise sk_shader_new_perlin_noise_fractal_noise_delegate;

	private static Delegates.sk_shader_new_perlin_noise_turbulence sk_shader_new_perlin_noise_turbulence_delegate;

	private static Delegates.sk_shader_new_radial_gradient sk_shader_new_radial_gradient_delegate;

	private static Delegates.sk_shader_new_radial_gradient_color4f sk_shader_new_radial_gradient_color4f_delegate;

	private static Delegates.sk_shader_new_sweep_gradient sk_shader_new_sweep_gradient_delegate;

	private static Delegates.sk_shader_new_sweep_gradient_color4f sk_shader_new_sweep_gradient_color4f_delegate;

	private static Delegates.sk_shader_new_two_point_conical_gradient sk_shader_new_two_point_conical_gradient_delegate;

	private static Delegates.sk_shader_new_two_point_conical_gradient_color4f sk_shader_new_two_point_conical_gradient_color4f_delegate;

	private static Delegates.sk_shader_ref sk_shader_ref_delegate;

	private static Delegates.sk_shader_unref sk_shader_unref_delegate;

	private static Delegates.sk_shader_with_color_filter sk_shader_with_color_filter_delegate;

	private static Delegates.sk_shader_with_local_matrix sk_shader_with_local_matrix_delegate;

	private static Delegates.sk_dynamicmemorywstream_copy_to sk_dynamicmemorywstream_copy_to_delegate;

	private static Delegates.sk_dynamicmemorywstream_destroy sk_dynamicmemorywstream_destroy_delegate;

	private static Delegates.sk_dynamicmemorywstream_detach_as_data sk_dynamicmemorywstream_detach_as_data_delegate;

	private static Delegates.sk_dynamicmemorywstream_detach_as_stream sk_dynamicmemorywstream_detach_as_stream_delegate;

	private static Delegates.sk_dynamicmemorywstream_new sk_dynamicmemorywstream_new_delegate;

	private static Delegates.sk_dynamicmemorywstream_write_to_stream sk_dynamicmemorywstream_write_to_stream_delegate;

	private static Delegates.sk_filestream_destroy sk_filestream_destroy_delegate;

	private static Delegates.sk_filestream_is_valid sk_filestream_is_valid_delegate;

	private static Delegates.sk_filestream_new sk_filestream_new_delegate;

	private static Delegates.sk_filewstream_destroy sk_filewstream_destroy_delegate;

	private static Delegates.sk_filewstream_is_valid sk_filewstream_is_valid_delegate;

	private static Delegates.sk_filewstream_new sk_filewstream_new_delegate;

	private static Delegates.sk_memorystream_destroy sk_memorystream_destroy_delegate;

	private static Delegates.sk_memorystream_new sk_memorystream_new_delegate;

	private static Delegates.sk_memorystream_new_with_data sk_memorystream_new_with_data_delegate;

	private static Delegates.sk_memorystream_new_with_length sk_memorystream_new_with_length_delegate;

	private static Delegates.sk_memorystream_new_with_skdata sk_memorystream_new_with_skdata_delegate;

	private static Delegates.sk_memorystream_set_memory sk_memorystream_set_memory_delegate;

	private static Delegates.sk_stream_asset_destroy sk_stream_asset_destroy_delegate;

	private static Delegates.sk_stream_destroy sk_stream_destroy_delegate;

	private static Delegates.sk_stream_duplicate sk_stream_duplicate_delegate;

	private static Delegates.sk_stream_fork sk_stream_fork_delegate;

	private static Delegates.sk_stream_get_length sk_stream_get_length_delegate;

	private static Delegates.sk_stream_get_memory_base sk_stream_get_memory_base_delegate;

	private static Delegates.sk_stream_get_position sk_stream_get_position_delegate;

	private static Delegates.sk_stream_has_length sk_stream_has_length_delegate;

	private static Delegates.sk_stream_has_position sk_stream_has_position_delegate;

	private static Delegates.sk_stream_is_at_end sk_stream_is_at_end_delegate;

	private static Delegates.sk_stream_move sk_stream_move_delegate;

	private static Delegates.sk_stream_peek sk_stream_peek_delegate;

	private static Delegates.sk_stream_read sk_stream_read_delegate;

	private static Delegates.sk_stream_read_bool sk_stream_read_bool_delegate;

	private static Delegates.sk_stream_read_s16 sk_stream_read_s16_delegate;

	private static Delegates.sk_stream_read_s32 sk_stream_read_s32_delegate;

	private static Delegates.sk_stream_read_s8 sk_stream_read_s8_delegate;

	private static Delegates.sk_stream_read_u16 sk_stream_read_u16_delegate;

	private static Delegates.sk_stream_read_u32 sk_stream_read_u32_delegate;

	private static Delegates.sk_stream_read_u8 sk_stream_read_u8_delegate;

	private static Delegates.sk_stream_rewind sk_stream_rewind_delegate;

	private static Delegates.sk_stream_seek sk_stream_seek_delegate;

	private static Delegates.sk_stream_skip sk_stream_skip_delegate;

	private static Delegates.sk_wstream_bytes_written sk_wstream_bytes_written_delegate;

	private static Delegates.sk_wstream_flush sk_wstream_flush_delegate;

	private static Delegates.sk_wstream_get_size_of_packed_uint sk_wstream_get_size_of_packed_uint_delegate;

	private static Delegates.sk_wstream_newline sk_wstream_newline_delegate;

	private static Delegates.sk_wstream_write sk_wstream_write_delegate;

	private static Delegates.sk_wstream_write_16 sk_wstream_write_16_delegate;

	private static Delegates.sk_wstream_write_32 sk_wstream_write_32_delegate;

	private static Delegates.sk_wstream_write_8 sk_wstream_write_8_delegate;

	private static Delegates.sk_wstream_write_bigdec_as_text sk_wstream_write_bigdec_as_text_delegate;

	private static Delegates.sk_wstream_write_bool sk_wstream_write_bool_delegate;

	private static Delegates.sk_wstream_write_dec_as_text sk_wstream_write_dec_as_text_delegate;

	private static Delegates.sk_wstream_write_hex_as_text sk_wstream_write_hex_as_text_delegate;

	private static Delegates.sk_wstream_write_packed_uint sk_wstream_write_packed_uint_delegate;

	private static Delegates.sk_wstream_write_scalar sk_wstream_write_scalar_delegate;

	private static Delegates.sk_wstream_write_scalar_as_text sk_wstream_write_scalar_as_text_delegate;

	private static Delegates.sk_wstream_write_stream sk_wstream_write_stream_delegate;

	private static Delegates.sk_wstream_write_text sk_wstream_write_text_delegate;

	private static Delegates.sk_string_destructor sk_string_destructor_delegate;

	private static Delegates.sk_string_get_c_str sk_string_get_c_str_delegate;

	private static Delegates.sk_string_get_size sk_string_get_size_delegate;

	private static Delegates.sk_string_new_empty sk_string_new_empty_delegate;

	private static Delegates.sk_string_new_with_copy sk_string_new_with_copy_delegate;

	private static Delegates.sk_surface_draw sk_surface_draw_delegate;

	private static Delegates.sk_surface_get_canvas sk_surface_get_canvas_delegate;

	private static Delegates.sk_surface_get_props sk_surface_get_props_delegate;

	private static Delegates.sk_surface_get_recording_context sk_surface_get_recording_context_delegate;

	private static Delegates.sk_surface_new_backend_render_target sk_surface_new_backend_render_target_delegate;

	private static Delegates.sk_surface_new_backend_texture sk_surface_new_backend_texture_delegate;

	private static Delegates.sk_surface_new_image_snapshot sk_surface_new_image_snapshot_delegate;

	private static Delegates.sk_surface_new_image_snapshot_with_crop sk_surface_new_image_snapshot_with_crop_delegate;

	private static Delegates.sk_surface_new_metal_layer sk_surface_new_metal_layer_delegate;

	private static Delegates.sk_surface_new_metal_view sk_surface_new_metal_view_delegate;

	private static Delegates.sk_surface_new_null sk_surface_new_null_delegate;

	private static Delegates.sk_surface_new_raster sk_surface_new_raster_delegate;

	private static Delegates.sk_surface_new_raster_direct sk_surface_new_raster_direct_delegate;

	private static Delegates.sk_surface_new_render_target sk_surface_new_render_target_delegate;

	private static Delegates.sk_surface_peek_pixels sk_surface_peek_pixels_delegate;

	private static Delegates.sk_surface_read_pixels sk_surface_read_pixels_delegate;

	private static Delegates.sk_surface_unref sk_surface_unref_delegate;

	private static Delegates.sk_surfaceprops_delete sk_surfaceprops_delete_delegate;

	private static Delegates.sk_surfaceprops_get_flags sk_surfaceprops_get_flags_delegate;

	private static Delegates.sk_surfaceprops_get_pixel_geometry sk_surfaceprops_get_pixel_geometry_delegate;

	private static Delegates.sk_surfaceprops_new sk_surfaceprops_new_delegate;

	private static Delegates.sk_svgcanvas_create_with_stream sk_svgcanvas_create_with_stream_delegate;

	private static Delegates.sk_textblob_builder_alloc_run sk_textblob_builder_alloc_run_delegate;

	private static Delegates.sk_textblob_builder_alloc_run_pos sk_textblob_builder_alloc_run_pos_delegate;

	private static Delegates.sk_textblob_builder_alloc_run_pos_h sk_textblob_builder_alloc_run_pos_h_delegate;

	private static Delegates.sk_textblob_builder_alloc_run_rsxform sk_textblob_builder_alloc_run_rsxform_delegate;

	private static Delegates.sk_textblob_builder_alloc_run_text sk_textblob_builder_alloc_run_text_delegate;

	private static Delegates.sk_textblob_builder_alloc_run_text_pos sk_textblob_builder_alloc_run_text_pos_delegate;

	private static Delegates.sk_textblob_builder_alloc_run_text_pos_h sk_textblob_builder_alloc_run_text_pos_h_delegate;

	private static Delegates.sk_textblob_builder_alloc_run_text_rsxform sk_textblob_builder_alloc_run_text_rsxform_delegate;

	private static Delegates.sk_textblob_builder_delete sk_textblob_builder_delete_delegate;

	private static Delegates.sk_textblob_builder_make sk_textblob_builder_make_delegate;

	private static Delegates.sk_textblob_builder_new sk_textblob_builder_new_delegate;

	private static Delegates.sk_textblob_get_bounds sk_textblob_get_bounds_delegate;

	private static Delegates.sk_textblob_get_intercepts sk_textblob_get_intercepts_delegate;

	private static Delegates.sk_textblob_get_unique_id sk_textblob_get_unique_id_delegate;

	private static Delegates.sk_textblob_ref sk_textblob_ref_delegate;

	private static Delegates.sk_textblob_unref sk_textblob_unref_delegate;

	private static Delegates.sk_fontmgr_count_families sk_fontmgr_count_families_delegate;

	private static Delegates.sk_fontmgr_create_default sk_fontmgr_create_default_delegate;

	private static Delegates.sk_fontmgr_create_from_data sk_fontmgr_create_from_data_delegate;

	private static Delegates.sk_fontmgr_create_from_file sk_fontmgr_create_from_file_delegate;

	private static Delegates.sk_fontmgr_create_from_stream sk_fontmgr_create_from_stream_delegate;

	private static Delegates.sk_fontmgr_create_styleset sk_fontmgr_create_styleset_delegate;

	private static Delegates.sk_fontmgr_get_family_name sk_fontmgr_get_family_name_delegate;

	private static Delegates.sk_fontmgr_match_family sk_fontmgr_match_family_delegate;

	private static Delegates.sk_fontmgr_match_family_style sk_fontmgr_match_family_style_delegate;

	private static Delegates.sk_fontmgr_match_family_style_character sk_fontmgr_match_family_style_character_delegate;

	private static Delegates.sk_fontmgr_ref_default sk_fontmgr_ref_default_delegate;

	private static Delegates.sk_fontmgr_unref sk_fontmgr_unref_delegate;

	private static Delegates.sk_fontstyle_delete sk_fontstyle_delete_delegate;

	private static Delegates.sk_fontstyle_get_slant sk_fontstyle_get_slant_delegate;

	private static Delegates.sk_fontstyle_get_weight sk_fontstyle_get_weight_delegate;

	private static Delegates.sk_fontstyle_get_width sk_fontstyle_get_width_delegate;

	private static Delegates.sk_fontstyle_new sk_fontstyle_new_delegate;

	private static Delegates.sk_fontstyleset_create_empty sk_fontstyleset_create_empty_delegate;

	private static Delegates.sk_fontstyleset_create_typeface sk_fontstyleset_create_typeface_delegate;

	private static Delegates.sk_fontstyleset_get_count sk_fontstyleset_get_count_delegate;

	private static Delegates.sk_fontstyleset_get_style sk_fontstyleset_get_style_delegate;

	private static Delegates.sk_fontstyleset_match_style sk_fontstyleset_match_style_delegate;

	private static Delegates.sk_fontstyleset_unref sk_fontstyleset_unref_delegate;

	private static Delegates.sk_typeface_copy_table_data sk_typeface_copy_table_data_delegate;

	private static Delegates.sk_typeface_count_glyphs sk_typeface_count_glyphs_delegate;

	private static Delegates.sk_typeface_count_tables sk_typeface_count_tables_delegate;

	private static Delegates.sk_typeface_create_default sk_typeface_create_default_delegate;

	private static Delegates.sk_typeface_create_from_data sk_typeface_create_from_data_delegate;

	private static Delegates.sk_typeface_create_from_file sk_typeface_create_from_file_delegate;

	private static Delegates.sk_typeface_create_from_name sk_typeface_create_from_name_delegate;

	private static Delegates.sk_typeface_create_from_stream sk_typeface_create_from_stream_delegate;

	private static Delegates.sk_typeface_get_family_name sk_typeface_get_family_name_delegate;

	private static Delegates.sk_typeface_get_font_slant sk_typeface_get_font_slant_delegate;

	private static Delegates.sk_typeface_get_font_weight sk_typeface_get_font_weight_delegate;

	private static Delegates.sk_typeface_get_font_width sk_typeface_get_font_width_delegate;

	private static Delegates.sk_typeface_get_fontstyle sk_typeface_get_fontstyle_delegate;

	private static Delegates.sk_typeface_get_kerning_pair_adjustments sk_typeface_get_kerning_pair_adjustments_delegate;

	private static Delegates.sk_typeface_get_post_script_name sk_typeface_get_post_script_name_delegate;

	private static Delegates.sk_typeface_get_table_data sk_typeface_get_table_data_delegate;

	private static Delegates.sk_typeface_get_table_size sk_typeface_get_table_size_delegate;

	private static Delegates.sk_typeface_get_table_tags sk_typeface_get_table_tags_delegate;

	private static Delegates.sk_typeface_get_units_per_em sk_typeface_get_units_per_em_delegate;

	private static Delegates.sk_typeface_is_fixed_pitch sk_typeface_is_fixed_pitch_delegate;

	private static Delegates.sk_typeface_open_stream sk_typeface_open_stream_delegate;

	private static Delegates.sk_typeface_ref_default sk_typeface_ref_default_delegate;

	private static Delegates.sk_typeface_unichar_to_glyph sk_typeface_unichar_to_glyph_delegate;

	private static Delegates.sk_typeface_unichars_to_glyphs sk_typeface_unichars_to_glyphs_delegate;

	private static Delegates.sk_typeface_unref sk_typeface_unref_delegate;

	private static Delegates.sk_vertices_make_copy sk_vertices_make_copy_delegate;

	private static Delegates.sk_vertices_ref sk_vertices_ref_delegate;

	private static Delegates.sk_vertices_unref sk_vertices_unref_delegate;

	private static Delegates.sk_compatpaint_clone sk_compatpaint_clone_delegate;

	private static Delegates.sk_compatpaint_delete sk_compatpaint_delete_delegate;

	private static Delegates.sk_compatpaint_get_filter_quality sk_compatpaint_get_filter_quality_delegate;

	private static Delegates.sk_compatpaint_get_font sk_compatpaint_get_font_delegate;

	private static Delegates.sk_compatpaint_get_lcd_render_text sk_compatpaint_get_lcd_render_text_delegate;

	private static Delegates.sk_compatpaint_get_text_align sk_compatpaint_get_text_align_delegate;

	private static Delegates.sk_compatpaint_get_text_encoding sk_compatpaint_get_text_encoding_delegate;

	private static Delegates.sk_compatpaint_make_font sk_compatpaint_make_font_delegate;

	private static Delegates.sk_compatpaint_new sk_compatpaint_new_delegate;

	private static Delegates.sk_compatpaint_new_with_font sk_compatpaint_new_with_font_delegate;

	private static Delegates.sk_compatpaint_reset sk_compatpaint_reset_delegate;

	private static Delegates.sk_compatpaint_set_filter_quality sk_compatpaint_set_filter_quality_delegate;

	private static Delegates.sk_compatpaint_set_is_antialias sk_compatpaint_set_is_antialias_delegate;

	private static Delegates.sk_compatpaint_set_lcd_render_text sk_compatpaint_set_lcd_render_text_delegate;

	private static Delegates.sk_compatpaint_set_text_align sk_compatpaint_set_text_align_delegate;

	private static Delegates.sk_compatpaint_set_text_encoding sk_compatpaint_set_text_encoding_delegate;

	private static Delegates.sk_manageddrawable_new sk_manageddrawable_new_delegate;

	private static Delegates.sk_manageddrawable_set_procs sk_manageddrawable_set_procs_delegate;

	private static Delegates.sk_manageddrawable_unref sk_manageddrawable_unref_delegate;

	private static Delegates.sk_managedstream_destroy sk_managedstream_destroy_delegate;

	private static Delegates.sk_managedstream_new sk_managedstream_new_delegate;

	private static Delegates.sk_managedstream_set_procs sk_managedstream_set_procs_delegate;

	private static Delegates.sk_managedwstream_destroy sk_managedwstream_destroy_delegate;

	private static Delegates.sk_managedwstream_new sk_managedwstream_new_delegate;

	private static Delegates.sk_managedwstream_set_procs sk_managedwstream_set_procs_delegate;

	private static Delegates.sk_managedtracememorydump_delete sk_managedtracememorydump_delete_delegate;

	private static Delegates.sk_managedtracememorydump_new sk_managedtracememorydump_new_delegate;

	private static Delegates.sk_managedtracememorydump_set_procs sk_managedtracememorydump_set_procs_delegate;

	private static T GetSymbol<T>(string name) where T : Delegate
	{
		return LibraryLoader.GetSymbolDelegate<T>(libSkiaSharpHandle.Value, name);
	}

	internal static void gr_backendrendertarget_delete(IntPtr rendertarget)
	{
		(gr_backendrendertarget_delete_delegate ?? (gr_backendrendertarget_delete_delegate = GetSymbol<Delegates.gr_backendrendertarget_delete>("gr_backendrendertarget_delete")))(rendertarget);
	}

	internal static GRBackendNative gr_backendrendertarget_get_backend(IntPtr rendertarget)
	{
		return (gr_backendrendertarget_get_backend_delegate ?? (gr_backendrendertarget_get_backend_delegate = GetSymbol<Delegates.gr_backendrendertarget_get_backend>("gr_backendrendertarget_get_backend")))(rendertarget);
	}

	internal unsafe static bool gr_backendrendertarget_get_gl_framebufferinfo(IntPtr rendertarget, GRGlFramebufferInfo* glInfo)
	{
		return (gr_backendrendertarget_get_gl_framebufferinfo_delegate ?? (gr_backendrendertarget_get_gl_framebufferinfo_delegate = GetSymbol<Delegates.gr_backendrendertarget_get_gl_framebufferinfo>("gr_backendrendertarget_get_gl_framebufferinfo")))(rendertarget, glInfo);
	}

	internal static int gr_backendrendertarget_get_height(IntPtr rendertarget)
	{
		return (gr_backendrendertarget_get_height_delegate ?? (gr_backendrendertarget_get_height_delegate = GetSymbol<Delegates.gr_backendrendertarget_get_height>("gr_backendrendertarget_get_height")))(rendertarget);
	}

	internal static int gr_backendrendertarget_get_samples(IntPtr rendertarget)
	{
		return (gr_backendrendertarget_get_samples_delegate ?? (gr_backendrendertarget_get_samples_delegate = GetSymbol<Delegates.gr_backendrendertarget_get_samples>("gr_backendrendertarget_get_samples")))(rendertarget);
	}

	internal static int gr_backendrendertarget_get_stencils(IntPtr rendertarget)
	{
		return (gr_backendrendertarget_get_stencils_delegate ?? (gr_backendrendertarget_get_stencils_delegate = GetSymbol<Delegates.gr_backendrendertarget_get_stencils>("gr_backendrendertarget_get_stencils")))(rendertarget);
	}

	internal static int gr_backendrendertarget_get_width(IntPtr rendertarget)
	{
		return (gr_backendrendertarget_get_width_delegate ?? (gr_backendrendertarget_get_width_delegate = GetSymbol<Delegates.gr_backendrendertarget_get_width>("gr_backendrendertarget_get_width")))(rendertarget);
	}

	internal static bool gr_backendrendertarget_is_valid(IntPtr rendertarget)
	{
		return (gr_backendrendertarget_is_valid_delegate ?? (gr_backendrendertarget_is_valid_delegate = GetSymbol<Delegates.gr_backendrendertarget_is_valid>("gr_backendrendertarget_is_valid")))(rendertarget);
	}

	internal unsafe static IntPtr gr_backendrendertarget_new_direct3d(int width, int height, GRD3DTextureResourceInfoNative* d3dInfo)
	{
		return (gr_backendrendertarget_new_direct3d_delegate ?? (gr_backendrendertarget_new_direct3d_delegate = GetSymbol<Delegates.gr_backendrendertarget_new_direct3d>("gr_backendrendertarget_new_direct3d")))(width, height, d3dInfo);
	}

	internal unsafe static IntPtr gr_backendrendertarget_new_gl(int width, int height, int samples, int stencils, GRGlFramebufferInfo* glInfo)
	{
		return (gr_backendrendertarget_new_gl_delegate ?? (gr_backendrendertarget_new_gl_delegate = GetSymbol<Delegates.gr_backendrendertarget_new_gl>("gr_backendrendertarget_new_gl")))(width, height, samples, stencils, glInfo);
	}

	internal unsafe static IntPtr gr_backendrendertarget_new_metal(int width, int height, GRMtlTextureInfoNative* mtlInfo)
	{
		return (gr_backendrendertarget_new_metal_delegate ?? (gr_backendrendertarget_new_metal_delegate = GetSymbol<Delegates.gr_backendrendertarget_new_metal>("gr_backendrendertarget_new_metal")))(width, height, mtlInfo);
	}

	internal unsafe static IntPtr gr_backendrendertarget_new_vulkan(int width, int height, GRVkImageInfo* vkImageInfo)
	{
		return (gr_backendrendertarget_new_vulkan_delegate ?? (gr_backendrendertarget_new_vulkan_delegate = GetSymbol<Delegates.gr_backendrendertarget_new_vulkan>("gr_backendrendertarget_new_vulkan")))(width, height, vkImageInfo);
	}

	internal static void gr_backendtexture_delete(IntPtr texture)
	{
		(gr_backendtexture_delete_delegate ?? (gr_backendtexture_delete_delegate = GetSymbol<Delegates.gr_backendtexture_delete>("gr_backendtexture_delete")))(texture);
	}

	internal static GRBackendNative gr_backendtexture_get_backend(IntPtr texture)
	{
		return (gr_backendtexture_get_backend_delegate ?? (gr_backendtexture_get_backend_delegate = GetSymbol<Delegates.gr_backendtexture_get_backend>("gr_backendtexture_get_backend")))(texture);
	}

	internal unsafe static bool gr_backendtexture_get_gl_textureinfo(IntPtr texture, GRGlTextureInfo* glInfo)
	{
		return (gr_backendtexture_get_gl_textureinfo_delegate ?? (gr_backendtexture_get_gl_textureinfo_delegate = GetSymbol<Delegates.gr_backendtexture_get_gl_textureinfo>("gr_backendtexture_get_gl_textureinfo")))(texture, glInfo);
	}

	internal static int gr_backendtexture_get_height(IntPtr texture)
	{
		return (gr_backendtexture_get_height_delegate ?? (gr_backendtexture_get_height_delegate = GetSymbol<Delegates.gr_backendtexture_get_height>("gr_backendtexture_get_height")))(texture);
	}

	internal static int gr_backendtexture_get_width(IntPtr texture)
	{
		return (gr_backendtexture_get_width_delegate ?? (gr_backendtexture_get_width_delegate = GetSymbol<Delegates.gr_backendtexture_get_width>("gr_backendtexture_get_width")))(texture);
	}

	internal static bool gr_backendtexture_has_mipmaps(IntPtr texture)
	{
		return (gr_backendtexture_has_mipmaps_delegate ?? (gr_backendtexture_has_mipmaps_delegate = GetSymbol<Delegates.gr_backendtexture_has_mipmaps>("gr_backendtexture_has_mipmaps")))(texture);
	}

	internal static bool gr_backendtexture_is_valid(IntPtr texture)
	{
		return (gr_backendtexture_is_valid_delegate ?? (gr_backendtexture_is_valid_delegate = GetSymbol<Delegates.gr_backendtexture_is_valid>("gr_backendtexture_is_valid")))(texture);
	}

	internal unsafe static IntPtr gr_backendtexture_new_direct3d(int width, int height, GRD3DTextureResourceInfoNative* d3dInfo)
	{
		return (gr_backendtexture_new_direct3d_delegate ?? (gr_backendtexture_new_direct3d_delegate = GetSymbol<Delegates.gr_backendtexture_new_direct3d>("gr_backendtexture_new_direct3d")))(width, height, d3dInfo);
	}

	internal unsafe static IntPtr gr_backendtexture_new_gl(int width, int height, [MarshalAs(UnmanagedType.I1)] bool mipmapped, GRGlTextureInfo* glInfo)
	{
		return (gr_backendtexture_new_gl_delegate ?? (gr_backendtexture_new_gl_delegate = GetSymbol<Delegates.gr_backendtexture_new_gl>("gr_backendtexture_new_gl")))(width, height, mipmapped, glInfo);
	}

	internal unsafe static IntPtr gr_backendtexture_new_metal(int width, int height, [MarshalAs(UnmanagedType.I1)] bool mipmapped, GRMtlTextureInfoNative* mtlInfo)
	{
		return (gr_backendtexture_new_metal_delegate ?? (gr_backendtexture_new_metal_delegate = GetSymbol<Delegates.gr_backendtexture_new_metal>("gr_backendtexture_new_metal")))(width, height, mipmapped, mtlInfo);
	}

	internal unsafe static IntPtr gr_backendtexture_new_vulkan(int width, int height, GRVkImageInfo* vkInfo)
	{
		return (gr_backendtexture_new_vulkan_delegate ?? (gr_backendtexture_new_vulkan_delegate = GetSymbol<Delegates.gr_backendtexture_new_vulkan>("gr_backendtexture_new_vulkan")))(width, height, vkInfo);
	}

	internal static void gr_direct_context_abandon_context(IntPtr context)
	{
		(gr_direct_context_abandon_context_delegate ?? (gr_direct_context_abandon_context_delegate = GetSymbol<Delegates.gr_direct_context_abandon_context>("gr_direct_context_abandon_context")))(context);
	}

	internal static void gr_direct_context_dump_memory_statistics(IntPtr context, IntPtr dump)
	{
		(gr_direct_context_dump_memory_statistics_delegate ?? (gr_direct_context_dump_memory_statistics_delegate = GetSymbol<Delegates.gr_direct_context_dump_memory_statistics>("gr_direct_context_dump_memory_statistics")))(context, dump);
	}

	internal static void gr_direct_context_flush(IntPtr context)
	{
		(gr_direct_context_flush_delegate ?? (gr_direct_context_flush_delegate = GetSymbol<Delegates.gr_direct_context_flush>("gr_direct_context_flush")))(context);
	}

	internal static void gr_direct_context_flush_and_submit(IntPtr context, [MarshalAs(UnmanagedType.I1)] bool syncCpu)
	{
		(gr_direct_context_flush_and_submit_delegate ?? (gr_direct_context_flush_and_submit_delegate = GetSymbol<Delegates.gr_direct_context_flush_and_submit>("gr_direct_context_flush_and_submit")))(context, syncCpu);
	}

	internal static void gr_direct_context_flush_image(IntPtr context, IntPtr image)
	{
		(gr_direct_context_flush_image_delegate ?? (gr_direct_context_flush_image_delegate = GetSymbol<Delegates.gr_direct_context_flush_image>("gr_direct_context_flush_image")))(context, image);
	}

	internal static void gr_direct_context_flush_surface(IntPtr context, IntPtr surface)
	{
		(gr_direct_context_flush_surface_delegate ?? (gr_direct_context_flush_surface_delegate = GetSymbol<Delegates.gr_direct_context_flush_surface>("gr_direct_context_flush_surface")))(context, surface);
	}

	internal static void gr_direct_context_free_gpu_resources(IntPtr context)
	{
		(gr_direct_context_free_gpu_resources_delegate ?? (gr_direct_context_free_gpu_resources_delegate = GetSymbol<Delegates.gr_direct_context_free_gpu_resources>("gr_direct_context_free_gpu_resources")))(context);
	}

	internal static IntPtr gr_direct_context_get_resource_cache_limit(IntPtr context)
	{
		return (gr_direct_context_get_resource_cache_limit_delegate ?? (gr_direct_context_get_resource_cache_limit_delegate = GetSymbol<Delegates.gr_direct_context_get_resource_cache_limit>("gr_direct_context_get_resource_cache_limit")))(context);
	}

	internal unsafe static void gr_direct_context_get_resource_cache_usage(IntPtr context, int* maxResources, IntPtr* maxResourceBytes)
	{
		(gr_direct_context_get_resource_cache_usage_delegate ?? (gr_direct_context_get_resource_cache_usage_delegate = GetSymbol<Delegates.gr_direct_context_get_resource_cache_usage>("gr_direct_context_get_resource_cache_usage")))(context, maxResources, maxResourceBytes);
	}

	internal static bool gr_direct_context_is_abandoned(IntPtr context)
	{
		return (gr_direct_context_is_abandoned_delegate ?? (gr_direct_context_is_abandoned_delegate = GetSymbol<Delegates.gr_direct_context_is_abandoned>("gr_direct_context_is_abandoned")))(context);
	}

	internal static IntPtr gr_direct_context_make_direct3d(GRD3DBackendContextNative d3dBackendContext)
	{
		return (gr_direct_context_make_direct3d_delegate ?? (gr_direct_context_make_direct3d_delegate = GetSymbol<Delegates.gr_direct_context_make_direct3d>("gr_direct_context_make_direct3d")))(d3dBackendContext);
	}

	internal unsafe static IntPtr gr_direct_context_make_direct3d_with_options(GRD3DBackendContextNative d3dBackendContext, GRContextOptionsNative* options)
	{
		return (gr_direct_context_make_direct3d_with_options_delegate ?? (gr_direct_context_make_direct3d_with_options_delegate = GetSymbol<Delegates.gr_direct_context_make_direct3d_with_options>("gr_direct_context_make_direct3d_with_options")))(d3dBackendContext, options);
	}

	internal static IntPtr gr_direct_context_make_gl(IntPtr glInterface)
	{
		return (gr_direct_context_make_gl_delegate ?? (gr_direct_context_make_gl_delegate = GetSymbol<Delegates.gr_direct_context_make_gl>("gr_direct_context_make_gl")))(glInterface);
	}

	internal unsafe static IntPtr gr_direct_context_make_gl_with_options(IntPtr glInterface, GRContextOptionsNative* options)
	{
		return (gr_direct_context_make_gl_with_options_delegate ?? (gr_direct_context_make_gl_with_options_delegate = GetSymbol<Delegates.gr_direct_context_make_gl_with_options>("gr_direct_context_make_gl_with_options")))(glInterface, options);
	}

	internal unsafe static IntPtr gr_direct_context_make_metal(void* device, void* queue)
	{
		return (gr_direct_context_make_metal_delegate ?? (gr_direct_context_make_metal_delegate = GetSymbol<Delegates.gr_direct_context_make_metal>("gr_direct_context_make_metal")))(device, queue);
	}

	internal unsafe static IntPtr gr_direct_context_make_metal_with_options(void* device, void* queue, GRContextOptionsNative* options)
	{
		return (gr_direct_context_make_metal_with_options_delegate ?? (gr_direct_context_make_metal_with_options_delegate = GetSymbol<Delegates.gr_direct_context_make_metal_with_options>("gr_direct_context_make_metal_with_options")))(device, queue, options);
	}

	internal static IntPtr gr_direct_context_make_vulkan(GRVkBackendContextNative vkBackendContext)
	{
		return (gr_direct_context_make_vulkan_delegate ?? (gr_direct_context_make_vulkan_delegate = GetSymbol<Delegates.gr_direct_context_make_vulkan>("gr_direct_context_make_vulkan")))(vkBackendContext);
	}

	internal unsafe static IntPtr gr_direct_context_make_vulkan_with_options(GRVkBackendContextNative vkBackendContext, GRContextOptionsNative* options)
	{
		return (gr_direct_context_make_vulkan_with_options_delegate ?? (gr_direct_context_make_vulkan_with_options_delegate = GetSymbol<Delegates.gr_direct_context_make_vulkan_with_options>("gr_direct_context_make_vulkan_with_options")))(vkBackendContext, options);
	}

	internal static void gr_direct_context_perform_deferred_cleanup(IntPtr context, long ms)
	{
		(gr_direct_context_perform_deferred_cleanup_delegate ?? (gr_direct_context_perform_deferred_cleanup_delegate = GetSymbol<Delegates.gr_direct_context_perform_deferred_cleanup>("gr_direct_context_perform_deferred_cleanup")))(context, ms);
	}

	internal static void gr_direct_context_purge_unlocked_resources(IntPtr context, [MarshalAs(UnmanagedType.I1)] bool scratchResourcesOnly)
	{
		(gr_direct_context_purge_unlocked_resources_delegate ?? (gr_direct_context_purge_unlocked_resources_delegate = GetSymbol<Delegates.gr_direct_context_purge_unlocked_resources>("gr_direct_context_purge_unlocked_resources")))(context, scratchResourcesOnly);
	}

	internal static void gr_direct_context_purge_unlocked_resources_bytes(IntPtr context, IntPtr bytesToPurge, [MarshalAs(UnmanagedType.I1)] bool preferScratchResources)
	{
		(gr_direct_context_purge_unlocked_resources_bytes_delegate ?? (gr_direct_context_purge_unlocked_resources_bytes_delegate = GetSymbol<Delegates.gr_direct_context_purge_unlocked_resources_bytes>("gr_direct_context_purge_unlocked_resources_bytes")))(context, bytesToPurge, preferScratchResources);
	}

	internal static void gr_direct_context_release_resources_and_abandon_context(IntPtr context)
	{
		(gr_direct_context_release_resources_and_abandon_context_delegate ?? (gr_direct_context_release_resources_and_abandon_context_delegate = GetSymbol<Delegates.gr_direct_context_release_resources_and_abandon_context>("gr_direct_context_release_resources_and_abandon_context")))(context);
	}

	internal static void gr_direct_context_reset_context(IntPtr context, uint state)
	{
		(gr_direct_context_reset_context_delegate ?? (gr_direct_context_reset_context_delegate = GetSymbol<Delegates.gr_direct_context_reset_context>("gr_direct_context_reset_context")))(context, state);
	}

	internal static void gr_direct_context_set_resource_cache_limit(IntPtr context, IntPtr maxResourceBytes)
	{
		(gr_direct_context_set_resource_cache_limit_delegate ?? (gr_direct_context_set_resource_cache_limit_delegate = GetSymbol<Delegates.gr_direct_context_set_resource_cache_limit>("gr_direct_context_set_resource_cache_limit")))(context, maxResourceBytes);
	}

	internal static bool gr_direct_context_submit(IntPtr context, [MarshalAs(UnmanagedType.I1)] bool syncCpu)
	{
		return (gr_direct_context_submit_delegate ?? (gr_direct_context_submit_delegate = GetSymbol<Delegates.gr_direct_context_submit>("gr_direct_context_submit")))(context, syncCpu);
	}

	internal unsafe static IntPtr gr_glinterface_assemble_gl_interface(void* ctx, GRGlGetProcProxyDelegate get)
	{
		return (gr_glinterface_assemble_gl_interface_delegate ?? (gr_glinterface_assemble_gl_interface_delegate = GetSymbol<Delegates.gr_glinterface_assemble_gl_interface>("gr_glinterface_assemble_gl_interface")))(ctx, get);
	}

	internal unsafe static IntPtr gr_glinterface_assemble_gles_interface(void* ctx, GRGlGetProcProxyDelegate get)
	{
		return (gr_glinterface_assemble_gles_interface_delegate ?? (gr_glinterface_assemble_gles_interface_delegate = GetSymbol<Delegates.gr_glinterface_assemble_gles_interface>("gr_glinterface_assemble_gles_interface")))(ctx, get);
	}

	internal unsafe static IntPtr gr_glinterface_assemble_interface(void* ctx, GRGlGetProcProxyDelegate get)
	{
		return (gr_glinterface_assemble_interface_delegate ?? (gr_glinterface_assemble_interface_delegate = GetSymbol<Delegates.gr_glinterface_assemble_interface>("gr_glinterface_assemble_interface")))(ctx, get);
	}

	internal unsafe static IntPtr gr_glinterface_assemble_webgl_interface(void* ctx, GRGlGetProcProxyDelegate get)
	{
		return (gr_glinterface_assemble_webgl_interface_delegate ?? (gr_glinterface_assemble_webgl_interface_delegate = GetSymbol<Delegates.gr_glinterface_assemble_webgl_interface>("gr_glinterface_assemble_webgl_interface")))(ctx, get);
	}

	internal static IntPtr gr_glinterface_create_native_interface()
	{
		return (gr_glinterface_create_native_interface_delegate ?? (gr_glinterface_create_native_interface_delegate = GetSymbol<Delegates.gr_glinterface_create_native_interface>("gr_glinterface_create_native_interface")))();
	}

	internal static bool gr_glinterface_has_extension(IntPtr glInterface, [MarshalAs(UnmanagedType.LPStr)] string extension)
	{
		return (gr_glinterface_has_extension_delegate ?? (gr_glinterface_has_extension_delegate = GetSymbol<Delegates.gr_glinterface_has_extension>("gr_glinterface_has_extension")))(glInterface, extension);
	}

	internal static void gr_glinterface_unref(IntPtr glInterface)
	{
		(gr_glinterface_unref_delegate ?? (gr_glinterface_unref_delegate = GetSymbol<Delegates.gr_glinterface_unref>("gr_glinterface_unref")))(glInterface);
	}

	internal static bool gr_glinterface_validate(IntPtr glInterface)
	{
		return (gr_glinterface_validate_delegate ?? (gr_glinterface_validate_delegate = GetSymbol<Delegates.gr_glinterface_validate>("gr_glinterface_validate")))(glInterface);
	}

	internal static GRBackendNative gr_recording_context_get_backend(IntPtr context)
	{
		return (gr_recording_context_get_backend_delegate ?? (gr_recording_context_get_backend_delegate = GetSymbol<Delegates.gr_recording_context_get_backend>("gr_recording_context_get_backend")))(context);
	}

	internal static IntPtr gr_recording_context_get_direct_context(IntPtr context)
	{
		return (gr_recording_context_get_direct_context_delegate ?? (gr_recording_context_get_direct_context_delegate = GetSymbol<Delegates.gr_recording_context_get_direct_context>("gr_recording_context_get_direct_context")))(context);
	}

	internal static int gr_recording_context_get_max_surface_sample_count_for_color_type(IntPtr context, SKColorTypeNative colorType)
	{
		return (gr_recording_context_get_max_surface_sample_count_for_color_type_delegate ?? (gr_recording_context_get_max_surface_sample_count_for_color_type_delegate = GetSymbol<Delegates.gr_recording_context_get_max_surface_sample_count_for_color_type>("gr_recording_context_get_max_surface_sample_count_for_color_type")))(context, colorType);
	}

	internal static bool gr_recording_context_is_abandoned(IntPtr context)
	{
		return (gr_recording_context_is_abandoned_delegate ?? (gr_recording_context_is_abandoned_delegate = GetSymbol<Delegates.gr_recording_context_is_abandoned>("gr_recording_context_is_abandoned")))(context);
	}

	internal static int gr_recording_context_max_render_target_size(IntPtr context)
	{
		return (gr_recording_context_max_render_target_size_delegate ?? (gr_recording_context_max_render_target_size_delegate = GetSymbol<Delegates.gr_recording_context_max_render_target_size>("gr_recording_context_max_render_target_size")))(context);
	}

	internal static int gr_recording_context_max_texture_size(IntPtr context)
	{
		return (gr_recording_context_max_texture_size_delegate ?? (gr_recording_context_max_texture_size_delegate = GetSymbol<Delegates.gr_recording_context_max_texture_size>("gr_recording_context_max_texture_size")))(context);
	}

	internal static void gr_recording_context_unref(IntPtr context)
	{
		(gr_recording_context_unref_delegate ?? (gr_recording_context_unref_delegate = GetSymbol<Delegates.gr_recording_context_unref>("gr_recording_context_unref")))(context);
	}

	internal static void gr_vk_extensions_delete(IntPtr extensions)
	{
		(gr_vk_extensions_delete_delegate ?? (gr_vk_extensions_delete_delegate = GetSymbol<Delegates.gr_vk_extensions_delete>("gr_vk_extensions_delete")))(extensions);
	}

	internal static bool gr_vk_extensions_has_extension(IntPtr extensions, [MarshalAs(UnmanagedType.LPStr)] string ext, uint minVersion)
	{
		return (gr_vk_extensions_has_extension_delegate ?? (gr_vk_extensions_has_extension_delegate = GetSymbol<Delegates.gr_vk_extensions_has_extension>("gr_vk_extensions_has_extension")))(extensions, ext, minVersion);
	}

	internal unsafe static void gr_vk_extensions_init(IntPtr extensions, GRVkGetProcProxyDelegate getProc, void* userData, IntPtr instance, IntPtr physDev, uint instanceExtensionCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] instanceExtensions, uint deviceExtensionCount, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] deviceExtensions)
	{
		(gr_vk_extensions_init_delegate ?? (gr_vk_extensions_init_delegate = GetSymbol<Delegates.gr_vk_extensions_init>("gr_vk_extensions_init")))(extensions, getProc, userData, instance, physDev, instanceExtensionCount, instanceExtensions, deviceExtensionCount, deviceExtensions);
	}

	internal static IntPtr gr_vk_extensions_new()
	{
		return (gr_vk_extensions_new_delegate ?? (gr_vk_extensions_new_delegate = GetSymbol<Delegates.gr_vk_extensions_new>("gr_vk_extensions_new")))();
	}

	internal static void sk_bitmap_destructor(IntPtr cbitmap)
	{
		(sk_bitmap_destructor_delegate ?? (sk_bitmap_destructor_delegate = GetSymbol<Delegates.sk_bitmap_destructor>("sk_bitmap_destructor")))(cbitmap);
	}

	internal static void sk_bitmap_erase(IntPtr cbitmap, uint color)
	{
		(sk_bitmap_erase_delegate ?? (sk_bitmap_erase_delegate = GetSymbol<Delegates.sk_bitmap_erase>("sk_bitmap_erase")))(cbitmap, color);
	}

	internal unsafe static void sk_bitmap_erase_rect(IntPtr cbitmap, uint color, SKRectI* rect)
	{
		(sk_bitmap_erase_rect_delegate ?? (sk_bitmap_erase_rect_delegate = GetSymbol<Delegates.sk_bitmap_erase_rect>("sk_bitmap_erase_rect")))(cbitmap, color, rect);
	}

	internal unsafe static bool sk_bitmap_extract_alpha(IntPtr cbitmap, IntPtr dst, IntPtr paint, SKPointI* offset)
	{
		return (sk_bitmap_extract_alpha_delegate ?? (sk_bitmap_extract_alpha_delegate = GetSymbol<Delegates.sk_bitmap_extract_alpha>("sk_bitmap_extract_alpha")))(cbitmap, dst, paint, offset);
	}

	internal unsafe static bool sk_bitmap_extract_subset(IntPtr cbitmap, IntPtr dst, SKRectI* subset)
	{
		return (sk_bitmap_extract_subset_delegate ?? (sk_bitmap_extract_subset_delegate = GetSymbol<Delegates.sk_bitmap_extract_subset>("sk_bitmap_extract_subset")))(cbitmap, dst, subset);
	}

	internal unsafe static void* sk_bitmap_get_addr(IntPtr cbitmap, int x, int y)
	{
		return (sk_bitmap_get_addr_delegate ?? (sk_bitmap_get_addr_delegate = GetSymbol<Delegates.sk_bitmap_get_addr>("sk_bitmap_get_addr")))(cbitmap, x, y);
	}

	internal unsafe static ushort* sk_bitmap_get_addr_16(IntPtr cbitmap, int x, int y)
	{
		return (sk_bitmap_get_addr_16_delegate ?? (sk_bitmap_get_addr_16_delegate = GetSymbol<Delegates.sk_bitmap_get_addr_16>("sk_bitmap_get_addr_16")))(cbitmap, x, y);
	}

	internal unsafe static uint* sk_bitmap_get_addr_32(IntPtr cbitmap, int x, int y)
	{
		return (sk_bitmap_get_addr_32_delegate ?? (sk_bitmap_get_addr_32_delegate = GetSymbol<Delegates.sk_bitmap_get_addr_32>("sk_bitmap_get_addr_32")))(cbitmap, x, y);
	}

	internal unsafe static byte* sk_bitmap_get_addr_8(IntPtr cbitmap, int x, int y)
	{
		return (sk_bitmap_get_addr_8_delegate ?? (sk_bitmap_get_addr_8_delegate = GetSymbol<Delegates.sk_bitmap_get_addr_8>("sk_bitmap_get_addr_8")))(cbitmap, x, y);
	}

	internal static IntPtr sk_bitmap_get_byte_count(IntPtr cbitmap)
	{
		return (sk_bitmap_get_byte_count_delegate ?? (sk_bitmap_get_byte_count_delegate = GetSymbol<Delegates.sk_bitmap_get_byte_count>("sk_bitmap_get_byte_count")))(cbitmap);
	}

	internal unsafe static void sk_bitmap_get_info(IntPtr cbitmap, SKImageInfoNative* info)
	{
		(sk_bitmap_get_info_delegate ?? (sk_bitmap_get_info_delegate = GetSymbol<Delegates.sk_bitmap_get_info>("sk_bitmap_get_info")))(cbitmap, info);
	}

	internal static uint sk_bitmap_get_pixel_color(IntPtr cbitmap, int x, int y)
	{
		return (sk_bitmap_get_pixel_color_delegate ?? (sk_bitmap_get_pixel_color_delegate = GetSymbol<Delegates.sk_bitmap_get_pixel_color>("sk_bitmap_get_pixel_color")))(cbitmap, x, y);
	}

	internal unsafe static void sk_bitmap_get_pixel_colors(IntPtr cbitmap, uint* colors)
	{
		(sk_bitmap_get_pixel_colors_delegate ?? (sk_bitmap_get_pixel_colors_delegate = GetSymbol<Delegates.sk_bitmap_get_pixel_colors>("sk_bitmap_get_pixel_colors")))(cbitmap, colors);
	}

	internal unsafe static void* sk_bitmap_get_pixels(IntPtr cbitmap, IntPtr* length)
	{
		return (sk_bitmap_get_pixels_delegate ?? (sk_bitmap_get_pixels_delegate = GetSymbol<Delegates.sk_bitmap_get_pixels>("sk_bitmap_get_pixels")))(cbitmap, length);
	}

	internal static IntPtr sk_bitmap_get_row_bytes(IntPtr cbitmap)
	{
		return (sk_bitmap_get_row_bytes_delegate ?? (sk_bitmap_get_row_bytes_delegate = GetSymbol<Delegates.sk_bitmap_get_row_bytes>("sk_bitmap_get_row_bytes")))(cbitmap);
	}

	internal unsafe static bool sk_bitmap_install_pixels(IntPtr cbitmap, SKImageInfoNative* cinfo, void* pixels, IntPtr rowBytes, SKBitmapReleaseProxyDelegate releaseProc, void* context)
	{
		return (sk_bitmap_install_pixels_delegate ?? (sk_bitmap_install_pixels_delegate = GetSymbol<Delegates.sk_bitmap_install_pixels>("sk_bitmap_install_pixels")))(cbitmap, cinfo, pixels, rowBytes, releaseProc, context);
	}

	internal static bool sk_bitmap_install_pixels_with_pixmap(IntPtr cbitmap, IntPtr cpixmap)
	{
		return (sk_bitmap_install_pixels_with_pixmap_delegate ?? (sk_bitmap_install_pixels_with_pixmap_delegate = GetSymbol<Delegates.sk_bitmap_install_pixels_with_pixmap>("sk_bitmap_install_pixels_with_pixmap")))(cbitmap, cpixmap);
	}

	internal static bool sk_bitmap_is_immutable(IntPtr cbitmap)
	{
		return (sk_bitmap_is_immutable_delegate ?? (sk_bitmap_is_immutable_delegate = GetSymbol<Delegates.sk_bitmap_is_immutable>("sk_bitmap_is_immutable")))(cbitmap);
	}

	internal static bool sk_bitmap_is_null(IntPtr cbitmap)
	{
		return (sk_bitmap_is_null_delegate ?? (sk_bitmap_is_null_delegate = GetSymbol<Delegates.sk_bitmap_is_null>("sk_bitmap_is_null")))(cbitmap);
	}

	internal unsafe static IntPtr sk_bitmap_make_shader(IntPtr cbitmap, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions* sampling, SKMatrix* cmatrix)
	{
		return (sk_bitmap_make_shader_delegate ?? (sk_bitmap_make_shader_delegate = GetSymbol<Delegates.sk_bitmap_make_shader>("sk_bitmap_make_shader")))(cbitmap, tmx, tmy, sampling, cmatrix);
	}

	internal static IntPtr sk_bitmap_new()
	{
		return (sk_bitmap_new_delegate ?? (sk_bitmap_new_delegate = GetSymbol<Delegates.sk_bitmap_new>("sk_bitmap_new")))();
	}

	internal static void sk_bitmap_notify_pixels_changed(IntPtr cbitmap)
	{
		(sk_bitmap_notify_pixels_changed_delegate ?? (sk_bitmap_notify_pixels_changed_delegate = GetSymbol<Delegates.sk_bitmap_notify_pixels_changed>("sk_bitmap_notify_pixels_changed")))(cbitmap);
	}

	internal static bool sk_bitmap_peek_pixels(IntPtr cbitmap, IntPtr cpixmap)
	{
		return (sk_bitmap_peek_pixels_delegate ?? (sk_bitmap_peek_pixels_delegate = GetSymbol<Delegates.sk_bitmap_peek_pixels>("sk_bitmap_peek_pixels")))(cbitmap, cpixmap);
	}

	internal static bool sk_bitmap_ready_to_draw(IntPtr cbitmap)
	{
		return (sk_bitmap_ready_to_draw_delegate ?? (sk_bitmap_ready_to_draw_delegate = GetSymbol<Delegates.sk_bitmap_ready_to_draw>("sk_bitmap_ready_to_draw")))(cbitmap);
	}

	internal static void sk_bitmap_reset(IntPtr cbitmap)
	{
		(sk_bitmap_reset_delegate ?? (sk_bitmap_reset_delegate = GetSymbol<Delegates.sk_bitmap_reset>("sk_bitmap_reset")))(cbitmap);
	}

	internal static void sk_bitmap_set_immutable(IntPtr cbitmap)
	{
		(sk_bitmap_set_immutable_delegate ?? (sk_bitmap_set_immutable_delegate = GetSymbol<Delegates.sk_bitmap_set_immutable>("sk_bitmap_set_immutable")))(cbitmap);
	}

	internal unsafe static void sk_bitmap_set_pixels(IntPtr cbitmap, void* pixels)
	{
		(sk_bitmap_set_pixels_delegate ?? (sk_bitmap_set_pixels_delegate = GetSymbol<Delegates.sk_bitmap_set_pixels>("sk_bitmap_set_pixels")))(cbitmap, pixels);
	}

	internal static void sk_bitmap_swap(IntPtr cbitmap, IntPtr cother)
	{
		(sk_bitmap_swap_delegate ?? (sk_bitmap_swap_delegate = GetSymbol<Delegates.sk_bitmap_swap>("sk_bitmap_swap")))(cbitmap, cother);
	}

	internal unsafe static bool sk_bitmap_try_alloc_pixels(IntPtr cbitmap, SKImageInfoNative* requestedInfo, IntPtr rowBytes)
	{
		return (sk_bitmap_try_alloc_pixels_delegate ?? (sk_bitmap_try_alloc_pixels_delegate = GetSymbol<Delegates.sk_bitmap_try_alloc_pixels>("sk_bitmap_try_alloc_pixels")))(cbitmap, requestedInfo, rowBytes);
	}

	internal unsafe static bool sk_bitmap_try_alloc_pixels_with_flags(IntPtr cbitmap, SKImageInfoNative* requestedInfo, uint flags)
	{
		return (sk_bitmap_try_alloc_pixels_with_flags_delegate ?? (sk_bitmap_try_alloc_pixels_with_flags_delegate = GetSymbol<Delegates.sk_bitmap_try_alloc_pixels_with_flags>("sk_bitmap_try_alloc_pixels_with_flags")))(cbitmap, requestedInfo, flags);
	}

	internal static IntPtr sk_blender_new_arithmetic(float k1, float k2, float k3, float k4, [MarshalAs(UnmanagedType.I1)] bool enforcePremul)
	{
		return (sk_blender_new_arithmetic_delegate ?? (sk_blender_new_arithmetic_delegate = GetSymbol<Delegates.sk_blender_new_arithmetic>("sk_blender_new_arithmetic")))(k1, k2, k3, k4, enforcePremul);
	}

	internal static IntPtr sk_blender_new_mode(SKBlendMode mode)
	{
		return (sk_blender_new_mode_delegate ?? (sk_blender_new_mode_delegate = GetSymbol<Delegates.sk_blender_new_mode>("sk_blender_new_mode")))(mode);
	}

	internal static void sk_blender_ref(IntPtr blender)
	{
		(sk_blender_ref_delegate ?? (sk_blender_ref_delegate = GetSymbol<Delegates.sk_blender_ref>("sk_blender_ref")))(blender);
	}

	internal static void sk_blender_unref(IntPtr blender)
	{
		(sk_blender_unref_delegate ?? (sk_blender_unref_delegate = GetSymbol<Delegates.sk_blender_unref>("sk_blender_unref")))(blender);
	}

	internal static void sk_canvas_clear(IntPtr ccanvas, uint color)
	{
		(sk_canvas_clear_delegate ?? (sk_canvas_clear_delegate = GetSymbol<Delegates.sk_canvas_clear>("sk_canvas_clear")))(ccanvas, color);
	}

	internal static void sk_canvas_clear_color4f(IntPtr ccanvas, SKColorF color)
	{
		(sk_canvas_clear_color4f_delegate ?? (sk_canvas_clear_color4f_delegate = GetSymbol<Delegates.sk_canvas_clear_color4f>("sk_canvas_clear_color4f")))(ccanvas, color);
	}

	internal static void sk_canvas_clip_path_with_operation(IntPtr ccanvas, IntPtr cpath, SKClipOperation op, [MarshalAs(UnmanagedType.I1)] bool doAA)
	{
		(sk_canvas_clip_path_with_operation_delegate ?? (sk_canvas_clip_path_with_operation_delegate = GetSymbol<Delegates.sk_canvas_clip_path_with_operation>("sk_canvas_clip_path_with_operation")))(ccanvas, cpath, op, doAA);
	}

	internal unsafe static void sk_canvas_clip_rect_with_operation(IntPtr ccanvas, SKRect* crect, SKClipOperation op, [MarshalAs(UnmanagedType.I1)] bool doAA)
	{
		(sk_canvas_clip_rect_with_operation_delegate ?? (sk_canvas_clip_rect_with_operation_delegate = GetSymbol<Delegates.sk_canvas_clip_rect_with_operation>("sk_canvas_clip_rect_with_operation")))(ccanvas, crect, op, doAA);
	}

	internal static void sk_canvas_clip_region(IntPtr ccanvas, IntPtr region, SKClipOperation op)
	{
		(sk_canvas_clip_region_delegate ?? (sk_canvas_clip_region_delegate = GetSymbol<Delegates.sk_canvas_clip_region>("sk_canvas_clip_region")))(ccanvas, region, op);
	}

	internal static void sk_canvas_clip_rrect_with_operation(IntPtr ccanvas, IntPtr crect, SKClipOperation op, [MarshalAs(UnmanagedType.I1)] bool doAA)
	{
		(sk_canvas_clip_rrect_with_operation_delegate ?? (sk_canvas_clip_rrect_with_operation_delegate = GetSymbol<Delegates.sk_canvas_clip_rrect_with_operation>("sk_canvas_clip_rrect_with_operation")))(ccanvas, crect, op, doAA);
	}

	internal unsafe static void sk_canvas_concat(IntPtr ccanvas, SKMatrix44* cmatrix)
	{
		(sk_canvas_concat_delegate ?? (sk_canvas_concat_delegate = GetSymbol<Delegates.sk_canvas_concat>("sk_canvas_concat")))(ccanvas, cmatrix);
	}

	internal static void sk_canvas_destroy(IntPtr ccanvas)
	{
		(sk_canvas_destroy_delegate ?? (sk_canvas_destroy_delegate = GetSymbol<Delegates.sk_canvas_destroy>("sk_canvas_destroy")))(ccanvas);
	}

	internal static void sk_canvas_discard(IntPtr ccanvas)
	{
		(sk_canvas_discard_delegate ?? (sk_canvas_discard_delegate = GetSymbol<Delegates.sk_canvas_discard>("sk_canvas_discard")))(ccanvas);
	}

	internal unsafe static void sk_canvas_draw_annotation(IntPtr t, SKRect* rect, void* key, IntPtr value)
	{
		(sk_canvas_draw_annotation_delegate ?? (sk_canvas_draw_annotation_delegate = GetSymbol<Delegates.sk_canvas_draw_annotation>("sk_canvas_draw_annotation")))(t, rect, key, value);
	}

	internal unsafe static void sk_canvas_draw_arc(IntPtr ccanvas, SKRect* oval, float startAngle, float sweepAngle, [MarshalAs(UnmanagedType.I1)] bool useCenter, IntPtr paint)
	{
		(sk_canvas_draw_arc_delegate ?? (sk_canvas_draw_arc_delegate = GetSymbol<Delegates.sk_canvas_draw_arc>("sk_canvas_draw_arc")))(ccanvas, oval, startAngle, sweepAngle, useCenter, paint);
	}

	internal unsafe static void sk_canvas_draw_atlas(IntPtr ccanvas, IntPtr atlas, SKRotationScaleMatrix* xform, SKRect* tex, uint* colors, int count, SKBlendMode mode, SKSamplingOptions* sampling, SKRect* cullRect, IntPtr paint)
	{
		(sk_canvas_draw_atlas_delegate ?? (sk_canvas_draw_atlas_delegate = GetSymbol<Delegates.sk_canvas_draw_atlas>("sk_canvas_draw_atlas")))(ccanvas, atlas, xform, tex, colors, count, mode, sampling, cullRect, paint);
	}

	internal static void sk_canvas_draw_circle(IntPtr ccanvas, float cx, float cy, float rad, IntPtr cpaint)
	{
		(sk_canvas_draw_circle_delegate ?? (sk_canvas_draw_circle_delegate = GetSymbol<Delegates.sk_canvas_draw_circle>("sk_canvas_draw_circle")))(ccanvas, cx, cy, rad, cpaint);
	}

	internal static void sk_canvas_draw_color(IntPtr ccanvas, uint color, SKBlendMode cmode)
	{
		(sk_canvas_draw_color_delegate ?? (sk_canvas_draw_color_delegate = GetSymbol<Delegates.sk_canvas_draw_color>("sk_canvas_draw_color")))(ccanvas, color, cmode);
	}

	internal static void sk_canvas_draw_color4f(IntPtr ccanvas, SKColorF color, SKBlendMode cmode)
	{
		(sk_canvas_draw_color4f_delegate ?? (sk_canvas_draw_color4f_delegate = GetSymbol<Delegates.sk_canvas_draw_color4f>("sk_canvas_draw_color4f")))(ccanvas, color, cmode);
	}

	internal unsafe static void sk_canvas_draw_drawable(IntPtr ccanvas, IntPtr cdrawable, SKMatrix* cmatrix)
	{
		(sk_canvas_draw_drawable_delegate ?? (sk_canvas_draw_drawable_delegate = GetSymbol<Delegates.sk_canvas_draw_drawable>("sk_canvas_draw_drawable")))(ccanvas, cdrawable, cmatrix);
	}

	internal static void sk_canvas_draw_drrect(IntPtr ccanvas, IntPtr outer, IntPtr inner, IntPtr paint)
	{
		(sk_canvas_draw_drrect_delegate ?? (sk_canvas_draw_drrect_delegate = GetSymbol<Delegates.sk_canvas_draw_drrect>("sk_canvas_draw_drrect")))(ccanvas, outer, inner, paint);
	}

	internal unsafe static void sk_canvas_draw_image(IntPtr ccanvas, IntPtr cimage, float x, float y, SKSamplingOptions* sampling, IntPtr cpaint)
	{
		(sk_canvas_draw_image_delegate ?? (sk_canvas_draw_image_delegate = GetSymbol<Delegates.sk_canvas_draw_image>("sk_canvas_draw_image")))(ccanvas, cimage, x, y, sampling, cpaint);
	}

	internal unsafe static void sk_canvas_draw_image_lattice(IntPtr ccanvas, IntPtr image, SKLatticeInternal* lattice, SKRect* dst, SKFilterMode mode, IntPtr paint)
	{
		(sk_canvas_draw_image_lattice_delegate ?? (sk_canvas_draw_image_lattice_delegate = GetSymbol<Delegates.sk_canvas_draw_image_lattice>("sk_canvas_draw_image_lattice")))(ccanvas, image, lattice, dst, mode, paint);
	}

	internal unsafe static void sk_canvas_draw_image_nine(IntPtr ccanvas, IntPtr image, SKRectI* center, SKRect* dst, SKFilterMode mode, IntPtr paint)
	{
		(sk_canvas_draw_image_nine_delegate ?? (sk_canvas_draw_image_nine_delegate = GetSymbol<Delegates.sk_canvas_draw_image_nine>("sk_canvas_draw_image_nine")))(ccanvas, image, center, dst, mode, paint);
	}

	internal unsafe static void sk_canvas_draw_image_rect(IntPtr ccanvas, IntPtr cimage, SKRect* csrcR, SKRect* cdstR, SKSamplingOptions* sampling, IntPtr cpaint)
	{
		(sk_canvas_draw_image_rect_delegate ?? (sk_canvas_draw_image_rect_delegate = GetSymbol<Delegates.sk_canvas_draw_image_rect>("sk_canvas_draw_image_rect")))(ccanvas, cimage, csrcR, cdstR, sampling, cpaint);
	}

	internal static void sk_canvas_draw_line(IntPtr ccanvas, float x0, float y0, float x1, float y1, IntPtr cpaint)
	{
		(sk_canvas_draw_line_delegate ?? (sk_canvas_draw_line_delegate = GetSymbol<Delegates.sk_canvas_draw_line>("sk_canvas_draw_line")))(ccanvas, x0, y0, x1, y1, cpaint);
	}

	internal unsafe static void sk_canvas_draw_link_destination_annotation(IntPtr t, SKRect* rect, IntPtr value)
	{
		(sk_canvas_draw_link_destination_annotation_delegate ?? (sk_canvas_draw_link_destination_annotation_delegate = GetSymbol<Delegates.sk_canvas_draw_link_destination_annotation>("sk_canvas_draw_link_destination_annotation")))(t, rect, value);
	}

	internal unsafe static void sk_canvas_draw_named_destination_annotation(IntPtr t, SKPoint* point, IntPtr value)
	{
		(sk_canvas_draw_named_destination_annotation_delegate ?? (sk_canvas_draw_named_destination_annotation_delegate = GetSymbol<Delegates.sk_canvas_draw_named_destination_annotation>("sk_canvas_draw_named_destination_annotation")))(t, point, value);
	}

	internal unsafe static void sk_canvas_draw_oval(IntPtr ccanvas, SKRect* crect, IntPtr cpaint)
	{
		(sk_canvas_draw_oval_delegate ?? (sk_canvas_draw_oval_delegate = GetSymbol<Delegates.sk_canvas_draw_oval>("sk_canvas_draw_oval")))(ccanvas, crect, cpaint);
	}

	internal static void sk_canvas_draw_paint(IntPtr ccanvas, IntPtr cpaint)
	{
		(sk_canvas_draw_paint_delegate ?? (sk_canvas_draw_paint_delegate = GetSymbol<Delegates.sk_canvas_draw_paint>("sk_canvas_draw_paint")))(ccanvas, cpaint);
	}

	internal unsafe static void sk_canvas_draw_patch(IntPtr ccanvas, SKPoint* cubics, uint* colors, SKPoint* texCoords, SKBlendMode mode, IntPtr paint)
	{
		(sk_canvas_draw_patch_delegate ?? (sk_canvas_draw_patch_delegate = GetSymbol<Delegates.sk_canvas_draw_patch>("sk_canvas_draw_patch")))(ccanvas, cubics, colors, texCoords, mode, paint);
	}

	internal static void sk_canvas_draw_path(IntPtr ccanvas, IntPtr cpath, IntPtr cpaint)
	{
		(sk_canvas_draw_path_delegate ?? (sk_canvas_draw_path_delegate = GetSymbol<Delegates.sk_canvas_draw_path>("sk_canvas_draw_path")))(ccanvas, cpath, cpaint);
	}

	internal unsafe static void sk_canvas_draw_picture(IntPtr ccanvas, IntPtr cpicture, SKMatrix* cmatrix, IntPtr cpaint)
	{
		(sk_canvas_draw_picture_delegate ?? (sk_canvas_draw_picture_delegate = GetSymbol<Delegates.sk_canvas_draw_picture>("sk_canvas_draw_picture")))(ccanvas, cpicture, cmatrix, cpaint);
	}

	internal static void sk_canvas_draw_point(IntPtr ccanvas, float x, float y, IntPtr cpaint)
	{
		(sk_canvas_draw_point_delegate ?? (sk_canvas_draw_point_delegate = GetSymbol<Delegates.sk_canvas_draw_point>("sk_canvas_draw_point")))(ccanvas, x, y, cpaint);
	}

	internal unsafe static void sk_canvas_draw_points(IntPtr ccanvas, SKPointMode pointMode, IntPtr count, SKPoint* points, IntPtr cpaint)
	{
		(sk_canvas_draw_points_delegate ?? (sk_canvas_draw_points_delegate = GetSymbol<Delegates.sk_canvas_draw_points>("sk_canvas_draw_points")))(ccanvas, pointMode, count, points, cpaint);
	}

	internal unsafe static void sk_canvas_draw_rect(IntPtr ccanvas, SKRect* crect, IntPtr cpaint)
	{
		(sk_canvas_draw_rect_delegate ?? (sk_canvas_draw_rect_delegate = GetSymbol<Delegates.sk_canvas_draw_rect>("sk_canvas_draw_rect")))(ccanvas, crect, cpaint);
	}

	internal static void sk_canvas_draw_region(IntPtr ccanvas, IntPtr cregion, IntPtr cpaint)
	{
		(sk_canvas_draw_region_delegate ?? (sk_canvas_draw_region_delegate = GetSymbol<Delegates.sk_canvas_draw_region>("sk_canvas_draw_region")))(ccanvas, cregion, cpaint);
	}

	internal unsafe static void sk_canvas_draw_round_rect(IntPtr ccanvas, SKRect* crect, float rx, float ry, IntPtr cpaint)
	{
		(sk_canvas_draw_round_rect_delegate ?? (sk_canvas_draw_round_rect_delegate = GetSymbol<Delegates.sk_canvas_draw_round_rect>("sk_canvas_draw_round_rect")))(ccanvas, crect, rx, ry, cpaint);
	}

	internal static void sk_canvas_draw_rrect(IntPtr ccanvas, IntPtr crect, IntPtr cpaint)
	{
		(sk_canvas_draw_rrect_delegate ?? (sk_canvas_draw_rrect_delegate = GetSymbol<Delegates.sk_canvas_draw_rrect>("sk_canvas_draw_rrect")))(ccanvas, crect, cpaint);
	}

	internal unsafe static void sk_canvas_draw_simple_text(IntPtr ccanvas, void* text, IntPtr byte_length, SKTextEncoding encoding, float x, float y, IntPtr cfont, IntPtr cpaint)
	{
		(sk_canvas_draw_simple_text_delegate ?? (sk_canvas_draw_simple_text_delegate = GetSymbol<Delegates.sk_canvas_draw_simple_text>("sk_canvas_draw_simple_text")))(ccanvas, text, byte_length, encoding, x, y, cfont, cpaint);
	}

	internal static void sk_canvas_draw_text_blob(IntPtr ccanvas, IntPtr text, float x, float y, IntPtr cpaint)
	{
		(sk_canvas_draw_text_blob_delegate ?? (sk_canvas_draw_text_blob_delegate = GetSymbol<Delegates.sk_canvas_draw_text_blob>("sk_canvas_draw_text_blob")))(ccanvas, text, x, y, cpaint);
	}

	internal unsafe static void sk_canvas_draw_url_annotation(IntPtr t, SKRect* rect, IntPtr value)
	{
		(sk_canvas_draw_url_annotation_delegate ?? (sk_canvas_draw_url_annotation_delegate = GetSymbol<Delegates.sk_canvas_draw_url_annotation>("sk_canvas_draw_url_annotation")))(t, rect, value);
	}

	internal static void sk_canvas_draw_vertices(IntPtr ccanvas, IntPtr vertices, SKBlendMode mode, IntPtr paint)
	{
		(sk_canvas_draw_vertices_delegate ?? (sk_canvas_draw_vertices_delegate = GetSymbol<Delegates.sk_canvas_draw_vertices>("sk_canvas_draw_vertices")))(ccanvas, vertices, mode, paint);
	}

	internal unsafe static bool sk_canvas_get_device_clip_bounds(IntPtr ccanvas, SKRectI* cbounds)
	{
		return (sk_canvas_get_device_clip_bounds_delegate ?? (sk_canvas_get_device_clip_bounds_delegate = GetSymbol<Delegates.sk_canvas_get_device_clip_bounds>("sk_canvas_get_device_clip_bounds")))(ccanvas, cbounds);
	}

	internal unsafe static bool sk_canvas_get_local_clip_bounds(IntPtr ccanvas, SKRect* cbounds)
	{
		return (sk_canvas_get_local_clip_bounds_delegate ?? (sk_canvas_get_local_clip_bounds_delegate = GetSymbol<Delegates.sk_canvas_get_local_clip_bounds>("sk_canvas_get_local_clip_bounds")))(ccanvas, cbounds);
	}

	internal unsafe static void sk_canvas_get_matrix(IntPtr ccanvas, SKMatrix44* cmatrix)
	{
		(sk_canvas_get_matrix_delegate ?? (sk_canvas_get_matrix_delegate = GetSymbol<Delegates.sk_canvas_get_matrix>("sk_canvas_get_matrix")))(ccanvas, cmatrix);
	}

	internal static int sk_canvas_get_save_count(IntPtr ccanvas)
	{
		return (sk_canvas_get_save_count_delegate ?? (sk_canvas_get_save_count_delegate = GetSymbol<Delegates.sk_canvas_get_save_count>("sk_canvas_get_save_count")))(ccanvas);
	}

	internal static bool sk_canvas_is_clip_empty(IntPtr ccanvas)
	{
		return (sk_canvas_is_clip_empty_delegate ?? (sk_canvas_is_clip_empty_delegate = GetSymbol<Delegates.sk_canvas_is_clip_empty>("sk_canvas_is_clip_empty")))(ccanvas);
	}

	internal static bool sk_canvas_is_clip_rect(IntPtr ccanvas)
	{
		return (sk_canvas_is_clip_rect_delegate ?? (sk_canvas_is_clip_rect_delegate = GetSymbol<Delegates.sk_canvas_is_clip_rect>("sk_canvas_is_clip_rect")))(ccanvas);
	}

	internal static IntPtr sk_canvas_new_from_bitmap(IntPtr bitmap)
	{
		return (sk_canvas_new_from_bitmap_delegate ?? (sk_canvas_new_from_bitmap_delegate = GetSymbol<Delegates.sk_canvas_new_from_bitmap>("sk_canvas_new_from_bitmap")))(bitmap);
	}

	internal unsafe static IntPtr sk_canvas_new_from_raster(SKImageInfoNative* cinfo, void* pixels, IntPtr rowBytes, IntPtr props)
	{
		return (sk_canvas_new_from_raster_delegate ?? (sk_canvas_new_from_raster_delegate = GetSymbol<Delegates.sk_canvas_new_from_raster>("sk_canvas_new_from_raster")))(cinfo, pixels, rowBytes, props);
	}

	internal unsafe static bool sk_canvas_quick_reject(IntPtr ccanvas, SKRect* crect)
	{
		return (sk_canvas_quick_reject_delegate ?? (sk_canvas_quick_reject_delegate = GetSymbol<Delegates.sk_canvas_quick_reject>("sk_canvas_quick_reject")))(ccanvas, crect);
	}

	internal static void sk_canvas_reset_matrix(IntPtr ccanvas)
	{
		(sk_canvas_reset_matrix_delegate ?? (sk_canvas_reset_matrix_delegate = GetSymbol<Delegates.sk_canvas_reset_matrix>("sk_canvas_reset_matrix")))(ccanvas);
	}

	internal static void sk_canvas_restore(IntPtr ccanvas)
	{
		(sk_canvas_restore_delegate ?? (sk_canvas_restore_delegate = GetSymbol<Delegates.sk_canvas_restore>("sk_canvas_restore")))(ccanvas);
	}

	internal static void sk_canvas_restore_to_count(IntPtr ccanvas, int saveCount)
	{
		(sk_canvas_restore_to_count_delegate ?? (sk_canvas_restore_to_count_delegate = GetSymbol<Delegates.sk_canvas_restore_to_count>("sk_canvas_restore_to_count")))(ccanvas, saveCount);
	}

	internal static void sk_canvas_rotate_degrees(IntPtr ccanvas, float degrees)
	{
		(sk_canvas_rotate_degrees_delegate ?? (sk_canvas_rotate_degrees_delegate = GetSymbol<Delegates.sk_canvas_rotate_degrees>("sk_canvas_rotate_degrees")))(ccanvas, degrees);
	}

	internal static void sk_canvas_rotate_radians(IntPtr ccanvas, float radians)
	{
		(sk_canvas_rotate_radians_delegate ?? (sk_canvas_rotate_radians_delegate = GetSymbol<Delegates.sk_canvas_rotate_radians>("sk_canvas_rotate_radians")))(ccanvas, radians);
	}

	internal static int sk_canvas_save(IntPtr ccanvas)
	{
		return (sk_canvas_save_delegate ?? (sk_canvas_save_delegate = GetSymbol<Delegates.sk_canvas_save>("sk_canvas_save")))(ccanvas);
	}

	internal unsafe static int sk_canvas_save_layer(IntPtr ccanvas, SKRect* crect, IntPtr cpaint)
	{
		return (sk_canvas_save_layer_delegate ?? (sk_canvas_save_layer_delegate = GetSymbol<Delegates.sk_canvas_save_layer>("sk_canvas_save_layer")))(ccanvas, crect, cpaint);
	}

	internal unsafe static int sk_canvas_save_layer_rec(IntPtr ccanvas, SKCanvasSaveLayerRecNative* crec)
	{
		return (sk_canvas_save_layer_rec_delegate ?? (sk_canvas_save_layer_rec_delegate = GetSymbol<Delegates.sk_canvas_save_layer_rec>("sk_canvas_save_layer_rec")))(ccanvas, crec);
	}

	internal static void sk_canvas_scale(IntPtr ccanvas, float sx, float sy)
	{
		(sk_canvas_scale_delegate ?? (sk_canvas_scale_delegate = GetSymbol<Delegates.sk_canvas_scale>("sk_canvas_scale")))(ccanvas, sx, sy);
	}

	internal unsafe static void sk_canvas_set_matrix(IntPtr ccanvas, SKMatrix44* cmatrix)
	{
		(sk_canvas_set_matrix_delegate ?? (sk_canvas_set_matrix_delegate = GetSymbol<Delegates.sk_canvas_set_matrix>("sk_canvas_set_matrix")))(ccanvas, cmatrix);
	}

	internal static void sk_canvas_skew(IntPtr ccanvas, float sx, float sy)
	{
		(sk_canvas_skew_delegate ?? (sk_canvas_skew_delegate = GetSymbol<Delegates.sk_canvas_skew>("sk_canvas_skew")))(ccanvas, sx, sy);
	}

	internal static void sk_canvas_translate(IntPtr ccanvas, float dx, float dy)
	{
		(sk_canvas_translate_delegate ?? (sk_canvas_translate_delegate = GetSymbol<Delegates.sk_canvas_translate>("sk_canvas_translate")))(ccanvas, dx, dy);
	}

	internal static IntPtr sk_get_recording_context(IntPtr canvas)
	{
		return (sk_get_recording_context_delegate ?? (sk_get_recording_context_delegate = GetSymbol<Delegates.sk_get_recording_context>("sk_get_recording_context")))(canvas);
	}

	internal static IntPtr sk_get_surface(IntPtr canvas)
	{
		return (sk_get_surface_delegate ?? (sk_get_surface_delegate = GetSymbol<Delegates.sk_get_surface>("sk_get_surface")))(canvas);
	}

	internal static void sk_nodraw_canvas_destroy(IntPtr t)
	{
		(sk_nodraw_canvas_destroy_delegate ?? (sk_nodraw_canvas_destroy_delegate = GetSymbol<Delegates.sk_nodraw_canvas_destroy>("sk_nodraw_canvas_destroy")))(t);
	}

	internal static IntPtr sk_nodraw_canvas_new(int width, int height)
	{
		return (sk_nodraw_canvas_new_delegate ?? (sk_nodraw_canvas_new_delegate = GetSymbol<Delegates.sk_nodraw_canvas_new>("sk_nodraw_canvas_new")))(width, height);
	}

	internal static void sk_nway_canvas_add_canvas(IntPtr t, IntPtr canvas)
	{
		(sk_nway_canvas_add_canvas_delegate ?? (sk_nway_canvas_add_canvas_delegate = GetSymbol<Delegates.sk_nway_canvas_add_canvas>("sk_nway_canvas_add_canvas")))(t, canvas);
	}

	internal static void sk_nway_canvas_destroy(IntPtr t)
	{
		(sk_nway_canvas_destroy_delegate ?? (sk_nway_canvas_destroy_delegate = GetSymbol<Delegates.sk_nway_canvas_destroy>("sk_nway_canvas_destroy")))(t);
	}

	internal static IntPtr sk_nway_canvas_new(int width, int height)
	{
		return (sk_nway_canvas_new_delegate ?? (sk_nway_canvas_new_delegate = GetSymbol<Delegates.sk_nway_canvas_new>("sk_nway_canvas_new")))(width, height);
	}

	internal static void sk_nway_canvas_remove_all(IntPtr t)
	{
		(sk_nway_canvas_remove_all_delegate ?? (sk_nway_canvas_remove_all_delegate = GetSymbol<Delegates.sk_nway_canvas_remove_all>("sk_nway_canvas_remove_all")))(t);
	}

	internal static void sk_nway_canvas_remove_canvas(IntPtr t, IntPtr canvas)
	{
		(sk_nway_canvas_remove_canvas_delegate ?? (sk_nway_canvas_remove_canvas_delegate = GetSymbol<Delegates.sk_nway_canvas_remove_canvas>("sk_nway_canvas_remove_canvas")))(t, canvas);
	}

	internal static void sk_overdraw_canvas_destroy(IntPtr canvas)
	{
		(sk_overdraw_canvas_destroy_delegate ?? (sk_overdraw_canvas_destroy_delegate = GetSymbol<Delegates.sk_overdraw_canvas_destroy>("sk_overdraw_canvas_destroy")))(canvas);
	}

	internal static IntPtr sk_overdraw_canvas_new(IntPtr canvas)
	{
		return (sk_overdraw_canvas_new_delegate ?? (sk_overdraw_canvas_new_delegate = GetSymbol<Delegates.sk_overdraw_canvas_new>("sk_overdraw_canvas_new")))(canvas);
	}

	internal static void sk_codec_destroy(IntPtr codec)
	{
		(sk_codec_destroy_delegate ?? (sk_codec_destroy_delegate = GetSymbol<Delegates.sk_codec_destroy>("sk_codec_destroy")))(codec);
	}

	internal static SKEncodedImageFormat sk_codec_get_encoded_format(IntPtr codec)
	{
		return (sk_codec_get_encoded_format_delegate ?? (sk_codec_get_encoded_format_delegate = GetSymbol<Delegates.sk_codec_get_encoded_format>("sk_codec_get_encoded_format")))(codec);
	}

	internal static int sk_codec_get_frame_count(IntPtr codec)
	{
		return (sk_codec_get_frame_count_delegate ?? (sk_codec_get_frame_count_delegate = GetSymbol<Delegates.sk_codec_get_frame_count>("sk_codec_get_frame_count")))(codec);
	}

	internal unsafe static void sk_codec_get_frame_info(IntPtr codec, SKCodecFrameInfo* frameInfo)
	{
		(sk_codec_get_frame_info_delegate ?? (sk_codec_get_frame_info_delegate = GetSymbol<Delegates.sk_codec_get_frame_info>("sk_codec_get_frame_info")))(codec, frameInfo);
	}

	internal unsafe static bool sk_codec_get_frame_info_for_index(IntPtr codec, int index, SKCodecFrameInfo* frameInfo)
	{
		return (sk_codec_get_frame_info_for_index_delegate ?? (sk_codec_get_frame_info_for_index_delegate = GetSymbol<Delegates.sk_codec_get_frame_info_for_index>("sk_codec_get_frame_info_for_index")))(codec, index, frameInfo);
	}

	internal unsafe static void sk_codec_get_info(IntPtr codec, SKImageInfoNative* info)
	{
		(sk_codec_get_info_delegate ?? (sk_codec_get_info_delegate = GetSymbol<Delegates.sk_codec_get_info>("sk_codec_get_info")))(codec, info);
	}

	internal static SKEncodedOrigin sk_codec_get_origin(IntPtr codec)
	{
		return (sk_codec_get_origin_delegate ?? (sk_codec_get_origin_delegate = GetSymbol<Delegates.sk_codec_get_origin>("sk_codec_get_origin")))(codec);
	}

	internal unsafe static SKCodecResult sk_codec_get_pixels(IntPtr codec, SKImageInfoNative* info, void* pixels, IntPtr rowBytes, SKCodecOptionsInternal* options)
	{
		return (sk_codec_get_pixels_delegate ?? (sk_codec_get_pixels_delegate = GetSymbol<Delegates.sk_codec_get_pixels>("sk_codec_get_pixels")))(codec, info, pixels, rowBytes, options);
	}

	internal static int sk_codec_get_repetition_count(IntPtr codec)
	{
		return (sk_codec_get_repetition_count_delegate ?? (sk_codec_get_repetition_count_delegate = GetSymbol<Delegates.sk_codec_get_repetition_count>("sk_codec_get_repetition_count")))(codec);
	}

	internal unsafe static void sk_codec_get_scaled_dimensions(IntPtr codec, float desiredScale, SKSizeI* dimensions)
	{
		(sk_codec_get_scaled_dimensions_delegate ?? (sk_codec_get_scaled_dimensions_delegate = GetSymbol<Delegates.sk_codec_get_scaled_dimensions>("sk_codec_get_scaled_dimensions")))(codec, desiredScale, dimensions);
	}

	internal static SKCodecScanlineOrder sk_codec_get_scanline_order(IntPtr codec)
	{
		return (sk_codec_get_scanline_order_delegate ?? (sk_codec_get_scanline_order_delegate = GetSymbol<Delegates.sk_codec_get_scanline_order>("sk_codec_get_scanline_order")))(codec);
	}

	internal unsafe static int sk_codec_get_scanlines(IntPtr codec, void* dst, int countLines, IntPtr rowBytes)
	{
		return (sk_codec_get_scanlines_delegate ?? (sk_codec_get_scanlines_delegate = GetSymbol<Delegates.sk_codec_get_scanlines>("sk_codec_get_scanlines")))(codec, dst, countLines, rowBytes);
	}

	internal unsafe static bool sk_codec_get_valid_subset(IntPtr codec, SKRectI* desiredSubset)
	{
		return (sk_codec_get_valid_subset_delegate ?? (sk_codec_get_valid_subset_delegate = GetSymbol<Delegates.sk_codec_get_valid_subset>("sk_codec_get_valid_subset")))(codec, desiredSubset);
	}

	internal unsafe static SKCodecResult sk_codec_incremental_decode(IntPtr codec, int* rowsDecoded)
	{
		return (sk_codec_incremental_decode_delegate ?? (sk_codec_incremental_decode_delegate = GetSymbol<Delegates.sk_codec_incremental_decode>("sk_codec_incremental_decode")))(codec, rowsDecoded);
	}

	internal static IntPtr sk_codec_min_buffered_bytes_needed()
	{
		return (sk_codec_min_buffered_bytes_needed_delegate ?? (sk_codec_min_buffered_bytes_needed_delegate = GetSymbol<Delegates.sk_codec_min_buffered_bytes_needed>("sk_codec_min_buffered_bytes_needed")))();
	}

	internal static IntPtr sk_codec_new_from_data(IntPtr data)
	{
		return (sk_codec_new_from_data_delegate ?? (sk_codec_new_from_data_delegate = GetSymbol<Delegates.sk_codec_new_from_data>("sk_codec_new_from_data")))(data);
	}

	internal unsafe static IntPtr sk_codec_new_from_stream(IntPtr stream, SKCodecResult* result)
	{
		return (sk_codec_new_from_stream_delegate ?? (sk_codec_new_from_stream_delegate = GetSymbol<Delegates.sk_codec_new_from_stream>("sk_codec_new_from_stream")))(stream, result);
	}

	internal static int sk_codec_next_scanline(IntPtr codec)
	{
		return (sk_codec_next_scanline_delegate ?? (sk_codec_next_scanline_delegate = GetSymbol<Delegates.sk_codec_next_scanline>("sk_codec_next_scanline")))(codec);
	}

	internal static int sk_codec_output_scanline(IntPtr codec, int inputScanline)
	{
		return (sk_codec_output_scanline_delegate ?? (sk_codec_output_scanline_delegate = GetSymbol<Delegates.sk_codec_output_scanline>("sk_codec_output_scanline")))(codec, inputScanline);
	}

	internal static bool sk_codec_skip_scanlines(IntPtr codec, int countLines)
	{
		return (sk_codec_skip_scanlines_delegate ?? (sk_codec_skip_scanlines_delegate = GetSymbol<Delegates.sk_codec_skip_scanlines>("sk_codec_skip_scanlines")))(codec, countLines);
	}

	internal unsafe static SKCodecResult sk_codec_start_incremental_decode(IntPtr codec, SKImageInfoNative* info, void* pixels, IntPtr rowBytes, SKCodecOptionsInternal* options)
	{
		return (sk_codec_start_incremental_decode_delegate ?? (sk_codec_start_incremental_decode_delegate = GetSymbol<Delegates.sk_codec_start_incremental_decode>("sk_codec_start_incremental_decode")))(codec, info, pixels, rowBytes, options);
	}

	internal unsafe static SKCodecResult sk_codec_start_scanline_decode(IntPtr codec, SKImageInfoNative* info, SKCodecOptionsInternal* options)
	{
		return (sk_codec_start_scanline_decode_delegate ?? (sk_codec_start_scanline_decode_delegate = GetSymbol<Delegates.sk_codec_start_scanline_decode>("sk_codec_start_scanline_decode")))(codec, info, options);
	}

	internal unsafe static IntPtr sk_colorfilter_new_color_matrix(float* array)
	{
		return (sk_colorfilter_new_color_matrix_delegate ?? (sk_colorfilter_new_color_matrix_delegate = GetSymbol<Delegates.sk_colorfilter_new_color_matrix>("sk_colorfilter_new_color_matrix")))(array);
	}

	internal static IntPtr sk_colorfilter_new_compose(IntPtr outer, IntPtr inner)
	{
		return (sk_colorfilter_new_compose_delegate ?? (sk_colorfilter_new_compose_delegate = GetSymbol<Delegates.sk_colorfilter_new_compose>("sk_colorfilter_new_compose")))(outer, inner);
	}

	internal unsafe static IntPtr sk_colorfilter_new_high_contrast(SKHighContrastConfig* config)
	{
		return (sk_colorfilter_new_high_contrast_delegate ?? (sk_colorfilter_new_high_contrast_delegate = GetSymbol<Delegates.sk_colorfilter_new_high_contrast>("sk_colorfilter_new_high_contrast")))(config);
	}

	internal unsafe static IntPtr sk_colorfilter_new_hsla_matrix(float* array)
	{
		return (sk_colorfilter_new_hsla_matrix_delegate ?? (sk_colorfilter_new_hsla_matrix_delegate = GetSymbol<Delegates.sk_colorfilter_new_hsla_matrix>("sk_colorfilter_new_hsla_matrix")))(array);
	}

	internal static IntPtr sk_colorfilter_new_lerp(float weight, IntPtr filter0, IntPtr filter1)
	{
		return (sk_colorfilter_new_lerp_delegate ?? (sk_colorfilter_new_lerp_delegate = GetSymbol<Delegates.sk_colorfilter_new_lerp>("sk_colorfilter_new_lerp")))(weight, filter0, filter1);
	}

	internal static IntPtr sk_colorfilter_new_lighting(uint mul, uint add)
	{
		return (sk_colorfilter_new_lighting_delegate ?? (sk_colorfilter_new_lighting_delegate = GetSymbol<Delegates.sk_colorfilter_new_lighting>("sk_colorfilter_new_lighting")))(mul, add);
	}

	internal static IntPtr sk_colorfilter_new_linear_to_srgb_gamma()
	{
		return (sk_colorfilter_new_linear_to_srgb_gamma_delegate ?? (sk_colorfilter_new_linear_to_srgb_gamma_delegate = GetSymbol<Delegates.sk_colorfilter_new_linear_to_srgb_gamma>("sk_colorfilter_new_linear_to_srgb_gamma")))();
	}

	internal static IntPtr sk_colorfilter_new_luma_color()
	{
		return (sk_colorfilter_new_luma_color_delegate ?? (sk_colorfilter_new_luma_color_delegate = GetSymbol<Delegates.sk_colorfilter_new_luma_color>("sk_colorfilter_new_luma_color")))();
	}

	internal static IntPtr sk_colorfilter_new_mode(uint c, SKBlendMode mode)
	{
		return (sk_colorfilter_new_mode_delegate ?? (sk_colorfilter_new_mode_delegate = GetSymbol<Delegates.sk_colorfilter_new_mode>("sk_colorfilter_new_mode")))(c, mode);
	}

	internal static IntPtr sk_colorfilter_new_srgb_to_linear_gamma()
	{
		return (sk_colorfilter_new_srgb_to_linear_gamma_delegate ?? (sk_colorfilter_new_srgb_to_linear_gamma_delegate = GetSymbol<Delegates.sk_colorfilter_new_srgb_to_linear_gamma>("sk_colorfilter_new_srgb_to_linear_gamma")))();
	}

	internal unsafe static IntPtr sk_colorfilter_new_table(byte* table)
	{
		return (sk_colorfilter_new_table_delegate ?? (sk_colorfilter_new_table_delegate = GetSymbol<Delegates.sk_colorfilter_new_table>("sk_colorfilter_new_table")))(table);
	}

	internal unsafe static IntPtr sk_colorfilter_new_table_argb(byte* tableA, byte* tableR, byte* tableG, byte* tableB)
	{
		return (sk_colorfilter_new_table_argb_delegate ?? (sk_colorfilter_new_table_argb_delegate = GetSymbol<Delegates.sk_colorfilter_new_table_argb>("sk_colorfilter_new_table_argb")))(tableA, tableR, tableG, tableB);
	}

	internal static void sk_colorfilter_unref(IntPtr filter)
	{
		(sk_colorfilter_unref_delegate ?? (sk_colorfilter_unref_delegate = GetSymbol<Delegates.sk_colorfilter_unref>("sk_colorfilter_unref")))(filter);
	}

	internal unsafe static void sk_color4f_from_color(uint color, SKColorF* color4f)
	{
		(sk_color4f_from_color_delegate ?? (sk_color4f_from_color_delegate = GetSymbol<Delegates.sk_color4f_from_color>("sk_color4f_from_color")))(color, color4f);
	}

	internal unsafe static uint sk_color4f_to_color(SKColorF* color4f)
	{
		return (sk_color4f_to_color_delegate ?? (sk_color4f_to_color_delegate = GetSymbol<Delegates.sk_color4f_to_color>("sk_color4f_to_color")))(color4f);
	}

	internal static bool sk_colorspace_equals(IntPtr src, IntPtr dst)
	{
		return (sk_colorspace_equals_delegate ?? (sk_colorspace_equals_delegate = GetSymbol<Delegates.sk_colorspace_equals>("sk_colorspace_equals")))(src, dst);
	}

	internal static bool sk_colorspace_gamma_close_to_srgb(IntPtr colorspace)
	{
		return (sk_colorspace_gamma_close_to_srgb_delegate ?? (sk_colorspace_gamma_close_to_srgb_delegate = GetSymbol<Delegates.sk_colorspace_gamma_close_to_srgb>("sk_colorspace_gamma_close_to_srgb")))(colorspace);
	}

	internal static bool sk_colorspace_gamma_is_linear(IntPtr colorspace)
	{
		return (sk_colorspace_gamma_is_linear_delegate ?? (sk_colorspace_gamma_is_linear_delegate = GetSymbol<Delegates.sk_colorspace_gamma_is_linear>("sk_colorspace_gamma_is_linear")))(colorspace);
	}

	internal static void sk_colorspace_icc_profile_delete(IntPtr profile)
	{
		(sk_colorspace_icc_profile_delete_delegate ?? (sk_colorspace_icc_profile_delete_delegate = GetSymbol<Delegates.sk_colorspace_icc_profile_delete>("sk_colorspace_icc_profile_delete")))(profile);
	}

	internal unsafe static byte* sk_colorspace_icc_profile_get_buffer(IntPtr profile, uint* size)
	{
		return (sk_colorspace_icc_profile_get_buffer_delegate ?? (sk_colorspace_icc_profile_get_buffer_delegate = GetSymbol<Delegates.sk_colorspace_icc_profile_get_buffer>("sk_colorspace_icc_profile_get_buffer")))(profile, size);
	}

	internal unsafe static bool sk_colorspace_icc_profile_get_to_xyzd50(IntPtr profile, SKColorSpaceXyz* toXYZD50)
	{
		return (sk_colorspace_icc_profile_get_to_xyzd50_delegate ?? (sk_colorspace_icc_profile_get_to_xyzd50_delegate = GetSymbol<Delegates.sk_colorspace_icc_profile_get_to_xyzd50>("sk_colorspace_icc_profile_get_to_xyzd50")))(profile, toXYZD50);
	}

	internal static IntPtr sk_colorspace_icc_profile_new()
	{
		return (sk_colorspace_icc_profile_new_delegate ?? (sk_colorspace_icc_profile_new_delegate = GetSymbol<Delegates.sk_colorspace_icc_profile_new>("sk_colorspace_icc_profile_new")))();
	}

	internal unsafe static bool sk_colorspace_icc_profile_parse(void* buffer, IntPtr length, IntPtr profile)
	{
		return (sk_colorspace_icc_profile_parse_delegate ?? (sk_colorspace_icc_profile_parse_delegate = GetSymbol<Delegates.sk_colorspace_icc_profile_parse>("sk_colorspace_icc_profile_parse")))(buffer, length, profile);
	}

	internal unsafe static bool sk_colorspace_is_numerical_transfer_fn(IntPtr colorspace, SKColorSpaceTransferFn* transferFn)
	{
		return (sk_colorspace_is_numerical_transfer_fn_delegate ?? (sk_colorspace_is_numerical_transfer_fn_delegate = GetSymbol<Delegates.sk_colorspace_is_numerical_transfer_fn>("sk_colorspace_is_numerical_transfer_fn")))(colorspace, transferFn);
	}

	internal static bool sk_colorspace_is_srgb(IntPtr colorspace)
	{
		return (sk_colorspace_is_srgb_delegate ?? (sk_colorspace_is_srgb_delegate = GetSymbol<Delegates.sk_colorspace_is_srgb>("sk_colorspace_is_srgb")))(colorspace);
	}

	internal static IntPtr sk_colorspace_make_linear_gamma(IntPtr colorspace)
	{
		return (sk_colorspace_make_linear_gamma_delegate ?? (sk_colorspace_make_linear_gamma_delegate = GetSymbol<Delegates.sk_colorspace_make_linear_gamma>("sk_colorspace_make_linear_gamma")))(colorspace);
	}

	internal static IntPtr sk_colorspace_make_srgb_gamma(IntPtr colorspace)
	{
		return (sk_colorspace_make_srgb_gamma_delegate ?? (sk_colorspace_make_srgb_gamma_delegate = GetSymbol<Delegates.sk_colorspace_make_srgb_gamma>("sk_colorspace_make_srgb_gamma")))(colorspace);
	}

	internal static IntPtr sk_colorspace_new_icc(IntPtr profile)
	{
		return (sk_colorspace_new_icc_delegate ?? (sk_colorspace_new_icc_delegate = GetSymbol<Delegates.sk_colorspace_new_icc>("sk_colorspace_new_icc")))(profile);
	}

	internal unsafe static IntPtr sk_colorspace_new_rgb(SKColorSpaceTransferFn* transferFn, SKColorSpaceXyz* toXYZD50)
	{
		return (sk_colorspace_new_rgb_delegate ?? (sk_colorspace_new_rgb_delegate = GetSymbol<Delegates.sk_colorspace_new_rgb>("sk_colorspace_new_rgb")))(transferFn, toXYZD50);
	}

	internal static IntPtr sk_colorspace_new_srgb()
	{
		return (sk_colorspace_new_srgb_delegate ?? (sk_colorspace_new_srgb_delegate = GetSymbol<Delegates.sk_colorspace_new_srgb>("sk_colorspace_new_srgb")))();
	}

	internal static IntPtr sk_colorspace_new_srgb_linear()
	{
		return (sk_colorspace_new_srgb_linear_delegate ?? (sk_colorspace_new_srgb_linear_delegate = GetSymbol<Delegates.sk_colorspace_new_srgb_linear>("sk_colorspace_new_srgb_linear")))();
	}

	internal unsafe static bool sk_colorspace_primaries_to_xyzd50(SKColorSpacePrimaries* primaries, SKColorSpaceXyz* toXYZD50)
	{
		return (sk_colorspace_primaries_to_xyzd50_delegate ?? (sk_colorspace_primaries_to_xyzd50_delegate = GetSymbol<Delegates.sk_colorspace_primaries_to_xyzd50>("sk_colorspace_primaries_to_xyzd50")))(primaries, toXYZD50);
	}

	internal static void sk_colorspace_ref(IntPtr colorspace)
	{
		(sk_colorspace_ref_delegate ?? (sk_colorspace_ref_delegate = GetSymbol<Delegates.sk_colorspace_ref>("sk_colorspace_ref")))(colorspace);
	}

	internal static void sk_colorspace_to_profile(IntPtr colorspace, IntPtr profile)
	{
		(sk_colorspace_to_profile_delegate ?? (sk_colorspace_to_profile_delegate = GetSymbol<Delegates.sk_colorspace_to_profile>("sk_colorspace_to_profile")))(colorspace, profile);
	}

	internal unsafe static bool sk_colorspace_to_xyzd50(IntPtr colorspace, SKColorSpaceXyz* toXYZD50)
	{
		return (sk_colorspace_to_xyzd50_delegate ?? (sk_colorspace_to_xyzd50_delegate = GetSymbol<Delegates.sk_colorspace_to_xyzd50>("sk_colorspace_to_xyzd50")))(colorspace, toXYZD50);
	}

	internal unsafe static float sk_colorspace_transfer_fn_eval(SKColorSpaceTransferFn* transferFn, float x)
	{
		return (sk_colorspace_transfer_fn_eval_delegate ?? (sk_colorspace_transfer_fn_eval_delegate = GetSymbol<Delegates.sk_colorspace_transfer_fn_eval>("sk_colorspace_transfer_fn_eval")))(transferFn, x);
	}

	internal unsafe static bool sk_colorspace_transfer_fn_invert(SKColorSpaceTransferFn* src, SKColorSpaceTransferFn* dst)
	{
		return (sk_colorspace_transfer_fn_invert_delegate ?? (sk_colorspace_transfer_fn_invert_delegate = GetSymbol<Delegates.sk_colorspace_transfer_fn_invert>("sk_colorspace_transfer_fn_invert")))(src, dst);
	}

	internal unsafe static void sk_colorspace_transfer_fn_named_2dot2(SKColorSpaceTransferFn* transferFn)
	{
		(sk_colorspace_transfer_fn_named_2dot2_delegate ?? (sk_colorspace_transfer_fn_named_2dot2_delegate = GetSymbol<Delegates.sk_colorspace_transfer_fn_named_2dot2>("sk_colorspace_transfer_fn_named_2dot2")))(transferFn);
	}

	internal unsafe static void sk_colorspace_transfer_fn_named_hlg(SKColorSpaceTransferFn* transferFn)
	{
		(sk_colorspace_transfer_fn_named_hlg_delegate ?? (sk_colorspace_transfer_fn_named_hlg_delegate = GetSymbol<Delegates.sk_colorspace_transfer_fn_named_hlg>("sk_colorspace_transfer_fn_named_hlg")))(transferFn);
	}

	internal unsafe static void sk_colorspace_transfer_fn_named_linear(SKColorSpaceTransferFn* transferFn)
	{
		(sk_colorspace_transfer_fn_named_linear_delegate ?? (sk_colorspace_transfer_fn_named_linear_delegate = GetSymbol<Delegates.sk_colorspace_transfer_fn_named_linear>("sk_colorspace_transfer_fn_named_linear")))(transferFn);
	}

	internal unsafe static void sk_colorspace_transfer_fn_named_pq(SKColorSpaceTransferFn* transferFn)
	{
		(sk_colorspace_transfer_fn_named_pq_delegate ?? (sk_colorspace_transfer_fn_named_pq_delegate = GetSymbol<Delegates.sk_colorspace_transfer_fn_named_pq>("sk_colorspace_transfer_fn_named_pq")))(transferFn);
	}

	internal unsafe static void sk_colorspace_transfer_fn_named_rec2020(SKColorSpaceTransferFn* transferFn)
	{
		(sk_colorspace_transfer_fn_named_rec2020_delegate ?? (sk_colorspace_transfer_fn_named_rec2020_delegate = GetSymbol<Delegates.sk_colorspace_transfer_fn_named_rec2020>("sk_colorspace_transfer_fn_named_rec2020")))(transferFn);
	}

	internal unsafe static void sk_colorspace_transfer_fn_named_srgb(SKColorSpaceTransferFn* transferFn)
	{
		(sk_colorspace_transfer_fn_named_srgb_delegate ?? (sk_colorspace_transfer_fn_named_srgb_delegate = GetSymbol<Delegates.sk_colorspace_transfer_fn_named_srgb>("sk_colorspace_transfer_fn_named_srgb")))(transferFn);
	}

	internal static void sk_colorspace_unref(IntPtr colorspace)
	{
		(sk_colorspace_unref_delegate ?? (sk_colorspace_unref_delegate = GetSymbol<Delegates.sk_colorspace_unref>("sk_colorspace_unref")))(colorspace);
	}

	internal unsafe static void sk_colorspace_xyz_concat(SKColorSpaceXyz* a, SKColorSpaceXyz* b, SKColorSpaceXyz* result)
	{
		(sk_colorspace_xyz_concat_delegate ?? (sk_colorspace_xyz_concat_delegate = GetSymbol<Delegates.sk_colorspace_xyz_concat>("sk_colorspace_xyz_concat")))(a, b, result);
	}

	internal unsafe static bool sk_colorspace_xyz_invert(SKColorSpaceXyz* src, SKColorSpaceXyz* dst)
	{
		return (sk_colorspace_xyz_invert_delegate ?? (sk_colorspace_xyz_invert_delegate = GetSymbol<Delegates.sk_colorspace_xyz_invert>("sk_colorspace_xyz_invert")))(src, dst);
	}

	internal unsafe static void sk_colorspace_xyz_named_adobe_rgb(SKColorSpaceXyz* xyz)
	{
		(sk_colorspace_xyz_named_adobe_rgb_delegate ?? (sk_colorspace_xyz_named_adobe_rgb_delegate = GetSymbol<Delegates.sk_colorspace_xyz_named_adobe_rgb>("sk_colorspace_xyz_named_adobe_rgb")))(xyz);
	}

	internal unsafe static void sk_colorspace_xyz_named_display_p3(SKColorSpaceXyz* xyz)
	{
		(sk_colorspace_xyz_named_display_p3_delegate ?? (sk_colorspace_xyz_named_display_p3_delegate = GetSymbol<Delegates.sk_colorspace_xyz_named_display_p3>("sk_colorspace_xyz_named_display_p3")))(xyz);
	}

	internal unsafe static void sk_colorspace_xyz_named_rec2020(SKColorSpaceXyz* xyz)
	{
		(sk_colorspace_xyz_named_rec2020_delegate ?? (sk_colorspace_xyz_named_rec2020_delegate = GetSymbol<Delegates.sk_colorspace_xyz_named_rec2020>("sk_colorspace_xyz_named_rec2020")))(xyz);
	}

	internal unsafe static void sk_colorspace_xyz_named_srgb(SKColorSpaceXyz* xyz)
	{
		(sk_colorspace_xyz_named_srgb_delegate ?? (sk_colorspace_xyz_named_srgb_delegate = GetSymbol<Delegates.sk_colorspace_xyz_named_srgb>("sk_colorspace_xyz_named_srgb")))(xyz);
	}

	internal unsafe static void sk_colorspace_xyz_named_xyz(SKColorSpaceXyz* xyz)
	{
		(sk_colorspace_xyz_named_xyz_delegate ?? (sk_colorspace_xyz_named_xyz_delegate = GetSymbol<Delegates.sk_colorspace_xyz_named_xyz>("sk_colorspace_xyz_named_xyz")))(xyz);
	}

	internal unsafe static byte* sk_data_get_bytes(IntPtr param0)
	{
		return (sk_data_get_bytes_delegate ?? (sk_data_get_bytes_delegate = GetSymbol<Delegates.sk_data_get_bytes>("sk_data_get_bytes")))(param0);
	}

	internal unsafe static void* sk_data_get_data(IntPtr param0)
	{
		return (sk_data_get_data_delegate ?? (sk_data_get_data_delegate = GetSymbol<Delegates.sk_data_get_data>("sk_data_get_data")))(param0);
	}

	internal static IntPtr sk_data_get_size(IntPtr param0)
	{
		return (sk_data_get_size_delegate ?? (sk_data_get_size_delegate = GetSymbol<Delegates.sk_data_get_size>("sk_data_get_size")))(param0);
	}

	internal static IntPtr sk_data_new_empty()
	{
		return (sk_data_new_empty_delegate ?? (sk_data_new_empty_delegate = GetSymbol<Delegates.sk_data_new_empty>("sk_data_new_empty")))();
	}

	internal unsafe static IntPtr sk_data_new_from_file(void* path)
	{
		return (sk_data_new_from_file_delegate ?? (sk_data_new_from_file_delegate = GetSymbol<Delegates.sk_data_new_from_file>("sk_data_new_from_file")))(path);
	}

	internal static IntPtr sk_data_new_from_stream(IntPtr stream, IntPtr length)
	{
		return (sk_data_new_from_stream_delegate ?? (sk_data_new_from_stream_delegate = GetSymbol<Delegates.sk_data_new_from_stream>("sk_data_new_from_stream")))(stream, length);
	}

	internal static IntPtr sk_data_new_subset(IntPtr src, IntPtr offset, IntPtr length)
	{
		return (sk_data_new_subset_delegate ?? (sk_data_new_subset_delegate = GetSymbol<Delegates.sk_data_new_subset>("sk_data_new_subset")))(src, offset, length);
	}

	internal static IntPtr sk_data_new_uninitialized(IntPtr size)
	{
		return (sk_data_new_uninitialized_delegate ?? (sk_data_new_uninitialized_delegate = GetSymbol<Delegates.sk_data_new_uninitialized>("sk_data_new_uninitialized")))(size);
	}

	internal unsafe static IntPtr sk_data_new_with_copy(void* src, IntPtr length)
	{
		return (sk_data_new_with_copy_delegate ?? (sk_data_new_with_copy_delegate = GetSymbol<Delegates.sk_data_new_with_copy>("sk_data_new_with_copy")))(src, length);
	}

	internal unsafe static IntPtr sk_data_new_with_proc(void* ptr, IntPtr length, SKDataReleaseProxyDelegate proc, void* ctx)
	{
		return (sk_data_new_with_proc_delegate ?? (sk_data_new_with_proc_delegate = GetSymbol<Delegates.sk_data_new_with_proc>("sk_data_new_with_proc")))(ptr, length, proc, ctx);
	}

	internal static void sk_data_ref(IntPtr param0)
	{
		(sk_data_ref_delegate ?? (sk_data_ref_delegate = GetSymbol<Delegates.sk_data_ref>("sk_data_ref")))(param0);
	}

	internal static void sk_data_unref(IntPtr param0)
	{
		(sk_data_unref_delegate ?? (sk_data_unref_delegate = GetSymbol<Delegates.sk_data_unref>("sk_data_unref")))(param0);
	}

	internal static void sk_document_abort(IntPtr document)
	{
		(sk_document_abort_delegate ?? (sk_document_abort_delegate = GetSymbol<Delegates.sk_document_abort>("sk_document_abort")))(document);
	}

	internal unsafe static IntPtr sk_document_begin_page(IntPtr document, float width, float height, SKRect* content)
	{
		return (sk_document_begin_page_delegate ?? (sk_document_begin_page_delegate = GetSymbol<Delegates.sk_document_begin_page>("sk_document_begin_page")))(document, width, height, content);
	}

	internal static void sk_document_close(IntPtr document)
	{
		(sk_document_close_delegate ?? (sk_document_close_delegate = GetSymbol<Delegates.sk_document_close>("sk_document_close")))(document);
	}

	internal static IntPtr sk_document_create_pdf_from_stream(IntPtr stream)
	{
		return (sk_document_create_pdf_from_stream_delegate ?? (sk_document_create_pdf_from_stream_delegate = GetSymbol<Delegates.sk_document_create_pdf_from_stream>("sk_document_create_pdf_from_stream")))(stream);
	}

	internal unsafe static IntPtr sk_document_create_pdf_from_stream_with_metadata(IntPtr stream, SKDocumentPdfMetadataInternal* metadata)
	{
		return (sk_document_create_pdf_from_stream_with_metadata_delegate ?? (sk_document_create_pdf_from_stream_with_metadata_delegate = GetSymbol<Delegates.sk_document_create_pdf_from_stream_with_metadata>("sk_document_create_pdf_from_stream_with_metadata")))(stream, metadata);
	}

	internal static IntPtr sk_document_create_xps_from_stream(IntPtr stream, float dpi)
	{
		return (sk_document_create_xps_from_stream_delegate ?? (sk_document_create_xps_from_stream_delegate = GetSymbol<Delegates.sk_document_create_xps_from_stream>("sk_document_create_xps_from_stream")))(stream, dpi);
	}

	internal static void sk_document_end_page(IntPtr document)
	{
		(sk_document_end_page_delegate ?? (sk_document_end_page_delegate = GetSymbol<Delegates.sk_document_end_page>("sk_document_end_page")))(document);
	}

	internal static void sk_document_unref(IntPtr document)
	{
		(sk_document_unref_delegate ?? (sk_document_unref_delegate = GetSymbol<Delegates.sk_document_unref>("sk_document_unref")))(document);
	}

	internal static IntPtr sk_drawable_approximate_bytes_used(IntPtr param0)
	{
		return (sk_drawable_approximate_bytes_used_delegate ?? (sk_drawable_approximate_bytes_used_delegate = GetSymbol<Delegates.sk_drawable_approximate_bytes_used>("sk_drawable_approximate_bytes_used")))(param0);
	}

	internal unsafe static void sk_drawable_draw(IntPtr param0, IntPtr param1, SKMatrix* param2)
	{
		(sk_drawable_draw_delegate ?? (sk_drawable_draw_delegate = GetSymbol<Delegates.sk_drawable_draw>("sk_drawable_draw")))(param0, param1, param2);
	}

	internal unsafe static void sk_drawable_get_bounds(IntPtr param0, SKRect* param1)
	{
		(sk_drawable_get_bounds_delegate ?? (sk_drawable_get_bounds_delegate = GetSymbol<Delegates.sk_drawable_get_bounds>("sk_drawable_get_bounds")))(param0, param1);
	}

	internal static uint sk_drawable_get_generation_id(IntPtr param0)
	{
		return (sk_drawable_get_generation_id_delegate ?? (sk_drawable_get_generation_id_delegate = GetSymbol<Delegates.sk_drawable_get_generation_id>("sk_drawable_get_generation_id")))(param0);
	}

	internal static IntPtr sk_drawable_new_picture_snapshot(IntPtr param0)
	{
		return (sk_drawable_new_picture_snapshot_delegate ?? (sk_drawable_new_picture_snapshot_delegate = GetSymbol<Delegates.sk_drawable_new_picture_snapshot>("sk_drawable_new_picture_snapshot")))(param0);
	}

	internal static void sk_drawable_notify_drawing_changed(IntPtr param0)
	{
		(sk_drawable_notify_drawing_changed_delegate ?? (sk_drawable_notify_drawing_changed_delegate = GetSymbol<Delegates.sk_drawable_notify_drawing_changed>("sk_drawable_notify_drawing_changed")))(param0);
	}

	internal static void sk_drawable_unref(IntPtr param0)
	{
		(sk_drawable_unref_delegate ?? (sk_drawable_unref_delegate = GetSymbol<Delegates.sk_drawable_unref>("sk_drawable_unref")))(param0);
	}

	internal unsafe static IntPtr sk_font_break_text(IntPtr font, void* text, IntPtr byteLength, SKTextEncoding encoding, float maxWidth, float* measuredWidth, IntPtr paint)
	{
		return (sk_font_break_text_delegate ?? (sk_font_break_text_delegate = GetSymbol<Delegates.sk_font_break_text>("sk_font_break_text")))(font, text, byteLength, encoding, maxWidth, measuredWidth, paint);
	}

	internal static void sk_font_delete(IntPtr font)
	{
		(sk_font_delete_delegate ?? (sk_font_delete_delegate = GetSymbol<Delegates.sk_font_delete>("sk_font_delete")))(font);
	}

	internal static SKFontEdging sk_font_get_edging(IntPtr font)
	{
		return (sk_font_get_edging_delegate ?? (sk_font_get_edging_delegate = GetSymbol<Delegates.sk_font_get_edging>("sk_font_get_edging")))(font);
	}

	internal static SKFontHinting sk_font_get_hinting(IntPtr font)
	{
		return (sk_font_get_hinting_delegate ?? (sk_font_get_hinting_delegate = GetSymbol<Delegates.sk_font_get_hinting>("sk_font_get_hinting")))(font);
	}

	internal unsafe static float sk_font_get_metrics(IntPtr font, SKFontMetrics* metrics)
	{
		return (sk_font_get_metrics_delegate ?? (sk_font_get_metrics_delegate = GetSymbol<Delegates.sk_font_get_metrics>("sk_font_get_metrics")))(font, metrics);
	}

	internal static bool sk_font_get_path(IntPtr font, ushort glyph, IntPtr path)
	{
		return (sk_font_get_path_delegate ?? (sk_font_get_path_delegate = GetSymbol<Delegates.sk_font_get_path>("sk_font_get_path")))(font, glyph, path);
	}

	internal unsafe static void sk_font_get_paths(IntPtr font, ushort* glyphs, int count, SKGlyphPathProxyDelegate glyphPathProc, void* context)
	{
		(sk_font_get_paths_delegate ?? (sk_font_get_paths_delegate = GetSymbol<Delegates.sk_font_get_paths>("sk_font_get_paths")))(font, glyphs, count, glyphPathProc, context);
	}

	internal unsafe static void sk_font_get_pos(IntPtr font, ushort* glyphs, int count, SKPoint* pos, SKPoint* origin)
	{
		(sk_font_get_pos_delegate ?? (sk_font_get_pos_delegate = GetSymbol<Delegates.sk_font_get_pos>("sk_font_get_pos")))(font, glyphs, count, pos, origin);
	}

	internal static float sk_font_get_scale_x(IntPtr font)
	{
		return (sk_font_get_scale_x_delegate ?? (sk_font_get_scale_x_delegate = GetSymbol<Delegates.sk_font_get_scale_x>("sk_font_get_scale_x")))(font);
	}

	internal static float sk_font_get_size(IntPtr font)
	{
		return (sk_font_get_size_delegate ?? (sk_font_get_size_delegate = GetSymbol<Delegates.sk_font_get_size>("sk_font_get_size")))(font);
	}

	internal static float sk_font_get_skew_x(IntPtr font)
	{
		return (sk_font_get_skew_x_delegate ?? (sk_font_get_skew_x_delegate = GetSymbol<Delegates.sk_font_get_skew_x>("sk_font_get_skew_x")))(font);
	}

	internal static IntPtr sk_font_get_typeface(IntPtr font)
	{
		return (sk_font_get_typeface_delegate ?? (sk_font_get_typeface_delegate = GetSymbol<Delegates.sk_font_get_typeface>("sk_font_get_typeface")))(font);
	}

	internal unsafe static void sk_font_get_widths_bounds(IntPtr font, ushort* glyphs, int count, float* widths, SKRect* bounds, IntPtr paint)
	{
		(sk_font_get_widths_bounds_delegate ?? (sk_font_get_widths_bounds_delegate = GetSymbol<Delegates.sk_font_get_widths_bounds>("sk_font_get_widths_bounds")))(font, glyphs, count, widths, bounds, paint);
	}

	internal unsafe static void sk_font_get_xpos(IntPtr font, ushort* glyphs, int count, float* xpos, float origin)
	{
		(sk_font_get_xpos_delegate ?? (sk_font_get_xpos_delegate = GetSymbol<Delegates.sk_font_get_xpos>("sk_font_get_xpos")))(font, glyphs, count, xpos, origin);
	}

	internal static bool sk_font_is_baseline_snap(IntPtr font)
	{
		return (sk_font_is_baseline_snap_delegate ?? (sk_font_is_baseline_snap_delegate = GetSymbol<Delegates.sk_font_is_baseline_snap>("sk_font_is_baseline_snap")))(font);
	}

	internal static bool sk_font_is_embedded_bitmaps(IntPtr font)
	{
		return (sk_font_is_embedded_bitmaps_delegate ?? (sk_font_is_embedded_bitmaps_delegate = GetSymbol<Delegates.sk_font_is_embedded_bitmaps>("sk_font_is_embedded_bitmaps")))(font);
	}

	internal static bool sk_font_is_embolden(IntPtr font)
	{
		return (sk_font_is_embolden_delegate ?? (sk_font_is_embolden_delegate = GetSymbol<Delegates.sk_font_is_embolden>("sk_font_is_embolden")))(font);
	}

	internal static bool sk_font_is_force_auto_hinting(IntPtr font)
	{
		return (sk_font_is_force_auto_hinting_delegate ?? (sk_font_is_force_auto_hinting_delegate = GetSymbol<Delegates.sk_font_is_force_auto_hinting>("sk_font_is_force_auto_hinting")))(font);
	}

	internal static bool sk_font_is_linear_metrics(IntPtr font)
	{
		return (sk_font_is_linear_metrics_delegate ?? (sk_font_is_linear_metrics_delegate = GetSymbol<Delegates.sk_font_is_linear_metrics>("sk_font_is_linear_metrics")))(font);
	}

	internal static bool sk_font_is_subpixel(IntPtr font)
	{
		return (sk_font_is_subpixel_delegate ?? (sk_font_is_subpixel_delegate = GetSymbol<Delegates.sk_font_is_subpixel>("sk_font_is_subpixel")))(font);
	}

	internal unsafe static float sk_font_measure_text(IntPtr font, void* text, IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, IntPtr paint)
	{
		return (sk_font_measure_text_delegate ?? (sk_font_measure_text_delegate = GetSymbol<Delegates.sk_font_measure_text>("sk_font_measure_text")))(font, text, byteLength, encoding, bounds, paint);
	}

	internal unsafe static void sk_font_measure_text_no_return(IntPtr font, void* text, IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, IntPtr paint, float* measuredWidth)
	{
		(sk_font_measure_text_no_return_delegate ?? (sk_font_measure_text_no_return_delegate = GetSymbol<Delegates.sk_font_measure_text_no_return>("sk_font_measure_text_no_return")))(font, text, byteLength, encoding, bounds, paint, measuredWidth);
	}

	internal static IntPtr sk_font_new()
	{
		return (sk_font_new_delegate ?? (sk_font_new_delegate = GetSymbol<Delegates.sk_font_new>("sk_font_new")))();
	}

	internal static IntPtr sk_font_new_with_values(IntPtr typeface, float size, float scaleX, float skewX)
	{
		return (sk_font_new_with_values_delegate ?? (sk_font_new_with_values_delegate = GetSymbol<Delegates.sk_font_new_with_values>("sk_font_new_with_values")))(typeface, size, scaleX, skewX);
	}

	internal static void sk_font_set_baseline_snap(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value)
	{
		(sk_font_set_baseline_snap_delegate ?? (sk_font_set_baseline_snap_delegate = GetSymbol<Delegates.sk_font_set_baseline_snap>("sk_font_set_baseline_snap")))(font, value);
	}

	internal static void sk_font_set_edging(IntPtr font, SKFontEdging value)
	{
		(sk_font_set_edging_delegate ?? (sk_font_set_edging_delegate = GetSymbol<Delegates.sk_font_set_edging>("sk_font_set_edging")))(font, value);
	}

	internal static void sk_font_set_embedded_bitmaps(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value)
	{
		(sk_font_set_embedded_bitmaps_delegate ?? (sk_font_set_embedded_bitmaps_delegate = GetSymbol<Delegates.sk_font_set_embedded_bitmaps>("sk_font_set_embedded_bitmaps")))(font, value);
	}

	internal static void sk_font_set_embolden(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value)
	{
		(sk_font_set_embolden_delegate ?? (sk_font_set_embolden_delegate = GetSymbol<Delegates.sk_font_set_embolden>("sk_font_set_embolden")))(font, value);
	}

	internal static void sk_font_set_force_auto_hinting(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value)
	{
		(sk_font_set_force_auto_hinting_delegate ?? (sk_font_set_force_auto_hinting_delegate = GetSymbol<Delegates.sk_font_set_force_auto_hinting>("sk_font_set_force_auto_hinting")))(font, value);
	}

	internal static void sk_font_set_hinting(IntPtr font, SKFontHinting value)
	{
		(sk_font_set_hinting_delegate ?? (sk_font_set_hinting_delegate = GetSymbol<Delegates.sk_font_set_hinting>("sk_font_set_hinting")))(font, value);
	}

	internal static void sk_font_set_linear_metrics(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value)
	{
		(sk_font_set_linear_metrics_delegate ?? (sk_font_set_linear_metrics_delegate = GetSymbol<Delegates.sk_font_set_linear_metrics>("sk_font_set_linear_metrics")))(font, value);
	}

	internal static void sk_font_set_scale_x(IntPtr font, float value)
	{
		(sk_font_set_scale_x_delegate ?? (sk_font_set_scale_x_delegate = GetSymbol<Delegates.sk_font_set_scale_x>("sk_font_set_scale_x")))(font, value);
	}

	internal static void sk_font_set_size(IntPtr font, float value)
	{
		(sk_font_set_size_delegate ?? (sk_font_set_size_delegate = GetSymbol<Delegates.sk_font_set_size>("sk_font_set_size")))(font, value);
	}

	internal static void sk_font_set_skew_x(IntPtr font, float value)
	{
		(sk_font_set_skew_x_delegate ?? (sk_font_set_skew_x_delegate = GetSymbol<Delegates.sk_font_set_skew_x>("sk_font_set_skew_x")))(font, value);
	}

	internal static void sk_font_set_subpixel(IntPtr font, [MarshalAs(UnmanagedType.I1)] bool value)
	{
		(sk_font_set_subpixel_delegate ?? (sk_font_set_subpixel_delegate = GetSymbol<Delegates.sk_font_set_subpixel>("sk_font_set_subpixel")))(font, value);
	}

	internal static void sk_font_set_typeface(IntPtr font, IntPtr value)
	{
		(sk_font_set_typeface_delegate ?? (sk_font_set_typeface_delegate = GetSymbol<Delegates.sk_font_set_typeface>("sk_font_set_typeface")))(font, value);
	}

	internal unsafe static int sk_font_text_to_glyphs(IntPtr font, void* text, IntPtr byteLength, SKTextEncoding encoding, ushort* glyphs, int maxGlyphCount)
	{
		return (sk_font_text_to_glyphs_delegate ?? (sk_font_text_to_glyphs_delegate = GetSymbol<Delegates.sk_font_text_to_glyphs>("sk_font_text_to_glyphs")))(font, text, byteLength, encoding, glyphs, maxGlyphCount);
	}

	internal static ushort sk_font_unichar_to_glyph(IntPtr font, int uni)
	{
		return (sk_font_unichar_to_glyph_delegate ?? (sk_font_unichar_to_glyph_delegate = GetSymbol<Delegates.sk_font_unichar_to_glyph>("sk_font_unichar_to_glyph")))(font, uni);
	}

	internal unsafe static void sk_font_unichars_to_glyphs(IntPtr font, int* uni, int count, ushort* glyphs)
	{
		(sk_font_unichars_to_glyphs_delegate ?? (sk_font_unichars_to_glyphs_delegate = GetSymbol<Delegates.sk_font_unichars_to_glyphs>("sk_font_unichars_to_glyphs")))(font, uni, count, glyphs);
	}

	internal unsafe static void sk_text_utils_get_path(void* text, IntPtr length, SKTextEncoding encoding, float x, float y, IntPtr font, IntPtr path)
	{
		(sk_text_utils_get_path_delegate ?? (sk_text_utils_get_path_delegate = GetSymbol<Delegates.sk_text_utils_get_path>("sk_text_utils_get_path")))(text, length, encoding, x, y, font, path);
	}

	internal unsafe static void sk_text_utils_get_pos_path(void* text, IntPtr length, SKTextEncoding encoding, SKPoint* pos, IntPtr font, IntPtr path)
	{
		(sk_text_utils_get_pos_path_delegate ?? (sk_text_utils_get_pos_path_delegate = GetSymbol<Delegates.sk_text_utils_get_pos_path>("sk_text_utils_get_pos_path")))(text, length, encoding, pos, font, path);
	}

	internal static SKColorTypeNative sk_colortype_get_default_8888()
	{
		return (sk_colortype_get_default_8888_delegate ?? (sk_colortype_get_default_8888_delegate = GetSymbol<Delegates.sk_colortype_get_default_8888>("sk_colortype_get_default_8888")))();
	}

	internal static int sk_nvrefcnt_get_ref_count(IntPtr refcnt)
	{
		return (sk_nvrefcnt_get_ref_count_delegate ?? (sk_nvrefcnt_get_ref_count_delegate = GetSymbol<Delegates.sk_nvrefcnt_get_ref_count>("sk_nvrefcnt_get_ref_count")))(refcnt);
	}

	internal static void sk_nvrefcnt_safe_ref(IntPtr refcnt)
	{
		(sk_nvrefcnt_safe_ref_delegate ?? (sk_nvrefcnt_safe_ref_delegate = GetSymbol<Delegates.sk_nvrefcnt_safe_ref>("sk_nvrefcnt_safe_ref")))(refcnt);
	}

	internal static void sk_nvrefcnt_safe_unref(IntPtr refcnt)
	{
		(sk_nvrefcnt_safe_unref_delegate ?? (sk_nvrefcnt_safe_unref_delegate = GetSymbol<Delegates.sk_nvrefcnt_safe_unref>("sk_nvrefcnt_safe_unref")))(refcnt);
	}

	internal static bool sk_nvrefcnt_unique(IntPtr refcnt)
	{
		return (sk_nvrefcnt_unique_delegate ?? (sk_nvrefcnt_unique_delegate = GetSymbol<Delegates.sk_nvrefcnt_unique>("sk_nvrefcnt_unique")))(refcnt);
	}

	internal static int sk_refcnt_get_ref_count(IntPtr refcnt)
	{
		return (sk_refcnt_get_ref_count_delegate ?? (sk_refcnt_get_ref_count_delegate = GetSymbol<Delegates.sk_refcnt_get_ref_count>("sk_refcnt_get_ref_count")))(refcnt);
	}

	internal static void sk_refcnt_safe_ref(IntPtr refcnt)
	{
		(sk_refcnt_safe_ref_delegate ?? (sk_refcnt_safe_ref_delegate = GetSymbol<Delegates.sk_refcnt_safe_ref>("sk_refcnt_safe_ref")))(refcnt);
	}

	internal static void sk_refcnt_safe_unref(IntPtr refcnt)
	{
		(sk_refcnt_safe_unref_delegate ?? (sk_refcnt_safe_unref_delegate = GetSymbol<Delegates.sk_refcnt_safe_unref>("sk_refcnt_safe_unref")))(refcnt);
	}

	internal static bool sk_refcnt_unique(IntPtr refcnt)
	{
		return (sk_refcnt_unique_delegate ?? (sk_refcnt_unique_delegate = GetSymbol<Delegates.sk_refcnt_unique>("sk_refcnt_unique")))(refcnt);
	}

	internal static int sk_version_get_increment()
	{
		return (sk_version_get_increment_delegate ?? (sk_version_get_increment_delegate = GetSymbol<Delegates.sk_version_get_increment>("sk_version_get_increment")))();
	}

	internal static int sk_version_get_milestone()
	{
		return (sk_version_get_milestone_delegate ?? (sk_version_get_milestone_delegate = GetSymbol<Delegates.sk_version_get_milestone>("sk_version_get_milestone")))();
	}

	internal unsafe static void* sk_version_get_string()
	{
		return (sk_version_get_string_delegate ?? (sk_version_get_string_delegate = GetSymbol<Delegates.sk_version_get_string>("sk_version_get_string")))();
	}

	internal static void sk_graphics_dump_memory_statistics(IntPtr dump)
	{
		(sk_graphics_dump_memory_statistics_delegate ?? (sk_graphics_dump_memory_statistics_delegate = GetSymbol<Delegates.sk_graphics_dump_memory_statistics>("sk_graphics_dump_memory_statistics")))(dump);
	}

	internal static int sk_graphics_get_font_cache_count_limit()
	{
		return (sk_graphics_get_font_cache_count_limit_delegate ?? (sk_graphics_get_font_cache_count_limit_delegate = GetSymbol<Delegates.sk_graphics_get_font_cache_count_limit>("sk_graphics_get_font_cache_count_limit")))();
	}

	internal static int sk_graphics_get_font_cache_count_used()
	{
		return (sk_graphics_get_font_cache_count_used_delegate ?? (sk_graphics_get_font_cache_count_used_delegate = GetSymbol<Delegates.sk_graphics_get_font_cache_count_used>("sk_graphics_get_font_cache_count_used")))();
	}

	internal static IntPtr sk_graphics_get_font_cache_limit()
	{
		return (sk_graphics_get_font_cache_limit_delegate ?? (sk_graphics_get_font_cache_limit_delegate = GetSymbol<Delegates.sk_graphics_get_font_cache_limit>("sk_graphics_get_font_cache_limit")))();
	}

	internal static IntPtr sk_graphics_get_font_cache_used()
	{
		return (sk_graphics_get_font_cache_used_delegate ?? (sk_graphics_get_font_cache_used_delegate = GetSymbol<Delegates.sk_graphics_get_font_cache_used>("sk_graphics_get_font_cache_used")))();
	}

	internal static IntPtr sk_graphics_get_resource_cache_single_allocation_byte_limit()
	{
		return (sk_graphics_get_resource_cache_single_allocation_byte_limit_delegate ?? (sk_graphics_get_resource_cache_single_allocation_byte_limit_delegate = GetSymbol<Delegates.sk_graphics_get_resource_cache_single_allocation_byte_limit>("sk_graphics_get_resource_cache_single_allocation_byte_limit")))();
	}

	internal static IntPtr sk_graphics_get_resource_cache_total_byte_limit()
	{
		return (sk_graphics_get_resource_cache_total_byte_limit_delegate ?? (sk_graphics_get_resource_cache_total_byte_limit_delegate = GetSymbol<Delegates.sk_graphics_get_resource_cache_total_byte_limit>("sk_graphics_get_resource_cache_total_byte_limit")))();
	}

	internal static IntPtr sk_graphics_get_resource_cache_total_bytes_used()
	{
		return (sk_graphics_get_resource_cache_total_bytes_used_delegate ?? (sk_graphics_get_resource_cache_total_bytes_used_delegate = GetSymbol<Delegates.sk_graphics_get_resource_cache_total_bytes_used>("sk_graphics_get_resource_cache_total_bytes_used")))();
	}

	internal static void sk_graphics_init()
	{
		(sk_graphics_init_delegate ?? (sk_graphics_init_delegate = GetSymbol<Delegates.sk_graphics_init>("sk_graphics_init")))();
	}

	internal static void sk_graphics_purge_all_caches()
	{
		(sk_graphics_purge_all_caches_delegate ?? (sk_graphics_purge_all_caches_delegate = GetSymbol<Delegates.sk_graphics_purge_all_caches>("sk_graphics_purge_all_caches")))();
	}

	internal static void sk_graphics_purge_font_cache()
	{
		(sk_graphics_purge_font_cache_delegate ?? (sk_graphics_purge_font_cache_delegate = GetSymbol<Delegates.sk_graphics_purge_font_cache>("sk_graphics_purge_font_cache")))();
	}

	internal static void sk_graphics_purge_resource_cache()
	{
		(sk_graphics_purge_resource_cache_delegate ?? (sk_graphics_purge_resource_cache_delegate = GetSymbol<Delegates.sk_graphics_purge_resource_cache>("sk_graphics_purge_resource_cache")))();
	}

	internal static int sk_graphics_set_font_cache_count_limit(int count)
	{
		return (sk_graphics_set_font_cache_count_limit_delegate ?? (sk_graphics_set_font_cache_count_limit_delegate = GetSymbol<Delegates.sk_graphics_set_font_cache_count_limit>("sk_graphics_set_font_cache_count_limit")))(count);
	}

	internal static IntPtr sk_graphics_set_font_cache_limit(IntPtr bytes)
	{
		return (sk_graphics_set_font_cache_limit_delegate ?? (sk_graphics_set_font_cache_limit_delegate = GetSymbol<Delegates.sk_graphics_set_font_cache_limit>("sk_graphics_set_font_cache_limit")))(bytes);
	}

	internal static IntPtr sk_graphics_set_resource_cache_single_allocation_byte_limit(IntPtr newLimit)
	{
		return (sk_graphics_set_resource_cache_single_allocation_byte_limit_delegate ?? (sk_graphics_set_resource_cache_single_allocation_byte_limit_delegate = GetSymbol<Delegates.sk_graphics_set_resource_cache_single_allocation_byte_limit>("sk_graphics_set_resource_cache_single_allocation_byte_limit")))(newLimit);
	}

	internal static IntPtr sk_graphics_set_resource_cache_total_byte_limit(IntPtr newLimit)
	{
		return (sk_graphics_set_resource_cache_total_byte_limit_delegate ?? (sk_graphics_set_resource_cache_total_byte_limit_delegate = GetSymbol<Delegates.sk_graphics_set_resource_cache_total_byte_limit>("sk_graphics_set_resource_cache_total_byte_limit")))(newLimit);
	}

	internal static SKAlphaType sk_image_get_alpha_type(IntPtr image)
	{
		return (sk_image_get_alpha_type_delegate ?? (sk_image_get_alpha_type_delegate = GetSymbol<Delegates.sk_image_get_alpha_type>("sk_image_get_alpha_type")))(image);
	}

	internal static SKColorTypeNative sk_image_get_color_type(IntPtr image)
	{
		return (sk_image_get_color_type_delegate ?? (sk_image_get_color_type_delegate = GetSymbol<Delegates.sk_image_get_color_type>("sk_image_get_color_type")))(image);
	}

	internal static IntPtr sk_image_get_colorspace(IntPtr image)
	{
		return (sk_image_get_colorspace_delegate ?? (sk_image_get_colorspace_delegate = GetSymbol<Delegates.sk_image_get_colorspace>("sk_image_get_colorspace")))(image);
	}

	internal static int sk_image_get_height(IntPtr cimage)
	{
		return (sk_image_get_height_delegate ?? (sk_image_get_height_delegate = GetSymbol<Delegates.sk_image_get_height>("sk_image_get_height")))(cimage);
	}

	internal static uint sk_image_get_unique_id(IntPtr cimage)
	{
		return (sk_image_get_unique_id_delegate ?? (sk_image_get_unique_id_delegate = GetSymbol<Delegates.sk_image_get_unique_id>("sk_image_get_unique_id")))(cimage);
	}

	internal static int sk_image_get_width(IntPtr cimage)
	{
		return (sk_image_get_width_delegate ?? (sk_image_get_width_delegate = GetSymbol<Delegates.sk_image_get_width>("sk_image_get_width")))(cimage);
	}

	internal static bool sk_image_is_alpha_only(IntPtr image)
	{
		return (sk_image_is_alpha_only_delegate ?? (sk_image_is_alpha_only_delegate = GetSymbol<Delegates.sk_image_is_alpha_only>("sk_image_is_alpha_only")))(image);
	}

	internal static bool sk_image_is_lazy_generated(IntPtr image)
	{
		return (sk_image_is_lazy_generated_delegate ?? (sk_image_is_lazy_generated_delegate = GetSymbol<Delegates.sk_image_is_lazy_generated>("sk_image_is_lazy_generated")))(image);
	}

	internal static bool sk_image_is_texture_backed(IntPtr image)
	{
		return (sk_image_is_texture_backed_delegate ?? (sk_image_is_texture_backed_delegate = GetSymbol<Delegates.sk_image_is_texture_backed>("sk_image_is_texture_backed")))(image);
	}

	internal static bool sk_image_is_valid(IntPtr image, IntPtr context)
	{
		return (sk_image_is_valid_delegate ?? (sk_image_is_valid_delegate = GetSymbol<Delegates.sk_image_is_valid>("sk_image_is_valid")))(image, context);
	}

	internal static IntPtr sk_image_make_non_texture_image(IntPtr cimage)
	{
		return (sk_image_make_non_texture_image_delegate ?? (sk_image_make_non_texture_image_delegate = GetSymbol<Delegates.sk_image_make_non_texture_image>("sk_image_make_non_texture_image")))(cimage);
	}

	internal static IntPtr sk_image_make_raster_image(IntPtr cimage)
	{
		return (sk_image_make_raster_image_delegate ?? (sk_image_make_raster_image_delegate = GetSymbol<Delegates.sk_image_make_raster_image>("sk_image_make_raster_image")))(cimage);
	}

	internal unsafe static IntPtr sk_image_make_raw_shader(IntPtr image, SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions* sampling, SKMatrix* cmatrix)
	{
		return (sk_image_make_raw_shader_delegate ?? (sk_image_make_raw_shader_delegate = GetSymbol<Delegates.sk_image_make_raw_shader>("sk_image_make_raw_shader")))(image, tileX, tileY, sampling, cmatrix);
	}

	internal unsafe static IntPtr sk_image_make_shader(IntPtr image, SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions* sampling, SKMatrix* cmatrix)
	{
		return (sk_image_make_shader_delegate ?? (sk_image_make_shader_delegate = GetSymbol<Delegates.sk_image_make_shader>("sk_image_make_shader")))(image, tileX, tileY, sampling, cmatrix);
	}

	internal unsafe static IntPtr sk_image_make_subset(IntPtr cimage, IntPtr context, SKRectI* subset)
	{
		return (sk_image_make_subset_delegate ?? (sk_image_make_subset_delegate = GetSymbol<Delegates.sk_image_make_subset>("sk_image_make_subset")))(cimage, context, subset);
	}

	internal unsafe static IntPtr sk_image_make_subset_raster(IntPtr cimage, SKRectI* subset)
	{
		return (sk_image_make_subset_raster_delegate ?? (sk_image_make_subset_raster_delegate = GetSymbol<Delegates.sk_image_make_subset_raster>("sk_image_make_subset_raster")))(cimage, subset);
	}

	internal static IntPtr sk_image_make_texture_image(IntPtr cimage, IntPtr context, [MarshalAs(UnmanagedType.I1)] bool mipmapped, [MarshalAs(UnmanagedType.I1)] bool budgeted)
	{
		return (sk_image_make_texture_image_delegate ?? (sk_image_make_texture_image_delegate = GetSymbol<Delegates.sk_image_make_texture_image>("sk_image_make_texture_image")))(cimage, context, mipmapped, budgeted);
	}

	internal unsafe static IntPtr sk_image_make_with_filter(IntPtr cimage, IntPtr context, IntPtr filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset)
	{
		return (sk_image_make_with_filter_delegate ?? (sk_image_make_with_filter_delegate = GetSymbol<Delegates.sk_image_make_with_filter>("sk_image_make_with_filter")))(cimage, context, filter, subset, clipBounds, outSubset, outOffset);
	}

	internal unsafe static IntPtr sk_image_make_with_filter_raster(IntPtr cimage, IntPtr filter, SKRectI* subset, SKRectI* clipBounds, SKRectI* outSubset, SKPointI* outOffset)
	{
		return (sk_image_make_with_filter_raster_delegate ?? (sk_image_make_with_filter_raster_delegate = GetSymbol<Delegates.sk_image_make_with_filter_raster>("sk_image_make_with_filter_raster")))(cimage, filter, subset, clipBounds, outSubset, outOffset);
	}

	internal static IntPtr sk_image_new_from_adopted_texture(IntPtr context, IntPtr texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, IntPtr colorSpace)
	{
		return (sk_image_new_from_adopted_texture_delegate ?? (sk_image_new_from_adopted_texture_delegate = GetSymbol<Delegates.sk_image_new_from_adopted_texture>("sk_image_new_from_adopted_texture")))(context, texture, origin, colorType, alpha, colorSpace);
	}

	internal static IntPtr sk_image_new_from_bitmap(IntPtr cbitmap)
	{
		return (sk_image_new_from_bitmap_delegate ?? (sk_image_new_from_bitmap_delegate = GetSymbol<Delegates.sk_image_new_from_bitmap>("sk_image_new_from_bitmap")))(cbitmap);
	}

	internal static IntPtr sk_image_new_from_encoded(IntPtr cdata)
	{
		return (sk_image_new_from_encoded_delegate ?? (sk_image_new_from_encoded_delegate = GetSymbol<Delegates.sk_image_new_from_encoded>("sk_image_new_from_encoded")))(cdata);
	}

	internal unsafe static IntPtr sk_image_new_from_picture(IntPtr picture, SKSizeI* dimensions, SKMatrix* cmatrix, IntPtr paint, [MarshalAs(UnmanagedType.I1)] bool useFloatingPointBitDepth, IntPtr colorSpace, IntPtr props)
	{
		return (sk_image_new_from_picture_delegate ?? (sk_image_new_from_picture_delegate = GetSymbol<Delegates.sk_image_new_from_picture>("sk_image_new_from_picture")))(picture, dimensions, cmatrix, paint, useFloatingPointBitDepth, colorSpace, props);
	}

	internal unsafe static IntPtr sk_image_new_from_texture(IntPtr context, IntPtr texture, GRSurfaceOrigin origin, SKColorTypeNative colorType, SKAlphaType alpha, IntPtr colorSpace, SKImageTextureReleaseProxyDelegate releaseProc, void* releaseContext)
	{
		return (sk_image_new_from_texture_delegate ?? (sk_image_new_from_texture_delegate = GetSymbol<Delegates.sk_image_new_from_texture>("sk_image_new_from_texture")))(context, texture, origin, colorType, alpha, colorSpace, releaseProc, releaseContext);
	}

	internal unsafe static IntPtr sk_image_new_raster(IntPtr pixmap, SKImageRasterReleaseProxyDelegate releaseProc, void* context)
	{
		return (sk_image_new_raster_delegate ?? (sk_image_new_raster_delegate = GetSymbol<Delegates.sk_image_new_raster>("sk_image_new_raster")))(pixmap, releaseProc, context);
	}

	internal unsafe static IntPtr sk_image_new_raster_copy(SKImageInfoNative* cinfo, void* pixels, IntPtr rowBytes)
	{
		return (sk_image_new_raster_copy_delegate ?? (sk_image_new_raster_copy_delegate = GetSymbol<Delegates.sk_image_new_raster_copy>("sk_image_new_raster_copy")))(cinfo, pixels, rowBytes);
	}

	internal static IntPtr sk_image_new_raster_copy_with_pixmap(IntPtr pixmap)
	{
		return (sk_image_new_raster_copy_with_pixmap_delegate ?? (sk_image_new_raster_copy_with_pixmap_delegate = GetSymbol<Delegates.sk_image_new_raster_copy_with_pixmap>("sk_image_new_raster_copy_with_pixmap")))(pixmap);
	}

	internal unsafe static IntPtr sk_image_new_raster_data(SKImageInfoNative* cinfo, IntPtr pixels, IntPtr rowBytes)
	{
		return (sk_image_new_raster_data_delegate ?? (sk_image_new_raster_data_delegate = GetSymbol<Delegates.sk_image_new_raster_data>("sk_image_new_raster_data")))(cinfo, pixels, rowBytes);
	}

	internal static bool sk_image_peek_pixels(IntPtr image, IntPtr pixmap)
	{
		return (sk_image_peek_pixels_delegate ?? (sk_image_peek_pixels_delegate = GetSymbol<Delegates.sk_image_peek_pixels>("sk_image_peek_pixels")))(image, pixmap);
	}

	internal unsafe static bool sk_image_read_pixels(IntPtr image, SKImageInfoNative* dstInfo, void* dstPixels, IntPtr dstRowBytes, int srcX, int srcY, SKImageCachingHint cachingHint)
	{
		return (sk_image_read_pixels_delegate ?? (sk_image_read_pixels_delegate = GetSymbol<Delegates.sk_image_read_pixels>("sk_image_read_pixels")))(image, dstInfo, dstPixels, dstRowBytes, srcX, srcY, cachingHint);
	}

	internal static bool sk_image_read_pixels_into_pixmap(IntPtr image, IntPtr dst, int srcX, int srcY, SKImageCachingHint cachingHint)
	{
		return (sk_image_read_pixels_into_pixmap_delegate ?? (sk_image_read_pixels_into_pixmap_delegate = GetSymbol<Delegates.sk_image_read_pixels_into_pixmap>("sk_image_read_pixels_into_pixmap")))(image, dst, srcX, srcY, cachingHint);
	}

	internal static void sk_image_ref(IntPtr cimage)
	{
		(sk_image_ref_delegate ?? (sk_image_ref_delegate = GetSymbol<Delegates.sk_image_ref>("sk_image_ref")))(cimage);
	}

	internal static IntPtr sk_image_ref_encoded(IntPtr cimage)
	{
		return (sk_image_ref_encoded_delegate ?? (sk_image_ref_encoded_delegate = GetSymbol<Delegates.sk_image_ref_encoded>("sk_image_ref_encoded")))(cimage);
	}

	internal unsafe static bool sk_image_scale_pixels(IntPtr image, IntPtr dst, SKSamplingOptions* sampling, SKImageCachingHint cachingHint)
	{
		return (sk_image_scale_pixels_delegate ?? (sk_image_scale_pixels_delegate = GetSymbol<Delegates.sk_image_scale_pixels>("sk_image_scale_pixels")))(image, dst, sampling, cachingHint);
	}

	internal static void sk_image_unref(IntPtr cimage)
	{
		(sk_image_unref_delegate ?? (sk_image_unref_delegate = GetSymbol<Delegates.sk_image_unref>("sk_image_unref")))(cimage);
	}

	internal unsafe static IntPtr sk_imagefilter_new_arithmetic(float k1, float k2, float k3, float k4, [MarshalAs(UnmanagedType.I1)] bool enforcePMColor, IntPtr background, IntPtr foreground, SKRect* cropRect)
	{
		return (sk_imagefilter_new_arithmetic_delegate ?? (sk_imagefilter_new_arithmetic_delegate = GetSymbol<Delegates.sk_imagefilter_new_arithmetic>("sk_imagefilter_new_arithmetic")))(k1, k2, k3, k4, enforcePMColor, background, foreground, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_blend(SKBlendMode mode, IntPtr background, IntPtr foreground, SKRect* cropRect)
	{
		return (sk_imagefilter_new_blend_delegate ?? (sk_imagefilter_new_blend_delegate = GetSymbol<Delegates.sk_imagefilter_new_blend>("sk_imagefilter_new_blend")))(mode, background, foreground, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_blender(IntPtr blender, IntPtr background, IntPtr foreground, SKRect* cropRect)
	{
		return (sk_imagefilter_new_blender_delegate ?? (sk_imagefilter_new_blender_delegate = GetSymbol<Delegates.sk_imagefilter_new_blender>("sk_imagefilter_new_blender")))(blender, background, foreground, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_blur(float sigmaX, float sigmaY, SKShaderTileMode tileMode, IntPtr input, SKRect* cropRect)
	{
		return (sk_imagefilter_new_blur_delegate ?? (sk_imagefilter_new_blur_delegate = GetSymbol<Delegates.sk_imagefilter_new_blur>("sk_imagefilter_new_blur")))(sigmaX, sigmaY, tileMode, input, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_color_filter(IntPtr cf, IntPtr input, SKRect* cropRect)
	{
		return (sk_imagefilter_new_color_filter_delegate ?? (sk_imagefilter_new_color_filter_delegate = GetSymbol<Delegates.sk_imagefilter_new_color_filter>("sk_imagefilter_new_color_filter")))(cf, input, cropRect);
	}

	internal static IntPtr sk_imagefilter_new_compose(IntPtr outer, IntPtr inner)
	{
		return (sk_imagefilter_new_compose_delegate ?? (sk_imagefilter_new_compose_delegate = GetSymbol<Delegates.sk_imagefilter_new_compose>("sk_imagefilter_new_compose")))(outer, inner);
	}

	internal unsafe static IntPtr sk_imagefilter_new_dilate(float radiusX, float radiusY, IntPtr input, SKRect* cropRect)
	{
		return (sk_imagefilter_new_dilate_delegate ?? (sk_imagefilter_new_dilate_delegate = GetSymbol<Delegates.sk_imagefilter_new_dilate>("sk_imagefilter_new_dilate")))(radiusX, radiusY, input, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_displacement_map_effect(SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, IntPtr displacement, IntPtr color, SKRect* cropRect)
	{
		return (sk_imagefilter_new_displacement_map_effect_delegate ?? (sk_imagefilter_new_displacement_map_effect_delegate = GetSymbol<Delegates.sk_imagefilter_new_displacement_map_effect>("sk_imagefilter_new_displacement_map_effect")))(xChannelSelector, yChannelSelector, scale, displacement, color, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_distant_lit_diffuse(SKPoint3* direction, uint lightColor, float surfaceScale, float kd, IntPtr input, SKRect* cropRect)
	{
		return (sk_imagefilter_new_distant_lit_diffuse_delegate ?? (sk_imagefilter_new_distant_lit_diffuse_delegate = GetSymbol<Delegates.sk_imagefilter_new_distant_lit_diffuse>("sk_imagefilter_new_distant_lit_diffuse")))(direction, lightColor, surfaceScale, kd, input, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_distant_lit_specular(SKPoint3* direction, uint lightColor, float surfaceScale, float ks, float shininess, IntPtr input, SKRect* cropRect)
	{
		return (sk_imagefilter_new_distant_lit_specular_delegate ?? (sk_imagefilter_new_distant_lit_specular_delegate = GetSymbol<Delegates.sk_imagefilter_new_distant_lit_specular>("sk_imagefilter_new_distant_lit_specular")))(direction, lightColor, surfaceScale, ks, shininess, input, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_drop_shadow(float dx, float dy, float sigmaX, float sigmaY, uint color, IntPtr input, SKRect* cropRect)
	{
		return (sk_imagefilter_new_drop_shadow_delegate ?? (sk_imagefilter_new_drop_shadow_delegate = GetSymbol<Delegates.sk_imagefilter_new_drop_shadow>("sk_imagefilter_new_drop_shadow")))(dx, dy, sigmaX, sigmaY, color, input, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_drop_shadow_only(float dx, float dy, float sigmaX, float sigmaY, uint color, IntPtr input, SKRect* cropRect)
	{
		return (sk_imagefilter_new_drop_shadow_only_delegate ?? (sk_imagefilter_new_drop_shadow_only_delegate = GetSymbol<Delegates.sk_imagefilter_new_drop_shadow_only>("sk_imagefilter_new_drop_shadow_only")))(dx, dy, sigmaX, sigmaY, color, input, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_erode(float radiusX, float radiusY, IntPtr input, SKRect* cropRect)
	{
		return (sk_imagefilter_new_erode_delegate ?? (sk_imagefilter_new_erode_delegate = GetSymbol<Delegates.sk_imagefilter_new_erode>("sk_imagefilter_new_erode")))(radiusX, radiusY, input, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_image(IntPtr image, SKRect* srcRect, SKRect* dstRect, SKSamplingOptions* sampling)
	{
		return (sk_imagefilter_new_image_delegate ?? (sk_imagefilter_new_image_delegate = GetSymbol<Delegates.sk_imagefilter_new_image>("sk_imagefilter_new_image")))(image, srcRect, dstRect, sampling);
	}

	internal unsafe static IntPtr sk_imagefilter_new_image_simple(IntPtr image, SKSamplingOptions* sampling)
	{
		return (sk_imagefilter_new_image_simple_delegate ?? (sk_imagefilter_new_image_simple_delegate = GetSymbol<Delegates.sk_imagefilter_new_image_simple>("sk_imagefilter_new_image_simple")))(image, sampling);
	}

	internal unsafe static IntPtr sk_imagefilter_new_magnifier(SKRect* lensBounds, float zoomAmount, float inset, SKSamplingOptions* sampling, IntPtr input, SKRect* cropRect)
	{
		return (sk_imagefilter_new_magnifier_delegate ?? (sk_imagefilter_new_magnifier_delegate = GetSymbol<Delegates.sk_imagefilter_new_magnifier>("sk_imagefilter_new_magnifier")))(lensBounds, zoomAmount, inset, sampling, input, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_matrix_convolution(SKSizeI* kernelSize, float* kernel, float gain, float bias, SKPointI* kernelOffset, SKShaderTileMode ctileMode, [MarshalAs(UnmanagedType.I1)] bool convolveAlpha, IntPtr input, SKRect* cropRect)
	{
		return (sk_imagefilter_new_matrix_convolution_delegate ?? (sk_imagefilter_new_matrix_convolution_delegate = GetSymbol<Delegates.sk_imagefilter_new_matrix_convolution>("sk_imagefilter_new_matrix_convolution")))(kernelSize, kernel, gain, bias, kernelOffset, ctileMode, convolveAlpha, input, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_matrix_transform(SKMatrix* cmatrix, SKSamplingOptions* sampling, IntPtr input)
	{
		return (sk_imagefilter_new_matrix_transform_delegate ?? (sk_imagefilter_new_matrix_transform_delegate = GetSymbol<Delegates.sk_imagefilter_new_matrix_transform>("sk_imagefilter_new_matrix_transform")))(cmatrix, sampling, input);
	}

	internal unsafe static IntPtr sk_imagefilter_new_merge(IntPtr* cfilters, int count, SKRect* cropRect)
	{
		return (sk_imagefilter_new_merge_delegate ?? (sk_imagefilter_new_merge_delegate = GetSymbol<Delegates.sk_imagefilter_new_merge>("sk_imagefilter_new_merge")))(cfilters, count, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_merge_simple(IntPtr first, IntPtr second, SKRect* cropRect)
	{
		return (sk_imagefilter_new_merge_simple_delegate ?? (sk_imagefilter_new_merge_simple_delegate = GetSymbol<Delegates.sk_imagefilter_new_merge_simple>("sk_imagefilter_new_merge_simple")))(first, second, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_offset(float dx, float dy, IntPtr input, SKRect* cropRect)
	{
		return (sk_imagefilter_new_offset_delegate ?? (sk_imagefilter_new_offset_delegate = GetSymbol<Delegates.sk_imagefilter_new_offset>("sk_imagefilter_new_offset")))(dx, dy, input, cropRect);
	}

	internal static IntPtr sk_imagefilter_new_picture(IntPtr picture)
	{
		return (sk_imagefilter_new_picture_delegate ?? (sk_imagefilter_new_picture_delegate = GetSymbol<Delegates.sk_imagefilter_new_picture>("sk_imagefilter_new_picture")))(picture);
	}

	internal unsafe static IntPtr sk_imagefilter_new_picture_with_rect(IntPtr picture, SKRect* targetRect)
	{
		return (sk_imagefilter_new_picture_with_rect_delegate ?? (sk_imagefilter_new_picture_with_rect_delegate = GetSymbol<Delegates.sk_imagefilter_new_picture_with_rect>("sk_imagefilter_new_picture_with_rect")))(picture, targetRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_point_lit_diffuse(SKPoint3* location, uint lightColor, float surfaceScale, float kd, IntPtr input, SKRect* cropRect)
	{
		return (sk_imagefilter_new_point_lit_diffuse_delegate ?? (sk_imagefilter_new_point_lit_diffuse_delegate = GetSymbol<Delegates.sk_imagefilter_new_point_lit_diffuse>("sk_imagefilter_new_point_lit_diffuse")))(location, lightColor, surfaceScale, kd, input, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_point_lit_specular(SKPoint3* location, uint lightColor, float surfaceScale, float ks, float shininess, IntPtr input, SKRect* cropRect)
	{
		return (sk_imagefilter_new_point_lit_specular_delegate ?? (sk_imagefilter_new_point_lit_specular_delegate = GetSymbol<Delegates.sk_imagefilter_new_point_lit_specular>("sk_imagefilter_new_point_lit_specular")))(location, lightColor, surfaceScale, ks, shininess, input, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_shader(IntPtr shader, [MarshalAs(UnmanagedType.I1)] bool dither, SKRect* cropRect)
	{
		return (sk_imagefilter_new_shader_delegate ?? (sk_imagefilter_new_shader_delegate = GetSymbol<Delegates.sk_imagefilter_new_shader>("sk_imagefilter_new_shader")))(shader, dither, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_spot_lit_diffuse(SKPoint3* location, SKPoint3* target, float specularExponent, float cutoffAngle, uint lightColor, float surfaceScale, float kd, IntPtr input, SKRect* cropRect)
	{
		return (sk_imagefilter_new_spot_lit_diffuse_delegate ?? (sk_imagefilter_new_spot_lit_diffuse_delegate = GetSymbol<Delegates.sk_imagefilter_new_spot_lit_diffuse>("sk_imagefilter_new_spot_lit_diffuse")))(location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, kd, input, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_spot_lit_specular(SKPoint3* location, SKPoint3* target, float specularExponent, float cutoffAngle, uint lightColor, float surfaceScale, float ks, float shininess, IntPtr input, SKRect* cropRect)
	{
		return (sk_imagefilter_new_spot_lit_specular_delegate ?? (sk_imagefilter_new_spot_lit_specular_delegate = GetSymbol<Delegates.sk_imagefilter_new_spot_lit_specular>("sk_imagefilter_new_spot_lit_specular")))(location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, ks, shininess, input, cropRect);
	}

	internal unsafe static IntPtr sk_imagefilter_new_tile(SKRect* src, SKRect* dst, IntPtr input)
	{
		return (sk_imagefilter_new_tile_delegate ?? (sk_imagefilter_new_tile_delegate = GetSymbol<Delegates.sk_imagefilter_new_tile>("sk_imagefilter_new_tile")))(src, dst, input);
	}

	internal static void sk_imagefilter_unref(IntPtr cfilter)
	{
		(sk_imagefilter_unref_delegate ?? (sk_imagefilter_unref_delegate = GetSymbol<Delegates.sk_imagefilter_unref>("sk_imagefilter_unref")))(cfilter);
	}

	internal static void sk_linker_keep_alive()
	{
		(sk_linker_keep_alive_delegate ?? (sk_linker_keep_alive_delegate = GetSymbol<Delegates.sk_linker_keep_alive>("sk_linker_keep_alive")))();
	}

	internal static IntPtr sk_maskfilter_new_blur(SKBlurStyle param0, float sigma)
	{
		return (sk_maskfilter_new_blur_delegate ?? (sk_maskfilter_new_blur_delegate = GetSymbol<Delegates.sk_maskfilter_new_blur>("sk_maskfilter_new_blur")))(param0, sigma);
	}

	internal static IntPtr sk_maskfilter_new_blur_with_flags(SKBlurStyle param0, float sigma, [MarshalAs(UnmanagedType.I1)] bool respectCTM)
	{
		return (sk_maskfilter_new_blur_with_flags_delegate ?? (sk_maskfilter_new_blur_with_flags_delegate = GetSymbol<Delegates.sk_maskfilter_new_blur_with_flags>("sk_maskfilter_new_blur_with_flags")))(param0, sigma, respectCTM);
	}

	internal static IntPtr sk_maskfilter_new_clip(byte min, byte max)
	{
		return (sk_maskfilter_new_clip_delegate ?? (sk_maskfilter_new_clip_delegate = GetSymbol<Delegates.sk_maskfilter_new_clip>("sk_maskfilter_new_clip")))(min, max);
	}

	internal static IntPtr sk_maskfilter_new_gamma(float gamma)
	{
		return (sk_maskfilter_new_gamma_delegate ?? (sk_maskfilter_new_gamma_delegate = GetSymbol<Delegates.sk_maskfilter_new_gamma>("sk_maskfilter_new_gamma")))(gamma);
	}

	internal static IntPtr sk_maskfilter_new_shader(IntPtr cshader)
	{
		return (sk_maskfilter_new_shader_delegate ?? (sk_maskfilter_new_shader_delegate = GetSymbol<Delegates.sk_maskfilter_new_shader>("sk_maskfilter_new_shader")))(cshader);
	}

	internal unsafe static IntPtr sk_maskfilter_new_table(byte* table)
	{
		return (sk_maskfilter_new_table_delegate ?? (sk_maskfilter_new_table_delegate = GetSymbol<Delegates.sk_maskfilter_new_table>("sk_maskfilter_new_table")))(table);
	}

	internal static void sk_maskfilter_ref(IntPtr param0)
	{
		(sk_maskfilter_ref_delegate ?? (sk_maskfilter_ref_delegate = GetSymbol<Delegates.sk_maskfilter_ref>("sk_maskfilter_ref")))(param0);
	}

	internal static void sk_maskfilter_unref(IntPtr param0)
	{
		(sk_maskfilter_unref_delegate ?? (sk_maskfilter_unref_delegate = GetSymbol<Delegates.sk_maskfilter_unref>("sk_maskfilter_unref")))(param0);
	}

	internal unsafe static void sk_matrix_concat(SKMatrix* result, SKMatrix* first, SKMatrix* second)
	{
		(sk_matrix_concat_delegate ?? (sk_matrix_concat_delegate = GetSymbol<Delegates.sk_matrix_concat>("sk_matrix_concat")))(result, first, second);
	}

	internal unsafe static void sk_matrix_map_points(SKMatrix* matrix, SKPoint* dst, SKPoint* src, int count)
	{
		(sk_matrix_map_points_delegate ?? (sk_matrix_map_points_delegate = GetSymbol<Delegates.sk_matrix_map_points>("sk_matrix_map_points")))(matrix, dst, src, count);
	}

	internal unsafe static float sk_matrix_map_radius(SKMatrix* matrix, float radius)
	{
		return (sk_matrix_map_radius_delegate ?? (sk_matrix_map_radius_delegate = GetSymbol<Delegates.sk_matrix_map_radius>("sk_matrix_map_radius")))(matrix, radius);
	}

	internal unsafe static void sk_matrix_map_rect(SKMatrix* matrix, SKRect* dest, SKRect* source)
	{
		(sk_matrix_map_rect_delegate ?? (sk_matrix_map_rect_delegate = GetSymbol<Delegates.sk_matrix_map_rect>("sk_matrix_map_rect")))(matrix, dest, source);
	}

	internal unsafe static void sk_matrix_map_vector(SKMatrix* matrix, float x, float y, SKPoint* result)
	{
		(sk_matrix_map_vector_delegate ?? (sk_matrix_map_vector_delegate = GetSymbol<Delegates.sk_matrix_map_vector>("sk_matrix_map_vector")))(matrix, x, y, result);
	}

	internal unsafe static void sk_matrix_map_vectors(SKMatrix* matrix, SKPoint* dst, SKPoint* src, int count)
	{
		(sk_matrix_map_vectors_delegate ?? (sk_matrix_map_vectors_delegate = GetSymbol<Delegates.sk_matrix_map_vectors>("sk_matrix_map_vectors")))(matrix, dst, src, count);
	}

	internal unsafe static void sk_matrix_map_xy(SKMatrix* matrix, float x, float y, SKPoint* result)
	{
		(sk_matrix_map_xy_delegate ?? (sk_matrix_map_xy_delegate = GetSymbol<Delegates.sk_matrix_map_xy>("sk_matrix_map_xy")))(matrix, x, y, result);
	}

	internal unsafe static void sk_matrix_post_concat(SKMatrix* result, SKMatrix* matrix)
	{
		(sk_matrix_post_concat_delegate ?? (sk_matrix_post_concat_delegate = GetSymbol<Delegates.sk_matrix_post_concat>("sk_matrix_post_concat")))(result, matrix);
	}

	internal unsafe static void sk_matrix_pre_concat(SKMatrix* result, SKMatrix* matrix)
	{
		(sk_matrix_pre_concat_delegate ?? (sk_matrix_pre_concat_delegate = GetSymbol<Delegates.sk_matrix_pre_concat>("sk_matrix_pre_concat")))(result, matrix);
	}

	internal unsafe static bool sk_matrix_try_invert(SKMatrix* matrix, SKMatrix* result)
	{
		return (sk_matrix_try_invert_delegate ?? (sk_matrix_try_invert_delegate = GetSymbol<Delegates.sk_matrix_try_invert>("sk_matrix_try_invert")))(matrix, result);
	}

	internal static IntPtr sk_paint_clone(IntPtr param0)
	{
		return (sk_paint_clone_delegate ?? (sk_paint_clone_delegate = GetSymbol<Delegates.sk_paint_clone>("sk_paint_clone")))(param0);
	}

	internal static void sk_paint_delete(IntPtr param0)
	{
		(sk_paint_delete_delegate ?? (sk_paint_delete_delegate = GetSymbol<Delegates.sk_paint_delete>("sk_paint_delete")))(param0);
	}

	internal static IntPtr sk_paint_get_blender(IntPtr cpaint)
	{
		return (sk_paint_get_blender_delegate ?? (sk_paint_get_blender_delegate = GetSymbol<Delegates.sk_paint_get_blender>("sk_paint_get_blender")))(cpaint);
	}

	internal static SKBlendMode sk_paint_get_blendmode(IntPtr param0)
	{
		return (sk_paint_get_blendmode_delegate ?? (sk_paint_get_blendmode_delegate = GetSymbol<Delegates.sk_paint_get_blendmode>("sk_paint_get_blendmode")))(param0);
	}

	internal static uint sk_paint_get_color(IntPtr param0)
	{
		return (sk_paint_get_color_delegate ?? (sk_paint_get_color_delegate = GetSymbol<Delegates.sk_paint_get_color>("sk_paint_get_color")))(param0);
	}

	internal unsafe static void sk_paint_get_color4f(IntPtr paint, SKColorF* color)
	{
		(sk_paint_get_color4f_delegate ?? (sk_paint_get_color4f_delegate = GetSymbol<Delegates.sk_paint_get_color4f>("sk_paint_get_color4f")))(paint, color);
	}

	internal static IntPtr sk_paint_get_colorfilter(IntPtr param0)
	{
		return (sk_paint_get_colorfilter_delegate ?? (sk_paint_get_colorfilter_delegate = GetSymbol<Delegates.sk_paint_get_colorfilter>("sk_paint_get_colorfilter")))(param0);
	}

	internal unsafe static bool sk_paint_get_fill_path(IntPtr cpaint, IntPtr src, IntPtr dst, SKRect* cullRect, SKMatrix* cmatrix)
	{
		return (sk_paint_get_fill_path_delegate ?? (sk_paint_get_fill_path_delegate = GetSymbol<Delegates.sk_paint_get_fill_path>("sk_paint_get_fill_path")))(cpaint, src, dst, cullRect, cmatrix);
	}

	internal static IntPtr sk_paint_get_imagefilter(IntPtr param0)
	{
		return (sk_paint_get_imagefilter_delegate ?? (sk_paint_get_imagefilter_delegate = GetSymbol<Delegates.sk_paint_get_imagefilter>("sk_paint_get_imagefilter")))(param0);
	}

	internal static IntPtr sk_paint_get_maskfilter(IntPtr param0)
	{
		return (sk_paint_get_maskfilter_delegate ?? (sk_paint_get_maskfilter_delegate = GetSymbol<Delegates.sk_paint_get_maskfilter>("sk_paint_get_maskfilter")))(param0);
	}

	internal static IntPtr sk_paint_get_path_effect(IntPtr cpaint)
	{
		return (sk_paint_get_path_effect_delegate ?? (sk_paint_get_path_effect_delegate = GetSymbol<Delegates.sk_paint_get_path_effect>("sk_paint_get_path_effect")))(cpaint);
	}

	internal static IntPtr sk_paint_get_shader(IntPtr param0)
	{
		return (sk_paint_get_shader_delegate ?? (sk_paint_get_shader_delegate = GetSymbol<Delegates.sk_paint_get_shader>("sk_paint_get_shader")))(param0);
	}

	internal static SKStrokeCap sk_paint_get_stroke_cap(IntPtr param0)
	{
		return (sk_paint_get_stroke_cap_delegate ?? (sk_paint_get_stroke_cap_delegate = GetSymbol<Delegates.sk_paint_get_stroke_cap>("sk_paint_get_stroke_cap")))(param0);
	}

	internal static SKStrokeJoin sk_paint_get_stroke_join(IntPtr param0)
	{
		return (sk_paint_get_stroke_join_delegate ?? (sk_paint_get_stroke_join_delegate = GetSymbol<Delegates.sk_paint_get_stroke_join>("sk_paint_get_stroke_join")))(param0);
	}

	internal static float sk_paint_get_stroke_miter(IntPtr param0)
	{
		return (sk_paint_get_stroke_miter_delegate ?? (sk_paint_get_stroke_miter_delegate = GetSymbol<Delegates.sk_paint_get_stroke_miter>("sk_paint_get_stroke_miter")))(param0);
	}

	internal static float sk_paint_get_stroke_width(IntPtr param0)
	{
		return (sk_paint_get_stroke_width_delegate ?? (sk_paint_get_stroke_width_delegate = GetSymbol<Delegates.sk_paint_get_stroke_width>("sk_paint_get_stroke_width")))(param0);
	}

	internal static SKPaintStyle sk_paint_get_style(IntPtr param0)
	{
		return (sk_paint_get_style_delegate ?? (sk_paint_get_style_delegate = GetSymbol<Delegates.sk_paint_get_style>("sk_paint_get_style")))(param0);
	}

	internal static bool sk_paint_is_antialias(IntPtr param0)
	{
		return (sk_paint_is_antialias_delegate ?? (sk_paint_is_antialias_delegate = GetSymbol<Delegates.sk_paint_is_antialias>("sk_paint_is_antialias")))(param0);
	}

	internal static bool sk_paint_is_dither(IntPtr param0)
	{
		return (sk_paint_is_dither_delegate ?? (sk_paint_is_dither_delegate = GetSymbol<Delegates.sk_paint_is_dither>("sk_paint_is_dither")))(param0);
	}

	internal static IntPtr sk_paint_new()
	{
		return (sk_paint_new_delegate ?? (sk_paint_new_delegate = GetSymbol<Delegates.sk_paint_new>("sk_paint_new")))();
	}

	internal static void sk_paint_reset(IntPtr param0)
	{
		(sk_paint_reset_delegate ?? (sk_paint_reset_delegate = GetSymbol<Delegates.sk_paint_reset>("sk_paint_reset")))(param0);
	}

	internal static void sk_paint_set_antialias(IntPtr param0, [MarshalAs(UnmanagedType.I1)] bool param1)
	{
		(sk_paint_set_antialias_delegate ?? (sk_paint_set_antialias_delegate = GetSymbol<Delegates.sk_paint_set_antialias>("sk_paint_set_antialias")))(param0, param1);
	}

	internal static void sk_paint_set_blender(IntPtr paint, IntPtr blender)
	{
		(sk_paint_set_blender_delegate ?? (sk_paint_set_blender_delegate = GetSymbol<Delegates.sk_paint_set_blender>("sk_paint_set_blender")))(paint, blender);
	}

	internal static void sk_paint_set_blendmode(IntPtr param0, SKBlendMode param1)
	{
		(sk_paint_set_blendmode_delegate ?? (sk_paint_set_blendmode_delegate = GetSymbol<Delegates.sk_paint_set_blendmode>("sk_paint_set_blendmode")))(param0, param1);
	}

	internal static void sk_paint_set_color(IntPtr param0, uint param1)
	{
		(sk_paint_set_color_delegate ?? (sk_paint_set_color_delegate = GetSymbol<Delegates.sk_paint_set_color>("sk_paint_set_color")))(param0, param1);
	}

	internal unsafe static void sk_paint_set_color4f(IntPtr paint, SKColorF* color, IntPtr colorspace)
	{
		(sk_paint_set_color4f_delegate ?? (sk_paint_set_color4f_delegate = GetSymbol<Delegates.sk_paint_set_color4f>("sk_paint_set_color4f")))(paint, color, colorspace);
	}

	internal static void sk_paint_set_colorfilter(IntPtr param0, IntPtr param1)
	{
		(sk_paint_set_colorfilter_delegate ?? (sk_paint_set_colorfilter_delegate = GetSymbol<Delegates.sk_paint_set_colorfilter>("sk_paint_set_colorfilter")))(param0, param1);
	}

	internal static void sk_paint_set_dither(IntPtr param0, [MarshalAs(UnmanagedType.I1)] bool param1)
	{
		(sk_paint_set_dither_delegate ?? (sk_paint_set_dither_delegate = GetSymbol<Delegates.sk_paint_set_dither>("sk_paint_set_dither")))(param0, param1);
	}

	internal static void sk_paint_set_imagefilter(IntPtr param0, IntPtr param1)
	{
		(sk_paint_set_imagefilter_delegate ?? (sk_paint_set_imagefilter_delegate = GetSymbol<Delegates.sk_paint_set_imagefilter>("sk_paint_set_imagefilter")))(param0, param1);
	}

	internal static void sk_paint_set_maskfilter(IntPtr param0, IntPtr param1)
	{
		(sk_paint_set_maskfilter_delegate ?? (sk_paint_set_maskfilter_delegate = GetSymbol<Delegates.sk_paint_set_maskfilter>("sk_paint_set_maskfilter")))(param0, param1);
	}

	internal static void sk_paint_set_path_effect(IntPtr cpaint, IntPtr effect)
	{
		(sk_paint_set_path_effect_delegate ?? (sk_paint_set_path_effect_delegate = GetSymbol<Delegates.sk_paint_set_path_effect>("sk_paint_set_path_effect")))(cpaint, effect);
	}

	internal static void sk_paint_set_shader(IntPtr param0, IntPtr param1)
	{
		(sk_paint_set_shader_delegate ?? (sk_paint_set_shader_delegate = GetSymbol<Delegates.sk_paint_set_shader>("sk_paint_set_shader")))(param0, param1);
	}

	internal static void sk_paint_set_stroke_cap(IntPtr param0, SKStrokeCap param1)
	{
		(sk_paint_set_stroke_cap_delegate ?? (sk_paint_set_stroke_cap_delegate = GetSymbol<Delegates.sk_paint_set_stroke_cap>("sk_paint_set_stroke_cap")))(param0, param1);
	}

	internal static void sk_paint_set_stroke_join(IntPtr param0, SKStrokeJoin param1)
	{
		(sk_paint_set_stroke_join_delegate ?? (sk_paint_set_stroke_join_delegate = GetSymbol<Delegates.sk_paint_set_stroke_join>("sk_paint_set_stroke_join")))(param0, param1);
	}

	internal static void sk_paint_set_stroke_miter(IntPtr param0, float miter)
	{
		(sk_paint_set_stroke_miter_delegate ?? (sk_paint_set_stroke_miter_delegate = GetSymbol<Delegates.sk_paint_set_stroke_miter>("sk_paint_set_stroke_miter")))(param0, miter);
	}

	internal static void sk_paint_set_stroke_width(IntPtr param0, float width)
	{
		(sk_paint_set_stroke_width_delegate ?? (sk_paint_set_stroke_width_delegate = GetSymbol<Delegates.sk_paint_set_stroke_width>("sk_paint_set_stroke_width")))(param0, width);
	}

	internal static void sk_paint_set_style(IntPtr param0, SKPaintStyle param1)
	{
		(sk_paint_set_style_delegate ?? (sk_paint_set_style_delegate = GetSymbol<Delegates.sk_paint_set_style>("sk_paint_set_style")))(param0, param1);
	}

	internal static void sk_opbuilder_add(IntPtr builder, IntPtr path, SKPathOp op)
	{
		(sk_opbuilder_add_delegate ?? (sk_opbuilder_add_delegate = GetSymbol<Delegates.sk_opbuilder_add>("sk_opbuilder_add")))(builder, path, op);
	}

	internal static void sk_opbuilder_destroy(IntPtr builder)
	{
		(sk_opbuilder_destroy_delegate ?? (sk_opbuilder_destroy_delegate = GetSymbol<Delegates.sk_opbuilder_destroy>("sk_opbuilder_destroy")))(builder);
	}

	internal static IntPtr sk_opbuilder_new()
	{
		return (sk_opbuilder_new_delegate ?? (sk_opbuilder_new_delegate = GetSymbol<Delegates.sk_opbuilder_new>("sk_opbuilder_new")))();
	}

	internal static bool sk_opbuilder_resolve(IntPtr builder, IntPtr result)
	{
		return (sk_opbuilder_resolve_delegate ?? (sk_opbuilder_resolve_delegate = GetSymbol<Delegates.sk_opbuilder_resolve>("sk_opbuilder_resolve")))(builder, result);
	}

	internal unsafe static void sk_path_add_arc(IntPtr cpath, SKRect* crect, float startAngle, float sweepAngle)
	{
		(sk_path_add_arc_delegate ?? (sk_path_add_arc_delegate = GetSymbol<Delegates.sk_path_add_arc>("sk_path_add_arc")))(cpath, crect, startAngle, sweepAngle);
	}

	internal static void sk_path_add_circle(IntPtr param0, float x, float y, float radius, SKPathDirection dir)
	{
		(sk_path_add_circle_delegate ?? (sk_path_add_circle_delegate = GetSymbol<Delegates.sk_path_add_circle>("sk_path_add_circle")))(param0, x, y, radius, dir);
	}

	internal unsafe static void sk_path_add_oval(IntPtr param0, SKRect* param1, SKPathDirection param2)
	{
		(sk_path_add_oval_delegate ?? (sk_path_add_oval_delegate = GetSymbol<Delegates.sk_path_add_oval>("sk_path_add_oval")))(param0, param1, param2);
	}

	internal static void sk_path_add_path(IntPtr cpath, IntPtr other, SKPathAddMode add_mode)
	{
		(sk_path_add_path_delegate ?? (sk_path_add_path_delegate = GetSymbol<Delegates.sk_path_add_path>("sk_path_add_path")))(cpath, other, add_mode);
	}

	internal unsafe static void sk_path_add_path_matrix(IntPtr cpath, IntPtr other, SKMatrix* matrix, SKPathAddMode add_mode)
	{
		(sk_path_add_path_matrix_delegate ?? (sk_path_add_path_matrix_delegate = GetSymbol<Delegates.sk_path_add_path_matrix>("sk_path_add_path_matrix")))(cpath, other, matrix, add_mode);
	}

	internal static void sk_path_add_path_offset(IntPtr cpath, IntPtr other, float dx, float dy, SKPathAddMode add_mode)
	{
		(sk_path_add_path_offset_delegate ?? (sk_path_add_path_offset_delegate = GetSymbol<Delegates.sk_path_add_path_offset>("sk_path_add_path_offset")))(cpath, other, dx, dy, add_mode);
	}

	internal static void sk_path_add_path_reverse(IntPtr cpath, IntPtr other)
	{
		(sk_path_add_path_reverse_delegate ?? (sk_path_add_path_reverse_delegate = GetSymbol<Delegates.sk_path_add_path_reverse>("sk_path_add_path_reverse")))(cpath, other);
	}

	internal unsafe static void sk_path_add_poly(IntPtr cpath, SKPoint* points, int count, [MarshalAs(UnmanagedType.I1)] bool close)
	{
		(sk_path_add_poly_delegate ?? (sk_path_add_poly_delegate = GetSymbol<Delegates.sk_path_add_poly>("sk_path_add_poly")))(cpath, points, count, close);
	}

	internal unsafe static void sk_path_add_rect(IntPtr param0, SKRect* param1, SKPathDirection param2)
	{
		(sk_path_add_rect_delegate ?? (sk_path_add_rect_delegate = GetSymbol<Delegates.sk_path_add_rect>("sk_path_add_rect")))(param0, param1, param2);
	}

	internal unsafe static void sk_path_add_rect_start(IntPtr cpath, SKRect* crect, SKPathDirection cdir, uint startIndex)
	{
		(sk_path_add_rect_start_delegate ?? (sk_path_add_rect_start_delegate = GetSymbol<Delegates.sk_path_add_rect_start>("sk_path_add_rect_start")))(cpath, crect, cdir, startIndex);
	}

	internal unsafe static void sk_path_add_rounded_rect(IntPtr param0, SKRect* param1, float param2, float param3, SKPathDirection param4)
	{
		(sk_path_add_rounded_rect_delegate ?? (sk_path_add_rounded_rect_delegate = GetSymbol<Delegates.sk_path_add_rounded_rect>("sk_path_add_rounded_rect")))(param0, param1, param2, param3, param4);
	}

	internal static void sk_path_add_rrect(IntPtr param0, IntPtr param1, SKPathDirection param2)
	{
		(sk_path_add_rrect_delegate ?? (sk_path_add_rrect_delegate = GetSymbol<Delegates.sk_path_add_rrect>("sk_path_add_rrect")))(param0, param1, param2);
	}

	internal static void sk_path_add_rrect_start(IntPtr param0, IntPtr param1, SKPathDirection param2, uint param3)
	{
		(sk_path_add_rrect_start_delegate ?? (sk_path_add_rrect_start_delegate = GetSymbol<Delegates.sk_path_add_rrect_start>("sk_path_add_rrect_start")))(param0, param1, param2, param3);
	}

	internal static void sk_path_arc_to(IntPtr param0, float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y)
	{
		(sk_path_arc_to_delegate ?? (sk_path_arc_to_delegate = GetSymbol<Delegates.sk_path_arc_to>("sk_path_arc_to")))(param0, rx, ry, xAxisRotate, largeArc, sweep, x, y);
	}

	internal unsafe static void sk_path_arc_to_with_oval(IntPtr param0, SKRect* oval, float startAngle, float sweepAngle, [MarshalAs(UnmanagedType.I1)] bool forceMoveTo)
	{
		(sk_path_arc_to_with_oval_delegate ?? (sk_path_arc_to_with_oval_delegate = GetSymbol<Delegates.sk_path_arc_to_with_oval>("sk_path_arc_to_with_oval")))(param0, oval, startAngle, sweepAngle, forceMoveTo);
	}

	internal static void sk_path_arc_to_with_points(IntPtr param0, float x1, float y1, float x2, float y2, float radius)
	{
		(sk_path_arc_to_with_points_delegate ?? (sk_path_arc_to_with_points_delegate = GetSymbol<Delegates.sk_path_arc_to_with_points>("sk_path_arc_to_with_points")))(param0, x1, y1, x2, y2, radius);
	}

	internal static IntPtr sk_path_clone(IntPtr cpath)
	{
		return (sk_path_clone_delegate ?? (sk_path_clone_delegate = GetSymbol<Delegates.sk_path_clone>("sk_path_clone")))(cpath);
	}

	internal static void sk_path_close(IntPtr param0)
	{
		(sk_path_close_delegate ?? (sk_path_close_delegate = GetSymbol<Delegates.sk_path_close>("sk_path_close")))(param0);
	}

	internal unsafe static void sk_path_compute_tight_bounds(IntPtr param0, SKRect* param1)
	{
		(sk_path_compute_tight_bounds_delegate ?? (sk_path_compute_tight_bounds_delegate = GetSymbol<Delegates.sk_path_compute_tight_bounds>("sk_path_compute_tight_bounds")))(param0, param1);
	}

	internal static void sk_path_conic_to(IntPtr param0, float x0, float y0, float x1, float y1, float w)
	{
		(sk_path_conic_to_delegate ?? (sk_path_conic_to_delegate = GetSymbol<Delegates.sk_path_conic_to>("sk_path_conic_to")))(param0, x0, y0, x1, y1, w);
	}

	internal static bool sk_path_contains(IntPtr cpath, float x, float y)
	{
		return (sk_path_contains_delegate ?? (sk_path_contains_delegate = GetSymbol<Delegates.sk_path_contains>("sk_path_contains")))(cpath, x, y);
	}

	internal unsafe static int sk_path_convert_conic_to_quads(SKPoint* p0, SKPoint* p1, SKPoint* p2, float w, SKPoint* pts, int pow2)
	{
		return (sk_path_convert_conic_to_quads_delegate ?? (sk_path_convert_conic_to_quads_delegate = GetSymbol<Delegates.sk_path_convert_conic_to_quads>("sk_path_convert_conic_to_quads")))(p0, p1, p2, w, pts, pow2);
	}

	internal static int sk_path_count_points(IntPtr cpath)
	{
		return (sk_path_count_points_delegate ?? (sk_path_count_points_delegate = GetSymbol<Delegates.sk_path_count_points>("sk_path_count_points")))(cpath);
	}

	internal static int sk_path_count_verbs(IntPtr cpath)
	{
		return (sk_path_count_verbs_delegate ?? (sk_path_count_verbs_delegate = GetSymbol<Delegates.sk_path_count_verbs>("sk_path_count_verbs")))(cpath);
	}

	internal static IntPtr sk_path_create_iter(IntPtr cpath, int forceClose)
	{
		return (sk_path_create_iter_delegate ?? (sk_path_create_iter_delegate = GetSymbol<Delegates.sk_path_create_iter>("sk_path_create_iter")))(cpath, forceClose);
	}

	internal static IntPtr sk_path_create_rawiter(IntPtr cpath)
	{
		return (sk_path_create_rawiter_delegate ?? (sk_path_create_rawiter_delegate = GetSymbol<Delegates.sk_path_create_rawiter>("sk_path_create_rawiter")))(cpath);
	}

	internal static void sk_path_cubic_to(IntPtr param0, float x0, float y0, float x1, float y1, float x2, float y2)
	{
		(sk_path_cubic_to_delegate ?? (sk_path_cubic_to_delegate = GetSymbol<Delegates.sk_path_cubic_to>("sk_path_cubic_to")))(param0, x0, y0, x1, y1, x2, y2);
	}

	internal static void sk_path_delete(IntPtr param0)
	{
		(sk_path_delete_delegate ?? (sk_path_delete_delegate = GetSymbol<Delegates.sk_path_delete>("sk_path_delete")))(param0);
	}

	internal unsafe static void sk_path_get_bounds(IntPtr param0, SKRect* param1)
	{
		(sk_path_get_bounds_delegate ?? (sk_path_get_bounds_delegate = GetSymbol<Delegates.sk_path_get_bounds>("sk_path_get_bounds")))(param0, param1);
	}

	internal static SKPathFillType sk_path_get_filltype(IntPtr param0)
	{
		return (sk_path_get_filltype_delegate ?? (sk_path_get_filltype_delegate = GetSymbol<Delegates.sk_path_get_filltype>("sk_path_get_filltype")))(param0);
	}

	internal unsafe static bool sk_path_get_last_point(IntPtr cpath, SKPoint* point)
	{
		return (sk_path_get_last_point_delegate ?? (sk_path_get_last_point_delegate = GetSymbol<Delegates.sk_path_get_last_point>("sk_path_get_last_point")))(cpath, point);
	}

	internal unsafe static void sk_path_get_point(IntPtr cpath, int index, SKPoint* point)
	{
		(sk_path_get_point_delegate ?? (sk_path_get_point_delegate = GetSymbol<Delegates.sk_path_get_point>("sk_path_get_point")))(cpath, index, point);
	}

	internal unsafe static int sk_path_get_points(IntPtr cpath, SKPoint* points, int max)
	{
		return (sk_path_get_points_delegate ?? (sk_path_get_points_delegate = GetSymbol<Delegates.sk_path_get_points>("sk_path_get_points")))(cpath, points, max);
	}

	internal static uint sk_path_get_segment_masks(IntPtr cpath)
	{
		return (sk_path_get_segment_masks_delegate ?? (sk_path_get_segment_masks_delegate = GetSymbol<Delegates.sk_path_get_segment_masks>("sk_path_get_segment_masks")))(cpath);
	}

	internal static bool sk_path_is_convex(IntPtr cpath)
	{
		return (sk_path_is_convex_delegate ?? (sk_path_is_convex_delegate = GetSymbol<Delegates.sk_path_is_convex>("sk_path_is_convex")))(cpath);
	}

	internal unsafe static bool sk_path_is_line(IntPtr cpath, SKPoint* line)
	{
		return (sk_path_is_line_delegate ?? (sk_path_is_line_delegate = GetSymbol<Delegates.sk_path_is_line>("sk_path_is_line")))(cpath, line);
	}

	internal unsafe static bool sk_path_is_oval(IntPtr cpath, SKRect* bounds)
	{
		return (sk_path_is_oval_delegate ?? (sk_path_is_oval_delegate = GetSymbol<Delegates.sk_path_is_oval>("sk_path_is_oval")))(cpath, bounds);
	}

	internal unsafe static bool sk_path_is_rect(IntPtr cpath, SKRect* rect, byte* isClosed, SKPathDirection* direction)
	{
		return (sk_path_is_rect_delegate ?? (sk_path_is_rect_delegate = GetSymbol<Delegates.sk_path_is_rect>("sk_path_is_rect")))(cpath, rect, isClosed, direction);
	}

	internal static bool sk_path_is_rrect(IntPtr cpath, IntPtr bounds)
	{
		return (sk_path_is_rrect_delegate ?? (sk_path_is_rrect_delegate = GetSymbol<Delegates.sk_path_is_rrect>("sk_path_is_rrect")))(cpath, bounds);
	}

	internal static float sk_path_iter_conic_weight(IntPtr iterator)
	{
		return (sk_path_iter_conic_weight_delegate ?? (sk_path_iter_conic_weight_delegate = GetSymbol<Delegates.sk_path_iter_conic_weight>("sk_path_iter_conic_weight")))(iterator);
	}

	internal static void sk_path_iter_destroy(IntPtr iterator)
	{
		(sk_path_iter_destroy_delegate ?? (sk_path_iter_destroy_delegate = GetSymbol<Delegates.sk_path_iter_destroy>("sk_path_iter_destroy")))(iterator);
	}

	internal static int sk_path_iter_is_close_line(IntPtr iterator)
	{
		return (sk_path_iter_is_close_line_delegate ?? (sk_path_iter_is_close_line_delegate = GetSymbol<Delegates.sk_path_iter_is_close_line>("sk_path_iter_is_close_line")))(iterator);
	}

	internal static int sk_path_iter_is_closed_contour(IntPtr iterator)
	{
		return (sk_path_iter_is_closed_contour_delegate ?? (sk_path_iter_is_closed_contour_delegate = GetSymbol<Delegates.sk_path_iter_is_closed_contour>("sk_path_iter_is_closed_contour")))(iterator);
	}

	internal unsafe static SKPathVerb sk_path_iter_next(IntPtr iterator, SKPoint* points)
	{
		return (sk_path_iter_next_delegate ?? (sk_path_iter_next_delegate = GetSymbol<Delegates.sk_path_iter_next>("sk_path_iter_next")))(iterator, points);
	}

	internal static void sk_path_line_to(IntPtr param0, float x, float y)
	{
		(sk_path_line_to_delegate ?? (sk_path_line_to_delegate = GetSymbol<Delegates.sk_path_line_to>("sk_path_line_to")))(param0, x, y);
	}

	internal static void sk_path_move_to(IntPtr param0, float x, float y)
	{
		(sk_path_move_to_delegate ?? (sk_path_move_to_delegate = GetSymbol<Delegates.sk_path_move_to>("sk_path_move_to")))(param0, x, y);
	}

	internal static IntPtr sk_path_new()
	{
		return (sk_path_new_delegate ?? (sk_path_new_delegate = GetSymbol<Delegates.sk_path_new>("sk_path_new")))();
	}

	internal static bool sk_path_parse_svg_string(IntPtr cpath, [MarshalAs(UnmanagedType.LPStr)] string str)
	{
		return (sk_path_parse_svg_string_delegate ?? (sk_path_parse_svg_string_delegate = GetSymbol<Delegates.sk_path_parse_svg_string>("sk_path_parse_svg_string")))(cpath, str);
	}

	internal static void sk_path_quad_to(IntPtr param0, float x0, float y0, float x1, float y1)
	{
		(sk_path_quad_to_delegate ?? (sk_path_quad_to_delegate = GetSymbol<Delegates.sk_path_quad_to>("sk_path_quad_to")))(param0, x0, y0, x1, y1);
	}

	internal static void sk_path_rarc_to(IntPtr param0, float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y)
	{
		(sk_path_rarc_to_delegate ?? (sk_path_rarc_to_delegate = GetSymbol<Delegates.sk_path_rarc_to>("sk_path_rarc_to")))(param0, rx, ry, xAxisRotate, largeArc, sweep, x, y);
	}

	internal static float sk_path_rawiter_conic_weight(IntPtr iterator)
	{
		return (sk_path_rawiter_conic_weight_delegate ?? (sk_path_rawiter_conic_weight_delegate = GetSymbol<Delegates.sk_path_rawiter_conic_weight>("sk_path_rawiter_conic_weight")))(iterator);
	}

	internal static void sk_path_rawiter_destroy(IntPtr iterator)
	{
		(sk_path_rawiter_destroy_delegate ?? (sk_path_rawiter_destroy_delegate = GetSymbol<Delegates.sk_path_rawiter_destroy>("sk_path_rawiter_destroy")))(iterator);
	}

	internal unsafe static SKPathVerb sk_path_rawiter_next(IntPtr iterator, SKPoint* points)
	{
		return (sk_path_rawiter_next_delegate ?? (sk_path_rawiter_next_delegate = GetSymbol<Delegates.sk_path_rawiter_next>("sk_path_rawiter_next")))(iterator, points);
	}

	internal static SKPathVerb sk_path_rawiter_peek(IntPtr iterator)
	{
		return (sk_path_rawiter_peek_delegate ?? (sk_path_rawiter_peek_delegate = GetSymbol<Delegates.sk_path_rawiter_peek>("sk_path_rawiter_peek")))(iterator);
	}

	internal static void sk_path_rconic_to(IntPtr param0, float dx0, float dy0, float dx1, float dy1, float w)
	{
		(sk_path_rconic_to_delegate ?? (sk_path_rconic_to_delegate = GetSymbol<Delegates.sk_path_rconic_to>("sk_path_rconic_to")))(param0, dx0, dy0, dx1, dy1, w);
	}

	internal static void sk_path_rcubic_to(IntPtr param0, float dx0, float dy0, float dx1, float dy1, float dx2, float dy2)
	{
		(sk_path_rcubic_to_delegate ?? (sk_path_rcubic_to_delegate = GetSymbol<Delegates.sk_path_rcubic_to>("sk_path_rcubic_to")))(param0, dx0, dy0, dx1, dy1, dx2, dy2);
	}

	internal static void sk_path_reset(IntPtr cpath)
	{
		(sk_path_reset_delegate ?? (sk_path_reset_delegate = GetSymbol<Delegates.sk_path_reset>("sk_path_reset")))(cpath);
	}

	internal static void sk_path_rewind(IntPtr cpath)
	{
		(sk_path_rewind_delegate ?? (sk_path_rewind_delegate = GetSymbol<Delegates.sk_path_rewind>("sk_path_rewind")))(cpath);
	}

	internal static void sk_path_rline_to(IntPtr param0, float dx, float yd)
	{
		(sk_path_rline_to_delegate ?? (sk_path_rline_to_delegate = GetSymbol<Delegates.sk_path_rline_to>("sk_path_rline_to")))(param0, dx, yd);
	}

	internal static void sk_path_rmove_to(IntPtr param0, float dx, float dy)
	{
		(sk_path_rmove_to_delegate ?? (sk_path_rmove_to_delegate = GetSymbol<Delegates.sk_path_rmove_to>("sk_path_rmove_to")))(param0, dx, dy);
	}

	internal static void sk_path_rquad_to(IntPtr param0, float dx0, float dy0, float dx1, float dy1)
	{
		(sk_path_rquad_to_delegate ?? (sk_path_rquad_to_delegate = GetSymbol<Delegates.sk_path_rquad_to>("sk_path_rquad_to")))(param0, dx0, dy0, dx1, dy1);
	}

	internal static void sk_path_set_filltype(IntPtr param0, SKPathFillType param1)
	{
		(sk_path_set_filltype_delegate ?? (sk_path_set_filltype_delegate = GetSymbol<Delegates.sk_path_set_filltype>("sk_path_set_filltype")))(param0, param1);
	}

	internal static void sk_path_to_svg_string(IntPtr cpath, IntPtr str)
	{
		(sk_path_to_svg_string_delegate ?? (sk_path_to_svg_string_delegate = GetSymbol<Delegates.sk_path_to_svg_string>("sk_path_to_svg_string")))(cpath, str);
	}

	internal unsafe static void sk_path_transform(IntPtr cpath, SKMatrix* cmatrix)
	{
		(sk_path_transform_delegate ?? (sk_path_transform_delegate = GetSymbol<Delegates.sk_path_transform>("sk_path_transform")))(cpath, cmatrix);
	}

	internal unsafe static void sk_path_transform_to_dest(IntPtr cpath, SKMatrix* cmatrix, IntPtr destination)
	{
		(sk_path_transform_to_dest_delegate ?? (sk_path_transform_to_dest_delegate = GetSymbol<Delegates.sk_path_transform_to_dest>("sk_path_transform_to_dest")))(cpath, cmatrix, destination);
	}

	internal static void sk_pathmeasure_destroy(IntPtr pathMeasure)
	{
		(sk_pathmeasure_destroy_delegate ?? (sk_pathmeasure_destroy_delegate = GetSymbol<Delegates.sk_pathmeasure_destroy>("sk_pathmeasure_destroy")))(pathMeasure);
	}

	internal static float sk_pathmeasure_get_length(IntPtr pathMeasure)
	{
		return (sk_pathmeasure_get_length_delegate ?? (sk_pathmeasure_get_length_delegate = GetSymbol<Delegates.sk_pathmeasure_get_length>("sk_pathmeasure_get_length")))(pathMeasure);
	}

	internal unsafe static bool sk_pathmeasure_get_matrix(IntPtr pathMeasure, float distance, SKMatrix* matrix, SKPathMeasureMatrixFlags flags)
	{
		return (sk_pathmeasure_get_matrix_delegate ?? (sk_pathmeasure_get_matrix_delegate = GetSymbol<Delegates.sk_pathmeasure_get_matrix>("sk_pathmeasure_get_matrix")))(pathMeasure, distance, matrix, flags);
	}

	internal unsafe static bool sk_pathmeasure_get_pos_tan(IntPtr pathMeasure, float distance, SKPoint* position, SKPoint* tangent)
	{
		return (sk_pathmeasure_get_pos_tan_delegate ?? (sk_pathmeasure_get_pos_tan_delegate = GetSymbol<Delegates.sk_pathmeasure_get_pos_tan>("sk_pathmeasure_get_pos_tan")))(pathMeasure, distance, position, tangent);
	}

	internal static bool sk_pathmeasure_get_segment(IntPtr pathMeasure, float start, float stop, IntPtr dst, [MarshalAs(UnmanagedType.I1)] bool startWithMoveTo)
	{
		return (sk_pathmeasure_get_segment_delegate ?? (sk_pathmeasure_get_segment_delegate = GetSymbol<Delegates.sk_pathmeasure_get_segment>("sk_pathmeasure_get_segment")))(pathMeasure, start, stop, dst, startWithMoveTo);
	}

	internal static bool sk_pathmeasure_is_closed(IntPtr pathMeasure)
	{
		return (sk_pathmeasure_is_closed_delegate ?? (sk_pathmeasure_is_closed_delegate = GetSymbol<Delegates.sk_pathmeasure_is_closed>("sk_pathmeasure_is_closed")))(pathMeasure);
	}

	internal static IntPtr sk_pathmeasure_new()
	{
		return (sk_pathmeasure_new_delegate ?? (sk_pathmeasure_new_delegate = GetSymbol<Delegates.sk_pathmeasure_new>("sk_pathmeasure_new")))();
	}

	internal static IntPtr sk_pathmeasure_new_with_path(IntPtr path, [MarshalAs(UnmanagedType.I1)] bool forceClosed, float resScale)
	{
		return (sk_pathmeasure_new_with_path_delegate ?? (sk_pathmeasure_new_with_path_delegate = GetSymbol<Delegates.sk_pathmeasure_new_with_path>("sk_pathmeasure_new_with_path")))(path, forceClosed, resScale);
	}

	internal static bool sk_pathmeasure_next_contour(IntPtr pathMeasure)
	{
		return (sk_pathmeasure_next_contour_delegate ?? (sk_pathmeasure_next_contour_delegate = GetSymbol<Delegates.sk_pathmeasure_next_contour>("sk_pathmeasure_next_contour")))(pathMeasure);
	}

	internal static void sk_pathmeasure_set_path(IntPtr pathMeasure, IntPtr path, [MarshalAs(UnmanagedType.I1)] bool forceClosed)
	{
		(sk_pathmeasure_set_path_delegate ?? (sk_pathmeasure_set_path_delegate = GetSymbol<Delegates.sk_pathmeasure_set_path>("sk_pathmeasure_set_path")))(pathMeasure, path, forceClosed);
	}

	internal static bool sk_pathop_as_winding(IntPtr path, IntPtr result)
	{
		return (sk_pathop_as_winding_delegate ?? (sk_pathop_as_winding_delegate = GetSymbol<Delegates.sk_pathop_as_winding>("sk_pathop_as_winding")))(path, result);
	}

	internal static bool sk_pathop_op(IntPtr one, IntPtr two, SKPathOp op, IntPtr result)
	{
		return (sk_pathop_op_delegate ?? (sk_pathop_op_delegate = GetSymbol<Delegates.sk_pathop_op>("sk_pathop_op")))(one, two, op, result);
	}

	internal static bool sk_pathop_simplify(IntPtr path, IntPtr result)
	{
		return (sk_pathop_simplify_delegate ?? (sk_pathop_simplify_delegate = GetSymbol<Delegates.sk_pathop_simplify>("sk_pathop_simplify")))(path, result);
	}

	internal unsafe static bool sk_pathop_tight_bounds(IntPtr path, SKRect* result)
	{
		return (sk_pathop_tight_bounds_delegate ?? (sk_pathop_tight_bounds_delegate = GetSymbol<Delegates.sk_pathop_tight_bounds>("sk_pathop_tight_bounds")))(path, result);
	}

	internal static IntPtr sk_path_effect_create_1d_path(IntPtr path, float advance, float phase, SKPath1DPathEffectStyle style)
	{
		return (sk_path_effect_create_1d_path_delegate ?? (sk_path_effect_create_1d_path_delegate = GetSymbol<Delegates.sk_path_effect_create_1d_path>("sk_path_effect_create_1d_path")))(path, advance, phase, style);
	}

	internal unsafe static IntPtr sk_path_effect_create_2d_line(float width, SKMatrix* matrix)
	{
		return (sk_path_effect_create_2d_line_delegate ?? (sk_path_effect_create_2d_line_delegate = GetSymbol<Delegates.sk_path_effect_create_2d_line>("sk_path_effect_create_2d_line")))(width, matrix);
	}

	internal unsafe static IntPtr sk_path_effect_create_2d_path(SKMatrix* matrix, IntPtr path)
	{
		return (sk_path_effect_create_2d_path_delegate ?? (sk_path_effect_create_2d_path_delegate = GetSymbol<Delegates.sk_path_effect_create_2d_path>("sk_path_effect_create_2d_path")))(matrix, path);
	}

	internal static IntPtr sk_path_effect_create_compose(IntPtr outer, IntPtr inner)
	{
		return (sk_path_effect_create_compose_delegate ?? (sk_path_effect_create_compose_delegate = GetSymbol<Delegates.sk_path_effect_create_compose>("sk_path_effect_create_compose")))(outer, inner);
	}

	internal static IntPtr sk_path_effect_create_corner(float radius)
	{
		return (sk_path_effect_create_corner_delegate ?? (sk_path_effect_create_corner_delegate = GetSymbol<Delegates.sk_path_effect_create_corner>("sk_path_effect_create_corner")))(radius);
	}

	internal unsafe static IntPtr sk_path_effect_create_dash(float* intervals, int count, float phase)
	{
		return (sk_path_effect_create_dash_delegate ?? (sk_path_effect_create_dash_delegate = GetSymbol<Delegates.sk_path_effect_create_dash>("sk_path_effect_create_dash")))(intervals, count, phase);
	}

	internal static IntPtr sk_path_effect_create_discrete(float segLength, float deviation, uint seedAssist)
	{
		return (sk_path_effect_create_discrete_delegate ?? (sk_path_effect_create_discrete_delegate = GetSymbol<Delegates.sk_path_effect_create_discrete>("sk_path_effect_create_discrete")))(segLength, deviation, seedAssist);
	}

	internal static IntPtr sk_path_effect_create_sum(IntPtr first, IntPtr second)
	{
		return (sk_path_effect_create_sum_delegate ?? (sk_path_effect_create_sum_delegate = GetSymbol<Delegates.sk_path_effect_create_sum>("sk_path_effect_create_sum")))(first, second);
	}

	internal static IntPtr sk_path_effect_create_trim(float start, float stop, SKTrimPathEffectMode mode)
	{
		return (sk_path_effect_create_trim_delegate ?? (sk_path_effect_create_trim_delegate = GetSymbol<Delegates.sk_path_effect_create_trim>("sk_path_effect_create_trim")))(start, stop, mode);
	}

	internal static void sk_path_effect_unref(IntPtr t)
	{
		(sk_path_effect_unref_delegate ?? (sk_path_effect_unref_delegate = GetSymbol<Delegates.sk_path_effect_unref>("sk_path_effect_unref")))(t);
	}

	internal static IntPtr sk_picture_approximate_bytes_used(IntPtr picture)
	{
		return (sk_picture_approximate_bytes_used_delegate ?? (sk_picture_approximate_bytes_used_delegate = GetSymbol<Delegates.sk_picture_approximate_bytes_used>("sk_picture_approximate_bytes_used")))(picture);
	}

	internal static int sk_picture_approximate_op_count(IntPtr picture, [MarshalAs(UnmanagedType.I1)] bool nested)
	{
		return (sk_picture_approximate_op_count_delegate ?? (sk_picture_approximate_op_count_delegate = GetSymbol<Delegates.sk_picture_approximate_op_count>("sk_picture_approximate_op_count")))(picture, nested);
	}

	internal static IntPtr sk_picture_deserialize_from_data(IntPtr data)
	{
		return (sk_picture_deserialize_from_data_delegate ?? (sk_picture_deserialize_from_data_delegate = GetSymbol<Delegates.sk_picture_deserialize_from_data>("sk_picture_deserialize_from_data")))(data);
	}

	internal unsafe static IntPtr sk_picture_deserialize_from_memory(void* buffer, IntPtr length)
	{
		return (sk_picture_deserialize_from_memory_delegate ?? (sk_picture_deserialize_from_memory_delegate = GetSymbol<Delegates.sk_picture_deserialize_from_memory>("sk_picture_deserialize_from_memory")))(buffer, length);
	}

	internal static IntPtr sk_picture_deserialize_from_stream(IntPtr stream)
	{
		return (sk_picture_deserialize_from_stream_delegate ?? (sk_picture_deserialize_from_stream_delegate = GetSymbol<Delegates.sk_picture_deserialize_from_stream>("sk_picture_deserialize_from_stream")))(stream);
	}

	internal unsafe static void sk_picture_get_cull_rect(IntPtr param0, SKRect* param1)
	{
		(sk_picture_get_cull_rect_delegate ?? (sk_picture_get_cull_rect_delegate = GetSymbol<Delegates.sk_picture_get_cull_rect>("sk_picture_get_cull_rect")))(param0, param1);
	}

	internal static IntPtr sk_picture_get_recording_canvas(IntPtr crec)
	{
		return (sk_picture_get_recording_canvas_delegate ?? (sk_picture_get_recording_canvas_delegate = GetSymbol<Delegates.sk_picture_get_recording_canvas>("sk_picture_get_recording_canvas")))(crec);
	}

	internal static uint sk_picture_get_unique_id(IntPtr param0)
	{
		return (sk_picture_get_unique_id_delegate ?? (sk_picture_get_unique_id_delegate = GetSymbol<Delegates.sk_picture_get_unique_id>("sk_picture_get_unique_id")))(param0);
	}

	internal unsafe static IntPtr sk_picture_make_shader(IntPtr src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode mode, SKMatrix* localMatrix, SKRect* tile)
	{
		return (sk_picture_make_shader_delegate ?? (sk_picture_make_shader_delegate = GetSymbol<Delegates.sk_picture_make_shader>("sk_picture_make_shader")))(src, tmx, tmy, mode, localMatrix, tile);
	}

	internal static void sk_picture_playback(IntPtr picture, IntPtr canvas)
	{
		(sk_picture_playback_delegate ?? (sk_picture_playback_delegate = GetSymbol<Delegates.sk_picture_playback>("sk_picture_playback")))(picture, canvas);
	}

	internal unsafe static IntPtr sk_picture_recorder_begin_recording(IntPtr param0, SKRect* param1)
	{
		return (sk_picture_recorder_begin_recording_delegate ?? (sk_picture_recorder_begin_recording_delegate = GetSymbol<Delegates.sk_picture_recorder_begin_recording>("sk_picture_recorder_begin_recording")))(param0, param1);
	}

	internal unsafe static IntPtr sk_picture_recorder_begin_recording_with_bbh_factory(IntPtr param0, SKRect* param1, IntPtr param2)
	{
		return (sk_picture_recorder_begin_recording_with_bbh_factory_delegate ?? (sk_picture_recorder_begin_recording_with_bbh_factory_delegate = GetSymbol<Delegates.sk_picture_recorder_begin_recording_with_bbh_factory>("sk_picture_recorder_begin_recording_with_bbh_factory")))(param0, param1, param2);
	}

	internal static void sk_picture_recorder_delete(IntPtr param0)
	{
		(sk_picture_recorder_delete_delegate ?? (sk_picture_recorder_delete_delegate = GetSymbol<Delegates.sk_picture_recorder_delete>("sk_picture_recorder_delete")))(param0);
	}

	internal static IntPtr sk_picture_recorder_end_recording(IntPtr param0)
	{
		return (sk_picture_recorder_end_recording_delegate ?? (sk_picture_recorder_end_recording_delegate = GetSymbol<Delegates.sk_picture_recorder_end_recording>("sk_picture_recorder_end_recording")))(param0);
	}

	internal static IntPtr sk_picture_recorder_end_recording_as_drawable(IntPtr param0)
	{
		return (sk_picture_recorder_end_recording_as_drawable_delegate ?? (sk_picture_recorder_end_recording_as_drawable_delegate = GetSymbol<Delegates.sk_picture_recorder_end_recording_as_drawable>("sk_picture_recorder_end_recording_as_drawable")))(param0);
	}

	internal static IntPtr sk_picture_recorder_new()
	{
		return (sk_picture_recorder_new_delegate ?? (sk_picture_recorder_new_delegate = GetSymbol<Delegates.sk_picture_recorder_new>("sk_picture_recorder_new")))();
	}

	internal static void sk_picture_ref(IntPtr param0)
	{
		(sk_picture_ref_delegate ?? (sk_picture_ref_delegate = GetSymbol<Delegates.sk_picture_ref>("sk_picture_ref")))(param0);
	}

	internal static IntPtr sk_picture_serialize_to_data(IntPtr picture)
	{
		return (sk_picture_serialize_to_data_delegate ?? (sk_picture_serialize_to_data_delegate = GetSymbol<Delegates.sk_picture_serialize_to_data>("sk_picture_serialize_to_data")))(picture);
	}

	internal static void sk_picture_serialize_to_stream(IntPtr picture, IntPtr stream)
	{
		(sk_picture_serialize_to_stream_delegate ?? (sk_picture_serialize_to_stream_delegate = GetSymbol<Delegates.sk_picture_serialize_to_stream>("sk_picture_serialize_to_stream")))(picture, stream);
	}

	internal static void sk_picture_unref(IntPtr param0)
	{
		(sk_picture_unref_delegate ?? (sk_picture_unref_delegate = GetSymbol<Delegates.sk_picture_unref>("sk_picture_unref")))(param0);
	}

	internal static void sk_rtree_factory_delete(IntPtr param0)
	{
		(sk_rtree_factory_delete_delegate ?? (sk_rtree_factory_delete_delegate = GetSymbol<Delegates.sk_rtree_factory_delete>("sk_rtree_factory_delete")))(param0);
	}

	internal static IntPtr sk_rtree_factory_new()
	{
		return (sk_rtree_factory_new_delegate ?? (sk_rtree_factory_new_delegate = GetSymbol<Delegates.sk_rtree_factory_new>("sk_rtree_factory_new")))();
	}

	internal unsafe static void sk_color_get_bit_shift(int* a, int* r, int* g, int* b)
	{
		(sk_color_get_bit_shift_delegate ?? (sk_color_get_bit_shift_delegate = GetSymbol<Delegates.sk_color_get_bit_shift>("sk_color_get_bit_shift")))(a, r, g, b);
	}

	internal static uint sk_color_premultiply(uint color)
	{
		return (sk_color_premultiply_delegate ?? (sk_color_premultiply_delegate = GetSymbol<Delegates.sk_color_premultiply>("sk_color_premultiply")))(color);
	}

	internal unsafe static void sk_color_premultiply_array(uint* colors, int size, uint* pmcolors)
	{
		(sk_color_premultiply_array_delegate ?? (sk_color_premultiply_array_delegate = GetSymbol<Delegates.sk_color_premultiply_array>("sk_color_premultiply_array")))(colors, size, pmcolors);
	}

	internal static uint sk_color_unpremultiply(uint pmcolor)
	{
		return (sk_color_unpremultiply_delegate ?? (sk_color_unpremultiply_delegate = GetSymbol<Delegates.sk_color_unpremultiply>("sk_color_unpremultiply")))(pmcolor);
	}

	internal unsafe static void sk_color_unpremultiply_array(uint* pmcolors, int size, uint* colors)
	{
		(sk_color_unpremultiply_array_delegate ?? (sk_color_unpremultiply_array_delegate = GetSymbol<Delegates.sk_color_unpremultiply_array>("sk_color_unpremultiply_array")))(pmcolors, size, colors);
	}

	internal unsafe static bool sk_jpegencoder_encode(IntPtr dst, IntPtr src, SKJpegEncoderOptions* options)
	{
		return (sk_jpegencoder_encode_delegate ?? (sk_jpegencoder_encode_delegate = GetSymbol<Delegates.sk_jpegencoder_encode>("sk_jpegencoder_encode")))(dst, src, options);
	}

	internal static bool sk_pixmap_compute_is_opaque(IntPtr cpixmap)
	{
		return (sk_pixmap_compute_is_opaque_delegate ?? (sk_pixmap_compute_is_opaque_delegate = GetSymbol<Delegates.sk_pixmap_compute_is_opaque>("sk_pixmap_compute_is_opaque")))(cpixmap);
	}

	internal static void sk_pixmap_destructor(IntPtr cpixmap)
	{
		(sk_pixmap_destructor_delegate ?? (sk_pixmap_destructor_delegate = GetSymbol<Delegates.sk_pixmap_destructor>("sk_pixmap_destructor")))(cpixmap);
	}

	internal unsafe static bool sk_pixmap_erase_color(IntPtr cpixmap, uint color, SKRectI* subset)
	{
		return (sk_pixmap_erase_color_delegate ?? (sk_pixmap_erase_color_delegate = GetSymbol<Delegates.sk_pixmap_erase_color>("sk_pixmap_erase_color")))(cpixmap, color, subset);
	}

	internal unsafe static bool sk_pixmap_erase_color4f(IntPtr cpixmap, SKColorF* color, SKRectI* subset)
	{
		return (sk_pixmap_erase_color4f_delegate ?? (sk_pixmap_erase_color4f_delegate = GetSymbol<Delegates.sk_pixmap_erase_color4f>("sk_pixmap_erase_color4f")))(cpixmap, color, subset);
	}

	internal unsafe static bool sk_pixmap_extract_subset(IntPtr cpixmap, IntPtr result, SKRectI* subset)
	{
		return (sk_pixmap_extract_subset_delegate ?? (sk_pixmap_extract_subset_delegate = GetSymbol<Delegates.sk_pixmap_extract_subset>("sk_pixmap_extract_subset")))(cpixmap, result, subset);
	}

	internal static IntPtr sk_pixmap_get_colorspace(IntPtr cpixmap)
	{
		return (sk_pixmap_get_colorspace_delegate ?? (sk_pixmap_get_colorspace_delegate = GetSymbol<Delegates.sk_pixmap_get_colorspace>("sk_pixmap_get_colorspace")))(cpixmap);
	}

	internal unsafe static void sk_pixmap_get_info(IntPtr cpixmap, SKImageInfoNative* cinfo)
	{
		(sk_pixmap_get_info_delegate ?? (sk_pixmap_get_info_delegate = GetSymbol<Delegates.sk_pixmap_get_info>("sk_pixmap_get_info")))(cpixmap, cinfo);
	}

	internal static float sk_pixmap_get_pixel_alphaf(IntPtr cpixmap, int x, int y)
	{
		return (sk_pixmap_get_pixel_alphaf_delegate ?? (sk_pixmap_get_pixel_alphaf_delegate = GetSymbol<Delegates.sk_pixmap_get_pixel_alphaf>("sk_pixmap_get_pixel_alphaf")))(cpixmap, x, y);
	}

	internal static uint sk_pixmap_get_pixel_color(IntPtr cpixmap, int x, int y)
	{
		return (sk_pixmap_get_pixel_color_delegate ?? (sk_pixmap_get_pixel_color_delegate = GetSymbol<Delegates.sk_pixmap_get_pixel_color>("sk_pixmap_get_pixel_color")))(cpixmap, x, y);
	}

	internal unsafe static void sk_pixmap_get_pixel_color4f(IntPtr cpixmap, int x, int y, SKColorF* color)
	{
		(sk_pixmap_get_pixel_color4f_delegate ?? (sk_pixmap_get_pixel_color4f_delegate = GetSymbol<Delegates.sk_pixmap_get_pixel_color4f>("sk_pixmap_get_pixel_color4f")))(cpixmap, x, y, color);
	}

	internal static IntPtr sk_pixmap_get_row_bytes(IntPtr cpixmap)
	{
		return (sk_pixmap_get_row_bytes_delegate ?? (sk_pixmap_get_row_bytes_delegate = GetSymbol<Delegates.sk_pixmap_get_row_bytes>("sk_pixmap_get_row_bytes")))(cpixmap);
	}

	internal unsafe static void* sk_pixmap_get_writable_addr(IntPtr cpixmap)
	{
		return (sk_pixmap_get_writable_addr_delegate ?? (sk_pixmap_get_writable_addr_delegate = GetSymbol<Delegates.sk_pixmap_get_writable_addr>("sk_pixmap_get_writable_addr")))(cpixmap);
	}

	internal unsafe static void* sk_pixmap_get_writeable_addr_with_xy(IntPtr cpixmap, int x, int y)
	{
		return (sk_pixmap_get_writeable_addr_with_xy_delegate ?? (sk_pixmap_get_writeable_addr_with_xy_delegate = GetSymbol<Delegates.sk_pixmap_get_writeable_addr_with_xy>("sk_pixmap_get_writeable_addr_with_xy")))(cpixmap, x, y);
	}

	internal static IntPtr sk_pixmap_new()
	{
		return (sk_pixmap_new_delegate ?? (sk_pixmap_new_delegate = GetSymbol<Delegates.sk_pixmap_new>("sk_pixmap_new")))();
	}

	internal unsafe static IntPtr sk_pixmap_new_with_params(SKImageInfoNative* cinfo, void* addr, IntPtr rowBytes)
	{
		return (sk_pixmap_new_with_params_delegate ?? (sk_pixmap_new_with_params_delegate = GetSymbol<Delegates.sk_pixmap_new_with_params>("sk_pixmap_new_with_params")))(cinfo, addr, rowBytes);
	}

	internal unsafe static bool sk_pixmap_read_pixels(IntPtr cpixmap, SKImageInfoNative* dstInfo, void* dstPixels, IntPtr dstRowBytes, int srcX, int srcY)
	{
		return (sk_pixmap_read_pixels_delegate ?? (sk_pixmap_read_pixels_delegate = GetSymbol<Delegates.sk_pixmap_read_pixels>("sk_pixmap_read_pixels")))(cpixmap, dstInfo, dstPixels, dstRowBytes, srcX, srcY);
	}

	internal static void sk_pixmap_reset(IntPtr cpixmap)
	{
		(sk_pixmap_reset_delegate ?? (sk_pixmap_reset_delegate = GetSymbol<Delegates.sk_pixmap_reset>("sk_pixmap_reset")))(cpixmap);
	}

	internal unsafe static void sk_pixmap_reset_with_params(IntPtr cpixmap, SKImageInfoNative* cinfo, void* addr, IntPtr rowBytes)
	{
		(sk_pixmap_reset_with_params_delegate ?? (sk_pixmap_reset_with_params_delegate = GetSymbol<Delegates.sk_pixmap_reset_with_params>("sk_pixmap_reset_with_params")))(cpixmap, cinfo, addr, rowBytes);
	}

	internal unsafe static bool sk_pixmap_scale_pixels(IntPtr cpixmap, IntPtr dst, SKSamplingOptions* sampling)
	{
		return (sk_pixmap_scale_pixels_delegate ?? (sk_pixmap_scale_pixels_delegate = GetSymbol<Delegates.sk_pixmap_scale_pixels>("sk_pixmap_scale_pixels")))(cpixmap, dst, sampling);
	}

	internal static void sk_pixmap_set_colorspace(IntPtr cpixmap, IntPtr colorspace)
	{
		(sk_pixmap_set_colorspace_delegate ?? (sk_pixmap_set_colorspace_delegate = GetSymbol<Delegates.sk_pixmap_set_colorspace>("sk_pixmap_set_colorspace")))(cpixmap, colorspace);
	}

	internal unsafe static bool sk_pngencoder_encode(IntPtr dst, IntPtr src, SKPngEncoderOptions* options)
	{
		return (sk_pngencoder_encode_delegate ?? (sk_pngencoder_encode_delegate = GetSymbol<Delegates.sk_pngencoder_encode>("sk_pngencoder_encode")))(dst, src, options);
	}

	internal unsafe static void sk_swizzle_swap_rb(uint* dest, uint* src, int count)
	{
		(sk_swizzle_swap_rb_delegate ?? (sk_swizzle_swap_rb_delegate = GetSymbol<Delegates.sk_swizzle_swap_rb>("sk_swizzle_swap_rb")))(dest, src, count);
	}

	internal unsafe static bool sk_webpencoder_encode(IntPtr dst, IntPtr src, SKWebpEncoderOptions* options)
	{
		return (sk_webpencoder_encode_delegate ?? (sk_webpencoder_encode_delegate = GetSymbol<Delegates.sk_webpencoder_encode>("sk_webpencoder_encode")))(dst, src, options);
	}

	internal static void sk_region_cliperator_delete(IntPtr iter)
	{
		(sk_region_cliperator_delete_delegate ?? (sk_region_cliperator_delete_delegate = GetSymbol<Delegates.sk_region_cliperator_delete>("sk_region_cliperator_delete")))(iter);
	}

	internal static bool sk_region_cliperator_done(IntPtr iter)
	{
		return (sk_region_cliperator_done_delegate ?? (sk_region_cliperator_done_delegate = GetSymbol<Delegates.sk_region_cliperator_done>("sk_region_cliperator_done")))(iter);
	}

	internal unsafe static IntPtr sk_region_cliperator_new(IntPtr region, SKRectI* clip)
	{
		return (sk_region_cliperator_new_delegate ?? (sk_region_cliperator_new_delegate = GetSymbol<Delegates.sk_region_cliperator_new>("sk_region_cliperator_new")))(region, clip);
	}

	internal static void sk_region_cliperator_next(IntPtr iter)
	{
		(sk_region_cliperator_next_delegate ?? (sk_region_cliperator_next_delegate = GetSymbol<Delegates.sk_region_cliperator_next>("sk_region_cliperator_next")))(iter);
	}

	internal unsafe static void sk_region_cliperator_rect(IntPtr iter, SKRectI* rect)
	{
		(sk_region_cliperator_rect_delegate ?? (sk_region_cliperator_rect_delegate = GetSymbol<Delegates.sk_region_cliperator_rect>("sk_region_cliperator_rect")))(iter, rect);
	}

	internal static bool sk_region_contains(IntPtr r, IntPtr region)
	{
		return (sk_region_contains_delegate ?? (sk_region_contains_delegate = GetSymbol<Delegates.sk_region_contains>("sk_region_contains")))(r, region);
	}

	internal static bool sk_region_contains_point(IntPtr r, int x, int y)
	{
		return (sk_region_contains_point_delegate ?? (sk_region_contains_point_delegate = GetSymbol<Delegates.sk_region_contains_point>("sk_region_contains_point")))(r, x, y);
	}

	internal unsafe static bool sk_region_contains_rect(IntPtr r, SKRectI* rect)
	{
		return (sk_region_contains_rect_delegate ?? (sk_region_contains_rect_delegate = GetSymbol<Delegates.sk_region_contains_rect>("sk_region_contains_rect")))(r, rect);
	}

	internal static void sk_region_delete(IntPtr r)
	{
		(sk_region_delete_delegate ?? (sk_region_delete_delegate = GetSymbol<Delegates.sk_region_delete>("sk_region_delete")))(r);
	}

	internal static bool sk_region_get_boundary_path(IntPtr r, IntPtr path)
	{
		return (sk_region_get_boundary_path_delegate ?? (sk_region_get_boundary_path_delegate = GetSymbol<Delegates.sk_region_get_boundary_path>("sk_region_get_boundary_path")))(r, path);
	}

	internal unsafe static void sk_region_get_bounds(IntPtr r, SKRectI* rect)
	{
		(sk_region_get_bounds_delegate ?? (sk_region_get_bounds_delegate = GetSymbol<Delegates.sk_region_get_bounds>("sk_region_get_bounds")))(r, rect);
	}

	internal static bool sk_region_intersects(IntPtr r, IntPtr src)
	{
		return (sk_region_intersects_delegate ?? (sk_region_intersects_delegate = GetSymbol<Delegates.sk_region_intersects>("sk_region_intersects")))(r, src);
	}

	internal unsafe static bool sk_region_intersects_rect(IntPtr r, SKRectI* rect)
	{
		return (sk_region_intersects_rect_delegate ?? (sk_region_intersects_rect_delegate = GetSymbol<Delegates.sk_region_intersects_rect>("sk_region_intersects_rect")))(r, rect);
	}

	internal static bool sk_region_is_complex(IntPtr r)
	{
		return (sk_region_is_complex_delegate ?? (sk_region_is_complex_delegate = GetSymbol<Delegates.sk_region_is_complex>("sk_region_is_complex")))(r);
	}

	internal static bool sk_region_is_empty(IntPtr r)
	{
		return (sk_region_is_empty_delegate ?? (sk_region_is_empty_delegate = GetSymbol<Delegates.sk_region_is_empty>("sk_region_is_empty")))(r);
	}

	internal static bool sk_region_is_rect(IntPtr r)
	{
		return (sk_region_is_rect_delegate ?? (sk_region_is_rect_delegate = GetSymbol<Delegates.sk_region_is_rect>("sk_region_is_rect")))(r);
	}

	internal static void sk_region_iterator_delete(IntPtr iter)
	{
		(sk_region_iterator_delete_delegate ?? (sk_region_iterator_delete_delegate = GetSymbol<Delegates.sk_region_iterator_delete>("sk_region_iterator_delete")))(iter);
	}

	internal static bool sk_region_iterator_done(IntPtr iter)
	{
		return (sk_region_iterator_done_delegate ?? (sk_region_iterator_done_delegate = GetSymbol<Delegates.sk_region_iterator_done>("sk_region_iterator_done")))(iter);
	}

	internal static IntPtr sk_region_iterator_new(IntPtr region)
	{
		return (sk_region_iterator_new_delegate ?? (sk_region_iterator_new_delegate = GetSymbol<Delegates.sk_region_iterator_new>("sk_region_iterator_new")))(region);
	}

	internal static void sk_region_iterator_next(IntPtr iter)
	{
		(sk_region_iterator_next_delegate ?? (sk_region_iterator_next_delegate = GetSymbol<Delegates.sk_region_iterator_next>("sk_region_iterator_next")))(iter);
	}

	internal unsafe static void sk_region_iterator_rect(IntPtr iter, SKRectI* rect)
	{
		(sk_region_iterator_rect_delegate ?? (sk_region_iterator_rect_delegate = GetSymbol<Delegates.sk_region_iterator_rect>("sk_region_iterator_rect")))(iter, rect);
	}

	internal static bool sk_region_iterator_rewind(IntPtr iter)
	{
		return (sk_region_iterator_rewind_delegate ?? (sk_region_iterator_rewind_delegate = GetSymbol<Delegates.sk_region_iterator_rewind>("sk_region_iterator_rewind")))(iter);
	}

	internal static IntPtr sk_region_new()
	{
		return (sk_region_new_delegate ?? (sk_region_new_delegate = GetSymbol<Delegates.sk_region_new>("sk_region_new")))();
	}

	internal static bool sk_region_op(IntPtr r, IntPtr region, SKRegionOperation op)
	{
		return (sk_region_op_delegate ?? (sk_region_op_delegate = GetSymbol<Delegates.sk_region_op>("sk_region_op")))(r, region, op);
	}

	internal unsafe static bool sk_region_op_rect(IntPtr r, SKRectI* rect, SKRegionOperation op)
	{
		return (sk_region_op_rect_delegate ?? (sk_region_op_rect_delegate = GetSymbol<Delegates.sk_region_op_rect>("sk_region_op_rect")))(r, rect, op);
	}

	internal unsafe static bool sk_region_quick_contains(IntPtr r, SKRectI* rect)
	{
		return (sk_region_quick_contains_delegate ?? (sk_region_quick_contains_delegate = GetSymbol<Delegates.sk_region_quick_contains>("sk_region_quick_contains")))(r, rect);
	}

	internal static bool sk_region_quick_reject(IntPtr r, IntPtr region)
	{
		return (sk_region_quick_reject_delegate ?? (sk_region_quick_reject_delegate = GetSymbol<Delegates.sk_region_quick_reject>("sk_region_quick_reject")))(r, region);
	}

	internal unsafe static bool sk_region_quick_reject_rect(IntPtr r, SKRectI* rect)
	{
		return (sk_region_quick_reject_rect_delegate ?? (sk_region_quick_reject_rect_delegate = GetSymbol<Delegates.sk_region_quick_reject_rect>("sk_region_quick_reject_rect")))(r, rect);
	}

	internal static bool sk_region_set_empty(IntPtr r)
	{
		return (sk_region_set_empty_delegate ?? (sk_region_set_empty_delegate = GetSymbol<Delegates.sk_region_set_empty>("sk_region_set_empty")))(r);
	}

	internal static bool sk_region_set_path(IntPtr r, IntPtr t, IntPtr clip)
	{
		return (sk_region_set_path_delegate ?? (sk_region_set_path_delegate = GetSymbol<Delegates.sk_region_set_path>("sk_region_set_path")))(r, t, clip);
	}

	internal unsafe static bool sk_region_set_rect(IntPtr r, SKRectI* rect)
	{
		return (sk_region_set_rect_delegate ?? (sk_region_set_rect_delegate = GetSymbol<Delegates.sk_region_set_rect>("sk_region_set_rect")))(r, rect);
	}

	internal unsafe static bool sk_region_set_rects(IntPtr r, SKRectI* rects, int count)
	{
		return (sk_region_set_rects_delegate ?? (sk_region_set_rects_delegate = GetSymbol<Delegates.sk_region_set_rects>("sk_region_set_rects")))(r, rects, count);
	}

	internal static bool sk_region_set_region(IntPtr r, IntPtr region)
	{
		return (sk_region_set_region_delegate ?? (sk_region_set_region_delegate = GetSymbol<Delegates.sk_region_set_region>("sk_region_set_region")))(r, region);
	}

	internal static void sk_region_spanerator_delete(IntPtr iter)
	{
		(sk_region_spanerator_delete_delegate ?? (sk_region_spanerator_delete_delegate = GetSymbol<Delegates.sk_region_spanerator_delete>("sk_region_spanerator_delete")))(iter);
	}

	internal static IntPtr sk_region_spanerator_new(IntPtr region, int y, int left, int right)
	{
		return (sk_region_spanerator_new_delegate ?? (sk_region_spanerator_new_delegate = GetSymbol<Delegates.sk_region_spanerator_new>("sk_region_spanerator_new")))(region, y, left, right);
	}

	internal unsafe static bool sk_region_spanerator_next(IntPtr iter, int* left, int* right)
	{
		return (sk_region_spanerator_next_delegate ?? (sk_region_spanerator_next_delegate = GetSymbol<Delegates.sk_region_spanerator_next>("sk_region_spanerator_next")))(iter, left, right);
	}

	internal static void sk_region_translate(IntPtr r, int x, int y)
	{
		(sk_region_translate_delegate ?? (sk_region_translate_delegate = GetSymbol<Delegates.sk_region_translate>("sk_region_translate")))(r, x, y);
	}

	internal unsafe static bool sk_rrect_contains(IntPtr rrect, SKRect* rect)
	{
		return (sk_rrect_contains_delegate ?? (sk_rrect_contains_delegate = GetSymbol<Delegates.sk_rrect_contains>("sk_rrect_contains")))(rrect, rect);
	}

	internal static void sk_rrect_delete(IntPtr rrect)
	{
		(sk_rrect_delete_delegate ?? (sk_rrect_delete_delegate = GetSymbol<Delegates.sk_rrect_delete>("sk_rrect_delete")))(rrect);
	}

	internal static float sk_rrect_get_height(IntPtr rrect)
	{
		return (sk_rrect_get_height_delegate ?? (sk_rrect_get_height_delegate = GetSymbol<Delegates.sk_rrect_get_height>("sk_rrect_get_height")))(rrect);
	}

	internal unsafe static void sk_rrect_get_radii(IntPtr rrect, SKRoundRectCorner corner, SKPoint* radii)
	{
		(sk_rrect_get_radii_delegate ?? (sk_rrect_get_radii_delegate = GetSymbol<Delegates.sk_rrect_get_radii>("sk_rrect_get_radii")))(rrect, corner, radii);
	}

	internal unsafe static void sk_rrect_get_rect(IntPtr rrect, SKRect* rect)
	{
		(sk_rrect_get_rect_delegate ?? (sk_rrect_get_rect_delegate = GetSymbol<Delegates.sk_rrect_get_rect>("sk_rrect_get_rect")))(rrect, rect);
	}

	internal static SKRoundRectType sk_rrect_get_type(IntPtr rrect)
	{
		return (sk_rrect_get_type_delegate ?? (sk_rrect_get_type_delegate = GetSymbol<Delegates.sk_rrect_get_type>("sk_rrect_get_type")))(rrect);
	}

	internal static float sk_rrect_get_width(IntPtr rrect)
	{
		return (sk_rrect_get_width_delegate ?? (sk_rrect_get_width_delegate = GetSymbol<Delegates.sk_rrect_get_width>("sk_rrect_get_width")))(rrect);
	}

	internal static void sk_rrect_inset(IntPtr rrect, float dx, float dy)
	{
		(sk_rrect_inset_delegate ?? (sk_rrect_inset_delegate = GetSymbol<Delegates.sk_rrect_inset>("sk_rrect_inset")))(rrect, dx, dy);
	}

	internal static bool sk_rrect_is_valid(IntPtr rrect)
	{
		return (sk_rrect_is_valid_delegate ?? (sk_rrect_is_valid_delegate = GetSymbol<Delegates.sk_rrect_is_valid>("sk_rrect_is_valid")))(rrect);
	}

	internal static IntPtr sk_rrect_new()
	{
		return (sk_rrect_new_delegate ?? (sk_rrect_new_delegate = GetSymbol<Delegates.sk_rrect_new>("sk_rrect_new")))();
	}

	internal static IntPtr sk_rrect_new_copy(IntPtr rrect)
	{
		return (sk_rrect_new_copy_delegate ?? (sk_rrect_new_copy_delegate = GetSymbol<Delegates.sk_rrect_new_copy>("sk_rrect_new_copy")))(rrect);
	}

	internal static void sk_rrect_offset(IntPtr rrect, float dx, float dy)
	{
		(sk_rrect_offset_delegate ?? (sk_rrect_offset_delegate = GetSymbol<Delegates.sk_rrect_offset>("sk_rrect_offset")))(rrect, dx, dy);
	}

	internal static void sk_rrect_outset(IntPtr rrect, float dx, float dy)
	{
		(sk_rrect_outset_delegate ?? (sk_rrect_outset_delegate = GetSymbol<Delegates.sk_rrect_outset>("sk_rrect_outset")))(rrect, dx, dy);
	}

	internal static void sk_rrect_set_empty(IntPtr rrect)
	{
		(sk_rrect_set_empty_delegate ?? (sk_rrect_set_empty_delegate = GetSymbol<Delegates.sk_rrect_set_empty>("sk_rrect_set_empty")))(rrect);
	}

	internal unsafe static void sk_rrect_set_nine_patch(IntPtr rrect, SKRect* rect, float leftRad, float topRad, float rightRad, float bottomRad)
	{
		(sk_rrect_set_nine_patch_delegate ?? (sk_rrect_set_nine_patch_delegate = GetSymbol<Delegates.sk_rrect_set_nine_patch>("sk_rrect_set_nine_patch")))(rrect, rect, leftRad, topRad, rightRad, bottomRad);
	}

	internal unsafe static void sk_rrect_set_oval(IntPtr rrect, SKRect* rect)
	{
		(sk_rrect_set_oval_delegate ?? (sk_rrect_set_oval_delegate = GetSymbol<Delegates.sk_rrect_set_oval>("sk_rrect_set_oval")))(rrect, rect);
	}

	internal unsafe static void sk_rrect_set_rect(IntPtr rrect, SKRect* rect)
	{
		(sk_rrect_set_rect_delegate ?? (sk_rrect_set_rect_delegate = GetSymbol<Delegates.sk_rrect_set_rect>("sk_rrect_set_rect")))(rrect, rect);
	}

	internal unsafe static void sk_rrect_set_rect_radii(IntPtr rrect, SKRect* rect, SKPoint* radii)
	{
		(sk_rrect_set_rect_radii_delegate ?? (sk_rrect_set_rect_radii_delegate = GetSymbol<Delegates.sk_rrect_set_rect_radii>("sk_rrect_set_rect_radii")))(rrect, rect, radii);
	}

	internal unsafe static void sk_rrect_set_rect_xy(IntPtr rrect, SKRect* rect, float xRad, float yRad)
	{
		(sk_rrect_set_rect_xy_delegate ?? (sk_rrect_set_rect_xy_delegate = GetSymbol<Delegates.sk_rrect_set_rect_xy>("sk_rrect_set_rect_xy")))(rrect, rect, xRad, yRad);
	}

	internal unsafe static bool sk_rrect_transform(IntPtr rrect, SKMatrix* matrix, IntPtr dest)
	{
		return (sk_rrect_transform_delegate ?? (sk_rrect_transform_delegate = GetSymbol<Delegates.sk_rrect_transform>("sk_rrect_transform")))(rrect, matrix, dest);
	}

	internal unsafe static void sk_runtimeeffect_get_child_from_index(IntPtr effect, int index, SKRuntimeEffectChildNative* cchild)
	{
		(sk_runtimeeffect_get_child_from_index_delegate ?? (sk_runtimeeffect_get_child_from_index_delegate = GetSymbol<Delegates.sk_runtimeeffect_get_child_from_index>("sk_runtimeeffect_get_child_from_index")))(effect, index, cchild);
	}

	internal unsafe static void sk_runtimeeffect_get_child_from_name(IntPtr effect, void* name, IntPtr len, SKRuntimeEffectChildNative* cchild)
	{
		(sk_runtimeeffect_get_child_from_name_delegate ?? (sk_runtimeeffect_get_child_from_name_delegate = GetSymbol<Delegates.sk_runtimeeffect_get_child_from_name>("sk_runtimeeffect_get_child_from_name")))(effect, name, len, cchild);
	}

	internal static void sk_runtimeeffect_get_child_name(IntPtr effect, int index, IntPtr name)
	{
		(sk_runtimeeffect_get_child_name_delegate ?? (sk_runtimeeffect_get_child_name_delegate = GetSymbol<Delegates.sk_runtimeeffect_get_child_name>("sk_runtimeeffect_get_child_name")))(effect, index, name);
	}

	internal static IntPtr sk_runtimeeffect_get_children_size(IntPtr effect)
	{
		return (sk_runtimeeffect_get_children_size_delegate ?? (sk_runtimeeffect_get_children_size_delegate = GetSymbol<Delegates.sk_runtimeeffect_get_children_size>("sk_runtimeeffect_get_children_size")))(effect);
	}

	internal static IntPtr sk_runtimeeffect_get_uniform_byte_size(IntPtr effect)
	{
		return (sk_runtimeeffect_get_uniform_byte_size_delegate ?? (sk_runtimeeffect_get_uniform_byte_size_delegate = GetSymbol<Delegates.sk_runtimeeffect_get_uniform_byte_size>("sk_runtimeeffect_get_uniform_byte_size")))(effect);
	}

	internal unsafe static void sk_runtimeeffect_get_uniform_from_index(IntPtr effect, int index, SKRuntimeEffectUniformNative* cuniform)
	{
		(sk_runtimeeffect_get_uniform_from_index_delegate ?? (sk_runtimeeffect_get_uniform_from_index_delegate = GetSymbol<Delegates.sk_runtimeeffect_get_uniform_from_index>("sk_runtimeeffect_get_uniform_from_index")))(effect, index, cuniform);
	}

	internal unsafe static void sk_runtimeeffect_get_uniform_from_name(IntPtr effect, void* name, IntPtr len, SKRuntimeEffectUniformNative* cuniform)
	{
		(sk_runtimeeffect_get_uniform_from_name_delegate ?? (sk_runtimeeffect_get_uniform_from_name_delegate = GetSymbol<Delegates.sk_runtimeeffect_get_uniform_from_name>("sk_runtimeeffect_get_uniform_from_name")))(effect, name, len, cuniform);
	}

	internal static void sk_runtimeeffect_get_uniform_name(IntPtr effect, int index, IntPtr name)
	{
		(sk_runtimeeffect_get_uniform_name_delegate ?? (sk_runtimeeffect_get_uniform_name_delegate = GetSymbol<Delegates.sk_runtimeeffect_get_uniform_name>("sk_runtimeeffect_get_uniform_name")))(effect, index, name);
	}

	internal static IntPtr sk_runtimeeffect_get_uniforms_size(IntPtr effect)
	{
		return (sk_runtimeeffect_get_uniforms_size_delegate ?? (sk_runtimeeffect_get_uniforms_size_delegate = GetSymbol<Delegates.sk_runtimeeffect_get_uniforms_size>("sk_runtimeeffect_get_uniforms_size")))(effect);
	}

	internal unsafe static IntPtr sk_runtimeeffect_make_blender(IntPtr effect, IntPtr uniforms, IntPtr* children, IntPtr childCount)
	{
		return (sk_runtimeeffect_make_blender_delegate ?? (sk_runtimeeffect_make_blender_delegate = GetSymbol<Delegates.sk_runtimeeffect_make_blender>("sk_runtimeeffect_make_blender")))(effect, uniforms, children, childCount);
	}

	internal unsafe static IntPtr sk_runtimeeffect_make_color_filter(IntPtr effect, IntPtr uniforms, IntPtr* children, IntPtr childCount)
	{
		return (sk_runtimeeffect_make_color_filter_delegate ?? (sk_runtimeeffect_make_color_filter_delegate = GetSymbol<Delegates.sk_runtimeeffect_make_color_filter>("sk_runtimeeffect_make_color_filter")))(effect, uniforms, children, childCount);
	}

	internal static IntPtr sk_runtimeeffect_make_for_blender(IntPtr sksl, IntPtr error)
	{
		return (sk_runtimeeffect_make_for_blender_delegate ?? (sk_runtimeeffect_make_for_blender_delegate = GetSymbol<Delegates.sk_runtimeeffect_make_for_blender>("sk_runtimeeffect_make_for_blender")))(sksl, error);
	}

	internal static IntPtr sk_runtimeeffect_make_for_color_filter(IntPtr sksl, IntPtr error)
	{
		return (sk_runtimeeffect_make_for_color_filter_delegate ?? (sk_runtimeeffect_make_for_color_filter_delegate = GetSymbol<Delegates.sk_runtimeeffect_make_for_color_filter>("sk_runtimeeffect_make_for_color_filter")))(sksl, error);
	}

	internal static IntPtr sk_runtimeeffect_make_for_shader(IntPtr sksl, IntPtr error)
	{
		return (sk_runtimeeffect_make_for_shader_delegate ?? (sk_runtimeeffect_make_for_shader_delegate = GetSymbol<Delegates.sk_runtimeeffect_make_for_shader>("sk_runtimeeffect_make_for_shader")))(sksl, error);
	}

	internal unsafe static IntPtr sk_runtimeeffect_make_shader(IntPtr effect, IntPtr uniforms, IntPtr* children, IntPtr childCount, SKMatrix* localMatrix)
	{
		return (sk_runtimeeffect_make_shader_delegate ?? (sk_runtimeeffect_make_shader_delegate = GetSymbol<Delegates.sk_runtimeeffect_make_shader>("sk_runtimeeffect_make_shader")))(effect, uniforms, children, childCount, localMatrix);
	}

	internal static void sk_runtimeeffect_unref(IntPtr effect)
	{
		(sk_runtimeeffect_unref_delegate ?? (sk_runtimeeffect_unref_delegate = GetSymbol<Delegates.sk_runtimeeffect_unref>("sk_runtimeeffect_unref")))(effect);
	}

	internal static IntPtr sk_shader_new_blend(SKBlendMode mode, IntPtr dst, IntPtr src)
	{
		return (sk_shader_new_blend_delegate ?? (sk_shader_new_blend_delegate = GetSymbol<Delegates.sk_shader_new_blend>("sk_shader_new_blend")))(mode, dst, src);
	}

	internal static IntPtr sk_shader_new_blender(IntPtr blender, IntPtr dst, IntPtr src)
	{
		return (sk_shader_new_blender_delegate ?? (sk_shader_new_blender_delegate = GetSymbol<Delegates.sk_shader_new_blender>("sk_shader_new_blender")))(blender, dst, src);
	}

	internal static IntPtr sk_shader_new_color(uint color)
	{
		return (sk_shader_new_color_delegate ?? (sk_shader_new_color_delegate = GetSymbol<Delegates.sk_shader_new_color>("sk_shader_new_color")))(color);
	}

	internal unsafe static IntPtr sk_shader_new_color4f(SKColorF* color, IntPtr colorspace)
	{
		return (sk_shader_new_color4f_delegate ?? (sk_shader_new_color4f_delegate = GetSymbol<Delegates.sk_shader_new_color4f>("sk_shader_new_color4f")))(color, colorspace);
	}

	internal static IntPtr sk_shader_new_empty()
	{
		return (sk_shader_new_empty_delegate ?? (sk_shader_new_empty_delegate = GetSymbol<Delegates.sk_shader_new_empty>("sk_shader_new_empty")))();
	}

	internal unsafe static IntPtr sk_shader_new_linear_gradient(SKPoint* points, uint* colors, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix)
	{
		return (sk_shader_new_linear_gradient_delegate ?? (sk_shader_new_linear_gradient_delegate = GetSymbol<Delegates.sk_shader_new_linear_gradient>("sk_shader_new_linear_gradient")))(points, colors, colorPos, colorCount, tileMode, localMatrix);
	}

	internal unsafe static IntPtr sk_shader_new_linear_gradient_color4f(SKPoint* points, SKColorF* colors, IntPtr colorspace, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix)
	{
		return (sk_shader_new_linear_gradient_color4f_delegate ?? (sk_shader_new_linear_gradient_color4f_delegate = GetSymbol<Delegates.sk_shader_new_linear_gradient_color4f>("sk_shader_new_linear_gradient_color4f")))(points, colors, colorspace, colorPos, colorCount, tileMode, localMatrix);
	}

	internal unsafe static IntPtr sk_shader_new_perlin_noise_fractal_noise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKSizeI* tileSize)
	{
		return (sk_shader_new_perlin_noise_fractal_noise_delegate ?? (sk_shader_new_perlin_noise_fractal_noise_delegate = GetSymbol<Delegates.sk_shader_new_perlin_noise_fractal_noise>("sk_shader_new_perlin_noise_fractal_noise")))(baseFrequencyX, baseFrequencyY, numOctaves, seed, tileSize);
	}

	internal unsafe static IntPtr sk_shader_new_perlin_noise_turbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKSizeI* tileSize)
	{
		return (sk_shader_new_perlin_noise_turbulence_delegate ?? (sk_shader_new_perlin_noise_turbulence_delegate = GetSymbol<Delegates.sk_shader_new_perlin_noise_turbulence>("sk_shader_new_perlin_noise_turbulence")))(baseFrequencyX, baseFrequencyY, numOctaves, seed, tileSize);
	}

	internal unsafe static IntPtr sk_shader_new_radial_gradient(SKPoint* center, float radius, uint* colors, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix)
	{
		return (sk_shader_new_radial_gradient_delegate ?? (sk_shader_new_radial_gradient_delegate = GetSymbol<Delegates.sk_shader_new_radial_gradient>("sk_shader_new_radial_gradient")))(center, radius, colors, colorPos, colorCount, tileMode, localMatrix);
	}

	internal unsafe static IntPtr sk_shader_new_radial_gradient_color4f(SKPoint* center, float radius, SKColorF* colors, IntPtr colorspace, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix)
	{
		return (sk_shader_new_radial_gradient_color4f_delegate ?? (sk_shader_new_radial_gradient_color4f_delegate = GetSymbol<Delegates.sk_shader_new_radial_gradient_color4f>("sk_shader_new_radial_gradient_color4f")))(center, radius, colors, colorspace, colorPos, colorCount, tileMode, localMatrix);
	}

	internal unsafe static IntPtr sk_shader_new_sweep_gradient(SKPoint* center, uint* colors, float* colorPos, int colorCount, SKShaderTileMode tileMode, float startAngle, float endAngle, SKMatrix* localMatrix)
	{
		return (sk_shader_new_sweep_gradient_delegate ?? (sk_shader_new_sweep_gradient_delegate = GetSymbol<Delegates.sk_shader_new_sweep_gradient>("sk_shader_new_sweep_gradient")))(center, colors, colorPos, colorCount, tileMode, startAngle, endAngle, localMatrix);
	}

	internal unsafe static IntPtr sk_shader_new_sweep_gradient_color4f(SKPoint* center, SKColorF* colors, IntPtr colorspace, float* colorPos, int colorCount, SKShaderTileMode tileMode, float startAngle, float endAngle, SKMatrix* localMatrix)
	{
		return (sk_shader_new_sweep_gradient_color4f_delegate ?? (sk_shader_new_sweep_gradient_color4f_delegate = GetSymbol<Delegates.sk_shader_new_sweep_gradient_color4f>("sk_shader_new_sweep_gradient_color4f")))(center, colors, colorspace, colorPos, colorCount, tileMode, startAngle, endAngle, localMatrix);
	}

	internal unsafe static IntPtr sk_shader_new_two_point_conical_gradient(SKPoint* start, float startRadius, SKPoint* end, float endRadius, uint* colors, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix)
	{
		return (sk_shader_new_two_point_conical_gradient_delegate ?? (sk_shader_new_two_point_conical_gradient_delegate = GetSymbol<Delegates.sk_shader_new_two_point_conical_gradient>("sk_shader_new_two_point_conical_gradient")))(start, startRadius, end, endRadius, colors, colorPos, colorCount, tileMode, localMatrix);
	}

	internal unsafe static IntPtr sk_shader_new_two_point_conical_gradient_color4f(SKPoint* start, float startRadius, SKPoint* end, float endRadius, SKColorF* colors, IntPtr colorspace, float* colorPos, int colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix)
	{
		return (sk_shader_new_two_point_conical_gradient_color4f_delegate ?? (sk_shader_new_two_point_conical_gradient_color4f_delegate = GetSymbol<Delegates.sk_shader_new_two_point_conical_gradient_color4f>("sk_shader_new_two_point_conical_gradient_color4f")))(start, startRadius, end, endRadius, colors, colorspace, colorPos, colorCount, tileMode, localMatrix);
	}

	internal static void sk_shader_ref(IntPtr shader)
	{
		(sk_shader_ref_delegate ?? (sk_shader_ref_delegate = GetSymbol<Delegates.sk_shader_ref>("sk_shader_ref")))(shader);
	}

	internal static void sk_shader_unref(IntPtr shader)
	{
		(sk_shader_unref_delegate ?? (sk_shader_unref_delegate = GetSymbol<Delegates.sk_shader_unref>("sk_shader_unref")))(shader);
	}

	internal static IntPtr sk_shader_with_color_filter(IntPtr shader, IntPtr filter)
	{
		return (sk_shader_with_color_filter_delegate ?? (sk_shader_with_color_filter_delegate = GetSymbol<Delegates.sk_shader_with_color_filter>("sk_shader_with_color_filter")))(shader, filter);
	}

	internal unsafe static IntPtr sk_shader_with_local_matrix(IntPtr shader, SKMatrix* localMatrix)
	{
		return (sk_shader_with_local_matrix_delegate ?? (sk_shader_with_local_matrix_delegate = GetSymbol<Delegates.sk_shader_with_local_matrix>("sk_shader_with_local_matrix")))(shader, localMatrix);
	}

	internal unsafe static void sk_dynamicmemorywstream_copy_to(IntPtr cstream, void* data)
	{
		(sk_dynamicmemorywstream_copy_to_delegate ?? (sk_dynamicmemorywstream_copy_to_delegate = GetSymbol<Delegates.sk_dynamicmemorywstream_copy_to>("sk_dynamicmemorywstream_copy_to")))(cstream, data);
	}

	internal static void sk_dynamicmemorywstream_destroy(IntPtr cstream)
	{
		(sk_dynamicmemorywstream_destroy_delegate ?? (sk_dynamicmemorywstream_destroy_delegate = GetSymbol<Delegates.sk_dynamicmemorywstream_destroy>("sk_dynamicmemorywstream_destroy")))(cstream);
	}

	internal static IntPtr sk_dynamicmemorywstream_detach_as_data(IntPtr cstream)
	{
		return (sk_dynamicmemorywstream_detach_as_data_delegate ?? (sk_dynamicmemorywstream_detach_as_data_delegate = GetSymbol<Delegates.sk_dynamicmemorywstream_detach_as_data>("sk_dynamicmemorywstream_detach_as_data")))(cstream);
	}

	internal static IntPtr sk_dynamicmemorywstream_detach_as_stream(IntPtr cstream)
	{
		return (sk_dynamicmemorywstream_detach_as_stream_delegate ?? (sk_dynamicmemorywstream_detach_as_stream_delegate = GetSymbol<Delegates.sk_dynamicmemorywstream_detach_as_stream>("sk_dynamicmemorywstream_detach_as_stream")))(cstream);
	}

	internal static IntPtr sk_dynamicmemorywstream_new()
	{
		return (sk_dynamicmemorywstream_new_delegate ?? (sk_dynamicmemorywstream_new_delegate = GetSymbol<Delegates.sk_dynamicmemorywstream_new>("sk_dynamicmemorywstream_new")))();
	}

	internal static bool sk_dynamicmemorywstream_write_to_stream(IntPtr cstream, IntPtr dst)
	{
		return (sk_dynamicmemorywstream_write_to_stream_delegate ?? (sk_dynamicmemorywstream_write_to_stream_delegate = GetSymbol<Delegates.sk_dynamicmemorywstream_write_to_stream>("sk_dynamicmemorywstream_write_to_stream")))(cstream, dst);
	}

	internal static void sk_filestream_destroy(IntPtr cstream)
	{
		(sk_filestream_destroy_delegate ?? (sk_filestream_destroy_delegate = GetSymbol<Delegates.sk_filestream_destroy>("sk_filestream_destroy")))(cstream);
	}

	internal static bool sk_filestream_is_valid(IntPtr cstream)
	{
		return (sk_filestream_is_valid_delegate ?? (sk_filestream_is_valid_delegate = GetSymbol<Delegates.sk_filestream_is_valid>("sk_filestream_is_valid")))(cstream);
	}

	internal unsafe static IntPtr sk_filestream_new(void* path)
	{
		return (sk_filestream_new_delegate ?? (sk_filestream_new_delegate = GetSymbol<Delegates.sk_filestream_new>("sk_filestream_new")))(path);
	}

	internal static void sk_filewstream_destroy(IntPtr cstream)
	{
		(sk_filewstream_destroy_delegate ?? (sk_filewstream_destroy_delegate = GetSymbol<Delegates.sk_filewstream_destroy>("sk_filewstream_destroy")))(cstream);
	}

	internal static bool sk_filewstream_is_valid(IntPtr cstream)
	{
		return (sk_filewstream_is_valid_delegate ?? (sk_filewstream_is_valid_delegate = GetSymbol<Delegates.sk_filewstream_is_valid>("sk_filewstream_is_valid")))(cstream);
	}

	internal unsafe static IntPtr sk_filewstream_new(void* path)
	{
		return (sk_filewstream_new_delegate ?? (sk_filewstream_new_delegate = GetSymbol<Delegates.sk_filewstream_new>("sk_filewstream_new")))(path);
	}

	internal static void sk_memorystream_destroy(IntPtr cstream)
	{
		(sk_memorystream_destroy_delegate ?? (sk_memorystream_destroy_delegate = GetSymbol<Delegates.sk_memorystream_destroy>("sk_memorystream_destroy")))(cstream);
	}

	internal static IntPtr sk_memorystream_new()
	{
		return (sk_memorystream_new_delegate ?? (sk_memorystream_new_delegate = GetSymbol<Delegates.sk_memorystream_new>("sk_memorystream_new")))();
	}

	internal unsafe static IntPtr sk_memorystream_new_with_data(void* data, IntPtr length, [MarshalAs(UnmanagedType.I1)] bool copyData)
	{
		return (sk_memorystream_new_with_data_delegate ?? (sk_memorystream_new_with_data_delegate = GetSymbol<Delegates.sk_memorystream_new_with_data>("sk_memorystream_new_with_data")))(data, length, copyData);
	}

	internal static IntPtr sk_memorystream_new_with_length(IntPtr length)
	{
		return (sk_memorystream_new_with_length_delegate ?? (sk_memorystream_new_with_length_delegate = GetSymbol<Delegates.sk_memorystream_new_with_length>("sk_memorystream_new_with_length")))(length);
	}

	internal static IntPtr sk_memorystream_new_with_skdata(IntPtr data)
	{
		return (sk_memorystream_new_with_skdata_delegate ?? (sk_memorystream_new_with_skdata_delegate = GetSymbol<Delegates.sk_memorystream_new_with_skdata>("sk_memorystream_new_with_skdata")))(data);
	}

	internal unsafe static void sk_memorystream_set_memory(IntPtr cmemorystream, void* data, IntPtr length, [MarshalAs(UnmanagedType.I1)] bool copyData)
	{
		(sk_memorystream_set_memory_delegate ?? (sk_memorystream_set_memory_delegate = GetSymbol<Delegates.sk_memorystream_set_memory>("sk_memorystream_set_memory")))(cmemorystream, data, length, copyData);
	}

	internal static void sk_stream_asset_destroy(IntPtr cstream)
	{
		(sk_stream_asset_destroy_delegate ?? (sk_stream_asset_destroy_delegate = GetSymbol<Delegates.sk_stream_asset_destroy>("sk_stream_asset_destroy")))(cstream);
	}

	internal static void sk_stream_destroy(IntPtr cstream)
	{
		(sk_stream_destroy_delegate ?? (sk_stream_destroy_delegate = GetSymbol<Delegates.sk_stream_destroy>("sk_stream_destroy")))(cstream);
	}

	internal static IntPtr sk_stream_duplicate(IntPtr cstream)
	{
		return (sk_stream_duplicate_delegate ?? (sk_stream_duplicate_delegate = GetSymbol<Delegates.sk_stream_duplicate>("sk_stream_duplicate")))(cstream);
	}

	internal static IntPtr sk_stream_fork(IntPtr cstream)
	{
		return (sk_stream_fork_delegate ?? (sk_stream_fork_delegate = GetSymbol<Delegates.sk_stream_fork>("sk_stream_fork")))(cstream);
	}

	internal static IntPtr sk_stream_get_length(IntPtr cstream)
	{
		return (sk_stream_get_length_delegate ?? (sk_stream_get_length_delegate = GetSymbol<Delegates.sk_stream_get_length>("sk_stream_get_length")))(cstream);
	}

	internal unsafe static void* sk_stream_get_memory_base(IntPtr cstream)
	{
		return (sk_stream_get_memory_base_delegate ?? (sk_stream_get_memory_base_delegate = GetSymbol<Delegates.sk_stream_get_memory_base>("sk_stream_get_memory_base")))(cstream);
	}

	internal static IntPtr sk_stream_get_position(IntPtr cstream)
	{
		return (sk_stream_get_position_delegate ?? (sk_stream_get_position_delegate = GetSymbol<Delegates.sk_stream_get_position>("sk_stream_get_position")))(cstream);
	}

	internal static bool sk_stream_has_length(IntPtr cstream)
	{
		return (sk_stream_has_length_delegate ?? (sk_stream_has_length_delegate = GetSymbol<Delegates.sk_stream_has_length>("sk_stream_has_length")))(cstream);
	}

	internal static bool sk_stream_has_position(IntPtr cstream)
	{
		return (sk_stream_has_position_delegate ?? (sk_stream_has_position_delegate = GetSymbol<Delegates.sk_stream_has_position>("sk_stream_has_position")))(cstream);
	}

	internal static bool sk_stream_is_at_end(IntPtr cstream)
	{
		return (sk_stream_is_at_end_delegate ?? (sk_stream_is_at_end_delegate = GetSymbol<Delegates.sk_stream_is_at_end>("sk_stream_is_at_end")))(cstream);
	}

	internal static bool sk_stream_move(IntPtr cstream, int offset)
	{
		return (sk_stream_move_delegate ?? (sk_stream_move_delegate = GetSymbol<Delegates.sk_stream_move>("sk_stream_move")))(cstream, offset);
	}

	internal unsafe static IntPtr sk_stream_peek(IntPtr cstream, void* buffer, IntPtr size)
	{
		return (sk_stream_peek_delegate ?? (sk_stream_peek_delegate = GetSymbol<Delegates.sk_stream_peek>("sk_stream_peek")))(cstream, buffer, size);
	}

	internal unsafe static IntPtr sk_stream_read(IntPtr cstream, void* buffer, IntPtr size)
	{
		return (sk_stream_read_delegate ?? (sk_stream_read_delegate = GetSymbol<Delegates.sk_stream_read>("sk_stream_read")))(cstream, buffer, size);
	}

	internal unsafe static bool sk_stream_read_bool(IntPtr cstream, byte* buffer)
	{
		return (sk_stream_read_bool_delegate ?? (sk_stream_read_bool_delegate = GetSymbol<Delegates.sk_stream_read_bool>("sk_stream_read_bool")))(cstream, buffer);
	}

	internal unsafe static bool sk_stream_read_s16(IntPtr cstream, short* buffer)
	{
		return (sk_stream_read_s16_delegate ?? (sk_stream_read_s16_delegate = GetSymbol<Delegates.sk_stream_read_s16>("sk_stream_read_s16")))(cstream, buffer);
	}

	internal unsafe static bool sk_stream_read_s32(IntPtr cstream, int* buffer)
	{
		return (sk_stream_read_s32_delegate ?? (sk_stream_read_s32_delegate = GetSymbol<Delegates.sk_stream_read_s32>("sk_stream_read_s32")))(cstream, buffer);
	}

	internal unsafe static bool sk_stream_read_s8(IntPtr cstream, sbyte* buffer)
	{
		return (sk_stream_read_s8_delegate ?? (sk_stream_read_s8_delegate = GetSymbol<Delegates.sk_stream_read_s8>("sk_stream_read_s8")))(cstream, buffer);
	}

	internal unsafe static bool sk_stream_read_u16(IntPtr cstream, ushort* buffer)
	{
		return (sk_stream_read_u16_delegate ?? (sk_stream_read_u16_delegate = GetSymbol<Delegates.sk_stream_read_u16>("sk_stream_read_u16")))(cstream, buffer);
	}

	internal unsafe static bool sk_stream_read_u32(IntPtr cstream, uint* buffer)
	{
		return (sk_stream_read_u32_delegate ?? (sk_stream_read_u32_delegate = GetSymbol<Delegates.sk_stream_read_u32>("sk_stream_read_u32")))(cstream, buffer);
	}

	internal unsafe static bool sk_stream_read_u8(IntPtr cstream, byte* buffer)
	{
		return (sk_stream_read_u8_delegate ?? (sk_stream_read_u8_delegate = GetSymbol<Delegates.sk_stream_read_u8>("sk_stream_read_u8")))(cstream, buffer);
	}

	internal static bool sk_stream_rewind(IntPtr cstream)
	{
		return (sk_stream_rewind_delegate ?? (sk_stream_rewind_delegate = GetSymbol<Delegates.sk_stream_rewind>("sk_stream_rewind")))(cstream);
	}

	internal static bool sk_stream_seek(IntPtr cstream, IntPtr position)
	{
		return (sk_stream_seek_delegate ?? (sk_stream_seek_delegate = GetSymbol<Delegates.sk_stream_seek>("sk_stream_seek")))(cstream, position);
	}

	internal static IntPtr sk_stream_skip(IntPtr cstream, IntPtr size)
	{
		return (sk_stream_skip_delegate ?? (sk_stream_skip_delegate = GetSymbol<Delegates.sk_stream_skip>("sk_stream_skip")))(cstream, size);
	}

	internal static IntPtr sk_wstream_bytes_written(IntPtr cstream)
	{
		return (sk_wstream_bytes_written_delegate ?? (sk_wstream_bytes_written_delegate = GetSymbol<Delegates.sk_wstream_bytes_written>("sk_wstream_bytes_written")))(cstream);
	}

	internal static void sk_wstream_flush(IntPtr cstream)
	{
		(sk_wstream_flush_delegate ?? (sk_wstream_flush_delegate = GetSymbol<Delegates.sk_wstream_flush>("sk_wstream_flush")))(cstream);
	}

	internal static int sk_wstream_get_size_of_packed_uint(IntPtr value)
	{
		return (sk_wstream_get_size_of_packed_uint_delegate ?? (sk_wstream_get_size_of_packed_uint_delegate = GetSymbol<Delegates.sk_wstream_get_size_of_packed_uint>("sk_wstream_get_size_of_packed_uint")))(value);
	}

	internal static bool sk_wstream_newline(IntPtr cstream)
	{
		return (sk_wstream_newline_delegate ?? (sk_wstream_newline_delegate = GetSymbol<Delegates.sk_wstream_newline>("sk_wstream_newline")))(cstream);
	}

	internal unsafe static bool sk_wstream_write(IntPtr cstream, void* buffer, IntPtr size)
	{
		return (sk_wstream_write_delegate ?? (sk_wstream_write_delegate = GetSymbol<Delegates.sk_wstream_write>("sk_wstream_write")))(cstream, buffer, size);
	}

	internal static bool sk_wstream_write_16(IntPtr cstream, ushort value)
	{
		return (sk_wstream_write_16_delegate ?? (sk_wstream_write_16_delegate = GetSymbol<Delegates.sk_wstream_write_16>("sk_wstream_write_16")))(cstream, value);
	}

	internal static bool sk_wstream_write_32(IntPtr cstream, uint value)
	{
		return (sk_wstream_write_32_delegate ?? (sk_wstream_write_32_delegate = GetSymbol<Delegates.sk_wstream_write_32>("sk_wstream_write_32")))(cstream, value);
	}

	internal static bool sk_wstream_write_8(IntPtr cstream, byte value)
	{
		return (sk_wstream_write_8_delegate ?? (sk_wstream_write_8_delegate = GetSymbol<Delegates.sk_wstream_write_8>("sk_wstream_write_8")))(cstream, value);
	}

	internal static bool sk_wstream_write_bigdec_as_text(IntPtr cstream, long value, int minDigits)
	{
		return (sk_wstream_write_bigdec_as_text_delegate ?? (sk_wstream_write_bigdec_as_text_delegate = GetSymbol<Delegates.sk_wstream_write_bigdec_as_text>("sk_wstream_write_bigdec_as_text")))(cstream, value, minDigits);
	}

	internal static bool sk_wstream_write_bool(IntPtr cstream, [MarshalAs(UnmanagedType.I1)] bool value)
	{
		return (sk_wstream_write_bool_delegate ?? (sk_wstream_write_bool_delegate = GetSymbol<Delegates.sk_wstream_write_bool>("sk_wstream_write_bool")))(cstream, value);
	}

	internal static bool sk_wstream_write_dec_as_text(IntPtr cstream, int value)
	{
		return (sk_wstream_write_dec_as_text_delegate ?? (sk_wstream_write_dec_as_text_delegate = GetSymbol<Delegates.sk_wstream_write_dec_as_text>("sk_wstream_write_dec_as_text")))(cstream, value);
	}

	internal static bool sk_wstream_write_hex_as_text(IntPtr cstream, uint value, int minDigits)
	{
		return (sk_wstream_write_hex_as_text_delegate ?? (sk_wstream_write_hex_as_text_delegate = GetSymbol<Delegates.sk_wstream_write_hex_as_text>("sk_wstream_write_hex_as_text")))(cstream, value, minDigits);
	}

	internal static bool sk_wstream_write_packed_uint(IntPtr cstream, IntPtr value)
	{
		return (sk_wstream_write_packed_uint_delegate ?? (sk_wstream_write_packed_uint_delegate = GetSymbol<Delegates.sk_wstream_write_packed_uint>("sk_wstream_write_packed_uint")))(cstream, value);
	}

	internal static bool sk_wstream_write_scalar(IntPtr cstream, float value)
	{
		return (sk_wstream_write_scalar_delegate ?? (sk_wstream_write_scalar_delegate = GetSymbol<Delegates.sk_wstream_write_scalar>("sk_wstream_write_scalar")))(cstream, value);
	}

	internal static bool sk_wstream_write_scalar_as_text(IntPtr cstream, float value)
	{
		return (sk_wstream_write_scalar_as_text_delegate ?? (sk_wstream_write_scalar_as_text_delegate = GetSymbol<Delegates.sk_wstream_write_scalar_as_text>("sk_wstream_write_scalar_as_text")))(cstream, value);
	}

	internal static bool sk_wstream_write_stream(IntPtr cstream, IntPtr input, IntPtr length)
	{
		return (sk_wstream_write_stream_delegate ?? (sk_wstream_write_stream_delegate = GetSymbol<Delegates.sk_wstream_write_stream>("sk_wstream_write_stream")))(cstream, input, length);
	}

	internal static bool sk_wstream_write_text(IntPtr cstream, [MarshalAs(UnmanagedType.LPStr)] string value)
	{
		return (sk_wstream_write_text_delegate ?? (sk_wstream_write_text_delegate = GetSymbol<Delegates.sk_wstream_write_text>("sk_wstream_write_text")))(cstream, value);
	}

	internal static void sk_string_destructor(IntPtr param0)
	{
		(sk_string_destructor_delegate ?? (sk_string_destructor_delegate = GetSymbol<Delegates.sk_string_destructor>("sk_string_destructor")))(param0);
	}

	internal unsafe static void* sk_string_get_c_str(IntPtr param0)
	{
		return (sk_string_get_c_str_delegate ?? (sk_string_get_c_str_delegate = GetSymbol<Delegates.sk_string_get_c_str>("sk_string_get_c_str")))(param0);
	}

	internal static IntPtr sk_string_get_size(IntPtr param0)
	{
		return (sk_string_get_size_delegate ?? (sk_string_get_size_delegate = GetSymbol<Delegates.sk_string_get_size>("sk_string_get_size")))(param0);
	}

	internal static IntPtr sk_string_new_empty()
	{
		return (sk_string_new_empty_delegate ?? (sk_string_new_empty_delegate = GetSymbol<Delegates.sk_string_new_empty>("sk_string_new_empty")))();
	}

	internal unsafe static IntPtr sk_string_new_with_copy(void* src, IntPtr length)
	{
		return (sk_string_new_with_copy_delegate ?? (sk_string_new_with_copy_delegate = GetSymbol<Delegates.sk_string_new_with_copy>("sk_string_new_with_copy")))(src, length);
	}

	internal static void sk_surface_draw(IntPtr surface, IntPtr canvas, float x, float y, IntPtr paint)
	{
		(sk_surface_draw_delegate ?? (sk_surface_draw_delegate = GetSymbol<Delegates.sk_surface_draw>("sk_surface_draw")))(surface, canvas, x, y, paint);
	}

	internal static IntPtr sk_surface_get_canvas(IntPtr param0)
	{
		return (sk_surface_get_canvas_delegate ?? (sk_surface_get_canvas_delegate = GetSymbol<Delegates.sk_surface_get_canvas>("sk_surface_get_canvas")))(param0);
	}

	internal static IntPtr sk_surface_get_props(IntPtr surface)
	{
		return (sk_surface_get_props_delegate ?? (sk_surface_get_props_delegate = GetSymbol<Delegates.sk_surface_get_props>("sk_surface_get_props")))(surface);
	}

	internal static IntPtr sk_surface_get_recording_context(IntPtr surface)
	{
		return (sk_surface_get_recording_context_delegate ?? (sk_surface_get_recording_context_delegate = GetSymbol<Delegates.sk_surface_get_recording_context>("sk_surface_get_recording_context")))(surface);
	}

	internal static IntPtr sk_surface_new_backend_render_target(IntPtr context, IntPtr target, GRSurfaceOrigin origin, SKColorTypeNative colorType, IntPtr colorspace, IntPtr props)
	{
		return (sk_surface_new_backend_render_target_delegate ?? (sk_surface_new_backend_render_target_delegate = GetSymbol<Delegates.sk_surface_new_backend_render_target>("sk_surface_new_backend_render_target")))(context, target, origin, colorType, colorspace, props);
	}

	internal static IntPtr sk_surface_new_backend_texture(IntPtr context, IntPtr texture, GRSurfaceOrigin origin, int samples, SKColorTypeNative colorType, IntPtr colorspace, IntPtr props)
	{
		return (sk_surface_new_backend_texture_delegate ?? (sk_surface_new_backend_texture_delegate = GetSymbol<Delegates.sk_surface_new_backend_texture>("sk_surface_new_backend_texture")))(context, texture, origin, samples, colorType, colorspace, props);
	}

	internal static IntPtr sk_surface_new_image_snapshot(IntPtr param0)
	{
		return (sk_surface_new_image_snapshot_delegate ?? (sk_surface_new_image_snapshot_delegate = GetSymbol<Delegates.sk_surface_new_image_snapshot>("sk_surface_new_image_snapshot")))(param0);
	}

	internal unsafe static IntPtr sk_surface_new_image_snapshot_with_crop(IntPtr surface, SKRectI* bounds)
	{
		return (sk_surface_new_image_snapshot_with_crop_delegate ?? (sk_surface_new_image_snapshot_with_crop_delegate = GetSymbol<Delegates.sk_surface_new_image_snapshot_with_crop>("sk_surface_new_image_snapshot_with_crop")))(surface, bounds);
	}

	internal unsafe static IntPtr sk_surface_new_metal_layer(IntPtr context, void* layer, GRSurfaceOrigin origin, int sampleCount, SKColorTypeNative colorType, IntPtr colorspace, IntPtr props, void** drawable)
	{
		return (sk_surface_new_metal_layer_delegate ?? (sk_surface_new_metal_layer_delegate = GetSymbol<Delegates.sk_surface_new_metal_layer>("sk_surface_new_metal_layer")))(context, layer, origin, sampleCount, colorType, colorspace, props, drawable);
	}

	internal unsafe static IntPtr sk_surface_new_metal_view(IntPtr context, void* mtkView, GRSurfaceOrigin origin, int sampleCount, SKColorTypeNative colorType, IntPtr colorspace, IntPtr props)
	{
		return (sk_surface_new_metal_view_delegate ?? (sk_surface_new_metal_view_delegate = GetSymbol<Delegates.sk_surface_new_metal_view>("sk_surface_new_metal_view")))(context, mtkView, origin, sampleCount, colorType, colorspace, props);
	}

	internal static IntPtr sk_surface_new_null(int width, int height)
	{
		return (sk_surface_new_null_delegate ?? (sk_surface_new_null_delegate = GetSymbol<Delegates.sk_surface_new_null>("sk_surface_new_null")))(width, height);
	}

	internal unsafe static IntPtr sk_surface_new_raster(SKImageInfoNative* param0, IntPtr rowBytes, IntPtr param2)
	{
		return (sk_surface_new_raster_delegate ?? (sk_surface_new_raster_delegate = GetSymbol<Delegates.sk_surface_new_raster>("sk_surface_new_raster")))(param0, rowBytes, param2);
	}

	internal unsafe static IntPtr sk_surface_new_raster_direct(SKImageInfoNative* param0, void* pixels, IntPtr rowBytes, SKSurfaceRasterReleaseProxyDelegate releaseProc, void* context, IntPtr props)
	{
		return (sk_surface_new_raster_direct_delegate ?? (sk_surface_new_raster_direct_delegate = GetSymbol<Delegates.sk_surface_new_raster_direct>("sk_surface_new_raster_direct")))(param0, pixels, rowBytes, releaseProc, context, props);
	}

	internal unsafe static IntPtr sk_surface_new_render_target(IntPtr context, [MarshalAs(UnmanagedType.I1)] bool budgeted, SKImageInfoNative* cinfo, int sampleCount, GRSurfaceOrigin origin, IntPtr props, [MarshalAs(UnmanagedType.I1)] bool shouldCreateWithMips)
	{
		return (sk_surface_new_render_target_delegate ?? (sk_surface_new_render_target_delegate = GetSymbol<Delegates.sk_surface_new_render_target>("sk_surface_new_render_target")))(context, budgeted, cinfo, sampleCount, origin, props, shouldCreateWithMips);
	}

	internal static bool sk_surface_peek_pixels(IntPtr surface, IntPtr pixmap)
	{
		return (sk_surface_peek_pixels_delegate ?? (sk_surface_peek_pixels_delegate = GetSymbol<Delegates.sk_surface_peek_pixels>("sk_surface_peek_pixels")))(surface, pixmap);
	}

	internal unsafe static bool sk_surface_read_pixels(IntPtr surface, SKImageInfoNative* dstInfo, void* dstPixels, IntPtr dstRowBytes, int srcX, int srcY)
	{
		return (sk_surface_read_pixels_delegate ?? (sk_surface_read_pixels_delegate = GetSymbol<Delegates.sk_surface_read_pixels>("sk_surface_read_pixels")))(surface, dstInfo, dstPixels, dstRowBytes, srcX, srcY);
	}

	internal static void sk_surface_unref(IntPtr param0)
	{
		(sk_surface_unref_delegate ?? (sk_surface_unref_delegate = GetSymbol<Delegates.sk_surface_unref>("sk_surface_unref")))(param0);
	}

	internal static void sk_surfaceprops_delete(IntPtr props)
	{
		(sk_surfaceprops_delete_delegate ?? (sk_surfaceprops_delete_delegate = GetSymbol<Delegates.sk_surfaceprops_delete>("sk_surfaceprops_delete")))(props);
	}

	internal static uint sk_surfaceprops_get_flags(IntPtr props)
	{
		return (sk_surfaceprops_get_flags_delegate ?? (sk_surfaceprops_get_flags_delegate = GetSymbol<Delegates.sk_surfaceprops_get_flags>("sk_surfaceprops_get_flags")))(props);
	}

	internal static SKPixelGeometry sk_surfaceprops_get_pixel_geometry(IntPtr props)
	{
		return (sk_surfaceprops_get_pixel_geometry_delegate ?? (sk_surfaceprops_get_pixel_geometry_delegate = GetSymbol<Delegates.sk_surfaceprops_get_pixel_geometry>("sk_surfaceprops_get_pixel_geometry")))(props);
	}

	internal static IntPtr sk_surfaceprops_new(uint flags, SKPixelGeometry geometry)
	{
		return (sk_surfaceprops_new_delegate ?? (sk_surfaceprops_new_delegate = GetSymbol<Delegates.sk_surfaceprops_new>("sk_surfaceprops_new")))(flags, geometry);
	}

	internal unsafe static IntPtr sk_svgcanvas_create_with_stream(SKRect* bounds, IntPtr stream)
	{
		return (sk_svgcanvas_create_with_stream_delegate ?? (sk_svgcanvas_create_with_stream_delegate = GetSymbol<Delegates.sk_svgcanvas_create_with_stream>("sk_svgcanvas_create_with_stream")))(bounds, stream);
	}

	internal unsafe static void sk_textblob_builder_alloc_run(IntPtr builder, IntPtr font, int count, float x, float y, SKRect* bounds, SKRunBufferInternal* runbuffer)
	{
		(sk_textblob_builder_alloc_run_delegate ?? (sk_textblob_builder_alloc_run_delegate = GetSymbol<Delegates.sk_textblob_builder_alloc_run>("sk_textblob_builder_alloc_run")))(builder, font, count, x, y, bounds, runbuffer);
	}

	internal unsafe static void sk_textblob_builder_alloc_run_pos(IntPtr builder, IntPtr font, int count, SKRect* bounds, SKRunBufferInternal* runbuffer)
	{
		(sk_textblob_builder_alloc_run_pos_delegate ?? (sk_textblob_builder_alloc_run_pos_delegate = GetSymbol<Delegates.sk_textblob_builder_alloc_run_pos>("sk_textblob_builder_alloc_run_pos")))(builder, font, count, bounds, runbuffer);
	}

	internal unsafe static void sk_textblob_builder_alloc_run_pos_h(IntPtr builder, IntPtr font, int count, float y, SKRect* bounds, SKRunBufferInternal* runbuffer)
	{
		(sk_textblob_builder_alloc_run_pos_h_delegate ?? (sk_textblob_builder_alloc_run_pos_h_delegate = GetSymbol<Delegates.sk_textblob_builder_alloc_run_pos_h>("sk_textblob_builder_alloc_run_pos_h")))(builder, font, count, y, bounds, runbuffer);
	}

	internal unsafe static void sk_textblob_builder_alloc_run_rsxform(IntPtr builder, IntPtr font, int count, SKRect* bounds, SKRunBufferInternal* runbuffer)
	{
		(sk_textblob_builder_alloc_run_rsxform_delegate ?? (sk_textblob_builder_alloc_run_rsxform_delegate = GetSymbol<Delegates.sk_textblob_builder_alloc_run_rsxform>("sk_textblob_builder_alloc_run_rsxform")))(builder, font, count, bounds, runbuffer);
	}

	internal unsafe static void sk_textblob_builder_alloc_run_text(IntPtr builder, IntPtr font, int count, float x, float y, int textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer)
	{
		(sk_textblob_builder_alloc_run_text_delegate ?? (sk_textblob_builder_alloc_run_text_delegate = GetSymbol<Delegates.sk_textblob_builder_alloc_run_text>("sk_textblob_builder_alloc_run_text")))(builder, font, count, x, y, textByteCount, bounds, runbuffer);
	}

	internal unsafe static void sk_textblob_builder_alloc_run_text_pos(IntPtr builder, IntPtr font, int count, int textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer)
	{
		(sk_textblob_builder_alloc_run_text_pos_delegate ?? (sk_textblob_builder_alloc_run_text_pos_delegate = GetSymbol<Delegates.sk_textblob_builder_alloc_run_text_pos>("sk_textblob_builder_alloc_run_text_pos")))(builder, font, count, textByteCount, bounds, runbuffer);
	}

	internal unsafe static void sk_textblob_builder_alloc_run_text_pos_h(IntPtr builder, IntPtr font, int count, float y, int textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer)
	{
		(sk_textblob_builder_alloc_run_text_pos_h_delegate ?? (sk_textblob_builder_alloc_run_text_pos_h_delegate = GetSymbol<Delegates.sk_textblob_builder_alloc_run_text_pos_h>("sk_textblob_builder_alloc_run_text_pos_h")))(builder, font, count, y, textByteCount, bounds, runbuffer);
	}

	internal unsafe static void sk_textblob_builder_alloc_run_text_rsxform(IntPtr builder, IntPtr font, int count, int textByteCount, SKRect* bounds, SKRunBufferInternal* runbuffer)
	{
		(sk_textblob_builder_alloc_run_text_rsxform_delegate ?? (sk_textblob_builder_alloc_run_text_rsxform_delegate = GetSymbol<Delegates.sk_textblob_builder_alloc_run_text_rsxform>("sk_textblob_builder_alloc_run_text_rsxform")))(builder, font, count, textByteCount, bounds, runbuffer);
	}

	internal static void sk_textblob_builder_delete(IntPtr builder)
	{
		(sk_textblob_builder_delete_delegate ?? (sk_textblob_builder_delete_delegate = GetSymbol<Delegates.sk_textblob_builder_delete>("sk_textblob_builder_delete")))(builder);
	}

	internal static IntPtr sk_textblob_builder_make(IntPtr builder)
	{
		return (sk_textblob_builder_make_delegate ?? (sk_textblob_builder_make_delegate = GetSymbol<Delegates.sk_textblob_builder_make>("sk_textblob_builder_make")))(builder);
	}

	internal static IntPtr sk_textblob_builder_new()
	{
		return (sk_textblob_builder_new_delegate ?? (sk_textblob_builder_new_delegate = GetSymbol<Delegates.sk_textblob_builder_new>("sk_textblob_builder_new")))();
	}

	internal unsafe static void sk_textblob_get_bounds(IntPtr blob, SKRect* bounds)
	{
		(sk_textblob_get_bounds_delegate ?? (sk_textblob_get_bounds_delegate = GetSymbol<Delegates.sk_textblob_get_bounds>("sk_textblob_get_bounds")))(blob, bounds);
	}

	internal unsafe static int sk_textblob_get_intercepts(IntPtr blob, float* bounds, float* intervals, IntPtr paint)
	{
		return (sk_textblob_get_intercepts_delegate ?? (sk_textblob_get_intercepts_delegate = GetSymbol<Delegates.sk_textblob_get_intercepts>("sk_textblob_get_intercepts")))(blob, bounds, intervals, paint);
	}

	internal static uint sk_textblob_get_unique_id(IntPtr blob)
	{
		return (sk_textblob_get_unique_id_delegate ?? (sk_textblob_get_unique_id_delegate = GetSymbol<Delegates.sk_textblob_get_unique_id>("sk_textblob_get_unique_id")))(blob);
	}

	internal static void sk_textblob_ref(IntPtr blob)
	{
		(sk_textblob_ref_delegate ?? (sk_textblob_ref_delegate = GetSymbol<Delegates.sk_textblob_ref>("sk_textblob_ref")))(blob);
	}

	internal static void sk_textblob_unref(IntPtr blob)
	{
		(sk_textblob_unref_delegate ?? (sk_textblob_unref_delegate = GetSymbol<Delegates.sk_textblob_unref>("sk_textblob_unref")))(blob);
	}

	internal static int sk_fontmgr_count_families(IntPtr param0)
	{
		return (sk_fontmgr_count_families_delegate ?? (sk_fontmgr_count_families_delegate = GetSymbol<Delegates.sk_fontmgr_count_families>("sk_fontmgr_count_families")))(param0);
	}

	internal static IntPtr sk_fontmgr_create_default()
	{
		return (sk_fontmgr_create_default_delegate ?? (sk_fontmgr_create_default_delegate = GetSymbol<Delegates.sk_fontmgr_create_default>("sk_fontmgr_create_default")))();
	}

	internal static IntPtr sk_fontmgr_create_from_data(IntPtr param0, IntPtr data, int index)
	{
		return (sk_fontmgr_create_from_data_delegate ?? (sk_fontmgr_create_from_data_delegate = GetSymbol<Delegates.sk_fontmgr_create_from_data>("sk_fontmgr_create_from_data")))(param0, data, index);
	}

	internal unsafe static IntPtr sk_fontmgr_create_from_file(IntPtr param0, void* path, int index)
	{
		return (sk_fontmgr_create_from_file_delegate ?? (sk_fontmgr_create_from_file_delegate = GetSymbol<Delegates.sk_fontmgr_create_from_file>("sk_fontmgr_create_from_file")))(param0, path, index);
	}

	internal static IntPtr sk_fontmgr_create_from_stream(IntPtr param0, IntPtr stream, int index)
	{
		return (sk_fontmgr_create_from_stream_delegate ?? (sk_fontmgr_create_from_stream_delegate = GetSymbol<Delegates.sk_fontmgr_create_from_stream>("sk_fontmgr_create_from_stream")))(param0, stream, index);
	}

	internal static IntPtr sk_fontmgr_create_styleset(IntPtr param0, int index)
	{
		return (sk_fontmgr_create_styleset_delegate ?? (sk_fontmgr_create_styleset_delegate = GetSymbol<Delegates.sk_fontmgr_create_styleset>("sk_fontmgr_create_styleset")))(param0, index);
	}

	internal static void sk_fontmgr_get_family_name(IntPtr param0, int index, IntPtr familyName)
	{
		(sk_fontmgr_get_family_name_delegate ?? (sk_fontmgr_get_family_name_delegate = GetSymbol<Delegates.sk_fontmgr_get_family_name>("sk_fontmgr_get_family_name")))(param0, index, familyName);
	}

	internal static IntPtr sk_fontmgr_match_family(IntPtr param0, IntPtr familyName)
	{
		return (sk_fontmgr_match_family_delegate ?? (sk_fontmgr_match_family_delegate = GetSymbol<Delegates.sk_fontmgr_match_family>("sk_fontmgr_match_family")))(param0, familyName);
	}

	internal static IntPtr sk_fontmgr_match_family_style(IntPtr param0, IntPtr familyName, IntPtr style)
	{
		return (sk_fontmgr_match_family_style_delegate ?? (sk_fontmgr_match_family_style_delegate = GetSymbol<Delegates.sk_fontmgr_match_family_style>("sk_fontmgr_match_family_style")))(param0, familyName, style);
	}

	internal static IntPtr sk_fontmgr_match_family_style_character(IntPtr param0, IntPtr familyName, IntPtr style, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] bcp47, int bcp47Count, int character)
	{
		return (sk_fontmgr_match_family_style_character_delegate ?? (sk_fontmgr_match_family_style_character_delegate = GetSymbol<Delegates.sk_fontmgr_match_family_style_character>("sk_fontmgr_match_family_style_character")))(param0, familyName, style, bcp47, bcp47Count, character);
	}

	internal static IntPtr sk_fontmgr_ref_default()
	{
		return (sk_fontmgr_ref_default_delegate ?? (sk_fontmgr_ref_default_delegate = GetSymbol<Delegates.sk_fontmgr_ref_default>("sk_fontmgr_ref_default")))();
	}

	internal static void sk_fontmgr_unref(IntPtr param0)
	{
		(sk_fontmgr_unref_delegate ?? (sk_fontmgr_unref_delegate = GetSymbol<Delegates.sk_fontmgr_unref>("sk_fontmgr_unref")))(param0);
	}

	internal static void sk_fontstyle_delete(IntPtr fs)
	{
		(sk_fontstyle_delete_delegate ?? (sk_fontstyle_delete_delegate = GetSymbol<Delegates.sk_fontstyle_delete>("sk_fontstyle_delete")))(fs);
	}

	internal static SKFontStyleSlant sk_fontstyle_get_slant(IntPtr fs)
	{
		return (sk_fontstyle_get_slant_delegate ?? (sk_fontstyle_get_slant_delegate = GetSymbol<Delegates.sk_fontstyle_get_slant>("sk_fontstyle_get_slant")))(fs);
	}

	internal static int sk_fontstyle_get_weight(IntPtr fs)
	{
		return (sk_fontstyle_get_weight_delegate ?? (sk_fontstyle_get_weight_delegate = GetSymbol<Delegates.sk_fontstyle_get_weight>("sk_fontstyle_get_weight")))(fs);
	}

	internal static int sk_fontstyle_get_width(IntPtr fs)
	{
		return (sk_fontstyle_get_width_delegate ?? (sk_fontstyle_get_width_delegate = GetSymbol<Delegates.sk_fontstyle_get_width>("sk_fontstyle_get_width")))(fs);
	}

	internal static IntPtr sk_fontstyle_new(int weight, int width, SKFontStyleSlant slant)
	{
		return (sk_fontstyle_new_delegate ?? (sk_fontstyle_new_delegate = GetSymbol<Delegates.sk_fontstyle_new>("sk_fontstyle_new")))(weight, width, slant);
	}

	internal static IntPtr sk_fontstyleset_create_empty()
	{
		return (sk_fontstyleset_create_empty_delegate ?? (sk_fontstyleset_create_empty_delegate = GetSymbol<Delegates.sk_fontstyleset_create_empty>("sk_fontstyleset_create_empty")))();
	}

	internal static IntPtr sk_fontstyleset_create_typeface(IntPtr fss, int index)
	{
		return (sk_fontstyleset_create_typeface_delegate ?? (sk_fontstyleset_create_typeface_delegate = GetSymbol<Delegates.sk_fontstyleset_create_typeface>("sk_fontstyleset_create_typeface")))(fss, index);
	}

	internal static int sk_fontstyleset_get_count(IntPtr fss)
	{
		return (sk_fontstyleset_get_count_delegate ?? (sk_fontstyleset_get_count_delegate = GetSymbol<Delegates.sk_fontstyleset_get_count>("sk_fontstyleset_get_count")))(fss);
	}

	internal static void sk_fontstyleset_get_style(IntPtr fss, int index, IntPtr fs, IntPtr style)
	{
		(sk_fontstyleset_get_style_delegate ?? (sk_fontstyleset_get_style_delegate = GetSymbol<Delegates.sk_fontstyleset_get_style>("sk_fontstyleset_get_style")))(fss, index, fs, style);
	}

	internal static IntPtr sk_fontstyleset_match_style(IntPtr fss, IntPtr style)
	{
		return (sk_fontstyleset_match_style_delegate ?? (sk_fontstyleset_match_style_delegate = GetSymbol<Delegates.sk_fontstyleset_match_style>("sk_fontstyleset_match_style")))(fss, style);
	}

	internal static void sk_fontstyleset_unref(IntPtr fss)
	{
		(sk_fontstyleset_unref_delegate ?? (sk_fontstyleset_unref_delegate = GetSymbol<Delegates.sk_fontstyleset_unref>("sk_fontstyleset_unref")))(fss);
	}

	internal static IntPtr sk_typeface_copy_table_data(IntPtr typeface, uint tag)
	{
		return (sk_typeface_copy_table_data_delegate ?? (sk_typeface_copy_table_data_delegate = GetSymbol<Delegates.sk_typeface_copy_table_data>("sk_typeface_copy_table_data")))(typeface, tag);
	}

	internal static int sk_typeface_count_glyphs(IntPtr typeface)
	{
		return (sk_typeface_count_glyphs_delegate ?? (sk_typeface_count_glyphs_delegate = GetSymbol<Delegates.sk_typeface_count_glyphs>("sk_typeface_count_glyphs")))(typeface);
	}

	internal static int sk_typeface_count_tables(IntPtr typeface)
	{
		return (sk_typeface_count_tables_delegate ?? (sk_typeface_count_tables_delegate = GetSymbol<Delegates.sk_typeface_count_tables>("sk_typeface_count_tables")))(typeface);
	}

	internal static IntPtr sk_typeface_create_default()
	{
		return (sk_typeface_create_default_delegate ?? (sk_typeface_create_default_delegate = GetSymbol<Delegates.sk_typeface_create_default>("sk_typeface_create_default")))();
	}

	internal static IntPtr sk_typeface_create_from_data(IntPtr data, int index)
	{
		return (sk_typeface_create_from_data_delegate ?? (sk_typeface_create_from_data_delegate = GetSymbol<Delegates.sk_typeface_create_from_data>("sk_typeface_create_from_data")))(data, index);
	}

	internal unsafe static IntPtr sk_typeface_create_from_file(void* path, int index)
	{
		return (sk_typeface_create_from_file_delegate ?? (sk_typeface_create_from_file_delegate = GetSymbol<Delegates.sk_typeface_create_from_file>("sk_typeface_create_from_file")))(path, index);
	}

	internal static IntPtr sk_typeface_create_from_name(IntPtr familyName, IntPtr style)
	{
		return (sk_typeface_create_from_name_delegate ?? (sk_typeface_create_from_name_delegate = GetSymbol<Delegates.sk_typeface_create_from_name>("sk_typeface_create_from_name")))(familyName, style);
	}

	internal static IntPtr sk_typeface_create_from_stream(IntPtr stream, int index)
	{
		return (sk_typeface_create_from_stream_delegate ?? (sk_typeface_create_from_stream_delegate = GetSymbol<Delegates.sk_typeface_create_from_stream>("sk_typeface_create_from_stream")))(stream, index);
	}

	internal static IntPtr sk_typeface_get_family_name(IntPtr typeface)
	{
		return (sk_typeface_get_family_name_delegate ?? (sk_typeface_get_family_name_delegate = GetSymbol<Delegates.sk_typeface_get_family_name>("sk_typeface_get_family_name")))(typeface);
	}

	internal static SKFontStyleSlant sk_typeface_get_font_slant(IntPtr typeface)
	{
		return (sk_typeface_get_font_slant_delegate ?? (sk_typeface_get_font_slant_delegate = GetSymbol<Delegates.sk_typeface_get_font_slant>("sk_typeface_get_font_slant")))(typeface);
	}

	internal static int sk_typeface_get_font_weight(IntPtr typeface)
	{
		return (sk_typeface_get_font_weight_delegate ?? (sk_typeface_get_font_weight_delegate = GetSymbol<Delegates.sk_typeface_get_font_weight>("sk_typeface_get_font_weight")))(typeface);
	}

	internal static int sk_typeface_get_font_width(IntPtr typeface)
	{
		return (sk_typeface_get_font_width_delegate ?? (sk_typeface_get_font_width_delegate = GetSymbol<Delegates.sk_typeface_get_font_width>("sk_typeface_get_font_width")))(typeface);
	}

	internal static IntPtr sk_typeface_get_fontstyle(IntPtr typeface)
	{
		return (sk_typeface_get_fontstyle_delegate ?? (sk_typeface_get_fontstyle_delegate = GetSymbol<Delegates.sk_typeface_get_fontstyle>("sk_typeface_get_fontstyle")))(typeface);
	}

	internal unsafe static bool sk_typeface_get_kerning_pair_adjustments(IntPtr typeface, ushort* glyphs, int count, int* adjustments)
	{
		return (sk_typeface_get_kerning_pair_adjustments_delegate ?? (sk_typeface_get_kerning_pair_adjustments_delegate = GetSymbol<Delegates.sk_typeface_get_kerning_pair_adjustments>("sk_typeface_get_kerning_pair_adjustments")))(typeface, glyphs, count, adjustments);
	}

	internal static IntPtr sk_typeface_get_post_script_name(IntPtr typeface)
	{
		return (sk_typeface_get_post_script_name_delegate ?? (sk_typeface_get_post_script_name_delegate = GetSymbol<Delegates.sk_typeface_get_post_script_name>("sk_typeface_get_post_script_name")))(typeface);
	}

	internal unsafe static IntPtr sk_typeface_get_table_data(IntPtr typeface, uint tag, IntPtr offset, IntPtr length, void* data)
	{
		return (sk_typeface_get_table_data_delegate ?? (sk_typeface_get_table_data_delegate = GetSymbol<Delegates.sk_typeface_get_table_data>("sk_typeface_get_table_data")))(typeface, tag, offset, length, data);
	}

	internal static IntPtr sk_typeface_get_table_size(IntPtr typeface, uint tag)
	{
		return (sk_typeface_get_table_size_delegate ?? (sk_typeface_get_table_size_delegate = GetSymbol<Delegates.sk_typeface_get_table_size>("sk_typeface_get_table_size")))(typeface, tag);
	}

	internal unsafe static int sk_typeface_get_table_tags(IntPtr typeface, uint* tags)
	{
		return (sk_typeface_get_table_tags_delegate ?? (sk_typeface_get_table_tags_delegate = GetSymbol<Delegates.sk_typeface_get_table_tags>("sk_typeface_get_table_tags")))(typeface, tags);
	}

	internal static int sk_typeface_get_units_per_em(IntPtr typeface)
	{
		return (sk_typeface_get_units_per_em_delegate ?? (sk_typeface_get_units_per_em_delegate = GetSymbol<Delegates.sk_typeface_get_units_per_em>("sk_typeface_get_units_per_em")))(typeface);
	}

	internal static bool sk_typeface_is_fixed_pitch(IntPtr typeface)
	{
		return (sk_typeface_is_fixed_pitch_delegate ?? (sk_typeface_is_fixed_pitch_delegate = GetSymbol<Delegates.sk_typeface_is_fixed_pitch>("sk_typeface_is_fixed_pitch")))(typeface);
	}

	internal unsafe static IntPtr sk_typeface_open_stream(IntPtr typeface, int* ttcIndex)
	{
		return (sk_typeface_open_stream_delegate ?? (sk_typeface_open_stream_delegate = GetSymbol<Delegates.sk_typeface_open_stream>("sk_typeface_open_stream")))(typeface, ttcIndex);
	}

	internal static IntPtr sk_typeface_ref_default()
	{
		return (sk_typeface_ref_default_delegate ?? (sk_typeface_ref_default_delegate = GetSymbol<Delegates.sk_typeface_ref_default>("sk_typeface_ref_default")))();
	}

	internal static ushort sk_typeface_unichar_to_glyph(IntPtr typeface, int unichar)
	{
		return (sk_typeface_unichar_to_glyph_delegate ?? (sk_typeface_unichar_to_glyph_delegate = GetSymbol<Delegates.sk_typeface_unichar_to_glyph>("sk_typeface_unichar_to_glyph")))(typeface, unichar);
	}

	internal unsafe static void sk_typeface_unichars_to_glyphs(IntPtr typeface, int* unichars, int count, ushort* glyphs)
	{
		(sk_typeface_unichars_to_glyphs_delegate ?? (sk_typeface_unichars_to_glyphs_delegate = GetSymbol<Delegates.sk_typeface_unichars_to_glyphs>("sk_typeface_unichars_to_glyphs")))(typeface, unichars, count, glyphs);
	}

	internal static void sk_typeface_unref(IntPtr typeface)
	{
		(sk_typeface_unref_delegate ?? (sk_typeface_unref_delegate = GetSymbol<Delegates.sk_typeface_unref>("sk_typeface_unref")))(typeface);
	}

	internal unsafe static IntPtr sk_vertices_make_copy(SKVertexMode vmode, int vertexCount, SKPoint* positions, SKPoint* texs, uint* colors, int indexCount, ushort* indices)
	{
		return (sk_vertices_make_copy_delegate ?? (sk_vertices_make_copy_delegate = GetSymbol<Delegates.sk_vertices_make_copy>("sk_vertices_make_copy")))(vmode, vertexCount, positions, texs, colors, indexCount, indices);
	}

	internal static void sk_vertices_ref(IntPtr cvertices)
	{
		(sk_vertices_ref_delegate ?? (sk_vertices_ref_delegate = GetSymbol<Delegates.sk_vertices_ref>("sk_vertices_ref")))(cvertices);
	}

	internal static void sk_vertices_unref(IntPtr cvertices)
	{
		(sk_vertices_unref_delegate ?? (sk_vertices_unref_delegate = GetSymbol<Delegates.sk_vertices_unref>("sk_vertices_unref")))(cvertices);
	}

	internal static IntPtr sk_compatpaint_clone(IntPtr paint)
	{
		return (sk_compatpaint_clone_delegate ?? (sk_compatpaint_clone_delegate = GetSymbol<Delegates.sk_compatpaint_clone>("sk_compatpaint_clone")))(paint);
	}

	internal static void sk_compatpaint_delete(IntPtr paint)
	{
		(sk_compatpaint_delete_delegate ?? (sk_compatpaint_delete_delegate = GetSymbol<Delegates.sk_compatpaint_delete>("sk_compatpaint_delete")))(paint);
	}

	internal static int sk_compatpaint_get_filter_quality(IntPtr paint)
	{
		return (sk_compatpaint_get_filter_quality_delegate ?? (sk_compatpaint_get_filter_quality_delegate = GetSymbol<Delegates.sk_compatpaint_get_filter_quality>("sk_compatpaint_get_filter_quality")))(paint);
	}

	internal static IntPtr sk_compatpaint_get_font(IntPtr paint)
	{
		return (sk_compatpaint_get_font_delegate ?? (sk_compatpaint_get_font_delegate = GetSymbol<Delegates.sk_compatpaint_get_font>("sk_compatpaint_get_font")))(paint);
	}

	internal static bool sk_compatpaint_get_lcd_render_text(IntPtr paint)
	{
		return (sk_compatpaint_get_lcd_render_text_delegate ?? (sk_compatpaint_get_lcd_render_text_delegate = GetSymbol<Delegates.sk_compatpaint_get_lcd_render_text>("sk_compatpaint_get_lcd_render_text")))(paint);
	}

	internal static SKTextAlign sk_compatpaint_get_text_align(IntPtr paint)
	{
		return (sk_compatpaint_get_text_align_delegate ?? (sk_compatpaint_get_text_align_delegate = GetSymbol<Delegates.sk_compatpaint_get_text_align>("sk_compatpaint_get_text_align")))(paint);
	}

	internal static SKTextEncoding sk_compatpaint_get_text_encoding(IntPtr paint)
	{
		return (sk_compatpaint_get_text_encoding_delegate ?? (sk_compatpaint_get_text_encoding_delegate = GetSymbol<Delegates.sk_compatpaint_get_text_encoding>("sk_compatpaint_get_text_encoding")))(paint);
	}

	internal static IntPtr sk_compatpaint_make_font(IntPtr paint)
	{
		return (sk_compatpaint_make_font_delegate ?? (sk_compatpaint_make_font_delegate = GetSymbol<Delegates.sk_compatpaint_make_font>("sk_compatpaint_make_font")))(paint);
	}

	internal static IntPtr sk_compatpaint_new()
	{
		return (sk_compatpaint_new_delegate ?? (sk_compatpaint_new_delegate = GetSymbol<Delegates.sk_compatpaint_new>("sk_compatpaint_new")))();
	}

	internal static IntPtr sk_compatpaint_new_with_font(IntPtr font)
	{
		return (sk_compatpaint_new_with_font_delegate ?? (sk_compatpaint_new_with_font_delegate = GetSymbol<Delegates.sk_compatpaint_new_with_font>("sk_compatpaint_new_with_font")))(font);
	}

	internal static void sk_compatpaint_reset(IntPtr paint)
	{
		(sk_compatpaint_reset_delegate ?? (sk_compatpaint_reset_delegate = GetSymbol<Delegates.sk_compatpaint_reset>("sk_compatpaint_reset")))(paint);
	}

	internal static void sk_compatpaint_set_filter_quality(IntPtr paint, int quality)
	{
		(sk_compatpaint_set_filter_quality_delegate ?? (sk_compatpaint_set_filter_quality_delegate = GetSymbol<Delegates.sk_compatpaint_set_filter_quality>("sk_compatpaint_set_filter_quality")))(paint, quality);
	}

	internal static void sk_compatpaint_set_is_antialias(IntPtr paint, [MarshalAs(UnmanagedType.I1)] bool antialias)
	{
		(sk_compatpaint_set_is_antialias_delegate ?? (sk_compatpaint_set_is_antialias_delegate = GetSymbol<Delegates.sk_compatpaint_set_is_antialias>("sk_compatpaint_set_is_antialias")))(paint, antialias);
	}

	internal static void sk_compatpaint_set_lcd_render_text(IntPtr paint, [MarshalAs(UnmanagedType.I1)] bool lcdRenderText)
	{
		(sk_compatpaint_set_lcd_render_text_delegate ?? (sk_compatpaint_set_lcd_render_text_delegate = GetSymbol<Delegates.sk_compatpaint_set_lcd_render_text>("sk_compatpaint_set_lcd_render_text")))(paint, lcdRenderText);
	}

	internal static void sk_compatpaint_set_text_align(IntPtr paint, SKTextAlign align)
	{
		(sk_compatpaint_set_text_align_delegate ?? (sk_compatpaint_set_text_align_delegate = GetSymbol<Delegates.sk_compatpaint_set_text_align>("sk_compatpaint_set_text_align")))(paint, align);
	}

	internal static void sk_compatpaint_set_text_encoding(IntPtr paint, SKTextEncoding encoding)
	{
		(sk_compatpaint_set_text_encoding_delegate ?? (sk_compatpaint_set_text_encoding_delegate = GetSymbol<Delegates.sk_compatpaint_set_text_encoding>("sk_compatpaint_set_text_encoding")))(paint, encoding);
	}

	internal unsafe static IntPtr sk_manageddrawable_new(void* context)
	{
		return (sk_manageddrawable_new_delegate ?? (sk_manageddrawable_new_delegate = GetSymbol<Delegates.sk_manageddrawable_new>("sk_manageddrawable_new")))(context);
	}

	internal static void sk_manageddrawable_set_procs(SKManagedDrawableDelegates procs)
	{
		(sk_manageddrawable_set_procs_delegate ?? (sk_manageddrawable_set_procs_delegate = GetSymbol<Delegates.sk_manageddrawable_set_procs>("sk_manageddrawable_set_procs")))(procs);
	}

	internal static void sk_manageddrawable_unref(IntPtr param0)
	{
		(sk_manageddrawable_unref_delegate ?? (sk_manageddrawable_unref_delegate = GetSymbol<Delegates.sk_manageddrawable_unref>("sk_manageddrawable_unref")))(param0);
	}

	internal static void sk_managedstream_destroy(IntPtr s)
	{
		(sk_managedstream_destroy_delegate ?? (sk_managedstream_destroy_delegate = GetSymbol<Delegates.sk_managedstream_destroy>("sk_managedstream_destroy")))(s);
	}

	internal unsafe static IntPtr sk_managedstream_new(void* context)
	{
		return (sk_managedstream_new_delegate ?? (sk_managedstream_new_delegate = GetSymbol<Delegates.sk_managedstream_new>("sk_managedstream_new")))(context);
	}

	internal static void sk_managedstream_set_procs(SKManagedStreamDelegates procs)
	{
		(sk_managedstream_set_procs_delegate ?? (sk_managedstream_set_procs_delegate = GetSymbol<Delegates.sk_managedstream_set_procs>("sk_managedstream_set_procs")))(procs);
	}

	internal static void sk_managedwstream_destroy(IntPtr s)
	{
		(sk_managedwstream_destroy_delegate ?? (sk_managedwstream_destroy_delegate = GetSymbol<Delegates.sk_managedwstream_destroy>("sk_managedwstream_destroy")))(s);
	}

	internal unsafe static IntPtr sk_managedwstream_new(void* context)
	{
		return (sk_managedwstream_new_delegate ?? (sk_managedwstream_new_delegate = GetSymbol<Delegates.sk_managedwstream_new>("sk_managedwstream_new")))(context);
	}

	internal static void sk_managedwstream_set_procs(SKManagedWStreamDelegates procs)
	{
		(sk_managedwstream_set_procs_delegate ?? (sk_managedwstream_set_procs_delegate = GetSymbol<Delegates.sk_managedwstream_set_procs>("sk_managedwstream_set_procs")))(procs);
	}

	internal static void sk_managedtracememorydump_delete(IntPtr param0)
	{
		(sk_managedtracememorydump_delete_delegate ?? (sk_managedtracememorydump_delete_delegate = GetSymbol<Delegates.sk_managedtracememorydump_delete>("sk_managedtracememorydump_delete")))(param0);
	}

	internal unsafe static IntPtr sk_managedtracememorydump_new([MarshalAs(UnmanagedType.I1)] bool detailed, [MarshalAs(UnmanagedType.I1)] bool dumpWrapped, void* context)
	{
		return (sk_managedtracememorydump_new_delegate ?? (sk_managedtracememorydump_new_delegate = GetSymbol<Delegates.sk_managedtracememorydump_new>("sk_managedtracememorydump_new")))(detailed, dumpWrapped, context);
	}

	internal static void sk_managedtracememorydump_set_procs(SKManagedTraceMemoryDumpDelegates procs)
	{
		(sk_managedtracememorydump_set_procs_delegate ?? (sk_managedtracememorydump_set_procs_delegate = GetSymbol<Delegates.sk_managedtracememorydump_set_procs>("sk_managedtracememorydump_set_procs")))(procs);
	}
}
