using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.WIC;

namespace SharpDX.Direct2D1;

[Guid("a898a84c-3873-4588-b08b-ebbf978df041")]
public class Bitmap1 : Bitmap
{
	public ColorContext ColorContext
	{
		get
		{
			GetColorContext(out var colorContext);
			return colorContext;
		}
	}

	public BitmapOptions Options => GetOptions();

	public Surface Surface
	{
		get
		{
			GetSurface(out var dxgiSurface);
			return dxgiSurface;
		}
	}

	public Bitmap1(DeviceContext deviceContext, Size2 size)
		: this(deviceContext, size, null, 0, new BitmapProperties1(new PixelFormat(Format.Unknown, AlphaMode.Unknown)))
	{
	}

	public Bitmap1(DeviceContext deviceContext, Size2 size, BitmapProperties1 bitmapProperties)
		: this(deviceContext, size, null, 0, bitmapProperties)
	{
	}

	public Bitmap1(DeviceContext deviceContext, Size2 size, DataStream dataStream, int pitch)
		: this(deviceContext, size, dataStream, pitch, new BitmapProperties1(new PixelFormat(Format.Unknown, AlphaMode.Unknown)))
	{
	}

	public Bitmap1(DeviceContext deviceContext, Size2 size, DataStream dataStream, int pitch, BitmapProperties1 bitmapProperties)
		: base(IntPtr.Zero)
	{
		deviceContext.CreateBitmap(size, dataStream?.PositionPointer ?? IntPtr.Zero, pitch, bitmapProperties, this);
	}

	public Bitmap1(DeviceContext deviceContext, Surface surface)
		: base(IntPtr.Zero)
	{
		deviceContext.CreateBitmapFromDxgiSurface(surface, null, this);
	}

	public Bitmap1(DeviceContext deviceContext, Surface surface, BitmapProperties1 bitmapProperties)
		: base(IntPtr.Zero)
	{
		deviceContext.CreateBitmapFromDxgiSurface(surface, bitmapProperties, this);
	}

	public static Bitmap1 FromWicBitmap(DeviceContext deviceContext, BitmapSource wicBitmapSource)
	{
		deviceContext.CreateBitmapFromWicBitmap(wicBitmapSource, null, out var bitmap);
		return bitmap;
	}

	public static Bitmap1 FromWicBitmap(DeviceContext deviceContext, BitmapSource wicBitmap, BitmapProperties1 bitmapProperties)
	{
		deviceContext.CreateBitmapFromWicBitmap(wicBitmap, bitmapProperties, out var bitmap);
		return bitmap;
	}

	public DataRectangle Map(MapOptions options)
	{
		Map(options, out var mappedRect);
		return new DataRectangle(mappedRect.Bits, mappedRect.Pitch);
	}

	public Bitmap1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Bitmap1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Bitmap1(nativePtr);
		}
		return null;
	}

	internal unsafe void GetColorContext(out ColorContext colorContext)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			colorContext = new ColorContext(zero);
		}
		else
		{
			colorContext = null;
		}
	}

	internal unsafe BitmapOptions GetOptions()
	{
		return ((delegate* unmanaged[Stdcall]<void*, BitmapOptions>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetSurface(out Surface dxgiSurface)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			dxgiSurface = new Surface(zero);
		}
		else
		{
			dxgiSurface = null;
		}
		result.CheckError();
	}

	internal unsafe void Map(MapOptions options, out MappedRectangle mappedRect)
	{
		mappedRect = default(MappedRectangle);
		Result result;
		fixed (MappedRectangle* ptr = &mappedRect)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, (int)options, ptr2);
		}
		result.CheckError();
	}

	public unsafe void Unmap()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}
}
