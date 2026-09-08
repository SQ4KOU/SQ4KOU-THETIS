using System;
using System.Runtime.InteropServices;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("54d7898a-a061-40a7-bec7-e465bcba2c4f")]
public interface CommandSink : IUnknown, ICallbackable, IDisposable
{
	AntialiasMode AntialiasMode { set; }

	TextAntialiasMode TextAntialiasMode { set; }

	RenderingParams TextRenderingParams { set; }

	RawMatrix3x2 Transform { set; }

	PrimitiveBlend PrimitiveBlend { set; }

	UnitMode UnitMode { set; }

	void BeginDraw();

	void EndDraw();

	void SetTags(long tag1, long tag2);

	void Clear(RawColor4? color = null);

	void DrawGlyphRun(RawVector2 baselineOrigin, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, Brush foregroundBrush, MeasuringMode measuringMode);

	void DrawLine(RawVector2 point0, RawVector2 point1, Brush brush, float strokeWidth, StrokeStyle strokeStyle);

	void DrawGeometry(Geometry geometry, Brush brush, float strokeWidth, StrokeStyle strokeStyle);

	void DrawRectangle(RawRectangleF rect, Brush brush, float strokeWidth, StrokeStyle strokeStyle);

	void DrawBitmap(Bitmap bitmap, RawRectangleF? destinationRectangle, float opacity, InterpolationMode interpolationMode, RawRectangleF? sourceRectangle, RawMatrix? erspectiveTransformRef);

	void DrawImage(Image image, RawVector2? targetOffset, RawRectangleF? imageRectangle, InterpolationMode interpolationMode, CompositeMode compositeMode);

	void DrawGdiMetafile(GdiMetafile gdiMetafile, RawVector2? targetOffset);

	void FillMesh(Mesh mesh, Brush brush);

	void FillOpacityMask(Bitmap opacityMask, Brush brush, RawRectangleF? destinationRectangle, RawRectangleF? sourceRectangle);

	void FillGeometry(Geometry geometry, Brush brush, Brush opacityBrush);

	void FillRectangle(RawRectangleF rect, Brush brush);

	void PushAxisAlignedClip(RawRectangleF clipRect, AntialiasMode antialiasMode);

	void PushLayer(ref LayerParameters1 layerParameters1, Layer layer);

	void PopAxisAlignedClip();

	void PopLayer();
}
