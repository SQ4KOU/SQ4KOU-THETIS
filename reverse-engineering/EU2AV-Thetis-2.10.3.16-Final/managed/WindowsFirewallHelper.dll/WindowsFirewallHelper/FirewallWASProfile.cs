using System;
using WindowsFirewallHelper.COMInterop;

namespace WindowsFirewallHelper;

public class FirewallWASProfile : IFirewallProfile
{
	private readonly FirewallWAS _firewall;

	private readonly NetFwProfileType2 _profileType;

	public bool BlockAllInboundTraffic
	{
		get
		{
			return _firewall.UnderlyingObject.get_BlockAllInboundTraffic(_profileType);
		}
		set
		{
			_firewall.UnderlyingObject.set_BlockAllInboundTraffic(_profileType, value);
		}
	}

	public FirewallAction DefaultInboundAction
	{
		get
		{
			if (_firewall.UnderlyingObject.get_DefaultInboundAction(_profileType) != NetFwAction.Allow)
			{
				return FirewallAction.Block;
			}
			return FirewallAction.Allow;
		}
		set
		{
			_firewall.UnderlyingObject.set_DefaultInboundAction(_profileType, (value == FirewallAction.Allow) ? NetFwAction.Allow : NetFwAction.Block);
		}
	}

	public FirewallAction DefaultOutboundAction
	{
		get
		{
			if (_firewall.UnderlyingObject.get_DefaultOutboundAction(_profileType) != NetFwAction.Allow)
			{
				return FirewallAction.Block;
			}
			return FirewallAction.Allow;
		}
		set
		{
			_firewall.UnderlyingObject.set_DefaultOutboundAction(_profileType, (value == FirewallAction.Allow) ? NetFwAction.Allow : NetFwAction.Block);
		}
	}

	public bool Enable
	{
		get
		{
			return _firewall.UnderlyingObject.get_FirewallEnabled(_profileType);
		}
		set
		{
			_firewall.UnderlyingObject.set_FirewallEnabled(_profileType, value);
		}
	}

	public bool IsActive
	{
		get
		{
			if (_firewall.UnderlyingObject.CurrentProfileTypes != int.MaxValue)
			{
				return ((uint)_firewall.UnderlyingObject.CurrentProfileTypes & (uint)_profileType) == (uint)_profileType;
			}
			return true;
		}
	}

	public bool ShowNotifications
	{
		get
		{
			return !_firewall.UnderlyingObject.get_NotificationsDisabled(_profileType);
		}
		set
		{
			_firewall.UnderlyingObject.set_NotificationsDisabled(_profileType, !value);
		}
	}

	public FirewallProfiles Type
	{
		get
		{
			if (_profileType == NetFwProfileType2.All)
			{
				throw new ArgumentOutOfRangeException();
			}
			return (FirewallProfiles)_profileType;
		}
	}

	public bool UnicastResponsesToMulticastBroadcast
	{
		get
		{
			return !_firewall.UnderlyingObject.get_UnicastResponsesToMulticastBroadcastDisabled(_profileType);
		}
		set
		{
			_firewall.UnderlyingObject.set_UnicastResponsesToMulticastBroadcastDisabled(_profileType, !value);
		}
	}

	internal FirewallWASProfile(FirewallWAS firewall, NetFwProfileType2 profileType)
	{
		_profileType = profileType;
		_firewall = firewall;
	}

	public override string ToString()
	{
		try
		{
			return Type.ToString();
		}
		catch (Exception)
		{
			return base.ToString();
		}
	}
}
