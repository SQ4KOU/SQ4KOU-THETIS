using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("DC102F47-A12D-4B1C-822D-9E117E33043F")]
public class InMemoryFontFileLoader : FontFileLoaderNative
{
	public int FileCount => GetFileCount();

	public InMemoryFontFileLoader(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator InMemoryFontFileLoader(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new InMemoryFontFileLoader(nativePtr);
		}
		return null;
	}

	public unsafe void CreateInMemoryFontFileReference(Factory factory, IntPtr fontData, int fontDataSize, IUnknown ownerObject, out FontFile fontFile)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Factory>(factory);
		zero2 = CppObject.ToCallbackPtr<IUnknown>(ownerObject);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)fontData, fontDataSize, (void*)zero2, &zero3);
		if (zero3 != IntPtr.Zero)
		{
			fontFile = new FontFile(zero3);
		}
		else
		{
			fontFile = null;
		}
		result.CheckError();
	}

	internal unsafe int GetFileCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer);
	}
}
