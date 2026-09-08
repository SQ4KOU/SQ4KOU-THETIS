using System;

namespace Thetis;

internal class UsbRelayStatusException : Exception
{
	public override string Message => "USB status not OK error.";
}
