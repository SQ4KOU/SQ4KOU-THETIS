using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct2D1;

[Guid("852f2087-802c-4037-ab60-ff2e7ee6fc01")]
public class Device3 : Device2
{
	public Device3(Factory4 factory, SharpDX.DXGI.Device device)
		: base(IntPtr.Zero)
	{
		factory.CreateDevice(device, this);
	}

	public Device3(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device3(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device3(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateDeviceContext(DeviceContextOptions options, DeviceContext3 deviceContext3)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, (int)options, &zero);
		deviceContext3.NativePointer = zero;
		result.CheckError();
	}
}
