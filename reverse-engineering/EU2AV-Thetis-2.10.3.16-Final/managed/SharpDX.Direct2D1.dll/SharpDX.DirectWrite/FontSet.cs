using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("53585141-D9F8-4095-8321-D73CF6BD116B")]
public class FontSet : ComObject
{
	public int FontCount => GetFontCount();

	public FontSet(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontSet(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontSet(nativePtr);
		}
		return null;
	}

	internal unsafe int GetFontCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void GetFontFaceReference(int listIndex, out FontFaceReference fontFaceReference)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, listIndex, &zero);
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

	public unsafe void FindFontFaceReference(FontFaceReference fontFaceReference, out int listIndex, out RawBool exists)
	{
		IntPtr zero = IntPtr.Zero;
		exists = default(RawBool);
		zero = CppObject.ToCallbackPtr<FontFaceReference>(fontFaceReference);
		Result result;
		fixed (RawBool* ptr = &exists)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &listIndex)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void FindFontFace(FontFace fontFace, out int listIndex, out RawBool exists)
	{
		IntPtr zero = IntPtr.Zero;
		exists = default(RawBool);
		zero = CppObject.ToCallbackPtr<FontFace>(fontFace);
		Result result;
		fixed (RawBool* ptr = &exists)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &listIndex)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void GetPropertyValues(FontPropertyId propertyID, out StringList values)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (int)propertyID, &zero);
		if (zero != IntPtr.Zero)
		{
			values = new StringList(zero);
		}
		else
		{
			values = null;
		}
		result.CheckError();
	}

	public unsafe void GetPropertyValues(FontPropertyId propertyID, string referredLocaleNamesRef, out StringList values)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (char* ptr = referredLocaleNamesRef)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (int)propertyID, ptr, &zero);
		}
		if (zero != IntPtr.Zero)
		{
			values = new StringList(zero);
		}
		else
		{
			values = null;
		}
		result.CheckError();
	}

	public unsafe void GetPropertyValues(int listIndex, FontPropertyId propertyId, out RawBool exists, out LocalizedStrings values)
	{
		exists = default(RawBool);
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (RawBool* ptr = &exists)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, listIndex, (int)propertyId, ptr2, &zero);
		}
		if (zero != IntPtr.Zero)
		{
			values = new LocalizedStrings(zero);
		}
		else
		{
			values = null;
		}
		result.CheckError();
	}

	public unsafe void GetPropertyOccurrenceCount(ref FontProperty ropertyRef, out int ropertyOccurrenceCountRef)
	{
		FontProperty.__Native @ref = default(FontProperty.__Native);
		ropertyRef.__MarshalTo(ref @ref);
		Result result;
		fixed (int* ptr = &ropertyOccurrenceCountRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, &@ref, ptr2);
		}
		ropertyRef.__MarshalFree(ref @ref);
		result.CheckError();
	}

	public unsafe void GetMatchingFonts(string familyName, FontWeight fontWeight, FontStretch fontStretch, FontStyle fontStyle, out FontSet filteredSet)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (char* ptr = familyName)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr, (int)fontWeight, (int)fontStretch, (int)fontStyle, &zero);
		}
		if (zero != IntPtr.Zero)
		{
			filteredSet = new FontSet(zero);
		}
		else
		{
			filteredSet = null;
		}
		result.CheckError();
	}

	public unsafe void GetMatchingFonts(FontProperty[] ropertiesRef, int propertyCount, out FontSet filteredSet)
	{
		FontProperty.__Native[] array = new FontProperty.__Native[ropertiesRef.Length];
		IntPtr zero = IntPtr.Zero;
		for (int i = 0; i < ropertiesRef.Length; i++)
		{
			ropertiesRef[i].__MarshalTo(ref array[i]);
		}
		Result result;
		fixed (FontProperty.__Native* ptr = array)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, ptr2, propertyCount, &zero);
		}
		if (zero != IntPtr.Zero)
		{
			filteredSet = new FontSet(zero);
		}
		else
		{
			filteredSet = null;
		}
		for (int j = 0; j < ropertiesRef.Length; j++)
		{
			ropertiesRef[j].__MarshalFree(ref array[j]);
		}
		result.CheckError();
	}
}
