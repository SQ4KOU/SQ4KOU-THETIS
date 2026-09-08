using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Shadow(typeof(GeometrySinkShadow))]
[Guid("2cd9069f-12e2-11dc-9fed-001143a055f9")]
public interface GeometrySink : SimplifiedGeometrySink, IUnknown, ICallbackable, IDisposable
{
	void AddLine(RawVector2 point);

	void AddBezier(BezierSegment bezier);

	void AddQuadraticBezier(QuadraticBezierSegment bezier);

	void AddQuadraticBeziers(QuadraticBezierSegment[] beziers);

	void AddArc(ArcSegment arc);
}
