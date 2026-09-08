using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.WIC;

[Guid("E4FBCF03-223D-4e81-9333-D635556DD1B5")]
public class BitmapClipper : BitmapSource
{
	public BitmapClipper(ImagingFactory factory)
		: base(IntPtr.Zero)
	{
		factory.CreateBitmapClipper(this);
	}

	public unsafe void Initialize(BitmapSource sourceRef, RawBox rectangleRef)
	{
		Initialize(sourceRef, new IntPtr(&rectangleRef));
	}

	public BitmapClipper(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapClipper(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BitmapClipper(nativePtr);
		}
		return null;
	}

	internal unsafe void Initialize(BitmapSource sourceRef, IntPtr rectangleRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<BitmapSource>(sourceRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)rectangleRef)).CheckError();
	}
}
