using System;

namespace Discord;

[Flags]
public enum ApplicationMonetizationEligibilityFlags
{
	None = 0,
	Verified = 1,
	HasTeam = 2,
	ApprovedCommands = 4,
	TermsOfService = 8,
	PrivacyPolicy = 0x10,
	SafeName = 0x20,
	SafeDescription = 0x40,
	SafeRoleConnections = 0x80,
	NotQuarantined = 0x200,
	TeamMembersEmailVerified = 0x8000,
	TeamMembersMfaEnabled = 0x10000,
	NoBlockingIssues = 0x20000,
	ValidPayoutStatus = 0x40000
}
