using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct LineMetrics
{
	public int Length;

	public int TrailingWhitespaceLength;

	public int NewlineLength;

	public float Height;

	public float Baseline;

	public RawBool IsTrimmed;
}
