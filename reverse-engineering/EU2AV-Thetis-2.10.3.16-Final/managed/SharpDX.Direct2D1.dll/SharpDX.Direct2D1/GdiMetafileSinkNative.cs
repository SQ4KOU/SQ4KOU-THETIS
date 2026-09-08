using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("82237326-8111-4f7c-bcf4-b5c1175564fe")]
internal class GdiMetafileSinkNative : ComObject, GdiMetafileSink, IUnknown, ICallbackable, IDisposable
{
	public GdiMetafileSinkNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GdiMetafileSinkNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new GdiMetafileSinkNative(nativePtr);
		}
		return null;
	}

	internal unsafe void ProcessRecord_(int recordType, IntPtr recordData, int recordDataSize)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, recordType, (void*)recordData, recordDataSize)).CheckError();
	}
}
