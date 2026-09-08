using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("CFEE3140-1157-47CA-8B85-31BFCF3F2D0E")]
public class StringList : ComObject
{
	public int Count => GetCount();

	public StringList(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator StringList(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new StringList(nativePtr);
		}
		return null;
	}

	internal unsafe int GetCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void GetLocaleNameLength(int listIndex, out int length)
	{
		Result result;
		fixed (int* ptr = &length)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, listIndex, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetLocaleName(int listIndex, IntPtr localeName, int size)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, listIndex, (void*)localeName, size)).CheckError();
	}

	public unsafe void GetStringLength(int listIndex, out int length)
	{
		Result result;
		fixed (int* ptr = &length)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, listIndex, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetString(int listIndex, IntPtr stringBuffer, int stringBufferSize)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, listIndex, (void*)stringBuffer, stringBufferSize)).CheckError();
	}
}
