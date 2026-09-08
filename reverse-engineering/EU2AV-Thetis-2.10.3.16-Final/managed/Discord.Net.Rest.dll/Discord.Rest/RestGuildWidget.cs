using System.Diagnostics;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct RestGuildWidget
{
	public bool IsEnabled { get; private set; }

	public ulong? ChannelId { get; private set; }

	private string DebuggerDisplay => string.Format("{0} ({1})", ChannelId, IsEnabled ? "Enabled" : "Disabled");

	internal RestGuildWidget(bool isEnabled, ulong? channelId)
	{
		ChannelId = channelId;
		IsEnabled = isEnabled;
	}

	internal static RestGuildWidget Create(GuildWidget model)
	{
		return new RestGuildWidget(model.Enabled, model.ChannelId);
	}

	public override string ToString()
	{
		return ChannelId?.ToString() ?? "Unknown";
	}
}
