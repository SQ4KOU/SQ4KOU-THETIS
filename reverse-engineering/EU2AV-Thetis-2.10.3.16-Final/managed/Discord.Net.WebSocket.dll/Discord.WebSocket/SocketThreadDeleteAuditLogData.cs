using System.Collections.Generic;
using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketThreadDeleteAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong ThreadId { get; }

	public string ThreadName { get; }

	public ThreadType ThreadType { get; }

	public bool IsArchived { get; }

	public ThreadArchiveDuration AutoArchiveDuration { get; }

	public bool IsLocked { get; }

	public int? SlowModeInterval { get; }

	public IReadOnlyCollection<ulong> AppliedTags { get; }

	public ChannelFlags? Flags { get; }

	private SocketThreadDeleteAuditLogData(ulong id, ThreadInfoAuditLogModel model)
	{
		ThreadId = id;
		ThreadName = model.Name;
		IsArchived = model.IsArchived.Value;
		AutoArchiveDuration = model.ArchiveDuration.Value;
		IsLocked = model.IsLocked.Value;
		SlowModeInterval = model.SlowModeInterval;
		AppliedTags = model.AppliedTags;
		Flags = model.ChannelFlags;
		ThreadType = model.Type;
	}

	internal static SocketThreadDeleteAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		ThreadInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<ThreadInfoAuditLogModel>(entry.Changes, discord).Item1;
		return new SocketThreadDeleteAuditLogData(entry.TargetId.Value, item);
	}
}
