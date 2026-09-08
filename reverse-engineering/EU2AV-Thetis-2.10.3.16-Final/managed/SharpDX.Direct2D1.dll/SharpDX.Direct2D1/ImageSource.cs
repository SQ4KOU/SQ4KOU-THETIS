using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("c9b664e5-74a1-4378-9ac2-eefc37a3f4d8")]
public class ImageSource : Image
{
	public ImageSource(DeviceContext2 context2, Surface[] surfaces, int surfaceCount, ColorSpaceType colorSpace, ImageSourceFromDxgiOptions options)
		: this(IntPtr.Zero)
	{
		context2.CreateImageSourceFromDxgi(surfaces, surfaceCount, colorSpace, options, this);
	}

	public ImageSource(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ImageSource(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ImageSource(nativePtr);
		}
		return null;
	}

	public unsafe void OfferResources()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public unsafe void TryReclaimResources(out RawBool resourcesDiscarded)
	{
		resourcesDiscarded = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &resourcesDiscarded)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
