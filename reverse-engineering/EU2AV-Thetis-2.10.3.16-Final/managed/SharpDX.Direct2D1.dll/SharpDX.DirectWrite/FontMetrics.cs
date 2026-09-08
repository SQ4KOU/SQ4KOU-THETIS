using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct FontMetrics
{
	public short DesignUnitsPerEm;

	public short Ascent;

	public short Descent;

	public short LineGap;

	public short CapHeight;

	public short XHeight;

	public short UnderlinePosition;

	public short UnderlineThickness;

	public short StrikethroughPosition;

	public short StrikethroughThickness;
}
