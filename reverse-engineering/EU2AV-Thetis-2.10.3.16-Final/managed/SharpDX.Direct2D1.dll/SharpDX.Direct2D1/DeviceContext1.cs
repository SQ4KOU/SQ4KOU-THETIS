using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("d37f57e4-6908-459f-a199-e72f24f79987")]
public class DeviceContext1 : DeviceContext
{
	public DeviceContext1(Device1 device, DeviceContextOptions options)
		: base(IntPtr.Zero)
	{
		device.CreateDeviceContext(options, this);
	}

	public DeviceContext1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceContext1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DeviceContext1(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateFilledGeometryRealization(Geometry geometry, float flatteningTolerance, GeometryRealization geometryRealization)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Geometry>(geometry);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)92 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, flatteningTolerance, &zero2);
		geometryRealization.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateStrokedGeometryRealization(Geometry geometry, float flatteningTolerance, float strokeWidth, StrokeStyle strokeStyle, GeometryRealization geometryRealization)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Geometry>(geometry);
		zero2 = CppObject.ToCallbackPtr<StrokeStyle>(strokeStyle);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, float, float, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)93 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, flatteningTolerance, strokeWidth, (void*)zero2, &zero3);
		geometryRealization.NativePointer = zero3;
		result.CheckError();
	}

	public unsafe void DrawGeometryRealization(GeometryRealization geometryRealization, Brush brush)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<GeometryRealization>(geometryRealization);
		zero2 = CppObject.ToCallbackPtr<Brush>(brush);
		((delegate* unmanaged[Stdcall]<void*, void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)94 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2);
	}
}
