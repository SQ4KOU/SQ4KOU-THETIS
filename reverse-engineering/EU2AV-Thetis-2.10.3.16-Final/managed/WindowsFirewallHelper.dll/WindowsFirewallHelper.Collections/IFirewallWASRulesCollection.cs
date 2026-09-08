using System.Collections;
using System.Collections.Generic;
using WindowsFirewallHelper.FirewallRules;

namespace WindowsFirewallHelper.Collections;

public interface IFirewallWASRulesCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
	FirewallWASRule this[string name] { get; }

	bool Remove(string name);
}
