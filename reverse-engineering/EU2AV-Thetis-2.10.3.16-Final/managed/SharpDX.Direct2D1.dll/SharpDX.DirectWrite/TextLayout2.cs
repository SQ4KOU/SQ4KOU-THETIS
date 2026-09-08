using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("1093C18F-8D5E-43F0-B064-0917311B525E")]
public class TextLayout2 : TextLayout1
{
	public new TextMetrics1 Metrics
	{
		get
		{
			GetMetrics(out var textMetrics);
			return textMetrics;
		}
	}

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

	public TextLayout2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TextLayout2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new TextLayout2(nativePtr);
		}
		return null;
	}

	internal unsafe void GetMetrics(out TextMetrics1 textMetrics)
	{
		textMetrics = default(TextMetrics1);
		Result result;
		fixed (TextMetrics1* ptr = &textMetrics)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)71 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void SetVerticalGlyphOrientation(VerticalGlyphOrientation glyphOrientation)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)72 * (nint)sizeof(void*))))(_nativePointer, (int)glyphOrientation)).CheckError();
	}

	internal unsafe VerticalGlyphOrientation GetVerticalGlyphOrientation()
	{
		return ((delegate* unmanaged[Stdcall]<void*, VerticalGlyphOrientation>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)73 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetLastLineWrapping(RawBool isLastLineWrappingEnabled)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)74 * (nint)sizeof(void*))))(_nativePointer, isLastLineWrappingEnabled)).CheckError();
	}

	internal unsafe RawBool GetLastLineWrapping()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)75 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetOpticalAlignment(OptimizationIcalAlignment opticalAlignment)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)76 * (nint)sizeof(void*))))(_nativePointer, (int)opticalAlignment)).CheckError();
	}

	internal unsafe OptimizationIcalAlignment GetOpticalAlignment()
	{
		return ((delegate* unmanaged[Stdcall]<void*, OptimizationIcalAlignment>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)77 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetFontFallback(FontFallback fontFallback)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFallback>(fontFallback);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)78 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void GetFontFallback(out FontFallback fontFallback)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)79 * (nint)sizeof(void*))))(_nativePointer, &zero);
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
