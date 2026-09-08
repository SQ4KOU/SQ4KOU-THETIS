using System;

namespace Thetis;

internal class UsbRelayConfigurationException : Exception
{
	public override string Message => "USB relay configuration error.";
}
