using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("d59bab0a-68a2-455b-a5dc-9eb2854e2490")]
public class SvgPaint : SvgAttribute
{
	public SvgPaintType PaintType
	{
		get
		{
			return GetPaintType();
		}
		set
		{
			SetPaintType(value);
		}
	}

	public RawColor4 Color
	{
		get
		{
			GetColor(out var color);
			return color;
		}
		set
		{
			SetColor(value);
		}
	}

	public int IdLength => GetIdLength();

	public SvgPaint(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SvgPaint(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SvgPaint(nativePtr);
		}
		return null;
	}

	internal unsafe void SetPaintType(SvgPaintType paintType)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, (int)paintType)).CheckError();
	}

	internal unsafe SvgPaintType GetPaintType()
	{
		return ((delegate* unmanaged[Stdcall]<void*, SvgPaintType>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void SetColor(RawColor4 color)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &color)).CheckError();
	}

	internal unsafe void GetColor(out RawColor4 color)
	{
		color = default(RawColor4);
		fixed (RawColor4* ptr = &color)
		{
			void* ptr2 = ptr;
			((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
	}

	public unsafe void SetId(string id)
	{
		Result result;
		fixed (char* ptr = id)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	public unsafe void GetId(IntPtr id, int idCount)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, (void*)id, idCount)).CheckError();
	}

	internal unsafe int GetIdLength()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer);
	}
}
