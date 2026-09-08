using System.Collections.Generic;
using Newtonsoft.Json;

namespace Discord.API.Rest;

internal class SearchQueryProperties
{
	[JsonProperty("and_query")]
	public Optional<IEnumerable<object>> AndQuery { get; set; }

	[JsonProperty("or_query")]
	public Optional<IEnumerable<object>> OrQuery { get; set; }

	[JsonProperty("range")]
	public Optional<SearchRangeProperties> Range { get; set; }
}
