using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class StickerUpdatedAuditLogData : IAuditLogData
{
	public ulong StickerId { get; }

	public StickerInfo Before { get; }

	public StickerInfo After { get; }

	internal StickerUpdatedAuditLogData(ulong id, StickerInfo before, StickerInfo after)
	{
		StickerId = id;
		Before = before;
		After = after;
	}

	internal static StickerUpdatedAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		var (model, model2) = AuditLogHelper.CreateAuditLogEntityInfo<StickerInfoAuditLogModel>(entry.Changes, discord);
		return new StickerUpdatedAuditLogData(entry.TargetId.Value, new StickerInfo(model), new StickerInfo(model2));
	}
}
