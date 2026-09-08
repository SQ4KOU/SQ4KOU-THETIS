using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct PanoseDecorative
{
	public byte FamilyKind;

	public byte DecorativeClass;

	public byte Weight;

	public byte Aspect;

	public byte Contrast;

	public byte SerifVariant;

	public byte Fill;

	public byte Lining;

	public byte DecorativeTopology;

	public byte CharacterRange;
}
