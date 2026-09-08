using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("db1800dd-0c34-4cf9-be90-31cc0a5653e1")]
public class SourceTransformNative : TransformNative, SourceTransform, Transform, TransformNode, IUnknown, ICallbackable, IDisposable
{
	public RenderInformation RenderInfo_
	{
		set
		{
			SetRenderInfo_(value);
		}
	}

	public void SetRenderInformation(RenderInformation renderInfo)
	{
		SetRenderInfo_(renderInfo);
	}

	public void Draw(Bitmap1 target, RawRectangle drawRect, RawPoint targetOrigin)
	{
		Draw_(target, drawRect, targetOrigin);
	}

	public SourceTransformNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SourceTransformNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SourceTransformNative(nativePtr);
		}
		return null;
	}

	internal unsafe void SetRenderInfo_(RenderInformation renderInfo)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<RenderInformation>(renderInfo);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void Draw_(Bitmap1 target, RawRectangle drawRect, RawPoint targetOrigin)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Bitmap1>(target);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, RawPoint, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &drawRect, targetOrigin)).CheckError();
	}
}
