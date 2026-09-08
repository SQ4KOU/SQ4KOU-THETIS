using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("eaf3a2da-ecf4-4d24-b644-b34f6842024b")]
[Shadow(typeof(PixelSnappingShadow))]
public interface PixelSnapping : IUnknown, ICallbackable, IDisposable
{
	bool IsPixelSnappingDisabled(object clientDrawingContext);

	RawMatrix3x2 GetCurrentTransform(object clientDrawingContext);

	float GetPixelsPerDip(object clientDrawingContext);
}
