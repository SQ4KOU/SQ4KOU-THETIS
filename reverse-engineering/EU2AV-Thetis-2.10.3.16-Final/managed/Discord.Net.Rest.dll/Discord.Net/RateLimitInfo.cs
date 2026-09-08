using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Discord.API;
using Discord.Rest;
using Newtonsoft.Json;

namespace Discord.Net;

public struct RateLimitInfo : IRateLimitInfo
{
	public bool IsGlobal { get; }

	public int? Limit { get; }

	public int? Remaining { get; }

	public int? RetryAfter { get; }

	public DateTimeOffset? Reset { get; }

	public TimeSpan? ResetAfter { get; private set; }

	public string Bucket { get; }

	public TimeSpan? Lag { get; }

	public string Endpoint { get; }

	internal RateLimitInfo(Dictionary<string, string> headers, string endpoint)
	{
		Endpoint = endpoint;
		bool result = default(bool);
		IsGlobal = (headers.TryGetValue("X-RateLimit-Global", out var value) && bool.TryParse(value, out result)) & result;
		Limit = ((headers.TryGetValue("X-RateLimit-Limit", out value) && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result2)) ? new int?(result2) : ((int?)null));
		Remaining = ((headers.TryGetValue("X-RateLimit-Remaining", out value) && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result3)) ? new int?(result3) : ((int?)null));
		Reset = ((headers.TryGetValue("X-RateLimit-Reset", out value) && double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var result4)) ? new DateTimeOffset?(DateTimeOffset.FromUnixTimeMilliseconds((long)(result4 * 1000.0))) : ((DateTimeOffset?)null));
		RetryAfter = ((headers.TryGetValue("Retry-After", out value) && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var result5)) ? new int?(result5) : ((int?)null));
		ResetAfter = ((headers.TryGetValue("X-RateLimit-Reset-After", out value) && double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var result6)) ? new TimeSpan?(TimeSpan.FromSeconds(result6)) : ((TimeSpan?)null));
		Bucket = (headers.TryGetValue("X-RateLimit-Bucket", out value) ? value : null);
		Lag = ((headers.TryGetValue("Date", out value) && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result7)) ? new TimeSpan?(DateTimeOffset.UtcNow - result7) : ((TimeSpan?)null));
	}

	internal Ratelimit ReadRatelimitPayload(Stream response)
	{
		if (response != null && response.Length != 0L)
		{
			using (TextReader reader = new StreamReader(response))
			{
				using JsonReader reader2 = new JsonTextReader(reader);
				return DiscordRestClient.Serializer.Deserialize<Ratelimit>(reader2);
			}
		}
		return null;
	}
}
