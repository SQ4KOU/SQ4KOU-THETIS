using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("36bfdcb6-9739-435d-a30d-a653beff6a6f")]
public class DrawTransformNative : TransformNative, DrawTransform, Transform, TransformNode, IUnknown, ICallbackable, IDisposable
{
	public DrawInformation DrawInfo_
	{
		set
		{
			SetDrawInfo_(value);
		}
	}

	public void SetDrawInformation(DrawInformation drawInfo)
	{
		SetDrawInfo_(drawInfo);
	}

	public DrawTransformNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DrawTransformNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DrawTransformNative(nativePtr);
		}
		return null;
	}

	internal unsafe void SetDrawInfo_(DrawInformation drawInfo)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<DrawInformation>(drawInfo);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}
}
