using System;
using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class CustomStatusGame : Game
{
	public IEmote Emote { get; internal set; }

	public DateTimeOffset CreatedAt { get; internal set; }

	public string State { get; internal set; }

	private string DebuggerDisplay => base.Name ?? "";

	internal CustomStatusGame()
	{
	}

	public CustomStatusGame(string state)
	{
		base.Name = "Custom Status";
		State = state;
		base.Type = ActivityType.CustomStatus;
	}

	public override string ToString()
	{
		return $"{Emote} {State}";
	}
}
