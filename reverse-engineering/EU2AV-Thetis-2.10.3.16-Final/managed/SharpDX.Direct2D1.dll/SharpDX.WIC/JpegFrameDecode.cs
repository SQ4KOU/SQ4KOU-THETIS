using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace SharpDX.WIC;

[Guid("8939F66E-C46A-4c21-A9D1-98B327CE1679")]
public class JpegFrameDecode : ComObject
{
	public JpegFrameHeader FrameHeader
	{
		get
		{
			GetFrameHeader(out var frameHeaderRef);
			return frameHeaderRef;
		}
	}

	public JpegFrameDecode(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator JpegFrameDecode(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new JpegFrameDecode(nativePtr);
		}
		return null;
	}

	public unsafe void DoesSupportIndexing(out RawBool fIndexingSupportedRef)
	{
		fIndexingSupportedRef = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &fIndexingSupportedRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void SetIndexing(JpegIndexingOptions options, int horizontalIntervalSize)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, (int)options, horizontalIntervalSize)).CheckError();
	}

	public unsafe void ClearIndexing()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public unsafe void GetAcHuffmanTable(int scanIndex, int tableIndex, out JpegAcHuffmanTable acHuffmanTableRef)
	{
		JpegAcHuffmanTable.__Native @ref = default(JpegAcHuffmanTable.__Native);
		acHuffmanTableRef = default(JpegAcHuffmanTable);
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, scanIndex, tableIndex, &@ref);
		acHuffmanTableRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}

	public unsafe void GetDcHuffmanTable(int scanIndex, int tableIndex, out JpegDeviceContextHuffmanTable dcHuffmanTableRef)
	{
		JpegDeviceContextHuffmanTable.__Native @ref = default(JpegDeviceContextHuffmanTable.__Native);
		dcHuffmanTableRef = default(JpegDeviceContextHuffmanTable);
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, scanIndex, tableIndex, &@ref);
		dcHuffmanTableRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}

	public unsafe void GetQuantizationTable(int scanIndex, int tableIndex, out JpegQuantizationTable quantizationTableRef)
	{
		JpegQuantizationTable.__Native @ref = default(JpegQuantizationTable.__Native);
		quantizationTableRef = default(JpegQuantizationTable);
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, scanIndex, tableIndex, &@ref);
		quantizationTableRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}

	internal unsafe void GetFrameHeader(out JpegFrameHeader frameHeaderRef)
	{
		frameHeaderRef = default(JpegFrameHeader);
		Result result;
		fixed (JpegFrameHeader* ptr = &frameHeaderRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetScanHeader(int scanIndex, out JpegScanHeader scanHeaderRef)
	{
		scanHeaderRef = default(JpegScanHeader);
		Result result;
		fixed (JpegScanHeader* ptr = &scanHeaderRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, scanIndex, ptr2);
		}
		result.CheckError();
	}

	public unsafe void CopyScan(int scanIndex, int scanOffset, int scanData, byte[] scanDataRef, out int scanDataActualRef)
	{
		Result result;
		fixed (int* ptr = &scanDataActualRef)
		{
			void* ptr2 = ptr;
			fixed (byte* ptr3 = scanDataRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, scanIndex, scanOffset, scanData, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void CopyMinimalStream(int streamOffset, int streamData, byte[] streamDataRef, out int streamDataActualRef)
	{
		Result result;
		fixed (int* ptr = &streamDataActualRef)
		{
			void* ptr2 = ptr;
			fixed (byte* ptr3 = streamDataRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, streamOffset, streamData, ptr4, ptr2);
			}
		}
		result.CheckError();
	}
}
