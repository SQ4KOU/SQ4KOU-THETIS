using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class InviteDeleteAuditLogData : IAuditLogData
{
	public int MaxAge { get; }

	public string Code { get; }

	public bool Temporary { get; }

	public IUser Creator { get; }

	public ulong ChannelId { get; }

	public int Uses { get; }

	public int MaxUses { get; }

	private InviteDeleteAuditLogData(InviteInfoAuditLogModel model, IUser inviter)
	{
		MaxAge = model.MaxAge.Value;
		Code = model.Code;
		Temporary = model.Temporary.Value;
		Creator = inviter;
		ChannelId = model.ChannelId.Value;
		Uses = model.Uses.Value;
		MaxUses = model.MaxUses.Value;
	}

	internal static InviteDeleteAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		InviteInfoAuditLogModel data = AuditLogHelper.CreateAuditLogEntityInfo<InviteInfoAuditLogModel>(entry.Changes, discord).Item1;
		RestUser inviter = null;
		if (data.InviterId.HasValue)
		{
			User user = log.Users.FirstOrDefault((User x) => x.Id == data.InviterId);
			inviter = ((user != null) ? RestUser.Create(discord, user) : null);
		}
		return new InviteDeleteAuditLogData(data, inviter);
	}
}
