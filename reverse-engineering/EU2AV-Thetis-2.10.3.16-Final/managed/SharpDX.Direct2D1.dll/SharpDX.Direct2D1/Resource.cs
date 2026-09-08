using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd90691-12e2-11dc-9fed-001143a055f9")]
public class Resource : ComObject
{
	public Factory Factory
	{
		get
		{
			GetFactory(out var factory);
			return factory;
		}
	}

	public Resource(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Resource(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Resource(nativePtr);
		}
		return null;
	}

	internal unsafe void GetFactory(out Factory factory)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			factory = new Factory(zero);
		}
		else
		{
			factory = null;
		}
	}
}
