using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("b2efe1e7-729f-4102-949f-505fa21bf666")]
public class TransformNodeNative : ComObject, TransformNode, IUnknown, ICallbackable, IDisposable
{
	public int InputCount => GetInputCount_();

	public int InputCount_ => GetInputCount_();

	public TransformNodeNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TransformNodeNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new TransformNodeNative(nativePtr);
		}
		return null;
	}

	internal unsafe int GetInputCount_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}
}
