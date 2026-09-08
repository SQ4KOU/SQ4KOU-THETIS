using System;

namespace SharpDX.Direct2D1;

[Flags]
public enum VertexOptions
{
	None = 0,
	DoNotClear = 1,
	UseDepthBuffer = 2,
	AssumeNoOverlap = 4
}
