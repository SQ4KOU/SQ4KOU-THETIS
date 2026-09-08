using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.WIC;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct BitmapPattern
{
	public long Position;

	public int Length;

	public IntPtr Pattern;

	public IntPtr Mask;

	public RawBool EndOfStream;
}
