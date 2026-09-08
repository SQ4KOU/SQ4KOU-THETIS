using System;

namespace WindowsFirewallHelper.Exceptions;

public class FirewallWASInvalidProtocolException : InvalidOperationException
{
	public FirewallWASInvalidProtocolException(string message)
		: base(message)
	{
	}
}
