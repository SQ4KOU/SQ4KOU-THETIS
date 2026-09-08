using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Discord.API;

namespace Discord.Rest;

public class RestGuildOnboardingPromptOption : RestEntity<ulong>, IGuildOnboardingPromptOption, ISnowflakeEntity, IEntity<ulong>
{
	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public IReadOnlyCollection<ulong> ChannelIds { get; private set; }

	public IReadOnlyCollection<ulong> RoleIds { get; private set; }

	public IEmote Emoji { get; private set; }

	public string Title { get; private set; }

	public string Description { get; private set; }

	internal RestGuildOnboardingPromptOption(BaseDiscordClient discord, ulong id, GuildOnboardingPromptOption model)
		: base(discord, id)
	{
		ChannelIds = ((IEnumerable<ulong>)model.ChannelIds).ToImmutableArray();
		RoleIds = ((IEnumerable<ulong>)model.RoleIds).ToImmutableArray();
		Title = model.Title;
		Description = model.Description;
		if (model.Emoji.Id.HasValue)
		{
			Emoji = new Emote(model.Emoji.Id.Value, model.Emoji.Name, model.Emoji.Animated.GetValueOrDefault(defaultValue: false));
		}
		else if (!string.IsNullOrWhiteSpace(model.Emoji.Name))
		{
			Emoji = new Emoji(model.Emoji.Name);
		}
		else
		{
			Emoji = null;
		}
	}
}
