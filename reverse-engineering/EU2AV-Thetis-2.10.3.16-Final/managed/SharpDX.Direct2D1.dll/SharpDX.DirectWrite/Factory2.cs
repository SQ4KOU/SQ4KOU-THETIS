using System;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("0439fc60-ca44-4994-8dee-3a9af7b732ec")]
public class Factory2 : Factory1
{
	public FontFallback SystemFontFallback
	{
		get
		{
			GetSystemFontFallback(out var fontFallback);
			return fontFallback;
		}
	}

	public void TranslateColorGlyphRun(float baselineOriginX, float baselineOriginY, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, MeasuringMode measuringMode, RawMatrix3x2? worldToDeviceTransform, int colorPaletteIndex, out ColorGlyphRunEnumerator colorLayers)
	{
		TryTranslateColorGlyphRun(baselineOriginX, baselineOriginY, glyphRun, glyphRunDescription, measuringMode, worldToDeviceTransform, colorPaletteIndex, out colorLayers).CheckError();
	}

	public ColorGlyphRunEnumerator TranslateColorGlyphRun(float baselineOriginX, float baselineOriginY, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, MeasuringMode measuringMode, RawMatrix3x2? worldToDeviceTransform, int colorPaletteIndex)
	{
		TranslateColorGlyphRun(baselineOriginX, baselineOriginY, glyphRun, glyphRunDescription, measuringMode, worldToDeviceTransform, colorPaletteIndex, out var colorLayers);
		return colorLayers;
	}

	public Factory2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory2(nativePtr);
		}
		return null;
	}

	internal unsafe void GetSystemFontFallback(out FontFallback fontFallback)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(_nativePointer, &zero);
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

	public unsafe void CreateFontFallbackBuilder(out FontFallbackBuilder fontFallbackBuilder)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			fontFallbackBuilder = new FontFallbackBuilder(zero);
		}
		else
		{
			fontFallbackBuilder = null;
		}
		result.CheckError();
	}

	public unsafe Result TryTranslateColorGlyphRun(float baselineOriginX, float baselineOriginY, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, MeasuringMode measuringMode, RawMatrix3x2? worldToDeviceTransform, int colorPaletteIndex, out ColorGlyphRunEnumerator colorLayers)
	{
		GlyphRun.__Native @ref = default(GlyphRun.__Native);
		GlyphRunDescription.__Native ref2 = default(GlyphRunDescription.__Native);
		IntPtr zero = IntPtr.Zero;
		glyphRun.__MarshalTo(ref @ref);
		glyphRunDescription?.__MarshalTo(ref ref2);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldToDeviceTransform.HasValue)
		{
			value = worldToDeviceTransform.Value;
		}
		void* nativePointer = _nativePointer;
		GlyphRun.__Native* num = &@ref;
		GlyphRunDescription.__Native* intPtr = ((glyphRunDescription == null) ? null : (&ref2));
		RawMatrix3x2* intPtr2 = ((!worldToDeviceTransform.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, float, float, void*, void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(nativePointer, baselineOriginX, baselineOriginY, num, intPtr, (int)measuringMode, intPtr2, colorPaletteIndex, &zero);
		if (zero != IntPtr.Zero)
		{
			colorLayers = new ColorGlyphRunEnumerator(zero);
		}
		else
		{
			colorLayers = null;
		}
		glyphRun.__MarshalFree(ref @ref);
		glyphRunDescription?.__MarshalFree(ref ref2);
		return result;
	}

	public unsafe void CreateCustomRenderingParams(float gamma, float enhancedContrast, float grayscaleEnhancedContrast, float clearTypeLevel, PixelGeometry pixelGeometry, RenderingMode renderingMode, GridFitMode gridFitMode, out RenderingParams2 renderingParams)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, float, float, float, float, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(_nativePointer, gamma, enhancedContrast, grayscaleEnhancedContrast, clearTypeLevel, (int)pixelGeometry, (int)renderingMode, (int)gridFitMode, &zero);
		if (zero != IntPtr.Zero)
		{
			renderingParams = new RenderingParams2(zero);
		}
		else
		{
			renderingParams = null;
		}
		result.CheckError();
	}

	public unsafe void CreateGlyphRunAnalysis(GlyphRun glyphRun, RawMatrix3x2? transform, RenderingMode renderingMode, MeasuringMode measuringMode, GridFitMode gridFitMode, TextAntialiasMode antialiasMode, float baselineOriginX, float baselineOriginY, out GlyphRunAnalysis glyphRunAnalysis)
	{
		GlyphRun.__Native @ref = default(GlyphRun.__Native);
		IntPtr zero = IntPtr.Zero;
		glyphRun.__MarshalTo(ref @ref);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		void* nativePointer = _nativePointer;
		GlyphRun.__Native* num = &@ref;
		RawMatrix3x2* intPtr = ((!transform.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int, int, int, float, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(nativePointer, num, intPtr, (int)renderingMode, (int)measuringMode, (int)gridFitMode, (int)antialiasMode, baselineOriginX, baselineOriginY, &zero);
		if (zero != IntPtr.Zero)
		{
			glyphRunAnalysis = new GlyphRunAnalysis(zero);
		}
		else
		{
			glyphRunAnalysis = null;
		}
		glyphRun.__MarshalFree(ref @ref);
		result.CheckError();
	}
}
