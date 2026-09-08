using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDX.Mathematics.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("Left: {Left}, Top: {Top}, Right: {Right}, Bottom: {Bottom}")]
public struct RawRectangle(int left, int top, int right, int bottom)
{
	public int Left = left;

	public int Top = top;

	public int Right = right;

	public int Bottom = bottom;

	public bool IsEmpty
	{
		get
		{
			if (Left == 0 && Top == 0 && Right == 0)
			{
				return Bottom == 0;
			}
			return false;
		}
	}
}
