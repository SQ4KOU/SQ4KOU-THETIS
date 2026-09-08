using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class ScheduledEventUpdateAuditLogData : IAuditLogData
{
	public IGuildScheduledEvent ScheduledEvent { get; }

	public ulong Id { get; }

	public ScheduledEventInfo Before { get; }

	public ScheduledEventInfo After { get; }

	private ScheduledEventUpdateAuditLogData(ulong id, ScheduledEventInfo before, ScheduledEventInfo after, IGuildScheduledEvent scheduledEvent)
	{
		Id = id;
		Before = before;
		After = after;
		ScheduledEvent = scheduledEvent;
	}

	internal static ScheduledEventUpdateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		(ScheduledEventInfoAuditLogModel, ScheduledEventInfoAuditLogModel) tuple = AuditLogHelper.CreateAuditLogEntityInfo<ScheduledEventInfoAuditLogModel>(entry.Changes, discord);
		ScheduledEventInfoAuditLogModel item = tuple.Item1;
		ScheduledEventInfoAuditLogModel item2 = tuple.Item2;
		GuildScheduledEvent model = log.GuildScheduledEvents.FirstOrDefault((GuildScheduledEvent x) => x.Id == entry.TargetId);
		return new ScheduledEventUpdateAuditLogData(entry.TargetId.Value, new ScheduledEventInfo(item), new ScheduledEventInfo(item2), RestGuildEvent.Create(discord, null, model));
	}
}
