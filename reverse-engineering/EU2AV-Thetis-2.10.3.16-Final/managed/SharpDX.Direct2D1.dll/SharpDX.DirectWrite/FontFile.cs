using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("739d886a-cef5-47dc-8769-1a8b41bebbb0")]
public class FontFile : ComObject
{
	private FontFileLoaderShadow fontLoaderShadow;

	public FontFileLoader Loader
	{
		get
		{
			if (fontLoaderShadow != null)
			{
				return (FontFileLoader)fontLoaderShadow.Callback;
			}
			GetLoader(out var fontFileLoader);
			return fontFileLoader;
		}
	}

	public FontFile(Factory factory, string filePath)
		: this(factory, filePath, null)
	{
	}

	public FontFile(Factory factory, string filePath, long? lastWriteTime)
	{
		factory.CreateFontFileReference(filePath, lastWriteTime, this);
	}

	public FontFile(Factory factory, IntPtr fontFileReferenceKey, int fontFileReferenceKeySize, FontFileLoader fontFileLoader)
	{
		factory.CreateCustomFontFileReference(fontFileReferenceKey, fontFileReferenceKeySize, fontFileLoader, this);
	}

	public unsafe DataPointer GetReferenceKey()
	{
		IntPtr pointer = default(IntPtr);
		GetReferenceKey(new IntPtr(&pointer), out var fontFileReferenceKeySize);
		return new DataPointer(pointer, fontFileReferenceKeySize);
	}

	public FontFile(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator FontFile(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new FontFile(nativePtr);
		}
		return null;
	}

	internal unsafe void GetReferenceKey(IntPtr fontFileReferenceKey, out int fontFileReferenceKeySize)
	{
		Result result;
		fixed (int* ptr = &fontFileReferenceKeySize)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)fontFileReferenceKey, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetLoader(out FontFileLoader fontFileLoader)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			fontFileLoader = new FontFileLoaderNative(zero);
		}
		else
		{
			fontFileLoader = null;
		}
		result.CheckError();
	}

	public unsafe void Analyze(out RawBool isSupportedFontType, out FontFileType fontFileType, out FontFaceType fontFaceType, out int numberOfFaces)
	{
		isSupportedFontType = default(RawBool);
		Result result;
		fixed (int* ptr = &numberOfFaces)
		{
			void* ptr2 = ptr;
			fixed (FontFaceType* ptr3 = &fontFaceType)
			{
				void* ptr4 = ptr3;
				fixed (FontFileType* ptr5 = &fontFileType)
				{
					void* ptr6 = ptr5;
					fixed (RawBool* ptr7 = &isSupportedFontType)
					{
						void* ptr8 = ptr7;
						result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr8, ptr6, ptr4, ptr2);
					}
				}
			}
		}
		result.CheckError();
	}
}
