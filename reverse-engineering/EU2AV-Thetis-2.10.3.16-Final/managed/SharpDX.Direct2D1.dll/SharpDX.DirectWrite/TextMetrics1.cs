using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct TextMetrics1
{
	public float Left;

	public float Top;

	public float Width;

	public float WidthIncludingTrailingWhitespace;

	public float Height;

	public float LayoutWidth;

	public float LayoutHeight;

	public int MaxBidiReorderingDepth;

	public int LineCount;

	public float HeightIncludingTrailingWhitespace;
}
