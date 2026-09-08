namespace SharpDX.DirectWrite;

public enum PanoseArmStyle
{
	Any = 0,
	NoFit = 1,
	StraightArmsHorizontal = 2,
	StraightArmsWedge = 3,
	StraightArmsVertical = 4,
	StraightArmsSingleSerif = 5,
	StraightArmsDoubleSerif = 6,
	NonstraightArmsHorizontal = 7,
	NonstraightArmsWedge = 8,
	NonstraightArmsVertical = 9,
	NonstraightArmsSingleSerif = 10,
	NonstraightArmsDoubleSerif = 11,
	StraightArmsHorz = StraightArmsHorizontal,
	StraightArmsVert = StraightArmsVertical,
	BentArmsHorz = NonstraightArmsHorizontal,
	BentArmsWedge = NonstraightArmsWedge,
	BentArmsVert = NonstraightArmsVertical,
	BentArmsSingleSerif = NonstraightArmsSingleSerif,
	BentArmsDoubleSerif = NonstraightArmsDoubleSerif
}
