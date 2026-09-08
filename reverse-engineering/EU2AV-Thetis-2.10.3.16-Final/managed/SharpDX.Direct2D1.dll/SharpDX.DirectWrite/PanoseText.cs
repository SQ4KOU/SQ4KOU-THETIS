using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct PanoseText
{
	public byte FamilyKind;

	public byte SerifStyle;

	public byte Weight;

	public byte Proportion;

	public byte Contrast;

	public byte StrokeVariation;

	public byte ArmStyle;

	public byte Letterform;

	public byte Midline;

	public byte XHeight;
}
