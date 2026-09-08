using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct2D1;

[Guid("d21768e1-23a4-4823-a14b-7c3eba85d658")]
public class Device1 : Device
{
	public RenderingPriority RenderingPriority
	{
		get
		{
			return GetRenderingPriority();
		}
		set
		{
			SetRenderingPriority(value);
		}
	}

	public Device1(Factory2 factory, SharpDX.DXGI.Device device)
		: base(IntPtr.Zero)
	{
		factory.CreateDevice(device, this);
	}

	public Device1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device1(nativePtr);
		}
		return null;
	}

	internal unsafe RenderingPriority GetRenderingPriority()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RenderingPriority>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetRenderingPriority(RenderingPriority renderingPriority)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, (int)renderingPriority);
	}

	internal unsafe void CreateDeviceContext(DeviceContextOptions options, DeviceContext1 deviceContext1)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, (int)options, &zero);
		deviceContext1.NativePointer = zero;
		result.CheckError();
	}
}
