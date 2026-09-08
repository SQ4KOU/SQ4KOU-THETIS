using System;

namespace SkiaSharp;

public class SKCanvas : SKObject
{
	private const int PatchCornerCount = 4;

	private const int PatchCubicsCount = 12;

	private const double RadiansCircle = Math.PI * 2.0;

	private const double DegreesCircle = 360.0;

	public SKRect LocalClipBounds
	{
		get
		{
			GetLocalClipBounds(out var bounds);
			return bounds;
		}
	}

	public SKRectI DeviceClipBounds
	{
		get
		{
			GetDeviceClipBounds(out var bounds);
			return bounds;
		}
	}

	public bool IsClipEmpty => SkiaApi.sk_canvas_is_clip_empty(Handle);

	public bool IsClipRect => SkiaApi.sk_canvas_is_clip_rect(Handle);

	public SKSurface? Surface => SKSurface.GetObject(SkiaApi.sk_get_surface(Handle), owns: false, unrefExisting: false);

	public GRRecordingContext? Context => GRRecordingContext.GetObject(SkiaApi.sk_get_recording_context(Handle), owns: false, unrefExisting: false);

	public SKMatrix TotalMatrix => TotalMatrix44.Matrix;

	public unsafe SKMatrix44 TotalMatrix44
	{
		get
		{
			SKMatrix44 result = default(SKMatrix44);
			SkiaApi.sk_canvas_get_matrix(Handle, &result);
			return result;
		}
	}

	public int SaveCount => SkiaApi.sk_canvas_get_save_count(Handle);

