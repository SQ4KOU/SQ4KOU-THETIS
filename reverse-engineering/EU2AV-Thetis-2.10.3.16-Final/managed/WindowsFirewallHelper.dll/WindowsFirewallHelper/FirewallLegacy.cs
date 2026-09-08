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

public class FirewallLegacy : IFirewall
{
	public COMTypeResolver TypeResolver { get; }

	public static FirewallLegacy Instance => ThreadSafeSingleton.GetInstance<FirewallLegacy>();

	public static bool IsLocallySupported => IsSupported(new COMTypeResolver());

	public ReadOnlyCollection<FirewallLegacyProfile> Profiles { get; }

	public IFirewallLegacyRulesCollection Rules => new FirewallLegacyRulesCollection(Profiles.ToArray(), this);

	internal INetFwMgr UnderlyingObject { get; private set; }

	public string Name => "Windows Firewall Legacy";

	ReadOnlyCollection<IFirewallProfile> IFirewall.Profiles => new ReadOnlyCollection<IFirewallProfile>(Profiles.Cast<IFirewallProfile>().ToArray());

	ICollection<IFirewallRule> IFirewall.Rules => Rules;

	public FirewallLegacy()
		: this(new COMTypeResolver())
	{
	}

	public FirewallLegacy(COMTypeResolver typeResolver)
	{
		if (!IsSupported(typeResolver))
		{
			throw new NotSupportedException("This type is not supported in this environment.");
		}
		TypeResolver = typeResolver;
		UnderlyingObject = TypeResolver.CreateInstance<INetFwMgr>();
		Profiles = new ReadOnlyCollection<FirewallLegacyProfile>(new FirewallLegacyProfile[2]
		{
			new FirewallLegacyProfile(this, FirewallProfiles.Domain),
			new FirewallLegacyProfile(this, FirewallProfiles.Private)
		});
	}

	public IFirewall Reload()
	{
		UnderlyingObject = TypeResolver.CreateInstance<INetFwMgr>();
		return this;
	}

	public static bool IsSupported(COMTypeResolver typeResolver)
	{
		return typeResolver.IsSupported<INetFwMgr>();
	}

	IFirewallRule IFirewall.CreateApplicationRule(FirewallProfiles profiles, string name, FirewallAction action, string filename, FirewallProtocol protocol)
	{
		if (!protocol.Equals(FirewallProtocol.Any))
		{
			throw new FirewallLegacyNotSupportedException("Application rules are only supported along with the `FirewallProtocol.Any` protocol in Windows Firewall Legacy.");
		}
		if (action != FirewallAction.Allow)
		{
			throw new FirewallLegacyNotSupportedException("Windows Firewall Legacy only accepts allow exception rules.");
		}
		return CreateApplicationRule(profiles, name, filename);
	}

	IFirewallRule IFirewall.CreateApplicationRule(FirewallProfiles profile, string name, FirewallAction action, string filename)
	{
		return ((IFirewall)this).CreateApplicationRule(profile, name, action, filename, FirewallProtocol.Any);
	}

	IFirewallRule IFirewall.CreateApplicationRule(FirewallProfiles profiles, string name, string filename)
	{
		return CreateApplicationRule(profiles, name, filename);
	}

	IFirewallRule IFirewall.CreateApplicationRule(string name, FirewallAction action, string filename, FirewallProtocol protocol)
	{
		FirewallLegacyProfile activeProfile = GetActiveProfile();
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
		return CreateApplicationRule(name, filename);
	}

	IFirewallRule IFirewall.CreatePortRule(FirewallProfiles profiles, string name, FirewallAction action, ushort portNumber, FirewallProtocol protocol)
	{
		if (action != FirewallAction.Allow)
		{
			throw new FirewallLegacyNotSupportedException("Windows Firewall Legacy only accepts allow exception rules.");
		}
		return new FirewallLegacyPortRule(name, portNumber, profiles, TypeResolver)
		{
			Protocol = protocol
		};
	}

	IFirewallRule IFirewall.CreatePortRule(FirewallProfiles profiles, string name, FirewallAction action, ushort portNumber)
	{
		return ((IFirewall)this).CreatePortRule(profiles, name, action, portNumber, FirewallProtocol.TCP);
	}

	IFirewallRule IFirewall.CreatePortRule(FirewallProfiles profiles, string name, ushort portNumber)
	{
		return CreatePortRule(profiles, name, portNumber);
	}

	IFirewallRule IFirewall.CreatePortRule(string name, FirewallAction action, ushort portNumber, FirewallProtocol protocol)
	{
		FirewallLegacyProfile activeProfile = GetActiveProfile();
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
		return CreatePortRule(name, portNumber);
	}

	IFirewallProfile IFirewall.GetActiveProfile()
	{
		return GetActiveProfile();
	}

	IFirewallProfile IFirewall.GetProfile(FirewallProfiles profile)
	{
		return GetProfile(profile);
	}

	public FirewallLegacyApplicationRule CreateApplicationRule(FirewallProfiles profiles, string name, string filename)
	{
		return new FirewallLegacyApplicationRule(name, filename, profiles, TypeResolver);
	}

	public FirewallLegacyApplicationRule CreateApplicationRule(string name, string filename)
	{
		FirewallLegacyProfile activeProfile = GetActiveProfile();
		if (activeProfile == null)
		{
			throw new InvalidOperationException("No firewall profile is currently active.");
		}
		return new FirewallLegacyApplicationRule(name, filename, activeProfile.Type, TypeResolver);
	}

	public FirewallLegacyPortRule CreatePortRule(FirewallProfiles profiles, string name, ushort portNumber)
	{
		return new FirewallLegacyPortRule(name, portNumber, profiles, TypeResolver)
		{
			Protocol = FirewallProtocol.TCP
		};
	}

	public FirewallLegacyPortRule CreatePortRule(string name, ushort portNumber)
	{
		FirewallLegacyProfile activeProfile = GetActiveProfile();
		if (activeProfile == null)
		{
			throw new InvalidOperationException("No firewall profile is currently active.");
		}
		return new FirewallLegacyPortRule(name, portNumber, activeProfile.Type, TypeResolver)
		{
			Protocol = FirewallProtocol.TCP
		};
	}

	public FirewallLegacyProfile GetActiveProfile()
	{
		return Profiles.FirstOrDefault((FirewallLegacyProfile p) => p.IsActive);
	}

	public FirewallLegacyProfile GetProfile(FirewallProfiles profile)
	{
		return Profiles.FirstOrDefault((FirewallLegacyProfile p) => p.Type == profile) ?? throw new FirewallLegacyNotSupportedException();
	}
}
