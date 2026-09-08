using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct MappedRectangle
{
	public int Pitch;

	public IntPtr Bits;
}