	internal SKCanvas(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	public SKCanvas(SKBitmap bitmap)
		: this(IntPtr.Zero, owns: true)
	{
		if (bitmap == null)
		{
			throw new ArgumentNullException("bitmap");
		}
		Handle = SkiaApi.sk_canvas_new_from_bitmap(bitmap.Handle);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void DisposeNative()
	{
		SkiaApi.sk_canvas_destroy(Handle);
	}

	public void Discard()
	{
		SkiaApi.sk_canvas_discard(Handle);
	}

	public unsafe bool QuickReject(SKRect rect)
	{
		return SkiaApi.sk_canvas_quick_reject(Handle, &rect);
	}

	public bool QuickReject(SKPath path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (!path.IsEmpty)
		{
			return QuickReject(path.Bounds);
		}
		return true;
	}

	public int Save()
	{
		if (Handle == IntPtr.Zero)
		{
			throw new ObjectDisposedException("SKCanvas");
		}
		return SkiaApi.sk_canvas_save(Handle);
	}

	public unsafe int SaveLayer(SKRect limit, SKPaint? paint)
	{
		return SkiaApi.sk_canvas_save_layer(Handle, &limit, paint?.Handle ?? IntPtr.Zero);
	}

	public unsafe int SaveLayer(SKPaint? paint)
	{
		return SkiaApi.sk_canvas_save_layer(Handle, null, paint?.Handle ?? IntPtr.Zero);
	}

	public unsafe int SaveLayer(in SKCanvasSaveLayerRec rec)
	{
		SKCanvasSaveLayerRecNative sKCanvasSaveLayerRecNative = rec.ToNative();
		return SkiaApi.sk_canvas_save_layer_rec(Handle, &sKCanvasSaveLayerRecNative);
	}

	public unsafe int SaveLayer()
	{
		return SkiaApi.sk_canvas_save_layer(Handle, null, IntPtr.Zero);
	}

	public void DrawColor(SKColor color, SKBlendMode mode = SKBlendMode.Src)
	{
		SkiaApi.sk_canvas_draw_color(Handle, (uint)color, mode);
	}

	public void DrawColor(SKColorF color, SKBlendMode mode = SKBlendMode.Src)
	{
		SkiaApi.sk_canvas_draw_color4f(Handle, color, mode);
	}

	public void DrawLine(SKPoint p0, SKPoint p1, SKPaint paint)
	{
		DrawLine(p0.X, p0.Y, p1.X, p1.Y, paint);
	}

	public void DrawLine(float x0, float y0, float x1, float y1, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_line(Handle, x0, y0, x1, y1, paint.Handle);
	}

	public void Clear()
	{
		Clear(SKColors.Empty);
	}

	public void Clear(SKColor color)
	{
		SkiaApi.sk_canvas_clear(Handle, (uint)color);
	}

	public void Clear(SKColorF color)
	{
		SkiaApi.sk_canvas_clear_color4f(Handle, color);
	}

	public void Restore()
	{
		SkiaApi.sk_canvas_restore(Handle);
	}

	public void RestoreToCount(int count)
	{
		SkiaApi.sk_canvas_restore_to_count(Handle, count);
	}

	public void Translate(float dx, float dy)
	{
		if (dx != 0f || dy != 0f)
		{
			SkiaApi.sk_canvas_translate(Handle, dx, dy);
		}
	}

	public void Translate(SKPoint point)
	{
		if (!point.IsEmpty)
		{
			SkiaApi.sk_canvas_translate(Handle, point.X, point.Y);
		}
	}

	public void Scale(float s)
	{
		if (s != 1f)
		{
			SkiaApi.sk_canvas_scale(Handle, s, s);
		}
	}

	public void Scale(float sx, float sy)
	{
		if (sx != 1f || sy != 1f)
		{
			SkiaApi.sk_canvas_scale(Handle, sx, sy);
		}
	}

	public void Scale(SKPoint size)
	{
		if (!size.IsEmpty)
		{
			SkiaApi.sk_canvas_scale(Handle, size.X, size.Y);
		}
	}

	public void Scale(float sx, float sy, float px, float py)
	{
		if (sx != 1f || sy != 1f)
		{
			Translate(px, py);
			Scale(sx, sy);
			Translate(0f - px, 0f - py);
		}
	}

	public void RotateDegrees(float degrees)
	{
		if ((double)degrees % 360.0 != 0.0)
		{
			SkiaApi.sk_canvas_rotate_degrees(Handle, degrees);
		}
	}

	public void RotateRadians(float radians)
	{
		if ((double)radians % (Math.PI * 2.0) != 0.0)
		{
			SkiaApi.sk_canvas_rotate_radians(Handle, radians);
		}
	}

	public void RotateDegrees(float degrees, float px, float py)
	{
		if ((double)degrees % 360.0 != 0.0)
		{
			Translate(px, py);
			RotateDegrees(degrees);
			Translate(0f - px, 0f - py);
		}
	}

	public void RotateRadians(float radians, float px, float py)
	{
		if ((double)radians % (Math.PI * 2.0) != 0.0)
		{
			Translate(px, py);
			RotateRadians(radians);
			Translate(0f - px, 0f - py);
		}
	}

	public void Skew(float sx, float sy)
	{
		if (sx != 0f || sy != 0f)
		{
			SkiaApi.sk_canvas_skew(Handle, sx, sy);
		}
	}

	public void Skew(SKPoint skew)
	{
		if (!skew.IsEmpty)
		{
			SkiaApi.sk_canvas_skew(Handle, skew.X, skew.Y);
		}
	}

	public void Concat(in SKMatrix m)
	{
		Concat((SKMatrix44)m);
	}

	public unsafe void Concat(in SKMatrix44 m)
	{
		fixed (SKMatrix44* cmatrix = &m)
		{
			SkiaApi.sk_canvas_concat(Handle, cmatrix);
		}
	}

	public unsafe void ClipRect(SKRect rect, SKClipOperation operation = SKClipOperation.Intersect, bool antialias = false)
	{
		SkiaApi.sk_canvas_clip_rect_with_operation(Handle, &rect, operation, antialias);
	}

	public void ClipRoundRect(SKRoundRect rect, SKClipOperation operation = SKClipOperation.Intersect, bool antialias = false)
	{
		if (rect == null)
		{
			throw new ArgumentNullException("rect");
		}
		SkiaApi.sk_canvas_clip_rrect_with_operation(Handle, rect.Handle, operation, antialias);
	}

	public void ClipPath(SKPath path, SKClipOperation operation = SKClipOperation.Intersect, bool antialias = false)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		SkiaApi.sk_canvas_clip_path_with_operation(Handle, path.Handle, operation, antialias);
	}

	public void ClipRegion(SKRegion region, SKClipOperation operation = SKClipOperation.Intersect)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		SkiaApi.sk_canvas_clip_region(Handle, region.Handle, operation);
	}

	public unsafe bool GetLocalClipBounds(out SKRect bounds)
	{
		fixed (SKRect* cbounds = &bounds)
		{
			return SkiaApi.sk_canvas_get_local_clip_bounds(Handle, cbounds);
		}
	}

