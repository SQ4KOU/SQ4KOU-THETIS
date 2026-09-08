using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketStickerDeletedAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public ulong StickerId { get; }

	public SocketStickerInfo Data { get; }

	internal SocketStickerDeletedAuditLogData(ulong id, SocketStickerInfo data)
	{
		StickerId = id;
		Data = data;
	}

	internal static SocketStickerDeletedAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry)
	{
		StickerInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<StickerInfoAuditLogModel>(entry.Changes, discord).Item1;
		return new SocketStickerDeletedAuditLogData(entry.TargetId.Value, new SocketStickerInfo(item));
	}
}
