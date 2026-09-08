using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("519ae1bd-d19a-420d-b849-364f594776b7")]
public class RenderInformation : ComObject
{
	public RawBool Cached
	{
		set
		{
			SetCached(value);
		}
	}

	public int InstructionCountHint
	{
		set
		{
			SetInstructionCountHint(value);
		}
	}

	public RenderInformation(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator RenderInformation(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new RenderInformation(nativePtr);
		}
		return null;
	}

	public unsafe void SetInputDescription(int inputIndex, InputDescription inputDescription)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, InputDescription, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, inputIndex, inputDescription)).CheckError();
	}

	public unsafe void SetOutputBuffer(BufferPrecision bufferPrecision, ChannelDepth channelDepth)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (int)bufferPrecision, (int)channelDepth)).CheckError();
	}

	internal unsafe void SetCached(RawBool isCached)
	{
		((delegate* unmanaged[Stdcall]<void*, RawBool, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, isCached);
	}

	internal unsafe void SetInstructionCountHint(int instructionCount)
	{
		((delegate* unmanaged[Stdcall]<void*, int, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, instructionCount);
	}
}
