using System;

namespace Thetis;

internal class UsbRelayDeviceNotFoundException : Exception
{
	public override string Message => "USB relay not found.";
}
