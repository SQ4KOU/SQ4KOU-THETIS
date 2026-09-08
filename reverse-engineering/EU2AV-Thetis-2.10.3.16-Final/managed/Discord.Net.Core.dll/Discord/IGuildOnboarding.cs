using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord;

public interface IGuildOnboarding
{
	ulong GuildId { get; }

	IGuild Guild { get; }

	IReadOnlyCollection<IGuildOnboardingPrompt> Prompts { get; }

	IReadOnlyCollection<ulong> DefaultChannelIds { get; }

	bool IsEnabled { get; }

	GuildOnboardingMode Mode { get; }

	bool IsBelowRequirements { get; }

	Task ModifyAsync(Action<GuildOnboardingProperties> props, RequestOptions options = null);
}
