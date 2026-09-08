using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("f1c0ca52-92a3-4f00-b4ce-f35691efd9d9")]
public class SvgStrokeDashArray : SvgAttribute
{
	public int DashesCount => GetDashesCount();

	public SvgStrokeDashArray(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SvgStrokeDashArray(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SvgStrokeDashArray(nativePtr);
		}
		return null;
	}

	public unsafe void RemoveDashesAtEnd(int dashesCount)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, dashesCount)).CheckError();
	}

	public unsafe void UpdateDashes(SvgLength[] dashes, int dashesCount, int startIndex)
	{
		Result result;
		fixed (SvgLength* ptr = dashes)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2, dashesCount, startIndex);
		}
		result.CheckError();
	}

	public unsafe void UpdateDashes(float[] dashes, int dashesCount, int startIndex)
	{
		Result result;
		fixed (float* ptr = dashes)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2, dashesCount, startIndex);
		}
		result.CheckError();
	}

	public unsafe void GetDashes(SvgLength[] dashes, int dashesCount, int startIndex)
	{
		Result result;
		fixed (SvgLength* ptr = dashes)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2, dashesCount, startIndex);
		}
		result.CheckError();
	}

	public unsafe void GetDashes(float[] dashes, int dashesCount, int startIndex)
	{
		Result result;
		fixed (float* ptr = dashes)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, ptr2, dashesCount, startIndex);
		}
		result.CheckError();
	}

	internal unsafe int GetDashesCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer);
	}
}
