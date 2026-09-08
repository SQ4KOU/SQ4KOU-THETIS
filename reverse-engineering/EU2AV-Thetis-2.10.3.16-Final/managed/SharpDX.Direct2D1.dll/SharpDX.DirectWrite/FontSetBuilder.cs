using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("2F642AFE-9C68-4F40-B8BE-457401AFCB3D")]
public class FontSetBuilder : ComObject
{
	public FontSetBuilder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontSetBuilder(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontSetBuilder(nativePtr);
		}
		return null;
	}

	public unsafe void AddFontFaceReference(FontFaceReference fontFaceReference, FontProperty[] ropertiesRef, int propertyCount)
	{
		IntPtr zero = IntPtr.Zero;
		FontProperty.__Native[] array = new FontProperty.__Native[ropertiesRef.Length];
		zero = CppObject.ToCallbackPtr<FontFaceReference>(fontFaceReference);
		for (int i = 0; i < ropertiesRef.Length; i++)
		{
			ropertiesRef[i].__MarshalTo(ref array[i]);
		}
		Result result;
		fixed (FontProperty.__Native* ptr = array)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2, propertyCount);
		}
		for (int j = 0; j < ropertiesRef.Length; j++)
		{
			ropertiesRef[j].__MarshalFree(ref array[j]);
		}
		result.CheckError();
	}

	public unsafe void AddFontFaceReference(FontFaceReference fontFaceReference)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFaceReference>(fontFaceReference);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void AddFontSet(FontSet fontSet)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontSet>(fontSet);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void CreateFontSet(out FontSet fontSet)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, &zero);
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
}
