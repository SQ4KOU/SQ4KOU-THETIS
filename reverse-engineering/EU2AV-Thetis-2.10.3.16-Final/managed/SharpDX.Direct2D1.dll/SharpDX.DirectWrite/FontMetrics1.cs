using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct FontMetrics1
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

	public short GlyphBoxLeft;

	public short GlyphBoxTop;

	public short GlyphBoxRight;

	public short GlyphBoxBottom;

	public short SubscriptPositionX;

	public short SubscriptPositionY;

	public short SubscriptSizeX;

	public short SubscriptSizeY;

	public short SuperscriptPositionX;

	public short SuperscriptPositionY;

	public short SuperscriptSizeX;

	public short SuperscriptSizeY;

	public RawBool HasTypographicMetrics;
}
