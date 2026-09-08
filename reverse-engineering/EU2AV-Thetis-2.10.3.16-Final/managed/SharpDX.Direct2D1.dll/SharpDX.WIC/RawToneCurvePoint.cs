using System.Runtime.InteropServices;

namespace SharpDX.WIC;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct RawToneCurvePoint
{
	public double Input;

	public double Output;
}
