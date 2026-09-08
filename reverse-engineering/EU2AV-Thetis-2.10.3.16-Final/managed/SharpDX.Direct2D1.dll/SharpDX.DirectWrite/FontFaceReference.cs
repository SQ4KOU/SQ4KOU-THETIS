using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("5E7FA7CA-DDE3-424C-89F0-9FCD6FED58CD")]
public class FontFaceReference : ComObject
{
	public int FontFaceIndex => GetFontFaceIndex();

	public FontSimulations Simulations => GetSimulations();

	public FontFile FontFile
	{
		get
		{
			GetFontFile(out var fontFile);
			return fontFile;
		}
	}

	public long LocalFileSize => GetLocalFileSize();

	public long FileSize => GetFileSize();

	public long FileTime
	{
		get
		{
			GetFileTime(out var lastWriteTime);
			return lastWriteTime;
		}
	}

	public Locality Locality => GetLocality();

	public FontFaceReference(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontFaceReference(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontFaceReference(nativePtr);
		}
		return null;
	}

	public unsafe void CreateFontFace(out FontFace3 fontFace)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			fontFace = new FontFace3(zero);
		}
		else
		{
			fontFace = null;
		}
		result.CheckError();
	}

	public unsafe void CreateFontFaceWithSimulations(FontSimulations fontFaceSimulationFlags, out FontFace3 fontFace)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (int)fontFaceSimulationFlags, &zero);
		if (zero != IntPtr.Zero)
		{
			fontFace = new FontFace3(zero);
		}
		else
		{
			fontFace = null;
		}
		result.CheckError();
	}

	public unsafe RawBool Equals(FontFaceReference fontFaceReference)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontFaceReference>(fontFaceReference);
		return ((delegate* unmanaged[Stdcall]<void*, void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	internal unsafe int GetFontFaceIndex()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe FontSimulations GetSimulations()
	{
		return ((delegate* unmanaged[Stdcall]<void*, FontSimulations>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetFontFile(out FontFile fontFile)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			fontFile = new FontFile(zero);
		}
		else
		{
			fontFile = null;
		}
		result.CheckError();
	}

	internal unsafe long GetLocalFileSize()
	{
		return ((delegate* unmanaged[Stdcall]<void*, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe long GetFileSize()
	{
		return ((delegate* unmanaged[Stdcall]<void*, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetFileTime(out long lastWriteTime)
	{
		Result result;
		fixed (long* ptr = &lastWriteTime)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe Locality GetLocality()
	{
		return ((delegate* unmanaged[Stdcall]<void*, Locality>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void EnqueueFontDownloadRequest()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public unsafe void EnqueueCharacterDownloadRequest(string characters, int characterCount)
	{
		Result result;
		fixed (char* ptr = characters)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, ptr, characterCount);
		}
		result.CheckError();
	}

	public unsafe void EnqueueGlyphDownloadRequest(short[] glyphIndices, int glyphCount)
	{
		Result result;
		fixed (short* ptr = glyphIndices)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, ptr2, glyphCount);
		}
		result.CheckError();
	}

	public unsafe void EnqueueFileFragmentDownloadRequest(long fileOffset, long fragmentSize)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, long, long, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, fileOffset, fragmentSize)).CheckError();
	}
}
