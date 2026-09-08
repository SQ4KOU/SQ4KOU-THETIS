using System;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX.IO;
using SharpDX.Win32;

namespace SharpDX.WIC;

[Guid("135FF860-22B7-4ddf-B0F6-218F4F299A43")]
public class WICStream : ComStream
{
	private ComStreamProxy streamProxy;

	public WICStream(ImagingFactory factory, string fileName, NativeFileAccess fileAccess)
		: base(IntPtr.Zero)
	{
		factory.CreateStream(this);
		InitializeFromFilename(fileName, (int)fileAccess);
	}

	public WICStream(ImagingFactory factory, Stream stream)
		: base(IntPtr.Zero)
	{
		factory.CreateStream(this);
		streamProxy = new ComStreamProxy(stream);
		InitializeFromIStream(streamProxy);
	}

	public WICStream(ImagingFactory factory, DataPointer dataStream)
		: base(IntPtr.Zero)
	{
		factory.CreateStream(this);
		InitializeFromMemory(dataStream.Pointer, dataStream.Size);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (streamProxy != null)
		{
			streamProxy.Dispose();
			streamProxy = null;
		}
	}

	public WICStream(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator WICStream(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new WICStream(nativePtr);
		}
		return null;
	}

	internal unsafe void InitializeFromIStream(IStream streamRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IStream>(streamRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void InitializeFromFilename(string fileName, int desiredAccess)
	{
		Result result;
		fixed (char* ptr = fileName)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, ptr, desiredAccess);
		}
		result.CheckError();
	}

	internal unsafe void InitializeFromMemory(IntPtr bufferRef, int bufferSize)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, (void*)bufferRef, bufferSize)).CheckError();
	}

	internal unsafe void InitializeFromIStreamRegion(IStream streamRef, long ulOffset, long ulMaxSize)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IStream>(streamRef);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, long, long, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ulOffset, ulMaxSize)).CheckError();
	}
}
