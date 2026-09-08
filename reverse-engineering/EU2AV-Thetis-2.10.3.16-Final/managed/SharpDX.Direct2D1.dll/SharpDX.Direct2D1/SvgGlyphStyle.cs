using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("af671749-d241-4db8-8e41-dcc2e5c1a438")]
public class SvgGlyphStyle : Resource
{
	public Brush Fill
	{
		get
		{
			GetFill(out var brush);
			return brush;
		}
		set
		{
			SetFill(value);
		}
	}

	public int StrokeDashesCount => GetStrokeDashesCount();

	public SvgGlyphStyle(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SvgGlyphStyle(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SvgGlyphStyle(nativePtr);
		}
		return null;
	}

	internal unsafe void SetFill(Brush brush)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Brush>(brush);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	internal unsafe void GetFill(out Brush brush)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			brush = new Brush(zero);
		}
		else
		{
			brush = null;
		}
	}

	public unsafe void SetStroke(Brush brush, float strokeWidth, float[] dashes, int dashesCount, float dashOffset)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Brush>(brush);
		Result result;
		fixed (float* ptr = dashes)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, float, void*, int, float, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, strokeWidth, ptr2, dashesCount, dashOffset);
		}
		result.CheckError();
	}

	internal unsafe int GetStrokeDashesCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void GetStroke(out Brush brush, out float strokeWidth, float[] dashes, int dashesCount, out float dashOffset)
	{
		IntPtr zero = IntPtr.Zero;
		fixed (float* ptr = &dashOffset)
		{
			void* ptr2 = ptr;
			fixed (float* ptr3 = dashes)
			{
				void* ptr4 = ptr3;
				fixed (float* ptr5 = &strokeWidth)
				{
					void* ptr6 = ptr5;
					((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &zero, ptr6, ptr4, dashesCount, ptr2);
				}
			}
		}
		if (zero != IntPtr.Zero)
		{
			brush = new Brush(zero);
		}
		else
		{
			brush = null;
		}
	}
}
