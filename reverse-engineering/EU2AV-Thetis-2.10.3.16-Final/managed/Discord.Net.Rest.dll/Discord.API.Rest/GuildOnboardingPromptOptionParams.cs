using Newtonsoft.Json;

namespace Discord.API.Rest;

internal class GuildOnboardingPromptOptionParams
{
	[JsonProperty("id")]
	public Optional<ulong> Id { get; set; }

	[JsonProperty("channel_ids")]
	public ulong[] ChannelIds { get; set; }

	[JsonProperty("role_ids")]
	public ulong[] RoleIds { get; set; }

	[JsonProperty("emoji_name")]
	public string EmojiName { get; set; }

	[JsonProperty("emoji_id")]
	public ulong? EmojiId { get; set; }

	[JsonProperty("emoji_animated")]
	public bool? EmojiAnimated { get; set; }

	[JsonProperty("title")]
	public string Title { get; set; }

	[JsonProperty("description")]
	public string Description { get; set; }
}
