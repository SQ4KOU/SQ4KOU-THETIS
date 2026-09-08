using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketScheduledEventUpdateAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong Id { get; }

	public SocketScheduledEventInfo Before { get; }

	public SocketScheduledEventInfo After { get; }

	private SocketScheduledEventUpdateAuditLogData(ulong id, SocketScheduledEventInfo before, SocketScheduledEventInfo after)
	{
		Id = id;
		Before = before;
		After = after;
	}

	internal static SocketScheduledEventUpdateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry)
	{
		var (model, model2) = AuditLogHelper.CreateAuditLogEntityInfo<ScheduledEventInfoAuditLogModel>(entry.Changes, discord);
		return new SocketScheduledEventUpdateAuditLogData(entry.TargetId.Value, new SocketScheduledEventInfo(model), new SocketScheduledEventInfo(model2));
	}
}
