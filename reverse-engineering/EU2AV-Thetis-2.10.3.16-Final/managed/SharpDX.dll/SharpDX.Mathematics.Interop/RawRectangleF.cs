using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("Left: {Left}, Top: {Top}, Right: {Right}, Bottom: {Bottom}")]
public struct RawRectangleF(float left, float top, float right, float bottom)
{
	public float Left = left;

	public float Top = top;

	public float Right = right;

	public float Bottom = bottom;
}
