using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("80DAD800-E21F-4E83-96CE-BFCCE500DB7C")]
public class TextAnalyzer1 : TextAnalyzer
{
	public TextAnalyzer1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TextAnalyzer1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new TextAnalyzer1(nativePtr);
		}
		return null;
	}

	public unsafe void ApplyCharacterSpacing(float leadingSpacing, float trailingSpacing, float minimumAdvanceWidth, int textLength, int glyphCount, short[] clusterMap, float[] glyphAdvances, GlyphOffset[] glyphOffsets, ShapingGlyphProperties[] glyphProperties, float[] modifiedGlyphAdvances, GlyphOffset[] modifiedGlyphOffsets)
	{
		Result result;
		fixed (GlyphOffset* ptr = modifiedGlyphOffsets)
		{
			void* ptr2 = ptr;
			fixed (float* ptr3 = modifiedGlyphAdvances)
			{
				void* ptr4 = ptr3;
				fixed (ShapingGlyphProperties* ptr5 = glyphProperties)
				{
					void* ptr6 = ptr5;
					fixed (GlyphOffset* ptr7 = glyphOffsets)
					{
						void* ptr8 = ptr7;
						fixed (float* ptr9 = glyphAdvances)
						{
							void* ptr10 = ptr9;
							fixed (short* ptr11 = clusterMap)
							{
								void* ptr12 = ptr11;
								result = ((delegate* unmanaged[Stdcall]<void*, float, float, float, int, int, void*, void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, leadingSpacing, trailingSpacing, minimumAdvanceWidth, textLength, glyphCount, ptr12, ptr10, ptr8, ptr6, ptr4, ptr2);
							}
						}
					}
				}
			}
		}
		result.CheckError();
	}

	public unsafe void GetBaseline(FontFace fontFace, Baseline baseline, RawBool isVertical, RawBool isSimulationAllowed, ScriptAnalysis scriptAnalysis, string localeName, out int baselineCoordinate, out RawBool exists)
	{
		IntPtr zero = IntPtr.Zero;
		exists = default(RawBool);
		zero = CppObject.ToCallbackPtr<FontFace>(fontFace);
		Result result;
		fixed (RawBool* ptr = &exists)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &baselineCoordinate)
			{
				void* ptr4 = ptr3;
				fixed (char* ptr5 = localeName)
				{
					result = ((delegate* unmanaged[Stdcall]<void*, void*, int, RawBool, RawBool, ScriptAnalysis, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (int)baseline, isVertical, isSimulationAllowed, scriptAnalysis, ptr5, ptr4, ptr2);
				}
			}
		}
		result.CheckError();
	}

	public unsafe void AnalyzeVerticalGlyphOrientation(TextAnalysisSource1 analysisSource, int textPosition, int textLength, TextAnalysisSink1 analysisSink)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TextAnalysisSource1>(analysisSource);
		zero2 = CppObject.ToCallbackPtr<TextAnalysisSink1>(analysisSink);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, textPosition, textLength, (void*)zero2)).CheckError();
	}

	public unsafe void GetGlyphOrientationTransform(GlyphOrientationAngle glyphOrientationAngle, RawBool isSideways, out RawMatrix3x2 transform)
	{
		transform = default(RawMatrix3x2);
		Result result;
		fixed (RawMatrix3x2* ptr = &transform)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, RawBool, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, (int)glyphOrientationAngle, isSideways, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetScriptProperties(ScriptAnalysis scriptAnalysis, out ScriptProperties scriptProperties)
	{
		scriptProperties = default(ScriptProperties);
		Result result;
		fixed (ScriptProperties* ptr = &scriptProperties)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, ScriptAnalysis, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, scriptAnalysis, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetTextComplexity(string textString, int textLength, FontFace fontFace, out RawBool isTextSimple, int textLengthRead, short[] glyphIndices)
	{
		IntPtr zero = IntPtr.Zero;
		isTextSimple = default(RawBool);
		zero = CppObject.ToCallbackPtr<FontFace>(fontFace);
		Result result;
		fixed (short* ptr = glyphIndices)
		{
			void* ptr2 = ptr;
			fixed (RawBool* ptr3 = &isTextSimple)
			{
				void* ptr4 = ptr3;
				fixed (char* ptr5 = textString)
				{
					result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, ptr5, textLength, (void*)zero, ptr4, &textLengthRead, ptr2);
				}
			}
		}
		result.CheckError();
	}

	public unsafe void GetJustificationOpportunities(FontFace fontFace, float fontEmSize, ScriptAnalysis scriptAnalysis, int textLength, int glyphCount, string textString, short[] clusterMap, ShapingGlyphProperties[] glyphProperties, JustificationOpportunity[] justificationOpportunities)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFace>(fontFace);
		Result result;
		fixed (JustificationOpportunity* ptr = justificationOpportunities)
		{
			void* ptr2 = ptr;
			fixed (ShapingGlyphProperties* ptr3 = glyphProperties)
			{
				void* ptr4 = ptr3;
				fixed (short* ptr5 = clusterMap)
				{
					void* ptr6 = ptr5;
					fixed (char* ptr7 = textString)
					{
						result = ((delegate* unmanaged[Stdcall]<void*, void*, float, ScriptAnalysis, int, int, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, fontEmSize, scriptAnalysis, textLength, glyphCount, ptr7, ptr6, ptr4, ptr2);
					}
				}
			}
		}
		result.CheckError();
	}

	public unsafe void JustifyGlyphAdvances(float lineWidth, int glyphCount, JustificationOpportunity[] justificationOpportunities, float[] glyphAdvances, GlyphOffset[] glyphOffsets, float[] justifiedGlyphAdvances, GlyphOffset[] justifiedGlyphOffsets)
	{
		Result result;
		fixed (GlyphOffset* ptr = justifiedGlyphOffsets)
		{
			void* ptr2 = ptr;
			fixed (float* ptr3 = justifiedGlyphAdvances)
			{
				void* ptr4 = ptr3;
				fixed (GlyphOffset* ptr5 = glyphOffsets)
				{
					void* ptr6 = ptr5;
					fixed (float* ptr7 = glyphAdvances)
					{
						void* ptr8 = ptr7;
						fixed (JustificationOpportunity* ptr9 = justificationOpportunities)
						{
							void* ptr10 = ptr9;
							result = ((delegate* unmanaged[Stdcall]<void*, float, int, void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, lineWidth, glyphCount, ptr10, ptr8, ptr6, ptr4, ptr2);
						}
					}
				}
			}
		}
		result.CheckError();
	}

	public unsafe void GetJustifiedGlyphs(FontFace fontFace, float fontEmSize, ScriptAnalysis scriptAnalysis, int textLength, int glyphCount, int maxGlyphCount, short[] clusterMap, short[] glyphIndices, float[] glyphAdvances, float[] justifiedGlyphAdvances, GlyphOffset[] justifiedGlyphOffsets, ShapingGlyphProperties[] glyphProperties, int actualGlyphCount, short[] modifiedClusterMap, short[] modifiedGlyphIndices, float[] modifiedGlyphAdvances, GlyphOffset[] modifiedGlyphOffsets)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFace>(fontFace);
		Result result;
		fixed (GlyphOffset* ptr = modifiedGlyphOffsets)
		{
			void* ptr2 = ptr;
			fixed (float* ptr3 = modifiedGlyphAdvances)
			{
				void* ptr4 = ptr3;
				fixed (short* ptr5 = modifiedGlyphIndices)
				{
					void* ptr6 = ptr5;
					fixed (short* ptr7 = modifiedClusterMap)
					{
						void* ptr8 = ptr7;
						fixed (ShapingGlyphProperties* ptr9 = glyphProperties)
						{
							void* ptr10 = ptr9;
							fixed (GlyphOffset* ptr11 = justifiedGlyphOffsets)
							{
								void* ptr12 = ptr11;
								fixed (float* ptr13 = justifiedGlyphAdvances)
								{
									void* ptr14 = ptr13;
									fixed (float* ptr15 = glyphAdvances)
									{
										void* ptr16 = ptr15;
										fixed (short* ptr17 = glyphIndices)
										{
											void* ptr18 = ptr17;
											fixed (short* ptr19 = clusterMap)
											{
												void* ptr20 = ptr19;
												result = ((delegate* unmanaged[Stdcall]<void*, void*, float, ScriptAnalysis, int, int, int, void*, void*, void*, void*, void*, void*, void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, fontEmSize, scriptAnalysis, textLength, glyphCount, maxGlyphCount, ptr20, ptr18, ptr16, ptr14, ptr12, ptr10, &actualGlyphCount, ptr8, ptr6, ptr4, ptr2);
											}
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
