using System;

namespace WindowsFirewallHelper;

[Flags]
public enum NetworkInterfaceTypes
{
	RemoteAccess = 1,
	Wireless = 2,
	Lan = 4
}
