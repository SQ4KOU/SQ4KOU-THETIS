using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("B71E6052-5AEA-4FA3-832E-F60D431F7E91")]
public class FontDownloadQueue : ComObject
{
	public RawBool IsEmpty => IsEmpty_();

	public long GenerationCount => GetGenerationCount();

	public FontDownloadQueue(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontDownloadQueue(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontDownloadQueue(nativePtr);
		}
		return null;
	}

	public unsafe void AddListener(FontDownloadListener listener, out int token)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontDownloadListener>(listener);
		Result result;
		fixed (int* ptr = &token)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2);
		}
		result.CheckError();
	}

	public unsafe void RemoveListener(int token)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, token)).CheckError();
	}

	internal unsafe RawBool IsEmpty_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void BeginDownload(IUnknown context)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IUnknown>(context);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void CancelDownload()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	internal unsafe long GetGenerationCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer);
	}
}
