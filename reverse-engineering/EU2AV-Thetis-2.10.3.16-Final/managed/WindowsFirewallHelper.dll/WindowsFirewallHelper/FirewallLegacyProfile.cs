using WindowsFirewallHelper.COMInterop;
using WindowsFirewallHelper.Collections;
using WindowsFirewallHelper.Exceptions;

namespace WindowsFirewallHelper;

public class FirewallLegacyProfile : IFirewallProfile
{
	private readonly FirewallLegacy _firewall;

	public IFirewallLegacyRulesCollection Rules => new FirewallLegacyRulesCollection(new FirewallLegacyProfile[1] { this }, _firewall);

	internal INetFwProfile UnderlyingObject { get; }

	bool IFirewallProfile.BlockAllInboundTraffic
	{
		get
		{
			return UnderlyingObject.ExceptionsNotAllowed;
		}
		set
		{
			UnderlyingObject.ExceptionsNotAllowed = value;
		}
	}

	FirewallAction IFirewallProfile.DefaultInboundAction
	{
		get
		{
			return FirewallAction.Block;
		}
		set
		{
			throw new FirewallLegacyNotSupportedException();
		}
	}

	FirewallAction IFirewallProfile.DefaultOutboundAction
	{
		get
		{
			return FirewallAction.Allow;
		}
		set
		{
			throw new FirewallLegacyNotSupportedException();
		}
	}

	public bool Enable
	{
		get
		{
			return UnderlyingObject.FirewallEnabled;
		}
		set
		{
			UnderlyingObject.FirewallEnabled = value;
		}
	}

	public bool IsActive => _firewall?.UnderlyingObject?.CurrentProfileType == UnderlyingObject.Type;

	public bool ShowNotifications
	{
		get
		{
			return !UnderlyingObject.NotificationsDisabled;
		}
		set
		{
			UnderlyingObject.NotificationsDisabled = !value;
		}
	}

	public FirewallProfiles Type => GetManagedProfileType(UnderlyingObject.Type);

	public bool UnicastResponsesToMulticastBroadcast
	{
		get
		{
			return !UnderlyingObject.UnicastResponsesToMulticastBroadcastDisabled;
		}
		set
		{
			UnderlyingObject.UnicastResponsesToMulticastBroadcastDisabled = !value;
		}
	}

	internal FirewallLegacyProfile(FirewallLegacy firewall, FirewallProfiles profileType)
	{
		INetFwPolicy localPolicy = firewall.UnderlyingObject.LocalPolicy;
		UnderlyingObject = localPolicy.GetProfileByType(GetNativeProfileType(profileType));
		_firewall = firewall;
	}

	private static FirewallProfiles GetManagedProfileType(NetFwProfileType profile)
	{
		return profile switch
		{
			NetFwProfileType.Domain => FirewallProfiles.Domain, 
			NetFwProfileType.Standard => FirewallProfiles.Private, 
			_ => throw new FirewallLegacyNotSupportedException(), 
		};
	}

	private static NetFwProfileType GetNativeProfileType(FirewallProfiles profile)
	{
		return profile switch
		{
			FirewallProfiles.Domain => NetFwProfileType.Domain, 
			FirewallProfiles.Private => NetFwProfileType.Standard, 
			_ => throw new FirewallLegacyNotSupportedException(), 
		};
	}

	public override string ToString()
	{
		try
		{
			return Type.ToString();
		}
		catch
		{
			return base.ToString();
		}
	}
}
