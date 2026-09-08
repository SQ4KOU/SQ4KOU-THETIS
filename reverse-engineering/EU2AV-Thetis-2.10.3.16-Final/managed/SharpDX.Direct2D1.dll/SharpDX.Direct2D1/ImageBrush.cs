using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("fe9e984d-3f95-407c-b5db-cb94d4e8f87c")]
public class ImageBrush : Brush
{
	public Image Image
	{
		get
		{
			GetImage(out var image);
			return image;
		}
		set
		{
			SetImage(value);
		}
	}

	public ExtendMode ExtendModeX
	{
		get
		{
			return GetExtendModeX();
		}
		set
		{
			SetExtendModeX(value);
		}
	}

	public ExtendMode ExtendModeY
	{
		get
		{
			return GetExtendModeY();
		}
		set
		{
			SetExtendModeY(value);
		}
	}

	public InterpolationMode InterpolationMode
	{
		get
		{
			return GetInterpolationMode();
		}
		set
		{
			SetInterpolationMode(value);
		}
	}

	public RawRectangleF SourceRectangle
	{
		get
		{
			GetSourceRectangle(out var sourceRectangle);
			return sourceRectangle;
		}
		set
		{
			SetSourceRectangle(value);
		}
	}

	public ImageBrush(DeviceContext context, Image image, ImageBrushProperties imageBrushProperties)
		: base(IntPtr.Zero)
	{
		context.CreateImageBrush(image, ref imageBrushProperties, null, this);
	}

	public ImageBrush(DeviceContext context, Image image, ImageBrushProperties imageBrushProperties, BrushProperties brushProperties)
		: base(IntPtr.Zero)
	{
		context.CreateImageBrush(image, ref imageBrushProperties, brushProperties, this);
	}

	public ImageBrush(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ImageBrush(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ImageBrush(nativePtr);
		}
		return null;
	}

	internal unsafe void SetImage(Image image)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Image>(image);
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (void*)zero);
	}

	internal unsafe void SetExtendModeX(ExtendMode extendModeX)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, (int)extendModeX);
	}

	internal unsafe void SetExtendModeY(ExtendMode extendModeY)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, (int)extendModeY);
	}

	internal unsafe void SetInterpolationMode(InterpolationMode interpolationMode)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, (int)interpolationMode);
	}

	internal unsafe void SetSourceRectangle(RawRectangleF sourceRectangle)
	{
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, &sourceRectangle);
	}

	internal unsafe void GetImage(out Image image)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			image = new Image(zero);
		}
		else
		{
			image = null;
		}
	}

	internal unsafe ExtendMode GetExtendModeX()
	{
		return ((delegate* unmanaged[Stdcall]<void*, ExtendMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe ExtendMode GetExtendModeY()
	{
		return ((delegate* unmanaged[Stdcall]<void*, ExtendMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe InterpolationMode GetInterpolationMode()
	{
		return ((delegate* unmanaged[Stdcall]<void*, InterpolationMode>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetSourceRectangle(out RawRectangleF sourceRectangle)
	{
		sourceRectangle = default(RawRectangleF);
		fixed (RawRectangleF* ptr = &sourceRectangle)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}
}
