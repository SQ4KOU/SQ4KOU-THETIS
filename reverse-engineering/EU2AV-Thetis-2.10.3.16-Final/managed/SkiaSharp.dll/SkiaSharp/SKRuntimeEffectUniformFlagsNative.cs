using System;

namespace SkiaSharp;

[Flags]
internal enum SKRuntimeEffectUniformFlagsNative
{
	None = 0,
	Array = 1,
	Color = 2,
	Vertex = 4,
	Fragment = 8,
	HalfPrecision = 0x10
}
