using System;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;

namespace SharpDX.WIC;

[Guid("04C75BF8-3CE1-473B-ACC5-3CC4F5E94999")]
public class ImageEncoder : ComObject
{
	public ImageEncoder(ImagingFactory2 factory, Device d2dDevice)
	{
		factory.CreateImageEncoder(d2dDevice, this);
	}

	public ImageEncoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ImageEncoder(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ImageEncoder(nativePtr);
		}
		return null;
	}

	public unsafe void WriteFrame(Image imageRef, BitmapFrameEncode frameEncodeRef, ImageParameters imageParametersRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Image>(imageRef);
		zero2 = CppObject.ToCallbackPtr<BitmapFrameEncode>(frameEncodeRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, &imageParametersRef)).CheckError();
	}

	public unsafe void WriteFrameThumbnail(Image imageRef, BitmapFrameEncode frameEncodeRef, ImageParameters imageParametersRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Image>(imageRef);
		zero2 = CppObject.ToCallbackPtr<BitmapFrameEncode>(frameEncodeRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, &imageParametersRef)).CheckError();
	}

	public unsafe void WriteThumbnail(Image imageRef, BitmapEncoder encoderRef, ImageParameters imageParametersRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Image>(imageRef);
		zero2 = CppObject.ToCallbackPtr<BitmapEncoder>(encoderRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, &imageParametersRef)).CheckError();
	}
}
