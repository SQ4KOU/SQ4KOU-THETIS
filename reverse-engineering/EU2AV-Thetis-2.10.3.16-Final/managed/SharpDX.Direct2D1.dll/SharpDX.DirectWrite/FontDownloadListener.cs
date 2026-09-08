using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("B06FE5B9-43EC-4393-881B-DBE4DC72FDA7")]
public class FontDownloadListener : ComObject
{
	public FontDownloadListener(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontDownloadListener(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontDownloadListener(nativePtr);
		}
		return null;
	}

	public unsafe void DownloadCompleted(FontDownloadQueue downloadQueue, IUnknown context, Result downloadResult)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontDownloadQueue>(downloadQueue);
		zero2 = CppObject.ToCallbackPtr<IUnknown>(context);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, Result, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, downloadResult);
	}
}
