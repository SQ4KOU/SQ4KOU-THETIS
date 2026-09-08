using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;

namespace Discord.WebSocket;

public class SocketGuildOnboardingPrompt : SocketEntity<ulong>, IGuildOnboardingPrompt, ISnowflakeEntity, IEntity<ulong>
{
	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public IReadOnlyCollection<SocketGuildOnboardingPromptOption> Options { get; private set; }

	public string Title { get; private set; }

	public bool IsSingleSelect { get; private set; }

	public bool IsRequired { get; private set; }

	public bool IsInOnboarding { get; private set; }

	public GuildOnboardingPromptType Type { get; private set; }

	IReadOnlyCollection<IGuildOnboardingPromptOption> IGuildOnboardingPrompt.Options => Options;

	internal SocketGuildOnboardingPrompt(DiscordSocketClient discord, ulong id, GuildOnboardingPrompt model, SocketGuild guild)
		: base(discord, id)
	{
		Title = model.Title;
		IsSingleSelect = model.IsSingleSelect;
		IsInOnboarding = model.IsInOnboarding;
		IsRequired = model.IsRequired;
		Type = model.Type;
		Options = model.Options.Select((GuildOnboardingPromptOption option) => new SocketGuildOnboardingPromptOption(discord, option.Id, option, guild)).ToImmutableArray();
	}
}
