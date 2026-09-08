namespace SharpDX.DirectWrite;

public enum RenderingMode
{
	Default = 0,
	Aliased = 1,
	GdiClassic = 2,
	GdiNatural = 3,
	Natural = 4,
	NaturalSymmetric = 5,
	Outline = 6,
	CleartypeGdiClassic = GdiClassic,
	CleartypeGdiNatural = GdiNatural,
	CleartypeNatural = Natural,
	CleartypeNaturalSymmetric = NaturalSymmetric
}
