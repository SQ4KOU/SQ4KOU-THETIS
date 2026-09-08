using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Shadow(typeof(SourceTransformShadow))]
[Guid("db1800dd-0c34-4cf9-be90-31cc0a5653e1")]
public interface SourceTransform : Transform, TransformNode, IUnknown, ICallbackable, IDisposable
{
	void SetRenderInformation(RenderInformation renderInfo);

	void Draw(Bitmap1 target, RawRectangle drawRect, RawPoint targetOrigin);
}
