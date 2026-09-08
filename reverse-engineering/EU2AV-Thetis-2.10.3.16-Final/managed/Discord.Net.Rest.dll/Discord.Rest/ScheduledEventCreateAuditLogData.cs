using System;
using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class ScheduledEventCreateAuditLogData : IAuditLogData
{
	public IGuildScheduledEvent ScheduledEvent { get; }

	public ulong Id { get; }

	public ulong? ChannelId { get; }

	public string Name { get; }

	public string Description { get; }

	public DateTimeOffset? ScheduledStartTime { get; }

	public DateTimeOffset? ScheduledEndTime { get; }

	public GuildScheduledEventPrivacyLevel PrivacyLevel { get; }

	public GuildScheduledEventStatus Status { get; }

	public GuildScheduledEventType EntityType { get; }

	public ulong? EntityId { get; }

	public string Location { get; }

	public string Image { get; }

	private ScheduledEventCreateAuditLogData(ulong id, ScheduledEventInfoAuditLogModel model, IGuildScheduledEvent scheduledEvent)
	{
		Id = id;
		ChannelId = model.ChannelId;
		Name = model.Name;
		Description = model.Description;
		ScheduledStartTime = model.StartTime;
		ScheduledEndTime = model.EndTime;
		PrivacyLevel = model.PrivacyLevel.Value;
		Status = model.EventStatus.Value;
		EntityType = model.EventType.Value;
		EntityId = model.EntityId;
		Location = model.Location;
		Image = model.Image;
		ScheduledEvent = scheduledEvent;
	}

	internal static ScheduledEventCreateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		ScheduledEventInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<ScheduledEventInfoAuditLogModel>(entry.Changes, discord).Item2;
		GuildScheduledEvent model = log.GuildScheduledEvents.FirstOrDefault((GuildScheduledEvent x) => x.Id == entry.TargetId);
		return new ScheduledEventCreateAuditLogData(entry.TargetId.Value, item, RestGuildEvent.Create(discord, null, model));
	}
}
