using System;

namespace WindowsFirewallHelper.Exceptions;

public class FirewallLegacyNotSupportedException : NotSupportedException
{
	public FirewallLegacyNotSupportedException()
		: this("Specific operation is not supported when working with Windows Firewall Legacy.")
	{
	}

	public FirewallLegacyNotSupportedException(string message)
		: base(message)
	{
	}
}
