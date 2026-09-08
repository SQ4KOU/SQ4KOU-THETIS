using System;

namespace SharpDX.DXGI;

[Flags]
public enum DebugRloFlags
{
	Summary = 1,
	Detail = 2,
	IgnoreInternal = 4,
	All = Summary | Detail | IgnoreInternal,
	None = 0
}
