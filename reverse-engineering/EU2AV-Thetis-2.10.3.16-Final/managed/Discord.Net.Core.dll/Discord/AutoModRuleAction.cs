using System;

namespace Discord;

public class AutoModRuleAction
{
	public AutoModActionType Type { get; }

	public ulong? ChannelId { get; }

	public Optional<string> CustomMessage { get; set; }

	public TimeSpan? TimeoutDuration { get; }

	internal AutoModRuleAction(AutoModActionType type, ulong? channelId, int? duration, string customMessage)
	{
		Type = type;
		ChannelId = channelId;
		TimeoutDuration = (duration.HasValue ? new TimeSpan?(TimeSpan.FromSeconds(duration.Value)) : ((TimeSpan?)null));
		CustomMessage = customMessage;
	}
}
