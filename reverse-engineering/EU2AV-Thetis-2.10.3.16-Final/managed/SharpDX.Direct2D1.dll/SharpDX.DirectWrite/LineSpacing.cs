using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct LineSpacing
{
	public LineSpacingMethod Method;

	public float Height;

	public float Baseline;

	public float LeadingBefore;

	public FontLineGapUsage FontLineGapUsage;
}
