using System;

namespace SkiaSharp;

public struct SKCanvasSaveLayerRec
{
	public SKRect? Bounds { get; set; }

	public SKPaint? Paint { get; set; }

	public SKImageFilter? Backdrop { get; set; }

	public SKCanvasSaveLayerRecFlags Flags { get; set; }

	internal unsafe readonly SKCanvasSaveLayerRecNative ToNative()
	{
		SKCanvasSaveLayerRecNative result = default(SKCanvasSaveLayerRecNative);
		SKRect? bounds = Bounds;
		nint fBounds;
		if (bounds.HasValue)
		{
			SKRect valueOrDefault = bounds.GetValueOrDefault();
			fBounds = (nint)(&valueOrDefault);
		}
		else
		{
			fBounds = 0;
		}
		result.fBounds = (SKRect*)fBounds;
		result.fPaint = Paint?.Handle ?? IntPtr.Zero;
		result.fBackdrop = Backdrop?.Handle ?? IntPtr.Zero;
		result.fFlags = Flags;
		return result;
	}
}
