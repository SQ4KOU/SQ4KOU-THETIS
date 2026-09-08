namespace WindowsFirewallHelper;

public interface IFirewallProfile
{
	bool BlockAllInboundTraffic { get; set; }

	FirewallAction DefaultInboundAction { get; set; }

	FirewallAction DefaultOutboundAction { get; set; }

	bool Enable { get; set; }

	bool IsActive { get; }

	bool ShowNotifications { get; set; }

	FirewallProfiles Type { get; }

	bool UnicastResponsesToMulticastBroadcast { get; set; }
}
