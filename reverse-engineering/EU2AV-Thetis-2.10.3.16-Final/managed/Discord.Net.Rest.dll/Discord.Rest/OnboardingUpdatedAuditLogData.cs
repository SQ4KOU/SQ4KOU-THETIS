using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class OnboardingUpdatedAuditLogData : IAuditLogData
{
	private OnboardingInfo After { get; set; }

	private OnboardingInfo Before { get; set; }

	internal OnboardingUpdatedAuditLogData(OnboardingInfo before, OnboardingInfo after)
	{
		Before = before;
		After = after;
	}

	internal static OnboardingUpdatedAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		var (model, model2) = AuditLogHelper.CreateAuditLogEntityInfo<OnboardingAuditLogModel>(entry.Changes, discord);
		return new OnboardingUpdatedAuditLogData(new OnboardingInfo(model, discord), new OnboardingInfo(model2, discord));
	}
}
