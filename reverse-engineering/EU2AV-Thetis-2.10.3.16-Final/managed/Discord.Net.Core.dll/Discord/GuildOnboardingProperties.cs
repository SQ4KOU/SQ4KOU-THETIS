namespace Discord;

public class GuildOnboardingProperties
{
	public Optional<GuildOnboardingPromptProperties[]> Prompts { get; set; }

	public Optional<ulong[]> ChannelIds { get; set; }

	public Optional<bool> IsEnabled { get; set; }

	public Optional<GuildOnboardingMode> Mode { get; set; }
}
