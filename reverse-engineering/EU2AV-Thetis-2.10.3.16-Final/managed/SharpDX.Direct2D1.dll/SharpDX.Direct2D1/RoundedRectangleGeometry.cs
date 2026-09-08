using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906a3-12e2-11dc-9fed-001143a055f9")]
public class RoundedRectangleGeometry : Geometry
{
	public RoundedRectangle RoundedRect
	{
		get
		{
			GetRoundedRect(out var roundedRect);
			return roundedRect;
		}
	}

	public RoundedRectangleGeometry(Factory factory, RoundedRectangle roundedRectangle)
		: base(IntPtr.Zero)
	{
		factory.CreateRoundedRectangleGeometry(ref roundedRectangle, this);
	}

	public RoundedRectangleGeometry(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RoundedRectangleGeometry(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RoundedRectangleGeometry(nativePtr);
		}
		return null;
	}

	internal unsafe void GetRoundedRect(out RoundedRectangle roundedRect)
	{
		roundedRect = default(RoundedRectangle);
		fixed (RoundedRectangle* ptr = &roundedRect)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
