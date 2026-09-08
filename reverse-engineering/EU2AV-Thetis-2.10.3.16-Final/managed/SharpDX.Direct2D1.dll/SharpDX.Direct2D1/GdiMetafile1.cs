using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("2e69f9e8-dd3f-4bf9-95ba-c04f49d788df")]
public class GdiMetafile1 : GdiMetafile
{
	public RawRectangleF SourceBounds
	{
		get
		{
			GetSourceBounds(out var bounds);
			return bounds;
		}
	}

	public GdiMetafile1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GdiMetafile1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new GdiMetafile1(nativePtr);
		}
		return null;
	}

	public unsafe void GetDpi(out float dpiX, out float dpiY)
	{
		Result result;
		fixed (float* ptr = &dpiY)
		{
			void* ptr2 = ptr;
			fixed (float* ptr3 = &dpiX)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void GetSourceBounds(out RawRectangleF bounds)
	{
		bounds = default(RawRectangleF);
		Result result;
		fixed (RawRectangleF* ptr = &bounds)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
