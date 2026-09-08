using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("DA20D8EF-812A-4C43-9802-62EC4ABD7ADE")]
public class FontList1 : FontList
{
	public FontList1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontList1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontList1(nativePtr);
		}
		return null;
	}

	public unsafe Locality GetFontLocality(int listIndex)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, Locality>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, listIndex);
	}

	public unsafe void GetFont(int listIndex, out Font3 font)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, listIndex, &zero);
		if (zero != IntPtr.Zero)
		{
			font = new Font3(zero);
		}
		else
		{
			font = null;
		}
		result.CheckError();
	}

	public unsafe void GetFontFaceReference(int listIndex, out FontFaceReference fontFaceReference)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, listIndex, &zero);
		if (zero != IntPtr.Zero)
		{
			fontFaceReference = new FontFaceReference(zero);
		}
		else
		{
			fontFaceReference = null;
		}
		result.CheckError();
	}
}
