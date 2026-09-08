using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("F9D711C3-9777-40AE-87E8-3E5AF9BF0948")]
public class RenderingParams2 : RenderingParams1
{
	public GridFitMode GridFitMode => GetGridFitMode();

	public RenderingParams2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RenderingParams2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RenderingParams2(nativePtr);
		}
		return null;
	}

	internal unsafe GridFitMode GetGridFitMode()
	{
		return ((delegate* unmanaged[Stdcall]<void*, GridFitMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer);
	}
}
