using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("0d85573c-01e3-4f7d-bfd9-0d60608bf3c3")]
public class ComputeTransformNative : TransformNative, ComputeTransform, Transform, TransformNode, IUnknown, ICallbackable, IDisposable
{
	public ComputeInformation ComputeInfo_
	{
		set
		{
			SetComputeInfo_(value);
		}
	}

	public void SetComputeInformation(ComputeInformation computeInfo)
	{
		SetComputeInfo_(computeInfo);
	}

	public RawInt3 CalculateThreadgroups(RawRectangle outputRect)
	{
		RawInt3 result = default(RawInt3);
		CalculateThreadgroups_(outputRect, out result.X, out result.Y, out result.Z);
		return result;
	}

	public ComputeTransformNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ComputeTransformNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ComputeTransformNative(nativePtr);
		}
		return null;
	}

	internal unsafe void SetComputeInfo_(ComputeInformation computeInfo)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<ComputeInformation>(computeInfo);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void CalculateThreadgroups_(RawRectangle outputRect, out int dimensionX, out int dimensionY, out int dimensionZ)
	{
		Result result;
		fixed (int* ptr = &dimensionZ)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &dimensionY)
			{
				void* ptr4 = ptr3;
				fixed (int* ptr5 = &dimensionX)
				{
					void* ptr6 = ptr5;
					result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &outputRect, ptr6, ptr4, ptr2);
				}
			}
		}
		result.CheckError();
	}
}
