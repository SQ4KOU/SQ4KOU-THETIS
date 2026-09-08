using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketOnboardingInfo
{
	private IReadOnlyCollection<IGuildOnboardingPrompt> Prompts { get; }

	private IReadOnlyCollection<ulong> DefaultChannelIds { get; }

	private bool? IsEnabled { get; }

	internal SocketOnboardingInfo(OnboardingAuditLogModel model, DiscordSocketClient discord)
	{
		Prompts = model.Prompts?.Select((GuildOnboardingPrompt x) => new RestGuildOnboardingPrompt(discord, x.Id, x)).ToImmutableArray();
		DefaultChannelIds = model.DefaultChannelIds;
		IsEnabled = model.Enabled;
	}
}
