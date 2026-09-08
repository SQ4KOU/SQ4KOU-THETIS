using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Win32;

namespace SharpDX.Direct2D1;

[Guid("7836d248-68cc-4df6-b9e8-de991bf62eb7")]
public class DeviceContext5 : DeviceContext4
{
	public DeviceContext5(Device5 device, DeviceContextOptions options)
		: base(IntPtr.Zero)
	{
		device.CreateDeviceContext(options, this);
	}

	public SvgDocument CreateSvgDocument(IStream stream, Size2F viewportSize)
	{
		CreateSvgDocument(stream, viewportSize, out var svgDocument);
		return svgDocument;
	}

	public DeviceContext5(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DeviceContext5(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new DeviceContext5(nativePtr);
		}
		return null;
	}

	public unsafe void CreateSvgDocument(IStream inputXmlStream, Size2F viewportSize, out SvgDocument svgDocument)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IStream>(inputXmlStream);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, Size2F, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)115 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, viewportSize, &zero2);
		if (zero2 != IntPtr.Zero)
		{
			svgDocument = new SvgDocument(zero2);
		}
		else
		{
			svgDocument = null;
		}
		result.CheckError();
	}

	public unsafe void DrawSvgDocument(SvgDocument svgDocument)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SvgDocument>(svgDocument);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)116 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	public unsafe void CreateColorContextFromDxgiColorSpace(ColorSpaceType colorSpace, ColorContext1 colorContext)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)117 * (nint)sizeof(void*))))(_nativePointer, (int)colorSpace, &zero);
		colorContext.NativePointer = zero;
		result.CheckError();
	}

	public unsafe void CreateColorContextFromSimpleColorProfile(ref SimpleColorProfile simpleProfile, ColorContext1 colorContext)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (SimpleColorProfile* ptr = &simpleProfile)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)118 * (nint)sizeof(void*))))(_nativePointer, ptr2, &zero);
		}
		colorContext.NativePointer = zero;
		result.CheckError();
	}
}
