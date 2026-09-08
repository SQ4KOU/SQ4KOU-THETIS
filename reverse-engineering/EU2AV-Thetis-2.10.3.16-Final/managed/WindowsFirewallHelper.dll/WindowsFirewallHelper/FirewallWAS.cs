using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WindowsFirewallHelper.COMInterop;
using WindowsFirewallHelper.Collections;
using WindowsFirewallHelper.Exceptions;
using WindowsFirewallHelper.FirewallRules;
using WindowsFirewallHelper.InternalHelpers;

namespace WindowsFirewallHelper;

public class FirewallWAS : IFirewall
{
	public COMTypeResolver TypeResolver { get; }

	public static FirewallWAS Instance => ThreadSafeSingleton.GetInstance<FirewallWAS>();

	public static bool IsLocallySupported => IsSupported(new COMTypeResolver());

	public FirewallModifyStatePolicy LocalModifyStatePolicy => (FirewallModifyStatePolicy)UnderlyingObject.LocalPolicyModifyState;

	public ReadOnlyCollection<FirewallWASProfile> Profiles { get; }

	public IEnumerable<FirewallWASRuleGroup> RuleGroups => from s in (from rule in Rules
			select rule.Grouping into s
			where !string.IsNullOrWhiteSpace(s)
			select s).Distinct()
		select new FirewallWASRuleGroup(this, s);

	public IFirewallWASRulesCollection<FirewallWASRule> Rules => new FirewallWASRulesCollection<FirewallWASRule>(UnderlyingObject.Rules);

	internal INetFwPolicy2 UnderlyingObject { get; private set; }

	public string Name => "Windows Firewall with Advanced Security";

	ReadOnlyCollection<IFirewallProfile> IFirewall.Profiles => new ReadOnlyCollection<IFirewallProfile>(Profiles.Cast<IFirewallProfile>().ToArray());

	ICollection<IFirewallRule> IFirewall.Rules => new FirewallWASRulesCollection<IFirewallRule>(UnderlyingObject.Rules);

	public FirewallWAS()
		: this(new COMTypeResolver())
	{
	}

	public FirewallWAS(COMTypeResolver typeResolver)
	{
		if (!IsSupported(typeResolver))
		{
			throw new NotSupportedException("This type is not supported in this environment.");
		}
		TypeResolver = typeResolver;
		UnderlyingObject = TypeResolver.CreateInstance<INetFwPolicy2>();
		Profiles = new ReadOnlyCollection<FirewallWASProfile>(new FirewallWASProfile[3]
		{
			new FirewallWASProfile(this, NetFwProfileType2.Domain),
			new FirewallWASProfile(this, NetFwProfileType2.Private),
			new FirewallWASProfile(this, NetFwProfileType2.Public)
		});
	}

	public IFirewall Reload()
	{
		UnderlyingObject = TypeResolver.CreateInstance<INetFwPolicy2>();
		return this;
	}

	public static bool IsSupported(COMTypeResolver typeResolver)
	{
		return typeResolver.IsSupported<INetFwPolicy2>();
	}

	IFirewallRule IFirewall.CreateApplicationRule(FirewallProfiles profiles, string name, FirewallAction action, string filename, FirewallProtocol protocol)
	{
		return CreateApplicationRule(profiles, name, action, FirewallDirection.Inbound, filename, protocol);
	}

	IFirewallRule IFirewall.CreateApplicationRule(FirewallProfiles profiles, string name, FirewallAction action, string filename)
	{
		return ((IFirewall)this).CreateApplicationRule(profiles, name, action, filename, FirewallProtocol.Any);
	}

	IFirewallRule IFirewall.CreateApplicationRule(FirewallProfiles profiles, string name, string filename)
	{
		return ((IFirewall)this).CreateApplicationRule(profiles, name, FirewallAction.Allow, filename);
	}

	IFirewallRule IFirewall.CreateApplicationRule(string name, FirewallAction action, string filename, FirewallProtocol protocol)
	{
		FirewallWASProfile activeProfile = GetActiveProfile();
		if (activeProfile == null)
		{
			throw new InvalidOperationException("No firewall profile is currently active.");
		}
		return ((IFirewall)this).CreateApplicationRule(activeProfile.Type, name, action, filename, FirewallProtocol.Any);
	}

	IFirewallRule IFirewall.CreateApplicationRule(string name, FirewallAction action, string filename)
	{
		return ((IFirewall)this).CreateApplicationRule(name, action, filename, FirewallProtocol.Any);
	}

	IFirewallRule IFirewall.CreateApplicationRule(string name, string filename)
	{
		return ((IFirewall)this).CreateApplicationRule(name, FirewallAction.Allow, filename);
	}

	IFirewallRule IFirewall.CreatePortRule(FirewallProfiles profiles, string name, FirewallAction action, ushort portNumber, FirewallProtocol protocol)
	{
		return CreatePortRule(profiles, name, action, FirewallDirection.Inbound, portNumber, protocol);
	}

