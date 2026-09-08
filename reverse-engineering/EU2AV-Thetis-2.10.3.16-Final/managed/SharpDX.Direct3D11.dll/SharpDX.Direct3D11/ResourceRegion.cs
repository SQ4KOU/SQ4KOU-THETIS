using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct ResourceRegion(int left, int top, int front, int right, int bottom, int back)
{
	public int Left = left;

	public int Top = top;

	public int Front = front;

	public int Right = right;

	public int Bottom = bottom;

	public int Back = back;
}
