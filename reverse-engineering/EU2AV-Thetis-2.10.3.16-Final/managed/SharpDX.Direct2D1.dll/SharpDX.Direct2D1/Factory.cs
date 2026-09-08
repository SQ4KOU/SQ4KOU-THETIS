using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;

namespace SharpDX.Direct2D1;

[Guid("06152247-6f50-465a-9245-118bfd3b6007")]
public class Factory : ComObject
{
	public Size2F DesktopDpi
	{
		get
		{
			GetDesktopDpi(out var dpiX, out var dpiY);
			return new Size2F(dpiX, dpiY);
		}
	}

	public Factory()
		: this(FactoryType.SingleThreaded)
	{
	}

	public Factory(FactoryType factoryType)
		: this(factoryType, DebugLevel.None)
	{
	}

	public Factory(FactoryType factoryType, DebugLevel debugLevel)
		: base(IntPtr.Zero)
	{
		FactoryOptions? factoryOptionsRef = null;
		if (debugLevel != DebugLevel.None)
		{
			factoryOptionsRef = new FactoryOptions
			{
				DebugLevel = debugLevel
			};
		}
		D2D1.CreateFactory(factoryType, Utilities.GetGuidFromType(GetType()), factoryOptionsRef, out var iFactoryOut);
		FromTemp(iFactoryOut);
	}

	public Factory(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory(nativePtr);
		}
		return null;
	}

	public unsafe void ReloadSystemMetrics()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	internal unsafe void GetDesktopDpi(out float dpiX, out float dpiY)
	{
		fixed (float* ptr = &dpiY)
		{
			void* ptr2 = ptr;
			fixed (float* ptr3 = &dpiX)
			{
				void* ptr4 = ptr3;
				((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2);
			}
		}
	}

	internal unsafe void CreateRectangleGeometry(RawRectangleF rectangle, RectangleGeometry rectangleGeometry)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, &rectangle, &zero);
		rectangleGeometry.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateRoundedRectangleGeometry(ref RoundedRectangle roundedRectangle, RoundedRectangleGeometry roundedRectangleGeometry)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (RoundedRectangle* ptr = &roundedRectangle)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, ptr2, &zero);
		}
		roundedRectangleGeometry.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateEllipseGeometry(Ellipse ellipse, EllipseGeometry ellipseGeometry)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, &ellipse, &zero);
		ellipseGeometry.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateGeometryGroup(FillMode fillMode, Geometry[] geometries, int geometriesCount, GeometryGroup geometryGroup)
	{
		IntPtr* ptr = null;
		if (geometries != null)
		{
			ptr = stackalloc IntPtr[geometries.Length];
		}
		IntPtr zero = IntPtr.Zero;
		if (geometries != null)
		{
			for (int i = 0; i < geometries.Length; i++)
			{
				ptr[i] = CppObject.ToCallbackPtr<Geometry>(geometries[i]);
			}
		}
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (int)fillMode, ptr, geometriesCount, &zero);
		geometryGroup.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateTransformedGeometry(Geometry sourceGeometry, ref RawMatrix3x2 transform, TransformedGeometry transformedGeometry)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Geometry>(sourceGeometry);
		Result result;
		fixed (RawMatrix3x2* ptr = &transform)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2, &zero2);
		}
		transformedGeometry.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreatePathGeometry(PathGeometry athGeometryRef)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, &zero);
		athGeometryRef.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateStrokeStyle(ref StrokeStyleProperties strokeStyleProperties, float[] dashes, int dashesCount, StrokeStyle strokeStyle)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (float* ptr = dashes)
		{
			void* ptr2 = ptr;
			fixed (StrokeStyleProperties* ptr3 = &strokeStyleProperties)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2, dashesCount, &zero);
			}
		}
		strokeStyle.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateDrawingStateBlock(DrawingStateDescription? drawingStateDescription, RenderingParams textRenderingParams, DrawingStateBlock drawingStateBlock)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		DrawingStateDescription value = default(DrawingStateDescription);
		if (drawingStateDescription.HasValue)
		{
			value = drawingStateDescription.Value;
		}
		zero = CppObject.ToCallbackPtr<RenderingParams>(textRenderingParams);
		void* nativePointer = _nativePointer;
		DrawingStateDescription* intPtr = ((!drawingStateDescription.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(nativePointer, intPtr, (void*)zero, &zero2);
		drawingStateBlock.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateWicBitmapRenderTarget(SharpDX.WIC.Bitmap target, ref RenderTargetProperties renderTargetProperties, RenderTarget renderTarget)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SharpDX.WIC.Bitmap>(target);
		Result result;
		fixed (RenderTargetProperties* ptr = &renderTargetProperties)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2, &zero2);
		}
		renderTarget.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateHwndRenderTarget(ref RenderTargetProperties renderTargetProperties, ref HwndRenderTargetProperties hwndRenderTargetProperties, WindowRenderTarget hwndRenderTarget)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (HwndRenderTargetProperties* ptr = &hwndRenderTargetProperties)
		{
			void* ptr2 = ptr;
			fixed (RenderTargetProperties* ptr3 = &renderTargetProperties)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2, &zero);
			}
		}
		hwndRenderTarget.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateDxgiSurfaceRenderTarget(Surface dxgiSurface, ref RenderTargetProperties renderTargetProperties, RenderTarget renderTarget)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Surface>(dxgiSurface);
		Result result;
		fixed (RenderTargetProperties* ptr = &renderTargetProperties)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, ptr2, &zero2);
		}
		renderTarget.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateDCRenderTarget(ref RenderTargetProperties renderTargetProperties, DeviceContextRenderTarget dcRenderTarget)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (RenderTargetProperties* ptr = &renderTargetProperties)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, ptr2, &zero);
		}
		dcRenderTarget.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateGeometryGroup(FillMode fillMode, ComArray<Geometry> geometries, int geometriesCount, GeometryGroup geometryGroup)
	{
		IntPtr zero = IntPtr.Zero;
		void* nativePointer = _nativePointer;
		IntPtr intPtr = geometries?.NativePointer ?? IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, (int)fillMode, (void*)intPtr, geometriesCount, &zero);
		geometryGroup.NativePointer = zero;
		result.CheckError();
	}

	private unsafe void CreateGeometryGroup(FillMode fillMode, IntPtr geometries, int geometriesCount, IntPtr geometryGroup)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (int)fillMode, (void*)geometries, geometriesCount, (void*)geometryGroup)).CheckError();
	}
}
