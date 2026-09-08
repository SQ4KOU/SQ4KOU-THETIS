using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("EFA008F9-F7A1-48BF-B05C-F224713CC0FF")]
public class FontFallback : ComObject
{
	public FontFallback(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontFallback(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontFallback(nativePtr);
		}
		return null;
	}

	public unsafe void MapCharacters(TextAnalysisSource analysisSource, int textPosition, int textLength, FontCollection baseFontCollection, string baseFamilyName, FontWeight baseWeight, FontStyle baseStyle, FontStretch baseStretch, out int mappedLength, out Font mappedFont, out float scale)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TextAnalysisSource>(analysisSource);
		zero2 = CppObject.ToCallbackPtr<FontCollection>(baseFontCollection);
		Result result;
		fixed (float* ptr = &scale)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &mappedLength)
			{
				void* ptr4 = ptr3;
				fixed (char* ptr5 = baseFamilyName)
				{
					result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, void*, int, int, int, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, textPosition, textLength, (void*)zero2, ptr5, (int)baseWeight, (int)baseStyle, (int)baseStretch, ptr4, &zero3, ptr2);
				}
			}
		}
		if (zero3 != IntPtr.Zero)
		{
			mappedFont = new Font(zero3);
		}
		else
		{
			mappedFont = null;
		}
		result.CheckError();
	}
}
