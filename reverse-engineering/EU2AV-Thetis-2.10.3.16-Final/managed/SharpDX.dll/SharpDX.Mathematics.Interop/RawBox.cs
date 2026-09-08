using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("X: {X}, Y: {Y}, Width: {Width}, Height: {Height}")]
public struct RawBox(int x, int y, int width, int height)
{
	public int X = x;

	public int Y = y;

	public int Width = width;

	public int Height = height;
}
