using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("958DB99A-BE2A-4F09-AF7D-65189803D1D3")]
public class Factory5 : Factory4
{
	public Factory5(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory5(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory5(nativePtr);
		}
		return null;
	}

	public unsafe void CreateFontSetBuilder(out FontSetBuilder1 fontSetBuilder)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)43 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			fontSetBuilder = new FontSetBuilder1(zero);
		}
		else
		{
			fontSetBuilder = null;
		}
		result.CheckError();
	}

	public unsafe void CreateInMemoryFontFileLoader(out InMemoryFontFileLoader newLoader)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)44 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			newLoader = new InMemoryFontFileLoader(zero);
		}
		else
		{
			newLoader = null;
		}
		result.CheckError();
	}

	public unsafe void CreateHttpFontFileLoader(string referrerUrl, string extraHeaders, out RemoteFontFileLoader newLoader)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (char* ptr = extraHeaders)
		{
			fixed (char* ptr2 = referrerUrl)
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)45 * (nint)sizeof(void*))))(_nativePointer, ptr2, ptr, &zero);
			}
		}
		if (zero != IntPtr.Zero)
		{
			newLoader = new RemoteFontFileLoader(zero);
		}
		else
		{
			newLoader = null;
		}
		result.CheckError();
	}

	public unsafe ContainerType AnalyzeContainerType(IntPtr fileData, int fileDataSize)
	{
		return ((delegate* unmanaged[Stdcall]<void*, void*, int, ContainerType>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)46 * (nint)sizeof(void*))))(_nativePointer, (void*)fileData, fileDataSize);
	}

	public unsafe void UnpackFontFile(ContainerType containerType, IntPtr fileData, int fileDataSize, out FontFileStream unpackedFontStream)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)47 * (nint)sizeof(void*))))(_nativePointer, (int)containerType, (void*)fileData, fileDataSize, &zero);
		if (zero != IntPtr.Zero)
		{
			unpackedFontStream = new FontFileStreamNative(zero);
		}
		else
		{
			unpackedFontStream = null;
		}
		result.CheckError();
	}
}
