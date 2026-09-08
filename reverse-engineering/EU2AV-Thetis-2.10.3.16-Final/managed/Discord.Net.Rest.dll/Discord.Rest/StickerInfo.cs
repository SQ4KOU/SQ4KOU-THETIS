using Discord.API.AuditLogs;

namespace Discord.Rest;

public class StickerInfo
{
	public string Name { get; set; }

	public string Tags { get; set; }

	public string Description { get; set; }

	internal StickerInfo(StickerInfoAuditLogModel model)
	{
		Name = model.Name;
		Tags = model.Tags;
		Description = model.Description;
	}
}
