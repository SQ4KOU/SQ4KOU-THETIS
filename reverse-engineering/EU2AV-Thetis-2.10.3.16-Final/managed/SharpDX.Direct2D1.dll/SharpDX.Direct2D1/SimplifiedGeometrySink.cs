using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Shadow(typeof(SimplifiedGeometrySinkShadow))]
[Guid("2cd9069e-12e2-11dc-9fed-001143a055f9")]
public interface SimplifiedGeometrySink : IUnknown, ICallbackable, IDisposable
{
	void SetFillMode(FillMode fillMode);

	void SetSegmentFlags(PathSegment vertexFlags);

	void BeginFigure(RawVector2 startPoint, FigureBegin figureBegin);

	void AddLines(RawVector2[] ointsRef);

	void AddBeziers(BezierSegment[] beziers);

	void EndFigure(FigureEnd figureEnd);

	void Close();
}
