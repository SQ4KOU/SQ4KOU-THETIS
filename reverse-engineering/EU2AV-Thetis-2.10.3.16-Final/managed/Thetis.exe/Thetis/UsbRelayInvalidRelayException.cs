using System;

namespace Thetis;

internal class UsbRelayInvalidRelayException : Exception
{
	public override string Message => "USB relay invalid relay number.";
}
