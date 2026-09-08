using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct2D1;

[Guid("bd4ec2d2-0662-4bee-ba8e-6f29f032e096")]
public class Factory4 : Factory3
{
	public Factory4(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory4(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory4(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateDevice(SharpDX.DXGI.Device dxgiDevice, Device3 d2dDevice3)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SharpDX.DXGI.Device>(dxgiDevice);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		d2dDevice3.NativePointer = zero2;
		result.CheckError();
	}
}
