using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("a248fd3f-3e6c-4e63-9f03-7f68ecc91db9")]
internal class CustomEffectNative : ComObject, CustomEffect, IUnknown, ICallbackable, IDisposable
{
	public TransformGraph Graph_
	{
		set
		{
			SetGraph_(value);
		}
	}

	public void Initialize(EffectContext effectContext, TransformGraph transformGraph)
	{
		Initialize_(effectContext, transformGraph);
	}

	public void PrepareForRender(ChangeType changeType)
	{
		PrepareForRender_(changeType);
	}

	public void SetGraph(TransformGraph transformGraph)
	{
		SetGraph_(transformGraph);
	}

	public CustomEffectNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator CustomEffectNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new CustomEffectNative(nativePtr);
		}
		return null;
	}

	internal unsafe void Initialize_(EffectContext effectContext, TransformGraph transformGraph)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<EffectContext>(effectContext);
		zero2 = CppObject.ToCallbackPtr<TransformGraph>(transformGraph);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2)).CheckError();
	}

	internal unsafe void PrepareForRender_(ChangeType changeType)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (int)changeType)).CheckError();
	}

	internal unsafe void SetGraph_(TransformGraph transformGraph)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TransformGraph>(transformGraph);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}
}
