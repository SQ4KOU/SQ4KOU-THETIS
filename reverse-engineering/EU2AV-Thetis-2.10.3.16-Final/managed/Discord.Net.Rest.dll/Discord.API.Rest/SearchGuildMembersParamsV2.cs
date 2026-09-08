using Newtonsoft.Json;

namespace Discord.API.Rest;

internal class SearchGuildMembersParamsV2
{
	[JsonProperty("limit")]
	public Optional<int?> Limit { get; set; }

	[JsonProperty("and_query")]
	public Optional<MemberSearchFilter> AndQuery { get; set; }

	[JsonProperty("or_query")]
	public Optional<MemberSearchFilter> OrQuery { get; set; }

	[JsonProperty("after")]
	public Optional<MemberSearchPaginationFilter> After { get; set; }

	[JsonProperty("before")]
	public Optional<MemberSearchPaginationFilter> Before { get; set; }

	[JsonProperty("sort")]
	public Optional<MemberSearchV2SortType> Sort { get; set; }
}
