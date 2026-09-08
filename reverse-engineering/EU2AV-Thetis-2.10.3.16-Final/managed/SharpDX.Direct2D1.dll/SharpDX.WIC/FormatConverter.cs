using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.WIC;

[Guid("00000301-a8f2-4877-ba0a-fd2b6645fb94")]
public class FormatConverter : BitmapSource
{
	public FormatConverter(FormatConverterInfo converterInfo)
		: base(IntPtr.Zero)
	{
		converterInfo.CreateInstance(this);
	}

	public void Initialize(BitmapSource sourceRef, Guid dstFormat)
	{
		Initialize(sourceRef, dstFormat, BitmapDitherType.None, null, 0.0, BitmapPaletteType.Custom);
	}

	public FormatConverter(ImagingFactory factory)
		: base(IntPtr.Zero)
	{
		factory.CreateFormatConverter(this);
	}

	public FormatConverter(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FormatConverter(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FormatConverter(nativePtr);
		}
		return null;
	}

	public unsafe void Initialize(BitmapSource sourceRef, Guid dstFormat, BitmapDitherType dither, Palette paletteRef, double alphaThresholdPercent, BitmapPaletteType paletteTranslate)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<BitmapSource>(sourceRef);
		zero2 = CppObject.ToCallbackPtr<Palette>(paletteRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, double, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &dstFormat, (int)dither, (void*)zero2, alphaThresholdPercent, (int)paletteTranslate)).CheckError();
	}

	public unsafe RawBool CanConvert(Guid srcPixelFormat, Guid dstPixelFormat)
	{
		RawBool result = default(RawBool);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, &srcPixelFormat, &dstPixelFormat, &result)).CheckError();
		return result;
	}
}
