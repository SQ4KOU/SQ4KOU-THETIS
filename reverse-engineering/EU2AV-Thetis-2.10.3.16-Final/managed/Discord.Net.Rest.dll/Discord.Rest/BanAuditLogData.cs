using System.Linq;
using Discord.API;

namespace Discord.Rest;

public class BanAuditLogData : IAuditLogData
{
	public IUser Target { get; }

	private BanAuditLogData(IUser user)
	{
		Target = user;
	}

	internal static BanAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		User user = log.Users.FirstOrDefault((User x) => x.Id == entry.TargetId);
		return new BanAuditLogData((user != null) ? RestUser.Create(discord, user) : null);
	}
}
