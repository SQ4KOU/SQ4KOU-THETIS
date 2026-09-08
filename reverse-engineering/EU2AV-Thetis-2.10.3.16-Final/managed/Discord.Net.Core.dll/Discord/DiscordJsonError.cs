using System.Collections.Generic;
using System.Collections.Immutable;

namespace Discord;

public struct DiscordJsonError
{
	public string Path { get; }

	public IReadOnlyCollection<DiscordError> Errors { get; }

	internal DiscordJsonError(string path, DiscordError[] errors)
	{
		Path = path;
		Errors = ((IEnumerable<DiscordError>)errors).ToImmutableArray();
	}
}
