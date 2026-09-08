using Discord.API;
using Discord.API.AuditLogs;

namespace Discord.Rest;

public class OnboardingPromptCreatedAuditLogData : IAuditLogData
{
	private OnboardingPromptInfo Data { get; set; }

	internal OnboardingPromptCreatedAuditLogData(OnboardingPromptInfo data)
	{
		Data = data;
	}

	internal static OnboardingPromptCreatedAuditLogData Create(BaseDiscordClient discord, AuditLogEntry entry, AuditLog log = null)
	{
		return new OnboardingPromptCreatedAuditLogData(new OnboardingPromptInfo(AuditLogHelper.CreateAuditLogEntityInfo<OnboardingPromptAuditLogModel>(entry.Changes, discord).Item2, discord));
	}
}
