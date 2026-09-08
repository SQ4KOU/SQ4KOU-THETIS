using System;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;

namespace SharpDX.WIC;

[Guid("7B816B45-1996-4476-B132-DE9E247C8AF0")]
public class ImagingFactory2 : ImagingFactory
{
	public ImagingFactory2()
	{
		Utilities.CreateComInstance(ImagingFactory.WICImagingFactoryClsid, Utilities.CLSCTX.ClsctxInprocServer, Utilities.GetGuidFromType(typeof(ImagingFactory2)), this);
	}

	public ImagingFactory2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ImagingFactory2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ImagingFactory2(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateImageEncoder(Device d2DDeviceRef, ImageEncoder wICImageEncoderOut)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Device>(d2DDeviceRef);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		wICImageEncoderOut.NativePointer = zero2;
		result.CheckError();
	}
}
