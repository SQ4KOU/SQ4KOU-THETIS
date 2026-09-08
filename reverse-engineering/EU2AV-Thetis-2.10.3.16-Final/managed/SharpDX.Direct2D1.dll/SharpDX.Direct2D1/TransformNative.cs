using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("ef1a287d-342a-4f76-8fdb-da0d6ea9f92b")]
public class TransformNative : TransformNodeNative, Transform, TransformNode, IUnknown, ICallbackable, IDisposable
{
	public void MapOutputRectangleToInputRectangles(RawRectangle outputRect, RawRectangle[] inputRects)
	{
		MapOutputRectToInputRects_(outputRect, inputRects, inputRects.Length);
	}

	public RawRectangle MapInputRectanglesToOutputRectangle(RawRectangle[] inputRects, RawRectangle[] inputOpaqueSubRects, out RawRectangle outputOpaqueSubRect)
	{
		return MapInputRectsToOutputRect_(inputRects, inputOpaqueSubRects, inputRects.Length, out outputOpaqueSubRect);
	}

	public RawRectangle MapInvalidRect(int inputIndex, RawRectangle invalidInputRect)
	{
		return MapInvalidRect_(inputIndex, invalidInputRect);
	}

	public TransformNative(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TransformNative(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new TransformNative(nativePtr);
		}
		return null;
	}

	internal unsafe void MapOutputRectToInputRects_(RawRectangle outputRect, RawRectangle[] inputRects, int inputRectsCount)
	{
		Result result;
		fixed (RawRectangle* ptr = inputRects)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &outputRect, ptr2, inputRectsCount);
		}
		result.CheckError();
	}

	internal unsafe RawRectangle MapInputRectsToOutputRect_(RawRectangle[] inputRects, RawRectangle[] inputOpaqueSubRects, int inputRectCount, out RawRectangle outputOpaqueSubRect)
	{
		outputOpaqueSubRect = default(RawRectangle);
		Result result;
		RawRectangle result2 = default(RawRectangle);
		fixed (RawRectangle* ptr = &outputOpaqueSubRect)
		{
			void* ptr2 = ptr;
			fixed (RawRectangle* ptr3 = inputOpaqueSubRects)
			{
				void* ptr4 = ptr3;
				fixed (RawRectangle* ptr5 = inputRects)
				{
					void* ptr6 = ptr5;
					result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr6, ptr4, inputRectCount, &result2, ptr2);
				}
			}
		}
		result.CheckError();
		return result2;
	}

	internal unsafe RawRectangle MapInvalidRect_(int inputIndex, RawRectangle invalidInputRect)
	{
		RawRectangle result = default(RawRectangle);
		((Result)((delegate* unmanaged[Stdcall]<void*, int, RawRectangle, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, inputIndex, invalidInputRect, &result)).CheckError();
		return result;
	}
}
