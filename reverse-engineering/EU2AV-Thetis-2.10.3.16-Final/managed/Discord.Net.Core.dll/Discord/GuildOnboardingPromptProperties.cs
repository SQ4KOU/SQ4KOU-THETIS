namespace Discord;

public class GuildOnboardingPromptProperties
{
	public ulong? Id { get; set; }

	public GuildOnboardingPromptOptionProperties[] Options { get; set; }

	public string Title { get; set; }

	public bool IsSingleSelect { get; set; }

	public bool IsRequired { get; set; }

	public bool IsInOnboarding { get; set; }

	public GuildOnboardingPromptType Type { get; set; }
}
