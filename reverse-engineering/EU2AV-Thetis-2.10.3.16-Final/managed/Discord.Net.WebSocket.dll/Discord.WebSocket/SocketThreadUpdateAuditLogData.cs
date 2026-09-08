using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketThreadUpdateAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ThreadType ThreadType { get; }

	public ThreadInfo Before { get; }

	public ThreadInfo After { get; }

	private SocketThreadUpdateAuditLogData(ThreadType type, ThreadInfo before, ThreadInfo after)
	{
		ThreadType = type;
		Before = before;
		After = after;
	}

	internal static SocketThreadUpdateAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		var (threadInfoAuditLogModel, model) = AuditLogHelper.CreateAuditLogEntityInfo<ThreadInfoAuditLogModel>(entry.Changes, discord);
		return new SocketThreadUpdateAuditLogData(threadInfoAuditLogModel.Type, new ThreadInfo(threadInfoAuditLogModel), new ThreadInfo(model));
	}
}
