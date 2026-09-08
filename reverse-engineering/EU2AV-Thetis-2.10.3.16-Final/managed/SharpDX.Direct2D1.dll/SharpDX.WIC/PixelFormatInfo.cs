using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("E8EDA601-3D48-431a-AB44-69059BE88BBE")]
public class PixelFormatInfo : ComponentInfo
{
	public Guid FormatGUID
	{
		get
		{
			GetFormatGUID(out var formatRef);
			return formatRef;
		}
	}

	public ColorContext ColorContext
	{
		get
		{
			GetColorContext(out var colorContextOut);
			return colorContextOut;
		}
	}

	public int BitsPerPixel
	{
		get
		{
			GetBitsPerPixel(out var bitsPerPixelRef);
			return bitsPerPixelRef;
		}
	}

	public int ChannelCount
	{
		get
		{
			GetChannelCount(out var channelCountRef);
			return channelCountRef;
		}
	}

	public byte[] GetChannelMask(int channelIndex)
	{
		int actualRef = 0;
		GetChannelMask(channelIndex, actualRef, null, out actualRef);
		if (actualRef == 0)
		{
			return new byte[0];
		}
		byte[] array = new byte[actualRef];
		GetChannelMask(channelIndex, actualRef, array, out actualRef);
		return array;
	}

	public PixelFormatInfo(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator PixelFormatInfo(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new PixelFormatInfo(nativePtr);
		}
		return null;
	}

	internal unsafe void GetFormatGUID(out Guid formatRef)
	{
		formatRef = default(Guid);
		Result result;
		fixed (Guid* ptr = &formatRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetColorContext(out ColorContext colorContextOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			colorContextOut = new ColorContext(zero);
		}
		else
		{
			colorContextOut = null;
		}
		result.CheckError();
	}

	internal unsafe void GetBitsPerPixel(out int bitsPerPixelRef)
	{
		Result result;
		fixed (int* ptr = &bitsPerPixelRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetChannelCount(out int channelCountRef)
	{
		Result result;
		fixed (int* ptr = &channelCountRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetChannelMask(int channelIndex, int maskBuffer, byte[] maskBufferRef, out int actualRef)
	{
		Result result;
		fixed (int* ptr = &actualRef)
		{
			void* ptr2 = ptr;
			fixed (byte* ptr3 = maskBufferRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, channelIndex, maskBuffer, ptr4, ptr2);
			}
		}
		result.CheckError();
	}
}
