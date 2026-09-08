using System;

namespace Discord;

[Flags]
public enum GatewayIntents
{
	None = 0,
	Guilds = 1,
	GuildMembers = 2,
	GuildBans = 4,
	GuildEmojis = 8,
	GuildIntegrations = 0x10,
	GuildWebhooks = 0x20,
	GuildInvites = 0x40,
	GuildVoiceStates = 0x80,
	GuildPresences = 0x100,
	GuildMessages = 0x200,
	GuildMessageReactions = 0x400,
	GuildMessageTyping = 0x800,
	DirectMessages = 0x1000,
	DirectMessageReactions = 0x2000,
	DirectMessageTyping = 0x4000,
	MessageContent = 0x8000,
	GuildScheduledEvents = 0x10000,
	AutoModerationConfiguration = 0x100000,
	AutoModerationActionExecution = 0x200000,
	GuildMessagePolls = 0x1000000,
	DirectMessagePolls = 0x2000000,
	AllUnprivileged = Guilds | GuildBans | GuildEmojis | GuildIntegrations | GuildWebhooks | GuildInvites | GuildVoiceStates | GuildMessages | GuildMessageReactions | GuildMessageTyping | DirectMessages | DirectMessageReactions | DirectMessageTyping | GuildScheduledEvents | AutoModerationConfiguration | AutoModerationActionExecution | GuildMessagePolls | DirectMessagePolls,
	All = AllUnprivileged | GuildMembers | GuildPresences | MessageContent
}
