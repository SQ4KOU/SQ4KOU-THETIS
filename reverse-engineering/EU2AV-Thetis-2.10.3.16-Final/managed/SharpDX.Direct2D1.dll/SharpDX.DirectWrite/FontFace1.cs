using System;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("a71efdb4-9fdb-4838-ad90-cfc3be8c3daf")]
public class FontFace1 : FontFace
{
	public new FontMetrics1 Metrics
	{
		get
		{
			GetMetrics(out var fontMetrics);
			return fontMetrics;
		}
	}

	public CaretMetrics CaretMetrics
	{
		get
		{
			GetCaretMetrics(out var caretMetrics);
			return caretMetrics;
		}
	}

	public RawBool IsMonospacedFont => IsMonospacedFont_();

	public FontFace1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontFace1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontFace1(nativePtr);
		}
		return null;
	}

	internal unsafe void GetMetrics(out FontMetrics1 fontMetrics)
	{
		fontMetrics = default(FontMetrics1);
		fixed (FontMetrics1* ptr = &fontMetrics)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	public unsafe void GetGdiCompatibleMetrics(float emSize, float pixelsPerDip, RawMatrix3x2? transform, out FontMetrics1 fontMetrics)
	{
		fontMetrics = default(FontMetrics1);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		Result result;
		fixed (FontMetrics1* ptr = &fontMetrics)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			RawMatrix3x2* intPtr = ((!transform.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, float, float, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(nativePointer, emSize, pixelsPerDip, intPtr, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetCaretMetrics(out CaretMetrics caretMetrics)
	{
		caretMetrics = default(CaretMetrics);
		fixed (CaretMetrics* ptr = &caretMetrics)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	public unsafe void GetUnicodeRanges(int maxRangeCount, UnicodeRange[] unicodeRanges, out int actualRangeCount)
	{
		Result result;
		fixed (int* ptr = &actualRangeCount)
		{
			void* ptr2 = ptr;
			fixed (UnicodeRange* ptr3 = unicodeRanges)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, maxRangeCount, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe RawBool IsMonospacedFont_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void GetDesignGlyphAdvances(int glyphCount, short[] glyphIndices, int[] glyphAdvances, RawBool isSideways)
	{
		Result result;
		fixed (int* ptr = glyphAdvances)
		{
			void* ptr2 = ptr;
			fixed (short* ptr3 = glyphIndices)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, glyphCount, ptr4, ptr2, isSideways);
			}
		}
		result.CheckError();
	}

	public unsafe void GetGdiCompatibleGlyphAdvances(float emSize, float pixelsPerDip, RawMatrix3x2? transform, RawBool useGdiNatural, RawBool isSideways, int glyphCount, short[] glyphIndices, int[] glyphAdvances)
	{
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		Result result;
		fixed (int* ptr = glyphAdvances)
		{
			void* ptr2 = ptr;
			fixed (short* ptr3 = glyphIndices)
			{
				void* ptr4 = ptr3;
				void* nativePointer = _nativePointer;
				RawMatrix3x2* intPtr = ((!transform.HasValue) ? null : (&value));
				result = ((delegate* unmanaged[Stdcall]<void*, float, float, void*, RawBool, RawBool, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(nativePointer, emSize, pixelsPerDip, intPtr, useGdiNatural, isSideways, glyphCount, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void GetKerningPairAdjustments(int glyphCount, short[] glyphIndices, int[] glyphAdvanceAdjustments)
	{
		Result result;
		fixed (int* ptr = glyphAdvanceAdjustments)
		{
			void* ptr2 = ptr;
			fixed (short* ptr3 = glyphIndices)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, glyphCount, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe RawBool HasKerningPairs()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void GetRecommendedRenderingMode(float fontEmSize, float dpiX, float dpiY, RawMatrix3x2? transform, RawBool isSideways, OutlineThreshold outlineThreshold, MeasuringMode measuringMode, out RenderingMode renderingMode)
	{
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		Result result;
		fixed (RenderingMode* ptr = &renderingMode)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			RawMatrix3x2* intPtr = ((!transform.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, float, float, float, void*, RawBool, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(nativePointer, fontEmSize, dpiX, dpiY, intPtr, isSideways, (int)outlineThreshold, (int)measuringMode, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetVerticalGlyphVariants(int glyphCount, short[] nominalGlyphIndices, short[] verticalGlyphIndices)
	{
		Result result;
		fixed (short* ptr = verticalGlyphIndices)
		{
			void* ptr2 = ptr;
			fixed (short* ptr3 = nominalGlyphIndices)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, glyphCount, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe RawBool HasVerticalGlyphVariants()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(_nativePointer);
	}
}
