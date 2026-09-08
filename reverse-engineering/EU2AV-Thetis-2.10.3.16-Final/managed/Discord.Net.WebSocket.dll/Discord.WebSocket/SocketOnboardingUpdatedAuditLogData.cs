using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketOnboardingUpdatedAuditLogData : ISocketAuditLogData, IAuditLogData
{
	private SocketOnboardingInfo After { get; set; }

	private SocketOnboardingInfo Before { get; set; }

	internal SocketOnboardingUpdatedAuditLogData(SocketOnboardingInfo before, SocketOnboardingInfo after)
	{
		Before = before;
		After = after;
	}

	internal static SocketOnboardingUpdatedAuditLogData Create(DiscordSocketClient discord, AuditLogEntry entry)
	{
		var (model, model2) = AuditLogHelper.CreateAuditLogEntityInfo<OnboardingAuditLogModel>(entry.Changes, discord);
		return new SocketOnboardingUpdatedAuditLogData(new SocketOnboardingInfo(model, discord), new SocketOnboardingInfo(model2, discord));
	}
}
