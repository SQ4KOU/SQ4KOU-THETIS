using Newtonsoft.Json;

namespace Discord.API.Rest;

internal class ModifyGuildOnboardingParams
{
	[JsonProperty("prompts")]
	public Optional<GuildOnboardingPromptParams[]> Prompts { get; set; }

	[JsonProperty("default_channel_ids")]
	public Optional<ulong[]> DefaultChannelIds { get; set; }

	[JsonProperty("enabled")]
	public Optional<bool> Enabled { get; set; }

	[JsonProperty("mode")]
	public Optional<GuildOnboardingMode> Mode { get; set; }
}
