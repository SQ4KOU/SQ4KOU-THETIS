namespace SharpDX.DirectWrite;

public enum FontFaceType
{
	Cff = 0,
	Truetype = 1,
	OpenTypeCollection = 2,
	Type1 = 3,
	Vector = 4,
	Bitmap = 5,
	Unknown = 6,
	RawCff = 7,
	TruetypeCollection = OpenTypeCollection
}
