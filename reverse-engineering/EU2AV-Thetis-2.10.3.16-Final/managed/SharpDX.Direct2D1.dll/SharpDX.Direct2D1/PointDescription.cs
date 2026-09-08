using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct PointDescription
{
	public RawVector2 Point;

	public RawVector2 UnitTangentVector;

	public int EndSegment;

	public int EndFigure;

	public float LengthToEndSegment;
}
