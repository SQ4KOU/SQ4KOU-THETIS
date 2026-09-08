using Newtonsoft.Json;

namespace Discord.API.Rest;

internal class SafetySignalsProperties
{
	[JsonProperty("unusual_dm_activity_until")]
	public Optional<SearchQueryProperties> UnusualDMActivityUntil { get; set; }

	[JsonProperty("communication_disabled_until")]
	public Optional<SearchQueryProperties> CommunicationDisabledUntil { get; set; }

	[JsonProperty("unusual_account_activity")]
	public Optional<bool> UnusualAccountActivity { get; set; }

	[JsonProperty("automod_quarantined_username")]
	public Optional<bool> AutomodQuarantinedUsername { get; set; }
}
