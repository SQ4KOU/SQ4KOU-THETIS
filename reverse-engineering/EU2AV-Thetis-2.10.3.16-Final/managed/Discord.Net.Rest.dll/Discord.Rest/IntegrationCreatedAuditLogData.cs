using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class IntegrationCreatedAuditLogData : IAuditLogData
{
	public IIntegration Integration { get; }

	public IntegrationInfo Data { get; }

	internal IntegrationCreatedAuditLogData(IntegrationInfo info, IIntegration integration)
	{
		Integration = integration;
		Data = info;
	}

	internal static IntegrationCreatedAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		IntegrationInfoAuditLogModel item = AuditLogHelper.CreateAuditLogEntityInfo<IntegrationInfoAuditLogModel>(entry.Changes, discord).Item2;
		return new IntegrationCreatedAuditLogData(integration: RestIntegration.Create(discord, null, log.Integrations.FirstOrDefault((Integration x) => x.Id == entry.TargetId)), info: new IntegrationInfo(item));
	}
}
