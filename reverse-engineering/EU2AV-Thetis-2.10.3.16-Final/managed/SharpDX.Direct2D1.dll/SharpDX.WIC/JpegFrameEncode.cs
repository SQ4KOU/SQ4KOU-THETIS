using System;
using System.Runtime.InteropServices;
using SharpDX.DXGI;

namespace SharpDX.WIC;

[Guid("2F0C601F-D2C6-468C-ABFA-49495D983ED1")]
public class JpegFrameEncode : ComObject
{
	public JpegFrameEncode(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator JpegFrameEncode(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new JpegFrameEncode(nativePtr);
		}
		return null;
	}

	public unsafe void GetAcHuffmanTable(int scanIndex, int tableIndex, out JpegAcHuffmanTable acHuffmanTableRef)
	{
		JpegAcHuffmanTable.__Native @ref = default(JpegAcHuffmanTable.__Native);
		acHuffmanTableRef = default(JpegAcHuffmanTable);
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, scanIndex, tableIndex, &@ref);
		acHuffmanTableRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}

	public unsafe void GetDcHuffmanTable(int scanIndex, int tableIndex, out JpegDeviceContextHuffmanTable dcHuffmanTableRef)
	{
		JpegDeviceContextHuffmanTable.__Native @ref = default(JpegDeviceContextHuffmanTable.__Native);
		dcHuffmanTableRef = default(JpegDeviceContextHuffmanTable);
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, scanIndex, tableIndex, &@ref);
		dcHuffmanTableRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}

	public unsafe void GetQuantizationTable(int scanIndex, int tableIndex, out JpegQuantizationTable quantizationTableRef)
	{
		JpegQuantizationTable.__Native @ref = default(JpegQuantizationTable.__Native);
		quantizationTableRef = default(JpegQuantizationTable);
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, scanIndex, tableIndex, &@ref);
		quantizationTableRef.__MarshalFrom(ref @ref);
		result.CheckError();
	}

	public unsafe void WriteScan(int scanData, byte[] scanDataRef)
	{
		Result result;
		fixed (byte* ptr = scanDataRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, scanData, ptr2);
		}
		result.CheckError();
	}
}
