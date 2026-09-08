using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("FD882D06-8ABA-4FB8-B849-8BE8B73E14DE")]
public class FontFallbackBuilder : ComObject
{
	public FontFallbackBuilder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontFallbackBuilder(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontFallbackBuilder(nativePtr);
		}
		return null;
	}

	public unsafe void AddMapping(UnicodeRange[] ranges, int rangesCount, string targetFamilyNames, int targetFamilyNamesCount, FontCollection fontCollection, string localeName, string baseFamilyName, float scale)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontCollection>(fontCollection);
		Result result;
		fixed (char* ptr = baseFamilyName)
		{
			fixed (char* ptr2 = localeName)
			{
				fixed (char* ptr3 = targetFamilyNames)
				{
					fixed (UnicodeRange* ptr4 = ranges)
					{
						void* ptr5 = ptr4;
						result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, void*, void*, void*, float, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr5, rangesCount, ptr3, targetFamilyNamesCount, (void*)zero, ptr2, ptr, scale);
					}
				}
			}
		}
		result.CheckError();
	}

	public unsafe void AddMappings(FontFallback fontFallback)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFallback>(fontFallback);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void CreateFontFallback(out FontFallback fontFallback)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			fontFallback = new FontFallback(zero);
		}
		else
		{
			fontFallback = null;
		}
		result.CheckError();
	}
}
