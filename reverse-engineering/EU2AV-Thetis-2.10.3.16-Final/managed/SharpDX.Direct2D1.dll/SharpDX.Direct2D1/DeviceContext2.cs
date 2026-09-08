using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;

namespace SharpDX.Direct2D1;

[Guid("394ea6a3-0c34-4321-950b-6ca20f0be6c7")]
public class DeviceContext2 : DeviceContext1
{
	public DeviceContext2(Device2 device, DeviceContextOptions options)
		: base(IntPtr.Zero)
	{
		device.CreateDeviceContext(options, this);
	}

	public DeviceContext2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceContext2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DeviceContext2(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateInk(InkPoint startPoint, Ink ink)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)95 * (nint)sizeof(void*))))(_nativePointer, &startPoint, &zero);
		ink.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateInkStyle(InkStyleProperties? inkStyleProperties, InkStyle inkStyle)
	{
		IntPtr zero = IntPtr.Zero;
		InkStyleProperties value = default(InkStyleProperties);
		if (inkStyleProperties.HasValue)
		{
			value = inkStyleProperties.Value;
		}
		void* nativePointer = _nativePointer;
		InkStyleProperties* intPtr = ((!inkStyleProperties.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)96 * (nint)sizeof(void*))))(nativePointer, intPtr, &zero);
		inkStyle.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateGradientMesh(GradientMeshPatch[] atchesRef, int patchesCount, GradientMesh gradientMesh)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (GradientMeshPatch* ptr = atchesRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)97 * (nint)sizeof(void*))))(_nativePointer, ptr2, patchesCount, &zero);
		}
		gradientMesh.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateImageSourceFromWic(BitmapSource wicBitmapSource, ImageSourceLoadingOptions loadingOptions, AlphaMode alphaMode, ImageSourceFromWic imageSource)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<BitmapSource>(wicBitmapSource);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)98 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (int)loadingOptions, (int)alphaMode, &zero2);
		imageSource.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateLookupTable3D(BufferPrecision precision, int[] extents, byte[] data, int dataCount, int[] strides, LookupTable3D lookupTable)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (int* ptr = strides)
		{
			void* ptr2 = ptr;
			fixed (byte* ptr3 = data)
			{
				void* ptr4 = ptr3;
				fixed (int* ptr5 = extents)
				{
					void* ptr6 = ptr5;
					result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)99 * (nint)sizeof(void*))))(_nativePointer, (int)precision, ptr6, ptr4, dataCount, ptr2, &zero);
				}
			}
		}
		lookupTable.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateImageSourceFromDxgi(Surface[] surfaces, int surfaceCount, ColorSpaceType colorSpace, ImageSourceFromDxgiOptions options, ImageSource imageSource)
	{
		IntPtr* ptr = null;
		if (surfaces != null)
		{
			ptr = stackalloc IntPtr[surfaces.Length];
		}
		IntPtr zero = IntPtr.Zero;
		if (surfaces != null)
		{
			for (int i = 0; i < surfaces.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<Surface>(surfaces[i]);
			}
		}
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)100 * (nint)sizeof(void*))))(_nativePointer, ptr, surfaceCount, (int)colorSpace, (int)options, &zero);
		imageSource.NativePointer = zero;
		result.CheckError();
	}

	public unsafe void GetGradientMeshWorldBounds(GradientMesh gradientMesh, out RawRectangleF boundsRef)
	{
		IntPtr zero = IntPtr.Zero;
		boundsRef = default(RawRectangleF);
		zero = CppObject.ToCallbackPtr<GradientMesh>(gradientMesh);
		Result result;
		fixed (RawRectangleF* ptr = &boundsRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)101 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2);
		}
		result.CheckError();
	}

	public unsafe void DrawInk(Ink ink, Brush brush, InkStyle inkStyle)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Ink>(ink);
		zero2 = CppObject.ToCallbackPtr<Brush>(brush);
		zero3 = CppObject.ToCallbackPtr<InkStyle>(inkStyle);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)102 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2, (void*)zero3);
	}

	public unsafe void DrawGradientMesh(GradientMesh gradientMesh)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<GradientMesh>(gradientMesh);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)103 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	public unsafe void DrawGdiMetafile(GdiMetafile gdiMetafile, RawRectangleF? destinationRectangle, RawRectangleF? sourceRectangle)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<GdiMetafile>(gdiMetafile);
		RawRectangleF value = default(RawRectangleF);
		if (destinationRectangle.HasValue)
		{
			value = destinationRectangle.Value;
		}
		RawRectangleF value2 = default(RawRectangleF);
		if (sourceRectangle.HasValue)
		{
			value2 = sourceRectangle.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RawRectangleF* intPtr2 = ((!destinationRectangle.HasValue) ? null : (&value));
		RawRectangleF* intPtr3 = ((!sourceRectangle.HasValue) ? null : (&value2));
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)104 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, intPtr3);
	}

	internal unsafe void CreateTransformedImageSource(ImageSource imageSource, ref TransformedImageSourceProperties ropertiesRef, TransformedImageSource transformedImageSource)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<ImageSource>(imageSource);
		Result result;
		fixed (TransformedImageSourceProperties* ptr = &ropertiesRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)105 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2, &zero2);
		}
		transformedImageSource.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateImageSourceFromDxgi(ComArray<Surface> surfaces, int surfaceCount, ColorSpaceType colorSpace, ImageSourceFromDxgiOptions options, ImageSource imageSource)
	{
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		IntPtr intPtr = surfaces?.NativePointer ?? IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)100 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr, surfaceCount, (int)colorSpace, (int)options, &zero);
		imageSource.NativePointer = zero;
		result.CheckError();
	}

	private unsafe void CreateImageSourceFromDxgi(IntPtr surfaces, int surfaceCount, ColorSpaceType colorSpace, ImageSourceFromDxgiOptions options, IntPtr imageSource)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)100 * (nint)sizeof(void*))))(_nativePointer, (void*)surfaces, surfaceCount, (int)colorSpace, (int)options, (void*)imageSource)).CheckError();
	}
}
