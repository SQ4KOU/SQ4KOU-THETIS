using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("c095e4f4-bb98-43d6-9745-4d1b84ec9888")]
public class SvgPathData : SvgAttribute
{
	public int SegmentDataCount => GetSegmentDataCount();

	public int CommandsCount => GetCommandsCount();

	public SvgPathData(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SvgPathData(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SvgPathData(nativePtr);
		}
		return null;
	}

	public unsafe void RemoveSegmentDataAtEnd(int dataCount)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, dataCount)).CheckError();
	}

	public unsafe void UpdateSegmentData(float[] data, int dataCount, int startIndex)
	{
		Result result;
		fixed (float* ptr = data)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr2, dataCount, startIndex);
		}
		result.CheckError();
	}

	public unsafe void GetSegmentData(float[] data, int dataCount, int startIndex)
	{
		Result result;
		fixed (float* ptr = data)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr2, dataCount, startIndex);
		}
		result.CheckError();
	}

	internal unsafe int GetSegmentDataCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void RemoveCommandsAtEnd(int commandsCount)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, commandsCount)).CheckError();
	}

	public unsafe void UpdateCommands(SvgPathCommand[] commands, int commandsCount, int startIndex)
	{
		Result result;
		fixed (SvgPathCommand* ptr = commands)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr2, commandsCount, startIndex);
		}
		result.CheckError();
	}

	public unsafe void GetCommands(SvgPathCommand[] commands, int commandsCount, int startIndex)
	{
		Result result;
		fixed (SvgPathCommand* ptr = commands)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, ptr2, commandsCount, startIndex);
		}
		result.CheckError();
	}

	internal unsafe int GetCommandsCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void CreatePathGeometry(FillMode fillMode, out PathGeometry1 athGeometryRef)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, (int)fillMode, &zero);
		if (zero != IntPtr.Zero)
		{
			athGeometryRef = new PathGeometry1(zero);
		}
		else
		{
			athGeometryRef = null;
		}
		result.CheckError();
	}
}
