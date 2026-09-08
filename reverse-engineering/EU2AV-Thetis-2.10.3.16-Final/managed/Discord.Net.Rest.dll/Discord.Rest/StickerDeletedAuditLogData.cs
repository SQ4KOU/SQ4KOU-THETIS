using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class StickerDeletedAuditLogData : IAuditLogData
{
	public ulong StickerId { get; }

	public StickerInfo Data { get; }

	internal StickerDeletedAuditLogData(ulong id, StickerInfo data)
	{
		StickerId = id;
		Data = data;
	}

	internal static StickerDeletedAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		StickerInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<StickerInfoAuditLogModel>(entry.Changes, discord).Item1;
		return new StickerDeletedAuditLogData(entry.TargetId.Value, new StickerInfo(item));
	}
}
