using System;

namespace Discord;

[Flags]
public enum SystemChannelMessageDeny
{
	None = 0,
	WelcomeMessage = 1,
	GuildBoost = 2,
	GuildSetupTip = 4,
	WelcomeMessageReply = 8,
	RoleSubscriptionPurchase = 0x10,
	RoleSubscriptionPurchaseReplies = 0x20
}
