using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct2D1;

[Guid("c4349994-838e-4b0f-8cab-44997d9eeacc")]
public class Factory5 : Factory4
{
	public Factory5(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory5(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory5(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateDevice(SharpDX.DXGI.Device dxgiDevice, Device4 d2dDevice4)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SharpDX.DXGI.Device>(dxgiDevice);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		d2dDevice4.NativePointer = zero2;
		result.CheckError();
	}
}
