using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("00000123-a8f2-4877-ba0a-fd2b6645fb94")]
public class BitmapLock : ComObject
{
	public Size2 Size
	{
		get
		{
			GetSize(out var widthRef, out var heightRef);
			return new Size2(widthRef, heightRef);
		}
	}

	public DataRectangle Data
	{
		get
		{
			int bufferSizeRef;
			return new DataRectangle(GetDataPointer(out bufferSizeRef), Stride);
		}
	}

	public int Stride
	{
		get
		{
			GetStride(out var strideRef);
			return strideRef;
		}
	}

	public Guid PixelFormat
	{
		get
		{
			GetPixelFormat(out var pixelFormatRef);
			return pixelFormatRef;
		}
	}

	public BitmapLock(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator BitmapLock(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new BitmapLock(nativePtr);
		}
		return null;
	}

	internal unsafe void GetSize(out int widthRef, out int heightRef)
	{
		Result result;
		fixed (int* ptr = &heightRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &widthRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void GetStride(out int strideRef)
	{
		Result result;
		fixed (int* ptr = &strideRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe IntPtr GetDataPointer(out int bufferSizeRef)
	{
		Result result;
		IntPtr result2 = default(IntPtr);
		fixed (int* ptr = &bufferSizeRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr2, &result2);
		}
		result.CheckError();
		return result2;
	}

	internal unsafe void GetPixelFormat(out Guid pixelFormatRef)
	{
		pixelFormatRef = default(Guid);
		Result result;
		fixed (Guid* ptr = &pixelFormatRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
