using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("29748ed6-8c9c-4a6a-be0b-d912e8538944")]
public class Font2 : Font1
{
	public RawBool IsColorFont => IsColorFont_();

	public Font2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Font2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Font2(nativePtr);
		}
		return null;
	}

	internal unsafe RawBool IsColorFont_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer);
	}
}
