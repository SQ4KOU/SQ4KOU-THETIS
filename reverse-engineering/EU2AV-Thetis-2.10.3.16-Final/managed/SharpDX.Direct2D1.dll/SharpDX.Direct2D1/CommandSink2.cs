using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("3bab440e-417e-47df-a2e2-bc0be6a00916")]
public interface CommandSink2 : CommandSink1, CommandSink, IUnknown, ICallbackable, IDisposable
{
	void DrawInk(Ink ink, Brush brush, InkStyle inkStyle);

	void DrawGradientMesh(GradientMesh gradientMesh);

	void DrawGdiMetafile(GdiMetafile gdiMetafile, RawRectangleF? destinationRectangle, RawRectangleF? sourceRectangle);
}
