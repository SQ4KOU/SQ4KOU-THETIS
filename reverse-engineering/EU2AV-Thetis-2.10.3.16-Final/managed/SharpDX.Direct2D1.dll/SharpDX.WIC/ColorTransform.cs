using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("B66F034F-D0E2-40ab-B436-6DE39E321A94")]
public class ColorTransform : BitmapSource
{
	public ColorTransform(ImagingFactory factory)
		: base(IntPtr.Zero)
	{
		factory.CreateColorTransformer(this);
	}

	public ColorTransform(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ColorTransform(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ColorTransform(nativePtr);
		}
		return null;
	}

	public unsafe void Initialize(BitmapSource bitmapSourceRef, ColorContext contextSourceRef, ColorContext contextDestRef, Guid ixelFmtDestRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<BitmapSource>(bitmapSourceRef);
		zero2 = CppObject.ToCallbackPtr<ColorContext>(contextSourceRef);
		zero3 = CppObject.ToCallbackPtr<ColorContext>(contextDestRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, (void*)zero3, &ixelFmtDestRef)).CheckError();
	}
}
