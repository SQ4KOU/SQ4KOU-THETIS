using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class IntegrationUpdatedAuditLogData : IAuditLogData
{
	public IIntegration Integration { get; }

	public IntegrationInfo Before { get; }

	public IntegrationInfo After { get; }

	internal IntegrationUpdatedAuditLogData(IntegrationInfo before, IntegrationInfo after, IIntegration integration)
	{
		Before = before;
		After = after;
		Integration = integration;
	}

	internal static IntegrationUpdatedAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log)
	{
		(IntegrationInfoAuditLogModel, IntegrationInfoAuditLogModel) tuple = AuditLogHelper.CreateAuditLogEntityInfo<IntegrationInfoAuditLogModel>(entry.Changes, discord);
		IntegrationInfoAuditLogModel item = tuple.Item1;
		IntegrationInfoAuditLogModel item2 = tuple.Item2;
		RestIntegration integration = RestIntegration.Create(discord, null, log.Integrations.FirstOrDefault((Integration x) => x.Id == entry.TargetId));
		return new IntegrationUpdatedAuditLogData(new IntegrationInfo(item), new IntegrationInfo(item2), integration);
	}
}
