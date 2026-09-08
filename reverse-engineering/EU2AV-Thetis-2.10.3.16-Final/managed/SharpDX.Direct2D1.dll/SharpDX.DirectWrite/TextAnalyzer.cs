using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("b7e6163e-7f46-43b4-84b3-e4e6249c365d")]
public class TextAnalyzer : ComObject
{
	public TextAnalyzer(Factory factory)
	{
		factory.CreateTextAnalyzer(this);
	}

	public void GetGlyphs(string textString, int textLength, FontFace fontFace, bool isSideways, bool isRightToLeft, ScriptAnalysis scriptAnalysis, string localeName, NumberSubstitution numberSubstitution, FontFeature[][] features, int[] featureRangeLengths, int maxGlyphCount, short[] clusterMap, ShapingTextProperties[] textProps, short[] glyphIndices, ShapingGlyphProperties[] glyphProps, out int actualGlyphCount)
	{
		IntPtr intPtr = AllocateFeatures(features);
		try
		{
			GetGlyphs(textString, textLength, fontFace, isSideways, isRightToLeft, scriptAnalysis, localeName, numberSubstitution, intPtr, featureRangeLengths, (featureRangeLengths != null) ? featureRangeLengths.Length : 0, maxGlyphCount, clusterMap, textProps, glyphIndices, glyphProps, out actualGlyphCount);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
	}

	public void GetGlyphPlacements(string textString, short[] clusterMap, ShapingTextProperties[] textProps, int textLength, short[] glyphIndices, ShapingGlyphProperties[] glyphProps, int glyphCount, FontFace fontFace, float fontEmSize, bool isSideways, bool isRightToLeft, ScriptAnalysis scriptAnalysis, string localeName, FontFeature[][] features, int[] featureRangeLengths, float[] glyphAdvances, GlyphOffset[] glyphOffsets)
	{
		IntPtr intPtr = AllocateFeatures(features);
		try
		{
			GetGlyphPlacements(textString, clusterMap, textProps, textLength, glyphIndices, glyphProps, glyphCount, fontFace, fontEmSize, isSideways, isRightToLeft, scriptAnalysis, localeName, intPtr, featureRangeLengths, (featureRangeLengths != null) ? featureRangeLengths.Length : 0, glyphAdvances, glyphOffsets);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
	}

	public void GetGdiCompatibleGlyphPlacements(string textString, short[] clusterMap, ShapingTextProperties[] textProps, int textLength, short[] glyphIndices, ShapingGlyphProperties[] glyphProps, int glyphCount, FontFace fontFace, float fontEmSize, float pixelsPerDip, RawMatrix3x2? transform, bool useGdiNatural, bool isSideways, bool isRightToLeft, ScriptAnalysis scriptAnalysis, string localeName, FontFeature[][] features, int[] featureRangeLengths, float[] glyphAdvances, GlyphOffset[] glyphOffsets)
	{
		IntPtr intPtr = AllocateFeatures(features);
		try
		{
			GetGdiCompatibleGlyphPlacements(textString, clusterMap, textProps, textLength, glyphIndices, glyphProps, glyphCount, fontFace, fontEmSize, pixelsPerDip, transform, useGdiNatural, isSideways, isRightToLeft, scriptAnalysis, localeName, intPtr, featureRangeLengths, (featureRangeLengths != null) ? featureRangeLengths.Length : 0, glyphAdvances, glyphOffsets);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
	}

	private unsafe static IntPtr AllocateFeatures(FontFeature[][] features)
	{
		byte* ptr = null;
		if (features != null && features.Length != 0)
		{
			int num = sizeof(IntPtr) * features.Length;
			int num2 = num + sizeof(TypographicFeatures) * features.Length;
			foreach (FontFeature[] array in features)
			{
				if (array == null)
				{
					throw new ArgumentNullException("features", "FontFeature[] inside features array cannot be null.");
				}
				num2 += sizeof(FontFeature) * array.Length;
			}
			ptr = (byte*)(void*)Marshal.AllocHGlobal(num2);
			TypographicFeatures* ptr2 = (TypographicFeatures*)(ptr + num);
			FontFeature* ptr3 = (FontFeature*)(ptr2 + features.Length);
			for (int j = 0; j < features.Length; j++)
			{
				*(TypographicFeatures**)(ptr + (nint)j * (nint)sizeof(void*)) = ptr2;
				FontFeature[] array2 = features[j];
				ptr2->Features = (IntPtr)ptr3;
				ptr2->FeatureCount = array2.Length;
				ptr2++;
				for (int k = 0; k < array2.Length; k++)
				{
					*ptr3 = array2[k];
					ptr3++;
				}
			}
		}
		return (IntPtr)ptr;
	}

	public TextAnalyzer(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TextAnalyzer(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new TextAnalyzer(nativePtr);
		}
		return null;
	}

	public unsafe void AnalyzeScript(TextAnalysisSource analysisSource, int textPosition, int textLength, TextAnalysisSink analysisSink)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TextAnalysisSource>(analysisSource);
		zero2 = CppObject.ToCallbackPtr<TextAnalysisSink>(analysisSink);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, textPosition, textLength, (void*)zero2)).CheckError();
	}

