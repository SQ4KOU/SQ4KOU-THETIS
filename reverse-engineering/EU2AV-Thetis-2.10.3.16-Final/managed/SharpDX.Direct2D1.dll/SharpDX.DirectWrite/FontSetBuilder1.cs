using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("3FF7715F-3CDC-4DC6-9B72-EC5621DCCAFD")]
public class FontSetBuilder1 : FontSetBuilder
{
	public FontSetBuilder1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontSetBuilder1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontSetBuilder1(nativePtr);
		}
		return null;
	}

	public unsafe void AddFontFile(FontFile fontFile)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFile>(fontFile);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}
}
