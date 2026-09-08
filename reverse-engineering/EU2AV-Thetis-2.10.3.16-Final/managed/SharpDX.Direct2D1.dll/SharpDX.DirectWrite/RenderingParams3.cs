using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("B7924BAA-391B-412A-8C5C-E44CC2D867DC")]
public class RenderingParams3 : RenderingParams2
{
	public RenderingMode1 RenderingMode1 => GetRenderingMode1();

	public RenderingParams3(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RenderingParams3(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RenderingParams3(nativePtr);
		}
		return null;
	}

	internal unsafe RenderingMode1 GetRenderingMode1()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RenderingMode1>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer);
	}
}
