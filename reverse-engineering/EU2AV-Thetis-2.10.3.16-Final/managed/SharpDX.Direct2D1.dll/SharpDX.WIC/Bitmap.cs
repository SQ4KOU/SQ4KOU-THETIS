using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.WIC;

[Guid("00000121-a8f2-4877-ba0a-fd2b6645fb94")]
public class Bitmap : BitmapSource
{
	public Palette Palette
	{
		set
		{
			SetPalette(value);
		}
	}

	public Bitmap(ImagingFactory factory, int width, int height, Guid pixelFormat, BitmapCreateCacheOption option)
		: base(IntPtr.Zero)
	{
		factory.CreateBitmap(width, height, pixelFormat, option, this);
	}

	public Bitmap(ImagingFactory factory, int width, int height, Guid pixelFormat, DataRectangle dataRectangle, int totalSizeInBytes = 0)
		: base(IntPtr.Zero)
	{
		if (totalSizeInBytes == 0)
		{
			totalSizeInBytes = height * dataRectangle.Pitch;
		}
		factory.CreateBitmapFromMemory(width, height, pixelFormat, dataRectangle.Pitch, totalSizeInBytes, dataRectangle.DataPointer, this);
	}

	public Bitmap(ImagingFactory factory, BitmapSource bitmapSource, BitmapCreateCacheOption option)
		: base(IntPtr.Zero)
	{
		factory.CreateBitmapFromSource(bitmapSource, option, this);
	}

	public Bitmap(ImagingFactory factory, BitmapSource bitmapSource, RawBox rectangle)
		: base(IntPtr.Zero)
	{
		factory.CreateBitmapFromSourceRect(bitmapSource, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, this);
	}

	public unsafe static Bitmap New<T>(ImagingFactory factory, int width, int height, Guid pixelFormat, T[] pixelDatas, int stride = 0) where T : struct
	{
		if (stride == 0)
		{
			stride = width * Utilities.SizeOf<T>();
		}
		fixed (T* ptr = &pixelDatas[0])
		{
			return new Bitmap(factory, width, height, pixelFormat, new DataRectangle((IntPtr)ptr, stride));
		}
	}

	public BitmapLock Lock(BitmapLockFlags flags)
	{
		return Lock(IntPtr.Zero, flags);
	}

	public unsafe BitmapLock Lock(RawBox rcLockRef, BitmapLockFlags flags)
	{
		return Lock(new IntPtr(&rcLockRef), flags);
	}

	public Bitmap(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Bitmap(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Bitmap(nativePtr);
		}
		return null;
	}

	internal unsafe BitmapLock Lock(IntPtr rcLockRef, BitmapLockFlags flags)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)rcLockRef, (int)flags, &zero);
		BitmapLock result2 = ((!(zero != IntPtr.Zero)) ? null : new BitmapLock(zero));
		result.CheckError();
		return result2;
	}

	internal unsafe void SetPalette(Palette paletteRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Palette>(paletteRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void SetResolution(double dpiX, double dpiY)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, double, double, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, dpiX, dpiY)).CheckError();
	}
}
