using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("31e6e7bc-e0ff-4d46-8c64-a0a8c41c15d3")]
public class Multithread : ComObject
{
	public RawBool MultithreadProtected => GetMultithreadProtected();

	public Multithread(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Multithread(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Multithread(nativePtr);
		}
		return null;
	}

	internal unsafe RawBool GetMultithreadProtected()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void Enter()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void Leave()
	{
		((delegate* unmanaged[Stdcall]<void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer);
	}
}
