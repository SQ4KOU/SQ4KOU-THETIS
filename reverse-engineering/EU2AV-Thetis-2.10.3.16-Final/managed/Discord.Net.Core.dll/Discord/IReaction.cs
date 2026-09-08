using System.Collections.Generic;

namespace Discord;

public interface IReaction
{
	IEmote Emote { get; }

	IReadOnlyCollection<Color> BurstColors { get; }
}
