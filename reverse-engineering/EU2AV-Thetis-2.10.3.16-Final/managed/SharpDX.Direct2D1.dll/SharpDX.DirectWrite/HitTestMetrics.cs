using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct HitTestMetrics
{
	public int TextPosition;

	public int Length;

	public float Left;

	public float Top;

	public float Width;

	public float Height;

	public int BidiLevel;

	public RawBool IsText;

	public RawBool IsTrimmed;
}
