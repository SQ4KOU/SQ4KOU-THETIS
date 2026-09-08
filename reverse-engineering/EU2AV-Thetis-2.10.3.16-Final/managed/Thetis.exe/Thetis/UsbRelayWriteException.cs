using System;

namespace Thetis;

internal class UsbRelayWriteException : Exception
{
	public override string Message => "USB relay write error.";
}
