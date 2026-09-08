using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketStickerUpdatedAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong StickerId { get; }

	public SocketStickerInfo Before { get; }

	public SocketStickerInfo After { get; }

	internal SocketStickerUpdatedAuditLogData(ulong id, SocketStickerInfo before, SocketStickerInfo after)
	{
		StickerId = id;
		Before = before;
		After = after;
	}

	internal static SocketStickerUpdatedAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		var (model, model2) = AuditLogHelper.CreateAuditLogEntityInfo<StickerInfoAuditLogModel>(entry.Changes, discord);
		return new SocketStickerUpdatedAuditLogData(entry.TargetId.Value, new SocketStickerInfo(model), new SocketStickerInfo(model2));
	}
}
