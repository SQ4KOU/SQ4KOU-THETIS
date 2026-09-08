using System;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class ScheduledEventDeleteAuditLogData : IAuditLogData
{
	public ulong Id { get; }

	public ulong? ChannelId { get; }

	public string Name { get; }

	public string Description { get; }

	public DateTimeOffset? ScheduledStartTime { get; }

	public DateTimeOffset? ScheduledEndTime { get; }

	public GuildScheduledEventPrivacyLevel? PrivacyLevel { get; }

	public GuildScheduledEventStatus? Status { get; }

	public GuildScheduledEventType? EntityType { get; }

	public ulong? EntityId { get; }

	public string Location { get; }

	public string Image { get; }

	private ScheduledEventDeleteAuditLogData(ulong id, ScheduledEventInfoAuditLogModel model)
	{
		Id = id;
		ChannelId = model.ChannelId;
		Name = model.Name;
		Description = model.Description;
		ScheduledStartTime = model.StartTime;
		ScheduledEndTime = model.EndTime;
		PrivacyLevel = model.PrivacyLevel;
		Status = model.EventStatus;
		EntityType = model.EventType;
		EntityId = model.EntityId;
		Location = model.Location;
		Image = model.Image;
	}

	internal static ScheduledEventDeleteAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		ScheduledEventInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<ScheduledEventInfoAuditLogModel>(entry.Changes, discord).Item1;
		return new ScheduledEventDeleteAuditLogData(entry.TargetId.Value, item);
	}
}
