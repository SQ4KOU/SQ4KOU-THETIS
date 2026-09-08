using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("a84cee02-3eea-4eee-a827-87c1a02a0fcc")]
public class FontCollection : ComObject
{
	public int FontFamilyCount => GetFontFamilyCount();

	public FontCollection(Factory factory, FontCollectionLoader collectionLoader, DataPointer collectionKey)
	{
		factory.CreateCustomFontCollection(collectionLoader, collectionKey.Pointer, collectionKey.Size, this);
	}

	public FontCollection(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontCollection(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontCollection(nativePtr);
		}
		return null;
	}

	internal unsafe int GetFontFamilyCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe FontFamily GetFontFamily(int index)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, index, &zero);
		FontFamily result2 = ((!(zero != IntPtr.Zero)) ? null : new FontFamily(zero));
		result.CheckError();
		return result2;
	}

	public unsafe RawBool FindFamilyName(string familyName, out int index)
	{
		Result result;
		RawBool result2 = default(RawBool);
		fixed (int* ptr = &index)
		{
			void* ptr2 = ptr;
			fixed (char* ptr3 = familyName)
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr3, ptr2, &result2);
			}
		}
		result.CheckError();
		return result2;
	}

	public unsafe Font GetFontFromFontFace(FontFace fontFace)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFace>(fontFace);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		Font result2 = ((!(zero2 != IntPtr.Zero)) ? null : new Font(zero2));
		result.CheckError();
		return result2;
	}
}
