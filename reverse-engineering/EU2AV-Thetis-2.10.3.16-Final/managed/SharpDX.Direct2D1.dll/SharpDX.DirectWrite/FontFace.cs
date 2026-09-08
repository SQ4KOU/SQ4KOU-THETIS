using System;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("5f49804d-7024-4d43-bfa9-d25984f53849")]
public class FontFace : ComObject
{
	public FontFaceType FaceType => GetFaceType();

	public int Index => GetIndex();

	public FontSimulations Simulations => GetSimulations();

	public RawBool IsSymbolFont => IsSymbolFont_();

	public FontMetrics Metrics
	{
		get
		{
			GetMetrics(out var fontFaceMetrics);
			return fontFaceMetrics;
		}
	}

	public short GlyphCount => GetGlyphCount();

	public FontFace(Factory factory, FontFaceType fontFaceType, FontFile[] fontFiles, int faceIndex, FontSimulations fontFaceSimulationFlags)
	{
		factory.CreateFontFace(fontFaceType, fontFiles.Length, fontFiles, faceIndex, fontFaceSimulationFlags, this);
	}

	public FontFace(Font font)
	{
		font.CreateFontFace(this);
	}

	public GlyphMetrics[] GetDesignGlyphMetrics(short[] glyphIndices, bool isSideways)
	{
		GlyphMetrics[] array = new GlyphMetrics[glyphIndices.Length];
		GetDesignGlyphMetrics(glyphIndices, glyphIndices.Length, array, isSideways);
		return array;
	}

	public GlyphMetrics[] GetGdiCompatibleGlyphMetrics(float fontSize, float pixelsPerDip, RawMatrix3x2? transform, bool useGdiNatural, short[] glyphIndices, bool isSideways)
	{
		GlyphMetrics[] array = new GlyphMetrics[glyphIndices.Length];
		GetGdiCompatibleGlyphMetrics(fontSize, pixelsPerDip, transform, useGdiNatural, glyphIndices, glyphIndices.Length, array, isSideways);
		return array;
	}

	public short[] GetGlyphIndices(int[] codePoints)
	{
		short[] array = new short[codePoints.Length];
		GetGlyphIndices(codePoints, codePoints.Length, array);
		return array;
	}

	public FontFile[] GetFiles()
	{
		int numberOfFiles = 0;
		GetFiles(ref numberOfFiles, null);
		FontFile[] array = new FontFile[numberOfFiles];
		GetFiles(ref numberOfFiles, array);
		return array;
	}

	public unsafe bool TryGetFontTable(int openTypeTableTag, out DataPointer tableData, out IntPtr tableContext)
	{
		tableData = DataPointer.Zero;
		IntPtr zero = IntPtr.Zero;
		TryGetFontTable(openTypeTableTag, new IntPtr(&zero), out var tableSize, out tableContext, out var exists);
		if (zero != IntPtr.Zero)
		{
			tableData = new DataPointer(zero, tableSize);
		}
		return exists;
	}

	public FontFace(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontFace(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontFace(nativePtr);
		}
		return null;
	}

