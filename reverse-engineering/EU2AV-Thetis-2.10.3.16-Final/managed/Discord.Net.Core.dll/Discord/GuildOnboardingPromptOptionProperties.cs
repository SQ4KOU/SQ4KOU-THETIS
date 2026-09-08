namespace Discord;

public class GuildOnboardingPromptOptionProperties
{
	public ulong? Id { get; set; }

	public ulong[] ChannelIds { get; set; }

	public ulong[] RoleIds { get; set; }

	public Optional<IEmote> Emoji { get; set; }

	public string Title { get; set; }

	public string Description { get; set; }
}
