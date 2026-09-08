using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class ChannelUpdateAuditLogData : IAuditLogData
{
	public ulong ChannelId { get; }

	public ChannelInfo Before { get; }

	public ChannelInfo After { get; }

	private ChannelUpdateAuditLogData(ulong id, ChannelInfo before, ChannelInfo after)
	{
		ChannelId = id;
		Before = before;
		After = after;
	}

	internal static ChannelUpdateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		var (model, model2) = AuditLogHelper.CreateAuditLogEntityInfo<ChannelInfoAuditLogModel>(entry.Changes, discord);
		return new ChannelUpdateAuditLogData(entry.TargetId.Value, new ChannelInfo(model), new ChannelInfo(model2));
	}
}
