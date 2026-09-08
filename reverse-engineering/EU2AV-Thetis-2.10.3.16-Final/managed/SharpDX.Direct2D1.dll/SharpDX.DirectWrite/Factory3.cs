using System;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("9A1B41C3-D3BB-466A-87FC-FE67556A3B65")]
public class Factory3 : Factory2
{
	public FontSet SystemFontSet
	{
		get
		{
			GetSystemFontSet(out var fontSet);
			return fontSet;
		}
	}

	public FontDownloadQueue FontDownloadQueue
	{
		get
		{
			GetFontDownloadQueue(out var fontDownloadQueue);
			return fontDownloadQueue;
		}
	}

	public Factory3(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory3(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory3(nativePtr);
		}
		return null;
	}

	public unsafe void CreateGlyphRunAnalysis(GlyphRun glyphRun, RawMatrix3x2? transform, RenderingMode1 renderingMode, MeasuringMode measuringMode, GridFitMode gridFitMode, TextAntialiasMode antialiasMode, float baselineOriginX, float baselineOriginY, out GlyphRunAnalysis glyphRunAnalysis)
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
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int, int, int, float, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)31 * (nint)sizeof(void*))))(nativePointer, num, intPtr, (int)renderingMode, (int)measuringMode, (int)gridFitMode, (int)antialiasMode, baselineOriginX, baselineOriginY, &zero);
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

	public unsafe void CreateCustomRenderingParams(float gamma, float enhancedContrast, float grayscaleEnhancedContrast, float clearTypeLevel, PixelGeometry pixelGeometry, RenderingMode1 renderingMode, GridFitMode gridFitMode, out RenderingParams3 renderingParams)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, float, float, float, float, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)32 * (nint)sizeof(void*))))(_nativePointer, gamma, enhancedContrast, grayscaleEnhancedContrast, clearTypeLevel, (int)pixelGeometry, (int)renderingMode, (int)gridFitMode, &zero);
		if (zero != IntPtr.Zero)
		{
			renderingParams = new RenderingParams3(zero);
		}
		else
		{
			renderingParams = null;
		}
		result.CheckError();
	}

	public unsafe void CreateFontFaceReference(FontFile fontFile, int faceIndex, FontSimulations fontSimulations, out FontFaceReference fontFaceReference)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFile>(fontFile);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)33 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, faceIndex, (int)fontSimulations, &zero2);
		if (zero2 != IntPtr.Zero)
		{
			fontFaceReference = new FontFaceReference(zero2);
		}
		else
		{
			fontFaceReference = null;
		}
		result.CheckError();
	}

	public unsafe void CreateFontFaceReference(string filePath, long? lastWriteTime, int faceIndex, FontSimulations fontSimulations, out FontFaceReference fontFaceReference)
	{
		IntPtr zero = IntPtr.Zero;
		long value = default(long);
		if (lastWriteTime.HasValue)
		{
			value = lastWriteTime.Value;
		}
		Result result;
		fixed (char* ptr = filePath)
		{
			void* nativePointer = _nativePointer;
			long* intPtr = ((!lastWriteTime.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)34 * (nint)sizeof(void*))))(nativePointer, ptr, intPtr, faceIndex, (int)fontSimulations, &zero);
		}
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

	internal unsafe void GetSystemFontSet(out FontSet fontSet)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)35 * (nint)sizeof(void*))))(_nativePointer, &zero);
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

	public unsafe void CreateFontSetBuilder(out FontSetBuilder fontSetBuilder)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)36 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			fontSetBuilder = new FontSetBuilder(zero);
		}
		else
		{
			fontSetBuilder = null;
		}
		result.CheckError();
	}

	public unsafe void CreateFontCollectionFromFontSet(FontSet fontSet, out FontCollection1 fontCollection)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontSet>(fontSet);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)37 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		if (zero2 != IntPtr.Zero)
		{
			fontCollection = new FontCollection1(zero2);
		}
		else
		{
			fontCollection = null;
		}
		result.CheckError();
	}

	public unsafe void GetSystemFontCollection(RawBool includeDownloadableFonts, out FontCollection1 fontCollection, RawBool checkForUpdates)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, RawBool, void*, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)38 * (nint)sizeof(void*))))(_nativePointer, includeDownloadableFonts, &zero, checkForUpdates);
		if (zero != IntPtr.Zero)
		{
			fontCollection = new FontCollection1(zero);
		}
		else
		{
			fontCollection = null;
		}
		result.CheckError();
	}

	internal unsafe void GetFontDownloadQueue(out FontDownloadQueue fontDownloadQueue)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)39 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			fontDownloadQueue = new FontDownloadQueue(zero);
		}
		else
		{
			fontDownloadQueue = null;
		}
		result.CheckError();
	}
}
