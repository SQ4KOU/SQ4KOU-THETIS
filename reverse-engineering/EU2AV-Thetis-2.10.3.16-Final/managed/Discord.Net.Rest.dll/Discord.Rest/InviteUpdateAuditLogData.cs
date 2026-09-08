using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class InviteUpdateAuditLogData : IAuditLogData
{
	public InviteInfo Before { get; }

	public InviteInfo After { get; }

	private InviteUpdateAuditLogData(InviteInfo before, InviteInfo after)
	{
		Before = before;
		After = after;
	}

	internal static InviteUpdateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		var (model, model2) = AuditLogHelper.CreateAuditLogEntityInfo<InviteInfoAuditLogModel>(entry.Changes, discord);
		return new InviteUpdateAuditLogData(new InviteInfo(model), new InviteInfo(model2));
	}
}
