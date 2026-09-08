using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.WIC;

[Guid("3AFF9CCE-BE95-4303-B927-E7D16FF4A613")]
public class PlanarBitmapSourceTransform : ComObject
{
	public PlanarBitmapSourceTransform(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator PlanarBitmapSourceTransform(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new PlanarBitmapSourceTransform(nativePtr);
		}
		return null;
	}

	public unsafe void DoesSupportTransform(ref int widthRef, ref int heightRef, BitmapTransformOptions dstTransform, PlanarOptions dstPlanarOptions, Guid[] guidDstFormatsRef, BitmapPlaneDescription[] planeDescriptionsRef, int planes, out RawBool fIsSupportedRef)
	{
		fIsSupportedRef = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &fIsSupportedRef)
		{
			void* ptr2 = ptr;
			fixed (BitmapPlaneDescription* ptr3 = planeDescriptionsRef)
			{
				void* ptr4 = ptr3;
				fixed (Guid* ptr5 = guidDstFormatsRef)
				{
					void* ptr6 = ptr5;
					fixed (int* ptr7 = &heightRef)
					{
						void* ptr8 = ptr7;
						fixed (int* ptr9 = &widthRef)
						{
							void* ptr10 = ptr9;
							result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, int, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr10, ptr8, (int)dstTransform, (int)dstPlanarOptions, ptr6, ptr4, planes, ptr2);
						}
					}
				}
			}
		}
		result.CheckError();
	}

	public unsafe void CopyPixels(RawBox? rcSourceRef, int width, int height, BitmapTransformOptions dstTransform, PlanarOptions dstPlanarOptions, BitmapPlane[] dstPlanesRef, int planes)
	{
		RawBox value = default(RawBox);
		if (rcSourceRef.HasValue)
		{
			value = rcSourceRef.Value;
		}
		Result result;
		fixed (BitmapPlane* ptr = dstPlanesRef)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			RawBox* intPtr = ((!rcSourceRef.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(nativePointer, intPtr, width, height, (int)dstTransform, (int)dstPlanarOptions, ptr2, planes);
		}
		result.CheckError();
	}
}
