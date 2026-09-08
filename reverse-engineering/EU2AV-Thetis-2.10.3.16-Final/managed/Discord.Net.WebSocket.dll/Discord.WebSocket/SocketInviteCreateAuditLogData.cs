using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketInviteCreateAuditLogData : ISocketAuditLogData, IAuditLogData
{
	public int MaxAge { get; }

	public string Code { get; }

	public bool Temporary { get; }

	public Cacheable<SocketUser, RestUser, IUser, ulong>? Creator { get; }

	public ulong ChannelId { get; }

	public int Uses { get; }

	public int MaxUses { get; }

	private SocketInviteCreateAuditLogData(InviteInfoAuditLogModel model, Cacheable<SocketUser, RestUser, IUser, ulong>? inviter)
	{
		MaxAge = model.MaxAge.Value;
		Code = model.Code;
		Temporary = model.Temporary.Value;
		ChannelId = model.ChannelId.Value;
		Uses = model.Uses.Value;
		MaxUses = model.MaxUses.Value;
		Creator = inviter;
	}

	internal static SocketInviteCreateAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		InviteInfoAuditLogModel data = AuditLogHelper.CreateAuditLogEntityInfo<InviteInfoAuditLogModel>(entry.Changes, discord).Item2;
		Cacheable<SocketUser, RestUser, IUser, ulong>? inviter = null;
		if (data.InviterId.HasValue)
		{
			SocketUser user = discord.GetUser(data.InviterId.Value);
			inviter = new Cacheable<SocketUser, RestUser, IUser, ulong>(user, data.InviterId.Value, user != null, async delegate
			{
				User user2 = await discord.ApiClient.GetUserAsync(data.InviterId.Value);
				return (user2 != null) ? RestUser.Create(discord, user2) : null;
			});
		}
		return new SocketInviteCreateAuditLogData(data, inviter);
	}
}
