using Newtonsoft.Json;

namespace Discord.API.Rest;

internal class MemberSearchFilter
{
	[JsonProperty("safety_signals")]
	public Optional<SafetySignalsProperties> SafetySignals { get; set; }

	[JsonProperty("role_ids")]
	public Optional<SearchQueryProperties> RoleIds { get; set; }

	[JsonProperty("user_id")]
	public Optional<SearchQueryProperties> UserId { get; set; }

	[JsonProperty("guild_joined_at")]
	public Optional<SearchQueryProperties> GuildJoinedAt { get; set; }

	[JsonProperty("source_invite_code")]
	public Optional<SearchQueryProperties> SourceInviteCode { get; set; }

	[JsonProperty("join_source_type")]
	public Optional<SearchQueryProperties> JoinSourceType { get; set; }

	[JsonProperty("did_rejoin")]
	public Optional<bool> DidRejoin { get; set; }

	[JsonProperty("is_pending")]
	public Optional<bool> IsPending { get; set; }

	[JsonProperty("usernames")]
	public Optional<SearchQueryProperties> Usernames { get; set; }
}
