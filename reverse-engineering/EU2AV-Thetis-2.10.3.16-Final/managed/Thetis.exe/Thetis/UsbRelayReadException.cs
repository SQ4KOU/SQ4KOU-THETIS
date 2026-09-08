using System;

namespace Thetis;

internal class UsbRelayReadException : Exception
{
	public override string Message => "USB relay read error.";
}
