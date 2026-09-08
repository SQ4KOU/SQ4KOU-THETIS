using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class ThreadUpdateAuditLogData : IAuditLogData
{
	public IThreadChannel Thread { get; }

	public ThreadType ThreadType { get; }

	public ThreadInfo Before { get; }

	public ThreadInfo After { get; }

	private ThreadUpdateAuditLogData(IThreadChannel thread, ThreadType type, ThreadInfo before, ThreadInfo after)
	{
		Thread = thread;
		ThreadType = type;
		Before = before;
		After = after;
	}

	internal static ThreadUpdateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		(ThreadInfoAuditLogModel, ThreadInfoAuditLogModel) tuple = AuditLogHelper.CreateAuditLogEntityInfo<ThreadInfoAuditLogModel>(entry.Changes, discord);
		ThreadInfoAuditLogModel item = tuple.Item1;
		ThreadInfoAuditLogModel item2 = tuple.Item2;
		Channel channel = log.Threads.FirstOrDefault((Channel x) => x.Id == entry.TargetId.Value);
		return new ThreadUpdateAuditLogData((channel == null) ? null : RestThreadChannel.Create(discord, null, channel), item.Type, new ThreadInfo(item), new ThreadInfo(item2));
	}
}
