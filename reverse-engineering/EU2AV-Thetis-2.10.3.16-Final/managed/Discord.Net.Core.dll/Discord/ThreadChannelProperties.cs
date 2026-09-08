using System.Collections.Generic;

namespace Discord;

public class ThreadChannelProperties : TextChannelProperties
{
	public Optional<IEnumerable<ulong>> AppliedTags { get; set; }

	public Optional<bool> Locked { get; set; }

	public Optional<bool> Archived { get; set; }
}
