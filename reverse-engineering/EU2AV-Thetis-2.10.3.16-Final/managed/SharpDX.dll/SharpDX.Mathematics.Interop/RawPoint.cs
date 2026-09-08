using System.Diagnostics;

namespace SharpDX.Mathematics.Interop;

[DebuggerDisplay("X: {X}, Y: {Y}")]
public struct RawPoint(int x, int y)
{
	public int X = x;

	public int Y = y;
}
