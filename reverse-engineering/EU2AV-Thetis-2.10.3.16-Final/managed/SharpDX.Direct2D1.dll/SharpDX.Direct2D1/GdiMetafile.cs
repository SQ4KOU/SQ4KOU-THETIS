using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("2f543dc3-cfc1-4211-864f-cfd91c6f3395")]
public class GdiMetafile : Resource
{
	public RawRectangleF Bounds
	{
		get
		{
			GetBounds(out var bounds);
			return bounds;
		}
	}

	public GdiMetafile(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GdiMetafile(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new GdiMetafile(nativePtr);
		}
		return null;
	}

	public unsafe void Stream(GdiMetafileSink sink)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<GdiMetafileSink>(sink);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void GetBounds(out RawRectangleF bounds)
	{
		bounds = default(RawRectangleF);
		Result result;
		fixed (RawRectangleF* ptr = &bounds)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
