using System;

namespace SharpDX.DXGI;

[Flags]
public enum WindowAssociationFlags
{
	IgnoreAll = 1,
	IgnoreAltEnter = 2,
	IgnorePrintScreen = 4,
	Valid = IgnoreAll | IgnoreAltEnter | IgnorePrintScreen,
	None = 0
}
