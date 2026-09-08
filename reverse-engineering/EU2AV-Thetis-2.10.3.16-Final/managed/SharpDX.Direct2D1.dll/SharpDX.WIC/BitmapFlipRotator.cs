using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("5009834F-2D6A-41ce-9E1B-17C5AFF7A782")]
public class BitmapFlipRotator : BitmapSource
{
	public BitmapFlipRotator(ImagingFactory factory)
		: base(IntPtr.Zero)
	{
		factory.CreateBitmapFlipRotator(this);
	}

	public BitmapFlipRotator(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapFlipRotator(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BitmapFlipRotator(nativePtr);
		}
		return null;
	}

	public unsafe void Initialize(BitmapSource sourceRef, BitmapTransformOptions options)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<BitmapSource>(sourceRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (int)options)).CheckError();
	}
}
