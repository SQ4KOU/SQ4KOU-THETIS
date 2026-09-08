using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketStickerCreatedAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong StickerId { get; }

	public SocketStickerInfo Data { get; }

	internal SocketStickerCreatedAuditLogData(ulong id, SocketStickerInfo data)
	{
		StickerId = id;
		Data = data;
	}

	internal static SocketStickerCreatedAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		StickerInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<StickerInfoAuditLogModel>(entry.Changes, discord).Item2;
		return new SocketStickerCreatedAuditLogData(entry.TargetId.Value, new SocketStickerInfo(item));
	}
}
