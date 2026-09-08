using System.Collections;
using System.Collections.Generic;
using WindowsFirewallHelper.COMInterop;
using WindowsFirewallHelper.FirewallRules;

namespace WindowsFirewallHelper.Collections;

public interface IFirewallLegacyRulesCollection : ICollection<IFirewallRule>, IEnumerable<IFirewallRule>, IEnumerable
{
	FirewallLegacyApplicationRule this[string applicationPath] { get; }

	FirewallLegacyPortRule this[ushort portNumber, NetFwIPProtocol protocol] { get; }

	bool Remove(ushort portNumber, NetFwIPProtocol protocol);

	bool Remove(string applicationPath);
}
