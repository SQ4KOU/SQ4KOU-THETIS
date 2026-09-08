using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("41343a53-e41a-49a2-91cd-21793bbb62e5")]
public class BitmapBrush1 : BitmapBrush
{
	public InterpolationMode InterpolationMode1
	{
		get
		{
			return GetInterpolationMode1();
		}
		set
		{
			SetInterpolationMode1(value);
		}
	}

	public BitmapBrush1(DeviceContext deviceContext, Bitmap1 bitmap)
		: this(deviceContext, bitmap, null, null)
	{
	}

	public BitmapBrush1(DeviceContext deviceContext, Bitmap1 bitmap, BitmapBrushProperties1 bitmapBrushProperties)
		: this(deviceContext, bitmap, bitmapBrushProperties, null)
	{
	}

	public BitmapBrush1(DeviceContext deviceContext, Bitmap1 bitmap, BrushProperties brushProperties)
		: this(deviceContext, bitmap, null, brushProperties)
	{
	}

	public BitmapBrush1(DeviceContext deviceContext, Bitmap1 bitmap, BitmapBrushProperties1? bitmapBrushProperties, BrushProperties? brushProperties)
		: base(IntPtr.Zero)
	{
		deviceContext.CreateBitmapBrush(bitmap, bitmapBrushProperties, brushProperties, this);
	}

	public BitmapBrush1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapBrush1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BitmapBrush1(nativePtr);
		}
		return null;
	}

	internal unsafe void SetInterpolationMode1(InterpolationMode interpolationMode)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, (int)interpolationMode);
	}

	internal unsafe InterpolationMode GetInterpolationMode1()
	{
		return ((delegate* unmanaged[Stdcall]<void*, InterpolationMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer);
	}
}
