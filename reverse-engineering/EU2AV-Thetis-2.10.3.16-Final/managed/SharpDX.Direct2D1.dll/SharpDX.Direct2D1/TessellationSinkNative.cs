using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906c1-12e2-11dc-9fed-001143a055f9")]
internal class TessellationSinkNative : ComObject, TessellationSink, IUnknown, ICallbackable, IDisposable
{
	public void AddTriangles(Triangle[] triangles)
	{
		AddTriangles_(triangles, triangles.Length);
	}

	public void Close()
	{
		Close_();
	}

	public TessellationSinkNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TessellationSinkNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new TessellationSinkNative(nativePtr);
		}
		return null;
	}

	internal unsafe void AddTriangles_(Triangle[] triangles, int trianglesCount)
	{
		fixed (Triangle* ptr = triangles)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr2, trianglesCount);
		}
	}

	internal unsafe void Close_()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}
}
