using System.Collections.Generic;

namespace Discord;

public class GuildChannelProperties
{
	public Optional<string> Name { get; set; }

	public Optional<int> Position { get; set; }

	public Optional<ulong?> CategoryId { get; set; }

	public Optional<IEnumerable<Overwrite>> PermissionOverwrites { get; set; }

	public Optional<ChannelFlags> Flags { get; set; }
}
