using System.Collections.Generic;
using System.Collections.Immutable;

namespace Discord;

public class WelcomeScreen
{
	public string Description { get; }

	public IReadOnlyCollection<WelcomeScreenChannel> Channels { get; }

	internal WelcomeScreen(string description, IReadOnlyCollection<WelcomeScreenChannel> channels)
	{
		Description = description;
		Channels = channels.ToImmutableArray();
	}
}
