using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("1a799d8a-69f7-4e4c-9fed-437ccc6684cc")]
public class ConcreteTransform : TransformNodeNative
{
	public RawBool Cached
	{
		set
		{
			SetCached(value);
		}
	}

	public ConcreteTransform(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ConcreteTransform(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ConcreteTransform(nativePtr);
		}
		return null;
	}

	public unsafe void SetOutputBuffer(BufferPrecision bufferPrecision, ChannelDepth channelDepth)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (int)bufferPrecision, (int)channelDepth)).CheckError();
	}

	internal unsafe void SetCached(RawBool isCached)
	{
		((delegate* unmanaged[Stdcall]<void*, RawBool, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, isCached);
	}
}
