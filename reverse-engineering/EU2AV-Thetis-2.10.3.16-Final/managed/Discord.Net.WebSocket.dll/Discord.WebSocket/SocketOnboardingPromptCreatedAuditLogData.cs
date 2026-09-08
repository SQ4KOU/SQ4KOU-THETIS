using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketOnboardingPromptCreatedAuditLogData : ISocketAuditLogData, IAuditLogData
{
	private SocketOnboardingPromptInfo Data { get; set; }

	internal SocketOnboardingPromptCreatedAuditLogData(SocketOnboardingPromptInfo data)
	{
		Data = data;
	}

	internal static SocketOnboardingPromptCreatedAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		return new SocketOnboardingPromptCreatedAuditLogData(new SocketOnboardingPromptInfo(AuditLogHelper.CreateAuditLogEntityInfo<OnboardingPromptAuditLogModel>(entry.Changes, discord).Item2, discord));
	}
}
