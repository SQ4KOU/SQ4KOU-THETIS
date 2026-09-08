using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("5F174B49-0D8B-4CFB-8BCA-F1CCE9D06C67")]
public class TextFormat1 : TextFormat
{
	public VerticalGlyphOrientation VerticalGlyphOrientation
	{
		get
		{
			return GetVerticalGlyphOrientation();
		}
		set
		{
			SetVerticalGlyphOrientation(value);
		}
	}

	public RawBool LastLineWrapping
	{
		get
		{
			return GetLastLineWrapping();
		}
		set
		{
			SetLastLineWrapping(value);
		}
	}

	public OptimizationIcalAlignment OpticalAlignment
	{
		get
		{
			return GetOpticalAlignment();
		}
		set
		{
			SetOpticalAlignment(value);
		}
	}

	public FontFallback FontFallback
	{
		get
		{
			GetFontFallback(out var fontFallback);
			return fontFallback;
		}
		set
		{
			SetFontFallback(value);
		}
	}

	public TextFormat1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TextFormat1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new TextFormat1(nativePtr);
		}
		return null;
	}

	internal unsafe void SetVerticalGlyphOrientation(VerticalGlyphOrientation glyphOrientation)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, (int)glyphOrientation)).CheckError();
	}

	internal unsafe VerticalGlyphOrientation GetVerticalGlyphOrientation()
	{
		return ((delegate* unmanaged[Stdcall]<void*, VerticalGlyphOrientation>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetLastLineWrapping(RawBool isLastLineWrappingEnabled)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(_nativePointer, isLastLineWrappingEnabled)).CheckError();
	}

	internal unsafe RawBool GetLastLineWrapping()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)31 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetOpticalAlignment(OptimizationIcalAlignment opticalAlignment)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)32 * (nint)sizeof(void*))))(_nativePointer, (int)opticalAlignment)).CheckError();
	}

	internal unsafe OptimizationIcalAlignment GetOpticalAlignment()
	{
		return ((delegate* unmanaged[Stdcall]<void*, OptimizationIcalAlignment>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)33 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetFontFallback(FontFallback fontFallback)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFallback>(fontFallback);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)34 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void GetFontFallback(out FontFallback fontFallback)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)35 * (nint)sizeof(void*))))(_nativePointer, &zero);
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
