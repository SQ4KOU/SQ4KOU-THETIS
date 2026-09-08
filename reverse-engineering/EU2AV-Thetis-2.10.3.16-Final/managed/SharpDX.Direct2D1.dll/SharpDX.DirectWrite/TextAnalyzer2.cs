using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("553A9FF3-5693-4DF7-B52B-74806F7F2EB9")]
public class TextAnalyzer2 : TextAnalyzer1
{
	public TextAnalyzer2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TextAnalyzer2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new TextAnalyzer2(nativePtr);
		}
		return null;
	}

	public unsafe void GetGlyphOrientationTransform(GlyphOrientationAngle glyphOrientationAngle, RawBool isSideways, float originX, float originY, out RawMatrix3x2 transform)
	{
		transform = default(RawMatrix3x2);
		Result result;
		fixed (RawMatrix3x2* ptr = &transform)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, RawBool, float, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, (int)glyphOrientationAngle, isSideways, originX, originY, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetTypographicFeatures(FontFace fontFace, ScriptAnalysis scriptAnalysis, string localeName, int maxTagCount, out int actualTagCount, FontFeatureTag[] tags)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFace>(fontFace);
		Result result;
		fixed (FontFeatureTag* ptr = tags)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &actualTagCount)
			{
				void* ptr4 = ptr3;
				fixed (char* ptr5 = localeName)
				{
					result = ((delegate* unmanaged[Stdcall]<void*, void*, ScriptAnalysis, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, scriptAnalysis, ptr5, maxTagCount, ptr4, ptr2);
				}
			}
		}
		result.CheckError();
	}

	public unsafe void CheckTypographicFeature(FontFace fontFace, ScriptAnalysis scriptAnalysis, string localeName, FontFeatureTag featureTag, int glyphCount, short[] glyphIndices, byte[] featureApplies)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFace>(fontFace);
		Result result;
		fixed (byte* ptr = featureApplies)
		{
			void* ptr2 = ptr;
			fixed (short* ptr3 = glyphIndices)
			{
				void* ptr4 = ptr3;
				fixed (char* ptr5 = localeName)
				{
					result = ((delegate* unmanaged[Stdcall]<void*, void*, ScriptAnalysis, void*, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, scriptAnalysis, ptr5, (int)featureTag, glyphCount, ptr4, ptr2);
				}
			}
		}
		result.CheckError();
	}
}
