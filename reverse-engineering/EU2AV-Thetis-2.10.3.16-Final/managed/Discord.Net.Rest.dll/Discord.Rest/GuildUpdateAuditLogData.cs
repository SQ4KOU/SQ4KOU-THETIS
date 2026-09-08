using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class GuildUpdateAuditLogData : IAuditLogData
{
	public GuildInfo Before { get; }

	public GuildInfo After { get; }

	private GuildUpdateAuditLogData(GuildInfo before, GuildInfo after)
	{
		Before = before;
		After = after;
	}

	internal static GuildUpdateAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		AuditLogChange[] changes = entry.Changes;
		AuditLogChange auditLogChange = changes.FirstOrDefault((AuditLogChange x) => x.ChangedProperty == "owner_id");
		ulong? oldOwnerId = auditLogChange?.OldValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		ulong? newOwnerId = auditLogChange?.NewValue?.ToObject<ulong>(discord.ApiClient.Serializer);
		IUser owner = null;
		if (oldOwnerId.HasValue)
		{
			User model = log.Users.FirstOrDefault((User x) => x.Id == oldOwnerId.Value);
			owner = RestUser.Create(discord, model);
		}
		IUser owner2 = null;
		if (newOwnerId.HasValue)
		{
			User model2 = log.Users.FirstOrDefault((User x) => x.Id == newOwnerId.Value);
			owner2 = RestUser.Create(discord, model2);
		}
		var (model3, model4) = AuditLogHelper.CreateAuditLogEntityInfo<GuildInfoAuditLogModel>(changes, discord);
		return new GuildUpdateAuditLogData(new GuildInfo(model3, owner), new GuildInfo(model4, owner2));
	}
}
