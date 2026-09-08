using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("94413cf4-a6fc-4248-8b50-6674348fcad3")]
public class RenderingParams1 : RenderingParams
{
	public float GrayscaleEnhancedContrast => GetGrayscaleEnhancedContrast();

	public RenderingParams1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RenderingParams1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RenderingParams1(nativePtr);
		}
		return null;
	}

	internal unsafe float GetGrayscaleEnhancedContrast()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer);
	}
}
