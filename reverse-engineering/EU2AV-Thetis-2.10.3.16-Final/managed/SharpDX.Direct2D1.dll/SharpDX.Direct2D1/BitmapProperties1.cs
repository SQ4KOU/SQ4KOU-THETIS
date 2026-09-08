using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

public class BitmapProperties1
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public PixelFormat PixelFormat;

		public float DpiX;

		public float DpiY;

		public BitmapOptions BitmapOptions;

		public IntPtr ColorContext;
	}

	private ColorContext colorContext;

	public PixelFormat PixelFormat;

	public float DpiX;

	public float DpiY;

	public BitmapOptions BitmapOptions;

	public ColorContext ColorContext;

	public BitmapProperties1()
	{
	}

	public BitmapProperties1(PixelFormat pixelFormat)
		: this(pixelFormat, 96f, 96f)
	{
	}

	public BitmapProperties1(PixelFormat pixelFormat, float dpiX, float dpiY)
		: this(pixelFormat, dpiX, dpiY, BitmapOptions.None)
	{
	}

	public BitmapProperties1(PixelFormat pixelFormat, float dpiX, float dpiY, BitmapOptions bitmapOptions)
		: this(pixelFormat, dpiX, dpiY, bitmapOptions, null)
	{
	}

	public BitmapProperties1(PixelFormat pixelFormat, float dpiX, float dpiY, BitmapOptions bitmapOptions, ColorContext colorContext)
	{
		PixelFormat = pixelFormat;
		DpiX = dpiX;
		DpiY = dpiY;
		BitmapOptions = bitmapOptions;
		ColorContext = colorContext;
	}

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		PixelFormat = @ref.PixelFormat;
		DpiX = @ref.DpiX;
		DpiY = @ref.DpiY;
		BitmapOptions = @ref.BitmapOptions;
		if (@ref.ColorContext != IntPtr.Zero)
		{
			ColorContext = new ColorContext(@ref.ColorContext);
		}
		else
		{
			ColorContext = null;
		}
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.PixelFormat = PixelFormat;
		@ref.DpiX = DpiX;
		@ref.DpiY = DpiY;
		@ref.BitmapOptions = BitmapOptions;
		@ref.ColorContext = CppObject.ToCallbackPtr<ColorContext>(ColorContext);
	}
}
