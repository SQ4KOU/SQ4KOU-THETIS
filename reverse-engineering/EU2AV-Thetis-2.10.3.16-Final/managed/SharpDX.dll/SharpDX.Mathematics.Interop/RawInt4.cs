using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("X: {X}, Y: {Y}, Z: {Z}, W: {W}")]
public struct RawInt4(int x, int y, int z, int w)
{
	public int X = x;

	public int Y = y;

	public int Z = z;

	public int W = w;
}
