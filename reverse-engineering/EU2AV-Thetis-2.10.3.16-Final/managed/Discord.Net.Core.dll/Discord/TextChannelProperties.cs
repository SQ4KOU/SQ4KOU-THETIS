namespace Discord;

public class TextChannelProperties : GuildChannelProperties
{
	public Optional<string> Topic { get; set; }

	public Optional<bool> IsNsfw { get; set; }

	public Optional<int> SlowModeInterval { get; set; }

	public Optional<ThreadArchiveDuration> AutoArchiveDuration { get; set; }

	public Optional<int> DefaultSlowModeInterval { get; set; }

	public Optional<ChannelType> ChannelType { get; set; }
}
