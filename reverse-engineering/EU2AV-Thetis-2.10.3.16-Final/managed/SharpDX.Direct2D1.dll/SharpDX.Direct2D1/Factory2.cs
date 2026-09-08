using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.Direct2D1;

[Guid("94f81a73-9212-4376-9c58-b16a3a0d3992")]
public class Factory2 : Factory1
{
	public Factory2()
	{
	}

	public Factory2(FactoryType factoryType)
		: base(factoryType)
	{
	}

	public Factory2(FactoryType factoryType, DebugLevel debugLevel)
		: base(factoryType, debugLevel)
	{
	}

	public Factory2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory2(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateDevice(SharpDX.DXGI.Device dxgiDevice, Device1 d2dDevice1)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SharpDX.DXGI.Device>(dxgiDevice);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		d2dDevice1.NativePointer = zero2;
		result.CheckError();
	}
}
