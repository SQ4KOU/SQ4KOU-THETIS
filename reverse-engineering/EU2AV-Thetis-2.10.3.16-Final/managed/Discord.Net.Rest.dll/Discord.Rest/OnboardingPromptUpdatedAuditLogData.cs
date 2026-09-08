using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class OnboardingPromptUpdatedAuditLogData : IAuditLogData
{
	private OnboardingPromptInfo After { get; set; }

	private OnboardingPromptInfo Before { get; set; }

	internal OnboardingPromptUpdatedAuditLogData(OnboardingPromptInfo before, OnboardingPromptInfo after)
	{
		Before = before;
		After = after;
	}

	internal static OnboardingPromptUpdatedAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		var (model, model2) = AuditLogHelper.CreateAuditLogEntityInfo<OnboardingPromptAuditLogModel>(entry.Changes, discord);
		return new OnboardingPromptUpdatedAuditLogData(new OnboardingPromptInfo(model, discord), new OnboardingPromptInfo(model2, discord));
	}
}