	public unsafe void AnalyzeBidi(TextAnalysisSource analysisSource, int textPosition, int textLength, TextAnalysisSink analysisSink)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TextAnalysisSource>(analysisSource);
		zero2 = CppObject.ToCallbackPtr<TextAnalysisSink>(analysisSink);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, textPosition, textLength, (void*)zero2)).CheckError();
	}

	public unsafe void AnalyzeNumberSubstitution(TextAnalysisSource analysisSource, int textPosition, int textLength, TextAnalysisSink analysisSink)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TextAnalysisSource>(analysisSource);
		zero2 = CppObject.ToCallbackPtr<TextAnalysisSink>(analysisSink);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, textPosition, textLength, (void*)zero2)).CheckError();
	}

	public unsafe void AnalyzeLineBreakpoints(TextAnalysisSource analysisSource, int textPosition, int textLength, TextAnalysisSink analysisSink)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TextAnalysisSource>(analysisSource);
		zero2 = CppObject.ToCallbackPtr<TextAnalysisSink>(analysisSink);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, textPosition, textLength, (void*)zero2)).CheckError();
	}

	internal unsafe void GetGlyphs(string textString, int textLength, FontFace fontFace, RawBool isSideways, RawBool isRightToLeft, ScriptAnalysis scriptAnalysis, string localeName, NumberSubstitution numberSubstitution, IntPtr features, int[] featureRangeLengths, int featureRanges, int maxGlyphCount, short[] clusterMap, ShapingTextProperties[] textProps, short[] glyphIndices, ShapingGlyphProperties[] glyphProps, out int actualGlyphCount)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFace>(fontFace);
		zero2 = CppObject.ToCallbackPtr<NumberSubstitution>(numberSubstitution);
		Result result;
		fixed (int* ptr = &actualGlyphCount)
		{
			void* ptr2 = ptr;
			fixed (ShapingGlyphProperties* ptr3 = glyphProps)
			{
				void* ptr4 = ptr3;
				fixed (short* ptr5 = glyphIndices)
				{
					void* ptr6 = ptr5;
					fixed (ShapingTextProperties* ptr7 = textProps)
					{
						void* ptr8 = ptr7;
						fixed (short* ptr9 = clusterMap)
						{
							void* ptr10 = ptr9;
							fixed (int* ptr11 = featureRangeLengths)
							{
								void* ptr12 = ptr11;
								fixed (char* ptr13 = localeName)
								{
									fixed (char* ptr14 = textString)
									{
										result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, RawBool, RawBool, void*, void*, void*, void*, void*, int, int, void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr14, textLength, (void*)zero, isSideways, isRightToLeft, &scriptAnalysis, ptr13, (void*)zero2, (void*)features, ptr12, featureRanges, maxGlyphCount, ptr10, ptr8, ptr6, ptr4, ptr2);
									}
								}
							}
						}
					}
				}
			}
		}
		result.CheckError();
	}

	internal unsafe void GetGlyphPlacements(string textString, short[] clusterMap, ShapingTextProperties[] textProps, int textLength, short[] glyphIndices, ShapingGlyphProperties[] glyphProps, int glyphCount, FontFace fontFace, float fontEmSize, RawBool isSideways, RawBool isRightToLeft, ScriptAnalysis scriptAnalysis, string localeName, IntPtr features, int[] featureRangeLengths, int featureRanges, float[] glyphAdvances, GlyphOffset[] glyphOffsets)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFace>(fontFace);
		Result result;
		fixed (GlyphOffset* ptr = glyphOffsets)
		{
			void* ptr2 = ptr;
			fixed (float* ptr3 = glyphAdvances)
			{
				void* ptr4 = ptr3;
				fixed (int* ptr5 = featureRangeLengths)
				{
					void* ptr6 = ptr5;
					fixed (char* ptr7 = localeName)
					{
						fixed (ShapingGlyphProperties* ptr8 = glyphProps)
						{
							void* ptr9 = ptr8;
							fixed (short* ptr10 = glyphIndices)
							{
								void* ptr11 = ptr10;
								fixed (ShapingTextProperties* ptr12 = textProps)
								{
									void* ptr13 = ptr12;
									fixed (short* ptr14 = clusterMap)
									{
										void* ptr15 = ptr14;
										fixed (char* ptr16 = textString)
										{
											result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int, void*, void*, int, void*, float, RawBool, RawBool, void*, void*, void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr16, ptr15, ptr13, textLength, ptr11, ptr9, glyphCount, (void*)zero, fontEmSize, isSideways, isRightToLeft, &scriptAnalysis, ptr7, (void*)features, ptr6, featureRanges, ptr4, ptr2);
										}
									}
								}
							}
						}
					}
				}
			}
		}
		result.CheckError();
	}

	internal unsafe void GetGdiCompatibleGlyphPlacements(string textString, short[] clusterMap, ShapingTextProperties[] textProps, int textLength, short[] glyphIndices, ShapingGlyphProperties[] glyphProps, int glyphCount, FontFace fontFace, float fontEmSize, float pixelsPerDip, RawMatrix3x2? transform, RawBool useGdiNatural, RawBool isSideways, RawBool isRightToLeft, ScriptAnalysis scriptAnalysis, string localeName, IntPtr features, int[] featureRangeLengths, int featureRanges, float[] glyphAdvances, GlyphOffset[] glyphOffsets)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFace>(fontFace);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (transform.HasValue)
		{
			value = transform.Value;
		}
		Result result;
		fixed (GlyphOffset* ptr = glyphOffsets)
		{
			void* ptr2 = ptr;
			fixed (float* ptr3 = glyphAdvances)
			{
				void* ptr4 = ptr3;
				fixed (int* ptr5 = featureRangeLengths)
				{
					void* ptr6 = ptr5;
					fixed (char* ptr7 = localeName)
					{
						fixed (ShapingGlyphProperties* ptr8 = glyphProps)
						{
							void* ptr9 = ptr8;
							fixed (short* ptr10 = glyphIndices)
							{
								void* ptr11 = ptr10;
								fixed (ShapingTextProperties* ptr12 = textProps)
								{
									void* ptr13 = ptr12;
									fixed (short* ptr14 = clusterMap)
									{
										void* ptr15 = ptr14;
										fixed (char* ptr16 = textString)
										{
											void* nativePointer = _nativePointer;
											void* intPtr = (void*)zero;
											RawMatrix3x2* intPtr2 = ((!transform.HasValue) ? null : (&value));
											result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int, void*, void*, int, void*, float, float, void*, RawBool, RawBool, RawBool, void*, void*, void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(nativePointer, ptr16, ptr15, ptr13, textLength, ptr11, ptr9, glyphCount, intPtr, fontEmSize, pixelsPerDip, intPtr2, useGdiNatural, isSideways, isRightToLeft, &scriptAnalysis, ptr7, (void*)features, ptr6, featureRanges, ptr4, ptr2);
										}
									}
								}
							}
						}
					}
				}
			}
		}
		result.CheckError();
	}
}
