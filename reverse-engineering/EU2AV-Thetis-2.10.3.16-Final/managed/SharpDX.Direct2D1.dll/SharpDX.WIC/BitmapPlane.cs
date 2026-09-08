using System;
using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct BitmapPlane
{
	public Guid Format;

	public IntPtr PbBuffer;

	public int CbStride;

	public int CbBufferSize;
}
