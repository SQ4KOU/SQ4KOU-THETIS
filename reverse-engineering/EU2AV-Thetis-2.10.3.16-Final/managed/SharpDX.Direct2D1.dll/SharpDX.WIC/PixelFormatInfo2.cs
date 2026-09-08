using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.WIC;

[Guid("A9DB33A2-AF5F-43C7-B679-74F5984B5AA4")]
public class PixelFormatInfo2 : PixelFormatInfo
{
	public RawBool IsSupportingTransparency
	{
		get
		{
			IsSupportingTransparency_(out var fSupportsTransparencyRef);
			return fSupportsTransparencyRef;
		}
	}

	public PixelFormatNumericRepresentation NumericRepresentation
	{
		get
		{
			GetNumericRepresentation(out var numericRepresentationRef);
			return numericRepresentationRef;
		}
	}

	public PixelFormatInfo2(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator PixelFormatInfo2(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new PixelFormatInfo2(nativePtr);
		}
		return null;
	}

	internal unsafe void IsSupportingTransparency_(out RawBool fSupportsTransparencyRef)
	{
		fSupportsTransparencyRef = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &fSupportsTransparencyRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetNumericRepresentation(out PixelFormatNumericRepresentation numericRepresentationRef)
	{
		Result result;
		fixed (PixelFormatNumericRepresentation* ptr = &numericRepresentationRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}
}
