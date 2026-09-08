using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Shadow(typeof(TransformShadow))]
[Guid("ef1a287d-342a-4f76-8fdb-da0d6ea9f92b")]
public interface Transform : TransformNode, IUnknown, ICallbackable, IDisposable
{
	void MapOutputRectangleToInputRectangles(RawRectangle outputRect, RawRectangle[] inputRects);

	RawRectangle MapInputRectanglesToOutputRectangle(RawRectangle[] inputRects, RawRectangle[] inputOpaqueSubRects, out RawRectangle outputOpaqueSubRect);

	RawRectangle MapInvalidRect(int inputIndex, RawRectangle invalidInputRect);
}
