using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("7C5F86DA-C7A1-4F05-B8E1-55A179FE5A35")]
public class ColorGlyphRunEnumerator1 : ColorGlyphRunEnumerator
{
	public new unsafe ColorGlyphRun1 CurrentRun
	{
		get
		{
			GetCurrentRun(out var colorGlyphRun);
			ColorGlyphRun1 result = default(ColorGlyphRun1);
			result.__MarshalFrom(ref *(ColorGlyphRun1.__Native*)(void*)colorGlyphRun);
			return result;
		}
	}

	public ColorGlyphRunEnumerator1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ColorGlyphRunEnumerator1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ColorGlyphRunEnumerator1(nativePtr);
		}
		return null;
	}

	internal new unsafe void GetCurrentRun(out IntPtr colorGlyphRun)
	{
		Result result;
		fixed (IntPtr* ptr = &colorGlyphRun)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
