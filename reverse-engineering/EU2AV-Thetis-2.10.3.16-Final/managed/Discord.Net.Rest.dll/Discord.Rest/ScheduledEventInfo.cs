using System;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class ScheduledEventInfo
{
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

	internal ScheduledEventInfo(ScheduledEventInfoAuditLogModel model)
	{
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
}
