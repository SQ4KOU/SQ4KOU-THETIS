using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct PanoseScript
{
	public byte FamilyKind;

	public byte ToolKind;

	public byte Weight;

	public byte Spacing;

	public byte AspectRatio;

	public byte Contrast;

	public byte ScriptTopology;

	public byte ScriptForm;

	public byte Finials;

	public byte XAscent;
}
