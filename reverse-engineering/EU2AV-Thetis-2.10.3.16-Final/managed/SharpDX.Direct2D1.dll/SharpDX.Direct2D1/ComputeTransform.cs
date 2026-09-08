using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("0d85573c-01e3-4f7d-bfd9-0d60608bf3c3")]
[Shadow(typeof(ComputeTransformShadow))]
public interface ComputeTransform : Transform, TransformNode, IUnknown, ICallbackable, IDisposable
{
	void SetComputeInformation(ComputeInformation computeInfo);

	RawInt3 CalculateThreadgroups(RawRectangle outputRect);
}
