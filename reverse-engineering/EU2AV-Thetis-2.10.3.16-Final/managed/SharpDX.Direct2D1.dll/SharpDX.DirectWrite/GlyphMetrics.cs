using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct GlyphMetrics
{
	public int LeftSideBearing;

	public int AdvanceWidth;

	public int RightSideBearing;

	public int TopSideBearing;

	public int AdvanceHeight;

	public int BottomSideBearing;

	public int VerticalOriginY;
}
