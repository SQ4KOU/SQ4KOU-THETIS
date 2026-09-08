using System.Collections.Generic;

namespace Discord;

public interface IGuildOnboardingPromptOption : ISnowflakeEntity, IEntity<ulong>
{
	IReadOnlyCollection<ulong> ChannelIds { get; }

	IReadOnlyCollection<ulong> RoleIds { get; }

	IEmote Emoji { get; }

	string Title { get; }

	string Description { get; }
}
