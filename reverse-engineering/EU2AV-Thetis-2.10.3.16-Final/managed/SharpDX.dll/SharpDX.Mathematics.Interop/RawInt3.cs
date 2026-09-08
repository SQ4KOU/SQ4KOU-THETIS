using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("X: {X}, Y: {Y}, Z: {Z}")]
public struct RawInt3(int x, int y, int z)
{
	public int X = x;

	public int Y = y;

	public int Z = z;
}
