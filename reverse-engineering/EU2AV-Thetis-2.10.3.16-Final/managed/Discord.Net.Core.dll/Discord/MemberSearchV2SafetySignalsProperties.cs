namespace Discord;

public struct MemberSearchV2SafetySignalsProperties
{
	public MemberSearchIntQuery? UnusualDmActivityUntil { get; set; }

	public MemberSearchIntQuery? CommunicationDisabledUntil { get; set; }

	public bool? UnusualAccountActivity { get; set; }

	public bool? AutomodQuarantinedUsername { get; set; }
}
