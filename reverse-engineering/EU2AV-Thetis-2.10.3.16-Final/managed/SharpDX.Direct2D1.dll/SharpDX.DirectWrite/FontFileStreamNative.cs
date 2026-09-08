using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("6d4865fe-0ab8-4d91-8f62-5dd6be34a3e0")]
public class FontFileStreamNative : ComObject, FontFileStream, IUnknown, ICallbackable, IDisposable
{
	public long FileSize_
	{
		get
		{
			GetFileSize_(out var fileSize);
			return fileSize;
		}
	}

	public long LastWriteTime_
	{
		get
		{
			GetLastWriteTime_(out var lastWriteTime);
			return lastWriteTime;
		}
	}

	public void ReadFileFragment(out IntPtr fragmentStart, long fileOffset, long fragmentSize, out IntPtr fragmentContext)
	{
		ReadFileFragment_(out fragmentStart, fileOffset, fragmentSize, out fragmentContext);
	}

	public void ReleaseFileFragment(IntPtr fragmentContext)
	{
		ReleaseFileFragment_(fragmentContext);
	}

	public long GetFileSize()
	{
		GetFileSize_(out var fileSize);
		return fileSize;
	}

	public long GetLastWriteTime()
	{
		GetLastWriteTime_(out var lastWriteTime);
		return lastWriteTime;
	}

	public FontFileStreamNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontFileStreamNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontFileStreamNative(nativePtr);
		}
		return null;
	}

	internal unsafe void ReadFileFragment_(out IntPtr fragmentStart, long fileOffset, long fragmentSize, out IntPtr fragmentContext)
	{
		Result result;
		fixed (IntPtr* ptr = &fragmentContext)
		{
			void* ptr2 = ptr;
			fixed (IntPtr* ptr3 = &fragmentStart)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, long, long, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr4, fileOffset, fragmentSize, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void ReleaseFileFragment_(IntPtr fragmentContext)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)fragmentContext);
	}

	internal unsafe void GetFileSize_(out long fileSize)
	{
		Result result;
		fixed (long* ptr = &fileSize)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetLastWriteTime_(out long lastWriteTime)
	{
		Result result;
		fixed (long* ptr = &lastWriteTime)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
