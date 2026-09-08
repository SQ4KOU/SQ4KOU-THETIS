using System;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public struct MemberInfo
{
	public string Nickname { get; }

	public bool? Deaf { get; }

	public bool? Mute { get; }

	public DateTimeOffset? TimedOutUntil { get; }

	internal MemberInfo(MemberInfoAuditLogModel model)
	{
		Nickname = model.Nickname;
		Deaf = model.IsDeafened;
		Mute = model.IsMuted;
		TimedOutUntil = model.TimeOutUntil;
	}
}
