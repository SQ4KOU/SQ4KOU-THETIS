using Discord.API.AuditLogs;

namespace Discord.WebSocket;

public class SocketStickerInfo
{
	public string Name { get; set; }

	public string Tags { get; set; }

	public string Description { get; set; }

	internal SocketStickerInfo(StickerInfoAuditLogModel model)
	{
		Name = model.Name;
		Tags = model.Tags;
		Description = model.Description;
	}
}
