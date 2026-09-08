using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct InlineObjectMetrics
{
	public float Width;

	public float Height;

	public float Baseline;

	public RawBool SupportsSideways;
}
