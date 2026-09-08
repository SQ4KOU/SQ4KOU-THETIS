using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct2D1;

[Guid("d55ba0a4-6405-4694-aef5-08ee1a4358b4")]
public class Device5 : Device4
{
	public Device5(Factory6 factory, SharpDX.DXGI.Device device)
		: base(IntPtr.Zero)
	{
		factory.CreateDevice(device, this);
	}

	public Device5(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device5(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device5(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateDeviceContext(DeviceContextOptions options, DeviceContext5 deviceContext5)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, (int)options, &zero);
		deviceContext5.NativePointer = zero;
		result.CheckError();
	}
}
