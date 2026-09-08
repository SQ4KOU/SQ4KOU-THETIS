using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("b859ee5a-d838-4b5b-a2e8-1adc7d93db48")]
public class Factory : ComObject
{
	private readonly List<FontCollectionLoader> _fontCollectionLoaderCallbacks = new List<FontCollectionLoader>();

	private readonly List<FontFileLoader> _fontFileLoaderCallbacks = new List<FontFileLoader>();

	public GdiInterop GdiInterop
	{
		get
		{
			GetGdiInterop(out var gdiInterop);
			return gdiInterop;
		}
	}

	internal FontCollectionLoader FindRegisteredFontCollectionLoaderCallback(FontCollectionLoader loader)
	{
		foreach (FontCollectionLoader fontCollectionLoaderCallback in _fontCollectionLoaderCallbacks)
		{
			if (fontCollectionLoaderCallback == loader)
			{
				return fontCollectionLoaderCallback;
			}
		}
		return null;
	}

	internal FontFileLoader FindRegisteredFontFileLoaderCallback(FontFileLoader loader)
	{
		foreach (FontFileLoader fontFileLoaderCallback in _fontFileLoaderCallbacks)
		{
			if (fontFileLoaderCallback == loader)
			{
				return fontFileLoaderCallback;
			}
		}
		return null;
	}

	public Factory()
		: this(FactoryType.Shared)
	{
	}

	public Factory(FactoryType factoryType)
		: base(IntPtr.Zero)
	{
		DWrite.CreateFactory(factoryType, Utilities.GetGuidFromType(typeof(Factory)), this);
	}

	public void RegisterFontCollectionLoader(FontCollectionLoader fontCollectionLoader)
	{
		FontCollectionLoaderShadow.SetFactory(fontCollectionLoader, this);
		RegisterFontCollectionLoader_(fontCollectionLoader);
		_fontCollectionLoaderCallbacks.Add(fontCollectionLoader);
	}

	public void UnregisterFontCollectionLoader(FontCollectionLoader fontCollectionLoader)
	{
		if (!_fontCollectionLoaderCallbacks.Contains(fontCollectionLoader))
		{
			throw new ArgumentException("This font collection loader is not registered", "fontCollectionLoader");
		}
		UnregisterFontCollectionLoader_(fontCollectionLoader);
		_fontCollectionLoaderCallbacks.Remove(fontCollectionLoader);
	}

	public void RegisterFontFileLoader(FontFileLoader fontFileLoader)
	{
		RegisterFontFileLoader_(fontFileLoader);
		_fontFileLoaderCallbacks.Add(fontFileLoader);
	}

	public void UnregisterFontFileLoader(FontFileLoader fontFileLoader)
	{
		if (!_fontFileLoaderCallbacks.Contains(fontFileLoader))
		{
			throw new ArgumentException("This font file loader is not registered", "fontFileLoader");
		}
		UnregisterFontFileLoader_(fontFileLoader);
		_fontFileLoaderCallbacks.Remove(fontFileLoader);
	}

