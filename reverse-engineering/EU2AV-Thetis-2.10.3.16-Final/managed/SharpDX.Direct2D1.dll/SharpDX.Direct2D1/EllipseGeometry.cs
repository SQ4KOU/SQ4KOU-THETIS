using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906a4-12e2-11dc-9fed-001143a055f9")]
public class EllipseGeometry : Geometry
{
	public Ellipse Ellipse
	{
		get
		{
			GetEllipse(out var ellipse);
			return ellipse;
		}
	}

	public EllipseGeometry(Factory factory, Ellipse ellipse)
		: base(IntPtr.Zero)
	{
		factory.CreateEllipseGeometry(ellipse, this);
	}

	public EllipseGeometry(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator EllipseGeometry(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new EllipseGeometry(nativePtr);
		}
		return null;
	}

	internal unsafe void GetEllipse(out Ellipse ellipse)
	{
		ellipse = default(Ellipse);
		fixed (Ellipse* ptr = &ellipse)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
