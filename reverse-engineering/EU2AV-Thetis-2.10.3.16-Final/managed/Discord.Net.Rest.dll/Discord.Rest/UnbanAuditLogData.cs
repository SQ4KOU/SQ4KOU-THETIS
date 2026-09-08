using System.Linq;
using Discord.API;

namespace Discord.Rest;

public class UnbanAuditLogData : IAuditLogData
{
	public IUser Target { get; }

	private UnbanAuditLogData(IUser user)
	{
		Target = user;
	}

	internal static UnbanAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		User user = log.Users.FirstOrDefault((User x) => x.Id == entry.TargetId);
		return new UnbanAuditLogData((user != null) ? RestUser.Create(discord, user) : null);
	}
}
