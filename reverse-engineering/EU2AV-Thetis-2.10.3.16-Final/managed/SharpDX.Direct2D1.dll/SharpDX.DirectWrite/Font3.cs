using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("29748ED6-8C9C-4A6A-BE0B-D912E8538944")]
public class Font3 : Font2
{
	public FontFaceReference FontFaceReference
	{
		get
		{
			GetFontFaceReference(out var fontFaceReference);
			return fontFaceReference;
		}
	}

	public Locality Locality => GetLocality();

	public Font3(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Font3(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Font3(nativePtr);
		}
		return null;
	}

	public unsafe void CreateFontFace(out FontFace3 fontFace)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			fontFace = new FontFace3(zero);
		}
		else
		{
			fontFace = null;
		}
		result.CheckError();
	}

	public unsafe RawBool Equals(Font font)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Font>(font);
		return ((delegate* unmanaged[Stdcall]<void*, void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	internal unsafe void GetFontFaceReference(out FontFaceReference fontFaceReference)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, &zero);
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

	public new unsafe RawBool HasCharacter(int unicodeValue)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, unicodeValue);
	}

	internal unsafe Locality GetLocality()
	{
		return ((delegate* unmanaged[Stdcall]<void*, Locality>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer);
	}
}
