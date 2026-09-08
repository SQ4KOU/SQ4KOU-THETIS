using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketGuildOnboarding : IGuildOnboarding
{
	internal DiscordSocketClient Discord;

	public ulong GuildId { get; private set; }

	public SocketGuild Guild { get; private set; }

	public IReadOnlyCollection<SocketGuildOnboardingPrompt> Prompts { get; private set; }

	public IReadOnlyCollection<ulong> DefaultChannelIds { get; private set; }

	public IReadOnlyCollection<SocketGuildChannel> DefaultChannels { get; private set; }

	public bool IsEnabled { get; private set; }

	public bool IsBelowRequirements { get; private set; }

	public GuildOnboardingMode Mode { get; private set; }

	IGuild IGuildOnboarding.Guild => Guild;

	IReadOnlyCollection<IGuildOnboardingPrompt> IGuildOnboarding.Prompts => Prompts;

	internal SocketGuildOnboarding(DiscordSocketClient discord, GuildOnboarding model, SocketGuild guild)
	{
		Discord = discord;
		Guild = guild;
		Update(model);
	}

	internal void Update(GuildOnboarding model)
	{
		GuildId = model.GuildId;
		IsEnabled = model.Enabled;
		Mode = model.Mode;
		IsBelowRequirements = model.IsBelowRequirements;
		DefaultChannelIds = model.DefaultChannelIds;
		DefaultChannels = model.DefaultChannelIds.Select(Guild.GetChannel).ToImmutableArray();
		Prompts = model.Prompts.Select((GuildOnboardingPrompt x) => new SocketGuildOnboardingPrompt(Discord, x.Id, x, Guild)).ToImmutableArray();
	}

	public async Task ModifyAsync(Action<GuildOnboardingProperties> props, RequestOptions options = null)
	{
		Update(await GuildHelper.ModifyGuildOnboardingAsync(Guild, props, Discord, options));
	}
}
