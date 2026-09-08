using Discord.Net.Rest;

namespace Discord.Rest;

public class DiscordRestConfig : DiscordConfig
{
	public RestClientProvider RestClientProvider { get; set; } = DefaultRestClientProvider.Instance;

	public bool APIOnRestInteractionCreation { get; set; } = true;
}
