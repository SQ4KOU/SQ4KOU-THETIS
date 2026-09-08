using System.Collections.Generic;

namespace Discord;

public interface IComponentInteractionData : IDiscordInteractionData
{
	string CustomId { get; }

	ComponentType Type { get; }

	IReadOnlyCollection<string> Values { get; }

	IReadOnlyCollection<IChannel> Channels { get; }

	IReadOnlyCollection<IUser> Users { get; }

	IReadOnlyCollection<IRole> Roles { get; }

	IReadOnlyCollection<IGuildUser> Members { get; }

	string Value { get; }
}
