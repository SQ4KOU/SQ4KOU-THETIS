using System.Collections.Generic;
using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class ThreadCreateAuditLogData : IAuditLogData
{
	public IThreadChannel Thread { get; }

	public ulong ThreadId { get; }

	public string ThreadName { get; }

	public ThreadType ThreadType { get; }

	public bool IsArchived { get; }

	public ThreadArchiveDuration AutoArchiveDuration { get; }

	public bool IsLocked { get; }

	public int? SlowModeInterval { get; }

	public IReadOnlyCollection<ulong> AppliedTags { get; }

	public ChannelFlags? Flags { get; }

	private ThreadCreateAuditLogData(IThreadChannel thread, ulong id, ThreadInfoAuditLogModel model)
	{
		Thread = thread;
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

	internal static ThreadCreateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		ThreadInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<ThreadInfoAuditLogModel>(entry.Changes, discord).Item2;
		Channel channel = log.Threads.FirstOrDefault((Channel x) => x.Id == entry.TargetId.Value);
		return new ThreadCreateAuditLogData((channel == null) ? null : RestThreadChannel.Create(discord, null, channel), entry.TargetId.Value, item);
	}
}
