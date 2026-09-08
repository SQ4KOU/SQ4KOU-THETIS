using System;

namespace RawInput_dll;

internal struct RawInputDevice
{
	internal HidUsagePage UsagePage;

	internal HidUsage Usage;

	internal RawInputDeviceFlags Flags;

	internal IntPtr Target;

	public override string ToString()
	{
		return $"{UsagePage}/{Usage}, flags: {Flags}, target: {Target}";
	}
}