	public unsafe bool GetDeviceClipBounds(out SKRectI bounds)
	{
		fixed (SKRectI* cbounds = &bounds)
		{
			return SkiaApi.sk_canvas_get_device_clip_bounds(Handle, cbounds);
		}
	}

	public void DrawPaint(SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_paint(Handle, paint.Handle);
	}

	public void DrawRegion(SKRegion region, SKPaint paint)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_region(Handle, region.Handle, paint.Handle);
	}

	public void DrawRect(float x, float y, float w, float h, SKPaint paint)
	{
		DrawRect(SKRect.Create(x, y, w, h), paint);
	}

	public unsafe void DrawRect(SKRect rect, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_rect(Handle, &rect, paint.Handle);
	}

	public void DrawRoundRect(SKRoundRect rect, SKPaint paint)
	{
		if (rect == null)
		{
			throw new ArgumentNullException("rect");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_rrect(Handle, rect.Handle, paint.Handle);
	}

	public void DrawRoundRect(float x, float y, float w, float h, float rx, float ry, SKPaint paint)
	{
		DrawRoundRect(SKRect.Create(x, y, w, h), rx, ry, paint);
	}

	public unsafe void DrawRoundRect(SKRect rect, float rx, float ry, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_round_rect(Handle, &rect, rx, ry, paint.Handle);
	}

	public void DrawRoundRect(SKRect rect, SKSize r, SKPaint paint)
	{
		DrawRoundRect(rect, r.Width, r.Height, paint);
	}

	public void DrawOval(float cx, float cy, float rx, float ry, SKPaint paint)
	{
		DrawOval(new SKRect(cx - rx, cy - ry, cx + rx, cy + ry), paint);
	}

	public void DrawOval(SKPoint c, SKSize r, SKPaint paint)
	{
		DrawOval(c.X, c.Y, r.Width, r.Height, paint);
	}

	public unsafe void DrawOval(SKRect rect, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_oval(Handle, &rect, paint.Handle);
	}

	public void DrawCircle(float cx, float cy, float radius, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_circle(Handle, cx, cy, radius, paint.Handle);
	}

	public void DrawCircle(SKPoint c, float radius, SKPaint paint)
	{
		DrawCircle(c.X, c.Y, radius, paint);
	}

	public void DrawPath(SKPath path, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		SkiaApi.sk_canvas_draw_path(Handle, path.Handle, paint.Handle);
	}

	public unsafe void DrawPoints(SKPointMode mode, SKPoint[] points, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		fixed (SKPoint* points2 = points)
		{
			SkiaApi.sk_canvas_draw_points(Handle, mode, (IntPtr)points.Length, points2, paint.Handle);
		}
	}

	public void DrawPoint(SKPoint p, SKPaint paint)
	{
		DrawPoint(p.X, p.Y, paint);
	}

	public void DrawPoint(float x, float y, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_point(Handle, x, y, paint.Handle);
	}

	public void DrawPoint(SKPoint p, SKColor color)
	{
		DrawPoint(p.X, p.Y, color);
	}

	public void DrawPoint(float x, float y, SKColor color)
	{
		using SKPaint paint = new SKPaint
		{
			Color = color,
			BlendMode = SKBlendMode.Src
		};
		DrawPoint(x, y, paint);
	}

	public void DrawImage(SKImage image, SKPoint p, SKPaint paint = null)
	{
		DrawImage(image, p.X, p.Y, paint?.FilterQuality.ToSamplingOptions() ?? SKSamplingOptions.Default, paint);
	}

	public void DrawImage(SKImage image, SKPoint p, SKSamplingOptions sampling, SKPaint paint = null)
	{
		DrawImage(image, p.X, p.Y, sampling, paint);
	}

	public void DrawImage(SKImage image, float x, float y, SKPaint paint = null)
	{
		DrawImage(image, x, y, paint?.FilterQuality.ToSamplingOptions() ?? SKSamplingOptions.Default, paint);
	}

	public unsafe void DrawImage(SKImage image, float x, float y, SKSamplingOptions sampling, SKPaint paint = null)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		SkiaApi.sk_canvas_draw_image(Handle, image.Handle, x, y, &sampling, paint?.Handle ?? IntPtr.Zero);
	}

	public unsafe void DrawImage(SKImage image, SKRect dest, SKPaint paint = null)
	{
		DrawImage(image, null, &dest, paint?.FilterQuality.ToSamplingOptions() ?? SKSamplingOptions.Default, paint);
	}

	public unsafe void DrawImage(SKImage image, SKRect dest, SKSamplingOptions sampling, SKPaint paint = null)
	{
		DrawImage(image, null, &dest, sampling, paint);
	}

	public unsafe void DrawImage(SKImage image, SKRect source, SKRect dest, SKPaint paint = null)
	{
		DrawImage(image, &source, &dest, paint?.FilterQuality.ToSamplingOptions() ?? SKSamplingOptions.Default, paint);
	}

	public unsafe void DrawImage(SKImage image, SKRect source, SKRect dest, SKSamplingOptions sampling, SKPaint paint = null)
	{
		DrawImage(image, &source, &dest, sampling, paint);
	}

	private unsafe void DrawImage(SKImage image, SKRect* source, SKRect* dest, SKSamplingOptions sampling, SKPaint paint = null)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		SkiaApi.sk_canvas_draw_image_rect(Handle, image.Handle, source, dest, &sampling, paint?.Handle ?? IntPtr.Zero);
	}

	public void DrawPicture(SKPicture picture, float x, float y, SKPaint paint = null)
	{
		DrawPicture(picture, SKMatrix.CreateTranslation(x, y), paint);
	}

	public void DrawPicture(SKPicture picture, SKPoint p, SKPaint paint = null)
	{
		DrawPicture(picture, p.X, p.Y, paint);
	}

	public unsafe void DrawPicture(SKPicture picture, in SKMatrix matrix, SKPaint paint = null)
	{
		if (picture == null)
		{
			throw new ArgumentNullException("picture");
		}
		fixed (SKMatrix* cmatrix = &matrix)
		{
			SkiaApi.sk_canvas_draw_picture(Handle, picture.Handle, cmatrix, paint?.Handle ?? IntPtr.Zero);
		}
	}

	public unsafe void DrawPicture(SKPicture picture, SKPaint paint = null)
	{
		if (picture == null)
		{
			throw new ArgumentNullException("picture");
		}
		SkiaApi.sk_canvas_draw_picture(Handle, picture.Handle, null, paint?.Handle ?? IntPtr.Zero);
	}

	public unsafe void DrawDrawable(SKDrawable drawable, in SKMatrix matrix)
	{
		if (drawable == null)
		{
			throw new ArgumentNullException("drawable");
		}
		fixed (SKMatrix* cmatrix = &matrix)
		{
			SkiaApi.sk_canvas_draw_drawable(Handle, drawable.Handle, cmatrix);
		}
	}

	public void DrawDrawable(SKDrawable drawable, float x, float y)
	{
		if (drawable == null)
		{
			throw new ArgumentNullException("drawable");
		}
		DrawDrawable(drawable, SKMatrix.CreateTranslation(x, y));
	}

	public void DrawDrawable(SKDrawable drawable, SKPoint p)
	{
		if (drawable == null)
		{
			throw new ArgumentNullException("drawable");
		}
		DrawDrawable(drawable, SKMatrix.CreateTranslation(p.X, p.Y));
	}

	public void DrawBitmap(SKBitmap bitmap, SKPoint p, SKPaint paint = null)
	{
		DrawBitmap(bitmap, p.X, p.Y, paint);
	}

	public void DrawBitmap(SKBitmap bitmap, float x, float y, SKPaint paint = null)
	{
		using SKImage image = SKImage.FromBitmap(bitmap);
		DrawImage(image, x, y, paint);
	}

	public void DrawBitmap(SKBitmap bitmap, SKRect dest, SKPaint paint = null)
	{
		using SKImage image = SKImage.FromBitmap(bitmap);
		DrawImage(image, dest, paint);
	}

	public void DrawBitmap(SKBitmap bitmap, SKRect source, SKRect dest, SKPaint paint = null)
	{
		using SKImage image = SKImage.FromBitmap(bitmap);
		DrawImage(image, source, dest, paint);
	}

	public void DrawSurface(SKSurface surface, SKPoint p, SKPaint paint = null)
	{
		DrawSurface(surface, p.X, p.Y, paint);
	}

	public void DrawSurface(SKSurface surface, float x, float y, SKPaint paint = null)
	{
		if (surface == null)
		{
			throw new ArgumentNullException("surface");
		}
		surface.Draw(this, x, y, paint);
	}

	public void DrawText(SKTextBlob text, float x, float y, SKPaint paint)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_text_blob(Handle, text.Handle, x, y, paint.Handle);
	}

	[Obsolete("Use DrawText(string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
	public void DrawText(string text, SKPoint p, SKPaint paint)
	{
		DrawText(text, p, paint.TextAlign, paint.GetFont(), paint);
	}

	[Obsolete("Use DrawText(string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
	public void DrawText(string text, float x, float y, SKPaint paint)
	{
		DrawText(text, x, y, paint.TextAlign, paint.GetFont(), paint);
	}

	public void DrawText(string text, SKPoint p, SKFont font, SKPaint paint)
	{
		DrawText(text, p, paint.TextAlign, font, paint);
	}

	public void DrawText(string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint)
	{
		DrawText(text, p.X, p.Y, textAlign, font, paint);
	}

	public void DrawText(string text, float x, float y, SKFont font, SKPaint paint)
	{
		DrawText(text, x, y, paint.TextAlign, font, paint);
	}

	public void DrawText(string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		if (textAlign != SKTextAlign.Left)
		{
			float num = font.MeasureText(text);
			if (textAlign == SKTextAlign.Center)
			{
				num *= 0.5f;
			}
			x -= num;
		}
		using SKTextBlob sKTextBlob = SKTextBlob.Create(text, font);
		if (sKTextBlob != null)
		{
			DrawText(sKTextBlob, x, y, paint);
		}
	}

	[Obsolete("Use DrawTextOnPath(string text, SKPath path, float hOffset, float vOffset, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
	public void DrawTextOnPath(string text, SKPath path, SKPoint offset, SKPaint paint)
	{
		DrawTextOnPath(text, path, offset, warpGlyphs: true, paint);
	}

	[Obsolete("Use DrawTextOnPath(string text, SKPath path, float hOffset, float vOffset, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
	public void DrawTextOnPath(string text, SKPath path, float hOffset, float vOffset, SKPaint paint)
	{
		DrawTextOnPath(text, path, new SKPoint(hOffset, vOffset), warpGlyphs: true, paint);
	}

	[Obsolete("Use DrawTextOnPath(string text, SKPath path, SKPoint offset, bool warpGlyphs, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
	public void DrawTextOnPath(string text, SKPath path, SKPoint offset, bool warpGlyphs, SKPaint paint)
	{
		DrawTextOnPath(text, path, offset, warpGlyphs, paint.GetFont(), paint);
	}

	public void DrawTextOnPath(string text, SKPath path, SKPoint offset, SKFont font, SKPaint paint)
	{
		DrawTextOnPath(text, path, offset, warpGlyphs: true, paint.TextAlign, font, paint);
	}

	public void DrawTextOnPath(string text, SKPath path, SKPoint offset, SKTextAlign textAlign, SKFont font, SKPaint paint)
	{
		DrawTextOnPath(text, path, offset, warpGlyphs: true, textAlign, font, paint);
	}

	public void DrawTextOnPath(string text, SKPath path, float hOffset, float vOffset, SKFont font, SKPaint paint)
	{
		DrawTextOnPath(text, path, new SKPoint(hOffset, vOffset), warpGlyphs: true, paint.TextAlign, font, paint);
	}

	public void DrawTextOnPath(string text, SKPath path, float hOffset, float vOffset, SKTextAlign textAlign, SKFont font, SKPaint paint)
	{
		DrawTextOnPath(text, path, new SKPoint(hOffset, vOffset), warpGlyphs: true, textAlign, font, paint);
	}

	public void DrawTextOnPath(string text, SKPath path, SKPoint offset, bool warpGlyphs, SKFont font, SKPaint paint)
	{
		DrawTextOnPath(text, path, offset, warpGlyphs, paint.TextAlign, font, paint);
	}

	public void DrawTextOnPath(string text, SKPath path, SKPoint offset, bool warpGlyphs, SKTextAlign textAlign, SKFont font, SKPaint paint)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		if (warpGlyphs)
		{
			using (SKPath path2 = font.GetTextPathOnPath(text, path, textAlign, offset))
			{
				DrawPath(path2, paint);
				return;
			}
		}
		using SKTextBlob sKTextBlob = SKTextBlob.CreatePathPositioned(text, font, path, textAlign, offset);
		if (sKTextBlob != null)
		{
			DrawText(sKTextBlob, 0f, 0f, paint);
		}
	}

	public void Flush()
	{
		(Context as GRContext)?.Flush();
	}

	public unsafe void DrawAnnotation(SKRect rect, string key, SKData value)
	{
		fixed (byte* encodedText = StringUtilities.GetEncodedText(key, SKTextEncoding.Utf8, addNull: true))
		{
			SkiaApi.sk_canvas_draw_annotation(base.Handle, &rect, encodedText, value?.Handle ?? IntPtr.Zero);
		}
	}

	public unsafe void DrawUrlAnnotation(SKRect rect, SKData value)
	{
		SkiaApi.sk_canvas_draw_url_annotation(Handle, &rect, value?.Handle ?? IntPtr.Zero);
	}

	public SKData DrawUrlAnnotation(SKRect rect, string value)
	{
		SKData sKData = SKData.FromCString(value);
		DrawUrlAnnotation(rect, sKData);
		return sKData;
	}

	public unsafe void DrawNamedDestinationAnnotation(SKPoint point, SKData value)
	{
		SkiaApi.sk_canvas_draw_named_destination_annotation(Handle, &point, value?.Handle ?? IntPtr.Zero);
	}

	public SKData DrawNamedDestinationAnnotation(SKPoint point, string value)
	{
		SKData sKData = SKData.FromCString(value);
		DrawNamedDestinationAnnotation(point, sKData);
		return sKData;
	}

	public unsafe void DrawLinkDestinationAnnotation(SKRect rect, SKData value)
	{
		SkiaApi.sk_canvas_draw_link_destination_annotation(Handle, &rect, value?.Handle ?? IntPtr.Zero);
	}

	public SKData DrawLinkDestinationAnnotation(SKRect rect, string value)
	{
		SKData sKData = SKData.FromCString(value);
		DrawLinkDestinationAnnotation(rect, sKData);
		return sKData;
	}

	public void DrawBitmapNinePatch(SKBitmap bitmap, SKRectI center, SKRect dst, SKPaint paint = null)
	{
		DrawBitmapNinePatch(bitmap, center, dst, SKFilterMode.Nearest, paint);
	}

	public void DrawBitmapNinePatch(SKBitmap bitmap, SKRectI center, SKRect dst, SKFilterMode filterMode, SKPaint paint = null)
	{
		using SKImage image = SKImage.FromBitmap(bitmap);
		DrawImageNinePatch(image, center, dst, filterMode, paint);
	}

	public void DrawImageNinePatch(SKImage image, SKRectI center, SKRect dst, SKPaint paint = null)
	{
		DrawImageNinePatch(image, center, dst, SKFilterMode.Nearest, paint);
	}

	public unsafe void DrawImageNinePatch(SKImage image, SKRectI center, SKRect dst, SKFilterMode filterMode = SKFilterMode.Nearest, SKPaint paint = null)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (!SKRect.Create(image.Width, image.Height).Contains(center))
		{
			throw new ArgumentException("Center rectangle must be contained inside the image bounds.", "center");
		}
		SkiaApi.sk_canvas_draw_image_nine(Handle, image.Handle, &center, &dst, filterMode, paint?.Handle ?? IntPtr.Zero);
	}

	public void DrawBitmapLattice(SKBitmap bitmap, int[] xDivs, int[] yDivs, SKRect dst, SKPaint paint = null)
	{
		DrawBitmapLattice(bitmap, xDivs, yDivs, dst, SKFilterMode.Nearest, paint);
	}

	public void DrawBitmapLattice(SKBitmap bitmap, int[] xDivs, int[] yDivs, SKRect dst, SKFilterMode filterMode, SKPaint paint = null)
	{
		using SKImage image = SKImage.FromBitmap(bitmap);
		DrawImageLattice(image, xDivs, yDivs, dst, filterMode, paint);
	}

	public void DrawImageLattice(SKImage image, int[] xDivs, int[] yDivs, SKRect dst, SKPaint paint = null)
	{
		DrawImageLattice(image, xDivs, yDivs, dst, SKFilterMode.Nearest, paint);
	}

	public void DrawImageLattice(SKImage image, int[] xDivs, int[] yDivs, SKRect dst, SKFilterMode filterMode, SKPaint paint = null)
	{
		SKLattice lattice = new SKLattice
		{
			XDivs = xDivs,
			YDivs = yDivs
		};
		DrawImageLattice(image, lattice, dst, filterMode, paint);
	}

	public void DrawBitmapLattice(SKBitmap bitmap, SKLattice lattice, SKRect dst, SKPaint paint = null)
	{
		DrawBitmapLattice(bitmap, lattice, dst, SKFilterMode.Nearest, paint);
	}

	public void DrawBitmapLattice(SKBitmap bitmap, SKLattice lattice, SKRect dst, SKFilterMode filterMode, SKPaint paint = null)
	{
		using SKImage image = SKImage.FromBitmap(bitmap);
		DrawImageLattice(image, lattice, dst, filterMode, paint);
	}

	public void DrawImageLattice(SKImage image, SKLattice lattice, SKRect dst, SKPaint paint = null)
	{
		DrawImageLattice(image, lattice, dst, SKFilterMode.Nearest, paint);
	}

	public unsafe void DrawImageLattice(SKImage image, SKLattice lattice, SKRect dst, SKFilterMode filterMode, SKPaint paint = null)
	{
		if (image == null)
		{
			throw new ArgumentNullException("image");
		}
		if (lattice.XDivs == null)
		{
			throw new ArgumentNullException("XDivs");
		}
		if (lattice.YDivs == null)
		{
			throw new ArgumentNullException("YDivs");
		}
		fixed (int* xDivs = lattice.XDivs)
		{
			fixed (int* yDivs = lattice.YDivs)
			{
				fixed (SKLatticeRectType* rectTypes = lattice.RectTypes)
				{
					fixed (SKColor* colors = lattice.Colors)
					{
						SKLatticeInternal sKLatticeInternal = new SKLatticeInternal
						{
							fBounds = null,
							fRectTypes = rectTypes,
							fXCount = lattice.XDivs.Length,
							fXDivs = xDivs,
							fYCount = lattice.YDivs.Length,
							fYDivs = yDivs,
							fColors = (uint*)colors
						};
						if (lattice.Bounds.HasValue)
						{
							SKRectI value = lattice.Bounds.Value;
							sKLatticeInternal.fBounds = &value;
						}
						SkiaApi.sk_canvas_draw_image_lattice(Handle, image.Handle, &sKLatticeInternal, &dst, filterMode, paint?.Handle ?? IntPtr.Zero);
					}
				}
			}
		}
	}

	public void ResetMatrix()
	{
		SkiaApi.sk_canvas_reset_matrix(Handle);
	}

	public void SetMatrix(in SKMatrix matrix)
	{
		SetMatrix((SKMatrix44)matrix);
	}

	[Obsolete("Use SetMatrix(in SKMatrix) instead.", true)]
	public void SetMatrix(SKMatrix matrix)
	{
		SetMatrix(in matrix);
	}

	public unsafe void SetMatrix(in SKMatrix44 matrix)
	{
		fixed (SKMatrix44* cmatrix = &matrix)
		{
			SkiaApi.sk_canvas_set_matrix(Handle, cmatrix);
		}
	}

	public void DrawVertices(SKVertexMode vmode, SKPoint[] vertices, SKColor[] colors, SKPaint paint)
	{
		SKVertices vertices2 = SKVertices.CreateCopy(vmode, vertices, colors);
		DrawVertices(vertices2, SKBlendMode.Modulate, paint);
	}

	public void DrawVertices(SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, SKPaint paint)
	{
		SKVertices vertices2 = SKVertices.CreateCopy(vmode, vertices, texs, colors);
		DrawVertices(vertices2, SKBlendMode.Modulate, paint);
	}

	public void DrawVertices(SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, ushort[] indices, SKPaint paint)
	{
		SKVertices vertices2 = SKVertices.CreateCopy(vmode, vertices, texs, colors, indices);
		DrawVertices(vertices2, SKBlendMode.Modulate, paint);
	}

	public void DrawVertices(SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, SKBlendMode mode, ushort[] indices, SKPaint paint)
	{
		SKVertices vertices2 = SKVertices.CreateCopy(vmode, vertices, texs, colors, indices);
		DrawVertices(vertices2, mode, paint);
	}

	public void DrawVertices(SKVertices vertices, SKBlendMode mode, SKPaint paint)
	{
		if (vertices == null)
		{
			throw new ArgumentNullException("vertices");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_vertices(Handle, vertices.Handle, mode, paint.Handle);
	}

	public unsafe void DrawArc(SKRect oval, float startAngle, float sweepAngle, bool useCenter, SKPaint paint)
	{
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_arc(Handle, &oval, startAngle, sweepAngle, useCenter, paint.Handle);
	}

	public void DrawRoundRectDifference(SKRoundRect outer, SKRoundRect inner, SKPaint paint)
	{
		if (outer == null)
		{
			throw new ArgumentNullException("outer");
		}
		if (inner == null)
		{
			throw new ArgumentNullException("inner");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		SkiaApi.sk_canvas_draw_drrect(Handle, outer.Handle, inner.Handle, paint.Handle);
	}

	public unsafe void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKPaint paint = null)
	{
		DrawAtlas(atlas, sprites, transforms, null, SKBlendMode.Dst, paint?.FilterQuality.ToSamplingOptions() ?? SKSamplingOptions.Default, null, paint);
	}

	public unsafe void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKSamplingOptions sampling, SKPaint paint = null)
	{
		DrawAtlas(atlas, sprites, transforms, null, SKBlendMode.Dst, sampling, null, paint);
	}

	public unsafe void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKPaint paint = null)
	{
		DrawAtlas(atlas, sprites, transforms, colors, mode, paint?.FilterQuality.ToSamplingOptions() ?? SKSamplingOptions.Default, null, paint);
	}

	public unsafe void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKSamplingOptions sampling, SKPaint paint = null)
	{
		DrawAtlas(atlas, sprites, transforms, colors, mode, sampling, null, paint);
	}

	public unsafe void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKRect cullRect, SKPaint paint = null)
	{
		DrawAtlas(atlas, sprites, transforms, colors, mode, paint?.FilterQuality.ToSamplingOptions() ?? SKSamplingOptions.Default, &cullRect, paint);
	}

	public unsafe void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKSamplingOptions sampling, SKRect cullRect, SKPaint paint = null)
	{
		DrawAtlas(atlas, sprites, transforms, colors, mode, sampling, &cullRect, paint);
	}

	private unsafe void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKSamplingOptions sampling, SKRect* cullRect, SKPaint paint = null)
	{
		if (atlas == null)
		{
			throw new ArgumentNullException("atlas");
		}
		if (sprites == null)
		{
			throw new ArgumentNullException("sprites");
		}
		if (transforms == null)
		{
			throw new ArgumentNullException("transforms");
		}
		if (transforms.Length != sprites.Length)
		{
			throw new ArgumentException("The number of transforms must match the number of sprites.", "transforms");
		}
		if (colors != null && colors.Length != sprites.Length)
		{
			throw new ArgumentException("The number of colors must match the number of sprites.", "colors");
		}
		fixed (SKRect* tex = sprites)
		{
			fixed (SKRotationScaleMatrix* xform = transforms)
			{
				fixed (SKColor* colors2 = colors)
				{
					SkiaApi.sk_canvas_draw_atlas(Handle, atlas.Handle, xform, tex, (uint*)colors2, transforms.Length, mode, &sampling, cullRect, paint?.Handle ?? IntPtr.Zero);
				}
			}
		}
	}

	public void DrawPatch(SKPoint[] cubics, SKColor[] colors, SKPoint[] texCoords, SKPaint paint)
	{
		DrawPatch(cubics, colors, texCoords, SKBlendMode.Modulate, paint);
	}

	public unsafe void DrawPatch(SKPoint[] cubics, SKColor[] colors, SKPoint[] texCoords, SKBlendMode mode, SKPaint paint)
	{
		if (cubics == null)
		{
			throw new ArgumentNullException("cubics");
		}
		if (cubics.Length != 12)
		{
			throw new ArgumentException($"Cubics must have a length of {12}.", "cubics");
		}
		if (colors != null && colors.Length != 4)
		{
			throw new ArgumentException($"Colors must have a length of {4}.", "colors");
		}
		if (texCoords != null && texCoords.Length != 4)
		{
			throw new ArgumentException($"Texture coordinates must have a length of {4}.", "texCoords");
		}
		if (paint == null)
		{
			throw new ArgumentNullException("paint");
		}
		fixed (SKPoint* cubics2 = cubics)
		{
			fixed (SKColor* colors2 = colors)
			{
				fixed (SKPoint* texCoords2 = texCoords)
				{
					SkiaApi.sk_canvas_draw_patch(Handle, cubics2, (uint*)colors2, texCoords2, mode, paint.Handle);
				}
			}
		}
	}

	internal static SKCanvas GetObject(IntPtr handle, bool owns = true, bool unrefExisting = true)
	{
		return SKObject.GetOrAddObject(handle, owns, unrefExisting, (IntPtr h, bool o) => new SKCanvas(h, o));
	}
}
