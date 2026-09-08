using System;

namespace Discord;

[Flags]
public enum DiscoveryEligibilityFlags
{
	None = 0,
	Verified = 1,
	Tag = 2,
	Description = 4,
	TermsOfService = 8,
	PrivacyPolicy = 0x10,
	InstallParams = 0x20,
	SafeName = 0x40,
	SafeDescription = 0x80,
	ApprovedCommands = 0x100,
	SupportGuild = 0x200,
	SafeCommands = 0x400,
	MfaEnabled = 0x800,
	SafeDirectoryOverview = 0x1000,
	SupportedLocales = 0x2000,
	SafeShortDescription = 0x4000,
	SafeRoleConnections = 0x8000,
	Eligible = 0x10000
}
