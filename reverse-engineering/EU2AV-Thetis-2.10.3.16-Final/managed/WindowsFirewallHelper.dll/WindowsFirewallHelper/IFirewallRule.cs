using System;

namespace WindowsFirewallHelper;

public interface IFirewallRule : IEquatable<IFirewallRule>
{
	FirewallAction Action { get; set; }

	string ApplicationName { get; set; }

	FirewallDirection Direction { get; set; }

	string FriendlyName { get; }

	bool IsEnable { get; set; }

	IAddress[] LocalAddresses { get; set; }

	ushort[] LocalPorts { get; set; }

	FirewallPortType LocalPortType { get; set; }

	string Name { get; set; }

	FirewallProfiles Profiles { get; }

	FirewallProtocol Protocol { get; set; }

	IAddress[] RemoteAddresses { get; set; }

	ushort[] RemotePorts { get; set; }

	FirewallScope Scope { get; set; }

	string ServiceName { get; set; }
}
