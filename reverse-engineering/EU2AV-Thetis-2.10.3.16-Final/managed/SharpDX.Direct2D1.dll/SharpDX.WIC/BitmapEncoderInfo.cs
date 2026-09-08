using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("94C9B4EE-A09F-4f92-8A1E-4A9BCE7E76FB")]
public class BitmapEncoderInfo : BitmapCodecInfo
{
	public BitmapEncoderInfo(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapEncoderInfo(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BitmapEncoderInfo(nativePtr);
		}
		return null;
	}

	public unsafe void CreateInstance(out BitmapEncoder bitmapEncoderOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			bitmapEncoderOut = new BitmapEncoder(zero);
		}
		else
		{
			bitmapEncoderOut = null;
		}
		result.CheckError();
	}
}
