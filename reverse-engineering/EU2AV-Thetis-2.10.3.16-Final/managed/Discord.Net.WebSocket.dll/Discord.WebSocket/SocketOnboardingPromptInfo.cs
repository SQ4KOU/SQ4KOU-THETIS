using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;
using Discord.API.AuditLogs;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketOnboardingPromptInfo
{
	private string Title { get; }

	private bool? IsSingleSelect { get; }

	private bool? IsRequired { get; }

	private bool? IsInOnboarding { get; }

	private GuildOnboardingPromptType? Type { get; }

	private IReadOnlyCollection<IGuildOnboardingPromptOption> Options { get; }

	internal SocketOnboardingPromptInfo(OnboardingPromptAuditLogModel model, DiscordSocketClient discord)
	{
		Title = model.Title;
		IsSingleSelect = model.IsSingleSelect;
		IsRequired = model.IsRequired;
		IsInOnboarding = model.IsInOnboarding;
		Type = model.Type;
		Options = model.Options?.Select((GuildOnboardingPromptOption x) => new RestGuildOnboardingPromptOption(discord, x.Id, x)).ToImmutableArray();
	}
}
