using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.WIC;

[Guid("3d4c0c61-18a4-41e4-bd80-481a4fc9f464")]
public class DdsFrameDecode : ComObject
{
	public Size2 SizeInBlocks
	{
		get
		{
			GetSizeInBlocks(out var widthInBlocksRef, out var heightInBlocksRef);
			return new Size2(widthInBlocksRef, heightInBlocksRef);
		}
	}

	public DdsFormatInfo FormatInfo
	{
		get
		{
			GetFormatInfo(out var formatInfoRef);
			return formatInfoRef;
		}
	}

	public void CopyBlocks(RawBox? boundsInBlocks, int stride, DataStream destination)
	{
		CopyBlocks(boundsInBlocks, stride, (int)(destination.Length - destination.Position), destination.PositionPointer);
	}

	public DdsFrameDecode(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DdsFrameDecode(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DdsFrameDecode(nativePtr);
		}
		return null;
	}

	internal unsafe void GetSizeInBlocks(out int widthInBlocksRef, out int heightInBlocksRef)
	{
		Result result;
		fixed (int* ptr = &heightInBlocksRef)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &widthInBlocksRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void GetFormatInfo(out DdsFormatInfo formatInfoRef)
	{
		formatInfoRef = default(DdsFormatInfo);
		Result result;
		fixed (DdsFormatInfo* ptr = &formatInfoRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void CopyBlocks(RawBox? rcBoundsInBlocksRef, int stride, int bufferSize, IntPtr bufferRef)
	{
		RawBox value = default(RawBox);
		if (rcBoundsInBlocksRef.HasValue)
		{
			value = rcBoundsInBlocksRef.Value;
		}
		void* nativePointer = _nativePointer;
		RawBox* intPtr = ((!rcBoundsInBlocksRef.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(nativePointer, intPtr, stride, bufferSize, (void*)bufferRef)).CheckError();
	}
}
