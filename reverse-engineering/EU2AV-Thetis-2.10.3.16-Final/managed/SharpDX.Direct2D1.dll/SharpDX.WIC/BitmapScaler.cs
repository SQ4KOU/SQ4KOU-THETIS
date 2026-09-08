using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("00000302-a8f2-4877-ba0a-fd2b6645fb94")]
public class BitmapScaler : BitmapSource
{
	public BitmapScaler(ImagingFactory factory)
		: base(IntPtr.Zero)
	{
		factory.CreateBitmapScaler(this);
	}

	public BitmapScaler(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapScaler(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BitmapScaler(nativePtr);
		}
		return null;
	}

	public unsafe void Initialize(BitmapSource sourceRef, int width, int height, BitmapInterpolationMode mode)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<BitmapSource>(sourceRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, width, height, (int)mode)).CheckError();
	}
}
