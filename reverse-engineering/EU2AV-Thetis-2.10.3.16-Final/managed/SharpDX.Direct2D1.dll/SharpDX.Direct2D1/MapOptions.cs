using System;

namespace SharpDX.Direct2D1;

[Flags]
public enum MapOptions
{
	None = 0,
	Read = 1,
	Write = 2,
	Discard = 4
}
