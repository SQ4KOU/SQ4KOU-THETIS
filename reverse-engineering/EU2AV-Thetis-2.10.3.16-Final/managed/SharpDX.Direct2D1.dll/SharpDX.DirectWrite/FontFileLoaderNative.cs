using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("727cad4e-d6af-4c9e-8a08-d695b11caa49")]
public class FontFileLoaderNative : ComObject, FontFileLoader, IUnknown, ICallbackable, IDisposable
{
	public FontFileStream CreateStreamFromKey(DataPointer fontFileReferenceKey)
	{
		CreateStreamFromKey_(fontFileReferenceKey.Pointer, fontFileReferenceKey.Size, out var fontFileStream);
		return fontFileStream;
	}

	public FontFileLoaderNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontFileLoaderNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontFileLoaderNative(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateStreamFromKey_(IntPtr fontFileReferenceKey, int fontFileReferenceKeySize, out FontFileStream fontFileStream)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)fontFileReferenceKey, fontFileReferenceKeySize, &zero);
		if (zero != IntPtr.Zero)
		{
			fontFileStream = new FontFileStreamNative(zero);
		}
		else
		{
			fontFileStream = null;
		}
		result.CheckError();
	}
}
