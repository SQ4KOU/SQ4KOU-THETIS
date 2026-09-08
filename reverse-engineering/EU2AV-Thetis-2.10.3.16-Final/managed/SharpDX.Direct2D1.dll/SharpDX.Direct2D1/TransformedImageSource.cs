using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("7f1f79e5-2796-416c-8f55-700f911445e5")]
public class TransformedImageSource : Image
{
	public ImageSource Source
	{
		get
		{
			GetSource(out var imageSource);
			return imageSource;
		}
	}

	public TransformedImageSourceProperties Properties
	{
		get
		{
			GetProperties(out var ropertiesRef);
			return ropertiesRef;
		}
	}

	public TransformedImageSource(DeviceContext2 context2, ImageSource imageSource, ref TransformedImageSourceProperties ropertiesRef)
		: this(IntPtr.Zero)
	{
		context2.CreateTransformedImageSource(imageSource, ref ropertiesRef, this);
	}

	public TransformedImageSource(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TransformedImageSource(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new TransformedImageSource(nativePtr);
		}
		return null;
	}

	internal unsafe void GetSource(out ImageSource imageSource)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			imageSource = new ImageSource(zero);
		}
		else
		{
			imageSource = null;
		}
	}

	internal unsafe void GetProperties(out TransformedImageSourceProperties ropertiesRef)
	{
		ropertiesRef = default(TransformedImageSourceProperties);
		fixed (TransformedImageSourceProperties* ptr = &ropertiesRef)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
