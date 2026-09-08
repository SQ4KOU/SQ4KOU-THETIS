using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct LinearGradientBrushProperties
{
	public RawVector2 StartPoint;

	public RawVector2 EndPoint;
}
