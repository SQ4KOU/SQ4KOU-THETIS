using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("4556BE70-3ABD-4F70-90BE-421780A6F515")]
public class GdiInterop1 : GdiInterop
{
	public GdiInterop1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GdiInterop1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new GdiInterop1(nativePtr);
		}
		return null;
	}

	internal unsafe Font CreateFontFromLOGFONT(IntPtr logFont, FontCollection fontCollection)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontCollection>(fontCollection);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)logFont, (void*)zero, &zero2);
		Font result2 = ((!(zero2 != IntPtr.Zero)) ? null : new Font(zero2));
		result.CheckError();
		return result2;
	}

	public unsafe FontSignature GetFontSignature(FontFace fontFace)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFace>(fontFace);
		FontSignature result = default(FontSignature);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &result)).CheckError();
		return result;
	}

	public unsafe FontSignature GetFontSignature(Font font)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Font>(font);
		FontSignature result = default(FontSignature);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &result)).CheckError();
		return result;
	}

	internal unsafe void GetMatchingFontsByLOGFONT(IntPtr logFont, FontSet fontSet, out FontSet filteredSet)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontSet>(fontSet);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, (void*)logFont, (void*)zero, &zero2);
		if (zero2 != IntPtr.Zero)
		{
			filteredSet = new FontSet(zero2);
		}
		else
		{
			filteredSet = null;
		}
		result.CheckError();
	}
}
