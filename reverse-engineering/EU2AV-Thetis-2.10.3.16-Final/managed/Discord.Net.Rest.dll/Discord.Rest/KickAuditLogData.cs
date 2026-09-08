using System.Linq;
using Discord.API;

namespace Discord.Rest;

public class KickAuditLogData : IAuditLogData
{
	public IUser Target { get; }

	public string IntegrationType { get; }

	private KickAuditLogData(RestUser user, string integrationType)
	{
		Target = user;
		IntegrationType = integrationType;
	}

	internal static KickAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		User user = log.Users.FirstOrDefault((User x) => x.Id == entry.TargetId);
		return new KickAuditLogData((user != null) ? RestUser.Create(discord, user) : null, entry.Options?.IntegrationType);
	}
}
