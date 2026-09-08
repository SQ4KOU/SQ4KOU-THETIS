using System;

namespace SharpDX.Direct2D1;

public abstract class CustomEffectBase : CallbackBase, CustomEffect, IUnknown, ICallbackable, IDisposable
{
	public virtual void Initialize(EffectContext effectContext, TransformGraph transformGraph)
	{
	}

	public virtual void PrepareForRender(ChangeType changeType)
	{
	}

	public virtual void SetGraph(TransformGraph transformGraph)
	{
	}
}
