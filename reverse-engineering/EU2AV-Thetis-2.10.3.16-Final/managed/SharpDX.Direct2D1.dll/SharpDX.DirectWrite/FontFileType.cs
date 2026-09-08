namespace SharpDX.DirectWrite;

public enum FontFileType
{
	Unknown = 0,
	Cff = 1,
	Truetype = 2,
	OpenTypeCollection = 3,
	Type1Pfm = 4,
	Type1Pfb = 5,
	Vector = 6,
	Bitmap = 7,
	TruetypeCollection = OpenTypeCollection
}
