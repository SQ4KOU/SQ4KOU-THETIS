using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WindowsFirewallHelper;

public interface IFirewall
{
	string Name { get; }

	ReadOnlyCollection<IFirewallProfile> Profiles { get; }

	ICollection<IFirewallRule> Rules { get; }

	COMTypeResolver TypeResolver { get; }

	IFirewallRule CreateApplicationRule(FirewallProfiles profiles, string name, FirewallAction action, string filename, FirewallProtocol protocol);

	IFirewallRule CreateApplicationRule(FirewallProfiles profiles, string name, FirewallAction action, string filename);

	IFirewallRule CreateApplicationRule(FirewallProfiles profiles, string name, string filename);

	IFirewallRule CreateApplicationRule(string name, FirewallAction action, string filename, FirewallProtocol protocol);

	IFirewallRule CreateApplicationRule(string name, FirewallAction action, string filename);

	IFirewallRule CreateApplicationRule(string name, string filename);

	IFirewallRule CreatePortRule(FirewallProfiles profiles, string name, FirewallAction action, ushort portNumber, FirewallProtocol protocol);

	IFirewallRule CreatePortRule(FirewallProfiles profiles, string name, FirewallAction action, ushort portNumber);

	IFirewallRule CreatePortRule(FirewallProfiles profiles, string name, ushort portNumber);

	IFirewallRule CreatePortRule(string name, FirewallAction action, ushort portNumber, FirewallProtocol protocol);

	IFirewallRule CreatePortRule(string name, FirewallAction action, ushort portNumber);

	IFirewall Reload();

	IFirewallRule CreatePortRule(string name, ushort portNumber);

	IFirewallProfile GetActiveProfile();

	IFirewallProfile GetProfile(FirewallProfiles profile);
}
