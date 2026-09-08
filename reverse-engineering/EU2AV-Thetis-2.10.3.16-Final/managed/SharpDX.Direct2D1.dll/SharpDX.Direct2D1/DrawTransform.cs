using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Shadow(typeof(DrawTransformShadow))]
[Guid("36bfdcb6-9739-435d-a30d-a653beff6a6f")]
public interface DrawTransform : Transform, TransformNode, IUnknown, ICallbackable, IDisposable
{
	void SetDrawInformation(DrawInformation drawInfo);
}
