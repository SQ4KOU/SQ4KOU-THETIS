using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("409cd537-8532-40cb-9774-e2feb2df4e9c")]
public class DdsDecoder : ComObject
{
	public DdsParameters Parameters
	{
		get
		{
			GetParameters(out var parametersRef);
			return parametersRef;
		}
	}

	public DdsDecoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DdsDecoder(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DdsDecoder(nativePtr);
		}
		return null;
	}

	internal unsafe void GetParameters(out DdsParameters parametersRef)
	{
		parametersRef = default(DdsParameters);
		Result result;
		fixed (DdsParameters* ptr = &parametersRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetFrame(int arrayIndex, int mipLevel, int sliceIndex, out BitmapFrameDecode bitmapFrameOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, arrayIndex, mipLevel, sliceIndex, &zero);
		if (zero != IntPtr.Zero)
		{
			bitmapFrameOut = new BitmapFrameDecode(zero);
		}
		else
		{
			bitmapFrameOut = null;
		}
		result.CheckError();
	}
}
