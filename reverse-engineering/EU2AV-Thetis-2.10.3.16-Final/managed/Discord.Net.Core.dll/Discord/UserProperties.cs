using System;

namespace Discord;

[Flags]
public enum UserProperties
{
	None = 0,
	Staff = 1,
	Partner = 2,
	HypeSquadEvents = 4,
	BugHunterLevel1 = 8,
	HypeSquadBravery = 0x40,
	HypeSquadBrilliance = 0x80,
	HypeSquadBalance = 0x100,
	EarlySupporter = 0x200,
	TeamUser = 0x400,
	System = 0x1000,
	BugHunterLevel2 = 0x4000,
	VerifiedBot = 0x10000,
	EarlyVerifiedBotDeveloper = 0x20000,
	DiscordCertifiedModerator = 0x40000,
	BotHTTPInteractions = 0x80000,
	ActiveDeveloper = 0x400000
}
