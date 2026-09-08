using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("53585141-D9F8-4095-8321-D73CF6BD116C")]
public class FontCollection1 : FontCollection
{
	public FontSet FontSet
	{
		get
		{
			GetFontSet(out var fontSet);
			return fontSet;
		}
	}

	public FontCollection1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontCollection1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontCollection1(nativePtr);
		}
		return null;
	}

	internal unsafe void GetFontSet(out FontSet fontSet)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			fontSet = new FontSet(zero);
		}
		else
		{
			fontSet = null;
		}
		result.CheckError();
	}

	public unsafe void GetFontFamily(int index, out FontFamily1 fontFamily)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, index, &zero);
		if (zero != IntPtr.Zero)
		{
			fontFamily = new FontFamily1(zero);
		}
		else
		{
			fontFamily = null;
		}
		result.CheckError();
	}
}
