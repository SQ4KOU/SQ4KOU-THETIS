using System;
using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketScheduledEventCreateAuditLogData : ISocketAuditLogData, IAuditLogData
{
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

	private SocketScheduledEventCreateAuditLogData(ulong id, ScheduledEventInfoAuditLogModel model)
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
	}

	internal static SocketScheduledEventCreateAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		ScheduledEventInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<ScheduledEventInfoAuditLogModel>(entry.Changes, discord).Item2;
		return new SocketScheduledEventCreateAuditLogData(entry.TargetId.Value, item);
	}
}
