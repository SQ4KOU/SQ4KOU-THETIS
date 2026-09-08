using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct2D1;

[Guid("a44472e1-8dfb-4e60-8492-6e2861c9ca8b")]
public class Device2 : Device1
{
	public SharpDX.DXGI.Device DxgiDevice
	{
		get
		{
			GetDxgiDevice(out var dxgiDevice);
			return dxgiDevice;
		}
	}

	public Device2(Factory3 factory, SharpDX.DXGI.Device device)
		: base(IntPtr.Zero)
	{
		factory.CreateDevice(device, this);
	}

	public Device2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device2(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateDeviceContext(DeviceContextOptions options, DeviceContext2 deviceContext2)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, (int)options, &zero);
		deviceContext2.NativePointer = zero;
		result.CheckError();
	}

	public unsafe void FlushDeviceContexts(Bitmap bitmap)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Bitmap>(bitmap);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	internal unsafe void GetDxgiDevice(out SharpDX.DXGI.Device dxgiDevice)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			dxgiDevice = new SharpDX.DXGI.Device(zero);
		}
		else
		{
			dxgiDevice = null;
		}
		result.CheckError();
	}
}
