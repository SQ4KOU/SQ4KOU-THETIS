using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;

namespace SharpDX.Direct2D1;

[Guid("77395441-1c8f-4555-8683-f50dab0fe792")]
public class ImageSourceFromWic : ImageSource
{
	public BitmapSource Source
	{
		get
		{
			GetSource(out var wicBitmapSource);
			return wicBitmapSource;
		}
	}

	public ImageSourceFromWic(DeviceContext2 context2, BitmapSource wicBitmapSource, ImageSourceLoadingOptions loadingOptions, AlphaMode alphaMode)
		: this(IntPtr.Zero)
	{
		context2.CreateImageSourceFromWic(wicBitmapSource, loadingOptions, alphaMode, this);
	}

	public ImageSourceFromWic(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ImageSourceFromWic(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ImageSourceFromWic(nativePtr);
		}
		return null;
	}

	public unsafe void EnsureCached(RawRectangle? rectangleToFill)
	{
		RawRectangle value = default(RawRectangle);
		if (rectangleToFill.HasValue)
		{
			value = rectangleToFill.Value;
		}
		void* nativePointer = _nativePointer;
		RawRectangle* intPtr = ((!rectangleToFill.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(nativePointer, intPtr)).CheckError();
	}

	public unsafe void TrimCache(RawRectangle? rectangleToPreserve)
	{
		RawRectangle value = default(RawRectangle);
		if (rectangleToPreserve.HasValue)
		{
			value = rectangleToPreserve.Value;
		}
		void* nativePointer = _nativePointer;
		RawRectangle* intPtr = ((!rectangleToPreserve.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(nativePointer, intPtr)).CheckError();
	}

	internal unsafe void GetSource(out BitmapSource wicBitmapSource)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			wicBitmapSource = new BitmapSource(zero);
		}
		else
		{
			wicBitmapSource = null;
		}
	}
}
