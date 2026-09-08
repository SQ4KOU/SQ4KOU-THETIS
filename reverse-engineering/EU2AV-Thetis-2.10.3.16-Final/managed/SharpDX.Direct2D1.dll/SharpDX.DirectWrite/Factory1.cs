using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("30572f99-dac6-41db-a16e-0486307e606a")]
public class Factory1 : Factory
{
	public Factory1()
		: this(FactoryType.Shared)
	{
	}

	public Factory1(FactoryType factoryType)
		: base(IntPtr.Zero)
	{
		DWrite.CreateFactory(factoryType, Utilities.GetGuidFromType(typeof(Factory1)), this);
	}

	public Factory1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory1(nativePtr);
		}
		return null;
	}

	public unsafe void GetEudcFontCollection(out FontCollection fontCollection, RawBool checkForUpdates)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, RawBool, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, &zero, checkForUpdates);
		if (zero != IntPtr.Zero)
		{
			fontCollection = new FontCollection(zero);
		}
		else
		{
			fontCollection = null;
		}
		result.CheckError();
	}

	public unsafe void CreateCustomRenderingParams(float gamma, float enhancedContrast, float enhancedContrastGrayscale, float clearTypeLevel, PixelGeometry pixelGeometry, RenderingMode renderingMode, out RenderingParams1 renderingParams)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, float, float, float, float, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, gamma, enhancedContrast, enhancedContrastGrayscale, clearTypeLevel, (int)pixelGeometry, (int)renderingMode, &zero);
		if (zero != IntPtr.Zero)
		{
			renderingParams = new RenderingParams1(zero);
		}
		else
		{
			renderingParams = null;
		}
		result.CheckError();
	}
}
