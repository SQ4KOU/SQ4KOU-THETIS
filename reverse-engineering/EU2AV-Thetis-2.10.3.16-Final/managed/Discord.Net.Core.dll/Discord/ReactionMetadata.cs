using System.Collections.Generic;

namespace Discord;

public struct ReactionMetadata
{
	public int ReactionCount { get; internal set; }

	public bool IsMe { get; internal set; }

	public int BurstCount { get; internal set; }

	public int NormalCount { get; internal set; }

	public IReadOnlyCollection<Color> BurstColors { get; internal set; }
}