	public Factory(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory(nativePtr);
		}
		return null;
	}

	public unsafe FontCollection GetSystemFontCollection(RawBool checkForUpdates)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, &zero, checkForUpdates);
		FontCollection result2 = ((!(zero != IntPtr.Zero)) ? null : new FontCollection(zero));
		result.CheckError();
		return result2;
	}

	internal unsafe void CreateCustomFontCollection(FontCollectionLoader collectionLoader, IntPtr collectionKey, int collectionKeySize, FontCollection fontCollection)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontCollectionLoader>(collectionLoader);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)collectionKey, collectionKeySize, &zero2);
		fontCollection.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void RegisterFontCollectionLoader_(FontCollectionLoader fontCollectionLoader)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontCollectionLoader>(fontCollectionLoader);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void UnregisterFontCollectionLoader_(FontCollectionLoader fontCollectionLoader)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontCollectionLoader>(fontCollectionLoader);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void CreateFontFileReference(string filePath, long? lastWriteTime, FontFile fontFile)
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
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(nativePointer, ptr, intPtr, &zero);
		}
		fontFile.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateCustomFontFileReference(IntPtr fontFileReferenceKey, int fontFileReferenceKeySize, FontFileLoader fontFileLoader, FontFile fontFile)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFileLoader>(fontFileLoader);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)fontFileReferenceKey, fontFileReferenceKeySize, (void*)zero, &zero2);
		fontFile.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateFontFace(FontFaceType fontFaceType, int numberOfFiles, FontFile[] fontFiles, int faceIndex, FontSimulations fontFaceSimulationFlags, FontFace fontFace)
	{
		IntPtr* ptr = null;
		if (fontFiles != null)
		{
			ptr = stackalloc IntPtr[fontFiles.Length];
		}
		IntPtr zero = IntPtr.Zero;
		if (fontFiles != null)
		{
			for (int i = 0; i < fontFiles.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<FontFile>(fontFiles[i]);
			}
		}
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (int)fontFaceType, numberOfFiles, ptr, faceIndex, (int)fontFaceSimulationFlags, &zero);
		fontFace.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateRenderingParams(RenderingParams renderingParams)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, &zero);
		renderingParams.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateMonitorRenderingParams(IntPtr monitor, RenderingParams renderingParams)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, (void*)monitor, &zero);
		renderingParams.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateCustomRenderingParams(float gamma, float enhancedContrast, float clearTypeLevel, PixelGeometry pixelGeometry, RenderingMode renderingMode, RenderingParams renderingParams)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, float, float, float, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, gamma, enhancedContrast, clearTypeLevel, (int)pixelGeometry, (int)renderingMode, &zero);
		renderingParams.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void RegisterFontFileLoader_(FontFileLoader fontFileLoader)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFileLoader>(fontFileLoader);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void UnregisterFontFileLoader_(FontFileLoader fontFileLoader)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFileLoader>(fontFileLoader);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void CreateTextFormat(string fontFamilyName, FontCollection fontCollection, FontWeight fontWeight, FontStyle fontStyle, FontStretch fontStretch, float fontSize, string localeName, TextFormat textFormat)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontCollection>(fontCollection);
		Result result;
		fixed (char* ptr = localeName)
		{
			fixed (char* ptr2 = fontFamilyName)
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int, int, float, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, ptr2, (void*)zero, (int)fontWeight, (int)fontStyle, (int)fontStretch, fontSize, ptr, &zero2);
			}
		}
		textFormat.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateTypography(Typography typography)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, &zero);
		typography.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void GetGdiInterop(out GdiInterop gdiInterop)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			gdiInterop = new GdiInterop(zero);
		}
		else
		{
			gdiInterop = null;
		}
		result.CheckError();
	}

	internal unsafe void CreateTextLayout(string text, int stringLength, TextFormat textFormat, float maxWidth, float maxHeight, TextLayout textLayout)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TextFormat>(textFormat);
		Result result;
		fixed (char* ptr = text)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, float, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, ptr, stringLength, (void*)zero, maxWidth, maxHeight, &zero2);
		}
		textLayout.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateGdiCompatibleTextLayout(string text, int stringLength, TextFormat textFormat, float layoutWidth, float layoutHeight, float pixelsPerDip, RawMatrix3x2? transform, RawBool useGdiNatural, TextLayout textLayout)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TextFormat>(textFormat);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		Result result;
		fixed (char* ptr = text)
		{
			void* nativePointer = _nativePointer;
			void* intPtr = (void*)zero;
			RawMatrix3x2* intPtr2 = ((!transform.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, float, float, float, void*, RawBool, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(nativePointer, ptr, stringLength, intPtr, layoutWidth, layoutHeight, pixelsPerDip, intPtr2, useGdiNatural, &zero2);
		}
		textLayout.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateEllipsisTrimmingSign(TextFormat textFormat, InlineObjectNative trimmingSign)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TextFormat>(textFormat);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		trimmingSign.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateTextAnalyzer(TextAnalyzer textAnalyzer)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, &zero);
		textAnalyzer.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateNumberSubstitution(NumberSubstitutionMethod substitutionMethod, string localeName, RawBool ignoreUserOverride, NumberSubstitution numberSubstitution)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (char* ptr = localeName)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, RawBool, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, (int)substitutionMethod, ptr, ignoreUserOverride, &zero);
		}
		numberSubstitution.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateGlyphRunAnalysis(GlyphRun glyphRun, float pixelsPerDip, RawMatrix3x2? transform, RenderingMode renderingMode, MeasuringMode measuringMode, float baselineOriginX, float baselineOriginY, GlyphRunAnalysis glyphRunAnalysis)
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
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, float, void*, int, int, float, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(nativePointer, num, pixelsPerDip, intPtr, (int)renderingMode, (int)measuringMode, baselineOriginX, baselineOriginY, &zero);
		glyphRunAnalysis.NativePointer = zero;
		glyphRun.__MarshalFree(ref @ref);
		result.CheckError();
	}

	internal unsafe void CreateFontFace(FontFaceType fontFaceType, int numberOfFiles, ComArray<FontFile> fontFiles, int faceIndex, FontSimulations fontFaceSimulationFlags, FontFace fontFace)
	{
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		IntPtr intPtr = fontFiles?.NativePointer ?? IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(nativePointer, (int)fontFaceType, numberOfFiles, (void*)intPtr, faceIndex, (int)fontFaceSimulationFlags, &zero);
		fontFace.NativePointer = zero;
		result.CheckError();
	}

	private unsafe void CreateFontFace(FontFaceType fontFaceType, int numberOfFiles, IntPtr fontFiles, int faceIndex, FontSimulations fontFaceSimulationFlags, IntPtr fontFace)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (int)fontFaceType, numberOfFiles, (void*)fontFiles, faceIndex, (int)fontFaceSimulationFlags, (void*)fontFace)).CheckError();
	}
}
