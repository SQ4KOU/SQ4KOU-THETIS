using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("2cd906a2-12e2-11dc-9fed-001143a055f9")]
public class RectangleGeometry : Geometry
{
	public RawRectangleF Rectangle
	{
		get
		{
			GetRectangle(out var rect);
			return rect;
		}
	}

	public RectangleGeometry(Factory factory, RawRectangleF rectangle)
		: base(IntPtr.Zero)
	{
		factory.CreateRectangleGeometry(rectangle, this);
	}

	public RectangleGeometry(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RectangleGeometry(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RectangleGeometry(nativePtr);
		}
		return null;
	}

	internal unsafe void GetRectangle(out RawRectangleF rect)
	{
		rect = default(RawRectangleF);
		fixed (RawRectangleF* ptr = &rect)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
