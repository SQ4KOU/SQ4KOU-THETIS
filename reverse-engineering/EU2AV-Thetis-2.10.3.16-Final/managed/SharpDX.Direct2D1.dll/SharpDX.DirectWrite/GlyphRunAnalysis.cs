using System;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("7d97dbf7-e085-42d4-81e3-6a883bded118")]
public class GlyphRunAnalysis : ComObject
{
	public GlyphRunAnalysis(Factory factory, GlyphRun glyphRun, float pixelsPerDip, RenderingMode renderingMode, MeasuringMode measuringMode, float baselineOriginX, float baselineOriginY)
		: this(factory, glyphRun, pixelsPerDip, null, renderingMode, measuringMode, baselineOriginX, baselineOriginY)
	{
	}

	public GlyphRunAnalysis(Factory factory, GlyphRun glyphRun, float pixelsPerDip, RawMatrix3x2? transform, RenderingMode renderingMode, MeasuringMode measuringMode, float baselineOriginX, float baselineOriginY)
	{
		factory.CreateGlyphRunAnalysis(glyphRun, pixelsPerDip, transform, renderingMode, measuringMode, baselineOriginX, baselineOriginY, this);
	}

	public GlyphRunAnalysis(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GlyphRunAnalysis(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new GlyphRunAnalysis(nativePtr);
		}
		return null;
	}

	public unsafe RawRectangle GetAlphaTextureBounds(TextureType textureType)
	{
		RawRectangle result = default(RawRectangle);
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (int)textureType, &result)).CheckError();
		return result;
	}

	public unsafe void CreateAlphaTexture(TextureType textureType, RawRectangle textureBounds, byte[] alphaValues, int bufferSize)
	{
		Result result;
		fixed (byte* ptr = alphaValues)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (int)textureType, &textureBounds, ptr2, bufferSize);
		}
		result.CheckError();
	}

	public unsafe void GetAlphaBlendParams(RenderingParams renderingParams, out float blendGamma, out float blendEnhancedContrast, out float blendClearTypeLevel)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<RenderingParams>(renderingParams);
		Result result;
		fixed (float* ptr = &blendClearTypeLevel)
		{
			void* ptr2 = ptr;
			fixed (float* ptr3 = &blendEnhancedContrast)
			{
				void* ptr4 = ptr3;
				fixed (float* ptr5 = &blendGamma)
				{
					void* ptr6 = ptr5;
					result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr6, ptr4, ptr2);
				}
			}
		}
		result.CheckError();
	}
}
