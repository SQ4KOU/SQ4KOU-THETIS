using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("c78a6519-40d6-4218-b2de-beeeb744bb3e")]
internal class CommandSink4Native : CommandSink3Native, CommandSink4, CommandSink3, CommandSink2, CommandSink1, CommandSink, IUnknown, ICallbackable, IDisposable
{
	public PrimitiveBlend PrimitiveBlend2_
	{
		set
		{
			SetPrimitiveBlend2_(value);
		}
	}

	public void SetPrimitiveBlend2(PrimitiveBlend primitiveBlend)
	{
		SetPrimitiveBlend2_(primitiveBlend);
	}

	public CommandSink4Native(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator CommandSink4Native(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new CommandSink4Native(nativePtr);
		}
		return null;
	}

	internal unsafe void SetPrimitiveBlend2_(PrimitiveBlend primitiveBlend)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)33 * (nint)sizeof(void*))))(_nativePointer, (int)primitiveBlend)).CheckError();
	}
}
