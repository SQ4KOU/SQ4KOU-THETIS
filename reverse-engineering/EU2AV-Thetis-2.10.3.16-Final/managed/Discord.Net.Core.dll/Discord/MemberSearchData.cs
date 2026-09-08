using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct MemberSearchData
{
	public IGuildUser User { get; }

	public string SourceInviteCode { get; }

	public JoinSourceType JoinSourceType { get; }

	public ulong? InviterId { get; }

	private string DebuggerDisplay => $"{User.Username} ({User.Id})";

	public MemberSearchData(IGuildUser user, string sourceInviteCode, JoinSourceType joinSourceType, ulong? inviterId)
	{
		User = user;
		SourceInviteCode = sourceInviteCode;
		JoinSourceType = joinSourceType;
		InviterId = inviterId;
	}
}
