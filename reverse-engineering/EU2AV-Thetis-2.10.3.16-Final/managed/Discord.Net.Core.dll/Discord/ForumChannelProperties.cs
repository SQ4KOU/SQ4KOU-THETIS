using System.Collections.Generic;

namespace Discord;

public class ForumChannelProperties : TextChannelProperties
{
	public new Optional<int> SlowModeInterval { get; }

	public Optional<int> ThreadCreationInterval { get; set; }

	public Optional<IEnumerable<IForumTag>> Tags { get; set; }

	public Optional<IEmote> DefaultReactionEmoji { get; set; }

	public Optional<ForumSortOrder> DefaultSortOrder { get; set; }

	public Optional<ForumLayout> DefaultLayout { get; set; }
}
