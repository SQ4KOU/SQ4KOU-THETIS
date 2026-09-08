using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Shadow(typeof(TransformNodeShadow))]
[Guid("b2efe1e7-729f-4102-949f-505fa21bf666")]
public interface TransformNode : IUnknown, ICallbackable, IDisposable
{
	int InputCount { get; }
}
