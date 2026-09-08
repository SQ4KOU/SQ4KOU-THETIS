using System.Collections.Generic;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class ThreadDeleteAuditLogData : IAuditLogData
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

	private ThreadDeleteAuditLogData(ulong id, ThreadInfoAuditLogModel model)
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

	internal static ThreadDeleteAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		ThreadInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<ThreadInfoAuditLogModel>(entry.Changes, discord).Item1;
		return new ThreadDeleteAuditLogData(entry.TargetId.Value, item);
	}
}
