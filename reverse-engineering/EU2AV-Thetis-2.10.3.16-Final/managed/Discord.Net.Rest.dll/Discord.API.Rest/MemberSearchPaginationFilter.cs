using Newtonsoft.Json;

namespace Discord.API.Rest;

internal class MemberSearchPaginationFilter
{
	[JsonProperty("guild_joined_at")]
	public long GuildJoinedAt { get; set; }

	[JsonProperty("user_id")]
	public ulong UserId { get; set; }
}
