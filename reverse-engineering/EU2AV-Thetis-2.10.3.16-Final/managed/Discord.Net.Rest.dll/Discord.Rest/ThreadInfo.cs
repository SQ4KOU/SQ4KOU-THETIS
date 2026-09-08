using System.Collections.Generic;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class ThreadInfo
{
	public string Name { get; }

	public bool? IsArchived { get; }

	public ThreadArchiveDuration? AutoArchiveDuration { get; }

	public bool? IsLocked { get; }

	public int? SlowModeInterval { get; }

	public IReadOnlyCollection<ulong> AppliedTags { get; }

	public ChannelFlags? Flags { get; }

	public ThreadType Type { get; }

	internal ThreadInfo(ThreadInfoAuditLogModel model)
	{
		Name = model.Name;
		IsArchived = model.IsArchived;
		AutoArchiveDuration = model.ArchiveDuration;
		IsLocked = model.IsLocked;
		SlowModeInterval = model.SlowModeInterval;
		AppliedTags = model.AppliedTags;
		Flags = model.ChannelFlags;
		Type = model.Type;
	}
}
