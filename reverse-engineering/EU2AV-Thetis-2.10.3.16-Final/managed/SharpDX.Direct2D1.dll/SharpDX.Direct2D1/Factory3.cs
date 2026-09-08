using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct2D1;

[Guid("0869759f-4f00-413f-b03e-2bda45404d0f")]
public class Factory3 : Factory2
{
	public Factory3(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory3(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory3(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateDevice(SharpDX.DXGI.Device dxgiDevice, Device2 d2dDevice2)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SharpDX.DXGI.Device>(dxgiDevice);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		d2dDevice2.NativePointer = zero2;
		result.CheckError();
	}
}
