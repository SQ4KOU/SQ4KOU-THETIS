using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("4DB3757A-2C72-4ED9-B2B6-1ABABE1AFF9C")]
public class RemoteFontFileStream : FontFileStreamNative
{
	public long LocalFileSize
	{
		get
		{
			GetLocalFileSize(out var localFileSize);
			return localFileSize;
		}
	}

	public Locality Locality => GetLocality();

	public RemoteFontFileStream(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RemoteFontFileStream(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RemoteFontFileStream(nativePtr);
		}
		return null;
	}

	internal unsafe void GetLocalFileSize(out long localFileSize)
	{
		Result result;
		fixed (long* ptr = &localFileSize)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetFileFragmentLocality(long fileOffset, long fragmentSize, out RawBool isLocal, long artialSizeRef)
	{
		isLocal = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &isLocal)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, long, long, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, fileOffset, fragmentSize, ptr2, &artialSizeRef);
		}
		result.CheckError();
	}

	internal unsafe Locality GetLocality()
	{
		return ((delegate* unmanaged[Stdcall]<void*, Locality>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe AsyncResult BeginDownload(Guid downloadOperationID, FileFragment[] fileFragments, int fragmentCount)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (FileFragment* ptr = fileFragments)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, &downloadOperationID, ptr2, fragmentCount, &zero);
		}
		AsyncResult result2 = ((!(zero != IntPtr.Zero)) ? null : new AsyncResult(zero));
		result.CheckError();
		return result2;
	}
}
