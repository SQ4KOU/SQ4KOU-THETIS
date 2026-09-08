using Newtonsoft.Json;

namespace Discord.API.Rest;

internal class SearchRangeProperties
{
	[JsonProperty("gte")]
	public Optional<long> GreaterThanOrEqual { get; set; }

	[JsonProperty("lte")]
	public Optional<long> LessThanOrEqual { get; set; }
}
