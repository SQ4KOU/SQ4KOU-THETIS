using System;

namespace Discord;

public class AutoModRuleActionProperties
{
	public AutoModActionType Type { get; set; }

	public ulong? ChannelId { get; set; }

	public TimeSpan? TimeoutDuration { get; set; }

	public Optional<string> CustomMessage { get; set; }
}
