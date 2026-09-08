using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketChannelUpdateAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong ChannelId { get; }

	public SocketChannelInfo Before { get; }

	public SocketChannelInfo After { get; }

	private SocketChannelUpdateAuditLogData(ulong id, SocketChannelInfo before, SocketChannelInfo after)
	{
		ChannelId = id;
		Before = before;
		After = after;
	}

	internal static SocketChannelUpdateAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		var (model, model2) = AuditLogHelper.CreateAuditLogEntityInfo<ChannelInfoAuditLogModel>(entry.Changes, discord);
		return new SocketChannelUpdateAuditLogData(entry.TargetId.Value, new SocketChannelInfo(model), new SocketChannelInfo(model2));
	}
}
