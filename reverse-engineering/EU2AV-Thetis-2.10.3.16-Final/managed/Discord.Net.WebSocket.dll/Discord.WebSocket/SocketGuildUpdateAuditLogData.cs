using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketGuildUpdateAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public SocketGuildInfo Before { get; }

	public SocketGuildInfo After { get; }

	private SocketGuildUpdateAuditLogData(SocketGuildInfo before, SocketGuildInfo after)
	{
		Before = before;
		After = after;
	}

	internal static SocketGuildUpdateAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		(GuildInfoAuditLogModel, GuildInfoAuditLogModel) tuple = AuditLogHelper.CreateAuditLogEntityInfo<GuildInfoAuditLogModel>(entry.Changes, discord);
		return new SocketGuildUpdateAuditLogData(SocketGuildInfo.Create(tuple.Item1), SocketGuildInfo.Create(tuple.Item2));
	}
}
