namespace Discord;

public struct MemberSearchFilter
{
	public MemberSearchV2SafetySignalsProperties? SafetySignals { get; set; }

	public MemberSearchSnowflakeQuery? RoleIds { get; set; }

	public MemberSearchSnowflakeQuery? UserId { get; set; }

	public MemberSearchIntQuery? GuildJoinedAt { get; set; }

	public MemberSearchStringQuery? SourceInviteCode { get; set; }

	public MemberSearchIntQuery? JoinSourceType { get; set; }

	public bool? DidRejoin { get; set; }

	public bool? IsPending { get; set; }

	public MemberSearchStringQuery? Usernames { get; set; }
}
