using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("e0db51c3-6f77-4bae-b3d5-e47509b35838")]
public class GdiInteropRenderTarget : ComObject
{
	public void ReleaseDC()
	{
		ReleaseDC(null);
	}

	public GdiInteropRenderTarget(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GdiInteropRenderTarget(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new GdiInteropRenderTarget(nativePtr);
		}
		return null;
	}

	public unsafe IntPtr GetDC(DeviceContextInitializeMode mode)
	{
		IntPtr result = default(IntPtr);
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (int)mode, &result)).CheckError();
		return result;
	}

	public unsafe void ReleaseDC(RawRectangle? update)
	{
		RawRectangle value = default(RawRectangle);
		if (update.HasValue)
		{
			value = update.Value;
		}
		void* nativePointer = _nativePointer;
		RawRectangle* intPtr = ((!update.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(nativePointer, intPtr)).CheckError();
	}
}
