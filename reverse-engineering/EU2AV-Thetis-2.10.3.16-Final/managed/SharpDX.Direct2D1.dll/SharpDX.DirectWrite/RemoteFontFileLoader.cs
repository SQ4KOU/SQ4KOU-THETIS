using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("68648C83-6EDE-46C0-AB46-20083A887FDE")]
public class RemoteFontFileLoader : FontFileLoaderNative
{
	public RemoteFontFileLoader(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RemoteFontFileLoader(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RemoteFontFileLoader(nativePtr);
		}
		return null;
	}

	public unsafe void CreateRemoteStreamFromKey(IntPtr fontFileReferenceKey, int fontFileReferenceKeySize, out RemoteFontFileStream fontFileStream)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)fontFileReferenceKey, fontFileReferenceKeySize, &zero);
		if (zero != IntPtr.Zero)
		{
			fontFileStream = new RemoteFontFileStream(zero);
		}
		else
		{
			fontFileStream = null;
		}
		result.CheckError();
	}

	public unsafe void GetLocalityFromKey(IntPtr fontFileReferenceKey, int fontFileReferenceKeySize, out Locality locality)
	{
		Result result;
		fixed (Locality* ptr = &locality)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)fontFileReferenceKey, fontFileReferenceKeySize, ptr2);
		}
		result.CheckError();
	}

	public unsafe void CreateFontFileReferenceFromUrl(Factory factory, string baseUrl, string fontFileUrl, out FontFile fontFile)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Factory>(factory);
		Result result;
		fixed (char* ptr = fontFileUrl)
		{
			fixed (char* ptr2 = baseUrl)
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2, ptr, &zero2);
			}
		}
		if (zero2 != IntPtr.Zero)
		{
			fontFile = new FontFile(zero2);
		}
		else
		{
			fontFile = null;
		}
		result.CheckError();
	}
}
