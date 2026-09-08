using System;
using System.Runtime.InteropServices;
using SharpDX.WIC;
using SharpDX.Win32;

namespace SharpDX.Direct2D1;

[Guid("2c1d867d-c290-41c8-ae7e-34a98702e9a5")]
public class PrintControl : ComObject
{
	public PrintControl(Device device, ImagingFactory wicFactory, ComObject documentTarget)
	{
		device.CreatePrintControl(wicFactory, documentTarget, null, this);
	}

	public PrintControl(Device device, ImagingFactory wicFactory, ComObject documentTarget, PrintControlProperties rintControlPropertiesRef)
	{
		device.CreatePrintControl(wicFactory, documentTarget, rintControlPropertiesRef, this);
	}

	public void AddPage(CommandList commandList, Size2F pageSize)
	{
		AddPage(commandList, pageSize, out var _, out var _);
	}

	public void AddPage(CommandList commandList, Size2F pageSize, out long tag1, out long tag2)
	{
		AddPage(commandList, pageSize, null, out tag1, out tag2);
	}

	public PrintControl(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator PrintControl(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new PrintControl(nativePtr);
		}
		return null;
	}

	public unsafe void AddPage(CommandList commandList, Size2F pageSize, IStream agePrintTicketStreamRef, out long tag1, out long tag2)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<CommandList>(commandList);
		zero2 = CppObject.ToCallbackPtr<IStream>(agePrintTicketStreamRef);
		Result result;
		fixed (long* ptr = &tag2)
		{
			void* ptr2 = ptr;
			fixed (long* ptr3 = &tag1)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, Size2F, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, pageSize, (void*)zero2, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void Close()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}
}
