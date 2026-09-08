using Discord.API.AuditLogs;

namespace Discord.WebSocket;

public struct SocketInviteInfo
{
	public int? MaxAge { get; }

	public string Code { get; }

	public bool? Temporary { get; }

	public ulong? ChannelId { get; }

	public int? MaxUses { get; }

	public ulong? CreatorId { get; }

	internal SocketInviteInfo(InviteInfoAuditLogModel model)
	{
		MaxAge = model.MaxAge;
		Code = model.Code;
		Temporary = model.Temporary;
		ChannelId = model.ChannelId;
		MaxUses = model.MaxUses;
		CreatorId = model.InviterId;
	}
}
