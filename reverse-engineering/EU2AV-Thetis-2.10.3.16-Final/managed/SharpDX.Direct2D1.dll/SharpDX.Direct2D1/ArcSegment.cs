using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct ArcSegment
{
	public RawVector2 Point;

	public Size2F Size;

	public float RotationAngle;

	public SweepDirection SweepDirection;

	public ArcSize ArcSize;
}
