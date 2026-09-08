using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[Guid("5cacdb4c-407e-41b3-b936-d0f010cd6732")]
public class DdsEncoder : ComObject
{
	public DdsParameters Parameters
	{
		get
		{
			GetParameters(out var parametersRef);
			return parametersRef;
		}
		set
		{
			SetParameters(ref value);
		}
	}

	public DdsEncoder(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DdsEncoder(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DdsEncoder(nativePtr);
		}
		return null;
	}

	internal unsafe void SetParameters(ref DdsParameters parametersRef)
	{
		Result result;
		fixed (DdsParameters* ptr = &parametersRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetParameters(out DdsParameters parametersRef)
	{
		parametersRef = default(DdsParameters);
		Result result;
		fixed (DdsParameters* ptr = &parametersRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void CreateNewFrame(out BitmapFrameEncode frameEncodeOut, out int arrayIndexRef, out int mipLevelRef, out int sliceIndexRef)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (int* ptr = &sliceIndexRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &mipLevelRef)
			{
				void* ptr4 = ptr3;
				fixed (int* ptr5 = &arrayIndexRef)
				{
					void* ptr6 = ptr5;
					result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, &zero, ptr6, ptr4, ptr2);
				}
			}
		}
		if (zero != IntPtr.Zero)
		{
			frameEncodeOut = new BitmapFrameEncode(zero);
		}
		else
		{
			frameEncodeOut = null;
		}
		result.CheckError();
	}
}
