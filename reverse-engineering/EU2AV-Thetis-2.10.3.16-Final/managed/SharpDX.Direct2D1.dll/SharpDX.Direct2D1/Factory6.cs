using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct2D1;

[Guid("f9976f46-f642-44c1-97ca-da32ea2a2635")]
public class Factory6 : Factory5
{
	public Factory6(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory6(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory6(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateDevice(SharpDX.DXGI.Device dxgiDevice, Device5 d2dDevice5)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SharpDX.DXGI.Device>(dxgiDevice);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)31 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		d2dDevice5.NativePointer = zero2;
		result.CheckError();
	}
}
