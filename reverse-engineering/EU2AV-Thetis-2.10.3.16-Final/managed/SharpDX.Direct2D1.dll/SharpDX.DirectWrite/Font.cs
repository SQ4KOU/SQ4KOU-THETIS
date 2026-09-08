using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("acd16696-8c14-4f5d-877e-fe3fc1d32737")]
public class Font : ComObject
{
	public FontFamily FontFamily
	{
		get
		{
			GetFontFamily(out var fontFamily);
			return fontFamily;
		}
	}

	public FontWeight Weight => GetWeight();

	public FontStretch Stretch => GetStretch();

	public FontStyle Style => GetStyle();

	public RawBool IsSymbolFont => IsSymbolFont_();

	public LocalizedStrings FaceNames
	{
		get
		{
			GetFaceNames(out var names);
			return names;
		}
	}

	public FontSimulations Simulations => GetSimulations();

	public FontMetrics Metrics
	{
		get
		{
			GetMetrics(out var fontMetrics);
			return fontMetrics;
		}
	}

	public Font(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Font(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Font(nativePtr);
		}
		return null;
	}

	internal unsafe void GetFontFamily(out FontFamily fontFamily)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			fontFamily = new FontFamily(zero);
		}
		else
		{
			fontFamily = null;
		}
		result.CheckError();
	}

	internal unsafe FontWeight GetWeight()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FontWeight>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe FontStretch GetStretch()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FontStretch>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe FontStyle GetStyle()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FontStyle>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe RawBool IsSymbolFont_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetFaceNames(out LocalizedStrings names)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			names = new LocalizedStrings(zero);
		}
		else
		{
			names = null;
		}
		result.CheckError();
	}

	public unsafe RawBool GetInformationalStrings(InformationalStringId informationalStringID, out LocalizedStrings informationalStrings)
	{
		IntPtr zero = IntPtr.Zero;
		RawBool result2 = default(RawBool);
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (int)informationalStringID, &zero, &result2);
		if (zero != IntPtr.Zero)
		{
			informationalStrings = new LocalizedStrings(zero);
		}
		else
		{
			informationalStrings = null;
		}
		result.CheckError();
		return result2;
	}

	internal unsafe FontSimulations GetSimulations()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FontSimulations>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetMetrics(out FontMetrics fontMetrics)
	{
		fontMetrics = default(FontMetrics);
		fixed (FontMetrics* ptr = &fontMetrics)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	public unsafe RawBool HasCharacter(int unicodeValue)
	{
		RawBool result = default(RawBool);
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, unicodeValue, &result)).CheckError();
		return result;
	}

	internal unsafe void CreateFontFace(FontFace fontFace)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, &zero);
		fontFace.NativePointer = zero;
		result.CheckError();
	}
}
