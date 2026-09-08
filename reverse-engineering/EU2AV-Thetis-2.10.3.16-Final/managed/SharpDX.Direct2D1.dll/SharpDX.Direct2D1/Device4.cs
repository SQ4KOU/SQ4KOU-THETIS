using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct2D1;

[Guid("d7bdb159-5683-4a46-bc9c-72dc720b858b")]
public class Device4 : Device3
{
	public long MaximumColorGlyphCacheMemory
	{
		get
		{
			return GetMaximumColorGlyphCacheMemory();
		}
		set
		{
			SetMaximumColorGlyphCacheMemory(value);
		}
	}

	public Device4(Factory5 factory, SharpDX.DXGI.Device device)
		: base(IntPtr.Zero)
	{
		factory.CreateDevice(device, this);
	}

	public Device4(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device4(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device4(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateDeviceContext(DeviceContextOptions options, DeviceContext4 deviceContext4)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, (int)options, &zero);
		deviceContext4.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void SetMaximumColorGlyphCacheMemory(long maximumInBytes)
	{
		((delegate* unmanaged[Stdcall]<void*, long, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, maximumInBytes);
	}

	internal unsafe long GetMaximumColorGlyphCacheMemory()
	{
		return ((delegate* unmanaged[Stdcall]<void*, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer);
	}
}
