using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WindowsFirewallHelper.COMInterop;
using WindowsFirewallHelper.FirewallRules;

namespace WindowsFirewallHelper.Collections;

internal class FirewallLegacyRulesCollection : IFirewallLegacyRulesCollection, ICollection<IFirewallRule>, IEnumerable<IFirewallRule>, IEnumerable
{
	private readonly FirewallLegacy _firewall;

	private readonly Dictionary<FirewallProfiles, FirewallLegacyApplicationCollection> _firewallApplicationCollections;

	private readonly Dictionary<FirewallProfiles, FirewallLegacyPortCollection> _firewallPortCollections;

	private readonly Dictionary<FirewallProfiles, FirewallLegacyServiceCollection> _firewallServiceCollections;

	public int Count
	{
		get
		{
			int num = 0;
			using IEnumerator<IFirewallRule> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				_ = enumerator.Current;
				num++;
			}
			return num;
		}
	}

	public bool IsReadOnly
	{
		get
		{
			if (!_firewallApplicationCollections.Any((KeyValuePair<FirewallProfiles, FirewallLegacyApplicationCollection> pair) => pair.Value.IsReadOnly))
			{
				return _firewallPortCollections.Any((KeyValuePair<FirewallProfiles, FirewallLegacyPortCollection> pair) => pair.Value.IsReadOnly);
			}
			return true;
		}
	}

	public FirewallLegacyApplicationRule this[string applicationPath]
	{
		get
		{
			Dictionary<FirewallProfiles, INetFwAuthorizedApplication[]> dictionary = (from pair in _firewallApplicationCollections.ToDictionary((KeyValuePair<FirewallProfiles, FirewallLegacyApplicationCollection> pair) => pair.Key, (KeyValuePair<FirewallProfiles, FirewallLegacyApplicationCollection> pair) => pair.Value[applicationPath])
				where pair.Value != null
				select pair).ToDictionary((KeyValuePair<FirewallProfiles, INetFwAuthorizedApplication> pair) => pair.Key, (KeyValuePair<FirewallProfiles, INetFwAuthorizedApplication> pair) => new INetFwAuthorizedApplication[1] { pair.Value });
			if (dictionary.Count == 0)
			{
				return null;
			}
			return new FirewallLegacyApplicationRule(dictionary);
		}
	}

	public FirewallLegacyPortRule this[ushort portNumber, NetFwIPProtocol protocol]
	{
		get
		{
			FirewallLegacyPortCollectionKey key = new FirewallLegacyPortCollectionKey(portNumber, protocol);
			Dictionary<FirewallProfiles, INetFwOpenPort[]> dictionary = (from pair in _firewallPortCollections.ToDictionary((KeyValuePair<FirewallProfiles, FirewallLegacyPortCollection> pair) => pair.Key, (KeyValuePair<FirewallProfiles, FirewallLegacyPortCollection> pair) => pair.Value[key])
				where pair.Value != null
				select pair).ToDictionary((KeyValuePair<FirewallProfiles, INetFwOpenPort> pair) => pair.Key, (KeyValuePair<FirewallProfiles, INetFwOpenPort> pair) => new INetFwOpenPort[1] { pair.Value });
			if (dictionary.Count == 0)
			{
				return null;
			}
			return new FirewallLegacyPortRule(dictionary, _firewall.TypeResolver);
		}
	}

	public FirewallLegacyRulesCollection(FirewallLegacyProfile[] profiles, FirewallLegacy firewall)
	{
		_firewall = firewall;
		_firewallPortCollections = profiles.ToDictionary((FirewallLegacyProfile profile) => profile.Type, (FirewallLegacyProfile profile) => new FirewallLegacyPortCollection(profile.UnderlyingObject.GloballyOpenPorts));
		_firewallApplicationCollections = profiles.ToDictionary((FirewallLegacyProfile profile) => profile.Type, (FirewallLegacyProfile profile) => new FirewallLegacyApplicationCollection(profile.UnderlyingObject.AuthorizedApplications));
		_firewallServiceCollections = profiles.ToDictionary((FirewallLegacyProfile profile) => profile.Type, (FirewallLegacyProfile profile) => new FirewallLegacyServiceCollection(profile.UnderlyingObject.Services));
	}

	public void Add(IFirewallRule rule)
	{
		if (rule is FirewallLegacyApplicationRule firewallLegacyApplicationRule)
		{
			{
				foreach (FirewallProfiles key in _firewallApplicationCollections.Keys)
				{
					if (firewallLegacyApplicationRule.Profiles.HasFlag(key))
					{
						INetFwAuthorizedApplication[] cOMObjects = firewallLegacyApplicationRule.GetCOMObjects(key);
						foreach (INetFwAuthorizedApplication item in cOMObjects)
						{
							_firewallApplicationCollections[key].Add(item);
						}
					}
				}
				return;
			}
		}
		if (rule is FirewallLegacyPortRule firewallLegacyPortRule)
		{
			{
				foreach (FirewallProfiles key2 in _firewallPortCollections.Keys)
				{
					if (firewallLegacyPortRule.Profiles.HasFlag(key2))
					{
						INetFwOpenPort[] cOMObjects2 = firewallLegacyPortRule.GetCOMObjects(key2);
						foreach (INetFwOpenPort item2 in cOMObjects2)
						{
							_firewallPortCollections[key2].Add(item2);
						}
					}
				}
				return;
			}
		}
		throw new ArgumentException("Invalid argument type passed.", "rule");
	}

	public void Clear()
	{
		throw new NotSupportedException();
	}

	public bool Contains(IFirewallRule item)
	{
		return this.Any((IFirewallRule rule) => rule.Equals(item));
	}

	public void CopyTo(IFirewallRule[] array, int arrayIndex)
	{
		List<IFirewallRule> list = new List<IFirewallRule>();
		using (IEnumerator<IFirewallRule> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				IFirewallRule current = enumerator.Current;
				list.Add(current);
			}
		}
		Array.Copy(list.ToArray(), 0, array, arrayIndex, list.Count);
	}

	public bool Remove(IFirewallRule rule)
	{
		bool flag = false;
		if (rule is FirewallLegacyApplicationRule firewallLegacyApplicationRule)
		{
			foreach (FirewallProfiles key in _firewallApplicationCollections.Keys)
			{
				if (firewallLegacyApplicationRule.Profiles.HasFlag(key))
				{
					INetFwAuthorizedApplication[] cOMObjects = firewallLegacyApplicationRule.GetCOMObjects(key);
					foreach (INetFwAuthorizedApplication item in cOMObjects)
					{
						flag = _firewallApplicationCollections[key].Remove(item) | flag;
					}
				}
			}
		}
		else
		{
			if (!(rule is FirewallLegacyPortRule firewallLegacyPortRule))
			{
				throw new ArgumentException("Invalid argument type passed.", "rule");
			}
			foreach (FirewallProfiles key2 in _firewallPortCollections.Keys)
			{
				if (firewallLegacyPortRule.Profiles.HasFlag(key2))
				{
					INetFwOpenPort[] cOMObjects2 = firewallLegacyPortRule.GetCOMObjects(key2);
					foreach (INetFwOpenPort item2 in cOMObjects2)
					{
						flag = _firewallPortCollections[key2].Remove(item2) | flag;
					}
				}
			}
		}
		return flag;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<IFirewallRule> GetEnumerator()
	{
		IEnumerable<IFirewallRule> first = (from arg in _firewallApplicationCollections.SelectMany((KeyValuePair<FirewallProfiles, FirewallLegacyApplicationCollection> pair) => pair.Value.Select((INetFwAuthorizedApplication rule) => new
			{
				Profile = pair.Key,
				Rule = rule
			}))
			group arg by Tuple.Create(arg.Rule.ProcessImageFileName, arg.Rule.RemoteAddresses, arg.Rule.Scope, arg.Rule.IpVersion) into @group
			select from arg in @group
				group arg by arg.Profile into @group
			select new FirewallLegacyApplicationRule(@group.ToDictionary(t => t.Key, t => t.Select(arg => arg.Rule).ToArray()))).OfType<IFirewallRule>();
		IEnumerable<IFirewallRule> second = (from arg in _firewallPortCollections.SelectMany((KeyValuePair<FirewallProfiles, FirewallLegacyPortCollection> pair) => pair.Value.Select((INetFwOpenPort rule) => new
			{
				Profile = pair.Key,
				Rule = rule
			}))
			group arg by Tuple.Create(arg.Rule.Port, arg.Rule.Protocol, arg.Rule.Scope, arg.Rule.RemoteAddresses, arg.Rule.BuiltIn, arg.Rule.IpVersion) into @group
			select from arg in @group
				group arg by arg.Profile into @group
			select new FirewallLegacyPortRule(@group.ToDictionary(t => t.Key, t => t.Select(arg => arg.Rule).ToArray()), _firewall.TypeResolver)).OfType<IFirewallRule>();
		return first.Concat(second).GetEnumerator();
	}

	public bool Remove(ushort portNumber, NetFwIPProtocol protocol)
	{
		FirewallLegacyPortCollectionKey key = new FirewallLegacyPortCollectionKey(portNumber, protocol);
		return _firewallPortCollections.Select((KeyValuePair<FirewallProfiles, FirewallLegacyPortCollection> pair) => pair.Value.Remove(key)).Any((bool success) => success);
	}

	public bool Remove(string applicationPath)
	{
		return _firewallApplicationCollections.Select((KeyValuePair<FirewallProfiles, FirewallLegacyApplicationCollection> pair) => pair.Value.Remove(applicationPath)).Any((bool success) => success);
	}
}
