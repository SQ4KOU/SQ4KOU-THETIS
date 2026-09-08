using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("9eb767fd-4269-4467-b8c2-eb30cb305743")]
internal class CommandSink1Native : CommandSinkNative, CommandSink1, CommandSink, IUnknown, ICallbackable, IDisposable
{
	public PrimitiveBlend PrimitiveBlend1
	{
		set
		{
			SetPrimitiveBlend1_(value);
		}
	}

	public PrimitiveBlend PrimitiveBlend1_
	{
		set
		{
			SetPrimitiveBlend1_(value);
		}
	}

	public CommandSink1Native(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator CommandSink1Native(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new CommandSink1Native(nativePtr);
		}
		return null;
	}

	internal unsafe void SetPrimitiveBlend1_(PrimitiveBlend primitiveBlend)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, (int)primitiveBlend)).CheckError();
	}
}
