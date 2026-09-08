namespace Discord;

public enum GuildUserFlags
{
	None = 0,
	DidRejoin = 1,
	CompletedOnboarding = 2,
	BypassesVerification = 4,
	StartedOnboarding = 8,
	IsGuest = 0x10,
	StartedHomeActions = 0x20,
	CompletedHomeActions = 0x40,
	AutomodQuarantinedUsername = 0x80,
	DmSettingUpsellAcknowledged = 0x200
}
