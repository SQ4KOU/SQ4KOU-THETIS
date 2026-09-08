using System;

namespace SharpDX.WIC;

[Flags]
public enum BitmapTransformOptions
{
	Rotate0 = 0,
	Rotate90 = 1,
	Rotate180 = 2,
	Rotate270 = Rotate90 | Rotate180,
	FlipHorizontal = 8,
	FlipVertical = 0x10
}
