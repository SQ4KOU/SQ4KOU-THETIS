using System;

namespace SharpDX.Direct2D1;

[Flags]
public enum ImageSourceLoadingOptions
{
	None = 0,
	ReleaseSource = 1,
	CacheOnDemand = 2
}
