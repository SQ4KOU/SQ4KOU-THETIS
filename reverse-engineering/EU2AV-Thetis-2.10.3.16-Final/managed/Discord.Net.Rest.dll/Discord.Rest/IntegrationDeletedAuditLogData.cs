using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class IntegrationDeletedAuditLogData : IAuditLogData
{
	public IntegrationInfo Data { get; }

	internal IntegrationDeletedAuditLogData(IntegrationInfo info)
	{
		Data = info;
	}

	internal static IntegrationDeletedAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		return new IntegrationDeletedAuditLogData(new IntegrationInfo(AuditLogHelper.CreateAuditLogEntityInfo<IntegrationInfoAuditLogModel>(entry.Changes, discord).Item1));
	}
}
