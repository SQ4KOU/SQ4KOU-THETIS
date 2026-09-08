using System;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("4B0B5BD3-0797-4549-8AC5-FE915CC53856")]
public class Factory4 : Factory3
{
	public Factory4(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory4(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory4(nativePtr);
		}
		return null;
	}

	public unsafe void TranslateColorGlyphRun(RawVector2 baselineOrigin, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, GlyphImageFormatS desiredGlyphImageFormats, MeasuringMode measuringMode, RawMatrix3x2? worldAndDpiTransform, int colorPaletteIndex, out ColorGlyphRunEnumerator1 colorLayers)
	{
		GlyphRun.__Native @ref = default(GlyphRun.__Native);
		GlyphRunDescription.__Native ref2 = default(GlyphRunDescription.__Native);
		IntPtr zero = IntPtr.Zero;
		glyphRun.__MarshalTo(ref @ref);
		glyphRunDescription?.__MarshalTo(ref ref2);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldAndDpiTransform.HasValue)
		{
			value = worldAndDpiTransform.Value;
		}
		void* nativePointer = _nativePointer;
		GlyphRun.__Native* num = &@ref;
		GlyphRunDescription.__Native* intPtr = ((glyphRunDescription == null) ? null : (&ref2));
		RawMatrix3x2* intPtr2 = ((!worldAndDpiTransform.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, RawVector2, void*, void*, int, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)40 * (nint)sizeof(void*))))(nativePointer, baselineOrigin, num, intPtr, (int)desiredGlyphImageFormats, (int)measuringMode, intPtr2, colorPaletteIndex, &zero);
		if (zero != IntPtr.Zero)
		{
			colorLayers = new ColorGlyphRunEnumerator1(zero);
		}
		else
		{
			colorLayers = null;
		}
		glyphRun.__MarshalFree(ref @ref);
		glyphRunDescription?.__MarshalFree(ref ref2);
		result.CheckError();
	}

	public unsafe void ComputeGlyphOrigins(GlyphRun glyphRun, RawVector2 baselineOrigin, RawVector2[] glyphOrigins)
	{
		GlyphRun.__Native @ref = default(GlyphRun.__Native);
		glyphRun.__MarshalTo(ref @ref);
		Result result;
		fixed (RawVector2* ptr = glyphOrigins)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, RawVector2, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)41 * (nint)sizeof(void*))))(_nativePointer, &@ref, baselineOrigin, ptr2);
		}
		glyphRun.__MarshalFree(ref @ref);
		result.CheckError();
	}

	public unsafe void ComputeGlyphOrigins(GlyphRun glyphRun, MeasuringMode measuringMode, RawVector2 baselineOrigin, RawMatrix3x2? worldAndDpiTransform, RawVector2[] glyphOrigins)
	{
		GlyphRun.__Native @ref = default(GlyphRun.__Native);
		glyphRun.__MarshalTo(ref @ref);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldAndDpiTransform.HasValue)
		{
			value = worldAndDpiTransform.Value;
		}
		Result result;
		fixed (RawVector2* ptr = glyphOrigins)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			GlyphRun.__Native* num = &@ref;
			RawMatrix3x2* intPtr = ((!worldAndDpiTransform.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, RawVector2, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)42 * (nint)sizeof(void*))))(nativePointer, num, (int)measuringMode, baselineOrigin, intPtr, ptr2);
		}
		glyphRun.__MarshalFree(ref @ref);
		result.CheckError();
	}
}
