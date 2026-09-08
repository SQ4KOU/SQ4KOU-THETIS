using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.WIC;

namespace SharpDX.Direct2D1;

[Guid("47dd575d-ac05-4cdd-8049-9b02cd16f44c")]
public class Device : Resource
{
	public long MaximumTextureMemory
	{
		get
		{
			return GetMaximumTextureMemory();
		}
		set
		{
			SetMaximumTextureMemory(value);
		}
	}

	public Device(SharpDX.DXGI.Device device)
		: base(IntPtr.Zero)
	{
		D2D1.CreateDevice(device, null, this);
	}

	public Device(SharpDX.DXGI.Device device, CreationProperties creationProperties)
		: base(IntPtr.Zero)
	{
		D2D1.CreateDevice(device, creationProperties, this);
	}

	public Device(Factory1 factory, SharpDX.DXGI.Device device)
		: base(IntPtr.Zero)
	{
		factory.CreateDevice(device, this);
	}

	public Device(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Device(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Device(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateDeviceContext(DeviceContextOptions options, DeviceContext deviceContext)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (int)options, &zero);
		deviceContext.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreatePrintControl(ImagingFactory wicFactory, ComObject documentTarget, PrintControlProperties? rintControlPropertiesRef, PrintControl rintControlRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr zero3 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<ImagingFactory>(wicFactory);
		zero2 = CppObject.ToCallbackPtr<ComObject>(documentTarget);
		PrintControlProperties value = default(PrintControlProperties);
		if (rintControlPropertiesRef.HasValue)
		{
			value = rintControlPropertiesRef.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		void* intPtr2 = (void*)zero2;
		PrintControlProperties* intPtr3 = ((!rintControlPropertiesRef.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, intPtr3, &zero3);
		rintControlRef.NativePointer = zero3;
		result.CheckError();
	}

	internal unsafe void SetMaximumTextureMemory(long maximumInBytes)
	{
		((delegate* unmanaged[Stdcall]<void*, long, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, maximumInBytes);
	}

	internal unsafe long GetMaximumTextureMemory()
	{
		return ((delegate* unmanaged[Stdcall]<void*, long>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void ClearResources(int millisecondsSinceUse)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, millisecondsSinceUse);
	}
}
