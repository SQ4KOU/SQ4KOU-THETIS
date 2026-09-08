using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

public class RestGuildOnboardingPrompt : RestEntity<ulong>, IGuildOnboardingPrompt, ISnowflakeEntity, IEntity<ulong>
{
	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public IReadOnlyCollection<RestGuildOnboardingPromptOption> Options { get; private set; }

	public string Title { get; private set; }

	public bool IsSingleSelect { get; private set; }

	public bool IsRequired { get; private set; }

	public bool IsInOnboarding { get; private set; }

	public GuildOnboardingPromptType Type { get; private set; }

	IReadOnlyCollection<IGuildOnboardingPromptOption> IGuildOnboardingPrompt.Options => Options;

	internal RestGuildOnboardingPrompt(BaseDiscordClient discord, ulong id, GuildOnboardingPrompt model)
		: base(discord, id)
	{
		Title = model.Title;
		IsSingleSelect = model.IsSingleSelect;
		IsInOnboarding = model.IsInOnboarding;
		IsRequired = model.IsRequired;
		Type = model.Type;
		Options = model.Options.Select((GuildOnboardingPromptOption option) => new RestGuildOnboardingPromptOption(discord, option.Id, option)).ToImmutableArray();
	}
}