	internal unsafe FontFaceType GetFaceType()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FontFaceType>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetFiles(ref int numberOfFiles, FontFile[] fontFiles)
	{
		IntPtr* ptr = null;
		if (fontFiles != null)
		{
			ptr = stackalloc IntPtr[fontFiles.Length];
		}
		Result result;
		fixed (int* ptr2 = &numberOfFiles)
		{
			void* ptr3 = ptr2;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr3, ptr);
		}
		if (fontFiles != null)
		{
			for (int i = 0; i < fontFiles.Length; i++)
			{
				if (ptr[i] != IntPtr.Zero)
				{
					fontFiles[i] = new FontFile(ptr[i]);
				}
				else
				{
					fontFiles[i] = null;
				}
			}
		}
		result.CheckError();
	}

	internal unsafe int GetIndex()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe FontSimulations GetSimulations()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FontSimulations>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe RawBool IsSymbolFont_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetMetrics(out FontMetrics fontFaceMetrics)
	{
		fontFaceMetrics = default(FontMetrics);
		fixed (FontMetrics* ptr = &fontFaceMetrics)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	internal unsafe short GetGlyphCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, short>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetDesignGlyphMetrics(short[] glyphIndices, int glyphCount, GlyphMetrics[] glyphMetrics, RawBool isSideways)
	{
		Result result;
		fixed (GlyphMetrics* ptr = glyphMetrics)
		{
			void* ptr2 = ptr;
			fixed (short* ptr3 = glyphIndices)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, ptr4, glyphCount, ptr2, isSideways);
			}
		}
		result.CheckError();
	}

	internal unsafe void GetGlyphIndices(int[] codePoints, int codePointCount, short[] glyphIndices)
	{
		Result result;
		fixed (short* ptr = glyphIndices)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = codePoints)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr4, codePointCount, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void TryGetFontTable(int openTypeTableTag, IntPtr tableData, out int tableSize, out IntPtr tableContext, out RawBool exists)
	{
		exists = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &exists)
		{
			void* ptr2 = ptr;
			fixed (IntPtr* ptr3 = &tableContext)
			{
				void* ptr4 = ptr3;
				fixed (int* ptr5 = &tableSize)
				{
					void* ptr6 = ptr5;
					result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, openTypeTableTag, (void*)tableData, ptr6, ptr4, ptr2);
				}
			}
		}
		result.CheckError();
	}

	public unsafe void ReleaseFontTable(IntPtr tableContext)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, (void*)tableContext);
	}

	public unsafe void GetGlyphRunOutline(float emSize, short[] glyphIndices, float[] glyphAdvances, GlyphOffset[] glyphOffsets, int glyphCount, RawBool isSideways, RawBool isRightToLeft, SimplifiedGeometrySink geometrySink)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SimplifiedGeometrySink>(geometrySink);
		Result result;
		fixed (GlyphOffset* ptr = glyphOffsets)
		{
			void* ptr2 = ptr;
			fixed (float* ptr3 = glyphAdvances)
			{
				void* ptr4 = ptr3;
				fixed (short* ptr5 = glyphIndices)
				{
					void* ptr6 = ptr5;
					result = ((delegate* unmanaged[Stdcall]<void*, float, void*, void*, void*, int, RawBool, RawBool, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, emSize, ptr6, ptr4, ptr2, glyphCount, isSideways, isRightToLeft, (void*)zero);
				}
			}
		}
		result.CheckError();
	}

	public unsafe RenderingMode GetRecommendedRenderingMode(float emSize, float pixelsPerDip, MeasuringMode measuringMode, RenderingParams renderingParams)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<RenderingParams>(renderingParams);
		RenderingMode result = default(RenderingMode);
		((Result)((delegate* unmanaged[Stdcall]<void*, float, float, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, emSize, pixelsPerDip, (int)measuringMode, (void*)zero, &result)).CheckError();
		return result;
	}

	public unsafe FontMetrics GetGdiCompatibleMetrics(float emSize, float pixelsPerDip, RawMatrix3x2? transform)
	{
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		void* nativePointer = _nativePointer;
		RawMatrix3x2* intPtr = ((!transform.HasValue) ? null : (&value));
		FontMetrics result = default(FontMetrics);
		((Result)((delegate* unmanaged[Stdcall]<void*, float, float, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(nativePointer, emSize, pixelsPerDip, intPtr, &result)).CheckError();
		return result;
	}

	internal unsafe void GetGdiCompatibleGlyphMetrics(float emSize, float pixelsPerDip, RawMatrix3x2? transform, RawBool useGdiNatural, short[] glyphIndices, int glyphCount, GlyphMetrics[] glyphMetrics, RawBool isSideways)
	{
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		Result result;
		fixed (GlyphMetrics* ptr = glyphMetrics)
		{
			void* ptr2 = ptr;
			fixed (short* ptr3 = glyphIndices)
			{
				void* ptr4 = ptr3;
				void* nativePointer = _nativePointer;
				RawMatrix3x2* intPtr = ((!transform.HasValue) ? null : (&value));
				result = ((delegate* unmanaged[Stdcall]<void*, float, float, void*, RawBool, void*, int, void*, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(nativePointer, emSize, pixelsPerDip, intPtr, useGdiNatural, ptr4, glyphCount, ptr2, isSideways);
			}
		}
		result.CheckError();
	}
}
