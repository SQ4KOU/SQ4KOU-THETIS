using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

public class RestGuildOnboarding : IGuildOnboarding
{
	internal BaseDiscordClient Discord;

	public ulong GuildId { get; private set; }

	public RestGuild Guild { get; private set; }

	public IReadOnlyCollection<ulong> DefaultChannelIds { get; private set; }

	public bool IsEnabled { get; private set; }

	public GuildOnboardingMode Mode { get; private set; }

	public bool IsBelowRequirements { get; private set; }

	public IReadOnlyCollection<RestGuildOnboardingPrompt> Prompts { get; private set; }

	IReadOnlyCollection<IGuildOnboardingPrompt> IGuildOnboarding.Prompts => Prompts;

	IGuild IGuildOnboarding.Guild => Guild;

	internal RestGuildOnboarding(BaseDiscordClient discord, GuildOnboarding model, RestGuild guild = null)
	{
		Discord = discord;
		Guild = guild;
		Update(model);
	}

	internal void Update(GuildOnboarding model)
	{
		GuildId = model.GuildId;
		DefaultChannelIds = ((IEnumerable<ulong>)model.DefaultChannelIds).ToImmutableArray();
		IsEnabled = model.Enabled;
		Mode = model.Mode;
		IsBelowRequirements = model.IsBelowRequirements;
		Prompts = model.Prompts.Select((GuildOnboardingPrompt prompt) => new RestGuildOnboardingPrompt(Discord, prompt.Id, prompt)).ToImmutableArray();
	}

	public async Task ModifyAsync(Action<GuildOnboardingProperties> props, RequestOptions options = null)
	{
		Update(await GuildHelper.ModifyGuildOnboardingAsync(Guild, props, Discord, options));
	}
}
