using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class StickerCreatedAuditLogData : IAuditLogData
{
	public ulong StickerId { get; }

	public StickerInfo Data { get; }

	internal StickerCreatedAuditLogData(ulong id, StickerInfo data)
	{
		StickerId = id;
		Data = data;
	}

	internal static StickerCreatedAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		StickerInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<StickerInfoAuditLogModel>(entry.Changes, discord).Item2;
		return new StickerCreatedAuditLogData(entry.TargetId.Value, new StickerInfo(item));
	}
}
