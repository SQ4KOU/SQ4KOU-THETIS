using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct PanoseSymbol
{
	public byte FamilyKind;

	public byte SymbolKind;

	public byte Weight;

	public byte Spacing;

	public byte AspectRatioAndContrast;

	public byte AspectRatio94;

	public byte AspectRatio119;

	public byte AspectRatio157;

	public byte AspectRatio163;

	public byte AspectRatio211;
}
