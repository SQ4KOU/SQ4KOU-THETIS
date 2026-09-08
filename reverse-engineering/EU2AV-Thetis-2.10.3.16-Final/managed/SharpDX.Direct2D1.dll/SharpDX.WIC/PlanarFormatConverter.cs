using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.WIC;

[Guid("BEBEE9CB-83B0-4DCC-8132-B0AAA55EAC96")]
public class PlanarFormatConverter : BitmapSource
{
	public PlanarFormatConverter(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator PlanarFormatConverter(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new PlanarFormatConverter(nativePtr);
		}
		return null;
	}

	public unsafe void Initialize(BitmapSource[] planesOut, int planes, Guid dstFormat, BitmapDitherType dither, Palette paletteRef, double alphaThresholdPercent, BitmapPaletteType paletteTranslate)
	{
		IntPtr* ptr = null;
		if (planesOut != null)
		{
			ptr = stackalloc IntPtr[planesOut.Length];
		}
		IntPtr zero = IntPtr.Zero;
		if (planesOut != null)
		{
			for (int i = 0; i < planesOut.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<BitmapSource>(planesOut[i]);
			}
		}
		zero = CppObject.ToCallbackPtr<Palette>(paletteRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, void*, double, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr, planes, &dstFormat, (int)dither, (void*)zero, alphaThresholdPercent, (int)paletteTranslate)).CheckError();
	}

	public unsafe void CanConvert(Guid[] srcPixelFormatsRef, int srcPlanes, Guid dstPixelFormat, out RawBool fCanConvertRef)
	{
		fCanConvertRef = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &fCanConvertRef)
		{
			void* ptr2 = ptr;
			fixed (Guid* ptr3 = srcPixelFormatsRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr4, srcPlanes, &dstPixelFormat, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void Initialize(ComArray<BitmapSource> planesOut, int planes, Guid dstFormat, BitmapDitherType dither, Palette paletteRef, double alphaThresholdPercent, BitmapPaletteType paletteTranslate)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Palette>(paletteRef);
		void* nativePointer = _nativePointer;
		IntPtr intPtr = planesOut?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, void*, double, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, planes, &dstFormat, (int)dither, (void*)zero, alphaThresholdPercent, (int)paletteTranslate)).CheckError();
	}

	private unsafe void Initialize(IntPtr planesOut, int planes, IntPtr dstFormat, BitmapDitherType dither, IntPtr paletteRef, double alphaThresholdPercent, BitmapPaletteType paletteTranslate)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, void*, double, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)planesOut, planes, (void*)dstFormat, (int)dither, (void*)paletteRef, alphaThresholdPercent, (int)paletteTranslate)).CheckError();
	}
}
