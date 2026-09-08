using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("CE25F8FD-863B-4D13-9651-C1F88DC73FE2")]
public class AsyncResult : ComObject
{
	public IntPtr WaitHandle => GetWaitHandle();

	public Result Result => GetResult();

	public AsyncResult(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator AsyncResult(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new AsyncResult(nativePtr);
		}
		return null;
	}

	internal unsafe IntPtr GetWaitHandle()
	{
		return ((delegate* unmanaged[Stdcall]<void*, IntPtr>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe Result GetResult()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}
}