	IFirewallRule IFirewall.CreatePortRule(FirewallProfiles profiles, string name, FirewallAction action, ushort portNumber)
	{
		return ((IFirewall)this).CreatePortRule(profiles, name, action, portNumber, FirewallProtocol.TCP);
	}

	IFirewallRule IFirewall.CreatePortRule(FirewallProfiles profiles, string name, ushort portNumber)
	{
		return ((IFirewall)this).CreatePortRule(profiles, name, FirewallAction.Allow, portNumber);
	}

	IFirewallRule IFirewall.CreatePortRule(string name, FirewallAction action, ushort portNumber, FirewallProtocol protocol)
	{
		FirewallWASProfile activeProfile = GetActiveProfile();
		if (activeProfile == null)
		{
			throw new InvalidOperationException("No firewall profile is currently active.");
		}
		return ((IFirewall)this).CreatePortRule(activeProfile.Type, name, action, portNumber, FirewallProtocol.TCP);
	}

	IFirewallRule IFirewall.CreatePortRule(string name, FirewallAction action, ushort portNumber)
	{
		return ((IFirewall)this).CreatePortRule(name, action, portNumber, FirewallProtocol.TCP);
	}

	IFirewallRule IFirewall.CreatePortRule(string name, ushort portNumber)
	{
		return ((IFirewall)this).CreatePortRule(name, FirewallAction.Allow, portNumber);
	}

	IFirewallProfile IFirewall.GetActiveProfile()
	{
		return GetActiveProfile();
	}

	IFirewallProfile IFirewall.GetProfile(FirewallProfiles profile)
	{
		return GetProfile(profile);
	}

	public FirewallWASRule CreateApplicationRule(FirewallProfiles profiles, string name, FirewallAction action, FirewallDirection direction, string filename, FirewallProtocol protocol)
	{
		if (FirewallWASRuleWin8.IsSupported(TypeResolver))
		{
			return new FirewallWASRuleWin8(name, filename, action, direction, profiles, TypeResolver)
			{
				Protocol = protocol
			};
		}
		if (FirewallWASRuleWin7.IsSupported(TypeResolver))
		{
			return new FirewallWASRuleWin7(name, filename, action, direction, profiles, TypeResolver)
			{
				Protocol = protocol
			};
		}
		if (FirewallWASRule.IsSupported(TypeResolver))
		{
			return new FirewallWASRule(name, filename, action, direction, profiles, TypeResolver)
			{
				Protocol = protocol
			};
		}
		throw new FirewallWASNotSupportedException();
	}

	public FirewallWASRule CreateApplicationRule(string name, FirewallAction action, FirewallDirection direction, string filename, FirewallProtocol protocol)
	{
		FirewallWASProfile activeProfile = GetActiveProfile();
		if (activeProfile == null)
		{
			throw new InvalidOperationException("No firewall profile is currently active.");
		}
		return CreateApplicationRule(activeProfile.Type, name, action, direction, filename, protocol);
	}

	public FirewallWASRule CreatePortRule(FirewallProfiles profiles, string name, FirewallAction action, FirewallDirection direction, ushort portNumber, FirewallProtocol protocol)
	{
		if (!protocol.Equals(FirewallProtocol.TCP) && !protocol.Equals(FirewallProtocol.UDP))
		{
			throw new FirewallWASInvalidProtocolException("Invalid protocol selected; rule's protocol should be TCP or UDP.");
		}
		if (FirewallWASRuleWin8.IsSupported(TypeResolver))
		{
			return new FirewallWASRuleWin8(name, portNumber, action, direction, profiles, TypeResolver)
			{
				Protocol = protocol
			};
		}
		if (FirewallWASRuleWin7.IsSupported(TypeResolver))
		{
			return new FirewallWASRuleWin7(name, portNumber, action, direction, profiles, TypeResolver)
			{
				Protocol = protocol
			};
		}
		if (FirewallWASRule.IsSupported(TypeResolver))
		{
			return new FirewallWASRule(name, portNumber, action, direction, profiles, TypeResolver)
			{
				Protocol = protocol
			};
		}
		throw new FirewallWASNotSupportedException();
	}

	public FirewallWASRule CreatePortRule(string name, FirewallAction action, FirewallDirection direction, ushort portNumber, FirewallProtocol protocol)
	{
		FirewallWASProfile activeProfile = GetActiveProfile();
		if (activeProfile == null)
		{
			throw new InvalidOperationException("No firewall profile is currently active.");
		}
		return CreatePortRule(activeProfile.Type, name, action, direction, portNumber, protocol);
	}

	public FirewallWASProfile GetActiveProfile()
	{
		return Profiles.FirstOrDefault((FirewallWASProfile p) => p.IsActive);
	}

	public FirewallWASRuleGroup GetGroupByName(string name)
	{
		return new FirewallWASRuleGroup(this, name);
	}

	public FirewallWASProfile GetProfile(FirewallProfiles profile)
	{
		return Profiles.FirstOrDefault((FirewallWASProfile p) => p.Type == profile) ?? throw new FirewallWASNotSupportedException();
	}

	public void ResetDefault()
	{
		UnderlyingObject.RestoreLocalFirewallDefaults();
	}
}
