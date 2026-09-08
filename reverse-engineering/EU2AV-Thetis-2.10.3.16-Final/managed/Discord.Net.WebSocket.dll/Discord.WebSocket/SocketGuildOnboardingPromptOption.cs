using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;

namespace Discord.WebSocket;

public class SocketGuildOnboardingPromptOption : SocketEntity<ulong>, IGuildOnboardingPromptOption, ISnowflakeEntity, IEntity<ulong>
{
	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public IReadOnlyCollection<ulong> ChannelIds { get; private set; }

	public IReadOnlyCollection<SocketGuildChannel> Channels { get; private set; }

	public IReadOnlyCollection<ulong> RoleIds { get; private set; }

	public IReadOnlyCollection<SocketRole> Roles { get; private set; }

	public IEmote Emoji { get; private set; }

	public string Title { get; private set; }

	public string Description { get; private set; }

	internal SocketGuildOnboardingPromptOption(DiscordSocketClient discord, ulong id, GuildOnboardingPromptOption model, SocketGuild guild)
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
		Roles = model.RoleIds.Select(guild.GetRole).ToImmutableArray();
		Channels = model.ChannelIds.Select(guild.GetChannel).ToImmutableArray();
	}
}
