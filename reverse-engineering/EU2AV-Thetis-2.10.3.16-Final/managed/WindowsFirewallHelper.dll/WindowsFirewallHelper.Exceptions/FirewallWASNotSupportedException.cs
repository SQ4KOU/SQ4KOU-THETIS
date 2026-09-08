using System;

namespace WindowsFirewallHelper.Exceptions;

public class FirewallWASNotSupportedException : NotSupportedException
{
	public FirewallWASNotSupportedException()
		: this("Specific operation is not supported when working with Windows Firewall With Advanced Security.")
	{
	}

	public FirewallWASNotSupportedException(string message)
		: base(message)
	{
	}
}
