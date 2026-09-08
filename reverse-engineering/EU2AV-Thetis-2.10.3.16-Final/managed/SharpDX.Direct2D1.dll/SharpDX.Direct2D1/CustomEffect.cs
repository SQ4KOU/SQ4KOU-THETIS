using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Shadow(typeof(CustomEffectShadow))]
[Guid("a248fd3f-3e6c-4e63-9f03-7f68ecc91db9")]
public interface CustomEffect : IUnknown, ICallbackable, IDisposable
{
	void Initialize(EffectContext effectContext, TransformGraph transformGraph);

	void PrepareForRender(ChangeType changeType);

	void SetGraph(TransformGraph transformGraph);
}
