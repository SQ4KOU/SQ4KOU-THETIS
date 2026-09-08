using System;

namespace Discord;

[Flags]
public enum RetryMode
{
	AlwaysFail = 0,
	RetryTimeouts = 1,
	RetryRatelimit = 4,
	Retry502 = 8,
	AlwaysRetry = RetryTimeouts | RetryRatelimit | Retry502
}
