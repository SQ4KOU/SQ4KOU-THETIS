using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("fd0ecb6b-91e6-411e-8655-395e760f91b4")]
internal class GdiMetafileSink1Native : GdiMetafileSinkNative, GdiMetafileSink1, GdiMetafileSink, IUnknown, ICallbackable, IDisposable
{
	public GdiMetafileSink1Native(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator GdiMetafileSink1Native(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new GdiMetafileSink1Native(nativePtr);
		}
		return null;
	}

	internal unsafe void ProcessRecord_(int recordType, IntPtr recordData, int recordDataSize, int flags)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, recordType, (void*)recordData, recordDataSize, flags)).CheckError();
	}
}
