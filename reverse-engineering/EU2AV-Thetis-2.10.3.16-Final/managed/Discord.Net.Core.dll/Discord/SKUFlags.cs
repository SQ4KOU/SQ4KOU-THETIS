using System;

namespace Discord;

[Flags]
public enum SKUFlags
{
	IsAvailable = 4,
	GuildSubscription = 0x80,
	UserSubscription = 0x100
}
