using System;

namespace WindowsFirewallHelper;

[Flags]
public enum FirewallProfiles
{
	Domain = 1,
	Private = 2,
	Public = 4
}
