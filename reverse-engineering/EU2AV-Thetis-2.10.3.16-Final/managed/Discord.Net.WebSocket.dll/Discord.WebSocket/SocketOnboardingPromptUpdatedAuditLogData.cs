using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketOnboardingPromptUpdatedAuditLogData : ISocketAuditLogData, IAuditLogData
{
	private SocketOnboardingPromptInfo After { get; set; }

	private SocketOnboardingPromptInfo Before { get; set; }

	internal SocketOnboardingPromptUpdatedAuditLogData(SocketOnboardingPromptInfo before, SocketOnboardingPromptInfo after)
	{
		Before = before;
		After = after;
	}

	internal static SocketOnboardingPromptUpdatedAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		var (model, model2) = AuditLogHelper.CreateAuditLogEntityInfo<OnboardingPromptAuditLogModel>(entry.Changes, discord);
		return new SocketOnboardingPromptUpdatedAuditLogData(new SocketOnboardingPromptInfo(model, discord), new SocketOnboardingPromptInfo(model2, discord));
	}
}
