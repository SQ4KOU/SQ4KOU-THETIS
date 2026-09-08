using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("b4f34a19-2383-4d76-94f6-ec343657c3dc")]
public class CommandList : Image
{
	public CommandList(DeviceContext deviceContext)
		: base(IntPtr.Zero)
	{
		deviceContext.CreateCommandList(this);
	}

	public CommandList(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator CommandList(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new CommandList(nativePtr);
		}
		return null;
	}

	public unsafe void Stream(CommandSink sink)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<CommandSink>(sink);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void Close()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}
}
