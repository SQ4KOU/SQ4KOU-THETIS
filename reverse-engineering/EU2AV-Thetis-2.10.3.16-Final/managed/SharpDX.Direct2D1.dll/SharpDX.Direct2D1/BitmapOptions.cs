using System;

namespace SharpDX.Direct2D1;

[Flags]
public enum BitmapOptions
{
	None = 0,
	Target = 1,
	CannotDraw = 2,
	CpuRead = 4,
	GdiCompatible = 8
}
