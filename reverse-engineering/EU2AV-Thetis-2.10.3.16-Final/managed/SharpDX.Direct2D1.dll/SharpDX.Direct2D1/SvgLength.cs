using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct SvgLength
{
	public float Value;

	public SvgLengthUnits Units;
}
