using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

internal static class DWrite
{
	public unsafe static void CreateFactory(FactoryType factoryType, Guid iid, ComObject factory)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = DWriteCreateFactory_((int)factoryType, &iid, &zero);
		factory.NativePointer = zero;
		result.CheckError();
	}

	[DllImport("dwrite.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "DWriteCreateFactory")]
	private unsafe static extern int DWriteCreateFactory_(int param0, void* param1, void* param2);
}
