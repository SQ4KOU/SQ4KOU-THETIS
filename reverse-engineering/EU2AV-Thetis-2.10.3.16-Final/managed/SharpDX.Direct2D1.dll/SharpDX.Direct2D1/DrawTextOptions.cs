using System;

namespace SharpDX.Direct2D1;

[Flags]
public enum DrawTextOptions
{
	NoSnap = 1,
	Clip = 2,
	EnableColorFont = 4,
	DisableColorBitmapSnapping = 8,
	None = 0
}
